using System.ComponentModel;
using System.Reflection;

namespace TpvVyber.Client.Extensions;

public static class ClassExtensions
{
    // Wraps data in quotes if it contains the separator
    public static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(";") || value.Contains("\n") || value.Contains("\""))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
    public static int CalculateClaimStrenght(string claimRole)
    {
        switch (claimRole.ToLower())
        {
            // FIXME Assign correct class names
            case "oktáva":
                return 9;
            case "septima":
                return 8;
            case "sexta":
                return 7;
            case "1.i":
                return 6;
            case "kvinta":
                return 5;
            case "kvarta":
                return 4;
            case "tercie":
                return 3;
            case "sekunda":
                return 2;
            case "prima":
                return 1;
            default:
                return 0;
        }
    }
}
