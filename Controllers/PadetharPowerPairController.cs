using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PadetharPowerPairController : Controller
    {
        private readonly CalendarContext _context;

        public PadetharPowerPairController(CalendarContext context)
        {
            _context = context;
        }

        // Padethar numbers
        private static readonly HashSet<string> PadetharNumbers = new()
        {
            "14","15","17","25","28","29","36","37","39","46","57","59","68","79",
            "41","51","71","52","82","92","63","73","93","64","75","95","86","97"
        };

        // Power numbers
        private static readonly HashSet<string> PowerNumbers = new()
        {
            "05","16","27","38","49",
            "50","61","72","83","94"
        };

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

        // ==================================================
        // ALL DAYS
        // GET api/PadetharPowerPair/alldaypadetharpowerpair?padetharpowerpair=padetharpowerpair
        // ==================================================
        [HttpGet("alldaypadetharpowerpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchAllDays(string padetharpowerpair)
        {
            if (padetharpowerpair != "padetharpowerpair")
                return BadRequest("Parameter must be 'padetharpowerpair'.");

            var foundRows = await _context.Table1
                .Where(c =>
                    PadetharNumbers.Contains(c.Am) &&
                    PowerNumbers.Contains(c.Pm))
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No padethar-power pairs found.");

            return Ok(await GetFourWeekSetsAsync(foundRows));
        }

        // ==================================================
        // WEEK SETS
        // GET api/PadetharPowerPair/weeksetspadetharpowerpair?padetharpowerpair=padetharpowerpair&day=Monday
        // ==================================================
        [HttpGet("weeksetspadetharpowerpair")]
        public async Task<ActionResult<List<List<Calendar>>>> SearchWeekSets(string padetharpowerpair, string day)
        {
            if (padetharpowerpair != "padetharpowerpair")
                return BadRequest("Parameter must be 'padetharpowerpair'.");

            if (!DayOrder.ContainsKey(day))
                return BadRequest("Invalid day. Use Monday–Friday.");

            var foundRows = await _context.Table1
                .Where(c =>
                    c.Days == day &&
                    PadetharNumbers.Contains(c.Am) &&
                    PowerNumbers.Contains(c.Pm))
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!foundRows.Any())
                return NotFound("No padethar-power pairs found.");

            return Ok(await GetFourWeekSetsAsync(foundRows));
        }

        // ==================================================
        // SHARED LOGIC (UNCHANGED)
        // ==================================================
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

        private async Task<List<List<Calendar>>> GetFourWeekSetsAsync(List<Calendar> foundRows)
        {
            var weekSets = new List<List<Calendar>>();
            var processed = new HashSet<(int, int)>();

            foreach (var row in foundRows)
            {
                var key = (row.Years, row.Weeks);
                if (processed.Contains(key)) continue;
                processed.Add(key);

                int[] offsets = { -2, -1, 0, 1 };
                var block = new List<Calendar>();

                foreach (var offset in offsets)
                {
                    var (y, w) = NormalizeWeek(row.Years, row.Weeks + offset);

                    var rows = await _context.Table1
                        .Where(c => c.Years == y && c.Weeks == w)
                        .ToListAsync();

                    block.AddRange(
                        rows.OrderBy(c => DayOrder.GetValueOrDefault(c.Days, 999))
                            .ThenBy(c => c.Id)
                    );
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
