#include<iostream>

#include<GL/glew.h>
#include<GLFW/glfw3.h>

#include<imgui/examples/imgui_impl_opengl3.h>
#include<imgui/examples/imgui_impl_glfw.h>

#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>

// https://github.com/blepfo/opengl_utils
#include "GlUtils/Camera.hpp"
#include "GlUtils/Shader.hpp"
#include "GlUtils/TwoTrianglesRenderer.hpp"
#include "GlUtils/TextureUtils.hpp"


class MengerRenderer : public GlUtils::TwoTrianglesRenderer {
    public: 
        // Need to declare overridden functions BEFORE constructor
        // to avoid vtable errors
        void processInputs();
        void createGui();
        void setUniforms();

        MengerRenderer(
            const char* fragmentShaderPath,
            int screenWidth, 
            int screenHeight, 
            const char* windowName,
            GlUtils::Camera* camera
        ) : GlUtils::TwoTrianglesRenderer(fragmentShaderPath, screenWidth, screenHeight, windowName, true),
            camera(camera) {}

    protected:
        GlUtils::Camera* camera;
        float ao=0.8;
        float marchHitDist=0.01;
        float iTime=0.0f;
};


void MengerRenderer::createGui() {
    // Dear ImGui
    ImGui_ImplOpenGL3_NewFrame();
    ImGui_ImplGlfw_NewFrame();
    ImGui::NewFrame();
    ImGui::Begin("ImGui");
    ImGui::SliderFloat("iTime", &this->iTime, 0.0f, 20.0f);
    ImGui::SliderFloat("AO", &this->ao, 0.0f, 5.0f);
    ImGui::SliderFloat("MARCH_HIT_DIST", &this->marchHitDist, 0.00001f, 0.1f);
    ImGui::Text("pitch: %f, yaw=%f", this->camera->getPitch(), this->camera->getYaw());
    ImGui::End();
}


void MengerRenderer::setUniforms() {
    this->shader->setVec2("iResolution", glm::vec2(this->screenWidth, this->screenHeight));
    // Camera
    this->shader->setVec3("eye", this->camera->getOrigin());
    this->shader->setVec3("forward", this->camera->getForward());
    this->shader->setVec3("up", this->camera->getUp());
    this->shader->setVec3("right", this->camera->getRight());
    // Shader params
    this->shader->setFloat("GLOBAL_AO", this->ao);
    this->shader->setFloat("MARCH_HIT_DIST", this->marchHitDist);
    this->shader->setFloat("iTime", this->iTime);
}


void MengerRenderer::processInputs() {
    GlUtils::Camera::standardWalkProcessing(this->camera, this->_window, this->deltaTime);
}

int main() {
    GlUtils::Camera camera(glm::vec3(0.0f, 0.0f, 0.0f), 0.0f, 0.0f, 0.1f, 0.025f);
    MengerRenderer m = MengerRenderer("./src/test.fs", 400, 400, "window", &camera);
    std::cout << "CALL RUN" << std::endl;
    return m.run();
}
