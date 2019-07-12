using OpenTK.Graphics.OpenGL;
using System;

namespace fl8_1
{
    class VertexBuffers
    {
        public static int InitAndGetAmountOfVertices(
            int program,
            float[] vertices,
            float[] colors,
            int[] indices)
        {
            // Write vertex information to buffer object
            if (!InitArrayBuffer(program, vertices, 3, "aPosition"))
            {
                return -1;
            }
            if (!InitArrayBuffer(program, colors, 3, "aColor"))
            {
                return -1;
            }

            // Unbind the buffer object
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // Create a buffer
            int indexBuffer;
            GL.GenBuffers(1, out indexBuffer);

            // Write the vertices to the buffer object
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(int) * indices.Length, indices, BufferUsageHint.StaticDraw);

            return indices.Length;
        }

        private static bool InitArrayBuffer(
            int program, float[] data, int n, string attributeName)
        {
            // Create a buffer object
            int buffer;
            GL.GenBuffers(1, out buffer);

            // Write data into the buffer object
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * data.Length, data, BufferUsageHint.StaticDraw);

            // Assign the buffer object to the attribute variable
            int attributeLocation = GL.GetAttribLocation(program, attributeName);
            if (attributeLocation < 0)
            {
                Console.WriteLine("Failed to get the storage location of " + attributeName);
                return false;
            }
            GL.VertexAttribPointer(attributeLocation, n, VertexAttribPointerType.Float, false, 0, 0);

            // Enable the assignment to attributeLocation variable
            GL.EnableVertexAttribArray(attributeLocation);

            return true;
        }
    }
}
