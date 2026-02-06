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
public class ApiTestGetAllCourses : BaseApiTest
{
    [Test]
    public async Task TestAdminApi()
    {
        await TestInsert();

        await TestGetAllCourses();

        await TestDelete();
    }

    private async Task TestGetAllCourses()
    {
        var allCourses = await AdminService.GetAllCoursesAsync(reThrowError: true).ToListAsync();

        Assert.IsNotNull(allCourses);
        Assert.That(allCourses.Count, Is.EqualTo(GenerateCourses.Count));

        var allSelectCourses = await SelectService.GetAllCourses(true, null);

        Assert.IsNotNull(allSelectCourses);
        Assert.That(allSelectCourses.Count(), Is.EqualTo(GenerateCourses.Count));

        Assert.That(
            allSelectCourses.Select(c => c.Id).OrderBy(n => n),
            Is.EquivalentTo(GenerateCourses.Select(c => c.Id).OrderBy(n => n))
        );
    }

    public List<CourseCln> GenerateCourses = (Enumerable.Range(1, 5))
        .Select(_ => GenerateCourse())
        .ToList();

    public List<StudentCln> GenerateStudents = (Enumerable.Range(1, 30))
        .Select(_ => GenerateStudent())
        .ToList();

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
    }

    private async Task TestDelete()
    {
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
