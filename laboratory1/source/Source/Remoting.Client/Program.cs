using System.Net.Http.Headers;
using Grpc.Net.Client;
using SpaceRemoving;

namespace Remoting.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:9090", new GrpcChannelOptions()
            {
                HttpClient = new HttpClient()
                {
                    DefaultRequestVersion = new Version(1, 1)
                }
            });

            var spaceRemovingClient = new ExtraSpaceRemovingService.ExtraSpaceRemovingServiceClient(channel);

            while (true)
            {
                Console.WriteLine("Write string to remove spaces from OR 'q' to exit application");

                var input = Console.ReadLine();

                if (input == null)
                {
                    Console.WriteLine("Invalid input");

                    continue;
                }

                if (input.Length == 1 && input[0] == 'q')
                {
                    break;
                }

                var request = new RemoveExtraSpacesRequest()
                {
                    ContentString = input,
                };

                var response = await spaceRemovingClient.RemoveExtraSpacesAsync(request);

                Console.WriteLine("Space removing result:");
                Console.WriteLine(response.ContentString);

                Console.ReadKey();

                Console.Clear();
            }
        }
    }
}
