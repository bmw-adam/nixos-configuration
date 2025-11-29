namespace TpvVyber.Client.Classes;

public class OrderCourseCln
{
    public int Id { get; set; }
    public int Order { get; set; }
    public int CourseId { get; set; }
    public int StudentId { get; set; }
    public OrderCourseClnExtended? Extended { get; set; }
}

public class OrderCourseClnExtended
{
    public StudentCln? Student { get; set; }
    public CourseCln? Course { get; set; }
}

public enum FillOrderCourseExtended
{
    Student = 1,
    Course = 2,
}
