using System;

namespace FactorioHelper.Items
{
    public class ProductionComponent
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Fraction PerSecQuantityRequirement { get; set; }
        public Fraction UseRate { get; set; }
        public string PerSecInformation => $"{Name}\r\n{PerSecQuantityRequirement}\r\n{Math.Round(UseRate.Decimal * 100, 1)} %";
    }
}
