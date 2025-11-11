using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VinylVibe.Models;

namespace VinylVibe.BusinessLogic.Interfaces
{
	public interface IHomeService
	{
		IQueryable<Product> GetFilteredProducts(string searchString, string selectedGenre);
		IEnumerable<string> GetGenres();
		ShoppingCart GetProductDetails(int productId);

		bool AddOrUpdateCart(ShoppingCart cart, string userId);

	}
}
