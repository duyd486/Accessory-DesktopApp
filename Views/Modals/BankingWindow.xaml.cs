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
        public BankingWindow(StaffViewModel dataContext)
        {
            InitializeComponent();
            DataContext = dataContext;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
