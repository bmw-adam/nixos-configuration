using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TpvVyber.Client.Classes.Client;

public class Access
{
    [JsonPropertyName("view")]
    public bool View { get; set; }

    [JsonPropertyName("configure")]
    public bool Configure { get; set; }

    [JsonPropertyName("manage")]
    public bool Manage { get; set; }
}
