using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GatheringTimer.Data.Database;
using GatheringTimer.Data.Model.Entity;
using GatheringTimer.Util;

namespace GatheringTimer.Data.ThirdParty
{
    public static class Updater
    {
        private static readonly Dictionary<String, String> config = DataConfig.ConfigInitialization();

        private static readonly SQLiteDatabase sqliteDatabase = new SQLiteDatabase();

        public static List<Item> ItemCache { get; set; } = default;
        public static List<GatheringItem> GatheringItemCache { get; set; } = default;
        public static List<SpearfishingItem> SpearfishingItemCache { get; set; } = default;
        public static List<GatheringPointBase> GatheringPointBaseCache { get; set; } = default;
        public static List<GatheringPointBaseExtension> GatheringPointBaseExtensionCache { get; set; } = default;
        public static List<TimeConditionExtension> TimeConditionExtensionCache { get; set; } = default;
        public static List<GatheringPoint> GatheringPointCache { get; set; } = default;
        public static List<PlaceName> PlaceNameCache { get; set; } = default;
        public static List<TerritoryType> TerritoryTypeCache { get; set; } = default;
        public static List<Map> MapCache { get; set; } = default;

        public static SQLiteDatabase GetSQLiteDatabase()
        {
            return sqliteDatabase;
        }

        private static async Task<bool> CreateTable<T>(CancellationToken cancellationToken)
        {

            Stopwatch watch = new Stopwatch();
            watch.Start();
            Logger.Info("Init Database Table " + typeof(T).Name + " Start");
            bool delete = await sqliteDatabase.DeleteTable<T>(cancellationToken);
            bool create = await sqliteDatabase.CreateTable<T>(cancellationToken);
            Logger.Info("Inited Database Table " + typeof(T).Name + " in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
            return true;

        }

        /// <summary>
        /// Initialize DataBase
        /// </summary>
        public static async Task<bool> CacheInitialization(CancellationToken cancellationToken)
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
                tasks.Enqueue(CreateTable<Item>(cancellationToken));
                tasks.Enqueue(CreateTable<GatheringItem>(cancellationToken));
                tasks.Enqueue(CreateTable<SpearfishingItem>(cancellationToken));
                tasks.Enqueue(CreateTable<GatheringPointBase>(cancellationToken));
                tasks.Enqueue(CreateTable<GatheringPoint>(cancellationToken));
                tasks.Enqueue(CreateTable<Map>(cancellationToken));
                tasks.Enqueue(CreateTable<PlaceName>(cancellationToken));
                tasks.Enqueue(CreateTable<TerritoryType>(cancellationToken));
                tasks.Enqueue(CreateTable<GatheringPointBaseExtension>(cancellationToken));
                tasks.Enqueue(CreateTable<TimeConditionExtension>(cancellationToken));
                tasks.Enqueue(CreateTable<FavouriteItem>(cancellationToken));
                tasks.Enqueue(CreateTable<FavouritePoint>(cancellationToken));
                tasks.Enqueue(CreateTable<TimerEnable>(cancellationToken));
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
                    File.Move(cacheSource, dataSource);
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

        private static void IntoCache<T>(List<T> tlist)
        {
            typeof(Updater).GetProperty(typeof(T).Name + "Cache").SetValue(typeof(Updater), tlist);
        }

        public static void ClearCache()
        {
            ItemCache = default;
            GatheringItemCache = default;
            SpearfishingItemCache = default;
            GatheringPointBaseCache = default;
            GatheringPointBaseExtensionCache = default;
            TimeConditionExtensionCache = default;
            GatheringPointCache = default;
            PlaceNameCache = default;
            TerritoryTypeCache = default;
            MapCache = default;
            GC.Collect();
        }

        private static async Task<bool> Sync<T, Q>(Type updater, CancellationToken cancellationToken)
        {
            List<Q> cacheEntities = (List<Q>)updater.GetProperty(typeof(T).Name + "Cache").GetValue(typeof(T));
            List<T> entities = ConvertTo<T, Q>(cacheEntities);
            IntoCache<T>(entities);
            await sqliteDatabase.InsertRowByRow<T>(entities,cancellationToken);
            return true;

        }

        private static async Task<bool> SyncSource(CancellationToken cancellationToken)
        {
            await Sync<Item, XIVApi.Item>(typeof(XIVApi.XIVApiUpdater), cancellationToken);
            await Sync<GatheringItem, XIVApi.GatheringItem>(typeof(XIVApi.XIVApiUpdater), cancellationToken);
            await Sync<SpearfishingItem, XIVApi.SpearfishingItem>(typeof(XIVApi.XIVApiUpdater), cancellationToken);
            await Sync<GatheringPointBase, XIVApi.GatheringPointBase>(typeof(XIVApi.XIVApiUpdater), cancellationToken);
            await Sync<GatheringPoint, XIVApi.GatheringPoint>(typeof(XIVApi.XIVApiUpdater), cancellationToken);
            await Sync<PlaceName, XIVApi.PlaceName>(typeof(XIVApi.XIVApiUpdater), cancellationToken);
            await Sync<TerritoryType, XIVApi.TerritoryType>(typeof(XIVApi.XIVApiUpdater), cancellationToken);
            await Sync<Map, XIVApi.Map>(typeof(XIVApi.XIVApiUpdater), cancellationToken);
            XIVApi.XIVApiUpdater.ClearCache();
            Logger.Info("Sync Raw Finish!");
            return true;
        }

        private static async Task<bool> SyncRawCHS(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                foreach (Item item in ItemCache)
                {
                    foreach (CafeMaker.Item itemCHS in CafeMaker.CafeMakerUpdater.ItemCache)
                    {
                        if (item.ID == itemCHS.ID)
                        {
                            item.Name_chs = itemCHS.Name_chs;
                            break;
                        }

                    }
                }
            });
            await sqliteDatabase.UpdateRowByRow<Item>(ItemCache, new List<string> { "Name_chs" }, new List<string> { "ID" },cancellationToken);
            await Task.Run(() =>
            {
                foreach (SpearfishingItem spearfishingItem in SpearfishingItemCache)
                {
                    foreach (CafeMaker.SpearfishingItem CafeMakerSpearfishingItemsCHS in CafeMaker.CafeMakerUpdater.SpearfishingItemCache)
                    {
                        if (spearfishingItem.ID == CafeMakerSpearfishingItemsCHS.ID)
                        {
                            spearfishingItem.Description_chs = CafeMakerSpearfishingItemsCHS.Description_chs;
                            break;
                        }

                    }
                }
            });
            await sqliteDatabase.UpdateRowByRow<SpearfishingItem>(SpearfishingItemCache, new List<string> { "Description_chs" }, new List<string> { "ID" },cancellationToken);
            await Task.Run(() =>
            {
                foreach (PlaceName placeName in PlaceNameCache)
                {
                    foreach (CafeMaker.PlaceName placeNameCHS in CafeMaker.CafeMakerUpdater.PlaceNameCache)
                    {
                        if (placeName.ID == placeNameCHS.ID)
                        {
                            placeName.Name_chs = placeNameCHS.Name_chs;
                            break;
                        }

                    }
                }
            });
            await sqliteDatabase.UpdateRowByRow<PlaceName>(PlaceNameCache, new List<string> { "Name_chs" }, new List<string> { "ID" },cancellationToken);
            Logger.Info("Sync CHS Finish!");
            CafeMaker.CafeMakerUpdater.ClearCache();
            return true;

        }

        private static async Task<bool> SyncExtension(CancellationToken cancellationToken)
        {
            await Sync<GatheringPointBaseExtension, HuiJiWiki.GatheringPointBaseExtension>(typeof(HuiJiWiki.HuiJiWikiUpdater),cancellationToken);
            await Sync<TimeConditionExtension, HuiJiWiki.TimeConditionExtension>(typeof(HuiJiWiki.HuiJiWikiUpdater),cancellationToken);
            HuiJiWiki.HuiJiWikiUpdater.ClearCache();
            Logger.Info("Sync GatheringPointBaseExtension Finish!");
            return true;
        }

        public static async Task<bool> SyncRaw(CancellationToken cancellationToken)
        {

            await CacheInitialization(cancellationToken);

            Task<bool> rawUpdate = XIVApi.XIVApiUpdater.XIVApiDataUpdate(cancellationToken);
            Task<bool> chsUpdate = CafeMaker.CafeMakerUpdater.CafeMakerDataUpdate(cancellationToken);
            Task<bool> hjwUpdate = HuiJiWiki.HuiJiWikiUpdater.HuiJiWikiDataUpdate(cancellationToken);
            await Task.WhenAll(rawUpdate, chsUpdate, hjwUpdate);
            if (!rawUpdate.Result)
            {
                Logger.Error("Raw Data Sync Fail!");
                return false;
            }
            else
            {

                await SyncSource(cancellationToken);
            }

            if (!chsUpdate.Result)
            {
                Logger.Warn("CHS Data Sync Fail!");
            }
            else
            {
                await SyncRawCHS(cancellationToken);
            }

            if (!hjwUpdate.Result)
            {
                Logger.Warn("Extension Data Sync Fail!");
            }
            else
            {
                await SyncExtension(cancellationToken);
            }

            if (CacheToData())
            {
                Logger.Info("Sync Data Success!");
            }
            else
            {
                Logger.Error("Sync Data Fail!");
            }
            ClearCache();
            Logger.Info("Sync Cache Finish!");
            return true;
        }

        public static async Task SyncDataBaseOnline()
        {

        }
    }
}
