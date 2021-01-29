using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FlaxEngine;

namespace Game
{
    public class Connection
    {
        private const bool DEBUG_PACKETS = false;

        private Guid m_Id;
        private UdpClient m_UdpClient;
        private IPEndPoint m_EndPoint;
        private Guid[] m_RecentMsgs = new Guid[byte.MaxValue];
        private byte m_RecentMsgIndex = default;
        private short m_Ping = -1;

        public bool Connected = false;
        public Dictionary<Guid, NetworkMessage> reliableMsgs = new Dictionary<Guid, NetworkMessage>();

        public short Ping
        {
            get => m_Ping;
            set
            {
                if (m_Ping != value)
                {
                    m_Ping = value;
                    PingText.s_PingText.Text = $"Ping: {m_Ping}";
                    PingText.s_PingText.TextColor = m_Ping < 50 ? Color.LimeGreen : m_Ping < 150 ? Color.Yellow : Color.Red;
                }
            }
        }

        public Connection(string host, ushort port)
        {
            m_EndPoint = new IPEndPoint(IPAddress.Parse(host), port);
            m_UdpClient = new UdpClient();

            Task.Run(() => { m_UdpClient.Connect(m_EndPoint); }).Wait();
            m_UdpClient.BeginReceive(OnRead, m_UdpClient);
            new Thread(new ThreadStart(ReliableLoop)).Start();
        }

        private void ReliableLoop()
        {
            float pingTimer = 0;
            while (!Protocol.s_MasterToken.IsCancellationRequested)
            {
                if (reliableMsgs.Count > 0)
                {
                    var msgs = reliableMsgs.Values;
                    foreach (var msg in msgs)
                    {
                        msg.Send();
                    }
                }

                if (Connected)
                {
                    pingTimer += 0.1f;
                    if (pingTimer > 3)
                    {
                        NetworkMessage msg = new NetworkMessage(MsgType.Ping);
                        msg.Write(Ping);
                        msg.Write(DateTime.Now.Ticks);
                        msg.Send();
                        pingTimer = default;
                    }
                }

                Thread.Sleep(100);
            }
        }

        public Guid GetId() => m_Id;
        public void SetId(Guid cid)
        {
            if (m_Id != Guid.Empty)
                return;

            m_Id = cid;
        }

        private void OnRead(IAsyncResult ar)
        {
            byte[] data = m_UdpClient.EndReceive(ar, ref m_EndPoint);
            if (data.Length > 0)
            {
                NetworkMessage msg = null;

                try
                {
                    msg = new NetworkMessage(data);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"\n>> Warning: Couldn't create networkmessage of received data from [{m_EndPoint}]\n{e}");
                    m_UdpClient.BeginReceive(OnRead, m_UdpClient);
                    return;
                }

                OnHandle(msg);
            }

            m_UdpClient.BeginReceive(OnRead, m_UdpClient);
        }

        private void OnHandle(NetworkMessage msg)
        {
            if (msg.PacketId() != Guid.Empty && MsgHasBeenHandle(msg.PacketId()))
            {
                Notify(msg.PacketId());
                return;
            }

            if (Packets.List.TryGetValue(msg.MsgType(), out Action<Connection, NetworkMessage> packet))
            {
                Task.Run(() => packet.Invoke(this, msg)).Wait();
            }

            if (msg.PacketId() != Guid.Empty)
            {
                m_RecentMsgs[m_RecentMsgIndex++] = msg.PacketId();
                if (m_RecentMsgIndex == m_RecentMsgs.Length - 1)
                    m_RecentMsgIndex = default;

                Notify(msg.PacketId());
            }

            if (DEBUG_PACKETS)
                Debug.Log($"[UDP] Connection [{m_Id}] Received MsgType: {msg.MsgType()}");

            msg.Dispose();
        }

        private bool MsgHasBeenHandle(Guid msgId)
        {
            foreach (var id in m_RecentMsgs)
                if (id == msgId)
                    return true;

            return false;
        }

        private void Notify(Guid packetId)
        {
            NetworkMessage notify = new NetworkMessage(MsgType.Notify);
            notify.Write(packetId);
            notify.Send();
        }

        internal void Send(byte[] data)
        {
            m_UdpClient.BeginSend(data, data.Length, (ar) => m_UdpClient.EndSend(ar), m_UdpClient);
        }

        public void Disconnect()
        {
            Connected = false;
            //m_UdpClient.Client.Disconnect(false);
        }
    }
}
