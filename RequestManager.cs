using System;
using System.Collections.Generic;
using System.Text;
using RestSharp;
using Newtonsoft.Json;
using System.Linq;

static class Constants
{
    public const string ClientID = "Insert Your Client ID";
    public const string SecretKey = "Insert Your Secret Key";
}

namespace NullMarketManager
{
    class RequestManager
    {
        public class CharacterInfo
        {
            public string CharacterID { get; set; }
            public string CharacterName { get; set; }
            public string CharacterOwnerHash { get; set; }

        }

        public static CharacterInfo GetCharacterInfo(AccessManager.AuthInfo authInfo)
        {
            var client = new RestClient("https://login.eveonline.com");
            var request = new RestRequest("oauth/verify", Method.GET);

            string header = "Bearer " + authInfo.access_token;
            request.AddHeader("Authorization", header);

            var response = client.Get(request);
            var content = response.Content;

            CharacterInfo characterInfo = JsonConvert.DeserializeObject<CharacterInfo>(content);

            return characterInfo;
        }

        // Only valid for player-owned stations, must use region endpoint for npc stations
        public static List<MarketOrder> GetMarketOrdersByPlayerStation(AccessManager.AuthInfo authInfo, long structureID)
        {
            List<MarketOrder> returnList = new List<MarketOrder>();

            int totalPages = 1;

            for (int i = 0; i < totalPages; i++)
            {
                var client = new RestClient("https://esi.evetech.net");
                var request = new RestRequest("/latest/markets/structures/" + structureID + "/?page=" + (i + 1));

                Console.WriteLine("Getting " + structureID + " Market Orders: Getting Page " + (i + 1));

                string header = "Bearer " + authInfo.access_token;
                request.AddHeader("Authorization", header);

                var response = client.Get(request);
                var content = response.Content;

                if ( response.IsSuccessful == false)
                {
                    i--;
                    continue;
                }

                totalPages = Int32.Parse(response.Headers.ToList().Find(x => x.Name == "X-Pages").Value.ToString());
                

                returnList.AddRange(JsonConvert.DeserializeObject<List<MarketOrder>>(content));

            }

            return returnList;
        }

        // Must get all orders in the region for public stations, then filter out unwanted results.
        public static List<MarketOrder> GetMarketOrdersByNPCStation(AccessManager.AuthInfo authInfo, long structureID, long regionID, bool getBuyOrders)
        {
            List<MarketOrder> returnList = new List<MarketOrder>();

            int totalPages = 1;

            for (int i = 0; i < totalPages; i++)
            {
                var client = new RestClient("https://esi.evetech.net");
                RestRequest request;

                if (getBuyOrders)
                {
                    request = new RestRequest("/latest/markets/" + regionID + "/orders/?page=" + (i + 1) + "&order_type=buy");
                }
                else
                {
                    request = new RestRequest("/latest/markets/" + regionID + "/orders/?page=" + (i + 1) + "&order_type=sell");
                }

                Console.WriteLine("Getting " + regionID + " Region Orders: Getting Page " + (i + 1));

                string header = "Bearer " + authInfo.access_token;
                request.AddHeader("Authorization", header);

                var response = client.Get(request);
                var content = response.Content;

                if ( response.IsSuccessful == false)
                {
                    Console.WriteLine("Error attempting to get market orders by NPC station. Retrying..");
                    i--;
                    continue;
                }

                totalPages = Int32.Parse(response.Headers.ToList().Find(x => x.Name == "X-Pages").Value.ToString());

                var pageList = JsonConvert.DeserializeObject<List<MarketOrder>>(content);

                // Only return items that are in the location we want.

                foreach ( var item in pageList)
                {
                    if ( item.location_id == structureID)
                    {
                        returnList.Add(item);
                    }
                }

            }

            return returnList;
        }

        public class TypeInfo
        {
            public string name { get; set; }
            public long type_id { get; set; }
            public float packaged_volume { get; set; }

        }

        public static TypeInfo GetTypeInfo(AccessManager.AuthInfo authInfo, long type_id)
        {
            var client = new RestClient("https://esi.evetech.net");
            var request = new RestRequest("/latest/universe/types/" + type_id);

            Console.WriteLine("Fetching information on type " + type_id);

            string header = "Bearer " + authInfo.access_token;
            request.AddHeader("Authorization", header);

            var response = client.Get(request);
            var content = response.Content;

            if (response.IsSuccessful == false)
            {
                Console.WriteLine("Failed to acquire type with ID: " + type_id);
                TypeInfo errorType = new TypeInfo();
                errorType.name = response.StatusDescription;
                errorType.type_id = type_id;
                errorType.packaged_volume = 1;
                return errorType;

            }

            return JsonConvert.DeserializeObject<TypeInfo>(content);
        }
    
    
    }
}
