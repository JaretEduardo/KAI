using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using System.Threading.Tasks;
using KAI_UI.Services;
using KAI_UI.ViewModels;

namespace KAI_UI
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _mainRouter;

        public MainWindow()
        {
            InitializeComponent();

            _mainRouter = new MainViewModel();

            DataContext = _mainRouter;

            try
            {
                KaiEngineService.InitEngine();
            }
            catch
            {
            }
        }


        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            _mainRouter.CurrentView = _mainRouter.DashboardVM;
        }

        private void BtnTraining_Click(object sender, RoutedEventArgs e)
        {
            _mainRouter.CurrentView = _mainRouter.TrainingVM;
        }

        private void BtnModels_Click(object sender, RoutedEventArgs e)
        {
            _mainRouter.CurrentView = _mainRouter.ModelsVM;
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            _mainRouter.CurrentView = _mainRouter.ForensicsVM;
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            _mainRouter.CurrentView = _mainRouter.SettingsVM;
        }
    }
}