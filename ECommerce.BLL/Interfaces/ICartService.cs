using ECommerce.DAL.Models;

namespace ECommerce.BLL.Interfaces
{
    public interface ICartService
    {
        // Get cart for a user
        Cart GetCartByUserId(string userId);

        // Get or create cart
        Cart GetOrCreateCart(string userId);

        // Add product to cart
        void AddToCart(string userId, int productId, int quantity = 1);

        // Update quantity
        void UpdateCartItemQuantity(int cartItemId, int quantity);

        // Remove item from cart
        void RemoveFromCart(int cartItemId);

        // Clear entire cart
        void ClearCart(string userId);

        // Get cart total
        decimal GetCartTotal(string userId);

        // Get cart item count
        int GetCartItemCount(string userId);
        IEnumerable<CartItem> GetCartItems(string userId);
        Task MergeCartsAsync(string fromUserId, string toUserId);
    }
}