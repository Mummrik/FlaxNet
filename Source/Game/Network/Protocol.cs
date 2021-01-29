using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FlaxEngine;

namespace Game
{
    public class Protocol : Script
    {
        public string host = "127.0.0.1";
        public ushort port = 7171;

        public static CancellationTokenSource s_MasterToken;
        public static Connection client = null;

        public override void OnAwake()
        {
            s_MasterToken = new CancellationTokenSource();
            Packets.InitPacketList();
        }
        public override void OnStart()
        {
            Connect(host, port);
        }
        public override void OnDestroy()
        {
            Task.Run(() =>
            {
                NetworkMessage msg = new NetworkMessage(MsgType.Disconnect);
                msg.Write(client.GetId());
                msg.Send();
            }).Wait();

            client.Disconnect();

            s_MasterToken.Cancel();
        }
        private void Connect(string host, ushort port)
        {
            client = new Connection(host, port);

            NetworkMessage msg = new NetworkMessage(MsgType.HandShake);
            msg.Send();
        }
    }
}
