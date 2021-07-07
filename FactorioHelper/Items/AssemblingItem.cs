namespace FactorioHelper.Items
{
    class AssemblingItem : Item
    {
        public override decimal GetRealBuildTime(ProductionService productionService)
        {
            return BuildTime / productionService.AssemblingType.GetRate();
        }
    }
}
