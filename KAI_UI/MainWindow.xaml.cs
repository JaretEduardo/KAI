using System.Windows;
using Microsoft.Win32;
using System.Threading.Tasks;
using KAI_UI.Services;

namespace KAI_UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            try
            {
                KaiEngineService.InitEngine();
            }
            catch
            {
            }
        }
    }
}