using System;
using System.Collections.Generic;
using GatheringTimer.Data.Model.Vo.XIVApiVo;

namespace GatheringTimer.Data.Update.Config
{
    public static class XIVApiConfig
    {
        public static Dictionary<String, String> dic = new Dictionary<String, String>();

        private static readonly string XIV_API_URL = "https://xivapi.com";

        public static Dictionary<String, String> ConfigInitialization()
        {
            DataBase();
            XivApiUrl();
            return dic;
        }

        private static Dictionary<String, String> DataBase()
        {
            dic.Add("Path", "./");
            dic.Add("Filename", "XIVApiCache.db");
            return dic;
        }

        private static Dictionary<String, String> XivApiUrl()
        {
            dic.Add("Item", GetXIVApiEntityUrl<Item>(XIV_API_URL, 3000));
            dic.Add("GatheringItem", GetXIVApiEntityUrl<GatheringItem>(XIV_API_URL, 3000));
            dic.Add("SpearfishingItem", GetXIVApiEntityUrl<SpearfishingItem>(XIV_API_URL, 3000));
            dic.Add("GatheringPointBase", GetXIVApiEntityUrl<GatheringPointBase>(XIV_API_URL, 3000));
            dic.Add("GatheringPoint", GetXIVApiEntityUrl<GatheringPoint>(XIV_API_URL, 3000));
            dic.Add("Map", GetXIVApiEntityUrl<Map>(XIV_API_URL, 3000));
            dic.Add("PlaceName", GetXIVApiEntityUrl<PlaceName>(XIV_API_URL, 3000));
            dic.Add("TerritoryType", GetXIVApiEntityUrl<TerritoryType>(XIV_API_URL, 3000));
            return dic;
        }

        private static string GetXIVApiEntityUrl<T>(string serverUrl, int? limit)
        {

            Type type = typeof(T);
            string url = serverUrl + "/" + type.Name;
            if (null != limit)
            {
                url += "?limit=" + limit;
            }
            if (url.Contains("?") || url.Contains("&"))
            {
                if (url.EndsWith("?") || url.EndsWith("&"))
                {
                    url += "columns=";
                }
                else
                {
                    url += "&columns=";
                }
            }
            else
            {
                url += "?columns=";
            }

            var modelPropertyInfos = type.GetProperties();
            foreach (var propertyInfo in modelPropertyInfos)
            {
                var name = propertyInfo.Name;
                url += name;
                url += ",";
            }
            if (modelPropertyInfos.Length > 0)
            {
                url = url.Substring(0, url.Length - 1);
            }
            return url;
        }


    }
}