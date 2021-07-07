namespace FactorioHelper.Items
{
    class FurnaceItem : Item
    {
        public override decimal GetRealBuildTime(ProductionService productionService)
        {
            return BuildTime / productionService.FurnaceType.GetRate();
        }
    }
}
