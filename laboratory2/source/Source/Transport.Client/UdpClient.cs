using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Transport.Client
{
    public class UdpClient : IClient, IDisposable
    {
        private CancellationTokenSource cts = new();

        private readonly EndPoint endPoint;
        private readonly Socket socket;

        public UdpClient(EndPoint endpoint)
        {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.endPoint = endpoint;

            //this.socket.Bind(endpoint);
        }

        public async Task SendAsync(Memory<byte> buffer)
        {
            await this.socket.SendToAsync(buffer, this.endPoint, this.cts.Token);
        }

        public async Task<int> ReceiveAsync(Memory<byte> buffer)
        {
            var result = await this.socket.ReceiveFromAsync(buffer, this.endPoint, this.cts.Token);

            return result.ReceivedBytes;
        }

        public void Dispose()
        {
            this.cts.Cancel();
            this.cts.Dispose();

            this.socket.Dispose();
        }
    }
}
