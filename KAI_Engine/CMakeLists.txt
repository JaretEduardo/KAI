cmake_minimum_required (VERSION 3.8)

set(CMAKE_CUDA_FLAGS "${CMAKE_CUDA_FLAGS} -allow-unsupported-compiler")

project ("KAI_Engine" LANGUAGES CXX)

set(CUDA_ROOT "C:/Program Files/NVIDIA GPU Computing Toolkit/CUDA/v13.1")
set(CMAKE_CUDA_COMPILER "${CUDA_ROOT}/bin/nvcc.exe")
set(CUDA_TOOLKIT_ROOT_DIR "${CUDA_ROOT}")

enable_language(CUDA)

set(CMAKE_CXX_STANDARD 17)

set(CMAKE_PREFIX_PATH "C:/libtorch/share/cmake/Torch")
find_package(Torch REQUIRED)

include_directories("${CMAKE_CURRENT_SOURCE_DIR}/src")
file(GLOB_RECURSE SOURCES "src/*.cpp" "src/*.h")

add_library (KAI_Engine SHARED ${SOURCES})

target_link_libraries (KAI_Engine "${TORCH_LIBRARIES}")
set_property(TARGET KAI_Engine PROPERTY CXX_STANDARD 17)
target_compile_definitions(KAI_Engine PRIVATE KAI_EXPORTS)

add_custom_command(TARGET KAI_Engine POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
    "$<TARGET_FILE:KAI_Engine>"
    "${CMAKE_CURRENT_SOURCE_DIR}/../KAI_UI/bin/Release/net8.0-windows/KAI_Engine.dll"
    
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
    "C:/libtorch/lib/torch_cpu.dll"
    "${CMAKE_CURRENT_SOURCE_DIR}/../KAI_UI/bin/Release/net8.0-windows/"
    
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
    "C:/libtorch/lib/c10.dll"
    "${CMAKE_CURRENT_SOURCE_DIR}/../KAI_UI/bin/Release/net8.0-windows/"

    COMMENT "🚀 Desplegando DLLs al Frontend (RELEASE MODE)..."
)