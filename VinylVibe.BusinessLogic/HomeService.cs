using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VinylVibe.BusinessLogic.Interfaces;
using VinylVibe.DataAccess.Repository.IRepository;
using VinylVibe.Models;

namespace VinylVibe.BusinessLogic
{
	public class HomeService :IHomeService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public HomeService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}
		public IQueryable<Product> GetFilteredProducts(string searchString, string selectedGenre)
		{
			var products = _unitOfWork.Product.GetAll(includeProperties: "Genre");

			
			if (!string.IsNullOrEmpty(searchString))
			{
				products = products.Where(p => p.Title.ToLower().Contains(searchString.ToLower()) ||
											   p.Artist.Contains(searchString.ToLower()));
			}

			if (!string.IsNullOrEmpty(selectedGenre))
			{
				products = products.Where(p => p.Genre.Name == selectedGenre);
			}

			return products;
		}

		public IEnumerable<string> GetGenres()
		{
			
			var products = _unitOfWork.Product.GetAll(includeProperties: "Genre");
			return products.Select(p => p.Genre.Name).Distinct().ToList();
		}

		public ShoppingCart GetProductDetails(int productId)
		{
			var product = _unitOfWork.Product.Get(x => x.Id == productId, includeProperties: "Genre");

			if (product == null)
			{
				return null; 
			}

			var cart = new ShoppingCart
			{
				Product = product,
				Count = 1,
				ProductId = productId
			};

			return cart;
		}

		public bool AddOrUpdateCart(ShoppingCart cart, string userId)
		{
			
			if (cart.Count <= 0)
			{
				return false; 
			}
			cart.UserId = userId;
			cart.Id = 0;
			
			var cartFromDb = _unitOfWork.ShoppingCart.Get(x => x.UserId == userId && x.ProductId == cart.ProductId);
			
			if (cartFromDb != null)
			{
				
				cartFromDb.Count += cart.Count;
				_unitOfWork.ShoppingCart.Update(cartFromDb);
			}
			else
			{
				
				_unitOfWork.ShoppingCart.Add(cart);
			}

			
			_unitOfWork.Save();
			var httpContext = _httpContextAccessor.HttpContext;
			
			if (httpContext != null)
			{
				httpContext.Session.SetInt32("SessionCart", _unitOfWork.ShoppingCart.GetAll(x => x.UserId == userId).Count());
			}

			return true; 
		}


	}
}
