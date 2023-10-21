using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenTKBase
{

    public class GameMap : Renderable
    {
        public float                tileSizeX = 5.0f, tileSizeY = 5.0f, tileSizeZ = 5.0f;

        private int[]               mapData;
        private int                 sizeX, sizeZ;
        private List<(int, int)>[]  noteworthyPositions = new List<(int, int)>[256];
        private Mesh                groundMesh;
        private Material            groundMaterial;
        private Texture             groundTexture;
        private Mesh                wallMesh;
        private Material            wallMaterial;
        private Texture             wallTexture;

        public override void Update()
        {
        }

        public void LoadMap(string filename)
        { 
            // Load the actual map
            List<string> textMapData = new List<string>();
            
            sizeX = 0;
            using (StreamReader reader = new StreamReader(filename))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string tmp = "";

                    if (line.Length > 1)
                    {
                        for (int i = 0; i < line.Length; i++)
                        {
                            char c = line[i];
                            if (c == '#') tmp += "#";
                            else
                            {

                                if ((c != '.') || (c != ' '))
                                {
                                    if (noteworthyPositions[(uint)c] == null)
                                    {
                                        noteworthyPositions[(uint)c] = new List<(int, int)>();
                                        noteworthyPositions[(uint)c].Add((i, textMapData.Count));
                                    }
                                }
                                tmp += ".";
                            }
                        }
                    }

                    sizeX = Math.Max(sizeX, tmp.Length);
                    textMapData.Add(tmp);
                }
            }

            mapData = new int[sizeX * textMapData.Count];
            for (int i = 0; i < textMapData.Count; i++)
            {
                var line = textMapData[i];
                var index = i * sizeX;
                for (int j = 0; j < line.Length; j++)
                {
                    if (textMapData[i][j] == '#')
                        mapData[index] = 1;
                    index++;
                }
            }
            sizeZ = textMapData.Count;
        }

        public void BuildGeometry()
        {
            // Build ground/ceiling geometry
            var groundVertices = new List<Vector3>();
            var groundNormals = new List<Vector3>();
            var groundUV = new List<Vector2>();
            var groundTriangles = new List<uint>();

            float tx = tileSizeX * 0.5f;
            float tz = tileSizeZ * 0.5f;

            for (int y = 1; y < sizeZ - 1; y++)
            {
                for (int x = 1; x < sizeX - 1; x++)
                {
                    if (mapData[x + y * sizeX] == 0)
                    {
                        // Create ground mesh
                        var centerPos = GetCenterPos(x, y);

                        uint index = (uint)groundVertices.Count;

                        var uvRect = GetGroundUV((((x * 3) + (y * 5)) % 8));

                        groundVertices.Add(centerPos + new Vector3(-tx, 0.0f, -tz)); groundNormals.Add(Vector3.UnitY); groundUV.Add(new Vector2(uvRect.X, uvRect.W));
                        groundVertices.Add(centerPos + new Vector3(-tx, 0.0f,  tz)); groundNormals.Add(Vector3.UnitY); groundUV.Add(new Vector2(uvRect.X, uvRect.Y));
                        groundVertices.Add(centerPos + new Vector3( tx, 0.0f,  tz)); groundNormals.Add(Vector3.UnitY); groundUV.Add(new Vector2(uvRect.Z, uvRect.Y));
                        groundVertices.Add(centerPos + new Vector3( tx, 0.0f, -tz)); groundNormals.Add(Vector3.UnitY); groundUV.Add(new Vector2(uvRect.Z, uvRect.W));

                        groundTriangles.Add(index); groundTriangles.Add(index + 1); groundTriangles.Add(index + 2);
                        groundTriangles.Add(index); groundTriangles.Add(index + 2); groundTriangles.Add(index + 3);

                        index = (uint)groundVertices.Count;

                        uvRect = GetGroundUV((((x * 7) + (y * 3)) % 8));

                        groundVertices.Add(centerPos + new Vector3(-tx, tileSizeY, -tz)); groundNormals.Add(Vector3.UnitY); groundUV.Add(new Vector2(uvRect.X, uvRect.W));
                        groundVertices.Add(centerPos + new Vector3(-tx, tileSizeY,  tz)); groundNormals.Add(Vector3.UnitY); groundUV.Add(new Vector2(uvRect.X, uvRect.Y));
                        groundVertices.Add(centerPos + new Vector3( tx, tileSizeY,  tz)); groundNormals.Add(Vector3.UnitY); groundUV.Add(new Vector2(uvRect.Z, uvRect.Y));
                        groundVertices.Add(centerPos + new Vector3( tx, tileSizeY, -tz)); groundNormals.Add(Vector3.UnitY); groundUV.Add(new Vector2(uvRect.Z, uvRect.W));

                        groundTriangles.Add(index); groundTriangles.Add(index + 2); groundTriangles.Add(index + 1);
                        groundTriangles.Add(index); groundTriangles.Add(index + 3); groundTriangles.Add(index + 2);
                    }
                }
            }

            groundMesh = new Mesh();
            groundMesh.SetVertices(groundVertices);
            groundMesh.SetNormals(groundNormals);
            groundMesh.SetUVs(groundUV);
            groundMesh.SetIndices(groundTriangles);

            groundTexture = new Texture(OpenTK.Graphics.OpenGL.TextureWrapMode.Repeat, OpenTK.Graphics.OpenGL.TextureMinFilter.Nearest, false);
            groundTexture.Load("Textures/ground_texture.png");

            groundMaterial = new Material(Shader.Find("Shaders/textured"));
            groundMaterial.Set("Color", new Color4(0.0f, 0.0f, 1.0f, 1.0f));
            groundMaterial.Set("Albedo", groundTexture);

            // Build wall geometry
            var wallVertices = new List<Vector3>();
            var wallNormals = new List<Vector3>();
            var wallUV = new List<Vector2>();
            var wallTriangles = new List<uint>();

            for (int y = 1; y < sizeZ - 1; y++)
            {
                for (int x = 1; x < sizeX - 1; x++)
                {
                    if (mapData[x + y * sizeX] == 0)
                    {
                        // Create ground mesh
                        var centerPos = GetCenterPos(x, y);

                        if (mapData[(x - 1) + y * sizeX] == 1)
                        {
                            uint index = (uint)wallVertices.Count;

                            var uvRect = GetWallUV((((x * 7) + (y * 3)) % 32));

                            wallVertices.Add(centerPos + new Vector3(-tx, 0.0f, -tz)); wallNormals.Add(Vector3.UnitX); wallUV.Add(new Vector2(uvRect.X, uvRect.W));
                            wallVertices.Add(centerPos + new Vector3(-tx, tileSizeY, -tz)); wallNormals.Add(Vector3.UnitX); wallUV.Add(new Vector2(uvRect.X, uvRect.Y));
                            wallVertices.Add(centerPos + new Vector3(-tx, tileSizeY,  tz)); wallNormals.Add(Vector3.UnitX); wallUV.Add(new Vector2(uvRect.Z, uvRect.Y));
                            wallVertices.Add(centerPos + new Vector3(-tx, 0.0f, tz)); wallNormals.Add(Vector3.UnitX); wallUV.Add(new Vector2(uvRect.Z, uvRect.W));

                            wallTriangles.Add(index); wallTriangles.Add(index + 1); wallTriangles.Add(index + 2);
                            wallTriangles.Add(index); wallTriangles.Add(index + 2); wallTriangles.Add(index + 3);
                        }
                        if (mapData[(x + 1) + y * sizeX] == 1)
                        {
                            uint index = (uint)wallVertices.Count;

                            var uvRect = GetWallUV((((x * 5) + (y * 3)) % 32));

                            wallVertices.Add(centerPos + new Vector3(tx, 0.0f, tz)); wallNormals.Add(-Vector3.UnitX); wallUV.Add(new Vector2(uvRect.X, uvRect.W));
                            wallVertices.Add(centerPos + new Vector3(tx, tileSizeY, tz)); wallNormals.Add(-Vector3.UnitX); wallUV.Add(new Vector2(uvRect.X, uvRect.Y));
                            wallVertices.Add(centerPos + new Vector3(tx, tileSizeY, -tz)); wallNormals.Add(-Vector3.UnitX); wallUV.Add(new Vector2(uvRect.Z, uvRect.Y));
                            wallVertices.Add(centerPos + new Vector3(tx, 0.0f, -tz)); wallNormals.Add(-Vector3.UnitX); wallUV.Add(new Vector2(uvRect.Z, uvRect.W));

                            wallTriangles.Add(index); wallTriangles.Add(index + 1); wallTriangles.Add(index + 2);
                            wallTriangles.Add(index); wallTriangles.Add(index + 2); wallTriangles.Add(index + 3);
                        }
                        if (mapData[x + (y + 1) * sizeX] == 1)
                        {
                            uint index = (uint)wallVertices.Count;

                            var uvRect = GetWallUV((((x * 3) + (y * 5)) % 32));

                            wallVertices.Add(centerPos + new Vector3(-tx, 0.0f, tz)); wallNormals.Add(-Vector3.UnitZ); wallUV.Add(new Vector2(uvRect.X, uvRect.W));
                            wallVertices.Add(centerPos + new Vector3(-tx, tileSizeY, tz)); wallNormals.Add(-Vector3.UnitZ); wallUV.Add(new Vector2(uvRect.X, uvRect.Y));
                            wallVertices.Add(centerPos + new Vector3( tx, tileSizeY, tz)); wallNormals.Add(-Vector3.UnitZ); wallUV.Add(new Vector2(uvRect.Z, uvRect.Y));
                            wallVertices.Add(centerPos + new Vector3( tx, 0.0f, tz)); wallNormals.Add(-Vector3.UnitZ); wallUV.Add(new Vector2(uvRect.Z, uvRect.W));

                            wallTriangles.Add(index); wallTriangles.Add(index + 1); wallTriangles.Add(index + 2);
                            wallTriangles.Add(index); wallTriangles.Add(index + 2); wallTriangles.Add(index + 3);
                        }
                        if (mapData[x + (y - 1) * sizeX] == 1)
                        {
                            uint index = (uint)wallVertices.Count;

                            var uvRect = GetWallUV((((x * 9) + (y * 5)) % 32));

                            wallVertices.Add(centerPos + new Vector3(tx, 0.0f, -tz)); wallNormals.Add(Vector3.UnitZ); wallUV.Add(new Vector2(uvRect.X, uvRect.W));
                            wallVertices.Add(centerPos + new Vector3(tx, tileSizeY, -tz)); wallNormals.Add(Vector3.UnitZ); wallUV.Add(new Vector2(uvRect.X, uvRect.Y));
                            wallVertices.Add(centerPos + new Vector3(-tx, tileSizeY, -tz)); wallNormals.Add(Vector3.UnitZ); wallUV.Add(new Vector2(uvRect.Z, uvRect.Y));
                            wallVertices.Add(centerPos + new Vector3(-tx, 0.0f, -tz)); wallNormals.Add(Vector3.UnitZ); wallUV.Add(new Vector2(uvRect.Z, uvRect.W));

                            wallTriangles.Add(index); wallTriangles.Add(index + 1); wallTriangles.Add(index + 2);
                            wallTriangles.Add(index); wallTriangles.Add(index + 2); wallTriangles.Add(index + 3);
                        }
                    }
                }
            }

            wallMesh = new Mesh();
            wallMesh.SetVertices(wallVertices);
            wallMesh.SetNormals(wallNormals);
            wallMesh.SetUVs(wallUV);
            wallMesh.SetIndices(wallTriangles);

            wallTexture = new Texture(OpenTK.Graphics.OpenGL.TextureWrapMode.Repeat, OpenTK.Graphics.OpenGL.TextureMinFilter.Nearest, false);
            wallTexture.Load("Textures/wall_texture.png");

            wallMaterial = new Material(Shader.Find("Shaders/textured"));
            wallMaterial.Set("Color", new Color4(0.0f, 1.0f, 1.0f, 1.0f));
            wallMaterial.Set("Albedo", wallTexture);
        }

        Vector3 GetCenterPos(int x, int y)
        {
            return new Vector3(x * tileSizeX - sizeX * tileSizeX * 0.5f, 0.0f, y * tileSizeZ - sizeZ * tileSizeZ * 0.5f);
        }

        Vector4 GetGroundUV(int idx)
        {
            if (idx >= 0)
            {
                var ret = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
                ret.X = (float)idx / 8.0f;
                ret.Y = 0.0f;
                ret.Z = ret.X + (float)1.0f / 8.0f;
                ret.W = 1.0f;

                return ret;
            }
            return new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
        }

        Vector4 GetWallUV(int idx)
        {
            if (idx >= 0)
            {
                var ret = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
                ret.X = (float)idx / 32.0f;
                ret.Y = 0.0f;
                ret.Z = ret.X + (float)1.0f / 32.0f;
                ret.W = 1.0f;

                return ret;
            }
            return new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
        }

        public Vector3 GetPosition(char c)
        {
            if (noteworthyPositions[(uint)c] != null)
            {
                foreach (var p in noteworthyPositions[(uint)c])
                {
                    return GetCenterPos(p.Item1, p.Item2);
                }
            }

            return Vector3.Zero;
        }

        public int GetTileFromWorldPos(Vector3 worldPos)
        {
            int tx = (int)MathF.Round((worldPos.X + sizeX * tileSizeX * 0.5f) / tileSizeX);
            int ty = (int)MathF.Round((worldPos.Z + sizeZ * tileSizeZ * 0.5f) / tileSizeZ);

            return mapData[tx + ty * sizeX];
        }

        public bool Raycast(Vector3 origin, Vector3 dir, float maxDist, ref float dist, ref Vector3 normal)
        {
            dist = float.MaxValue;
            normal = Vector3.Zero;

            // Don't check boundaryand only check places where there is a wall, but next to an 
            // empty space
            bool ret = false;
            for (int y = 1; y < sizeZ - 1; y++)
            {
                for (int x = 1; x < sizeX - 1; x++)
                {
                    if (mapData[x + y * sizeX] == 1)
                    {
                        if ((mapData[(x - 1) + y * sizeX] == 0) ||
                            (mapData[(x + 1) + y * sizeX] == 0) ||
                            (mapData[x + (y - 1) * sizeX] == 0) ||
                            (mapData[x + (y + 1) * sizeX] == 0))
                        {
                            var aabb = GetAABB(x, y);

                            // Check distance to AABB
                            float distToAABB = aabb.DistanceToPoint(origin);
                            if (distToAABB < maxDist)
                            {
                                float d = float.MaxValue;
                                Vector3 n = Vector3.Zero;
                                if (aabb.Raycast(origin, dir, ref d, ref n))
                                {
                                    if (d < maxDist)
                                    {
                                        ret = true;
                                        if (d < dist)
                                        {
                                            dist = d;
                                            normal = n;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return ret;
        }

        public bool Spherecast(Vector3 origin, Vector3 dir, float radius, float maxDist, ref float dist, ref Vector3 normal)
        {
            dist = float.MaxValue;
            normal = Vector3.Zero;

            // Don't check boundaryand only check places where there is a wall, but next to an 
            // empty space
            bool ret = false;
            for (int y = 1; y < sizeZ - 1; y++)
            {
                for (int x = 1; x < sizeX - 1; x++)
                {
                    if (mapData[x + y * sizeX] == 1)
                    {
                        if ((mapData[(x - 1) + y * sizeX] == 0) ||
                            (mapData[(x + 1) + y * sizeX] == 0) ||
                            (mapData[x + (y - 1) * sizeX] == 0) ||
                            (mapData[x + (y + 1) * sizeX] == 0))
                        {
                            var aabb = GetAABB(x, y);

                            // Check distance to AABB
                            float distToAABB = aabb.DistanceToPoint(origin);
                            if (distToAABB < maxDist - radius)
                            {
                                float d = float.MaxValue;
                                Vector3 n = Vector3.Zero;
                                if (aabb.Spherecast(origin, dir, radius, ref d, ref n))
                                {
                                    if (d < maxDist)
                                    {
                                        ret = true;
                                        if (d < dist)
                                        {
                                            dist = d;
                                            normal = n;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return ret;
        }

        public AABB GetAABB(int x, int y)
        {
            Vector3 center = GetCenterPos(x, y);
            center += Vector3.UnitY * tileSizeY * 0.5f;

            return new AABB
            {
                center = center,
                extents = new Vector3(tileSizeX * 0.5f, tileSizeY * 0.5f, tileSizeZ * 0.5f)
            };
        }

        public override void Render(Camera camera, Material material)
        {
            Shader.SetMatrix(Shader.MatrixType.World, transform.localToWorldMatrix);

            groundMesh.Render(groundMaterial);
            wallMesh.Render(wallMaterial);
        }
    }
}
