cmake_minimum_required (VERSION 3.8)

project ("KAI" LANGUAGES CXX CUDA)

set(CUDA_ROOT "C:/Program Files/NVIDIA GPU Computing Toolkit/CUDA/v13.1")

set(CMAKE_CUDA_COMPILER "${CUDA_ROOT}/bin/nvcc.exe")

set(CUDA_TOOLKIT_ROOT_DIR "${CUDA_ROOT}")
set(ENV{CUDA_PATH} "${CUDA_ROOT}")
set(ENV{CUDA_HOME} "${CUDA_ROOT}")

set(CMAKE_CUDA_FLAGS "${CMAKE_CUDA_FLAGS} -allow-unsupported-compiler")
set(CMAKE_CXX_STANDARD 17)

set(CMAKE_PREFIX_PATH "C:/libtorch/share/cmake/Torch")
find_package(Torch REQUIRED)

include_directories("${CMAKE_CURRENT_SOURCE_DIR}/src")
file(GLOB_RECURSE SOURCES "src/*.cpp" "src/*.h")

add_executable (KAI ${SOURCES})

target_link_libraries (KAI "${TORCH_LIBRARIES}")
set_property(TARGET KAI PROPERTY CXX_STANDARD 17)

add_custom_command(TARGET KAI POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_directory
    "C:/libtorch/lib"
    $<TARGET_FILE_DIR:KAI>
    COMMENT "Copiando DLLs..."
)