using GatheringTimer.Data.Database;
using GatheringTimer.Data.Model.Entity;
using GatheringTimer.Data.Update;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GatheringTimer.Data
{
    public class Service
    {

        private static readonly Dictionary<String, String> config = DataConfig.ConfigInitialization();

        private static readonly SQLiteDatabase sqliteDatabase = new SQLiteDatabase();

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

        public static async Task<List<Item>> SearchItem(String searchStr)
        {
            return await sqliteDatabase.Select<Item>(
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
                5
            );
        }

        public static async Task<Item> GetItem(int ID)
        {
            List<Item> items = await sqliteDatabase.Select<Item>(
                new List<string>() {
                    "ID"
                },
                new List<string>() {
                "="
                },
                new Item()
                {
                    ID = ID
                }
            );


            return items.Count > 0 ? items[0] : null;
        }

        public static async Task<List<GatheringItem>> SearchGatheringItem(int itemTargetID)
        {
            return await sqliteDatabase.Select<GatheringItem>(
                new List<string>() {
                    "ItemTargetID"
                },
                new List<string>() {
                "="
                },
                new GatheringItem()
                {
                    ItemTargetID = itemTargetID
                }
            );
        }

        public static async Task<List<GatheringPointBase>> SearchGatheringPointBase(int GatheringItemID)
        {
            return await sqliteDatabase.Select<GatheringPointBase>(
                new List<string>() {
                    "=", "=", "=", "=", "=", "=", "=", "=", "=", "=", "=", "=", "=", "=", "=", "="
                },
                new GatheringPointBase()
                {
                    Item0Target = "GatheringItem",
                    Item1Target = "GatheringItem",
                    Item2Target = "GatheringItem",
                    Item3Target = "GatheringItem",
                    Item4Target = "GatheringItem",
                    Item5Target = "GatheringItem",
                    Item6Target = "GatheringItem",
                    Item7Target = "GatheringItem",
                    Item0TargetID = GatheringItemID,
                    Item1TargetID = GatheringItemID,
                    Item2TargetID = GatheringItemID,
                    Item3TargetID = GatheringItemID,
                    Item4TargetID = GatheringItemID,
                    Item5TargetID = GatheringItemID,
                    Item6TargetID = GatheringItemID,
                    Item7TargetID = GatheringItemID
                }
            );
        }

        public static async Task<List<Item>> Search(String searchStr)
        {
            if (DatabaseInitialization())
            {
                List<Item> items = await SearchItem(searchStr);

                foreach (Item item in items)
                {
                    await Task.Run(() =>
                    {
                        Logger.Info(item.ID + ":" + item.Name_chs);
                    });

                }

                return items;

            }
            return null;

        }

        public static async Task<List<Model.Vo.DisplayVo.Item>> SearchDetail(int ID)
        {
            if (DatabaseInitialization())
            {
                Model.Vo.DisplayVo.Item itemVo = ConvertTo<Model.Vo.DisplayVo.Item, Item>(await GetItem(ID));
                List<GatheringItem> gatheringItems = await sqliteDatabase.Select<GatheringItem>(new List<string>(), new GatheringItem());
                List<GatheringPointBase> gatheringPointBases = await sqliteDatabase.Select<GatheringPointBase>(new List<string>(), new GatheringPointBase());
                List<GatheringPointBaseExtension> gatheringPointBaseExtensions = await sqliteDatabase.Select<GatheringPointBaseExtension>(new List<string>(), new GatheringPointBaseExtension());
                List<TimeConditionExtension> timeConditionExtensions = await sqliteDatabase.Select<TimeConditionExtension>(new List<string>(), new TimeConditionExtension());
                List<GatheringPoint> gatheringPoints = await sqliteDatabase.Select<GatheringPoint>(new List<string>(), new GatheringPoint());
                List<PlaceName> placeNames = await sqliteDatabase.Select<PlaceName>(new List<string>(), new PlaceName());
                List<TerritoryType> territoryTypes = await sqliteDatabase.Select<TerritoryType>(new List<string>(), new TerritoryType());
                List<Map> maps = await sqliteDatabase.Select<Map>(new List<string>(), new Map());

                gatheringItems = (from gatheringItem in gatheringItems where gatheringItem.ItemTargetID == ID select gatheringItem).ToList<GatheringItem>();
                if (gatheringItems.Count > 0)
                {
                    itemVo.GatheringItem = ConvertTo<Model.Vo.DisplayVo.GatheringItem, GatheringItem>(gatheringItems)[0];

                    List<GatheringPointBase> entities =
                        (from gatheringPointBase in gatheringPointBases
                         where gatheringPointBase.Item0TargetID == itemVo.GatheringItem.ID
                     || gatheringPointBase.Item1TargetID == itemVo.GatheringItem.ID
                     || gatheringPointBase.Item2TargetID == itemVo.GatheringItem.ID
                     || gatheringPointBase.Item3TargetID == itemVo.GatheringItem.ID
                     || gatheringPointBase.Item4TargetID == itemVo.GatheringItem.ID
                     || gatheringPointBase.Item5TargetID == itemVo.GatheringItem.ID
                     || gatheringPointBase.Item6TargetID == itemVo.GatheringItem.ID
                     || gatheringPointBase.Item7TargetID == itemVo.GatheringItem.ID
                         select gatheringPointBase).ToList<GatheringPointBase>();

                    if (entities.Count > 0)
                    {
                        itemVo.GatheringItem.GatheringPointBases = ConvertTo<Model.Vo.DisplayVo.GatheringPointBase, GatheringPointBase>(entities);
                        foreach (Model.Vo.DisplayVo.GatheringPointBase gatheringPointBase in itemVo.GatheringItem.GatheringPointBases)
                        {
                            List<GatheringPointBaseExtension> exEntities =
                               (from gatheringPointBaseExtension in gatheringPointBaseExtensions
                                where gatheringPointBaseExtension.ID == gatheringPointBase.ID
                                select gatheringPointBaseExtension).ToList<GatheringPointBaseExtension>();
                            if (exEntities.Count > 0)
                            {
                                gatheringPointBase.GatheringPointBaseExtension = ConvertTo<Model.Vo.DisplayVo.GatheringPointBaseExtension, GatheringPointBaseExtension>(exEntities).First();
                            }
                            List<TimeConditionExtension> timeEntities =
                               (from timeConditionExtension in timeConditionExtensions
                                where timeConditionExtension.GatheringPointBaseId == gatheringPointBase.ID
                                select timeConditionExtension).ToList<TimeConditionExtension>();
                            if (timeEntities.Count > 0)
                            {
                                gatheringPointBase.TimeConditionExtension = ConvertTo<Model.Vo.DisplayVo.TimeConditionExtension, TimeConditionExtension>(timeEntities);
                            }
                            List<GatheringPoint> pointEntities =
                               (from gatheringPoint in gatheringPoints
                                where gatheringPoint.GatheringPointBaseTargetID == gatheringPointBase.ID
                                select gatheringPoint).ToList<GatheringPoint>();
                            if (pointEntities.Count > 0)
                            {
                                gatheringPointBase.GatheringPoint = ConvertTo<Model.Vo.DisplayVo.GatheringPoint, GatheringPoint>(pointEntities).First();
                                List<PlaceName> placeEntities =
                                  (from placeName in placeNames
                                   where placeName.ID == gatheringPointBase.GatheringPoint.PlaceNameTargetID
                                   select placeName).ToList<PlaceName>();
                                if (placeEntities.Count > 0)
                                {
                                    gatheringPointBase.GatheringPoint.PlaceName = ConvertTo<Model.Vo.DisplayVo.PlaceName, PlaceName>(placeEntities).First();
                                }
                                List<TerritoryType> territoryEntities =
                                  (from territoryType in territoryTypes
                                   where territoryType.ID == gatheringPointBase.GatheringPoint.TerritoryTypeTargetID
                                   select territoryType).ToList<TerritoryType>();
                                if (territoryEntities.Count > 0)
                                {
                                    gatheringPointBase.GatheringPoint.TerritoryType = ConvertTo<Model.Vo.DisplayVo.TerritoryType, TerritoryType>(territoryEntities).First();
                                    List<Map> mapEntities =
                                      (from map in maps
                                       where map.ID == gatheringPointBase.GatheringPoint.TerritoryType.MapTargetID
                                       select map).ToList<Map>();
                                    if (mapEntities.Count > 0)
                                    {
                                        gatheringPointBase.GatheringPoint.TerritoryType.Map = ConvertTo<Model.Vo.DisplayVo.Map, Map>(mapEntities).First();
                                    }
                                }



                            }




                        }
                    }






                }
                //List<GatheringItem> gatheringItems = await SearchGatheringItem(ID);
                //GatheringItem gatheringItem = gatheringItems[0];
                //List<GatheringPointBase> gatheringPointBases = await SearchGatheringPointBase(gatheringItem.ID);

                Logger.Info(JsonConvert.SerializeObject(itemVo));
            }
            return null;

        }

    }
}
