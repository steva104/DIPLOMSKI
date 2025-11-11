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
    public class CompanyController : Controller
    {

		private readonly ICompanyService _companyService;

		public CompanyController(ICompanyService companyService)
		{
			_companyService = companyService;
		}

		public IActionResult Index()
		{
			//var objCompanyList = _companyService.GetAllCompanies();
			return View();
		}

		public IActionResult Upsert(int? id)
		{
			if (id == null || id == 0)
			{
				return View(new Company());
			}
			else
			{
				var company = _companyService.GetCompanyById(id.Value);
				return View(company);
			}
		}

		[HttpPost]
		public IActionResult Upsert(Company company)
		{
			if (ModelState.IsValid)
			{
				if (company.Id == 0)
				{
					_companyService.AddCompany(company);
				}
				else
				{
					_companyService.UpdateCompany(company);
				}

				TempData["success"] = "Company saved successfully!";
				return RedirectToAction("Index");
			}

			return View(company);
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


			var query = _companyService.GetAllCompanies();

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(o =>
                    o.Name.ToLower().Contains(searchValue) ||
                    o.StreetAddress.ToLower().Contains(searchValue) ||
                    o.Country.ToLower().Contains(searchValue) ||
                    o.City.ToLower().Contains(searchValue) ||
                    o.PostalCode.ToLower().Contains(searchValue) ||
                    o.PhoneNumber.ToLower().Contains(searchValue));
            }
            query = _companyService.ApplySorting(query, sortColumn, sortDirection);
            int recordsTotal = query.Count();
            var data = query
                            .Skip(start)
                            .Take(length)
                            .Select(o => new {
                                o.Id,
                                o.Name,
                                o.StreetAddress,
                                o.City,
                                o.Country,
                                o.PostalCode,
								o.PhoneNumber,
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
		public IActionResult Delete(int? id)
		{
			if (id == null)
			{
				return Json(new { success = false, message = "Invalid request" });
			}

			_companyService.DeleteCompany(id.Value);
			return Json(new { success = true, message = "Successfully deleted" });
		}
		#endregion
	}
}
