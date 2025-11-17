namespace TpvVyber.Client.Classes;

public class Course
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PdfUrl { get; set; } = string.Empty;

    public List<OrderCourse> OrderCourses { get; } = [];
}
