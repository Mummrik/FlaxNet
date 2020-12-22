using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Numerics;
using System.Text;

namespace Server
{
    public class Creature
    {
        private uint id;
        private Vector3 position;
        private float speed = 5f;

        public Creature(uint id)
        {
            this.id = id;
        }

        public uint GetID() => id;
        public Vector3 GetPosition() => position;

        public void Move(Vector2 direction)
        {
            if (direction == Vector2.Zero)
                return;

            var dirNormalized = Vector2.Normalize(direction);
            position += new Vector3(dirNormalized.X, position.Y, dirNormalized.Y) * speed;

            NetworkMessage msg = new NetworkMessage(MsgType.MoveDirection);
            msg.Write(id);
            msg.Write(position);
            msg.Write(speed);
            //msg.Send(Protocol.connections[id], ProtocolType.Udp);

            foreach (var client in Protocol.connections.Values)
            {
                msg.Send(client, ProtocolType.Udp);
            }
        }
    }
}
