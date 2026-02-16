using System;
using KAI_UI.ViewModels;
using static KAI_UI.ViewModels.TrainingViewModel;

namespace KAI_UI.Core
{
    public static class GlobalEvents
    {
        public static event Action<ModelMetadata> OnRetrainRequested;

        public static void RequestRetrain(ModelMetadata model)
        {
            OnRetrainRequested?.Invoke(model);
        }
    }
}
