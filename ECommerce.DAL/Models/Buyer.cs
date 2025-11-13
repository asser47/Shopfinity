using System.ComponentModel.DataAnnotations;

namespace ECommerce.DAL.Models
{
    public class Buyer : ApplicationUser
    {
        [StringLength(200)]
        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; }

        [StringLength(50)]
        [Display(Name = "Tax Number")]
        public string? TaxNumber { get; set; }
    }
}