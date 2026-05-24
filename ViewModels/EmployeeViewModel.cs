using Accessory_DesktopApp.Models;
using Accessory_DesktopApp.Models.Dtos;
using Accessory_DesktopApp.Models.Response;
using Accessory_DesktopApp.Singletons;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace Accessory_DesktopApp.ViewModels
{
    public partial class EmployeeViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<User> customers = new();

        [ObservableProperty]
        private ObservableCollection<User> filteredCustomers = new();

        [ObservableProperty]
        private string? searchKeyword;

        [ObservableProperty]
        private CreateEmployeeRequest employeeForm = new();

        [ObservableProperty]
        private bool isDialogOpen;

        [ObservableProperty]
        private string dialogTitle = "Thêm nhân viên";

        public ObservableCollection<string> RoleOptions { get; } =
        [
            "Nhân viên",
            "Admin"
        ];

        [ObservableProperty]
        private string selectedRole = "Nhân viên";

        public EmployeeViewModel()
        {
            _ = FetchCustomersAsync();
        }

        private async Task FetchCustomersAsync()
        {
            var items = await ApiManager
                .GetInstance()
                .HttpGetAsync<List<User>>("list-customer")
                .ConfigureAwait(false);

            if (items is null)
                return;

            items = items
                .Where(x => x.role == 0 || x.role == 2)
                .ToList();

            if (App.Current?.Dispatcher?.CheckAccess() == true)
            {
                ReplaceCustomers(items);
            }
            else
            {
                App.Current?.Dispatcher?.Invoke(() =>
                {
                    ReplaceCustomers(items);
                });
            }
        }

        private void ReplaceCustomers(IEnumerable<User> items)
        {
            Customers.Clear();

            foreach (var item in items)
            {
                Customers.Add(item);
            }

            ApplyFilter();
        }

        [RelayCommand]
        private void Refresh()
        {
            _ = FetchCustomersAsync();
        }

        [RelayCommand]
        private void Search()
        {
            ApplyFilter();
        }

        [RelayCommand]
        private void OpenAdd()
        {
            DialogTitle = "Thêm nhân viên";
            EmployeeForm = new CreateEmployeeRequest();
            SelectedRole = RoleOptions.FirstOrDefault() ?? "Nhân viên";
            IsDialogOpen = true;
        }

        [RelayCommand]
        private void CloseDialog()
        {
            IsDialogOpen = false;
        }

        [RelayCommand]
        private async Task SubmitEmployeeAsync()
        {
            if (string.IsNullOrWhiteSpace(EmployeeForm.name)
                || string.IsNullOrWhiteSpace(EmployeeForm.email)
                || string.IsNullOrWhiteSpace(EmployeeForm.password)
                || string.IsNullOrWhiteSpace(EmployeeForm.password_confirmation))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin (tên, email, mật khẩu).", "Thiếu dữ liệu");
                return;
            }

            var endpoint = string.Equals(SelectedRole, "Admin", StringComparison.OrdinalIgnoreCase)
                ? "create-admin"
                : "create-staff";

            var response = await ApiManager
                .GetInstance()
                .HttpPostPlainAsync<ResponseBase<CreateEmployeeResponse>>(endpoint, EmployeeForm)
                .ConfigureAwait(false);

            if (response?.status != true)
            {
                var message = response?.message;
                if (string.IsNullOrWhiteSpace(message))
                    message = "Tạo nhân viên thất bại.";

                if (Application.Current?.Dispatcher?.CheckAccess() == true)
                {
                    MessageBox.Show(message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    Application.Current?.Dispatcher?.Invoke(() =>
                        MessageBox.Show(message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error));
                }

                return;
            }

            var user = response.data?.user;

            IsDialogOpen = false;

            if (Application.Current?.Dispatcher?.CheckAccess() == true)
            {
                MessageBox.Show(response.message ?? "Tạo nhân viên thành công.", "Thành công");
            }
            else
            {
                Application.Current?.Dispatcher?.Invoke(() =>
                    MessageBox.Show(response.message ?? "Tạo nhân viên thành công.", "Thành công"));
            }

            if (user != null)
            {
                if (Application.Current?.Dispatcher?.CheckAccess() == true)
                {
                    Customers.Insert(0, user);
                    ApplyFilter();
                }
                else
                {
                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        Customers.Insert(0, user);
                        ApplyFilter();
                    });
                }
            }
            else
            {
                await FetchCustomersAsync().ConfigureAwait(false);
            }
        }

        private void ApplyFilter()
        {
            IEnumerable<User> query = Customers;

            if (!string.IsNullOrWhiteSpace(SearchKeyword))
            {
                var keyword = SearchKeyword.Trim().ToLower();

                query = query.Where(x =>
                    (!string.IsNullOrWhiteSpace(x.name) &&
                     x.name.ToLower().Contains(keyword))
                    ||
                    (!string.IsNullOrWhiteSpace(x.email) &&
                     x.email.ToLower().Contains(keyword)));
            }

            FilteredCustomers.Clear();

            foreach (var item in query)
            {
                FilteredCustomers.Add(item);
            }
        }
    }
}
