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
public class ApiTestStudents : BaseApiTest
{
    [Test]
    public async Task TestAdminApi()
    {
        await TestInsert();
        await TestRead();
        await TestUpdate();
        await TestDelete();
    }

    public StudentCln TestStudent = new StudentCln
    {
        Id = 0,
        Name = "Sample Student",
        Email = "email@example.com",
        Class = "oktáva",
    };

    private async Task TestInsert()
    {
        var addedStudent = await AdminService.AddStudentAsync(TestStudent, reThrowError: true);
        Assert.IsNotNull(addedStudent);
        TestStudent.Id = addedStudent.Id;
        Assert.That(addedStudent.Name, Is.EqualTo(TestStudent.Name));
    }

    private async Task TestRead()
    {
        var studentList = new List<StudentCln>();

        await foreach (var student in AdminService.GetAllStudentsAsync(reThrowError: true))
        {
            studentList.Add(student);
        }

        Assert.That(studentList, Is.Not.Empty);
        Assert.That(studentList.All(s => s.Name == TestStudent.Name), Is.True);
    }

    private async Task TestUpdate()
    {
        var student = await AdminService.GetStudentByIdAsync(TestStudent.Id, reThrowError: true);
        var updatedName = "Updated Student Name";

        Assert.IsNotNull(student);
        student.Name = updatedName;

        await AdminService.UpdateStudentAsync(student, reThrowError: true);

        var newStudent = await AdminService.GetStudentByIdAsync(student.Id, reThrowError: true);

        Assert.IsNotNull(newStudent);
        Assert.That(newStudent.Name, Is.EqualTo(updatedName));
    }

    private async Task TestDelete()
    {
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
    }
}
