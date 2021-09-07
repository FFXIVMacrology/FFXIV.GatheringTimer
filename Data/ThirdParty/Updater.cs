using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using GatheringTimer.Data.Database;
using GatheringTimer.Data.Model.Entity;
using GatheringTimer.Data.Update.ApiSync;
using GatheringTimer.Data.Update.Config;
using GatheringTimer.Util;

namespace GatheringTimer.Data.Update
{
    public static class Updater
    {
        private static readonly Dictionary<String, String> config = DataConfig.ConfigInitialization();

        private static readonly SQLiteDatabase sqliteDatabase = new SQLiteDatabase();

        public static SQLiteDatabase GetSQLiteDatabase()
        {
            return sqliteDatabase;
        }

        private static async Task<bool> CreateTable<T>()
        {

            Stopwatch watch = new Stopwatch();
            watch.Start();
            Logger.Info("Init Database Table " + typeof(T).Name + " Start");
            bool delete = await sqliteDatabase.DeleteTable<T>();
            bool create = await sqliteDatabase.CreateTable<T>();
            Logger.Info("Inited Database Table " + typeof(T).Name + " in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
            return true;

        }

        /// <summary>
        /// Initialize DataBase
        /// </summary>
        public static async Task<bool> CacheInitialization()
        {
            Logger.Info("Cache File Initializing");
            String path = config["Path"];
            String cacheFilename = config["CacheFilename"];
            if (String.IsNullOrEmpty(path) || String.IsNullOrEmpty(cacheFilename))
            {
                Logger.Error("Config Invaild", new Exception("path or filename is null or Empty"));
                return false;
            }
            else
            {
                String cacheSource = path + cacheFilename;
                sqliteDatabase.SetDataSource(cacheSource);
                sqliteDatabase.DeleteDatabase();
                sqliteDatabase.CreateDatabase();
                Logger.Info("Cache File Already");
                Queue<Task> tasks = new Queue<Task>();
                tasks.Enqueue(CreateTable<Item>());
                tasks.Enqueue(CreateTable<GatheringItem>());
                tasks.Enqueue(CreateTable<SpearfishingItem>());
                tasks.Enqueue(CreateTable<GatheringPointBase>());
                tasks.Enqueue(CreateTable<GatheringPoint>());
                tasks.Enqueue(CreateTable<Map>());
                tasks.Enqueue(CreateTable<PlaceName>());
                tasks.Enqueue(CreateTable<TerritoryType>());
                tasks.Enqueue(CreateTable<GatheringPointBaseExtension>());
                tasks.Enqueue(CreateTable<TimeConditionExtension>());
                tasks.Enqueue(CreateTable<FavouriteItem>());
                tasks.Enqueue(CreateTable<FavouritePoint>());
                tasks.Enqueue(CreateTable<TimerEnable>());
                await Task.WhenAll(tasks);
                return true;
            }

        }

        /// <summary>
        /// Initialize DataBase
        /// </summary>
        public static bool CacheToData()
        {
            Logger.Info("Data File Init");
            String path = config["Path"];
            String cacheFilename = config["CacheFilename"];
            String filename = config["Filename"];
            if (String.IsNullOrEmpty(path) || String.IsNullOrEmpty(filename))
            {
                Logger.Error("Config Invaild", new Exception("path or filename is null or Empty"));
                return false;
            }
            else
            {
                String cacheSource = path + cacheFilename;
                String dataSource = path + filename;
                if (!File.Exists(cacheSource))
                {
                    Logger.Error("Cache File is not exist", null);
                    return false;
                }
                else
                {
                    if (File.Exists(dataSource))
                    {
                        File.Delete(dataSource);
                    }
                    File.Move(cacheFilename, dataSource);
                    Logger.Info("Data File Already");
                    return true;
                }
            }

        }

        private static T ConvertTo<T, Q>(Q q)
        {
            T t = Activator.CreateInstance<T>();
            Type tType = typeof(T);
            PropertyInfo[] tPropertyInfos = tType.GetProperties();

            Type qType = typeof(Q);
            PropertyInfo[] qPropertyInfos = qType.GetProperties();
            foreach (PropertyInfo qPropertyInfo in qPropertyInfos)
            {
                foreach (PropertyInfo tPropertyInfo in tPropertyInfos)
                {
                    if (tPropertyInfo.Name.Equals(qPropertyInfo.Name) && (tPropertyInfo.GetType() == qPropertyInfo.GetType()))
                    {
                        tType.GetProperty(tPropertyInfo.Name).SetValue(t, qType.GetProperty(qPropertyInfo.Name).GetValue(q));
                    }
                }

            }
            return t;

        }

        private static List<T> ConvertTo<T, Q>(List<Q> qList)
        {
            List<T> tList = new List<T>();
            foreach (Q q in qList)
            {
                T t = ConvertTo<T, Q>(q);
                tList.Add(t);
            }
            return tList;
        }

        private static async Task<bool> Sync<T,Q>(SQLiteDatabase cache) {

            List<Q> cacheEntities = await cache.Select<Q>(
                new List<string>(),
                new List<string>(),
                Activator.CreateInstance<Q>()
            ); 
            List<T> entities = ConvertTo<T,Q>(cacheEntities);
            await sqliteDatabase.InsertRowByRow<T>(entities);
            return true;

        }

        private static async Task<bool> SyncSource() {
            Logger.Info("Sync Raw Start!");
            SQLiteDatabase XIVApiCache = XIVApiUpdater.GetSQLiteDatabase();
            await Sync<Item, Model.Vo.XIVApiVo.Item>(XIVApiCache);
            await Sync<GatheringItem, Model.Vo.XIVApiVo.GatheringItem>(XIVApiCache);
            await Sync<SpearfishingItem, Model.Vo.XIVApiVo.SpearfishingItem>(XIVApiCache);
            await Sync<GatheringPointBase, Model.Vo.XIVApiVo.GatheringPointBase>(XIVApiCache);
            await Sync<GatheringPoint, Model.Vo.XIVApiVo.GatheringPoint>(XIVApiCache);
            await Sync<PlaceName, Model.Vo.XIVApiVo.PlaceName>(XIVApiCache);
            await Sync<TerritoryType, Model.Vo.XIVApiVo.TerritoryType>(XIVApiCache);
            await Sync<Map, Model.Vo.XIVApiVo.Map>(XIVApiCache);
            Logger.Info("Sync Raw Finish!");
            return true;


        }

        private static async Task<bool> SyncRawCHS()
        {
            Logger.Info("Sync CHS Start!");
            SQLiteDatabase XIVApiCache = XIVApiUpdater.GetSQLiteDatabase();
            List<Model.Vo.XIVApiVo.Item> XIVApiItems = await XIVApiCache.Select<Model.Vo.XIVApiVo.Item>(
                new List<string>(),
                new List<string>(),
                new Model.Vo.XIVApiVo.Item()
            );
            List<Item> items = ConvertTo<Item, Model.Vo.XIVApiVo.Item>(XIVApiItems);
            SQLiteDatabase CafeMakerCache = CafeMakerUpdater.GetSQLiteDatabase();
            List<Model.Vo.CafeMakerVo.Item> CafeMakerItems = await CafeMakerCache.Select<Model.Vo.CafeMakerVo.Item>(
                new List<string>(),
                new List<string>(),
                new Model.Vo.CafeMakerVo.Item()
            );
            await Task.Run(() =>
            {
                foreach (Item item in items)
                {
                    foreach (Model.Vo.CafeMakerVo.Item itemCHS in CafeMakerItems)
                    {
                        if (item.ID == itemCHS.ID)
                        {
                            item.Name_chs = itemCHS.Name_chs;
                            break;
                        }

                    }
                }
            });

            await sqliteDatabase.UpdateRowByRow<Model.Entity.Item>(items,new List<string> { "Name_chs" },new List<string>{ "ID" });
            Logger.Info("Sync CHS Finish!");
            return true;

        }

        private static async Task<bool> SyncExtension()
        {
            Logger.Info("Sync GatheringPointBaseExtension Start!");
            SQLiteDatabase HuiJiWikiCache = HuiJiWikiUpdater.GetSQLiteDatabase();
            await Sync<GatheringPointBaseExtension, Model.Vo.HuiJiWikiVo.GatheringPointBaseExtension>(HuiJiWikiCache);
            await Sync<TimeConditionExtension, Model.Vo.HuiJiWikiVo.TimeConditionExtension>(HuiJiWikiCache);
            Logger.Info("Sync GatheringPointBaseExtension Finish!");
            return true;
        }

        public static async Task<bool> SyncRaw()
        {
            Logger.Info("Sync RawData Start!");

            await CacheInitialization();

            Task<bool> rawUpdate = XIVApiUpdater.XIVApiDataUpdate();
            Task<bool> chsUpdate = CafeMakerUpdater.CafeMakerDataUpdate();
            Task<bool> hjwUpdate = HuiJiWikiUpdater.HuiJiWikiDataUpdate();
            await Task.WhenAll(rawUpdate, chsUpdate, hjwUpdate);
            if (!rawUpdate.Result)
            {
                Logger.Error("Raw Data Sync Fail!");
                return false;
            }
            else {

                await SyncSource();
            }

            if (!chsUpdate.Result)
            {
                Logger.Warning("CHS Data Sync Fail!");
            }
            else
            {
                await SyncRawCHS();
            }

            if (!hjwUpdate.Result)
            {
                Logger.Warning("Extension Data Sync Fail!");
            }
            else
            {
                await SyncExtension();
            }

            if (CacheToData())
            {
                Logger.Info("Sync Data Success!");
            }
            else
            {
                Logger.Error("Sync Data Fail!");
            }
            Logger.Info("Sync Cache Finish!");
            return true;
        }

        public static async Task SyncDataBaseOnline()
        {

        }
    }
}
