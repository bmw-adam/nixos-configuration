using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TpvVyber.Classes;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Data;

namespace TpvVyber.Endpoints.Admin;

public static class CoursesAdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var baseAdminPath = "api/admin";
        app.MapGet($"{baseAdminPath}/courses/get_all", HandlerGetAllCourses);
        app.MapGet($"{baseAdminPath}/courses/get_by_id", HandlerGetCourseById);
        app.MapPost($"{baseAdminPath}/courses/add", HandlerAddCourse);
        app.MapDelete($"{baseAdminPath}/courses/delete/{{id}}", HandlerDeleteCourse);
        app.MapPut($"{baseAdminPath}/courses/update", HandlerUpdateCourse);
    }

    private static async Task<IResult> HandlerGetAllCourses(
        IAdminService adminService,
        FillCourseExtended? fillExtended
    )
    {
        try
        {
            var result = await adminService.GetAllCoursesAsync(fillExtended);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> HandlerGetCourseById(
        IAdminService adminService,
        int id,
        FillCourseExtended? fillExtended
    )
    {
        try
        {
            var result = await adminService.GetCourseByIdAsync(id, fillExtended);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> HandlerAddCourse(
        IAdminService adminService,
        CourseCln item,
        FillCourseExtended? fillExtended
    )
    {
        try
        {
            var result = await adminService.AddCourseAsync(item, fillExtended);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> HandlerDeleteCourse(IAdminService adminService, int id)
    {
        try
        {
            await adminService.DeleteCourseAsync(id);
            return Results.Ok();
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> HandlerUpdateCourse(
        IAdminService adminService,
        CourseCln item
    )
    {
        try
        {
            await adminService.UpdateCourseAsync(item);
            return Results.Ok();
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }
}
