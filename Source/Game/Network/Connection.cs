using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FlaxEngine;

namespace Game
{
    public class Connection
    {
        private uint id;
        private TcpClient tcpClient = null;
        private UdpClient udpClient = null;

        private byte[] buffer = new byte[Protocol.BUFFER_SIZE];
        private List<byte> ReadBuffer = new List<byte>();
        private int packetSize;

        public static CancellationTokenSource s_MasterToken;

        public Connection(IPEndPoint endpoint, UdpClient refUdpClient)
        {
            s_MasterToken = new CancellationTokenSource();
            tcpClient = new TcpClient();

            udpClient = refUdpClient;

            tcpClient.Connect(endpoint);
            tcpClient.GetStream().BeginRead(buffer, default, buffer.Length, OnRead, tcpClient);

        }

        public uint GetId() => id;
        public void SetId(uint newId)
        {
            if (id != 0)
                return;

            id = newId;
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
                tcpClient.GetStream().BeginRead(buffer, 0, buffer.Length, OnRead, tcpClient);
            }
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
                packet.Invoke(this, msg);
            }
            else
            {
                Disconnect();
            }
        }

        public void Send(byte[] data, ProtocolType protocol = ProtocolType.Tcp)
        {
            if (tcpClient != null && tcpClient.Connected)
                if (protocol == ProtocolType.Udp)
                {
                    udpClient.BeginSend(data, data.Length, (ar) =>
                    {
                        if (tcpClient.Connected)
                            udpClient.EndSend(ar);
                    }, udpClient);
                }
                else
                {
                    tcpClient.Client.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
                    {
                        if (tcpClient.Connected)
                            tcpClient.Client.EndSend(ar);
                    }, tcpClient);
                }
        }

        private void Disconnect()
        {
            tcpClient.Close();
        }
    }
}
