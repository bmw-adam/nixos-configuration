using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TpvVyber.Classes;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Data;

namespace TpvVyber.Endpoints.Admin;

public static class StudentsAdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var baseAdminPath = "api/admin";
        app.MapGet($"{baseAdminPath}/students/get_all", HandlerGetAllStudents);
        app.MapGet($"{baseAdminPath}/students/get_by_id", HandlerGetStudentById);
        app.MapPost($"{baseAdminPath}/students/add", HandlerAddStudent);
        app.MapDelete($"{baseAdminPath}/students/delete/{{id}}", HandlerDeleteStudent);
        app.MapPut($"{baseAdminPath}/students/update", HandlerUpdateStudent);
    }

    private static async Task<IResult> HandlerGetAllStudents(
        IAdminService adminService,
        FillStudentExtended? fillExtended
    )
    {
        try
        {
            var result = await adminService.GetAllStudentsAsync(fillExtended);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> HandlerGetStudentById(
        IAdminService adminService,
        int id,
        FillStudentExtended? fillExtended
    )
    {
        try
        {
            var result = await adminService.GetStudentByIdAsync(id, fillExtended);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> HandlerAddStudent(
        IAdminService adminService,
        StudentCln item,
        FillStudentExtended? fillExtended
    )
    {
        try
        {
            var result = await adminService.AddStudentAsync(item, fillExtended);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> HandlerDeleteStudent(IAdminService adminService, int id)
    {
        try
        {
            await adminService.DeleteStudentAsync(id);
            return Results.Ok();
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> HandlerUpdateStudent(
        IAdminService adminService,
        StudentCln item
    )
    {
        try
        {
            await adminService.UpdateStudentAsync(item);
            return Results.Ok();
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }
}
