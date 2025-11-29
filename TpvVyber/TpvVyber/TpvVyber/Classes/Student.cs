using TpvVyber.Classes.Interfaces;
using TpvVyber.Client.Classes;
using TpvVyber.Data;

namespace TpvVyber.Classes;

public class Student : IClientConvertible<StudentCln, Student, FillStudentExtended>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public int ClaimStrength => 0; //TODO: implement claim strength calculation

    public List<OrderCourse> OrderCourses { get; } = [];

    public StudentCln ToClient(TpvVyberContext context, FillStudentExtended? fillExtended = null)
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
                extended.Courses = OrderCourses
                    .Where(r => r.Course != null)
                    .Select(oc => oc.Course!.ToClient(context))
                    .ToList();
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
            throw new Exception("Nepodařilo se najít kurz v databázi");
        }

        entity.Name = clientObject.Name;
        entity.Email = clientObject.Email;
        entity.Class = clientObject.Class;

        // FIXME check if it breaks things

        return entity;
    }
}
