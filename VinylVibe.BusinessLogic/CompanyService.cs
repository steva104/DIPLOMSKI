using Microsoft.AspNetCore.Identity;
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

namespace VinylVibe.BusinessLogic
{
	public class CompanyService :ICompanyService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
	
		public CompanyService(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			
			_userManager = userManager;
			_roleManager = roleManager;
			_unitOfWork = unitOfWork;
		}

		public IQueryable<Company> GetAllCompanies()
		{
			return _unitOfWork.Company.GetAll();
		}

        public IQueryable<Company> ApplySorting(IQueryable<Company> query, string sortColumn, string sortDirection)
        {
            if (string.IsNullOrEmpty(sortColumn))
                return query.OrderBy(o => o.Id);

            switch (sortColumn.ToLower())
            {
                case "name":
                    return sortDirection == "asc" ?
                        query.OrderBy(o => o.Name) :
                        query.OrderByDescending(o => o.Name);

                case "streetAddress":
                    return sortDirection == "asc" ?
                        query.OrderBy(o => o.StreetAddress) :
                        query.OrderByDescending(o => o.StreetAddress);

                case "country":
                    return sortDirection == "asc" ?
                        query.OrderBy(o => o.Country) :
                        query.OrderByDescending(o => o.Country);

                case "city":
                    return sortDirection == "asc" ?
                        query.OrderBy(o => o.City) :
                        query.OrderByDescending(o => o.City);

                case "postalCode":
                    return sortDirection == "asc" ?
                        query.OrderBy(o => o.PostalCode) :
                        query.OrderByDescending(o => o.PostalCode);
                case "phoneNumber":
                    return sortDirection == "asc" ?
                        query.OrderBy(o => o.PhoneNumber) :
                        query.OrderByDescending(o => o.PhoneNumber);

                case "id":
                default:
                    return sortDirection == "asc" ?
                        query.OrderBy(o => o.Id) :
                        query.OrderByDescending(o => o.Id);
            }
        }



        public Company GetCompanyById(int id)
		{
			return _unitOfWork.Company.Get(x => x.Id == id);
		}

		public void AddCompany(Company company)
		{
			_unitOfWork.Company.Add(company);
			_unitOfWork.Save();
		}

		public void UpdateCompany(Company company)
		{
			_unitOfWork.Company.Update(company);
			_unitOfWork.Save();
		}

		public void DeleteCompany(int id)
		{
			
			var company = _unitOfWork.Company.Get(x => x.Id == id);

			if (company != null)
			{				
				var users = _unitOfWork.User.GetAll(u => u.CompanyId == id).ToList();

				if (users.Any())
				{
					foreach (var user in users)
					{
			
						_userManager.RemoveFromRoleAsync(user, "Company").GetAwaiter().GetResult();
						_userManager.AddToRoleAsync(user, "Customer").GetAwaiter().GetResult();

						
						_unitOfWork.User.Update(user);
					}

					
					_unitOfWork.Save();
				}				
				_unitOfWork.Company.Delete(company);
				_unitOfWork.Save();

			}
			}


	}
}
