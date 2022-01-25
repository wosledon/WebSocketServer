using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SuperSocket;

namespace WebSocketServer.Services
{
    public class ServerService: IServerService
    {
        protected ConcurrentDictionary<string, IServer> ServerDict = new ConcurrentDictionary<string, IServer>();

        public Task<bool> TryAddServer(string key, IServer server)
        {
            return Task.Run(() => ServerDict.TryAdd(key, server));
        }

        public async Task<bool> ReStartServer(string key)
        {
            ServerDict.TryGetValue(key, out var server);
            if (server != null)
            {
                await server.StopAsync();

                await server.StartAsync();

                return true;
            }

            return false;
        }
    }
}