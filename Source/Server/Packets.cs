using System;
using System.Collections.Generic;
using System.Numerics;

namespace Server
{
    public enum MsgType
    {
        Disconnect,
        HandShake,
        Connect,
        Message,
        Ping,
        PlayerData,
        MoveDirection,

    }
    class Packets
    {
        public static Dictionary<MsgType, Action<Connection, NetworkMessage>> List;

        public static void InitPacketList()
        {
            List = new Dictionary<MsgType, Action<Connection, NetworkMessage>>();
            List.Add(MsgType.Disconnect, Disconnect);
            List.Add(MsgType.HandShake, HandShake);
            List.Add(MsgType.Connect, Connect);
            List.Add(MsgType.Message, Message);
            List.Add(MsgType.Ping, Ping);
            List.Add(MsgType.PlayerData, PlayerData);
            List.Add(MsgType.MoveDirection, MoveDirection);
        }

        private static void MoveDirection(Connection client, NetworkMessage msg)
        {
            Vector2 direction = msg.ReadVector2();
            Creature player = client.player;
            player.Move(direction);
        }

        private static void PlayerData(Connection client, NetworkMessage msg)
        {
        }

        private static void Ping(Connection client, NetworkMessage msg)
        {
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
            client.endpoint = Protocol.GetEndPoint();
            Creature player = new Creature(client.Id);
            client.player = player;

            msg = new NetworkMessage(MsgType.PlayerData);
            msg.Write(player.GetID());
            msg.Write(player.GetPosition());
            //msg.Send(client);

            //Testing
            foreach (var c in Protocol.connections.Values)
            {
                if (c != null)
                {
                    msg.Send(c);
                }
            }

            foreach (var c in Protocol.connections.Values)
            {
                if (c == client)
                {
                    continue;
                }
                Creature p = c.player;
                NetworkMessage msg2 = new NetworkMessage(MsgType.PlayerData);
                msg2.Write(p.GetID());
                msg2.Write(p.GetPosition());
                msg2.Send(client, System.Net.Sockets.ProtocolType.Udp);
            }
        }

        private static void HandShake(Connection client, NetworkMessage msg)
        {
        }

        private static void Disconnect(Connection client, NetworkMessage msg)
        {
            uint playerId = msg.ReadUInt();
            client.Disconnect();

        }
    }
}