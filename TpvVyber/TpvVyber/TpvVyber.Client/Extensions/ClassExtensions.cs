using System.ComponentModel;
using System.Reflection;

namespace TpvVyber.Client.Extensions;

public static class ClassExtensions
{
    // Wraps data in quotes if it contains the separator
    public static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "";
        if (value.Contains(";") || value.Contains("\n") || value.Contains("\""))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }

    public static string[] GetAllClasses()
    {
        return
        [
            "oktáva",
            "4. aj",
            "4. b",
            "4. c",
            "4. l",
            "septima",
            "3. aj",
            "3. b",
            "3. c",
            "3. l",
            "sexta",
            "2. aj",
            "2. b",
            "2. c",
            "2. l",
            "1. i",
            "kvinta",
            "1. aj",
            "1. b",
            "1. c",
            "1. l",
            "kvarta",
            "tercie",
            "sekunda",
            "prima",
        ];
    }

    public static int CalculateClaimStrenght(string claimRole)
    {
        switch (claimRole.ToLower())
        {
            case "oktáva":
            case "4. aj":
            case "4. b":
            case "4. c":
            case "4. l":
                return 9;
            case "septima":
            case "3. aj":
            case "3. b":
            case "3. c":
            case "3. l":
                return 8;
            case "sexta":
            case "2. aj":
            case "2. b":
            case "2. c":
            case "2. l":
                return 7;
            case "1. i":
                return 6;
            case "kvinta":
            case "1. aj":
            case "1. b":
            case "1. c":
            case "1. l":
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
