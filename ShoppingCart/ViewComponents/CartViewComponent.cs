using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Extensions;
using ShoppingCart.ViewModels;

namespace ShoppingCart.ViewComponents
{
	public class CartViewComponent : ViewComponent
	{
		public IViewComponentResult Invoke() 
		{ 
			var cart = HttpContext.Session.Get<List<CartItemViewModel>>(SettingExtensions.CARD_KEY) 
											?? new List<CartItemViewModel>();

			return View("CartPanel", new CartModel //Tao 1 view CartPanel.cshtml o thu muc Component
			{
				Quantity = cart.Sum(p => p.SoLuong),
				Total = cart.Sum(p => p.ThanhTien)
			});
		}

	}
}
