using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MudBlazor;
using TpvVyber.Client.Classes;

namespace TpvVyber.Client.Services.Admin;

public class ClientAdminService(HttpClient httpClient, ISnackbar snackbarService) : IAdminService
{
    #region Courses
    public async Task<CourseCln> AddCourseAsync(
        CourseCln item,
        FillCourseExtended? fillExtended = null
    )
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                $"api/admin/courses/add{(fillExtended == null ? "" : $"?fillExtended={fillExtended}")}",
                item
            );
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CourseCln>()
                ?? throw new Exception("Nepodařilo se načíst přidaný kurz");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            snackbarService.Add("Nepodařilo se přidat kurz", Severity.Error);
            return item;
        }
    }

    public async Task DeleteCourseAsync(int Id)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/admin/courses/delete/{Id}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            snackbarService.Add("Nepodařilo se odebrat kurz", Severity.Error);
            return;
        }
    }

    public async Task<IEnumerable<CourseCln>> GetAllCoursesAsync(
        FillCourseExtended? fillExtended = null
    )
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"api/admin/courses/get_all{(fillExtended == null ? "" : $"?fillExtended={fillExtended}")}"
            );
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<CourseCln>>() ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            snackbarService.Add("Nepodařilo se získat kurzy z databáze", Severity.Error);
            return [];
        }
    }

    public async Task<CourseCln?> GetCourseByIdAsync(
        int Id,
        FillCourseExtended? fillExtended = null
    )
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"api/admin/courses/get_by_id?id={Id}{(fillExtended == null ? "" : $"&fillExtended={fillExtended}")}"
            );
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CourseCln?>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            snackbarService.Add("Nepodařilo se získat kurz z databáze", Severity.Error);
            return null;
        }
    }

    public async Task UpdateCourseAsync(CourseCln item)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync("api/admin/courses/update", item);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            snackbarService.Add("Nepodařilo se aktualizovat kurz v databázi", Severity.Error);
            return;
        }
    }
    #endregion
    #region Students
    public async Task<StudentCln> AddStudentAsync(
        StudentCln item,
        FillStudentExtended? fillExtended = null
    )
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                $"api/admin/students/add{(fillExtended == null ? "" : $"?fillExtended={fillExtended}")}",
                item
            );
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<StudentCln>()
                ?? throw new Exception("Nepodařilo se načíst studenta");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            snackbarService.Add("Nepodařilo se přidat kurz do databáze", Severity.Error);
            return item;
        }
    }

    public async Task DeleteStudentAsync(int Id)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/admin/students/delete/{Id}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            snackbarService.Add("Nepodařilo se odebrat žáka z databáze", Severity.Error);
            return;
        }
    }

    public async Task<IEnumerable<StudentCln>> GetAllStudentsAsync(
        FillStudentExtended? fillExtended = null
    )
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"api/admin/students/get_all{(fillExtended == null ? "" : $"?fillExtended={fillExtended}")}"
            );
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<StudentCln>>() ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            snackbarService.Add("Nepodařilo se získat žáky z databáze", Severity.Error);
            return [];
        }
    }

    public async Task<StudentCln?> GetStudentByIdAsync(
        int Id,
        FillStudentExtended? fillExtended = null
    )
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"api/admin/students/get_by_id?id={Id}{(fillExtended == null ? "" : $"&fillExtended={fillExtended}")}"
            );
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<StudentCln?>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            snackbarService.Add("Nepodařilo se získat žáka z databáze", Severity.Error);
            return null;
        }
    }

    public async Task UpdateStudentAsync(StudentCln item)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync("api/admin/students/update", item);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            snackbarService.Add("Nepodařilo se aktualizovat žáka v databázi", Severity.Error);
            return;
        }
    }
    #endregion
    #region OrderCourses
    public async Task<OrderCourseCln> AddOrderCourseAsync(
        OrderCourseCln item,
        FillOrderCourseExtended? fillExtended = null
    )
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                $"api/admin/order_courses/add{(fillExtended == null ? "" : $"?fillExtended={fillExtended}")}",
                item
            );
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<OrderCourseCln>()
                ?? throw new Exception("Nepodařilo se načíst pořadí kurzů");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            snackbarService.Add("Nepodařilo se přidat pořadí kurzu do databáze", Severity.Error);
            return item;
        }
    }

    public async Task DeleteOrderCourseAsync(int Id)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/admin/order_courses/delete/{Id}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            snackbarService.Add("Nepodařilo se odebrat pořadí kurzu z databáze", Severity.Error);
            return;
        }
    }

    public async Task<IEnumerable<OrderCourseCln>> GetAllOrderCourseAsync(
        FillOrderCourseExtended? fillExtended = null
    )
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"api/admin/order_courses/get_all{(fillExtended == null ? "" : $"?fillExtended={fillExtended}")}"
            );
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<OrderCourseCln>>() ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            snackbarService.Add("Nepodařilo se získat pořadí kurzů z databáze", Severity.Error);
            return [];
        }
    }

    public async Task<OrderCourseCln?> GetOrderCourseByIdAsync(
        int Id,
        FillOrderCourseExtended? fillExtended = null
    )
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"api/admin/order_courses/get_by_id?id={Id}{(fillExtended == null ? "" : $"&fillExtended={fillExtended}")}"
            );
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<OrderCourseCln?>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            snackbarService.Add("Nepodařilo se získat pořadí kurzu z databáze", Severity.Error);
            return null;
        }
    }

    public async Task UpdateOrderCourseAsync(OrderCourseCln item)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync("api/admin/order_courses/update", item);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            snackbarService.Add(
                "Nepodařilo se aktualizovat pořadí kurzu v databázi",
                Severity.Error
            );
            return;
        }
    }
    #endregion
}
