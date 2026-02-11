using KAI_UI.ViewModels;
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

namespace KAI_UI.Views
{
    /// <summary>
    /// Lógica de interacción para TrainingView_xaml.xaml
    /// </summary>
    public partial class TrainingView : UserControl
    {
        public TrainingView()
        {
            InitializeComponent();
        }

        private void Border_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files != null && files.Length > 0)
                {
                    string path = files[0];

                    if (DataContext is TrainingViewModel vm)
                    {
                        vm.SetDatasetPath(path);
                    }
                }
            }
        }
    }
}
