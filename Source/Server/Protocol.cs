using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    internal class Protocol
    {
        private TcpListener listener = null;
        private UdpClient udpClient = null;
        private static IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);

        public static IPEndPoint GetEndPoint() => endpoint;
        public static Dictionary<uint, Connection> connections = null;
        public static bool debugPackages = false;

        public const short BUFFER_SIZE = 8192;

        public Protocol(IPAddress host, uint port)
        {
            Start(host, (int)port);
        }

        private void Start(IPAddress host, int port)
        {
            if (listener == null)
            {
                Console.WriteLine(">> Initialize Protocol...");
                Packets.InitPacketList();

                connections = new Dictionary<uint, Connection>();

                udpClient = new UdpClient(port);

                listener = new TcpListener(host, port);
                listener.Start();
                listener.BeginAcceptTcpClient(OnAccept, listener);
                udpClient.BeginReceive(OnRead, udpClient);
            }
        }

        private void OnRead(IAsyncResult ar)
        {
            byte[] received = udpClient.EndReceive(ar, ref endpoint);

            if (received.Length > 0)
            {
                byte[] size = new byte[sizeof(uint)];
                for (int i = 0; i < size.Length; i++)
                    size[i] = received[i];

                uint id = BitConverter.ToUInt32(size, default);

                byte[] data = new byte[received.Length - sizeof(uint)];
                Array.Copy(received, sizeof(uint), data, default, received.Length - sizeof(uint));
                OnHandle(data, connections[id]);
            }

            udpClient.BeginReceive(OnRead, udpClient);
        }

        private void OnHandle(byte[] data, Connection client)
        {
            NetworkMessage msg = new NetworkMessage(data);

            // Debug what packets received
            if (debugPackages && msg.MsgType() != MsgType.Ping)
                Console.WriteLine($"[UDP] Connection [{client.Id}] Received MsgType: {msg.MsgType()}");

            if (Packets.List.TryGetValue(msg.MsgType(), out Action<Connection, NetworkMessage> packet))
            {
                packet.Invoke(client, msg);
            }

            msg.Dispose();
        }

        private void OnAccept(IAsyncResult ar)
        {
            TcpClient newClient = listener.EndAcceptTcpClient(ar);

            uint uid = GenerateUID();
            connections.Add(uid, new Connection(uid, newClient, udpClient));

            listener.BeginAcceptTcpClient(OnAccept, listener);
        }

        private uint GenerateUID()
        {
            uint uid = 1;
            while (connections.ContainsKey(uid))
                uid++;

            return uid;
        }

        private void Stop()
        {
            listener.Stop();
            udpClient.Close();

            ICollection<Connection> _connections = connections.Values;
            foreach (var client in _connections)
                if (client != null)
                    client.Disconnect();

            connections.Clear();
        }
    }
}