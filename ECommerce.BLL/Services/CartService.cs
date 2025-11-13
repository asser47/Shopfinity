using ECommerce.BLL.Interfaces;
using ECommerce.DAL.Models;
using ECommerce.DAL.Repositories.Interfaces;

namespace ECommerce.BLL.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CartService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Cart GetCartByUserId(string userId)
        {
            return _unitOfWork.Carts.Find(c => c.UserId == userId).FirstOrDefault();
        }

        public Cart GetOrCreateCart(string userId)
        {
            var cart = GetCartByUserId(userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId, CreatedDate = DateTime.Now };
                _unitOfWork.Carts.Add(cart);
                _unitOfWork.Complete();
            }
            return cart;
        }

        public void AddToCart(string userId, int productId, int quantity = 1)
        {
            var cart = GetOrCreateCart(userId);
            var existingItem = _unitOfWork.CartItems
                .Find(ci => ci.CartId == cart.Id && ci.ProductId == productId)
                .FirstOrDefault();

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                _unitOfWork.CartItems.Update(existingItem);
            }
            else
            {
                _unitOfWork.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity
                });
            }
            _unitOfWork.Complete();
        }

        public void UpdateCartItemQuantity(int cartItemId, int quantity)
        {
            var cartItem = _unitOfWork.CartItems.GetById(cartItemId);
            if (cartItem != null)
            {
                if (quantity <= 0)
                    _unitOfWork.CartItems.Delete(cartItem);
                else
                {
                    cartItem.Quantity = quantity;
                    _unitOfWork.CartItems.Update(cartItem);
                }
                _unitOfWork.Complete();
            }
        }

        public void RemoveFromCart(int cartItemId)
        {
            _unitOfWork.CartItems.Delete(cartItemId);
            _unitOfWork.Complete();
        }

        public void ClearCart(string userId)
        {
            var cart = GetCartByUserId(userId);
            if (cart != null)
            {
                var cartItems = _unitOfWork.CartItems.Find(ci => ci.CartId == cart.Id);
                foreach (var item in cartItems)
                    _unitOfWork.CartItems.Delete(item);
                _unitOfWork.Complete();
            }
        }

        public decimal GetCartTotal(string userId)
        {
            var cart = GetCartByUserId(userId);
            if (cart == null) return 0;

            var cartItems = _unitOfWork.CartItems.Find(ci => ci.CartId == cart.Id);
            decimal total = 0;
            foreach (var item in cartItems)
            {
                var product = _unitOfWork.Products.GetById(item.ProductId);
                if (product != null)
                    total += product.Price * item.Quantity;
            }
            return total;
        }

        public int GetCartItemCount(string userId)
        {
            var cart = GetCartByUserId(userId);
            if (cart == null) return 0;
            return _unitOfWork.CartItems.Find(ci => ci.CartId == cart.Id).Sum(ci => ci.Quantity);
        }

        public IEnumerable<CartItem> GetCartItems(string userId)
        {
            var cart = GetCartByUserId(userId);
            if (cart == null) return new List<CartItem>();
            return _unitOfWork.CartItems.Find(ci => ci.CartId == cart.Id);
        }

        // SIMPLE cart migration method
        public async Task MergeCartsAsync(string fromUserId, string toUserId)
        {
            var sourceCart = GetCartByUserId(fromUserId);
            if (sourceCart == null) return;

            var targetCart = GetCartByUserId(toUserId);

            if (targetCart == null)
            {
                // No existing cart - just change ownership
                sourceCart.UserId = toUserId;
                _unitOfWork.Carts.Update(sourceCart);
            }
            else
            {
                // Merge items
                var sourceItems = _unitOfWork.CartItems.Find(ci => ci.CartId == sourceCart.Id).ToList();
                var targetItems = _unitOfWork.CartItems.Find(ci => ci.CartId == targetCart.Id).ToList();

                foreach (var sourceItem in sourceItems)
                {
                    var existing = targetItems.FirstOrDefault(ti => ti.ProductId == sourceItem.ProductId);
                    if (existing != null)
                    {
                        existing.Quantity += sourceItem.Quantity;
                        _unitOfWork.CartItems.Update(existing);
                    }
                    else
                    {
                        sourceItem.CartId = targetCart.Id;
                        _unitOfWork.CartItems.Update(sourceItem);
                    }
                }
                _unitOfWork.Carts.Delete(sourceCart);
            }
            _unitOfWork.Complete();
            await Task.CompletedTask;
        }
    }
}
