using System;
using System.Collections.Generic;
using System.Text;


namespace GatheringTimer.Data.Model.Vo.CafeMakerVo
{
    public class Pagination
    {
        public int? Page { get; set; }

        public int? PageNext { get; set; }

        public int? PagePrev { get; set; }

        public int? PageTotal { get; set; }

        public int? Results { get; set; }

        public int? ResultsPerPage { get; set; }

        public int? ResultsTotal { get; set; }
    }

    /// <summary>
    /// XIV Item Data
    /// </summary>
    public class Item
    {
        public int?  ID { get; set; }
        public String Name_chs { get; set; }
    }

}
