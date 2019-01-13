using SIS.HTTP.Requests;
using SIS.HTTP.Responses;

namespace SIS.WebServer.Api
{
    public interface IHttpHandler
    {
        IHttpResponse Handle(IHttpRequest request);
    }
}