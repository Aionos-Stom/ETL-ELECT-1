using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
        public DateTime OrderDate { get; set; }
        public string? Status { get; set; }

        public Customer Customer { get; set; } = null!;
        public List<OrderDetail> OrderDetails { get; set; } = new();
    }
}
