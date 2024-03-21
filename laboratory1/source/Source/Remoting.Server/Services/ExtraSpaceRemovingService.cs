using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using SpaceRemoving;

namespace Remoting.Server.Services
{
    public class ExtraSpaceRemovingService : SpaceRemoving.ExtraSpaceRemovingService.ExtraSpaceRemovingServiceBase
    {
        public override Task<RemoveExtraSpacesResponse> RemoveExtraSpaces(RemoveExtraSpacesRequest request, ServerCallContext context)
        {
            var content = request.ContentString;
            var trimmedContent = content.Trim();

            var resultBuilder = new StringBuilder();

            var contentParts = trimmedContent.Split(' ').Where(str => str != string.Empty);

            resultBuilder.AppendJoin(' ', contentParts);

            var response = new RemoveExtraSpacesResponse()
            {
                ContentString = resultBuilder.ToString(),
            };

            return Task.FromResult(response);
        }
    }
}
