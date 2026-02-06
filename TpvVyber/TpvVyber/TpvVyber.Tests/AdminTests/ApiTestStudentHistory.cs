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
public class ApiTestStudentHistory : BaseApiTest
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
    public CourseCln TestCourse2 = new ApiTestCourses().TestCourse;

    public int HistoryStudentCourseId;
    public HistoryStudentCourseCln TestHistoryStudentCourse =>
        new HistoryStudentCourseCln { StudentId = TestStudent.Id, CourseId = TestCourse.Id };

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

        var addedHistory = await AdminService.AddHistoryStudentCourseAsync(
            TestHistoryStudentCourse,
            reThrowError: true
        );
        Assert.IsNotNull(addedHistory);
        Assert.That(addedHistory.StudentId, Is.EqualTo(TestHistoryStudentCourse.StudentId));
        Assert.That(addedHistory.CourseId, Is.EqualTo(TestHistoryStudentCourse.CourseId));
        HistoryStudentCourseId = addedHistory.Id;

        TestCourse2.Name = "Another Course";
        var addedCourse2 = await AdminService.AddCourseAsync(TestCourse2, reThrowError: true);
        Assert.IsNotNull(addedCourse2);
        TestCourse2.Id = addedCourse2.Id;
        Assert.That(addedCourse2.Name, Is.EqualTo(TestCourse2.Name));
    }

    private async Task TestRead()
    {
        var historyStudentCourseList = new List<HistoryStudentCourseCln>();

        await foreach (
            var historyStudentCourse in AdminService.GetAllHistoryStudentCourseAsync(
                reThrowError: true
            )
        )
        {
            historyStudentCourseList.Add(historyStudentCourse);
        }

        Assert.That(historyStudentCourseList, Is.Not.Empty);
        Assert.That(historyStudentCourseList.All(s => s.StudentId == TestStudent.Id), Is.True);
        Assert.That(historyStudentCourseList.All(s => s.CourseId == TestCourse.Id), Is.True);
        Assert.That(historyStudentCourseList.All(s => s.Id == HistoryStudentCourseId), Is.True);
    }

    private async Task TestUpdate()
    {
        var historyStudentCourse = await AdminService.GetHistoryStudentCourseByIdAsync(
            HistoryStudentCourseId,
            reThrowError: true
        );

        Assert.IsNotNull(historyStudentCourse);
        historyStudentCourse.CourseId = TestCourse2.Id;

        await AdminService.UpdateHistoryStudentCourseAsync(
            historyStudentCourse,
            reThrowError: true
        );

        var newHistoryStudentCourse = await AdminService.GetHistoryStudentCourseByIdAsync(
            HistoryStudentCourseId,
            reThrowError: true
        );

        Assert.IsNotNull(newHistoryStudentCourse);
        Assert.That(newHistoryStudentCourse.CourseId, Is.EqualTo(TestCourse2.Id));
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
