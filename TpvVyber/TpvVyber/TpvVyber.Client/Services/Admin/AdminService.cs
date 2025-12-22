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

    public async IAsyncEnumerable<CourseCln> GetAllCoursesAsync(
        bool reThrowError,
        FillCourseExtended? fillExtended = null
    )
    {
        // Construct the URL
        var url =
            $"api/admin/courses/get_all{(fillExtended == null ? "" : $"?fillExtended={fillExtended}")}";

        // We declare the stream variable outside the loop
        IAsyncEnumerable<CourseCln?>? courseStream = null;

        try
        {
            // This method initiates the stream but does not download the whole body yet.
            // It maps to the JSON array coming from the server.
            courseStream = httpClient.GetFromJsonAsAsyncEnumerable<CourseCln>(url);
        }
        catch (Exception ex)
        {
            // This catches immediate connection errors (e.g., server down)
            HandleError(ex);
            if (reThrowError)
                throw;
            yield break;
        }

        if (courseStream is not null)
        {
            // We must iterate here to trigger the actual data flow and catch
            // network interruptions mid-stream within this method's context.
            await using var enumerator = courseStream.GetAsyncEnumerator();
            bool hasNext = true;

            while (hasNext)
            {
                CourseCln? course = null;
                try
                {
                    // Downloads the next JSON object in the array
                    hasNext = await enumerator.MoveNextAsync();
                    if (hasNext)
                    {
                        course = enumerator.Current;
                    }
                }
                catch (Exception ex)
                {
                    // This catches errors that happen mid-stream (e.g. WiFi drops)
                    HandleError(ex);
                    if (reThrowError)
                        throw;
                    yield break; // Stop streaming
                }

                if (course != null)
                {
                    yield return course;
                }
            }
        }

        void HandleError(Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            notificationService.Notify($"Nepodařilo se získat kurzy: {ex.Message}", Severity.Error);
        }
    }

    public async Task<uint?> GetAllCoursesCountAsync(bool reThrowError)
    {
        try
        {
            var response = await httpClient.GetAsync("api/admin/courses/get_all_count");
            response.EnsureSuccessStatusCode();
            var deserialized = await response.Content.ReadFromJsonAsync<uint?>();
            return deserialized;
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

    public async IAsyncEnumerable<StudentCln> GetAllStudentsAsync(
        bool reThrowError,
        FillStudentExtended? fillExtended = null
    )
    {
        // 1. Construct the URL
        var url =
            $"api/admin/students/get_all{(fillExtended == null ? "" : $"?fillExtended={fillExtended}")}";

        IAsyncEnumerable<StudentCln?>? studentStream = null;

        // 2. Start the request
        try
        {
            // GetFromJsonAsAsyncEnumerable initiates the stream without buffering the whole list.
            studentStream = httpClient.GetFromJsonAsAsyncEnumerable<StudentCln>(url);
        }
        catch (Exception ex)
        {
            // Catch immediate errors (e.g. DNS failure, Server 500 immediately)
            HandleError(ex);
            if (reThrowError)
                throw;
            yield break;
        }

        // 3. Iterate and Yield
        if (studentStream is not null)
        {
            // We use a manual enumerator to wrap the network activity in a try/catch
            await using var enumerator = studentStream.GetAsyncEnumerator();
            bool hasNext = true;

            while (hasNext)
            {
                StudentCln? student = null;
                try
                {
                    // Reads the next item from the network stream
                    hasNext = await enumerator.MoveNextAsync();

                    if (hasNext)
                    {
                        student = enumerator.Current;
                    }
                }
                catch (Exception ex)
                {
                    // Catch streaming errors (e.g. Connection lost halfway)
                    HandleError(ex);
                    if (reThrowError)
                        throw;
                    yield break;
                }

                if (student != null)
                {
                    yield return student;
                }
            }
        }

        // Helper for consistent error handling
        void HandleError(Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            notificationService.Notify(
                $"Nepodařilo se získat žáky z databáze: {ex.Message}",
                Severity.Error
            );
        }
    }

    public async Task<uint?> GetAllStudentsCountAsync(bool reThrowError)
    {
        try
        {
            var response = await httpClient.GetAsync("api/admin/students/get_all_count");
            response.EnsureSuccessStatusCode();
            var deserialized = await response.Content.ReadFromJsonAsync<uint?>();
            return deserialized;
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

    public async IAsyncEnumerable<OrderCourseCln> GetAllOrderCourseAsync(
        bool reThrowError,
        FillOrderCourseExtended? fillExtended = null
    )
    {
        // 1. Construct the URL
        var url =
            $"api/admin/order_courses/get_all{(fillExtended == null ? "" : $"?fillExtended={fillExtended}")}";

        IAsyncEnumerable<OrderCourseCln?>? stream = null;

        // 2. Initiate the stream
        try
        {
            // This starts the request but does not wait for the whole body
            stream = httpClient.GetFromJsonAsAsyncEnumerable<OrderCourseCln>(url);
        }
        catch (Exception ex)
        {
            // Handle connection start failures (e.g. 404, 500, DNS)
            HandleError(ex);
            if (reThrowError)
                throw;
            yield break;
        }

        // 3. Process the stream
        if (stream is not null)
        {
            await using var enumerator = stream.GetAsyncEnumerator();
            bool hasNext = true;

            while (hasNext)
            {
                OrderCourseCln? item = null;
                try
                {
                    // Pull the next item from the network
                    hasNext = await enumerator.MoveNextAsync();

                    if (hasNext)
                    {
                        item = enumerator.Current;
                    }
                }
                catch (Exception ex)
                {
                    // Handle mid-stream failures (e.g. network drop)
                    HandleError(ex);
                    if (reThrowError)
                        throw;
                    yield break;
                }

                if (item != null)
                {
                    yield return item;
                }
            }
        }

        void HandleError(Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            notificationService.Notify(
                $"Nepodařilo se získat pořadí kurzů z databáze: {ex.Message}",
                Severity.Error
            );
        }
    }

    public async Task<uint?> GetAllOrderCourseCountAsync(bool reThrowError)
    {
        try
        {
            var response = await httpClient.GetAsync("api/admin/order_courses/get_all_count");
            response.EnsureSuccessStatusCode();
            var deserialized = await response.Content.ReadFromJsonAsync<uint?>();
            return deserialized;
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
