using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class Connection
    {
        private Guid m_Id;
        private UdpClient m_UdpClient;
        private IPEndPoint m_EndPoint;

        public bool Connected = false;
        public ConcurrentDictionary<Guid, NetworkMessage> reliableMsgs = new ConcurrentDictionary<Guid, NetworkMessage>();
        public Guid[] recentMsgs = new Guid[byte.MaxValue];
        public byte recentMsgIndex = default;
        public short ping;
        public Player player = null;

        public Connection(Guid id, UdpClient udpClient, IPEndPoint endPoint)
        {
            m_Id = id;
            m_UdpClient = udpClient;
            m_EndPoint = endPoint;

            Console.WriteLine($"[{DateTime.Now.ToString("H:mm:ss")}] Connection [{m_Id}] | {m_EndPoint}");

            NetworkMessage msg = new NetworkMessage(MsgType.HandShake);
            msg.Write(m_Id);
            msg.Write("Hello Client");
            msg.Send(this, true);
        }

        public Guid GetId() => m_Id;
        internal void Send(byte[] data)
        {
            m_UdpClient.BeginSend(data, data.Length, m_EndPoint, (ar) => { m_UdpClient.EndSend(ar); }, m_UdpClient);
        }

        public void Disconnect()
        {
            Console.WriteLine($"[{DateTime.Now.ToString("H:mm:ss")}] Disconnect [{m_Id}] | {m_EndPoint}");
            Protocol.s_Connections.Remove(m_Id);
        }
    }
}