using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VinylVibe.Models.ViewModel;
using VinylVibe.Models;

namespace VinylVibe.BusinessLogic.Interfaces
{
	public interface IUserService
	{
		Task<IList<User>> GetAllUsersAsync();
		Task<User> GetUserByIdAsync(string userId);
		Task<RoleManagementViewModel> GetRoleManagementViewModelAsync(string userId);
		Task UpdateUserRoleAsync(RoleManagementViewModel roleManagementVM);
		Task LockUnlockUserAsync(string userId);

		IQueryable<User> GetAllUsers();

    }
}
