using System;
using System.Collections.Generic;
using System.Text;

namespace Accessory_DesktopApp.Models
{
    public class Category
    {
        public int? id { get; set; }

        public string? title { get; set; }

        public int? parent_id { get; set; }

        public List<Category>? children { get; set; }
    }
}
