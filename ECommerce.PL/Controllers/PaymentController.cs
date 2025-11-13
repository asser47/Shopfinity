using ECommerce.BLL.Interfaces;
using ECommerce.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.PL.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;

        public PaymentController(IPaymentService paymentService, IOrderService orderService)
        {
            _paymentService = paymentService;
            _orderService = orderService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCheckoutSession(int orderId)
        {
            try
            {
                var order = _orderService.GetOrderById(orderId);

                if (order == null || order.UserId != GetUserId())
                    return BadRequest(new { error = "Invalid order" });

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var session = _paymentService.CreateCheckoutSession(
                    orderId,
                    $"{baseUrl}/Payment/PaymentSuccess?sessionId={{CHECKOUT_SESSION_ID}}",
                    $"{baseUrl}/Payment/PaymentCancel?orderId={orderId}"
                );

                return Json(new { sessionId = session.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult PaymentSuccess(string sessionId)
        {
            try
            {
                if (!_paymentService.ValidatePayment(sessionId))
                {
                    TempData["ErrorMessage"] = "Payment validation failed.";
                    return RedirectToAction("MyOrders", "Order");
                }

                var session = _paymentService.GetSession(sessionId);
                var order = _orderService.GetOrderById(int.Parse(session.Metadata["order_id"]));

                order.PaymentMethod = "Credit Card (Stripe)";
                order.TransactionId = session.PaymentIntentId;
                order.PaymentDate = DateTime.Now;
                order.Status = OrderStatus.Processing;

                _orderService.UpdateOrder(order);

                TempData["SuccessMessage"] = "Payment successful!";
                return RedirectToAction("OrderConfirmation", "Order", new { id = order.Id });
            }
            catch
            {
                TempData["ErrorMessage"] = "Error processing payment.";
                return RedirectToAction("MyOrders", "Order");
            }
        }

        [HttpGet]
        public IActionResult PaymentCancel(int orderId)
        {
            TempData["ErrorMessage"] = "Payment cancelled.";
            return RedirectToAction("Details", "Order", new { id = orderId });
        }
    }
}
