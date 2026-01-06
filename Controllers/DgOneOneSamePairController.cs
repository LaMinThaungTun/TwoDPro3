using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DgOneOneSamePairController : Controller
    {
        private readonly CalendarContext _context;

        public DgOneOneSamePairController(CalendarContext context)
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

        private static readonly string ClosedCode = "aa";

        // ==========================================================
        // 1) ALL DAYS DG ONE–ONE SAME PAIR
        // GET api/DgOneOneSamePair/alldaydgoneonesamepair?dgoneonesamepair=dgoneonesamepair
        // ==========================================================
        [HttpGet("alldaydgoneonesamepair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchAllDays(string dgoneonesamepair)
        {
            if (dgoneonesamepair != "dgoneonesamepair")
                return BadRequest("Parameter must be 'dgoneonesamepair'.");

            var foundRows = await _context.Table1
                .Where(c =>
                    c.AmDgOne == c.PmDgOne &&
                    c.Am != ClosedCode &&
                    c.Pm != ClosedCode &&
                    c.Am != null &&
                    c.Pm != null )
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No valid DG One–One same pairs found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

        // ==========================================================
        // 2) WEEK SETS DG ONE–ONE SAME PAIR
        // GET api/DgOneOneSamePair/weeksetsdgoneonesamepair?dgoneonesamepair=dgoneonesamepair&day=Monday
        // ==========================================================
        [HttpGet("weeksetsdgoneonesamepair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchWeekSets(
            string dgoneonesamepair, string day)
        {
            if (dgoneonesamepair != "dgoneonesamepair")
                return BadRequest("Parameter must be 'dgoneonesamepair'.");

            if (!DayOrder.ContainsKey(day))
                return BadRequest("Invalid day. Use Monday–Friday.");

            var foundRows = await _context.Table1
                .Where(c =>
                    c.Days == day &&
                    c.AmDgOne == c.PmDgOne &&
                    c.Am != ClosedCode &&
                    c.Pm != ClosedCode &&
                    c.Am != null &&
                    c.Pm != null)
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No valid DG One–One same pairs found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

        // ----------------------------------------------------------
        // Normalize week/year
        private (int Year, int Week) NormalizeWeek(int year, int week)
        {
            int maxWeeks = WeeksInYear.ContainsKey(year) ? WeeksInYear[year] : 52;

            if (week < 1)
            {
                int prevYear = year - 1;
                int prevWeeks = WeeksInYear.ContainsKey(prevYear) ? WeeksInYear[prevYear] : 52;
                return (prevYear, prevWeeks + week);
            }

            if (week > maxWeeks)
            {
                int nextYear = year + 1;
                return (nextYear, week - maxWeeks);
            }

            return (year, week);
        }

        // ----------------------------------------------------------
        // 4-week block logic
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

                var offsets = new int[] { -2, -1, 0, 1 };

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
