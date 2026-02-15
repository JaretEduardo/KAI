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

#include <algorithm>
#include <random>
#include <numeric>

#ifdef KAI_EXPORTS
#define KAI_API __declspec(dllexport)
#else
#define KAI_API __declspec(dllimport)
#endif

struct TrainingResult {
    float final_loss;
    float final_accuracy;
    bool success;
};

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
            LogToUI("[KAI DLL] NVIDIA GPU Detected.");
            return 1;
        }
        LogToUI("[KAI DLL] CPU Mode.");
        return 0;
    }

    KAI_API TrainingResult TrainAutoML(const char* datasetPath, const char* outputPath, int epochs, float learning_rate, int batch_size, int base_filters, int hidden_neurons, bool use_early_stopping, float target_loss) {

        TrainingResult result = { 0.0f, 0.0f, false };

        try {
            std::string path(datasetPath);
            std::string outPath(outputPath);

            torch::Device device(torch::kCPU);
            if (torch::cuda::is_available()) {
                device = torch::Device(torch::kCUDA);
            }

            LogToUI("\n[KAI DLL] --- STARTING TRAINING (VRAM CACHED) ---");
            LogToUI("[KAI DLL] Target: " + outPath);

            LogToUI("[KAI DLL] Reading images from the disc...");
            auto dataset = ImageDataset(path, 64);
            auto dataset_size = dataset.size().value();

            if (dataset_size == 0) {
                LogToUI("[ERROR] There is no data.");
                return result;
            }

            int num_classes = dataset.getClassMap().size();
            LogToUI("[KAI DLL] Classes: " + std::to_string(num_classes) + " | Images: " + std::to_string(dataset_size));

            LogToUI("[KAI DLL] Uploading dataset to GPU...");

            std::vector<torch::Tensor> all_images;
            std::vector<torch::Tensor> all_targets;
            all_images.reserve(dataset_size);
            all_targets.reserve(dataset_size);

            for (size_t i = 0; i < dataset_size; ++i) {
                try {
                    auto sample = dataset.get(i);
                    all_images.push_back(sample.data);
                    all_targets.push_back(sample.target);
                }
                catch (...) { continue; }
            }

            auto full_images_tensor = torch::stack(all_images).to(device);
            auto full_targets_tensor = torch::stack(all_targets).to(device);

            LogToUI("[KAI DLL] Dataset in VRAM. Starting ultra-fast loop...");

            SimpleCNN model(num_classes, base_filters, hidden_neurons);
            model->to(device);
            model->train();

            torch::optim::Adam optimizer(model->parameters(), torch::optim::AdamOptions(learning_rate));

            std::vector<size_t> indices(dataset_size);
            std::iota(indices.begin(), indices.end(), 0);
            std::random_device rd;
            std::mt19937 g(rd());

            int num_batches = dataset_size / batch_size;
            if (num_batches == 0) num_batches = 1;

            float final_epoch_loss = 0.0f;
            float final_epoch_acc = 0.0f;

            for (int epoch = 1; epoch <= epochs; ++epoch) {

                std::shuffle(indices.begin(), indices.end(), g);

                auto indices_tensor = torch::from_blob(indices.data(), { (long)dataset_size }, torch::kLong).to(device);
                auto shuffled_images = full_images_tensor.index_select(0, indices_tensor);
                auto shuffled_targets = full_targets_tensor.index_select(0, indices_tensor);

                float epoch_loss = 0.0;
                float epoch_correct = 0.0;
                int batches_processed = 0;
                int total_samples_epoch = 0;

                for (int b = 0; b < num_batches; ++b) {

                    int start_idx = b * batch_size;
                    int end_idx = start_idx + batch_size;
                    if (end_idx > dataset_size) end_idx = dataset_size;

                    auto batch_imgs = shuffled_images.slice(0, start_idx, end_idx);
                    auto batch_lbls = shuffled_targets.slice(0, start_idx, end_idx);

                    optimizer.zero_grad();
                    auto output = model->forward(batch_imgs);
                    auto loss = torch::nn::functional::cross_entropy(output, batch_lbls);
                    loss.backward();
                    optimizer.step();

                    epoch_loss += loss.item<float>();

                    auto pred = output.argmax(1);
                    epoch_correct += pred.eq(batch_lbls).sum().item<float>();

                    batches_processed++;
                    total_samples_epoch += (end_idx - start_idx);
                }

                float avg_loss = (batches_processed > 0) ? (epoch_loss / batches_processed) : 0.0f;
                float avg_acc = (total_samples_epoch > 0) ? (epoch_correct / total_samples_epoch) : 0.0f;

                final_epoch_loss = avg_loss;
                final_epoch_acc = avg_acc;

                std::string msg = "Epoch [" + std::to_string(epoch) + "/" + std::to_string(epochs) +
                    "] Loss: " + std::to_string(avg_loss) +
                    " | Acc: " + std::to_string(avg_acc * 100.0f) + "%";
                LogToUI(msg);

                if (use_early_stopping && avg_loss <= target_loss) {
                    LogToUI("> [ENGINE] EARLY STOPPING TRIGGERED.");
                    break;
                }
            }

            LogToUI("[KAI DLL] Saving model...");
            torch::save(model, outPath);
            LogToUI("[KAI DLL] Model saved SUCCESSFULLY.");
            LogToUI("TRAINING_COMPLETE");

            result.final_loss = final_epoch_loss;
            result.final_accuracy = final_epoch_acc;
            result.success = true;

        }
        catch (const std::exception& e) {
            std::string err = "[CRITICAL ERROR]: ";
            err += e.what();
            LogToUI(err);
            result.success = false;
        }
        catch (...) {
            LogToUI("[CRITICAL ERROR] Unknown exception.");
            result.success = false;
        }

        return result;
    }

    KAI_API int PredictImage(const char* imagePath) {
        return 7;
    }
}