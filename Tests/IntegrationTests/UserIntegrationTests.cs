using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VinylVibe.BusinessLogic;
using VinylVibe.DataAccess.Data;
using VinylVibe.Models;
using VinylVibe.Models.ViewModel;

namespace Tests.IntegrationTests
{
    public class UserIntegrationTests
    {

        private ApplicationDbContext _dbContext;
        private UserService _userService;
        private UserManager<IdentityUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _dbContext = new ApplicationDbContext(options);

            var userStore = new UserStore<IdentityUser>(_dbContext);
            var roleStore = new RoleStore<IdentityRole>(_dbContext);

            _userManager = new UserManager<IdentityUser>(
                userStore, null, new PasswordHasher<IdentityUser>(),
                new List<IUserValidator<IdentityUser>>(),
                new List<IPasswordValidator<IdentityUser>>(),
                null, null, null, null);


            _roleManager = new RoleManager<IdentityRole>(
                roleStore, null, null, null, null);

            _userService = new UserService(_dbContext, _userManager, _roleManager);

            SeedData();
        }
        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
        }
        private void SeedData()
        {
            var company = new Company { Id = 1, Name = "CompanyA" };
            _dbContext.Companies.Add(company);

            var adminRole = new IdentityRole("Admin") { Id = "r1" };
            var customerRole = new IdentityRole("Customer") { Id = "r2" };
            var companyRole = new IdentityRole("Company") { Id = "r3" };
            _dbContext.Roles.AddRange(adminRole, customerRole, companyRole);

            var user1 = new User { Id = "u1", UserName = "user1", Name = "User One", Company = null };
            var user2 = new User { Id = "u2", UserName = "user2", Name = "User Two", Company = null };
            var user3 = new User { Id = "u3", UserName = "user3", Name = "User Three", Company = company };
            _dbContext.Users.AddRange(user1, user2, user3);

            _dbContext.UserRoles.Add(new IdentityUserRole<string> { UserId = "u1", RoleId = "r1" });
            _dbContext.UserRoles.Add(new IdentityUserRole<string> { UserId = "u2", RoleId = "r2" });
            _dbContext.UserRoles.Add(new IdentityUserRole<string> { UserId = "u3", RoleId = "r3" });
            
            _dbContext.SaveChanges();
        }

        #region UsersIntegrationTests
        [Test]
        public async Task GetAllUsersAsync_ReturnsUsersWithRolesAndCompanies()
        {
            var users = await _userService.GetAllUsersAsync();

            Assert.That(users.Count, Is.EqualTo(3));

            var user1 = users.First(u => u.Id == "u1");
            Assert.That(user1.Role, Is.EqualTo("Admin"));
            Assert.That(user1.Company.Name, Is.EqualTo(string.Empty));

            var user2 = users.First(u => u.Id == "u2");
            Assert.That(user2.Role, Is.EqualTo("Customer"));
            Assert.That(user2.Company.Name, Is.EqualTo(string.Empty));

            var user3 = users.First(u => u.Id == "u3");
            Assert.That(user3.Role, Is.EqualTo("Company"));
            Assert.That(user3.Company, Is.Not.Null);
            Assert.That(user3.Company.Name, Is.EqualTo("CompanyA"));
        }

        [Test]
        public async Task GetAllUsersAsync_WhenNoUsersExist_ReturnsEmptyList()
        {
            _dbContext.Users.RemoveRange(_dbContext.Users);
            _dbContext.SaveChanges();

            var users = await _userService.GetAllUsersAsync();

            Assert.That(users, Is.Not.Null);
            Assert.That(users.Count, Is.EqualTo(0));
        }
        

        [Test]
        public async Task GetUserByIdAsync_WhenUserExists_ReturnsUserWithCompany()
        {
            var result = await _userService.GetUserByIdAsync("u3");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo("u3"));
            Assert.That(result.Company, Is.Not.Null);
            Assert.That(result.Company.Name, Is.EqualTo("CompanyA"));
        }

        [Test]
        public async Task GetUserByIdAsync_WhenUserDoesNotExist_ReturnsNull()
        {
            var result = await _userService.GetUserByIdAsync("non_existing_id");

            Assert.That(result, Is.Null);
        }
        #endregion

        #region RoleManagementViewModelTest
        [Test]
        public async Task GetRoleManagementViewModelAsync_WhenUserHasRole_ReturnsUserWithRole()
        {
            var result = await _userService.GetRoleManagementViewModelAsync("u1");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.User.Id, Is.EqualTo("u1"));
            Assert.That(result.User.Role, Is.EqualTo("Admin"));

            Assert.That(result.RoleList.Count(), Is.EqualTo(3));
            Assert.That(result.RoleList.First().Text, Is.EqualTo("Admin"));

            Assert.That(result.CompanyList.Count(), Is.EqualTo(1));
            Assert.That(result.CompanyList.First().Text, Is.EqualTo("CompanyA"));
        }
        #endregion

        #region RoleIntegrationTests
        [Test]
        public async Task UpdateUserRoleAsync_WhenCompanyIdChanges_UpdatesCompany()
        {
            var company2 = new Company { Id = 2, Name = "CompanyB" };
            var company3 = new Company { Id = 3, Name = "CompanyC" };
            var user = new User { Id = "u4", UserName = "user4", Name = "User Four", CompanyId = 1 };


            _dbContext.Companies.AddRange(company2, company3);
            _dbContext.Users.Add(user);
            _dbContext.UserRoles.Add(new IdentityUserRole<string> { UserId = "u4", RoleId = "r3" });
            await _dbContext.SaveChangesAsync();

            var vm = new RoleManagementViewModel
            {
                User = new User { Id = "u4", CompanyId = 2, Role = "Company" }
            };

            await _userService.UpdateUserRoleAsync(vm);
            var updatedUser = await _dbContext.Users.FindAsync("u4");

            Assert.That(updatedUser.CompanyId, Is.EqualTo(2));
        }
        [Test]
        public async Task UpdateUserRoleAsync_WhenRoleChanges_UpdatesRole()
        {
            if (!await _roleManager.RoleExistsAsync("Admin"))
                await _roleManager.CreateAsync(new IdentityRole("Admin") { Id = "r4" });
            if (!await _roleManager.RoleExistsAsync("Customer"))
                await _roleManager.CreateAsync(new IdentityRole("Customer") { Id = "r5" });

            var user = new User { Id = "u5", UserName = "user5", Name = "User Five" };
            await _userManager.CreateAsync(user);

            await _userManager.AddToRoleAsync(user, "Customer");

            var vm = new RoleManagementViewModel
            {
                User = new User { Id = "u5", Role = "Admin" }
            };

            await _userService.UpdateUserRoleAsync(vm);

            var userRoleEntry = await _dbContext.UserRoles
                .Where(ur => ur.UserId == "u5")
                .FirstOrDefaultAsync();

            Assert.That(userRoleEntry, Is.Not.Null);

            var role = await _dbContext.Roles
                .Where(r => r.Id == userRoleEntry.RoleId)
                .FirstOrDefaultAsync();

            Assert.That(role, Is.Not.Null);
            Assert.That(role.Name, Is.EqualTo("Admin"));
        }
        #endregion

        #region Lock/UnlockTests
        [Test]
        public async Task LockUnlockUserAsync_WhenUserIsUnlocked_ShouldLockUser()
        {
            var user = new User { Id = "u6", UserName = "user6", Name = "User Six" ,LockoutEnd = null };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

           
            await _userService.LockUnlockUserAsync("u6");

            var updatedUser = await _dbContext.Users.FindAsync("u6");
            Assert.That(updatedUser.LockoutEnd, Is.Not.Null);
            Assert.That(updatedUser.LockoutEnd.Value, Is.GreaterThan(DateTimeOffset.Now.AddYears(9))); 
        }
        [Test]
        public async Task LockUnlockUserAsync_WhenUserIsLocked_ShouldUnlockUser()
        {
            var user = new User { Id = "u7", UserName = "user7", Name= "User Seven", LockoutEnd = DateTime.Now.AddYears(1) };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            await _userService.LockUnlockUserAsync("u7");

            var updatedUser = await _dbContext.Users.FindAsync("u7");
            Assert.That(updatedUser.LockoutEnd, Is.Not.Null);
            Assert.That(updatedUser.LockoutEnd.Value, Is.LessThanOrEqualTo(DateTimeOffset.Now)); 
        }

        [Test]
        public async Task LockUnlockUserAsync_WhenUserDoesNotExist_ShouldNotThrow()
        {
            Assert.DoesNotThrowAsync(async () => await _userService.LockUnlockUserAsync("nonexistent"));
        }
        #endregion







    }
}
