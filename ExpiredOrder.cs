using System;
using System.Collections.Generic;
using System.Text;

namespace NullMarketManager
{
    class ExpiredOrder
    {

        public long FirstSeen { get; set; }
        public long LastSeen { get; set; }
        public long TimeOnMarket { get; set; }
        public string Item { get; set; }
        public double TotalProfit { get; set; }
        public int type_id { get; set; }
        public long ExportQuantity { get; set; }

        public void SerializeToDisk()
        {
            TimeOnMarket = LastSeen - FirstSeen;
            DiskManager.SerializeExpiredOrderToDisk(this);
        }

    }
}
