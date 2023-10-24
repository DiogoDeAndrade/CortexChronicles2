using OpenTK.Mathematics;
using System;

namespace OpenTKBase
{
    public class SpriteRenderer : Renderable
    {
        public enum Mode { Billboard, BillboardXZ, World };
        public Sprite   sprite;
        public Mode     mode = Mode.Billboard;
        public float    rotation;
        public Color4   color = Color4.White;

        public override void Render(Camera camera, Material override_material)
        {
            var activeMaterial = (override_material == null) ? (sprite.GetMaterial()) : (override_material);

            Shader.SetMatrix(Shader.MatrixType.World, transform.localToWorldMatrix);

            activeMaterial.Set("SpriteRotation", rotation * MathF.PI / 180.0f);
            activeMaterial.Set("Color", color);


            if ((camera != null) && (mode != Mode.World))
            {
                if (mode == Mode.Billboard)
                {
                    activeMaterial.Set("SpriteRight", camera.transform.right);
                    activeMaterial.Set("SpriteUp", camera.transform.up);
                }
                else
                {
                    activeMaterial.Set("SpriteRight", camera.transform.right);
                    activeMaterial.Set("SpriteUp", camera.transform.forward);
                }
            }
            else
            {
                activeMaterial.Set("SpriteRight", Vector3.UnitX);
                activeMaterial.Set("SpriteUp", Vector3.UnitY);
            }

            Mesh mesh = sprite.GetMesh();

            mesh.Render(activeMaterial);
        }
    }
}
