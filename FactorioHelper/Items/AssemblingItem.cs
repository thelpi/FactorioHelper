namespace FactorioHelper.Items
{
    internal class AssemblingItem : Item
    {
        public override Fraction GetRealBuildTime(ProductionService productionService)
        {
            var sourceRate = BuildTime / productionService.AssemblingType.GetRate();
            return GetRealBuildTimeFromCustomBuildTime(sourceRate, productionService);
        }
    }
}
