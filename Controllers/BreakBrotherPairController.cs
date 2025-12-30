using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BreakBrotherPairController : Controller
    {
        private readonly CalendarContext _context;

        public BreakBrotherPairController(CalendarContext context)
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
        [HttpGet("alldaybreakbrotherpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchAllDays(string breakbrotherpair)
        {
            if (breakbrotherpair != "breakbrotherpair")
                return BadRequest("Parameter must be 'breakbrotherpair'.");

            var foundRows = await _context.Table1
                .Where(c =>
                    c.AmBreak != null &&
                    c.PmBreak != null &&
                    c.AmBreak != ClosedCode &&
                    c.PmBreak != ClosedCode &&
                    (
                        (c.AmBreak == "0" && c.PmBreak == "1") || (c.AmBreak == "1" && c.PmBreak == "0") ||
                        (c.AmBreak == "1" && c.PmBreak == "2") || (c.AmBreak == "2" && c.PmBreak == "1") ||
                        (c.AmBreak == "2" && c.PmBreak == "3") || (c.AmBreak == "3" && c.PmBreak == "2") ||
                        (c.AmBreak == "3" && c.PmBreak == "4") || (c.AmBreak == "4" && c.PmBreak == "3") ||
                        (c.AmBreak == "4" && c.PmBreak == "5") || (c.AmBreak == "5" && c.PmBreak == "4") ||
                        (c.AmBreak == "5" && c.PmBreak == "6") || (c.AmBreak == "6" && c.PmBreak == "5") ||
                        (c.AmBreak == "6" && c.PmBreak == "7") || (c.AmBreak == "7" && c.PmBreak == "6") ||
                        (c.AmBreak == "7" && c.PmBreak == "8") || (c.AmBreak == "8" && c.PmBreak == "7") ||
                        (c.AmBreak == "8" && c.PmBreak == "9") || (c.AmBreak == "9" && c.PmBreak == "8") ||
                        (c.AmBreak == "9" && c.PmBreak == "0") || (c.AmBreak == "0" && c.PmBreak == "9")
                    ) && (c.Years == 2024 || c.Years == 2025 || c.Years == 2026)
                )
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No matching break brother pairs found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

        // ================= WEEK SETS =================
        [HttpGet("weeksetsbreakbrotherpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchWeekSets(string breakbrotherpair, string day)
        {
            if (breakbrotherpair != "breakbrotherpair")
                return BadRequest("Parameter must be 'breakbrotherpair'.");

            if (!DayOrder.ContainsKey(day))
                return BadRequest("Invalid day. Use Monday–Friday.");

            var foundRows = await _context.Table1
                .Where(c =>
                    c.Days == day &&
                    c.AmBreak != null &&
                    c.PmBreak != null &&
                    c.AmBreak != ClosedCode &&
                    c.PmBreak != ClosedCode &&
                    (
                        (c.AmBreak == "0" && c.PmBreak == "1") || (c.AmBreak == "1" && c.PmBreak == "0") ||
                        (c.AmBreak == "1" && c.PmBreak == "2") || (c.AmBreak == "2" && c.PmBreak == "1") ||
                        (c.AmBreak == "2" && c.PmBreak == "3") || (c.AmBreak == "3" && c.PmBreak == "2") ||
                        (c.AmBreak == "3" && c.PmBreak == "4") || (c.AmBreak == "4" && c.PmBreak == "3") ||
                        (c.AmBreak == "4" && c.PmBreak == "5") || (c.AmBreak == "5" && c.PmBreak == "4") ||
                        (c.AmBreak == "5" && c.PmBreak == "6") || (c.AmBreak == "6" && c.PmBreak == "5") ||
                        (c.AmBreak == "6" && c.PmBreak == "7") || (c.AmBreak == "7" && c.PmBreak == "6") ||
                        (c.AmBreak == "7" && c.PmBreak == "8") || (c.AmBreak == "8" && c.PmBreak == "7") ||
                        (c.AmBreak == "8" && c.PmBreak == "9") || (c.AmBreak == "9" && c.PmBreak == "8") ||
                        (c.AmBreak == "9" && c.PmBreak == "0") || (c.AmBreak == "0" && c.PmBreak == "9")
                    )
                )
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No matching break brother pairs found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

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

        // Fetch 4-week blocks around each found row
        private async Task<List<List<Calendar>>> GetFourWeekSetsAsync(List<Calendar> foundRows)
        {
            var weekSets = new List<List<Calendar>>();
            var processedWeeks = new HashSet<(int year, int week)>(); // track processed base weeks

            foreach (var row in foundRows)
            {
                // Skip if we've already processed this base week (year + week combination)
                var baseKey = (row.Years, row.Weeks);
                if (processedWeeks.Contains(baseKey))
                    continue;

                processedWeeks.Add(baseKey);

                // Define offsets for 4-week block
                var offsets = new int[] { -2, -1, 0, 1 };

                // Normalize and collect week-year pairs for this block
                var normalizedWeeks = offsets
                    .Select(offset => NormalizeWeek(row.Years, row.Weeks + offset))
                    .Distinct()
                    .ToList();

                var block = new List<Calendar>();

                foreach (var (normYear, normWeek) in normalizedWeeks)
                {
                    var weekRows = await _context.Table1
                        .Where(c => c.Years == normYear && c.Weeks == normWeek)
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
                    // Remove duplicates within this block (in case of data overlaps)
                    var uniqueBlock = block
                        .GroupBy(c => c.Id)
                        .Select(g => g.First())
                        .OrderBy(c => c.Id)
                        .ToList();

                    weekSets.Add(uniqueBlock);
                }
            }

            // Sort final list by the earliest record in each block
            weekSets = weekSets
                .OrderBy(b => b.Min(c => c.Id))
                .ToList();

            return weekSets;
        }
    }
}
