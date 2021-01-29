using System;
using System.Collections.Generic;
using System.Numerics;

namespace Server
{
    public class Creature
    {
        public Creature(Guid id) => m_Id = id;

        protected Guid m_Id;
        protected Vector3 m_Position;
        protected Quaternion m_Rotation;
        protected List<Creature> m_Spectators = new List<Creature>();
        protected float m_Speed = 5;
        protected ushort m_WorldInstance;

        public Guid Id { get => m_Id; }
        public Vector3 Position
        {
            get => m_Position;
            set
            {
                if (m_Position != value)
                {
                    m_Position = value;

                    NetworkMessage msg = new NetworkMessage(MsgType.Movement);
                    msg.Write(Id);
                    msg.Write(Position);
                    msg.Write(m_Speed);
                    Protocol.SendToAll(msg);
                }
            }
        }

        public Quaternion Rotation
        {
            get => m_Rotation;
            set
            {
                if (m_Rotation != value)
                {
                    m_Rotation = value;

                    //NetworkMessage msg = new NetworkMessage(MsgType.Movement);
                    //msg.Write(Id);
                    //msg.Write(Position);
                    //msg.Write(m_Speed);
                    //Protocol.SendToAll(msg);
                }
            }
        }

        public ushort WorldInstance { get => m_WorldInstance; protected set => m_WorldInstance = value; }

        public virtual void Move(Vector2 direction)
        {

        }
    }
}