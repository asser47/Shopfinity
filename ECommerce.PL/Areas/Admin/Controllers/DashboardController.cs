using ECommerce.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.PL.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IOrderService _orderService;

        public DashboardController(
            IProductService productService,
            ICategoryService categoryService,
            IOrderService orderService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _orderService = orderService;
        }

        public IActionResult Index()
        {
            var totalProducts = _productService.GetAllProducts().Count();
            var totalCategories = _categoryService.GetAllCategories().Count();
            var allOrders = _orderService.GetAllOrders();

            var totalOrders = allOrders.Count();
            var totalRevenue = allOrders.Sum(o => o.TotalAmount);

            var lowStockProducts = _orderService.GetLowStockProducts(10);
            var outOfStockProducts = _orderService.GetOutOfStockProducts();

            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalCategories = totalCategories;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.LowStockCount = lowStockProducts.Count();
            ViewBag.OutOfStockCount = outOfStockProducts.Count();
            ViewBag.LowStockProducts = lowStockProducts.Take(5).ToList();

            return View();
        }
    }
}
