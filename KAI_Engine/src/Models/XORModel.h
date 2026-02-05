#pragma once
#include <torch/torch.h>

struct XORModel : torch::nn::Module {
    torch::nn::Linear fc1{ nullptr }, fc2{ nullptr };

    XORModel() {
        fc1 = register_module("fc1", torch::nn::Linear(2, 8));
        fc2 = register_module("fc2", torch::nn::Linear(8, 1));
    }

    torch::Tensor forward(torch::Tensor x) {
        x = torch::relu(fc1->forward(x));
        x = torch::sigmoid(fc2->forward(x));
        return x;
    }
};