﻿using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace OpenTKBase
{
    public class PlayerBike : Component
    {
        private float           rightDir = 0.0f;
        private float           rightInc = 10.0f;
        private float           rightDamp = 0.75f;
        private Vector3         bikeSpeed = new Vector3(10.0f, 0.0f, 10.0f);
        private float           accelSpeed = 10.0f;
        private float           breakSpeed = 20.0f;
        private SpriteRenderer  spriteRenderer;

        public  GameMap         gameMap;

        public override void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.mode = SpriteRenderer.Mode.World;

            GameObject playerShadow = new GameObject();
            playerShadow.transform.SetParent(transform);
            playerShadow.transform.localPosition = Vector3.UnitY * 0.1f;
            playerShadow.transform.localRotation = Quaternion.FromEulerAngles(MathF.PI * 0.5f, 0.0f, 0.0f);
            var sr = playerShadow.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.FindSprite("bike_shadow");
            sr.mode = SpriteRenderer.Mode.World;
        }

        public override void Update()
        {
            rightDir *= rightDamp;
            if (Input.GetKey(Keys.Left)) rightDir -= rightInc * Time.deltaTime;
            if (Input.GetKey(Keys.Right)) rightDir += rightInc * Time.deltaTime;
            rightDir = MathF.Min(1, MathF.Max(-1.0f, rightDir));

            spriteRenderer.rotation = -20 * rightDir;

            if (Input.GetKey(Keys.Up)) bikeSpeed.Z = MathF.Max(10.0f, MathF.Min(bikeSpeed.Z + accelSpeed * Time.deltaTime, 100.0f));
            if (Input.GetKey(Keys.Down)) bikeSpeed.Z = MathF.Max(10.0f, MathF.Min(bikeSpeed.Z - breakSpeed * Time.deltaTime, 100.0f));

            if (Input.GetKey(Keys.Up))
                spriteRenderer.sprite = Resources.FindSprite("bike_accel");
            else if (Input.GetKey(Keys.Down))
                spriteRenderer.sprite = Resources.FindSprite("bike_break");
            else
                spriteRenderer.sprite = Resources.FindSprite("bike");

            Vector3 newPos = transform.position;
            Vector3 oldPos = newPos;

            newPos += transform.forward * bikeSpeed.Z * Time.deltaTime;

            transform.position = newPos;

            transform.rotation *= Quaternion.FromEulerAngles(0.0f, -4.0f * rightDir * Time.deltaTime, 0.0f);

            // Check collision
            Vector3 dir = (newPos - oldPos);
            if (dir.LengthSquared > 0)
            {
                float maxDist = dir.Length;
                dir /= maxDist;
                maxDist += 1.0f;
             
                float   dist = 0.0f;
                Vector3 normal = Vector3.Zero;
                Vector3 origin = oldPos + Vector3.UnitY * 1.0f;
                
                if (gameMap.Spherecast(origin, dir, 0.5f, maxDist, ref dist, ref normal))
                {
                    Vector3 reflectDir = dir.Reflect(normal);

                    if (Vector3.Dot(reflectDir, dir) > 0)
                    {
                        // Just ping out of it change direction, take some damage maybe?
                        transform.position = origin.X0Z() + dir * dist + reflectDir * 1.0f;
                        
                        transform.rotation = MathHelpers.LookRotation(reflectDir, Vector3.UnitY);
                    }
                    else
                    {
                        Console.WriteLine("EXPLODE!");
                    }

                    Console.WriteLine($"Collision with ray {oldPos}/{dir} at distance {dist}, normal = {normal}!");
                    //displayHit.transform.position = origin + dir * dist;
                }
            }
        }        
    }
}
