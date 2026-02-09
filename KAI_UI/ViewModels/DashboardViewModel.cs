using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KAI_UI.Models;

namespace KAI_UI.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        public SystemMetrics Metrics { get; set; }

        public DashboardViewModel()
        {
            Metrics = new SystemMetrics
            {
                Status = "ONLINE",
                CpuLoad = 45
            };
        }
    }
}
