using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
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

        private static async Task<bool> Sync<T, Q>(Type updater)
        {
            List<Q> cacheEntities = (List<Q>)updater.GetProperty(typeof(T).Name + "Cache").GetValue(typeof(T));
            List<T> entities = ConvertTo<T, Q>(cacheEntities);
            IntoCache<T>(entities);
            await sqliteDatabase.InsertRowByRow<T>(entities);
            return true;

        }

        private static async Task<bool> SyncSource()
        {
            await Sync<Item, XIVApi.Item>(typeof(XIVApi.XIVApiUpdater));
            await Sync<GatheringItem, XIVApi.GatheringItem>(typeof(XIVApi.XIVApiUpdater));
            await Sync<SpearfishingItem, XIVApi.SpearfishingItem>(typeof(XIVApi.XIVApiUpdater));
            await Sync<GatheringPointBase, XIVApi.GatheringPointBase>(typeof(XIVApi.XIVApiUpdater));
            await Sync<GatheringPoint, XIVApi.GatheringPoint>(typeof(XIVApi.XIVApiUpdater));
            await Sync<PlaceName, XIVApi.PlaceName>(typeof(XIVApi.XIVApiUpdater));
            await Sync<TerritoryType, XIVApi.TerritoryType>(typeof(XIVApi.XIVApiUpdater));
            await Sync<Map, XIVApi.Map>(typeof(XIVApi.XIVApiUpdater));
            XIVApi.XIVApiUpdater.ClearCache();
            Logger.Info("Sync Raw Finish!");
            return true;
        }

        private static async Task<bool> SyncRawCHS()
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
            await sqliteDatabase.UpdateRowByRow<Item>(ItemCache, new List<string> { "Name_chs" }, new List<string> { "ID" });
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
            await sqliteDatabase.UpdateRowByRow<SpearfishingItem>(SpearfishingItemCache, new List<string> { "Description_chs" }, new List<string> { "ID" });
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
            await sqliteDatabase.UpdateRowByRow<PlaceName>(PlaceNameCache, new List<string> { "Name_chs" }, new List<string> { "ID" });
            Logger.Info("Sync CHS Finish!");
            CafeMaker.CafeMakerUpdater.ClearCache();
            return true;

        }

        private static async Task<bool> SyncExtension()
        {
            await Sync<GatheringPointBaseExtension, HuiJiWiki.GatheringPointBaseExtension>(typeof(HuiJiWiki.HuiJiWikiUpdater));
            await Sync<TimeConditionExtension, HuiJiWiki.TimeConditionExtension>(typeof(HuiJiWiki.HuiJiWikiUpdater));
            HuiJiWiki.HuiJiWikiUpdater.ClearCache();
            Logger.Info("Sync GatheringPointBaseExtension Finish!");
            return true;
        }

        public static async Task<bool> SyncRaw()
        {

            await CacheInitialization();

            Task<bool> rawUpdate = XIVApi.XIVApiUpdater.XIVApiDataUpdate();
            Task<bool> chsUpdate = CafeMaker.CafeMakerUpdater.CafeMakerDataUpdate();
            Task<bool> hjwUpdate = HuiJiWiki.HuiJiWikiUpdater.HuiJiWikiDataUpdate();
            await Task.WhenAll(rawUpdate, chsUpdate, hjwUpdate);
            if (!rawUpdate.Result)
            {
                Logger.Error("Raw Data Sync Fail!");
                return false;
            }
            else
            {

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
            ClearCache();
            Logger.Info("Sync Cache Finish!");
            return true;
        }

        public static async Task SyncDataBaseOnline()
        {

        }
    }
}
