using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrotherPairController : Controller
    {
        private readonly CalendarContext _context;

        public BrotherPairController(CalendarContext context)
        {
            _context = context;
        }

        // Brother numbers (both directions)
        private static readonly HashSet<string> BrotherNumbers = new()
        {
            "01", "12", "23", "34", "45", "56", "67", "78", "89", "90",
            "10", "21", "32", "43", "54", "65", "76", "87", "98", "09"
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

        // ==========================================================
        // 1) ALL DAYS BROTHER PAIR SEARCH
        // GET api/BrotherPair/alldaybrotherpair?brotherpair=brotherpair
        // ==========================================================
        [HttpGet("alldaybrotherpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchAllDays(string brotherpair)
        {
            if (brotherpair != "brotherpair")
                return BadRequest("Parameter must be 'brotherpair'.");

            var foundRows = await _context.Table1
                .Where(c =>
                    BrotherNumbers.Contains(c.Am) &&
                    BrotherNumbers.Contains(c.Pm))
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No brother number pairs found.");

            var weekSets = await GetFourWeekSetsAsync(foundRows);
            return Ok(weekSets);
        }

        // ==========================================================
        // 2) WEEK SETS BROTHER PAIR SEARCH
        // GET api/BrotherPair/weeksetsbrotherpair?brotherpair=brotherpair&day=Monday
        // ==========================================================
        [HttpGet("weeksetsbrotherpair")]
        private async Task<List<List<Calendar>>> GetFourWeekSetsAsync(List<Calendar> foundRows)
        {
            var weekSets = new List<List<Calendar>>();
            var processedWeeks = new HashSet<(int year, int week)>(); // track processed base weeks

            foreach (var row in foundRows)
            {
                var baseKey = (row.Years, row.Weeks);
                if (processedWeeks.Contains(baseKey))
                    continue;

                processedWeeks.Add(baseKey);

                var offsets = new int[] { -2, -1, 0, 1 };
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
                    var uniqueBlock = block
                        .GroupBy(c => c.Id)
                        .Select(g => g.First())
                        .OrderBy(c => c.Id)
                        .ToList();

                    weekSets.Add(uniqueBlock);
                }
            }

            return weekSets.OrderBy(b => b.Min(c => c.Id)).ToList();
        }
    }
}
