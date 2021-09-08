using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using GatheringTimer.Data.Database;
using GatheringTimer.Util;

namespace GatheringTimer.Data.ThirdParty.XIVApi
{
    public static class XIVApiUpdater
    {
        private static readonly string XIV_API_URL = "https://xivapi.com";

        public static List<Item> ItemCache { get; set; } = default;
        public static List<GatheringItem> GatheringItemCache { get; set; } = default;
        public static List<SpearfishingItem> SpearfishingItemCache { get; set; } = default;
        public static List<GatheringPointBase> GatheringPointBaseCache { get; set; } = default;
        public static List<GatheringPoint> GatheringPointCache { get; set; } = default;
        public static List<PlaceName> PlaceNameCache { get; set; } = default;
        public static List<TerritoryType> TerritoryTypeCache { get; set; } = default;
        public static List<Map> MapCache { get; set; } = default;

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

        /// <summary>
        /// Get all json with async And return json list(XIVApi)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private static async Task<List<String>> RequestXIVApiAsync(String url, String param)
        {
            ServicePointManager.DefaultConnectionLimit = 12;
            String json = await RequestUtil.GetResponseDataAsync(url, param);
            Pagination pagination = (Pagination)await RequestUtil.JsonToObjectAsync<Pagination>(json, "Pagination");
            List<String> jsonList = new List<String>() { json };
            if (pagination.Page == pagination.PageTotal)
            {
                return jsonList;
            }
            Queue<Task> tasks = new Queue<Task>();
            for (int page = 2; page <= pagination.PageTotal; page++)
            {
                tasks.Enqueue(RequestUtil.GetResponseDataAsync(url, param + "&Page=" + page));
            }
            await Task.WhenAll(tasks);
            foreach (Task<String> task in tasks)
            {
                String jsonPart = task.Result;
                jsonList.Add(jsonPart);
            }
            return jsonList;
        }

        /// <summary>
        /// Default update method(XivApi)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private static async Task<List<T>> GetDataList<T>(String url, String param, String key)
        {
            List<String> jsonList = await RequestXIVApiAsync(url, param);
            return await RequestUtil.ParseResultList<T>(jsonList, key);
        }

        private static void IntoCache<T>(List<T> tlist)
        {
            typeof(XIVApiUpdater).GetProperty(typeof(T).Name + "Cache").SetValue(typeof(XIVApiUpdater), tlist);
        }

        private static async Task<bool> GetRawData<T>(String url)
        {

            Stopwatch watch = new Stopwatch();
            watch.Start();
            List<T> dataList = await GetDataList<T>(url, "", "Results");
            IntoCache<T>(dataList);
            Logger.Info("Get XIVApi " + typeof(T).Name + " Data in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
            return true;
        }

        public static async Task<bool> XIVApiDataUpdate()
        {

            try
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                Queue<Task> tasks = new Queue<Task>();
                tasks.Enqueue(GetRawData<Item>(GetXIVApiEntityUrl<Item>(XIV_API_URL, 3000)));
                tasks.Enqueue(GetRawData<GatheringItem>(GetXIVApiEntityUrl<GatheringItem>(XIV_API_URL, 3000)));
                tasks.Enqueue(GetRawData<SpearfishingItem>(GetXIVApiEntityUrl<SpearfishingItem>(XIV_API_URL, 3000)));
                tasks.Enqueue(GetRawData<GatheringPointBase>(GetXIVApiEntityUrl<GatheringPointBase>(XIV_API_URL, 3000)));
                tasks.Enqueue(GetRawData<GatheringPoint>(GetXIVApiEntityUrl<GatheringPoint>(XIV_API_URL, 3000)));
                tasks.Enqueue(GetRawData<Map>(GetXIVApiEntityUrl<Map>(XIV_API_URL, 3000)));
                tasks.Enqueue(GetRawData<PlaceName>(GetXIVApiEntityUrl<PlaceName>(XIV_API_URL, 3000)));
                tasks.Enqueue(GetRawData<TerritoryType>(GetXIVApiEntityUrl<TerritoryType>(XIV_API_URL, 3000)));
                await Task.WhenAll(tasks);
                Logger.Info("Finished XIVApiDataUpdate in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Update XIVApiData Error,Exception:" + ex.Message);
                return false;
            }

        }

        public static void ClearCache()
        {
            ItemCache = default;
            GatheringItemCache = default;
            SpearfishingItemCache = default;
            GatheringPointBaseCache = default;
            GatheringPointCache = default;
            PlaceNameCache = default;
            TerritoryTypeCache = default;
            MapCache = default;
            GC.Collect();
        }

    }
}
