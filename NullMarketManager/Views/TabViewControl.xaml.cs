using NullMarketManager.Export;
using NullMarketManager.Models;
using System;
using System.Collections.Generic;
using System.Text;
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

namespace NullMarketManager.Views
{
    /// <summary>
    /// Interaction logic for TabViewControl.xaml
    /// </summary>
    public partial class TabViewControl : UserControl
    {
        Timer exportManagerTimer;
        string tabType;
        string originStation;
        string destinationStation;
        bool startStopToggle;
        ExportManager exportManager = null;

        public TabViewControl(string tabType)
        {
            InitializeComponent();

            startStopToggle = false;

            this.tabType = tabType;

            originStation = "1DQ";
            destinationStation = "Jita";

            

            OriginLabel.Content = "Origin: " + originStation;
            DestinationLabel.Content = "Destination: " + destinationStation;
            TabHeader.Content = tabType;

            SetupExportTimer();


        }

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {

            startStopToggle = !startStopToggle;

            StartStopButton.IsEnabled = false;

            if (exportManagerTimer != null)
            {
                exportManagerTimer.Enabled = startStopToggle;
            }

            /*ExportManager exportManager1 = new ExportManager(originStation, destinationStation);
            Thread exportManagerThread1 = new Thread(new ThreadStart(exportManager1.RunExportManager));
            exportManagerThread1.Start();*/

        }

        private void SetupExportTimer()
        {
            exportManagerTimer = new Timer(1000);

            exportManagerTimer.Elapsed += new ElapsedEventHandler(CalculateOrder);
            exportManagerTimer.AutoReset = true;
        }

        private void CalculateOrder(object source, ElapsedEventArgs e)
        {
            exportManagerTimer.Interval = 1000 * 60 * 15;

            if (exportManager == null)
            {
                exportManager = new ExportManager(originStation, destinationStation);
            }

            var results = exportManager.CalculateExportResults();

            if (results.didExecute)
            {
                Dispatcher.Invoke(() =>
                {
                    TestDataGrid.ItemsSource = results.orderResults;
                    TestDataGrid.Visibility = Visibility.Visible;
                });
            }

           
        }
    }
}
