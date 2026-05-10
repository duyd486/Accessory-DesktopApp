using Accessory_DesktopApp.Models.Dtos;
using Accessory_DesktopApp.Singletons;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Accessory_DesktopApp.ViewModels
{
    public partial class OrderViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<OrderDto> orders = new();

        [ObservableProperty]
        private ObservableCollection<OrderDto> filteredOrders = new();

        [ObservableProperty]
        private string? search;

        [ObservableProperty]
        private string? filterStatus;

        public OrderViewModel()
        {
            _ = FetchOrdersAsync();
        }

        [RelayCommand]
        private void Refresh()
        {
            _ = FetchOrdersAsync();
        }

        [RelayCommand]
        private void SearchOrders()
        {
            ApplyFilter();
        }

        partial void OnSearchChanged(string? value)
        {
            ApplyFilter();
        }

        partial void OnFilterStatusChanged(string? value)
        {
            ApplyFilter();
        }

        private async Task FetchOrdersAsync()
        {
            var response = await ApiManager
                .GetInstance()
                .HttpGetAsync<List<OrderDto>>("list-orders")
                .ConfigureAwait(false);

            if (response == null)
                return;

            App.Current.Dispatcher.Invoke(() =>
            {
                Orders.Clear();

                foreach (var item in response)
                {
                    item.DisplayOrderCode = $"#{item.order_code}";

                    item.DisplayTotalPrice = string.Format(
                        new CultureInfo("vi-VN"),
                        "{0:c0}",
                        item.total_price);

                    if (item.created_at != null)
                    {
                        item.DisplayDate = item.created_at.Value
                            .ToLocalTime()
                            .ToString("dd/MM/yyyy HH:mm");
                    }

                    Orders.Add(item);
                }

                ApplyFilter();
            });
        }

        private void ApplyFilter()
        {
            IEnumerable<OrderDto> query = Orders;

            if (!string.IsNullOrWhiteSpace(Search))
            {
                query = query.Where(x =>
                    x.order_code?
                    .ToString()
                    .Contains(Search.ToLower()) == true);
            }

            if (!string.IsNullOrWhiteSpace(FilterStatus))
            {
                query = query.Where(x =>
                    x.status?.ToString() == FilterStatus);
            }

            FilteredOrders.Clear();

            foreach (var item in query)
            {
                FilteredOrders.Add(item);
            }
        }

        [RelayCommand]
        private async Task UpdateOrderStatusAsync(OrderDto order)
        {
            var body = new
            {
                order_id = order.id,
                status = order.status
            };

            MessageBox.Show(body.ToString());

            await ApiManager
                .GetInstance()
                .HttpPostNoDataAsync(
                    "update-order-status",
                    body);

            MessageBox.Show(
                "Cập nhật trạng thái đơn hàng thành công",
                "Thông báo",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
