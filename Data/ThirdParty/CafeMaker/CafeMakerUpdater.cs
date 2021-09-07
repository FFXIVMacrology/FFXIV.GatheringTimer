using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using GatheringTimer.Data.Database;
using GatheringTimer.Data.Model.Vo.CafeMakerVo;
using GatheringTimer.Data.Update.Config;
using GatheringTimer.Util;

namespace GatheringTimer.Data.Update.ApiSync
{
    public static class CafeMakerUpdater
    {
        private static readonly Dictionary<String, String> config = CafeMakerConfig.ConfigInitialization();

        private static readonly SQLiteDatabase cacheDatabase = new SQLiteDatabase();

        public static SQLiteDatabase GetSQLiteDatabase()
        {
            return cacheDatabase;
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
            Logger.Info("Get [" + typeof(T).Name + "] Data Success!");
            return await RequestUtil.ParseResultList<T>(jsonList, key);
        }

        /// <summary>
        /// Reinitialize DataBase
        /// </summary>
        private static bool CacheInitialization()
        {
            Logger.Info("CafeMakerCache File Reinitializing");
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
                Logger.Info("CafeMakerCache File Already");
                return true;
            }

        }

        private static async Task<bool> GetRawData<T>(String url)
        {

            Logger.Info("Get CafeMaker " + typeof(T).Name + " Data Start");
            Stopwatch watch = new Stopwatch();
            watch.Start();
            List<T> dataList = await GetDataList<T>(url, "", "Results");
            Logger.Info("Get CafeMaker " + typeof(T).Name + " Data in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
            watch.Reset();
            watch.Start();
            Logger.Info("Init CafeMaker Cache " + typeof(T).Name + " Start");
            bool delete = await cacheDatabase.DeleteTable<T>();
            bool create = await cacheDatabase.CreateTable<T>();
            Logger.Info("Inited CafeMaker Cache " + typeof(T).Name + " in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
            watch.Reset();
            watch.Start();
            Logger.Info("Save CafeMaker Cache " + typeof(T).Name + " Start");
            bool itemsInsert = await cacheDatabase.InsertRowByRow<T>(dataList);
            Logger.Info("Saved CafeMaker Cache " + typeof(T).Name + " in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
            return itemsInsert;

        }

        public static async Task<bool> CafeMakerDataUpdate()
        {
            try
            {
                CacheInitialization();

                Logger.Info("Loading Config");
                Stopwatch watch = new Stopwatch();
                watch.Start();
                String dataSource = config["Path"] + config["Filename"];
                String urlItem = config["Item"];
                Logger.Info("Loaded Config in " + (watch.ElapsedMilliseconds / 1000.0) + " s");

                Logger.Info("CafeMakerDataUpdate Start");
                watch.Reset();
                watch.Start();
                Queue<Task> tasks = new Queue<Task>();
                tasks.Enqueue(GetRawData<Item>(urlItem));
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


    }
}
