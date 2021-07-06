using System.Collections.Generic;

namespace FactorioHelper
{
    class BaseItem
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{Id} - {Name}";
        }
    }

    abstract class Item : BaseItem
    {
        public decimal BuildTime { get; set; }
        public int BuildResult { get; set; }
        public IReadOnlyDictionary<int, int> Composition { get; set; }

        public virtual decimal GetRealBuildTime(ProductionService productionService) => BuildTime;
    }

    class AssemblingItem : Item
    {
        public override decimal GetRealBuildTime(ProductionService productionService)
        {
            return BuildTime / productionService._assemblingType.GetRate();
        }
    }

    class FurnaceItem : Item
    {
        public override decimal GetRealBuildTime(ProductionService productionService)
        {
            return BuildTime / productionService._furnaceType.GetRate();
        }
    }

    class MiningItem : Item
    {
        public override decimal GetRealBuildTime(ProductionService productionService)
        {
            return BuildTime / productionService._miningDrillType.GetRate(productionService._miningBonus);
        }
    }

    class ChemicalItem : Item
    {
        // no specific behavior for now
    }

    class RefiningItem : Item
    {
        public override decimal GetRealBuildTime(ProductionService productionService)
        {
            // to display the quantity required by sec on the main screen
            // (realtime / buildresult = 1)
            return BuildResult;
        }
    }
}
