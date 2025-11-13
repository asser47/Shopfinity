using System.ComponentModel.DataAnnotations;

namespace ECommerce.PL.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Shipping address is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Address must be between 10-500 characters")]
        [Display(Name = "Shipping Address")]
        [DataType(DataType.MultilineText)]
        public string ShippingAddress { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(100)]
        [Display(Name = "Phone Number")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; }

        [StringLength(500)]
        [Display(Name = "Order Notes (Optional)")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        // Display-only properties (NOT user input)
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Total { get; set; }
        public int ItemCount { get; set; }
    }
}
