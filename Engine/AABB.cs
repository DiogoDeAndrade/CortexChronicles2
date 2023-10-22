using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKBase
{
    public struct AABB
    {
        public Vector3 center;
        public Vector3 extents;

        public bool Spherecast(Vector3 origin, Vector3 dir, float radius, ref float dist, ref Vector3 normal)
        {
            AABB inflatedAABB = new AABB
            {
                center = center,
                extents = extents + Vector3.One * radius * 0.5f
            };

            return inflatedAABB.Raycast(origin, dir, ref dist, ref normal);
        }

        public bool Raycast(Vector3 origin, Vector3 dir, ref float dist, ref Vector3 normal)
        {
            float tmin = float.NegativeInfinity;
            float tmax = float.PositiveInfinity;

            Vector3[] normals = {
            new Vector3(-1, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, -1, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, -1),
            new Vector3(0, 0, 1)
        };

            int normalIndex = -1;

            for (int i = 0; i < 3; ++i)
            {
                if (Math.Abs(dir[i]) < 1e-5)
                {
                    if (origin[i] < center[i] - extents[i] || origin[i] > center[i] + extents[i])
                        return false;
                }
                else
                {
                    float ood = 1.0f / dir[i];
                    float t1 = (center[i] - extents[i] - origin[i]) * ood;
                    float t2 = (center[i] + extents[i] - origin[i]) * ood;

                    if (t1 > t2)
                    {
                        var tmp = t1;
                        t1 = t2;
                        t2 = tmp;
                        int offset = i * 2 + 1;
                        if (tmin < t1)
                        {
                            tmin = t1;
                            normalIndex = offset;
                        }
                        tmax = Math.Min(tmax, t2);
                    }
                    else
                    {
                        int offset = i * 2;
                        if (tmin < t1)
                        {
                            tmin = t1;
                            normalIndex = offset;
                        }
                        tmax = Math.Min(tmax, t2);
                    }

                    if (tmin > tmax)
                        return false;
                }
            }

            if (tmin < 0)
                return false;

            if (normalIndex >= 0)
            {
                normal = normals[normalIndex];
            }
            else
            {
                normal = Vector3.Zero;
            }

            dist = tmin;
            return true;
        }

        public float DistanceToPoint(Vector3 point)
        {
            float dx = Math.Max(0, Math.Max(center.X - extents.X - point.X, point.X - center.X - extents.X));
            float dy = Math.Max(0, Math.Max(center.Y - extents.Y - point.Y, point.Y - center.Y - extents.Y));
            float dz = Math.Max(0, Math.Max(center.Z - extents.Z - point.Z, point.Z - center.Z - extents.Z));

            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
    }
}
