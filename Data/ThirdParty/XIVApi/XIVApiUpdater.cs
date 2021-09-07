using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using GatheringTimer.Data.Database;
using GatheringTimer.Data.Model.Vo.XIVApiVo;
using GatheringTimer.Data.Update.Config;
using GatheringTimer.Util;

namespace GatheringTimer.Data.Update.ApiSync
{
    public static class XIVApiUpdater
    {

        private static readonly Dictionary<String, String> config = XIVApiConfig.ConfigInitialization();

        private static readonly SQLiteDatabase cacheDatabase = new SQLiteDatabase();

        public static SQLiteDatabase GetSQLiteDatabase()
        {
            return cacheDatabase;
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
            Logger.Info("Get [" + typeof(T).Name + "] Data Success!");
            return await RequestUtil.ParseResultList<T>(jsonList, key);
        }

        /// <summary>
        /// Reinitialize DataBase
        /// </summary>
        private static bool CacheInitialization()
        {
            Logger.Info("XIVApiCache File Reinitializing");
            String path = config["Path"];
            String filename = config["Filename"];
            if (String.IsNullOrEmpty(path) || String.IsNullOrEmpty(filename))
            {
                Logger.Error("Config Invaild", new Exception("path or filename is null or Empty"));
                return false;
            }
            else
            {
                String dataSource = path + filename;
                cacheDatabase.SetDataSource(dataSource);
                cacheDatabase.DeleteDatabase();
                cacheDatabase.CreateDatabase();
                Logger.Info("XIVApiCache File Already");
                return true;
            }

        }

        private static async Task<bool> GetRawData<T>(String url) {

            Logger.Info("Get XIVApi " + typeof(T).Name + " Data Start");
            Stopwatch watch = new Stopwatch();
            watch.Start();
            List<T> dataList = await GetDataList<T>(url, "", "Results");
            Logger.Info("Get XIVApi " + typeof(T).Name + " Data in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
            watch.Reset();
            watch.Start();
            Logger.Info("Init XIVApi Cache " + typeof(T).Name + " Start");
            bool delete = await cacheDatabase.DeleteTable<T>();
            bool create = await cacheDatabase.CreateTable<T>();
            Logger.Info("Inited XIVApi Cache " + typeof(T).Name + " in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
            watch.Reset();
            watch.Start();
            Logger.Info("Save XIVApi Cache " + typeof(T).Name + " Start");
            bool itemsInsert = await cacheDatabase.InsertRowByRow<T>(dataList);
            Logger.Info("Saved XIVApi Cache " + typeof(T).Name + " in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
            return itemsInsert;

        }

        public static async Task<bool> XIVApiDataUpdate() {

            try {
                CacheInitialization();

                Logger.Info("Loading Config");
                Stopwatch watch = new Stopwatch();
                watch.Start();
                String dataSource = config["Path"] + config["Filename"];
                String urlItem = config["Item"];
                String urlGatheringItem = config["GatheringItem"];
                String urlSpearfishingItem = config["SpearfishingItem"];
                String urlGatheringPointBase = config["GatheringPointBase"];
                String urlGatheringPoint = config["GatheringPoint"];
                String urlMap = config["Map"];
                String urlPlaceName = config["PlaceName"];
                String urlTerritoryType = config["TerritoryType"];
                Logger.Info("Loaded Config in " + (watch.ElapsedMilliseconds / 1000.0) + " s");

                Logger.Info("XIVApiDataUpdate Start");
                watch.Reset();
                watch.Start();
                Queue<Task> tasks = new Queue<Task>();
                tasks.Enqueue(GetRawData<Item>(urlItem));
                tasks.Enqueue(GetRawData<GatheringItem>(urlGatheringItem));
                tasks.Enqueue(GetRawData<SpearfishingItem>(urlSpearfishingItem));
                tasks.Enqueue(GetRawData<GatheringPointBase>(urlGatheringPointBase));
                tasks.Enqueue(GetRawData<GatheringPoint>(urlGatheringPoint));
                tasks.Enqueue(GetRawData<Map>(urlMap));
                tasks.Enqueue(GetRawData<PlaceName>(urlPlaceName));
                tasks.Enqueue(GetRawData<TerritoryType>(urlTerritoryType));
                await Task.WhenAll(tasks);
                Logger.Info("Finished XIVApiDataUpdate in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
                return true;
            } catch (Exception ex) {
                Logger.Error("Update XIVApiData Error,Exception:"+ex.Message);
                return false;
            }

        }
    }
}
