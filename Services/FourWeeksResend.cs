using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Services
{
    public class FourWeeksResend
    {
        private readonly CalendarContext _context;

        public FourWeeksResend(CalendarContext context)
        {
            _context = context;
        }

        public async Task<List<Calendar>> GetFourWeeksDataAsync(int number, string day, string time)
        {
            // Convert number to string for comparison
            string numberStr = number.ToString();

            IQueryable<Calendar> query = _context.Table1;

            if (time.Equals("AM", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(c => c.Am == numberStr);
            }
            else if (time.Equals("PM", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(c => c.Pm == numberStr);
            }

            var matches = await query.ToListAsync();

            var results = new List<Calendar>();

            foreach (var match in matches)
            {
                var ranges = new List<(int Year, int Week)>
        {
            (match.Years, match.Weeks - 2),
            (match.Years, match.Weeks - 1),
            (match.Years, match.Weeks),
            (match.Years, match.Weeks + 1)
        };

                string columnCheck = numberStr;

                IQueryable<Calendar> fourWeeksQuery = _context.Table1;

                if (time.Equals("AM", StringComparison.OrdinalIgnoreCase))
                {
                    fourWeeksQuery = fourWeeksQuery.Where(c => c.Am == columnCheck);
                }
                else if (time.Equals("PM", StringComparison.OrdinalIgnoreCase))
                {
                    fourWeeksQuery = fourWeeksQuery.Where(c => c.Pm == columnCheck);
                }

                var allCandidates = await fourWeeksQuery.ToListAsync();

                var filtered = allCandidates
                    .Where(c => ranges.Any(r => r.Year == c.Years && r.Week == c.Weeks))
                    .ToList();

                results.AddRange(filtered);
            }

            return results;
        }
    }
}