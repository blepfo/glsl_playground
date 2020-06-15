BUILD=./build
BIN=./bin
SRC=./src

# MacOS gets Opengl stuff from -framework Opengl
OPENGL_ARGS = -framework Opengl -I/usr/local/include -lGLFW -lglew 

MAKEFILE_DIR=$(dir $(abspath $(firstword $(MAKEFILE_LIST))))

# OpenGl Utils
UTILS_DIR=$(MAKEFILE_DIR)../opengl_utils
UTILS_LIB=$(UTILS_DIR)/lib

# Dear ImGui
IMGUI=/usr/local/include/imgui
IMGUI_EXAMPLES=$(IMGUI)/examples

# Compiler options
CC = g++ --std=c++17
CPP_ARGS = -Wall

all: $(BIN)/main.o


$(BIN)/main.o: $(SRC)/main.cpp $(SRC)/test.fs $(UTILS_LIB)/imgui.a $(UTILS_LIB)/GlUtils.a
	$(CC) $(CPP_ARGS) \
		-I$(IMGUI) \
		-I$(IMGUI_EXAMPLES) \
		-I$(UTILS_DIR) \
		$(UTILS_LIB)/imgui.a \
		$(UTILS_LIB)/GlUtils.a \
		$(OPENGL_ARGS) \
		-o $@ \
		$(SRC)/main.cpp


.PHONY: clean
clean: 
	rm -rf $(BUILD) $(BIN)
