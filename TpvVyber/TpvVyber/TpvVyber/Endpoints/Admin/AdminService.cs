using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TpvVyber.Classes;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Data;

namespace TpvVyber.Endpoints.Admin;

public class ServerAdminService(IDbContextFactory<TpvVyberContext> _factory) : IAdminService
{
    #region Courses
    private async Task<CourseCln> courseAddIntern(
        CourseCln item,
        FillCourseExtended? fillExtended = null
    )
    {
        await using var ctx = _factory.CreateDbContext();
        var newEntity = Course.ToServer(item, ctx, createNew: true);
        var element = await ctx.Courses.AddAsync(newEntity);
        await ctx.SaveChangesAsync();

        return ctx.Courses.Find(element.Entity.Id)?.ToClient(ctx, fillExtended)
            ?? throw new Exception("Nepodařilo se přidat do databáze");
    }

    private async Task courseDeleteIntern(int Id)
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

    private async Task courseUpdateIntern(CourseCln item)
    {
        await using var ctx = _factory.CreateDbContext();
        var entityToUpdate = Course.ToServer(item, ctx);
        ctx.Courses.Update(entityToUpdate);
        await ctx.SaveChangesAsync();
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
        await using var ctx = _factory.CreateDbContext();
        var course = ctx.Courses.Include(r => r.OrderCourses).First(a => a.Id == id);
        if (course == null)
        {
            return null;
        }
        return course.ToClient(ctx, fillExtended);
    }

    public async Task<IEnumerable<CourseCln>> GetAllCoursesAsync(
        FillCourseExtended? fillExtended = null
    )
    {
        await using var ctx = _factory.CreateDbContext();
        return ctx
            .Courses.Include(r => r.OrderCourses)
                .ThenInclude(oc => oc.Student)
            .Select((course) => course.ToClient(ctx, fillExtended))
            .ToList();
    }
    #endregion
    #region Students
    private async Task<StudentCln> studentAddIntern(
        StudentCln item,
        FillStudentExtended? fillExtended = null
    )
    {
        await using var ctx = _factory.CreateDbContext();
        var newEntity = Student.ToServer(item, ctx, createNew: true);
        var element = await ctx.Students.AddAsync(newEntity);
        await ctx.SaveChangesAsync();

        return ctx.Students.Find(element.Entity.Id)?.ToClient(ctx, fillExtended)
            ?? throw new Exception("Nepodařilo se přidat do databáze");
    }

    private async Task studentDeleteIntern(int Id)
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
            throw new Exception("Nepodařilo se najít kurz v databázi");
        }
    }

    private async Task studentUpdateIntern(StudentCln item)
    {
        await using var ctx = _factory.CreateDbContext();
        var entityToUpdate = Student.ToServer(item, ctx);
        ctx.Students.Update(entityToUpdate);
        await ctx.SaveChangesAsync();
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
        await using var ctx = _factory.CreateDbContext();
        var student = ctx.Students.Find(id);
        if (student == null)
        {
            return null;
        }
        return student.ToClient(ctx, fillExtended);
    }

    public async Task<IEnumerable<StudentCln>> GetAllStudentsAsync(
        FillStudentExtended? fillExtended = null
    )
    {
        await using var ctx = _factory.CreateDbContext();
        return ctx
            .Students.Include(s => s.OrderCourses)
                .ThenInclude(oc => oc.Course)
            .AsEnumerable()
            .Select((student) => student.ToClient(ctx, fillExtended))
            .ToList();
    }
    #endregion
    #region OrderCourses
    private async Task<OrderCourseCln> orderCourseAddIntern(
        OrderCourseCln item,
        FillOrderCourseExtended? fillExtended = null
    )
    {
        await using var ctx = _factory.CreateDbContext();
        var newEntity = OrderCourse.ToServer(item, ctx, createNew: true);
        var element = await ctx.OrderCourses.AddAsync(newEntity);
        await ctx.SaveChangesAsync();

        return ctx.OrderCourses.Find(element.Entity.Id)?.ToClient(ctx, fillExtended)
            ?? throw new Exception("Nepodařilo se přidat do databáze");
    }

    private async Task orderCourseDeleteIntern(int Id)
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

    private async Task orderCourseUpdateIntern(OrderCourseCln item)
    {
        await using var ctx = _factory.CreateDbContext();
        var entityToUpdate = OrderCourse.ToServer(item, ctx);
        ctx.OrderCourses.Update(entityToUpdate);
        await ctx.SaveChangesAsync();
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
        await using var ctx = _factory.CreateDbContext();
        var orderCourse = ctx.OrderCourses.Find(id);
        if (orderCourse == null)
        {
            return null;
        }
        return orderCourse.ToClient(ctx, fillExtended);
    }

    public async Task<IEnumerable<OrderCourseCln>> GetAllOrderCourseAsync(
        FillOrderCourseExtended? fillExtended = null
    )
    {
        await using var ctx = _factory.CreateDbContext();
        return ctx
            .OrderCourses.Include(oc => oc.Course)
            .Include(oc => oc.Student)
            .Select((orderCourse) => orderCourse.ToClient(ctx, fillExtended))
            .ToList();
    }
    #endregion
}
