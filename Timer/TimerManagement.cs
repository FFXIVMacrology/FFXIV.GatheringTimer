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
            AddTimer(gatheringPointId);
        }

        public static void Callback(object param)
        {
            int gatheringPointId = (int)param;
            AddTimer(gatheringPointId);
        }

        private static void AddTimer(int gatheringPointId)
        {
            List<EorzeaTimer> eorzeaTimerSelect = (from eorzeaTimerEntity in eorzeaTimers where eorzeaTimerEntity.Id == gatheringPointId select eorzeaTimerEntity).ToList<EorzeaTimer>();
            DateTime eorzeaNow = EorzeaDateTimeExtension.ToEorzeaTime(DateTime.Now);
            DateTime nextEorzeaTime = Service.RecentGatheringTime(gatheringPointId);
            DateTime nextLocalTime = EorzeaDateTimeExtension.ToLocalTime(nextEorzeaTime);
            System.Threading.Timer timer;
            if (eorzeaTimerSelect.Count > 0)
            {
                EorzeaTimer eorzeaTimer = eorzeaTimerSelect.First();
                timer = eorzeaTimer.Timer;
            }
            else
            {
                timer = new System.Threading.Timer(
                    new System.Threading.TimerCallback(Callback),
                    gatheringPointId,
                    Timeout.InfiniteTimeSpan,
                    Timeout.InfiniteTimeSpan
                    );
                Data.Model.DisplayVo.GatheringPointBase gatheringPointBase = Service.GetGatheringPointBaseDetail(gatheringPointId).Result;
                eorzeaTimers.Add(new EorzeaTimer()
                {
                    Id = gatheringPointId,
                    Name = gatheringPointBase.Description_chs,
                    LocalTime = DateTime.Now,
                    EorzeaTime = eorzeaNow,
                    NextLocalTime = nextLocalTime,
                    NextEorzeaTime = nextEorzeaTime,
                    Timer = timer
                });

            }
            timer.Change(nextLocalTime.Subtract(DateTime.Now), Timeout.InfiniteTimeSpan);
            Logger.Info("LT:" + DateTime.Now + ",ET:" + eorzeaNow + ",NextLT:" + nextLocalTime + ",NextET:" + nextEorzeaTime);
        }

    }
}
