using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TpvVyber.Classes.Interfaces;
using TpvVyber.Client.Classes;
using TpvVyber.Data;

namespace TpvVyber.Classes;

public class Course : IClientConvertible<CourseCln, Course, FillCourseExtended>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PdfUrl { get; set; } = string.Empty;

    public List<OrderCourse> OrderCourses { get; } = [];

    public CourseCln ToClient(TpvVyberContext context, FillCourseExtended? fillExtended = null)
    {
        var clientObject = new CourseCln
        {
            Id = Id,
            Name = Name,
            Description = Description,
            PdfUrl = PdfUrl,
        };

        if (fillExtended != null)
        {
            var extended = new CourseClnExtended();

            if (fillExtended.Value.HasFlag(FillCourseExtended.Students))
            {
                extended.Students = OrderCourses
                    .Where(r => r.Student != null)
                    .Select(oc => oc.Student!.ToClient(context))
                    .ToList();
            }

            clientObject.Extended = extended;
        }

        return clientObject;
    }

    public static Course ToServer(
        CourseCln clientObject,
        TpvVyberContext context,
        bool createNew = false
    )
    {
        if (createNew)
        {
            return new Course
            {
                Name = clientObject.Name,
                Description = clientObject.Description,
                PdfUrl = clientObject.PdfUrl,
            };
        }

        var entity = context.Courses.Find(clientObject.Id);
        // Apply potentional changes
        if (entity == null)
        {
            throw new Exception("Nepodařilo se najít kurz v databázi");
        }

        entity.Name = clientObject.Name;
        entity.Description = clientObject.Description;
        entity.PdfUrl = clientObject.PdfUrl;

        // FIXME check if it breaks things
        // if (clientObject.Extended?.OrderCourses != null)
        // {
        //     // Update OrderCourses if Extended data is provided
        //     entity.OrderCourses.Clear();
        //     foreach (var orderCourseCln in clientObject.Extended.OrderCourses)
        //     {
        //         var orderCourseEntity = context.OrderCourses.Find(orderCourseCln.Id);
        //         if (orderCourseEntity != null)
        //         {
        //             entity.OrderCourses.Add(orderCourseEntity);
        //         }
        //     }
        // }

        return entity;
    }
}
