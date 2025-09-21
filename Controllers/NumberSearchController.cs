using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        // Map day names to order (Monday = 1, ..., Friday = 5)
        private static readonly Dictionary<string, int> DayOrder = new()
        {
            {"Monday", 1},
            {"Tuesday", 2},
            {"Wednesday", 3},
            {"Thursday", 4},
            {"Friday", 5}
        };

        [HttpGet("weeksets")]
        public async Task<IActionResult> SearchWeekSets(
            [FromQuery] string number,
            [FromQuery] string day,
            [FromQuery] bool am = true,
            [FromQuery] bool pm = true)
        {
            if (string.IsNullOrEmpty(number))
                return BadRequest("Number is required.");
            if (string.IsNullOrEmpty(day))
                return BadRequest("Day is required.");

            // Step 1: Get all rows on requested day & time containing the number
            var dayQuery = _context.Table1.AsQueryable();
            dayQuery = dayQuery.Where(c => c.Days == day);

            if (am && !pm)
                dayQuery = dayQuery.Where(c => c.Am.Contains(number));
            else if (!am && pm)
                dayQuery = dayQuery.Where(c => c.Pm.Contains(number));
            else if (am && pm)
                dayQuery = dayQuery.Where(c => c.Am.Contains(number) || c.Pm.Contains(number));
            else
                return BadRequest("At least one of AM or PM must be true.");

            var foundRows = await dayQuery.ToListAsync();
            if (!foundRows.Any())
                return Ok(new List<List<Calendar>>());

            // Step 2: For each found row, fetch all rows of that week
            var weekSets = new List<List<Calendar>>();

            var processedWeeks = new HashSet<(int Year, int Week)>();

            foreach (var row in foundRows)
            {
                var weekKey = (row.Years, row.Weeks);
                if (processedWeeks.Contains(weekKey))
                    continue;

                processedWeeks.Add(weekKey);

                var weekRows = await _context.Table1
                    .Where(c => c.Years == row.Years && c.Weeks == row.Weeks)
                    .ToListAsync();

                // Step 3: Arrange Monday → Friday
                var sortedWeekRows = weekRows
                    .OrderBy(c => DayOrder.ContainsKey(c.Days) ? DayOrder[c.Days] : 999)
                    .ToList();

                weekSets.Add(sortedWeekRows);
            }

            return Ok(weekSets);
        }
    }
}