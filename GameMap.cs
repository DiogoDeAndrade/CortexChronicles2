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

                        var uvRect = GetUV(-1);

                        groundVertices.Add(centerPos + new Vector3(-tx, 0.0f, -tz)); groundNormals.Add(Vector3.UnitY); groundUV.Add(new Vector2(uvRect.X, uvRect.W));
                        groundVertices.Add(centerPos + new Vector3(-tx, 0.0f,  tz)); groundNormals.Add(Vector3.UnitY); groundUV.Add(new Vector2(uvRect.X, uvRect.Y));
                        groundVertices.Add(centerPos + new Vector3( tx, 0.0f,  tz)); groundNormals.Add(Vector3.UnitY); groundUV.Add(new Vector2(uvRect.Z, uvRect.Y));
                        groundVertices.Add(centerPos + new Vector3( tx, 0.0f, -tz)); groundNormals.Add(Vector3.UnitY); groundUV.Add(new Vector2(uvRect.Z, uvRect.W));

                        groundTriangles.Add(index); groundTriangles.Add(index + 1); groundTriangles.Add(index + 2);
                        groundTriangles.Add(index); groundTriangles.Add(index + 2); groundTriangles.Add(index + 3);

                        index = (uint)groundVertices.Count;

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

                            var uvRect = GetUV(-1);

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

                            var uvRect = GetUV(-1);

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

                            var uvRect = GetUV(-1);

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

                            var uvRect = GetUV(-1);

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

        Vector4 GetUV(int idx)
        {
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

        public override void Render(Camera camera, Material material)
        {
            Shader.SetMatrix(Shader.MatrixType.World, transform.localToWorldMatrix);

            groundMesh.Render(groundMaterial);
            wallMesh.Render(wallMaterial);
        }
    }
}
