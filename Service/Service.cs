using GatheringTimer.Data.Database;
using GatheringTimer.Data.Model.Entity;
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

        private static readonly Dictionary<String, String> config = Data.DataConfig.ConfigInitialization();

        private static readonly SQLiteDatabase sqliteDatabase = new SQLiteDatabase();

        private static List<Item> itemCache = default;
        private static List<GatheringItem> gatheringItemCache = default;
        private static List<SpearfishingItem> spearfishingItemCache = default;
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

        public static async Task LoadSource(bool force)
        {
            GC.Collect();
            if (DatabaseInitialization())
            {
                if (force)
                {
                    itemCache = await sqliteDatabase.Select<Item>(new List<string>(), new Item());
                    gatheringItemCache = await sqliteDatabase.Select<GatheringItem>(new List<string>(), new GatheringItem());
                    spearfishingItemCache = await sqliteDatabase.Select<SpearfishingItem>(new List<string>(), new SpearfishingItem());
                    gatheringPointBaseCache = await sqliteDatabase.Select<GatheringPointBase>(new List<string>(), new GatheringPointBase());
                    gatheringPointBaseExtensionCache = await sqliteDatabase.Select<GatheringPointBaseExtension>(new List<string>(), new GatheringPointBaseExtension());
                    timeConditionExtensionCache = await sqliteDatabase.Select<TimeConditionExtension>(new List<string>(), new TimeConditionExtension());
                    gatheringPointCache = await sqliteDatabase.Select<GatheringPoint>(new List<string>(), new GatheringPoint());
                    placeNameCache = await sqliteDatabase.Select<PlaceName>(new List<string>(), new PlaceName());
                    territoryTypeCache = await sqliteDatabase.Select<TerritoryType>(new List<string>(), new TerritoryType());
                    mapCache = await sqliteDatabase.Select<Map>(new List<string>(), new Map());
                }
                else
                {
                    itemCache = itemCache ?? await sqliteDatabase.Select<Item>(new List<string>(), new Item());
                    gatheringItemCache = gatheringItemCache ?? await sqliteDatabase.Select<GatheringItem>(new List<string>(), new GatheringItem());
                    spearfishingItemCache = spearfishingItemCache ?? await sqliteDatabase.Select<SpearfishingItem>(new List<string>(), new SpearfishingItem());
                    gatheringPointBaseCache = gatheringPointBaseCache ?? await sqliteDatabase.Select<GatheringPointBase>(new List<string>(), new GatheringPointBase());
                    gatheringPointBaseExtensionCache = gatheringPointBaseExtensionCache ?? await sqliteDatabase.Select<GatheringPointBaseExtension>(new List<string>(), new GatheringPointBaseExtension());
                    timeConditionExtensionCache = timeConditionExtensionCache ?? await sqliteDatabase.Select<TimeConditionExtension>(new List<string>(), new TimeConditionExtension());
                    gatheringPointCache = gatheringPointCache ?? await sqliteDatabase.Select<GatheringPoint>(new List<string>(), new GatheringPoint());
                    placeNameCache = placeNameCache ?? await sqliteDatabase.Select<PlaceName>(new List<string>(), new PlaceName());
                    territoryTypeCache = territoryTypeCache ?? await sqliteDatabase.Select<TerritoryType>(new List<string>(), new TerritoryType());
                    mapCache = mapCache ?? await sqliteDatabase.Select<Map>(new List<string>(), new Map());
                }

            }
        }

        public static async Task<List<Data.Model.DisplayVo.Item>> GetItems(String searchStr)
        {
            await LoadSource(false);
            List<int?> fishList = (from spearfishingItem in spearfishingItemCache select spearfishingItem.ItemTargetID).ToList<int?>();
            List<int?> gatheringList = (from gatheringItem in gatheringItemCache select gatheringItem.ItemTargetID).ToList<int?>();
            List<int?> allList = fishList.Union<int?>(gatheringList).ToList<int?>();
            List<Item> items = (from item in itemCache
                                join itemTargetID in allList on item.ID equals itemTargetID
                                where item.Name.Contains(searchStr)
                                || item.Name_de.Contains(searchStr)
                                || item.Name_en.Contains(searchStr)
                                || item.Name_fr.Contains(searchStr)
                                || item.Name_ja.Contains(searchStr)
                                || item.Name_chs.Contains(searchStr)
                                select item).Take(10).ToList<Item>();
            return ConvertTo<Data.Model.DisplayVo.Item, Item>(items);
        }

        public static async Task<Data.Model.DisplayVo.Item> GetItemDetail(int itemId)
        {
            await LoadSource(false);
            List<Item> items = (from item in itemCache where item.ID == itemId select item).ToList<Item>();
            if (items.Count > 0)
            {
                Data.Model.DisplayVo.Item item = ConvertTo<Data.Model.DisplayVo.Item, Item>(items.First());

                List<GatheringItem> gatheringItems = (from gatheringItem in gatheringItemCache where gatheringItem.ItemTargetID == itemId select gatheringItem).ToList<GatheringItem>();
                List<SpearfishingItem> spearfishingItems = (from spearfishingItem in spearfishingItemCache where spearfishingItem.ItemTargetID == itemId select spearfishingItem).ToList<SpearfishingItem>();
                if (gatheringItems.Count > 0 || spearfishingItems.Count > 0)
                {
                    int? targetID = 0;
                    string target = "";

                    if (gatheringItems.Count > 0)
                    {
                        item.GatheringItem = ConvertTo<Data.Model.DisplayVo.GatheringItem, GatheringItem>(gatheringItems)[0];
                        target = "GatheringItem";
                        targetID = item.GatheringItem.ID;
                    }

                    if (spearfishingItems.Count > 0)
                    {
                        item.SpearfishingItem = ConvertTo<Data.Model.DisplayVo.SpearfishingItem, SpearfishingItem>(spearfishingItems)[0];
                        target = "SpearfishingItem";
                        targetID = item.SpearfishingItem.ID;
                    }

                    List<GatheringPointBase> entities =
                        (from gatheringPointBase in gatheringPointBaseCache
                         where ((gatheringPointBase.Item0Target == target && gatheringPointBase.Item0TargetID == targetID)
                     || (gatheringPointBase.Item1Target == target && gatheringPointBase.Item1TargetID == targetID)
                     || (gatheringPointBase.Item2Target == target && gatheringPointBase.Item2TargetID == targetID)
                     || (gatheringPointBase.Item3Target == target && gatheringPointBase.Item3TargetID == targetID)
                     || (gatheringPointBase.Item4Target == target && gatheringPointBase.Item4TargetID == targetID)
                     || (gatheringPointBase.Item5Target == target && gatheringPointBase.Item5TargetID == targetID)
                     || (gatheringPointBase.Item6Target == target && gatheringPointBase.Item6TargetID == targetID)
                     || (gatheringPointBase.Item7Target == target && gatheringPointBase.Item7TargetID == targetID))
                         select gatheringPointBase).ToList<GatheringPointBase>();

                    if (entities.Count > 0)
                    {
                        List<Data.Model.DisplayVo.GatheringPointBase> gatheringPointBases = ConvertTo<Data.Model.DisplayVo.GatheringPointBase, GatheringPointBase>(entities);

                        if (gatheringItems.Count > 0)
                        {
                            item.GatheringItem.GatheringPointBases = gatheringPointBases;
                        }

                        if (spearfishingItems.Count > 0)
                        {
                            item.SpearfishingItem.GatheringPointBases = gatheringPointBases;
                        }

                        foreach (Data.Model.DisplayVo.GatheringPointBase gatheringPointBase in gatheringPointBases)
                        {
                            List<GatheringPointBaseExtension> exEntities =
                               (from gatheringPointBaseExtension in gatheringPointBaseExtensionCache
                                where gatheringPointBaseExtension.ID == gatheringPointBase.ID
                                select gatheringPointBaseExtension).ToList<GatheringPointBaseExtension>();
                            if (exEntities.Count > 0)
                            {
                                gatheringPointBase.GatheringPointBaseExtension = ConvertTo<Data.Model.DisplayVo.GatheringPointBaseExtension, GatheringPointBaseExtension>(exEntities).First();
                            }
                            List<TimeConditionExtension> timeEntities =
                               (from timeConditionExtension in timeConditionExtensionCache
                                where timeConditionExtension.GatheringPointBaseId == gatheringPointBase.ID
                                select timeConditionExtension).ToList<TimeConditionExtension>();
                            if (timeEntities.Count > 0)
                            {
                                gatheringPointBase.TimeConditionExtension = ConvertTo<Data.Model.DisplayVo.TimeConditionExtension, TimeConditionExtension>(timeEntities);
                            }
                            List<GatheringPoint> pointEntities =
                               (from gatheringPoint in gatheringPointCache
                                where gatheringPoint.GatheringPointBaseTargetID == gatheringPointBase.ID
                                select gatheringPoint).ToList<GatheringPoint>();
                            if (pointEntities.Count > 0)
                            {
                                gatheringPointBase.GatheringPoint = ConvertTo<Data.Model.DisplayVo.GatheringPoint, GatheringPoint>(pointEntities);
                                foreach (Data.Model.DisplayVo.GatheringPoint gatheringPoint in gatheringPointBase.GatheringPoint)
                                {
                                    List<PlaceName> placeEntities =
                                     (from placeName in placeNameCache
                                      where placeName.ID == gatheringPoint.PlaceNameTargetID
                                      select placeName).ToList<PlaceName>();
                                    if (placeEntities.Count > 0)
                                    {
                                        gatheringPoint.PlaceName = ConvertTo<Data.Model.DisplayVo.PlaceName, PlaceName>(placeEntities).First();
                                    }
                                    List<TerritoryType> territoryEntities =
                                      (from territoryType in territoryTypeCache
                                       where territoryType.ID == gatheringPoint.TerritoryTypeTargetID
                                       select territoryType).ToList<TerritoryType>();
                                    if (territoryEntities.Count > 0)
                                    {
                                        gatheringPoint.TerritoryType = ConvertTo<Data.Model.DisplayVo.TerritoryType, TerritoryType>(territoryEntities).First();

                                        List<PlaceName> trrPlaceEntities =
                                            (from placeName in placeNameCache
                                             where placeName.ID == gatheringPoint.TerritoryType.PlaceNameTargetID
                                             select placeName).ToList<PlaceName>();
                                        if (trrPlaceEntities.Count > 0)
                                        {
                                            gatheringPoint.TerritoryType.PlaceName = ConvertTo<Data.Model.DisplayVo.PlaceName, PlaceName>(trrPlaceEntities).First();
                                        }
                                        List<PlaceName> trrPlaceRegionEntities =
                                            (from placeName in placeNameCache
                                             where placeName.ID == gatheringPoint.TerritoryType.PlaceNameRegionTargetID
                                             select placeName).ToList<PlaceName>();
                                        if (trrPlaceRegionEntities.Count > 0)
                                        {
                                            gatheringPoint.TerritoryType.PlaceNameRegion = ConvertTo<Data.Model.DisplayVo.PlaceName, PlaceName>(trrPlaceRegionEntities).First();
                                        }
                                        List<PlaceName> trrPlaceZoneEntities =
                                            (from placeName in placeNameCache
                                             where placeName.ID == gatheringPoint.TerritoryType.PlaceNameZoneTargetID
                                             select placeName).ToList<PlaceName>();
                                        if (trrPlaceZoneEntities.Count > 0)
                                        {
                                            gatheringPoint.TerritoryType.PlaceNameZone = ConvertTo<Data.Model.DisplayVo.PlaceName, PlaceName>(trrPlaceZoneEntities).First();
                                        }
                                        List<Map> mapEntities =
                                          (from map in mapCache
                                           where map.ID == gatheringPoint.TerritoryType.MapTargetID
                                           select map).ToList<Map>();
                                        if (mapEntities.Count > 0)
                                        {
                                            gatheringPoint.TerritoryType.Map = ConvertTo<Data.Model.DisplayVo.Map, Map>(mapEntities).First();

                                            List<PlaceName> mapPlaceEntities =
                                                (from placeName in placeNameCache
                                                 where placeName.ID == gatheringPoint.TerritoryType.Map.PlaceNameTargetID
                                                 select placeName).ToList<PlaceName>();
                                            if (mapPlaceEntities.Count > 0)
                                            {
                                                gatheringPoint.TerritoryType.Map.PlaceName = ConvertTo<Data.Model.DisplayVo.PlaceName, PlaceName>(mapPlaceEntities).First();
                                            }
                                            List<PlaceName> mapPlaceRegionEntities =
                                                (from placeName in placeNameCache
                                                 where placeName.ID == gatheringPoint.TerritoryType.Map.PlaceNameRegionTargetID
                                                 select placeName).ToList<PlaceName>();
                                            if (mapPlaceRegionEntities.Count > 0)
                                            {
                                                gatheringPoint.TerritoryType.Map.PlaceNameRegion = ConvertTo<Data.Model.DisplayVo.PlaceName, PlaceName>(mapPlaceRegionEntities).First();
                                            }

                                            string location = "";
                                            if (gatheringPointBase.GatheringPointBaseExtension != null)
                                            {
                                                location = "(X:" + gatheringPointBase.GatheringPointBaseExtension.LocationX + ",Y:" + gatheringPointBase.GatheringPointBaseExtension.LocationY + ")";
                                            }

                                            gatheringPointBase.Description = gatheringPoint.TerritoryType.PlaceName.Name + location + gatheringPoint.PlaceName.Name;
                                            gatheringPointBase.Description_de = gatheringPoint.TerritoryType.PlaceName.Name_de + location + gatheringPoint.PlaceName.Name_de;
                                            gatheringPointBase.Description_en = gatheringPoint.TerritoryType.PlaceName.Name_en + location + gatheringPoint.PlaceName.Name_en;
                                            gatheringPointBase.Description_fr = gatheringPoint.TerritoryType.PlaceName.Name_fr + location + gatheringPoint.PlaceName.Name_fr;
                                            gatheringPointBase.Description_ja = gatheringPoint.TerritoryType.PlaceName.Name_ja + location + gatheringPoint.PlaceName.Name_ja;
                                            gatheringPointBase.Description_chs = gatheringPoint.TerritoryType.PlaceName.Name_chs + location + gatheringPoint.PlaceName.Name_chs;
                                        }


                                    }
                                }
                            }
                        }
                    }
                }
                Logger.Info(JsonConvert.SerializeObject(item));
                return item;

            }
            return null;

        }

        public static async Task<Data.Model.DisplayVo.GatheringPointBase> GetGatheringPointBaseDetail(int gatheringPointBaseId)
        {
            await LoadSource(false);
            List<GatheringPointBase> gatheringPointBases =
                (from gatheringPointBaseEntity in gatheringPointBaseCache
                 where gatheringPointBaseEntity.ID == gatheringPointBaseId
                 select gatheringPointBaseEntity).ToList<GatheringPointBase>();

            if (gatheringPointBases.Count > 0)
            {
                Data.Model.DisplayVo.GatheringPointBase gatheringPointBase = ConvertTo<Data.Model.DisplayVo.GatheringPointBase, GatheringPointBase>(gatheringPointBases).First();
                List<GatheringPointBaseExtension> exEntities =
                   (from gatheringPointBaseExtension in gatheringPointBaseExtensionCache
                    where gatheringPointBaseExtension.ID == gatheringPointBase.ID
                    select gatheringPointBaseExtension).ToList<GatheringPointBaseExtension>();
                if (exEntities.Count > 0)
                {
                    gatheringPointBase.GatheringPointBaseExtension = ConvertTo<Data.Model.DisplayVo.GatheringPointBaseExtension, GatheringPointBaseExtension>(exEntities).First();
                }
                List<TimeConditionExtension> timeEntities =
                   (from timeConditionExtension in timeConditionExtensionCache
                    where timeConditionExtension.GatheringPointBaseId == gatheringPointBase.ID
                    select timeConditionExtension).ToList<TimeConditionExtension>();
                if (timeEntities.Count > 0)
                {
                    gatheringPointBase.TimeConditionExtension = ConvertTo<Data.Model.DisplayVo.TimeConditionExtension, TimeConditionExtension>(timeEntities);
                }
                List<GatheringPoint> pointEntities =
                   (from gatheringPoint in gatheringPointCache
                    where gatheringPoint.GatheringPointBaseTargetID == gatheringPointBase.ID
                    select gatheringPoint).ToList<GatheringPoint>();
                if (pointEntities.Count > 0)
                {
                    gatheringPointBase.GatheringPoint = ConvertTo<Data.Model.DisplayVo.GatheringPoint, GatheringPoint>(pointEntities);
                    foreach (Data.Model.DisplayVo.GatheringPoint gatheringPoint in gatheringPointBase.GatheringPoint)
                    {
                        List<PlaceName> placeEntities =
                         (from placeName in placeNameCache
                          where placeName.ID == gatheringPoint.PlaceNameTargetID
                          select placeName).ToList<PlaceName>();
                        if (placeEntities.Count > 0)
                        {
                            gatheringPoint.PlaceName = ConvertTo<Data.Model.DisplayVo.PlaceName, PlaceName>(placeEntities).First();
                        }
                        List<TerritoryType> territoryEntities =
                          (from territoryType in territoryTypeCache
                           where territoryType.ID == gatheringPoint.TerritoryTypeTargetID
                           select territoryType).ToList<TerritoryType>();
                        if (territoryEntities.Count > 0)
                        {
                            gatheringPoint.TerritoryType = ConvertTo<Data.Model.DisplayVo.TerritoryType, TerritoryType>(territoryEntities).First();

                            List<PlaceName> trrPlaceEntities =
                                (from placeName in placeNameCache
                                 where placeName.ID == gatheringPoint.TerritoryType.PlaceNameTargetID
                                 select placeName).ToList<PlaceName>();
                            if (trrPlaceEntities.Count > 0)
                            {
                                gatheringPoint.TerritoryType.PlaceName = ConvertTo<Data.Model.DisplayVo.PlaceName, PlaceName>(trrPlaceEntities).First();
                            }
                            List<PlaceName> trrPlaceRegionEntities =
                                (from placeName in placeNameCache
                                 where placeName.ID == gatheringPoint.TerritoryType.PlaceNameRegionTargetID
                                 select placeName).ToList<PlaceName>();
                            if (trrPlaceRegionEntities.Count > 0)
                            {
                                gatheringPoint.TerritoryType.PlaceNameRegion = ConvertTo<Data.Model.DisplayVo.PlaceName, PlaceName>(trrPlaceRegionEntities).First();
                            }
                            List<PlaceName> trrPlaceZoneEntities =
                                (from placeName in placeNameCache
                                 where placeName.ID == gatheringPoint.TerritoryType.PlaceNameZoneTargetID
                                 select placeName).ToList<PlaceName>();
                            if (trrPlaceZoneEntities.Count > 0)
                            {
                                gatheringPoint.TerritoryType.PlaceNameZone = ConvertTo<Data.Model.DisplayVo.PlaceName, PlaceName>(trrPlaceZoneEntities).First();
                            }
                            List<Map> mapEntities =
                              (from map in mapCache
                               where map.ID == gatheringPoint.TerritoryType.MapTargetID
                               select map).ToList<Map>();
                            if (mapEntities.Count > 0)
                            {
                                gatheringPoint.TerritoryType.Map = ConvertTo<Data.Model.DisplayVo.Map, Map>(mapEntities).First();

                                List<PlaceName> mapPlaceEntities =
                                    (from placeName in placeNameCache
                                     where placeName.ID == gatheringPoint.TerritoryType.Map.PlaceNameTargetID
                                     select placeName).ToList<PlaceName>();
                                if (mapPlaceEntities.Count > 0)
                                {
                                    gatheringPoint.TerritoryType.Map.PlaceName = ConvertTo<Data.Model.DisplayVo.PlaceName, PlaceName>(mapPlaceEntities).First();
                                }
                                List<PlaceName> mapPlaceRegionEntities =
                                    (from placeName in placeNameCache
                                     where placeName.ID == gatheringPoint.TerritoryType.Map.PlaceNameRegionTargetID
                                     select placeName).ToList<PlaceName>();
                                if (mapPlaceRegionEntities.Count > 0)
                                {
                                    gatheringPoint.TerritoryType.Map.PlaceNameRegion = ConvertTo<Data.Model.DisplayVo.PlaceName, PlaceName>(mapPlaceRegionEntities).First();
                                }

                                string location = "";
                                if (gatheringPointBase.GatheringPointBaseExtension != null)
                                {
                                    location = "(X:" + gatheringPointBase.GatheringPointBaseExtension.LocationX + ",Y:" + gatheringPointBase.GatheringPointBaseExtension.LocationY + ")";
                                }

                                gatheringPointBase.Description = gatheringPoint.TerritoryType.PlaceName.Name + location + gatheringPoint.PlaceName.Name;
                                gatheringPointBase.Description_de = gatheringPoint.TerritoryType.PlaceName.Name_de + location + gatheringPoint.PlaceName.Name_de;
                                gatheringPointBase.Description_en = gatheringPoint.TerritoryType.PlaceName.Name_en + location + gatheringPoint.PlaceName.Name_en;
                                gatheringPointBase.Description_fr = gatheringPoint.TerritoryType.PlaceName.Name_fr + location + gatheringPoint.PlaceName.Name_fr;
                                gatheringPointBase.Description_ja = gatheringPoint.TerritoryType.PlaceName.Name_ja + location + gatheringPoint.PlaceName.Name_ja;
                                gatheringPointBase.Description_chs = gatheringPoint.TerritoryType.PlaceName.Name_chs + location + gatheringPoint.PlaceName.Name_chs;
                            }


                        }
                    }
                }

                return gatheringPointBase;
            }
            return null;
        }

        public static DateTime RecentGatheringTime(int gatheringPointBaseId)
        {
            Task<Data.Model.DisplayVo.GatheringPointBase> task = GetGatheringPointBaseDetail(gatheringPointBaseId);
            Task.WaitAll(task);
            Data.Model.DisplayVo.GatheringPointBase gatheringPointBase = task.Result;
            if (gatheringPointBase != null && gatheringPointBase.TimeConditionExtension != null)
            {
                DateTime eorzeaDate = Timer.EorzeaDateTimeExtension.ToEorzeaTime(DateTime.Now);
                int eorzeaHour = int.Parse(eorzeaDate.ToString("HH"));
                int compare = 48;


                foreach (Data.Model.DisplayVo.TimeConditionExtension timeConditionExtension in gatheringPointBase.TimeConditionExtension)
                {
                    int hour = (int)timeConditionExtension.Hour;
                    if (hour - eorzeaHour <= 0)
                    {
                        hour += 24;
                    }
                    if (hour - eorzeaHour <= compare)
                    {
                        compare = hour - eorzeaHour;
                    }

                }

                return DateTime.Parse(eorzeaDate.AddHours(compare).ToString("MM/dd/yyyy HH") + ":00:00");
            }
            return new DateTime();
        }
    }
}