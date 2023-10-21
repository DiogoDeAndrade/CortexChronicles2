using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using static OpenTKBase.SpriteRenderer;

namespace OpenTKBase
{
    public class UI : Renderable
    {
        public enum State { None, GameOver };

        public State state = State.None;

        private Material    ssmaterial;

        public override void Update()
        {
            if (state == State.GameOver)
            {
                if (Input.GetKeyDown(Keys.Space))
                {
                    OpenTKApp.APP.Restart();
                }
            }
        }

        public override void Render(Camera camera, Material material)
        {
            if (state == State.GameOver)
            {
                RenderGameOver();
            }
        }

        public override int GetOrder() { return 1; }

        public void RenderGameOver()
        {
            GL.Disable(EnableCap.DepthTest);

            Sprite gameOver = Resources.FindSprite("GameOver");

            DrawScreenspace(gameOver, (int)(OpenTKApp.APP.resX * 0.5f), (int)(OpenTKApp.APP.resY * 0.5f));

            GL.Enable(EnableCap.DepthTest);
        }

        private void DrawScreenspace(Sprite sprite, int x, int y)
        { 
            if (ssmaterial == null)
            {

                ssmaterial = new Material(Shader.Find("Shaders/screenspace"));                
                ssmaterial.cullMode = CullFaceMode.FrontAndBack;
            }
            ssmaterial.Set("Albedo", sprite.texture);
            ssmaterial.Set("WorldPos", new Vector2(x, y));
            ssmaterial.Set("WorldScale", new Vector2(sprite.pixelsPerUnit * 5.0f, sprite.pixelsPerUnit * 5.0f));

            Shader.SetMatrix(Shader.MatrixType.World, transform.localToWorldMatrix);

            Mesh mesh = sprite.GetMesh();

            mesh.Render(ssmaterial);

        }
    }
}
