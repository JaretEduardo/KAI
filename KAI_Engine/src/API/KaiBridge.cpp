#include <torch/torch.h>
#include <iostream>
#include <string>

#ifdef KAI_EXPORTS
#define KAI_API __declspec(dllexport)
#else
#define KAI_API __declspec(dllimport)
#endif

std::shared_ptr<torch::jit::script::Module> current_model;
bool is_model_loaded = false;

extern "C" {

    KAI_API int InitEngine() {
        if (torch::cuda::is_available()) {
            std::cout << "[KAI DLL] Motor iniciado con CUDA." << std::endl;
            return 1;
        }
        std::cout << "[KAI DLL] Motor iniciado en CPU." << std::endl;
        return 0;
    }

    KAI_API void TrainAutoML(const char* datasetPath, int epochs, float learning_rate) {
        std::string path(datasetPath);
        std::cout << "[KAI DLL] Iniciando entrenamiento en: " << path << std::endl;

        std::cout << "Entrenando " << epochs << " epocas..." << std::endl;
    }

    KAI_API int PredictImage(const char* imagePath) {
        std::cout << "[KAI DLL] Analizando imagen: " << imagePath << std::endl;
        return 7;
    }
}