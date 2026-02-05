#pragma once
#include <torch/torch.h>

struct VisionModel : torch::nn::Module {
    torch::nn::Conv2d conv1{ nullptr }, conv2{ nullptr };

    torch::nn::Linear fc1{ nullptr }, fc2{ nullptr }, fc3{ nullptr };

    VisionModel() {
        conv1 = register_module("conv1", torch::nn::Conv2d(torch::nn::Conv2dOptions(1, 6, 5)));

        conv2 = register_module("conv2", torch::nn::Conv2d(torch::nn::Conv2dOptions(6, 16, 5)));

        fc1 = register_module("fc1", torch::nn::Linear(16 * 4 * 4, 120));
        fc2 = register_module("fc2", torch::nn::Linear(120, 84));
        fc3 = register_module("fc3", torch::nn::Linear(84, 10));
    }

    torch::Tensor forward(torch::Tensor x) {
        x = torch::max_pool2d(torch::relu(conv1->forward(x)), 2);

        x = torch::max_pool2d(torch::relu(conv2->forward(x)), 2);

        x = x.view({ -1, 16 * 4 * 4 });

        x = torch::relu(fc1->forward(x));
        x = torch::relu(fc2->forward(x));
        x = fc3->forward(x);

        return torch::log_softmax(x, 1);
    }
};