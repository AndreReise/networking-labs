using Microsoft.Extensions.Logging;
using SpaceRemoving;

namespace Remoting.Client
{
    public class RequesterHostedService : IHostedService
    {
        private readonly CancellationTokenSource cts = new();
        private readonly RequestContentAccessor requestContentAccessor;
        private readonly ExtraSpaceRemovingService.ExtraSpaceRemovingServiceClient removingServiceClient;
        private readonly ILogger logger;

        private Task requestingTask;

        public RequesterHostedService(
            RequestContentAccessor requestContentAccessor,
            ExtraSpaceRemovingService.ExtraSpaceRemovingServiceClient removingServiceClient,
            ILogger<RequesterHostedService> logger)
        {
            this.requestContentAccessor = requestContentAccessor;
            this.removingServiceClient = removingServiceClient;
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.requestingTask = Task.Run(() => this.RequestLoopAsync(this.cts.Token));

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await this.cts.CancelAsync();

            await this.requestingTask;
        }

        public async Task RequestLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var requestString = this.requestContentAccessor.RequestString;

                var request = new RemoveExtraSpacesRequest()
                {
                    ContentString = requestString,
                };

                this.logger.LogInformation("Sending request string: {0}", request.ContentString);

                var response = await this.removingServiceClient.RemoveExtraSpacesAsync(request);

                this.logger.LogInformation("Received response from server: {0}", response.ContentString);

                await Task.Delay(5000)
                    .ConfigureAwait(false);
            }
        }
    }
}
