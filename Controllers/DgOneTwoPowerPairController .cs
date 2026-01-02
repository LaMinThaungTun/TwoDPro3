using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DgOneTwoPowerPairController : Controller
    {
        private readonly CalendarContext _context;

        public DgOneTwoPowerPairController(CalendarContext context)
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
        [HttpGet("alldaydgonetwopowerpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchAllDays(string dgonetwopowerpair)
        {
            if (dgonetwopowerpair != "dgonetwopowerpair")
                return BadRequest("Parameter must be 'dgonetwopowerpair'.");

            var foundRows = await _context.Table1
                .Where(c =>
                    c.AmDgOne != null &&
                    c.PmDgTwo != null &&
                    c.AmDgOne != ClosedCode &&
                    c.PmDgTwo != ClosedCode &&
                    (
                        (c.AmDgOne == "0" && c.PmDgTwo == "5") || (c.AmDgOne == "5" && c.PmDgTwo == "0") ||
                        (c.AmDgOne == "1" && c.PmDgTwo == "6") || (c.AmDgOne == "6" && c.PmDgTwo == "1") ||
                        (c.AmDgOne == "2" && c.PmDgTwo == "7") || (c.AmDgOne == "7" && c.PmDgTwo == "2") ||
                        (c.AmDgOne == "3" && c.PmDgTwo == "8") || (c.AmDgOne == "8" && c.PmDgTwo == "3") ||
                        (c.AmDgOne == "4" && c.PmDgTwo == "9") || (c.AmDgOne == "9" && c.PmDgTwo == "4")
                    )
                    
                )
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No matching DG One-Two power pairs found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

        // ================= WEEK SETS =================
        [HttpGet("weeksetsdgonetwopowerpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchWeekSets(string dgonetwopowerpair, string day)
        {
            if (dgonetwopowerpair != "dgonetwopowerpair")
                return BadRequest("Parameter must be 'dgonetwopowerpair'.");

            if (!DayOrder.ContainsKey(day))
                return BadRequest("Invalid day. Use Monday–Friday.");

            var foundRows = await _context.Table1
                .Where(c =>
                    c.Days == day &&
                    c.AmDgOne != null &&
                    c.PmDgTwo != null &&
                    c.AmDgOne != ClosedCode &&
                    c.PmDgTwo != ClosedCode &&
                    (
                        (c.AmDgOne == "0" && c.PmDgTwo == "5") || (c.AmDgOne == "5" && c.PmDgTwo == "0") ||
                        (c.AmDgOne == "1" && c.PmDgTwo == "6") || (c.AmDgOne == "6" && c.PmDgTwo == "1") ||
                        (c.AmDgOne == "2" && c.PmDgTwo == "7") || (c.AmDgOne == "7" && c.PmDgTwo == "2") ||
                        (c.AmDgOne == "3" && c.PmDgTwo == "8") || (c.AmDgOne == "8" && c.PmDgTwo == "3") ||
                        (c.AmDgOne == "4" && c.PmDgTwo == "9") || (c.AmDgOne == "9" && c.PmDgTwo == "4")
                    )
                )
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No matching DG One-Two power pairs found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

        // ================= WEEK NORMALIZATION =================
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

        // ================= 4-WEEK BLOCK =================
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

                var offsets = new[] { -2, -1, 0, 1 };
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
