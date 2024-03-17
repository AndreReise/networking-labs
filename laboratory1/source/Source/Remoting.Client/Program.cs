using Grpc.Core;
using Grpc.Net.Client;
using SpaceRemoving;

namespace Remoting.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = NamedPipesConnectionFactory.CreateChannel("MyPipe");

            var spaceRemovingClient = new ExtraSpaceRemovingService.ExtraSpaceRemovingServiceClient(channel);

            while (true)
            {
                Console.WriteLine("Write string to remove spaces from OR 'q' to exit applicaiton");

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

                var response = await spaceRemovingClient.RemoveExtraSpacesAsync(request, new CallOptions()
                {
                    
                });

                Console.WriteLine("Space removing result:");
                Console.WriteLine(response.ContentString);

                Console.ReadKey();

                Console.Clear();
            }
        }
    }
}
