using System;
using System.Collections.Generic;
using System.Text;

namespace Accessory_DesktopApp.Models
{
    public class User
    {
        public int? id { get; set; }
        public string? name { get; set; }
        public string? email { get; set; }
        public int? role { get; set; } = 2;
        public string? token { get; set; }
        public string? phone { get; set; }
        public string? address { get; set; }
    }
}
