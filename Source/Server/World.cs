using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace Server
{
    public struct World
    {
        public uint id;
        public Vector2 bounds;
        public Tile[,] tiles;

        public bool IsInBounds(Creature creature) => IsInBounds(creature.Position);
        public bool IsInBounds(Vector2 position) => IsInBounds(position.X * 0.01f, position.Y * 0.01f);
        public bool IsInBounds(Vector3 position) => IsInBounds(position.X * 0.01f, position.Z * 0.01f);
        public bool IsInBounds(float x, float y) => x >= 0 && y >= 0 && x < bounds.X && y < bounds.Y;

        public Tile[] GetMapSector(Creature creature) => GetMapSector(creature.Position);
        public Tile[] GetMapSector(Vector2 position) => GetMapSector((int)(position.X * 0.01f), (int)(position.Y * 0.01f));
        public Tile[] GetMapSector(Vector3 position) => GetMapSector((int)(position.X * 0.01f), (int)(position.Z * 0.01f));
        public Tile[] GetMapSector(int x, int y)
        {
            List<Tile> sector = new List<Tile>();

            // X Offsets
            int startX = x - Game.WORLD_CHUNKSIZE_HALF;
            int endX = x + Game.WORLD_CHUNKSIZE_HALF;

            // Y Offsets
            int startY = y - Game.WORLD_CHUNKSIZE_HALF;
            int endY = y + Game.WORLD_CHUNKSIZE_HALF;

            for (int X = startX; X < endX; X++)
            {
                for (int Y = startY; Y < endY; Y++)
                {
                    if (IsInBounds(X, Y))
                        sector.Add(tiles[X, Y]);
                }
            }

            return sector.Count > 0 ? sector.ToArray() : null;
        }

        public Tile[] GetTilesToLoad(Vector2 position, Vector2 direction) => GetTilesToLoad((int)position.X, (int)position.Y, direction);
        public Tile[] GetTilesToLoad(int x, int y, Vector2 direction)
        {
            List<Tile> Tiles = new List<Tile>();

            if (direction.X != 0)
            {
                int X = x + (int)direction.X * Game.WORLD_CHUNKSIZE_HALF + (direction.X > 0 ? -1 : 0);
                int startY = y - Game.WORLD_CHUNKSIZE_HALF;
                int endY = startY + Game.WORLD_CHUNKSIZE;

                for (int Y = startY; Y < endY; Y++)
                {
                    if (IsInBounds(X, Y))
                        Tiles.Add(tiles[X, Y]);
                }
            }

            if (direction.Y != 0)
            {
                int Y = y + (int)direction.Y * Game.WORLD_CHUNKSIZE_HALF + (direction.Y > 0 ? -1 : 0);
                int startX = x - Game.WORLD_CHUNKSIZE_HALF;
                int endX = startX + Game.WORLD_CHUNKSIZE;

                for (int X = startX; X < endX; X++)
                {
                    if (IsInBounds(X, Y))
                        Tiles.Add(tiles[X, Y]);
                }
            }

            return Tiles.Count > 0 ? Tiles.ToArray() : null;
        }

        public Tile[] GetTilesToUnload(Vector2 position, Vector2 direction) => GetTilesToUnload((int)position.X, (int)position.Y, direction);

        public Tile[] GetTilesToUnload(int x, int y, Vector2 direction)
        {
            List<Tile> unloadTiles = new List<Tile>();

            if (direction.X != 0)
            {
                int X = x - (int)direction.X * Game.WORLD_CHUNKSIZE_HALF + (direction.X > 0 ? -1 : 0);
                int startY = y - Game.WORLD_CHUNKSIZE_HALF;
                int endY = startY + Game.WORLD_CHUNKSIZE;

                for (int Y = startY; Y < endY; Y++)
                {
                    if (IsInBounds(X, Y))
                        unloadTiles.Add(tiles[X, Y]);
                }
            }

            if (direction.Y != 0)
            {
                int Y = y - (int)direction.Y * Game.WORLD_CHUNKSIZE_HALF + (direction.Y > 0 ? -1 : 0);
                int startX = x - Game.WORLD_CHUNKSIZE_HALF;
                int endX = startX + Game.WORLD_CHUNKSIZE;

                for (int X = startX; X < endX; X++)
                {
                    if (IsInBounds(X, Y))
                        unloadTiles.Add(tiles[X, Y]);
                }
            }

            return unloadTiles.Count > 0 ? unloadTiles.ToArray() : null;
        }

        public static void GenerateBaseMap(uint width, uint height)
        {
            //Test method to generate a base map
            World world = new World();
            world.id = (uint)Directory.GetFiles(Program.WORLDS_PATH).Length;
            world.bounds = new Vector2(width, height);
            world.tiles = new Tile[width, height];
            Random rnd = new Random();
            for (int z = 0; z < width; z++)
            {
                for (int x = 0; x < height; x++)
                {
                    Tile tile = new Tile();
                    tile.position = new Vector3(x, 0, z);
                    tile.type = rnd.Next((int)TileType.Grass, (int)TileType.Stone) > (int)TileType.Grass ? TileType.Dirt : TileType.Grass;
                    world.tiles[x, z] = tile;
                }
            }

            string json = JsonConvert.SerializeObject(world, Formatting.Indented);

            using (FileStream fs = File.Create($"{Program.WORLDS_PATH}{world.id}.json"))
            {
                byte[] jsonString = new UTF8Encoding(true).GetBytes(json);
                fs.Write(jsonString, 0, jsonString.Length);
            }
            Console.WriteLine($"Generated a new map @{Program.WORLDS_PATH}{world.id}.json");
        }
    }
}
