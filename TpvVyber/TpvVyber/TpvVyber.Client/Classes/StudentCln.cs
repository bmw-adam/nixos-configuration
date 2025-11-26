namespace TpvVyber.Client.Classes;

public class StudentCln
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public int ClaimStrength => 0;

    public StudentClnExtended? Extended { get; set; }
}

public class StudentClnExtended
{
    public List<OrderCourseCln>? OrderCourses { get; set; }
}
