namespace FactorioHelper.Items
{
    internal class MiningItem : Item
    {
        public override Fraction GetRealBuildTime(ProductionService productionService)
        {
            var sourceRate = BuildTime / productionService.MiningDrillType.GetRate(productionService.MiningBonus);
            return GetRealBuildTimeFromCustomBuildTime(sourceRate, productionService);
        }
    }
}
