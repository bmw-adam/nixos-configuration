using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using NUnit.Framework;
using NUnit.Framework.Internal;
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
public class ApiTestOrderCourses : BaseApiTest
{
    [Test]
    public async Task TestAdminApi()
    {
        await TestInsert();
        await TestRead();
        await TestUpdate();
        await TestDelete();
    }

    public StudentCln TestStudent = new ApiTestStudents().TestStudent;
    public CourseCln TestCourse = new ApiTestCourses().TestCourse;

    public int OrderCourseId;
    public OrderCourseCln TestOrderCourse =>
        new OrderCourseCln
        {
            StudentId = TestStudent.Id,
            CourseId = TestCourse.Id,
            Order = 0,
        };

    private async Task TestInsert()
    {
        var addedStudent = await AdminService.AddStudentAsync(TestStudent, reThrowError: true);
        Assert.IsNotNull(addedStudent);
        TestStudent.Id = addedStudent.Id;
        Assert.That(addedStudent.Name, Is.EqualTo(TestStudent.Name));

        var addedCourse = await AdminService.AddCourseAsync(TestCourse, reThrowError: true);
        Assert.IsNotNull(addedCourse);
        TestCourse.Id = addedCourse.Id;
        Assert.That(addedCourse.Name, Is.EqualTo(TestCourse.Name));

        var addedOrderCourse = await AdminService.AddOrderCourseAsync(
            TestOrderCourse,
            reThrowError: true
        );
        Assert.IsNotNull(addedOrderCourse);
        Assert.That(addedOrderCourse.StudentId, Is.EqualTo(TestOrderCourse.StudentId));
        Assert.That(addedOrderCourse.CourseId, Is.EqualTo(TestOrderCourse.CourseId));
        OrderCourseId = addedOrderCourse.Id;
    }

    private async Task TestRead()
    {
        var orderCourseList = new List<OrderCourseCln>();

        await foreach (var orderCourse in AdminService.GetAllOrderCourseAsync(reThrowError: true))
        {
            orderCourseList.Add(orderCourse);
        }

        Assert.That(orderCourseList, Is.Not.Empty);
        Assert.That(orderCourseList.All(s => s.StudentId == TestStudent.Id), Is.True);
        Assert.That(orderCourseList.All(s => s.CourseId == TestCourse.Id), Is.True);
        Assert.That(orderCourseList.All(s => s.Id == OrderCourseId), Is.True);
        Assert.That(orderCourseList.All(s => s.Order == TestOrderCourse.Order), Is.True);
    }

    private async Task TestUpdate()
    {
        var orderCourse = await AdminService.GetOrderCourseByIdAsync(OrderCourseId, reThrowError: true);
        var updatedOrder = 5;

        Assert.IsNotNull(orderCourse);
        orderCourse.Order = updatedOrder;

        await AdminService.UpdateOrderCourseAsync(orderCourse, reThrowError: true);

        var newOrderCourse = await AdminService.GetOrderCourseByIdAsync(OrderCourseId, reThrowError: true);

        Assert.IsNotNull(newOrderCourse);
        Assert.That(newOrderCourse.Order, Is.EqualTo(updatedOrder));
    }

    private async Task TestDelete()
    {
        var OrderCourses = AdminService.GetAllOrderCourseAsync(reThrowError: true);
        await foreach (var orderCourse in OrderCourses)
        {
            await AdminService.DeleteOrderCourseAsync(orderCourse.Id, reThrowError: true);
        }

        var remainingOrderCourses = AdminService.GetAllOrderCourseAsync(reThrowError: true);
        await foreach (var orderCourse in remainingOrderCourses)
        {
            Assert.Fail("There should be no OrderCourses left after deletion.");
        }

        var Students = AdminService.GetAllStudentsAsync(reThrowError: true);
        await foreach (var student in Students)
        {
            await AdminService.DeleteStudentAsync(student.Id, reThrowError: true);
        }
        var remainingStudents = AdminService.GetAllStudentsAsync(reThrowError: true);
        await foreach (var student in remainingStudents)
        {
            Assert.Fail("There should be no Students left after deletion.");
        }

        var Courses = AdminService.GetAllCoursesAsync(reThrowError: true);
        await foreach (var course in Courses)
        {
            await AdminService.DeleteCourseAsync(course.Id, reThrowError: true);
        }
        var remainingCourses = AdminService.GetAllCoursesAsync(reThrowError: true);
        await foreach (var course in remainingCourses)
        {
            Assert.Fail("There should be no Courses left after deletion.");
        }
    }
}
