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

        // 🔹 Weeks per year (adjust as needed or fetch dynamically)
        private static readonly Dictionary<int, int> WeeksInYear = new()
        {
            [2013] = 53,
            [2014] = 53,
            [2015] = 52,
            [2016] = 52,
            [2017] = 52,
            [2018] = 53,
            [2019] = 52,
            [2020] = 52,
            [2021] = 52,
            [2022] = 52,
            [2023] = 52,
            [2024] = 52
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

        // 🔹 Normalize year/week (handles cross-year boundaries)
        private (int Year, int Week) NormalizeWeek(int year, int week)
        {
            // Get how many weeks this year has, fallback to 52
            int maxWeeks = WeeksInYear.ContainsKey(year) ? WeeksInYear[year] : 52;

            // If going back before W1 → go to previous year
            if (week < 1)
            {
                int prevYear = year - 1;
                int prevYearWeeks = WeeksInYear.ContainsKey(prevYear) ? WeeksInYear[prevYear] : 52;
                return (prevYear, prevYearWeeks + week);
            }

            // If going beyond last week → go to next year
            if (week > maxWeeks)
            {
                int nextYear = year + 1;
                int nextYearWeeks = WeeksInYear.ContainsKey(nextYear) ? WeeksInYear[nextYear] : 52;
                return (nextYear, week - maxWeeks);
            }

            return (year, week);
        }

        // 🔹 Helper: Fetch 4 weeks (two before, current, one after) for each found row
        private async Task<List<List<Calendar>>> GetFourWeekSetsAsync(List<Calendar> foundRows)
        {
            var weekSets = new List<List<Calendar>>();

            foreach (var row in foundRows)
            {
                // Always build a 4-week block around each found row
                var targetOffsets = new int[] { -2, -1, 0, 1 };

                var normalizedWeeks = targetOffsets
                    .Select(offset => NormalizeWeek(row.Years, row.Weeks + offset))
                    .ToList();

                var block = new List<Calendar>();

                foreach (var (normYear, normWeek) in normalizedWeeks)
                {
                    var weekRows = await _context.Table1
                        .Where(c => c.Years == normYear && c.Weeks == normWeek)
                        .ToListAsync();

                    if (weekRows.Any())
                    {
                        var ordered = weekRows
                            .OrderBy(c => DayOrder.ContainsKey(c.Days) ? DayOrder[c.Days] : 999)
                            .ToList();

                        block.AddRange(ordered);
                    }
                }

                if (block.Any())
                    weekSets.Add(block);
            }

            return weekSets;
        }
    }
}