using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using TpvVyber.Classes;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Data;

namespace TpvVyber.Endpoints.Admin;

public class ServerAdminService(
    IDbContextFactory<TpvVyberContext> _factory,
    ILogger<ServerAdminService> logger,
    ISnackbar snackbar
) : IAdminService
{
    #region Courses
    private async Task<CourseCln> courseAddIntern(
        CourseCln item,
        FillCourseExtended? fillExtended = null
    )
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();
            var newEntity = Course.ToServer(item, ctx, createNew: true);
            var element = await ctx.Courses.AddAsync(newEntity);
            await ctx.SaveChangesAsync();

            return ctx.Courses.Find(element.Entity.Id)?.ToClient(ctx, fillExtended)
                ?? throw new Exception(
                    $"Nepodařilo se přidat do databáze: kurz {JsonSerializer.Serialize(item)}"
                );
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se přidat kurz do databáze {ex.Message}");
            snackbar.Add("Nepodařilo se přidat kurz do databáze", Severity.Error);
            return item;
        }
    }

    private async Task courseDeleteIntern(int Id)
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();
            var entityToDelete = ctx.Courses.Find(Id);
            if (entityToDelete != null)
            {
                ctx.Courses.Remove(entityToDelete);
                await ctx.SaveChangesAsync();
            }
            else
            {
                throw new Exception("Nepodařilo se najít kurz v databázi");
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se odebrat kurz z databáze {Id} - {ex.Message}");
            snackbar.Add("Nepodařilo se odebrat kurz z databáze", Severity.Error);
            return;
        }
    }

    private async Task courseUpdateIntern(CourseCln item)
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();
            var entityToUpdate = Course.ToServer(item, ctx);
            ctx.Courses.Update(entityToUpdate);
            await ctx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(
                $"Nepodařilo se aktualizovat kurz v databázi {JsonSerializer.Serialize(item)} - {ex.Message}"
            );
            snackbar.Add("Nepodařilo se aktualizovat kurz v databázi", Severity.Error);
            return;
        }
    }

    public Task<CourseCln> AddCourseAsync(CourseCln item, FillCourseExtended? fillExtended = null)
    {
        return courseAddIntern(item, fillExtended);
    }

    public Task DeleteCourseAsync(int Id)
    {
        return courseDeleteIntern(Id);
    }

    public Task UpdateCourseAsync(CourseCln item)
    {
        return courseUpdateIntern(item);
    }

    public async Task<CourseCln?> GetCourseByIdAsync(
        int id,
        FillCourseExtended? fillExtended = null
    )
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();
            var course = ctx.Courses.Include(r => r.OrderCourses).First(a => a.Id == id);
            if (course == null)
            {
                return null;
            }
            return course.ToClient(ctx, fillExtended);
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se získat kurz z databáze Id: {id} - {ex.Message}");
            snackbar.Add("Nepodařilo se získat kurz z databáze", Severity.Error);
            return null;
        }
    }

    public async Task<IEnumerable<CourseCln>> GetAllCoursesAsync(
        FillCourseExtended? fillExtended = null
    )
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();
            return ctx
                .Courses.Include(r => r.OrderCourses)
                    .ThenInclude(oc => oc.Student)
                .Select((course) => course.ToClient(ctx, fillExtended))
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se získat kurzy z databáze - {ex.Message}");
            snackbar.Add("Nepodařilo se získat kurzy z databáze", Severity.Error);
            return [];
        }
    }
    #endregion
    #region Students
    private async Task<StudentCln> studentAddIntern(
        StudentCln item,
        FillStudentExtended? fillExtended = null
    )
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();
            var newEntity = Student.ToServer(item, ctx, createNew: true);
            var element = await ctx.Students.AddAsync(newEntity);
            await ctx.SaveChangesAsync();

            return ctx.Students.Find(element.Entity.Id)?.ToClient(ctx, fillExtended)
                ?? throw new Exception(
                    $"Nepodařilo se přidat do databáze. Item: {JsonSerializer.Serialize(item)}"
                );
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se přidat kurz do databáze {ex.Message}");
            snackbar.Add("Nepodařilo se přidat kurz do databáze", Severity.Error);
            return item;
        }
    }

    private async Task studentDeleteIntern(int Id)
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();
            var entityToDelete = ctx.Students.Find(Id);
            if (entityToDelete != null)
            {
                ctx.Students.Remove(entityToDelete);
                await ctx.SaveChangesAsync();
            }
            else
            {
                throw new Exception("Nepodařilo se najít žáka v databázi");
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se odebrat žáka z databáze {Id} - {ex.Message}");
            snackbar.Add("Nepodařilo se odebrat žáka z databáze", Severity.Error);
            return;
        }
    }

    private async Task studentUpdateIntern(StudentCln item)
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();
            var entityToUpdate = Student.ToServer(item, ctx);
            ctx.Students.Update(entityToUpdate);
            await ctx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(
                $"Nepodařilo se aktualizovat žáka v databázi {JsonSerializer.Serialize(item)} - {ex.Message}"
            );
            snackbar.Add("Nepodařilo se aktualizovat žáka v databázi", Severity.Error);
            return;
        }
    }

    public Task<StudentCln> AddStudentAsync(
        StudentCln item,
        FillStudentExtended? fillExtended = null
    )
    {
        return studentAddIntern(item, fillExtended);
    }

    public Task DeleteStudentAsync(int Id)
    {
        return studentDeleteIntern(Id);
    }

    public Task UpdateStudentAsync(StudentCln item)
    {
        return studentUpdateIntern(item);
    }

    public async Task<StudentCln?> GetStudentByIdAsync(
        int id,
        FillStudentExtended? fillExtended = null
    )
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();
            var student = ctx.Students.Find(id);
            if (student == null)
            {
                return null;
            }
            return student.ToClient(ctx, fillExtended);
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se získat žáka z databáze Id: {id} - {ex.Message}");
            snackbar.Add("Nepodařilo se získat žáka z databáze", Severity.Error);
            return null;
        }
    }

    public async Task<IEnumerable<StudentCln>> GetAllStudentsAsync(
        FillStudentExtended? fillExtended = null
    )
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();
            return ctx
                .Students.Include(s => s.OrderCourses)
                    .ThenInclude(oc => oc.Course)
                .AsEnumerable()
                .Select((student) => student.ToClient(ctx, fillExtended))
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se získat žáky z databáze - {ex.Message}");
            snackbar.Add("Nepodařilo se získat žáky z databáze", Severity.Error);
            return [];
        }
    }
    #endregion
    #region OrderCourses
    private async Task<OrderCourseCln> orderCourseAddIntern(
        OrderCourseCln item,
        FillOrderCourseExtended? fillExtended = null
    )
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();
            var newEntity = OrderCourse.ToServer(item, ctx, createNew: true);
            var element = await ctx.OrderCourses.AddAsync(newEntity);
            await ctx.SaveChangesAsync();

            return ctx.OrderCourses.Find(element.Entity.Id)?.ToClient(ctx, fillExtended)
                ?? throw new Exception("Nepodařilo se přidat do databáze");
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se přidat pořadí kurzu do databáze {ex.Message}");
            snackbar.Add("Nepodařilo se přidat pořadí kurzu do databáze", Severity.Error);
            return item;
        }
    }

    private async Task orderCourseDeleteIntern(int Id)
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();
            var entityToDelete = ctx.OrderCourses.Find(Id);
            if (entityToDelete != null)
            {
                ctx.OrderCourses.Remove(entityToDelete);
                await ctx.SaveChangesAsync();
            }
            else
            {
                throw new Exception("Nepodařilo se najít kurz v databázi");
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se odebrat pořadí kurzu z databáze {Id} - {ex.Message}");
            snackbar.Add("Nepodařilo se odebrat pořadí kurzu z databáze", Severity.Error);
            return;
        }
    }

    private async Task orderCourseUpdateIntern(OrderCourseCln item)
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();
            var entityToUpdate = OrderCourse.ToServer(item, ctx);
            ctx.OrderCourses.Update(entityToUpdate);
            await ctx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(
                $"Nepodařilo se aktualizovat pořadí kurzu v databázi {JsonSerializer.Serialize(item)} - {ex.Message}"
            );
            snackbar.Add("Nepodařilo se aktualizovat pořadí kurzu v databázi", Severity.Error);
            return;
        }
    }

    public Task<OrderCourseCln> AddOrderCourseAsync(
        OrderCourseCln item,
        FillOrderCourseExtended? fillExtended = null
    )
    {
        return orderCourseAddIntern(item, fillExtended);
    }

    public Task DeleteOrderCourseAsync(int Id)
    {
        return orderCourseDeleteIntern(Id);
    }

    public Task UpdateOrderCourseAsync(OrderCourseCln item)
    {
        return orderCourseUpdateIntern(item);
    }

    public async Task<OrderCourseCln?> GetOrderCourseByIdAsync(
        int id,
        FillOrderCourseExtended? fillExtended = null
    )
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();
            var orderCourse = ctx.OrderCourses.Find(id);
            if (orderCourse == null)
            {
                return null;
            }
            return orderCourse.ToClient(ctx, fillExtended);
        }
        catch (Exception ex)
        {
            logger.LogError(
                $"Nepodařilo se získat pořadí kurzu z databáze Id: {id} - {ex.Message}"
            );
            snackbar.Add("Nepodařilo se získat pořadí kurzu z databáze", Severity.Error);
            return null;
        }
    }

    public async Task<IEnumerable<OrderCourseCln>> GetAllOrderCourseAsync(
        FillOrderCourseExtended? fillExtended = null
    )
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();
            return ctx
                .OrderCourses.Include(oc => oc.Course)
                .Include(oc => oc.Student)
                .Select((orderCourse) => orderCourse.ToClient(ctx, fillExtended))
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se získat pořadí kurzů z databáze - {ex.Message}");
            snackbar.Add("Nepodařilo se získat pořadí kurzů z databáze", Severity.Error);
            return [];
        }
    }
    #endregion
}
