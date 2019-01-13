using System.IO;
using SIS.HTTP.Enums;
using SIS.HTTP.Requests;
using SIS.HTTP.Responses;
using SIS.WebServer.Results;
using SIS.WebServer.Routing;

namespace SIS.WebServer.Api
{
    public class HttpHandler : IHttpHandler
    {
        private ServerRoutingTable serverRoutingTable;

        public HttpHandler(ServerRoutingTable routingTable)
        {
            this.serverRoutingTable = routingTable;
        }

        public IHttpResponse Handle(IHttpRequest httpRequest)
        {
            if (!this.serverRoutingTable.Contains(httpRequest.RequestMethod, httpRequest.Path))
            {
                //return new HttpResponse(HttpResponseStatusCode.NotFound);
                return this.ReturnIfResource(httpRequest.Path);
            }

            return this.serverRoutingTable.Get(httpRequest.RequestMethod, httpRequest.Path).Invoke(httpRequest);
        }

        private IHttpResponse ReturnIfResource(string path)
        {
            if (path.EndsWith(".css") || path.EndsWith(".js"))
            {
                //Resources/css/bootstrap.min.css
                int indexOfStartOfExtension = path.LastIndexOf('.');
                string extension = path.Substring(indexOfStartOfExtension);
                int indexOfStartOfResourseName = path.LastIndexOf('/');
                string resourceName = path.Substring(indexOfStartOfResourseName);
                //string location = Assembly.GetExecutingAssembly().Location; --> така взимам текущото местоположение

                string resourcePath = $"../../../Resources/{extension.Substring(1)}/{resourceName}";
                if (File.Exists(resourcePath))
                {
                    byte[] fileContent = File.ReadAllBytes(resourcePath);
                    return new InlineResourceResult(fileContent, HttpResponseStatusCode.Ok);
                }
            }

            return new HttpResponse(HttpResponseStatusCode.NotFound);
        }
    }
}