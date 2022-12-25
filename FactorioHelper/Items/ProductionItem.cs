using System;

namespace FactorioHelper.Items
{
    internal class ProductionItem : BaseItem
    {
        public int MachineRequirement => (int)Math.Ceiling(RealMachineRequirement.Decimal);
        public Fraction RealMachineRequirement { get; set; }
        public int TotalQuantityRequirement { get; set; }
        public Fraction PerSecQuantityRequirement { get; set; }
    }
}
