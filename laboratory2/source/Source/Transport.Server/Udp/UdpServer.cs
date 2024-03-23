using System.Net;
using System.Net.Sockets;

namespace Transport.Server.Udp
{
    public class UdpServer : IDisposable
    {
        private readonly CancellationTokenSource cts = new();

        private readonly Socket socket;
        private readonly Action<Socket, EndPoint, Memory<byte>> receiveHandler;

        public UdpServer(AddressFamily addressFamily, EndPoint endpoint, Action<Socket, EndPoint, Memory<byte>> receiveHandler)
        {
            this.socket = new Socket(addressFamily, SocketType.Dgram, ProtocolType.Udp);
            this.receiveHandler = receiveHandler;

            this.socket.Bind(endpoint);
        }

        public void Start()
        {
            Task.Run(() => this.ReceiveLoop(this.cts.Token));
        }

        public void ReceiveLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                EndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);

                var buffer = new byte[1024];

                var read = this.socket.ReceiveFrom(buffer, 0, 1024, SocketFlags.None, ref remoteEndpoint);

                this.receiveHandler(this.socket, remoteEndpoint, buffer.AsMemory(0, read));
            }
        }

        public void Dispose()
        {
            this.socket.Dispose();

            this.socket.Close();
        }



    }
}
