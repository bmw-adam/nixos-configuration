using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TpvVyber.Client.Classes;

namespace TpvVyber.Client.Services.Admin;

public class ClientAdminService(HttpClient httpClient) : IAdminService
{
    #region Courses
    public async Task<CourseCln> AddCourseAsync(
        CourseCln item,
        FillCourseExtended? fillExtended = null
    )
    {
        var response = await httpClient.PostAsJsonAsync(
            $"api/admin/courses/add{(fillExtended == null ? "" : $"?fillExtended={fillExtended}")}",
            item
        );
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CourseCln>()
            ?? throw new Exception("Nepodařilo se načíst přidaný kurz");
    }

    public async Task DeleteCourseAsync(int Id)
    {
        var response = await httpClient.DeleteAsync($"api/admin/courses/delete/{Id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<IEnumerable<CourseCln>> GetAllCoursesAsync(
        FillCourseExtended? fillExtended = null
    )
    {
        var response = await httpClient.GetAsync(
            $"api/admin/courses/get_all{(fillExtended == null ? "" : $"?fillExtended={fillExtended}")}"
        );
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<CourseCln>>() ?? [];
    }

    public async Task<CourseCln?> GetCourseByIdAsync(
        int Id,
        FillCourseExtended? fillExtended = null
    )
    {
        var response = await httpClient.GetAsync(
            $"api/admin/courses/get_by_id?id={Id}{(fillExtended == null ? "" : $"&fillExtended={fillExtended}")}"
        );
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CourseCln?>();
    }

    public async Task UpdateCourseAsync(CourseCln item)
    {
        var response = await httpClient.PutAsJsonAsync("api/admin/courses/update", item);
        response.EnsureSuccessStatusCode();
    }
    #endregion
    #region Students
    public async Task<StudentCln> AddStudentAsync(
        StudentCln item,
        FillStudentExtended? fillExtended = null
    )
    {
        var response = await httpClient.PostAsJsonAsync(
            $"api/admin/students/add{(fillExtended == null ? "" : $"?fillExtended={fillExtended}")}",
            item
        );
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<StudentCln>()
            ?? throw new Exception("Nepodařilo se načíst studenta");
    }

    public async Task DeleteStudentAsync(int Id)
    {
        var response = await httpClient.DeleteAsync($"api/admin/students/delete/{Id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<IEnumerable<StudentCln>> GetAllStudentsAsync(
        FillStudentExtended? fillExtended = null
    )
    {
        var response = await httpClient.GetAsync(
            $"api/admin/students/get_all{(fillExtended == null ? "" : $"?fillExtended={fillExtended}")}"
        );
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<StudentCln>>() ?? [];
    }

    public async Task<StudentCln?> GetStudentByIdAsync(
        int Id,
        FillStudentExtended? fillExtended = null
    )
    {
        var response = await httpClient.GetAsync(
            $"api/admin/students/get_by_id?id={Id}{(fillExtended == null ? "" : $"&fillExtended={fillExtended}")}"
        );
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<StudentCln?>();
    }

    public async Task UpdateStudentAsync(StudentCln item)
    {
        var response = await httpClient.PutAsJsonAsync("api/admin/students/update", item);
        response.EnsureSuccessStatusCode();
    }
    #endregion
    #region OrderCourses
    public async Task<OrderCourseCln> AddOrderCourseAsync(
        OrderCourseCln item,
        FillOrderCourseExtended? fillExtended = null
    )
    {
        var response = await httpClient.PostAsJsonAsync(
            $"api/admin/order_courses/add{(fillExtended == null ? "" : $"?fillExtended={fillExtended}")}",
            item
        );
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OrderCourseCln>()
            ?? throw new Exception("Nepodařilo se načíst pořadí kurzů");
    }

    public async Task DeleteOrderCourseAsync(int Id)
    {
        var response = await httpClient.DeleteAsync($"api/admin/order_courses/delete/{Id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<IEnumerable<OrderCourseCln>> GetAllOrderCourseAsync(
        FillOrderCourseExtended? fillExtended = null
    )
    {
        var response = await httpClient.GetAsync(
            $"api/admin/order_courses/get_all{(fillExtended == null ? "" : $"?fillExtended={fillExtended}")}"
        );
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<OrderCourseCln>>() ?? [];
    }

    public async Task<OrderCourseCln?> GetOrderCourseByIdAsync(
        int Id,
        FillOrderCourseExtended? fillExtended = null
    )
    {
        var response = await httpClient.GetAsync(
            $"api/admin/order_courses/get_by_id?id={Id}{(fillExtended == null ? "" : $"&fillExtended={fillExtended}")}"
        );
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OrderCourseCln?>();
    }

    public async Task UpdateOrderCourseAsync(OrderCourseCln item)
    {
        var response = await httpClient.PutAsJsonAsync("api/admin/order_courses/update", item);
        response.EnsureSuccessStatusCode();
    }
    #endregion
}
