using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoubleBrotherPairController : Controller
    {
        private readonly CalendarContext _context;

        public DoubleBrotherPairController(CalendarContext context)
        {
            _context = context;
        }

        // ==========================
        // DOUBLE & BROTHER NUMBERS
        // ==========================
        private static readonly HashSet<string> DoubleNumbers = new()
        {
            "00","11","22","33","44","55","66","77","88","99"
        };

        private static readonly HashSet<string> BrotherNumbers = new()
        {
            "01","12","23","34","45","56","67","78","89","90",
            "10","21","32","43","54","65","76","87","98","09"
        };

        // ==========================
        // WEEKDAY ORDER
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

        // ==========================================================
        // 1) ALL DAY DOUBLE–BROTHER PAIR
        // GET api/DoubleBrotherPair/alldaydoublebrotherpair?doublebrotherpair=doublebrotherpair
        // ==========================================================
        [HttpGet("alldaydoublebrotherpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchAllDays(string doublebrotherpair)
        {
            if (doublebrotherpair != "doublebrotherpair")
                return BadRequest("Parameter must be 'doublebrotherpair'.");

            var foundRows = await _context.Table1
                .Where(c =>
                    DoubleNumbers.Contains(c.Am) &&
                    BrotherNumbers.Contains(c.Pm))
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No double brother pairs found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

        // ==========================================================
        // 2) WEEK SETS DOUBLE–BROTHER PAIR
        // GET api/DoubleBrotherPair/weeksetsdoublebrotherpair?doublebrotherpair=doublebrotherpair&day=Monday
        // ==========================================================
        [HttpGet("weeksetsdoublebrotherpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchWeekSets(string doublebrotherpair, string day)
        {
            if (doublebrotherpair != "doublebrotherpair")
                return BadRequest("Parameter must be 'doublebrotherpair'.");

            if (!DayOrder.ContainsKey(day))
                return BadRequest("Invalid day. Use Monday–Friday.");

            var foundRows = await _context.Table1
                .Where(c =>
                    c.Days == day &&
                    DoubleNumbers.Contains(c.Am) &&
                    BrotherNumbers.Contains(c.Pm))
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No double brother pairs found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

        // ==========================================================
        // WEEK NORMALIZER
        // ==========================================================
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
                return (year + 1, week - maxWeeks);
            }

            return (year, week);
        }

        // ==========================================================
        // FOUR WEEK SET BUILDER
        // ==========================================================
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
