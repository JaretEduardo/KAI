using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using KAI_UI.Core;
using KAI_UI.Services;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Threading;
using System.Text.Json;

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

        private List<(double Epoch, double Loss, double Accuracy)> _trainingHistory = new List<(double, double, double)>();

        private double _gpuUsage;
        public double GpuUsage { get => _gpuUsage; set { _gpuUsage = value; OnPropertyChanged(nameof(GpuUsage)); } }

        private string _gpuUsageText = "GPU: Idle";
        public string GpuUsageText { get => _gpuUsageText; set { _gpuUsageText = value; OnPropertyChanged(nameof(GpuUsageText)); } }


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

            DispatcherTimer gpuTimer = new DispatcherTimer();
            gpuTimer.Interval = TimeSpan.FromSeconds(1);
            gpuTimer.Tick += UpdateGpuUsage;
            gpuTimer.Start();
        }

        private int _totalEpochs = 50;
        public int TotalEpochs
        {
            get => _totalEpochs;
            set { _totalEpochs = value; OnPropertyChanged(nameof(TotalEpochs)); }
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

                    _trainingHistory.Add((epoch, displayLoss, accuracy));

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var newLossPoints = new PointCollection();
                        var newAccPoints = new PointCollection();

                        newLossPoints.Add(new Point(0, 0));
                        newAccPoints.Add(new Point(0, 500));

                        double maxEpoch = Math.Max(2.0, _trainingHistory.Last().Epoch);

                        foreach (var item in _trainingHistory)
                        {
                            double plotX = (item.Epoch / maxEpoch) * 1000.0;
                            double plotLossY = 500.0 - (item.Loss * (500.0 / 1.5));
                            double plotAccY = 500.0 - (item.Accuracy * 500.0);

                            newLossPoints.Add(new Point(plotX, plotLossY));
                            newAccPoints.Add(new Point(plotX, plotAccY));
                        }

                        double lastX = (_trainingHistory.Last().Epoch / maxEpoch) * 1000.0;

                        var lossFill = new PointCollection();
                        lossFill.Add(new Point(0, 0));
                        foreach (var p in newLossPoints) lossFill.Add(p);
                        lossFill.Add(new Point(lastX, 0));

                        var accFill = new PointCollection();
                        accFill.Add(new Point(0, 500));
                        foreach (var p in newAccPoints) accFill.Add(p);
                        accFill.Add(new Point(lastX, 500));

                        LossPoints = newLossPoints;
                        AccuracyPoints = newAccPoints;
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

        public class ModelMetadata
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public DateTime CreatedAt { get; set; }
            public string WeightsFilename { get; set; }
            public string Framework { get; set; } = "LibTorch (C++)";
            public ModelHyperparameters Hyperparameters { get; set; }
            public ModelMetrics Metrics { get; set; }
            public List<EpochData> TrainingHistory { get; set; }
        }

        public class EpochData
        {
            public int Epoch { get; set; }
            public float Loss { get; set; }
            public float Accuracy { get; set; }
        }

        public class ModelHyperparameters
        {
            public int Epochs { get; set; }
            public int BatchSize { get; set; }
            public float LearningRate { get; set; }
            public int BaseFilters { get; set; }
            public int HiddenNeurons { get; set; }
        }

        public class ModelMetrics
        {
            public float FinalLoss { get; set; }
            public float FinalAccuracy { get; set; }
        }

        private async void StartTraining() 
        { 
            if (string.IsNullOrEmpty(DatasetPath) || DatasetPath == "No folder selected") return;
            
            LossPoints.Clear();
            AccuracyPoints.Clear();
            LossFillPoints.Clear();
            AccuracyFillPoints.Clear();
            _trainingHistory.Clear();
            try
            {
                string baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "KAI_Models");
                string modelUUID = Guid.NewGuid().ToString();

                string outputFolder = Path.Combine(baseDir, ModelName);
                if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

                string weightsFileName = $"{modelUUID}.pt";
                string fullWeightsPath = Path.Combine(outputFolder, weightsFileName);
                string jsonPath = Path.Combine(outputFolder, "metadata.json");

                AppendLog("SUCCESS", $"STARTING TRAINING SESSION: {ModelName} (ID: {modelUUID})");

                int epochs = AppSettings.Instance.Epochs;
                int batch = AppSettings.Instance.BatchSize;
                int filters = AppSettings.Instance.BaseFilters;
                int neurons = AppSettings.Instance.HiddenNeurons;
                bool useEarly = AppSettings.Instance.UseEarlyStopping;
                float tLoss = AppSettings.Instance.TargetLoss;
                float learningRate = 0.001f;

                TotalEpochs = epochs;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    LossPoints = new PointCollection(); AccuracyPoints = new PointCollection();
                    LossFillPoints = new PointCollection(); AccuracyFillPoints = new PointCollection();
                });

                AppendLog("INFO", $"Architecture -> Epochs: {epochs} | Batch: {batch} | Filters: {filters} | Neurons: {neurons}");

                var result = await KaiBridge.TrainAsync(DatasetPath, fullWeightsPath, epochs, learningRate, batch, filters, neurons, useEarly, tLoss);

                if (result.Success)
                {
                    AppendLog("SUCCESS", $"Training Finished! Accuracy: {result.FinalAccuracy:P2} | Loss: {result.FinalLoss:F4}");

                    var historyList = _trainingHistory.Select(h => new EpochData
                    {
                        Epoch = (int)h.Epoch,
                        Loss = (float)h.Loss,
                        Accuracy = (float)h.Accuracy
                    }).ToList();

                    var metadata = new ModelMetadata
                    {
                        Id = modelUUID,
                        Name = ModelName,
                        CreatedAt = DateTime.Now,
                        WeightsFilename = weightsFileName,
                        Framework = "LibTorch (C++) / KAI Engine",
                        Hyperparameters = new ModelHyperparameters
                        {
                            Epochs = epochs,
                            BatchSize = batch,
                            LearningRate = learningRate,
                            BaseFilters = filters,
                            HiddenNeurons = neurons
                        },
                        Metrics = new ModelMetrics
                        {
                            FinalAccuracy = result.FinalAccuracy,
                            FinalLoss = result.FinalLoss
                        },
                        TrainingHistory = historyList
                    };

                    string jsonString = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(jsonPath, jsonString);

                    AppendLog("INFO", $"Metadata saved to: {jsonPath}");
                }
                else
                {
                    AppendLog("FATAL", "Training failed in backend.");
                }
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

        private async void UpdateGpuUsage(object sender, EventArgs e)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "nvidia-smi",
                    Arguments = "--query-gpu=utilization.gpu --format=csv,noheader,nounits",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processInfo))
                {
                    string output = await process.StandardOutput.ReadToEndAsync();
                    if (double.TryParse(output.Trim(), out double usage))
                    {
                        GpuUsage = usage;
                        GpuUsageText = usage > 0 ? $"GPU: {usage}%" : "GPU: Idle";
                    }
                }
            }
            catch
            {
                GpuUsageText = "GPU: N/A";
            }
        }
    }
}