﻿#version 140

in vec3 aPosition;
in vec3 aColor;
out vec3 vColor;
uniform mat4 uModelMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjMatrix;

void main()
{
    gl_Position = uProjMatrix * uViewMatrix * uModelMatrix * vec4(aPosition, 1.0);
    vColor = aColor;
}
