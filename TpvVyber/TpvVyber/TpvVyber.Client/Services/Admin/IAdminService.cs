using System.Threading.Tasks;
using TpvVyber.Client.Classes;

namespace TpvVyber.Client.Services.Admin;

public interface IAdminService
{
    public Task<LoggingEndingCln?> GetLoggingEndings();
    public Task<LoggingEndingCln?> UpdateLoggingEnding(LoggingEndingCln loggingEnding);
    #region Courses
    public Task<CourseCln> AddCourseAsync(CourseCln item, FillCourseExtended? fillExtended = null);
    public Task DeleteCourseAsync(int Id);
    public Task UpdateCourseAsync(CourseCln item);
    public Task<CourseCln?> GetCourseByIdAsync(int id, FillCourseExtended? fillExtended = null);
    public Task<IEnumerable<CourseCln>> GetAllCoursesAsync(FillCourseExtended? fillExtended = null);
    #endregion
    #region Students
    public Task<StudentCln> AddStudentAsync(
        StudentCln item,
        FillStudentExtended? fillExtended = null
    );
    public Task DeleteStudentAsync(int Id);
    public Task UpdateStudentAsync(StudentCln item);
    public Task<StudentCln?> GetStudentByIdAsync(int id, FillStudentExtended? fillExtended = null);
    public Task<IEnumerable<StudentCln>> GetAllStudentsAsync(
        FillStudentExtended? fillExtended = null
    );
    #endregion
    #region OrderCourses
    public Task<OrderCourseCln> AddOrderCourseAsync(
        OrderCourseCln item,
        FillOrderCourseExtended? fillExtended = null
    );
    public Task DeleteOrderCourseAsync(int Id);
    public Task UpdateOrderCourseAsync(OrderCourseCln item);
    public Task<OrderCourseCln?> GetOrderCourseByIdAsync(
        int id,
        FillOrderCourseExtended? fillExtended = null
    );
    public Task<IEnumerable<OrderCourseCln>> GetAllOrderCourseAsync(
        FillOrderCourseExtended? fillExtended = null
    );
    public Task<Dictionary<int, List<StudentCln>>> ShowFillCourses(
        bool? forceRedo,
        FillCourseExtended? fillCourse = null,
        FillStudentExtended? fillStudent = null
    );
    #endregion
}
