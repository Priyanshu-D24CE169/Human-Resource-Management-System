namespace hrms.Models
{
    public class CompanySettings
    {
        public int Id { get; set; }
        public string CompanyCode { get; set; } = "OI";
        public string CompanyName { get; set; } = "Odoo India";
        public int CurrentYear { get; set; } = DateTime.Now.Year;
    }
}
