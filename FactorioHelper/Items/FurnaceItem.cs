namespace FactorioHelper.Items
{
    internal class FurnaceItem : Item
    {
        public override Fraction GetRealBuildTime(ProductionService productionService)
        {
            var sourceRate = BuildTime / productionService.FurnaceType.GetRate();
            var rate = GetSpeedModuleRate(productionService);
            return rate >= 1
                ? sourceRate / rate
                : sourceRate * (1 - rate);
        }
    }
}
