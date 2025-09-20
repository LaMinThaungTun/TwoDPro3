using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;   // your DbContext namespace
using TwoDPro3.Models; // your model namespace
using System.Threading.Tasks;
using System.Linq;

namespace TwoDPro3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NumberSearchController : ControllerBase
    {
        private readonly CalendarContext _context;

        public NumberSearchController(CalendarContext context)
        {
            _context = context;
        }

        // Case 1: Search across all days (both AM + PM)
        [HttpGet("alldays")]
        public IActionResult SearchAllDays([FromQuery] string number)
        {
            var result = _context.Table1
                .Where(c => c.Am.Contains(number) || c.Pm.Contains(number))
                .ToList();

            return Ok(result);
        }

        // Case 2: Search with day + time filter
        [HttpGet("daytime")]
        public IActionResult SearchDayTime([FromQuery] string number, [FromQuery] string day, [FromQuery] string time)
        {
            var query = _context.Table1.AsQueryable();

            query = query.Where(c => c.Days == day);

            if (time == "AM")
            {
                query = query.Where(c => c.Am.Contains(number));
            }
            else if (time == "PM")
            {
                query = query.Where(c => c.Pm.Contains(number));
            }
            else if (time == "Both")
            {
                query = query.Where(c => c.Am.Contains(number) || c.Pm.Contains(number));
            }

            var result = query.ToList();

            return Ok(result);
        }
    }
}
