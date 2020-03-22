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

namespace NullMarketManager.Views
{
    /// <summary>
    /// Interaction logic for NewTabDialog.xaml
    /// </summary>
    public partial class NewTabDialog : Window
    {

        public string selection = "";
        public NewTabDialog()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            selection = TabTypeSelect.Text;
            this.DialogResult = true;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            okButton.IsEnabled = true;
        }
    }
}
