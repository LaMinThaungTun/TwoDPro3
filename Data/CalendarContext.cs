using Microsoft.EntityFrameworkCore;
using TwoDPro3.Models;

namespace TwoDPro3.Data
{
    public class CalendarContext : DbContext
    {
        public CalendarContext(DbContextOptions<CalendarContext> options) : base(options) { }

        public DbSet<Calendar> Table1 { get; set; }  // maps Calendar to table1
    }
}