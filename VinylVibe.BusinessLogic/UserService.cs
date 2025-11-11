using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VinylVibe.BusinessLogic.Interfaces;
using VinylVibe.DataAccess.Data;
using VinylVibe.DataAccess.Repository.IRepository;
using VinylVibe.Models;
using VinylVibe.Models.ViewModel;
using VinylVibe.Utility;

namespace VinylVibe.BusinessLogic
{
	public class UserService :IUserService
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public UserService(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			_db = db;
			_userManager = userManager;
			_roleManager = roleManager;
		}

		public async Task<IList<User>> GetAllUsersAsync()
		{
			var users = await _db.Users.Include(x => x.Company).ToListAsync();
			var userRoles = await _db.UserRoles.ToListAsync();
			var roles = await _db.Roles.ToListAsync();

			foreach (var user in users)
			{
				var roleId = userRoles.FirstOrDefault(x => x.UserId == user.Id)?.RoleId;
				var role = roles.FirstOrDefault(x => x.Id == roleId)?.Name;

				user.Role = role ?? string.Empty;
				user.Company ??= new Company { Name = string.Empty };
			}

			return users;
		}

		public IQueryable<User> GetAllUsers()
		{
           
            return _db.Users.Include(u => u.Company).AsQueryable();
        }


		public async Task<User> GetUserByIdAsync(string userId)
		{
			var user = await _db.Users.Include(x => x.Company).FirstOrDefaultAsync(x => x.Id == userId);
			return user;
		}

		public async Task<RoleManagementViewModel> GetRoleManagementViewModelAsync(string userId)
		{
			var roleId = await _db.UserRoles.Where(x => x.UserId == userId).Select(x => x.RoleId).FirstOrDefaultAsync();
			var role = await _db.Roles.FirstOrDefaultAsync(x => x.Id == roleId);

			var roleViewModel = new RoleManagementViewModel
			{
				User = await GetUserByIdAsync(userId),
				RoleList = _db.Roles.Select(i => new SelectListItem
				{
					Text = i.Name,
					Value = i.Name
				}),
				CompanyList = _db.Companies.Select(i => new SelectListItem
				{
					Text = i.Name,
					Value = i.Id.ToString()
				}),
			};

			roleViewModel.User.Role = role?.Name ?? string.Empty;

			return roleViewModel;
		}

		public async Task UpdateUserRoleAsync(RoleManagementViewModel roleManagementVM)
		{
			var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == roleManagementVM.User.Id);
			var oldRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

			
			if (user.CompanyId != roleManagementVM.User.CompanyId)
			{
				user.CompanyId = roleManagementVM.User.CompanyId; 
				await _db.SaveChangesAsync(); 
			}

			
			if (roleManagementVM.User.Role != oldRole)
			{
				if (oldRole == VinylVibe.Utility.Details.Role_Company)
				{
					user.CompanyId = null; 
				}

				await _userManager.RemoveFromRoleAsync(user, oldRole); 
				await _userManager.AddToRoleAsync(user, roleManagementVM.User.Role);
			}

			await _db.SaveChangesAsync(); 
		}

		public async Task LockUnlockUserAsync(string userId)
		{
			var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);

			if (user != null)
			{
				if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
				{
					user.LockoutEnd = DateTime.Now;
				}
				else
				{
					user.LockoutEnd = DateTime.Now.AddYears(10);
				}
				await _db.SaveChangesAsync();
			}
		}


	}
}
