using Newtonsoft.Json;
using NullMarketManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NullMarketManager.IO
{
    class DiskManager
    {
        public static void SerializeTypesToDisk(Dictionary<long, TypeInfo> typeInfo)
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

        public static Dictionary<long, TypeInfo> DeserializeTypesFromDisk()
        {
            var returnMap = new Dictionary<long, TypeInfo>();
            var typeList = new List<TypeInfo>();

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "typeInfo.json"))
            {
                var input = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "typeInfo.json");
                typeList = JsonConvert.DeserializeObject<List<TypeInfo>>(input);
            }

            foreach (var item in typeList)
            {
                returnMap.Add(item.type_id, item);
            }

            return returnMap;

        }

        public static void SerializeAuthInfoToDisk(AuthInfo authInfo)
        {
            string jsonString = JsonConvert.SerializeObject(authInfo);
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "authInfo.json", jsonString);
        }

        public static bool DeserializeAuthInfoFromDisk(ref AuthInfo authInfo)
        {
            bool bSuccess = false;
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "authInfo.json"))
            {
                var input = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "authInfo.json");
                authInfo = JsonConvert.DeserializeObject<AuthInfo>(input);
                bSuccess = true;
            }

            return bSuccess;
        }
    }
}
