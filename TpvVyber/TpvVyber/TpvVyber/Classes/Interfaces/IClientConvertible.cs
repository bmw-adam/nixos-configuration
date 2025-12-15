using TpvVyber.Client.Services.Admin;
using TpvVyber.Data;

namespace TpvVyber.Classes.Interfaces;

public interface IClientConvertible<TClient, TServer, TExtendedFillEnum>
    where TExtendedFillEnum : struct, System.Enum
{
    Task<TClient> ToClient(
        TpvVyberContext context,
        Student? currentUser,
        IAdminService adminService,
        TExtendedFillEnum? fillExtended
    );
    static abstract TServer ToServer(
        TClient clientObject,
        TpvVyberContext context,
        bool createNew = false
    );
}
