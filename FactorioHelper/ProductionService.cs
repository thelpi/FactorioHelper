using System;
using System.Collections.Generic;
using System.Linq;
using FactorioHelper.Enums;
using FactorioHelper.Items;

namespace FactorioHelper
{
    class ProductionService
    {
        private const int PetroleumGasId = 26;
        private const int HeavyOilId = 41;
        private const int LightOilId = 42;
        private const int CrudeOilId = 43;
        private const int WaterId = 44;

        private const int BasicOilProcessingRecipeId = 1;
        private const int AdvancedOilProcessingRecipeId = 2;
        private const int LightOilCrackingRecipeId = 4;
        private const int HeavyOilCrackingRecipeId = 5;
        
        private const int EachSciencePackItemId = 0;
        private const string EachSciencePackItemName = "Each science pack";

        private static IReadOnlyDictionary<ItemBuildType, Func<Item, Item>> SpecificItemTypes =
            new Dictionary<ItemBuildType, Func<Item, Item>>
            {
                { ItemBuildType.AssemblingMachine, x => x.ToItem<AssemblingItem>() },
                { ItemBuildType.Furnace, x => x.ToItem<FurnaceItem>() },
                { ItemBuildType.MiningDrill, x => x.ToItem<MiningItem>() },
                { ItemBuildType.Refining, x => x.ToItem<RefiningItem>() }
            };

        private readonly IDataProvider _dataProvider;

        public FurnaceType FurnaceType { get; set; }
        public MiningDrillType MiningDrillType { get; set; }
        public int MiningBonus { get; set; }
        public AssemblingType AssemblingType { get; set; }
        public bool AdvancedOilProcessing { get; set; }
        public IReadOnlyDictionary<ItemBuildType, IReadOnlyCollection<KeyValuePair<ModuleType, int>>> StandardModulesConfiguration { get; private set; }
        public IReadOnlyDictionary<ItemBuildType, IReadOnlyCollection<KeyValuePair<ModuleType, int>>> OilRecipesModulesConfiguration { get; private set; }


        internal ProductionService(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        internal void SetModulesConfiguration(IReadOnlyCollection<ModuleConfiguration> modulesConfiguration)
        {
            StandardModulesConfiguration = modulesConfiguration
                .Where(x => x.BuildType != ItemBuildType.Refining && x.BuildType != ItemBuildType.Other)
                .GroupBy(x => x.BuildType)
                .ToDictionary(x => x.Key, x => x
                    .GroupBy(y => y.Module)
                    .Select(y => new KeyValuePair<ModuleType, int>(y.Key, y.Sum(z => z.Count)))
                    .ToList()
                    as IReadOnlyCollection<KeyValuePair<ModuleType, int>>);
            OilRecipesModulesConfiguration = modulesConfiguration
                .Where(x => x.BuildType == ItemBuildType.Refining || x.BuildType == ItemBuildType.ChemicalPlant)
                .GroupBy(x => x.BuildType)
                .ToDictionary(x => x.Key, x => x
                    .GroupBy(y => y.Module)
                    .Select(y => new KeyValuePair<ModuleType, int>(y.Key, y.Sum(z => z.Count)))
                    .ToList()
                    as IReadOnlyCollection<KeyValuePair<ModuleType, int>>);
        }

        internal OilProductionOutput GetOilToProduce(List<ProductionItem> fromProduction)
        {
            var lightReqPerSec = GetOilRequirement(fromProduction, LightOilId);
            var heavyReqPerSec = GetOilRequirement(fromProduction, HeavyOilId);
            var gazReqPerSec = GetOilRequirement(fromProduction, PetroleumGasId);

            if (lightReqPerSec > 0 || heavyReqPerSec > 0)
            {
                // required anyway
                AdvancedOilProcessing = true;
            }
            else if (gazReqPerSec == 0)
            {
                return new OilProductionOutput
                {
                    ChemicalPlantRequirements = new Dictionary<int, int>(),
                    RefineryRequirements = new Dictionary<int, int>(),
                    RemainsPerSec = new Dictionary<int, decimal>()
                };
            }

            if (!AdvancedOilProcessing)
            {
                var basicOilProcessingRecipe = GetRecipeById(BasicOilProcessingRecipeId);
                
                var gazProdPerSec = basicOilProcessingRecipe.GetTargetPerSec(PetroleumGasId);
                
                var refineryCountReq = (int)Math.Ceiling(gazReqPerSec / gazProdPerSec);
                
                fromProduction.RemoveAll(_ => _.Id == PetroleumGasId);

                var sourceItems = basicOilProcessingRecipe.SourceItems
                    .Select(_ => new KeyValuePair<int, decimal>(_.Key, basicOilProcessingRecipe.GetSourcePerSec(_.Key) * refineryCountReq))
                    .ToList();

                foreach (var sourceItem in sourceItems)
                {
                    AddOrUpdateItemProuction(fromProduction, sourceItem);
                }

                return new OilProductionOutput
                {
                    ChemicalPlantRequirements = new Dictionary<int, int>(),
                    RefineryRequirements = new Dictionary<int, int> { { BasicOilProcessingRecipeId, refineryCountReq } },
                    RemainsPerSec = new Dictionary<int, decimal>()
                };
            }

            var recipes = new[]
            {
                GetRecipeById(AdvancedOilProcessingRecipeId),
                GetRecipeById(LightOilCrackingRecipeId),
                GetRecipeById(HeavyOilCrackingRecipeId)
            };

            decimal gazTotalRemains = 0;
            decimal heavyTotalRemains = 0;
            decimal lightTotalRemains = 0;
            Dictionary<int, int> countFactoriesByRecipeFlagged = null;
            var totalRemains = gazTotalRemains + heavyTotalRemains + lightTotalRemains;

            var countFactoriesByRecipe = recipes.ToDictionary(_ => _.Id, _ => 0);

            decimal GetDeltaPerSec(int id) =>
                recipes.Sum(_ => _.GetDeltaPerSec(id) * countFactoriesByRecipe[_.Id]);

            var minimalHeavyFactoriesCount = heavyReqPerSec == 0
                ? 1
                : (int)Math.Ceiling(heavyReqPerSec / recipes.Single(_ => _.Id == AdvancedOilProcessingRecipeId).GetTargetPerSec(HeavyOilId));

            const int MaxAttemps = 100;
            for (int j = minimalHeavyFactoriesCount; j < MaxAttemps; j++)
            {
                for (int k = 0; k < MaxAttemps; k++)
                {
                    for (int l = 0; l < MaxAttemps; l++)
                    {
                        countFactoriesByRecipe[AdvancedOilProcessingRecipeId] = j;
                        countFactoriesByRecipe[LightOilCrackingRecipeId] = k;
                        countFactoriesByRecipe[HeavyOilCrackingRecipeId] = l;
                            
                        var gazRemains = GetDeltaPerSec(PetroleumGasId) - gazReqPerSec;
                        if (gazRemains >= 0)
                        {
                            var heavyRemains = GetDeltaPerSec(HeavyOilId) - heavyReqPerSec;
                            if (heavyRemains >= 0)
                            {
                                var lightRemains = GetDeltaPerSec(LightOilId) - lightReqPerSec;
                                if (lightRemains >= 0)
                                {
                                    if (countFactoriesByRecipeFlagged == null
                                        || totalRemains > (gazRemains + heavyRemains + lightRemains))
                                    {
                                        gazTotalRemains = gazRemains;
                                        heavyTotalRemains = heavyRemains;
                                        lightTotalRemains = lightRemains;
                                        totalRemains = gazTotalRemains + heavyTotalRemains + lightTotalRemains;
                                        countFactoriesByRecipeFlagged = new Dictionary<int, int>
                                        {
                                            { AdvancedOilProcessingRecipeId, j },
                                            { LightOilCrackingRecipeId, k },
                                            { HeavyOilCrackingRecipeId, l },
                                        };
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var crudeConsome = recipes.Sum(_ => _.GetSourcePerSec(CrudeOilId) * countFactoriesByRecipeFlagged[_.Id]);
            var waterConsome = recipes.Sum(_ => _.GetSourcePerSec(WaterId) * countFactoriesByRecipeFlagged[_.Id]);

            fromProduction.RemoveAll(_ => _.Id == PetroleumGasId);
            fromProduction.RemoveAll(_ => _.Id == HeavyOilId);
            fromProduction.RemoveAll(_ => _.Id == LightOilId);

            AddOrUpdateItemProuction(fromProduction, new KeyValuePair<int, decimal>(WaterId, waterConsome));
            AddOrUpdateItemProuction(fromProduction, new KeyValuePair<int, decimal>(CrudeOilId, crudeConsome));

            var chemicalPlantReq = new Dictionary<int, int>();
            var refineryReq = new Dictionary<int, int>();
            foreach (var recipeId in countFactoriesByRecipeFlagged.Keys)
            {
                if (countFactoriesByRecipeFlagged[recipeId] > 0)
                {
                    if (recipes.First(_ => _.Id == recipeId).BuildType == ItemBuildType.ChemicalPlant)
                    {
                        chemicalPlantReq.Add(recipeId, countFactoriesByRecipeFlagged[recipeId]);
                    }
                    else
                    {
                        refineryReq.Add(recipeId, countFactoriesByRecipeFlagged[recipeId]);
                    }
                }
            }

            var oilOutput = new OilProductionOutput
            {
                RemainsPerSec = new Dictionary<int, decimal>
                {
                    { PetroleumGasId, gazTotalRemains },
                    { HeavyOilId, heavyTotalRemains },
                    { LightOilId, lightTotalRemains }
                },
                ChemicalPlantRequirements = chemicalPlantReq,
                RefineryRequirements = refineryReq
            };

            return oilOutput;
        }

        private void AddOrUpdateItemProuction(List<ProductionItem> fromProduction, KeyValuePair<int, decimal> sourceItem)
        {
            var itembaseInfo = GetItemById(sourceItem.Key);
            var existingItem = fromProduction.FirstOrDefault(_ => _.Id == sourceItem.Key);
            var timeRate = itembaseInfo.GetProductionRate(this);
            if (existingItem == null)
            {
                fromProduction.Add(new ProductionItem
                {
                    Id = itembaseInfo.Id,
                    Name = itembaseInfo.Name,
                    BuildType = itembaseInfo.BuildType,
                    RealMachineRequirement = sourceItem.Value * timeRate,
                    PerSecQuantityRequirement = sourceItem.Value
                });
            }
            else
            {
                existingItem.PerSecQuantityRequirement += sourceItem.Value;
                existingItem.RealMachineRequirement = existingItem.PerSecQuantityRequirement / timeRate;
            }
        }

        private static decimal GetOilRequirement(IReadOnlyCollection<ProductionItem> fromProduction, int oilId)
        {
            return GetOilItem(fromProduction, oilId)?.PerSecQuantityRequirement ?? 0;
        }

        private static ProductionItem GetOilItem(IReadOnlyCollection<ProductionItem> fromProduction, int oilId)
        {
            return fromProduction.SingleOrDefault(_ => _.Id == oilId);
        }

        internal List<ProductionItem> GetItemsToProduce(decimal targetPerSec, int itemId)
        {
            var itemsToProduce = GetFullListOfItemsToProduce(itemId);

            var itemsResult = new List<ProductionItem>();

            var i = 0;
            while (i < itemsToProduce.Count)
            {
                CheckSuitableItem(itemsToProduce, itemsResult, i);

                var item = itemsToProduce[i];

                var itemTargetPerSec = (item.Id == itemId || (itemId == EachSciencePackItemId && GetSciencePackItemIds().Contains(item.Id)))
                    ? targetPerSec
                    : GetItemPerSecFromParents(itemsToProduce, itemsResult, item);

                var rate = item.GetProductionRate(this);

                itemsResult.Add(new ProductionItem
                {
                    Id = item.Id,
                    RealMachineRequirement = itemTargetPerSec * rate,
                    PerSecQuantityRequirement = itemTargetPerSec,
                    Name = item.Name,
                    BuildType = item.BuildType
                });
                i++;
            }

            return itemsResult;
        }

        internal IReadOnlyCollection<BaseItem> GetBaseItemsList(bool withEachSciencePackItem)
        {
            var items = _dataProvider
                .GetDatas(
                    "SELECT id, name FROM item",
                    _ => new BaseItem
                    {
                        Id = _.Get<int>("id"),
                        Name = _.Get<string>("name")
                    }).ToList();

            if (withEachSciencePackItem)
            {
                items.Insert(0, new BaseItem { Id = EachSciencePackItemId, Name = EachSciencePackItemName });
            }

            return items;
        }

        private List<Item> GetFullListOfItemsToProduce(int itemId)
        {
            var itemsToProduce = new List<Item>();

            if (itemId == EachSciencePackItemId)
            {
                itemsToProduce.AddRange(GetSciencePackItemIds().Select(GetItemById));
            }
            else
                itemsToProduce.Add(GetItemById(itemId));

            var currentItemIndex = 0;

            while (currentItemIndex < itemsToProduce.Count)
            {
                foreach (var subItemId in itemsToProduce[currentItemIndex].Composition.Keys)
                {
                    if (!itemsToProduce.Any(_ => _.Id == subItemId))
                    {
                        var subItem = GetItemById(subItemId);
                        itemsToProduce.Add(subItem);
                    }
                }
                currentItemIndex++;
            }

            return itemsToProduce;
        }

        private IReadOnlyCollection<int> GetSciencePackItemIds()
        {
            return _dataProvider
                .GetDatas(
                    "SELECT id FROM item WHERE is_science_pack = 1",
                    _ => _.Get<int>("id"));
        }

        private Item GetItemById(int itemId)
        {
            var item = _dataProvider
                .GetData(
                    $"SELECT id, name, build_time, build_result, build_type_id, is_science_pack, apply_real_requirement FROM item WHERE id = {itemId}",
                    _ =>
                    {
                        var localItem = new Item
                        {
                            Id = _.Get<int>("id"),
                            Name = _.Get<string>("name"),
                            BuildResult = _.Get<int>("build_result"),
                            BuildTime = _.Get<decimal>("build_time"),
                            BuildType = (ItemBuildType)_.Get<int>("build_type_id"),
                            IsSciencePack = _.Get<byte>("is_science_pack") != 0,
                            ApplyRealRequirement = _.Get<byte>("apply_real_requirement") != 0
                        };

                        return SpecificItemTypes.ContainsKey(localItem.BuildType)
                            ? SpecificItemTypes[localItem.BuildType](localItem)
                            : localItem;
                    });

            item.Composition = _dataProvider
                .GetDatas(
                    $"SELECT source_item_id, quantity FROM component WHERE target_item_id = {itemId}",
                    _ => new KeyValuePair<int, int>(
                        _.Get<int>("source_item_id"),
                        _.Get<int>("quantity")))
                .ToDictionary(_ => _.Key, _ => _.Value);

            return item;
        }

        private RecipeItem GetRecipeById(int recipeId)
        {
            var recipe = _dataProvider
                .GetData(
                    $"SELECT id, name, build_time, build_type_id FROM recipe WHERE id = {recipeId}",
                    _ => new RecipeItem
                   {
                       BuildTime = _.Get<decimal>("build_time"),
                       BuildType = (ItemBuildType)_.Get<int>("build_type_id"),
                       Id = _.Get<int>("id"),
                       Name = _.Get<string>("name")
                   });

            recipe.SourceItems = _dataProvider
                .GetDatas(
                    $"SELECT item_id, quantity FROM recipe_source WHERE recipe_id = {recipeId}",
                    _ => new KeyValuePair<int, int>(_.Get<int>("item_id"), _.Get<int>("quantity")))
                .ToDictionary(_ => _.Key, _ => _.Value);

            recipe.TargetItems = _dataProvider
                .GetDatas(
                    $"SELECT item_id, quantity FROM recipe_target WHERE recipe_id = {recipeId}",
                    _ => new KeyValuePair<int, int>(_.Get<int>("item_id"), _.Get<int>("quantity")))
                .ToDictionary(_ => _.Key, _ => _.Value);

            return recipe;
        }

        private decimal GetItemPerSecFromParents(
            List<Item> itemsToProduce,
            List<ProductionItem> itemsResult,
            Item item)
        {
            return itemsToProduce
                .Where(_ => _.Composition.ContainsKey(item.Id))
                .Sum(_ => (_.Composition[item.Id] * (_.ApplyRealRequirement ? itemsResult.Single(x => x.Id == _.Id).RealMachineRequirement : itemsResult.Single(x => x.Id == _.Id).MachineRequirement)) / _.GetRealBuildTime(this));
        }

        private void CheckSuitableItem(
            List<Item> itemsToProduce,
            List<ProductionItem> itemsResult,
            int i)
        {
            var currentItemIsSuitable = false;
            while (!currentItemIsSuitable)
            {
                var it = itemsToProduce[i];
                var parentItems = itemsToProduce.Where(_ => _.Composition.ContainsKey(it.Id)).ToList();
                if (parentItems.Count > 0 && !parentItems.All(_ => itemsResult.Any(x => x.Id == _.Id)))
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
    }
}
