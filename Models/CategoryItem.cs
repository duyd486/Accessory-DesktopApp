using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Accessory_DesktopApp.Models
{
    public partial class CategoryItem : ObservableObject
    {
        public int id { get; set; }
        public string title { get; set; }
        public int parent_id { get; set; }
        public string thumbnail_url { get; set; }
        public List<CategoryItem> children { get; set; }
    }
}
