using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VinylVibe.DataAccess.Data;
using VinylVibe.Models;
using VinylVibe.Utility;

namespace VinylVibe.DataAccess.DbInitializer
{




    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(UserManager<IdentityUser> userManager,
           RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public void Initialize()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }

            }
            catch (Exception ex)
            { }
            if (!_roleManager.RoleExistsAsync(Details.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(Details.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Details.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Details.Role_Company)).GetAwaiter().GetResult();


                _userManager.CreateAsync(new User
                {
                    UserName = "admin1@gmail.com",
                    Email = "admin1@gmail.com",
                    Name = "Stevan Djurdjic",
                    PhoneNumber = "22132311",
                    StreetAddress = "Bulevar Oslobodjenja 120",
                    Country = "Serbia",
                    City = "Belgrad",
                    PostalCode = "11000"
                }, "Steva104!").GetAwaiter().GetResult();

                User user = _db.Users.FirstOrDefault(x => x.Email == "admin1@gmail.com");
                _userManager.AddToRoleAsync(user, Details.Role_Admin).GetAwaiter().GetResult();

            }

            return;
          


        }
    }
}
