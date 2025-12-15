using System;
using System.Collections;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MudBlazor;
using TpvVyber.Client.Classes;

namespace TpvVyber.Client.Services.Select;

public class ClientSelectService(HttpClient httpClient, ISnackbar snackbarService) : ISelectService
{
    public async Task<Dictionary<int, CourseCln>> GetSortedCoursesAsync(
        FillCourseExtended? fillExtended = null
    )
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<Dictionary<int, CourseCln>>(
                $"api/select/get_sorted_courses{(fillExtended == null ? "" : $"?fillExtended={fillExtended}")}"
            );
            return response ?? [];
        }
        catch
        {
            snackbarService.Add("Nepodařilo se získat kurzy", Severity.Error);
            return [];
        }
    }

    public async Task UpdateOrderAsync(Dictionary<int, CourseCln> input)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync("api/select/update_order", input);
            response.EnsureSuccessStatusCode();
            snackbarService.Add("Aktualizoval jsem seřazení kurzů", Severity.Success);
        }
        catch
        {
            snackbarService.Add("Nepodařilo se aktualizovat seřazení kurzů", Severity.Error);
        }
    }

    public async Task<CourseCln?> GetCourseInfo(int id, FillCourseExtended? fillExtended = null)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<CourseCln>(
                $"api/select/get_course_info?id={id}{(fillExtended == null ? "" : $"&fillExtended={fillExtended}")}"
            );

            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            snackbarService.Add("Nepodařilo se získat informace o kurzu", Severity.Error);
            return null;
        }
    }

    public async Task<List<CourseCln>?> GetAllCourses(FillCourseExtended? fillExtended = null)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<List<CourseCln>?>(
                $"api/select/get_all_courses{(fillExtended == null ? "" : $"?fillExtended={fillExtended}")}"
            );
            return response;
        }
        catch
        {
            snackbarService.Add("Nepodařilo se získat kurzy", Severity.Error);
            return null;
        }
    }
}
