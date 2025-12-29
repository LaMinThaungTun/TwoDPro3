using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BreakPairController : Controller
    {
        private readonly CalendarContext _context;

        public BreakPairController(CalendarContext context)
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
        // GET api/BreakPair/alldaybreakpair?number=3&number2=7
        // ==========================================================
        [HttpGet("alldaybreakpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchAllDays(
            string number, string number2)
        {
            if (number.Length != 1 || number2.Length != 1)
                return BadRequest("Both number and number2 must be 1-digit strings.");

            var foundRows = await _context.Table1
                .Where(c =>
                    c.AmBreak == number &&
                    c.PmBreak == number2)
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No matching AMBreak-PMBreak pair found.");

            return Ok(await GetFourWeekSetsAsync(foundRows));
        }

        // ==========================================================
        // 2) WEEKSETS SEARCH
        // GET api/BreakPair/weeksetsbreakpair?number=3&number2=7&day=Monday
        // ==========================================================
        [HttpGet("weeksetsbreakpair")]
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
                    c.AmBreak == number &&
                    c.PmBreak == number2)
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No matching AMBreak-PMBreak pair found.");

            return Ok(await GetFourWeekSetsAsync(foundRows));
        }

        // ==================================================
        // SHARED LOGIC (UNCHANGED)
        // ==================================================
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
