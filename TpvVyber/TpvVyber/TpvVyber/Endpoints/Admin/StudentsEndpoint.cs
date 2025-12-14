using Microsoft.AspNetCore.Authorization;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Admin;

namespace TpvVyber.Endpoints.Admin;

public static class StudentsAdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var baseAdminPath = "api/admin";

        var studentsGroup = app.MapGroup($"{baseAdminPath}/students")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        studentsGroup.MapGet("get_all", HandlerGetAllStudents);
        studentsGroup.MapGet("get_by_id", HandlerGetStudentById);
        studentsGroup.MapPost("add", HandlerAddStudent);
        studentsGroup.MapDelete("delete/{id}", HandlerDeleteStudent);
        studentsGroup.MapPut("update", HandlerUpdateStudent);
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
