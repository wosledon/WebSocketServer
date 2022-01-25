using System.Threading.Tasks;
using WebSocketServer.Data;
using WebSocketServer.Entities;

namespace WebSocketServer.Services
{
    public class ErrorService
    {
        private readonly TestDbContext _context;

        public ErrorService(TestDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ErrorLog log)
        {
            await _context.ErrorLogs.AddAsync(log);
        }

        public void Add(ErrorLog log)
        {
            _context.ErrorLogs.Add(log);
        }
    }
}