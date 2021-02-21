using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Copren.Net.Domain.Messaging.Transport;
using Serilog;

namespace Copren.Net.Core.Messaging.Transport
{
    public class UdpClientTransport : IClientTransport
    {
        private const int UdpPacketSize = 4096;

        public ProtocolType ProtocolType => ProtocolType.Udp;

        public event TransportHandler OnConnected;
        public event TransportMessageHandler OnReceived;
        public event TransportHandler OnClosed;

        private readonly EndPoint _localEndPoint;
        private readonly EndPoint _remoteEndPoint;
        private Socket _clientSocket;
        private readonly ILogger _logger;

        public UdpClientTransport(ClientOptions clientOptions, ILogger logger)
        {
            _localEndPoint = clientOptions.LocalEndPoint;
            _remoteEndPoint = clientOptions.RemoteEndPoint;
            _logger = logger.ForContext<UdpClientTransport>();
        }

        public Task StartAsync(CancellationToken ct = default)
        {
            _logger.Verbose("UDP:StartAsync");

            _clientSocket = new Socket(_remoteEndPoint.AddressFamily, SocketType.Dgram, ProtocolType);
            _logger.Verbose("UDP:StartAsync2");
            // _clientSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
            _logger.Verbose("UDP:StartAsync3");
            if (!(_localEndPoint == default))
            {
                _logger.Verbose($"Binding to udp://{_localEndPoint}");
                _clientSocket.Bind(_localEndPoint);
            }

            _logger.Information(
                "{LocalEndPoint} -> {RemoteEndPoint}",
                _clientSocket.LocalEndPoint,
                _remoteEndPoint);

            StartReceivingAsync(this);

            OnConnected?.Invoke(ProtocolType, _remoteEndPoint, null);

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            throw new System.NotImplementedException();
        }

        private void StartReceivingAsync(UdpClientTransport transport)
        {
            var acceptedSaea = new SocketAsyncEventArgs();
            var transportMessage = new TransportMessage(acceptedSaea, UdpPacketSize);
            acceptedSaea.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            acceptedSaea.UserToken = (transport, transportMessage);
            acceptedSaea.Completed += ProcessReceive;
            acceptedSaea.SetBuffer(transportMessage.Buffer, 0, transportMessage.Buffer.Length);

            if (!_clientSocket.ReceiveFromAsync(acceptedSaea))
            {
                ProcessReceive(null, acceptedSaea);
            }
        }

        private void ProcessReceive(object sender, SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs.SocketError != SocketError.Success)
            {
                Console.Error.WriteLine($"Error {eventArgs.SocketError.ToString()}");
            }

            (var transport, var transportMessage) = ((UdpClientTransport, TransportMessage))eventArgs.UserToken;

            if (eventArgs.BytesTransferred == 0)
            {
                OnClosed?.Invoke(ProtocolType.Udp, eventArgs.RemoteEndPoint, eventArgs);
                eventArgs.Dispose();
                return;
            }

            transportMessage.UpdateBytesReceived();

            // Check if we're done
            if (transportMessage.IsReady)
            {
                OnReceived?.Invoke(ProtocolType.Udp, eventArgs.RemoteEndPoint, transportMessage);
                transportMessage.Reset(UdpPacketSize);
            }

            StartReceivingAsync(transport);
        }

        public Task SendServerMessageAsync(byte[] data)
        {
            return SendMessageAsync(_remoteEndPoint, data);
        }

        public async Task SendMessageAsync(EndPoint endPoint, byte[] data)
        {
            var taskCompletionSource = new TaskCompletionSource<object>();

            var saea = new SocketAsyncEventArgs();
            saea.Completed += (o, e) =>
            {
                taskCompletionSource.SetResult(null);
            };
            saea.SetBuffer(data, 0, data.Length);
            saea.RemoteEndPoint = endPoint;
            if (!_clientSocket.SendToAsync(saea))
            {
                taskCompletionSource.SetResult(null);
            }

            await taskCompletionSource.Task.ConfigureAwait(false);
        }
    }
}