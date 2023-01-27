using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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

        public MainWindow()
        {
            InitializeComponent();

            _productionService = new ProductionService();
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
            CrudeOilInitialYieldText.Text = 500.ToString();
            SolidFuelHeavyRate.Text = "0/3";
            SolidFuelLightRate.Text = "3/3";
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckFormInput(out var targetPerSec,
                out var crudeOilInitialYield,
                out var solidFuelHeavyRate,
                out var solidFuelLightRate))
            {
                MessageBox.Show("Fields missing or invalid.", "FactorioHelper", MessageBoxButton.OK);
                return;
            }

            var itemId = (ItemsComboBox.SelectedItem as BaseItem).Id;

            _productionService.CrudeOilInitialYield = crudeOilInitialYield;
            _productionService.AssemblingType = (AssemblingType)AssemblingTypeComboBox.SelectedItem;
            _productionService.FurnaceType = (FurnaceType)FurnaceTypeComboBox.SelectedItem;
            _productionService.MiningDrillType = (MiningDrillType)MiningDrillTypeComboBox.SelectedItem;
            _productionService.MiningBonus = MiningBonusComboBox.SelectedIndex;
            _productionService.AdvancedOilProcessing = AdvancedRefiningCheckBox.IsChecked == true;
            _productionService.SolidFuelRateConsumption = new Dictionary<int, Fraction>
            {
                { ProductionService.HeavyOilId, solidFuelHeavyRate },
                { ProductionService.LightOilId, solidFuelLightRate },
                { ProductionService.PetroleumGasId, 1 - (solidFuelLightRate + solidFuelHeavyRate) }
            };
            var modulesList = _modules.ToList();

            Loadingbar.Visibility = Visibility.Visible;
            MainPanel.Visibility = Visibility.Hidden;

            var worker = new BackgroundWorker();
            worker.DoWork += (object _, DoWorkEventArgs dwe) =>
            {
                _productionService.SetModulesConfiguration(modulesList);
                var production = _productionService.GetItemsToProduce(targetPerSec, itemId);
                var oilProduction = _productionService.GetOilToProduce(production);
                dwe.Result = new Tuple<IEnumerable<ProductionItem>, OilProductionOutput>(production.Values, oilProduction);
            };
            worker.RunWorkerCompleted += (object _, RunWorkerCompletedEventArgs rwce) =>
            {
                var actualResult = rwce.Result as Tuple<IEnumerable<ProductionItem>, OilProductionOutput>;

                DataContext = actualResult;
                ProductionSelectionCombo.ItemsSource = actualResult.Item1;
                ResultsListBox.ItemsSource = actualResult.Item1;
                ResultsPanel.Visibility = Visibility.Visible;

                OilRemainsListBox.ItemsSource = actualResult.Item2.RemainsPerSec;
                RefineryOilResultsListBox.ItemsSource = actualResult.Item2.RefineryRequirements;
                ChemicalOilResultsListBox.ItemsSource = actualResult.Item2.ChemicalPlantRequirements;
                OilResultsScrollViewer.Visibility = Visibility.Visible;

                Loadingbar.Visibility = Visibility.Collapsed;
                MainPanel.Visibility = Visibility.Visible;

                CenterWindow();
            };
            worker.RunWorkerAsync();
        }

        private void CenterWindow()
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                System.Threading.Thread.Sleep(10);
                Dispatcher.Invoke(() =>
                {
                    Left = (SystemParameters.PrimaryScreenWidth / 2) - (Width / 2);
                    Top = (SystemParameters.PrimaryScreenHeight / 2) - (Height / 2);
                });
            });
        }

        private bool CheckFormInput(out Fraction targetPerSec,
            out int crudeOilInitialYield,
            out Fraction solidFuelHeavyRate,
            out Fraction solidFuelLightRate)
        {
            targetPerSec = 0;
            crudeOilInitialYield = 0;
            solidFuelHeavyRate = 0;
            solidFuelLightRate = 0;

            if (MiningDrillTypeComboBox.SelectedIndex < 0
                || FurnaceTypeComboBox.SelectedIndex < 0
                || AssemblingTypeComboBox.SelectedIndex < 0
                || ItemsComboBox.SelectedIndex < 0
                || MiningBonusComboBox.SelectedIndex < 0)
            {
                return false;
            }

            if (!Fraction.TryParse(SolidFuelHeavyRate.Text, out solidFuelHeavyRate)
                || !Fraction.TryParse(SolidFuelLightRate.Text, out solidFuelLightRate)
                || solidFuelHeavyRate + solidFuelLightRate > 1
                || solidFuelHeavyRate + solidFuelLightRate < 0)
            {
                return false;
            }

            Fraction.TryParse(TargetPerSecText.Text, out targetPerSec);

            int.TryParse(CrudeOilInitialYieldText.Text, out crudeOilInitialYield);

            return targetPerSec > 0 && crudeOilInitialYield > 0;
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

        private void ProductionSelectionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductionSelectionCombo.SelectedItem is ProductionItem pItem)
            {
                var data = DataContext as Tuple<IEnumerable<ProductionItem>, OilProductionOutput>;

                ResultsListBox.ItemsSource = data.Item1.Where(x => x.Id == pItem.Id || x.Components.Any(_ => _.Id == pItem.Id));
            }
        }

        private void ProductionSelectRemoval_Click(object sender, RoutedEventArgs e)
        {
            var data = DataContext as Tuple<IEnumerable<ProductionItem>, OilProductionOutput>;

            ProductionSelectionCombo.SelectedItem = null;
            ResultsListBox.ItemsSource = data.Item1;
        }
    }
}
