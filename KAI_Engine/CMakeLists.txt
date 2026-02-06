cmake_minimum_required (VERSION 3.8)

# --- CORRECCION CRITICA ---
# 1. Definimos la bandera ANTES de activar el lenguaje CUDA.
#    Esto es vital para que el chequeo inicial de CMake no falle.
set(CMAKE_CUDA_FLAGS "${CMAKE_CUDA_FLAGS} -allow-unsupported-compiler")

# 2. Iniciamos el proyecto SOLO con CXX (C++) primero.
#    No pongas CUDA aqui, o fallará antes de leer la bandera.
project ("KAI_Engine" LANGUAGES CXX)

# 3. Configuraciones de CUDA
set(CUDA_ROOT "C:/Program Files/NVIDIA GPU Computing Toolkit/CUDA/v13.1")
set(CMAKE_CUDA_COMPILER "${CUDA_ROOT}/bin/nvcc.exe")
set(CUDA_TOOLKIT_ROOT_DIR "${CUDA_ROOT}")

# 4. AHORA SI activamos CUDA manualmente.
#    Como la bandera ya esta seteada arriba, CMake pasará el chequeo.
enable_language(CUDA)

set(CMAKE_CXX_STANDARD 17)

# LibTorch
set(CMAKE_PREFIX_PATH "C:/libtorch/share/cmake/Torch")
find_package(Torch REQUIRED)

include_directories("${CMAKE_CURRENT_SOURCE_DIR}/src")
file(GLOB_RECURSE SOURCES "src/*.cpp" "src/*.h")

# Definir la DLL
add_library (KAI_Engine SHARED ${SOURCES})

target_link_libraries (KAI_Engine "${TORCH_LIBRARIES}")
set_property(TARGET KAI_Engine PROPERTY CXX_STANDARD 17)
target_compile_definitions(KAI_Engine PRIVATE KAI_EXPORTS)

# Automatizacion de copia (Tu script original)
add_custom_command(TARGET KAI_Engine POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
    "$<TARGET_FILE:KAI_Engine>"
    "${CMAKE_CURRENT_SOURCE_DIR}/../KAI_UI/bin/Debug/net8.0-windows/KAI_Engine.dll"
    
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
    "C:/libtorch/lib/torch_cpu.dll"
    "${CMAKE_CURRENT_SOURCE_DIR}/../KAI_UI/bin/Debug/net8.0-windows/"
    
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
    "C:/libtorch/lib/c10.dll"
    "${CMAKE_CURRENT_SOURCE_DIR}/../KAI_UI/bin/Debug/net8.0-windows/"

    COMMENT "🚀 Desplegando DLLs al Frontend WPF..."
)