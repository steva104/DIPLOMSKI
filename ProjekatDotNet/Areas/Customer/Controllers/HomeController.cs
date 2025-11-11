using Microsoft.AspNetCore.Mvc;
using VinylVibe.Models;
using System.Diagnostics;
using VinylVibe.Models;
using VinylVibe.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using VinylVibe.BusinessLogic.Interfaces;

namespace VinylVibeWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
      
		private readonly IHomeService _homeService;
		public HomeController(ILogger<HomeController> logger, IHomeService homeService)
        {
            _logger = logger;
            
            _homeService = homeService;
        }

        public IActionResult Index(string searchString, string selectedGenre, int pageNumber=1)
        {
            int pageSize = 6;

           
            var productsQuery = _homeService.GetFilteredProducts(searchString, selectedGenre);

            
            var totalCount = productsQuery.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            
            var pagedProducts = productsQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Genres = _homeService.GetGenres();
            ViewBag.SelectedGenre = selectedGenre;
            ViewData["CurrentFilter"] = searchString;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageNumber;

            return View(pagedProducts);


        }
		public IActionResult Details(int id)
		{
			var cart = _homeService.GetProductDetails(id);

			if (cart == null)
			{
				return NotFound(); 
			}

			return View(cart);
		}
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart cart)
        {

			var identity = (ClaimsIdentity)User.Identity;
			var userId = identity.FindFirst(ClaimTypes.NameIdentifier).Value;

			
			var success = _homeService.AddOrUpdateCart(cart, userId);

			if (!success)
			{
				TempData["error"] = "Count must be greater than zero.";
				return RedirectToAction("Details", new { id = cart.ProductId });
			}

			TempData["success"] = "Cart updated successfully";

			return RedirectToAction("Index");


			
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
