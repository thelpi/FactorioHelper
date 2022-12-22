using System.Collections.Generic;

namespace FactorioHelper
{
    internal class OilProductionOutput
    {
        public IReadOnlyDictionary<int, decimal> RemainsPerSec { get; set; }
        public IReadOnlyDictionary<int, int> ChemicalPlantRequirements { get; set; }
        public IReadOnlyDictionary<int, int> RefineryRequirements { get; set; }
    }
}
