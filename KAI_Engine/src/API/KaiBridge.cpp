#include <torch/torch.h>
#include <iostream>
#include <string>
#include "Core/AutoDetector.h"

#ifdef KAI_EXPORTS
#define KAI_API __declspec(dllexport)
#else
#define KAI_API __declspec(dllimport)
#endif

extern "C" {

    KAI_API int InitEngine() {
        if (torch::cuda::is_available()) {
            return 1;
        }
        return 0;
    }

    KAI_API void TrainAutoML(const char* datasetPath, int epochs, float learning_rate) {
        std::string path(datasetPath);

        TaskType tipo = AutoDetector::DetectTaskType(path);

        if (tipo == TaskType::VISION) {
        }
    }

    KAI_API int PredictImage(const char* imagePath) {
        return 7;
    }
}