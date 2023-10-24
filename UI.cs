using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using static OpenTKBase.SpriteRenderer;
using static System.Net.Mime.MediaTypeNames;

namespace OpenTKBase
{
    public class UI : Renderable
    {
        public enum State { None, Game, Map, Cutscene, GameOver };

        public State state = State.Game;

        private Material ssmaterial;
        private PlayerBike player;
        private Font font;
        private GameMap gameMap;
        private Mesh rectMesh;
        private Material rectMaterial;

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
                if ((Input.GetKeyDown(Keys.T)) && (Input.GetKey(Keys.LeftShift)) && (Input.GetKey(Keys.LeftControl)))
                {
                    player.GotoPos('T');
                }

                var targetPos = gameMap.GetPosition('T');

                if (Vector3.Distance(player.transform.position, targetPos) < 5.0f)
                {
                    // Completed mission
                    PlayCutscene(nextCutscene);
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
            else if (state == State.Cutscene)
            {
                UpdateCutscene();
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
            else if (state == State.Cutscene)
            {
                RenderCutscene();
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

            float s = 1.25f + 0.25f * MathF.Sin(Time.time * 4.0f);
            DrawScreenspace(Resources.FindSprite("Arrow"), (int)px, (int)py, s, s, a - MathF.PI * 0.5f);

            // Draw target
            pos = gameMap.GetPosition('T');
            // Convert to tiles
            tilePos = gameMap.WorldToTile(pos);
            px = mx - mapSprite.texture.width * mapSprite.pixelsPerUnit * 10.0f * 0.5f + tilePos.X * 10.0f;
            py = my - mapSprite.texture.height * mapSprite.pixelsPerUnit * 10.0f * 0.5f + (tilePos.Y + 0.5f) * 10.0f;

            s = 1.5f + 0.5f * MathF.Sin(Time.time * 8.0f);
            DrawScreenspace(Resources.FindSprite("CircleMarker"), (int)px, (int)py, s, s, 0.0f);
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

        private void DrawRect(int x1, int y1, int x2, int y2, Color4 color)
        {
            if (rectMesh == null)
            {
                rectMesh = new Mesh();
                rectMesh.SetUVs(new List<Vector2>
                {
                    new Vector2(0.0f, 0.0f),
                    new Vector2(1.0f, 0.0f),
                    new Vector2(1.0f, 1.0f),
                    new Vector2(0.0f, 1.0f)
                });

                rectMesh.SetIndices(new List<uint> { 0, 1, 2, 0, 2, 3 });
            }

            if (rectMaterial == null)
            {
                rectMaterial = new Material(Shader.Find("Shaders/screenspacerect"));
                rectMaterial.cullMode = CullFaceMode.FrontAndBack;
            }

            rectMaterial.Set("Color", color);
            rectMesh.SetVertices(new List<Vector3>
            {
                new Vector3(x1, y1, 0.0f),
                new Vector3(x2, y1, 0.0f),
                new Vector3(x2, y2, 0.0f),
                new Vector3(x1, y2, 0.0f),
            });

            rectMesh.Render(rectMaterial);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // CUTSCENE STUFF
        struct CutsceneElem
        {
            public enum Type { Background, WaitTime, WaitKey, Talk, ChangeMap, Quit };

            public Type     type;
            public string   textData;
            public float    floatData;
            public Speaker  speaker;
        }
        class Speaker
        {
            public string name;
            public Color4 nameColor;
            public Color4 textColor;
        }
        private Dictionary<string, Speaker> speakers = new Dictionary<string, Speaker>();
        private List<CutsceneElem>          cutsceneElems;
        private int                         cutsceneIndex;
        private float                       cutsceneTimer;
        private string                      nextCutscene;

        public void PlayCutscene(string cutsceneFile)
        {
            player = FindObjectOfType<PlayerBike>();
            player.enable = false;
            state = State.Cutscene;

            // Parse file
            cutsceneElems = new List<CutsceneElem>();
            cutsceneIndex = 0;

            char[] delimiters = new char[] { ' ', '\t' };
            List<string> textMapData = new List<string>();

            using (StreamReader reader = new StreamReader(cutsceneFile))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var l = line.Trim();
                    if (l == "") continue;
                    if (l[0] == '#') continue;
                    var tokens = line.Split(delimiters);
                    if (tokens.Length > 0)
                    {
                        switch (tokens[0].ToUpper())
                        {
                            case "DEFSPEAKER":
                                {
                                    string name = "";
                                    for (int i = 4; i < tokens.Length; i++) { name += ((i > 4) ? (" ") : ("")) + tokens[i]; }
                                    speakers[tokens[1]] = new Speaker
                                    {
                                        name = name,
                                        nameColor = GetColorByName(tokens[2]),
                                        textColor = GetColorByName(tokens[3])
                                    };
                                }
                                break;
                            case "BACKGROUND":
                                {
                                    cutsceneElems.Add(new CutsceneElem
                                    {
                                        type = CutsceneElem.Type.Background,
                                        textData = (tokens.Length > 1) ? (tokens[1]) : ("")
                                    });
                                }
                                break;
                            case "WAITTIME":
                                {
                                    cutsceneElems.Add(new CutsceneElem
                                    {
                                        type = CutsceneElem.Type.WaitTime,
                                        floatData = float.Parse(tokens[1])
                                    });
                                }
                                break;
                            case "WAITKEY":
                                {
                                    cutsceneElems.Add(new CutsceneElem
                                    {
                                        type = CutsceneElem.Type.WaitKey,
                                    });
                                }
                                break;
                            case "TALK":
                                {
                                    string text = "";
                                    for (int i = 2; i < tokens.Length; i++) { text += ((i > 2) ? (" ") : ("")) + tokens[i]; }
                                    cutsceneElems.Add(new CutsceneElem
                                    {
                                        type = CutsceneElem.Type.Talk,
                                        textData = text,
                                        speaker = speakers[tokens[1]]
                                    });
                                }
                                break;
                            case "NARRATE":
                                {
                                    string text = "";
                                    for (int i = 1; i < tokens.Length; i++) { text += ((i > 1) ? (" ") : ("")) + tokens[i]; }
                                    cutsceneElems.Add(new CutsceneElem
                                    {
                                        type = CutsceneElem.Type.Talk,
                                        textData = text,
                                        speaker = null
                                    });
                                }
                                break;
                            case "NEXTCUTSCENE":
                                {
                                    nextCutscene = tokens[1];
                                }
                                break;
                            case "CHANGEMAP":
                                cutsceneElems.Add(new CutsceneElem
                                {
                                    type = CutsceneElem.Type.ChangeMap,
                                    textData = (tokens.Length > 1) ? (tokens[1]) : ("")
                                });
                                break;
                            case "ENDGAME":
                                cutsceneElems.Add(new CutsceneElem
                                {
                                    type = CutsceneElem.Type.Quit,
                                });
                                break;
                        }
                    }
                }
            }

            CutsceneNextStep();
        }

        void UpdateCutscene()
        {
            if (cutsceneIndex >= cutsceneElems.Count) return;

            var elem = cutsceneElems[cutsceneIndex];
            switch (elem.type)
            {
                case CutsceneElem.Type.WaitTime:
                    cutsceneTimer -= Time.deltaTime;
                    if (cutsceneTimer <= 0)
                    {
                        cutsceneIndex++;
                        CutsceneNextStep();
                    }
                    break;
                case CutsceneElem.Type.Talk:
                case CutsceneElem.Type.WaitKey:
                    if (Input.GetKeyDown(Keys.Space))
                    {
                        cutsceneIndex++;
                        CutsceneNextStep();
                    }
                    break;
                default:
                    break;
            }
        }

        void CutsceneNextStep()
        {
            while (cutsceneIndex < cutsceneElems.Count)
            {
                var elem = cutsceneElems[cutsceneIndex];

                switch (elem.type)
                {
                    case CutsceneElem.Type.Background:
                        // Remove texture/sprite
                        Resources.RemoveTexture("BackTexture");
                        Resources.RemoveSprite("BackSprite");
                        if (elem.textData != "")
                        {
                            var texture = Resources.LoadTexture("BackTexture", elem.textData, TextureWrapMode.Clamp, TextureMinFilter.Nearest, false);
                            Resources.CreateSprite("BackSprite", texture, new Vector2(0.5f, 0.5f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), 1.0f);
                        }
                        break;
                    case CutsceneElem.Type.WaitTime:
                        // Setup a timer
                        cutsceneTimer = elem.floatData;
                        return;
                    case CutsceneElem.Type.WaitKey:
                    case CutsceneElem.Type.Talk:
                        return;
                    case CutsceneElem.Type.ChangeMap:
                        gameMap.LoadMap(elem.textData);
                        player.GotoPos('S');                        
                        break;
                    case CutsceneElem.Type.Quit:
                        OpenTKApp.APP.Quit();
                        return;
                    default:
                        break;
                }

                cutsceneIndex++;
            }

            // Cutscene is done
            state = State.Game;
            player.enable = true;
        }

        public void RenderCutscene()
        {
            int mx = (int)(OpenTKApp.APP.resX * 0.5f);
            int my = (int)(OpenTKApp.APP.resY * 0.5f);

            var sprite = Resources.FindSprite("BackSprite");
            if (sprite != null)
            {
                DrawScreenspace(sprite, mx, my, 6.0f);
            }

            if (cutsceneIndex < cutsceneElems.Count)
            {
                var elem = cutsceneElems[cutsceneIndex];
                if (elem.type == CutsceneElem.Type.Talk)
                {
                    int x1 = 50;
                    int y1 = 600;
                    int x2 = 1920 - x1;
                    int y2 = 1000;
                    int border = 2;
                    var nameColor = (elem.speaker != null)?(elem.speaker.nameColor):(GetColorByName("WHITE"));
                    nameColor.A = 128;
                    var textColor = (elem.speaker != null) ? (elem.speaker.textColor) : (GetColorByName("GREY"));
                    DrawRect(x1, y1, x2, y1 + border, nameColor);
                    DrawRect(x1, y1, x1 + border, y2, nameColor);
                    DrawRect(x2 - border, y1, x2, y2, nameColor);
                    DrawRect(x1, y2 - border, x2, y2, nameColor);
                    DrawRect(x1 + border, y1 + border, x2 - border, y2 - border, new Color4(0.0f, 0.0f, 0.0f, 0.80f));

                    if (elem.speaker != null)
                    {
                        font.RenderSingleMonospace(x1 + border + 20, y1 + border + 15, 50, 50, 4, elem.speaker.name, elem.speaker.nameColor, Font.Align.Left);
                    }

                    font.RenderMonospace(x1 + border + 20, y1 + border + 120, x2 - x1 - (border + 20) * 2, 36, 36, 4, 20, elem.textData, textColor, Font.Align.Left);
                }
            }
        }

        public Color4 GetColorByName(string name)
        {
            switch (name.ToUpper())
            {
                case "BLACK": return Color4.Black;
                case "DARKBLUE": return HexToRGB("0000d8");
                case "BLUE": return HexToRGB("0000ff");
                case "DARKRED": return HexToRGB("d80000");
                case "RED": return HexToRGB("ff0000");
                case "DARKMAGENTA": return HexToRGB("d800d8");
                case "MAGENTA": return HexToRGB("ff00ff");
                case "DARKGREEN": return HexToRGB("00d800");
                case "GREEN": return HexToRGB("00ff00");
                case "DARKCYAN": return HexToRGB("00d8d8");
                case "CYAN": return HexToRGB("00ffff");
                case "DARKYELLOW": return HexToRGB("d8d800");
                case "YELLOW": return HexToRGB("ffff00");
                case "GRAY": return HexToRGB("d8d8d8");
                case "GREY": return HexToRGB("d8d8d8");
                case "WHITE": return HexToRGB("ffffff");
                default:
                    break;
            }
            return Color4.Red;
        }

        public static Color4 HexToRGB(string hex)
        {
            // Ensure the hex string has either 6 or 7 characters (#FFFFFF or FFFFFF format)
            if (hex.Length < 6 || hex.Length > 7 || (hex.Length == 7 && hex[0] != '#'))
                throw new ArgumentException("Invalid hex color value.");

            // If the hex string includes the '#' symbol, remove it.
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1); // remove #
            }

            // Convert the characters to the RGB values
            int r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            return new Color4(r, g, b, 255);
        }

    }
}
