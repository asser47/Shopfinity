using System.ComponentModel.DataAnnotations;

namespace ECommerce.DAL.Models
{
    public class Customer : ApplicationUser
    {
        [StringLength(500)]
        [Display(Name = "Shipping Address")]
        public string? Address { get; set; }
    }
}