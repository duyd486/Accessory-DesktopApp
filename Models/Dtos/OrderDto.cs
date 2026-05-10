using System;
using System.Collections.Generic;
using System.Text;

namespace Accessory_DesktopApp.Models.Dtos
{
    public class OrderDto
    {
        public int? id { get; set; }

        public int? order_code { get; set; }

        public int? status { get; set; }

        public string? user_name { get; set; }

        public decimal? total_price { get; set; }

        public DateTime? created_at { get; set; }

        public List<OrderProductDto> products { get; set; } = new();

        public string? DisplayOrderCode { get; set; }

        public string? DisplayDate { get; set; }

        public string? DisplayTotalPrice { get; set; }
    }
}
