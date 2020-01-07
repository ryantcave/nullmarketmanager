using System;
using System.Collections.Generic;
using System.Text;

namespace NullMarketManager
{
    public class MarketOrder
    {
        public long location_id { get; set; }
        public double price { get; set; }
        public int volume_remain { get; set; }
        public bool is_buy_order { get; set; }
        public int type_id { get; set; }
        public int duration { get; set; }
        public int volume_total { get; set; }        


        /* Fields Set By Us */

        public double margin { get; set; }
        public double sellPrice { get; set; }
        public double buyPrice { get; set; }
        public double profit { get; set; }
        // The amount of this resource that is available to purchase.
        public int sellQuantity { get; set; }
        // The amount that people are attempting to buy.
        public int buyQuantity { get; set; }

        // The amount we should export.
        public int exportQuantity { get; set; }
    }
}
