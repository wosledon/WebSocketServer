using System;

namespace WebSocketServer.Entities
{
    public class ErrorLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Uid { get; set; }
        public string SessionId { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
        public string Log { get; set; }
    }
}