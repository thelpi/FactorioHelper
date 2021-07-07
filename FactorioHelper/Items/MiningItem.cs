namespace FactorioHelper.Items
{
    class MiningItem : Item
    {
        public override decimal GetRealBuildTime(ProductionService productionService)
        {
            return BuildTime / productionService.MiningDrillType.GetRate(productionService.MiningBonus);
        }
    }
}
