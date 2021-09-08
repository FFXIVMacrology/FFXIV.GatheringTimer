using System;
using System.Collections.Generic;
using System.Text;


namespace GatheringTimer.Data.ThirdParty.CafeMaker
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

    public class Item
    {
        public int? ID { get; set; }
        public string Name_chs { get; set; }
        public string Description_chs { get; set; }

    }

    public class SpearfishingItem
    {
        public int? ID { get; set; }

        public string Description_chs { get; set; }

    }

    public class PlaceName
    {
        public int? ID { get; set; }

        public string Name_chs { get; set; }

    }

}
