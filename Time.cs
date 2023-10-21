
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenTKBase
{
    public static class Time
    {
        static private float _timeDeltaTime;
        static public float timeDeltaTime => _timeDeltaTime;

        static public void SetTime(float timeDeltaTime)
        {
            Time._timeDeltaTime = timeDeltaTime;
        }

    }
}
