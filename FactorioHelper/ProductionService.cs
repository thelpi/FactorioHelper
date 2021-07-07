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

        private const int BasicOilProcessingRecipeId = 1;

        private const int EachSciencePackItemId = 0;
        private static readonly int[] SciencePackIdList = new[] { 1, 7, 12, 21, 28, 33 };

        private readonly IDataProvider _dataProvider;

        public FurnaceType FurnaceType { get; set; }
        public MiningDrillType MiningDrillType { get; set; }
        public int MiningBonus { get; set; }
        public AssemblingType AssemblingType { get; set; }
        public bool AdvancedOilProcessing { get; set; }

        internal ProductionService(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        internal OilProductionOutput GetOilToProduce(List<ProductionItem> fromProduction)
        {
            var lightReq = GetOilRequirement(fromProduction, LightOilId);
            var heavyReq = GetOilRequirement(fromProduction, HeavyOilId);
            var gazReqPerSec = GetOilRequirement(fromProduction, PetroleumGasId);

            if (lightReq > 0 || heavyReq > 0)
            {
                // required anyway
                AdvancedOilProcessing = true;
            }
            else if (gazReqPerSec == 0)
            {
                return new OilProductionOutput
                {
                    ChemicalPlantRequirements = new Dictionary<int, int>(),
                    RefineryRequirements = new Dictionary<int, int>()
                };
            }

            if (!AdvancedOilProcessing)
            {
                var basicOilProcessingRecipe = GetRecipeById(BasicOilProcessingRecipeId);
                
                var gazProdPerSec = basicOilProcessingRecipe.GetTargetPerSec(PetroleumGasId);
                
                var refineryReq = (int)Math.Ceiling(gazReqPerSec / gazProdPerSec);
                
                fromProduction.RemoveAll(_ => _.Id == PetroleumGasId);

                var sourceItems = basicOilProcessingRecipe.SourceItems
                    .Select(_ => new KeyValuePair<int, decimal>(_.Key, basicOilProcessingRecipe.GetSourcePerSec(_.Key) * refineryReq))
                    .ToList();

                foreach (var sourceItem in sourceItems)
                {
                    var itembaseInfo = GetItemById(sourceItem.Key);
                    var existingItem = fromProduction.FirstOrDefault(_ => _.Id == sourceItem.Key);
                    var timeRate = itembaseInfo.GetRealBuildTime(this) / itembaseInfo.BuildResult;
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

                return new OilProductionOutput
                {
                    ChemicalPlantRequirements = new Dictionary<int, int>(),
                    RefineryRequirements = new Dictionary<int, int> { { BasicOilProcessingRecipeId, refineryReq } }
                };
            }

            var recipes = GetRecipes();

            return null;
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

                var itemTargetPerSec = (item.Id == itemId || (itemId == EachSciencePackItemId && SciencePackIdList.Contains(item.Id)))
                    ? targetPerSec
                    : GetItemPerSecFromParents(itemsToProduce, itemsResult, item);

                var rate = item.GetRealBuildTime(this) / item.BuildResult;

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
                items.Insert(0, new BaseItem { Id = EachSciencePackItemId, Name = "Each science pack" });
            }

            return items;
        }

        private List<Item> GetFullListOfItemsToProduce(int itemId)
        {
            var itemsToProduce = new List<Item>();

            if (itemId == EachSciencePackItemId)
                itemsToProduce.AddRange(SciencePackIdList.Select(_ => GetItemById(_)));
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

        private Item GetItemById(int itemId)
        {
            var item = _dataProvider
                .GetData(
                    $"SELECT id, name, build_time, build_result, build_type_id FROM item WHERE id = {itemId}",
                    _ =>
                    {
                        Item localItem = null;

                        var buildType = (ItemBuildType)_.Get<int>("build_type_id");
                        switch (buildType)
                        {
                            case ItemBuildType.AssemblingMachine:
                                localItem = new AssemblingItem();
                                break;
                            case ItemBuildType.Furnace:
                                localItem = new FurnaceItem();
                                break;
                            case ItemBuildType.MiningDrill:
                                localItem = new MiningItem();
                                break;
                            case ItemBuildType.Refining:
                                localItem = new RefiningItem();
                                break;
                            case ItemBuildType.ChemicalPlant:
                                localItem = new ChemicalItem();
                                break;
                            case ItemBuildType.Other:
                                localItem = new OtherItem();
                                break;
                        }

                        localItem.Id = _.Get<int>("id");
                        localItem.Name = _.Get<string>("name");
                        localItem.BuildResult = _.Get<int>("build_result");
                        localItem.BuildTime = _.Get<decimal>("build_time");
                        localItem.BuildType = buildType;
                        return localItem;
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

        private IReadOnlyCollection<RecipeItem> GetRecipes()
        {
            return _dataProvider
                .GetDatas($"SELECT id FROM recipe", _ => _.Get<int>("id"))
                .Select(_ => GetRecipeById(_))
                .ToList();
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
                .Sum(_ => (_.Composition[item.Id] * itemsResult.Single(x => x.Id == _.Id).MachineRequirement) / _.GetRealBuildTime(this));
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
