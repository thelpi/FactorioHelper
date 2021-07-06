﻿using System;

namespace FactorioHelper
{
    class ProductionItem : BaseItem
    {
        public int MachineRequirement => (int)Math.Ceiling(RealMachineRequirement);
        public decimal RealMachineRequirement { get; set; }
        public int TotalQuantityRequirement { get; set; }
        public decimal PerSecQuantityRequirement { get; set; }
    }
}
