using Accessory_DesktopApp.Singletons;
using Accessory_DesktopApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Accessory_DesktopApp.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly LoginWindow _loginWindow;

        [ObservableProperty] private string? emailText;
        [ObservableProperty] private string? passwordText;
        [ObservableProperty] private string? rePasswordText;

        public LoginViewModel(LoginWindow loginWindow)
        {
            _loginWindow = loginWindow;
        }

        private void Dev()
        {
            MessageBox.Show("Đăng nhập thành công với quyền admin");
            AdminWindow adminWindow = new AdminWindow();
            adminWindow.Show();
            _loginWindow.Close();
        }

        private void AdminDev()
        {
            EmailText = "tomnguyenhieu2004@gmail.com";
            PasswordText = "12345678";
        }

        private void StaffDev()
        {
            EmailText = "feyd153@gmail.com";
            PasswordText = "12345678";
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            //Dev();
            //return;


            if (EmailText == null || PasswordText == null)
            {
                //MessageBox.Show("Vui lòng điền đủ tài khoản và mật khẩu");
                //AdminDev();
                StaffDev();
                //return;
            }

            bool result = await ApiManager.GetInstance().LoginAsync(EmailText, PasswordText);

            if (result && ApiManager.GetInstance().GetCurrentUser() != null)
            {
                switch(ApiManager.GetInstance().GetCurrentUser()?.role)
                {
                    case 0:
                        MessageBox.Show("Đăng nhập thành công với quyền admin");
                        AdminWindow adminWindow = new AdminWindow();
                        adminWindow.Show();
                        _loginWindow.Close();
                        break;
                    case 2:
                        MessageBox.Show("Đăng nhập thành công với quyền nhân viên");
                        StaffWindow staffWindow = new StaffWindow();
                        staffWindow.Show();
                        _loginWindow.Close();
                        break;
                    default:
                        MessageBox.Show("Đây là tài khoản người dùng");
                        break;
                }
            }
        }

        public void SetPassword(string password)
        {
            PasswordText = password;
        }
    }
}
