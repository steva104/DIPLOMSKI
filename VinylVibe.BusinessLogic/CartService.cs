using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VinylVibe.DataAccess.Repository.IRepository;
using VinylVibe.Models.ViewModel;
using VinylVibe.Models;
using VinylVibe.Utility;
using VinylVibe.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Http;

namespace VinylVibe.BusinessLogic
{
	public class CartService : ICartService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public ShoppingCartViewModel viewModel{ get; set; }


		public CartService(IUnitOfWork unitOfWork,IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;

		}

		public ShoppingCartViewModel GetShoppingCartData(string userId)
		{
			var cartItems = _unitOfWork.ShoppingCart.GetAll(x => x.UserId == userId, includeProperties: "Product").ToList();

			viewModel = new ShoppingCartViewModel
			{
				Items = cartItems,
				OrderHeader = new OrderHeader()
			};

			foreach (var cart in viewModel.Items)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				viewModel.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			return viewModel;
		}
		public ShoppingCartViewModel GetCheckoutData(string userId)
		{
			
			var cartItems = _unitOfWork.ShoppingCart.GetAll(x => x.UserId == userId, includeProperties: "Product").ToList();
			viewModel = new ShoppingCartViewModel
			{
				Items = cartItems,
				OrderHeader = new OrderHeader()
			};
			viewModel.OrderHeader.User= _unitOfWork.User.Get(x => x.Id == userId);


			viewModel.OrderHeader.Name=viewModel.OrderHeader.User.Name;
			viewModel.OrderHeader.StreetAddress = viewModel.OrderHeader.User.StreetAddress;
			viewModel.OrderHeader.PhoneNumber = viewModel.OrderHeader.User.PhoneNumber;
			viewModel.OrderHeader.City = viewModel.OrderHeader.User.City;
			viewModel.OrderHeader.Country = viewModel.OrderHeader.User.Country;
			viewModel.OrderHeader.PostalCode = viewModel.OrderHeader.User.PostalCode;

			foreach (var cart in viewModel.Items)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				viewModel.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			return viewModel;
		}

		public double GetPriceBasedOnQuantity(ShoppingCart cart)
		{
			if (cart.Count <= 5)
			{
				return cart.Product.Price;
			}
			else if (cart.Count <= 10)
			{
				return cart.Product.Price5;
			}
			else
			{
				return cart.Product.Price10;
			}
			
		}

		public void AddItemToCart(int cartId)
		{
			var cartFromDb = _unitOfWork.ShoppingCart.Get(x => x.Id == cartId);
			cartFromDb.Count += 1;
			_unitOfWork.ShoppingCart.Update(cartFromDb);
			_unitOfWork.Save();
		}
		public void DecreaseCartItem(int cartId)
		{
			var cartFromDb = _unitOfWork.ShoppingCart.Get(x => x.Id == cartId, tracked: true);
			if (cartFromDb == null) return;

			if (cartFromDb.Count <= 1)
			{
				var httpContext = _httpContextAccessor.HttpContext;
				if (httpContext != null)
				{
					httpContext.Session.SetInt32("SessionCart", _unitOfWork.ShoppingCart.GetAll(x => x.UserId == cartFromDb.UserId).Count() - 1);
				}

				_unitOfWork.ShoppingCart.Delete(cartFromDb);
			}
			else
			{
				cartFromDb.Count -= 1;
				_unitOfWork.ShoppingCart.Update(cartFromDb);
			}

			_unitOfWork.Save();
		}
		public void RemoveCartItem(int cartId)
		{
			var cartFromDb = _unitOfWork.ShoppingCart.Get(x => x.Id == cartId, tracked: true);
			if (cartFromDb == null) return;

			var httpContext = _httpContextAccessor.HttpContext;
			if (httpContext != null)
			{
				int newCartCount = _unitOfWork.ShoppingCart.GetAll(x => x.UserId == cartFromDb.UserId).ToList().Count() - 1;
				httpContext.Session.SetInt32("SessionCart", newCartCount);
			}

			_unitOfWork.ShoppingCart.Delete(cartFromDb);
			_unitOfWork.Save();
		}
		public int ProcessCheckout(string userId, ShoppingCartViewModel cartViewModel)
		{
			viewModel= cartViewModel;

			viewModel.Items= _unitOfWork.ShoppingCart.GetAll(x => x.UserId == userId, includeProperties: "Product").ToList();
			viewModel.OrderHeader.OrderDate=DateTime.Now;
			viewModel.OrderHeader.UserId=userId;

			User user = _unitOfWork.User.Get(x => x.Id == userId);
			foreach (var cart in viewModel.Items)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				viewModel.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}
			if (user.CompanyId.GetValueOrDefault() == 0)
			{
				viewModel.OrderHeader.PaymentStatus = Details.PaymentStatusApproved;
				viewModel.OrderHeader.OrderStatus = Details.StatusApproved;
				viewModel.OrderHeader.PaymentDate = DateTime.Now;
			}
			else
			{
				viewModel.OrderHeader.PaymentStatus = Details.PaymentStatusDelayedPayment;
				viewModel.OrderHeader.OrderStatus = Details.StatusApproved;
			}

			_unitOfWork.OrderHeader.Add(viewModel.OrderHeader);
			_unitOfWork.Save();

			foreach (var cart in viewModel.Items)
			{
				OrderDetails orderDetails = new()
				{
					ProductId = cart.ProductId,
					OrderHeaderId = viewModel.OrderHeader.Id,
					Price = cart.Price,
					Count = cart.Count
				};
				_unitOfWork.OrderDetails.Add(orderDetails);
				_unitOfWork.Save();

			}
			var cartItems = _unitOfWork.ShoppingCart.GetAll(x => x.UserId == userId).ToList();
			_unitOfWork.ShoppingCart.RemoveRange(cartItems);
			_unitOfWork.Save();

			return viewModel.OrderHeader.Id;

		}

	}
}
