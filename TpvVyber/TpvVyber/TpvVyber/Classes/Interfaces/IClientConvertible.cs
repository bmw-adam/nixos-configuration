using TpvVyber.Data;

namespace TpvVyber.Classes.Interfaces;

public interface IClientConvertible<TClient, TServer, TExtendedFillEnum>
    where TExtendedFillEnum : struct, System.Enum
{
    TClient ToClient(TpvVyberContext context, TExtendedFillEnum? fillExtended); // TODO Add Extended
    static abstract TServer ToServer(
        TClient clientObject,
        TpvVyberContext context,
        bool createNew = false
    ); // TODO Add Extended
}
