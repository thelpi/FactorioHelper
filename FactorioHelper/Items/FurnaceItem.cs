namespace FactorioHelper.Items
{
    internal class FurnaceItem : Item
    {
        public override Fraction GetRealBuildTime(ProductionService productionService)
        {
            var sourceRate = BuildTime / productionService.FurnaceType.GetRate();
            return GetRealBuildTimeFromCustomBuildTime(sourceRate, productionService);
        }
    }
}
