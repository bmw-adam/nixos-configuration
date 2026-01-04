using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TpvVyber.Classes.Interfaces;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Data;
using TpvVyber.Extensions;

namespace TpvVyber.Classes;

public class HistoryStudentCourse
    : IClientConvertible<
        HistoryStudentCourseCln,
        HistoryStudentCourse,
        FillHistoryStudentCourseExtended
    >
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }

    public async Task<HistoryStudentCourseCln> ToClient(
        TpvVyberContext context,
        Student? currentUser,
        IAdminService adminService,
        FillHistoryStudentCourseExtended? fillExtended = null
    )
    {
        var clientObject = new HistoryStudentCourseCln
        {
            Id = Id,
            CourseId = CourseId,
            StudentId = StudentId,
        };

        if (fillExtended != null)
        {
            var extended = new HistoryStudentCourseClnExtended();

            if (
                fillExtended.Value.HasFlag(FillHistoryStudentCourseExtended.Student)
                && Student is not null
            )
            {
                extended.Student = await Student.ToClient(context, currentUser, adminService);
            }
            if (
                fillExtended.Value.HasFlag(FillHistoryStudentCourseExtended.Course)
                && Course is not null
            )
            {
                extended.Course = await Course.ToClient(context, currentUser, adminService);
            }

            clientObject.Extended = extended;
        }

        return clientObject;
    }

    public static HistoryStudentCourse ToServer(
        HistoryStudentCourseCln clientObject,
        TpvVyberContext context,
        bool createNew = false
    )
    {
        if (createNew)
        {
            return new HistoryStudentCourse
            {
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

        var entity = context.HistoryStudentCourses.Find(clientObject.Id);
        // Apply potentional changes
        if (entity == null)
        {
            throw new Exception("Nepodařilo se najít kurz v databázi");
        }

        entity.CourseId = clientObject.CourseId;
        entity.StudentId = clientObject.StudentId;

        return entity;
    }
}
