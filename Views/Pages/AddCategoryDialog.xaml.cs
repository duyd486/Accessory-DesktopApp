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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Accessory_DesktopApp.Models;

namespace Accessory_DesktopApp.Views 
{
    public partial class AddCategoryDialog : Window
    {
        public string title { get; private set; }  
        public int parent_id { get; private set; }    
        public string thumbnail_url { get; private set; }
        public AddCategoryDialog(CategoryItem item = null)
        {
            InitializeComponent();
            if (item != null)
            {
                NameTextBox.Text = item.title;
                ImagePathTextBox.Text = item.thumbnail_url;
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            title = NameTextBox.Text;
            int temp;
            int.TryParse(ParentTextBox.Text, out temp);
            parent_id = temp;
            thumbnail_url = ImagePathTextBox.Text;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void PickImageButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp";
            if (dialog.ShowDialog() == true)
            {
                ImagePathTextBox.Text = dialog.FileName;
            }
        }
    }
}