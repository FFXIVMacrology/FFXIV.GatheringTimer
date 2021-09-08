using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using GatheringTimer.Data.Database;
using GatheringTimer.Util;

namespace GatheringTimer.Data.ThirdParty.CafeMaker
{
    public static class CafeMakerUpdater
    {
        private static readonly string CAFE_MAKER_URL = "https://cafemaker.wakingsands.com";

        public static List<Item> ItemCache { get; set; } = default;
        public static List<SpearfishingItem> SpearfishingItemCache { get; set; } = default;
        public static List<PlaceName> PlaceNameCache { get; set; } = default;

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

        /// <summary>
        /// Get all json with async And return json list(CafeMaker)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private static async Task<List<String>> RequestCafeMakerAsync(String url, String param)
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
        /// Default update method(CafeMaker)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private static async Task<List<T>> GetDataList<T>(String url, String param, String key)
        {
            List<String> jsonList = await RequestCafeMakerAsync(url, param);
            return await RequestUtil.ParseResultList<T>(jsonList, key);
        }

        private static void IntoCache<T>(List<T> tlist)
        {
            typeof(CafeMakerUpdater).GetProperty(typeof(T).Name + "Cache").SetValue(typeof(CafeMakerUpdater), tlist);
        }

        private static async Task<bool> GetRawData<T>(String url)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            List<T> dataList = await GetDataList<T>(url, "", "Results");
            IntoCache<T>(dataList);
            Logger.Info("Get CafeMaker " + typeof(T).Name + " Data in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
            return true;

        }

        public static async Task<bool> CafeMakerDataUpdate()
        {
            try
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                Queue<Task> tasks = new Queue<Task>();
                tasks.Enqueue(GetRawData<Item>(GetCafeMakerEntityUrl<Item>(CAFE_MAKER_URL,3000)));
                tasks.Enqueue(GetRawData<SpearfishingItem>(GetCafeMakerEntityUrl<SpearfishingItem>(CAFE_MAKER_URL, 3000)));
                tasks.Enqueue(GetRawData<PlaceName>(GetCafeMakerEntityUrl<PlaceName>(CAFE_MAKER_URL, 3000)));
                await Task.WhenAll(tasks);
                Logger.Info("Finished CafeMakerDataUpdate in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
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
            SpearfishingItemCache = default;
            PlaceNameCache = default;
            GC.Collect();
        }

    }
}
