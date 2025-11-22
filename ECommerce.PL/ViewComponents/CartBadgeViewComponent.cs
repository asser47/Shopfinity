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
            string userId;

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            else
            {
                userId = HttpContext.Session.GetString("UserId") ?? "";
            }

            int count = 0;
            if (!string.IsNullOrEmpty(userId))
            {
                count = _cartService.GetCartItemCount(userId);
            }

            return Content(count.ToString());
        }
    }
}
