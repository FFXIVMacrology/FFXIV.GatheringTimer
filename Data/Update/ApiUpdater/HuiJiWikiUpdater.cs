using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using GatheringTimer.Data.Database;
using GatheringTimer.Data.Model.Vo.HuiJiWikiVo;
using GatheringTimer.Data.Update.Config;
using GatheringTimer.Util;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace GatheringTimer.Data.Update.ApiSync
{
    public static class HuiJiWikiUpdater
    {

        private static readonly Dictionary<String, String> config = HuiJiWikiConfig.ConfigInitialization();

        private static readonly SQLiteDatabase cacheDatabase = new SQLiteDatabase();

        public static SQLiteDatabase GetSQLiteDatabase()
        {
            return cacheDatabase;
        }

        /// <summary>
        /// Pretreat url and update T list with async(HuiJiWiki)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="entityList"></param>
        /// <param name="keywordName"></param>
        /// <param name="searchwordName"></param>
        /// <param name="keywordNodeStr"></param>
        /// <param name="searchwordNodeStr"></param>
        /// <returns></returns>
        private static async Task<List<T>> UpdateFFWikiHandleAsync<T>(String url, List<T> entityList, String keywordName, String searchwordName, String keywordNodeStr, String searchwordNodeStr)
        {
            ServicePointManager.DefaultConnectionLimit = 100;
            Queue<Task> tasks = new Queue<Task>();
            foreach (T t in entityList)
            {
                String keyword = t.GetType().GetProperty(keywordName).GetValue(t, null).ToString();
                String urlString = url.Replace("[" + keywordName + "]", keyword);
                Task<List<T>> task = UpdateListAsync(urlString, entityList, keywordName, searchwordName, keywordNodeStr, searchwordNodeStr);
                tasks.Enqueue(task);
            }
            await Task.WhenAll(tasks);
            return entityList;
        }

        /// <summary>
        /// Get response with url ,parse it with Node info and update T list with replace colum value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="keywordList"></param>
        /// <param name="keywordName"></param>
        /// <param name="searchwordName"></param>
        /// <param name="keywordNodeStr"></param>
        /// <param name="searchwordNodeStr"></param>
        /// <returns></returns>
        private static async Task<List<T>> UpdateListAsync<T>(String url, List<T> keywordList, String keywordName, String searchwordName, String keywordNodeStr, String searchwordNodeStr)
        {
            String responseData = await RequestUtil.GetResponseDataAsync(url, "");
            await Task.Run(() =>
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(responseData);
                HtmlNode keywordNode = doc.DocumentNode.SelectSingleNode(keywordNodeStr);
                HtmlNode searchwordNode = doc.DocumentNode.SelectSingleNode(searchwordNodeStr);
                if (!(searchwordNode is null) && !(keywordNode is null))
                {
                    T t = keywordList.Find(s => s.GetType().GetProperty(keywordName).GetValue(s, null).ToString().Equals(keywordNode.InnerText));
                    t.GetType().GetProperty(searchwordName).SetValue(t, searchwordNode.InnerText);
                    Logger.Debug("Keyword " + keywordNode.InnerText + " find,the value is " + searchwordNode.InnerText);
                }

            });
            return keywordList;
        }

        /// <summary>
        /// Reinitialize DataBase
        /// </summary>
        private static bool CacheInitialization()
        {
            Logger.Info("HuiJiWikiCache File Reinitializing");
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
                Logger.Info("HuiJiWikiCache File Already");
                return true;
            }

        }

        private static async Task<bool> GetRawData<T>()
        {
            Logger.Info("Load HuiJiWiki " + typeof(T).Name + " Data Start");
            Stopwatch watch = new Stopwatch();
            watch.Start();
            String url = config[typeof(T).Name];
            String keywordNodeStr = config[typeof(T).Name + "KeywordNodeStr"];
            String searchwordNodeStr = config[typeof(T).Name + "SearchwordNodeStr"];
            Logger.Info("Loaded HuiJiWiki " + typeof(T).Name + " Data in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
            watch.Reset();
            watch.Start();
            Logger.Info("Get HuiJiWiki " + typeof(T).Name + " Data");
            DataTable dataTable = Updater.GetSQLiteDatabase().Select("Select * from " + typeof(T).Name);
            List<T> dataList = DataToModel.DataTableToList<T>(dataTable);
            Task<List<T>> taskItem = UpdateFFWikiHandleAsync<T>(url, dataList, "ID", "Name_chs", keywordNodeStr, searchwordNodeStr);
            Task.WaitAll(taskItem);
            Logger.Info("Get HuiJiWiki " + typeof(T).Name + " Data in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
            watch.Reset();
            watch.Start();
            Logger.Info("Save HuiJiWiki Cache " + typeof(T).Name + " Start");
            bool inserted = await cacheDatabase.InsertRowByRow<T>((List<T>)taskItem.Result);
            //await DataManagement.sqliteDatabase.UpdateRowByRow<T>(dataList, new List<String> { "Name_chs" }, new List<String> { "ID" });
            Logger.Info("Saved HuiJiWiki Cache " + typeof(T).Name + " in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
            return inserted;
        }

        private static async Task<bool> GetExtension()
        {

            ServicePointManager.DefaultConnectionLimit = 12;
            Logger.Info("Get HuiJiWiki Extension Data");
            Stopwatch watch = new Stopwatch();
            watch.Start();
            String responseData = await RequestUtil.GetResponseDataAsync("https://ff14.huijiwiki.com/wiki/GatheringTimer", "");
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(responseData);
            HtmlNode keywordNode = doc.DocumentNode.SelectSingleNode("/html/head/script[2]");
            Logger.Info("Get HuiJiWiki Extension Data in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
            if (!(keywordNode is null))
            {
                string scriptStr = keywordNode.InnerText;
                string gatheringTimerJson = scriptStr.Substring(
                    scriptStr.IndexOf("\"node_data\":") + 12,
                    scriptStr.Length - (scriptStr.IndexOf("\"node_data\":") + 12) - (scriptStr.Length - scriptStr.IndexOf("},\"wgCommentsSortDescending\""))

                    );
                Logger.Debug("Keyword \"node_data\": find,the value is " + gatheringTimerJson);
                try
                {
                    List<GatheringPointBaseExtension> baseExtensions = new List<GatheringPointBaseExtension>();
                    List<TimeConditionExtension> timerExtension = new List<TimeConditionExtension>();
                    JObject jsonObject = JObject.Parse(gatheringTimerJson);
                    IEnumerable<JProperty> properties = jsonObject.Properties();
                    foreach (JProperty property in properties)
                    {
                        GatheringPointBaseExtension entity = new GatheringPointBaseExtension();
                        entity.ID = int.Parse(property.Name);
                        entity.LocationX = jsonObject[property.Name]["位置"]["X"].ToString();
                        entity.LocationY = jsonObject[property.Name]["位置"]["Y"].ToString();
                        entity.Orderly = jsonObject[property.Name]["无序列表"].ToString().ToLowerInvariant().Equals("false") ? 1 : 0;
                        entity.TimeConditions = jsonObject[property.Name]["时间条件"] !=null?(jsonObject[property.Name]["时间条件"].ToString().ToLowerInvariant().Equals("true")?1:0):0;

                        if (1 == entity.TimeConditions) {
                            JObject timerList = (JObject)jsonObject[property.Name]["出现时间数据"];
                            IEnumerable<JProperty> timerProperties = timerList.Properties();
                            foreach (JProperty timerProperty in timerProperties)
                            {
                                TimeConditionExtension timer = new TimeConditionExtension();
                                timer.ID = Guid.NewGuid().ToString("N");
                                timer.GatheringPointBaseId = entity.ID;
                                timer.Hour = int.Parse(timerList[timerProperty.Name]["hour"].ToString());
                                timer.During = int.Parse(timerList[timerProperty.Name]["dur"].ToString());
                                timerExtension.Add(timer);
                            }
                        }                       


                        JObject gatheringList = (JObject)jsonObject[property.Name]["物品列表"];
                        IEnumerable<JProperty> gatheringProperties = gatheringList.Properties();
                        foreach (JProperty gatheringProperty in gatheringProperties)
                        {
                            bool hasID = gatheringList[gatheringProperty.Name]["has_ID"].ToString().ToLowerInvariant().Equals("true");
                            int index = int.Parse(gatheringList[gatheringProperty.Name]["index"].ToString()) - 1;
                            int ID = 0;
                            if (hasID)
                            {
                                ID = int.Parse(gatheringList[gatheringProperty.Name]["ID"].ToString()) - 1;
                            }
                            entity.GetType().GetProperty("Item" + index + "ID").SetValue(entity, ID);
                        }
                        baseExtensions.Add(entity);
                    }
                    if (baseExtensions.Count > 0)
                    {
                        Logger.Info("Init CafeMaker Cache Extension Start");
                        watch.Reset();
                        watch.Start();
                        await cacheDatabase.DeleteTable<GatheringPointBaseExtension>();
                        await cacheDatabase.CreateTable<GatheringPointBaseExtension>();
                        await cacheDatabase.DeleteTable<TimeConditionExtension>();
                        await cacheDatabase.CreateTable<TimeConditionExtension>();
                        Logger.Info("Inited CafeMaker Cache Extension in " + (watch.ElapsedMilliseconds / 1000.0) + " s");

                        Logger.Info("Save HuiJiWiki Cache Extension Start");
                        watch.Reset();
                        watch.Start();
                        await cacheDatabase.InsertRowByRow<GatheringPointBaseExtension>(baseExtensions);
                        await cacheDatabase.InsertRowByRow<TimeConditionExtension>(timerExtension);
                        Logger.Info("Saved HuiJiWiki Cache Extension in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
                    }
                    return true;

                }
                catch (Exception ex)
                {
                    Logger.Error("Json To Object Error,Exception:" + ex.Message);
                    return false;
                }
            }
            else
            {
                Logger.Error("Get HuiJiWiki GatheringPointBaseExtension Data Fail", null);
                return false;
            }
        }

        public static async Task<bool> HuiJiWikiDataUpdate()
        {

            CacheInitialization();

            Logger.Info("Loading Config");
            Stopwatch watch = new Stopwatch();
            watch.Start();
            String dataSource = config["Path"] + config["Filename"];
            Logger.Info("Loaded Config in " + (watch.ElapsedMilliseconds / 1000.0) + " s");

            Logger.Info("HuiJiWikiDataUpdate Start");
            watch.Reset();
            watch.Start();
            Queue<Task> tasks = new Queue<Task>();
            tasks.Enqueue(GetExtension());
            //tasks.Enqueue(GetRawData<Item>());
            await Task.WhenAll(tasks);
            Logger.Info("Finished HuiJiWikiDataUpdate in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
            return true;
        }



    }
}
