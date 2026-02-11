#include <torch/torch.h>
#include <iostream>
#include <string>
#include <cstdio>
#include <windows.h>
#include <exception>
#include <vector>

#include "Core/AutoDetector.h"
#include "Data/ImageDataset.h"
#include "Models/SimpleCNN.h"

#ifdef KAI_EXPORTS
#define KAI_API __declspec(dllexport)
#else
#define KAI_API __declspec(dllimport)
#endif

typedef void(__stdcall* LogCallbackType)(const char*);

LogCallbackType g_logCallback = nullptr;

void LogToUI(const std::string& message) {
    if (g_logCallback) {
        g_logCallback(message.c_str());
    }
}

extern "C" {

    KAI_API void SetLogCallback(LogCallbackType callback) {
        g_logCallback = callback;
    }

    KAI_API int InitEngine() {

        if (torch::cuda::is_available()) {
            LogToUI("[KAI DLL] GPU NVIDIA Detectada.");
            return 1;
        }
        LogToUI("[KAI DLL] CPU Mode.");
        return 0;
    }

    KAI_API void TrainAutoML(const char* datasetPath, const char* outputPath, int epochs, float learning_rate) {
        try {
            std::string path(datasetPath);
            std::string outPath(outputPath);

            torch::Device device(torch::kCPU);
            if (torch::cuda::is_available()) {
                device = torch::Device(torch::kCUDA);
            }

            LogToUI("\n[KAI DLL] --- INICIANDO ENTRENAMIENTO ---");
            LogToUI("[KAI DLL] Target: " + outPath);

            auto dataset = ImageDataset(path, 64);
            auto dataset_size = dataset.size().value();

            if (dataset_size == 0) {
                LogToUI("[ERROR] No hay datos.");
                return;
            }

            int num_classes = dataset.getClassMap().size();
            LogToUI("[KAI DLL] Clases: " + std::to_string(num_classes) + " | Imagenes: " + std::to_string(dataset_size));

            SimpleCNN model(num_classes);
            model->to(device);
            model->train();

            torch::optim::Adam optimizer(model->parameters(), torch::optim::AdamOptions(learning_rate));

            int batch_size = 4;

            for (int epoch = 1; epoch <= epochs; ++epoch) {
                float epoch_loss = 0.0;
                int batches_processed = 0;

                std::vector<torch::Tensor> batch_images;
                std::vector<torch::Tensor> batch_targets;

                for (size_t i = 0; i < dataset_size; ++i) {
                    try {
                        auto sample = dataset.get(i);
                        batch_images.push_back(sample.data);
                        batch_targets.push_back(sample.target);

                        if (batch_images.size() == batch_size || i == dataset_size - 1) {
                            auto imgs = torch::stack(batch_images).to(device);
                            auto lbls = torch::stack(batch_targets).to(device);

                            optimizer.zero_grad();
                            auto output = model->forward(imgs);
                            auto loss = torch::nn::functional::cross_entropy(output, lbls);
                            loss.backward();
                            optimizer.step();

                            epoch_loss += loss.item<float>();
                            batches_processed++;

                            batch_images.clear();
                            batch_targets.clear();
                        }
                    }
                    catch (...) {
                    }
                }

                std::string msg = "Epoca [" + std::to_string(epoch) + "/" + std::to_string(epochs) +
                    "] Loss: " + std::to_string(epoch_loss / batches_processed);
                LogToUI(msg);
            }

            LogToUI("[KAI DLL] Guardando modelo...");
            torch::save(model, outPath);
            LogToUI("[KAI DLL] Modelo guardado EXITOSAMENTE.");
            LogToUI("TRAINING_COMPLETE");

        }
        catch (const std::exception& e) {
            std::string err = "[ERROR CRITICO]: ";
            err += e.what();
            LogToUI(err);
        }
    }

    KAI_API int PredictImage(const char* imagePath) {
        return 7;
    }
}