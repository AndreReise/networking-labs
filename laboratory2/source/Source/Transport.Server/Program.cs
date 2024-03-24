using System.Net;
using System.Net.Sockets;
using Transport.Server.Tcp;
using Transport.Server.Udp;

namespace Transport.Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9090);

            //using var server = new TcpServer(AddressFamily.InterNetwork, endpoint, RequestHandler.Process);
            using var server = new UdpServer(AddressFamily.InterNetwork, endpoint, RequestHandler.Process);

            server.Start();

            Console.ReadKey();
        }
    }
}
