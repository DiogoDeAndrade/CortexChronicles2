﻿
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenTKBase
{
    public static class Input
    {
        static private GameWindow window = null;

        static public void SetWindow(GameWindow window)
        {
            Input.window = window;
        }

        static public bool GetKey(Keys key)
        {
            return window.KeyboardState.IsKeyDown(key);
        }

        static public bool GetKeyDown(Keys key)
        {
            return window.KeyboardState.IsKeyDown(key) && !window.KeyboardState.WasKeyDown(key);
        }
        static public bool GetKeyUp(Keys key)
        {
            return !window.KeyboardState.IsKeyDown(key) && window.KeyboardState.WasKeyDown(key);
        }

        static public bool GetAnyKey()
        {
            return window.KeyboardState.IsAnyKeyDown;
        }

    }
}
