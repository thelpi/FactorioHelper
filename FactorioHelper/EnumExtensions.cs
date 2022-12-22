using System;
using System.Collections.Generic;
using System.Linq;
using FactorioHelper.Enums;

namespace FactorioHelper
{
    internal static class EnumExtensions
    {
        public static decimal GetRate(this FurnaceType furnaceType)
        {
            switch (furnaceType)
            {
                case FurnaceType.Electric:
                case FurnaceType.Steel:
                    return 2;
                default: // stone
                    return 1;
            }
        }

        public static decimal GetRate(this MiningDrillType miningDrillType, int bonus)
        {
            var baseRate = miningDrillType == MiningDrillType.Burner
                ? 0.25M
                : 0.5M;
            return baseRate + (baseRate * bonus * 0.1M);
        }

        public static decimal GetRate(this AssemblingType assemblingType)
        {
            switch (assemblingType)
            {
                case AssemblingType.Machine3:
                    return 1.25M;
                case AssemblingType.Machine2:
                    return 0.75M;
                default: // machine 1
                    return 0.5M;
            }
        }

        public static IReadOnlyCollection<T> Values<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToList();
        }

        public static decimal GetSpeedBonus(this ModuleType moduleType)
        {
            if (moduleType == ModuleType.Speed1)
                return 0.2M;
            else if (moduleType == ModuleType.Speed2)
                return 0.3M;
            else if (moduleType == ModuleType.Speed3)
                return 0.5M;
            else if (moduleType == ModuleType.Productivity1)
                return -0.05M;
            else if (moduleType == ModuleType.Productivity2)
                return -0.10M;
            else if (moduleType == ModuleType.Productivity3)
                return -0.15M;
            else
                return 0;
        }

        public static decimal GetProductivityBonus(this ModuleType moduleType)
        {
            if (moduleType == ModuleType.Productivity1)
                return 0.04M;
            else if (moduleType == ModuleType.Productivity2)
                return 0.06M;
            else if (moduleType == ModuleType.Productivity3)
                return 0.10M;
            else
                return 0;
        }
    }
}
