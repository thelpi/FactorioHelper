using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FactorioHelper
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IDataProvider _dataProvider;

        private Properties.Settings Settings => Properties.Settings.Default;

        public MainWindow()
        {
            InitializeComponent();
            _dataProvider = new MySqlDataProvider(Settings.sqlServer, Settings.sqlDatabase, Settings.sqlUid, Settings.sqlPwd);
            MiningDrillTypeComboBox.ItemsSource = Enum.GetValues(typeof(MiningDrillType));
            FurnaceTypeComboBox.ItemsSource = Enum.GetValues(typeof(FurnaceType));
            AssemblingTypeComboBox.ItemsSource = Enum.GetValues(typeof(AssemblingType));
            ItemsComboBox.ItemsSource = GetBaseItemsList();

            // arbitrary default values
            MiningDrillTypeComboBox.SelectedIndex = 1;
            FurnaceTypeComboBox.SelectedIndex = 2;
            AssemblingTypeComboBox.SelectedIndex = 1;
            MiningBonusComboBox.SelectedIndex = 2;
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (MiningDrillTypeComboBox.SelectedIndex < 0
                || FurnaceTypeComboBox.SelectedIndex < 0
                || AssemblingTypeComboBox.SelectedIndex < 0
                || ItemsComboBox.SelectedIndex < 0
                || MiningBonusComboBox.SelectedIndex < 0
                || !decimal.TryParse(
                    TargetPerSecText.Text,
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out decimal targetPerSec)
                || targetPerSec <= 0)
            {
                MessageBox.Show("All fields are mandatory.", "FactorioHelper", MessageBoxButton.OK);
                return;
            }

            var itemId = (ItemsComboBox.SelectedItem as BaseItem).Id;
            var furnaceType = (FurnaceType)FurnaceTypeComboBox.SelectedItem;
            var miningDrillType = (MiningDrillType)MiningDrillTypeComboBox.SelectedItem;
            var miningBonus = MiningBonusComboBox.SelectedIndex;
            var assemblingType = (AssemblingType)AssemblingTypeComboBox.SelectedItem;

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

                var requirements = item.GetProductionUnitRequirements(
                    itemTargetPerSec, furnaceType, miningDrillType, miningBonus, assemblingType);

                itemsResult.Add(item, (int)Math.Ceiling(requirements));
                i++;
            }

            ResultsListBox.ItemsSource = itemsResult;
            ResultsScrollViewer.Visibility = Visibility.Visible;
        }

        private static decimal GetItemPerSecFromParents(
            List<Item> itemsToProduce,
            Dictionary<BaseItem, int> itemsResult,
            Item item)
        {
            return itemsToProduce
                .Where(_ => _.Composition.ContainsKey(item.Id))
                .Sum(_ => (_.Composition[item.Id] * itemsResult[_]) / _.BuildTime);
        }

        private static void CheckSuitableItem(
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

        private IReadOnlyCollection<BaseItem> GetBaseItemsList()
        {
            return _dataProvider
                .GetDatas(
                    "SELECT id, name FROM item",
                    _ => new BaseItem
                    {
                        Id = _.Get<int>("id"),
                        Name = _.Get<string>("name")
                    });
        }
    }

    enum MiningDrillType
    {
        Burner,
        Electric
    }

    enum FurnaceType
    {
        Stone,
        Steel,
        Electric
    }

    enum AssemblingType
    {
        Machine1,
        Machine2,
        Machine3
    }

    enum ItemBuildType
    {
        AssemblingMachine = 1,
        ChemicalPlant,
        Furnace,
        MiningDrill
    }

    class BaseItem
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{Id} - {Name}";
        }
    }

    class Item : BaseItem
    {
        public decimal BuildTime { get; set; }
        public int BuildResult { get; set; }
        public ItemBuildType BuildType { get; set; }
        public IReadOnlyDictionary<int, decimal> Composition { get; set; }

        public decimal GetProductionUnitRequirements(
            decimal targetPerSec,
            FurnaceType furnaceType,
            MiningDrillType miningDrillType,
            int miningBonus, AssemblingType assemblingType)
        {
            decimal productionUnitRequirements = (targetPerSec * BuildTime) / BuildResult;

            switch (BuildType)
            {
                case ItemBuildType.AssemblingMachine:
                    productionUnitRequirements *= assemblingType.GetRate();
                    break;
                case ItemBuildType.ChemicalPlant:
                    productionUnitRequirements *= 1; // nothing more
                    break;
                case ItemBuildType.Furnace:
                    productionUnitRequirements /= furnaceType.GetRate();
                    break;
                case ItemBuildType.MiningDrill:
                    productionUnitRequirements *= miningDrillType.GetRate(miningBonus);
                    break;
            }

            return productionUnitRequirements;
        }
    }

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
            var baseRate = miningDrillType == MiningDrillType.Burner ? 0.25M : 0.5M;
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
    }
}
