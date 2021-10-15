﻿using System;
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
    public static class GatheringTimerResource
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

        public static async Task<bool> SyncRaw(CancellationToken cancellationToken)
        {

            await CacheInitialization(cancellationToken);

            Task rawUpdate = XIVApi.XIVApiUpdater.Updater().DataUpdate(cancellationToken);
            Task chsUpdate = CafeMaker.CafeMakerUpdater.Updater().DataUpdate(cancellationToken);
            Task hjwUpdate = HuiJiWiki.HuiJiWikiUpdater.Updater().DataUpdate(cancellationToken);
            await Task.WhenAll(rawUpdate, chsUpdate, hjwUpdate);

            await XIVApi.XIVApiUpdater.Updater().Sync(cancellationToken);
            await CafeMaker.CafeMakerUpdater.Updater().Sync(cancellationToken);
            await HuiJiWiki.HuiJiWikiUpdater.Updater().Sync(cancellationToken);
            ClearCache();
            Logger.Info("Sync Cache Finish!");
            return true;
        }

        public static async Task SyncDataBaseOnline()
        {

        }
    }
}