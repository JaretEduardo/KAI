#include "SimpleCNN.h"

SimpleCNNImpl::SimpleCNNImpl(int num_classes, int base_filters, int hidden_neurons) {

    conv1 = register_module("conv1", torch::nn::Conv2d(torch::nn::Conv2dOptions(3, base_filters, 3).padding(1)));
    bn1 = register_module("bn1", torch::nn::BatchNorm2d(base_filters));

    conv2 = register_module("conv2", torch::nn::Conv2d(torch::nn::Conv2dOptions(base_filters, base_filters * 2, 3).padding(1)));
    bn2 = register_module("bn2", torch::nn::BatchNorm2d(base_filters * 2));

    conv3 = register_module("conv3", torch::nn::Conv2d(torch::nn::Conv2dOptions(base_filters * 2, base_filters * 4, 3).padding(1)));
    bn3 = register_module("bn3", torch::nn::BatchNorm2d(base_filters * 4));

    int final_channels = base_filters * 4;
    int flattened_size = final_channels * 8 * 8;

    fc1 = register_module("fc1", torch::nn::Linear(flattened_size, hidden_neurons));
    fc2 = register_module("fc2", torch::nn::Linear(hidden_neurons, num_classes));
}

torch::Tensor SimpleCNNImpl::forward(torch::Tensor x) {

    x = torch::relu(bn1(conv1(x)));
    x = torch::max_pool2d(x, 2);

    x = torch::relu(bn2(conv2(x)));
    x = torch::max_pool2d(x, 2);

    x = torch::relu(bn3(conv3(x)));
    x = torch::max_pool2d(x, 2);

    x = x.view({ x.size(0), -1 });

    x = torch::relu(fc1(x));
    x = torch::dropout(x, 0.5, is_training());
    x = fc2(x);

    return x;
}