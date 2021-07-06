using System;
using System.Collections.Generic;
using System.Linq;

namespace FactorioHelper
{
    class ProductionService
    {
        private const int EachSciencePackItemId = 0;
        private static readonly int[] SciencePackIdList = new[] { 1, 7, 12, 21, 28, 33 };

        private readonly IDataProvider _dataProvider;

        public FurnaceType _furnaceType { get; set; }
        public MiningDrillType _miningDrillType { get; set; }
        public int _miningBonus { get; set; }
        public AssemblingType _assemblingType { get; set; }

        internal ProductionService(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        internal Dictionary<BaseItem, int> GetItemsToProduce(decimal targetPerSec, int itemId)
        {
            var itemsToProduce = GetFullListOfItemsToProduce(itemId);

            var itemsResult = new Dictionary<BaseItem, int>();

            var i = 0;
            while (i < itemsToProduce.Count)
            {
                CheckSuitableItem(itemsToProduce, itemsResult, i);

                var item = itemsToProduce[i];

                var itemTargetPerSec = (item.Id == itemId || (itemId == EachSciencePackItemId && SciencePackIdList.Contains(item.Id)))
                    ? targetPerSec
                    : GetItemPerSecFromParents(itemsToProduce, itemsResult, item);

                var requirements = itemTargetPerSec * (item.GetRealBuildTime(this) / item.BuildResult);

                itemsResult.Add(item, (int)Math.Ceiling(requirements));
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

                        switch ((ItemBuildType)_.Get<int>("build_type_id"))
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
                        }

                        localItem.Id = _.Get<int>("id");
                        localItem.Name = _.Get<string>("name");
                        localItem.BuildResult = _.Get<int>("build_result");
                        localItem.BuildTime = _.Get<decimal>("build_time");
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

        private decimal GetItemPerSecFromParents(
            List<Item> itemsToProduce,
            Dictionary<BaseItem, int> itemsResult,
            Item item)
        {
            return itemsToProduce
                .Where(_ => _.Composition.ContainsKey(item.Id))
                .Sum(_ => (_.Composition[item.Id] * itemsResult[_]) / _.GetRealBuildTime(this));
        }

        private void CheckSuitableItem(
            List<Item> itemsToProduce,
            Dictionary<BaseItem, int>
            itemsResult, int i)
        {
            var currentItemIsSuitable = false;
            while (!currentItemIsSuitable)
            {
                var it = itemsToProduce[i];
                var parentItems = itemsToProduce.Where(_ => _.Composition.ContainsKey(it.Id)).ToList();
                if (parentItems.Count > 0 && !parentItems.All(_ => itemsResult.ContainsKey(_)))
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
