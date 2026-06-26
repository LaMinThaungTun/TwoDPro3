using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DgTwoOneNatsatPairController : Controller
    {
        private readonly CalendarContext _context;

        public DgTwoOneNatsatPairController(CalendarContext context)
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
        [HttpGet("alldaydgtwoonenatsatpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchAllDays(string dgtwoonenatsatpair)
        {
            if (dgtwoonenatsatpair != "dgtwoonenatsatpair")
                return BadRequest("Parameter must be 'dgtwoonenatsatpair'.");

            var foundRows = await _context.Table1
                .Where(c =>
                    c.AmDgTwo != null &&
                    c.PmDgOne != null &&
                    c.AmDgTwo != ClosedCode &&
                    c.PmDgOne != ClosedCode &&
                    (
                        (c.AmDgTwo == "0" && c.PmDgOne == "7") || (c.AmDgTwo == "7" && c.PmDgOne == "0") ||
                        (c.AmDgTwo == "1" && c.PmDgOne == "8") || (c.AmDgTwo == "8" && c.PmDgOne == "1") ||
                        (c.AmDgTwo == "2" && c.PmDgOne == "4") || (c.AmDgTwo == "4" && c.PmDgOne == "2") ||
                        (c.AmDgTwo == "3" && c.PmDgOne == "5") || (c.AmDgTwo == "5" && c.PmDgOne == "3") ||
                        (c.AmDgTwo == "6" && c.PmDgOne == "9") || (c.AmDgTwo == "9" && c.PmDgOne == "6")
                    )
                    
                )
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No matching DG Two-One natsat pairs found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

        // ================= WEEK SETS =================
        [HttpGet("weeksetsdgtwoonenatsatpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchWeekSets(string dgtwoonenatsatpair, string day)
        {
            if (dgtwoonenatsatpair != "dgtwoonenatsatpair")
                return BadRequest("Parameter must be 'dgtwoonenatsatpair'.");

            if (!DayOrder.ContainsKey(day))
                return BadRequest("Invalid day. Use Monday–Friday.");

            var foundRows = await _context.Table1
                .Where(c =>
                    c.Days == day &&
                    c.AmDgTwo != null &&
                    c.PmDgOne != null &&
                    c.AmDgTwo != ClosedCode &&
                    c.PmDgOne != ClosedCode &&
                    (
                        (c.AmDgTwo == "0" && c.PmDgOne == "7") || (c.AmDgTwo == "7" && c.PmDgOne == "0") ||
                        (c.AmDgTwo == "1" && c.PmDgOne == "8") || (c.AmDgTwo == "8" && c.PmDgOne == "1") ||
                        (c.AmDgTwo == "2" && c.PmDgOne == "4") || (c.AmDgTwo == "4" && c.PmDgOne == "2") ||
                        (c.AmDgTwo == "3" && c.PmDgOne == "5") || (c.AmDgTwo == "5" && c.PmDgOne == "3") ||
                        (c.AmDgTwo == "6" && c.PmDgOne == "9") || (c.AmDgTwo == "9" && c.PmDgOne == "6")
                    )
                )
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No matching DG Two-One natsat pairs found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

        // ================= HELPERS =================
        private (int Year, int Week) NormalizeWeek(int year, int week)
        {
            int maxWeeks = WeeksInYear.ContainsKey(year) ? WeeksInYear[year] : 52;

            if (week < 1)
                return (year - 1, (WeeksInYear.ContainsKey(year - 1) ? WeeksInYear[year - 1] : 52) + week);

            if (week > maxWeeks)
                return (year + 1, week - maxWeeks);

            return (year, week);
        }

        private async Task<List<List<Calendar>>> GetFourWeekSetsAsync(List<Calendar> foundRows)
        {
            var weekSets = new List<List<Calendar>>();
            var processedWeeks = new HashSet<(int, int)>();

            foreach (var row in foundRows)
            {
                if (!processedWeeks.Add((row.Years, row.Weeks)))
                    continue;

                var offsets = new[] { -2, -1, 0, 1 };
                var weeks = offsets.Select(o => NormalizeWeek(row.Years, row.Weeks + o)).Distinct();

                var block = new List<Calendar>();
                foreach (var (y, w) in weeks)
                {
                    var rows = await _context.Table1.Where(c => c.Years == y && c.Weeks == w).ToListAsync();
                    block.AddRange(rows.OrderBy(c => DayOrder.ContainsKey(c.Days) ? DayOrder[c.Days] : 999).ThenBy(c => c.Id));
                }

                weekSets.Add(block.GroupBy(c => c.Id).Select(g => g.First()).OrderBy(c => c.Id).ToList());
            }

            return weekSets.OrderBy(b => b.Min(c => c.Id)).ToList();
        }
    }
}
