using MudBlazor;

public class UiNotification
{
    public string Message { get; set; } = "";
    public Severity Severity { get; set; }
}

public class NotificationService
{
    public event Action<UiNotification>? OnNotify;

    public void Notify(string message, Severity severity) =>
        OnNotify?.Invoke(new UiNotification { Message = message, Severity = severity });
}
