using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hrms.Models
{
    public class Salary
    {
        [Key]
        public int SalaryId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyWage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal YearlyWage { get; set; }

        public int WorkingDaysPerMonth { get; set; } = 26;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PerDayWage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Basic { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal HRA { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PF { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Allowances { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Deductions { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ProfessionalTax { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }
    }
}
