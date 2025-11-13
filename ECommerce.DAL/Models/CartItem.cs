using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.DAL.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Cart ID is required")]
        [Display(Name = "Cart")]
        public int CartId { get; set; }

        [Required(ErrorMessage = "Product is required")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [ForeignKey(nameof(CartId))]
        public  Cart Cart { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        [NotMapped]
        [Display(Name = "Subtotal")]
        [DataType(DataType.Currency)]
        public decimal Subtotal => Product?.Price * Quantity ?? 0;
    }
}
