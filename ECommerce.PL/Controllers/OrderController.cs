using ECommerce.BLL.Interfaces;
using ECommerce.DAL.Enums;
using ECommerce.PL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.PL.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly IProductService _productService;
        private readonly IConfiguration _configuration;


        public OrderController(
            IOrderService orderService,
            ICartService cartService,
            IProductService productService,
            IConfiguration configuration)
        {
            _orderService = orderService;
            _cartService = cartService;
            _productService = productService;
            _configuration = configuration;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // GET: Order/Checkout
        [HttpGet]
        public IActionResult Checkout()
        {
            var userId = GetUserId();
            var cartItems = _cartService.GetCartItems(userId).ToList();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            decimal subtotal = 0;
            foreach (var item in cartItems)
            {
                var product = _productService.GetProductById(item.ProductId);
                if (product != null)
                    subtotal += product.Price * item.Quantity;
            }

            var viewModel = new CheckoutViewModel
            {
                Subtotal = subtotal,
                Tax = subtotal * 0.14m,
                ShippingCost = 10.00m,
                Total = subtotal + (subtotal * 0.14m) + 10.00m,
                ItemCount = cartItems.Sum(ci => ci.Quantity)
            };

            return View(viewModel);
        }

        // POST: Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload model data
                var userId = GetUserId();
                var cartItems = _cartService.GetCartItems(userId).ToList();

                decimal subtotal = 0;
                foreach (var item in cartItems)
                {
                    var product = _productService.GetProductById(item.ProductId);
                    if (product != null)
                        subtotal += product.Price * item.Quantity;
                }

                model.Subtotal = subtotal;
                model.Tax = subtotal * 0.14m;
                model.ShippingCost = 10.00m;
                model.Total = subtotal + (subtotal * 0.14m) + 10.00m;

                return View(model);
            }

            try
            {
                var userId = GetUserId();

                // Create order
                var order = _orderService.CreateOrderFromCart(
                    userId,
                    model.ShippingAddress,
                    model.PhoneNumber,
                    model.Notes
                );

                // ✅ REDIRECT to payment page
                return RedirectToAction("ProcessPayment", new { orderId = order.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                // Reload model data on error
                var userId = GetUserId();
                var cartItems = _cartService.GetCartItems(userId).ToList();

                decimal subtotal = 0;
                foreach (var item in cartItems)
                {
                    var product = _productService.GetProductById(item.ProductId);
                    if (product != null)
                        subtotal += product.Price * item.Quantity;
                }

                model.Subtotal = subtotal;
                model.Tax = subtotal * 0.14m;
                model.ShippingCost = 10.00m;
                model.Total = subtotal + (subtotal * 0.14m) + 10.00m;

                return View(model);
            }
        }

        // GET: Order/ProcessPayment
        [HttpGet]
        public IActionResult ProcessPayment(int orderId)
        {
            var userId = GetUserId();
            var order = _orderService.GetOrderById(orderId);

            if (order == null || order.UserId != userId)
            {
                return NotFound();
            }

            ViewBag.OrderId = orderId;
            ViewBag.TotalAmount = order.TotalAmount;
            ViewBag.StripePublishableKey = _configuration.GetSection("Stripe")["PublishableKey"];

            return View();
        }

        // GET: Order/OrderConfirmation/5
        [HttpGet]
        public IActionResult OrderConfirmation(int id)
        {
            var userId = GetUserId();
            var order = _orderService.GetOrderByIdWithDetails(id);

            if (order == null || order.UserId != userId)
            {
                return NotFound();
            }

            var viewModel = new OrderViewModel
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status,
                Subtotal = order.Subtotal,
                Tax = order.Tax,
                ShippingCost = order.ShippingCost,
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress,
                PhoneNumber = order.PhoneNumber
            };

            foreach (var item in order.OrderItems)
            {
                var product = _productService.GetProductById(item.ProductId);
                viewModel.Items.Add(new OrderItemViewModel
                {
                    ProductId = item.ProductId,
                    ProductName = product?.Name ?? "Unknown Product",
                    ProductImage = product?.ImageUrl,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                });
            }

            return View(viewModel);
        }

        // GET: Order/MyOrders
        [HttpGet]
        public IActionResult MyOrders()
        {
            var userId = GetUserId();
            var orders = _orderService.GetOrdersByUserId(userId);

            var viewModels = orders.Select(o => new OrderViewModel
            {
                OrderId = o.Id,
                OrderDate = o.OrderDate,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                ShippingAddress = o.ShippingAddress
            }).ToList();

            return View(viewModels);
        }

        // GET: Order/Details/5
        [HttpGet]
        public IActionResult Details(int id)
        {
            var userId = GetUserId();
            var order = _orderService.GetOrderByIdWithDetails(id);

            if (order == null || order.UserId != userId)
            {
                return NotFound();
            }

            var viewModel = new OrderViewModel
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status,
                Subtotal = order.Subtotal,
                Tax = order.Tax,
                ShippingCost = order.ShippingCost,
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress,
                PhoneNumber = order.PhoneNumber,
                TrackingNumber = order.TrackingNumber,
                ShippedDate = order.ShippedDate,
                DeliveredDate = order.DeliveredDate
            };

            foreach (var item in order.OrderItems)
            {
                var product = _productService.GetProductById(item.ProductId);
                viewModel.Items.Add(new OrderItemViewModel
                {
                    ProductId = item.ProductId,
                    ProductName = product?.Name ?? "Unknown Product",
                    ProductImage = product?.ImageUrl,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                });
            }

            return View(viewModel);
        }

        // POST: Order/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            try
            {
                var userId = GetUserId();
                var order = _orderService.GetOrderById(id);

                if (order == null || order.UserId != userId)
                {
                    return NotFound();
                }

                _orderService.CancelOrder(id);
                TempData["SuccessMessage"] = "Order cancelled successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Details", new { id });
        }
    }
}
