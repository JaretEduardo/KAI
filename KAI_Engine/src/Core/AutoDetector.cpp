#include "AutoDetector.h"
#include <algorithm>
#include <iostream>

const std::vector<std::string> AutoDetector::visionExts = { ".jpg", ".jpeg", ".png", ".bmp", ".tiff" };
const std::vector<std::string> AutoDetector::nlpExts = { ".txt", ".csv", ".md", ".cpp", ".h", ".cs", ".py", ".json", ".xml", ".html" };
const std::vector<std::string> AutoDetector::audioExts = { ".wav", ".mp3", ".flac", ".ogg" };

TaskType AutoDetector::DetectTaskType(const std::string& folderPath) {
    if (!fs::exists(folderPath)) return TaskType::UNKNOWN;

    int visionCount = 0;
    int nlpCount = 0;
    int audioCount = 0;
    int totalFiles = 0;

    for (const auto& entry : fs::recursive_directory_iterator(folderPath)) {
        if (entry.is_regular_file()) {
            std::string ext = entry.path().extension().string();

            std::transform(ext.begin(), ext.end(), ext.begin(), ::tolower);

            for (const auto& e : visionExts) if (ext == e) visionCount++;
            for (const auto& e : nlpExts) if (ext == e) nlpCount++;
            for (const auto& e : audioExts) if (ext == e) audioCount++;

            totalFiles++;
        }
    }

    if (totalFiles == 0) return TaskType::UNKNOWN;

    if (visionCount > nlpCount && visionCount > audioCount) return TaskType::VISION;
    if (nlpCount > visionCount && nlpCount > audioCount) return TaskType::NLP;
    if (audioCount > visionCount && audioCount > nlpCount) return TaskType::AUDIO;

    return TaskType::UNKNOWN;
}

std::string AutoDetector::TaskTypeToString(TaskType type) {
    switch (type) {
    case TaskType::VISION: return "Computer Vision (Imagenes)";
    case TaskType::NLP: return "NLP (Texto/Codigo)";
    case TaskType::AUDIO: return "Procesamiento de Audio";
    default: return "Desconocido / Carpeta Vacia";
    }
}