using Stripe.Checkout;

namespace ECommerce.BLL.Interfaces
{
    public interface IPaymentService
    {
        Session CreateCheckoutSession(int orderId, string successUrl, string cancelUrl);
        Session GetSession(string sessionId);
        bool ValidatePayment(string sessionId);
    }
}
