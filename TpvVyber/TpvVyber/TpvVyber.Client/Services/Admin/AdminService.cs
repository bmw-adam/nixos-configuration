using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MudBlazor;
using TpvVyber.Client.Classes;

namespace TpvVyber.Client.Services.Admin;

public class ClientAdminService(HttpClient httpClient, NotificationService notificationService)
    : IAdminService
{
    #region Courses
    public async Task<CourseCln> AddCourseAsync(
        CourseCln item,
        bool reThrowError,
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
            notificationService.Notify($"Nepodařilo se přidat kurz {ex.Message}", Severity.Error);
            return item;
        }
    }

    public async Task DeleteCourseAsync(int Id, bool reThrowError)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/admin/courses/delete/{Id}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            notificationService.Notify($"Nepodařilo se odebrat kurz {ex.Message}", Severity.Error);
            return;
        }
    }

    public async Task<IEnumerable<CourseCln>> GetAllCoursesAsync(
        bool reThrowError,
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
            notificationService.Notify(
                $"Nepodařilo se získat kurzy z databáze {ex.Message}",
                Severity.Error
            );
            return [];
        }
    }

    public async Task<CourseCln?> GetCourseByIdAsync(
        int Id,
        bool reThrowError,
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
            notificationService.Notify(
                $"Nepodařilo se získat kurz z databáze {ex.Message}",
                Severity.Error
            );
            return null;
        }
    }

    public async Task UpdateCourseAsync(CourseCln item, bool reThrowError)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync("api/admin/courses/update", item);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            notificationService.Notify(
                $"Nepodařilo se aktualizovat kurz v databázi",
                Severity.Error
            );
            return;
        }
    }
    #endregion
    #region Students
    public async Task<StudentCln> AddStudentAsync(
        StudentCln item,
        bool reThrowError,
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
            notificationService.Notify(
                $"Nepodařilo se přidat kurz do databáze {ex.Message}",
                Severity.Error
            );
            return item;
        }
    }

    public async Task DeleteStudentAsync(int Id, bool reThrowError)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/admin/students/delete/{Id}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            notificationService.Notify(
                $"Nepodařilo se odebrat žáka z databáze {ex.Message}",
                Severity.Error
            );
            return;
        }
    }

    public async Task<IEnumerable<StudentCln>> GetAllStudentsAsync(
        bool reThrowError,
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
            notificationService.Notify(
                $"Nepodařilo se získat žáky z databáze {ex.Message}",
                Severity.Error
            );
            return [];
        }
    }

    public async Task<StudentCln?> GetStudentByIdAsync(
        int Id,
        bool reThrowError,
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
            notificationService.Notify(
                $"Nepodařilo se získat žáka z databáze {ex.Message}",
                Severity.Error
            );
            return null;
        }
    }

    public async Task UpdateStudentAsync(StudentCln item, bool reThrowError)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync("api/admin/students/update", item);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            notificationService.Notify(
                $"Nepodařilo se aktualizovat žáka v databázi {ex.Message}",
                Severity.Error
            );
            return;
        }
    }
    #endregion
    #region OrderCourses
    public async Task<OrderCourseCln> AddOrderCourseAsync(
        OrderCourseCln item,
        bool reThrowError,
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
            notificationService.Notify(
                $"Nepodařilo se přidat pořadí kurzu do databáze {ex.Message}",
                Severity.Error
            );
            return item;
        }
    }

    public async Task DeleteOrderCourseAsync(int Id, bool reThrowError)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/admin/order_courses/delete/{Id}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            notificationService.Notify(
                $"Nepodařilo se odebrat pořadí kurzu z databáze {ex.Message}",
                Severity.Error
            );
            return;
        }
    }

    public async Task<IEnumerable<OrderCourseCln>> GetAllOrderCourseAsync(
        bool reThrowError,
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
            notificationService.Notify(
                $"Nepodařilo se získat pořadí kurzů z databáze {ex.Message}",
                Severity.Error
            );
            return [];
        }
    }

    public async Task<OrderCourseCln?> GetOrderCourseByIdAsync(
        int Id,
        bool reThrowError,
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
            notificationService.Notify(
                $"Nepodařilo se získat pořadí kurzu z databáze {ex.Message}",
                Severity.Error
            );
            return null;
        }
    }

    public async Task UpdateOrderCourseAsync(OrderCourseCln item, bool reThrowError)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync("api/admin/order_courses/update", item);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            notificationService.Notify(
                $"Nepodařilo se aktualizovat pořadí kurzu v databázi {ex.Message}",
                Severity.Error
            );
            return;
        }
    }

    public async Task<Dictionary<int, List<StudentCln>>> ShowFillCourses(
        bool? forceRedo,
        bool reThrowError,
        FillCourseExtended? fillCourse = null,
        FillStudentExtended? fillStudent = null
    )
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<Dictionary<int, List<StudentCln>>>(
                "api/admin/courses/show_fill_courses"
            );
            return response ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            notificationService.Notify(
                $"Nepodařilo se získat dočasné výsledky {ex.Message}",
                Severity.Error
            );
            return [];
        }
    }

    public async Task<LoggingEndingCln?> GetLoggingEndings(bool reThrowError)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<LoggingEndingCln?>(
                "api/admin/courses/get_logging_ending"
            );
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            notificationService.Notify(
                $"Nepodařilo se získat konec přihlašování {ex.Message}",
                Severity.Error
            );
            return null;
        }
    }

    public async Task<LoggingEndingCln?> UpdateLoggingEnding(
        LoggingEndingCln loggingEnding,
        bool reThrowError
    )
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync<LoggingEndingCln?>(
                "api/admin/courses/update_logging_ending",
                loggingEnding
            );
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<LoggingEndingCln?>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            notificationService.Notify(
                $"Nepodařilo se aktualizovat konec přihlašování {ex.Message}",
                Severity.Error
            );
            return null;
        }
    }
    #endregion
}
