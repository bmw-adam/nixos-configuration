using System;

namespace TpvVyber.Client.Classes.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class TooltipAttribute : Attribute
{
    public string Message { get; }

    public TooltipAttribute(string message)
    {
        Message = message;
    }
}
