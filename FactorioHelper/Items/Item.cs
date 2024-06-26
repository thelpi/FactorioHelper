﻿using System.Collections.Generic;
using System.Linq;

namespace FactorioHelper.Items
{
    internal class Item : BaseItem
    {
        public decimal BuildTime { get; set; }
        public int BuildResult { get; set; }
        public IReadOnlyDictionary<int, Fraction> Composition { get; set; }
        public bool IsSciencePack { get; set; }
        public bool ApplyRealRequirement { get; set; }

        public virtual Fraction GetRealBuildTime(ProductionService productionService)
        {
            return GetRealBuildTimeFromCustomBuildTime(BuildTime, productionService);
        }

        protected virtual Fraction GetRealBuildResult(ProductionService productionService)
        {
            return GetRealBuildResultFromCustomBuildResult(BuildResult, productionService);
        }

        public Fraction GetProductionRate(ProductionService productionService)
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
                ApplyRealRequirement = ApplyRealRequirement,
            };
        }

        protected Fraction GetSpeedModuleRate(ProductionService productionService, bool skipBonus)
        {
            var rate = productionService.StandardModulesConfiguration.ContainsKey(BuildType)
                ? productionService.StandardModulesConfiguration[BuildType].FractionSum(_ => _.Key.GetSpeedBonus(skipBonus) * _.Value)
                : 0;
            return rate;
        }

        protected Fraction GetProductivityModuleBonus(ProductionService productionService, bool skipBonus)
        {
            var rate = productionService.StandardModulesConfiguration.ContainsKey(BuildType)
                ? productionService.StandardModulesConfiguration[BuildType].Sum(_ => _.Key.GetProductivityBonus(skipBonus) * _.Value)
                : 0;
            return rate;
        }

        protected Fraction GetRealBuildTimeFromCustomBuildTime(Fraction buildTime, ProductionService productionService)
        {
            var rate = GetSpeedModuleRate(productionService, Id == ProductionService.SpaceSciencePackId);
            return rate >= 1
                ? buildTime / rate
                : buildTime * (1 - rate);
        }

        protected Fraction GetRealBuildResultFromCustomBuildResult(Fraction buildResult, ProductionService productionService)
        {
            return buildResult + (buildResult * GetProductivityModuleBonus(productionService, Id == ProductionService.SpaceSciencePackId));
        }
    }
}
