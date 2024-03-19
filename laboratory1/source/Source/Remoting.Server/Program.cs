
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
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

            var webApp = webBuilder.Build();

            webApp.UseHttpLogging();
            webApp.MapGrpcService<ExtraSpaceRemovingService>();

            await webApp.RunAsync();
        }
    }
}
