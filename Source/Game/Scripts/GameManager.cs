using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game
{
    public class GameManager : Script
    {
        public static GameManager s_Instance;
        public static Dictionary<Guid, Player> s_Players;
        public static Dictionary<Vector3, Tile> s_Tiles;
        public static EmptyActor s_WorldParent;
        public static TilePool s_TilePool;

        //TODO: Make prefab list
        public Prefab playerPrefab;

        public Material grass;
        public Material dirt;

        public override void OnAwake()
        {
            if (s_Instance != null)
            {
                Debug.Log("Found more than one GameManager script!");
                Destroy(this);
                return;
            }
            else
                s_Instance = this;

            s_WorldParent = new EmptyActor();
            s_WorldParent.Name = "World";
            Level.SpawnActor(s_WorldParent);

            MeshManager.Init();

            s_TilePool = new TilePool();
            s_Players = new Dictionary<Guid, Player>();
            s_Tiles = new Dictionary<Vector3, Tile>();
        }

        public override void OnDestroy()
        {
            if (s_Instance == this)
            {
                s_Instance = null;
                MeshManager.Unload();
            }
        }
    }
}
