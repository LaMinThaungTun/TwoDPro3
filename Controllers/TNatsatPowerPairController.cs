using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TNatsatPowerPairController : Controller
    {
        private readonly CalendarContext _context;

        public TNatsatPowerPairController(CalendarContext context)
        {
            _context = context;
        }

        // ==========================
        // NUMBER SETS
        // ==========================
        private static readonly HashSet<string> TNatsatNumbers = new()
        {
            "19","23","48","56","70",
            "91","32","84","65","07"
        };

        private static readonly HashSet<string> PowerNumbers = new()
        {
            "05","16","27","38","49",
            "50","61","72","83","94"
        };

        // ==========================
        // DAY ORDER
        // ==========================
        private static readonly Dictionary<string, int> DayOrder = new()
        {
            ["Monday"] = 1,
            ["Tuesday"] = 2,
            ["Wednesday"] = 3,
            ["Thursday"] = 4,
            ["Friday"] = 5
        };

        // ==========================
        // WEEKS PER YEAR
        // ==========================
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

        // ==================================================
        // ALL DAY T NATSAT – POWER
        // GET api/TNatsatPowerPair/alldaytnatsatpowerpair
        // ==================================================
        [HttpGet("alldaytnatsatpowerpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchAllDays(string tnatsatpowerpair)
        {
            if (tnatsatpowerpair != "tnatsatpowerpair")
                return BadRequest("Parameter must be 'tnatsatpowerpair'.");

            var foundRows = await _context.Table1
                .Where(c =>
                    TNatsatNumbers.Contains(c.Am) &&
                    PowerNumbers.Contains(c.Pm))
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound();

            return Ok(await GetFourWeekSetsAsync(foundRows));
        }

        // ==================================================
        // WEEK SETS T NATSAT – POWER
        // GET api/TNatsatPowerPair/weeksetstnatsatpowerpair
        // ==================================================
        [HttpGet("weeksetstnatsatpowerpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchWeekSets(
            string tnatsatpowerpair, string day)
        {
            if (tnatsatpowerpair != "tnatsatpowerpair")
                return BadRequest("Parameter must be 'tnatsatpowerpair'.");

            if (!DayOrder.ContainsKey(day))
                return BadRequest("Invalid day.");

            var foundRows = await _context.Table1
                .Where(c =>
                    c.Days == day &&
                    TNatsatNumbers.Contains(c.Am) &&
                    PowerNumbers.Contains(c.Pm))
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound();

            return Ok(await GetFourWeekSetsAsync(foundRows));
        }

        // ==========================
        // HELPERS (UNCHANGED)
        // ==========================
        private (int Year, int Week) NormalizeWeek(int year, int week)
        {
            int max = WeeksInYear.ContainsKey(year) ? WeeksInYear[year] : 52;

            if (week < 1)
            {
                int py = year - 1;
                int pw = WeeksInYear.ContainsKey(py) ? WeeksInYear[py] : 52;
                return (py, pw + week);
            }

            if (week > max)
                return (year + 1, week - max);

            return (year, week);
        }

        private async Task<List<List<Calendar>>> GetFourWeekSetsAsync(List<Calendar> foundRows)
        {
            var result = new List<List<Calendar>>();
            var seen = new HashSet<(int y, int w)>();

            foreach (var row in foundRows)
            {
                if (!seen.Add((row.Years, row.Weeks))) continue;

                int[] offsets = { -2, -1, 0, 1 };
                var block = new List<Calendar>();

                foreach (var o in offsets)
                {
                    var (y, w) = NormalizeWeek(row.Years, row.Weeks + o);

                    var rows = await _context.Table1
                        .Where(c => c.Years == y && c.Weeks == w)
                        .ToListAsync();

                    block.AddRange(
                        rows.OrderBy(c => DayOrder.ContainsKey(c.Days) ? DayOrder[c.Days] : 999)
                            .ThenBy(c => c.Id)
                    );
                }

                if (block.Any())
                    result.Add(block.GroupBy(c => c.Id).Select(g => g.First()).ToList());
            }

            return result.OrderBy(b => b.Min(c => c.Id)).ToList();
        }
    }
}
