namespace TpvVyber.Classes;

public class OrderCourse
{
    public int Id { get; set; }
    public int Order { get; set; }

    public int CourseId { get; set; }
    public Course? Course { get; set; }

    public int StudentId { get; set; }
    public Student? Student { get; set; }
}
