
OBJ = ./obj

# MacOS gets Opengl stuff from -framework Opengl
OPENGL_ARGS_MACOS = -framework Opengl \
        -I/usr/local/include -lGLFW -lglew 

# Convert any/path/file.cpp -> $(OBJ)/file.o
define ObjFromSrc
	$(patsubst %, $(OBJ)/%,  $(notdir $(patsubst %.cpp, %.o, $(1))))
endef

# Find .cpp for a .o file with arbitrary directory paths
# Arg1 - Obj filename
# Arg2 - List of CPP filenames
define FindCppSrc
	$(filter %/$(notdir $(patsubst %.o, %.cpp, $(1))), $(2))
endef


# opengl_utils stuff
OPENGL_UTILS_SRC = $(wildcard ../opengl_utils/src/*.cpp) 
OPENGL_UTILS_INCLUDE = -I../
OPENGL_UTILS_OBJ = $(call ObjFromSrc, $(OPENGL_UTILS_SRC))

# Dear ImGui stuff
IMGUI_INCLUDE = -I/usr/local/include/imgui
IMGUI_SRC = $(wildcard /usr/local/include/imgui/*.cpp) \
    /usr/local/include/imgui/examples/imgui_impl_opengl3.cpp \
    /usr/local/include/imgui/examples/imgui_impl_glfw.cpp 
IMGUI_OBJ = $(call ObjFromSrc, $(IMGUI_SRC))

# Compiler options
CC = g++
CPP_ARGS = -Wall

all: main.o


main.o: $(IMGUI_OBJ) $(OPENGL_UTILS_OBJ)
	$(CC) $(CPP_ARGS) \
		$(OPENGL_UTILS_INCLUDE) $(OPENGL_UTILS_OBJ) \
		$(IMGUI_INCLUDE) $(IMGUI_OBJ) \
        $(OPENGL_ARGS_MACOS) \
        -o bin/$@ \
		src/main.cpp


$(OPENGL_UTILS_OBJ): %.o : $(OPENGL_UTILS_SRC)
	$(eval CPP_NAME := $(call FindCppSrc, $@, $^))
	$(CC) $(CPP_ARGS) -c $(OPENGL_UTILS_INCLUDE) -o $@ $(CPP_NAME)


$(IMGUI_OBJ): %.o : $(IMGUI_SRC)
	$(eval CPP_NAME := $(call FindCppSrc, $@, $^))
	$(CC) -c $(IMGUI_INCLUDE) -o $@ $(CPP_NAME)


.PHONY: clean
clean: 
	rm bin/* \
	&& rm obj/*
