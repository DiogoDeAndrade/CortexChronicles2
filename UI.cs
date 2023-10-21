using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using static OpenTKBase.SpriteRenderer;

namespace OpenTKBase
{
    public class UI : Renderable
    {
        public enum State { None, Game, GameOver };

        public State state = State.Game;

        private Material    ssmaterial;
        private PlayerBike  player;

        public override void Start()
        {
            player = FindObjectOfType<PlayerBike>();
        }

        public override void Update()
        {
            if (state == State.GameOver)
            {
                if (Input.GetKeyDown(Keys.Space))
                {
                    OpenTKApp.APP.Restart();
                    return;
                }
            }
        }

        public override void Render(Camera camera, Material material)
        {
            if (state == State.Game)
            {
                RenderGame();
            }
            else if (state == State.GameOver)
            {
                RenderGameOver();
            }
        }

        public override int GetOrder() { return 1; }

        public void RenderGame()
        {
            GL.Disable(EnableCap.DepthTest);

            Sprite empty = Resources.FindSprite("HealthbarEmpty");
            Sprite full = Resources.FindSprite("HealthbarFull");

            int N = 10;
            int filled_cell = (int)(player.healthPercentage * N);
            for (int i = 0; i < N; i++)
            {
                if (i < filled_cell)
                    DrawScreenspace(full, 10 + i * 55, 10, 4.0f);
                else
                    DrawScreenspace(empty, 10 + i * 55, 10, 4.0f);
            }

            GL.Enable(EnableCap.DepthTest);
        }
        public void RenderGameOver()
        {
            GL.Disable(EnableCap.DepthTest);

            Sprite gameOver = Resources.FindSprite("GameOver");

            DrawScreenspace(gameOver, (int)(OpenTKApp.APP.resX * 0.5f), (int)(OpenTKApp.APP.resY * 0.5f), 5.0f);

            GL.Enable(EnableCap.DepthTest);
        }

        private void DrawScreenspace(Sprite sprite, int x, int y, float scale)
        {
            DrawScreenspace(sprite, x, y, scale, scale);
        }

        private void DrawScreenspace(Sprite sprite, int x, int y, float scaleX, float scaleY)
        { 
            if (ssmaterial == null)
            {

                ssmaterial = new Material(Shader.Find("Shaders/screenspace"));                
                ssmaterial.cullMode = CullFaceMode.FrontAndBack;
            }
            ssmaterial.Set("Albedo", sprite.texture);
            ssmaterial.Set("WorldPos", new Vector2(x, y));
            ssmaterial.Set("WorldScale", new Vector2(sprite.pixelsPerUnit * scaleX, -sprite.pixelsPerUnit * scaleY));

            Shader.SetMatrix(Shader.MatrixType.World, transform.localToWorldMatrix);

            Mesh mesh = sprite.GetMesh();

            mesh.Render(ssmaterial);

        }
    }
}
