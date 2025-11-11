using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VinylVibe.BusinessLogic;
using VinylVibe.BusinessLogic.Interfaces;
using VinylVibe.DataAccess.Data;
using VinylVibe.DataAccess.Repository.IRepository;
using VinylVibe.Models;
using VinylVibe.Models.ViewModel;


namespace VinylVibeWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = VinylVibe.Utility.Details.Role_Admin)]
    public class ProductController : Controller
    {

		private readonly IProductService _productService;
		private readonly IWebHostEnvironment _webHostEnvironment;

		public ProductController(IProductService productService, IWebHostEnvironment webHostEnvironment)
		{
			_productService = productService;
			_webHostEnvironment = webHostEnvironment;
		}

		public IActionResult Index()
		{
			//var products = _productService.GetAllProducts();  NE TREBA VRACATI SVE
			return View();
		}

		public IActionResult Upsert(int? id)
		{
			var productViewModel = _productService.GetProductViewModel(id);
			return View(productViewModel);
		}

		[HttpPost]
		public IActionResult Upsert(ProductViewModel productVM, IFormFile? file)
		{
			bool success = _productService.UpsertProduct(productVM, file, _webHostEnvironment.WebRootPath);

			if (!success)
			{
				ModelState.AddModelError("", "Image is required for new products.");
				return View(productVM);
			}

			TempData["success"] = "Product saved successfully!";
			return RedirectToAction("Index");
		}

		#region API Calls

		[HttpGet]
		public IActionResult GetAll()
		{
            int draw = int.Parse(Request.Query["draw"]);
            int start = int.Parse(Request.Query["start"]);
            int length = int.Parse(Request.Query["length"]);
            string searchValue = Request.Query["search[value]"].ToString().ToLower();
            // Get sort parameters
            string sortColumn = Request.Query["columns[" + Request.Query["order[0][column]"] + "][name]"].ToString();
            string sortDirection = Request.Query["order[0][dir]"].ToString();


			var query = _productService.GetAllProducts();

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(o =>
                    o.Title.ToLower().Contains(searchValue) ||
                    o.Artist.ToLower().Contains(searchValue) ||
                    o.Year.ToString().ToLower().Contains(searchValue) ||
                    o.Genre.Name.ToLower().Contains(searchValue) ||
                    o.ListPrice.ToString().ToLower().Contains(searchValue)||
                    o.UPC.ToString().ToLower().Contains(searchValue));
            }

            query = _productService.ApplySorting(query, sortColumn, sortDirection);

            int recordsTotal = query.Count();
            var data = query
                            .Skip(start)
                            .Take(length)
                            .Select(o => new {
                                o.Id,
                                o.Title,
                                o.Artist,
                                o.Year,
                                o.UPC,
                                o.ListPrice,
                                o.Genre.Name
                            })
                            .ToList();
            return Json(new
            {
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = data
            });


        }

        [HttpDelete]
		public IActionResult Delete(int id)
		{
			bool success = _productService.DeleteProduct(id, _webHostEnvironment.WebRootPath);

			if (!success)
				return Json(new { success = false, message = "There was an error while deleting the product." });

			return Json(new { success = true, message = "Successfully deleted the product." });
		}

		#endregion
	}
}
