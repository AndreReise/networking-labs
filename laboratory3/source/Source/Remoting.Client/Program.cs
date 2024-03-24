using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using SpaceRemoving;

namespace Remoting.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var webBuilder = WebApplication.CreateBuilder(args);

            webBuilder.Services.AddLogging();

            webBuilder.Services.AddSingleton<GrpcChannel>(_ => GrpcChannel.ForAddress("http://remoting-server:9090", new GrpcChannelOptions()
            {
                HttpClient = new HttpClient()
                {
                    DefaultRequestVersion = new Version(1, 1)
                }
            }));

            webBuilder.Services.AddSingleton<RequestContentAccessor>();
            webBuilder.Services.AddSingleton<ExtraSpaceRemovingService.ExtraSpaceRemovingServiceClient>(provider =>
            {
                var channel = provider.GetRequiredService<GrpcChannel>();

                return new ExtraSpaceRemovingService.ExtraSpaceRemovingServiceClient(channel);
            });

            webBuilder.Services.AddHostedService<RequesterHostedService>();

            var webApp = webBuilder.Build();

            webApp.MapPatch("/request-content", HandleRequestContentChange);

            await webApp.RunAsync();
        }

        private static void HandleRequestContentChange([FromServices] RequestContentAccessor accessor, [FromBody] ContentChangeRequest request) => accessor.RequestString = request.Content;
    }
}
