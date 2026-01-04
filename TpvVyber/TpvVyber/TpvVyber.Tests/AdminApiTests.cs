using System;
using System.Security.Principal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using NUnit.Framework;
using Serilog;
using TpvVyber.Classes;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Extensions;
using TpvVyber.Data;
using TpvVyber.Endpoints.Admin;

namespace TpvVyber.Tests;

[NonParallelizable]
[TestFixture]
public class AdminApiTests : EeTestClass
{
    [Test]
    public async Task TestAdminApi()
    {
        await Page.GotoAsync("/");
        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Kurzy" })
            .ClickAsync();
        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Admin" })
            .ClickAsync();

        _context.Courses.RemoveRange(_context.Courses);
        await _context.SaveChangesAsync();

        _context.Students.RemoveRange(_context.Students);
        await _context.SaveChangesAsync();

        _context.OrderCourses.RemoveRange(_context.OrderCourses);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();
        Assert.IsFalse(_context.Courses.Any());
        Assert.IsFalse(_context.Students.Any());
        Assert.IsFalse(_context.OrderCourses.Any());

        var newCourse = new Client.Classes.CourseCln
        {
            Capacity = 1,
            Description = "Popis",
            ForClasses = "Prima",
            MaxPrice = 1,
            MinPrice = 2,
            Name = "Kurz 1",
            MinCapacity = 1,
        };

        var course = await adminService.AddCourseAsync(newCourse, true);

        newCourse.Id = course.Id;

        Assert.That(newCourse == course);
        Assert.That(_context.Courses.Count() == 1);

        _context.Courses.RemoveRange(_context.Courses);
        await _context.SaveChangesAsync();

        _context.Students.RemoveRange(_context.Students);
        await _context.SaveChangesAsync();

        _context.OrderCourses.RemoveRange(_context.OrderCourses);
        await _context.SaveChangesAsync();

        Assert.IsFalse(_context.Courses.Any());
        Assert.IsFalse(_context.Students.Any());
        Assert.IsFalse(_context.OrderCourses.Any());

        var newStudent = new Client.Classes.StudentCln
        {
            Name = "Jmeno",
            Class = "Prima",
            Email = "email",
        };

        var student = await adminService.AddStudentAsync(newStudent, true);
        newStudent.Id = student.Id;

        Assert.That(newStudent == student);
        Assert.That(_context.Students.Count() == 1);

        _context.Courses.RemoveRange(_context.Courses);
        await _context.SaveChangesAsync();

        _context.Students.RemoveRange(_context.Students);
        await _context.SaveChangesAsync();

        _context.OrderCourses.RemoveRange(_context.OrderCourses);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        Assert.IsFalse(_context.Courses.Any());
        Assert.IsFalse(_context.Students.Any());
        Assert.IsFalse(_context.OrderCourses.Any());

        var st = await adminService.AddStudentAsync(newStudent, true);
        var co = await adminService.AddCourseAsync(newCourse, true);
        var newOrderCourse = new OrderCourseCln
        {
            CourseId = co.Id,
            Order = 0,
            StudentId = st.Id,
        };
        var actualOrderC = await adminService.AddOrderCourseAsync(newOrderCourse, true);
        newOrderCourse.Id = actualOrderC.Id;

        Assert.That(newOrderCourse == actualOrderC);
        Assert.That(_context.OrderCourses.Count() == 1);

        _context.Courses.RemoveRange(_context.Courses);
        await _context.SaveChangesAsync();

        _context.Students.RemoveRange(_context.Students);
        await _context.SaveChangesAsync();

        _context.OrderCourses.RemoveRange(_context.OrderCourses);
        await _context.SaveChangesAsync();

        Assert.IsFalse(_context.Courses.Any());
        Assert.IsFalse(_context.Students.Any());
        Assert.IsFalse(_context.OrderCourses.Any());
        Assert.Pass();
    }

    [Test]
    public async Task TestResultGeneration()
    {
        await Page.GotoAsync("/");
        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Kurzy" })
            .ClickAsync();
        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Admin" })
            .ClickAsync();
        _context.ChangeTracker.Clear();
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "delete-btn" })
            .ClickAsync();
        _context.ChangeTracker.Clear();
        Thread.Sleep(10000);
        Assert.IsFalse(_context.Courses.Any());
        Assert.IsFalse(_context.Students.Any());
        Assert.IsFalse(_context.OrderCourses.Any());

        var groupedClasses = ClassExtensions
            .GetAllClasses()
            .GroupBy(r => ClassExtensions.CalculateClaimStrenght(r));

        Random random = new Random();

        //TODO Normal function - ideal world
        //TODO set ordering based on something else - <max capacity> students - one fave course
        // Test with 100 Courses
        var courses_count = 100;
        for (int i = 0; i < courses_count; i++)
        {
            var courseNew = new Course
            {
                Capacity = 10,
                MinCapacity = 1,
                Description = $"Popis {i}",
                ForClasses = string.Join(";", ClassExtensions.GetAllClasses()),
                Name = $"Nazev {i}",
                PdfUrl = $"Url {i}",
                MaxPrice = 1,
                MinPrice = 2,
            };

            _context.Courses.Add(courseNew);
            await _context.SaveChangesAsync();
        }

        Assert.That(_context.Courses.Count() == courses_count);
        var dbCourses = new List<CourseCln>();

        await foreach (var dbCourse in adminService.GetAllCoursesAsync(true))
        {
            dbCourses.Add(dbCourse);
        }

        var allPeople = dbCourses.Sum(e => e.Capacity);

        for (int i = 0; i < allPeople; i++)
        {
            int index = random.Next(ClassExtensions.GetAllClasses().Count());
            var userClass = ClassExtensions.GetAllClasses()[index];

            var student = new Student()
            {
                Class = userClass,
                Email = $"user{i}@example.com",
                Name = $"user{i}",
            };

            _context.Students.Add(student);
            _context.SaveChanges();
        }

        Assert.That(courses_count < allPeople);
        for (int i = 0; i < allPeople; i++)
        {
            var orderCourse = new OrderCourse
            {
                CourseId = _context.Courses.ElementAt(i % courses_count).Id,
                Order = i,
                StudentId = _context.Students.ElementAt(i).Id,
            };
            _context.OrderCourses.Add(orderCourse);

            _context.SaveChanges();
        }

        Assert.That(_context.Students.Count() == allPeople);
        Assert.That(_context.OrderCourses.Count() == (allPeople * courses_count));

        var results = await adminService.ShowFillCourses(true, true);
        Assert.That(results.Count == courses_count);
        var usersInResult = results.SelectMany(e => e.Value).Select(r => r.Id).ToHashSet();
        var actualUsers = _context.Students.Select(r => r.Id).ToHashSet();
        Assert.That(actualUsers.SetEquals(usersInResult));
        Assert.Pass();

        //TODO Too much people of the same claim strenght
        //TODO Too few people
        //TODO Too much people of different claim strenght
        Assert.Pass();
    }
}
