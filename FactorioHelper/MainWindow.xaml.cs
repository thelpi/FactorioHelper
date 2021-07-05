using System;
using System.Collections.Generic;
using System.Globalization;
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
            if (!CheckFormInput(out decimal targetPerSec))
            {
                MessageBox.Show("All fields are mandatory.", "FactorioHelper", MessageBoxButton.OK);
                return;
            }

            var itemId = (ItemsComboBox.SelectedItem as BaseItem).Id;

            var productionService = new ProductionService(
                _dataProvider,
                (FurnaceType)FurnaceTypeComboBox.SelectedItem,
                (MiningDrillType)MiningDrillTypeComboBox.SelectedItem,
                MiningBonusComboBox.SelectedIndex,
                (AssemblingType)AssemblingTypeComboBox.SelectedItem);

            ResultsListBox.ItemsSource = productionService.GetItemsToProduce(targetPerSec, itemId);
            ResultsScrollViewer.Visibility = Visibility.Visible;
        }

        private bool CheckFormInput(out decimal targetPerSec)
        {
            targetPerSec = 0;
            return MiningDrillTypeComboBox.SelectedIndex >= 0
                && FurnaceTypeComboBox.SelectedIndex >= 0
                && AssemblingTypeComboBox.SelectedIndex >= 0
                && ItemsComboBox.SelectedIndex >= 0
                && MiningBonusComboBox.SelectedIndex >= 0
                && decimal.TryParse(
                    TargetPerSecText.Text,
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out targetPerSec)
                && targetPerSec > 0;
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
