using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using GatheringTimer.Data.Database;
using GatheringTimer.Data.ThirdParty.HuiJiWiki;
using GatheringTimer.Util;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace GatheringTimer.Data.ThirdParty.HuiJiWiki
{
    public static class HuiJiWikiUpdater
    {
        public static List<GatheringPointBaseExtension> GatheringPointBaseExtensionCache { get; set; } = default;
        public static List<TimeConditionExtension> TimeConditionExtensionCache { get; set; } = default;
        private static void IntoCache<T>(List<T> tlist)
        {
            typeof(HuiJiWikiUpdater).GetProperty(typeof(T).Name + "Cache").SetValue(typeof(HuiJiWikiUpdater), tlist);
        }

        private static async Task<bool> GetExtension()
        {

            ServicePointManager.DefaultConnectionLimit = 12;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            String responseData = await RequestUtil.GetResponseDataAsync("https://ff14.huijiwiki.com/wiki/GatheringTimer", "");
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(responseData);
            HtmlNode keywordNode = doc.DocumentNode.SelectSingleNode("/html/head/script[2]");
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
                                ID = int.Parse(gatheringList[gatheringProperty.Name]["ID"].ToString());
                            }
                            entity.GetType().GetProperty("Item" + index + "ID").SetValue(entity, ID);
                        }
                        baseExtensions.Add(entity);
                    }
                    if (baseExtensions.Count > 0)
                    {
                        watch.Reset();
                        watch.Start();
                        IntoCache<GatheringPointBaseExtension>(baseExtensions);
                        IntoCache<TimeConditionExtension>(timerExtension);
                        Logger.Info("Get HuiJiWiki Extension Data in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
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
            Stopwatch watch = new Stopwatch();
            watch.Start();
            Queue<Task> tasks = new Queue<Task>();
            tasks.Enqueue(GetExtension());
            await Task.WhenAll(tasks);
            Logger.Info("Finished HuiJiWikiDataUpdate in " + (watch.ElapsedMilliseconds / 1000.0) + " s");
            return true;
        }

        public static void ClearCache()
        {
            GatheringPointBaseExtensionCache = default;
            TimeConditionExtensionCache = default;
            GC.Collect();
        }
    }
}
