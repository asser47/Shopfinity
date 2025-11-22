using ECommerce.BLL.Interfaces;
using ECommerce.PL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.PL.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;

        public CartController(ICartService cartService, IProductService productService)
        {
            _cartService = cartService;
            _productService = productService;
        }

        private string GetUserId()
        {
            if (User.Identity.IsAuthenticated)
            {
                return User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                userId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("UserId", userId);
            }
            return userId;
        }

        public IActionResult Index()
        {
            var userId = GetUserId();
            var cartItems = _cartService.GetCartItems(userId);
            var viewModel = new CartViewModel();

            foreach (var item in cartItems)
            {
                var product = _productService.GetProductById(item.ProductId);
                if (product != null)
                {
                    viewModel.Items.Add(new CartItemViewModel
                    {
                        CartItemId = item.Id,
                        ProductId = product.Id,
                        ProductName = product.Name,
                        ProductImage = product.ImageUrl ?? "",
                        Price = product.Price,
                        Quantity = item.Quantity,
                        MaxStock = product.Stock
                    });
                }
            }

            viewModel.Subtotal = viewModel.Items.Sum(i => i.Subtotal);
            viewModel.Tax = viewModel.Subtotal * 0.14m;
            viewModel.Total = viewModel.Subtotal + viewModel.Tax;
            viewModel.ItemCount = viewModel.Items.Sum(i => i.Quantity);

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            var userId = GetUserId();

            try
            {
                var product = _productService.GetProductById(productId);
                var cartProduct = _cartService.GetCartItems(userId).FirstOrDefault(c => c.ProductId == productId);

                if (product == null)
                {
                    TempData["ErrorMessage"] = "Product not found.";
                    return RedirectToAction("Index", "Product");
                }

                if (product.Stock < quantity + (cartProduct?.Quantity ?? 0))
                {
                    TempData["ErrorMessage"] = "Not enough stock available.";
                    return RedirectToAction("Details", "Product", new { id = productId });
                }

                _cartService.AddToCart(userId, productId, quantity);
                TempData["SuccessMessage"] = $"{product.Name} added to cart successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to add product to cart.";
            }

            return RedirectToAction("Index", "Product");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int cartItemId, int quantity)
        {
            try
            {
                _cartService.UpdateCartItemQuantity(cartItemId, quantity);
                TempData["SuccessMessage"] = "Quantity updated!";
            }
            catch
            {
                TempData["ErrorMessage"] = "Failed to update quantity.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Remove(int cartItemId)
        {
            try
            {
                _cartService.RemoveFromCart(cartItemId);
                TempData["SuccessMessage"] = "Item removed from cart.";
            }
            catch
            {
                TempData["ErrorMessage"] = "Failed to remove item.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Clear()
        {
            var userId = GetUserId();
            try
            {
                _cartService.ClearCart(userId);
                TempData["SuccessMessage"] = "Cart cleared successfully.";
            }
            catch
            {
                TempData["ErrorMessage"] = "Failed to clear cart.";
            }
            return RedirectToAction("Index");
        }
    }
}