namespace OpenTKBase
{
    public class SpriteRenderer : Renderable
    {
        public Sprite sprite;

        public override void Render(Camera camera, Material override_material)
        {
            var activeMaterial = (override_material == null) ? (sprite.GetMaterial()) : (override_material);

            Shader.SetMatrix(Shader.MatrixType.World, transform.localToWorldMatrix);

            Mesh mesh = sprite.GetMesh();

            mesh.Render(activeMaterial);

        }
    }
}
