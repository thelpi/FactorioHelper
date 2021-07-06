using System.Collections.Generic;

namespace FactorioHelper
{
    class OilProductionOutput
    {
        public IReadOnlyDictionary<Recipe, int> ChemicalPlantRequirements { get; set; }
        public IReadOnlyDictionary<Recipe, int> RefineryRequirements { get; set; }
    }
}
