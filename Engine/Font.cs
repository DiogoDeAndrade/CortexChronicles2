using OpenTK.Mathematics;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using System.Reflection.Metadata;
using System;

namespace OpenTKBase
{
    public class Font
    {
        public enum Align { Left, CenterX, Right };

        private Texture     texture;
        private Vector4[]   rect;
        private Material    material;
        private Mesh        mesh;

        public Font()
        {
        }

        ~Font()
        {
            Clear();
        }

        public void Clear()
        {
            if (texture != null)
            {
                texture.Clear();
                texture = null;
            }
        }

        public bool Load(string fontName, int gridX, int gridY, bool gridSize, int startX = 0, int startY = 0, int spacingX = 0, int spacingY = 0)
        {
            texture = new Texture(TextureWrapMode.Clamp, TextureMinFilter.Nearest, false);
            if (!texture.Load(fontName)) return false;

            // Create ascii character rectangles
            if (gridSize)
            {
                int px = startX;
                int py = startY;
                float w = 1.0f / texture.width;
                float h = 1.0f / texture.height;
                float sx = gridX * w;
                float sy = gridY * h;

                rect = new Vector4[256];
                for (int characterId = 32; characterId < 256; characterId++)
                {
                    rect[characterId].X = px * w;
                    rect[characterId].Y = py * h;
                    rect[characterId].Z = rect[characterId].X + sx;
                    rect[characterId].W = rect[characterId].Y + sy;

                    px += gridX + spacingX;
                    if ((px + gridX) >= texture.width)
                    {
                        px = startX;
                        py += gridY + spacingY;
                        if ((py + gridY) >= texture.height) break;
                    }
                }
            }
            else
            {
                float sx = 1.0f / gridX;
                float sy = 1.0f / gridY;

                rect = new Vector4[256];
                for (int y = 0; y < gridY; y++)
                {
                    for (int x = 0; x < gridX; x++)
                    {
                        int characterId = (x + y * gridX) + 32;
                        rect[characterId].X = x * sx;
                        rect[characterId].Y = y * sy;
                        rect[characterId].Z = rect[characterId].X + sx;
                        rect[characterId].W = rect[characterId].Y + sy;
                    }
                }
            }

            material = new Material(Shader.Find("Shaders/font"));
            material.cullMode = CullFaceMode.FrontAndBack;
            material.Set("Albedo", texture);

            mesh = new Mesh();

            return true;
        }

        public Vector2 GetSizeSingleMonospace(int charWidth, int charHeight, int spacingX, int spacingY, string text)
        {
            return new Vector2((charWidth + spacingX) * text.Length, (charHeight + spacingY));
        }

        internal void RenderSingleMonospace(int x, int y, int charWidth, int charHeight, int spacingX, string text, Color4 color, Align align = Align.Left)
        {
            List<Vector3>   vertex = new List<Vector3>();
            List<Vector2>   uvs = new List<Vector2>();
            List<Color4>    colors = new List<Color4>();
            List<uint>      indices = new List<uint>();

            var     textSize = GetSizeSingleMonospace(charWidth, charHeight, spacingX, 0, text);

            float   px = x;
            if (align == Align.CenterX) { px = x - textSize.X * 0.5f; }
            else if (align == Align.Right) { px = x - textSize.X; }

            float   py = y;
            uint    index = (uint)vertex.Count;

            for (int i = 0; i < text.Length; i++)
            {
                uint c = (uint)text[i];
                var  r = rect[c];

                vertex.Add(new Vector3(px, py, 0)); uvs.Add(new Vector2(r.X, r.Y)); colors.Add(color);
                vertex.Add(new Vector3(px + charWidth, py, 0)); uvs.Add(new Vector2(r.Z, r.Y)); colors.Add(color);
                vertex.Add(new Vector3(px + charWidth, py + charHeight, 0)); uvs.Add(new Vector2(r.Z, r.W)); colors.Add(color);
                vertex.Add(new Vector3(px, py + charHeight, 0)); uvs.Add(new Vector2(r.X, r.W)); colors.Add(color);

                indices.Add(index); indices.Add(index + 1); indices.Add(index + 2);
                indices.Add(index); indices.Add(index + 2); indices.Add(index + 3);

                index += 4;
                px += charWidth + spacingX;
            }

            mesh.SetVertices(vertex);
            mesh.SetColors(colors);
            mesh.SetUVs(uvs);
            mesh.SetIndices(indices);
            mesh.Render(material);
        }
    }
}
