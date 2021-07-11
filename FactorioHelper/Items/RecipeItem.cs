﻿using System.Collections.Generic;

namespace FactorioHelper.Items
{
    class RecipeItem : BaseItem
    {
        public decimal BuildTime { get; set; }
        public IReadOnlyDictionary<int, int> SourceItems { get; set; }
        public IReadOnlyDictionary<int, int> TargetItems { get; set; }

        public readonly Dictionary<int, decimal> _sourceBuildTimes = new Dictionary<int, decimal>();
        public readonly Dictionary<int, decimal> _targetBuildTimes = new Dictionary<int, decimal>();
        public readonly Dictionary<int, decimal> _deltaBuildTimes = new Dictionary<int, decimal>();

        public decimal GetTargetPerSec(int id)
        {
            if (!_targetBuildTimes.ContainsKey(id))
            {
                _targetBuildTimes.Add(id, (TargetItems.ContainsKey(id) ? TargetItems[id] : 0) / BuildTime);
            }
            return _targetBuildTimes[id];
        }

        public decimal GetSourcePerSec(int id)
        {
            if (!_sourceBuildTimes.ContainsKey(id))
            {
                _sourceBuildTimes.Add(id, (SourceItems.ContainsKey(id) ? SourceItems[id] : 0) / BuildTime);
            }
            return _sourceBuildTimes[id];
        }

        public decimal GetDeltaPerSec(int id)
        {
            if (!_deltaBuildTimes.ContainsKey(id))
            {
                _deltaBuildTimes.Add(id, ((TargetItems.ContainsKey(id) ? TargetItems[id] : 0) - (SourceItems.ContainsKey(id) ? SourceItems[id] : 0)) / BuildTime);
            }
            return _deltaBuildTimes[id];
        }
    }
}
