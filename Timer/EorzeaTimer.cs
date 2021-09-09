using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatheringTimer.Timer
{
    class EorzeaTimer
    {

        public int Id { get; set; }

        public DateTime LocalTime { get; set; }

        public DateTime EorzeaTime { get; set; }

        public DateTime NextLocalTime { get; set; }

        public DateTime NextEorzeaTime { get; set; }

        public System.Threading.Timer Timer { get; set; }
    }
}
