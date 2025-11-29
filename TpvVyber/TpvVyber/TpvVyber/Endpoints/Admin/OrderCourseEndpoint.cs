using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TpvVyber.Classes;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Data;

namespace TpvVyber.Endpoints.Admin;

public static class OrderCourseAdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var baseAdminPath = "api/admin";
        app.MapGet($"{baseAdminPath}/order_courses/get_all", HandlerGetAllOrderCourses);
        app.MapGet($"{baseAdminPath}/order_courses/get_by_id", HandlerGetOrderCourseById);
        app.MapPost($"{baseAdminPath}/order_courses/add", HandlerAddOrderCourse);
        app.MapDelete($"{baseAdminPath}/order_courses/delete/{{id}}", HandlerDeleteOrderCourse);
        app.MapPut($"{baseAdminPath}/order_courses/update", HandlerUpdateOrderCourse);
    }

    private static async Task<IResult> HandlerGetAllOrderCourses(
        IAdminService adminService,
        FillOrderCourseExtended? fillExtended
    )
    {
        try
        {
            var result = await adminService.GetAllOrderCourseAsync(fillExtended);
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
        FillOrderCourseExtended? fillExtended
    )
    {
        try
        {
            var result = await adminService.GetOrderCourseByIdAsync(id, fillExtended);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> HandlerAddOrderCourse(
        IAdminService adminService,
        OrderCourseCln item,
        FillOrderCourseExtended? fillExtended
    )
    {
        try
        {
            var result = await adminService.AddOrderCourseAsync(item, fillExtended);
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
            await adminService.DeleteOrderCourseAsync(id);
            return Results.Ok();
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> HandlerUpdateOrderCourse(
        IAdminService adminService,
        OrderCourseCln item
    )
    {
        try
        {
            await adminService.UpdateOrderCourseAsync(item);
            return Results.Ok();
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }
}
