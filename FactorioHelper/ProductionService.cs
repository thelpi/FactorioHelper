using System;
using System.Collections.Generic;
using System.Linq;
using FactorioHelper.Enums;
using FactorioHelper.Items;

namespace FactorioHelper
{
    public class ProductionService
    {
        public const int PetroleumGasId = 26;
        public const int HeavyOilId = 41;
        public const int LightOilId = 42;
        private const int CrudeOilId = 43;
        private const int WaterId = 44;
        private const int SolidFuelId = 50;

        private const int BasicOilProcessingRecipeId = 1;
        private const int AdvancedOilProcessingRecipeId = 2;
        private const int LightOilCrackingRecipeId = 4;
        private const int HeavyOilCrackingRecipeId = 5;

        private static readonly IReadOnlyDictionary<int, int> SolidFuelRequirements= new Dictionary<int, int>
        {
            { LightOilId, 10 },
            { HeavyOilId, 20 },
            { PetroleumGasId, 20 }
        };

        private static readonly IReadOnlyDictionary<int, string> SciencePackGroups = new Dictionary<int, string>
        {
            { -1, "Each pack to Logistic" },
            { -2, "Each pack to Military" },
            { -3, "Each pack to Chemical" },
            { -4, "Each pack to Production" },
            { -5, "Each pack to Utility" },
            { -6, "Each pack to Space" }
        };

        private static readonly IReadOnlyDictionary<int, int[]> SciencePackGroupsItems = new Dictionary<int, int[]>
        {
            { -1, new[] { 1, 7 } },
            { -2, new[] { 1, 7, 12 } },
            { -3, new[] { 1, 7, 12, 21 } },
            { -4, new[] { 1, 7, 12, 21, 28 } },
            { -5, new[] { 1, 7, 12, 21, 28, 33 } },
            { -6, new[] { 1, 7, 12, 21, 28, 33, 55 } }
        };

        private static readonly IReadOnlyDictionary<ItemBuildType, Func<Item, Item>> SpecificItemTypes =
            new Dictionary<ItemBuildType, Func<Item, Item>>
            {
                { ItemBuildType.AssemblingMachine, x => x.ToItem<AssemblingItem>() },
                { ItemBuildType.Furnace, x => x.ToItem<FurnaceItem>() },
                { ItemBuildType.MiningDrill, x => x.ToItem<MiningItem>() },
                { ItemBuildType.Refining, x => x.ToItem<RefiningItem>() },
                { ItemBuildType.Pumpjack, x => x.ToItem<PumpjackItem>() }
            };

        private Dictionary<int, Fraction> _solidFuelRateConsumption;

        public FurnaceType FurnaceType { get; set; }
        public MiningDrillType MiningDrillType { get; set; }
        public int MiningBonus { get; set; }
        public AssemblingType AssemblingType { get; set; }
        public bool AdvancedOilProcessing { get; set; }
        public int CrudeOilInitialYield { get; set; }
        public IReadOnlyDictionary<ItemBuildType, IReadOnlyCollection<KeyValuePair<ModuleType, int>>> StandardModulesConfiguration { get; private set; }
        public IReadOnlyDictionary<ItemBuildType, IReadOnlyCollection<KeyValuePair<ModuleType, int>>> OilRecipesModulesConfiguration { get; private set; }
        
        public IReadOnlyDictionary<int, Fraction> SolidFuelRateConsumption => _solidFuelRateConsumption;

        private readonly IReadOnlyDictionary<int, Item> _items;
        private readonly IReadOnlyDictionary<int, RecipeItem> _recipes;

        internal ProductionService()
        {
            var items = GetDatas("items", data =>
                new Item
                {
                    Id = Convert.ToInt32(data[0]),
                    Name = data[1],
                    BuildResult = Convert.ToInt32(data[3]),
                    BuildTime = Convert.ToDecimal(data[2].Replace('.', ',')),
                    BuildType = (ItemBuildType)Convert.ToInt32(data[4]),
                    ApplyRealRequirement = Convert.ToInt32(data[5]) != 0
                });

            _items = items
                .Select(item => SpecificItemTypes.ContainsKey(item.BuildType)
                    ? SpecificItemTypes[item.BuildType](item)
                    : item)
                .ToDictionary(x => x.Id, x => x);

            var components = GetDatas("components", data =>
                new Tuple<int, int, Fraction>(Convert.ToInt32(data[0]), Convert.ToInt32(data[1]), Convert.ToInt32(data[2])));

            foreach (var itemId in _items.Keys)
            {
                if (itemId != SolidFuelId)
                {
                    _items[itemId].Composition = components
                        .Where(x => x.Item1 == itemId)
                        .ToDictionary(x => x.Item2, x => x.Item3);
                }
            }

            var recipes = GetDatas("recipes", data =>
                new RecipeItem
                {
                    BuildTime = Convert.ToDecimal(data[3].Replace('.', ',')),
                    BuildType = (ItemBuildType)Convert.ToInt32(data[2]),
                    Id = Convert.ToInt32(data[0]),
                    Name = data[1]
                });

            var recipesSources = GetDatas("recipes_sources", data =>
                new Tuple<int, int, int>(Convert.ToInt32(data[0]), Convert.ToInt32(data[1]), Convert.ToInt32(data[2])));

            var recipesTargets = GetDatas("recipes_targets", data =>
                new Tuple<int, int, int>(Convert.ToInt32(data[0]), Convert.ToInt32(data[1]), Convert.ToInt32(data[2])));

            foreach (var recipe in recipes)
            {
                recipe.SourceItems = recipesSources
                    .Where(x => x.Item1 == recipe.Id)
                    .ToDictionary(_ => _.Item2, _ => _.Item3);

                recipe.TargetItems = recipesTargets
                    .Where(x => x.Item1 == recipe.Id)
                    .ToDictionary(_ => _.Item2, _ => _.Item3);
            }

            _recipes = recipes.ToDictionary(x => x.Id, x => x);
        }

        internal void SetModulesConfiguration(IReadOnlyCollection<ModuleConfiguration> modulesConfiguration)
        {
            StandardModulesConfiguration = modulesConfiguration
                .Where(x => EnumExtensions.ModulableBuildTypes().Contains(x.BuildType)
                    && !EnumExtensions.OilModulableBuildTypes().Contains(x.BuildType))
                .GroupBy(x => x.BuildType)
                .ToDictionary(x => x.Key, x => x
                    .GroupBy(y => y.Module)
                    .Select(y => new KeyValuePair<ModuleType, int>(y.Key, y.Sum(z => z.Count)))
                    .ToList()
                    as IReadOnlyCollection<KeyValuePair<ModuleType, int>>);
            OilRecipesModulesConfiguration = modulesConfiguration
                .Where(x => EnumExtensions.OilModulableBuildTypes().Contains(x.BuildType))
                .GroupBy(x => x.BuildType)
                .ToDictionary(x => x.Key, x => x
                    .GroupBy(y => y.Module)
                    .Select(y => new KeyValuePair<ModuleType, int>(y.Key, y.Sum(z => z.Count)))
                    .ToList()
                    as IReadOnlyCollection<KeyValuePair<ModuleType, int>>);
        }

        internal void SetSolidFuelRateConsumption(Dictionary<int, Fraction> solidFuelRateConsumption)
        {
            _solidFuelRateConsumption = solidFuelRateConsumption;

            var dic = new Dictionary<int, Fraction>(3);
            foreach (var k in SolidFuelRequirements.Keys)
            {
                if (SolidFuelRateConsumption.ContainsKey(k) && SolidFuelRateConsumption[k] > 0)
                {
                    dic.Add(k, SolidFuelRequirements[k] * SolidFuelRateConsumption[k]);
                }
            }
            _items[SolidFuelId].Composition = dic;
        }

        internal OilProductionOutput GetOilToProduce(Dictionary<int, ProductionItem> fromProduction)
        {
            var lightReqPerSec = GetOilRequirement(fromProduction, LightOilId);
            var heavyReqPerSec = GetOilRequirement(fromProduction, HeavyOilId);
            var gasReqPerSec = GetOilRequirement(fromProduction, PetroleumGasId);

            if (lightReqPerSec > 0 || heavyReqPerSec > 0)
            {
                // required anyway
                AdvancedOilProcessing = true;
            }
            else if (gasReqPerSec == 0)
            {
                return new OilProductionOutput
                {
                    ChemicalPlantRequirements = new Dictionary<int, int>(),
                    RefineryRequirements = new Dictionary<int, int>(),
                    RemainsPerSec = new Dictionary<int, Fraction>()
                };
            }

            if (!AdvancedOilProcessing)
            {
                return GetOilToProduceWithoutAdvancedProcessing(fromProduction, gasReqPerSec);
            }

            var recipes = new Dictionary<int, RecipeItem>
            {
                { AdvancedOilProcessingRecipeId, _recipes[AdvancedOilProcessingRecipeId] },
                { LightOilCrackingRecipeId, _recipes[LightOilCrackingRecipeId] },
                { HeavyOilCrackingRecipeId, _recipes[HeavyOilCrackingRecipeId] }
            };

            var remains = new Dictionary<int, Fraction>
            {
                { PetroleumGasId, 0 },
                { HeavyOilId, 0 },
                { LightOilId, 0 }
            };
            Dictionary<int, int> recipesFactoriesCountFinal = null;

            var recipesFactoriesCount = recipes.ToDictionary(_ => _.Key, _ => 0);

            Fraction GetDeltaPerSec(int id)
            {
                return recipes.Values.FractionSum(_ => _.GetDeltaPerSec(id) * recipesFactoriesCount[_.Id]);
            }

            int GetFactoriesCount(int recipeId, Fraction reqPerSec, int oilId)
            {
                return reqPerSec <= 0
                    ? 0
                    : (int)Math.Ceiling((reqPerSec / recipes[recipeId].GetTargetPerSec(oilId)).Decimal);
            }

            int GetFactoriesCountAfterSettleAop(int recipeId, int oilId, int aopFactoryCount, Fraction reqPerSec)
            {
                var perSecFromAop = recipes[AdvancedOilProcessingRecipeId].GetTargetPerSec(oilId) * aopFactoryCount;
                return GetFactoriesCount(recipeId, reqPerSec - perSecFromAop, oilId);
            }

            // The required count of factories for all the heavy oil is produced with "AdvancedOilProcessing" recipe
            // it's minimal because the heavy oil is always produced that way
            var minAopFactoriesCount = GetFactoriesCount(AdvancedOilProcessingRecipeId, heavyReqPerSec, HeavyOilId);
            // The required count of factories if all the light oil is produced with "AdvancedOilProcessing" recipe
            var maxAopFactoriesCountForLight = GetFactoriesCount(AdvancedOilProcessingRecipeId, lightReqPerSec, LightOilId);
            // The required count of factories if all the petroleum gas is produced with "AdvancedOilProcessing" recipe
            var maxAopFactoriesCountForGas = GetFactoriesCount(AdvancedOilProcessingRecipeId, gasReqPerSec, PetroleumGasId);

            // from all the above, computes the maximal count of AOP factories
            var maxAopFactoriesCount = SystemExtensions.Max(
                minAopFactoriesCount,
                maxAopFactoriesCountForLight,
                maxAopFactoriesCountForGas);

            // We try several combination from the lowest count to the highest count
            for (var aopFactoriesCount = minAopFactoriesCount;
                aopFactoriesCount <= maxAopFactoriesCount;
                aopFactoriesCount++)
            {
                // The number of "LightOilCracking" factories to produce "petroleum gas" requirements
                var locFactoriesCount = GetFactoriesCountAfterSettleAop(LightOilCrackingRecipeId, PetroleumGasId, aopFactoriesCount, gasReqPerSec);
                
                // The "light oil" requirements to produces the "petroleum gas" requirements
                var lightReqPerSecForGas = recipes[LightOilCrackingRecipeId].GetSourcePerSec(LightOilId) * locFactoriesCount;

                // The number of "HeavyOilCracking" factories to produce "light oil" requirements
                var hocFactoriesCount = GetFactoriesCountAfterSettleAop(HeavyOilCrackingRecipeId, LightOilId, aopFactoriesCount, lightReqPerSec + lightReqPerSecForGas);

                // sets the current count of factories
                recipesFactoriesCount[AdvancedOilProcessingRecipeId] = aopFactoriesCount;
                recipesFactoriesCount[LightOilCrackingRecipeId] = locFactoriesCount;
                recipesFactoriesCount[HeavyOilCrackingRecipeId] = hocFactoriesCount;

                // computes the related production
                var heavyRemains = GetDeltaPerSec(HeavyOilId) - heavyReqPerSec;
                var lightRemains = GetDeltaPerSec(LightOilId) - lightReqPerSec;
                var gazRemains = GetDeltaPerSec(PetroleumGasId) - gasReqPerSec;
                if (gazRemains >= 0 && heavyRemains >= 0 && lightRemains >= 0)
                {
                    // keeps the current production as "the one" if:
                    // it's the first try
                    // OR remains are lowest as possible
                    if (recipesFactoriesCountFinal == null
                        || remains.Values.FractionSum(x => x) > (gazRemains + heavyRemains + lightRemains).Decimal) // TODO: operator ">" on fractions
                    {
                        remains[PetroleumGasId] = gazRemains;
                        remains[HeavyOilId] = heavyRemains;
                        remains[LightOilId] = lightRemains;
                        recipesFactoriesCountFinal = new Dictionary<int, int>
                        {
                            { AdvancedOilProcessingRecipeId, aopFactoriesCount },
                            { LightOilCrackingRecipeId, locFactoriesCount },
                            { HeavyOilCrackingRecipeId, hocFactoriesCount },
                        };
                    }
                }
            }

            var crudeConsome = recipes.Values.FractionSum(_ => _.GetSourcePerSec(CrudeOilId) * recipesFactoriesCountFinal[_.Id]);
            var waterConsome = recipes.Values.FractionSum(_ => _.GetSourcePerSec(WaterId) * recipesFactoriesCountFinal[_.Id]);

            // TODO: remove when ready to display properly
            fromProduction.Remove(PetroleumGasId);
            fromProduction.Remove(HeavyOilId);
            fromProduction.Remove(LightOilId);

            AddOrUpdateItemProduction(fromProduction, new KeyValuePair<int, Fraction>(WaterId, waterConsome));
            AddOrUpdateItemProduction(fromProduction, new KeyValuePair<int, Fraction>(CrudeOilId, crudeConsome));

            return new OilProductionOutput
            {
                RemainsPerSec = remains,
                ChemicalPlantRequirements = recipesFactoriesCountFinal
                    .Where(kvp => kvp.Value > 0 && recipes[kvp.Key].BuildType == ItemBuildType.ChemicalPlant)
                    .ToDictionary(x => x.Key, x => x.Value),
                RefineryRequirements = recipesFactoriesCountFinal
                    .Where(kvp => kvp.Value > 0 && recipes[kvp.Key].BuildType != ItemBuildType.ChemicalPlant)
                    .ToDictionary(x => x.Key, x => x.Value)
            };
        }

        internal Dictionary<int, ProductionItem> GetItemsToProduce(Fraction targetPerSec, int itemId)
        {
            var itemsToProduce = GetFullListOfItemsToProduce(itemId);

            var itemsResult = new Dictionary<int, ProductionItem>();

            var i = 0;
            while (i < itemsToProduce.Count)
            {
                CheckSuitableItem(itemsToProduce, itemsResult, i);

                var item = itemsToProduce[i];

                var itemTargetPerSec = (item.Id == itemId || (itemId < 0 && SciencePackGroupsItems[itemId].Contains(item.Id)))
                    ? targetPerSec
                    : GetItemPerSecFromParents(itemsToProduce, itemsResult, item);

                AddOrUpdateItemRequirements(itemsResult, itemTargetPerSec, item);
                i++;
            }

            return itemsResult;
        }

        internal IReadOnlyCollection<BaseItem> GetBaseItemsList()
        {
            var baseItems = _items.Values.Cast<BaseItem>().ToList();

            foreach (var spgId in SciencePackGroups.Keys)
            {
                baseItems.Insert(0, new BaseItem { Id = spgId, Name = SciencePackGroups[spgId] });
            }

            return baseItems;
        }

        private OilProductionOutput GetOilToProduceWithoutAdvancedProcessing(Dictionary<int, ProductionItem> fromProduction, Fraction gasReqPerSec)
        {
            var basicOilProcessingRecipe = _recipes[BasicOilProcessingRecipeId];

            var gazProdPerSec = basicOilProcessingRecipe.GetTargetPerSec(PetroleumGasId);

            var refineryCountReq = (int)Math.Ceiling((gasReqPerSec / gazProdPerSec).Decimal);

            fromProduction.Remove(PetroleumGasId);

            var sourceItems = basicOilProcessingRecipe.SourceItems
                .Select(_ => new KeyValuePair<int, Fraction>(_.Key, basicOilProcessingRecipe.GetSourcePerSec(_.Key) * refineryCountReq))
                .ToList();

            foreach (var sourceItem in sourceItems)
            {
                AddOrUpdateItemProduction(fromProduction, sourceItem);
            }

            return new OilProductionOutput
            {
                ChemicalPlantRequirements = new Dictionary<int, int>(),
                RefineryRequirements = new Dictionary<int, int> { { BasicOilProcessingRecipeId, refineryCountReq } },
                RemainsPerSec = new Dictionary<int, Fraction>()
            };
        }

        private void AddOrUpdateItemProduction(Dictionary<int, ProductionItem> fromProduction, KeyValuePair<int, Fraction> sourceItem)
        {
            var itembaseInfo = _items[sourceItem.Key];
            AddOrUpdateItemRequirements(fromProduction, sourceItem.Value, itembaseInfo);
        }

        private void AddOrUpdateItemRequirements(Dictionary<int, ProductionItem> fromProduction, Fraction reqToAdd, Item itembaseInfo)
        {
            var timeRate = itembaseInfo.GetProductionRate(this);
            if (!fromProduction.ContainsKey(itembaseInfo.Id))
            {
                var item = new ProductionItem
                {
                    Id = itembaseInfo.Id,
                    Name = itembaseInfo.Name,
                    BuildType = itembaseInfo.BuildType,
                    RealMachineRequirement = reqToAdd * timeRate,
                    PerSecQuantityRequirement = reqToAdd
                };
                fromProduction.Add(item.Id, item);
            }
            else
            {
                fromProduction[itembaseInfo.Id].PerSecQuantityRequirement += reqToAdd;
            }
            fromProduction[itembaseInfo.Id].RealMachineRequirement = fromProduction[itembaseInfo.Id].PerSecQuantityRequirement * timeRate;
        }

        private static Fraction GetOilRequirement(Dictionary<int, ProductionItem> fromProduction, int oilId)
        {
            return (fromProduction.ContainsKey(oilId) ? fromProduction[oilId] : null)?.PerSecQuantityRequirement ?? 0;
        }

        private List<Item> GetFullListOfItemsToProduce(int itemId)
        {
            var itemsToProduce = new List<Item>();

            if (itemId < 0)
            {
                itemsToProduce.AddRange(SciencePackGroupsItems[itemId].Select(x => _items[x]));
            }
            else
            {
                itemsToProduce.Add(_items[itemId]);
            }

            var currentItemIndex = 0;

            while (currentItemIndex < itemsToProduce.Count)
            {
                foreach (var subItemId in itemsToProduce[currentItemIndex].Composition.Keys)
                {
                    if (!itemsToProduce.Any(_ => _.Id == subItemId))
                    {
                        var subItem = _items[subItemId];
                        itemsToProduce.Add(subItem);
                    }
                }
                currentItemIndex++;
            }

            return itemsToProduce;
        }

        private Fraction GetItemPerSecFromParents(
            List<Item> itemsToProduce,
            Dictionary<int, ProductionItem> itemsResult,
            Item item)
        {
            var parentItems = new List<ProductionItem>(10);

            var perSec = new Fraction(0);
            foreach (var localItem in itemsToProduce.Where(_ => _.Composition.ContainsKey(item.Id)))
            {
                var parentItem = itemsResult[localItem.Id];
                var localPerSec = localItem.Composition[item.Id] * (
                    localItem.ApplyRealRequirement
                        ? parentItem.RealMachineRequirement
                        : parentItem.MachineRequirement)
                    / localItem.GetRealBuildTime(this);
                parentItem.AddComponent(item.Id,  item.Name, localPerSec);
                perSec += localPerSec;
                parentItems.Add(parentItem);
            }

            foreach (var parentItem in parentItems)
                parentItem.SetComponentUseRate(item.Id, perSec);

            return perSec;
        }

        private void CheckSuitableItem(
            List<Item> itemsToProduce,
            Dictionary<int, ProductionItem> itemsResult,
            int i)
        {
            var currentItemIsSuitable = false;
            while (!currentItemIsSuitable)
            {
                var it = itemsToProduce[i];
                var parentItems = itemsToProduce.Where(_ => _.Composition.ContainsKey(it.Id)).ToList();
                if (parentItems.Count > 0 && !parentItems.All(_ => itemsResult.ContainsKey(_.Id)))
                {
                    itemsToProduce.Add(it);
                    itemsToProduce.RemoveAt(i);
                }
                else
                {
                    currentItemIsSuitable = true;
                }
            }
        }

        private static List<T> GetDatas<T>(string resourceName, Func<string[], T> converter)
        {
            return Properties.Resources.ResourceManager
                .GetString(resourceName)
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(row => converter(row.Split(',')))
                .ToList();
        }
    }
}
