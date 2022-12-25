namespace FactorioHelper.Items
{
    internal class AssemblingItem : Item
    {
        public override Fraction GetRealBuildTime(ProductionService productionService)
        {
            var sourceRate = BuildTime / productionService.AssemblingType.GetRate();
            var rate = GetSpeedModuleRate(productionService);
            return rate >= 1
                ? sourceRate / rate
                : sourceRate * (1 - rate);
        }
    }
}
