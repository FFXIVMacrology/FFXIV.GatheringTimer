using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatheringTimer.Data.Model.Entity
{
    public class Item
    {
        public int?  ID { get; set; }
        public String Name { get; set; }
        public String Name_de { get; set; }
        public String Name_en { get; set; }
        public String Name_fr { get; set; }
        public String Name_ja { get; set; }
        public String Name_chs { get; set; }

    }

    /// <summary>
    /// XIV GatheringItem Data
    /// </summary>
    public class GatheringItem
    {
        public int?  ID { get; set; }
        public int? IsHidden { get; set; }
        public String ItemTarget { get; set; }
        public int? ItemTargetID { get; set; }
        public String GatheringItemLevelTarget { get; set; }
        public int? GatheringItemLevelTargetID { get; set; }

    }

    public class SpearfishingItem
    {
        public string Description { get; set; }

        public string Description_de { get; set; }

        public string Description_en { get; set; }

        public string Description_fr { get; set; }

        public string Description_ja { get; set; }

        public string Description_chs { get; set; }

        public string FishingRecordTypeTarget { get; set; }

        public int? FishingRecordTypeTargetID { get; set; }

        public string GatheringItemLevelTarget { get; set; }

        public int? GatheringItemLevelTargetID { get; set; }

        public int? ID { get; set; }

        public int? IsVisible { get; set; }

        public string ItemTarget { get; set; }

        public int? ItemTargetID { get; set; }

        public string TerritoryTypeTarget { get; set; }

        public int? TerritoryTypeTargetID { get; set; }
    }

    /// <summary>
    /// XIV GatheringPointBase Data
    /// </summary>
    public class GatheringPointBase
    {
        public int? GatheringLevel { get; set; }
        public String GatheringTypeTarget { get; set; }
        public int? GatheringTypeTargetID { get; set; }
        public int?  ID { get; set; }
        public int? IsLimited { get; set; }
        public String Item0Target { get; set; }
        public int? Item0TargetID { get; set; }
        public String Item1Target { get; set; }
        public int? Item1TargetID { get; set; }
        public String Item2Target { get; set; }
        public int? Item2TargetID { get; set; }
        public String Item3Target { get; set; }
        public int? Item3TargetID { get; set; }
        public String Item4Target { get; set; }
        public int? Item4TargetID { get; set; }
        public String Item5Target { get; set; }
        public int? Item5TargetID { get; set; }
        public String Item6Target { get; set; }
        public int? Item6TargetID { get; set; }
        public String Item7Target { get; set; }
        public int? Item7TargetID { get; set; }
    }

    /// <summary>
    /// XIV GatheringPoint Data
    /// </summary>
    public class GatheringPoint
    {
        public int? Count { get; set; }

        public String GatheringPointBaseTarget { get; set; }

        public int? GatheringPointBaseTargetID { get; set; }

        public String GatheringPointBonus0Target { get; set; }

        public int? GatheringPointBonus0TargetID { get; set; }

        public String GatheringPointBonus1Target { get; set; }

        public int? GatheringPointBonus1TargetID { get; set; }

        public String GatheringSubCategoryTarget { get; set; }

        public int? GatheringSubCategoryTargetID { get; set; }

        public int?  ID { get; set; }

        public int? Patch { get; set; }

        public String PlaceNameTarget { get; set; }

        public int? PlaceNameTargetID { get; set; }

        public String TerritoryTypeTarget { get; set; }

        public int? TerritoryTypeTargetID { get; set; }

        public int? Type { get; set; }
    }

    public class PlaceName
    {
        public int?  ID { get; set; }

        public string Name { get; set; }

        public string Name_de { get; set; }

        public string Name_en { get; set; }

        public string Name_fr { get; set; }

        public string Name_ja { get; set; }

        public string Name_chs { get; set; }
    }

    public class TerritoryType
    {

        public string AetheryteTarget { get; set; }

        public int?  AetheryteTargetID { get; set; }

        public int?  ID { get; set; }

        public string MapTarget { get; set; }

        public int?  MapTargetID { get; set; }

        public string PlaceNameRegionTarget { get; set; }

        public int?  PlaceNameRegionTargetID { get; set; }

        public string PlaceNameTarget { get; set; }

        public int?  PlaceNameTargetID { get; set; }

        public string PlaceNameZoneTarget { get; set; }

        public int?  PlaceNameZoneTargetID { get; set; }
    }

    /// <summary>
    /// XIV Map Data
    /// </summary>
    public class Map
    {

        public int?  ID { get; set; }

        public int? OffsetX { get; set; }

        public int? OffsetY { get; set; }

        public String PlaceNameRegionTarget { get; set; }

        public int? PlaceNameRegionTargetID { get; set; }

        public String PlaceNameTarget { get; set; }

        public int? PlaceNameTargetID { get; set; }

        public int? SizeFactor { get; set; }

    }

    public class GatheringPointBaseExtension
    {
        public int?  ID { get; set; }
        public String LocationX { get; set; }
        public String LocationY { get; set; }
        public int?  Orderly { get; set; }
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

    public class FavouriteItem {

        public String Id { get; set; }

        public String itemId { get; set; }
    }

    public class FavouritePoint
    {

        public String Id { get; set; }

        public String PointBaseId { get; set; }
    }

    public class TimerEnable {

        public String Id { get; set; }

        public String PointBaseId{ get; set; }

    }

}
