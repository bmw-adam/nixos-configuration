using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TpvVyber.Classes;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Data;
using TpvVyber.Extensions;

namespace TpvVyber.Endpoints.Admin;

public static class StudentHistoryAdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var baseAdminPath = "api/admin";

        var coursesOrdersGroup = app.MapGroup($"{baseAdminPath}/history_students")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        coursesOrdersGroup.MapGet("get_all", HandlerGetAllHistoryStudent);
        coursesOrdersGroup.MapGet("get_by_id", HandlerGetOrderCourseById);
        coursesOrdersGroup.MapPost("add", HandlerAddOrderCourse);
        coursesOrdersGroup.MapDelete("delete/{id}", HandlerDeleteOrderCourse);
        coursesOrdersGroup.MapPut("update", HandlerUpdateOrderCourse);
        coursesOrdersGroup.MapGet("get_all_count", HandlerGetCount);
    }

    private static IResult HandlerGetAllHistoryStudent(
        IAdminService adminService,
        FillHistoryStudentCourseExtended? fillExtended
    )
    {
        try
        {
            var result = adminService.GetAllHistoryStudentCourseAsync(true, fillExtended);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> HandlerGetOrderCourseById(
        IAdminService adminService,
        int id,
        FillHistoryStudentCourseExtended? fillExtended
    )
    {
        try
        {
            var result = await adminService.GetHistoryStudentCourseByIdAsync(
                id,
                true,
                fillExtended
            );
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> HandlerAddOrderCourse(
        IAdminService adminService,
        HistoryStudentCourseCln item,
        FillHistoryStudentCourseExtended? fillExtended
    )
    {
        try
        {
            var result = await adminService.AddHistoryStudentCourseAsync(item, true, fillExtended);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> HandlerDeleteOrderCourse(IAdminService adminService, int id)
    {
        try
        {
            await adminService.DeleteHistoryStudentCourseAsync(id, true);
            return Results.Ok();
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> HandlerUpdateOrderCourse(
        IAdminService adminService,
        HistoryStudentCourseCln item
    )
    {
        try
        {
            await adminService.UpdateHistoryStudentCourseAsync(item, true);
            return Results.Ok();
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
            var result = await adminService.GetAllHistoryStudentCourseCountAsync(true);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }
}
