using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TheWholeTableController : ControllerBase
    {
        private readonly CalendarContext _context;

        public TheWholeTableController(CalendarContext context)
        {
            _context = context;
        }

        // GET: api/Calendar
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Calendar>>> GetTable1()
        {
            return await _context.Table1.ToListAsync();
        }
    }
}
