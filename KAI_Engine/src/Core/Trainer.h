#pragma once
#include <torch/torch.h>
#include <iostream>
#include <string>
#include <filesystem>

class Trainer {
public:
    template <typename ModelType>
    static void Train(
        ModelType& model,
        torch::Tensor& inputs,
        torch::Tensor& targets,
        torch::optim::Optimizer& optim,
        int epochs,
        std::string save_path = ""
    ) {
        model->train();
        for (int epoch = 1; epoch <= epochs; ++epoch) {
            optim.zero_grad();
            auto prediction = model->forward(inputs);
            auto loss = torch::nn::functional::mse_loss(prediction, targets);
            loss.backward();
            optim.step();

            if (epoch % (epochs / 10) == 0) {
                std::cout << "Epoca: " << epoch << " | Error: " << loss.item<float>() << std::endl;
            }
        }
        SaveModel(model, save_path);
    }

    template <typename ModelType, typename DataLoaderType>
    static void Train(
        ModelType& model,
        DataLoaderType& data_loader,
        torch::optim::Optimizer& optim,
        torch::Device device,
        int epochs,
        size_t dataset_size,
        std::string save_path = ""
    ) {
        model->train();

        for (int epoch = 1; epoch <= epochs; ++epoch) {
            double epoch_loss = 0.0;
            size_t batch_idx = 0;
            size_t total_batches = 0;

            for (auto& batch : *data_loader) {
                auto data = batch.data.to(device);
                auto targets = batch.target.to(device);

                optim.zero_grad();
                auto output = model->forward(data);

                auto loss = torch::nn::functional::nll_loss(output, targets);

                loss.backward();
                optim.step();

                epoch_loss += loss.item<double>();
                total_batches++;

                if (batch_idx++ % 100 == 0) {
                    std::cout << "\rEpoca [" << epoch << "/" << epochs << "] "
                        << "Procesando batch..." << std::flush;
                }
            }

            std::cout << "\rEpoca [" << epoch << "/" << epochs << "] "
                << "Error Promedio: " << (epoch_loss / total_batches) << std::endl;
        }
        SaveModel(model, save_path);
    }

private:
    template <typename ModelType>
    static void SaveModel(ModelType& model, std::string path) {
        if (!path.empty()) {
            std::filesystem::path ruta(path);
            if (ruta.has_parent_path()) {
                std::filesystem::create_directories(ruta.parent_path());
            }
            std::cout << "[SISTEMA] Guardando cerebro en: " << path << " ... ";
            try {
                torch::save(model, path);
                std::cout << "HECHO." << std::endl;
            }
            catch (const std::exception& e) {
                std::cout << "\n[ERROR] " << e.what() << std::endl;
            }
        }
    }
};