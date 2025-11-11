using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VinylVibe.Models.ViewModel;
using VinylVibe.Models;

namespace VinylVibe.BusinessLogic.Interfaces
{
	public interface ICartService
	{
		ShoppingCartViewModel GetShoppingCartData(string userId);

		ShoppingCartViewModel GetCheckoutData(string userId);
		double GetPriceBasedOnQuantity(ShoppingCart cart);

		void AddItemToCart(int cartId);
		void DecreaseCartItem(int cartId);
		void RemoveCartItem(int cartId);

		int ProcessCheckout(string userId, ShoppingCartViewModel cartViewModel);

	}
}
