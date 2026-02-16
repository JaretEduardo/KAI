using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using KAI_UI.Core;
using static KAI_UI.ViewModels.TrainingViewModel;
using KAI_UI.Dialogs;

namespace KAI_UI.ViewModels
{
    public class ModelsViewModel : ViewModelBase
    {
        private ObservableCollection<ModelMetadata> _models;
        public ObservableCollection<ModelMetadata> Models { get => _models; set { _models = value; OnPropertyChanged(nameof(Models)); } }

        private ModelMetadata _selectedModel;
        public ModelMetadata SelectedModel
        {
            get => _selectedModel;
            set
            {
                _selectedModel = value;
                OnPropertyChanged(nameof(SelectedModel));
                UpdateDashboard();
            }
        }

        private PointCollection _accPoints;
        public PointCollection AccPoints { get => _accPoints; set { _accPoints = value; OnPropertyChanged(nameof(AccPoints)); } }

        private PointCollection _accFillPoints;
        public PointCollection AccFillPoints { get => _accFillPoints; set { _accFillPoints = value; OnPropertyChanged(nameof(AccFillPoints)); } }

        private PointCollection _lossPoints;
        public PointCollection LossPoints { get => _lossPoints; set { _lossPoints = value; OnPropertyChanged(nameof(LossPoints)); } }

        private PointCollection _lossFillPoints;
        public PointCollection LossFillPoints { get => _lossFillPoints; set { _lossFillPoints = value; OnPropertyChanged(nameof(LossFillPoints)); } }

        private ObservableCollection<string> _historyLogs;
        public ObservableCollection<string> HistoryLogs { get => _historyLogs; set { _historyLogs = value; OnPropertyChanged(nameof(HistoryLogs)); } }

        public ICommand RefreshCommand { get; }
        public ICommand DeleteModelCommand { get; }
        public ICommand RetrainCommand { get; }

        public ModelsViewModel()
        {
            Models = new ObservableCollection<ModelMetadata>();
            HistoryLogs = new ObservableCollection<string>();
            RefreshCommand = new RelayCommand(o => LoadModels());
            DeleteModelCommand = new RelayCommand(o => DeleteSelectedModel());
            RetrainCommand = new RelayCommand(o => MessageBox.Show("Retrain logic here"));

            LoadModels();
        }

        private void LoadModels()
        {
            Models.Clear();
            string modelsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "KAI_Models");

            if (!Directory.Exists(modelsDir)) return;

            var tempModels = new List<ModelMetadata>();
            var directories = Directory.GetDirectories(modelsDir);

            foreach (var dir in directories)
            {
                string jsonPath = Path.Combine(dir, "metadata.json");
                if (File.Exists(jsonPath))
                {
                    try
                    {
                        string jsonContent = File.ReadAllText(jsonPath);
                        var metadata = JsonSerializer.Deserialize<ModelMetadata>(jsonContent);
                        if (metadata != null)
                        {
                            tempModels.Add(metadata);
                        }
                    }
                    catch { }
                }
            }

            foreach (var model in tempModels.OrderByDescending(m => m.CreatedAt))
            {
                Models.Add(model);
            }

            if (Models.Count > 0) SelectedModel = Models[0];
        }

        private void UpdateDashboard()
        {
            if (SelectedModel == null || SelectedModel.TrainingHistory == null) return;

            GenerateCharts();

            HistoryLogs.Clear();
            int total = SelectedModel.Hyperparameters.Epochs;
            foreach (var epoch in SelectedModel.TrainingHistory.OrderBy(x => x.Epoch))
            {
                string log = $"Log: {SelectedModel.Name} (Epoch {epoch.Epoch}/{total}) - Acc: {epoch.Accuracy:P1}";
                HistoryLogs.Add(log);
            }
        }

        private void GenerateCharts()
        {
            var history = SelectedModel.TrainingHistory;
            if (history.Count == 0) return;

            double width = 100;
            double height = 100;

            double minEpoch = history.Min(x => x.Epoch);
            double maxEpoch = history.Max(x => x.Epoch);
            double epochRange = maxEpoch - minEpoch;
            if (epochRange == 0) epochRange = 1;

            var lineAcc = new PointCollection();
            foreach (var h in history)
            {
                double x = ((h.Epoch - minEpoch) / epochRange) * width;
                double y = height - (h.Accuracy * height);
                lineAcc.Add(new Point(x, y));
            }
            AccPoints = lineAcc;

            var fillAcc = new PointCollection(lineAcc);
            fillAcc.Insert(0, new Point(0, height));
            fillAcc.Add(new Point(width, height));
            AccFillPoints = fillAcc;

            var lineLoss = new PointCollection();
            foreach (var h in history)
            {
                double x = ((h.Epoch - minEpoch) / epochRange) * width;
                double normalizedLoss = Math.Min(h.Loss, 1.5) / 1.5;
                double y = height - (normalizedLoss * height);
                lineLoss.Add(new Point(x, y));
            }
            LossPoints = lineLoss;

            var fillLoss = new PointCollection(lineLoss);
            fillLoss.Insert(0, new Point(0, height));
            fillLoss.Add(new Point(width, height));
            LossFillPoints = fillLoss;
        }

        private void DeleteSelectedModel()
        {
            if (SelectedModel == null) return;

            string msg = $"Permanently delete protocol for neural model:\n\n'{SelectedModel.Name}'\n\nThis action is irreversible. Proceed?";

            var dialog = new ConfirmationWindow(msg);

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                try
                {
                    string modelsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "KAI_Models");

                    string modelFolder = Path.Combine(modelsDir, SelectedModel.Name);

                    if (Directory.Exists(modelFolder))
                    {
                        Directory.Delete(modelFolder, true);
                    }

                    Models.Remove(SelectedModel);

                    if (Models.Count > 0)
                        SelectedModel = Models[0];
                    else
                    {
                        SelectedModel = null;
                        AccPoints = null; AccFillPoints = null;
                        LossPoints = null; LossFillPoints = null;
                        HistoryLogs.Clear();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting model: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}