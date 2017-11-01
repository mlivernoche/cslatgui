using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CslAutomatedTestApi;
using CudaSharper;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Threading;

namespace CSLAutomatedTestApp
{
    public struct TestParallelismOption
    {
        public CudaTestParallelism DegreeOfParallelism { get; }
        public int Index => (int)DegreeOfParallelism;
        public string OptionName { get; }

        public TestParallelismOption(CudaTestParallelism dop, string name)
        {
            DegreeOfParallelism = dop;
            OptionName = name;
        }
    }

    public class TestParallelismCollection : ObservableCollection<TestParallelismOption>
    {
        public TestParallelismCollection()
        {
            Add(new TestParallelismOption(CudaTestParallelism.DoParallelizeTests, "Do use parallelism"));
            Add(new TestParallelismOption(CudaTestParallelism.DoNotParallelizeTests, "Do not use parallelism"));
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DataBindings Bindings { get; }

        public MainWindow()
        {
            CudaSettings.Load();
            InitializeComponent();
            Bindings = new DataBindings();
            DataContext = Bindings;
        }

        public void AddTextToTestResultTextBox(StringBuilder sb)
        {
            TextBoxOutput.Text += sb.ToString();
        }

        public void AddTextToTestResultTextBox(string sb)
        {
            TextBoxOutput.Text += sb;
        }

        public void ClearText()
        {
            TextBoxOutput.Text = string.Empty;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Bindings.ParametersList.Add(new CudaTestParameters()
            {
                Range = 100_000,
                NumOfTests = 50,
                DeviceId = 0,
                DegreeOfParallelism = CudaTestParallelism.DoNotParallelizeTests
            });
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var stringbuilder = new StringBuilder();
            foreach (var parameters in Bindings.ParametersList)
            {
                var device = new CudaDevice(parameters.DeviceId, 0);
                stringbuilder.AppendLine($"Using device: {device.DeviceName} ({parameters.DeviceId})");
                stringbuilder.AppendLine($"Number of tests: {parameters.NumOfTests}");
                stringbuilder.AppendLine($"Range per test: {parameters.Range}");
                stringbuilder.AppendLine($"Degree of Parallelism: {parameters.DegreeOfParallelism.ToString()}");
            }

            AddTextToTestResultTextBox(stringbuilder);
            RunTestsButton.IsEnabled = true;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ClearText();
            TestResultsTreeView.Items.Clear();
            RunTestsButton.IsEnabled = false;
        }

        private async void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var tasks = new List<CudaDeviceTestFp32>();

            foreach (var parameters in Bindings.ParametersList)
            {
                var local_parameters = parameters;
                var progress = new Progress<double>(i => { ProgressPerentageLabel.Content = string.Format("Test #{1}: {0:##0.000%}", i, tasks.Count + 1); });
                var asyncTest = Task.Run(() => new CudaDeviceTestFp32(local_parameters, progress));
                tasks.Add(await asyncTest);
                ProgressPerentageLabel.Content = string.Format("Test #{0}: Finished", tasks.Count);
            }

            foreach (var testResults in tasks)
            {
                AddTextToTestResultTextBox(new CudaTestString(testResults.Results).ToString());

                var tree_view_level1 = new TreeViewItem() { Header = $"Results for GPU{testResults.DeviceId}" };

                var testParameters = testResults.Results.TestParametersUsed;
                var testParametersTreeView = new TreeViewItem() { Header = "Test Parameters" };
                testParametersTreeView.Items.Add(new TreeViewItem() { Header = $"DeviceId: {testParameters.DeviceId}" });
                testParametersTreeView.Items.Add(new TreeViewItem() { Header = $"Degree of parallelism: {testParameters.DegreeOfParallelism}" });
                testParametersTreeView.Items.Add(new TreeViewItem() { Header = $"Number of tests: {testParameters.NumOfTests}" });
                testParametersTreeView.Items.Add(new TreeViewItem() { Header = $"Range per test: {testParameters.Range}" });

                tree_view_level1.Items.Add(testParametersTreeView);

                foreach (var testResultCollection in testResults.Results)
                {
                    var tree_view_level2 = new TreeViewItem()
                    {
                        Header = $"Passed: {testResultCollection.Passed.ToString()}",
                        FontWeight = !testResultCollection.Passed ? FontWeights.Bold : FontWeights.Regular
                    };

                    foreach (var testResult in testResultCollection)
                    {
                        var tree_view_level3 = new TreeViewItem()
                        {
                            Header = testResult.Name,
                            FontWeight = !testResult.PassedTest ? FontWeights.Bold : FontWeights.Regular
                        };
                        tree_view_level3.Items.Add(new TreeViewItem() { Header = "Value: " + testResult.Value });
                        tree_view_level3.Items.Add(new TreeViewItem() { Header = "Passed Test: " + testResult.PassedTest.ToString() });
                        tree_view_level3.Items.Add(new TreeViewItem() { Header = "Error Code: " + testResult.ErrorCode.ToString() });
                        tree_view_level2.Items.Add(tree_view_level3);
                    }

                    tree_view_level1.Items.Add(tree_view_level2);
                }

                TestResultsTreeView.Items.Add(tree_view_level1);
            }
        }

        private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            RunTestsButton.IsEnabled = false;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Bindings.ParametersList.Clear();
        }
    }
}
