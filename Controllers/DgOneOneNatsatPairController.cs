using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DgOneOneNatsatPairController : Controller
    {
        private readonly CalendarContext _context;

        public DgOneOneNatsatPairController(CalendarContext context)
        {
            _context = context;
        }

        private static readonly Dictionary<string, int> DayOrder = new()
        {
            ["Monday"] = 1,
            ["Tuesday"] = 2,
            ["Wednesday"] = 3,
            ["Thursday"] = 4,
            ["Friday"] = 5
        };

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

        // ================= ALL DAYS =================
        [HttpGet("alldaydgoneonenatsatpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchAllDays(string dgoneonenatsatpair)
        {
            if (dgoneonenatsatpair != "dgoneonenatsatpair")
                return BadRequest("Parameter must be 'dgoneonenatsatpair'.");

            var foundRows = await _context.Table1
                .Where(c =>
                    c.AmDgOne != null &&
                    c.PmDgOne != null &&
                    c.AmDgOne != ClosedCode &&
                    c.PmDgOne != ClosedCode &&
                    c.AmDgOne.Length == 1 &&
                    c.PmDgOne.Length == 1 &&
                    (
                        (c.AmDgOne.Substring(0, 1) == "0" && c.PmDgOne.Substring(0, 1) == "7") ||
                        (c.AmDgOne.Substring(0, 1) == "7" && c.PmDgOne.Substring(0, 1) == "0") ||

                        (c.AmDgOne.Substring(0, 1) == "1" && c.PmDgOne.Substring(0, 1) == "8") ||
                        (c.AmDgOne.Substring(0, 1) == "8" && c.PmDgOne.Substring(0, 1) == "1") ||

                        (c.AmDgOne.Substring(0, 1) == "2" && c.PmDgOne.Substring(0, 1) == "4") ||
                        (c.AmDgOne.Substring(0, 1) == "4" && c.PmDgOne.Substring(0, 1) == "2") ||

                        (c.AmDgOne.Substring(0, 1) == "3" && c.PmDgOne.Substring(0, 1) == "5") ||
                        (c.AmDgOne.Substring(0, 1) == "5" && c.PmDgOne.Substring(0, 1) == "3") ||

                        (c.AmDgOne.Substring(0, 1) == "6" && c.PmDgOne.Substring(0, 1) == "9") ||
                        (c.AmDgOne.Substring(0, 1) == "9" && c.PmDgOne.Substring(0, 1) == "6")
                    )
                    && (c.Years == 2024 || c.Years == 2025 || c.Years == 2026)
                )
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No matching DG One-One natsat pairs found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

        // ================= WEEK SETS =================
        [HttpGet("weeksetsdgoneonenatsatpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchWeekSets(string dgoneonenatsatpair, string day)
        {
            if (dgoneonenatsatpair != "dgoneonenatsatpair")
                return BadRequest("Parameter must be 'dgoneonenatsatpair'.");

            if (!DayOrder.ContainsKey(day))
                return BadRequest("Invalid day. Use Monday–Friday.");

            var foundRows = await _context.Table1
                .Where(c =>
                    c.Days == day &&
                    c.AmDgOne != null &&
                    c.PmDgOne != null &&
                    c.AmDgOne != ClosedCode &&
                    c.PmDgOne != ClosedCode &&
                    c.AmDgOne.Length == 1 &&
                    c.PmDgOne.Length == 1 &&
                    (
                        (c.AmDgOne.Substring(0, 1) == "0" && c.PmDgOne.Substring(0, 1) == "7") ||
                        (c.AmDgOne.Substring(0, 1) == "7" && c.PmDgOne.Substring(0, 1) == "0") ||

                        (c.AmDgOne.Substring(0, 1) == "1" && c.PmDgOne.Substring(0, 1) == "8") ||
                        (c.AmDgOne.Substring(0, 1) == "8" && c.PmDgOne.Substring(0, 1) == "1") ||

                        (c.AmDgOne.Substring(0, 1) == "2" && c.PmDgOne.Substring(0, 1) == "4") ||
                        (c.AmDgOne.Substring(0, 1) == "4" && c.PmDgOne.Substring(0, 1) == "2") ||

                        (c.AmDgOne.Substring(0, 1) == "3" && c.PmDgOne.Substring(0, 1) == "5") ||
                        (c.AmDgOne.Substring(0, 1) == "5" && c.PmDgOne.Substring(0, 1) == "3") ||

                        (c.AmDgOne.Substring(0, 1) == "6" && c.PmDgOne.Substring(0, 1) == "9") ||
                        (c.AmDgOne.Substring(0, 1) == "9" && c.PmDgOne.Substring(0, 1) == "6")
                    )
                )
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No matching DG One-One natsat pairs found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

        // ================= HELPERS =================
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

        private async Task<List<List<Calendar>>> GetFourWeekSetsAsync(List<Calendar> foundRows)
        {
            var weekSets = new List<List<Calendar>>();
            var processedWeeks = new HashSet<(int year, int week)>();

            foreach (var row in foundRows)
            {
                if (!processedWeeks.Add((row.Years, row.Weeks)))
                    continue;

                var offsets = new[] { -2, -1, 0, 1 };
                var weeks = offsets
                    .Select(o => NormalizeWeek(row.Years, row.Weeks + o))
                    .Distinct();

                var block = new List<Calendar>();

                foreach (var (y, w) in weeks)
                {
                    var rows = await _context.Table1
                        .Where(c => c.Years == y && c.Weeks == w)
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
