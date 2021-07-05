using System;
using System.Globalization;
using System.Windows;

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

            _productionService._assemblingType = (AssemblingType)AssemblingTypeComboBox.SelectedItem;
            _productionService._furnaceType = (FurnaceType)FurnaceTypeComboBox.SelectedItem;
            _productionService._miningDrillType = (MiningDrillType)MiningDrillTypeComboBox.SelectedItem;
            _productionService._miningBonus = MiningBonusComboBox.SelectedIndex;

            ResultsListBox.ItemsSource = _productionService.GetItemsToProduce(targetPerSec, itemId);
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
    }
}
