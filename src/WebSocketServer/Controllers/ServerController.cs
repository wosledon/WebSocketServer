using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WebSocketServer.Services;

namespace WebSocketServer.Controllers
{
    [ApiController]
    [EnableCors("Any")]
    [Route("api/server")]
    public class ServerController: ControllerBase
    {
        private readonly IServerService _serverService;

        public ServerController(IServerService serverService)
        {
            _serverService = serverService;
        }

        [HttpGet]
        [Route("restart")]
        public async Task<IActionResult> ReStartServer()
        {
            await _serverService.ReStartServer("server");

            return Ok(new
            {
                Result = true,
                Message = "重启服务器成功"
            });
        }
    }
}
