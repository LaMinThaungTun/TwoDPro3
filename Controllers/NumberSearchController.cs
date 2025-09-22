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
    public class NumberSearchController : ControllerBase
    {
        private readonly CalendarContext _context;

        public NumberSearchController(CalendarContext context)
        {
            _context = context;
        }

        // ✅ Order for weekdays
        private static readonly Dictionary<string, int> DayOrder = new()
        {
            ["Monday"] = 1,
            ["Tuesday"] = 2,
            ["Wednesday"] = 3,
            ["Thursday"] = 4,
            ["Friday"] = 5
        };

        // 🔹 Endpoint 1: Search across ALL days (AM + PM)
        [HttpGet("alldays")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchAllDays(string number)
        {
            var foundRows = await _context.Table1
                .Where(c => c.Am == number || c.Pm == number)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No results found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

        // 🔹 Endpoint 2: Search with Day + Time filter
        [HttpGet("weeksets")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchWeekSets(
            string number, string day, bool am = false, bool pm = false)
        {
            if (!DayOrder.ContainsKey(day))
                return BadRequest("Invalid day. Use Monday–Friday.");

            IQueryable<Calendar> query = _context.Table1.Where(c => c.Days == day);

            if (am && pm)
            {
                query = query.Where(c => c.Am == number || c.Pm == number);
            }
            else if (am)
            {
                query = query.Where(c => c.Am == number);
            }
            else if (pm)
            {
                query = query.Where(c => c.Pm == number);
            }
            else
            {
                return BadRequest("At least one of AM or PM must be true.");
            }

            var foundRows = await query.ToListAsync();

            if (!foundRows.Any())
                return NotFound("No results found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

        // 🔹 Helper: Fetch 4 weeks (two before, current, one after) for each found row
        private async Task<List<List<Calendar>>> GetFourWeekSetsAsync(List<Calendar> foundRows)
        {
            var weekSets = new List<List<Calendar>>();
            var processedWeeks = new HashSet<(int Year, int Week)>();

            foreach (var row in foundRows)
            {
                var targetWeeks = new int[] { row.Weeks - 2, row.Weeks - 1, row.Weeks, row.Weeks + 1 };

                foreach (var weekNum in targetWeeks)
                {
                    var weekKey = (row.Years, weekNum);
                    if (processedWeeks.Contains(weekKey))
                        continue;

                    processedWeeks.Add(weekKey);

                    var weekRows = await _context.Table1
                        .Where(c => c.Years == row.Years && c.Weeks == weekNum)
                        .ToListAsync();

                    if (weekRows.Any())
                    {
                        var ordered = weekRows
                            .OrderBy(c => DayOrder.ContainsKey(c.Days) ? DayOrder[c.Days] : 999)
                            .ToList();

                        weekSets.Add(ordered);
                    }
                }
            }

            return weekSets;
        }
    }
}