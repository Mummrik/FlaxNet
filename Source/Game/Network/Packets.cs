using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game
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
        public static Protocol protocol;

        public static Dictionary<MsgType, Action<Connection, NetworkMessage>> List;

        public static void InitPacketList(Protocol owner)
        {
            protocol = owner;
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
            uint id = msg.ReadUInt();
            Vector3 position = msg.ReadVector3();
            float speed = msg.ReadFloat();

            Player player = Protocol.players[id];
            player.position = position;
            player.moveSpeed = speed;
        }

        private static void PlayerData(Connection client, NetworkMessage msg)
        {
            Prefab prefab = protocol.playerPrefab;
            uint id = msg.ReadUInt();
            Vector3 position = msg.ReadVector3();

            Actor actor = PrefabManager.SpawnPrefab(prefab, position);
            actor.Name = $"Player[{id}]";
            Protocol.players.Add(id, actor.GetScript<Player>());

            if (client.GetId() == id)
                protocol.ExecuteAction(() =>
                {
                    var inputManager = actor.AddScript<InputManager>();
                    inputManager.owner = actor.GetScript<Player>();
                });
        }

        private static void HandShake(Connection client, NetworkMessage msg)
        {
        }

        private static void Ping(Connection client, NetworkMessage msg)
        {
            int latency = (DateTime.Now - new DateTime(msg.ReadLong())).Milliseconds;
            protocol.ping.Get<FlaxEngine.GUI.Label>().Text = $"Ping: {latency}ms";
        }

        private static void Message(Connection client, NetworkMessage msg)
        {

        }

        private static void Connect(Connection client, NetworkMessage msg)
        {
            uint id = msg.ReadUInt();
            client.SetId(id);
            protocol.ConnectUdp();
        }

        private static void Disconnect(Connection client, NetworkMessage msg)
        {

        }
    }
}