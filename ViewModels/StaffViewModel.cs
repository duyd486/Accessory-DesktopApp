using Accessory_DesktopApp.Models;
using Accessory_DesktopApp.Models.Dtos;
using Accessory_DesktopApp.Models.Response;
using Accessory_DesktopApp.Singletons;
using Accessory_DesktopApp.Views;
using Accessory_DesktopApp.Views.Modals;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;

namespace Accessory_DesktopApp.ViewModels
{
    public partial class StaffViewModel : ObservableObject
    {
        private readonly StaffWindow _staffWindow;

        private CancellationTokenSource? _checkPaymentCts;


        public event EventHandler? PaymentPaid;

        [ObservableProperty]
        private ObservableCollection<ProductDto> products = new();

        [ObservableProperty]
        private ObservableCollection<Channel> channels = new();

        [ObservableProperty]
        private ObservableCollection<ProductDto> filteredProducts = new();

        [ObservableProperty]
        private ObservableCollection<CartProduct> cartItems = new();

        [ObservableProperty]
        private string? searchKey;

        [ObservableProperty]
        private string? phone;

        [ObservableProperty]
        private string? address;

        [ObservableProperty]
        private string? modalTotalPrice;

        [ObservableProperty]
        private string? modalQrImageUrl;

        [ObservableProperty]
        private string? modalOrderCode;

        public ObservableCollection<string> PaymentMethods { get; set; } =
        [
            "Tiền mặt",
            "Chuyển khoản",
            "Shopee"
        ];

        [ObservableProperty]
        private string selectedPaymentMethod = "Tiền mặt";

        [ObservableProperty]
        private bool isBankingModalOpen;

        public string TotalPriceDisplay =>
            string.Format(
                new CultureInfo("vi-VN"),
                "{0:c0}",
                CartItems.Sum(x => (x.price ?? 0) * x.CartQuantity));

        public StaffViewModel(StaffWindow staffWindow)
        {
            _staffWindow = staffWindow;
            _ = FetchProductsAsync();
        }

        [RelayCommand]
        private void Logout()
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            _staffWindow.Close();
        }

        partial void OnSearchKeyChanged(string? value)
        {
            ApplyFilter();
        }

        [RelayCommand]
        private void Search()
        {
            ApplyFilter();
        }

        private async Task FetchProductsAsync()
        {
            var response = await ApiManager
                .GetInstance()
                .HttpGetAsync<ProductListData>("list-product-v2");

            if (response?.list_products == null)
                return;

            App.Current.Dispatcher.Invoke(() =>
            {
                Products.Clear();

                foreach (var item in response.list_products)
                {
                    Products.Add(item);
                }

                ApplyFilter();
            });
        }

        private int GetChannelIdFromPaymentMethod()
        {
            return SelectedPaymentMethod switch
            {
                "Shopee" => 2,
                "Tiền mặt" => 1,
                "Chuyển khoản" => 1,
                _ => 1
            };
        }

        public void StartCheckPaymentStatusPolling()
        {
            if (string.IsNullOrWhiteSpace(ModalOrderCode))
                return;

            StopCheckPaymentStatusPolling();

            _checkPaymentCts = new CancellationTokenSource();
            _ = PollCheckPaymentStatusAsync(ModalOrderCode!, _checkPaymentCts.Token);
        }

        public void StopCheckPaymentStatusPolling()
        {
            _checkPaymentCts?.Cancel();
            _checkPaymentCts = null;
        }

        private async Task PollCheckPaymentStatusAsync(string orderCode, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var encodedOrderCode = Uri.EscapeDataString(orderCode);
                    var res = await ApiManager
                        .GetInstance()
                        .HttpGetPlainAsync<ResponseBase<CheckPaymentStatusData>>(
                            $"check-payment-status?orderCode={encodedOrderCode}")
                        .ConfigureAwait(false);

                    if (res?.status == true &&
                        string.Equals(res.data?.status, "PAID", StringComparison.OrdinalIgnoreCase))
                    {
                        StopCheckPaymentStatusPolling();
                        PaymentPaid?.Invoke(this, EventArgs.Empty);
                        return;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
        }

        private void ApplyFilter()
        {
            IEnumerable<ProductDto> query = Products;

            if (!string.IsNullOrWhiteSpace(SearchKey))
            {
                query = query.Where(x =>
                    x.name?.ToLower()
                    .Contains(SearchKey.ToLower()) == true);
            }

            FilteredProducts.Clear();

            foreach (var item in query)
            {
                FilteredProducts.Add(item);
            }
        }

        [RelayCommand]
        private void AddToCart(ProductDto product)
        {
            var existing = CartItems.FirstOrDefault(x => x.id == product.id);

            if (existing != null)
            {
                existing.CartQuantity++;
            }
            else
            {
                CartItems.Add(new CartProduct
                {
                    id = (int?)product.id,
                    name = product.name,
                    price = (decimal?)product.price,
                    thumbnail_url = product.thumbnail_url,
                    CartQuantity = 1
                });
            }

            OnPropertyChanged(nameof(TotalPriceDisplay));
        }

        [RelayCommand]
        private void IncreaseQuantity(CartProduct item)
        {
            item.CartQuantity++;
            OnPropertyChanged(nameof(TotalPriceDisplay));
        }

        [RelayCommand]
        private void DecreaseQuantity(CartProduct item)
        {
            item.CartQuantity--;

            if (item.CartQuantity <= 0)
            {
                CartItems.Remove(item);
            }

            OnPropertyChanged(nameof(TotalPriceDisplay));
        }

        [RelayCommand]
        private void RemoveCartItem(CartProduct item)
        {
            CartItems.Remove(item);
            OnPropertyChanged(nameof(TotalPriceDisplay));
        }

        [RelayCommand]
        private async Task CheckoutAsync()
        {
            if (CartItems.Count == 0)
                return;

            StopCheckPaymentStatusPolling();

            var body = new
            {
                payment_method = SelectedPaymentMethod,
                channelId = GetChannelIdFromPaymentMethod(),
                total_price = CartItems.Sum(x => (x.price ?? 0) * x.CartQuantity),
                phone = Phone ?? "00",
                address = Address ?? "Offline",
                items = CartItems.Select(x => new
                {
                    id = x.id,
                    quantity = x.CartQuantity,
                    total_price = (x.price ?? 0) * x.CartQuantity,
                    price = x.price
                })
            };

            var response = await ApiManager
                .GetInstance()
                .HttpPostJsonAsync<CreateOrderResponse>("create-bill", body);


            if (SelectedPaymentMethod == "Chuyển khoản")
            {
                ModalTotalPrice = string.Format(
                    new CultureInfo("vi-VN"),
                    "{0:c0}",
                    body.total_price);

                //MessageBox.Show("Cost: " + ModalTotalPrice);

                //MessageBox.Show(response?.bill.qr_image_url);

                ModalQrImageUrl = response?.bill.qr_image_url;

                ModalOrderCode = response?.bill.order_code;

                ModalTotalPrice = string.Format(
                    new CultureInfo("vi-VN"),
                    "{0:c0}",
                    response?.bill.total_price ?? 0);

                var window = new BankingWindow(this);

                window.Owner = Application.Current.MainWindow;

                window.ShowDialog();
            }
            else
            {
                MessageBox.Show("Tạo đơn hàng thành công");
            }

            CartItems.Clear();

            OnPropertyChanged(nameof(TotalPriceDisplay));
        }
    }
}
