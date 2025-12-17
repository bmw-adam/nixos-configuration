using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MudBlazor;
using TpvVyber.Classes;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Data;
using TpvVyber.Extensions;
using TpvVyber.Services;

namespace TpvVyber.Endpoints.Admin;

public class ServerAdminService(
    IDbContextFactory<TpvVyberContext> _factory,
    ILogger<ServerAdminService> logger,
    NotificationService notificationService,
    IMemoryCache cache
) : IAdminService
{
    public async Task<LoggingEndingCln?> GetLoggingEndings(bool reThrowError)
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();
            var loggingEnding = ctx.LoggingEndings.FirstOrDefault();

            if (loggingEnding == null)
            {
                return null;
            }

            return await loggingEnding.ToClient(ctx, null, this, null);
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se získat konec přihlašování {ex.Message}");
            notificationService.Notify("Nepodařilo se získat konec přihlašování", Severity.Error);
            if (reThrowError)
            {
                throw new Exception(ex.Message);
            }
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
            await using var ctx = _factory.CreateDbContext();
            var loggingEndingServer = LoggingEnding.ToServer(loggingEnding, ctx, false);

            if (loggingEndingServer == null)
            {
                return null;
            }

            await ctx.SaveChangesAsync();
            return await loggingEndingServer.ToClient(ctx, null, this, null);
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se aktualizovat konec přihlašování {ex.Message}");
            notificationService.Notify(
                "Nepodařilo se aktualizovat konec přihlašování",
                Severity.Error
            );
            if (reThrowError)
            {
                throw new Exception(ex.Message);
            }
            return null;
        }
    }

    #region Courses
    private async Task<CourseCln> courseAddIntern(
        CourseCln item,
        bool reThrowError,
        FillCourseExtended? fillExtended = null
    )
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();

            var newEntity = Course.ToServer(item, ctx, createNew: true);
            var element = await ctx.Courses.AddAsync(newEntity);
            await ctx.SaveChangesAsync();

            var course =
                await ctx.Courses.FindAsync(element.Entity.Id)
                ?? throw new Exception(
                    $"Nepodařilo se přidat do databáze: kurz {JsonSerializer.Serialize(item)}"
                );

            return await course.ToClient(ctx, null, this, fillExtended);
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se přidat kurz do databáze {ex.Message}");
            notificationService.Notify("Nepodařilo se přidat kurz do databáze", Severity.Error);
            if (reThrowError)
            {
                throw new Exception(ex.Message);
            }
            return item;
        }
    }

    private async Task courseDeleteIntern(int Id, bool reThrowError)
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
            notificationService.Notify("Nepodařilo se odebrat kurz z databáze", Severity.Error);
            if (reThrowError)
            {
                throw new Exception(ex.Message);
            }
            return;
        }
    }

    private async Task courseUpdateIntern(CourseCln item, bool reThrowError)
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
            notificationService.Notify(
                "Nepodařilo se aktualizovat kurz v databázi",
                Severity.Error
            );
            if (reThrowError)
            {
                throw new Exception(ex.Message);
            }
            return;
        }
    }

    public Task<CourseCln> AddCourseAsync(
        CourseCln item,
        bool reThrowError,
        FillCourseExtended? fillExtended = null
    )
    {
        return courseAddIntern(item, reThrowError, fillExtended);
    }

    public Task DeleteCourseAsync(int Id, bool reThrowError)
    {
        return courseDeleteIntern(Id, reThrowError);
    }

    public Task UpdateCourseAsync(CourseCln item, bool reThrowError)
    {
        return courseUpdateIntern(item, reThrowError);
    }

    public async Task<CourseCln?> GetCourseByIdAsync(
        int id,
        bool reThrowError,
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
            return await course.ToClient(ctx, null, this, fillExtended);
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se získat kurz z databáze Id: {id} - {ex.Message}");
            notificationService.Notify("Nepodařilo se získat kurz z databáze", Severity.Error);
            if (reThrowError)
            {
                throw new Exception(ex.Message);
            }
            return null;
        }
    }

    public async Task<IEnumerable<CourseCln>> GetAllCoursesAsync(
        bool reThrowError,
        FillCourseExtended? fillExtended = null
    )
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();

            // Load courses and related entities into memory
            var courses = await ctx
                .Courses.Include(r => r.OrderCourses)
                    .ThenInclude(oc => oc.Student)
                .ToListAsync();

            var result = new List<CourseCln>();
            foreach (var course in courses)
            {
                // Sequentially await ToClient for safety
                result.Add(await course.ToClient(ctx, null, this, fillExtended));
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se získat kurzy z databáze - {ex.Message}");
            notificationService.Notify("Nepodařilo se získat kurzy z databáze", Severity.Error);
            if (reThrowError)
            {
                throw new Exception(ex.Message);
            }
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

            var student =
                await ctx.Students.FindAsync(element.Entity.Id)
                ?? throw new Exception(
                    $"Nepodařilo se přidat do databáze. Item: {JsonSerializer.Serialize(item)}"
                );

            return await student.ToClient(ctx, null, this, fillExtended);
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se přidat kurz do databáze {ex.Message}");
            notificationService.Notify("Nepodařilo se přidat kurz do databáze", Severity.Error);
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
            notificationService.Notify("Nepodařilo se odebrat žáka z databáze", Severity.Error);
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
            notificationService.Notify(
                "Nepodařilo se aktualizovat žáka v databázi",
                Severity.Error
            );
            return;
        }
    }

    public Task<StudentCln> AddStudentAsync(
        StudentCln item,
        bool reThrowError,
        FillStudentExtended? fillExtended = null
    )
    {
        return studentAddIntern(item, fillExtended);
    }

    public Task DeleteStudentAsync(int Id, bool reThrowError)
    {
        return studentDeleteIntern(Id);
    }

    public Task UpdateStudentAsync(StudentCln item, bool reThrowError)
    {
        return studentUpdateIntern(item);
    }

    public async Task<StudentCln?> GetStudentByIdAsync(
        int id,
        bool reThrowError,
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
            return await student.ToClient(ctx, null, this, fillExtended);
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se získat žáka z databáze Id: {id} - {ex.Message}");
            notificationService.Notify("Nepodařilo se získat žáka z databáze", Severity.Error);
            if (reThrowError)
            {
                throw new Exception(ex.Message);
            }
            return null;
        }
    }

    public async Task<IEnumerable<StudentCln>> GetAllStudentsAsync(
        bool reThrowError,
        FillStudentExtended? fillExtended = null
    )
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();

            // Load students and related entities into memory
            var students = ctx
                .Students.Include(s => s.OrderCourses)
                    .ThenInclude(oc => oc.Course)
                .AsEnumerable();

            var result = new List<StudentCln>();
            foreach (var student in students)
            {
                // Sequentially await ToClient for safety
                result.Add(await student.ToClient(ctx, null, this, fillExtended));
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se získat žáky z databáze - {ex.Message}");
            notificationService.Notify("Nepodařilo se získat žáky z databáze", Severity.Error);
            if (reThrowError)
            {
                throw new Exception(ex.Message);
            }
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

            var orderCourse =
                await ctx.OrderCourses.FindAsync(element.Entity.Id)
                ?? throw new Exception("Nepodařilo se přidat do databáze");

            return await orderCourse.ToClient(ctx, null, this, fillExtended);
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se přidat pořadí kurzu do databáze {ex.Message}");
            notificationService.Notify(
                "Nepodařilo se přidat pořadí kurzu do databáze",
                Severity.Error
            );
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
            notificationService.Notify(
                "Nepodařilo se odebrat pořadí kurzu z databáze",
                Severity.Error
            );
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
            notificationService.Notify(
                "Nepodařilo se aktualizovat pořadí kurzu v databázi",
                Severity.Error
            );
            return;
        }
    }

    public Task<OrderCourseCln> AddOrderCourseAsync(
        OrderCourseCln item,
        bool reThrowError,
        FillOrderCourseExtended? fillExtended = null
    )
    {
        return orderCourseAddIntern(item, fillExtended);
    }

    public Task DeleteOrderCourseAsync(int Id, bool reThrowError)
    {
        return orderCourseDeleteIntern(Id);
    }

    public Task UpdateOrderCourseAsync(OrderCourseCln item, bool reThrowError)
    {
        return orderCourseUpdateIntern(item);
    }

    public async Task<OrderCourseCln?> GetOrderCourseByIdAsync(
        int id,
        bool reThrowError,
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
            return await orderCourse.ToClient(ctx, null, this, fillExtended);
        }
        catch (Exception ex)
        {
            logger.LogError(
                $"Nepodařilo se získat pořadí kurzu z databáze Id: {id} - {ex.Message}"
            );
            notificationService.Notify(
                "Nepodařilo se získat pořadí kurzu z databáze",
                Severity.Error
            );
            if (reThrowError)
            {
                throw new Exception(ex.Message);
            }
            return null;
        }
    }

    public async Task<IEnumerable<OrderCourseCln>> GetAllOrderCourseAsync(
        bool reThrowError,
        FillOrderCourseExtended? fillExtended = null
    )
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();

            var orderCourses = await ctx
                .OrderCourses.Include(oc => oc.Course)
                .Include(oc => oc.Student)
                .ToListAsync();

            var result = new List<OrderCourseCln>();
            foreach (var orderCourse in orderCourses)
            {
                result.Add(await orderCourse.ToClient(ctx, null, this, fillExtended));
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se získat pořadí kurzů z databáze - {ex.Message}");
            notificationService.Notify(
                "Nepodařilo se získat pořadí kurzů z databáze",
                Severity.Error
            );
            if (reThrowError)
            {
                throw new Exception(ex.Message);
            }
            return [];
        }
    }

    private const string CacheKey = "ShowFillCourses";

    private async Task<Dictionary<int, List<StudentCln>>> ComputeFillCourses(
        FillCourseExtended? fillCourse,
        FillStudentExtended? fillStudent
    )
    {
        await using var ctx = _factory.CreateDbContext();

        var studentGroups = ctx
            .Students.Include(r => r.OrderCourses)
                .ThenInclude(r => r.Course)
            .AsEnumerable()
            .GroupBy(e => e.ClaimStrength)
            .OrderByDescending(r => r.Key);

        var courseContainers = new Dictionary<int, List<StudentCln>>(); // CourseId, Student List

        ctx.Courses.ToList().ForEach(a => courseContainers.Add(a.Id, []));

        foreach (var studentGroup in studentGroups)
        {
            var shuffledGroup = studentGroup.ToList();
            shuffledGroup.ShuffleList();

            foreach (var student in shuffledGroup.ToList())
            {
                if (student != null)
                {
                    var thisStudentsOrderings = student.OrderCourses.OrderBy(r => r.Order);
                    foreach (var wish in thisStudentsOrderings)
                    {
                        var course = wish.Course;
                        if (course == null)
                        {
                            continue;
                        }

                        var container = courseContainers[course.Id];

                        var studentsNumber = container.Count;

                        if (studentsNumber < course.Capacity)
                        {
                            // Hooray - got in
                            courseContainers[course.Id] = container
                                .Append(await student.ToClient(ctx, null, this))
                                .ToList();
                            break;
                        }
                    }
                }
            }
        }
        return courseContainers;
    }

    public async Task<Dictionary<int, List<StudentCln>>> ShowFillCourses(
        bool? forceRedo,
        bool reThrowError,
        FillCourseExtended? fillCourse,
        FillStudentExtended? fillStudent
    )
    {
        if (
            forceRedo != true
            && cache.TryGetValue(CacheKey, out Dictionary<int, List<StudentCln>>? cached)
        )
        {
            if (cached != null)
            {
                return cached;
            }
        }

        var result = await ComputeFillCourses(fillCourse, fillStudent);

        cache.Set(CacheKey, result, TimeSpan.FromSeconds(30));

        return result;
    }
    #endregion
}
