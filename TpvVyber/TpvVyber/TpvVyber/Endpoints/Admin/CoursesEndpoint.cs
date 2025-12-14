using Microsoft.AspNetCore.Authorization;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Admin;

namespace TpvVyber.Endpoints.Admin;

public static class CoursesAdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var baseAdminPath = "api/admin";

        // Create a group for "api/admin/courses" and apply the Admin role requirement
        var coursesGroup = app.MapGroup($"{baseAdminPath}/courses")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        coursesGroup.MapGet("get_all", HandlerGetAllCourses);
        coursesGroup.MapGet("get_by_id", HandlerGetCourseById);
        coursesGroup.MapPost("add", HandlerAddCourse);
        coursesGroup.MapDelete("delete/{id}", HandlerDeleteCourse);
        coursesGroup.MapPut("update", HandlerUpdateCourse);
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
