using Sample;

namespace Transport.Shared
{
    public class FindMaxElementHandler
    {
        public static FindMaxElementResponse Handle(FindMaxElementRequest request)
        {
            var maxValue = request.Values.Max();

            return new FindMaxElementResponse
            {
                Result = maxValue,
            };
        }
    }
}
