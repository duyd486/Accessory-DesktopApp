using Accessory_DesktopApp.Models.Dtos;
using Accessory_DesktopApp.Singletons;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Accessory_DesktopApp.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty]
        private int countCustomers;

        [ObservableProperty]
        private int countProducts;

        [ObservableProperty]
        private int countOrders;

        [ObservableProperty]
        private int countStaff;

        public ObservableCollection<MonthlyRevenueDto> MonthlyRevenue { get; } = new();

        public ObservableCollection<RevenueByYearDto> RevenueByYear { get; } = new();

        public DashboardViewModel()
        {
            _ = LoadAsync();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadAsync().ConfigureAwait(false);
        }

        private async Task LoadAsync()
        {
            await FetchStatisticsAsync().ConfigureAwait(false);
            await FetchMonthlyRevenueAsync().ConfigureAwait(false);
            await FetchRevenueByYearAsync().ConfigureAwait(false);
        }

        private async Task FetchStatisticsAsync()
        {
            var dto = await ApiManager.GetInstance().HttpGetAsync<StatisticsDto>("get-statistics").ConfigureAwait(false);
            if (dto is null)
                return;

            CountCustomers = dto.CountCustomers;
            CountProducts = dto.CountProducts;
            CountOrders = dto.CountOrders;
            CountStaff = dto.CountStaff;
        }

        private async Task FetchMonthlyRevenueAsync()
        {
            var items = await ApiManager.GetInstance().HttpGetAsync<List<MonthlyRevenueDto>>("get-monthly-revenue").ConfigureAwait(false);
            if (items is null)
                return;

            if (App.Current?.Dispatcher?.CheckAccess() == true)
            {
                ReplaceMonthlyRevenue(items);
            }
            else
            {
                App.Current?.Dispatcher?.Invoke(() => ReplaceMonthlyRevenue(items));
            }
        }

        private async Task FetchRevenueByYearAsync()
        {
            var items = await ApiManager.GetInstance().HttpGetAsync<List<RevenueByYearDto>>("get-revenue-by-year").ConfigureAwait(false);
            if (items is null)
                return;

            if (App.Current?.Dispatcher?.CheckAccess() == true)
            {
                ReplaceRevenueByYear(items);
            }
            else
            {
                App.Current?.Dispatcher?.Invoke(() => ReplaceRevenueByYear(items));
            }
        }

        private void ReplaceMonthlyRevenue(IReadOnlyList<MonthlyRevenueDto> items)
        {
            MonthlyRevenue.Clear();
            foreach (var item in items)
                MonthlyRevenue.Add(item);
        }

        private void ReplaceRevenueByYear(IReadOnlyList<RevenueByYearDto> items)
        {
            RevenueByYear.Clear();
            foreach (var item in items)
                RevenueByYear.Add(item);
        }
    }
}
