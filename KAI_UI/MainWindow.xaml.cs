using System.Windows;
using Microsoft.Win32;
using KAI_UI.Services;
using System.Threading.Tasks;

namespace KAI_UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CheckHardware();
        }

        private void CheckHardware()
        {
            try
            {
                int estado = KaiEngineService.InitEngine();
                if (estado == 1)
                {
                    TxtHardwareStatus.Text = "NVIDIA CUDA 🟢";
                    TxtHardwareStatus.Foreground = System.Windows.Media.Brushes.LightGreen;
                }
                else
                {
                    TxtHardwareStatus.Text = "CPU MODE 🟡";
                    TxtHardwareStatus.Foreground = System.Windows.Media.Brushes.Yellow;
                }
            }
            catch
            {
                TxtHardwareStatus.Text = "ERROR 🔴";
                TxtHardwareStatus.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private async void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FolderName;

                // Feedback visual inmediato
                TxtAnalysisResult.Text = "Analizando estructura...";

                // Ejecutamos la carga en segundo plano para no congelar la UI
                await Task.Run(() =>
                {
                    // Llamamos a C++ (Esto disparara el ImageDataset y stb_image)
                    // Nota: 0 epocas para solo probar carga
                    KaiEngineService.TrainAutoML(path, 50, 0.001f);
                });

                // Simulación de respuesta (Ya que aun no tenemos return string desde C++)
                // En el futuro, C++ nos devolverá "VISION" o "NLP"
                TxtAnalysisResult.Text = $"Carpeta cargada: {System.IO.Path.GetFileName(path)}\n(Verifica la consola de Visual Studio para el conteo)";
            }
        }
    }
}