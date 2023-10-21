using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace OpenTKBase
{
    public class PlayerBike : Component
    {
        private float           rightDir = 0.0f;
        private float           rightInc = 10.0f;
        private float           rightDamp = 0.95f;
        private SpriteRenderer  spriteRenderer;

        public override void Start()
        {
            GameObject playerShadow = new GameObject();
            playerShadow.transform.position = transform.position + Vector3.UnitY * 0.1f;
            playerShadow.transform.SetParent(transform);
            var sr = playerShadow.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.FindSprite("bike_shadow");

            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public override void Update()
        {
            rightDir *= rightDamp;
            if (Input.GetKey(Keys.Left)) rightDir -= rightInc * Time.timeDeltaTime;
            if (Input.GetKey(Keys.Right)) rightDir += rightInc * Time.timeDeltaTime;
            rightDir = MathF.Min(1, MathF.Max(-1.0f, rightDir));

            spriteRenderer.rotation = -20 * rightDir;
        }
    }
}
