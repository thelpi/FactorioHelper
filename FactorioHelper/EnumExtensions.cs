using System;
using System.Collections.Generic;
using System.Linq;
using FactorioHelper.Enums;

namespace FactorioHelper
{
    static class EnumExtensions
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
    }
}
