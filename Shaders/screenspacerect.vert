#version 330 core
layout (location = 0) in vec3 position;

uniform vec2        Resolution;

void main()
{
    gl_Position = vec4((position.x / Resolution.x) * 2 - 1,
                       -((position.y / Resolution.y) * 2 - 1),
                       0.01,
                       1);
}
