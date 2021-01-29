using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    internal class Protocol
    {
        private const bool DEBUG_PACKETS = false;

        private UdpClient m_UdpClient = null;
        private IPEndPoint m_EndPoint = new IPEndPoint(IPAddress.Any, 0);

        public static Dictionary<Guid, Connection> s_Connections = null;

        public Protocol(ushort port)
        {
            Start(port);
        }

        private void Start(ushort port)
        {
            if (!Program.s_MasterToken.IsCancellationRequested)
            {
                Console.WriteLine(">> Initialize Protocol...");
                Packets.InitPacketList();

                s_Connections = new Dictionary<Guid, Connection>();
                m_UdpClient = new UdpClient(port);
                new Thread(new ThreadStart(ReliableLoop)).Start();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n\tServer is Live!\n");
                Console.ResetColor();
            }
        }

        private void Stop()
        {
            ICollection<Connection> clients = s_Connections.Values;
            foreach (var client in clients)
                if (client != null)
                    client.Disconnect();

            s_Connections.Clear();

            m_UdpClient.Close();
        }

        private void ReliableLoop()
        {
            m_UdpClient.BeginReceive(OnRead, m_UdpClient);
            while (!Program.s_MasterToken.IsCancellationRequested)
            {
                ICollection<Connection> clients = s_Connections.Values;
                foreach (var client in clients)
                {
                    if (client.reliableMsgs.Count > 0)
                    {
                        ICollection<NetworkMessage> msgs = client.reliableMsgs.Values;
                        foreach (var msg in msgs)
                        {
                            msg.Send(client);
                        }
                    }
                }
                Thread.Sleep(100);
            }

            Stop();
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
                    Console.WriteLine($"\n>> Warning: Couldn't create networkmessage of received data from [{m_EndPoint}]\n{e}");
                    m_UdpClient.BeginReceive(OnRead, m_UdpClient);
                    return;
                }

                if (!s_Connections.ContainsKey(msg.GetClientId()))
                {
                    Guid id = Guid.NewGuid();
                    Connection newClient = new Connection(id, m_UdpClient, m_EndPoint);
                    s_Connections.Add(id, newClient);
                    m_UdpClient.BeginReceive(OnRead, m_UdpClient);
                    return;
                }

                OnHandle(s_Connections[msg.GetClientId()], msg);
            }

            m_UdpClient.BeginReceive(OnRead, m_UdpClient);
        }

        private void OnHandle(Connection client, NetworkMessage msg)
        {
            if (msg.PacketId() != Guid.Empty && MsgHasBeenHandle(client, msg.PacketId()))
            {
                Notify(client, msg.PacketId());
                return;
            }

            if (Packets.List.TryGetValue(msg.MsgType(), out Action<Connection, NetworkMessage> packet))
            {
                Task.Run(() => packet.Invoke(client, msg)).Wait();
            }

            if (msg.PacketId() != Guid.Empty)
            {
                client.recentMsgs[client.recentMsgIndex++] = msg.PacketId();
                if (client.recentMsgIndex == client.recentMsgs.Length - 1)
                    client.recentMsgIndex = default;

                Notify(client, msg.PacketId());
            }

            if (DEBUG_PACKETS)
                Console.WriteLine($"[UDP] Connection [{client.GetId()}] Received MsgType: {msg.MsgType()}");

            msg.Dispose();
        }

        private bool MsgHasBeenHandle(Connection client, Guid packetId)
        {
            foreach (var id in client.recentMsgs)
                if (id == packetId)
                    return true;

            return false;
        }

        private void Notify(Connection receiver, Guid packetId)
        {
            NetworkMessage notify = new NetworkMessage(MsgType.Notify);
            notify.Write(packetId);
            notify.Send(receiver);
        }

        public static void SendToAll(NetworkMessage msg, Connection ignore = null, bool isReliable = false)
        {
            ICollection<Connection> clients = s_Connections.Values;
            foreach (var client in clients)
                if (client != ignore)
                    msg.Send(client, isReliable);
        }

    }
}