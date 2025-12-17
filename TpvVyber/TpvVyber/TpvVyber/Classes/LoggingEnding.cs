using System.Threading.Tasks;
using TpvVyber.Classes.Interfaces;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Data;

namespace TpvVyber.Classes;

public class LoggingEnding
    : IClientConvertible<LoggingEndingCln, LoggingEnding, FillLoggingEndingExtended>
{
    public int Id { get; set; }
    public DateTime TimeEnding { get; set; }

    public Task<LoggingEndingCln> ToClient(
        TpvVyberContext context,
        Student? currentUser,
        IAdminService adminService,
        FillLoggingEndingExtended? fillExtended = null
    )
    {
        var clientObject = new LoggingEndingCln { Id = Id, TimeEnding = TimeEnding.ToLocalTime() };
        return Task.FromResult(clientObject);
    }

    public static LoggingEnding ToServer(
        LoggingEndingCln clientObject,
        TpvVyberContext context,
        bool createNew = false
    )
    {
        if (createNew)
        {
            return new LoggingEnding { TimeEnding = clientObject.TimeEnding };
        }

        var entity = context.LoggingEndings.Find(clientObject.Id);
        // Apply potentional changes
        if (entity == null)
        {
            throw new Exception("Nepodařilo se najít LoggingEndings v databázi");
        }

        entity.TimeEnding = clientObject.TimeEnding.ToUniversalTime();
        return entity;
    }
}

public enum FillLoggingEndingExtended { }
