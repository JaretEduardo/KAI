using KAI_UI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KAI_UI.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public AppSettings Config => AppSettings.Instance;

        public ICommand RestoreDefaultsCommand { get; }

        public SettingsViewModel()
        {
            RestoreDefaultsCommand = new RelayCommand(o => RestoreDefaults());
        }

        private void RestoreDefaults()
        {
            Config.RestoreDefaults();
        }
    }
}
