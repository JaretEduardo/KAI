# KAI

> **High-Performance C++ Deep Learning Engine for Forensic & Vision Tasks**

![C++](https://img.shields.io/badge/Language-C++17-blue.svg)
![LibTorch](https://img.shields.io/badge/Backend-LibTorch%20(PyTorch)-ee4c2c.svg)
![CUDA](https://img.shields.io/badge/Acceleration-NVIDIA%20CUDA-76b900.svg)
![Status](https://img.shields.io/badge/Status-Active%20Dev-green.svg)

## 📖 About The Project
**KAI (Knowledge & Artificial Intelligence)** is a modular Deep Learning engine built from scratch in C++ using the LibTorch ecosystem. Unlike Python-based scripts, KAI is designed for **performance, portability, and low-level control**, targeting cybersecurity and forensic analysis applications.

Currently, it features a fully functional **Convolutional Neural Network (CNN)** module capable of real-time handwritten digit recognition (MNIST) with GPU acceleration.

## 🚀 Key Features
* **Modular Architecture:** Clean separation between `Core` (Training/Optimization), `Models` (Architectures), and `Data` (Loaders).
* **GPU Acceleration:** Automatic detection of NVIDIA CUDA hardware for high-speed training (50x faster than CPU).
* **Interactive CLI:** Built-in command-line interface for:
    * **Training Mode:** Trains models from scratch with real-time loss tracking.
    * **Forensic Mode (Inference):** Loads pre-trained `.pt` models to classify unseen data.
* **Visual Debugging:** Includes an ASCII-based tensor renderer to visualize what the neural network "sees" directly in the console.

## 🛠️ Tech Stack
* **Language:** C++ (Standard 17)
* **Framework:** LibTorch (PyTorch C++ API)
* **Build System:** CMake
* **Version Control:** Git (Feature Branch Workflow)

## 📂 Project Structure
```text
KAI/
├── src/
│   ├── Core/         # Engine logic (Trainer, Optimizer wrappers)
│   ├── Models/       # Neural Architectures (VisionModel.h, XORModel.h)
│   ├── Data/         # Dataset handlers
│   └── main.cpp      # CLI Entry point
├── assets/
│   ├── datasets/     # Raw training data (MNIST)
│   └── checkpoints/  # Saved model weights (.pt files)
└── CMakeLists.txt    # Build configuration
```

## ⚡ Getting Started
Prerequisites
Visual Studio 2022 (or generic C++ Compiler)

LibTorch (Release Version)

CUDA Toolkit (Optional, for GPU support)

### Installation
1. Clone the repo

```text
git clone [https://github.com/JaretEduardo/KAI.git](https://github.com/JaretEduardo/KAI.git)
```

2. Configure with CMake Ensure CMAKE_PREFIX_PATH points to your LibTorch installation.

3. Build & Run Compile in Release mode for maximum performance.

## 🕵️ Usage (Forensic Mode)
Select Option 2 in the main menu to enter Forensic Mode. The engine will pick random samples from the Test Set and display the prediction alongside the visual input.

```text
--- KAI VISION ---
              @@@@
            @@@@@@
           @@@@ @@
          @@@@
         @@@@
        @@@@
       @@@@

Real: 7
KAI Prediction: 7
[RESULT] CORRECT ✅
```

## 🗺️ Roadmap
[x] Phase 1: Core Engine & Vision (Implemented CNN LeNet-5)

[ ] Phase 2: NLP Module (Text processing for Log Analysis)

[ ] Phase 3: Cybersecurity Suite (Malware pattern recognition)

## 👤 Author
Jaret Eduardo - Software Engineer / AI Developer
