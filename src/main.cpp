#include <torch/torch.h>
#include <iostream>
#include "Models/VisionModel.h"
#include "Core/Trainer.h"

int main() {
    std::cout << "=== KAI ENGINE v2.5: Vision Module ===" << std::endl;

    torch::Device device(torch::kCPU);
    if (torch::cuda::is_available()) {
        std::cout << "[HARDWARE] NVIDIA RTX Detectada. Usando CUDA." << std::endl;
        device = torch::Device(torch::kCUDA);
    }

    std::cout << "[DATA] Cargando imagenes MNIST..." << std::endl;

    auto dataset = torch::data::datasets::MNIST("assets/datasets/mnist")
        .map(torch::data::transforms::Stack<>());

    auto data_loader = torch::data::make_data_loader(
        std::move(dataset),
        torch::data::DataLoaderOptions().batch_size(64).workers(2)
    );

    auto cerebro = std::make_shared<VisionModel>();
    cerebro->to(device);

    std::cout << "[MODELO] VisionModel (LeNet) inicializado." << std::endl;

    torch::optim::SGD optimizador(cerebro->parameters(), torch::optim::SGDOptions(0.01).momentum(0.5));

    std::cout << "[ENTRENAMIENTO] Iniciando lectura de 60,000 imagenes..." << std::endl;

    Trainer::Train(
        cerebro,
        data_loader,
        optimizador,
        device,
        10,
        60000,
        "assets/checkpoints/vision_brain.pt"
    );

    std::cout << "\nPresiona Enter para salir.";
    std::cin.get();
    return 0;
}