using System;

namespace OpenTKBase
{
    public class SpriteRenderer : Renderable
    {
        public Sprite   sprite;
        public float    rotation;

        public override void Render(Camera camera, Material override_material)
        {
            var activeMaterial = (override_material == null) ? (sprite.GetMaterial()) : (override_material);

            Shader.SetMatrix(Shader.MatrixType.World, transform.localToWorldMatrix);

            activeMaterial.Set("SpriteRotation", rotation * MathF.PI / 180.0f);
            activeMaterial.Set("BillboardSprite", sprite.billboardSprite);

            Mesh mesh = sprite.GetMesh();

            mesh.Render(activeMaterial);

        }
    }
}
