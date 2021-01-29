using System;
using System.Collections.Generic;
using System.Numerics;

namespace Server
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
        }

        private static void MapData(Connection client, NetworkMessage msg)
        {
        }

        private static void MapSector(Connection client, NetworkMessage msg)
        {
            
        }

        private static void Rotation(Connection client, NetworkMessage msg)
        {
        }

        private static void RemoveCreature(Connection client, NetworkMessage msg)
        {
        }

        private static void Movement(Connection client, NetworkMessage msg)
        {
            Vector2 direction = msg.ReadVector2();
            client.player.Move(direction);
        }

        private static void PlayerData(Connection client, NetworkMessage msg)
        {
            Guid cid = msg.ReadGuid();
            Player player = Protocol.s_Connections[cid].player;

            if (player != null)
            {
                msg = new NetworkMessage(MsgType.PlayerData);
                msg.Write(player.Id);
                msg.Write(player.Position);
                msg.Send(client, true);
            }
        }

        private static void Ping(Connection client, NetworkMessage msg)
        {
            client.ping = msg.ReadShort();
            long time = msg.ReadLong();

            msg = new NetworkMessage(MsgType.Ping);
            msg.Write(time);
            msg.Send(client);
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
                client.reliableMsgs.TryRemove(packetId, out var value);
        }

        private static void HandShake(Connection client, NetworkMessage msg)
        {
            client.Connected = true;
            
            // Send a start ping
            msg = new NetworkMessage(MsgType.Ping);
            msg.Write(DateTime.Now.Ticks);
            msg.Send(client);

            //Create new player and send to all clients
            Player player = new Player(client.GetId());
            client.player = player;

            msg = new NetworkMessage(MsgType.PlayerData);
            msg.Write(player.Id);
            msg.Write(player.Position);
            Protocol.SendToAll(msg, null, true);

            //Get all connected players and send to new connected client
            ICollection<Connection> clients = Protocol.s_Connections.Values;
            foreach (var c in clients)
            {
                if (c.player != null)
                {
                    msg = new NetworkMessage(MsgType.PlayerData);
                    msg.Write(c.player.Id);
                    msg.Write(c.player.Position);
                    msg.Send(client, true);
                }
            }

            //Send map sector to the new player
            Tile[] tiles = Game.s_WorldInstances[player.WorldInstance].GetMapSector(player);
            msg = new NetworkMessage(MsgType.MapSector);
            msg.Write(tiles.Length);

            foreach (var tile in tiles)
            {
                msg.Write((byte)tile.type);
                msg.Write(tile.position * Game.WORLD_TILESIZE);
            }
            msg.Send(client, true);

        }

        private static void Disconnect(Connection client, NetworkMessage msg)
        {
            client.Connected = false;
            Guid cid = msg.ReadGuid();

            msg = new NetworkMessage(MsgType.RemoveCreature);
            msg.Write(cid);
            Protocol.SendToAll(msg, client, true);

            client.Disconnect();
        }
    }
}