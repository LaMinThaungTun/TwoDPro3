using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class DoubleSearchController : Controller
    {
        private readonly CalendarContext _context;

        public DoubleSearchController(CalendarContext context)
        {
            _context = context;
        }

        // List of DOUBLE numbers
        private static readonly List<string> DoubleNumbers = new()
        {
            "00","11","22","33","44","55","66","77","88","99"
        };

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
        // GET api/DoubleSearch/alldaysdouble?double=double
        // ==========================================================
        [HttpGet("alldaysdouble")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchAllDays(string dbl)
        {
            if (dbl != "double")
                return BadRequest("Parameter must be 'double'.");

            var foundRows = await _context.Table1
                .Where(c =>
                    (DoubleNumbers.Contains(c.Am) ||
                     DoubleNumbers.Contains(c.Pm))
                    && c.Years == 2025)
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No double numbers found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

        // ==========================================================
        // 2) WEEKSETS SEARCH
        // GET api/DoubleSearch/weeksetsdouble?double=double&day=Monday&am=true
        // ==========================================================
        [HttpGet("weeksetsdouble")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchWeekSets(
            string dbl, string day, bool am = false, bool pm = false)
        {
            if (dbl != "double")
                return BadRequest("Parameter must be 'double'.");

            if (!DayOrder.ContainsKey(day))
                return BadRequest("Invalid day. Use Monday–Friday.");

            IQueryable<Calendar> query = _context.Table1.Where(c => c.Days == day);

            if (am && pm)
            {
                query = query.Where(c =>
                    DoubleNumbers.Contains(c.Am) ||
                    DoubleNumbers.Contains(c.Pm));
            }
            else if (am)
            {
                query = query.Where(c =>
                    DoubleNumbers.Contains(c.Am));
            }
            else if (pm)
            {
                query = query.Where(c =>
                    DoubleNumbers.Contains(c.Pm));
            }
            else
            {
                return BadRequest("Either AM or PM must be true.");
            }

            var foundRows = await query.OrderBy(c => c.Id).ToListAsync();

            if (!foundRows.Any())
                return NotFound("No double numbers found.");

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
                int nextYear = year + 1;
                return (nextYear, week - maxWeeks);
            }

            return (year, week);
        }

        // ==========================================================
        // FOUR-WEEK BLOCK BUILDER
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

            return weekSets
                .OrderBy(b => b.Min(c => c.Id))
                .ToList();
        }



    }
}
