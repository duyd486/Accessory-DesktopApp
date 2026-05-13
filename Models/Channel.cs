using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Accessory_DesktopApp.Models
{
    public class Channel : ObservableObject
    {
        public long? id { get; set; }
        public string? name { get; set; }
        public int? type { get; set; }
        public string? DisplayType => type switch
            {
                0 => "Nội bộ",
                1 => "API ngoài",
                _ => "Unknown"
            };
    }
}
