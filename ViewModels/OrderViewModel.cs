using Accessory_DesktopApp.Models;
using Accessory_DesktopApp.Models.Dtos;
using Accessory_DesktopApp.Models.Response;
using Accessory_DesktopApp.Singletons;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace Accessory_DesktopApp.ViewModels
{
    public partial class OrderViewModel : ObservableObject
    {
        private const long ShopeeChannelId = 2;

        [ObservableProperty]
        private ObservableCollection<OrderDto> orders = new();

        [ObservableProperty]
        private ObservableCollection<Channel> channels = new();

        [ObservableProperty]
        private ObservableCollection<OrderDto> filteredOrders = new();

        [ObservableProperty]
        private ObservableCollection<OrderDto> systemShopeeOrders = new();

        [ObservableProperty]
        private ObservableCollection<OrderDto> shopeeOrders = new();

        [ObservableProperty]
        private bool isShopeeSyncModalOpen;

        [ObservableProperty]
        private string? search;

        [ObservableProperty]
        private string? filterStatus;

        public OrderViewModel()
        {
            _ = FetchChannelAsync();
            _ = FetchOrdersAsync();
        }

        [RelayCommand]
        private void Refresh()
        {
            _ = FetchChannelAsync();
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

        private async Task FetchChannelAsync()
        {
            var response = await ApiManager
                .GetInstance()
                .HttpGetAsync<ChannelListResponse>("channels")
                .ConfigureAwait(false);

            if (response == null)
            {
                MessageBox.Show(
                    "Không thể tải danh sách kênh bán hàng",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            App.Current.Dispatcher.Invoke(() =>
            {
                Channels.Clear();

                if(response.channels == null || response.channels.Count == 0)
                {
                    MessageBox.Show(
                        "Không có kênh bán hàng nào được tìm thấy",
                        "Thông báo",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                foreach (var item in response.channels)
                {
                    Channels.Add(item);
                }
            });
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


                    item.DisplayChannelName = GetChannelName(item.channel_id);
                    Orders.Add(item);
                }

                ApplyFilter();
            });
        }

        private string GetChannelName(long? id)
        {
            var c = Channels.FirstOrDefault(x => x.id == id);
            return c?.name ?? "---";
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
        private async Task OpenShopeeSyncModalAsync()
        {
            IsShopeeSyncModalOpen = true;
            LoadSystemShopeeOrdersFromOrders();
            await FetchShopeeOrdersAsync();
        }

        [RelayCommand]
        private void CloseShopeeSyncModal()
        {
            IsShopeeSyncModalOpen = false;
        }

        [RelayCommand]
        private async Task ProceedShopeeSyncAsync()
        {
            var response = await ApiManager
                .GetInstance()
                .HttpPostPlainAsync<ShopeeSyncOrdersResponse>("shopee/sync-orders")
                .ConfigureAwait(false);

            Application.Current.Dispatcher.Invoke(() =>
                MessageBox.Show(
                    response?.message ?? "Đồng bộ thành công",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information));
        }

        private void LoadSystemShopeeOrdersFromOrders()
        {
            SystemShopeeOrders.Clear();

            var query = Orders.Where(x =>
                x.channel_id == ShopeeChannelId ||
                (x.order_code?.Contains("SHOPEE", StringComparison.OrdinalIgnoreCase) == true));

            foreach (var item in query)
            {
                SystemShopeeOrders.Add(item);
            }
        }

        private async Task FetchShopeeOrdersAsync()
        {
            var response = await ApiManager
                .GetInstance()
                .HttpGetPlainAsync<ShopeeOrdersResponse>("shopee/orders")
                .ConfigureAwait(false);

            var data = response?.data ?? new();

            App.Current.Dispatcher.Invoke(() =>
            {
                ShopeeOrders.Clear();

                foreach (var item in data)
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

                    item.DisplayChannelName = GetChannelName(item.channel_id);
                    ShopeeOrders.Add(item);
                }
            });
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

    public class ShopeeOrdersResponse
    {
        public List<OrderDto> data { get; set; } = new();
    }

    public class ShopeeSyncOrdersResponse
    {
        public string? message { get; set; }
    }
}
