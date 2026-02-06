using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using TpvVyber.Classes.Interfaces;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Extensions;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Data;

namespace TpvVyber.Classes;

public class Student : IClientConvertible<StudentCln, Student, FillStudentExtended>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;

    [NotMapped]
    public int ClaimStrength
    {
        get => Class.Split(";").Select(r => ClassExtensions.CalculateClaimStrenght(r)).Max();
    }

    public List<OrderCourse> OrderCourses { get; } = [];
    public List<HistoryStudentCourse> HistoryStudentCourses { get; } = [];

    public async Task<StudentCln> ToClient(
        TpvVyberContext context,
        Student? currentUser,
        IAdminService adminService,
        FillStudentExtended? fillExtended = null
    )
    {
        var clientObject = new StudentCln
        {
            Id = Id,
            Name = Name,
            Email = Email,
            Class = Class,
        };

        if (fillExtended != null)
        {
            var extended = new StudentClnExtended();

            if (fillExtended.Value.HasFlag(FillStudentExtended.Courses))
            {
                extended.Courses = new List<CourseCln>();
                foreach (var oc in OrderCourses.Where(oc => oc.Course != null))
                {
                    extended.Courses.Add(
                        await oc.Course!.ToClient(context, currentUser, adminService)
                    );
                }
            }

            if (fillExtended.Value.HasFlag(FillStudentExtended.OrderCourses))
            {
                extended.OrderCourses = new();

                foreach (var orderCourse in OrderCourses)
                {
                    extended.OrderCourses.Add(
                        await orderCourse.ToClient(context, currentUser, adminService, null)
                    );
                }
            }

            if (fillExtended.Value.HasFlag(FillStudentExtended.ClaimStrength))
            {
                extended.ClaimStrength = ClaimStrength;
            }

            clientObject.Extended = extended;
        }

        return clientObject;
    }

    public static Student ToServer(
        StudentCln clientObject,
        TpvVyberContext context,
        bool createNew = false
    )
    {
        if (createNew)
        {
            return new Student
            {
                Name = clientObject.Name,
                Email = clientObject.Email,
                Class = clientObject.Class,
            };
        }

        var entity = context.Students.Find(clientObject.Id);
        // Apply potentional changes
        if (entity == null)
        {
            throw new Exception("Nepodařilo se najít studenta v databázi");
        }

        entity.Name = clientObject.Name;
        entity.Email = clientObject.Email;
        entity.Class = clientObject.Class;

        // FIXME check if it breaks things

        return entity;
    }
}
