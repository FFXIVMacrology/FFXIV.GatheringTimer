using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GatheringTimer.Timer
{
    class TimerManagement
    {

        public static List<EorzeaTimer> eorzeaTimers = new List<EorzeaTimer>();

        public static void CreateTimer(int gatheringPointId)
        {

            System.Threading.Timer timer = new System.Threading.Timer(
                new System.Threading.TimerCallback(Callback), gatheringPointId, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            DateTime eorzeaNow = EorzeaDateTimeExtension.ToEorzeaTime(DateTime.Now);
            DateTime nextEorzeaTime = Service.RecentGatheringTime(gatheringPointId);
            DateTime nextLocalTime = EorzeaDateTimeExtension.ToLocalTime(nextEorzeaTime);
            timer.Change(nextLocalTime.Subtract(DateTime.Now), Timeout.InfiniteTimeSpan);
            Logger.Info("LT:" + DateTime.Now + ",ET:" + eorzeaNow + ",NextLT:" + nextLocalTime + ",NextET:" + nextEorzeaTime);
            eorzeaTimers.Add(new EorzeaTimer()
            {
                Id = gatheringPointId,
                LocalTime = DateTime.Now,
                EorzeaTime = eorzeaNow,
                NextLocalTime = nextLocalTime,
                NextEorzeaTime = nextEorzeaTime,
                Timer = timer
            });
        }

        public static void Callback(object param)
        {
            int gatheringPointId = (int)param;
            try
            {
                System.Threading.Timer timer = (from eorzeaTimer in eorzeaTimers
                                                where eorzeaTimer.Id == gatheringPointId
                                                select eorzeaTimer).ToList<EorzeaTimer>().First().Timer;
                DateTime eorzeaNow = EorzeaDateTimeExtension.ToEorzeaTime(DateTime.Now);
                DateTime nextEorzeaTime = Service.RecentGatheringTime((int)gatheringPointId);
                DateTime nextLocalTime = EorzeaDateTimeExtension.ToLocalTime(nextEorzeaTime);
                timer.Change(nextLocalTime.Subtract(DateTime.Now), Timeout.InfiniteTimeSpan);
                Logger.Info("LT:" + DateTime.Now + ",ET:" + eorzeaNow + ",NextLT:" + nextLocalTime + ",NextET:" + nextEorzeaTime);
            }
            catch (Exception ex)
            {
                Logger.Error("Timer Error,ID:" + gatheringPointId + ",Exception:" + ex.Message);
            }
            finally
            {
                GC.Collect();
            }
        }



    }
}
