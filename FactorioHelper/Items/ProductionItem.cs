using System;
using System.Collections.Generic;
using System.Linq;

namespace FactorioHelper.Items
{
    public class ProductionItem : BaseItem
    {
        private readonly Dictionary<int, ProductionComponent> _components = new Dictionary<int, ProductionComponent>();

        public Fraction MachineRequirementRate => RealMachineRequirement / MachineRequirement;
        public int MachineRequirement => (int)Math.Ceiling(RealMachineRequirement.Decimal);
        public Fraction RealMachineRequirement { get; set; }
        public int TotalQuantityRequirement { get; set; }
        public Fraction PerSecQuantityRequirement { get; set; }
        public IReadOnlyList<ProductionComponent> Components => _components.Values.ToList();

        public void AddComponent(int id, string name, Fraction perSecQuantityRequirement)
        {
            // crash if duplicate (that's what we want)
            _components.Add(id, new ProductionComponent
            {
                Id = id,
                Name = name, 
                PerSecQuantityRequirement = perSecQuantityRequirement
            });
        }

        public void SetComponentUseRate(int id, Fraction totalPerSec)
        {
            _components[id].UseRate = _components[id].PerSecQuantityRequirement / totalPerSec;
        }
    }
}
