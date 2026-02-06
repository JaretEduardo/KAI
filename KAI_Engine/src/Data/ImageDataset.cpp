#include "ImageDataset.h"
#include <iostream>
#include <algorithm>

#define STB_IMAGE_IMPLEMENTATION
#include "../Vendor/stb_image.h"

#include <torch/torch.h>

namespace F = torch::nn::functional;

ImageDataset::ImageDataset(const std::string& root, int image_size)
    : target_size(image_size) {

    if (!fs::exists(root)) {
        std::cerr << "ERROR: La carpeta del dataset no existe: " << root << std::endl;
        return;
    }

    int classID = 0;
    for (const auto& entry : fs::directory_iterator(root)) {
        if (entry.is_directory()) {
            std::string className = entry.path().filename().string();
            classMap[classID] = className;

            for (const auto& file : fs::directory_iterator(entry.path())) {
                std::string ext = file.path().extension().string();
                std::transform(ext.begin(), ext.end(), ext.begin(), ::tolower);

                if (ext == ".jpg" || ext == ".png" || ext == ".jpeg" || ext == ".bmp") {
                    data.push_back({ file.path().string(), classID });
                }
            }
            classID++;
        }
    }

    std::cout << "[ImageDataset] Cargadas " << data.size() << " imagenes en " << classMap.size() << " clases." << std::endl;
}

torch::data::Example<> ImageDataset::get(size_t index) {
    std::string imagePath = data[index].first;
    int64_t label = data[index].second;

    torch::Tensor img_tensor = load_image(imagePath);

    return { img_tensor, torch::tensor(label, torch::kLong) };
}

torch::optional<size_t> ImageDataset::size() const {
    return data.size();
}

torch::Tensor ImageDataset::load_image(const std::string& path) {
    int width, height, channels;

    unsigned char* img_data = stbi_load(path.c_str(), &width, &height, &channels, 3);

    if (!img_data) {
        std::cerr << "ERROR: No se pudo cargar la imagen: " << path << std::endl;
        return torch::zeros({ 3, target_size, target_size });
    }

    auto tensor = torch::from_blob(img_data, { height, width, 3 }, torch::kUInt8);

    tensor = tensor.clone();
    stbi_image_free(img_data);

    tensor = tensor.to(torch::kFloat32).div(255.0);
    tensor = tensor.permute({ 2, 0, 1 });

    tensor = tensor.unsqueeze(0);
    tensor = F::interpolate(tensor, F::InterpolateFuncOptions()
        .size(std::vector<int64_t>({ target_size, target_size }))
        .mode(torch::kBilinear)
        .align_corners(false));

    tensor = tensor.squeeze(0);

    return tensor;
}