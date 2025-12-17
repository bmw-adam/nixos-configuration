using System.Text.Json.Serialization;

namespace TpvVyber.Client.Classes;

public class ProblemDetailsResponse
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "about:blank";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "An error occurred while processing your request.";

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("detail")]
    public string Detail { get; set; } = string.Empty;

    [JsonPropertyName("instance")]
    public string? Instance { get; set; } // Optional, can be null
}
