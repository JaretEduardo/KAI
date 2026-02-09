using System.Windows;
using System.Windows.Input; // Necesario si usas eventos de mouse
using Microsoft.Win32;      // Necesario para abrir carpetas
using System.Threading.Tasks;
using KAI_UI.Services;      // Conexión con C++

namespace KAI_UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Iniciamos el motor al abrir la ventana
            try
            {
                KaiEngineService.InitEngine();
            }
            catch
            {
                // Si falla, el UI sigue funcionando
            }
        }

        // --- EVENTOS DEL MENÚ LATERAL ---
        // Estos son los "cables" que faltaban para conectar tus botones

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            // Aquí mostraremos el Dashboard
        }

        private void BtnTraining_Click(object sender, RoutedEventArgs e)
        {
            // Aquí cambiaremos a la vista de Entrenamiento
        }

        private void BtnModels_Click(object sender, RoutedEventArgs e)
        {
            // Aquí mostraremos la lista de modelos
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            // Aquí abriremos los ajustes
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            // Lógica del botón nuevo
        }
    }
}