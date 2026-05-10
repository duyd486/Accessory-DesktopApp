using Accessory_DesktopApp.Models;
using Accessory_DesktopApp.Models.Response;
using Accessory_DesktopApp.Singletons;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace Accessory_DesktopApp.ViewModels
{
    public partial class ChannelViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Channel> channels = new();

        public ChannelViewModel()
        {
            _= FetchChannelAsync();
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

                if (response.channels == null || response.channels.Count == 0)
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
    }
}
