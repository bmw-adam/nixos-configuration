using System;
using System.Collections;
using System.Threading.Tasks;
using TpvVyber.Client.Classes;

namespace TpvVyber.Client.Services.Select;

public interface ISelectService
{
    /// <summary>
    /// Get Sorted List of courses for certain user
    /// </summary>
    /// <returns>Dictionary <index> <course></returns>
    public Task<Dictionary<int, CourseCln>> GetSortedCoursesAsync(
        FillCourseExtended? fillExtended = null
    );

    /// <summary>
    /// Update Order Of A User. Enter Dictionary <index> <course>
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public Task UpdateOrderAsync(Dictionary<int, CourseCln> input);

    /// <summary>
    /// Get Course Info
    /// </summary>
    /// <param name="id"></param>
    /// <param name="fillExtended"></param>
    /// <returns></returns>
    public Task<CourseCln?> GetCourseInfo(int id, FillCourseExtended? fillExtended = null);
}
