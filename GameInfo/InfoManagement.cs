using Sharlayan.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatheringTimer.GameInfo
{
    public class InfoManagement
    {
        public Data.Model.Entity.Map GetMap(int mapId) {

            return new Data.Model.Entity.Map();
        }

        public void GetPlayerPosition() {

            ActorItem player = Memory.GetPlayer();
            if (player.Name.Length > 0 && (int)player.MapID > 0)
            {
                try
                {
                    Data.Model.Entity.Map map = GetMap((int)player.MapID);
                    double x = Math.Round(Map.ConvertCoordinatesIntoMapPosition((double)map.SizeFactor, (double)map.OffsetX, player.X), 2);
                    double y = Math.Round(Map.ConvertCoordinatesIntoMapPosition((double)map.SizeFactor, (double)map.OffsetY, player.Y), 2);

                    Logger.Debug("当前坐标:"+ String.Format("x {0} / y {1}  -  [{2} / {3}]   -   [OX: {4} / OY: {5}]", x, y, player.X, player.Y, map.OffsetX, map.OffsetY));
                }
                catch { }
            }

        }

    }
}
