using Accessory_DesktopApp.Models;
using Accessory_DesktopApp.Singletons;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Accessory_DesktopApp.ViewModels
{
    public partial class CategoryViewModel : ObservableObject
    {

        [ObservableProperty]
        private ObservableCollection<CategoryItem> categories = new();

        [ObservableProperty]
        private ObservableCollection<CategoryItem> filteredCategories = new();

        [ObservableProperty]
        private string? searchText;

        [ObservableProperty]
        private CategoryUpsertDto categoryForm = new();

        [ObservableProperty]
        private BitmapImage? thumbnailPreview;

        [ObservableProperty]
        private bool isDialogOpen;

        [ObservableProperty]
        private string dialogTitle = "Thêm danh mục";

        [ObservableProperty]
        private bool isParentCategory;

        [ObservableProperty]
        private ObservableCollection<CategoryItem> parentCandidates = new();

        public CategoryViewModel()
        {
            _ = LoadCategoriesAsync();
        }

        public bool IsChildCategory => !IsParentCategory;

        partial void OnSearchTextChanged(string? value)
        {
            ApplyFilter();
        }

        partial void OnIsParentCategoryChanged(bool value)
        {
            OnPropertyChanged(nameof(IsChildCategory));

            if (value)
            {
                CategoryForm.parent_id = 0;
            }
        }

        [RelayCommand]
        private void Search()
        {
            ApplyFilter();
        }

        [RelayCommand]
        private void Refresh()
        {
            _ = LoadCategoriesAsync();
        }

        [RelayCommand]
        private void OpenAdd()
        {
            DialogTitle = "Thêm danh mục";

            CategoryForm = new CategoryUpsertDto
            {
                parent_id = 0
            };

            IsParentCategory = true;
            ThumbnailPreview = null;

            BuildParentCandidates(excludeId: null);

            IsDialogOpen = true;
        }

        [RelayCommand]
        private void OpenEdit(CategoryItem item)
        {
            DialogTitle = "Cập nhật danh mục";

            CategoryForm = new CategoryUpsertDto
            {
                id = item.id,
                title = item.title,
                parent_id = item.parent_id,
                thumbnail_path = null
            };

            IsParentCategory = item.parent_id == 0;

            // preview ảnh hiện tại
            ThumbnailPreview = null;
            var absUrl = ApiManager.GetInstance().ToAbsoluteUrl(item.thumbnail_url);
            if (!string.IsNullOrWhiteSpace(absUrl))
            {
                ThumbnailPreview = new BitmapImage();
                ThumbnailPreview.BeginInit();
                ThumbnailPreview.UriSource = new Uri(absUrl, UriKind.Absolute);
                ThumbnailPreview.CacheOption = BitmapCacheOption.OnLoad;
                ThumbnailPreview.EndInit();
            }

            BuildParentCandidates(excludeId: item.id);

            IsDialogOpen = true;
        }

        [RelayCommand]
        private void CloseDialog()
        {
            IsDialogOpen = false;
        }

        [RelayCommand]
        private void PickImage()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg"
            };

            if (dialog.ShowDialog() == true)
            {
                CategoryForm.thumbnail_path = dialog.FileName;
                ThumbnailPreview = new BitmapImage(new Uri(dialog.FileName));
            }
        }

        [RelayCommand]
        private async Task SubmitCategoryAsync()
        {
            try
            {
                var form = new MultipartFormDataContent();

                if (CategoryForm.id != null)
                {
                    form.Add(new StringContent(CategoryForm.id.ToString()!), "id");
                }

                form.Add(new StringContent(CategoryForm.title ?? ""), "title");

                var parentId = IsParentCategory ? 0 : CategoryForm.parent_id;
                form.Add(new StringContent(parentId.ToString()), "parentid");

                if (!string.IsNullOrWhiteSpace(CategoryForm.thumbnail_path))
                {
                    var bytes = await File.ReadAllBytesAsync(CategoryForm.thumbnail_path);
                    var fileContent = new ByteArrayContent(bytes);
                    fileContent.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");

                    form.Add(fileContent, "thumbnail", "image.png");
                }

                var response = await ApiManager
                    .GetInstance()
                    .HttpPostFormAsync<object>("update-or-create-cate", form);

                IsDialogOpen = false;

                if (response == null)
                {
                    MessageBox.Show("Lỗi khi lưu danh mục");
                    return;
                }

                MessageBox.Show("Lưu danh mục thành công");

                await LoadCategoriesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        [RelayCommand]
        private async Task Delete(int id)
        {
            try
            {
                await ApiManager.GetInstance().HttpGetNoDataAsync($"delete-cate?category_id={id}");
                await LoadCategoriesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var result = await ApiManager
                    .GetInstance()
                    .HttpGetAsync<CategoryResponse>("list-category")
                    .ConfigureAwait(false);

                App.Current.Dispatcher.Invoke(() =>
                {
                    Categories.Clear();

                    if (result?.categories != null)
                    {
                        foreach (var item in result.categories)
                        {
                            // normalize thumbnail url
                            item.thumbnail_url = ApiManager.GetInstance().ToAbsoluteUrl(item.thumbnail_url) ?? item.thumbnail_url;
                            NormalizeChildren(item);
                            Categories.Add(item);
                        }
                    }

                    ApplyFilter();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                Categories = new ObservableCollection<CategoryItem>();
                FilteredCategories = new ObservableCollection<CategoryItem>();
            }
        }

        private static void NormalizeChildren(CategoryItem item)
        {
            if (item.children == null)
                return;

            foreach (var child in item.children)
            {
                child.thumbnail_url = ApiManager.GetInstance().ToAbsoluteUrl(child.thumbnail_url) ?? child.thumbnail_url;
                NormalizeChildren(child);
            }
        }

        private void ApplyFilter()
        {
            if (Categories == null)
                return;

            var term = SearchText?.Trim();

            FilteredCategories.Clear();

            foreach (var item in Categories)
            {
                var filtered = FilterTree(item, term);
                if (filtered != null)
                {
                    FilteredCategories.Add(filtered);
                }
            }
        }

        private static CategoryItem? FilterTree(CategoryItem item, string? term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return item;

            var isMatch = item.title?.Contains(term, StringComparison.CurrentCultureIgnoreCase) == true;

            List<CategoryItem>? filteredChildren = null;

            if (item.children != null)
            {
                foreach (var child in item.children)
                {
                    var c = FilterTree(child, term);
                    if (c != null)
                    {
                        filteredChildren ??= new List<CategoryItem>();
                        filteredChildren.Add(c);
                    }
                }
            }

            if (!isMatch && (filteredChildren == null || filteredChildren.Count == 0))
                return null;

            return new CategoryItem
            {
                id = item.id,
                title = item.title,
                parent_id = item.parent_id,
                thumbnail_url = item.thumbnail_url,
                children = filteredChildren
            };
        }

        private void BuildParentCandidates(int? excludeId)
        {
            ParentCandidates.Clear();

            foreach (var item in Flatten(Categories))
            {
                if (excludeId != null && item.id == excludeId)
                    continue;

                if(item.parent_id != 0)
                    continue;

                ParentCandidates.Add(item);
            }
        }

        private static IEnumerable<CategoryItem> Flatten(IEnumerable<CategoryItem> source)
        {
            foreach (var item in source)
            {
                yield return item;

                if (item.children == null)
                    continue;

                foreach (var child in Flatten(item.children))
                {
                    yield return child;
                }
            }
        }
    }
}