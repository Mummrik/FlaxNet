using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FlaxEngine;

namespace Game
{
    public class Protocol : Script
    {
        private UdpClient udpClient;
        private IPEndPoint endpoint;
        private List<byte> readBuffer = new List<byte>();
        private int packetSize;
        private List<Action> ActionList = new List<Action>();

        public const short BUFFER_SIZE = 8192;

        public string host = "127.0.0.1";
        [Limit(0, ushort.MaxValue)]
        public int port = 7171;

        public static Connection client = null;

        public UIControl ping = null;

        [Limit(0, 900)]
        public int simulateLatency = 0;
        public bool debugPackets = true;

        private float pingTimer = 0;

        public Prefab playerPrefab;

        public static Dictionary<uint, Player> players;

        public override void OnAwake()
        {
            Packets.InitPacketList(this);
            players = new Dictionary<uint, Player>();
            Connect(host, port);
        }
        public override void OnDestroy()
        {
            NetworkMessage msg = new NetworkMessage(MsgType.Disconnect);
            msg.Write(client.GetId());
            msg.Send();

            Connection.s_MasterToken.Cancel();
        }

        private void Connect(string host, int port)
        {
            endpoint = new IPEndPoint(IPAddress.Parse(host), port);
            udpClient = new UdpClient();
            client = new Connection(endpoint, udpClient);
        }

        public void ConnectUdp()
        {
            udpClient.Connect(endpoint);
            udpClient.BeginReceive(OnRead, udpClient);

            NetworkMessage msg = new NetworkMessage(MsgType.Connect);
            msg.Write(client.GetId());
            msg.Send(ProtocolType.Udp);
        }

        private void OnRead(IAsyncResult ar)
        {
            byte[] received = udpClient.EndReceive(ar, ref endpoint);

            if (received.Length > 0)
            {
                if (packetSize == default)
                {
                    byte[] size = new byte[sizeof(int)];
                    for (int i = 0; i < size.Length; i++)
                        size[i] = received[i];

                    packetSize = BitConverter.ToInt32(size, default);
                }

                foreach (byte b in received)
                {
                    if (readBuffer.Count < packetSize)
                        readBuffer.Add(b);
                    else
                        break;
                }

                if (readBuffer.Count >= packetSize)
                {
                    OnHandle(readBuffer.ToArray());
                    readBuffer.Clear();
                    packetSize = default;
                }
            }

            udpClient.BeginReceive(OnRead, udpClient);
        }

        private void OnHandle(byte[] data)
        {
            int latency = Packets.protocol.simulateLatency;
            if (latency > 0)
            {
                Thread.Sleep(latency);
            }

            NetworkMessage msg = new NetworkMessage(data);
            if (Packets.protocol.debugPackets)
            {
                Debug.Log($"Received MsgType: {msg.MsgType()}");
            }

            if (Packets.List.TryGetValue(msg.MsgType(), out Action<Connection, NetworkMessage> packet))
            {
                packet.Invoke(client, msg);
            }

        }

        public override void OnUpdate()
        {
            if (ActionList.Count > 0)
            {
                Action action = ActionList[0];
                action.Invoke();
                ActionList.Remove(action);
            }

            pingTimer -= Time.DeltaTime;
            if (pingTimer <= 0)
            {
                NetworkMessage msg = new NetworkMessage(MsgType.Ping);
                msg.Write(DateTime.Now.Ticks);
                msg.Send();
                pingTimer = 3;
            }
        }

        public void ExecuteAction(Action action)
        {
            ActionList.Add(action);
        }
    }
}
