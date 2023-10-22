using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using static OpenTKBase.SpriteRenderer;

namespace OpenTKBase
{
    public class UI : Renderable
    {
        public enum State { None, Game, Map, GameOver };

        public State state = State.Game;

        private Material    ssmaterial;
        private PlayerBike  player;
        private Font        font;
        private GameMap     gameMap;

        public override void Start()
        {
            player = FindObjectOfType<PlayerBike>();
            font = Resources.FindFont("Fonts/simple_ascii.png");
            gameMap = FindObjectOfType<GameMap>();
        }

        public override void Update()
        {
            if (state == State.Game)
            {
                if (Input.GetKeyDown(Keys.M))
                {
                    // Open map
                    player.enable = false;
                    state = State.Map;
                }
            }
            else if (state == State.Map)
            {
                if (Input.GetKeyDown(Keys.M))
                {
                    // Open map
                    player.enable = true;
                    state = State.Game;
                }
            }
            else if (state == State.GameOver)
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
            GL.Disable(EnableCap.DepthTest);

            if (state == State.Game)
            {
                RenderGame();
            }
            else if (state == State.Map)
            {
                RenderMap();
            }
            else if (state == State.GameOver)
            {
                RenderGameOver();
            }

            GL.Enable(EnableCap.DepthTest);
        }

        public override int GetOrder() { return 1; }

        public void RenderMap()
        {
            int mx = (int)(OpenTKApp.APP.resX * 0.5f);
            int my = (int)(OpenTKApp.APP.resY * 0.5f);

            DrawScreenspace(Resources.FindSprite("TransparentBlack"), mx, my, 10.0f);

            var mapSprite = Resources.FindSprite("Map");
            font.RenderSingleMonospace(mx, my - 400, 50, 50, 4, "MAP", Color4.Yellow, Font.Align.CenterX);
            DrawScreenspace(mapSprite, mx, my, 10, 10.0f);

            //////////////////////////////////
            // Draw player 
            var pos = player.transform.position;
            // Convert to tiles
            var tilePos = gameMap.WorldToTile(pos);
            float px = mx - mapSprite.texture.width * mapSprite.pixelsPerUnit * 10.0f * 0.5f + tilePos.X * 10.0f;
            float py = my - mapSprite.texture.height * mapSprite.pixelsPerUnit * 10.0f * 0.5f + (tilePos.Y + 0.5f) * 10.0f;
            // Get angle
            var d = player.transform.forward.Xz;
            var a = MathF.Atan2(-d.Y, d.X);

            DrawScreenspace(Resources.FindSprite("Arrow"), (int)px, (int)py, 1, 1, a - MathF.PI * 0.5f);
        }

        public void RenderGame()
        {
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
        }

        public void RenderGameOver()
        {
            int mx = (int)(OpenTKApp.APP.resX * 0.5f);
            int my = (int)(OpenTKApp.APP.resY * 0.5f);

            Sprite gameOver = Resources.FindSprite("GameOver");

            DrawScreenspace(gameOver, mx, my, 5.0f);

            font.RenderSingleMonospace(mx, my + 100, 50, 50, 4, "PRESS [SPACE] TO CONTINUE", Color4.Yellow, Font.Align.CenterX);
        }

        private void DrawScreenspace(Sprite sprite, int x, int y, float scale)
        {
            DrawScreenspace(sprite, x, y, scale, scale);
        }

        private void DrawScreenspace(Sprite sprite, int x, int y, float scaleX, float scaleY, float rotation = 0)
        { 
            if (ssmaterial == null)
            {

                ssmaterial = new Material(Shader.Find("Shaders/screenspace"));                
                ssmaterial.cullMode = CullFaceMode.FrontAndBack;
            }
            ssmaterial.Set("Albedo", sprite.texture);
            ssmaterial.Set("WorldPos", new Vector2(x, y));
            ssmaterial.Set("WorldScale", new Vector2(sprite.pixelsPerUnit * scaleX, -sprite.pixelsPerUnit * scaleY));
            ssmaterial.Set("WorldRotation", rotation);

            Shader.SetMatrix(Shader.MatrixType.World, transform.localToWorldMatrix);

            Mesh mesh = sprite.GetMesh();

            mesh.Render(ssmaterial);

        }
    }
}
