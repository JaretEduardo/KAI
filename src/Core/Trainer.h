#pragma once
#include <torch/torch.h>
#include <iostream>
#include <string>
#include <filesystem>

class Trainer {
public:
    template <typename ModelType, typename BatchType>
    static void Train(
        ModelType& model,
        BatchType& inputs,
        torch::Tensor& targets,
        torch::optim::Optimizer& optim,
        int epochs,
        std::string save_path = ""
    ) {
        model->train();

        for (int epoch = 1; epoch <= epochs; ++epoch) {
            optim.zero_grad();
            auto prediction = model->forward(inputs);
            auto loss = torch::mse_loss(prediction, targets);
            loss.backward();
            optim.step();

            if (epoch % (epochs / 10) == 0) {
                std::cout << "Epoca: " << epoch
                    << " | Error: " << loss.item<float>() << std::endl;
            }
        }

        if (!save_path.empty()) {
            std::filesystem::path ruta(save_path);
            if (ruta.has_parent_path()) {
                std::filesystem::create_directories(ruta.parent_path());
            }

            std::cout << "[SISTEMA] Guardando cerebro en: " << save_path << " ... ";
            try {
                torch::save(model, save_path);
                std::cout << "HECHO." << std::endl;
            }
            catch (const std::exception& e) {
                std::cout << "\n[ERROR AL GUARDAR] " << e.what() << std::endl;
            }
        }
    }
};