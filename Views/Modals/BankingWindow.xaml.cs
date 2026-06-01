using Accessory_DesktopApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Accessory_DesktopApp.Views.Modals
{
    /// <summary>
    /// Interaction logic for BankingWindow.xaml
    /// </summary>
    public partial class BankingWindow : Window
    {
        private readonly StaffViewModel _staffViewModel;

        public BankingWindow(StaffViewModel dataContext)
        {
            InitializeComponent();
            _staffViewModel = dataContext;
            DataContext = dataContext;

            Loaded += BankingWindow_Loaded;
            Closed += BankingWindow_Closed;
            _staffViewModel.PaymentPaid += StaffViewModel_PaymentPaid;
        }

        private void BankingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _staffViewModel.StartCheckPaymentStatusPolling();
        }

        private void BankingWindow_Closed(object? sender, EventArgs e)
        {
            _staffViewModel.PaymentPaid -= StaffViewModel_PaymentPaid;
            _staffViewModel.StopCheckPaymentStatusPolling();
        }

        private void StaffViewModel_PaymentPaid(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (IsVisible)
                {
                    Close();
                    MessageBox.Show("Chuyển khoản thành công");
                }
            });
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
