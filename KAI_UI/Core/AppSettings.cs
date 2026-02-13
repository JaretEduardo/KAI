using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KAI_UI.Core
{
    public class AppSettings : INotifyPropertyChanged
    {
        private static AppSettings _instance;
        public static AppSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Load();
                }
                return _instance;
            }
        }

        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "kai_config.json");

        [JsonIgnore]
        public bool IsSavingEnabled { get; set; } = true;

        private int _batchSize = 64;
        public int BatchSize
        {
            get => _batchSize;
            set { _batchSize = value; OnPropertyChanged(nameof(BatchSize)); Save(); }
        }

        private int _baseFilters = 32;
        public int BaseFilters
        {
            get => _baseFilters;
            set { _baseFilters = value; OnPropertyChanged(nameof(BaseFilters)); Save(); }
        }

        private int _hiddenNeurons = 512;
        public int HiddenNeurons
        {
            get => _hiddenNeurons;
            set { _hiddenNeurons = value; OnPropertyChanged(nameof(HiddenNeurons)); Save(); }
        }

        public void Save()
        {
            if (!IsSavingEnabled) return;

            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(ConfigPath, json);
            }
            catch { }
        }

        private static AppSettings Load()
        {
            if (File.Exists(ConfigPath))
            {
                try
                {
                    string json = File.ReadAllText(ConfigPath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null) return settings;
                }
                catch { }
            }

            return new AppSettings();
        }

        public void RestoreDefaults()
        {
            IsSavingEnabled = false;

            Epochs = 50;
            BatchSize = 64;
            BaseFilters = 32;
            HiddenNeurons = 512;

            IsSavingEnabled = true;
            Save();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private int _epochs = 50;
        public int Epochs
        {
            get => _epochs;
            set { _epochs = value; OnPropertyChanged(nameof(Epochs)); Save(); }
        }
    }
}