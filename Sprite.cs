using OpenTK.Mathematics;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace OpenTKBase
{
    public class Sprite
    {
        public Texture  texture;
        public Vector2  hotspot;
        public Vector4  uvRect;
        public float    pixelsPerUnit = 100.0f;
        public bool     billboardSprite = true;

        private Mesh        mesh;
        private Material    material;

        public Mesh GetMesh()
        {
            if (mesh == null)
            {
                Vector3 axisX = Vector3.UnitX;
                Vector3 axisY = Vector3.UnitY;

                float width = texture.width * (uvRect.Z - uvRect.X) / pixelsPerUnit;
                float height = texture.height * (uvRect.W - uvRect.Y) / pixelsPerUnit;

                float x = -hotspot.X * width;
                float y = hotspot.Y * height;

                float u1 = uvRect.X;
                float v1 = uvRect.Y;
                float u2 = uvRect.Z;
                float v2 = uvRect.W;

                if (!billboardSprite)
                {
                    axisY = Vector3.UnitZ;
                    v1 = uvRect.W;
                    v2 = uvRect.Y;
                }

                mesh = new Mesh();
                mesh.SetVertices(new List<Vector3> {
                    axisX * x + axisY * (y - height),
                    axisX * x + axisY * y,
                    axisX * (x + width) + axisY * y,
                    axisX * (x + width) + axisY * (y - height)
                });
                mesh.SetNormals(new List<Vector3> { -Vector3.UnitZ, -Vector3.UnitZ, -Vector3.UnitZ, -Vector3.UnitZ });
                mesh.SetUVs(new List<Vector2> { new Vector2(u1, v2), new Vector2(u1, v1), new Vector2(u2, v1), new Vector2(u2, v2) });
                mesh.SetIndices(new List<uint> { 0, 2, 1, 0, 3, 2 });
            }

            return mesh;
        }

        public Material GetMaterial()
        {
            if (material == null)
            {
                material = new Material(Shader.Find("Shaders/sprite"));
                material.Set("Albedo", texture);
                if (!billboardSprite) material.cullMode = CullFaceMode.Front;
            }

            return material;
        }
    }
}
