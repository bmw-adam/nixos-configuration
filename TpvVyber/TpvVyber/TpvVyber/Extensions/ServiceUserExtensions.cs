using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TpvVyber.Classes;
using TpvVyber.Data;

namespace TpvVyber.Extensions;

public static class ServiceUserExtensions
{
    public static UserInfo GetCurrentUser(this IHttpContextAccessor accessor)
    {
        // Access user claims
        var userClaims = accessor.HttpContext?.User.Claims;
        var userEmail = userClaims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var userName = userClaims?.FirstOrDefault(c => c.Type == "name")?.Value;

        var userRoles =
            userClaims
                ?.Where(c => c.Type == ClaimTypes.Role || c.Type == "description")
                ?.Select(e => e.Value) ?? [];

        if (string.IsNullOrEmpty(userName))
        {
            throw new NullReferenceException("Uživatelské jméno bylo null");
        }

        if (string.IsNullOrEmpty(userEmail))
        {
            throw new NullReferenceException("Uživatelský email byl null");
        }

        return new UserInfo
        {
            UserEmail = userEmail,
            UserName = userName,
            UserRoles = userRoles.ToList(),
        };
    }

    public class UserInfo
    {
        public required string UserEmail { get; set; }
        public required string UserName { get; set; }
        public required List<string> UserRoles { get; set; }
    }
}
