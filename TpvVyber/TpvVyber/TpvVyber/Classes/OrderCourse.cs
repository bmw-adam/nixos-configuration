using System.Threading.Tasks;
using TpvVyber.Classes.Interfaces;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Data;

namespace TpvVyber.Classes;

public class OrderCourse : IClientConvertible<OrderCourseCln, OrderCourse, FillOrderCourseExtended>
{
    public int Id { get; set; }
    public int Order { get; set; }

    public int CourseId { get; set; }
    public Course? Course { get; set; }

    public int StudentId { get; set; }
    public Student? Student { get; set; }

    public async Task<OrderCourseCln> ToClient(
        TpvVyberContext context,
        Student? currentUser,
        IAdminService adminService,
        FillOrderCourseExtended? fillExtended = null
    )
    {
        var clientObject = new OrderCourseCln
        {
            Id = Id,
            Order = Order,
            CourseId = CourseId,
            StudentId = StudentId,
        };

        if (fillExtended != null)
        {
            var extended = new OrderCourseClnExtended();

            if (fillExtended.Value.HasFlag(FillOrderCourseExtended.Student))
            {
                if (Student is not null)
                {
                    extended.Student = await Student.ToClient(context, currentUser, adminService);
                }
            }
            if (fillExtended.Value.HasFlag(FillOrderCourseExtended.Course))
            {
                if (Course is not null)
                {
                    extended.Course = await Course.ToClient(context, currentUser, adminService);
                }
            }

            clientObject.Extended = extended;
        }

        return clientObject;
    }

    public static OrderCourse ToServer(
        OrderCourseCln clientObject,
        TpvVyberContext context,
        bool createNew = false
    )
    {
        if (createNew)
        {
            return new OrderCourse
            {
                Order = clientObject.Order,
                CourseId = clientObject.CourseId,
                StudentId = clientObject.StudentId,
                Course =
                    clientObject.Extended?.Course != null
                        ? Course.ToServer(clientObject.Extended.Course, context)
                        : null,
                Student =
                    clientObject.Extended?.Student != null
                        ? Student.ToServer(clientObject.Extended.Student, context)
                        : null,
            };
        }

        var entity = context.OrderCourses.Find(clientObject.Id);
        // Apply potentional changes
        if (entity == null)
        {
            throw new Exception("Nepodařilo se najít kurz v databázi");
        }

        entity.Order = clientObject.Order;
        entity.CourseId = clientObject.CourseId;
        entity.StudentId = clientObject.StudentId;

        // FIXME check if it breaks things

        return entity;
    }
}
