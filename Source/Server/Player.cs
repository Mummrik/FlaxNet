using System;
using System.Numerics;

namespace Server
{
    public class Player : Creature
    {
        private Vector2 m_TilePosition;
        public Player(Guid id) : base(id)
        {
            Vector2 bounds = Game.s_WorldInstances[WorldInstance].bounds * Game.WORLD_TILESIZE * 0.5f;
            Position = new Vector3(bounds.X, 0, bounds.Y);
            m_TilePosition = new Vector2((int)Position.X, (int)Position.Z) * 0.01f;
        }

        public Vector2 TilePosition
        {
            get => m_TilePosition;
            protected set
            {
                Vector2 position = new Vector2((int)value.X, (int)value.Y);
                if (m_TilePosition != position)
                {
                    Vector2 direction = position - m_TilePosition;
                    m_TilePosition = position;

                    if (direction != Vector2.Zero)
                    {
                        // Tiles to unload on client side
                        Tile[] unloadTiles = Game.s_WorldInstances[WorldInstance].GetTilesToUnload(m_TilePosition, direction);
                        if (unloadTiles != null)
                        {
                            NetworkMessage msg = new NetworkMessage(MsgType.UnloadMapData);
                            msg.Write(unloadTiles.Length);

                            foreach (var tile in unloadTiles)
                            {
                                msg.Write(tile.position * Game.WORLD_TILESIZE);
                            }
                            msg.Send(Protocol.s_Connections[Id], true);
                        }

                        // Tiles to load on client side
                        Tile[] Tiles = Game.s_WorldInstances[WorldInstance].GetTilesToLoad(m_TilePosition, direction);
                        if (Tiles != null)
                        {
                            NetworkMessage msg = new NetworkMessage(MsgType.MapData);
                            msg.Write(Tiles.Length);

                            foreach (var tile in Tiles)
                            {
                                msg.Write((byte)tile.type);
                                msg.Write(tile.position * Game.WORLD_TILESIZE);
                            }
                            msg.Send(Protocol.s_Connections[Id], true);
                        }
                    }
                }
            }
        }

        public Quaternion CameraRotation
        {
            get => CameraRotation;
            set
            {
                CameraRotation = value;
            }
        }

        public override void Move(Vector2 direction)
        {
            if (direction == Vector2.Zero)
                return;

            direction = Vector2.Normalize(direction);
            var newPosition = new Vector3(direction.X, Position.Y, direction.Y) * m_Speed;

            if (Game.s_WorldInstances[m_WorldInstance].IsInBounds(Position + newPosition))
            {
                Position += newPosition;
                TilePosition = new Vector2((int)Position.X, (int)Position.Z) * 0.01f;
            }
        }
    }
}