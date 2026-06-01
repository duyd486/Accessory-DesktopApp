using System;
using System.Collections.Generic;
using System.Text;

namespace Accessory_DesktopApp.Models.Dtos
{
    public class BillDto
    {
        public int? id { get; set; }
        public int? user_id { get; set; }
        public string? order_code { get; set; }
        public int? payment_method { get; set; }
        public decimal? total_price { get; set; }
        public string? phone { get; set; }
        public string? address { get; set; }
        public int? status { get; set; }
        public string? checkout_url { get; set; }
        public string? qr_image_url { get; set; }
    }
}
