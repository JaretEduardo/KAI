using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace KAI_UI.Services
{
    public static class KaiBridge
    {
        private const string DllName = "KAI_Engine.dll";

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void LogCallback(string message);

        private static LogCallback _callbackInstance;

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetLogCallback(LogCallback callback);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int InitEngine();

        [DllImport("KAI_Engine.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void TrainAutoML(string datasetPath, string outputPath, int epochs, float learningRate, int batchSize, int baseFilters, int hiddenNeurons, [MarshalAs(UnmanagedType.I1)] bool useEarlyStopping, float targetLoss);

        public static void Initialize(LogCallback callbackAction)
        {
            _callbackInstance = callbackAction;

            try
            {
                SetLogCallback(_callbackInstance);
                InitEngine();
            }
            catch (DllNotFoundException)
            {
                Console.WriteLine("Warning: KAI_Engine.dll not found during init.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing bridge: {ex.Message}");
            }
        }

        public static Task TrainAsync(string datasetPath, string outputPath, int epochs, float learningRate, int batchSize, int baseFilters, int hiddenNeurons, bool useEarlyStopping, float targetLoss)
        {
            return Task.Run(() =>
            {
                TrainAutoML(datasetPath, outputPath, epochs, learningRate, batchSize, baseFilters, hiddenNeurons, useEarlyStopping, targetLoss);
            });
        }
    }
}