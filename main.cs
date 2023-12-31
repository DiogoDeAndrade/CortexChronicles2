﻿using OpenTK.Mathematics;
using OpenTKBase;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SDLBase
{
    public static class OpenTKProgram
    {
        public static void Main()
        {
            OpenTKApp app = new OpenTKApp(1920, 1080, "Cortex Chronicles 2", true);

            app.Initialize();
            //app.LockMouse(true);

            RunGame(app);

            app.Shutdown();
        }

        static RPCortex renderPipeline;

        static void RunGame(OpenTKApp app)
        {
            app.Run(
            () =>
            {
                // Load resources
                Resources.LoadFont("Fonts/simple_ascii.png", 5, 7, true, 1, 1, 2, 2);
                Resources.CreateSpriteForPixelArt("bike", "Textures/motorbike.png", new Vector2(0.5f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), 90);
                Resources.CreateSpriteForPixelArt("bike_accel", "Textures/motorbike_accel.png", new Vector2(0.5f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), 90);
                Resources.CreateSpriteForPixelArt("bike_break", "Textures/motorbike_break.png", new Vector2(0.5f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), 90);
                Resources.CreateSpriteForPixelArt("bike_shadow", "Textures/shadow.png", new Vector2(0.5f, 0.5f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), 90);
                Resources.CreateSpriteForPixelArt("dot", "Textures/dot.png", new Vector2(0.5f, 0.5f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), 90);
                Resources.CreateSpriteSheetForPixelArt("explosion", "Textures/explosion.png", new Vector2(0.5f, 0.5f), 4, 4, 16, 90);
                Resources.CreateSpriteForPixelArt("Drone", "Textures/drone.png", new Vector2(0.5f, 0.5f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), 150);
                Resources.CreateSpriteForPixelArt("DroneShadow", "Textures/drone_shadow.png", new Vector2(0.5f, 0.5f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), 90);
                Resources.CreateSpriteForPixelArt("DroneTarget", "Textures/drone_target.png", new Vector2(0.5f, 0.5f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), 90);

                Resources.CreateSpriteForPixelArt("GameOver", "Textures/gameover.png", new Vector2(0.5f, 0.5f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), 90);
                Resources.CreateSpriteForPixelArt("HealthbarFull", "Textures/health_cell_full.png", new Vector2(0.0f, 0.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), 20);
                Resources.CreateSpriteForPixelArt("HealthbarEmpty", "Textures/health_cell_empty.png", new Vector2(0.0f, 0.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), 20);

                Resources.CreateSpriteForPixelArt("TransparentBlack", "Textures/transparent_black.png", new Vector2(0.5f, 0.5f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), 1);
                Resources.CreateSpriteForPixelArt("Arrow", "Textures/arrow.png", new Vector2(0.5f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), 1);
                Resources.CreateSpriteForPixelArt("CircleMarker", "Textures/circle_marker.png", new Vector2(0.5f, 0.5f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), 1);

                Resources.CreateSpriteForPixelArt("Title", "Textures/title.png", new Vector2(0.5f, 0.5f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), 1);

                // Create map
                GameObject gameMapObject = new GameObject();
                GameMap gameMap = gameMapObject.AddComponent<GameMap>();
                gameMap.LoadMap("Data/game_map01.dat");
                gameMap.BuildGeometry();

                // Get coordinates of special places
                var startPos = gameMap.GetPosition('S');

                // Setup environment
                var env = OpenTKApp.APP.mainScene.environment;

                env.Set("FogParams", new Vector2(0.0f, 80.0f));
                env.Set("FogColor", Color.Black);

                // Create player character
                GameObject playerCharacter = new GameObject();
                playerCharacter.transform.position = startPos;
                playerCharacter.transform.rotation = Quaternion.FromEulerAngles(0.0f, -MathF.PI / 2.0f, 0.0f);
                SpriteRenderer sr = playerCharacter.AddComponent<SpriteRenderer>();
                sr.sprite = Resources.FindSprite("bike");
                var pb = playerCharacter.AddComponent<PlayerBike>();
                pb.gameMap = gameMap;

                // Create camera
                GameObject cameraObject = new GameObject();
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.transform.position = startPos + Vector3.UnitY * 2.5f - Vector3.UnitX * 5;
                camera.transform.rotation = Quaternion.FromEulerAngles(0.0f, -MathF.PI / 2.0f, 0.0f);
                camera.ortographic = false;
                var track = cameraObject.AddComponent<CameraTrack>();
                track.player = pb;
                track.gameMap = gameMap;

                GameObject uiObject = new GameObject();
                var ui = uiObject.AddComponent<UI>();

                // Create pipeline
                renderPipeline = new RPCortex();

                ui.GotoTitle();
            },
            () =>
            {
                app.Render(renderPipeline);
            });
        }
    }
}
