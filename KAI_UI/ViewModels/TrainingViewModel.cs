using System;
using System.IO;
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
        public string DatasetPath
        {
            get { return _datasetPath; }
            set { _datasetPath = value; OnPropertyChanged(nameof(DatasetPath)); }
        }

        private string _modelName;
        public string ModelName
        {
            get { return _modelName; }
            set { _modelName = value; OnPropertyChanged(nameof(ModelName)); }
        }

        private string _logOutput;
        public string LogOutput
        {
            get { return _logOutput; }
            set { _logOutput = value; OnPropertyChanged(nameof(LogOutput)); }
        }

        private PointCollection _lossPoints;
        public PointCollection LossPoints
        {
            get { return _lossPoints; }
            set { _lossPoints = value; OnPropertyChanged(nameof(LossPoints)); }
        }

        private PointCollection _accuracyPoints;
        public PointCollection AccuracyPoints
        {
            get { return _accuracyPoints; }
            set { _accuracyPoints = value; OnPropertyChanged(nameof(AccuracyPoints)); }
        }

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

            KaiBridge.Initialize(RecibirMensajeDeCpp);
            AppendLog("INFO", "KAI Engine & Log System initialized...");
        }

        private void RecibirMensajeDeCpp(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (message.StartsWith("["))
                    LogOutput += $"> {message}\n";
                else
                    LogOutput += $"> [ENGINE] {message}\n";

                if (message.Contains("Loss:"))
                {
                    ParseLogForChart(message);
                }
            });
        }

        private void ParseLogForChart(string message)
        {
            try
            {
                var regex = new Regex(@".*Epoca \[(\d+)/(\d+)\] Loss: ([\d\.,]+)");
                var match = regex.Match(message);

                if (match.Success)
                {
                    double epoch = double.Parse(match.Groups[1].Value);

                    string lossString = match.Groups[3].Value.Replace(',', '.');
                    double loss = double.Parse(lossString, System.Globalization.CultureInfo.InvariantCulture);

                    double accuracy = 1.0 / (1.0 + loss);

                    double visualLoss = loss * 100;
                    if (visualLoss > 100) visualLoss = 100;

                    double visualAccuracy = accuracy * 100;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var newLossPoints = new PointCollection(LossPoints);
                        var newAccPoints = new PointCollection(AccuracyPoints);

                        newLossPoints.Add(new Point(epoch, visualLoss));
                        newAccPoints.Add(new Point(epoch, visualAccuracy));

                        LossPoints = newLossPoints;
                        AccuracyPoints = newAccPoints;
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

            try
            {
                string baseDir = @"C:\KAI_Models";
                string outputFolder = Path.Combine(baseDir, ModelName);
                string modelFile = Path.Combine(outputFolder, $"{ModelName}.pt");

                if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

                AppendLog("SUCCESS", $"STARTING TRAINING SESSION: {ModelName}");
                await KaiBridge.TrainAsync(DatasetPath, modelFile, 50, 0.001f);
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