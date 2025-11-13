using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.DAL.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Order ID is required")]
        [ForeignKey(nameof(Order))]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Product ID is required")]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 10000, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
        [Required(ErrorMessage = "Price is required")]
        [Range(1, 1000000, ErrorMessage = "Price must be between 1 and 10000")]
        [Display(Name = "Unit Price")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        public Order Order { get; set; }
        public Product Product { get; set; }
        [NotMapped]
        [Display(Name = "Subtotal")]
        [DataType(DataType.Currency)]
        public decimal Subtotal => Price * Quantity;
    }
}

