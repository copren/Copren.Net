using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Copren.Net.Domain.Messaging.Transport;
using Serilog;

namespace Copren.Net.Core.Messaging.Transport
{
    public class TcpClientTransport : IClientTransport
    {
        public ProtocolType ProtocolType => ProtocolType.Tcp;

        public event TransportHandler OnConnected;
        public event TransportMessageHandler OnReceived;
        public event TransportHandler OnClosed;
        private readonly EndPoint _localEndPoint;
        private readonly EndPoint _remoteEndPoint;
        private Socket _clientSocket;
        private readonly ILogger _logger;

        public TcpClientTransport(ClientOptions clientOptions, ILogger logger)
        {
            _localEndPoint = clientOptions.LocalEndPoint;
            _remoteEndPoint = clientOptions.RemoteEndPoint;
            _logger = logger.ForContext<TcpClientTransport>();
        }

        public Task StartAsync(CancellationToken ct = default)
        {
            _logger.Verbose("TCP:StartAsync");

            _clientSocket = new Socket(_remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType);
            if (!(_localEndPoint == default))
            {
                _logger.Verbose($"Binding to tcp://{_localEndPoint}");
                _clientSocket.Bind(_localEndPoint);
            }

            var saea = new SocketAsyncEventArgs();
            saea.UserToken = this;
            saea.Completed += ProcessConnect;
            saea.RemoteEndPoint = _remoteEndPoint;

            if (!_clientSocket.ConnectAsync(saea))
            {
                ProcessConnect(_clientSocket, saea);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            throw new System.NotImplementedException();
        }

        private void ProcessConnect(object sender, SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs.SocketError != SocketError.Success)
            {
                Console.Error.WriteLine($"Error {eventArgs.SocketError.ToString()} - Ignoring");
                return;
            }

            var client = eventArgs.UserToken as TcpClientTransport;

            _logger.Information(
                "{LocalEndPoint} -> {RemoteEndPoint}",
                _clientSocket.LocalEndPoint,
                _remoteEndPoint);

            OnConnected?.Invoke(ProtocolType.Tcp, eventArgs.RemoteEndPoint, eventArgs);

            var acceptedSaea = new SocketAsyncEventArgs();
            var transportMessage = new TransportMessage(acceptedSaea);
            acceptedSaea.UserToken = (client, transportMessage);
            acceptedSaea.Completed += ProcessReceive;
            acceptedSaea.SetBuffer(transportMessage.Buffer, 0, transportMessage.Buffer.Length);

            StartReceivingAsync(acceptedSaea);
        }

        private void StartReceivingAsync(SocketAsyncEventArgs eventArgs)
        {
            var token = ((TcpClientTransport, TransportMessage))eventArgs.UserToken;
            if (!_clientSocket.ReceiveAsync(eventArgs))
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

            (var transport, var transportMessage) = ((TcpClientTransport, TransportMessage))eventArgs.UserToken;

            if (eventArgs.BytesTransferred == 0)
            {
                OnClosed?.Invoke(ProtocolType.Tcp, _remoteEndPoint, eventArgs);
                eventArgs.Dispose();
                return;
            }

            transportMessage.UpdateBytesReceived();

            // Check if we're done
            if (transportMessage.IsReady)
            {
                OnReceived?.Invoke(ProtocolType.Tcp, _remoteEndPoint, transportMessage);
                transportMessage.Reset();
            }

            StartReceivingAsync(eventArgs);
        }

        public Task SendServerMessageAsync(byte[] data)
        {
            return SendMessageAsync(_remoteEndPoint, data);
        }

        public async Task SendMessageAsync(EndPoint endPoint, byte[] data)
        {
            if (!endPoint.Equals(_remoteEndPoint)) throw new InvalidTransportException("Cannot send peer message through TCP");

            var taskCompletionSource = new TaskCompletionSource<object>();

            var saea = new SocketAsyncEventArgs();
            saea.Completed += (o, e) =>
            {
                taskCompletionSource.SetResult(null);
            };
            saea.SetBuffer(data, 0, data.Length);
            if (!_clientSocket.SendAsync(saea))
            {
                taskCompletionSource.SetResult(null);
            }

            await taskCompletionSource.Task.ConfigureAwait(false);
        }
    }
}