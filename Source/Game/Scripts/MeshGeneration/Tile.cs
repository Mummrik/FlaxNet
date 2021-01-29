using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game
{
    public enum TileType
    {
        Default,
        Grass,
        Dirt,
        Stone,
        Water
    }

    public class Tile : Script
    {
        private TileType type = TileType.Default;
        private Vector3 position;

        public TileType Type
        {
            get => type;
            set
            {
                if (value != type)
                {
                    type = value;
                    if (Actor != null)
                        Actor?.As<StaticModel>().SetMaterial(0, Type == TileType.Grass ? GameManager.s_Instance.grass : GameManager.s_Instance.dirt);
                }
            }
        }

        public Vector3 Position
        {
            get => position;
            set
            {
                if (position != value)
                {
                    position = value;
                    if (Actor != null)
                        Actor.Position = value;
                }
            }
        }

        public override void OnStart()
        {
            var Model = Actor.As<StaticModel>();
            Model.Model = MeshManager.tileMesh;
        }
    }
}
