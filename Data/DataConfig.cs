using GatheringTimer.Data.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace GatheringTimer.Data
{
    public static class DataConfig
    {
        public static Dictionary<String, String> dic = new Dictionary<String, String>();

        public static Dictionary<String, String> ConfigInitialization()
        {
            DataBase();
            return dic;
        }

        private static Dictionary<String, String> DataBase()
        {
            try
            {
                dic.Add("Path", "./");
                dic.Add("Filename", "GatheringTimer.db");
                dic.Add("CacheFilename", "GatheringTimerCache.db");
            }
            catch {
            }

            return dic;
        }

    }
}