namespace FactorioHelper.Items
{
    internal class PumpjackItem : Item
    {
        protected override Fraction GetRealBuildResult(ProductionService productionService)
        {
            // the yield of the field decrease over time, this is not considered here
            var buildRate = productionService.CrudeOilInitialYield / BuildResult;
            return GetRealBuildResultFromCustomBuildResult(buildRate, productionService);
        }
    }
}
