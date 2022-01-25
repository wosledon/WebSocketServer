using System;

namespace WebSocketServer.Model
{
    public class RunClockDto
    {
        public string SIM { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int KeepTime { get; set; }

        public bool IsCheck { get; set; } = false;
    }
}