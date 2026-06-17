using Accessory_DesktopApp.Views;
using Accessory_DesktopApp.Models;
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

        [ObservableProperty] public AdminMenuItem selectedMenuItem;

        [ObservableProperty] public object currentViewModel;

        public ObservableCollection<AdminMenuItem> MenuItems { get; } = new()
        {
            new AdminMenuItem { Title = "Thống kê", IconGlyph = "\uE9D2" },
            new AdminMenuItem { Title = "Nhân viên", IconGlyph = "\uE716" },
            new AdminMenuItem { Title = "Người dùng", IconGlyph = "\uE77B" },
            new AdminMenuItem { Title = "Sản phẩm", IconGlyph = "\uE719" },
            new AdminMenuItem { Title = "Danh mục", IconGlyph = "\uE8FD" },
            new AdminMenuItem { Title = "Đơn hàng", IconGlyph = "\uE7BF" },
            new AdminMenuItem { Title = "Kênh bán", IconGlyph = "\uE774" }
        };

        public AdminViewModel(AdminWindow adminWindow)
        {
            _adminWindow = adminWindow;
            ChangeView(MenuItems[0]);
        }

        [RelayCommand]
        private void ChangeView(AdminMenuItem view)
        {
            SelectedMenuItem = view;
            switch (view?.Title)
            {
                case "Thống kê":
                    CurrentViewModel = new DashboardViewModel();
                    break;
                case "Nhân viên":
                    CurrentViewModel = new EmployeeViewModel();
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
                case "Kênh bán":
                    CurrentViewModel = new ChannelViewModel();
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
