using System;
using System.Collections.Generic;
using System.Text;

namespace Accessory_DesktopApp.Models
{
    public class LoginResponse
    {
        public bool status { get; set; }
        public LoginData? data { get; set; }
        public string? message { get; set; }
    }

    public class LoginData
    {
        public User? user { get; set; }
        public string? token { get; set; }
    }
}
