using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TpvVyber.Classes;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Data;

namespace TpvVyber.Endpoints.Admin;

public class ServerAdminService(TpvVyberContext context) : IAdminService
{
    #region Courses
    private async Task<CourseCln> courseAddIntern(
        CourseCln item,
        FillCourseExtended? fillExtended = null
    )
    {
        var newEntity = Course.ToServer(item, context, createNew: true);
        var element = await context.Courses.AddAsync(newEntity);
        await context.SaveChangesAsync();

        return context.Courses.Find(element.Entity.Id)?.ToClient(context, fillExtended)
            ?? throw new Exception("Nepodařilo se přidat do databáze");
    }

    private async Task courseDeleteIntern(int Id)
    {
        var entityToDelete = context.Courses.Find(Id);
        if (entityToDelete != null)
        {
            context.Courses.Remove(entityToDelete);
            await context.SaveChangesAsync();
        }
        else
        {
            throw new Exception("Nepodařilo se najít kurz v databázi");
        }
    }

    private async Task courseUpdateIntern(CourseCln item)
    {
        var entityToUpdate = Course.ToServer(item, context);
        context.Courses.Update(entityToUpdate);
        await context.SaveChangesAsync();
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

    public Task<CourseCln?> GetCourseByIdAsync(int id, FillCourseExtended? fillExtended = null)
    {
        var course = context.Courses.Include(r => r.OrderCourses).First(a => a.Id == id);
        if (course == null)
        {
            return Task.FromResult<CourseCln?>(null);
        }
        return Task.FromResult<CourseCln?>(course.ToClient(context, fillExtended));
    }

    public Task<IEnumerable<CourseCln>> GetAllCoursesAsync(FillCourseExtended? fillExtended = null)
    {
        return Task.FromResult<IEnumerable<CourseCln>>(
            context
                .Courses.Include(r => r.OrderCourses)
                    .ThenInclude(oc => oc.Student)
                .Select((course) => course.ToClient(context, fillExtended))
        );
    }
    #endregion
    #region Students
    private async Task<StudentCln> studentAddIntern(
        StudentCln item,
        FillStudentExtended? fillExtended = null
    )
    {
        var newEntity = Student.ToServer(item, context, createNew: true);
        var element = await context.Students.AddAsync(newEntity);
        await context.SaveChangesAsync();

        return context.Students.Find(element.Entity.Id)?.ToClient(context, fillExtended)
            ?? throw new Exception("Nepodařilo se přidat do databáze");
    }

    private async Task studentDeleteIntern(int Id)
    {
        var entityToDelete = context.Students.Find(Id);
        if (entityToDelete != null)
        {
            context.Students.Remove(entityToDelete);
            await context.SaveChangesAsync();
        }
        else
        {
            throw new Exception("Nepodařilo se najít kurz v databázi");
        }
    }

    private async Task studentUpdateIntern(StudentCln item)
    {
        var entityToUpdate = Student.ToServer(item, context);
        context.Students.Update(entityToUpdate);
        await context.SaveChangesAsync();
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

    public Task<StudentCln?> GetStudentByIdAsync(int id, FillStudentExtended? fillExtended = null)
    {
        var student = context.Students.Find(id);
        if (student == null)
        {
            return Task.FromResult<StudentCln?>(null);
        }
        return Task.FromResult<StudentCln?>(student.ToClient(context, fillExtended));
    }

    public Task<IEnumerable<StudentCln>> GetAllStudentsAsync(
        FillStudentExtended? fillExtended = null
    )
    {
        return Task.FromResult<IEnumerable<StudentCln>>(
            context
                .Students.Include(s => s.OrderCourses)
                    .ThenInclude(oc => oc.Course)
                .AsEnumerable()
                .Select((student) => student.ToClient(context, fillExtended))
        );
    }
    #endregion
    #region OrderCourses
    private async Task<OrderCourseCln> orderCourseAddIntern(
        OrderCourseCln item,
        FillOrderCourseExtended? fillExtended = null
    )
    {
        var newEntity = OrderCourse.ToServer(item, context, createNew: true);
        var element = await context.OrderCourses.AddAsync(newEntity);
        await context.SaveChangesAsync();

        return context.OrderCourses.Find(element.Entity.Id)?.ToClient(context, fillExtended)
            ?? throw new Exception("Nepodařilo se přidat do databáze");
    }

    private async Task orderCourseDeleteIntern(int Id)
    {
        var entityToDelete = context.OrderCourses.Find(Id);
        if (entityToDelete != null)
        {
            context.OrderCourses.Remove(entityToDelete);
            await context.SaveChangesAsync();
        }
        else
        {
            throw new Exception("Nepodařilo se najít kurz v databázi");
        }
    }

    private async Task orderCourseUpdateIntern(OrderCourseCln item)
    {
        var entityToUpdate = OrderCourse.ToServer(item, context);
        context.OrderCourses.Update(entityToUpdate);
        await context.SaveChangesAsync();
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

    public Task<OrderCourseCln?> GetOrderCourseByIdAsync(
        int id,
        FillOrderCourseExtended? fillExtended = null
    )
    {
        var orderCourse = context.OrderCourses.Find(id);
        if (orderCourse == null)
        {
            return Task.FromResult<OrderCourseCln?>(null);
        }
        return Task.FromResult<OrderCourseCln?>(orderCourse.ToClient(context, fillExtended));
    }

    public Task<IEnumerable<OrderCourseCln>> GetAllOrderCourseAsync(
        FillOrderCourseExtended? fillExtended = null
    )
    {
        return Task.FromResult<IEnumerable<OrderCourseCln>>(
            context
                .OrderCourses.Include(oc => oc.Course)
                .Include(oc => oc.Student)
                .Select((orderCourse) => orderCourse.ToClient(context, fillExtended))
                .AsEnumerable()
        );
    }
    #endregion
}
