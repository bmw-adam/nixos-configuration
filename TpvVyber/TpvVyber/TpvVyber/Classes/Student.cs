namespace TpvVyber.Classes;

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public int ClaimStrength => 0; //TODO: implement claim strength calculation

    public List<OrderCourse> OrderCourses { get; } = [];
}
