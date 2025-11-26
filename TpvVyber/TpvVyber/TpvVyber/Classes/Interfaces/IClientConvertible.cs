using TpvVyber.Data;

namespace TpvVyber.Classes.Interfaces;

public interface IClientConvertible<TClient, TServer>
{
    TClient ToClient(TpvVyberContext context); // TODO Add Extended
    static abstract TServer ToServer(
        TClient clientObject,
        TpvVyberContext context,
        bool createNew = false
    ); // TODO Add Extended
}
