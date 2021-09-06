using GatheringTimer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatheringTimer
{
    interface IGatheringTimer {

    }

    interface IGatheringTimerSource
    {

        Task<bool> ClearCache();

        Task<bool> SyncDataToDatabase();

    }

    interface IGatheringTimerService
    {

        Task<List<Data.Model.Vo.DisplayVo.Item>> GetItems(GatheringTimerForm gatheringTimerForm, String searchStr);

        Task<List<Data.Model.Vo.DisplayVo.Item>> GetItemDetail(GatheringTimerForm gatheringTimerForm, String itemId);


    }


    public class GatheringTimer
    {
        public static async Task GetItem(GatheringTimerForm gatheringTimerForm, String searchStr)
        {

            try
            {
                List<Data.Model.Vo.DisplayVo.Item> items = await Service.GetItems(searchStr);
                gatheringTimerForm.ItemList_SetContent(items);
            }
            catch (Exception ex)
            {
                Logger.Error("Search Error!Exception:" + ex.Message);
            }

        }

        public static async Task GetItems(GatheringTimerForm gatheringTimerForm, String searchStr)
        {

            try
            {
                List<Data.Model.Vo.DisplayVo.Item> items = await Service.GetItemDetail(int.Parse(searchStr));
                gatheringTimerForm.ItemList_SetContent(items);
            }
            catch (Exception ex)
            {
                Logger.Error("Search Error!Exception:" + ex.Message);
            }

        }

    }
}
