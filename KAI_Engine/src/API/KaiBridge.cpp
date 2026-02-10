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

extern "C" {

    KAI_API int InitEngine() {
        AllocConsole();
        FILE* fDummy;
        freopen_s(&fDummy, "CONOUT$", "w", stdout);
        freopen_s(&fDummy, "CONOUT$", "w", stderr);
        std::cout.clear();

        if (torch::cuda::is_available()) {
            std::cout << "[KAI DLL] GPU NVIDIA Detectada." << std::endl;
            return 1;
        }
        std::cout << "[KAI DLL] CPU Mode." << std::endl;
        return 0;
    }

    KAI_API void TrainAutoML(const char* datasetPath, int epochs, float learning_rate) {
        try {
            std::string path(datasetPath);

            // 1. Configurar Dispositivo (GPU si es posible)
            torch::Device device(torch::kCPU);
            if (torch::cuda::is_available()) {
                device = torch::Device(torch::kCUDA);
            }

            std::cout << "\n[KAI DLL] --- INICIANDO ENTRENAMIENTO (MODO MANUAL) ---" << std::endl;

            // 2. Cargar Dataset
            auto dataset = ImageDataset(path, 64);
            auto dataset_size = dataset.size().value();

            if (dataset_size == 0) {
                std::cout << "[ERROR] No hay datos." << std::endl;
                return;
            }

            int num_classes = dataset.getClassMap().size();
            std::cout << "[KAI DLL] Clases: " << num_classes << " | Imagenes: " << dataset_size << std::endl;

            // 3. Crear el Modelo (Cerebro)
            SimpleCNN model(num_classes);
            model->to(device);
            model->train();

            // 4. Optimizador
            torch::optim::Adam optimizer(model->parameters(), torch::optim::AdamOptions(learning_rate));

            std::cout << "[KAI DLL] Comenzando " << epochs << " epocas..." << std::endl;

            // --- BUCLE DE ENTRENAMIENTO MANUAL (Sin DataLoader) ---
            // Esto es mas seguro en DLLs porque controlamos la memoria nosotros mismos
            int batch_size = 2; // Pequeño para tu prueba

            for (int epoch = 1; epoch <= epochs; ++epoch) {
                float epoch_loss = 0.0;
                int batches_processed = 0;

                // Vectores temporales para armar el lote (batch)
                std::vector<torch::Tensor> batch_images;
                std::vector<torch::Tensor> batch_targets;

                for (size_t i = 0; i < dataset_size; ++i) {
                    // A. Obtener dato individual (Seguro)
                    auto sample = dataset.get(i);
                    batch_images.push_back(sample.data);
                    batch_targets.push_back(sample.target);

                    // B. Si llenamos el batch o es el ultimo dato, ENTRENAMOS
                    if (batch_images.size() == batch_size || i == dataset_size - 1) {

                        // 1. Convertir lista de tensores a un solo Tensor Grande (Stack)
                        auto imgs = torch::stack(batch_images).to(device);
                        auto lbls = torch::stack(batch_targets).to(device);

                        // 2. Paso de Entrenamiento
                        optimizer.zero_grad();
                        auto output = model->forward(imgs);
                        auto loss = torch::nn::functional::cross_entropy(output, lbls);
                        loss.backward();
                        optimizer.step();

                        // 3. Acumular error y limpiar para el siguiente batch
                        epoch_loss += loss.item<float>();
                        batches_processed++;

                        batch_images.clear();
                        batch_targets.clear();
                    }
                }

                std::cout << "Epoca [" << epoch << "/" << epochs << "] Loss: "
                    << (epoch_loss / batches_processed) << std::endl;
            }

            std::cout << "[KAI DLL] Entrenamiento Finalizado Exitosamente." << std::endl;
        }
        catch (const std::exception& e) {
            std::cerr << "\n[ERROR]: " << e.what() << std::endl;
        }
        std::cout << "------------------------------------------------\n" << std::endl;
    }

    KAI_API int PredictImage(const char* imagePath) {
        return 7;
    }
}