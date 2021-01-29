using System.Numerics;

namespace Server
{
    public enum TileType
    {
        Default,
        Grass,
        Dirt,
        Stone,
        Water
    }

    public struct Tile
    {
        public TileType type;
        public Vector3 position;
        //public Dictionary<uint, GameObject> content;

    }
}