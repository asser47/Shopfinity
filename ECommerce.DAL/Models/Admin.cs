using System.ComponentModel.DataAnnotations;

namespace ECommerce.DAL.Models
{
    public class Admin : ApplicationUser
    {
        [StringLength(100)]
        [Display(Name = "Department")]
        public string? Department { get; set; }
    }
}