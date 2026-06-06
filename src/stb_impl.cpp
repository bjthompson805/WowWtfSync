// Single translation unit that compiles the stb_image implementation.
// Keeping it isolated avoids polluting other files with the large macro expansion.
#define STB_IMAGE_IMPLEMENTATION
#include "stb_image.h"
