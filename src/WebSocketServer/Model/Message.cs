using System;

namespace WebSocketServer.Model
{
    public class Message
    {
        public OpCode OpCode { get; set; }
        public string Uid { get; set; }
        public Guid MessageId { get; set; } = Guid.NewGuid();

        public string VCode { get; set; }
        public object Body { get; set; }
    }
}