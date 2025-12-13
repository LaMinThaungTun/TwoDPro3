using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PowerPairController : Controller
    {
        private readonly CalendarContext _context;

        public PowerPairController(CalendarContext context)
        {
            _context = context;
        }

        // Power numbers list
        private static readonly HashSet<string> PowerNumbers = new()
        {
            "05", "16", "27", "38", "49", "50", "61", "72", "83", "94"
        };

        // Weekday order
        private static readonly Dictionary<string, int> DayOrder = new()
        {
            ["Monday"] = 1,
            ["Tuesday"] = 2,
            ["Wednesday"] = 3,
            ["Thursday"] = 4,
            ["Friday"] = 5
        };

        // ==========================================================
        // 1) ALL DAYS POWER PAIR SEARCH
        // GET api/PowerPair/alldaypowerpair?powerpair=powerpair
        // ==========================================================
        [HttpGet("alldaypowerpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchAllDays(string powerpair)
        {
            if (powerpair != "powerpair")
                return BadRequest("Parameter must be 'powerpair'.");

            var foundRows = await _context.Table1
                .Where(c =>
                    PowerNumbers.Contains(c.Am) &&
                    PowerNumbers.Contains(c.Pm))
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No power number pairs found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

        // ==========================================================
        // 2) WEEK SETS POWER PAIR SEARCH
        // GET api/PowerPair/weeksetspowerpair?powerpair=powerpair&day=Monday
        // ==========================================================
        [HttpGet("weeksetspowerpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchWeekSets(string powerpair, string day)
        {
            if (powerpair != "powerpair")
                return BadRequest("Parameter must be 'powerpair'.");

            if (!DayOrder.ContainsKey(day))
                return BadRequest("Invalid day. Use Monday–Friday.");

            var foundRows = await _context.Table1
                .Where(c =>
                    c.Days == day &&
                    PowerNumbers.Contains(c.Am) &&
                    PowerNumbers.Contains(c.Pm))
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No power number pairs found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

        // ==========================================================
        // WEEK NORMALIZER + FOUR WEEK SET BUILDER (same as others)
        // ==========================================================
        private (int Year, int Week) NormalizeWeek(int year, int week)
        {
            int maxWeeks = 52;
            return (year, Math.Max(1, Math.Min(week, maxWeeks)));
        }

        private async Task<List<List<Calendar>>> GetFourWeekSetsAsync(List<Calendar> foundRows)
        {
            var weekSets = new List<List<Calendar>>();
            var processed = new HashSet<(int y, int w)>();

            foreach (var row in foundRows)
            {
                var key = (row.Years, row.Weeks);
                if (processed.Contains(key)) continue;

                processed.Add(key);

                int[] offsets = { -2, -1, 0, 1 };

                var normalizedWeeks = offsets
                    .Select(o => NormalizeWeek(row.Years, row.Weeks + o))
                    .Distinct()
                    .ToList();

                var block = new List<Calendar>();

                foreach (var (yr, wk) in normalizedWeeks)
                {
                    var rows = await _context.Table1
                        .Where(c => c.Years == yr && c.Weeks == wk)
                        .ToListAsync();

                    if (rows.Any())
                    {
                        block.AddRange(
                            rows.OrderBy(c => DayOrder.ContainsKey(c.Days) ? DayOrder[c.Days] : 999)
                                .ThenBy(c => c.Id)
                        );
                    }
                }

                if (block.Any())
                {
                    weekSets.Add(
                        block.GroupBy(c => c.Id)
                             .Select(g => g.First())
                             .OrderBy(c => c.Id)
                             .ToList()
                    );
                }
            }

            return weekSets.OrderBy(b => b.Min(c => c.Id)).ToList();
        }
    }
}
