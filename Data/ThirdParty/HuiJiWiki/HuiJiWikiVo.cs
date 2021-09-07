using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatheringTimer.Data.Model.Vo.HuiJiWikiVo
{

    public class GatheringPointBaseExtension
    {
        public int?  ID { get; set; }
        public String LocationX { get; set; }
        public String LocationY { get; set; }
        public int?  Orderly { get; set; }
        public int?  TimeConditions { get; set; }
        public int? Item0ID { get; set; }
        public int? Item1ID { get; set; }
        public int? Item2ID { get; set; }
        public int? Item3ID { get; set; }
        public int? Item4ID { get; set; }
        public int? Item5ID { get; set; }
        public int? Item6ID { get; set; }
        public int? Item7ID { get; set; }
    }

    public class TimeConditionExtension
    {
        public string ID { get; set; }
        public int?  GatheringPointBaseId { get; set; }
        public int?  Hour { get; set; }
        public int?  During { get; set; }
    }
}
