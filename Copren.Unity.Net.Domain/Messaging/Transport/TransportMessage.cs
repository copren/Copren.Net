using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Copren.Unity.Net.Domain;
using Copren.Unity.Net.Domain.Messaging.Messages;

namespace Copren.Unity.Net.Domain.Messaging.Transport
{
    public class TransportMessage
    {
        private const int DefaultBufferSize = 10;
        public EndPoint RemoteEndPoint { get; }
        public SocketAsyncEventArgs SocketAsyncEventArgs { get; }
        private long? _length;
        private int? _typeLength;
        public long Length
        {
            get
            {
                if (BytesReceived < 8) return -1;
                if (_length == null)
                {
                    _length = ((long)Buffer[0] << 56)
                            | ((long)Buffer[1] << 48)
                            | ((long)Buffer[2] << 40)
                            | ((long)Buffer[3] << 32)
                            | ((long)Buffer[4] << 24)
                            | ((long)Buffer[5] << 16)
                            | ((long)Buffer[6] << 8)
                            | ((long)Buffer[7] << 0);
                }

                return _length ?? -1;
            }
        }
        public int TypeLength
        {
            get
            {
                if (BytesReceived < 12) return -1;
                if (_typeLength == null)
                {
                    _typeLength = ((int)Buffer[8] << 24)
                                | ((int)Buffer[9] << 16)
                                | ((int)Buffer[10] << 8)
                                | ((int)Buffer[11] << 0);
                }

                return _typeLength ?? -1;
            }
        }
        private Type _type;
        public Type Type
        {
            get
            {
                if (TypeLength < 0 || BytesReceived < 12 + TypeLength) return null;
                if (_type == null)
                {
                    _type = Type.GetType(Encoding.UTF8.GetString(Buffer, 12, TypeLength));
                }

                return _type;
            }
        }
        public int BytesReceived { get; set; }
        private byte[] _buffer;
        public byte[] Buffer => _buffer;
        public bool IsHeaderReady => ValidateHeader();
        public bool IsReady => ValidatePayload();
        public bool IsFull => BytesReceived >= Buffer.Length;

        public TransportMessage(SocketAsyncEventArgs socketAsyncEventArgs, int bufferSize = DefaultBufferSize)
        {
            _buffer = new byte[bufferSize];
            SocketAsyncEventArgs = socketAsyncEventArgs;
            socketAsyncEventArgs.SetBuffer(Buffer, 0, Buffer.Length);
        }

        public void UpdateBytesReceived()
        {
            BytesReceived += SocketAsyncEventArgs.BytesTransferred;

            if (IsFull)
            {
                Array.Resize(ref _buffer, Buffer.Length * 2);
                SocketAsyncEventArgs.SetBuffer(_buffer, BytesReceived, Buffer.Length / 2);
            }
        }

        private bool ValidateHeader()
        {
            return BytesReceived >= 12 && BytesReceived >= 12 + TypeLength;
        }

        private bool ValidatePayload()
        {
            return IsHeaderReady && BytesReceived == 8 + 4 + TypeLength + Length;
        }

        public void Reset(int length = DefaultBufferSize)
        {
            _length = null;
            _typeLength = null;
            _type = null;
            BytesReceived = 0;
            _buffer = new byte[length];
            SocketAsyncEventArgs.SetBuffer(_buffer, 0, Buffer.Length);
        }

        public Message DeserializeMessage()
        {
            return Message.Deserialize<Message>(Buffer, 8 + 4 + TypeLength, (int)Length, Type);
        }

        public T DeserializeMessage<T>()
            where T : Message
        {
            return Message.Deserialize<T>(Buffer, 8 + 4 + TypeLength, (int)Length, Type);
        }
    }
}