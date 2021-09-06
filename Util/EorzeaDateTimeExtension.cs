using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatheringTimer.Util
{
    public static class EorzeaDateTimeExtension
    {
        public static DateTime ToEorzeaTime(this DateTime date)
        {
            const double EORZEA_MULTIPLIER = 3600D / 175D;

            // Calculate how many ticks have elapsed since 1/1/1970
            long epochTicks = date.ToUniversalTime().Ticks - (new DateTime(1970, 1, 1).Ticks);

            // Multiply those ticks by the Eorzea multipler (approx 20.5x)
            long eorzeaTicks = (long)Math.Round(epochTicks * EORZEA_MULTIPLIER);

            return new DateTime(eorzeaTicks);
        }
    }
}
