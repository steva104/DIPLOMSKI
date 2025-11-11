using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VinylVibe.Models.ViewModel
{
    public class ShoppingCartViewModel
    {
        public IEnumerable<ShoppingCart> Items { get; set; }

        public OrderHeader OrderHeader { get; set; }


    }
}
