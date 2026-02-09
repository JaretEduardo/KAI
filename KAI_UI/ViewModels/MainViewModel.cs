using KAI_UI.ViewModels;

namespace KAI_UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
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
        }
    }
}