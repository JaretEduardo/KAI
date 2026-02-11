using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using KAI_UI.Core;
using Microsoft.Win32;

namespace KAI_UI.ViewModels
{
    public class TrainingViewModel : ViewModelBase
    {
        private string _datasetPath;
        public string DatasetPath
        {
            get { return _datasetPath; }
            set
            {
                _datasetPath = value;
                OnPropertyChanged(nameof(DatasetPath));
            }
        }

        public ICommand BrowseFolderCommand { get; }

        public TrainingViewModel()
        {
            BrowseFolderCommand = new RelayCommand(o => SelectFolder());

            DatasetPath = "No folder selected";
        }

        private void SelectFolder()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select Dataset Directory",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                DatasetPath = dialog.FolderName;
            }
        }

        public void SetDatasetPath(string path)
        {
            DatasetPath = path;
        }
    }
}
