using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSocketServer.Data;
using WebSocketServer.Entities;

namespace WebSocketServer.Services
{
    public class RunClockService
    {
        private readonly TestDbContext _context;

        public RunClockService(TestDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RunClock>> GetAllRunClocks()
        {
            return await _context.RunClocks.ToListAsync();
        }

        public RunClock GetRunClock(string key)
        {
            var res = _context.RunClocks.FirstOrDefault(x => x.SIM == key && x.IsCheck == false) ?? null;

            return res;
        }

        public void UpdateRunClock(RunClock run)
        {
            _context.Update(run);
        }

        public async Task AddRunClockAsync(RunClock run)
        {
            await _context.AddAsync(run);
        }
    }
}