using System.Threading.Tasks;
using SuperSocket;

namespace WebSocketServer.Services
{
    public interface IServerService
    {
        Task<bool> TryAddServer(string key, IServer server);

        Task<bool> ReStartServer(string key);
    }
}