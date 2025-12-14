namespace TpvVyber.Client.Classes;

public class CourseCln
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PdfUrl { get; set; } = string.Empty;
    public string ForClasses { get; set; } = string.Empty;
    public decimal Price { get; set; } = 0;
    public CourseClnExtended? Extended { get; set; }
}

public class CourseClnExtended
{
    public List<StudentCln>? Students { get; set; }
}

public enum FillCourseExtended
{
    Students = 1,
}
