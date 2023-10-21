using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenTKBase
{
    public class RPCortex: RenderPipeline
    {
        public override void Render(Scene scene)
        {
            if (scene == null) return;

            var allCameras = scene.FindObjectsOfType<Camera>();
            var allRender = scene.FindObjectsOfType<Renderable>();

            // Restore cull mode to normal
            //GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            var envMaterial = OpenTKApp.APP.mainScene.environment;

            GL.Viewport(0, 0, OpenTKApp.APP.resX, OpenTKApp.APP.resY);

            foreach (var camera in allCameras)
            {
                if (!camera.enable) continue;

                // Clear color buffer and the depth buffer
                GL.ClearColor(camera.GetClearColor());
                GL.ClearDepth(camera.GetClearDepth());
                GL.Clear(camera.GetClearFlags());

                Shader.SetMatrix(Shader.MatrixType.Camera, camera.transform.worldToLocalMatrix);
                Shader.SetMatrix(Shader.MatrixType.InvCamera, camera.transform.localToWorldMatrix);
                Shader.SetMatrix(Shader.MatrixType.Projection, camera.projection);

                allRender.Sort((r1, r2) => r1.GetOrder().CompareTo(r2.GetOrder()));

                foreach (var render in allRender)
                {
                    if (!render.enable) continue;

                    render.Render(camera, null);
                }
            }
        }
    }
}
