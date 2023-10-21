
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenTKBase
{
    public static class Resources
    {
        static private Dictionary<string, Texture> _textures = new Dictionary<string, Texture>();
        static private Dictionary<string, Sprite>  _sprites = new Dictionary<string, Sprite>();

        public static Texture LoadTexture(string textureFilename, TextureWrapMode wrapMode = TextureWrapMode.Repeat, TextureMinFilter filter = TextureMinFilter.Linear, bool mips = true)
        {
            Texture texture = new Texture(wrapMode, filter, false);
            texture.Load(textureFilename);

            _textures.Add(textureFilename, texture);

            return texture;
        }

        public static Sprite CreateSprite(string name, Texture texture, Vector2 hotspot, Vector4 uvRect, int pixelsPerUnit)
        {
            Sprite sprite = new Sprite()
            {
                texture = texture,
                hotspot = hotspot,
                uvRect = uvRect,
                pixelsPerUnit = pixelsPerUnit
            };

            _sprites.Add(name, sprite);

            return sprite;
        }

        public static Sprite CreateSpriteForPixelArt(string name, string textureName, Vector2 hotspot, Vector4 uvRect, int pixelsPerUnit)
        {
            var texture = LoadTexture(textureName, TextureWrapMode.Clamp, TextureMinFilter.Nearest, false);
            
            return CreateSprite(name, texture, hotspot, uvRect, pixelsPerUnit);
        }

        public static void CreateSpriteSheetForPixelArt(string name, string textureName, Vector2 hotspot, int tilesX, int tilesY, int maxSprites, int pixelsPerUnit)
        {
            var texture = LoadTexture(textureName, TextureWrapMode.Clamp, TextureMinFilter.Nearest, false);

            for (int y = 0; y < tilesY; y++)
            {
                for (int x = 0; x < tilesX; x++)
                {
                    int n = x + y * tilesX;
                    if (n >= maxSprites) break;

                    Vector4 r = new Vector4((float)x / tilesX, (float)y / tilesY, 0.0f, 0.0f);
                    r.Z = r.X + 1.0f / tilesX;
                    r.W = r.Y + 1.0f / tilesY;

                    CreateSprite($"{name}{n}", texture, hotspot, r, pixelsPerUnit);
                }
            }
        }

        public static Sprite FindSprite(string name)
        {
            return _sprites[name];
        }
    }
}
