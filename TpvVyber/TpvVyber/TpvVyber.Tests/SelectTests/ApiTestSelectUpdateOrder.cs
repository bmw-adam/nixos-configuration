using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // Added for Task
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using NUnit.Framework;
using Serilog;
using Serilog.Extensions.Logging;
using TpvVyber.Classes;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Data;
using TpvVyber.Endpoints.Admin;
using TpvVyber.Tests.BaseTest;

namespace TpvVyber.Tests;

[NonParallelizable]
[TestFixture]
public class ApiTestUpdateOrder : BaseApiTest
{
    [Test]
    public async Task TestApiTestGetSortedCourses()
    {
        await TestInsert();

        await TestUpdateOrder();

        await TestDelete();
    }

    private async Task TestUpdateOrder()
    {
        var sorted = await SelectService.GetSortedCoursesAsync(
            reThrowError: true,
            FillCourseExtended.Students
                | FillCourseExtended.Availability
                | FillCourseExtended.Occupied
                | FillCourseExtended.OrderCourses
        );

        var allCourses = await SelectService.GetAllCourses(
            true,
            FillCourseExtended.Students
                | FillCourseExtended.Availability
                | FillCourseExtended.Occupied
                | FillCourseExtended.OrderCourses
        );

        var filledCourses = await AdminService.ShowFillCourses(
            true,
            true,
            null,
            FillStudentExtended.ClaimStrength
        );

        Assert.IsNotNull(allCourses);
        Assert.That(allCourses.Count, Is.EqualTo(GenerateCourses.Count));

        try
        {
            // Try an impossible update
            var usersOrdersCoursesTry = OrderCourses.Where(e => e.StudentId == TestStudent.Id);
            var newOrdersCourses = usersOrdersCoursesTry.Append(
                new OrderCourseCln
                {
                    Order = usersOrdersCoursesTry.Select(t => t.Order).Max() + 1,
                    CourseId = GenerateCourses
                        .First(e =>
                            e.ForClasses.Split(";").All(d => !TestStudent.Class.Contains(d))
                        )
                        .Id,
                    StudentId = TestStudent.Id,
                    Extended = new OrderCourseClnExtended()
                    {
                        Course = GenerateCourses.First(e =>
                            e.ForClasses.Split(";").All(d => !TestStudent.Class.Contains(d))
                        ),
                    },
                }
            );

            await SelectService.UpdateOrderAsync(
                newOrdersCourses
                    .Select(t => (t.Order, allCourses.First(y => y.Id == t.CourseId)))
                    .ToDictionary(),
                true
            );

            Assert.Fail();
        }
        catch (Exception ex) { }

        // Try a possible update
        var usersOrdersCourses = OrderCourses.Where(e => e.StudentId == TestStudent.Id).ToList();
        Assert.That(usersOrdersCourses.Count > 1, Is.True);

        var tmpFrst = usersOrdersCourses[0].Order;
        usersOrdersCourses[0].Order = usersOrdersCourses.Last().Order;
        usersOrdersCourses[usersOrdersCourses.Count - 1].Order = tmpFrst;

        await SelectService.UpdateOrderAsync(
            usersOrdersCourses
                .Select(t => (t.Order, allCourses.First(y => y.Id == t.CourseId)))
                .ToDictionary(),
            true
        );

        // Chect if it worked?
        var afterUpdate = await SelectService.GetSortedCoursesAsync(
            reThrowError: true,
            FillCourseExtended.Students
                | FillCourseExtended.Availability
                | FillCourseExtended.Occupied
                | FillCourseExtended.OrderCourses
        );

        Assert.IsNotNull(afterUpdate);
        Assert.That(afterUpdate.Count() == usersOrdersCourses.Count(), Is.True);

        foreach (var pair in afterUpdate)
        {
            Assert.That(usersOrdersCourses.Count(a => a.Order == pair.Key) == 1, Is.True);

            Assert.Pass();

            Assert.That(
                pair.Value.Id == usersOrdersCourses.First(r => r.Order == pair.Key).Id,
                Is.True
            );
        }
    }

    public List<CourseCln> GenerateCourses = (Enumerable.Range(1, 5))
        .Select(_ => GenerateCourse(2))
        .ToList();

    public List<StudentCln> GenerateStudents = (Enumerable.Range(1, 30))
        .Select(_ => GenerateStudent())
        .ToList();

    public List<OrderCourseCln> OrderCourses = new List<OrderCourseCln>();

    private async Task TestInsert()
    {
        await CreateTestUserAsync();

        while (
            GenerateCourses
                .Where(c =>
                    c.ForClasses.Split(";").Any(d => TestStudent.Class.Contains(d))
                )
                .Count() < 2
        )
        {
            GenerateCourses = (Enumerable.Range(1, 5)).Select(_ => GenerateCourse(2)).ToList();
        }

        var i = 0;
        foreach (var course in GenerateCourses)
        {
            var addedCourse = await AdminService.AddCourseAsync(course, reThrowError: true);
            Assert.IsNotNull(addedCourse);
            Assert.That(course.Name, Is.EqualTo(addedCourse.Name));
            GenerateCourses[i].Id = addedCourse.Id;
            i++;
        }

        var allCourses = await AdminService.GetAllCoursesCountAsync(reThrowError: true);
        Assert.That(allCourses, Is.EqualTo(GenerateCourses.Count));

        var s = 0;
        foreach (var student in GenerateStudents)
        {
            var addedStudent = await AdminService.AddStudentAsync(student, reThrowError: true);
            Assert.IsNotNull(addedStudent);
            Assert.That(student.Name, Is.EqualTo(addedStudent.Name));
            GenerateStudents[s].Id = addedStudent.Id;
            s++;
        }

        var allStudents = await AdminService.GetAllStudentsCountAsync(reThrowError: true);
        Assert.That(allStudents, Is.EqualTo(GenerateStudents.Count + 1)); // +1 for test user

        var orderCourses = GenerateOrderCourses(
            GenerateCourses,
            GenerateStudents.Append(TestStudent).ToList()
        );
        foreach (var orderCourse in orderCourses)
        {
            var addedOrderCourse = await AdminService.AddOrderCourseAsync(
                orderCourse,
                reThrowError: true
            );
            Assert.IsNotNull(addedOrderCourse);
            Assert.That(addedOrderCourse.CourseId, Is.EqualTo(orderCourse.CourseId));
            Assert.That(addedOrderCourse.StudentId, Is.EqualTo(orderCourse.StudentId));
            Assert.That(addedOrderCourse.Order, Is.EqualTo(orderCourse.Order));

            OrderCourses.Add(addedOrderCourse);
        }
    }

    private async Task TestDelete()
    {
        var o = 0;
        foreach (var orderCourse in OrderCourses)
        {
            o++;
            await AdminService.DeleteOrderCourseAsync(orderCourse.Id, reThrowError: true);
            var count = await AdminService.GetAllOrderCourseCountAsync(reThrowError: true);
            Assert.That(count, Is.EqualTo(OrderCourses.Count - o));
        }

        var cnt0 = await AdminService.GetAllOrderCourseCountAsync(reThrowError: true);
        Assert.That(cnt0, Is.EqualTo(0), "All order courses should have been deleted.");

        var i = 0;
        foreach (var course in GenerateCourses)
        {
            i++;
            await AdminService.DeleteCourseAsync(course.Id, reThrowError: true);
            var count = await AdminService.GetAllCoursesCountAsync(reThrowError: true);
            Assert.That(count, Is.EqualTo(GenerateCourses.Count - i));
        }

        var cnt = await AdminService.GetAllCoursesCountAsync(reThrowError: true);
        Assert.That(cnt, Is.EqualTo(0), "All courses should have been deleted.");

        i = 0;
        foreach (var student in GenerateStudents)
        {
            i++;
            await AdminService.DeleteStudentAsync(student.Id, reThrowError: true);
            var count = await AdminService.GetAllStudentsCountAsync(reThrowError: true);
            Assert.That(count, Is.EqualTo(GenerateStudents.Count - i + 1)); // +1 for test user
        }

        var cnt2 = await AdminService.GetAllStudentsCountAsync(reThrowError: true);
        Assert.That(
            cnt2,
            Is.EqualTo(1),
            "All students (except the test user) should have been deleted."
        ); // 1 for test user

        await DeleteTestUserAsync();

        var cnt3 = await AdminService.GetAllStudentsCountAsync(reThrowError: true);
        Assert.That(cnt3, Is.EqualTo(0), "All students should have been deleted."); // 0 for test user deleted
    }
}
