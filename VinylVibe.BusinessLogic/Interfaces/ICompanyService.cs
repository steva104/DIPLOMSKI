using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VinylVibe.Models;

namespace VinylVibe.BusinessLogic.Interfaces
{
	public interface ICompanyService
	{

		IQueryable<Company> GetAllCompanies();

		IQueryable<Company> ApplySorting(IQueryable<Company> query, string sortColumn, string sortDirection);

        Company GetCompanyById(int id);
		void AddCompany(Company company);
		void UpdateCompany(Company company);
		void DeleteCompany(int id);
		

	}
}
