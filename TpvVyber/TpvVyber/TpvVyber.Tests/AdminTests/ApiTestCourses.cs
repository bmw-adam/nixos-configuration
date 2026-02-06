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
public class ApiTestCourses : BaseApiTest
{
    [Test]
    public async Task TestAdminApi()
    {
        await TestInsert();
        await TestRead();
        await TestUpdate();
        await TestDelete();
    }

    public CourseCln TestCourse = new CourseCln
    {
        Name = "Sample Course",
        Description = "This is a sample course for testing.",
        PdfUrl = "http://example.com/sample.pdf",
        ForClasses = "10A, 10B",
        MinPrice = 100,
        MaxPrice = 200,
        Capacity = 30,
        MinCapacity = 10,
    };

    private async Task TestInsert()
    {
        var addedCourse = await AdminService.AddCourseAsync(TestCourse, reThrowError: true);
        Assert.IsNotNull(addedCourse);
        TestCourse.Id = addedCourse.Id;
        Assert.That(addedCourse.Name, Is.EqualTo(TestCourse.Name));
    }

    private async Task TestRead()
    {
        var courseList = new List<CourseCln>();

        await foreach (var course in AdminService.GetAllCoursesAsync(reThrowError: true))
        {
            courseList.Add(course);
        }

        Assert.That(courseList, Is.Not.Empty);
        Assert.That(courseList.All(c => c.Name == TestCourse.Name), Is.True);
    }

    private async Task TestUpdate()
    {
        var crs = await AdminService.GetCourseByIdAsync(TestCourse.Id, reThrowError: true);
        var updatedDesc = "Updated Description";

        Assert.IsNotNull(crs);
        crs.Description = updatedDesc;

        await AdminService.UpdateCourseAsync(crs, reThrowError: true);

        var newCrs = await AdminService.GetCourseByIdAsync(crs.Id, reThrowError: true);

        Assert.IsNotNull(newCrs);
        Assert.That(newCrs.Description, Is.EqualTo(updatedDesc));
    }

    private async Task TestDelete()
    {
        var courses = AdminService.GetAllCoursesAsync(reThrowError: true);
        await foreach (var course in courses)
        {
            await AdminService.DeleteCourseAsync(course.Id, reThrowError: true);
        }

        var remainingCourses = AdminService.GetAllCoursesAsync(reThrowError: true);
        await foreach (var course in remainingCourses)
        {
            Assert.Fail("There should be no courses left after deletion.");
        }
    }
}
