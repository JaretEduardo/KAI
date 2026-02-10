#pragma once
#include <torch/torch.h>

struct SimpleCNNImpl : torch::nn::Module {

    SimpleCNNImpl(int num_classes);

    torch::Tensor forward(torch::Tensor x);

    torch::nn::Conv2d conv1{ nullptr };
    torch::nn::BatchNorm2d bn1{ nullptr };

    torch::nn::Conv2d conv2{ nullptr };
    torch::nn::BatchNorm2d bn2{ nullptr };

    torch::nn::Conv2d conv3{ nullptr };
    torch::nn::BatchNorm2d bn3{ nullptr };

    torch::nn::Linear fc1{ nullptr };
    torch::nn::Linear fc2{ nullptr };
};

TORCH_MODULE(SimpleCNN);