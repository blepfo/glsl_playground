#include<math.h>
#include<iostream>
#include<random>

#include<GL/glew.h>
#include<GLFW/glfw3.h>

#include<imgui/examples/imgui_impl_opengl3.h>
#include<imgui/examples/imgui_impl_glfw.h>

#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>

// https://github.com/blepfo/opengl_utils
#include "opengl_utils/include/Camera.h"
#include "opengl_utils/include/Init.h"
#include "opengl_utils/include/Shader.h"
#include "opengl_utils/include/texture.h"

void framebuffer_size_callback(GLFWwindow* window, int width, int height);
void process_input(GLFWwindow* window, float deltaTime);

int SCREEN_WIDTH = 800;
int SCREEN_HEIGHT = 600;


int main() {
    try {
        // Initialize GLFW window with OpenGL 3.3
        GLFWwindow* window =  Init::basicWindow(3, 3, SCREEN_WIDTH, SCREEN_HEIGHT, "window");
        glfwMakeContextCurrent(window);
        Init::glew();
        // Setup GLFW window
        glfwSetFramebufferSizeCallback(window, framebuffer_size_callback);
        framebuffer_size_callback(window, SCREEN_WIDTH, SCREEN_HEIGHT);
        // Use depth test
        glEnable(GL_DEPTH_TEST);

        // Setup Dear Imgui
        IMGUI_CHECKVERSION();
        ImGui::CreateContext();
        //ImGuiIO &io = ImGui::GetIO();
        ImGui_ImplGlfw_InitForOpenGL(window, true);
        ImGui_ImplOpenGL3_Init("#version 330 core");
        ImGui::StyleColorsDark();

        // Vertex data          
		float vertices[] = {
            -1.0, -1.0, 0.0,
            -1.0, 1.0, 0.0,
            1.0, -1.0, 0.0,
            1.0, 1.0, 0.0,
		};
        unsigned int indices[] = {
            0, 1, 2,
            1, 3, 2
        };
        unsigned int vao;
        glGenVertexArrays(1, &vao);
        glBindVertexArray(vao);
        unsigned int vbo;
        glGenBuffers(1, &vbo);
        glBindBuffer(GL_ARRAY_BUFFER, vbo);
        glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);
        glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 3*sizeof(float), (void*) 0);
        glEnableVertexAttribArray(0);
        unsigned int ebo;
        glGenBuffers(1, &ebo);
        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
        glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(indices), indices, GL_STATIC_DRAW);

        Shader shader(
            "./src/twotriangle.vs", 
            "./src/test.fs"
        );

        // ImGui Params

        // Render loop
        while(!glfwWindowShouldClose(window)) {
            // Process inputs with camera update
            float deltaTime = 0.0f;
            process_input(window, deltaTime);

            // Render commands
            glClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

            // Dear ImGui
            ImGui_ImplOpenGL3_NewFrame();
            ImGui_ImplGlfw_NewFrame();
            ImGui::NewFrame();
            ImGui::Begin("ImGui");
            ImGui::Text("Test");
            ImGui::End();

            // Render TwoTriangles
            shader.activate();
            shader.setVec2("iResolution", glm::vec2(SCREEN_WIDTH, SCREEN_HEIGHT));
            glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, 0);
            
            // Render Gui
            ImGui::Render();
            ImGui_ImplOpenGL3_RenderDrawData(ImGui::GetDrawData());

            glfwSwapBuffers(window);
            glfwPollEvents();
        }
    } catch(const std::exception &e) {
        std::cerr << e.what() << std::endl;
        glfwTerminate();
        return -1;
    }

    std::cout << "EXIT MAIN" << std::endl;
    glfwTerminate();
    ImGui_ImplOpenGL3_Shutdown();
    ImGui_ImplGlfw_Shutdown();
    ImGui::DestroyContext();
    return 0;
}


void framebuffer_size_callback(GLFWwindow* window, int width, int height) {
    glViewport(0, 0, width, height);
    SCREEN_WIDTH = width;
    SCREEN_HEIGHT = height;
}


void process_input(GLFWwindow* window, float deltaTime) {
    if (glfwGetKey(window, GLFW_KEY_ESCAPE) == GLFW_PRESS) {
        glfwSetWindowShouldClose(window, true);
        std::cout << "ESC KEY -> CLOSE WINDOW" << std::endl;
    }
    /*
    // Walk input
    if (glfwGetKey(window, GLFW_KEY_W) == GLFW_PRESS) {
        camera.translate(CameraDirection::FORWARD, true, deltaTime);
    }
    if (glfwGetKey(window, GLFW_KEY_A) == GLFW_PRESS) {
        camera.translate(CameraDirection::RIGHT, true, deltaTime);
    }
    if (glfwGetKey(window, GLFW_KEY_S) == GLFW_PRESS) {
        camera.translate(CameraDirection::FORWARD, false, deltaTime);
    }
    if (glfwGetKey(window, GLFW_KEY_D) == GLFW_PRESS) {
        camera.translate(CameraDirection::RIGHT, false, deltaTime);
    }
    if (glfwGetKey(window, GLFW_KEY_Q) == GLFW_PRESS) {
        camera.translate(CameraDirection::UP, false, deltaTime);
    }
    if (glfwGetKey(window, GLFW_KEY_E) == GLFW_PRESS) {
        camera.translate(CameraDirection::UP, true, deltaTime);
    }
    // Rotation
    if (glfwGetKey(window, GLFW_KEY_UP) == GLFW_PRESS) {
        camera.updateRotation(-1.0f, 0.0f);
    }
    if (glfwGetKey(window, GLFW_KEY_DOWN) == GLFW_PRESS) {
        camera.updateRotation(1.0f, 0.0f);
    }
    if (glfwGetKey(window, GLFW_KEY_RIGHT) == GLFW_PRESS) {
        camera.updateRotation(0.0f, -1.0f);
    }
    if (glfwGetKey(window, GLFW_KEY_LEFT) == GLFW_PRESS) {
        camera.updateRotation(0.0f, 1.0f);
    }
    */
}
