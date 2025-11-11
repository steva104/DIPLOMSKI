using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VinylVibe.BusinessLogic.Interfaces;
using VinylVibe.DataAccess.Repository.IRepository;
using VinylVibe.Models;
using VinylVibe.Models.ViewModel;
using VinylVibe.Utility;

namespace VinylVibeWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {

        private readonly ICartService _cartService;
        [BindProperty]
        public ShoppingCartViewModel ShoppingCartViewModel { get; set; }
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }


        public IActionResult Index()//DOBRO
        {
            var identity = (ClaimsIdentity)User.Identity;
            var userId = identity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartViewModel = _cartService.GetShoppingCartData(userId);

            return View(ShoppingCartViewModel);
        }

        public IActionResult Checkout()//DOBRO
        {
            var identity = (ClaimsIdentity)User.Identity;
            var userId = identity.FindFirst(ClaimTypes.NameIdentifier).Value;

           ShoppingCartViewModel=_cartService.GetCheckoutData(userId);


            return View(ShoppingCartViewModel);
        }

        [HttpPost]
        [ActionName("Checkout")]
        public IActionResult CheckoutPOST()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var userId = identity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartViewModel.OrderHeader.Id = _cartService.ProcessCheckout(userId, ShoppingCartViewModel);

            HttpContext.Session.Clear();
            return RedirectToAction("OrderConfirmation", new {id=ShoppingCartViewModel.OrderHeader.Id});
        }

        public IActionResult OrderConfirmation(int id)
        {
          
            

            return View(id);
        }



        public IActionResult Plus(int cartId)
        {
            _cartService.AddItemToCart(cartId);
            return RedirectToAction("Index");
        }
        public IActionResult Minus(int cartId)
        {
           _cartService.DecreaseCartItem(cartId);
            return RedirectToAction("Index");
        }
        public IActionResult Remove(int cartId)
        {
            _cartService.RemoveCartItem(cartId);
            
            return RedirectToAction("Index");
        }

      
    }
}
