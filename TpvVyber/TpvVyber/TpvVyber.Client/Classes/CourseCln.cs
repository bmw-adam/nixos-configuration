using System.ComponentModel;
using TpvVyber.Client.Classes.Attributes;

namespace TpvVyber.Client.Classes;

public class CourseCln : IEntityId
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PdfUrl { get; set; } = string.Empty;
    public string ForClasses { get; set; } = string.Empty;
    public decimal MinPrice { get; set; } = 0;
    public decimal MaxPrice { get; set; } = 0;
    public uint Capacity { get; set; } = 0;
    public uint MinCapacity { get; set; } = 0;
    public CourseClnExtended? Extended { get; set; }
}

public class CourseClnExtended
{
    public List<StudentCln>? Students { get; set; }
    public List<OrderCourseCln>? OrderCourses { get; set; }
    public Availability? Availability { get; set; }
    public int? Occupied { get; set; }
}

public enum FillCourseExtended
{
    Students = 1,
    Availability = 2,
    Occupied = 4,
    OrderCourses = 8,
}

public enum Availability
{
    [Description("Volno - Bude se konat")]
    [Tooltip("Volno")]
    Free = 1,

    [Description("Ruleta - O vaše místo se bude losovat")]
    [Tooltip("Ruleta")]
    Rullette = 2,

    [Description("Obsazeno - Nemáte šanci")]
    [Tooltip("Obsazeno")]
    Occupied = 3,

    [Description("Volno - Ale nezaplněno minimální množství žáků")]
    [Tooltip("Volno")]
    NotHappening = 4,
}
