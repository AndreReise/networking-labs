using Grpc.Net.Client;
using System.IO.Pipes;
using System.Security.Principal;

namespace Remoting.Client
{
    public class NamedPipesConnectionFactory
    {
        private readonly string pipeName;

        public NamedPipesConnectionFactory(string pipeName)
        {
            this.pipeName = pipeName;
        }

        public async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext _,
            CancellationToken cancellationToken = default)
        {
            var clientStream = new NamedPipeClientStream(
                serverName: ".",
                pipeName: this.pipeName,
                direction: PipeDirection.InOut,
                options: PipeOptions.WriteThrough | PipeOptions.Asynchronous,
                impersonationLevel: TokenImpersonationLevel.Anonymous);

            try
            {
                await clientStream.ConnectAsync(cancellationToken).ConfigureAwait(false);
                return clientStream;
            }
            catch
            {
                clientStream.Dispose();
                throw;
            }
        }

        public static GrpcChannel CreateChannel(string pipeName)
        {
            var connectionFactory = new NamedPipesConnectionFactory(pipeName);
            var socketsHttpHandler = new SocketsHttpHandler
            {
                ConnectCallback = connectionFactory.ConnectAsync
            };

            var httpClient = new HttpClient(socketsHttpHandler, true)
            {
                DefaultRequestVersion = new Version("2.0")
            };

            return GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
            {
                HttpClient = httpClient,
            });
        }
    }
}
