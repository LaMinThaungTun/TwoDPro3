using Microsoft.EntityFrameworkCore;
//using System.Globalization;
using TwoDPro3.Models;   // so it knows about your Calendar model

namespace TwoDPro3.Data
{
    public class CalendarContext : DbContext
    {
        public CalendarContext(DbContextOptions<CalendarContext> options) : base(options) { }

        public DbSet<Calendar> Table1 { get; set; }  // maps Calendar model to table1
    }
}