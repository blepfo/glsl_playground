#version 330 core

out vec4 FragColor;

uniform vec2 iResolution;

float sphereSdf(vec2 p, float r) {
    return length(p) - r;
}

void main() {
    vec2 uv = gl_FragCoord.xy / iResolution.xy;;
    uv -= 0.5;
    uv.x *= iResolution.x / iResolution.y;
    uv *= 2.;

    vec2 grid = fract(uv * 10.);

    float sdf = sphereSdf(uv, 0.5);
    float s = step(0., sdf);

    vec3 color = vec3(grid.x*grid.y);
    color = mix(color, vec3(1.), 1.-s);
    FragColor = vec4(color, 1.0);
}
