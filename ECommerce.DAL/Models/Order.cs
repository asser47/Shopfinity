using ECommerce.DAL.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.DAL.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        [StringLength(450)]
        public string UserId { get; set; }

        [Required]
        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingCost { get; set; } = 10.00m;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // ✅ Store as enum directly (EF Core will handle as int)
        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required(ErrorMessage = "Shipping address is required")]
        [StringLength(500, MinimumLength = 10)]
        [DataType(DataType.MultilineText)]
        public string ShippingAddress { get; set; }

        [StringLength(100)]
        [Phone]
        public string PhoneNumber { get; set; }

        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? PaymentMethod { get; set; }

        [StringLength(200)]
        public string? TransactionId { get; set; }

        public DateTime? PaymentDate { get; set; }
        [StringLength(100)]
        [Display(Name = "Tracking Number")]
        public string? TrackingNumber { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }

        // Navigation property
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
