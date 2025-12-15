using Microsoft.AspNetCore.Authorization;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Select;

namespace TpvVyber.Endpoints.Select;

public static class SelectEndpoints
{
    public static void MapSelectEndpoints(this WebApplication app)
    {
        var baseAdminPath = "api/select";

        // Create a group
        var selectGroup = app.MapGroup($"{baseAdminPath}").RequireAuthorization();
        var nonAuthSelectGroup = app.MapGroup($"{baseAdminPath}");

        // Define endpoints relative to the group path
        selectGroup.MapGet("get_sorted_courses", HandlerGetSortedCourses);

        selectGroup.MapPut("update_order", HandlerUpdateCourse);
        selectGroup.MapGet("get_course_info", HandlerCourseInfo);
        nonAuthSelectGroup.MapGet("get_all_courses", HandlerGetAllCourses);
    }

    private static async Task<IResult> HandlerGetSortedCourses(
        ISelectService selectService,
        FillCourseExtended? fillExtended
    )
    {
        try
        {
            var result = await selectService.GetSortedCoursesAsync(fillExtended);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> HandlerUpdateCourse(
        ISelectService selectService,
        Dictionary<int, CourseCln> updateItems
    )
    {
        try
        {
            await selectService.UpdateOrderAsync(updateItems);
            return Results.Ok();
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> HandlerCourseInfo(
        ISelectService selectService,
        int id,
        FillCourseExtended? fillExtended
    )
    {
        try
        {
            var result = await selectService.GetCourseInfo(id, fillExtended);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> HandlerGetAllCourses(
        ISelectService selectService,
        FillCourseExtended? fillExtended
    )
    {
        try
        {
            var result = await selectService.GetAllCourses(fillExtended);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }
}
