using ECommerce.BLL.Interfaces;
using ECommerce.PL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.PL.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        // GET: Product/Index with filters
        public IActionResult Index(
            string searchTerm = null,
            int? categoryId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? inStock = null,
            string sortBy = "name_asc")
        {
            // Get filtered products
            var products = _productService.SearchProducts(
                searchTerm,
                categoryId,
                minPrice,
                maxPrice,
                inStock,
                sortBy);

            // Prepare categories for dropdown
            var categories = _categoryService.GetAllCategories()
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString(),
                    Selected = c.Id == categoryId
                }).ToList();

            categories.Insert(0, new SelectListItem { Text = "All Categories", Value = "" });

            // Build view model
            var viewModel = new ProductFilterViewModel
            {
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                InStock = inStock,
                SortBy = sortBy,
                Products = products,
                TotalResults = products.Count(),
                Categories = categories
            };

            return View(viewModel);
        }

        // GET: Product/Details/5
        public IActionResult Details(int id)
        {
            var product = _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
    }
}
