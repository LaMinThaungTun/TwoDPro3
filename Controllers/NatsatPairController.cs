using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NatsatPairController : Controller
    {
        private readonly CalendarContext _context;

        public NatsatPairController(CalendarContext context)
        {
            _context = context;
        }

        // Natsat numbers list
        private static readonly HashSet<string> NatsatNumbers = new()
        {
            "07", "18", "24", "35", "69", "70", "81", "42", "53", "96"
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
        // Weeks per year (adjust as needed)
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
        // 1) ALL DAYS NATSAT PAIR SEARCH
        // GET api/NatsatPair/alldaynatsatpair?natsatpair=natsatpair
        // ==========================================================
        [HttpGet("alldaynatsatpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchAllDays(string natsatpair)
        {
            if (natsatpair != "natsatpair")
                return BadRequest("Parameter must be 'natsatpair'.");

            var foundRows = await _context.Table1
                .Where(c =>
                    NatsatNumbers.Contains(c.Am) &&
                    NatsatNumbers.Contains(c.Pm))
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No natsat number pairs found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

        // ==========================================================
        // 2) WEEK SETS NATSAT PAIR SEARCH
        // GET api/NatsatPair/weeksetsnatsatpair?natsatpair=natsatpair&day=Monday
        // ==========================================================
        [HttpGet("weeksetsnatsatpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchWeekSets(string natsatpair, string day)
        {
            if (natsatpair != "natsatpair")
                return BadRequest("Parameter must be 'natsatpair'.");

            if (!DayOrder.ContainsKey(day))
                return BadRequest("Invalid day. Use Monday–Friday.");

            var foundRows = await _context.Table1
                .Where(c =>
                    c.Days == day &&
                    NatsatNumbers.Contains(c.Am) &&
                    NatsatNumbers.Contains(c.Pm))
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No natsat number pairs found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

        // ==========================================================
        // WEEK NORMALIZER + FOUR WEEK SET BUILDER (same as before)
        // ==========================================================
        // 🔹 Normalize year/week (handles cross-year boundaries)
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
