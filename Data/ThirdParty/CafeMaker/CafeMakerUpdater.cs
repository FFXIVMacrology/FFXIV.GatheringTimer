using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GatheringTimer.Data.Database;
using GatheringTimer.Util;

namespace GatheringTimer.Data.ThirdParty.CafeMaker
{
    public class CafeMakerUpdater : BaseUpdater
    {
        private static readonly string CAFE_MAKER_URL = "https://cafemaker.wakingsands.com";

        private static CafeMakerUpdater updater = new CafeMakerUpdater();

        public static CafeMakerUpdater Updater()
        {
            return updater;
        }

        public List<Item> ItemCache { get; set; } = default;
        public List<SpearfishingItem> SpearfishingItemCache { get; set; } = default;
        public List<PlaceName> PlaceNameCache { get; set; } = default;

        private string GetCafeMakerEntityUrl<T>(string serverUrl, int? limit)
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
        private async Task<List<String>> RequestCafeMakerAsync(String url, String param, CancellationToken cancellationToken)
        {
            ServicePointManager.DefaultConnectionLimit = 12;
            String json = await RequestUtil.GetResponseDataAsync(url, param, cancellationToken);
            Pagination pagination = (Pagination)await RequestUtil.JsonToObjectAsync<Pagination>(json, "Pagination", cancellationToken);
            List<String> jsonList = new List<String>() { json };
            if (pagination.Page == pagination.PageTotal)
            {
                return jsonList;
            }
            Queue<Task> tasks = new Queue<Task>();
            for (int page = 2; page <= pagination.PageTotal; page++)
            {
                tasks.Enqueue(RequestUtil.GetResponseDataAsync(url, param + "&Page=" + page, cancellationToken));
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
        private async Task<List<T>> GetDataList<T>(String url, String param, String key, CancellationToken cancellationToken)
        {
            List<String> jsonList = await RequestCafeMakerAsync(url, param, cancellationToken);
            return await RequestUtil.ParseResultList<T>(jsonList, key, cancellationToken);
        }

        private async Task<bool> GetRawData<T>(String url, CancellationToken cancellationToken)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            List<T> dataList = await GetDataList<T>(url, "", "Results", cancellationToken);
            IntoCache<T>(updater, dataList);
            Logger.Info("Get CafeMaker " + typeof(T).Name + " Data in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
            return true;

        }

        public override void ClearCache()
        {
            ItemCache = default;
            SpearfishingItemCache = default;
            PlaceNameCache = default;
            GC.Collect();
        }

        public override async Task DataUpdate(CancellationToken cancellationToken)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            Queue<Task> tasks = new Queue<Task>();
            tasks.Enqueue(GetRawData<Item>(GetCafeMakerEntityUrl<Item>(CAFE_MAKER_URL, 3000), cancellationToken));
            tasks.Enqueue(GetRawData<SpearfishingItem>(GetCafeMakerEntityUrl<SpearfishingItem>(CAFE_MAKER_URL, 3000), cancellationToken));
            tasks.Enqueue(GetRawData<PlaceName>(GetCafeMakerEntityUrl<PlaceName>(CAFE_MAKER_URL, 3000), cancellationToken));
            await Task.WhenAll(tasks);
            Logger.Info("Finished CafeMakerDataUpdate in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
        }

        public override async Task Sync(CancellationToken cancellationToken)
        {
            Queue<Task> cacheTasks = new Queue<Task>();
            cacheTasks.Enqueue(Task.Run(() =>
            {
                foreach (Model.Entity.Item item in GatheringTimerResource.ItemCache)
                {
                    foreach (Item itemCHS in ItemCache)
                    {
                        if (item.ID == itemCHS.ID)
                        {
                            item.Name_chs = itemCHS.Name_chs;
                            break;
                        }

                    }
                }
            }, cancellationToken));
            cacheTasks.Enqueue(Task.Run(() =>
            {
                foreach (Model.Entity.SpearfishingItem spearfishingItem in GatheringTimerResource.SpearfishingItemCache)
                {
                    foreach (SpearfishingItem CafeMakerSpearfishingItemsCHS in SpearfishingItemCache)
                    {
                        if (spearfishingItem.ID == CafeMakerSpearfishingItemsCHS.ID)
                        {
                            spearfishingItem.Description_chs = CafeMakerSpearfishingItemsCHS.Description_chs;
                            break;
                        }

                    }
                }
            }, cancellationToken));
            cacheTasks.Enqueue(Task.Run(() =>
            {
                foreach (Model.Entity.PlaceName placeName in GatheringTimerResource.PlaceNameCache)
                {
                    foreach (PlaceName placeNameCHS in PlaceNameCache)
                    {
                        if (placeName.ID == placeNameCHS.ID)
                        {
                            placeName.Name_chs = placeNameCHS.Name_chs;
                            break;
                        }

                    }
                }
            },cancellationToken));
            await Task.WhenAll(cacheTasks);
            Queue<Task> updateTasks = new Queue<Task>();
            updateTasks.Enqueue(GatheringTimerResource.GetSQLiteDatabase().UpdateRowByRow<Model.Entity.Item>(GatheringTimerResource.ItemCache, new List<string> { "Name_chs" }, new List<string> { "ID" }, cancellationToken));
            updateTasks.Enqueue(GatheringTimerResource.GetSQLiteDatabase().UpdateRowByRow<Model.Entity.SpearfishingItem>(GatheringTimerResource.SpearfishingItemCache, new List<string> { "Description_chs" }, new List<string> { "ID" }, cancellationToken));
            updateTasks.Enqueue(GatheringTimerResource.GetSQLiteDatabase().UpdateRowByRow<Model.Entity.PlaceName>(GatheringTimerResource.PlaceNameCache, new List<string> { "Name_chs" }, new List<string> { "ID" }, cancellationToken));
            await Task.WhenAll(cacheTasks);
            ClearCache();
            Logger.Info("Sync CHS Finish!");
        }
    }
}
