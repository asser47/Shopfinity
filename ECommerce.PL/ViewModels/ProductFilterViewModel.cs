using ECommerce.DAL.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.PL.ViewModels
{
    public class ProductFilterViewModel
    {
        // Filter parameters
        public string SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? InStock { get; set; }
        public string SortBy { get; set; }

        // Results
        public IEnumerable<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
        public int TotalResults { get; set; }

        // For dropdowns
        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> SortOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Text = "Name (A-Z)", Value = "name_asc" },
            new SelectListItem { Text = "Name (Z-A)", Value = "name_desc" },
            new SelectListItem { Text = "Price (Low to High)", Value = "price_asc" },
            new SelectListItem { Text = "Price (High to Low)", Value = "price_desc" },
            new SelectListItem { Text = "Newest First", Value = "newest" }
        };
    }
}
