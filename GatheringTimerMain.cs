using Autofac;
using GatheringTimer.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GatheringTimer.Container;


namespace GatheringTimer
{

    public interface IGatheringTimerMain:IDependency {

        Task Sync();

        void SyncCancel();

    }


    public class GatheringTimerMain:IGatheringTimerMain
    {
        public static IGatheringTimerMain GetInstance() { 
      
            return Container.Container.GetContainer().Resolve<IGatheringTimerMain>();

        }

        public async Task Sync() {
            try
            {
                Logger.Info("Sync Start");
                var dataManagement = Container.Container.GetContainer().Resolve<IDataManagement>();
                await dataManagement.Sync();
            }
            catch (TaskCanceledException)
            {
                Logger.Info("Sync Cacnel");
            }
            catch (Exception ex)
            {
                Logger.Error("Sync Error", ex);
            }
            finally
            {
                Logger.Info("Sync Finish");
            }
        }

        public void SyncCancel()
        {
            var dataManagement = Container.Container.GetContainer().Resolve<IDataManagement>();
            dataManagement.SyncCancel();
        }

        public static async Task GetItems(GatheringTimerForm gatheringTimerForm, String searchStr)
        {

            try
            {
              List<Data.Model.DisplayVo.Item> items = await Service.GetItems(searchStr);
                gatheringTimerForm.ItemList_SetContent(items);
            }
            catch (Exception ex)
            {
                Logger.Error("Search Error!Exception:" + ex.Message);
            }

        }

        public static async Task<Data.Model.DisplayVo.Item> GetItem(GatheringTimerForm gatheringTimerForm, int itemId)
        {

            try
            {
                return await Service.GetItem(itemId);
            }
            catch (Exception ex)
            {
                Logger.Error("Search Error!Exception:" + ex.Message);
            }
            return null;
        }

        public static async Task GetItemDetail(GatheringTimerForm gatheringTimerForm, int itemId)
        {

            try
            {
                Data.Model.DisplayVo.Item item = await Service.GetItemDetail(itemId);
                gatheringTimerForm.DetailList_SetContent(item);
            }
            catch (Exception ex)
            {
                Logger.Error("Search Error!Exception:" + ex.Message);
            }

        }

        public static async Task GetGatheringPointBaseDetail(GatheringTimerForm gatheringTimerForm, int gatheringPointBaseId)
        {

            try
            {
                Data.Model.DisplayVo.GatheringPointBase gatheringPointBase = await Service.GetGatheringPointBaseDetail(gatheringPointBaseId);
                gatheringTimerForm.GatheringPointVIew_SetContent(gatheringPointBase);
            }
            catch (Exception ex)
            {
                Logger.Error("Search Error!Exception:" + ex.Message);
            }

        }

        public static async Task CreateTimer(GatheringTimerForm gatheringTimerForm, int gatheringPointId)
        {
            await Task.Run(() =>
            {
                try
                {
                    Timer.TimerManagement.CreateTimer(gatheringPointId);
                    gatheringTimerForm.EorzeaTimer_SetContent(Timer.TimerManagement.eorzeaTimers);
                }
                catch (Exception ex)
                {
                    Logger.Error("Timer Error!Exception:" + ex.Message);
                }
            });
        }

    }
}
