using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using VinylVibe.BusinessLogic.Interfaces;
using VinylVibe.DataAccess.Data;
using VinylVibe.DataAccess.Repository.IRepository;
using VinylVibe.Models;
using VinylVibe.Models.ViewModel;


namespace VinylVibeWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = VinylVibe.Utility.Details.Role_Admin)]
    public class UserController : Controller
    {

		private readonly IUserService _userService;

		public UserController(IUserService userService)
		{
			_userService = userService;
		}

		public IActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> RoleManagement(string userId)
		{
			var roleViewModel = await _userService.GetRoleManagementViewModelAsync(userId);
			return View(roleViewModel);
		}

		[HttpPost]
		public async Task<IActionResult> RoleManagement(RoleManagementViewModel roleManagementVM)
		{
			await _userService.UpdateUserRoleAsync(roleManagementVM);
			return RedirectToAction("Index");
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var users = await _userService.GetAllUsersAsync();
			return Json(new { data = users });
		}

		[HttpPost]
		public async Task<IActionResult> LockUnlock([FromBody] string id)
		{
			await _userService.LockUnlockUserAsync(id);
			return Json(new { success = true, message = "Successfully locked/unlocked" });
		}
	}
}
