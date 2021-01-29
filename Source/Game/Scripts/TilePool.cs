using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game
{
    public class TilePool : ObjectPool<Actor>
    {
        public TilePool()
        {
            // Prime Pool with tiles
            int amount = 32 * 32;
            for (int i = 0; i < amount; i++)
            {
                Actor actor = new StaticModel();
                actor.SetParent(GameManager.s_WorldParent, true);
                actor.As<StaticModel>().Model = MeshManager.tileMesh;
                actor.AddScript<Tile>();
                actor.IsActive = false;
                Return(actor);
            }
        }

        public Actor Rent(Tile tile)
        {
            var poolObj = Get();

            if (poolObj == null)
                return CreateNewTile(tile);

            return OnSpawn(poolObj, tile);
        }

        public void UnRent(Actor tileObj)
        {
            tileObj.IsActive = false;
            GameManager.s_Tiles.Remove(tileObj.Position);
            Return(tileObj);
        }

        private Actor CreateNewTile(Tile tileData)
        {
            Actor actor = new StaticModel();
            actor.SetParent(GameManager.s_WorldParent, true);
            actor.As<StaticModel>().Model = MeshManager.tileMesh;

            return OnSpawn(actor, tileData);
        }

        private Actor OnSpawn(Actor poolObj, Tile tileData)
        {
            Tile tile = poolObj.GetScript<Tile>();

            if (tile == null)
            {
                tile = poolObj.AddScript<Tile>();
            }

            tile.Type = tileData.Type;
            tile.Position = tileData.Position;

            if (!GameManager.s_Tiles.ContainsKey(tile.Position))
                GameManager.s_Tiles.Add(tile.Position, tile);

            poolObj.IsActive = true;
            return poolObj;
        }
    }
}
