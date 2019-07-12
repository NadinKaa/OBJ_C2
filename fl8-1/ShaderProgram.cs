using OpenTK.Graphics.OpenGL;
using System;
using System.IO;

namespace fl8_1
{
    class ShaderProgram
    {
        public static int InitAndGetShaderProgramId(
            string vShaderPath,
            string fShaderPath)
        {
            int vShader = GetShaderId(vShaderPath, ShaderType.VertexShader);
            int fShader = GetShaderId(fShaderPath, ShaderType.FragmentShader);
            if (vShader == -1 || fShader == -1)
            {
                return -1;
            }

            int program = GL.CreateProgram();
            GL.AttachShader(program, vShader);
            GL.AttachShader(program, fShader);
            GL.LinkProgram(program);
            GL.UseProgram(program);

            return program;
        }

        private static int GetShaderId(string path, ShaderType type)
        {
            string source = null;

            try
            {
                source = File.ReadAllText(path);
            }
            catch (Exception)
            {
                Console.WriteLine(string.Format("Failed to open the file \"{0}\"", path));
                return -1;
            }

            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            int ok;
            GL.GetShader(shader, ShaderParameter.CompileStatus, out ok);
            if (ok == 0)
            {
                Console.WriteLine(GL.GetShaderInfoLog(shader));
                return -1;
            }

            return shader;
        }
    }
}
