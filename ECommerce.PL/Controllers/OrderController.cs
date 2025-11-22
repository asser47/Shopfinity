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
        private readonly IEmailSender _emailSender;


        public OrderController(
            IOrderService orderService,
            ICartService cartService,
            IProductService productService,
            IConfiguration configuration,
            IEmailSender emailSender)
        {
            _orderService = orderService;
            _cartService = cartService;
            _productService = productService;
            _configuration = configuration;
            _emailSender = emailSender;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
        private string GetUserEmail()  
        {
            return User.FindFirstValue(ClaimTypes.Email);
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
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

                var order = _orderService.CreateOrderFromCart(
                    userId,
                    model.ShippingAddress,
                    model.PhoneNumber,
                    model.Notes
                );

                return RedirectToAction("ProcessPayment", new { orderId = order.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;

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

        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(int id)
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

            try
            {
                var userEmail = GetUserEmail();
                if (!string.IsNullOrEmpty(userEmail))
                {
                    string subject = $"Order Confirmation #{order.Id} - Shopfinity";
                    string body = BuildOrderConfirmationEmail(viewModel);
                    await _emailSender.SendEmailAsync(userEmail, subject, body);

                    Console.WriteLine($"Email successfully sent to {userEmail}");
                }
                else
                {
                    Console.WriteLine("User email is null or empty, cannot send confirmation email");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                TempData["WarningMessage"] = "Order confirmed, but we couldn't send a confirmation email.";
            }
            return View(viewModel);
        }


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
        private string BuildOrderConfirmationEmail(OrderViewModel order)
        {
            var itemsHtml = string.Join("", order.Items.Select(item => $@"
        <tr>
            <td style='padding: 10px; border-bottom: 1px solid #ddd;'>{item.ProductName}</td>
            <td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: center;'>{item.Quantity}</td>
            <td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: right;'>${item.UnitPrice:F2}</td>
            <td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: right;'>${(item.Quantity * item.UnitPrice):F2}</td>
        </tr>
    "));

            return $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
            <h2 style='color: #333; text-align: center;'>Order Confirmation</h2>
            <p>Thank you for your order! Your order #{order.OrderId} has been confirmed.</p>
            <p><strong>Order Date:</strong> {order.OrderDate:MMMM dd, yyyy}</p>
            <p><strong>Shipping Address:</strong> {order.ShippingAddress}</p>
            <p><strong>Phone Number:</strong> {order.PhoneNumber}</p>
            
            <h3 style='margin-top: 20px;'>Order Details</h3>
            <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
                <thead>
                    <tr style='background-color: #f8f8f8;'>
                        <th style='padding: 10px; text-align: left;'>Product</th>
                        <th style='padding: 10px; text-align: center;'>Quantity</th>
                        <th style='padding: 10px; text-align: right;'>Price</th>
                        <th style='padding: 10px; text-align: right;'>Total</th>
                    </tr>
                </thead>
                <tbody>
                    {itemsHtml}
                </tbody>
                <tfoot>
                    <tr>
                        <td colspan='3' style='padding: 10px; text-align: right;'><strong>Subtotal:</strong></td>
                        <td style='padding: 10px; text-align: right;'>${order.Subtotal:F2}</td>
                    </tr>
                    <tr>
                        <td colspan='3' style='padding: 10px; text-align: right;'><strong>Tax:</strong></td>
                        <td style='padding: 10px; text-align: right;'>${order.Tax:F2}</td>
                    </tr>
                    <tr>
                        <td colspan='3' style='padding: 10px; text-align: right;'><strong>Shipping:</strong></td>
                        <td style='padding: 10px; text-align: right;'>${order.ShippingCost:F2}</td>
                    </tr>
                    <tr style='background-color: #f8f8f8; font-weight: bold;'>
                        <td colspan='3' style='padding: 10px; text-align: right;'>Total:</td>
                        <td style='padding: 10px; text-align: right;'>${order.TotalAmount:F2}</td>
                    </tr>
                </tfoot>
            </table>
            
            <p style='text-align: center; margin-top: 30px; color: #666;'>
                Thank you for shopping with Shopfinity!
            </p>
        </div>
    ";
        }
    }
}
