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
    IMemoryCache cache,
    RerunFillCoursesService rerunFillCoursesService
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
            var course = ctx
                .Courses.Include(r => r.OrderCourses)
                .Include(t => t.HistoryStudentCourses)
                .First(a => a.Id == id);
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

    public async IAsyncEnumerable<CourseCln> GetAllCoursesAsync(
        bool reThrowError,
        FillCourseExtended? fillExtended = null
    )
    {
        await using var ctx = _factory.CreateDbContext();

        // 1. Prepare the query (No execution yet)
        var coursesQuery = ctx
            .Courses.Include(r => r.OrderCourses)
                .ThenInclude(oc => oc.Student)
            .Include(t => t.HistoryStudentCourses)
            .AsAsyncEnumerable();

        // 2. Use manual enumeration to allow error handling around DB calls
        await using var enumerator = coursesQuery.GetAsyncEnumerator();
        bool hasNext = true;

        while (hasNext)
        {
            CourseCln? resultItem = null;

            try
            {
                // Execute DB fetch for the next item
                hasNext = await enumerator.MoveNextAsync();

                if (hasNext)
                {
                    var course = enumerator.Current;
                    // Execute conversion logic
                    resultItem = await course.ToClient(ctx, null, this, fillExtended);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Nepodařilo se získat kurzy z databáze - {ex.Message}");
                notificationService.Notify("Nepodařilo se získat kurzy z databáze", Severity.Error);

                if (reThrowError)
                {
                    throw new Exception(ex.Message);
                }

                // If an error occurs (DB or Mapping), we stop the stream here.
                // Items yielded previously are preserved.
                yield break;
            }

            // 3. Yield result (Must be outside try/catch)
            if (resultItem != null)
            {
                yield return resultItem;
            }
        }
    }

    public async Task<uint?> GetAllCoursesCountAsync(bool reThrowError)
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();
            var count = ctx.Courses.Count();
            return (uint)Math.Abs(count);
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se počet kurzů z databáze - {ex.Message}");
            notificationService.Notify("Nepodařilo se počet kurzů z databáze", Severity.Error);
            if (reThrowError)
            {
                throw new Exception(ex.Message);
            }
            return null;
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

    public async IAsyncEnumerable<StudentCln> GetAllStudentsAsync(
        bool reThrowError,
        FillStudentExtended? fillExtended = null
    )
    {
        await using var ctx = _factory.CreateDbContext();

        var studentsQuery = ctx
            .Students.Include(t => t.HistoryStudentCourses)
            .Include(s => s.OrderCourses)
                .ThenInclude(oc => oc.Course)
            .AsAsyncEnumerable();

        await using var enumerator = studentsQuery.GetAsyncEnumerator();

        bool hasNext = true;
        while (hasNext)
        {
            StudentCln? clientItem = null;
            try
            {
                hasNext = await enumerator.MoveNextAsync();

                if (hasNext)
                {
                    var student = enumerator.Current;
                    clientItem = await student.ToClient(ctx, null, this, fillExtended);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Nepodařilo se získat žáky z databáze - {ex.Message}");
                notificationService.Notify("Nepodařilo se získat žáky z databáze", Severity.Error);

                if (reThrowError)
                {
                    throw new Exception(ex.Message);
                }

                yield break;
            }

            if (clientItem != null)
            {
                yield return clientItem;
            }
        }
    }

    public async Task<uint?> GetAllStudentsCountAsync(bool reThrowError)
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();
            var count = ctx.Students.Count();
            return (uint)Math.Abs(count);
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se počet kurzů z databáze - {ex.Message}");
            notificationService.Notify("Nepodařilo se počet kurzů z databáze", Severity.Error);
            if (reThrowError)
            {
                throw new Exception(ex.Message);
            }
            return null;
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

    public async IAsyncEnumerable<OrderCourseCln> GetAllOrderCourseAsync(
        bool reThrowError,
        FillOrderCourseExtended? fillExtended = null
    )
    {
        await using var ctx = _factory.CreateDbContext();

        await foreach (
            var orderCourse in ctx
                .OrderCourses.Include(oc => oc.Course)
                .Include(oc => oc.Student)
                .AsAsyncEnumerable()
        )
        {
            OrderCourseCln? resultItem = null;

            try
            {
                resultItem = await orderCourse.ToClient(ctx, null, this, fillExtended);
            }
            catch (Exception ex)
            {
                HandleError(ex, reThrowError);
                yield break;
            }

            if (resultItem != null)
            {
                yield return resultItem;
            }
        }

        void HandleError(Exception ex, bool shouldThrow)
        {
            logger.LogError($"Nepodařilo se získat pořadí kurzů z databáze - {ex.Message}");
            notificationService.Notify(
                "Nepodařilo se získat pořadí kurzů z databáze",
                Severity.Error
            );

            if (shouldThrow)
            {
                throw new Exception(ex.Message);
            }
        }
    }

    public async Task<uint?> GetAllOrderCourseCountAsync(bool reThrowError)
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();
            var count = ctx.OrderCourses.Count();
            return (uint)Math.Abs(count);
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se počet kurzů z databáze - {ex.Message}");
            notificationService.Notify("Nepodařilo se počet kurzů z databáze", Severity.Error);
            if (reThrowError)
            {
                throw new Exception(ex.Message);
            }
            return null;
        }
    }
    #endregion

    private const string CacheKey = "ShowFillCourses";

    public async Task<Dictionary<int, List<StudentCln>>> ShowFillCourses(
        bool? forceRedo,
        bool reThrowError,
        FillCourseExtended? fillCourse,
        FillStudentExtended? fillStudent
    )
    {
        await using var ctx = _factory.CreateDbContext();

        // Check Cache
        if (
            forceRedo != true
            && cache.TryGetValue(CacheKey, out Dictionary<int, List<StudentCln>>? cached)
            && !rerunFillCoursesService.Rerun
        )
        {
            if (cached != null)
            {
                return cached;
            }
        }

        // Fetch all data ONCE (Reduce I/O)
        var allCoursesDict = await ctx.Courses.AsNoTracking().ToDictionaryAsync(c => c.Id);

        // Fetch students with included orders
        var allStudents = await ctx
            .Students.AsNoTracking() // Faster if we aren't modifying them
            .Include(r => r.OrderCourses)
            .Include(t => t.HistoryStudentCourses)
            .ToListAsync();

        // 3. Pre-process Students
        // Group them once. We will iterate this structure multiple times in memory.
        // Note: We materialize the groups to avoid re-evaluating the GroupBy logic.
        var studentGroups = allStudents
            .GroupBy(e => e.ClaimStrength)
            .OrderByDescending(r => r.Key)
            .Select(g =>
            {
                return g.OrderBy(a => Guid.NewGuid()).ToList();
            })
            .ToList();

        // 4. Algorithm Loop (Replace Recursion)
        var excludedCourseIds = new HashSet<int>();
        Dictionary<int, List<Student>>? finalAllocation = null;
        bool isStable = false;

        // Safety break to prevent infinite loops if logic is flawed
        int maxIterations = allCoursesDict.Count + 1;
        int currentIteration = 0;

        while (!isStable && currentIteration < maxIterations)
        {
            currentIteration++;

            // Run the allocation logic in-memory
            finalAllocation = RunAllocationInMemory(
                studentGroups,
                allCoursesDict,
                excludedCourseIds
            );

            // Check MinCapacity constraints
            var coursesToExclude = new List<int>();

            foreach (var kvp in finalAllocation)
            {
                var courseId = kvp.Key;
                var assignedStudents = kvp.Value;

                // Fast lookup
                if (allCoursesDict.TryGetValue(courseId, out var courseMeta))
                {
                    if (assignedStudents.Count < courseMeta.MinCapacity)
                    {
                        coursesToExclude.Add(courseId);
                    }
                }
            }

            if (coursesToExclude.Count == 0)
            {
                // Logic satisfied
                isStable = true;
            }
            else
            {
                // Add failed courses to exclusion list and RERUN loop
                foreach (var id in coursesToExclude)
                {
                    excludedCourseIds.Add(id);
                }
            }
        }

        // 5. Convert to Client Objects (Final Result Construction)
        var resultDict = new Dictionary<int, List<StudentCln>>();

        // If finalAllocation is somehow null (no students), handle gracefully
        if (finalAllocation != null)
        {
            foreach (var kvp in finalAllocation)
            {
                // Only return courses that actually have students
                if (kvp.Value.Count > 0)
                {
                    var clientList = new List<StudentCln>(kvp.Value.Count);
                    foreach (var student in kvp.Value)
                    {
                        // Pass the existing Context to ToClient to avoid creating new connections
                        clientList.Add(await student.ToClient(ctx, null, this, fillStudent));
                    }
                    resultDict.Add(kvp.Key, clientList);
                }
            }
        }

        // cache.Set(CacheKey, resultDict);
        cache.Set(CacheKey, resultDict, TimeSpan.FromSeconds(30));
        rerunFillCoursesService.Rerun = false;
        return resultDict;
    }

    // Pure in-memory logic. No DB calls here.
    private Dictionary<int, List<Student>> RunAllocationInMemory(
        List<List<Student>> groupedStudents,
        Dictionary<int, Course> allCourses,
        HashSet<int> excludedCourseIds
    )
    {
        // Initialize containers for valid courses
        var courseContainers = new Dictionary<int, List<Student>>();

        foreach (var courseId in allCourses.Keys)
        {
            if (!excludedCourseIds.Contains(courseId))
            {
                // Pre-allocate List capacity if possible to avoid resizing,
                // though exact size is unknown, standard default is fine.
                courseContainers.Add(
                    courseId,
                    new List<Student>(groupedStudents.SelectMany(r => r).Count())
                );
                // courseContainers[courseId] = new List<Student>();
            }
        }

        foreach (var group in groupedStudents)
        {
            // Shuffle the list in-place (avoid creating new lists if possible)
            // Assuming ShuffleList() is an extension method you have.
            // We create a copy to shuffle so we don't mess up the original order for the next iteration
            var currentGroupProcessing = new List<Student>(group);
            currentGroupProcessing.ShuffleList();

            foreach (var student in currentGroupProcessing)
            {
                // Filter orders in memory
                var validOrders = student
                    .OrderCourses.Where(w => !excludedCourseIds.Contains(w.CourseId))
                    .OrderBy(r => r.Order);

                foreach (var wish in validOrders)
                {
                    // Verify course exists in our scope (it should)
                    if (courseContainers.TryGetValue(wish.CourseId, out var container))
                    {
                        var capacity = allCourses[wish.CourseId].Capacity;

                        if (container.Count < capacity)
                        {
                            // Optimization: .Add is O(1).
                            container.Add(student);
                            break; // Student placed, move to next student
                        }
                    }
                }
            }
        }

        return courseContainers;
    }

    private async Task<HistoryStudentCourseCln> historyStudentAddIntern(
        HistoryStudentCourseCln item,
        FillHistoryStudentCourseExtended? fillExtended = null
    )
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();

            var newEntity = HistoryStudentCourse.ToServer(item, ctx, createNew: true);
            var element = await ctx.HistoryStudentCourses.AddAsync(newEntity);
            await ctx.SaveChangesAsync();

            var historyStud =
                await ctx.HistoryStudentCourses.FindAsync(element.Entity.Id)
                ?? throw new Exception("Nepodařilo se přidat do databáze");

            return await historyStud.ToClient(ctx, null, this, fillExtended);
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se přidat historii žáka do databáze {ex.Message}");
            notificationService.Notify(
                "Nepodařilo se přidat historii žáka do databáze",
                Severity.Error
            );
            return item;
        }
    }

    public Task<HistoryStudentCourseCln> AddHistoryStudentCourseAsync(
        HistoryStudentCourseCln item,
        bool reThrowError,
        FillHistoryStudentCourseExtended? fillExtended = null
    )
    {
        return historyStudentAddIntern(item, fillExtended);
    }

    private async Task historyStudentDeleteIntern(int Id)
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
                throw new Exception("Nepodařilo se najít historii žáka v databázi");
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se odebrat historii žáka z databáze {Id} - {ex.Message}");
            notificationService.Notify(
                "Nepodařilo se odebrat historii žáka z databáze",
                Severity.Error
            );
            return;
        }
    }

    public Task DeleteHistoryStudentCourseAsync(int Id, bool reThrowError)
    {
        return historyStudentDeleteIntern(Id);
    }

    private async Task historyStudentUpdateIntern(HistoryStudentCourseCln item)
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();
            var entityToUpdate = HistoryStudentCourse.ToServer(item, ctx);
            ctx.HistoryStudentCourses.Update(entityToUpdate);
            await ctx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(
                $"Nepodařilo se aktualizovat historii žáků v databázi {JsonSerializer.Serialize(item)} - {ex.Message}"
            );
            notificationService.Notify(
                "Nepodařilo se aktualizovat historii žáků v databázi",
                Severity.Error
            );
            return;
        }
    }

    public Task UpdateHistoryStudentCourseAsync(HistoryStudentCourseCln item, bool reThrowError)
    {
        return historyStudentUpdateIntern(item);
    }

    public async Task<HistoryStudentCourseCln?> GetHistoryStudentCourseByIdAsync(
        int id,
        bool reThrowError,
        FillHistoryStudentCourseExtended? fillExtended = null
    )
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();
            var orderCourse = ctx.HistoryStudentCourses.Find(id);
            if (orderCourse == null)
            {
                return null;
            }
            return await orderCourse.ToClient(ctx, null, this, fillExtended);
        }
        catch (Exception ex)
        {
            logger.LogError(
                $"Nepodařilo se získat historii žáků z databáze Id: {id} - {ex.Message}"
            );
            notificationService.Notify(
                "Nepodařilo se získat historii žáků z databáze",
                Severity.Error
            );
            if (reThrowError)
            {
                throw new Exception(ex.Message);
            }
            return null;
        }
    }

    public async IAsyncEnumerable<HistoryStudentCourseCln> GetAllHistoryStudentCourseAsync(
        bool reThrowError,
        FillHistoryStudentCourseExtended? fillExtended = null
    )
    {
        await using var ctx = _factory.CreateDbContext();

        await foreach (
            var historyStudent in ctx
                .HistoryStudentCourses.Include(oc => oc.Course)
                .Include(oc => oc.Student)
                .AsAsyncEnumerable()
        )
        {
            HistoryStudentCourseCln? resultItem = null;

            try
            {
                resultItem = await historyStudent.ToClient(ctx, null, this, fillExtended);
            }
            catch (Exception ex)
            {
                HandleError(ex, reThrowError);
                yield break;
            }

            if (resultItem != null)
            {
                yield return resultItem;
            }
        }

        void HandleError(Exception ex, bool shouldThrow)
        {
            logger.LogError($"Nepodařilo se získat historii žáků z databáze - {ex.Message}");
            notificationService.Notify(
                "Nepodařilo se získat historii žáků z databáze",
                Severity.Error
            );

            if (shouldThrow)
            {
                throw new Exception(ex.Message);
            }
        }
    }

    public async Task<uint?> GetAllHistoryStudentCourseCountAsync(bool reThrowError)
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();
            var count = ctx.HistoryStudentCourses.Count();
            return (uint)Math.Abs(count);
        }
        catch (Exception ex)
        {
            logger.LogError($"Nepodařilo se získat historii žáků z databáze - {ex.Message}");
            notificationService.Notify(
                "Nepodařilo se získat historii žáků z databáze",
                Severity.Error
            );
            if (reThrowError)
            {
                throw new Exception(ex.Message);
            }
            return null;
        }
    }
}
