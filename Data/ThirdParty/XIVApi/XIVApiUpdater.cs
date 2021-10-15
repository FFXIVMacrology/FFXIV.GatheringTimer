using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GatheringTimer.Data.Database;
using GatheringTimer.Util;

namespace GatheringTimer.Data.ThirdParty.XIVApi
{
    public class XIVApiUpdater : BaseUpdater
    {
        private static readonly string XIV_API_URL = "https://xivapi.com";

        private static XIVApiUpdater updater = new XIVApiUpdater();

        public static XIVApiUpdater Updater()
        {
            return updater;
        }

        public List<Item> ItemCache { get; set; } = default;
        public List<GatheringItem> GatheringItemCache { get; set; } = default;
        public List<SpearfishingItem> SpearfishingItemCache { get; set; } = default;
        public List<GatheringPointBase> GatheringPointBaseCache { get; set; } = default;
        public List<GatheringPoint> GatheringPointCache { get; set; } = default;
        public List<PlaceName> PlaceNameCache { get; set; } = default;
        public List<TerritoryType> TerritoryTypeCache { get; set; } = default;
        public List<Map> MapCache { get; set; } = default;

        /// <summary>
        /// Get all json with async And return json list(XIVApi)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private async Task<List<String>> RequestXIVApiAsync(String url, String param, CancellationToken cancellationToken)
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

        private string GetXIVApiEntityUrl<T>(string serverUrl, int? limit)
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
        /// Default update method(XivApi)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private async Task<List<T>> GetDataList<T>(String url, String param, String key, CancellationToken cancellationToken)
        {
            List<String> jsonList = await RequestXIVApiAsync(url, param, cancellationToken);
            return await RequestUtil.ParseResultList<T>(jsonList, key, cancellationToken);
        }

        private async Task GetRawData<T>(String url, CancellationToken cancellToken)
        {

            Stopwatch watch = new Stopwatch();
            watch.Start();
            List<T> dataList = await GetDataList<T>(url, "", "Results", cancellToken);
            IntoCache<T>(this, dataList);
            Logger.Info("Get XIVApi " + typeof(T).Name + " Data in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
        }

        public override async Task DataUpdate(CancellationToken cancellToken)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            Queue<Task> tasks = new Queue<Task>();
            tasks.Enqueue(GetRawData<Item>(GetXIVApiEntityUrl<Item>(XIV_API_URL, 3000), cancellToken));
            tasks.Enqueue(GetRawData<GatheringItem>(GetXIVApiEntityUrl<GatheringItem>(XIV_API_URL, 3000), cancellToken));
            tasks.Enqueue(GetRawData<SpearfishingItem>(GetXIVApiEntityUrl<SpearfishingItem>(XIV_API_URL, 3000), cancellToken));
            tasks.Enqueue(GetRawData<GatheringPointBase>(GetXIVApiEntityUrl<GatheringPointBase>(XIV_API_URL, 3000), cancellToken));
            tasks.Enqueue(GetRawData<GatheringPoint>(GetXIVApiEntityUrl<GatheringPoint>(XIV_API_URL, 3000), cancellToken));
            tasks.Enqueue(GetRawData<Map>(GetXIVApiEntityUrl<Map>(XIV_API_URL, 3000), cancellToken));
            tasks.Enqueue(GetRawData<PlaceName>(GetXIVApiEntityUrl<PlaceName>(XIV_API_URL, 3000), cancellToken));
            tasks.Enqueue(GetRawData<TerritoryType>(GetXIVApiEntityUrl<TerritoryType>(XIV_API_URL, 3000), cancellToken));
            await Task.WhenAll(tasks);
            Logger.Info("Finished XIVApiDataUpdate in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
        }

        public override void ClearCache()
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

        public override async Task Sync(CancellationToken cancellationToken)
        {
            Queue<Task> tasks = new Queue<Task>();
            tasks.Enqueue(Sync<Model.Entity.Item, Item>(updater, cancellationToken));
            tasks.Enqueue(Sync<Model.Entity.GatheringItem, GatheringItem>(updater, cancellationToken));
            tasks.Enqueue(Sync<Model.Entity.SpearfishingItem,SpearfishingItem>(updater, cancellationToken));
            tasks.Enqueue(Sync<Model.Entity.GatheringPointBase,GatheringPointBase>(updater, cancellationToken));
            tasks.Enqueue(Sync<Model.Entity.GatheringPoint,GatheringPoint>(updater, cancellationToken));
            tasks.Enqueue(Sync<Model.Entity.PlaceName,PlaceName>(updater, cancellationToken));
            tasks.Enqueue(Sync<Model.Entity.TerritoryType,TerritoryType>(updater, cancellationToken));
            tasks.Enqueue(Sync<Model.Entity.Map,Map>(updater, cancellationToken));
            await Task.WhenAll(tasks);
            ClearCache();
            Logger.Info("Sync XIVData Finish!");
        }

    }
}
