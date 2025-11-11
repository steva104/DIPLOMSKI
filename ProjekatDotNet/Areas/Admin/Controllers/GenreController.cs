using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using VinylVibe.BusinessLogic.Interfaces;
using VinylVibe.DataAccess.Data;
using VinylVibe.DataAccess.Repository.IRepository;
using VinylVibe.Models;


namespace VinylVibeWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = VinylVibe.Utility.Details.Role_Admin)]
    public class GenreController : Controller
    {

		private readonly IGenreService _genreService;

		public GenreController(IGenreService genreService)
		{
			_genreService = genreService;
		}

		public IActionResult Index()
		{
			return View(_genreService.GetAllGenres().ToList());
		}

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Create(Genre genre)
		{
			if (_genreService.CreateGenre(genre, out string errorMessage))
			{
				TempData["success"] = "Genre created successfully!";
				return RedirectToAction("Index");
			}

			ModelState.AddModelError("Name", errorMessage);
			return View(genre);
		}

		public IActionResult Edit(int id)
		{
			var genre = _genreService.GetGenreById(id);
			if (genre == null) return NotFound();
			return View(genre);
		}

		[HttpPost]
		public IActionResult Edit(Genre genre)
		{
			if (_genreService.UpdateGenre(genre, out string errorMessage))
			{
				TempData["success"] = "Genre updated successfully!";
				return RedirectToAction("Index");
			}

			ModelState.AddModelError("Name", errorMessage);
			return View(genre);
		}

		public IActionResult Delete(int id)
		{
			var genre = _genreService.GetGenreById(id);
			if (genre == null) return NotFound();
			return View(genre);
		}

		[HttpPost, ActionName("Delete")]
		public IActionResult DeletePOST(int id)
		{
			if (_genreService.DeleteGenre(id))
			{
				TempData["success"] = "Genre deleted successfully!";
				return RedirectToAction("Index");
			}

			return NotFound();
		}
	}
}
