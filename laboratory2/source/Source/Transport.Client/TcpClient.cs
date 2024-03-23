using System.Net;
using System.Net.Sockets;

namespace Transport.Client
{
    public class TcpClient : IClient, IDisposable
    {
        private CancellationTokenSource cts = new ();

        private readonly Socket socket;

        public TcpClient(string ipAddress, int port) :
            this(AddressFamily.InterNetwork, new IPEndPoint(IPAddress.Parse(ipAddress), port))
        {

        }

        public TcpClient(AddressFamily addressFamily, EndPoint endpoint)
        {
            this.socket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);

            this.socket.Connect(endpoint);
        }

        public async Task SendAsync(Memory<byte> buffer)
        {
            await this.socket.SendAsync(buffer, this.cts.Token);
        }

        public async Task<int> ReceiveAsync(Memory<byte> buffer)
        {
            return await this.socket.ReceiveAsync(buffer, this.cts.Token);
        }

        public void Dispose()
        {
            this.cts.Cancel();
            this.cts.Dispose();

            this.socket.Dispose();
        }
    }
}
