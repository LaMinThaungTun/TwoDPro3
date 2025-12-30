using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DgTwoOnePairController : Controller
    {
        private readonly CalendarContext _context;

        public DgTwoOnePairController(CalendarContext context)
        {
            _context = context;
        }

        // Order for weekdays
        private static readonly Dictionary<string, int> DayOrder = new()
        {
            ["Monday"] = 1,
            ["Tuesday"] = 2,
            ["Wednesday"] = 3,
            ["Thursday"] = 4,
            ["Friday"] = 5
        };

        // Weeks per year
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
            [2024] = 52,
            [2025] = 53
        };

        // ==========================================================
        // 1) ALL DAYS SEARCH
        // GET api/DgTwoOnePair/alldaydgtwoonepair?number=1&number2=2
        // ==========================================================
        [HttpGet("alldaydgtwoonepair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchAllDays(
            string number, string number2)
        {
            if (number.Length != 1 || number2.Length != 1)
                return BadRequest("Both number and number2 must be 1-digit strings.");

            var foundRows = await _context.Table1
                .Where(c =>
                    c.AmDgTwo == number &&
                    c.PmDgOne == number2 && (c.Years == 2024 || c.Years == 2025 || c.Years == 2026))
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No matching DG Two–One pair found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

        // ==========================================================
        // 2) WEEKSETS SEARCH
        // GET api/DgTwoOnePair/weeksetsdgtwoonepair?number=1&number2=2&day=Monday
        // ==========================================================
        [HttpGet("weeksetsdgtwoonepair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchWeekSets(
            string number, string number2, string day)
        {
            if (number.Length != 1 || number2.Length != 1)
                return BadRequest("Both number and number2 must be 1-digit strings.");

            if (!DayOrder.ContainsKey(day))
                return BadRequest("Invalid day. Use Monday–Friday.");

            var foundRows = await _context.Table1
                .Where(c =>
                    c.Days == day &&
                    c.AmDgTwo == number &&
                    c.PmDgOne == number2)
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No matching DG Two–One pair found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

        // 🔹 Normalize year/week
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
                return (nextYear, week - maxWeeks);
            }

            return (year, week);
        }

        // Fetch 4-week blocks
        private async Task<List<List<Calendar>>> GetFourWeekSetsAsync(List<Calendar> foundRows)
        {
            var weekSets = new List<List<Calendar>>();
            var processedWeeks = new HashSet<(int year, int week)>();

            foreach (var row in foundRows)
            {
                var baseKey = (row.Years, row.Weeks);
                if (processedWeeks.Contains(baseKey))
                    continue;

                processedWeeks.Add(baseKey);

                int[] offsets = { -2, -1, 0, 1 };

                var normalizedWeeks = offsets
                    .Select(o => NormalizeWeek(row.Years, row.Weeks + o))
                    .Distinct()
                    .ToList();

                var block = new List<Calendar>();

                foreach (var (y, w) in normalizedWeeks)
                {
                    var weekRows = await _context.Table1
                        .Where(c => c.Years == y && c.Weeks == w)
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
                        .GroupBy(c => c.Id)
                        .Select(g => g.First())
                        .OrderBy(c => c.Id)
                        .ToList();

                    weekSets.Add(uniqueBlock);
                }
            }

            return weekSets
                .OrderBy(b => b.Min(c => c.Id))
                .ToList();
        }
    }
}
