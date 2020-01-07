using System;
using System.Net;
using RestSharp;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace NullMarketManager
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            MarketOrder marketOrder1 = new MarketOrder();
            MarketOrder marketOrder2 = new MarketOrder();
            MarketOrder marketOrder3 = new MarketOrder();

            marketOrder1.price = 10002.39;
            marketOrder2.price = 10002.38;
            marketOrder3.price = 10002.13;
            marketOrder1.volume_remain = 7967;
            marketOrder2.volume_remain = 87225;
            marketOrder3.volume_remain = 30770;

            marketOrder1.is_buy_order = true;
            marketOrder2.is_buy_order = true;
            marketOrder3.is_buy_order = true;

            var testList = new List<MarketOrder>();
            testList.Add(marketOrder1);
            testList.Add(marketOrder2);
            testList.Add(marketOrder3);
            var test = MarketOrderManager.GetBestPriceMapping(testList, true);*/


            string authCode = AuthorizationManager.ReceiveAccess(new[] { "http://localhost/oauth-callback/" });
            
            RequestManager.AuthInfo authInfo = RequestManager.GetAccessToken(authCode);
            RequestManager.CharacterInfo charInfo = RequestManager.GetCharacterInfo(authInfo);
            List<MarketOrder> onedqAllOrders = RequestManager.GetMarketOrdersByPlayerStation(authInfo, Constants.DQStructureID);
            List<MarketOrder> jitaAllOrders = RequestManager.GetMarketOrdersByNPCStation(authInfo, Constants.TheForgeID, Constants.JitaStructureID);

            // Grab Sell Orders for 1DQ & Buy Orders for Jita
            var onedqBestSellOrders = MarketOrderManager.GetBestPriceMapping(onedqAllOrders, false);
            var jitaBestBuyOrders = MarketOrderManager.GetBestPriceMapping(jitaAllOrders, true);

            MarketOrder armorPlates = jitaBestBuyOrders[25605];

            // Calculate items to export based on the prices
            var exportItemsUnadjusted = MarketOrderManager.CalculateExportItems(onedqBestSellOrders, jitaBestBuyOrders);

            // Grab quantity within a specified percentage margin of the best price (e.g. we want to include prices that are 0.01 away to understand true supply/demand)

            var exportItems = MarketOrderManager.CorrectQuantityAvailable(exportItemsUnadjusted);

            // Grab all of the export items information
            var itemDictionary = DiskManager.DeserializeTypesFromDisk();
            bool saveToDisk = false;
            foreach ( var item in exportItems)
            {
                // If we don't have the item information saved to disk, fetch it.
                if (!itemDictionary.ContainsKey(item.type_id))
                {
                    var itemInfo = RequestManager.GetTypeInfo(authInfo, item.type_id);
                    itemDictionary.Add(item.type_id, itemInfo);
                    saveToDisk = true;
                }                
            }

            if (saveToDisk)
            {
                DiskManager.SerializeTypesToDisk(itemDictionary);
            }


            // Run Shipping Cost Calculations on all Export Items
            var finalResults = MarketOrderManager.CalculateProfitWithShippingCost(exportItems, itemDictionary);
            finalResults.Sort(delegate (MarketOrder c1, MarketOrder c2) { return c1.profit.CompareTo(c2.profit); });

            foreach ( var item in finalResults)
            {
                Console.WriteLine(itemDictionary[item.type_id].name + " - Profit: " + (item.profit / 1000000) + "mil - Item Volume: " + itemDictionary[item.type_id].packaged_volume + " - Export Quantity: " + item.exportQuantity);
            }


        }
    }
}
