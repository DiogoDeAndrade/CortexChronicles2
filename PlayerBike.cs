using OpenTK.Mathematics;
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
        private SpriteRenderer  shadowRenderer;

        public GameMap          gameMap;
        public float            maxHealth = 100.0f;
        public float health;

        public float            healthPercentage => health / maxHealth;

        public override void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.mode = SpriteRenderer.Mode.World;

            GameObject playerShadow = new GameObject();
            playerShadow.transform.SetParent(transform);
            playerShadow.transform.localPosition = Vector3.UnitY * 0.1f;
            playerShadow.transform.localRotation = Quaternion.FromEulerAngles(MathF.PI * 0.5f, 0.0f, 0.0f);
            shadowRenderer = playerShadow.AddComponent<SpriteRenderer>();
            shadowRenderer.sprite = Resources.FindSprite("bike_shadow");
            shadowRenderer.mode = SpriteRenderer.Mode.World;

            health = maxHealth;
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
                    
                    float dp = Vector3.Dot(reflectDir, dir);
                    if (dp > 0)
                    {
                        // Just ping out of it change direction, take some damage maybe?
                        transform.position = origin.X0Z() + dir * dist + reflectDir * 1.0f;

                        transform.rotation = MathHelpers.LookRotation(reflectDir, Vector3.UnitY);

                        health -= (1.0f - dp) * maxHealth;
                    }
                    else
                    {
                        health = 0.0f;
                    }

                    if (health <= 0.0f)
                    { 
                        spriteRenderer.enable = false;
                        this.enable = false;
                        shadowRenderer.enable = false;

                        GameObject playerExplosion = new GameObject();
                        playerExplosion.transform.position = transform.position + Vector3.UnitY * 1.5f;
                        playerExplosion.AddComponent<Explosion>();

                        FindObjectOfType<UI>().state = UI.State.GameOver;
                    }
                }
            }
        }        
    }
}
