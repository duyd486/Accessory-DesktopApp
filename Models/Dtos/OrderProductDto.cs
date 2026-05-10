using System;
using System.Collections.Generic;
using System.Text;

namespace Accessory_DesktopApp.Models.Dtos
{
    public class OrderProductDto
    {
        public int? product_id { get; set; }

        public string? product_name { get; set; }

        public int? quantity { get; set; }
    }
}
