using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace WoTMapWPF.Graphics
{
    public sealed class Map : IDrawable, IDisposable
    {
        public int TextureID;
        public int Vao;
        public int IndicesCount;
        public readonly string TexturePath;
        private readonly List<int> buffers = new List<int>();

        public Map()
        {
            TexturePath = "../Res/no_map.png";
            Uri textureUri = new Uri(TexturePath, UriKind.Relative);
            TextureID = TextureLoader.LoadTexture(textureUri, out int wp, out int hp);
            AspectRatio = (float)wp / (float)hp;
            HeightP = hp;
            WidthP = wp;
            Init();
        }

        public Map(string texPath)
        {
            TexturePath = texPath;
            TextureID = TextureLoader.LoadTexture(texPath, out int wp, out int hp);
            AspectRatio = (float)wp / (float)hp;
            HeightP = hp;
            WidthP = wp;
            Init();
        }

        public void Dispose()
        {
            if (buffers.Count > 0)
            {
                foreach (int buffer in buffers)
                    GL.DeleteBuffer(buffer);
                GL.DeleteVertexArray(Vao);
            }
            TextureLoader.DeleteTexture(TexturePath);
        }

        public void Draw(Scene scene)
        {
            Matrix4 modelView = Matrix4.Identity;
            GL.UniformMatrix4(scene.ModelMatrixULoc, false, ref modelView);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, TextureID);
            GL.BindVertexArray(Vao);
            GL.DrawElements(PrimitiveType.Triangles, IndicesCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public float AspectRatio { get; private set; }
        public float HeightP { get; private set; }
        public float WidthP { get; private set; }

        private void Init()
        {
            float halfH = Scene.VERTICAL_UNITS / 2;
            float halfW = halfH * AspectRatio;
            Vector2[] vertices = new Vector2[]
               {
                new Vector2(-halfW, -halfH),
                new Vector2(halfW, -halfH),
                new Vector2(halfW, halfH),
                new Vector2(-halfW, halfH)
               };
            Vector2[] uv = new Vector2[]
            {
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(0, 0)
            };
            int[] indices = new int[] {
                0, 1, 2,
                0, 2, 3
            };
            IndicesCount = indices.Length;

            GL.GenBuffers(1, out int posVbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, posVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, Vector2.SizeInBytes * vertices.Length, vertices, BufferUsageHint.StaticDraw);
            GL.GenBuffers(1, out int uvVbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, uvVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, Vector2.SizeInBytes * uv.Length, uv, BufferUsageHint.StaticDraw);
            GL.GenBuffers(1, out int ebo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(int) * indices.Length, indices, BufferUsageHint.StaticDraw);

            GL.GenVertexArrays(1, out Vao);
            GL.BindVertexArray(Vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, posVbo);
            GL.EnableVertexAttribArray((int)ShaderAttribute.Position);
            GL.VertexAttribPointer((int)ShaderAttribute.Position, 2, VertexAttribPointerType.Float, true, Vector2.SizeInBytes, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, uvVbo);
            GL.EnableVertexAttribArray((int)ShaderAttribute.UV);
            GL.VertexAttribPointer((int)ShaderAttribute.UV, 2, VertexAttribPointerType.Float, true, Vector2.SizeInBytes, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            buffers.Add(posVbo);
            buffers.Add(uvVbo);
            buffers.Add(ebo);
        }
    }
}
