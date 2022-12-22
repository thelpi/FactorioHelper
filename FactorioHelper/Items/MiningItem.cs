namespace FactorioHelper.Items
{
    internal class MiningItem : Item
    {
        public override decimal GetRealBuildTime(ProductionService productionService)
        {
            var sourceRate = BuildTime / productionService.MiningDrillType.GetRate(productionService.MiningBonus);
            var rate = GetSpeedModuleRate(productionService);
            return rate >= 1
                ? sourceRate / rate
                : sourceRate * (1 - rate);
        }
    }
}
