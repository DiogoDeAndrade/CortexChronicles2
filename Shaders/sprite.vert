#version 330 core
layout (location = 0) in vec3 position;
layout (location = 1) in vec2 uv;

uniform mat4        MatrixClip;
uniform mat4        MatrixWorld;
uniform vec3        ViewUp;
uniform vec3        ViewRight;

out vec3 fragWorldPos;
out vec2 fragUV;

void main()
{
    // Get world position
    vec3 actualPos = ViewRight * position.x + ViewUp * position.y;
    fragWorldPos = (MatrixWorld * vec4(actualPos, 1.0)).xyz; 
    fragUV = uv;
    gl_Position = MatrixClip * vec4(actualPos, 1.0);
}
