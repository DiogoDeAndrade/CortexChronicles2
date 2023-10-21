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


            Console.WriteLine($"Speed = {bikeSpeed.Z}");


            Vector3 newPos = transform.position;
            
            newPos += transform.forward * bikeSpeed.Z * Time.deltaTime;

            transform.position = newPos;

            transform.rotation *= Quaternion.FromEulerAngles(0.0f, -4.0f * rightDir * Time.deltaTime, 0.0f);
        }
    }
}
