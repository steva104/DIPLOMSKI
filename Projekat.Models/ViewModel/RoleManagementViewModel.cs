using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VinylVibe.Models.ViewModel
{
    public class RoleManagementViewModel
    {
        public User User { get; set; }

        public IEnumerable<SelectListItem> RoleList { get; set; }

        public IEnumerable<SelectListItem> CompanyList { get; set; }
        



    }
}
