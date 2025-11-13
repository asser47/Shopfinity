using ECommerce.BLL.Interfaces;
using ECommerce.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.PL.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;

        public OrderController(IOrderService orderService, IProductService productService)
        {
            _orderService = orderService;
            _productService = productService;
        }

        // GET: Admin/Order
        public IActionResult Index()
        {
            var orders = _orderService.GetAllOrders();
            return View(orders);
        }

        // GET: Admin/Order/Details/5
        public IActionResult Details(int id)
        {
            var order = _orderService.GetOrderByIdWithDetails(id);
            if (order == null)
                return NotFound();

            return View(order);
        }

        // POST: Admin/Order/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(int orderId, OrderStatus status)
        {
            try
            {
                _orderService.UpdateOrderStatus(orderId, status);
                TempData["SuccessMessage"] = $"Order status updated to {status}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Details", new { id = orderId });
        }
    }
}
