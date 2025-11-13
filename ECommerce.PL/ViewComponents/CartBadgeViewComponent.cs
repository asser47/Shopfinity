using ECommerce.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.PL.ViewComponents
{
    public class CartBadgeViewComponent : ViewComponent
    {
        private readonly ICartService _cartService;

        public CartBadgeViewComponent(ICartService cartService)
        {
            _cartService = cartService;
        }

        public IViewComponentResult Invoke()
        {
            // IMPORTANT: Use the same logic as CartController.GetUserId()
            string userId;

            // Check authenticated user FIRST (same as CartController)
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            else
            {
                // Then check session (same as CartController)
                userId = HttpContext.Session.GetString("UserId") ?? "";
            }

            // Get count (handle empty userId)
            int count = 0;
            if (!string.IsNullOrEmpty(userId))
            {
                count = _cartService.GetCartItemCount(userId);
            }

            return Content(count.ToString());
        }
    }
}
