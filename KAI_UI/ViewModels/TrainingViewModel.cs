using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using KAI_UI.Core;
using Microsoft.Win32;
using KAI_UI.Services;
using System.Threading.Tasks;

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

        public ICommand BrowseFolderCommand { get; }
        public ICommand StartTrainingCommand { get; }

        public TrainingViewModel()
        {
            BrowseFolderCommand = new RelayCommand(o => SelectFolder());
            StartTrainingCommand = new RelayCommand(o => StartTraining());

            DatasetPath = "No folder selected";
            ModelName = "new_model_v1";

            KaiBridge.Initialize(ReceiveMessageFromCpp);

            AppendLog("INFO", "KAI Engine & Log System initialized...");
        }

        private void ReceiveMessageFromCpp(string message)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (message.StartsWith("["))
                    LogOutput += $"> {message}\n";
                else
                    LogOutput += $"> [ENGINE] {message}\n";
            });
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
            if (string.IsNullOrEmpty(DatasetPath) || DatasetPath == "No folder selected")
            {
                AppendLog("ERROR", "Please select a valid dataset folder first.");
                return;
            }

            try
            {
                string baseDir = @"C:\KAI_Models";
                string outputFolder = Path.Combine(baseDir, ModelName);

                string modelFile = Path.Combine(outputFolder, $"{ModelName}.pt");

                if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

                AppendLog("SUCCESS", $"STARTING TRAINING SESSION: {ModelName}");
                AppendLog("INFO", $"Target File: {modelFile}");
                AppendLog("WAIT", "Handing over control to Neural Engine (C++)...");

                await KaiBridge.TrainAsync(DatasetPath, modelFile, 50, 0.001f);

                AppendLog("SUCCESS", "Training process finished.");
                AppendLog("INFO", $"Model saved at: {modelFile}");
            }
            catch (DllNotFoundException)
            {
                AppendLog("FATAL", "Could not find 'KAI_Engine.dll'. Make sure it is in the bin folder.");
            }
            catch (Exception ex)
            {
                AppendLog("FATAL", $"Engine Error: {ex.Message}");
            }
        }

        private void AppendLog(string type, string message)
        {
            string newLog = $"> [{type}] {message}\n";
            LogOutput += newLog;
        }
    }
}