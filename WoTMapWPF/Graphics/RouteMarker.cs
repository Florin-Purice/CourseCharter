using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace WoTMapWPF.Graphics
{
    internal class RouteMarker : IDrawable, IDisposable
    {
        public int TextureID;
        public readonly int Vao;
        public readonly int IndicesCount;
        private readonly List<int> buffers = [];
        private readonly MarkerType markerType;

        public RouteMarker(MarkerType markerType)
        {
            Uri? textureUri = null;
            this.markerType = markerType;
            switch (markerType)
            {
                case MarkerType.Normal:
                    textureUri = new Uri("../Res/marker.png", UriKind.Relative);
                    break;
                case MarkerType.NormalSelected:
                    textureUri = new Uri("../Res/marker_selected.png", UriKind.Relative);
                    break;
                case MarkerType.End:
                    textureUri = new Uri("../Res/end_marker.png", UriKind.Relative);
                    break;
                case MarkerType.EndSelected:
                    textureUri = new Uri("../Res/end_marker_selected.png", UriKind.Relative);
                    break;
            }
            TextureID = TextureLoader.LoadTexture(textureUri);

            PosX = 0; PosY = 0;
            float width = 1f;
            float height = 1f;
            Vector2[] vertices = [
                new Vector2(-width/2, -height/2),
                new Vector2(width/2, -height/2),
                new Vector2(width/2, height/2),
                new Vector2(-width/2, height/2)];
            Vector2[] uv = [
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(0, 0)];
            int[] indices = [
                0, 1, 2,
                0, 2, 3];
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

        public void Dispose()
        {
            if (buffers.Count > 0)
            {
                foreach (int buffer in buffers)
                    GL.DeleteBuffer(buffer);
                GL.DeleteVertexArray(Vao);
            }
        }

        public void Draw(Scene scene)
        {
            Color pinColor;
            if (markerType == MarkerType.Normal || markerType == MarkerType.End)
                pinColor = ((SolidColorBrush)Application.Current.Resources["PinColor"]).Color;
            else
                pinColor = ((SolidColorBrush)Application.Current.Resources["PinSelectedColor"]).Color;
            scene.ChangeShaderFixedColor(pinColor.R, pinColor.G, pinColor.B, pinColor.A);
            double pinSize = (double)App.Current.Resources["PinSize"];
            float scale = (float)pinSize / scene.PixelsPerUnit;
            Matrix4 modelView = Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(PosX, PosY, 0f);
            GL.UniformMatrix4(scene.ModelMatrixULoc, false, ref modelView);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, TextureID);
            GL.BindVertexArray(Vao);
            GL.DrawElements(PrimitiveType.Triangles, IndicesCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public float PosX { get; set; }
        public float PosY { get; set; }

        public enum MarkerType
        {
            Normal,
            NormalSelected,
            End,
            EndSelected
        }
    }
}
