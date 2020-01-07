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

    }
}
