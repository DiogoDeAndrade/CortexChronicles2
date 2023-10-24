
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenTKBase
{
    public static class MathHelpers
    {
        static public Vector3 Reflect(this Vector3 V, Vector3 N)
        {
            // Ensure the normal is normalized
            N.Normalize();

            float dotProduct = Vector3.Dot(V, N);
            Vector3 reflectedVector = V - 2 * dotProduct * N;

            return reflectedVector;
        }

        static public Vector3 X0Z(this Vector3 v)
        {
            return new Vector3(v.X, 0.0f, v.Z);
        }

        static public Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistance)
        {
            Vector3 delta = target - current;
            if (delta.Length < maxDistance)
            {
                return target;
            }

            return current + delta.Normalized() * maxDistance;
        }

        public static Quaternion LookRotation(Vector3 forward, Vector3 up)
        {
            forward = -Vector3.Normalize(forward);

            Vector3 right = Vector3.Normalize(Vector3.Cross(up, forward));
            up = Vector3.Cross(forward, right);

            float m00 = right.X;
            float m01 = right.Y;
            float m02 = right.Z;
            float m10 = up.X;
            float m11 = up.Y;
            float m12 = up.Z;
            float m20 = forward.X;
            float m21 = forward.Y;
            float m22 = forward.Z;

            float num8 = (m00 + m11) + m22;

            if (num8 > 0f)
            {
                float num0 = (float)Math.Sqrt(num8 + 1f);
                float num2 = 0.5f / num0;
                return new Quaternion(
                    (m12 - m21) * num2,
                    (m20 - m02) * num2,
                    (m01 - m10) * num2,
                    num0 * 0.5f
                );
            }

            if ((m00 >= m11) && (m00 >= m22))
            {
                float num7 = (float)Math.Sqrt(((1f + m00) - m11) - m22);
                float num4 = 0.5f / num7;
                return new Quaternion(
                    0.5f * num7,
                    (m01 + m10) * num4,
                    (m02 + m20) * num4,
                    (m12 - m21) * num4
                );
            }

            if (m11 > m22)
            {
                float num6 = (float)Math.Sqrt(((1f + m11) - m00) - m22);
                float num3 = 0.5f / num6;
                return new Quaternion(
                    (m10 + m01) * num3,
                    0.5f * num6,
                    (m21 + m12) * num3,
                    (m20 - m02) * num3
                );
            }

            float num5 = (float)Math.Sqrt(((1f + m22) - m00) - m11);
            float num = 0.5f / num5;
            return new Quaternion(
                (m20 + m02) * num,
                (m21 + m12) * num,
                0.5f * num5,
                (m01 - m10) * num
            );
        }
    }
}
