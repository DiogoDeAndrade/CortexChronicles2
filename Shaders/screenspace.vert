#version 330 core
layout (location = 0) in vec3 position;
layout (location = 1) in vec2 uv;

uniform mat4        MatrixClip;
uniform vec2        MaterialWorldPos;
uniform vec2        MaterialWorldScale;
uniform vec2        Resolution;

out vec3 fragWorldPos;
out vec2 fragUV;

void main()
{
    // Get world position
    vec3 actualPos = position;

    fragUV = uv;
    gl_Position = vec4(((position.x * MaterialWorldScale.x + MaterialWorldPos.x) / Resolution.x) * 2 - 1,
                       ((position.y * MaterialWorldScale.y + MaterialWorldPos.y) / Resolution.y) * 2 - 1,
                       0.01,
                       1);
}
