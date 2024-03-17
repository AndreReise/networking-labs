
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Remoting.Server.Services;

namespace Remoting.Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var webBuilder = WebApplication.CreateBuilder(args);

            webBuilder.Services.AddHttpLogging(_ => { });
            webBuilder.Services.AddGrpc();
            webBuilder.Services.AddSingleton<ExtraSpaceRemovingService>();

            webBuilder.WebHost.ConfigureKestrel(kestrelOptions =>
            {
                kestrelOptions.ListenNamedPipe("MyPipe", options => { options.Protocols = HttpProtocols.Http2; });
            });

            var webApp = webBuilder.Build();

            webApp.UseHttpLogging();
            webApp.MapGrpcService<ExtraSpaceRemovingService>();

            await webApp.RunAsync();
        }
    }
}
