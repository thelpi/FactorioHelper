using System.Collections.Generic;
using FactorioHelper.Enums;
using FactorioHelper.Items;

namespace FactorioHelper
{
    internal class ProductionResult
    {
        public OilProductionOutput OilProductionOutput { get; set; }

        public IReadOnlyCollection<ProductionItem> ItemsToProduce { get; set; }

        public IReadOnlyDictionary<ItemBuildType, int> ItemBuildTypesCount { get; set; }
    }
}
