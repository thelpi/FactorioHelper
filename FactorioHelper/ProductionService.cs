using System;
using System.Collections.Generic;
using System.Linq;

namespace FactorioHelper
{
    class ProductionService
    {
        private readonly IDataProvider _dataProvider;

        private readonly FurnaceType _furnaceType;
        private readonly MiningDrillType _miningDrillType;
        private readonly int _miningBonus;
        private readonly AssemblingType _assemblingType;

        internal ProductionService(
            IDataProvider dataProvider,
            FurnaceType furnaceType,
            MiningDrillType miningDrillType,
            int miningBonus,
            AssemblingType assemblingType)
        {
            _dataProvider = dataProvider;
            _furnaceType = furnaceType;
            _miningDrillType = miningDrillType;
            _miningBonus = miningBonus;
            _assemblingType = assemblingType;
        }

        private decimal GetRealBuildTime(Item item)
        {
            var buildTime = item.BuildTime;
            switch (item.BuildType)
            {
                case ItemBuildType.AssemblingMachine:
                    buildTime /= _assemblingType.GetRate();
                    break;
                case ItemBuildType.ChemicalPlant:
                    buildTime *= 1; // nothing more
                    break;
                case ItemBuildType.Furnace:
                    buildTime /= _furnaceType.GetRate();
                    break;
                case ItemBuildType.MiningDrill:
                    buildTime /= _miningDrillType.GetRate(_miningBonus);
                    break;
            }
            return buildTime / item.BuildResult;
        }

        private List<Item> GetFullListOfItemsToProduce(int itemId)
        {
            var itemsToProduce = new List<Item>();

            var item = GetItemById(itemId);
            itemsToProduce.Add(item);
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
                    _ => new Item
                    {
                        BuildResult = _.Get<int>("build_result"),
                        BuildTime = _.Get<decimal>("build_time"),
                        BuildType = (ItemBuildType)_.Get<int>("build_type_id"),
                        Id = _.Get<int>("id"),
                        Name = _.Get<string>("name")
                    });

            item.Composition = _dataProvider
                .GetDatas(
                    $"SELECT source_item_id, quantity FROM component WHERE target_item_id = {itemId}",
                    _ => new KeyValuePair<int, decimal>(
                        _.Get<int>("source_item_id"),
                        _.Get<decimal>("quantity")))
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
                .Sum(_ => (_.Composition[item.Id] * itemsResult[_]) / GetRealBuildTime(_));
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

        internal Dictionary<BaseItem, int> GetItemsToProduce(decimal targetPerSec, int itemId)
        {
            var itemsToProduce = GetFullListOfItemsToProduce(itemId);

            var itemsResult = new Dictionary<BaseItem, int>();

            var i = 0;
            while (i < itemsToProduce.Count)
            {
                CheckSuitableItem(itemsToProduce, itemsResult, i);

                var item = itemsToProduce[i];

                var itemTargetPerSec = item.Id == itemId
                    ? targetPerSec
                    : GetItemPerSecFromParents(itemsToProduce, itemsResult, item);

                var requirements = targetPerSec * GetRealBuildTime(item);

                itemsResult.Add(item, (int)Math.Ceiling(requirements));
                i++;
            }

            return itemsResult;
        }
    }
}
