#version 330 core

uniform vec2        EnvFogParams;
uniform vec4        EnvFogColor;
uniform vec3        ViewPos;
uniform sampler2D   TextureAlbedo;

in vec3 fragWorldPos;
in vec2 fragUV;

out vec4 FragColor;

void main()
{
    float distToCamera = length(fragWorldPos - ViewPos);
    float fogFactor = (distToCamera - EnvFogParams.x) / (EnvFogParams.y - EnvFogParams.x);

    fogFactor = floor(fogFactor * 4)/4.0;

    FragColor =  mix(texture(TextureAlbedo, fragUV), EnvFogColor, fogFactor);
}
