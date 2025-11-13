using ECommerce.BLL.Interfaces;
using ECommerce.DAL.Repositories.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace ECommerce.BLL.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Session CreateCheckoutSession(int orderId, string successUrl, string cancelUrl)
        {
            var order = _unitOfWork.Orders.GetById(orderId);
            if (order == null)
                throw new InvalidOperationException("Order not found");

            var orderItems = _unitOfWork.OrderItems
                .Find(oi => oi.OrderId == orderId)
                .ToList();

            // Create line items for Stripe
            var lineItems = new List<SessionLineItemOptions>();
            foreach (var item in orderItems)
            {
                var product = _unitOfWork.Products.GetById(item.ProductId);
                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = product?.Name ?? "Product",
                            Description = product?.Description
                        },
                        UnitAmount = (long)(item.Price * 100) // Convert to cents
                    },
                    Quantity = item.Quantity
                });
            }

            // Add shipping as a line item
            lineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Shipping"
                    },
                    UnitAmount = (long)(order.ShippingCost * 100)
                },
                Quantity = 1
            });

            // Add tax as a line item
            lineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Tax"
                    },
                    UnitAmount = (long)(order.Tax * 100)
                },
                Quantity = 1
            });

            // Create Stripe checkout session
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                CustomerEmail = order.User?.Email,
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", orderId.ToString() }
                }
            };

            var service = new SessionService();
            return service.Create(options);
        }

        public Session GetSession(string sessionId)
        {
            var service = new SessionService();
            return service.Get(sessionId);
        }

        public bool ValidatePayment(string sessionId)
        {
            var session = GetSession(sessionId);
            return session.PaymentStatus == "paid";
        }
    }
}
