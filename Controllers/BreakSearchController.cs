using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BreakSearchController : ControllerBase
    {
        private readonly CalendarContext _context;

        public BreakSearchController(CalendarContext context)
        {
            _context = context;
        }

        // 🔹 Define week counts per year
        private static readonly Dictionary<int, int> WeeksInYear = new()
        {
            [2013] = 52,
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

        // 🔹 Weekday ordering
        private static readonly Dictionary<string, int> DayOrder = new()
        {
            ["Monday"] = 1,
            ["Tuesday"] = 2,
            ["Wednesday"] = 3,
            ["Thursday"] = 4,
            ["Friday"] = 5
        };

        // 🔹 Helper: Calculate break value from a 2-digit string
        private static int? GetBreakValue(string? num)
        {
            if (string.IsNullOrWhiteSpace(num) || num.Length != 2)
                return null;

            if (int.TryParse(num, out int val))
            {
                int tens = val / 10;
                int ones = val % 10;
                int sum = tens + ones;
                if (sum >= 10) sum -= 10;
                return sum;
            }
            return null;
        }

        // 🔹 Normalize week across year boundaries
        private (int Year, int Week) NormalizeWeek(int year, int week)
        {
            int maxWeeks = WeeksInYear.ContainsKey(year) ? WeeksInYear[year] : 52;

            if (week < 1)
            {
                int prevYear = year - 1;
                int prevYearWeeks = WeeksInYear.ContainsKey(prevYear) ? WeeksInYear[prevYear] : 52;
                return (prevYear, prevYearWeeks + week);
            }

            if (week > maxWeeks)
            {
                int nextYear = year + 1;
                int nextYearWeeks = WeeksInYear.ContainsKey(nextYear) ? WeeksInYear[nextYear] : 52;
                return (nextYear, week - maxWeeks);
            }

            return (year, week);
        }

        // 🔹 Endpoint 1: Search all days (both AM and PM)
        // Example: GET api/breaksearch/alldaybreak?breakValue=1
        [HttpGet("alldaybreak")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchAllDayBreak(string breakValue)
        {
            

            // Filter for Year = 2025 and matching AMBreak or PMBreak
            var foundRows = await _context.Table1
                .Where(c => c.Years == 2025 && (c.AmBreak == breakValue || c.PmBreak == breakValue))
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No results found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

        // 🔹 Endpoint 2: Search by Day + Time (AM/PM)
        // Example: GET api/breaksearch/weeksetsbreak?breakValue=1&day=Tuesday&am=true&pm=false
        [HttpGet("weeksetsbreak")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchWeekSetsBreak(string breakValue, string day, bool am = false, bool pm = false)
        {
            

            if (!DayOrder.ContainsKey(day))
                return BadRequest("Invalid day. Use Monday–Friday.");

            if (!am && !pm)
                return BadRequest("At least one of AM or PM must be true.");

            // Filter rows by day only
            var query = _context.Table1
                .Where(c => c.Days == day);

            // Filter by AMBreak and PMBreak
            if (am && pm)
                query = query.Where(c => c.AmBreak == breakValue || c.PmBreak == breakValue);
            else if (am)
                query = query.Where(c => c.AmBreak == breakValue);
            else if (pm)
                query = query.Where(c => c.PmBreak == breakValue);

            var foundRows = await query
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No results found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

        // 🔹 Collect 4-week blocks (−2, −1, 0, +1) for each found row
        private async Task<List<List<Calendar>>> GetFourWeekSetsAsync(List<Calendar> foundRows)
        {
            var weekSets = new List<List<Calendar>>();

            foreach (var row in foundRows)
            {
                var offsets = new int[] { -2, -1, 0, 1 };
                var normalizedWeeks = offsets
                    .Select(offset => NormalizeWeek(row.Years, row.Weeks + offset))
                    .Distinct()
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
                            .ThenBy(c => c.Id)
                            .ToList();

                        block.AddRange(ordered);
                    }
                }

                if (block.Any())
                {
                    var uniqueBlock = block
                        .OrderBy(c => c.Id)
                        .GroupBy(c => c.Id)
                        .Select(g => g.First())
                        .ToList();

                    weekSets.Add(uniqueBlock);
                }
            }

            weekSets = weekSets
                .OrderBy(b => b.Min(c => c.Id))
                .ToList();

            return weekSets;
        }
    }
}
