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
            "Dashboard", "User", "Product", "Category", "Order"
        };

        public AdminViewModel(AdminWindow adminWindow)
        {
            _adminWindow = adminWindow;
            ChangeView("Dashboard");
        }

        [RelayCommand]
        private void ChangeView(string view)
        {
            SelectedMenuItem = view;
            switch (view)
            {
                case "Dashboard":
                    CurrentViewModel = new DashboardViewModel();
                    break;
                case "User":
                    CurrentViewModel = new UserViewModel();
                    break;
                case "Product":
                    CurrentViewModel = new ProductViewModel();
                    break;
                case "Category":
                    CurrentViewModel = new CategoryViewModel();
                    break;
                case "Order":
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
