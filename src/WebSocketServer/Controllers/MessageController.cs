using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using WebSocketServer.Model;
using WebSocketServer.Services;

namespace WebSocketServer.Controllers
{
    [ApiController]
    [EnableCors("Any")]
    [Route("api/message")]
    public class MessageController : ControllerBase
    {
        private readonly SessionManager _sessionManager;

        public MessageController(SessionManager sessionManager)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] Message msg)
        {
            var (id, session) = _sessionManager.TryGetValue(msg.Uid);
            var data = JsonConvert.SerializeObject(msg);

            await session.SendAsync(data);

            return Ok(id);
        }

        [HttpPost("bc")]
        public async Task<IActionResult> SendBroadcastingMessage([FromBody] Message msg)
        {
            var sessions = _sessionManager.GetValues();
            var data = JsonConvert.SerializeObject(msg);
            foreach (var session in sessions)
            {
                await session.SendAsync(data);
            }

            return Ok(_sessionManager.Count());
        }
    }
}