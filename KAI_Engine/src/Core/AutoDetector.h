#pragma once
#include <string>
#include <map>
#include <filesystem>
#include <vector>

namespace fs = std::filesystem;

enum class TaskType {
    UNKNOWN,
    VISION,
    NLP,
    AUDIO
};

class AutoDetector {
public:
    static TaskType DetectTaskType(const std::string& folderPath);

    static std::string TaskTypeToString(TaskType type);

private:
    static const std::vector<std::string> visionExts;
    static const std::vector<std::string> nlpExts;
    static const std::vector<std::string> audioExts;
};