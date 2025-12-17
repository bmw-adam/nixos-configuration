using System;
using System.Collections;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MudBlazor;
using Newtonsoft.Json;
using TpvVyber.Client.Classes;

namespace TpvVyber.Client.Services.Select;

public class ClientSelectService(HttpClient httpClient, NotificationService notificationService)
    : ISelectService
{
    public async Task<Dictionary<int, CourseCln>> GetSortedCoursesAsync(
        bool reThrowError,
        FillCourseExtended? fillExtended = null
    )
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"api/select/get_sorted_courses{(fillExtended == null ? "" : $"?fillExtended={fillExtended}")}"
            );

            if (!response.IsSuccessStatusCode)
            {
                var desEx = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();
                if (desEx != null)
                {
                    notificationService.Notify(desEx.Detail, Severity.Error);
                }
                return [];
            }
            else
            {
                var dic = await response.Content.ReadFromJsonAsync<Dictionary<int, CourseCln>>();

                // notificationService.Notify("Získal jsem seřazené kurzy", Severity.Success);
                return dic ?? [];
            }
        }
        catch
        {
            notificationService.Notify($"Nepodařilo se získat kurzy", Severity.Error);
            return [];
        }
    }

    public async Task UpdateOrderAsync(Dictionary<int, CourseCln> input, bool reThrowError)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync("api/select/update_order", input);
            if (!response.IsSuccessStatusCode)
            {
                var desEx = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();
                if (desEx != null)
                {
                    notificationService.Notify(desEx.Detail, Severity.Error);
                }
            }
            else
            {
                notificationService.Notify("Aktualizoval jsem seřazení kurzů", Severity.Success);
            }
        }
        catch
        {
            notificationService.Notify(
                $"Nepodařilo se aktualizovat seřazení kurzů",
                Severity.Error
            );
        }
    }

    public async Task<CourseCln?> GetCourseInfo(
        int id,
        bool reThrowError,
        FillCourseExtended? fillExtended = null
    )
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"api/select/get_course_info?id={id}{(fillExtended == null ? "" : $"&fillExtended={fillExtended}")}"
            );
            if (!response.IsSuccessStatusCode)
            {
                var desEx = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();
                if (desEx != null)
                {
                    notificationService.Notify(desEx.Detail, Severity.Error);
                }
                return null;
            }
            else
            {
                var res = await response.Content.ReadFromJsonAsync<CourseCln>();
                // notificationService.Notify("Získat info o kurzu", Severity.Success);
                return res;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            notificationService.Notify(
                $"Nepodařilo se získat informace o kurzu {ex.Message}",
                Severity.Error
            );
            return null;
        }
    }

    public async Task<List<CourseCln>?> GetAllCourses(
        bool reThrowError,
        FillCourseExtended? fillExtended = null
    )
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"api/select/get_all_courses{(fillExtended == null ? "" : $"?fillExtended={fillExtended}")}"
            );

            if (!response.IsSuccessStatusCode)
            {
                var desEx = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();
                if (desEx != null)
                {
                    notificationService.Notify(desEx.Detail, Severity.Error);
                }
                return null;
            }
            else
            {
                var res = await response.Content.ReadFromJsonAsync<List<CourseCln>?>();
                // notificationService.Notify("Získat všechny kurzy", Severity.Success);
                return res;
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify($"Nepodařilo se získat kurzy {ex.Message}", Severity.Error);
            return null;
        }
    }
}
