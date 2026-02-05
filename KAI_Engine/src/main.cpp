#include <torch/torch.h>
#include <iostream>
#include <thread>
#include <chrono>
#include "Models/VisionModel.h"
#include "Core/Trainer.h"

void DibujarNumero(torch::Tensor& imagen) {
    std::cout << "\n--- VISION DE KAI ---\n";

    torch::Tensor img_procesada = imagen.squeeze();

    auto acces = img_procesada.accessor<float, 2>();

    for (int i = 0; i < 28; i++) {
        for (int j = 0; j < 28; j++) {
            if (acces[i][j] > 0.5) std::cout << "@@";
            else std::cout << "  ";
        }
        std::cout << "\n";
    }
    std::cout << "---------------------\n";
}

int main() {
    std::cout << "=== KAI ENGINE v2.6: Forensic Mode ===\n";

    torch::Device device(torch::kCPU);
    if (torch::cuda::is_available()) {
        std::cout << "[HARDWARE] NVIDIA RTX Detectada.\n";
        device = torch::Device(torch::kCUDA);
    }

    auto cerebro = std::make_shared<VisionModel>();

    int opcion = 0;
    std::cout << "\nSelecciona modo:\n1. ENTRENAR (Crear nuevo cerebro)\n2. PRUEBA FORENSE (Usar cerebro existente)\n> ";
    std::cin >> opcion;

    if (opcion == 1) {
        cerebro->to(device);
        auto dataset = torch::data::datasets::MNIST("assets/datasets/mnist")
            .map(torch::data::transforms::Stack<>());
        auto data_loader = torch::data::make_data_loader(std::move(dataset), 64);

        torch::optim::Adam optimizador(cerebro->parameters(), torch::optim::AdamOptions(0.001));

        Trainer::Train(cerebro, data_loader, optimizador, device, 10, 60000, "assets/checkpoints/vision_brain.pt");
    }
    else {
        std::cout << "[SISTEMA] Cargando cerebro entrenado...\n";
        try {
            torch::load(cerebro, "assets/checkpoints/vision_brain.pt");
            cerebro->to(device);
            cerebro->eval();
        }
        catch (...) {
            std::cout << "[ERROR] No se encontro el archivo .pt. Entrena primero (Opcion 1)!\n";
            return -1;
        }

        std::cout << "[DATA] Cargando casos de prueba (Test Set)...\n";

        auto test_dataset = torch::data::datasets::MNIST("assets/datasets/mnist", torch::data::datasets::MNIST::Mode::kTest);

        std::cout << "[PRUEBA] Analizando evidencia aleatoria...\n";

        while (true) {
            int indice = rand() % 10000;

            auto sample = test_dataset.get(indice);

            auto data = sample.data.to(device);
            auto target = sample.target.item<int>();

            auto output = cerebro->forward(data.unsqueeze(0));
            auto prediccion = output.argmax(1).item<int>();

            auto data_cpu = data.to(torch::kCPU);
            DibujarNumero(data_cpu);

            std::cout << "Realidad: " << target << "\n";
            std::cout << "KAI Dice: " << prediccion << "\n";

            if (target == prediccion) std::cout << "[RESULTADO] CORRECTO (Identificacion Positiva) \n";
            else std::cout << "[RESULTADO] FALLO \n";

            std::cout << "\nPresiona Enter para siguiente caso...";
            std::cin.ignore();
            std::cin.get();
        }
    }

    return 0;
}