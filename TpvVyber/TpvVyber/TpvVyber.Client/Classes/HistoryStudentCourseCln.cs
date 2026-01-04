using System.ComponentModel;
using TpvVyber.Client.Classes.Attributes;

namespace TpvVyber.Client.Classes;

public class HistoryStudentCourseCln
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public HistoryStudentCourseClnExtended? Extended { get; set; }
}

public class HistoryStudentCourseClnExtended
{
    public StudentCln? Student { get; set; }
    public CourseCln? Course { get; set; }
}

public enum FillHistoryStudentCourseExtended
{
    Student = 1,
    Course = 2,
}
