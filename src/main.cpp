#include <torch/torch.h>
#include <iostream>
#include "Models/XORModel.h"
#include "Core/Trainer.h"

int main() {
    std::cout << "=== KAI ENGINE v2.0 ===" << std::endl;

    torch::Device device(torch::kCPU);
    if (torch::cuda::is_available()) {
        std::cout << "[HARDWARE] NVIDIA RTX Detectada. Usando CUDA." << std::endl;
        device = torch::Device(torch::kCUDA);
    }

    auto cerebro = std::make_shared<XORModel>();
    cerebro->to(device);

    auto inputs = torch::tensor({ {0.0f, 0.0f}, {0.0f, 1.0f}, {1.0f, 0.0f}, {1.0f, 1.0f} }).to(device);
    auto targets = torch::tensor({ {0.0f}, {1.0f}, {1.0f}, {0.0f} }).to(device);

    torch::optim::Adam optimizador(cerebro->parameters(), torch::optim::AdamOptions(0.01));

    Trainer::Train(cerebro, inputs, targets, optimizador, 2000, "assets/checkpoints/xor_brain.pt");

    std::cout << "\nPresiona Enter para salir.";
    std::cin.get();
    return 0;
}