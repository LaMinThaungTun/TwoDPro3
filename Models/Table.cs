using System.ComponentModel.DataAnnotations.Schema;

namespace TwoDPro3.Models
{
    [Table("table1")]   // maps this class to your PostgreSQL table named "table1"
    public class Calendar
    {
        [Column("years")]
        public int Years { get; set; }

        [Column("weeks")]
        public int Weeks { get; set; }

        [Column("days")]
        public string? Days { get; set; }

        [Column("am")]
        public string? Am { get; set; }

        [Column("pm")]
        public string? Pm { get; set; }

        [Column("id")]   
        public int Id { get; set; }

        [Column("ambreak")]
        public string? AmBreak { get; set; }

        [Column("pmbreak")]
        public string? PmBreak { get; set; }

        [Column("amdgone")]
        public string? AmDgOne { get; set; }

        [Column("amdgtwo")]
        public string? AmDgTwo { get; set; }

        [Column("pmdgone")]
        public string? PmDgOne { get; set; }

        [Column("pmdgtwo")]
        public string? PmDgTwo { get; set; }

    }
}