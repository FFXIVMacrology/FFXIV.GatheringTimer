using GatheringTimer.Data.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace GatheringTimer.Data
{
    public interface IDataConfig:IDependency {

        Dictionary<String, String> ConfigInitialization();


    }

    public class DataConfig:IDataConfig
    {
        private static Dictionary<String, String> dic = new Dictionary<String, String>();


        public Dictionary<String, String> ConfigInitialization()
        {
            DataBase();
            return dic;
        }

        private static Dictionary<String, String> DataBase()
        {
            try
            {
                dic.Add("Path", "./Plugins/FFXIV.GatheringTimer/");
                dic.Add("Filename", "GatheringTimer.db");
                dic.Add("CacheFilename", "GatheringTimerCache.db");
            }
            catch {
            }

            return dic;
        }

    }
}