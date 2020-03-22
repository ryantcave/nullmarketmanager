using NullMarketManager.Access;
using NullMarketManager.IO;
using NullMarketManager.Models;
using NullMarketManager.Request;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NullMarketManager.Export
{
    class ExportManager : IExportManager
    {
        enum ExportState { WAIT_AUTH, GET_ORDERS, FILTER_ORDERS, CALCULATE_EXPORTS, GET_TYPEINFO, PRINT_RESULTS, WAIT_TIMER }
        ExportState m_eExportState;
        string m_strOriginStation;
        string m_strDestinationStation;
        List<MarketOrder> m_vOriginStationOrders;
        List<MarketOrder> m_vDestinationStationOrders;
        Dictionary<long, MarketOrder> m_dicFilteredOriginOrders;
        Dictionary<long, MarketOrder> m_dicFilteredDestinationOrders;
        List<MarketOrder> m_vExportItems;
        List<long> m_vCurrentResultIDs;
        List<long> m_vPreviousResultIDs;
        Dictionary<long, TypeInfo> m_dicItemDictionary;
        List<OrderResult> orderResults;

        public ExportManager(string originStation, string destinationStation)
        {
            m_eExportState = ExportState.WAIT_AUTH;
            m_strOriginStation = originStation;
            m_strDestinationStation = destinationStation;
            m_vCurrentResultIDs = new List<long>();
            m_vPreviousResultIDs = new List<long>();
            orderResults = new List<OrderResult>();
        }

        public void RunExportManager()
        {
            while (true)
            {
                ExportManagerStateMachineDoWork();
            }
        }

        public ExportResult CalculateExportResults()
        {

            if (m_eExportState == ExportState.WAIT_AUTH)
            {
                ExportStateWaitAuth();
            }

            if (m_eExportState == ExportState.GET_ORDERS)
            {
                ExportStateGetOrders();
            }

            if (m_eExportState == ExportState.FILTER_ORDERS)
            {
                ExportStateFilterOrders();
            }

            if (m_eExportState == ExportState.CALCULATE_EXPORTS)
            {
                ExportStateCalculateExports();
            }

            if (m_eExportState == ExportState.GET_TYPEINFO)
            {
                ExportStateGetTypeInfo();
            }

            if (m_eExportState == ExportState.PRINT_RESULTS)
            {
                ExportStatePrintResults();
            }

            if (m_eExportState == ExportState.WAIT_TIMER)
            {
                int sleepTime = 1000 * 60 * 15;

                Console.WriteLine("Renewing in " + sleepTime);

                return new ExportResult(orderResults, true);
            }

            return new ExportResult(null,false);
        }

        private void ExportManagerStateMachineDoWork()
        {
            switch (m_eExportState)
            {
                case ExportState.WAIT_AUTH:
                    ExportStateWaitAuth();
                    break;
                case ExportState.GET_ORDERS:
                    ExportStateGetOrders();
                    break;
                case ExportState.FILTER_ORDERS:
                    ExportStateFilterOrders();
                    break;
                case ExportState.CALCULATE_EXPORTS:
                    ExportStateCalculateExports();
                    break;
                case ExportState.GET_TYPEINFO:
                    ExportStateGetTypeInfo();
                    break;
                case ExportState.PRINT_RESULTS:
                    ExportStatePrintResults();
                    break;
                case ExportState.WAIT_TIMER:
                    // wait 15 minutes before running again
                    Console.WriteLine(m_strOriginStation + " -> " + m_strDestinationStation + ": Thread Going To Sleep.");
                    Thread.Sleep(1000 * 60 * 15);
                    m_eExportState = ExportState.WAIT_AUTH;
                    break;
                default:
                    Console.WriteLine("ExportManager: Unknown state.");
                    break;
            }
        }

        private void ExportStateWaitAuth()
        {
            while (AccessManager.accessCodeIsValid == false)
            {
                Thread.Sleep(2000);
            }

            m_eExportState = ExportState.GET_ORDERS;
        }

        private void ExportStateGetOrders()
        {
            var originStationData = LocationData.GetLocationIDByName(m_strOriginStation);
            var destinationStationData = LocationData.GetLocationIDByName(m_strDestinationStation);

            // Get origin station orders
            // If the region id is -1 we know it's a player station
            if (originStationData.Item2 == -1)
            {
                m_vOriginStationOrders = RequestManager.GetMarketOrdersByPlayerStation(AccessManager.authInfo, originStationData.Item1);
            }
            else
            {
                m_vOriginStationOrders = RequestManager.GetMarketOrdersByNPCStation(AccessManager.authInfo, originStationData.Item1, originStationData.Item2, false);
            }

            // Get destination station orders

            if (destinationStationData.Item2 == -1)
            {
                m_vDestinationStationOrders = RequestManager.GetMarketOrdersByPlayerStation(AccessManager.authInfo, destinationStationData.Item1);
            }
            else
            {
                m_vDestinationStationOrders = RequestManager.GetMarketOrdersByNPCStation(AccessManager.authInfo, destinationStationData.Item1, destinationStationData.Item2, true);
            }

            m_eExportState = ExportState.FILTER_ORDERS;

        }

        private void ExportStateFilterOrders()
        {
            m_dicFilteredOriginOrders = MarketOrderManager.GetBestPriceMapping(m_vOriginStationOrders, false);
            m_dicFilteredDestinationOrders = MarketOrderManager.GetBestPriceMapping(m_vDestinationStationOrders, true);

            m_eExportState = ExportState.CALCULATE_EXPORTS;

            // free up memory
            m_vOriginStationOrders.Clear();
            m_vDestinationStationOrders.Clear();
        }

        private void ExportStateCalculateExports()
        {
            var exportItemsUnadjusted = MarketOrderManager.CalculateExportItems(m_dicFilteredOriginOrders, m_dicFilteredDestinationOrders);
            m_vExportItems = MarketOrderManager.CorrectQuantityAvailable(exportItemsUnadjusted);
            m_eExportState = ExportState.GET_TYPEINFO;
        }

        private void ExportStateGetTypeInfo()
        {
            m_dicItemDictionary = DiskManager.DeserializeTypesFromDisk();
            bool saveToDisk = false;

            foreach (var item in m_vExportItems)
            {
                // If we don't have the item information saved to disk, fetch it.
                if (!m_dicItemDictionary.ContainsKey(item.type_id))
                {
                    var itemInfo = RequestManager.GetTypeInfo(AccessManager.authInfo, item.type_id);
                    m_dicItemDictionary.Add(item.type_id, itemInfo);
                    saveToDisk = true;
                }
            }

            if (saveToDisk)
            {
                DiskManager.SerializeTypesToDisk(m_dicItemDictionary);
            }

            m_eExportState = ExportState.PRINT_RESULTS;
        }

        private void ExportStatePrintResults()
        {
            var finalResults = MarketOrderManager.CalculateProfitWithShippingCost(m_vExportItems, m_dicItemDictionary);
            finalResults.Sort(delegate (MarketOrder c1, MarketOrder c2) { return c1.profit.CompareTo(c2.profit); });
            string header = m_strOriginStation + " -> " + m_strDestinationStation + ": ";
            m_vCurrentResultIDs.Clear();

            if (finalResults.Count > 0)
            {
                orderResults.Clear();

                foreach (var item in finalResults)
                {
                    OrderResult orderResult = new OrderResult();
                    orderResult.Name = m_dicItemDictionary[item.type_id].name;
                    orderResult.Profit = Math.Round(item.profit / 1000000, 4);
                    orderResult.Volume = m_dicItemDictionary[item.type_id].packaged_volume;
                    orderResult.ExportQuantity = item.exportQuantity;
                    orderResults.Add(orderResult);

                    m_vCurrentResultIDs.Add(item.type_id);
                    if (m_vPreviousResultIDs.Contains(item.type_id))
                    {
                        Console.WriteLine(header + m_dicItemDictionary[item.type_id].name + " - Profit: " + Math.Round(item.profit / 1000000, 4) + " mil - Item Volume: " + m_dicItemDictionary[item.type_id].packaged_volume + " - Export Quantity: " + item.exportQuantity);
                    }
                    else
                    {
                        string newHeader = "*NEW* " + header;
                        Console.WriteLine(newHeader + m_dicItemDictionary[item.type_id].name + " - Profit: " + Math.Round(item.profit / 1000000, 4) + " mil - Item Volume: " + m_dicItemDictionary[item.type_id].packaged_volume + " - Export Quantity: " + item.exportQuantity);
                        m_vPreviousResultIDs.Add(item.type_id);
                        Console.Beep();
                    }

                }
            } else
            {
                Console.WriteLine("No results.");
            }

            m_vPreviousResultIDs.RemoveAll(item => !m_vCurrentResultIDs.Contains(item));

            m_eExportState = ExportState.WAIT_TIMER;
        }
    }
}
