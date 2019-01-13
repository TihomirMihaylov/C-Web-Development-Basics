using System.Text;
using SIS.HTTP.Enums;
using SIS.HTTP.Headers;
using SIS.HTTP.Common;
using System.Linq;
using SIS.HTTP.Cookies;
using SIS.HTTP.Extensions;

namespace SIS.HTTP.Responses
{
    public class HttpResponse : IHttpResponse
    {
        public HttpResponse()
        {
            this.Headers = new HttpHeaderCollection();
            this.Cookies = new HttpCookieCollection();
            this.Content = new byte[0];
        }

        public HttpResponse(HttpResponseStatusCode statusCode)
            :this()
        {
            this.StatusCode = statusCode;
        }

        public HttpResponseStatusCode StatusCode { get; set; }

        public IHttpHeaderCollection Headers { get; private set; }

        public IHttpCookieCollection Cookies { get; }

        public byte[] Content { get; set; }

        public void AddHeader(HttpHeader header)
        {
            this.Headers.Add(header);
        }

        public void AddCookie(HttpCookie cookie)
        {
            this.Cookies.Add(cookie);
        }

        public byte[] GetBytes()
        {
            //converts the result from the ToString() method to a byte[] array, and concatenates to it the Content bytes, thus forming the full Response in byte format
            return Encoding.UTF8.GetBytes(this.ToString()).Concat(this.Content).ToArray();
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder(); // HTTP1.1 200 OK

            //Да не би от това да е проблема, че в браузъра ми се дисплейва кукито
            //result.Append($"{GlobalConstants.HttpOneProtocolFragment} {(int)this.StatusCode} {this.StatusCode.ToString()}")
            result.Append($"{GlobalConstants.HttpOneProtocolFragment} {this.StatusCode.GetResponseLine()}")
                  .Append(GlobalConstants.HttpNewLine)
                  .Append(this.Headers)
                  .Append(GlobalConstants.HttpNewLine);

            if (this.Cookies.HasCookies())
            {
                foreach (HttpCookie cookie in this.Cookies)
                {
                    result.Append($"Set-Cookie: {cookie}").Append(GlobalConstants.HttpNewLine);
                }
            }

            result.Append(GlobalConstants.HttpNewLine);

            return result.ToString();
        }
    }
}