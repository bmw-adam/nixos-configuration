using Microsoft.AspNetCore.Authorization;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Extensions;

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
        coursesGroup.MapGet("show_fill_courses", HandlerShowFillCourses);
        coursesGroup.MapPut("update_logging_ending", HandlerUpdateLoggingEnding);
        coursesGroup.MapGet("get_all_count", HandlerGetCount);

        var coursesGroupNoAdmin = app.MapGroup($"{baseAdminPath}/courses");

        coursesGroupNoAdmin.MapGet("get_logging_ending", HandlerGetLoggingEnding);
    }

    private static IResult HandlerGetAllCourses(
        IAdminService adminService,
        FillCourseExtended? fillExtended
    )
    {
        try
        {
            var result = adminService.GetAllCoursesAsync(true, fillExtended);
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
            var result = await adminService.GetCourseByIdAsync(id, true, fillExtended);
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
            var result = await adminService.AddCourseAsync(item, true, fillExtended);
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
            await adminService.DeleteCourseAsync(id, true);
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
            await adminService.UpdateCourseAsync(item, true);
            return Results.Ok();
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> HandlerShowFillCourses(
        IAdminService adminService,
        bool? forceRedo,
        FillCourseExtended? fillCourse,
        FillStudentExtended? fillStudent
    )
    {
        try
        {
            var result = await adminService.ShowFillCourses(
                forceRedo,
                true,
                fillCourse,
                fillStudent
            );
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> HandlerGetLoggingEnding(IAdminService adminService)
    {
        try
        {
            var result = await adminService.GetLoggingEndings(true);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> HandlerUpdateLoggingEnding(
        IAdminService adminService,
        LoggingEndingCln loggingEndingCln
    )
    {
        try
        {
            var result = await adminService.UpdateLoggingEnding(loggingEndingCln, true);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> HandlerGetCount(IAdminService adminService)
    {
        try
        {
            var result = await adminService.GetAllCoursesCountAsync(true);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }
}
