using System.Collections.Generic;

namespace FactorioHelper.Items
{
    class Item : BaseItem
    {
        public decimal BuildTime { get; set; }
        public int BuildResult { get; set; }
        public IReadOnlyDictionary<int, int> Composition { get; set; }
        public bool IsSciencePack { get; set; }
        public bool ApplyRealRequirement { get; set; }

        public virtual decimal GetRealBuildTime(ProductionService productionService) => BuildTime;

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
    }
}
