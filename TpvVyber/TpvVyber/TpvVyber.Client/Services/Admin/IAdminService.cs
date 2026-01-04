using System.Threading.Tasks;
using TpvVyber.Client.Classes;

namespace TpvVyber.Client.Services.Admin;

public interface IAdminService
{
    public Task<LoggingEndingCln?> GetLoggingEndings(bool reThrowError);
    public Task<LoggingEndingCln?> UpdateLoggingEnding(
        LoggingEndingCln loggingEnding,
        bool reThrowError
    );
    #region Courses
    public Task<CourseCln> AddCourseAsync(
        CourseCln item,
        bool reThrowError,
        FillCourseExtended? fillExtended = null
    );
    public Task DeleteCourseAsync(int Id, bool reThrowError);
    public Task UpdateCourseAsync(CourseCln item, bool reThrowError);
    public Task<CourseCln?> GetCourseByIdAsync(
        int id,
        bool reThrowError,
        FillCourseExtended? fillExtended = null
    );
    public IAsyncEnumerable<CourseCln> GetAllCoursesAsync(
        bool reThrowError,
        FillCourseExtended? fillExtended = null
    );
    public Task<uint?> GetAllCoursesCountAsync(bool reThrowError);
    #endregion
    #region Students
    public Task<StudentCln> AddStudentAsync(
        StudentCln item,
        bool reThrowError,
        FillStudentExtended? fillExtended = null
    );
    public Task DeleteStudentAsync(int Id, bool reThrowError);
    public Task UpdateStudentAsync(StudentCln item, bool reThrowError);
    public Task<StudentCln?> GetStudentByIdAsync(
        int id,
        bool reThrowError,
        FillStudentExtended? fillExtended = null
    );
    public IAsyncEnumerable<StudentCln> GetAllStudentsAsync(
        bool reThrowError,
        FillStudentExtended? fillExtended = null
    );
    public Task<uint?> GetAllStudentsCountAsync(bool reThrowError);
    #endregion
    #region OrderCourses
    public Task<OrderCourseCln> AddOrderCourseAsync(
        OrderCourseCln item,
        bool reThrowError,
        FillOrderCourseExtended? fillExtended = null
    );
    public Task DeleteOrderCourseAsync(int Id, bool reThrowError);
    public Task UpdateOrderCourseAsync(OrderCourseCln item, bool reThrowError);
    public Task<OrderCourseCln?> GetOrderCourseByIdAsync(
        int id,
        bool reThrowError,
        FillOrderCourseExtended? fillExtended = null
    );
    public IAsyncEnumerable<OrderCourseCln> GetAllOrderCourseAsync(
        bool reThrowError,
        FillOrderCourseExtended? fillExtended = null
    );
    public Task<uint?> GetAllOrderCourseCountAsync(bool reThrowError);
    public Task<Dictionary<int, List<StudentCln>>> ShowFillCourses(
        bool? forceRedo,
        bool reThrowError,
        FillCourseExtended? fillCourse = null,
        FillStudentExtended? fillStudent = null
    );
    #endregion
    #region HistoryStudents
    public Task<HistoryStudentCourseCln> AddHistoryStudentCourseAsync(
        HistoryStudentCourseCln item,
        bool reThrowError,
        FillHistoryStudentCourseExtended? fillExtended = null
    );
    public Task DeleteHistoryStudentCourseAsync(int Id, bool reThrowError);
    public Task UpdateHistoryStudentCourseAsync(HistoryStudentCourseCln item, bool reThrowError);
    public Task<HistoryStudentCourseCln?> GetHistoryStudentCourseByIdAsync(
        int id,
        bool reThrowError,
        FillHistoryStudentCourseExtended? fillExtended = null
    );
    public IAsyncEnumerable<HistoryStudentCourseCln> GetAllHistoryStudentCourseAsync(
        bool reThrowError,
        FillHistoryStudentCourseExtended? fillExtended = null
    );
    public Task<uint?> GetAllHistoryStudentCourseCountAsync(bool reThrowError);
    #endregion
}
