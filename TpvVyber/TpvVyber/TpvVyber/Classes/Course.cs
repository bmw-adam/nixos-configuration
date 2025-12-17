using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TpvVyber.Classes.Interfaces;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Data;
using TpvVyber.Extensions;

namespace TpvVyber.Classes;

public class Course : IClientConvertible<CourseCln, Course, FillCourseExtended>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PdfUrl { get; set; } = string.Empty;
    public string ForClasses { get; set; } = string.Empty;
    public decimal Price { get; set; } = 0;
    public uint Capacity { get; set; } = 0;

    public List<OrderCourse> OrderCourses { get; } = [];

    public async Task<CourseCln> ToClient(
        TpvVyberContext context,
        Student? currentUser,
        IAdminService adminService,
        FillCourseExtended? fillExtended = null
    )
    {
        var clientObject = new CourseCln
        {
            Id = Id,
            Name = Name,
            Description = Description,
            PdfUrl = PdfUrl,
            ForClasses = ForClasses,
            Price = Price,
            Capacity = Capacity,
        };

        if (fillExtended != null)
        {
            var extended = new CourseClnExtended();

            if (fillExtended.Value.HasFlag(FillCourseExtended.Students))
            {
                extended.Students = new List<StudentCln>();

                foreach (var oc in OrderCourses.Where(r => r.Student != null))
                {
                    extended.Students.Add(
                        await oc.Student!.ToClient(context, currentUser, adminService)
                    );
                }
            }
            if (fillExtended.Value.HasFlag(FillCourseExtended.OrderCourses))
            {
                extended.OrderCourses = new();

                foreach (var orderCourse in OrderCourses)
                {
                    extended.OrderCourses.Add(
                        await orderCourse.ToClient(context, currentUser, adminService, null)
                    );
                }
            }
            if (fillExtended.Value.HasFlag(FillCourseExtended.Availability) && currentUser != null)
            {
                var currentCourse = context
                    .Courses.Include(r => r.OrderCourses)
                        .ThenInclude(r => r.Student)
                    .FirstOrDefault(a => a.Id == Id);

                if (currentCourse == null)
                {
                    throw new Exception("Nemohl jsem najít aktuální kurz");
                }

                var claimingPeople = currentCourse.OrderCourses.Select(r => r.Student).Distinct();

                var courseContainers = await adminService.ShowFillCourses(false, false, null, null);

                // Can I gen in?
                // Option 1 - no
                if (
                    courseContainers[this.Id]
                        .All(e =>
                            Student.ToServer(e, context, false).ClaimStrength
                            > currentUser.ClaimStrength
                        )
                    && courseContainers[this.Id].Count >= this.Capacity
                )
                {
                    extended.Availability = Availability.Occupied;
                }
                // Option 2 - maybe
                else if (
                    courseContainers[this.Id].Count >= this.Capacity
                    && courseContainers[this.Id]
                        .Select(r => Student.ToServer(r, context, false).ClaimStrength)
                        .Min() == currentUser.ClaimStrength
                    && claimingPeople.Count() > this.Capacity
                )
                {
                    extended.Availability = Availability.Rullette;
                }
                else
                {
                    extended.Availability = Availability.Free;
                }
            }

            if (fillExtended.Value.HasFlag(FillCourseExtended.Occupied))
            {
                var tempResult = await adminService.ShowFillCourses(false, false, null, null);
                var possible = tempResult.TryGetValue(this.Id, out var list);
                extended.Occupied = possible ? list?.Count() : null;
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
                ForClasses = clientObject.ForClasses,
                Price = clientObject.Price,
                Capacity = clientObject.Capacity,
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
        entity.ForClasses = clientObject.ForClasses;
        entity.Price = clientObject.Price;
        entity.Capacity = clientObject.Capacity;

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
