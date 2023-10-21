namespace OpenTKBase
{
    public abstract class Renderable : Component
    {
        public abstract void Render(Camera camera, Material material);
        public virtual int GetOrder() { return 0; }
    }
}
