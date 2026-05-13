using System;
using System.Collections.Generic;
using System.Text;

namespace Accessory_DesktopApp.Models.Response
{
    public class ProductDto
    {
        public int? id { get; set; }

        public string? name { get; set; }

        public int? category_id { get; set; }

        public decimal? price { get; set; }

        public int? quantity { get; set; }

        public int? total_sold { get; set; }

        public string? description { get; set; }

        public string? thumbnail_url { get; set; }

        public string? CategoryTitle { get; set; }

        public string? DisplayPrice { get; set; }

        public string? ShortDescription { get; set; }
    }
}
