
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
        static private Dictionary<string, Font> _fonts = new Dictionary<string, Font>();

        public static void Clear()
        {
            foreach (var texture in _textures.Values)
            {
                texture.Clear();
            }
            foreach (var sprite in _sprites.Values)
            {
                sprite.Clear();
            }
            foreach (var font in _fonts.Values)
            {
                font.Clear();
            }

            _textures = new Dictionary<string, Texture>();
            _sprites = new Dictionary<string, Sprite>();
            _fonts = new Dictionary<string, Font>();
        }

        public static void AddTexture(string textureName, Texture texture)
        {
            _textures.Add(textureName, texture);
        }

        public static void RemoveTexture(string textureName)
        {
            Texture texture;
            if (!_textures.TryGetValue(textureName, out texture)) return;

            texture.Clear();
            _textures.Remove(textureName);
        }

        public static Texture LoadTexture(string textureFilename, TextureWrapMode wrapMode = TextureWrapMode.Repeat, TextureMinFilter filter = TextureMinFilter.Linear, bool mips = true)
        {
            Texture texture = new Texture(wrapMode, filter, false);
            texture.Load(textureFilename);

            AddTexture(textureFilename, texture);

            return texture;
        }

        public static Texture LoadTexture(string textureName, string textureFilename, TextureWrapMode wrapMode = TextureWrapMode.Repeat, TextureMinFilter filter = TextureMinFilter.Linear, bool mips = true)
        {
            Texture texture = new Texture(wrapMode, filter, false);
            texture.Load(textureFilename);

            AddTexture(textureName, texture);

            return texture;
        }

        public static Font LoadFont(string fontName, int gridX, int gridY, bool gridSize, int startX = 0, int startY = 0, int spacingX = 0, int spacingY = 0)
        {
            Font font = new Font();
            font.Load(fontName, gridX, gridY, gridSize, startX, startY, spacingX, spacingY);

            _fonts.Add(fontName, font);

            return font;
        }
 
        public static Font FindFont(string name)
        {
            return _fonts[name];
        }

        public static Sprite CreateSprite(string name, Texture texture, Vector2 hotspot, Vector4 uvRect, float pixelsPerUnit)
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

        public static Sprite CreateSpriteForPixelArt(string name, string textureName, Vector2 hotspot, Vector4 uvRect, float pixelsPerUnit)
        {
            var texture = LoadTexture(textureName, TextureWrapMode.Clamp, TextureMinFilter.Nearest, false);
            
            return CreateSprite(name, texture, hotspot, uvRect, pixelsPerUnit);
        }

        public static void CreateSpriteSheetForPixelArt(string name, string textureName, Vector2 hotspot, int tilesX, int tilesY, int maxSprites, float pixelsPerUnit)
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
            Sprite ret;
            if (_sprites.TryGetValue(name, out ret))
            {
                return ret;
            }
            return null;
        }

        public static void RemoveSprite(string spriteName)
        {
            Sprite sprite;
            if (!_sprites.TryGetValue(spriteName, out sprite)) return;

            sprite.Clear();
            _sprites.Remove(spriteName);
        }
    }
}
