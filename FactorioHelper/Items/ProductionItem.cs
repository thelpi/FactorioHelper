using System;

namespace FactorioHelper.Items
{
    internal class ProductionItem : BaseItem
    {
        public int MachineRequirement => (int)Math.Ceiling(RealMachineRequirement);
        public decimal RealMachineRequirement { get; set; }
        public int TotalQuantityRequirement { get; set; }
        public decimal PerSecQuantityRequirement { get; set; }
    }
}
