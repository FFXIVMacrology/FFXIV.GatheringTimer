using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatheringTimer.GameInfo
{
    public static class Map
    {
        //
        // Using ingame coordinate to map position in pixels
        //
        public static double ConvertCoordinatesIntoMapPosition(double scale, double offset, double val)
        {
            val = Math.Round(val, 3);
            val = (val + offset) * scale;
            return ((41.0 / scale) * ((val + 1024.0) / 2048.0)) + 1;
        }

        //
        // Convert map position to pixels
        //
        public static int ConvertMapPositionToPixels(double value, double scale)
        {
            return Convert.ToInt32((value - 1) * 50 * scale);
        }



    }
}
