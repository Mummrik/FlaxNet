using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game
{
    public class MeshManager
    {
        //TODO: Fix array for all types of meshes
        public static Model tileMesh;
        public static Model wallMesh;
        public static void Init()
        {
            tileMesh = CreateTileMesh();
            wallMesh = CreateWallMesh();
        }

        public static void Unload()
        {
            FlaxEngine.Object.Destroy(ref tileMesh);
            FlaxEngine.Object.Destroy(ref wallMesh);
            tileMesh = null;
            wallMesh = null;
        }

        public static Vector3 TileSize() => tileMesh.LODs[0].Meshes[0].Box.Size;
        private static Model CreateTileMesh(uint TileSize = 100)
        {
            var Model = Content.CreateVirtualAsset<Model>();

            var vertices = new Vector3[]
            {
                Vector3.Zero * TileSize,
                Vector3.UnitZ * TileSize,
                Vector3.UnitX * TileSize,
                (Vector3.UnitX + Vector3.UnitZ) * TileSize
            };

            var uv = new Vector2[]
            {
                Vector2.Zero * TileSize,
                Vector2.UnitY * TileSize,
                Vector2.UnitX * TileSize,
                Vector2.One * TileSize
            };

            var triangles = new ushort[]
            {
                0,1,2,
                1,3,2
            };

            var normals = new Vector3[]
            {
                Vector3.Up,
                Vector3.Up,
                Vector3.Up,
                Vector3.Up
            };

            Model.LODs[0].Meshes[0].UpdateMesh(vertices, triangles, normals, vertices, uv);

            return Model;
        }

        private static Model CreateWallMesh(uint TileSize = 100, uint WallHeight = 250, uint WallThickness = 25)
        {
            var Model = Content.CreateVirtualAsset<Model>();

            var vertices = new Vector3[]
            {
                Vector3.Zero * TileSize,                                                                        // 0
                Vector3.UnitY * WallHeight,                                                                     // 1
                (Vector3.UnitY * WallHeight) + (Vector3.UnitX * TileSize),                                      // 2
                Vector3.UnitX * TileSize,                                                                       // 3
                (Vector3.UnitY * WallHeight) + (Vector3.UnitZ * WallThickness),                                 // 4
                (Vector3.UnitY * WallHeight) + (Vector3.UnitZ * WallThickness) + (Vector3.UnitX * TileSize),    // 5
                (Vector3.UnitZ * WallThickness),                                                                // 6
                (Vector3.UnitZ * WallThickness) + (Vector3.UnitX * TileSize)                                    // 7
            };

            var triangles = new ushort[]
            {
                //Front
                0,1,2,
                0,2,3,

                //Top
                1,4,5,
                1,5,2,

                //Back
                4,6,7,
                4,7,5,

                //Left
                0,6,4,
                0,4,1,

                //Right
                3,2,7,
                5,7,2
            };

            var normals = new Vector3[]
            {
                Vector3.Up,
                Vector3.Up,
                Vector3.Up,
                Vector3.Up,
                Vector3.Up,
                Vector3.Up,
                Vector3.Up,
                Vector3.Up
            };

            //var normals = new Vector3[]
            //{
            //    vertices[0] * Vector3.Backward,
            //    vertices[0] * Vector3.Backward,
            //    vertices[1] * Vector3.Up,
            //    vertices[1] * Vector3.Up,
            //    vertices[4] * Vector3.Forward,
            //    vertices[4] * Vector3.Forward,
            //    vertices[0] * Vector3.Right,
            //    vertices[0] * Vector3.Right
            //};

            Model.LODs[0].Meshes[0].UpdateMesh(vertices, triangles, normals);

            return Model;
        }
    }
}
