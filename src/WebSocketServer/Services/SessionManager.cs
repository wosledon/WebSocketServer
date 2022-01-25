using SuperSocket.WebSocket.Server;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace WebSocketServer.Services
{
    public class SessionManager
    {
        public ConcurrentDictionary<string, WebSocketSession> Dict { get; set; } = new();

        public bool TryAdd(string uid, WebSocketSession session)
        {
            return Dict.TryAdd(uid, session);
        }

        public bool TryRemove(string uid)
        {
            return Dict.TryRemove(uid, out var session);
        }

        public bool TryRemove(WebSocketSession session)
        {
            var uid = Dict.First(x => x.Value.SessionID.Equals(session.SessionID)).Key;

            return TryRemove(uid);
        }

        public bool TryUpdate(string uid, WebSocketSession session)
        {
            if (Dict.TryGetValue(uid, out var oldSession))
            {
                return Dict.TryUpdate(uid, session, oldSession);
            }
            else
            {
                return Dict.TryAdd(uid, session);
            }
        }

        public string GetUid(WebSocketSession session)
        {
            foreach (var item in Dict)
            {
                if (item.Value.SessionID == session.SessionID)
                {
                    return item.Key;
                }
            }

            return null;
        }

        public (string, WebSocketSession) TryGetValue(string uid)
        {
            Dict.TryGetValue(uid, out var session);
            return (uid, session);
        }

        public WebSocketSession TryGetValueByUid(string uid)
        {
            Dict.TryGetValue(uid, out var session);
            return session;
        }

        public bool ValueExist(string uid)
        {
            return Dict.TryGetValue(uid, out var session);
        }

        public int Count()
        {
            return Dict.Count;
        }

        public IEnumerable<WebSocketSession> GetValues()
        {
            return Dict.Values;
        }
    }
}