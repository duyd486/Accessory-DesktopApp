using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Accessory_DesktopApp.Models.Dtos
{
    public partial class CartProduct : ObservableObject
    {
        public int? id { get; set; }

        public string? name { get; set; }

        public string? thumbnail_url { get; set; }

        public decimal? price { get; set; }

        [ObservableProperty]
        private int cartQuantity = 1;

        public string DisplayPrice =>
            string.Format(new CultureInfo("vi-VN"), "{0:c0}", price);

        public string CartTotalPrice =>
            string.Format(
                new CultureInfo("vi-VN"),
                "{0:c0}",
                (price ?? 0) * CartQuantity);
    }
}
