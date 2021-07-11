using System;
using System.Globalization;
using System.Windows;
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
            MiningDrillTypeComboBox.ItemsSource = Enum.GetValues(typeof(MiningDrillType));
            FurnaceTypeComboBox.ItemsSource = Enum.GetValues(typeof(FurnaceType));
            AssemblingTypeComboBox.ItemsSource = Enum.GetValues(typeof(AssemblingType));
            ItemsComboBox.ItemsSource = _productionService.GetBaseItemsList(true);

            // arbitrary default values
            ItemsComboBox.SelectedIndex = 0;
            MiningDrillTypeComboBox.SelectedIndex = 1;
            FurnaceTypeComboBox.SelectedIndex = 2;
            AssemblingTypeComboBox.SelectedIndex = 1;
            MiningBonusComboBox.SelectedIndex = 2;
            TargetPerSecText.Text = "1.2";
            AdvancedRefiningCheckBox.IsChecked = true;
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckFormInput(out decimal targetPerSec, out decimal targetCount))
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

            var production = _productionService.GetItemsToProduce(targetPerSec, targetCount, itemId);
            var oilProduction = _productionService.GetOilToProduce(production);

            ResultsListBox.ItemsSource = production;
            ResultsScrollViewer.Visibility = Visibility.Visible;

            RefineryOilResultsListBox.ItemsSource = oilProduction.RefineryRequirements;
            ChemicalOilResultsListBox.ItemsSource = oilProduction.ChemicalPlantRequirements;
            OilResultsScrollViewer.Visibility = Visibility.Visible;
        }

        private bool CheckFormInput(out decimal targetPerSec, out decimal targetCount)
        {
            targetCount = 0;
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
            decimal.TryParse(TargetMachineText.Text,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out targetCount);

            return targetPerSec > 0 || targetCount > 0;
        }
    }
}
