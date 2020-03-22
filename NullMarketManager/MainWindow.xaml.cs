using NullMarketManager.Access;
using NullMarketManager.Models;
using NullMarketManager.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NullMarketManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Timer authTimer;
        AccessManager accessManager = null;


        public MainWindow()
        {
            InitializeComponent();

            SetupAuthTimer();
        }

        private void CheckAuth(object source, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                AuthLabel.Content = "Authenticating...";
                StatusIndicator.Fill = new SolidColorBrush(Colors.Yellow);
            });

            if (accessManager == null)
            {
                accessManager = new AccessManager();
            }

            AuthResult authResult = accessManager.CheckAuth();

            string label = "Unauthenticated";
            Color color = Colors.Red;

            if (authResult.state == AccessManager.AccessState.WAIT_SLEEP)
            {
                label = "Authenticated";
                authTimer.Interval = authResult.expiry > 0 ? authResult.expiry : 50000;
                authTimer.Enabled = true;
                color = Colors.Green;
            } else
            {
                label = "Failed to Authenticate";
                //TODO when to try again?
                //authTimer.Interval = authResult.expiry;
                //authTimer.Enabled = true;
            }

            Dispatcher.Invoke(() =>
            {
                AuthLabel.Content = label;
                StatusIndicator.Fill = new SolidColorBrush(color);
            });
        }

        private void SetupAuthTimer()
        {
            authTimer = new Timer(1000);

            authTimer.Elapsed += new ElapsedEventHandler(CheckAuth);
            authTimer.AutoReset = false;
            authTimer.Enabled = true;
        }

        private void MarketTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void AddTabButton_Click(object sender, RoutedEventArgs e)
        {

            NewTabDialog dialog = new NewTabDialog();
            bool? result = dialog.ShowDialog();
            
            if (result == true)
            {

                var tab = new TabItem();
                tab.Header = "Export";
                tab.Content = new TabViewControl("Export");

                var count = MarketTabs.Items.Count;
                MarketTabs.Items.Add(tab);
                MarketTabs.SelectedIndex = count > -1 ? count : 0;

            }
            
        }
    }
}
