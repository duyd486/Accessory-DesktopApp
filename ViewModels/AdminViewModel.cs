using Accessory_DesktopApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Accessory_DesktopApp.ViewModels
{
    public partial class AdminViewModel : ObservableObject
    {
        private readonly AdminWindow _adminWindow;

        [ObservableProperty] public string selectedMenuItem;

        [ObservableProperty] public object currentViewModel;

        public ObservableCollection<string> MenuItems { get; } = new()
        {
            "Thống kê", "Người dùng", "Sản phẩm", "Danh mục", "Đơn hàng"
        };

        public AdminViewModel(AdminWindow adminWindow)
        {
            _adminWindow = adminWindow;
            ChangeView("Thống kê");
        }

        [RelayCommand]
        private void ChangeView(string view)
        {
            SelectedMenuItem = view;
            switch (view)
            {
                case "Thống kê":
                    CurrentViewModel = new DashboardViewModel();
                    break;
                case "Người dùng":
                    CurrentViewModel = new UserViewModel();
                    break;
                case "Sản phẩm":
                    CurrentViewModel = new ProductViewModel();
                    break;
                case "Danh mục":
                    CurrentViewModel = new CategoryViewModel();
                    break;
                case "Đơn hàng":
                    CurrentViewModel = new OrderViewModel();
                    break;
            }
        }

        [RelayCommand]
        private void Logout()
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            _adminWindow.Close();
        }

    }
}
