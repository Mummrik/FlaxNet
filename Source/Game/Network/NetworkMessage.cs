using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;

namespace Game
{
    public class NetworkMessage : IDisposable
    {
        private List<byte> body;
        private MsgType msgType;
        private int size;
        private bool sizeSet;
        private int index;

        /// <summary>
        /// Create a new network message, that later on can be sent to a client.
        /// </summary>
        /// <param name="type">Indicate what type of message this will be.</param>
        public NetworkMessage(MsgType type)
        {
            if (body != null)
                body.Clear();

            body = new List<byte>();
            size = default;
            sizeSet = false;
            index = default;

            msgType = type;
            body.InsertRange(default, BitConverter.GetBytes((int)msgType));
        }

        /// <summary>
        /// Create a new network message, with data that a client has sent.
        /// </summary>
        /// <param name="data">Array of data, that a client sent.</param>
        public NetworkMessage(byte[] data)
        {
            body = new List<byte>(data);
            size = body.Count();
            sizeSet = true;
            index = sizeof(int);

            msgType = (MsgType)ReadInt();
        }

        public MsgType MsgType() => msgType;
        public int Size() => size;
        public void Send(ProtocolType protocol = ProtocolType.Tcp)
        {
            if (!sizeSet)
            {
                size += protocol == ProtocolType.Udp ? (sizeof(uint) + sizeof(int)) : sizeof(int);
                body.InsertRange(default, BitConverter.GetBytes(size));
                sizeSet = true;

                if (protocol == ProtocolType.Udp)
                    body.InsertRange(default, BitConverter.GetBytes(Protocol.client.GetId()));
            }
            Task.Run(() => Protocol.client.Send(ToArray(), protocol));
        }
        public byte[] ToArray() => body.ToArray();

        #region Write
        public void Write(byte value)
        {
            body.Add(value);
            size = body.Count();
        }
        public void Write(byte[] values)
        {
            body.AddRange(values);
            size = body.Count();
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
            byte[] bytes = value.ToByteArray();
            Write((byte)bytes.Length);
            Write(bytes);
        }
        #endregion Write

        #region Read
        public byte ReadByte()
        {
            if (index + sizeof(byte) > size)
                return default;
            return body.ElementAt(index++);
        }
        public byte[] ReadBytes(int length)
        {
            byte[] bytes = new byte[length];
            Array.Copy(body.ToArray(), index, bytes, default, length);
            index += length;
            return bytes;
        }
        public short ReadShort()
        {
            if (index + sizeof(short) > size)
                return default;
            return BitConverter.ToInt16(ReadBytes(sizeof(short)), default);
        }
        public ushort ReadUShort()
        {
            if (index + sizeof(ushort) > size)
                return default;
            return BitConverter.ToUInt16(ReadBytes(sizeof(ushort)), default);
        }
        public int ReadInteger() => ReadInt();
        public int ReadInt()
        {
            if (index + sizeof(int) > size)
                return default;
            return BitConverter.ToInt32(ReadBytes(sizeof(int)), default);
        }
        public uint ReadUInt()
        {
            if (index + sizeof(uint) > size)
                return default;
            return BitConverter.ToUInt32(ReadBytes(sizeof(uint)), default);
        }
        public long ReadLong()
        {
            if (index + sizeof(long) > size)
                return default;
            return BitConverter.ToInt64(ReadBytes(sizeof(long)), default);
        }
        public ulong ReadULong()
        {
            if (index + sizeof(ulong) > size)
                return default;
            return BitConverter.ToUInt64(ReadBytes(sizeof(ulong)), default);
        }
        public bool ReadBoolean() => ReadBool();
        public bool ReadBool()
        {
            if (index + sizeof(bool) > size)
                return default;
            return BitConverter.ToBoolean(ReadBytes(sizeof(bool)), default);
        }
        public float ReadSingle() => ReadFloat();
        public float ReadFloat()
        {
            if (index + sizeof(float) > size)
                return default;
            return BitConverter.ToSingle(ReadBytes(sizeof(float)), default);
        }
        public double ReadDouble()
        {
            if (index + sizeof(double) > size)
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
            return new Guid(ReadBytes(ReadByte()));
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
                body.Clear();
                body = null;
                msgType = default;
                size = default;
                index = default;
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