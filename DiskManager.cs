using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace NullMarketManager
{
    class DiskManager
    {
        public static void SerializeTypesToDisk(Dictionary<long, RequestManager.TypeInfo> typeInfo)
        {
            StringBuilder builder = new StringBuilder("[");

            foreach (var type in typeInfo)
            {
                builder.Append(JsonConvert.SerializeObject(type.Value));
                builder.Append(",");
                

            }
            builder[builder.Length - 1] = ']';
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "typeInfo.json", builder.ToString());
        }

        public static Dictionary<long, RequestManager.TypeInfo> DeserializeTypesFromDisk()
        {
            var returnMap = new Dictionary<long, RequestManager.TypeInfo>();
            var typeList = new List<RequestManager.TypeInfo>();

            if ( File.Exists(AppDomain.CurrentDomain.BaseDirectory + "typeInfo.json")){
                var input = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "typeInfo.json");
                typeList = JsonConvert.DeserializeObject<List<RequestManager.TypeInfo>>(input);
            }

            foreach (var item in typeList)
            {
                returnMap.Add(item.type_id, item);
            }

            return returnMap;

        }

        public static void SerializeAuthInfoToDisk(AccessManager.AuthInfo authInfo)
        {
            string jsonString = JsonConvert.SerializeObject(authInfo);
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "authInfo.json", jsonString);
        }

        public static bool DeserializeAuthInfoFromDisk(ref AccessManager.AuthInfo authInfo)
        {
            bool bSuccess = false;
            if ( File.Exists(AppDomain.CurrentDomain.BaseDirectory + "authInfo.json"))
            {
                var input = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "authInfo.json");
                authInfo = JsonConvert.DeserializeObject<AccessManager.AuthInfo>(input);
                bSuccess = true;
            }

            return bSuccess;
        }

        public static void SerializeExpiredOrderToDisk(ExpiredOrder order)
        {            
            List<ExpiredOrder> expiredOrders = new List<ExpiredOrder>();

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "expiredOrders.json"))
            {
                string expiredOrdersJson = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "expiredOrders.json");
                expiredOrders = JsonConvert.DeserializeObject<List<ExpiredOrder>>(expiredOrdersJson);
            }

            expiredOrders.Add(order);
            string jsonString = JsonConvert.SerializeObject(expiredOrders);
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "expiredOrders.json", jsonString);
        }
    }
}
