using System.Collections.Generic;

namespace FactorioHelper.Items
{
    class RecipeItem : BaseItem
    {
        public decimal BuildTime { get; set; }
        public IReadOnlyDictionary<int, int> SourceItems { get; set; }
        public IReadOnlyDictionary<int, int> TargetItems { get; set; }

        public decimal GetTargetPerSec(int id)
        {
            return (TargetItems.ContainsKey(id) ? TargetItems[id] : 0) / BuildTime;
        }

        public decimal GetSourcePerSec(int id)
        {
            return (SourceItems.ContainsKey(id) ? SourceItems[id] : 0) / BuildTime;
        }
    }
}
