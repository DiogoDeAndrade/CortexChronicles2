
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static OpenTK.Graphics.OpenGL.GL;
using static OpenTKBase.OpenTKApp;

namespace OpenTKBase
{
    public class OpenTKApp
    {
        public  int         resX { private set; get; }
        public  int         resY { private set; get; }
        public  int         windowSizeX { private set; get; }
        public  int         windowSizeY { private set; get; }
        private string      title = "Test App";
        private bool        debug = false;
        private Action      initAction = null;
        private Action      runAction = null;
        private GameWindow  window = null;
        private Scene       _mainScene = null;
        private float       _timeDeltaTime = 0.0f;
        private float       _time = 0.0f;
        private Vector3     _mouseDelta;
        private long        _timeSinceLastUpdate;
        private bool        _reset = false;
        private bool        _restart = false;

        private bool    exit = false;

        public Scene    mainScene { get { return _mainScene; } }
        public float    aspectRatio { get { return (float)resX / resY; } }

        public static OpenTKApp APP;
        private static DebugProc DebugMessageDelegate = OnDebugMessage;

        public OpenTKApp(int resX, int resY, string title, bool debug = false)
        {
            this.resX = windowSizeX = resX;
            this.resY = windowSizeY = resY;
            this.title = title;
            this.debug = debug;

            APP = this;
        }

        public bool Initialize()
        {
            window = new GameWindow(GameWindowSettings.Default,
                                    new NativeWindowSettings
                                    {
                                        Size = (resX, resY),
                                        Title = title,
                                        Profile = ContextProfile.Compatability,
                                        API = ContextAPI.OpenGL,
                                        APIVersion = new Version(4, 1),
                                        Flags = (debug)?(ContextFlags.Debug):(ContextFlags.Default)
                                    });

            if (debug)
            {
                GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
                GL.Enable(EnableCap.DebugOutput);
                GL.Enable(EnableCap.DebugOutputSynchronous);
            }

            Console.WriteLine($"OpenGL Version: {GL.GetString(StringName.Version)}");

            window.UpdateFrequency = 60.0f;
            window.UpdateFrame += OnUpdateFrame;
            window.RenderFrame += OnRender;
            window.Resize += OnResize;

            Input.SetWindow(window);

            // Activate depth testing
            GL.Enable(EnableCap.DepthTest);
            // Set the test function
            GL.DepthFunc(DepthFunction.Lequal);
            // Enable depth write
            GL.DepthMask(true);
            // Set blend operation
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            // Set cull mode
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            _mainScene = new Scene();

            _timeSinceLastUpdate = Stopwatch.GetTimestamp();

            return true;
        }

        private void OnResize(ResizeEventArgs obj)
        {
            windowSizeX = obj.Size.X;
            windowSizeY = obj.Size.Y;
        }

        public void Shutdown()
        {
        }

        public void Restart()
        {
            _restart = true;
        }

        public void Run(Action initFunction, Action mainLoopFunction)
        {
            initAction = initFunction;
            runAction = mainLoopFunction;

            initAction();

            window?.Run();
        }

        public void Render(RenderPipeline rp)
        {
            rp.Render(mainScene);
        }

        public Vector3 GetMouseDelta()
        {
            return _mouseDelta;
        }

        public void LockMouse(bool b)
        {
            window.CursorState = (b)?(CursorState.Grabbed):(CursorState.Normal);
        }

        private void OnUpdateFrame(FrameEventArgs e)
        {
            RunUpdate();
        }

        void RunUpdate()
        { 
            if (window == null) return;
            if (_restart)
            {
                _reset = true;
                // Clear everything and initialize everything again
                Resources.Clear();

                _mainScene = new Scene();

                initAction();

                _restart = false;

                return;
            }
            if (_reset) return;

            if (((window.KeyboardState.IsKeyDown(Keys.Escape)) && (window.KeyboardState.IsKeyDown(Keys.LeftShift))) || (exit))
            {
                window.Close();
                return;
            }

            if (((window.KeyboardState.IsKeyDown(Keys.Enter)) && (window.KeyboardState.IsKeyDown(Keys.LeftAlt))) || (exit))
            {
                if (window.WindowState == WindowState.Normal)
                {
                    window.WindowState = WindowState.Fullscreen;
                }
                else if (window.WindowState == WindowState.Fullscreen)
                {
                    window.WindowState = WindowState.Normal;
                    window.Size = new Vector2i(resX, resY);
                }
            }

            long timestamp = Stopwatch.GetTimestamp();
            
            _timeDeltaTime = MathF.Min(0.05f, (float)(timestamp - _timeSinceLastUpdate) / Stopwatch.Frequency);
            _time += _timeDeltaTime;
            Time.SetTimeParams(_time, _timeDeltaTime);
            _timeSinceLastUpdate = timestamp;

            while ((Component.needToAwake) ||
                   (Component.needToStart))
            {
                Component.AwakeAll();
                if (_reset) return;
                Component.StartAll();
                if (_reset) return;
            }

            var tmp = window.MouseState.Delta;
            _mouseDelta = new Vector3(tmp.X, tmp.Y, window.MouseState.ScrollDelta.Y);

            if (mainScene != null)
            {
                var allObjects = new List<GameObject>(mainScene.GetAllObjects());
                foreach (var obj in allObjects)
                {
                    var allComponents = obj.GetAllComponents();
                    foreach (var c in allComponents)
                    {
                        if (c.enable)
                        {
                            c.Update();
                            if (_reset) return;
                        }
                    }
                }

                GameObject.DestroyAllObjects();
            }
        }

        private void OnRender(FrameEventArgs e)
        {
            if (window == null) return;

            ExecuteQueue();

            if (_restart) return;
            if (_reset)
            {
                _reset = false;
                return;
            }

            runAction?.Invoke();

            window.SwapBuffers();
        }

        private static void OnDebugMessage(
            DebugSource source,     // Source of the debugging message.
            DebugType type,         // Type of the debugging message.
            int id,                 // ID associated with the message.
            DebugSeverity severity, // Severity of the message.
            int length,             // Length of the string in pMessage.
            IntPtr pMessage,        // Pointer to message string.
            IntPtr pUserParam)      // The pointer you gave to OpenGL, explained later.
        {
            // In order to access the string pointed to by pMessage, you can use Marshal
            // class to copy its contents to a C# string without unsafe code. You can
            // also use the new function Marshal.PtrToStringUTF8 since .NET Core 1.1.
            string message = Marshal.PtrToStringAnsi(pMessage, length);

            // The rest of the function is up to you to implement, however a debug output
            // is always useful.
            Console.WriteLine("[{0} source={1} type={2} id={3}] {4}", severity, source, type, id, message);

            // Potentially, you may want to throw from the function for certain severity
            // messages.
            if (type == DebugType.DebugTypeError)
            {
                throw new Exception(message);
            }
        }

        public enum GfxOp { DeleteVbo, DeleteIbo, DeleteTexture };
        struct GfxQueueElem
        {
            public GfxOp    op;
            public int      handle;

            public void Execute()
            {
                switch (op)
                {
                    case GfxOp.DeleteVbo:
                        Console.WriteLine("Destroy VBO = " + handle);

                        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                        GL.DeleteBuffer(handle);
                        break;
                    case GfxOp.DeleteIbo:
                        Console.WriteLine("Destroy IBO = " + handle);
                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                        GL.DeleteBuffer(handle);
                        break;
                    case GfxOp.DeleteTexture:
                        for (int i = 0; i < 8; i++)
                        {
                            GL.ActiveTexture(TextureUnit.Texture0 + i);
                            GL.BindTexture(TextureTarget.Texture2D, 0);
                        }
                        GL.DeleteTexture(handle);
                        break;
                    default:
                        break;
                }
            }
        }
        static List<GfxQueueElem> gfxQueue = new List<GfxQueueElem>();

        static public void AddToGfxQueue(GfxOp op, int handle)
        {
            gfxQueue.Add(new GfxQueueElem { op = op, handle = handle });
        }
        
        private void ExecuteQueue()
        {
            foreach (var o in gfxQueue)
            {
                o.Execute();
            }
            gfxQueue.Clear();
        }
    }
}
