using Accessory_DesktopApp.Models;
using Accessory_DesktopApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace Accessory_DesktopApp.ViewModels
{
    public partial class CategoryViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<CategoryItem> categories;

        [ObservableProperty]
        private string searchText;

        public CategoryViewModel()
        {
            LoadCategories();
        }

        private void LoadCategories()
        {
            categories = new ObservableCollection<CategoryItem>
            {
                new CategoryItem { Id = "#1", Name = "Laptop", ParentName = "---" },
                new CategoryItem { Id = "#6", Name = "⌞ Laptop Gaming", ParentName = "Laptop" },
                new CategoryItem { Id = "#2", Name = "Điện thoại", ParentName = "---" },
            };
        }

        [RelayCommand]
        private void Delete(string id)
        {
            var item = categories.FirstOrDefault(c => c.Id == id);
            if (item != null) categories.Remove(item);
        }

        [RelayCommand]
        private void Add()
        {
          
        }
    }
}
