using Accessory_DesktopApp.Models;
using Accessory_DesktopApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Net.Http;
using System.Text.Json;
namespace Accessory_DesktopApp.ViewModels
{
    public partial class CategoryViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private const string BaseUrl = "https://localhost:7210";

        [ObservableProperty]
        private ObservableCollection<CategoryItem> categories;

        [ObservableProperty]
        private string searchText;

        public CategoryViewModel()
        {
            LoadCategories();
        }

        private async void LoadCategories()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{BaseUrl}/api/list-category");
                var result = JsonSerializer.Deserialize<ApiResponse>(response);
                categories = new ObservableCollection<CategoryItem>();
            }
            catch (HttpRequestException)
            {
                categories = new ObservableCollection<CategoryItem>();
            }
        }
        [RelayCommand]
        private async Task Delete(string id)
        {
            await _httpClient.GetAsync($"{BaseUrl}/api/delete-cate?category_id={id}");
        }

        [RelayCommand]
        private void Add()
        {
            var dialog = new AddCategoryDialog();
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                Categories.Add(new CategoryItem
                {
                    Id = $"#{Categories.Count + 1}",
                    Name = dialog.CategoryName,
                    ParentName = string.IsNullOrEmpty(dialog.ParentName) ? "---" : dialog.ParentName,
                    ImagePath = dialog.ImagePath
                });
            }
        }

        [RelayCommand]
        private void Edit(CategoryItem item)
        {
            var dialog = new AddCategoryDialog();
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
        }
    }
}