using SIS.HTTP.Cookies;
using SIS.HTTP.Enums;
using SIS.HTTP.Exceptions;
using SIS.HTTP.Requests;
using SIS.HTTP.Responses;
using SIS.HTTP.Sessions;
using SIS.WebServer.Api;
using SIS.WebServer.Results;
using SIS.WebServer.Routing;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SIS.WebServer
{
    public class ConnectionHandler
    {
        private readonly Socket client;

        private readonly ServerRoutingTable serverRoutingTable;

        public ConnectionHandler(Socket client, ServerRoutingTable serverRoutingTable)
        {
            this.client = client;
            this.serverRoutingTable = serverRoutingTable;
        }

        private async Task<IHttpRequest> ReadRequest()
        {
            StringBuilder result = new StringBuilder();
            var data = new ArraySegment<byte>(new byte[1024]); //това пък какво е?

            while (true)
            {
                int numberOfBytesRead = await this.client.ReceiveAsync(data.Array, SocketFlags.None);
                if (numberOfBytesRead == 0)
                {
                    break;
                }

                string bytesAsString = Encoding.UTF8.GetString(data.Array, 0, numberOfBytesRead);
                result.Append(bytesAsString);
                if (numberOfBytesRead < 1023)
                {
                    break;
                }
            }

            if (result.Length == 0)
            {
                return null;
            }

            return new HttpRequest(result.ToString());
        }

        private IHttpResponse HandleRequest(IHttpRequest httpRequest)
        {
            if (!this.serverRoutingTable.Contains(httpRequest.RequestMethod, httpRequest.Path))
            {
                return new TextResult($"Route with method {httpRequest.RequestMethod} and path \"{httpRequest.Path}\" not found", HttpResponseStatusCode.NotFound);
                //return this.ReturnIfResource(httpRequest.Path); //Това може да е от упражнението?!
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

        private async Task PrepareResponse(IHttpResponse httpResponse)
        {
            byte[] byteSegments = httpResponse.GetBytes();
            await this.client.SendAsync(byteSegments, SocketFlags.None);
        }

        private string SetRequestSession(IHttpRequest httpRequest)
        {
            string sessionId = null;
            if (httpRequest.Cookies.ContainsCookie(HttpSessionStorage.SessionCookieKey))
            {
                HttpCookie cookie = httpRequest.Cookies.GetCookie(HttpSessionStorage.SessionCookieKey);
                sessionId = cookie.Value;
                httpRequest.Session = HttpSessionStorage.GetSession(sessionId);
            }
            else
            {
                sessionId = Guid.NewGuid().ToString();
                httpRequest.Session = HttpSessionStorage.GetSession(sessionId);
            }

            return sessionId;
        }

        private void SetResponseSession(IHttpResponse httpResponse, string sessionId)
        {
            if (sessionId != null)
            {
                HttpCookie newCookie = new HttpCookie(HttpSessionStorage.SessionCookieKey, $"{sessionId}; HttpOnly");
                httpResponse.AddCookie(newCookie);
            }
        }

        public async Task ProcessRequestAsync()
        {
            try
            {
                IHttpRequest httpRequest = await this.ReadRequest();
                if (httpRequest != null)
                {
                    Console.WriteLine($"Processing: {httpRequest.RequestMethod} {httpRequest.Path}..."); //Ще е полезно при дебъг

                    string sessionId = this.SetRequestSession(httpRequest); //!
                    IHttpResponse httpResponse = this.HandleRequest(httpRequest);
                    this.SetResponseSession(httpResponse, sessionId); //!
                    await this.PrepareResponse(httpResponse);
                }
            }
            catch (BadRequestException e)
            {
                await this.PrepareResponse(new TextResult(e.ToString(), HttpResponseStatusCode.BadRequest));
            }
            catch (Exception e)
            {
                await this.PrepareResponse(new TextResult(e.ToString(), HttpResponseStatusCode.InternalServerError));
            }

            this.client.Shutdown(SocketShutdown.Both);
        }
    }
}