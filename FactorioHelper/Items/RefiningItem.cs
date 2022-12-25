namespace FactorioHelper.Items
{
    internal class RefiningItem : Item
    {
        public override Fraction GetRealBuildTime(ProductionService productionService)
        {
            // to display the quantity required by sec on the main screen
            // (realtime / buildresult = 1)
            // refinery type is excluded from module configuration, so result will always be 1
            return GetRealBuildResult(productionService);
        }
    }
}
