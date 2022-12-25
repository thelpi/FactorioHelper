using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using FactorioHelper.Enums;
using FactorioHelper.Items;

namespace FactorioHelper
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ProductionService _productionService;
        private readonly ObservableCollection<ModuleConfiguration> _modules;

        private Properties.Settings Settings => Properties.Settings.Default;

        public MainWindow()
        {
            InitializeComponent();

            _productionService = new ProductionService(
                new MySqlDataProvider(
                    Settings.sqlServer,
                    Settings.sqlDatabase,
                    Settings.sqlUid,
                    Settings.sqlPwd));
            _modules = new ObservableCollection<ModuleConfiguration>();

            MiningDrillTypeComboBox.ItemsSource = Enum.GetValues(typeof(MiningDrillType));
            FurnaceTypeComboBox.ItemsSource = Enum.GetValues(typeof(FurnaceType));
            AssemblingTypeComboBox.ItemsSource = Enum.GetValues(typeof(AssemblingType));
            ItemsComboBox.ItemsSource = _productionService.GetBaseItemsList();
            ModulesListBox.ItemsSource = _modules;

            // arbitrary default values
            ItemsComboBox.SelectedIndex = 0;
            MiningDrillTypeComboBox.SelectedIndex = 1;
            FurnaceTypeComboBox.SelectedIndex = 2;
            AssemblingTypeComboBox.SelectedIndex = 1;
            MiningBonusComboBox.SelectedIndex = 2;
            TargetPerSecText.Text = 1.2M.ToString(CultureInfo.InvariantCulture);
            AdvancedRefiningCheckBox.IsChecked = true;
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckFormInput(out var targetPerSec))
            {
                MessageBox.Show("All fields are mandatory.", "FactorioHelper", MessageBoxButton.OK);
                return;
            }

            var itemId = (ItemsComboBox.SelectedItem as BaseItem).Id;

            _productionService.AssemblingType = (AssemblingType)AssemblingTypeComboBox.SelectedItem;
            _productionService.FurnaceType = (FurnaceType)FurnaceTypeComboBox.SelectedItem;
            _productionService.MiningDrillType = (MiningDrillType)MiningDrillTypeComboBox.SelectedItem;
            _productionService.MiningBonus = MiningBonusComboBox.SelectedIndex;
            _productionService.AdvancedOilProcessing = AdvancedRefiningCheckBox.IsChecked == true;
            _productionService.SetModulesConfiguration(_modules);

            var production = _productionService.GetItemsToProduce(targetPerSec, itemId);
            var oilProduction = _productionService.GetOilToProduce(production);

            ResultsListBox.ItemsSource = production;
            ResultsScrollViewer.Visibility = Visibility.Visible;

            OilRemainsListBox.ItemsSource = oilProduction.RemainsPerSec;
            RefineryOilResultsListBox.ItemsSource = oilProduction.RefineryRequirements;
            ChemicalOilResultsListBox.ItemsSource = oilProduction.ChemicalPlantRequirements;
            OilResultsScrollViewer.Visibility = Visibility.Visible;
        }

        private bool CheckFormInput(out decimal targetPerSec)
        {
            targetPerSec = 0;

            if (MiningDrillTypeComboBox.SelectedIndex < 0
                || FurnaceTypeComboBox.SelectedIndex < 0
                || AssemblingTypeComboBox.SelectedIndex < 0
                || ItemsComboBox.SelectedIndex < 0
                || MiningBonusComboBox.SelectedIndex < 0)
            {
                return false;
            }

            decimal.TryParse(TargetPerSecText.Text,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out targetPerSec);

            return targetPerSec > 0;
        }

        private void AddModuleButton_Click(object sender, RoutedEventArgs e)
        {
            _modules.Add(new ModuleConfiguration
            {
                BuildType = ItemBuildType.AssemblingMachine,
                Count = 1,
                Module = ModuleType.Speed1
            });
            ModulesListBox.Visibility = Visibility.Visible;
        }

        private void RemoveModuleButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            _modules.Remove(button.DataContext as ModuleConfiguration);
            ModulesListBox.Visibility = _modules.Count == 0
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void TextBoxModuleCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (int.TryParse(textBox.Text, out var countValue) && countValue > 0)
            {
                var item = textBox.DataContext as ModuleConfiguration;
                item.Count = countValue;
            }
        }

        private void ComboBoxItemType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo.SelectedIndex > -1)
            {
                var item = combo.DataContext as ModuleConfiguration;
                item.BuildType = (ItemBuildType)combo.SelectedItem;
            }
        }

        private void ComboBoxModule_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo.SelectedIndex > -1)
            {
                var item = combo.DataContext as ModuleConfiguration;
                item.Module = (ModuleType)combo.SelectedItem;
            }
        }
    }
}
