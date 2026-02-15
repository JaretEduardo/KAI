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
using System.Windows.Navigation;
using System.Windows.Shapes;
using KAI_UI.ViewModels;

namespace KAI_UI.Views
{
    /// <summary>
    /// Lógica de interacción para ModelsView.xaml
    /// </summary>
    public partial class ModelsView : UserControl
    {
        public ModelsView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Verificamos si el DataContext es nuestro ViewModel y llamamos al comando Refresh
            if (DataContext is ModelsViewModel vm)
            {
                vm.RefreshCommand.Execute(null);
            }
        }
    }
}
