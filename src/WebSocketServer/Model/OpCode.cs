namespace WebSocketServer.Model
{
    public enum OpCode
    {
        HearBeat = 0,
        Login = 1,
        Message = 2,
        BC = 3,
        Single = 4,
        Ack = 5
    }
}