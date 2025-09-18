using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly CalendarContext _context;

        public CalendarController(CalendarContext context)
        {
            _context = context;
        }

        // GET api/calendar/2014
        [HttpGet("{year}")]
        public async Task<ActionResult<IEnumerable<Calendar>>> GetByYear(int year)
        {
            var result = await _context.Table1
                .Where(c => c.Years == year)
                .ToListAsync();

            return Ok(result);
        }
    }
}
