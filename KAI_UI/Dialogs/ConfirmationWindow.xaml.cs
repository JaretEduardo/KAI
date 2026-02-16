using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Input;

namespace KAI_UI.Dialogs
{
    /// <summary>
    /// Lógica de interacción para ConfirmationWindow.xaml
    /// </summary>
    public enum ConfirmationType
    {
        Danger,
        Info
    }
    public partial class ConfirmationWindow : Window
    {
        public ConfirmationWindow(string message, ConfirmationType type = ConfirmationType.Danger, string title = "SYSTEM WARNING", string confirmText = "CONFIRM")
        {
            InitializeComponent();

            MessageText.Text = message;
            TitleText.Text = title;
            ConfirmBtn.Content = confirmText;

            if (type == ConfirmationType.Info)
            {
                ApplyInfoStyle();
            }
        }

        private void ApplyInfoStyle()
        {
            var neonBlue = (Color)ColorConverter.ConvertFromString("#00D4FF");
            var brushBlue = new SolidColorBrush(neonBlue);

            MainBorder.BorderBrush = brushBlue;
            MainGlow.Color = neonBlue;

            TitleText.Foreground = brushBlue;
            TextGlow.Color = neonBlue;

            IconPath.Fill = brushBlue;
            IconGlow.Color = neonBlue;

            ConfirmBtn.Style = (Style)FindResource("ConfirmButtonStyle");
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
