using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;

namespace OpenTKBase
{
    public class Explosion : Component
    {
        private SpriteRenderer  spriteRenderer;
        private float           timer;
        private float           frameTime = 0.1f;
        private int             index;
        public  float           scale = 8.0f;
        private List<Sprite>    sprites;

        public override void Start()
        {
            transform.localScale = new Vector3(scale, scale, scale);
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = Resources.FindSprite("explosion1");
            spriteRenderer.mode = SpriteRenderer.Mode.Billboard;

            // Animate
            sprites = new List<Sprite>();
            for (int i = 0; i < 16; i++)
            {
                sprites.Add(Resources.FindSprite($"explosion{i}"));
            }

            timer = frameTime;
            index = 0;
        }

        public override void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer += frameTime;
                index += 2;
                if (index < sprites.Count)
                {
                    spriteRenderer.sprite = sprites[index];
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }        
    }
}
