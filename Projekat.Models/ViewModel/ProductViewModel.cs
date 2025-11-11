using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VinylVibe.Models.ViewModel
{
    public class ProductViewModel
    {
        public Product Product { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> GenreList { get; set; }


    }
}
