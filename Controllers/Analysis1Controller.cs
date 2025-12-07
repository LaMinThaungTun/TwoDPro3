using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

   
    public class Analysis1Controller : Controller
    {
        private readonly CalendarContext _context;
        public Analysis1Controller(CalendarContext context)
        {
            _context = context;
        }

        // Endpoint: fetch the last 10 entries
        [HttpGet("last5")]
        public async Task<ActionResult<List<Calendar>>> GetLast10Entries()
        {
            var last6 = await _context.Table1
                .OrderByDescending(c => c.Id)  // assuming Id is auto-increment
                .Take(6)
                .OrderBy(c => c.Id)            // optional: restore ascending order
                .ToListAsync();

            if (!last6.Any())
                return NotFound("No entries found in Table1.");

            return Ok(last6);
        }


    }
}
