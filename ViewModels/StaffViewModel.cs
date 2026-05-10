using Accessory_DesktopApp.Models;
using Accessory_DesktopApp.Models.Dtos;
using Accessory_DesktopApp.Models.Response;
using Accessory_DesktopApp.Singletons;
using Accessory_DesktopApp.Views.Modals;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Text;
using System.Windows;

namespace Accessory_DesktopApp.ViewModels
{
    public partial class StaffViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<ProductDto> products = new();

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

        public ObservableCollection<string> PaymentMethods { get; set; } =
        [
            "offline",
            "online"
        ];

        public ObservableCollection<string> Channels { get; set; } =
        [
            "store",
            "shopee",
            "tiktok",
            "facebook"
        ];

        [ObservableProperty]
        private string selectedPaymentMethod = "offline";

        [ObservableProperty]
        private string selectedChannel = "store";

        [ObservableProperty]
        private bool isBankingModalOpen;

        public string TotalPriceDisplay =>
            string.Format(
                new CultureInfo("vi-VN"),
                "{0:c0}",
                CartItems.Sum(x => (x.price ?? 0) * x.CartQuantity));

        public StaffViewModel()
        {
            _ = FetchProductsAsync();
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

            var body = new
            {
                payment_method = SelectedPaymentMethod,
                channel = SelectedChannel,
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


            if (SelectedPaymentMethod == "online")
            {
                ModalTotalPrice = string.Format(
                    new CultureInfo("vi-VN"),
                    "{0:c0}",
                    body.total_price);

                MessageBox.Show("Cost: " + ModalTotalPrice);

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
