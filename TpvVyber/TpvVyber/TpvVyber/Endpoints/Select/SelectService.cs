using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using TpvVyber.Classes;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Client.Services.Select;
using TpvVyber.Data;
using TpvVyber.Extensions;

namespace TpvVyber.Endpoints.Select;

public class ServerSelectService(
    IDbContextFactory<TpvVyberContext> _factory,
    IHttpContextAccessor httpContextAccessor,
    IAdminService adminService,
    NotificationService notificationService,
    ILogger<ServerSelectService> logger
) : ISelectService
{
    public async Task<List<CourseCln>?> GetAllCourses(
        bool reThrowError,
        FillCourseExtended? fillExtended = null
    )
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();

            var courses = await ctx
                .Courses.Include(c => c.OrderCourses)
                    .ThenInclude(e => e.Student)
                .ToListAsync();

            var lst = new List<CourseCln>();
            foreach (var course in courses)
            {
                lst.Add(await course.ToClient(ctx, null, adminService, fillExtended));
            }

            return lst;
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se získat kurzy - {ex.Message}");
            notificationService.Notify("Nepodařilo se získat informace o kurzu", Severity.Error);
            if (reThrowError)
            {
                throw new Exception(ex.Message);
            }
            return null;
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
            var userInfo = httpContextAccessor.GetCurrentUser();
            await using var ctx = _factory.CreateDbContext();

            var course = ctx
                .Courses.Include(r => r.OrderCourses)
                    .ThenInclude(e => e.Student)
                .FirstOrDefault(i => i.Id == id);

            if (course == null)
            {
                throw new Exception("Nepodařilo se najít kurz");
            }

            var currentUser = ctx.Students.FirstOrDefault(e => e.Email == userInfo.UserEmail);
            if (currentUser == null)
            {
                throw new Exception("Nepoznal jsem aktuálního uživatele");
            }

            return await course.ToClient(ctx, currentUser, adminService, fillExtended);
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se získat informace o kurzu - {ex.Message}");
            notificationService.Notify("Nepodařilo se získat informace o kurzu", Severity.Error);
            if (reThrowError)
            {
                throw new Exception(ex.Message);
            }
            return null;
        }
    }

    public async Task<Dictionary<int, CourseCln>> GetSortedCoursesAsync(
        bool reThrowError,
        FillCourseExtended? fillExtended = null
    )
    {
        try
        {
            var endLogging = await adminService.GetLoggingEndings(reThrowError);
            // logger.LogInformation($"Logging ends: {endLogging?.TimeEnding}");
            if (endLogging?.TimeEnding != null && (endLogging.TimeEnding < DateTime.Now))
            {
                notificationService.Notify("Už je po termínu přihlašování", Severity.Warning);
            }

            var userInfo = httpContextAccessor.GetCurrentUser();

            await using var ctx = _factory.CreateDbContext();

            var student = ctx.Students.SingleOrDefault(s => s.Email == userInfo.UserEmail);

            if (student == null)
            {
                throw new NullReferenceException(
                    $"Nepodařilo se najít žáka/nepodařilo se přidat žáka (email: {userInfo.UserEmail})"
                );
            }

            // načti ordering studenta
            var ordering = await ctx
                .OrderCourses.Where(o => o.StudentId == student.Id)
                .ToListAsync();

            // dostupné kurzy podle role
            var availableCourses = ctx
                .Courses.ToList()
                .Where(c => c.ForClasses.Split(";").Any(r => userInfo.UserRoles.Contains(r)));

            var result = new Dictionary<int, CourseCln>();
            int index = 0;

            // kurzy s orderingem
            foreach (var item in ordering.OrderBy(o => o.Order))
            {
                var course = availableCourses.FirstOrDefault(c => c.Id == item.CourseId);
                if (course == null)
                    continue;

                result[index++] = await course.ToClient(ctx, student, adminService, fillExtended);
            }

            // kurzy bez orderingu
            foreach (
                var course in availableCourses
                    .Where(c => !ordering.Any(o => o.CourseId == c.Id))
                    .OrderBy(a => a.Price)
            )
            {
                result[index++] = await course.ToClient(ctx, student, adminService, fillExtended);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se získat seřazení kurzů - {ex.Message}");
            notificationService.Notify("Nepodařilo se získat seřazení kurzů", Severity.Error);
            if (reThrowError)
            {
                throw new Exception(ex.Message);
            }
            return [];
        }
    }

    public async Task UpdateOrderAsync(Dictionary<int, CourseCln> input, bool reThrowError)
    {
        try
        {
            var endLogging = await adminService.GetLoggingEndings(reThrowError);
            if (endLogging?.TimeEnding != null && (endLogging.TimeEnding < DateTime.Now))
            {
                throw new Exception("Na to už je pozdě");
            }

            var userInfo = httpContextAccessor.GetCurrentUser();

            await using var ctx = _factory.CreateDbContext();

            var actualStudent = ctx.Students.SingleOrDefault(a => a.Email == userInfo.UserEmail);

            if (actualStudent == null)
            {
                throw new NullReferenceException(
                    $"Nenašel jsem aktuálního uživatele. (userEmail: {userInfo.UserEmail})"
                );
            }

            ctx.OrderCourses.RemoveRange(
                ctx.OrderCourses.Where(a => a.StudentId == actualStudent.Id)
            );
            ctx.OrderCourses.AddRange(
                input.Select(a => new OrderCourse
                {
                    CourseId = a.Value.Id,
                    Order = a.Key,
                    StudentId = actualStudent.Id,
                })
            );

            await ctx.SaveChangesAsync();
            notificationService.Notify("Aktualizoval jsem seřazení kurzů", Severity.Success);
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se aktualizovat seřazení kurzů - {ex.Message}");
            notificationService.Notify("Nepodařilo se aktualizovat seřazení kurzů", Severity.Error);
            if (reThrowError)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
