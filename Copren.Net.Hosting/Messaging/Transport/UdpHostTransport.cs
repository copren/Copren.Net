using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Copren.Shared.Async;
using Copren.Net.Domain.Messaging.Transport;

namespace Copren.Net.Hosting.Messaging.Transport
{
    public class UdpHostTransport : AsyncTask, IHostTransport
    {
        private const int UdpPacketSize = 4096;
        public event TransportHandler OnConnected;
        public event TransportMessageHandler OnReceived;
        public event TransportHandler OnClosed;

        public ProtocolType ProtocolType => ProtocolType.Udp;

        private readonly EndPoint _localEndPoint;
        private Socket _listeningSocket;
        private readonly AutoResetEvent _autoResetEvent;

        public UdpHostTransport(HostOptions hostOptions) : base()
        {
            _localEndPoint = hostOptions.LocalEndPoint;
            _autoResetEvent = new AutoResetEvent(false);
        }

        protected override void Run()
        {
            StartReceiving(this);
        }

        private void StartReceiving(UdpHostTransport transport)
        {
            if (_listeningSocket != null)
            {
                _listeningSocket.Disconnect(false);
                _listeningSocket.Dispose();
            }

            _listeningSocket = new Socket(_localEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            _listeningSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
            _listeningSocket.Bind(_localEndPoint);

            var acceptedSaea = new SocketAsyncEventArgs();
            var transportMessage = new TransportMessage(acceptedSaea, UdpPacketSize);
            acceptedSaea.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            acceptedSaea.UserToken = (transport, transportMessage);
            acceptedSaea.Completed += ProcessReceive;
            acceptedSaea.SetBuffer(transportMessage.Buffer, 0, transportMessage.Buffer.Length);

            while (!CancellationToken.IsCancellationRequested)
            {
                if (!_listeningSocket.ReceiveMessageFromAsync(acceptedSaea))
                {
                    ProcessReceive(null, acceptedSaea);
                }

                while (!CancellationToken.IsCancellationRequested && !_autoResetEvent.WaitOne(10)) { }
            }
        }

        private void ProcessReceive(object sender, SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs.SocketError != SocketError.Success)
            {
                Console.Error.WriteLine($"Error {eventArgs.SocketError.ToString()}");
            }

            OnConnected?.Invoke(ProtocolType.Udp, eventArgs.RemoteEndPoint, eventArgs);

            if (eventArgs.BytesTransferred == 0)
            {
                OnClosed?.Invoke(ProtocolType.Udp, eventArgs.RemoteEndPoint, eventArgs);
                eventArgs.Dispose();
                return;
            }

            (var transport, var transportMessage) = ((UdpHostTransport, TransportMessage))eventArgs.UserToken;

            transportMessage.UpdateBytesReceived();

            // For now, UDP messages cannot extend across a single message
            OnReceived?.Invoke(ProtocolType.Udp, eventArgs.RemoteEndPoint, transportMessage);
            transportMessage.Reset(UdpPacketSize);

            _autoResetEvent.Set();
        }

        public Task SendMessage(EndPoint endPoint, byte[] data)
        {
            var taskCompletionSource = new TaskCompletionSource<object>();

            var saea = new SocketAsyncEventArgs();
            saea.Completed += (o, e) => taskCompletionSource.SetResult(null);
            saea.SetBuffer(data, 0, data.Length);
            saea.RemoteEndPoint = endPoint;

            if (!_listeningSocket.SendToAsync(saea))
            {
                return Task.CompletedTask;
            }

            return taskCompletionSource.Task;
        }
    }
}