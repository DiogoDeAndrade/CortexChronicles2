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
            if (Input.GetKey(Keys.Left)) rightDir -= rightInc * Time.timeDeltaTime;
            if (Input.GetKey(Keys.Right)) rightDir += rightInc * Time.timeDeltaTime;
            rightDir = MathF.Min(1, MathF.Max(-1.0f, rightDir));

            spriteRenderer.rotation = -20 * rightDir;

            transform.position += transform.forward * 1.0f * Time.timeDeltaTime;
        }
    }
}
