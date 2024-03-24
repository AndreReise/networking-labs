using System.Net;
using System.Net.Sockets;
using Google.Protobuf;
using Sample;
using Transport.Server.Tcp;
using Transport.Shared;

namespace Transport.Server
{
    public class RequestHandler
    {
        public static void Process(TcpClientSession session, Memory<byte> receiveBytes)
        {
            var request = FindMaxElementRequest.Parser.ParseFrom(receiveBytes.Span);

            var response = FindMaxElementHandler.Handle(request);

            var responseBytes = response.ToByteArray();

            session.SendAsync(responseBytes).GetAwaiter().GetResult();
        }

        public static void Process(Socket socket, EndPoint endpoint, Memory<byte> receiveBytes)
        {
            var request = FindMaxElementRequest.Parser.ParseFrom(receiveBytes.Span);

            var response = FindMaxElementHandler.Handle(request);

            var responseBytes = response.ToByteArray();

            socket.SendTo(responseBytes, SocketFlags.None, endpoint);
        }
    }
}
