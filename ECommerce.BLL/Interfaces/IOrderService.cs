using ECommerce.DAL.Enums;
using ECommerce.DAL.Models;

namespace ECommerce.BLL.Interfaces
{
    public interface IOrderService
    {
        Order CreateOrderFromCart(string userId, string shippingAddress, string phoneNumber, string? notes = null);
        IEnumerable<Order> GetAllOrders();
        IEnumerable<Order> GetOrdersByUserId(string userId);
        Order GetOrderById(int orderId);
        Order GetOrderByIdWithDetails(int orderId);
        void UpdateOrderStatus(int orderId, OrderStatus status);
        void UpdateOrder(Order order);
        void CancelOrder(int orderId);
        IEnumerable<Order> GetOrdersByStatus(OrderStatus status);
        decimal GetOrderTotal(int orderId);
        int GetOrderItemCount(int orderId);
        void DeleteOrder(int orderId);

        // ✅ NEW: Inventory management methods
        IEnumerable<Product> GetLowStockProducts(int threshold = 10);
        IEnumerable<Product> GetOutOfStockProducts();
    }
}
