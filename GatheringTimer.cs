using GatheringTimer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatheringTimer
{
    interface IGatheringTimer {

        Task<bool> SetConfig(String configFilePath);

        Task<bool> SetDatabaseFile(String databaseFilePath);

        Task<bool> SetLoggerLevel(Logger.LogType logType);

        Task<bool> UpdateDataBase();

    }

    interface IGatheringTimerSource
    {
        Task<bool> UpdateXIVApiData();

        Task<bool> UpdateCafeMakerData();

        Task<bool> UpdateHuiJiWikiData();

        Task<bool> SyncDataToDatabase();

    }


    public class GatheringTimer
    {
        public static async Task Search(GatheringTimerForm gatheringTimerForm, String searchStr) {

            try
            {
                List<Data.Model.Entity.Item> items = await  DataManagement.Search(searchStr);
                gatheringTimerForm.ItemList_SetContent(items);
            }
            catch (Exception ex) {
                Logger.Error("Search Error!Exception:" + ex.Message);
            }

        }




    }
}
