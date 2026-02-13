using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using KAI_UI.Core;
using KAI_UI.Services;
using Microsoft.Win32;

namespace KAI_UI.ViewModels
{
    public class TrainingViewModel : ViewModelBase
    {
        private string _datasetPath;
        public string DatasetPath { get => _datasetPath; set { _datasetPath = value; OnPropertyChanged(nameof(DatasetPath)); } }

        private string _modelName;
        public string ModelName { get => _modelName; set { _modelName = value; OnPropertyChanged(nameof(ModelName)); } }

        private string _logOutput;
        public string LogOutput { get => _logOutput; set { _logOutput = value; OnPropertyChanged(nameof(LogOutput)); } }

        private PointCollection _lossPoints;
        public PointCollection LossPoints { get => _lossPoints; set { _lossPoints = value; OnPropertyChanged(nameof(LossPoints)); } }

        private PointCollection _accuracyPoints;
        public PointCollection AccuracyPoints { get => _accuracyPoints; set { _accuracyPoints = value; OnPropertyChanged(nameof(AccuracyPoints)); } }

        private PointCollection _lossFillPoints;
        public PointCollection LossFillPoints { get => _lossFillPoints; set { _lossFillPoints = value; OnPropertyChanged(nameof(LossFillPoints)); } }

        private PointCollection _accuracyFillPoints;
        public PointCollection AccuracyFillPoints { get => _accuracyFillPoints; set { _accuracyFillPoints = value; OnPropertyChanged(nameof(AccuracyFillPoints)); } }


        public ICommand BrowseFolderCommand { get; }
        public ICommand StartTrainingCommand { get; }

        public TrainingViewModel()
        {
            BrowseFolderCommand = new RelayCommand(o => SelectFolder());
            StartTrainingCommand = new RelayCommand(o => StartTraining());

            DatasetPath = "No folder selected";
            ModelName = "new_model_v1";

            LossPoints = new PointCollection();
            AccuracyPoints = new PointCollection();
            LossFillPoints = new PointCollection();
            AccuracyFillPoints = new PointCollection();

            KaiBridge.Initialize(RecibirMensajeDeCpp);
            AppendLog("INFO", "KAI Engine & Log System initialized...");
        }

        private void RecibirMensajeDeCpp(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (message.StartsWith("[")) LogOutput += $"> {message}\n";
                else LogOutput += $"> [ENGINE] {message}\n";

                if (message.Contains("Loss:")) ParseLogForChart(message);
            });
        }

        private void ParseLogForChart(string message)
        {
            try
            {
                var regex = new Regex(@".*Epoch \[(\d+)/(\d+)\] Loss: ([\d\.,]+)");
                var match = regex.Match(message);

                if (match.Success)
                {
                    double epoch = double.Parse(match.Groups[1].Value);
                    string lossString = match.Groups[3].Value.Replace(',', '.');
                    double loss = double.Parse(lossString, System.Globalization.CultureInfo.InvariantCulture);

                    double accuracy = 1.0 / (1.0 + loss);
                    double displayLoss = loss > 1.5 ? 1.5 : loss;

                    double plotLossY = 100.0 - (displayLoss * (100.0 / 1.5));
                    double plotAccY = 100.0 - (accuracy * 100.0);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var currentLossPoints = new PointCollection(LossPoints);
                        var currentAccPoints = new PointCollection(AccuracyPoints);

                        if (currentLossPoints.Count == 0)
                        {
                            currentLossPoints.Add(new Point(0, 0));
                            currentAccPoints.Add(new Point(0, 100));
                        }

                        currentLossPoints.Add(new Point(epoch, plotLossY));
                        currentAccPoints.Add(new Point(epoch, plotAccY));

                        var lossFill = new PointCollection();
                        lossFill.Add(new Point(0, 0));
                        foreach (var p in currentLossPoints) lossFill.Add(p);
                        lossFill.Add(new Point(epoch, 0));

                        var accFill = new PointCollection();
                        accFill.Add(new Point(0, 100));
                        foreach (var p in currentAccPoints) accFill.Add(p);
                        accFill.Add(new Point(epoch, 100));

                        LossPoints = currentLossPoints;
                        AccuracyPoints = currentAccPoints;
                        LossFillPoints = lossFill;
                        AccuracyFillPoints = accFill;
                    });
                }
            }
            catch { }
        }

        private void SelectFolder() 
        { 
            var dialog = new OpenFolderDialog { Title = "Select Dataset Directory" };

            if (dialog.ShowDialog() == true) 
            { 
                DatasetPath = dialog.FolderName;
                AppendLog("ACTION", $"Dataset selected: {DatasetPath}");
            } 
        }
        public void SetDatasetPath(string path) 
        { 
            DatasetPath = path;
            AppendLog("ACTION", $"Dataset dropped: {DatasetPath}");
        }
        private async void StartTraining() 
        { 
            if (string.IsNullOrEmpty(DatasetPath) || DatasetPath == "No folder selected") return;
            
            LossPoints.Clear();
            AccuracyPoints.Clear();
            LossFillPoints.Clear();
            AccuracyFillPoints.Clear();
            try
            {
                string baseDir = @"C:\KAI_Models";
                string outputFolder = Path.Combine(baseDir, ModelName);
                string modelFile = Path.Combine(outputFolder, $"{ModelName}.pt");

                if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

                AppendLog("SUCCESS", $"STARTING TRAINING SESSION: {ModelName}");

                int batch = AppSettings.Instance.BatchSize;
                int filters = AppSettings.Instance.BaseFilters;
                int neurons = AppSettings.Instance.HiddenNeurons;

                AppendLog("INFO", $"Architecture -> Batch: {batch} | Filters: {filters} | Neurons: {neurons}");

                await KaiBridge.TrainAsync(DatasetPath, modelFile, 50, 0.001f, batch, filters, neurons);

                AppendLog("SUCCESS", "Training process finished.");
            }
            catch (Exception ex) 
            {
                AppendLog("FATAL", $"Engine Error: {ex.Message}");
            } 
        }
        private void AppendLog(string type, string message) 
        {
            LogOutput += $"> [{type}] {message}\n"; 
        }
    }
}