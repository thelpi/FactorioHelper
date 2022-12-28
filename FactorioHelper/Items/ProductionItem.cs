using System;
using System.Collections.Generic;
using System.Linq;

namespace FactorioHelper.Items
{
    public class ProductionItem : BaseItem
    {
        private readonly Dictionary<int, ProductionComponent> _components = new Dictionary<int, ProductionComponent>();

        public int MachineRequirement => (int)Math.Ceiling(RealMachineRequirement.Decimal);
        public Fraction RealMachineRequirement { get; set; }
        public int TotalQuantityRequirement { get; set; }
        public Fraction PerSecQuantityRequirement { get; set; }
        public IReadOnlyList<ProductionComponent> Components => _components.Values.ToList();

        public void AddComponent(int id, Fraction perSecQuantityRequirement)
        {
            // crash if duplicate (that's what we want)
            _components.Add(id, new ProductionComponent
            {
                Id = id,
                PerSecQuantityRequirement = perSecQuantityRequirement
            });
        }
    }

    public class ProductionComponent
    {
        public int Id { get; set; }
        public Fraction PerSecQuantityRequirement { get; set; }
    }
}
