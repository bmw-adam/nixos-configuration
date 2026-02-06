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
public class ApiTestGetSortedCourses : BaseApiTest
{
    [Test]
    public async Task TestApiTestGetSortedCourses()
    {
        await TestInsert();

        await TestGetSortedCourses();

        await TestDelete();
    }

    private async Task TestGetSortedCourses()
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

        // if (
        //     filledCourses.SelectMany(r => r.Value).Count()
        //     < await AdminService.GetAllStudentsCountAsync(true)
        // )
        // {
        //     Assert.That(
        //         filledCourses
        //             .Select(r => r.Key)
        //             .Select(e => allCourses.First(c => c.Id == e))
        //             .All(c => c.Capacity == filledCourses[c.Id].Count()),
        //         Is.True
        //     );
        // }

        foreach (var course in allCourses)
        {
            var courseDetail = await SelectService.GetCourseInfo(
                course.Id,
                reThrowError: true,
                fillExtended: FillCourseExtended.Students
                    | FillCourseExtended.Availability
                    | FillCourseExtended.Occupied
                    | FillCourseExtended.OrderCourses
            );

            Assert.IsNotNull(courseDetail);
            Assert.That(courseDetail!.Id, Is.EqualTo(course.Id));
            Assert.That(courseDetail.Name, Is.EqualTo(course.Name));

            Assert.That(courseDetail.Extended, Is.Not.Null);

            await using var db_context = DbContextFactory.CreateDbContext();
            Assert.That(
                courseDetail.Extended!.Students.Count,
                Is.EqualTo(courseDetail.Extended.OrderCourses.Count)
            );
            Assert.That(
                courseDetail.Extended!.Students.Count,
                Is.EqualTo(db_context.OrderCourses.Where(oc => oc.CourseId == course.Id).Count())
            );

            foreach (
                var student in await AdminService
                    .GetAllStudentsAsync(reThrowError: true, FillStudentExtended.ClaimStrength)
                    .ToListAsync()
            )
            {
                var isEnrolled = courseDetail.Extended.Students.Any(s => s.Id == student.Id);
                var isInOrderCourses = courseDetail.Extended.OrderCourses.Any(oc =>
                    oc.StudentId == student.Id
                );

                Assert.That(isEnrolled, Is.EqualTo(isInOrderCourses));

                if (filledCourses.ContainsKey(course.Id))
                {
                    Assert.That(filledCourses[course.Id], Is.Not.Null);

                    var isActuallyEnrolled = filledCourses[course.Id].Any(s => s.Id == student.Id);

                    Assert.That(filledCourses[course.Id].All(e => e.Extended != null), Is.True);
                    Assert.That(
                        filledCourses[course.Id].All(e => e.Extended.ClaimStrength != null),
                        Is.True
                    );

                    Assert.That(student.Extended, Is.Not.Null);
                    Assert.That(student.Extended.ClaimStrength, Is.Not.Null);

                    if (isActuallyEnrolled)
                    {
                        Assert.That(isEnrolled, Is.True);

                        var weakestCourseMember = filledCourses[course.Id]
                            .Select(e => e.Extended.ClaimStrength)
                            .Min();

                        Assert.That(
                            weakestCourseMember,
                            Is.LessThanOrEqualTo(student.Extended.ClaimStrength)
                        );
                    }
                    else
                    {
                        if (isEnrolled)
                        {
                            Assert.That(isEnrolled, Is.True);
                            var weakestCourseMember = filledCourses[course.Id]
                                .Select(e => e.Extended.ClaimStrength)
                                .Min();

                            Assert.That(
                                weakestCourseMember >= student.Extended.ClaimStrength,
                                Is.True
                            );
                        }
                        else
                        {
                            Assert.That(isEnrolled, Is.False);
                        }
                    }
                }
                else
                {
                    // Course would not happen, as the required minimal capacity wasn't reached
                    // Assert.That(
                    //     course.MinCapacity,
                    //     Is.LessThanOrEqualTo(course.Extended!.Students.Count)
                    // );
                }
            }
        }

        var myOrderCourses = await AdminService
            .GetAllOrderCourseAsync(
                reThrowError: true,
                FillOrderCourseExtended.Student | FillOrderCourseExtended.Course
            )
            .ToListAsync();
        myOrderCourses = myOrderCourses
            .Where(e => e.StudentId == TestStudent.Id)
            .OrderBy(e => e.Order)
            .ToList();

        foreach (var i in sorted.Select(r => r.Key))
        {
            Assert.That(sorted[myOrderCourses[i].Order], Is.Not.Null);
            Assert.That(i, Is.EqualTo(myOrderCourses[i].Order));
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

        var orderCourses = GenerateOrderCourses(GenerateCourses, GenerateStudents);
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
