using GatheringTimer.Data.Database;
using GatheringTimer.Data.Model.Entity;
using GatheringTimer.Data.Update;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GatheringTimer
{
    public class Service
    {

        private static readonly Dictionary<String, String> config = DataConfig.ConfigInitialization();

        private static readonly SQLiteDatabase sqliteDatabase = new SQLiteDatabase();

        private static List<Item> itemCache = default;
        private static List<GatheringItem> gatheringItemCache = default;
        private static List<GatheringPointBase> gatheringPointBaseCache = default;
        private static List<GatheringPointBaseExtension> gatheringPointBaseExtensionCache = default;
        private static List<TimeConditionExtension> timeConditionExtensionCache = default;
        private static List<GatheringPoint> gatheringPointCache = default;
        private static List<PlaceName> placeNameCache = default;
        private static List<TerritoryType> territoryTypeCache = default;
        private static List<Map> mapCache = default;

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

        private static bool DatabaseInitialization()
        {
            Logger.Info("Database File initializing");
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
                if (File.Exists(dataSource))
                {
                    sqliteDatabase.SetDataSource(dataSource);
                    Logger.Info("Database File Already");
                    return true;
                }
                else
                {
                    Logger.Info("Database File Not Exists");
                    return false;
                }

            }
        }

        public static SQLiteDatabase GetSQLiteDatabase()
        {
            return sqliteDatabase;
        }

        public static async Task LoadSource()
        {
            if (DatabaseInitialization()) {
                itemCache = await sqliteDatabase.Select<Item>(new List<string>(), new Item());
                gatheringItemCache = await sqliteDatabase.Select<GatheringItem>(new List<string>(), new GatheringItem());
                gatheringPointBaseCache = await sqliteDatabase.Select<GatheringPointBase>(new List<string>(), new GatheringPointBase());
                gatheringPointBaseExtensionCache = await sqliteDatabase.Select<GatheringPointBaseExtension>(new List<string>(), new GatheringPointBaseExtension());
                timeConditionExtensionCache = await sqliteDatabase.Select<TimeConditionExtension>(new List<string>(), new TimeConditionExtension());
                gatheringPointCache = await sqliteDatabase.Select<GatheringPoint>(new List<string>(), new GatheringPoint());
                placeNameCache = await sqliteDatabase.Select<PlaceName>(new List<string>(), new PlaceName());
                territoryTypeCache = await sqliteDatabase.Select<TerritoryType>(new List<string>(), new TerritoryType());
                mapCache = await sqliteDatabase.Select<Map>(new List<string>(), new Map());
            }
        }

        public static async Task<List<Data.Model.Vo.DisplayVo.Item>> GetItems(String searchStr)
        {
            DatabaseInitialization();
            List<Item> itemCache = await sqliteDatabase.Select<Item>(
                new List<string>() {
                    "Name",
                    "Name_de",
                    "Name_en",
                    "Name_fr",
                    "Name_ja",
                    "Name_chs"
                },
                new List<string>() {
                "LIKE",
                "LIKE",
                "LIKE",
                "LIKE",
                "LIKE",
                "LIKE",
                },
                new Item()
                {
                    Name = searchStr,
                    Name_de = searchStr,
                    Name_en = searchStr,
                    Name_fr = searchStr,
                    Name_ja = searchStr,
                    Name_chs = searchStr
                },
                new List<string>() {
                "OR",
                "OR",
                "OR",
                "OR",
                "OR"
                },
                10
            );
            return ConvertTo<Data.Model.Vo.DisplayVo.Item, Item>(itemCache);
        }

        public static async Task<List<Data.Model.Vo.DisplayVo.Item>> GetItemDetail(int itemId)
        {
            await LoadSource();
            List<Item> items = (from item in itemCache where item.ID == itemId select item).ToList<Item>();
            if (items.Count > 0)
            {
                Data.Model.Vo.DisplayVo.Item item = ConvertTo<Data.Model.Vo.DisplayVo.Item, Item>(items.First());

                List<GatheringItem> gatheringItems = (from gatheringItem in gatheringItemCache where gatheringItem.ItemTargetID == itemId select gatheringItem).ToList<GatheringItem>();
                if (gatheringItems.Count > 0)
                {
                    item.GatheringItem = ConvertTo<Data.Model.Vo.DisplayVo.GatheringItem, GatheringItem>(gatheringItems)[0];

                    List<GatheringPointBase> entities =
                        (from gatheringPointBase in gatheringPointBaseCache
                         where gatheringPointBase.Item0TargetID == item.GatheringItem.ID
                     || gatheringPointBase.Item1TargetID == item.GatheringItem.ID
                     || gatheringPointBase.Item2TargetID == item.GatheringItem.ID
                     || gatheringPointBase.Item3TargetID == item.GatheringItem.ID
                     || gatheringPointBase.Item4TargetID == item.GatheringItem.ID
                     || gatheringPointBase.Item5TargetID == item.GatheringItem.ID
                     || gatheringPointBase.Item6TargetID == item.GatheringItem.ID
                     || gatheringPointBase.Item7TargetID == item.GatheringItem.ID
                         select gatheringPointBase).ToList<GatheringPointBase>();

                    if (entities.Count > 0)
                    {
                        item.GatheringItem.GatheringPointBases = ConvertTo<Data.Model.Vo.DisplayVo.GatheringPointBase, GatheringPointBase>(entities);
                        foreach (Data.Model.Vo.DisplayVo.GatheringPointBase gatheringPointBase in item.GatheringItem.GatheringPointBases)
                        {
                            List<GatheringPointBaseExtension> exEntities =
                               (from gatheringPointBaseExtension in gatheringPointBaseExtensionCache
                                where gatheringPointBaseExtension.ID == gatheringPointBase.ID
                                select gatheringPointBaseExtension).ToList<GatheringPointBaseExtension>();
                            if (exEntities.Count > 0)
                            {
                                gatheringPointBase.GatheringPointBaseExtension = ConvertTo<Data.Model.Vo.DisplayVo.GatheringPointBaseExtension, GatheringPointBaseExtension>(exEntities).First();
                            }
                            List<TimeConditionExtension> timeEntities =
                               (from timeConditionExtension in timeConditionExtensionCache
                                where timeConditionExtension.GatheringPointBaseId == gatheringPointBase.ID
                                select timeConditionExtension).ToList<TimeConditionExtension>();
                            if (timeEntities.Count > 0)
                            {
                                gatheringPointBase.TimeConditionExtension = ConvertTo<Data.Model.Vo.DisplayVo.TimeConditionExtension, TimeConditionExtension>(timeEntities);
                            }
                            List<GatheringPoint> pointEntities =
                               (from gatheringPoint in gatheringPointCache
                                where gatheringPoint.GatheringPointBaseTargetID == gatheringPointBase.ID
                                select gatheringPoint).ToList<GatheringPoint>();
                            if (pointEntities.Count > 0)
                            {
                                gatheringPointBase.GatheringPoint = ConvertTo<Data.Model.Vo.DisplayVo.GatheringPoint, GatheringPoint>(pointEntities);
                                foreach (Data.Model.Vo.DisplayVo.GatheringPoint gatheringPoint in gatheringPointBase.GatheringPoint)
                                {
                                    List<PlaceName> placeEntities =
                                     (from placeName in placeNameCache
                                      where placeName.ID == gatheringPoint.PlaceNameTargetID
                                      select placeName).ToList<PlaceName>();
                                    if (placeEntities.Count > 0)
                                    {
                                        gatheringPoint.PlaceName = ConvertTo<Data.Model.Vo.DisplayVo.PlaceName, PlaceName>(placeEntities).First();
                                    }
                                    List<TerritoryType> territoryEntities =
                                      (from territoryType in territoryTypeCache
                                       where territoryType.ID == gatheringPoint.TerritoryTypeTargetID
                                       select territoryType).ToList<TerritoryType>();
                                    if (territoryEntities.Count > 0)
                                    {
                                        gatheringPoint.TerritoryType = ConvertTo<Data.Model.Vo.DisplayVo.TerritoryType, TerritoryType>(territoryEntities).First();
                                        List<Map> mapEntities =
                                          (from map in mapCache
                                           where map.ID == gatheringPoint.TerritoryType.MapTargetID
                                           select map).ToList<Map>();
                                        if (mapEntities.Count > 0)
                                        {
                                            gatheringPoint.TerritoryType.Map = ConvertTo<Data.Model.Vo.DisplayVo.Map, Map>(mapEntities).First();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                Logger.Info(JsonConvert.SerializeObject(item));

            }
            return null;

        }

    }
}
