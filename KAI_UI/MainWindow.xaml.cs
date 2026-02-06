using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using KAI_UI.Services;
using System.Windows;

namespace KAI_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            try
            {
                // Llamamos a C++
                int estado = KaiEngineService.InitEngine();

                if (estado == 1)
                    MessageBox.Show("Conexión Exitosa: KAI Engine detectó NVIDIA CUDA 🚀", "Sistema KAI");
                else
                    MessageBox.Show("Conexión Exitosa: KAI Engine corriendo en CPU 🐢", "Sistema KAI");
            }
            catch (System.DllNotFoundException)
            {
                MessageBox.Show("ERROR CRITICO: No se encuentra 'KAI_Engine.dll'. \n¿Compilaste el proyecto de C++?", "Error de Enlace");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error desconocido: {ex.Message}", "Fallo de Sistema");
            }
        }
    }
}