namespace TpvVyber.Client.Classes;

public class StudentCln : IEntityId
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;

    public StudentClnExtended? Extended { get; set; }
}

public class StudentClnExtended
{
    public List<CourseCln>? Courses { get; set; }
    public List<OrderCourseCln>? OrderCourses { get; set; }
    public int? ClaimStrength { get; set; }
    public List<CourseCln>? AttendanceHistory { get; set; }
}

public enum FillStudentExtended
{
    Courses = 1,
    OrderCourses = 2,
    ClaimStrength = 4,
    AttendanceHistory = 8,
}
