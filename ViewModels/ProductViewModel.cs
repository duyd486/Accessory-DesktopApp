using Accessory_DesktopApp.Models;
using Accessory_DesktopApp.Models.Response;
using Accessory_DesktopApp.Singletons;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Windows;

namespace Accessory_DesktopApp.ViewModels
{
    public partial class ProductViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<ProductDto> products = new();

        [ObservableProperty]
        private ObservableCollection<ProductDto> filteredProducts = new();

        [ObservableProperty]
        private ObservableCollection<Category> categories = new();

        [ObservableProperty]
        private ProductDto productForm = new();

        [ObservableProperty]
        private bool isDialogOpen;

        [ObservableProperty]
        private string dialogTitle = "Thêm sản phẩm";

        [ObservableProperty]
        private string? searchKey;

        public ProductViewModel()
        {
            _ = FetchProductsAsync();
            _ = FetchCategoriesAsync();
        }

        partial void OnSearchKeyChanged(string? value)
        {
            ApplyFilter();
        }

        private async Task FetchProductsAsync()
        {
            var response = await ApiManager
                .GetInstance()
                .HttpGetAsync<ProductListData>("list-product-v2")
                .ConfigureAwait(false);

            if (response?.list_products == null)
                return;

            App.Current.Dispatcher.Invoke(() =>
            {
                Products.Clear();

                foreach (var item in response.list_products)
                {
                    item.CategoryTitle = GetCategoryTitle(item.category_id);
                    item.DisplayPrice = $"{item.price:N0} ₫";

                    if (!string.IsNullOrWhiteSpace(item.description))
                    {
                        item.ShortDescription =
                            item.description.Length > 50
                            ? item.description.Substring(0, 50) + "..."
                            : item.description;
                    }

                    Products.Add(item);
                }

                ApplyFilter();
            });
        }

        private async Task FetchCategoriesAsync()
        {
            var items = await ApiManager
                .GetInstance()
                .HttpGetAsync<CategoryListResponse>("list-category")
                .ConfigureAwait(false);

            if (items.categories == null)
                //MessageBox.Show("categories null");
                return;
             
            App.Current.Dispatcher.Invoke(() =>
            {
                Categories.Clear();

                foreach (var item in items.categories)
                {
                    if(item.children != null)
                    {
                        foreach (var child in item.children)
                        {
                            Categories.Add(child);
                        }
                    }
                    Categories.Add(item);
                }
            });
        }

        private void ApplyFilter()
        {
            IEnumerable<ProductDto> query = Products;

            if (!string.IsNullOrWhiteSpace(SearchKey))
            {
                query = query.Where(x =>
                    x.name?.ToLower().Contains(SearchKey.ToLower()) == true);
            }

            FilteredProducts.Clear();

            foreach (var item in query)
            {
                FilteredProducts.Add(item);
            }
        }

        [RelayCommand]
        private void Refresh()
        {
            _ = FetchProductsAsync();
        }


        [RelayCommand]
        private void OpenAdd()
        {
            DialogTitle = "Thêm sản phẩm";

            ProductForm = new ProductDto();

            IsDialogOpen = true;
        }

        [RelayCommand]
        private void OpenEdit(ProductDto product)
        {
            DialogTitle = "Cập nhật sản phẩm";

            ProductForm = new ProductDto
            {
                id = product.id,
                name = product.name,
                category_id = product.category_id,
                price = product.price,
                quantity = product.quantity,
                description = product.description,
            };

            IsDialogOpen = true;
        }

        [RelayCommand]
        private void CloseDialog()
        {
            IsDialogOpen = false;
        }

        [RelayCommand]
        private async Task SubmitProductAsync()
        {
            var form = new MultipartFormDataContent();

            if (ProductForm.id != null)
            {
                form.Add(new StringContent(ProductForm.id.ToString()), "id");
            }

            form.Add(new StringContent(ProductForm.name ?? ""), "name");
            form.Add(new StringContent(ProductForm.category_id.ToString()), "category_id");
            form.Add(new StringContent(ProductForm.description ?? ""), "description");
            form.Add(new StringContent(ProductForm.price.ToString()), "price");
            form.Add(new StringContent(ProductForm.quantity.ToString()), "quantity");

            MessageBox.Show($"Submitting: {ProductForm.name}, Category ID: {ProductForm.category_id}, Price: {ProductForm.price}, Quantity: {ProductForm.quantity}");

            await ApiManager
                .GetInstance()
                .HttpPostNoDataAsync("update-or-create-product", ProductForm);

            IsDialogOpen = false;

            await FetchProductsAsync();
        }

        [RelayCommand]
        private async Task DeleteProduct(int id)
        {
            var item = Products.FirstOrDefault(x => x.id == id);

            if (item != null)
            {
                Products.Remove(item);
            }

            await ApiManager.GetInstance().HttpGetNoDataAsync($"delete-product?product_id={id}");

            ApplyFilter();
        }

        private string GetCategoryTitle(int? id)
        {
            var c = Categories.FirstOrDefault(x => x.id == id);
            return c?.title ?? "---";
        }
    }
}
