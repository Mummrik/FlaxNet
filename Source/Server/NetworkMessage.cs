using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class NetworkMessage : IDisposable
    {
        private Guid m_Id;
        private Guid m_ClientId;
        private MsgType m_MsgType;
        private List<byte> m_Body;
        private int m_Index;
        private int m_Size;
        private int m_FullSize;
        private bool m_SizeSet;

        public NetworkMessage(MsgType msgType)
        {
            if (m_Body == null)
                m_Body = new List<byte>();
            else
                m_Body.Clear();

            m_Index = default;
            m_Size = m_Body.Count;
            m_SizeSet = m_Size > 0;

            m_MsgType = msgType;
            Write((int)m_MsgType);
        }

        public NetworkMessage(byte[] data)
        {
            m_Body = new List<byte>(data);
            m_Index = default;
            //m_Index = sizeof(int);
            m_Size = m_Body.Count;
            m_SizeSet = m_Size > 0;

            m_FullSize = ReadInt();

            m_Id = ReadGuid();
            m_ClientId = ReadGuid();
            m_MsgType = (MsgType)ReadInt();
        }

        /// <summary>
        /// Get the MsgType this message contains
        /// </summary>
        /// <returns>MsgType that the packet list will invoke</returns>
        public MsgType MsgType() => m_MsgType;
        /// <summary>
        /// Get the full size of the packet
        /// </summary>
        /// <returns></returns>
        /// 
        public int Size() => m_Size;
        /// <summary>
        /// Get the Guid that the packet should be sent to.
        /// </summary>
        /// <returns></returns>
        public Guid GetClientId() => m_ClientId;
        /// <summary>
        /// Get the unique packet id, mainly used for reliable packets
        /// </summary>
        /// <returns></returns>
        public Guid PacketId() => m_Id;
        /// <summary>
        /// Get the packet body as byte array
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray() => m_Body.ToArray();

        public void Send(Connection receiver, bool isReliable = false)
        {
            if (m_SizeSet == false)
            {
                if (isReliable)
                {
                    m_Id = Guid.NewGuid();
                    receiver.reliableMsgs.TryAdd(m_Id, this);
                }

                m_ClientId = receiver.GetId();

                m_Body.InsertRange(default, m_ClientId.ToByteArray());
                m_Body.InsertRange(default, m_Id.ToByteArray());

                //m_Size += 32;
                m_Size += 32 + sizeof(int);   // add size of 2 guid (msgid and clientid) and the actual size of the msg body
                m_Body.InsertRange(default, BitConverter.GetBytes(m_Size));
                m_SizeSet = true;
            }

            Task.Run(() => { receiver.Send(ToArray()); });
        }

        #region Write
        public void Write(byte value)
        {
            m_Body.Add(value);
            m_Size = m_Body.Count;
        }
        public void Write(byte[] values)
        {
            m_Body.AddRange(values);
            m_Size = m_Body.Count;
        }
        public void Write(short value) => Write(BitConverter.GetBytes(value));
        public void Write(ushort value) => Write(BitConverter.GetBytes(value));
        public void Write(int value) => Write(BitConverter.GetBytes(value));
        public void Write(uint value) => Write(BitConverter.GetBytes(value));
        public void Write(long value) => Write(BitConverter.GetBytes(value));
        public void Write(ulong value) => Write(BitConverter.GetBytes(value));
        public void Write(bool value) => Write(BitConverter.GetBytes(value));
        public void Write(float value) => Write(BitConverter.GetBytes(value));
        public void Write(double value) => Write(BitConverter.GetBytes(value));
        public void Write(string value)
        {
            byte[] str = Encoding.Unicode.GetBytes(value);
            Write(str.Length);
            Write(str);
        }
        public void Write(Vector3 value)
        {
            Write(BitConverter.GetBytes(value.X));
            Write(BitConverter.GetBytes(value.Y));
            Write(BitConverter.GetBytes(value.Z));
        }
        public void Write(Vector2 value)
        {
            Write(BitConverter.GetBytes(value.X));
            Write(BitConverter.GetBytes(value.Y));
        }
        public void Write(Quaternion value)
        {
            Write(BitConverter.GetBytes(value.X));
            Write(BitConverter.GetBytes(value.Y));
            Write(BitConverter.GetBytes(value.Z));
            Write(BitConverter.GetBytes(value.W));
        }
        public void Write(Guid value)
        {
            Write(value.ToByteArray());

            //byte[] bytes = value.ToByteArray();
            //Write((byte)bytes.Length);
            //Write(bytes);
        }

        #endregion Write

        #region Read
        public byte ReadByte()
        {
            if (m_Index + sizeof(byte) > m_Size)
                return default;
            return m_Body.ElementAt(m_Index++);
        }
        public byte[] ReadBytes(int length)
        {
            byte[] bytes = new byte[length];
            Array.Copy(m_Body.ToArray(), m_Index, bytes, default, length);
            m_Index += length;
            return bytes;
        }
        public short ReadShort()
        {
            if (m_Index + sizeof(short) > m_Size)
                return default;
            return BitConverter.ToInt16(ReadBytes(sizeof(short)), default);
        }
        public ushort ReadUShort()
        {
            if (m_Index + sizeof(ushort) > m_Size)
                return default;
            return BitConverter.ToUInt16(ReadBytes(sizeof(ushort)), default);
        }
        public int ReadInteger() => ReadInt();
        public int ReadInt()
        {
            if (m_Index + sizeof(int) > m_Size)
                return default;
            return BitConverter.ToInt32(ReadBytes(sizeof(int)), default);
        }
        public uint ReadUInt()
        {
            if (m_Index + sizeof(uint) > m_Size)
                return default;
            return BitConverter.ToUInt32(ReadBytes(sizeof(uint)), default);
        }
        public long ReadLong()
        {
            if (m_Index + sizeof(long) > m_Size)
                return default;
            return BitConverter.ToInt64(ReadBytes(sizeof(long)), default);
        }
        public ulong ReadULong()
        {
            if (m_Index + sizeof(ulong) > m_Size)
                return default;
            return BitConverter.ToUInt64(ReadBytes(sizeof(ulong)), default);
        }
        public bool ReadBoolean() => ReadBool();
        public bool ReadBool()
        {
            if (m_Index + sizeof(bool) > m_Size)
                return default;
            return BitConverter.ToBoolean(ReadBytes(sizeof(bool)), default);
        }
        public float ReadSingle() => ReadFloat();
        public float ReadFloat()
        {
            if (m_Index + sizeof(float) > m_Size)
                return default;
            return BitConverter.ToSingle(ReadBytes(sizeof(float)), default);
        }
        public double ReadDouble()
        {
            if (m_Index + sizeof(double) > m_Size)
                return default;
            return BitConverter.ToDouble(ReadBytes(sizeof(double)), default);
        }
        public string ReadString()
        {
            return Encoding.Unicode.GetString(ReadBytes(ReadInt()));
        }
        public Vector3 ReadVector3()
        {
            float x = ReadFloat();
            float y = ReadFloat();
            float z = ReadFloat();

            return new Vector3(x, y, z);
        }
        public Vector2 ReadVector2()
        {
            float x = ReadFloat();
            float y = ReadFloat();

            return new Vector2(x, y);
        }
        public Quaternion ReadQuaternion()
        {
            float x = ReadFloat();
            float y = ReadFloat();
            float z = ReadFloat();
            float w = ReadFloat();

            return new Quaternion(x, y, z, w);
        }
        public Guid ReadGuid()
        {
            return new Guid(ReadBytes(16));
        }
        #endregion Read

        #region Dispose
        private bool isDisposed;
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
                return;

            if (disposing)
            {
                m_Body.Clear();
                m_Body = null;
                //m_Id = default;
                //m_ClientId = default;
                //m_MsgType = default;
                //m_Index = default;
                //m_Size = default;
            }
            isDisposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion Dispose
    }
}