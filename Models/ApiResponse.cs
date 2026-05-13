using System;
using System.Collections.Generic;
using System.Text;

namespace Accessory_DesktopApp.Models
{
    public class ApiResponse
    {
        public bool Status { get; set; }
        public object Data { get; set; }
        public string Message { get; set; }
    }
}
