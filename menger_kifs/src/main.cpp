#include<iostream>

#include<GL/glew.h>
#include<GLFW/glfw3.h>

#include<imgui/examples/imgui_impl_opengl3.h>
#include<imgui/examples/imgui_impl_glfw.h>

#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>

// https://github.com/blepfo/opengl_utils
#include "opengl_utils/include/Camera.h"
#include "opengl_utils/include/Shader.h"
#include "opengl_utils/include/SimpleRenderer.h"
#include "opengl_utils/include/texture.h"


class MengerRenderer : public SimpleRenderer {
    public: 
        // Need to declare overridden functions BEFORE constructor
        // to avoid vtable errors
        void initScene();
        void processInputs();
        void createGui();
        void renderObjects();

        MengerRenderer(
            int screenWidth, 
            int screenHeight, 
            const char* windowName,
            Camera camera
        ) : SimpleRenderer(screenWidth, screenHeight, windowName, true),
            camera(camera) {}

    protected:
        // TODO - SceneObject class
        unsigned int vao;
        unsigned int vbo;
        unsigned int ebo;
        Shader* shader;
        Camera camera;

};

void MengerRenderer::initScene() {
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
        // Store for access in renderObjects()
        this->vao = vao;
        this->vbo = vbo;
        this->ebo = ebo;
    
        // Init shader    
        this->shader = new Shader(
            "./src/twotriangle.vs", 
            "./src/test.fs"
        );
}

void MengerRenderer::createGui() {
    // Dear ImGui
    ImGui_ImplOpenGL3_NewFrame();
    ImGui_ImplGlfw_NewFrame();
    ImGui::NewFrame();
    ImGui::Begin("ImGui");
    ImGui::Text("pitch: %f, yaw=%f", this->camera.getPitch(), this->camera.getYaw());
    ImGui::End();
}

void MengerRenderer::renderObjects() {
    // Render TwoTriangles
    glBindVertexArray(this->vao);
    this->shader->activate();
    this->shader->setVec2("iResolution", glm::vec2(this->screenWidth, this->screenHeight));
    // Camera
    this->shader->setVec3("eye", this->camera.getOrigin());
    this->shader->setVec3("forward", this->camera.getForward());
    this->shader->setVec3("up", this->camera.getUp());
    this->shader->setVec3("right", this->camera.getRight());

    glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, 0);
}

void MengerRenderer::processInputs() {
    GLFWwindow* window = this->_window;
    if (glfwGetKey(window, GLFW_KEY_ESCAPE) == GLFW_PRESS) {
        glfwSetWindowShouldClose(window, true);
        std::cout << "ESC KEY -> CLOSE WINDOW" << std::endl;
    }
    // Walk input
    Camera camera = this->camera;
    float deltaTime = this->deltaTime;
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
}

int main() {
    Camera camera(glm::vec3(0.0f, 0.0f, 0.0f), 0.0f, 0.0f, 0.1f, 0.025f);
    MengerRenderer m = MengerRenderer(800, 600, "window", camera);
    std::cout << "CALL RUN" << std::endl;
    return m.run();
}
