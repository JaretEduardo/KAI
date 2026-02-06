#pragma once
#include <torch/torch.h>
#include <vector>
#include <string>
#include <map>
#include <filesystem>

namespace fs = std::filesystem;

using DataPair = std::pair<std::string, int64_t>;

class ImageDataset : public torch::data::datasets::Dataset<ImageDataset> {
public:
    ImageDataset(const std::string& root, int image_size = 64);

    torch::data::Example<> get(size_t index) override;

    torch::optional<size_t> size() const override;

    const std::map<int64_t, std::string>& getClassMap() const { return classMap; }

private:
    std::vector<DataPair> data;
    std::map<int64_t, std::string> classMap;
    int target_size;

    torch::Tensor load_image(const std::string& path);
};