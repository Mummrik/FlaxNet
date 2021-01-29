using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlaxEngine;

namespace Game
{
    public enum MsgType
    {
        Disconnect,
        HandShake,
        Notify,
        Connect,
        Message,
        Ping,
        PlayerData,
        Movement,
        RemoveCreature,
        Rotation,
        MapSector,
        MapData,
        UnloadMapData
    }
    internal class Packets
    {
        public static Dictionary<MsgType, Action<Connection, NetworkMessage>> List;

        internal static void InitPacketList()
        {
            List = new Dictionary<MsgType, Action<Connection, NetworkMessage>>();
            List.Add(MsgType.Disconnect, Disconnect);
            List.Add(MsgType.HandShake, HandShake);
            List.Add(MsgType.Notify, Notify);
            List.Add(MsgType.Connect, Connect);
            List.Add(MsgType.Message, Message);
            List.Add(MsgType.Ping, Ping);
            List.Add(MsgType.PlayerData, PlayerData);
            List.Add(MsgType.Movement, Movement);
            List.Add(MsgType.RemoveCreature, RemoveCreature);
            List.Add(MsgType.Rotation, Rotation);
            List.Add(MsgType.MapSector, MapSector);
            List.Add(MsgType.MapData, MapData);
            List.Add(MsgType.UnloadMapData, UnloadMapData);
        }

        private static void UnloadMapData(Connection client, NetworkMessage msg)
        {
            int length = msg.ReadInt();
            for (int i = 0; i < length; i++)
            {
                Vector3 tilePos = msg.ReadVector3();

                if (GameManager.s_Tiles.TryGetValue(tilePos, out Tile tile))
                {
                    Scripting.RunOnUpdate(() => GameManager.s_TilePool.UnRent(tile.Actor));
                }
            }
        }

        private static void MapData(Connection client, NetworkMessage msg)
        {
            int length = msg.ReadInt();
            for (int i = 0; i < length; i++)
            {
                Tile tileData = new Tile();
                tileData.Type = (TileType)msg.ReadByte();
                tileData.Position = msg.ReadVector3();

                if (!GameManager.s_Tiles.ContainsKey(tileData.Position))
                {
                    Scripting.RunOnUpdate(() => GameManager.s_TilePool.Rent(tileData));
                }
            }
        }

        private static void MapSector(Connection client, NetworkMessage msg)
        {
            //if (GameManager.s_Tiles.Count > 0)
            //{
            //    IEnumerable<Tile> tiles = GameManager.s_Tiles.Values;
            //    foreach (var tile in tiles)
            //    {
            //        GameManager.s_TilePool.UnRent(tile.Actor);
            //    }
            //}

            int length = msg.ReadInt();
            for (int i = 0; i < length; i++)
            {
                Tile tileData = new Tile();
                tileData.Type = (TileType)msg.ReadByte();
                tileData.Position = msg.ReadVector3();

                if (!GameManager.s_Tiles.ContainsKey(tileData.Position))
                {
                    Scripting.RunOnUpdate(() => GameManager.s_TilePool.Rent(tileData));
                }
            }
        }

        private static void Rotation(Connection client, NetworkMessage msg)
        {
        }

        private static void RemoveCreature(Connection client, NetworkMessage msg)
        {
            Guid cid = msg.ReadGuid();

            if (GameManager.s_Players.ContainsKey(cid))
            {
                Player player = GameManager.s_Players[cid];
                if (GameManager.s_Players.Remove(cid))
                {
                    Actor.Destroy(player.Actor);
                }
            }
        }

        private static void Movement(Connection client, NetworkMessage msg)
        {
            Guid cid = msg.ReadGuid();
            if (!GameManager.s_Players.ContainsKey(cid))
            {
                msg = new NetworkMessage(MsgType.PlayerData);
                msg.Write(cid);
                msg.Send();
                return;
            }

            Vector3 position = msg.ReadVector3();
            float speed = msg.ReadFloat();

            Player player = GameManager.s_Players[cid];
            player.position = position;
            player.moveSpeed = speed;

        }

        private static void PlayerData(Connection client, NetworkMessage msg)
        {
            Guid cid = msg.ReadGuid();
            if (!GameManager.s_Players.ContainsKey(cid))
            {
                Vector3 position = msg.ReadVector3();

                Prefab prefab = GameManager.s_Instance.playerPrefab;
                Actor actor = PrefabManager.SpawnPrefab(prefab, position);
                actor.Name = $"Player[{cid}]";

                Player player = actor.GetScript<Player>();
                player.id = cid;
                player.position = position;

                GameManager.s_Players.Add(cid, player);

                if (client.GetId() == cid)
                {
                    Scripting.RunOnUpdate(() =>
                    {
                        actor.AddScript<CameraManager>().SetFocusTarget(actor);
                        actor.AddScript<InputManager>();
                    });
                }
            }
        }

        private static void Ping(Connection client, NetworkMessage msg)
        {
            client.Ping = (short)(DateTime.Now - new DateTime(msg.ReadLong())).Milliseconds;
        }

        private static void Message(Connection client, NetworkMessage msg)
        {
        }

        private static void Connect(Connection client, NetworkMessage msg)
        {
        }

        private static void Notify(Connection client, NetworkMessage msg)
        {
            Guid packetId = msg.ReadGuid();
            if (client.reliableMsgs.ContainsKey(packetId))
                client.reliableMsgs.Remove(packetId);
        }

        private static void HandShake(Connection client, NetworkMessage msg)
        {
            Guid cid = msg.ReadGuid();
            string message = msg.ReadString();

            client.SetId(cid);
            Debug.Log(message);

            msg = new NetworkMessage(MsgType.HandShake);
            msg.Send(true);
            client.Connected = true;
        }

        private static void Disconnect(Connection client, NetworkMessage msg)
        {
            client.Connected = false;
        }
    }
}
