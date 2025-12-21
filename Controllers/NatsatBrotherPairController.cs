using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NatsatBrotherPairController : Controller
    {
        private readonly CalendarContext _context;

        public NatsatBrotherPairController(CalendarContext context)
        {
            _context = context;
        }

        // ==========================
        // NUMBER SETS
        // ==========================
        private static readonly HashSet<string> NatsatNumbers = new()
        {
            "07","18","24","35","69",
            "70","81","42","53","96"
        };

        private static readonly HashSet<string> BrotherNumbers = new()
        {
            "01","12","23","34","45","56","67","78","89","90",
            "10","21","32","43","54","65","76","87","98","09"
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
        // ALL DAY NATSAT–BROTHER
        // GET api/NatsatBrotherPair/alldaynatsatbrotherpair
        // ==================================================
        [HttpGet("alldaynatsatbrotherpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchAllDays(string natsatbrotherpair)
        {
            if (natsatbrotherpair != "natsatbrotherpair")
                return BadRequest("Parameter must be 'natsatbrotherpair'.");

            var foundRows = await _context.Table1
                .Where(c =>
                    NatsatNumbers.Contains(c.Am) &&
                    BrotherNumbers.Contains(c.Pm))
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound();

            return Ok(await GetFourWeekSetsAsync(foundRows));
        }

        // ==================================================
        // WEEK SETS NATSAT–BROTHER
        // GET api/NatsatBrotherPair/weeksetsnatsatbrotherpair
        // ==================================================
        [HttpGet("weeksetsnatsatbrotherpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchWeekSets(
            string natsatbrotherpair, string day)
        {
            if (natsatbrotherpair != "natsatbrotherpair")
                return BadRequest("Parameter must be 'natsatbrotherpair'.");

            if (!DayOrder.ContainsKey(day))
                return BadRequest("Invalid day.");

            var foundRows = await _context.Table1
                .Where(c =>
                    c.Days == day &&
                    NatsatNumbers.Contains(c.Am) &&
                    BrotherNumbers.Contains(c.Pm))
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
