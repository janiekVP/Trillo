using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BoardServiceTest.Helpers
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"result\":\"42\"}")
            });
        }
    }
}
