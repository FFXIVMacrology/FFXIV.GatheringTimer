using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using GatheringTimer.Data.Model.Vo.HuiJiWikiVo;
using HtmlAgilityPack;

namespace GatheringTimer.Data.Update.Config
{
    public static class HuiJiWikiConfig
    {
        public static Dictionary<String, String> dic = new Dictionary<String, String>();

        public static Dictionary<String, String> ConfigInitialization()
        {
            DataBase();
            HuiJiWikiUrl();
            return dic;
        }

        private static Dictionary<String, String> DataBase()
        {
            dic.Add("Path", "./");
            dic.Add("Filename", "HuiJiWikiCache.db");
            return dic;
        }

        private static Dictionary<String, String> HuiJiWikiUrl()
        {
            dic.Add("Item", "https://ff14.huijiwiki.com/wiki/Data:Item/[ID].json");
            dic.Add("ItemKeywordNodeStr", "//div[@id='mw-content-text']/table[1]/tbody[1]/tr[1]/td[1]");
            dic.Add("ItemSearchwordNodeStr", "//div[@id='mw-content-text']/table[1]/tbody[1]/tr[4]/td[1]");
            return dic;
        }

    }
}