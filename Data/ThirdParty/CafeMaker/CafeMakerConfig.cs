using System;
using System.Collections.Generic;
using GatheringTimer.Data.Model.Vo.CafeMakerVo;

namespace GatheringTimer.Data.Update.Config
{
    public static class CafeMakerConfig
    {
        public static Dictionary<String, String> dic = new Dictionary<String, String>();

        private static readonly string CAFE_MAKER_URL = "https://cafemaker.wakingsands.com";

        public static Dictionary<String, String> ConfigInitialization()
        {
            DataBase();
            CafeMakerUrl();
            return dic;
        }

        private static Dictionary<String, String> DataBase()
        {
            dic.Add("Path", "./");
            dic.Add("Filename", "CafeMakerCache.db");
            return dic;
        }

        private static Dictionary<String, String> CafeMakerUrl()
        {
            dic.Add("Item", GetCafeMakerEntityUrl<Item>(CAFE_MAKER_URL, 3000));
            return dic;
        }

        private static string GetCafeMakerEntityUrl<T>(string serverUrl, int? limit)
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