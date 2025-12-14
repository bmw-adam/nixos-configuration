using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TpvVyber.Classes.Interfaces;
using TpvVyber.Client.Classes;
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

    public CourseCln ToClient(
        TpvVyberContext context,
        Student? currentUser,
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
                extended.Students = OrderCourses
                    .Where(r => r.Student != null)
                    .Select(oc => oc.Student!.ToClient(context, currentUser))
                    .ToList();
            }

            if (fillExtended.Value.HasFlag(FillCourseExtended.Availability) && currentUser != null)
            {
                var studentGroups = context
                    .Students.Include(r => r.OrderCourses)
                        .ThenInclude(r => r.Course)
                    .GroupBy(e => e.ClaimStrength)
                    .OrderByDescending(r => r.Key);

                var currentCourse = context
                    .Courses.Include(r => r.OrderCourses)
                        .ThenInclude(r => r.Student)
                    .FirstOrDefault(a => a.Id == Id);

                if (currentCourse == null)
                {
                    throw new Exception("Nemohl jsem najít aktuální kurz");
                }

                var claimingPeople = currentCourse.OrderCourses.Select(r => r.Student).Distinct();

                var courseContainers = new Dictionary<int, List<Student>>(); // CourseId, Student List

                foreach (var studentGroup in studentGroups)
                {
                    var shuffledGroup = studentGroup.ToList();
                    shuffledGroup.ShuffleList();

                    foreach (var student in shuffledGroup.ToList())
                    {
                        if (student != null)
                        {
                            var thisStudentsOrderings = student.OrderCourses.OrderBy(r => r.Order);
                            foreach (var wish in thisStudentsOrderings)
                            {
                                var course = wish.Course;
                                if (course == null)
                                {
                                    break;
                                }

                                var container = courseContainers[course.Id];
                                var studentsNumber = container.Count;

                                if (studentsNumber < course.Capacity)
                                {
                                    // Hooray - got in
                                    courseContainers.Add(
                                        course.Id,
                                        container.Append(student).ToList()
                                    );
                                    break;
                                }
                            }
                        }
                    }
                }

                // Can I gen in?
                // Option 1 - no
                if (
                    courseContainers[this.Id].All(e => e.ClaimStrength > currentUser.ClaimStrength)
                    && courseContainers[this.Id].Count >= this.Capacity
                )
                {
                    extended.Availability = Availability.Occupied;
                }
                // Option 2 - maybe
                else if (
                    courseContainers[this.Id].Count >= this.Capacity
                    && courseContainers[this.Id].Select(r => r.ClaimStrength).Min()
                        == currentUser.ClaimStrength
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
