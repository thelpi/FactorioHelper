namespace FactorioHelper.Items
{
    class RefiningItem : Item
    {
        public override decimal GetRealBuildTime(ProductionService productionService)
        {
            // to display the quantity required by sec on the main screen
            // (realtime / buildresult = 1)
            return BuildResult;
        }
    }
}
