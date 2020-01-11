using System;
using System.Collections.Generic;
using System.Text;

namespace NullMarketManager
{
    // This class exists solely to store station/region ID pairings and return them when requested.
    class LocationData
    {
        public static Tuple<long, long> GetLocationIDByName(string stationName)
        {
            // The format for NPC stations is Structure ID, Region ID
            // The format for player stations is Structure ID, -1
            switch (stationName)
            {                
                case "Jita": return new Tuple<long, long>(60003760, 10000002);
                case "1DQ": return new Tuple<long, long>(1030049082711, -1);
                default: Console.WriteLine("GetLocationIDByName: Invalid Station Name Provided"); return new Tuple<long, long>(-1, -1);
            }
        }


    }
}
