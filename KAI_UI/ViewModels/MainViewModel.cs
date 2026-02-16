using KAI_UI.Core;
using KAI_UI.ViewModels;

namespace KAI_UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private bool _isDashboardActive;
        public bool IsDashboardActive { get => _isDashboardActive; set { _isDashboardActive = value; OnPropertyChanged(); } }

        private bool _isTrainingActive;
        public bool IsTrainingActive { get => _isTrainingActive; set { _isTrainingActive = value; OnPropertyChanged(); } }

        private bool _isModelsActive;
        public bool IsModelsActive { get => _isModelsActive; set { _isModelsActive = value; OnPropertyChanged(); } }

        private bool _isForensicsActive;
        public bool IsForensicsActive { get => _isForensicsActive; set { _isForensicsActive = value; OnPropertyChanged(); } }

        private bool _isSettingsActive;
        public bool IsSettingsActive { get => _isSettingsActive; set { _isSettingsActive = value; OnPropertyChanged(); } }

        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        public DashboardViewModel DashboardVM { get; set; }
        public TrainingViewModel TrainingVM { get; set; }
        public ModelsViewModel ModelsVM { get; set; }
        public ForensicsViewModel ForensicsVM { get; set; }
        public SettingsViewModel SettingsVM { get; set; }

        public MainViewModel()
        {
            DashboardVM = new DashboardViewModel();
            TrainingVM = new TrainingViewModel();
            ModelsVM = new ModelsViewModel();
            ForensicsVM = new ForensicsViewModel();
            SettingsVM = new SettingsViewModel();

            CurrentView = DashboardVM;
            IsDashboardActive = true;

            GlobalEvents.OnRetrainRequested += (model) =>
            {
                CurrentView = TrainingVM;

                IsModelsActive = false;
                IsTrainingActive = true;

                IsDashboardActive = false;
                IsForensicsActive = false;
                IsSettingsActive = false;
            };
        }
    }
}