
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

        public static Sprite FindSprite(string name)
        {
            return _sprites[name];
        }
    }
}
