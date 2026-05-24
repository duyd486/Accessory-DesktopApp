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

                Categories = new ObservableCollection<CategoryItem>(result.categories);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Categories = new ObservableCollection<CategoryItem>();
            }
        }

        [RelayCommand]
        private async void Add()
        {
            var dialog = new AddCategoryDialog(); // KHÔNG truyền parent list
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                await ApiManager.GetInstance().HttpPostNoDataAsync("update-or-create-cate", new
                {
                    id = 0,
                    title = dialog.title,
                    parent_id = 0, // CHA
                    thumbnail_url = dialog.thumbnail_url
                });

                LoadCategories();
            }
        }

        [RelayCommand]
        private async void AddChild(CategoryItem parent)
        {
            var dialog = new AddCategoryDialog(parent, Categories.ToList());
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                await ApiManager.GetInstance().HttpPostNoDataAsync("update-or-create-cate", new
                {
                    id = 0,
                    title = dialog.title,
                    parent_id = parent.id,
                    thumbnail_url = dialog.thumbnail_url
                });

                LoadCategories();
            }
        }

        [RelayCommand]
        private async Task Edit(CategoryItem item)
        {
            var dialog = new AddCategoryDialog(item, Categories.ToList());
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