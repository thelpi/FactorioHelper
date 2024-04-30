using System.Collections.Generic;
using System.Linq;
using FactorioHelper.Enums;
using FactorioHelper.Items;

namespace FactorioHelper
{
    internal class ProductionResult
    {
        public OilProductionOutput OilProductionOutput { get; set; }

        public IReadOnlyCollection<ProductionItem> ItemsToProduce { get; set; }

        public IReadOnlyDictionary<ItemBuildType, int> ItemBuildTypesCount { get; set; }

        internal int ComputeBuildTypeCount(ItemBuildType x)
        {
            var count = ItemsToProduce.Where(i => i.BuildType == x).Sum(i => i.MachineRequirement);
            switch (x)
            {
                case ItemBuildType.Refining:
                    return count + OilProductionOutput.RefineryRequirements.Sum(kvp => kvp.Value);
                case ItemBuildType.ChemicalPlant:
                    return count + OilProductionOutput.ChemicalPlantRequirements.Sum(kvp => kvp.Value);
                case ItemBuildType.RocketSilo:
                    // note: the silo is used for both rocket parts and space science pack
                    return count / 2;
                default:
                    return count;
            }
        }
    }
}
