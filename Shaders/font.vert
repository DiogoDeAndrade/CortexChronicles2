#version 330 core
layout (location = 0) in vec3 position;
layout (location = 2) in vec4 color;
layout (location = 1) in vec2 uv;

uniform mat4        MatrixClip;
uniform vec2        Resolution;

out vec4 fragColor;
out vec2 fragUV;

void main()
{
    // Get world position
    vec3 actualPos = position;

    fragColor = color;
    fragUV = uv;
    gl_Position = vec4(  (position.x / Resolution.x) * 2 - 1,
                       -((position.y / Resolution.y) * 2 - 1),
                       0.01,
                       1);
}
