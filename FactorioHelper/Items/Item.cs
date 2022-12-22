using System.Collections.Generic;
using System.Linq;

namespace FactorioHelper.Items
{
    internal class Item : BaseItem
    {
        public decimal BuildTime { get; set; }
        public int BuildResult { get; set; }
        public IReadOnlyDictionary<int, int> Composition { get; set; }
        public bool IsSciencePack { get; set; }
        public bool ApplyRealRequirement { get; set; }

        public virtual decimal GetRealBuildTime(ProductionService productionService)
        {
            var rate = GetSpeedModuleRate(productionService);
            return rate >= 1
                ? BuildTime / rate
                : BuildTime * (1 - rate);
        }

        public decimal GetRealBuildResult(ProductionService productionService)
        {
            return BuildResult + (BuildResult * GetProductivityModuleBonus(productionService));
        }

        public decimal GetProductionRate(ProductionService productionService)
        {
            return GetRealBuildTime(productionService) / GetRealBuildResult(productionService);
        }

        public T ToItem<T>() where T : Item, new()
        {
            return new T
            {
                Id = Id,
                Name = Name,
                BuildType = BuildType,
                BuildTime = BuildTime,
                BuildResult = BuildResult,
                Composition = Composition,
                IsSciencePack = IsSciencePack,
                ApplyRealRequirement = ApplyRealRequirement,
            };
        }

        protected decimal GetSpeedModuleRate(ProductionService productionService)
        {
            var rate = productionService.StandardModulesConfiguration.ContainsKey(BuildType)
                ? productionService.StandardModulesConfiguration[BuildType].Sum(_ => _.Key.GetSpeedBonus() * _.Value)
                : 0;
            return rate;
        }

        protected decimal GetProductivityModuleBonus(ProductionService productionService)
        {
            var rate = productionService.StandardModulesConfiguration.ContainsKey(BuildType)
                ? productionService.StandardModulesConfiguration[BuildType].Sum(_ => _.Key.GetProductivityBonus() * _.Value)
                : 0;
            return rate;
        }
    }
}
