using System;
using System.Collections.Generic;
using System.Text;

namespace Accessory_DesktopApp.Models.Response
{
    public class ProductListResponse
    {
        public ProductListData? data { get; set; }
    }
    public class ProductListData
    {
        public List<ProductDto>? list_products { get; set; }
    }
}
