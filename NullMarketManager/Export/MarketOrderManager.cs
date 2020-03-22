using NullMarketManager.Models;
using NullMarketManager.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace NullMarketManager.Export
{
    class MarketOrderManager
    {
        // Isk cost per 1m^3 of volume
        public const double m_nPricePerM3 = 1150;
        // Minimum percentage margin to consider an item
        public const double m_nDesiredMargin = 0.10;
        // Percentage above or below the best price to consider for market quantity
        public const double m_nQuantityMargin = 0.02;
        // Minimum total profit of items to display
        public const double m_nMinimumTotalProfit = 7500000;

        // Returns a list of only the "top" orders (e.g. the Highest Buy Price or the Lowest Sell Price)
        public static Dictionary<long, MarketOrder> GetBestPriceMapping(List<MarketOrder> marketOrders, bool selectBuyOrders)
        {

            Dictionary<long, MarketOrder> returnMap = new Dictionary<long, MarketOrder>();

            foreach (var order in marketOrders)
            {

                long key = order.type_id;

                // Skip over market orders that aren't the type we selected.
                if (order.is_buy_order && !selectBuyOrders)
                {
                    continue;
                }
                else if (!order.is_buy_order && selectBuyOrders)
                {
                    continue;
                }

                // Check map to see if the market order already exists
                MarketOrder orderInMap;
                if (returnMap.TryGetValue(key, out orderInMap))
                {
                    // If we want buy orders, we want the highest price
                    if (selectBuyOrders)
                    {
                        if (order.price > orderInMap.price)
                        {
                            returnMap[key] = order;
                        }
                    }
                    // If we want sell orders, we want the lowest price
                    else
                    {
                        if (order.price < orderInMap.price)
                        {
                            returnMap[key] = order;
                        }
                    }


                }
                // If the map doesn't have the order, just add it.
                else
                {
                    returnMap.Add(key, order);
                }


            }

            // Once we have our best prices, we want to iterate again to find the quantity within our accepted price range.
            foreach (var item in marketOrders)
            {
                long key = item.type_id;
                double differencePercent;

                if (returnMap.ContainsKey(key))
                {
                    // If it's a buy order, the other prices will all be lower
                    if (selectBuyOrders && item.is_buy_order)
                    {
                        differencePercent = (returnMap[key].price - item.price) / returnMap[key].price;

                        if (differencePercent <= m_nQuantityMargin)
                        {
                            returnMap[key].buyQuantity += item.volume_remain;
                        }
                    }
                    // If it's a sell order, the other prices will all be higher
                    else if (!selectBuyOrders && !item.is_buy_order)
                    {
                        differencePercent = (item.price - returnMap[key].price) / item.price;

                        if (differencePercent <= m_nQuantityMargin)
                        {
                            returnMap[key].sellQuantity += item.volume_remain;
                        }
                    }
                }

            }

            return returnMap;
        }

        // Compares an origin Market Order mapping to a destination mapping to find items with suitable margins
        public static List<MarketOrder> CalculateExportItems(Dictionary<long, MarketOrder> originOrders, Dictionary<long, MarketOrder> destinationOrders)
        {
            var marginList = new List<MarketOrder>();

            // Calculate items with a sufficient margin and add them to the list.

            foreach (var originItem in originOrders)
            {
                long key = originItem.Key;

                // Make sure there are market orders at the desination for the current item.
                if (destinationOrders.ContainsKey(key))
                {
                    var destinationItem = destinationOrders[key];

                    // If the destination price is higher, this is not a valid item for exportation.
                    if (originItem.Value.price > destinationItem.price)
                    {
                        continue;
                    }

                    double margin = 1 - (originItem.Value.price / destinationItem.price);

                    // If we have at least a 15% margin, add the item to the exportation list.
                    if (margin >= m_nDesiredMargin)
                    {
                        // Add margin/profit
                        destinationItem.margin = margin;
                        destinationItem.profit = destinationItem.price - originItem.Value.price;
                        destinationItem.sellPrice = originItem.Value.price;
                        destinationItem.buyPrice = destinationItem.price;
                        destinationItem.sellQuantity = originItem.Value.sellQuantity;
                        marginList.Add(destinationItem);
                    }
                }

            }

            return marginList;

        }

        public static List<MarketOrder> CalculateProfitWithShippingCost(List<MarketOrder> exportItems, Dictionary<long, TypeInfo> itemInfo)
        {
            var returnList = new List<MarketOrder>();

            foreach (var item in exportItems)
            {
                double volume = itemInfo[item.type_id].packaged_volume;

                double profit = (item.profit - (volume * m_nPricePerM3)) * item.exportQuantity;

                if (profit > m_nMinimumTotalProfit)
                {
                    var returnItem = item;
                    item.profit = profit;
                    returnList.Add(returnItem);
                }

            }

            return returnList;
        }

        public static List<MarketOrder> CorrectQuantityAvailable(List<MarketOrder> bestOrders)
        {
            var returnList = new List<MarketOrder>();

            foreach (var regularOrder in bestOrders)
            {
                var returnOrder = regularOrder;

                if (returnOrder.buyQuantity > returnOrder.sellQuantity)
                {
                    returnOrder.exportQuantity = returnOrder.sellQuantity;
                }
                else
                {
                    returnOrder.exportQuantity = returnOrder.buyQuantity;
                }

                returnList.Add(returnOrder);

            }

            return returnList;

        }

    }
}
