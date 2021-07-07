using System.Collections.Generic;

namespace FactorioHelper.Items
{
    abstract class Item : BaseItem
    {
        public decimal BuildTime { get; set; }
        public int BuildResult { get; set; }
        public IReadOnlyDictionary<int, int> Composition { get; set; }

        public virtual decimal GetRealBuildTime(ProductionService productionService) => BuildTime;
    }
}
