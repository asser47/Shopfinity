using ECommerce.BLL.Interfaces;
using ECommerce.PL.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.PL.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: Category/Index
        public IActionResult Index()
        {
            var categories = _categoryService.GetAllCategories().ToList();  // ⭐ ToList() FIRST!

            var viewModels = new List<CategoryViewModel>();  // ⭐ Create list

            foreach (var category in categories)
            {
                var productCount = _categoryService.GetProductsByCategory(category.Id).Count();
                viewModels.Add(new CategoryViewModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ProductCount = productCount
                });
            }

            return View(viewModels);
        }

        // GET: Category/Details/5
        public IActionResult Details(int id)
        {
            var category = _categoryService.GetCategoryById(id);

            if (category == null)
            {
                return NotFound();
            }

            var products = _categoryService.GetProductsByCategory(id).ToList();  // ⭐ ToList()!

            Console.WriteLine($"Details - Category: {category.Name}, Products: {products.Count}");

            var viewModel = new CategoryProductsViewModel
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                CategoryDescription = category.Description,
                Products = products.Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description ?? "",
                    Price = p.Price,
                    ImageUrl = p.ImageUrl ?? "",
                    Stock = p.Stock,
                    IsOutOfStock = p.Stock == 0
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: Category/Products/5
        public IActionResult Products(int id)
        {
            var category = _categoryService.GetCategoryById(id);

            if (category == null)
            {
                return NotFound();
            }

            var products = _categoryService.GetProductsByCategory(id).ToList();  // ⭐ ToList()!

            Console.WriteLine($"Products - Category: {category.Name}, Products: {products.Count}");
            foreach (var p in products)
            {
                Console.WriteLine($"  - {p.Name}");
            }

            var viewModel = new CategoryProductsViewModel
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                CategoryDescription = category.Description ?? "",
                Products = products.Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description ?? "",
                    Price = p.Price,
                    ImageUrl = p.ImageUrl ?? "",
                    Stock = p.Stock,
                    IsOutOfStock = p.Stock == 0
                }).ToList()
            };

            return View(viewModel);
        }
    }
}