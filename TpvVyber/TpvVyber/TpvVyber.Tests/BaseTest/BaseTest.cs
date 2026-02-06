using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using TpvVyber.Classes;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Client.Services.Select;
using TpvVyber.Data;
using TpvVyber.Endpoints.Admin;
using TpvVyber.Endpoints.Select;

namespace TpvVyber.Tests.BaseTest;

public class BaseApiTest
{
    public IAdminService AdminService { get; private set; }
    public ISelectService SelectService { get; private set; }
    public TestDbContextFactory DbContextFactory { get; private set; }

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<TpvVyberContext>()
            .UseInMemoryDatabase("TestDb_" + Guid.NewGuid())
            .Options;

        var dbFactory = new TestDbContextFactory(options);

        AdminService = new ServerAdminService(
            dbFactory,
            new Logger<ServerAdminService>(new SerilogLoggerFactory()),
            new NotificationService(),
            cache: new MemoryCache(new MemoryCacheOptions())
        );

        SelectService = new ServerSelectService(
            dbFactory,
            CreateTestHttpContextAccessor(),
            AdminService,
            new NotificationService(),
            new Logger<ServerSelectService>(new SerilogLoggerFactory())
        );
        DbContextFactory = dbFactory;
    }

    private IHttpContextAccessor CreateTestHttpContextAccessor()
    {
        var httpContext = new DefaultHttpContext();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "Admin"),
            new Claim(ClaimTypes.Email, "tester-tpvselect@gasos-ro.cz"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "Oktáva"),
            new Claim("description", "Admin"),
            new Claim("email", "tester-tpvselect@gasos-ro.cz"),
            new Claim("name", "Admin Tester"),
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        httpContext.User = new ClaimsPrincipal(identity);

        return new HttpContextAccessor { HttpContext = httpContext };
    }

    protected StudentCln TestStudent = new StudentCln
    {
        Name = "Admin",
        Email = "tester-tpvselect@gasos-ro.cz",
        Class = "oktáva;admin",
    };

    protected async Task CreateTestUserAsync()
    {
        var added_student = await AdminService.AddStudentAsync(TestStudent, reThrowError: true);
        TestStudent.Id = added_student.Id;
    }

    protected async Task DeleteTestUserAsync()
    {
        await AdminService.DeleteStudentAsync(TestStudent.Id, reThrowError: true);
    }

    protected List<OrderCourseCln> GenerateOrderCourses(
        List<CourseCln> courses,
        List<StudentCln> students
    )
    {
        var orderCourses = new List<OrderCourseCln>();

        foreach (
            var student in students.Select(r => r.Id).Contains(TestStudent.Id)
                ? students
                : students.Append(TestStudent)
        )
        {
            var shuffledCourses = courses
                .Where(c =>
                    c.ForClasses.Split(";")
                        .Any(r => student.Class.ToLower().Split(";").Contains(r.ToLower()))
                )
                .OrderBy(_ => Guid.NewGuid())
                .ToList();

            var thisStudentCourses = shuffledCourses
                .Select(
                    (course, index) =>
                        new OrderCourseCln
                        {
                            CourseId = course.Id,
                            StudentId = student.Id,
                            Order = index,
                        }
                )
                .ToList();
            orderCourses.AddRange(thisStudentCourses);
        }

        return orderCourses;
    }

    public static CourseCln GenerateCourse(uint max_capacity = 5) =>
        new CourseCln
        {
            Name = $"Course-{Guid.NewGuid()}",
            Description = $"Description-{Guid.NewGuid()}",
            PdfUrl = "http://example.com/sample.pdf",
            ForClasses = string.Join(
                ";",
                Enumerable.Range(1, 2).Select(_ => GenerateStudent().Class).Distinct()
            ),
            MinPrice = (new Random()).Next(100, 500),
            MaxPrice = (new Random()).Next(600, 1000),
            Capacity = max_capacity,
            MinCapacity = 1,
        };

    public static StudentCln GenerateStudent() =>
        new StudentCln
        {
            Name = $"Student-{Guid.NewGuid()}",
            Email = $"student{Guid.NewGuid()}@example.com",
            Class =
                $"{new[] { "oktáva", "septima", "sexta", "kvinta" }.OrderBy(_ => Guid.NewGuid()).First()}",
        };
}

public static class BaseApiTestExtensions
{
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        var list = new List<T>();
        await foreach (var item in source)
        {
            list.Add(item);
        }
        return list;
    }
}

public class TestDbContextFactory : IDbContextFactory<TpvVyberContext>
{
    private readonly DbContextOptions<TpvVyberContext> _options;

    public TestDbContextFactory(DbContextOptions<TpvVyberContext> options)
    {
        _options = options;
    }

    public TpvVyberContext CreateDbContext()
    {
        return new TpvVyberContext(_options);
    }
}
