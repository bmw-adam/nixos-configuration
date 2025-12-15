using System.ComponentModel;
using System.Reflection;

namespace TpvVyber.Client.Extensions;

public static class EnumsExtensions
{
    public static string GetDescription(this Enum e)
    {
        var type = e.GetType();
        var member = type.GetMember(e.ToString());

        if (member.Length > 0)
        {
            var attribute = member[0].GetCustomAttribute<DescriptionAttribute>();
            if (attribute != null)
            {
                return attribute.Description;
            }
        }

        // Fallback if no [Description] attribute is present
        return e.ToString();
    }
}
