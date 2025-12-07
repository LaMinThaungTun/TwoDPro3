using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DgOneSearchController : Controller
    {
            private readonly CalendarContext _context;
            public DgOneSearchController(CalendarContext context)
            {
                _context = context;
            }

            //Order for weekdays
            private static readonly Dictionary<string, int> DayOrder = new()
            {
                ["Monday"] = 1, ["Tuesday"] = 2,["Wednesday"] = 3, ["Thursday"] = 4, ["Friday"] = 5
            };

            // Weeks per year
            private static readonly Dictionary<int, int> WeeksInYear = new()
            {
                [2013] = 52, [2014] = 53, [2015] = 52, [2016] = 52, [2017] = 52, [2018] = 53,
                [2019] = 52, [2020] = 52, [2021] = 52, [2022] = 52, [2023] = 52, [2024] = 52,
                [2025] = 53
            };

        //Endpoint 1: Search across ALL days (AM + PM)
        // Example: GET api/DgOneSearch/alldaysdgone?number=1
        [HttpGet("alldaysdgone")]
            public async Task<ActionResult<List<List<Calendar>>>> SearchAllDays(string number)
            {
                var foundRows = await _context.Table1
                    .Where(c => (c.AmDgOne == number || c.PmDgOne == number) && (c.Years == 2025 || c.Years == 2026))
                    .OrderBy(c => c.Id)
                    .ToListAsync();
                if (!foundRows.Any())
                    return NotFound("No results found.");
                var weekSets = await GetFourWeekSetsAsync(foundRows);
                return Ok(weekSets);

            }

            //Endpoint 2: Search with Day + Time filter
            [HttpGet("weeksetsdgone")]
            public async Task<ActionResult<List<List<Calendar>>>> SearchWeekSets(string number, string day, bool am = false, bool pm = false)
            {
                if (!DayOrder.ContainsKey(day))
                    return BadRequest("Invalid day. Use Monday–Friday.");
                IQueryable<Calendar> query = _context.Table1.Where(c => c.Days == day);

                if (am && pm)
                {
                    query = query.Where(c => c.AmDgOne == number || c.PmDgOne == number);
                }
                else if (am)
                {
                    query = query.Where(c => c.AmDgOne == number);
                }
                else if (pm)
                {
                    query = query.Where(c => c.PmDgOne == number);
                }
                else
                {
                    return BadRequest("At least one of AM or PM must be true.");
                }

                var foundRows = await query
                    .OrderBy(c => c.Id)
                    .ToListAsync();

                if (!foundRows.Any())
                    return NotFound("No results found.");

                var weekSets = await GetFourWeekSetsAsync(foundRows);
                return Ok(weekSets);

            }

            //Normalize year/week (handles cross-year boundaries)
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

        // Helper: Fetch 4-week blocks around each found row
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
