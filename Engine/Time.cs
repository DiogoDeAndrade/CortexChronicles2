
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenTKBase
{
    public static class Time
    {
        static private float _timeDeltaTime;
        static private float _time;
        static public float deltaTime => _timeDeltaTime;
        static public float time => _time;

        static public void SetTimeParams(float time, float timeDeltaTime)
        {
            Time._timeDeltaTime = timeDeltaTime;
            Time._time = time;
        }

    }
}
