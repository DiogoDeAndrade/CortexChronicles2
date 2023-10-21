#version 330 core
layout (location = 0) in vec3 position;
layout (location = 1) in vec2 uv;

uniform mat4        MatrixClip;
uniform mat4        MatrixWorld;
uniform vec3        ViewDir;
uniform vec3        ViewUp;
uniform vec3        ViewRight;
uniform float       MaterialSpriteRotation;
uniform bool        MaterialBillboardSprite;

out vec3 fragWorldPos;
out vec2 fragUV;

void main()
{
    // Get world position
    vec3 actualPos = position;

    float s = sin(MaterialSpriteRotation);
    float c = cos(MaterialSpriteRotation);
    float x = actualPos.x;
    float y = actualPos.y;
    float z = actualPos.z;

    if (MaterialBillboardSprite)
    {
        actualPos.x = x * c - y * s;
        actualPos.y = x * s + y * c;
    }
    else
    {
        actualPos.x = x * c - z * s;
        actualPos.z = x * s + z * c;
    }

    actualPos = ViewRight * actualPos.x + ViewUp * actualPos.y + ViewDir * actualPos.z;

    fragWorldPos = (MatrixWorld * vec4(actualPos, 1.0)).xyz; 
    fragUV = uv;
    gl_Position = MatrixClip * vec4(actualPos, 1.0);
}
