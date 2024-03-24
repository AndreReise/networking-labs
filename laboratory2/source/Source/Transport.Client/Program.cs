using Google.Protobuf;
using Sample;
using System.Net;

namespace Transport.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9090);

            //using var client = new TcpClient("127.0.0.1", 9090);
            using var client = new UdpClient(endpoint);

            var cts = new CancellationTokenSource();

            var task = Task.Run(() => CommunicationTask(client, cts.Token));

            Console.ReadKey();

            await cts.CancelAsync();

            await task;
        }

        public static async Task CommunicationTask(IClient client, CancellationToken cancellationToken)
        {
            try
            {
                var values = new [] { 1, -1, 5, 8, 0, 1, 88, 12, 45 };

                var request = new FindMaxElementRequest()
                {
                    Values = { values }
                };

                var requestBytes = request.ToByteArray();

                var receiveBuffer = new byte[1024];

                while (!cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("Sending request {0}", string.Join(' ', values.Select(x => x.ToString())));

                    await client.SendAsync(requestBytes);

                    Console.WriteLine("Waiting for response");

                    var read = await client.ReceiveAsync(receiveBuffer);

                    if (read == 0)
                    {
                        Console.WriteLine("End of stream");

                        break;
                    }

                    var response = FindMaxElementResponse.Parser.ParseFrom(receiveBuffer.AsSpan(0, read));

                    Console.WriteLine("Response: {0}", response.Result);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}
