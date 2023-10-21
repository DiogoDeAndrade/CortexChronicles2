using OpenTK.Mathematics;
using System.Collections.Generic;

namespace OpenTKBase
{
    public class Sprite
    {
        public Texture  texture;
        public Vector2  hotspot;
        public Vector4  uvRect;
        public float    pixelsPerUnit = 100.0f;

        private Mesh        mesh;
        private Material    material;

        public Mesh GetMesh()
        {
            if (mesh == null)
            {
                float width = texture.width * (uvRect.Z - uvRect.X) / pixelsPerUnit;
                float height = texture.height * (uvRect.W - uvRect.Y) / pixelsPerUnit;

                float x = -hotspot.X * width;
                float y = hotspot.Y * height;

                mesh = new Mesh();
                mesh.SetVertices(new List<Vector3> { new Vector3(x, y - height, 0.0f), new Vector3(x, y, 0.0f), new Vector3(x + width, y, 0.0f), new Vector3(x + width, y - height, 0.0f) });
                mesh.SetNormals(new List<Vector3> { -Vector3.UnitZ, -Vector3.UnitZ, -Vector3.UnitZ, -Vector3.UnitZ });
                mesh.SetUVs(new List<Vector2> { new Vector2(uvRect.X, uvRect.W), new Vector2(uvRect.X, uvRect.Y), new Vector2(uvRect.Z, uvRect.Y), new Vector2(uvRect.Z, uvRect.W) });
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
            }

            return material;
        }
    }
}
