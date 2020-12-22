using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class Connection
    {
        private uint id;
        private TcpClient tcpClient;
        private UdpClient udpClient;
        public IPEndPoint endpoint;

        private byte[] buffer = new byte[Protocol.BUFFER_SIZE];
        private List<byte> ReadBuffer = new List<byte>();
        private int packetSize;

        public Creature player;
        public uint Id { get => id; }
        public Connection(uint uid, TcpClient newClient, UdpClient refUdpClient)
        {
            id = uid;
            tcpClient = newClient;
            udpClient = refUdpClient;
            endpoint = (IPEndPoint)tcpClient.Client.RemoteEndPoint;

            Console.WriteLine($"[{DateTime.Now.ToString("H:mm:ss")}] Connection [{id}] | {endpoint}");

            tcpClient.GetStream().BeginRead(buffer, default, buffer.Length, OnRead, tcpClient);

            //TODO: Send Handshake message

            // Test message
            NetworkMessage msg = new NetworkMessage(MsgType.Connect);
            msg.Write(id);
            msg.Send(this);
        }


        private void OnRead(IAsyncResult ar)
        {
            int bytes = tcpClient.GetStream().EndRead(ar);

            if (bytes > 0)
            {
                if (packetSize == default)
                {
                    byte[] size = new byte[sizeof(int)];
                    for (int i = 0; i < size.Length; i++)
                        size[i] = buffer[i];

                    packetSize = BitConverter.ToInt32(size, default);
                }

                foreach (byte b in buffer)
                {
                    if (ReadBuffer.Count < packetSize)
                        ReadBuffer.Add(b);
                    else
                        break;
                }

                if (ReadBuffer.Count >= packetSize)
                {
                    OnHandle(ReadBuffer.ToArray());
                    ReadBuffer.Clear();
                    packetSize = default;
                }
            }

            if (tcpClient.Connected)
            {
                tcpClient.GetStream().BeginRead(buffer, default, buffer.Length, OnRead, tcpClient);
            }
        }

        private void OnHandle(byte[] data)
        {
            NetworkMessage msg = new NetworkMessage(data);

            // Debug what packets received
            //if (msg.MsgType() != MsgType.Ping)
            //    Console.WriteLine($"[TCP] Connection [{id}] Received MsgType: {msg.MsgType()}");

            if (Packets.List.TryGetValue(msg.MsgType(), out Action<Connection, NetworkMessage> packet))
            {
                packet.Invoke(this, msg);
            }
            else
            {
                Disconnect();
            }

            msg.Dispose();
        }

        public async void Send(byte[] data, ProtocolType protocol = ProtocolType.Tcp)
        {
            if (tcpClient != null && tcpClient.Connected)
            {
                if (protocol == ProtocolType.Udp)
                {
                    udpClient.BeginSend(data, data.Length, endpoint, (ar) => { udpClient.EndSend(ar); }, udpClient);
                }
                else
                {
                    tcpClient.Client.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) => { tcpClient.Client.EndSend(ar); }, tcpClient);
                }
            }
        }

        public void Disconnect()
        {
            Console.WriteLine($"[{DateTime.Now.ToString("H:mm:ss")}] Disconnect [{id}] | {tcpClient.Client.RemoteEndPoint}");
            Protocol.connections.Remove(id);
            tcpClient.Close();
        }
    }
}