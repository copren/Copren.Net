using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Copren.Shared.Async;
using Copren.Unity.Net.Domain.Messaging.Transport;

namespace Copren.Unity.Net.Hosting.Messaging.Transport
{
    public class TcpHostTransport : AsyncTask, IHostTransport
    {
        public event TransportHandler OnConnected;
        public event TransportMessageHandler OnReceived;
        public event TransportHandler OnClosed;

        public ProtocolType ProtocolType => ProtocolType.Tcp;

        private readonly EndPoint _localEndPoint;
        private Socket _listeningSocket;
        private readonly AutoResetEvent _autoResetEvent;
        private readonly IDictionary<EndPoint, (Socket, SocketAsyncEventArgs)> _endPointToSocket = new Dictionary<EndPoint, (Socket, SocketAsyncEventArgs)>();

        public TcpHostTransport(HostOptions hostOptions) : base()
        {
            _localEndPoint = hostOptions.LocalEndPoint;
            _autoResetEvent = new AutoResetEvent(false);
        }

        protected override void Run()
        {
            StartListening(this);
        }

        private void StartListening(TcpHostTransport transport)
        {
            if (_listeningSocket != null)
            {
                _listeningSocket.Disconnect(false);
                _listeningSocket.Dispose();
            }

            _listeningSocket = new Socket(_localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listeningSocket.Bind(_localEndPoint);
            _listeningSocket.Listen(10);

            StartAccept(this);
        }

        private void StartAccept(TcpHostTransport transport)
        {
            while (!CancellationToken.IsCancellationRequested)
            {
                var saea = new SocketAsyncEventArgs();
                saea.UserToken = transport;
                saea.Completed += ProcessAccept;
                while (!_listeningSocket.AcceptAsync(saea))
                {
                    ProcessAccept(null, saea);
                }

                while (!CancellationToken.IsCancellationRequested && !_autoResetEvent.WaitOne(500)) { }
            }
        }

        private void ProcessAccept(object sender, SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs.SocketError != SocketError.Success)
            {
                Console.Error.WriteLine($"Error {eventArgs.SocketError.ToString()} - Restarting");
                StartListening(eventArgs.UserToken as TcpHostTransport);
                return;
            }

            var host = eventArgs.UserToken as TcpHostTransport;

            OnConnected?.Invoke(ProtocolType.Tcp, eventArgs.AcceptSocket.RemoteEndPoint, eventArgs);

            var acceptedSaea = new SocketAsyncEventArgs();
            var transportMessage = new TransportMessage(acceptedSaea);
            acceptedSaea.UserToken = (host, eventArgs.AcceptSocket, transportMessage);
            acceptedSaea.Completed += ProcessReceive;
            acceptedSaea.SetBuffer(transportMessage.Buffer, 0, transportMessage.Buffer.Length);

            _endPointToSocket.Add(eventArgs.AcceptSocket.RemoteEndPoint, (eventArgs.AcceptSocket, acceptedSaea));

            StartReceivingAsync(acceptedSaea);
            _autoResetEvent.Set();
            // StartAccept(host);
        }

        private void StartReceivingAsync(SocketAsyncEventArgs eventArgs)
        {
            var token = ((TcpHostTransport, Socket, TransportMessage))eventArgs.UserToken;
            if (!token.Item2.ReceiveAsync(eventArgs))
            {
                ProcessReceive(null, eventArgs);
            }
        }

        private void ProcessReceive(object sender, SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs.SocketError != SocketError.Success)
            {
                Console.Error.WriteLine($"Error {eventArgs.SocketError.ToString()}");
            }

            (var transport, var socket, var transportMessage) = ((TcpHostTransport, Socket, TransportMessage))eventArgs.UserToken;

            if (eventArgs.BytesTransferred == 0)
            {
                OnClosed?.Invoke(ProtocolType.Tcp, socket.RemoteEndPoint, eventArgs);
                _endPointToSocket.Remove(socket.RemoteEndPoint);
                eventArgs.Dispose();
                return;
            }

            transportMessage.UpdateBytesReceived();

            // Check if we're done
            if (transportMessage.IsReady)
            {
                OnReceived?.Invoke(ProtocolType.Tcp, socket.RemoteEndPoint, transportMessage);
                transportMessage.Reset();
            }

            StartReceivingAsync(eventArgs);
        }

        public Task SendMessage(EndPoint endPoint, byte[] data)
        {
            var taskCompletionSource = new TaskCompletionSource<object>();

            (var socket, var _) = _endPointToSocket[endPoint];

            var saea = new SocketAsyncEventArgs();
            saea.Completed += (o, e) => taskCompletionSource.SetResult(null);
            saea.SetBuffer(data, 0, data.Length);

            if (!socket.SendAsync(saea))
            {
                return Task.CompletedTask;
            }

            return taskCompletionSource.Task;
        }
    }
}