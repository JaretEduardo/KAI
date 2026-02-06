using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace KAI_UI.Services
{
    class KaiEngineService
    {
        private const string DllName = "KAI_Engine.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int InitEngine();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TrainAutoML(string datasetPath, int epochs, float lr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int PredictImage(string imagePath);
    }
}
