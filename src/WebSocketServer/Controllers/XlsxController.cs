using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.IO;
using System.Threading.Tasks;
using WebSocketServer.Data;

namespace WebSocketServer.Controllers
{
    /// <summary>
    /// 生成Excel表格
    /// </summary>
    [ApiController]
    [EnableCors("Any")]
    [Route("api/Xlsx")]
    public class XlsxController : ControllerBase
    {
        private readonly TestDbContext _context;

        public XlsxController(TestDbContext context)
        {
            _context = context;
        }

        [HttpGet("Company")]
        public async Task<IActionResult> GetCompanyXlsx()
        {
            var newPath = Path.Combine($"uploads/xlsx/", $"{Guid.NewGuid()}.xlsx");
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", newPath);
            using (ExcelPackage pack = new ExcelPackage())
            {
                var workSheet = pack.Workbook.Worksheets.Add("result");
                var data = await _context.Companies.ToListAsync();
                workSheet.Cells.LoadFromCollection(data, true);
                pack.SaveAs(new FileStream(path, FileMode.Create));
            }
            return Ok(new
            {
                Result = true,
                Path = newPath
            });
        }

        [HttpGet("Employee")]
        public async Task<IActionResult> GetEmployeXlsx()
        {
            var newPath = Path.Combine($"uploads/xlsx/", $"{Guid.NewGuid()}.xlsx");
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", newPath);
            using (ExcelPackage pack = new ExcelPackage())
            {
                var workSheet = pack.Workbook.Worksheets.Add("result");
                var data = await _context.Employees.ToListAsync();
                workSheet.Cells.LoadFromCollection(data, true);
                pack.SaveAs(new FileStream(path, FileMode.Create));
            }
            return Ok(new
            {
                Result = true,
                Path = newPath
            });
        }
    }
}