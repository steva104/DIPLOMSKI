using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VinylVibe.Models.ViewModel
{
    public class OrderViewModel
    {
        public OrderHeader OrderHeader { get; set; }

        public IEnumerable<OrderDetails> OrderDetails { get; set; }


    }
}
