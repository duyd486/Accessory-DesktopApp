using Accessory_DesktopApp.Models;
using Accessory_DesktopApp.Singletons;
using Accessory_DesktopApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private async void LoadCategories()
        {
            try
            {
                var result = await ApiManager.GetInstance().HttpGetAsync<CategoryResponse>("list-category");

                var allItems = new List<CategoryItem>();
                foreach (var item in result.categories)
                {
                    allItems.Add(item);
                    if (item.children != null)
                        allItems.AddRange(item.children);
                }

                Categories = new ObservableCollection<CategoryItem>(allItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Categories = new ObservableCollection<CategoryItem>();
            }
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
                    id = Categories.Count + 1,
                    title = dialog.title,
                    parent_id = 0,
                    thumbnail_url = dialog.thumbnail_url
                });
            }
        }

        [RelayCommand]
        private async Task Edit(CategoryItem item)
        {
            var dialog = new AddCategoryDialog(item);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    await ApiManager.GetInstance().HttpPostNoDataAsync("update-or-create-cate", new
                    {
                        id = item.id,
                        title = dialog.title,
                        parent_id = dialog.parent_id,
                        thumbnail_url = dialog.thumbnail_url
                    });

                    LoadCategories();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        [RelayCommand]
        private async Task Delete(int id)
        {
            try
            {
                await ApiManager.GetInstance().HttpGetNoDataAsync($"delete-cate?category_id={id}");
                LoadCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}