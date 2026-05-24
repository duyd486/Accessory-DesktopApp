using Accessory_DesktopApp.Models;
using Accessory_DesktopApp.Singletons;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Accessory_DesktopApp.ViewModels
{
    public partial class UserViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<User> customers = new();

        [ObservableProperty]
        private ObservableCollection<User> filteredCustomers = new();

        [ObservableProperty]
        private string? searchKeyword;

        public UserViewModel()
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
                .Where(x => x.role == 1)
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
