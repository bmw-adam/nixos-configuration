using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using TpvVyber.Classes;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Client.Services.Select;
using TpvVyber.Data;

namespace TpvVyber.Endpoints.Select;

public class ServerSelectService(
    IDbContextFactory<TpvVyberContext> _factory,
    IHttpContextAccessor httpContextAccessor,
    IAdminService adminService,
    ISnackbar snackbarService,
    ILogger<ServerSelectService> logger
) : ISelectService
{
    public async Task<Dictionary<int, CourseCln>> GetSortedCoursesAsync(
        FillCourseExtended? fillExtended = null
    )
    {
        try
        {
            // Access user claims
            var userClaims = httpContextAccessor.HttpContext?.User.Claims;
            var userEmail = userClaims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var userName = userClaims?.FirstOrDefault(c => c.Type == "name")?.Value;

            var userRoles =
                userClaims
                    ?.Where(c => c.Type == ClaimTypes.Role || c.Type == "description")
                    ?.Select(e => e.Value) ?? [];

            if (string.IsNullOrEmpty(userName))
            {
                throw new NullReferenceException("Uživatelské jméno bylo null");
            }

            if (string.IsNullOrEmpty(userEmail))
            {
                throw new NullReferenceException("Uživatelský email byl null");
            }

            await using var ctx = _factory.CreateDbContext();

            bool alreadyExisting = ctx.Students.Any(s => s.Email == userEmail);

            if (!alreadyExisting)
            {
                var newStudent = new StudentCln
                {
                    Class = string.Join(";", userRoles),
                    Email = userEmail,
                    Name = userName,
                };

                await adminService.AddStudentAsync(newStudent);
            }

            var student = ctx.Students.SingleOrDefault(s => s.Email == userEmail);

            if (student == null)
            {
                throw new NullReferenceException(
                    $"Nepodařilo se najít žáka/nepodařilo se přidat žáka (email: {userEmail})"
                );
            }

            // načti ordering studenta
            var ordering = await ctx
                .OrderCourses.Where(o => o.StudentId == student.Id)
                .ToListAsync();

            // dostupné kurzy podle role
            var availableCourses = ctx
                .Courses.ToList()
                .Where(c => c.ForClasses.Split(";").Any(r => userRoles.Contains(r)));

            var result = new Dictionary<int, CourseCln>();
            int index = 0;

            // kurzy s orderingem
            foreach (var item in ordering.OrderBy(o => o.Order))
            {
                var course = availableCourses.FirstOrDefault(c => c.Id == item.CourseId);
                if (course == null)
                    continue;

                result[index++] = course.ToClient(ctx, fillExtended);
            }

            // kurzy bez orderingu
            foreach (
                var course in availableCourses
                    .Where(c => !ordering.Any(o => o.CourseId == c.Id))
                    .OrderBy(a => a.Price)
            )
            {
                result[index++] = course.ToClient(ctx, fillExtended);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se získat seřazení kurzů - {ex.Message}");
            snackbarService.Add("Nepodařilo se získat seřazení kurzů", Severity.Error);
            return [];
        }
    }

    public async Task UpdateOrderAsync(Dictionary<int, CourseCln> input)
    {
        try
        {
            // Access user claims
            var userClaims = httpContextAccessor.HttpContext?.User.Claims;
            var userEmail = userClaims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var userName = userClaims?.FirstOrDefault(c => c.Type == "name")?.Value;

            var userRoles =
                userClaims
                    ?.Where(c => c.Type == ClaimTypes.Role || c.Type == "description")
                    ?.Select(e => e.Value) ?? [];

            if (string.IsNullOrEmpty(userName))
            {
                throw new NullReferenceException("Uživatelské jméno bylo null");
            }

            if (string.IsNullOrEmpty(userEmail))
            {
                throw new NullReferenceException("Uživatelský email byl null");
            }

            await using var ctx = _factory.CreateDbContext();

            var actualStudent = ctx.Students.SingleOrDefault(a => a.Email == userEmail);

            if (actualStudent == null)
            {
                throw new NullReferenceException(
                    $"Nenašel jsem aktuálního uživatele. (userEmail: {userEmail})"
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
            snackbarService.Add("Aktualizoval jsem seřazení kurzů", Severity.Success);
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se aktualizovat seřazení kurzů - {ex.Message}");
            snackbarService.Add("Nepodařilo se aktualizovat seřazení kurzů", Severity.Error);
        }
    }
}
