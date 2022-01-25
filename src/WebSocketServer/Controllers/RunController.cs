using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSocketServer.Data;
using WebSocketServer.Entities;
using WebSocketServer.Model;
using WebSocketServer.Services;

namespace WebSocketServer.Controllers
{
    [ApiController]
    [EnableCors("Any")]
    [Route("api")]
    public class RunController : ControllerBase
    {
        private readonly TestDbContext _context;
        private readonly IMapper _mapper;
        private readonly SessionManager _sessionManager;

        public RunController(TestDbContext context, IMapper mapper, SessionManager sessionManager)
        {
            _context = context;
            _mapper = mapper;
            _sessionManager = sessionManager;
        }

        [HttpGet("run")]
        public async Task<IActionResult> GetRun()

        {
            var data = await _context.RunClocks.Where(x => x.SIM != "server" && !x.IsCheck).ToListAsync();
            var res = _mapper.Map<IEnumerable<RunClockDto>>(data);

            return Ok(res);
        }

        [HttpGet("runh")]
        public async Task<IActionResult> GetRunHistory()

        {
            var data = await _context.RunClocks.Where(x => x.SIM != "server").ToListAsync();
            var res = _mapper.Map<IEnumerable<RunClockDto>>(data);

            return Ok(res);
        }

        [HttpGet("log")]
        public async Task<IActionResult> GetLog()

        {
            var data = await _context.ErrorLogs.ToListAsync();
            var res = _mapper.Map<IEnumerable<ErrorLog>>(data);

            return Ok(res);
        }

        [HttpGet("del")]
        public async Task<IActionResult> Delete()
        {
            var runs = await _context.RunClocks.ToListAsync();
            var logs = await _context.ErrorLogs.ToListAsync();

            _context.RunClocks.RemoveRange(runs);
            _context.ErrorLogs.RemoveRange(logs);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "数据已删除",
                RemoveRuns = runs.Count,
                RemoveLogs = logs.Count
            });
        }

        [HttpGet("delr")]
        public async Task<IActionResult> DeleteRuns()
        {
            var runs = await _context.RunClocks.ToListAsync();

            _context.RunClocks.RemoveRange(runs);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "数据已删除",
                RemoveRuns = runs.Count
            });
        }

        [HttpGet("delg")]
        public async Task<IActionResult> DeleteLogs()
        {
            var logs = await _context.ErrorLogs.ToListAsync();

            _context.ErrorLogs.RemoveRange(logs);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "数据已删除",
                RemoveLogs = logs.Count
            });
        }

        [HttpGet("reset")]
        public async Task<IActionResult> Reset()
        {
            try
            {
                if (_context?.Database != null)
                {
                    await _context.Database.EnsureDeletedAsync();
                    await _context.Database.MigrateAsync();
                }

                return Ok(new
                {
                    Message = "数据库已重置"
                });
            }
            catch (Exception e)
            {
                return Ok(new
                {
                    Message = "数据库重置异常"
                });
            }
        }

        [HttpGet("dev")]
        public async Task<IActionResult> Dev()
        {
            var sessions = _sessionManager.Dict;

            List<ReturnDto> rets = new List<ReturnDto>();

            foreach (var session in sessions)
            {
                rets.Add(new ReturnDto()
                {
                    Uid = session.Key,
                    SessionId = session.Value.SessionID,
                    Statue = session.Value.State.ToString(),
                    Closed = session.Value.LastActiveTime
                });
            }

            return Ok(rets);
        }

        [HttpGet("deld")]
        public async Task<IActionResult> DeleteDev()
        {
            var sessions = _sessionManager.Dict;
            foreach (var session in sessions)
            {
                await session.Value.CloseAsync();
            }

            _sessionManager.Dict.Clear();

            return Ok(new
            {
                Message = "连接重置成功"
            });
        }

        [HttpGet("res")]
        public async Task<IActionResult> GetResult()
        {
            //var res = await _context.RunClocks
            //    .GroupBy(x => x.SIM)
            //    .Select(x => new
            //    {
            //        SIM = x.Key,
            //        Alive = x.Sum(o => (int)o.EndTime.Subtract(o.StartTime).TotalSeconds)
            //    }).ToListAsync();

            var data = await _context.RunClocks.Where(x => x.SIM != "server" && x.IsCheck).ToListAsync();
            var res = _mapper.Map<IEnumerable<RunClockDto>>(data);

            var returnData = res
                .GroupBy(x => x.SIM)
                .Select(x => new
                {
                    SIM = x.Key,
                    Alive = $"{(int)(x.Sum(o => o.KeepTime) / 60 / 60)}小时" +
                            $"{(int)(x.Sum(o => o.KeepTime) / 60 % 60)}分钟" +
                            $"{(int)(x.Sum(o => o.KeepTime) % 60)}秒"
                });

            return Ok(returnData);
        }
    }

    public class ReturnDto
    {
        public string Uid { get; set; }
        public string SessionId { get; set; }
        public string Statue { get; set; }
        public DateTimeOffset Closed { get; set; }
    }
}