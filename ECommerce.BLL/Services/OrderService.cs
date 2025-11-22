using ECommerce.BLL.Interfaces;
using ECommerce.DAL.Enums;
using ECommerce.DAL.Models;
using ECommerce.DAL.Repositories.Interfaces;

namespace ECommerce.BLL.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICartService _cartService;

        public OrderService(IUnitOfWork unitOfWork, ICartService cartService)
        {
            _unitOfWork = unitOfWork;
            _cartService = cartService;
        }

        public Order CreateOrderFromCart(string userId, string shippingAddress, string phoneNumber, string? notes = null)
        {
            var cartItems = _cartService.GetCartItems(userId).ToList();

            if (!cartItems.Any())
                throw new InvalidOperationException("Cart is empty");

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                ShippingAddress = shippingAddress,
                PhoneNumber = phoneNumber,
                Notes = notes,
                ShippingCost = 10.00m
            };

            decimal subtotal = 0;

            foreach (var cartItem in cartItems)
            {
                var product = _unitOfWork.Products.GetById(cartItem.ProductId);

                if (product == null)
                    throw new InvalidOperationException($"Product {cartItem.ProductId} not found");

                if (product.Stock < cartItem.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for {product.Name}");

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = cartItem.Quantity,
                    Price = product.Price
                };

                order.OrderItems.Add(orderItem);
                subtotal += orderItem.Price * orderItem.Quantity;

                product.Stock -= cartItem.Quantity;
                _unitOfWork.Products.Update(product);
            }

            order.Subtotal = subtotal;
            order.Tax = subtotal * 0.14m;
            order.TotalAmount = order.Subtotal + order.Tax + order.ShippingCost;

            _unitOfWork.Orders.Add(order);
            _unitOfWork.Complete();

            _cartService.ClearCart(userId);

            return order;
        }

        public IEnumerable<Order> GetAllOrders()
        {
            return _unitOfWork.Orders.GetAll().OrderByDescending(o => o.OrderDate);
        }

        public IEnumerable<Order> GetOrdersByUserId(string userId)
        {
            return _unitOfWork.Orders
                .Find(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate);
        }

        public Order GetOrderById(int orderId)
        {
            return _unitOfWork.Orders.GetById(orderId);
        }

        public Order GetOrderByIdWithDetails(int orderId)
        {
            var order = _unitOfWork.Orders.GetById(orderId);
            if (order == null) return null;

            order.OrderItems = _unitOfWork.OrderItems
                .Find(oi => oi.OrderId == orderId)
                .ToList();

            return order;
        }

        public void UpdateOrderStatus(int orderId, OrderStatus newStatus)
        {
            var order = _unitOfWork.Orders.GetById(orderId);
            if (order == null)
                throw new InvalidOperationException("Order not found");

            var oldStatus = order.Status;

            if (newStatus == OrderStatus.Cancelled && oldStatus != OrderStatus.Cancelled)
            {
                RestoreStock(orderId);
            }

            order.Status = newStatus;

            if (newStatus == OrderStatus.Shipped && !order.ShippedDate.HasValue)
                order.ShippedDate = DateTime.Now;
            else if (newStatus == OrderStatus.Delivered && !order.DeliveredDate.HasValue)
                order.DeliveredDate = DateTime.Now;

            _unitOfWork.Orders.Update(order);
            _unitOfWork.Complete();
        }

        public void UpdateOrder(Order order)
        {
            _unitOfWork.Orders.Update(order);
            _unitOfWork.Complete();
        }

        public void CancelOrder(int orderId)
        {
            var order = _unitOfWork.Orders.GetById(orderId);
            if (order == null)
                throw new InvalidOperationException("Order not found");

            if (order.Status == OrderStatus.Delivered)
                throw new InvalidOperationException("Cannot cancel delivered orders");

            if (order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Order is already cancelled");

            RestoreStock(orderId);

            order.Status = OrderStatus.Cancelled;
            _unitOfWork.Orders.Update(order);
            _unitOfWork.Complete();
        }

        private void RestoreStock(int orderId)
        {
            var orderItems = _unitOfWork.OrderItems
                .Find(oi => oi.OrderId == orderId)
                .ToList();

            foreach (var item in orderItems)
            {
                var product = _unitOfWork.Products.GetById(item.ProductId);
                if (product != null)
                {
                    product.Stock += item.Quantity;
                    product.LastModifiedDate = DateTime.Now;
                    _unitOfWork.Products.Update(product);
                }
            }
        }

        public IEnumerable<Order> GetOrdersByStatus(OrderStatus status)
        {
            return _unitOfWork.Orders
                .Find(o => o.Status == status)
                .OrderByDescending(o => o.OrderDate);
        }

        public decimal GetOrderTotal(int orderId)
        {
            var order = _unitOfWork.Orders.GetById(orderId);
            return order?.TotalAmount ?? 0;
        }

        public int GetOrderItemCount(int orderId)
        {
            return _unitOfWork.OrderItems
                .Find(oi => oi.OrderId == orderId)
                .Sum(oi => oi.Quantity);
        }

        public void DeleteOrder(int orderId)
        {
            RestoreStock(orderId);

            _unitOfWork.Orders.Delete(orderId);
            _unitOfWork.Complete();
        }

        public IEnumerable<Product> GetLowStockProducts(int threshold = 10)
        {
            return _unitOfWork.Products
                .Find(p => p.Stock <= threshold && p.Stock > 0)
                .OrderBy(p => p.Stock);
        }

        public IEnumerable<Product> GetOutOfStockProducts()
        {
            return _unitOfWork.Products
                .Find(p => p.Stock == 0)
                .OrderBy(p => p.Name);
        }
    }
}
