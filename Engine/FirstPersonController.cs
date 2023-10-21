using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenTKBase
{
    public class FirstPersonController : Component
    {
        public float moveSpeed = 50.0f;
        public float rotateSpeed = MathF.PI;

        public Vector2 rotation = Vector2.Zero;

        public override void Update()
        {
            Vector3 moveDir = Vector3.Zero;

            if (Input.GetKey(Keys.W)) moveDir.Z = 1.0f;
            if (Input.GetKey(Keys.S)) moveDir.Z = -1.0f;
            if (Input.GetKey(Keys.A)) moveDir.X = -1.0f;
            if (Input.GetKey(Keys.D)) moveDir.X = 1.0f;
            if (Input.GetKey(Keys.Q)) moveDir.Y = -1.0f;
            if (Input.GetKey(Keys.E)) moveDir.Y = 1.0f;

            var tf = transform.forward; tf.Y = 0.0f; tf.Normalize();
            var tr = Vector3.Cross(tf, Vector3.UnitY); tr.Y = 0.0f; tr.Normalize();

            moveDir = moveDir.X * tr + moveDir.Z * tf + moveDir.Y * Vector3.UnitY;
            moveDir *= moveSpeed * Time.deltaTime;

            transform.position += moveDir;

            var mouseDelta = OpenTKApp.APP.GetMouseDelta();

            mouseDelta.X = MathF.Sign(mouseDelta.X);
            mouseDelta.Y = 0.0f;// MathF.Sign(mouseDelta.Y);

            float   angleY = -rotateSpeed * mouseDelta.X * Time.deltaTime;
            float   angleX = -rotateSpeed * mouseDelta.Y * Time.deltaTime;

            rotation.X += angleX;
            rotation.Y += angleY;
            rotation.X = Math.Clamp(rotation.X, -1.0f, 1.0f);
            var xQuat = Quaternion.FromAxisAngle(Vector3.UnitX, rotation.X);
            var yQuat = Quaternion.FromAxisAngle(Vector3.UnitY, rotation.Y);

            transform.rotation = yQuat * xQuat;
        }
    }
}
