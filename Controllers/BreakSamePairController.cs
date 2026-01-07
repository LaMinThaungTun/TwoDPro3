using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BreakSamePairController : Controller
    {
        private readonly CalendarContext _context;

        public BreakSamePairController(CalendarContext context)
        {
            _context = context;
        }

        // Weekday order
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
        // 1) ALL DAYS SAME BREAK PAIR SEARCH
        // GET api/BreakSamePair/alldaysamebreakpair?samebreakpair=samebreakpair
        // ==========================================================
        [HttpGet("alldaysamebreakpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchAllDays(string? samebreakpair)
        {
            if (samebreakpair != "samebreakpair")
                return BadRequest();

            var foundRows = await _context.Table1
                .Where(c =>
                    c.AmBreak == c.PmBreak &&
                    c.AmBreak != ClosedCode &&
                    c.PmBreak != ClosedCode &&
                    c.AmBreak != null &&
                    c.PmBreak != null &&
                    c.AmBreak != "aa" &&
                    c.PmBreak != "aa")
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound();

            return Ok(await GetFourWeekSetsAsync(foundRows));
        }

        // ==========================================================
        // 2) WEEKSETS SAME BREAK PAIR SEARCH
        // GET api/BreakSamePair/weeksetssamebreakpair?samebreakpair=samebreakpair&day=Monday
        // ==========================================================
        [HttpGet("weeksetssamebreakpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchWeekSets(string samebreakpair, string day)
        {
            if (samebreakpair != "samebreakpair")
                return BadRequest();

            if (!DayOrder.ContainsKey(day))
                return BadRequest("Invalid day. Use Monday–Friday.");

            var foundRows = await _context.Table1
                .Where(c =>
                    c.Days == day &&
                    c.AmBreak == c.PmBreak &&
                    c.AmBreak != ClosedCode &&
                    c.PmBreak != ClosedCode &&
                    c.AmBreak != null &&
                    c.PmBreak != null)
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound();

            return Ok(await GetFourWeekSetsAsync(foundRows));
        }

        // SHARED LOGIC (unchanged)
        private (int Year, int Week) NormalizeWeek(int year, int week)
        {
            int maxWeeks = WeeksInYear.ContainsKey(year) ? WeeksInYear[year] : 52;

            if (week < 1)
            {
                int py = year - 1;
                int pw = WeeksInYear.ContainsKey(py) ? WeeksInYear[py] : 52;
                return (py, pw + week);
            }

            if (week > maxWeeks)
                return (year + 1, week - maxWeeks);

            return (year, week);
        }

        private async Task<List<List<Calendar>>> GetFourWeekSetsAsync(List<Calendar> foundRows)
        {
            var weekSets = new List<List<Calendar>>();
            var processed = new HashSet<(int, int)>();

            foreach (var row in foundRows)
            {
                var key = (row.Years, row.Weeks);
                if (processed.Contains(key)) continue;
                processed.Add(key);

                int[] offsets = { -2, -1, 0, 1 };
                var block = new List<Calendar>();

                foreach (var offset in offsets)
                {
                    var (y, w) = NormalizeWeek(row.Years, row.Weeks + offset);

                    var rows = await _context.Table1
                        .Where(c => c.Years == y && c.Weeks == w)
                        .ToListAsync();

                    block.AddRange(
                        rows.OrderBy(c => DayOrder.GetValueOrDefault(c.Days, 999))
                            .ThenBy(c => c.Id)
                    );
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
