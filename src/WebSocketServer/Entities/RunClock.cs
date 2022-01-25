using System;

namespace WebSocketServer.Entities
{
    public class RunClock
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string SIM { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public bool IsCheck { get; set; } = false;
    }
}