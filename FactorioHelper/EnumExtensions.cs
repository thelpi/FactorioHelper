using System;
using System.Collections.Generic;
using System.Linq;
using FactorioHelper.Enums;

namespace FactorioHelper
{
    public static class EnumExtensions
    {
        public static Fraction GetRate(this InserterType inserterType)
        {
            switch (inserterType)
            {
                case InserterType.Burner:
                    return 0;
                case InserterType.Fast:
                case InserterType.Filter:
                    return 0;
                case InserterType.Stack:
                case InserterType.StackFilter:
                    return 0;
                default: // Standard & LongHanded
                    return 0;
            }
        }

        public static Fraction GetRate(this FurnaceType furnaceType)
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

        public static Fraction GetRate(this MiningDrillType miningDrillType, int bonus)
        {
            var baseRate = miningDrillType == MiningDrillType.Burner
                ? 0.25M
                : 0.5M;
            return baseRate + (baseRate * bonus * 0.1M);
        }

        public static Fraction GetRate(this AssemblingType assemblingType)
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

        public static Fraction GetSpeedBonus(this ModuleType moduleType, bool forceNotApply)
        {
            if (forceNotApply) return 0;

            switch (moduleType)
            {
                case ModuleType.Speed1:
                    return 0.2M;
                case ModuleType.Speed2:
                    return 0.3M;
                case ModuleType.Speed3:
                    return 0.5M;
                case ModuleType.Productivity1:
                    return -0.05M;
                case ModuleType.Productivity2:
                    return -0.10M;
                case ModuleType.Productivity3:
                    return -0.15M;
                default:
                    return 0;
            }
        }

        public static decimal GetProductivityBonus(this ModuleType moduleType, bool forceNotApply)
        {
            if (forceNotApply) return 0;

            switch (moduleType)
            {
                case ModuleType.Productivity1:
                    return 0.04M;
                case ModuleType.Productivity2:
                    return 0.06M;
                case ModuleType.Productivity3:
                    return 0.10M;
                default:
                    return 0;
            }
        }

        public static ItemBuildType[] ModulableBuildTypes()
        {
            return Values<ItemBuildType>()
                .Where(x => x != ItemBuildType.OffshorePump && x != ItemBuildType.Other)
                .ToArray();
        }

        public static ItemBuildType[] OilModulableBuildTypes()
        {
            return Values<ItemBuildType>()
                .Where(x => x == ItemBuildType.Refining && x == ItemBuildType.ChemicalPlant)
                .ToArray();
        }
    }
}
