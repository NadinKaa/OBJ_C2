#version 140

in vec3 aPosition;
in vec3 aColor;
out vec3 vColor;
uniform mat4 uMvpMatrix;

void main()
{
    gl_Position = uMvpMatrix * vec4(aPosition, 1.0);
    vColor = aColor;
}
