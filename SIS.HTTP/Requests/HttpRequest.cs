using System;
using System.Collections.Generic;
using SIS.HTTP.Enums;
using SIS.HTTP.Headers;
using SIS.HTTP.Exceptions;
using System.Linq;
using SIS.HTTP.Common;
using SIS.HTTP.Extensions;
using SIS.HTTP.Cookies;
using SIS.HTTP.Sessions;

namespace SIS.HTTP.Requests
{
    public class HttpRequest : IHttpRequest
    {
        public HttpRequest(string requestString)
        {
            this.FormData = new Dictionary<string, object>();
            this.QueryData = new Dictionary<string, object>();
            this.Headers = new HttpHeaderCollection();
            this.Cookies = new HttpCookieCollection();

            this.ParseRequest(requestString);
        }

        public string Path { get; private set; }

        public string Url { get; private set; }

        public Dictionary<string, object> FormData { get; }

        public Dictionary<string, object> QueryData { get; }

        public IHttpHeaderCollection Headers { get; }

        public IHttpCookieCollection Cookies { get; }

        public IHttpSession Session { get; set; } 

        public HttpRequestMethod RequestMethod { get; private set; }

        private bool isValidRequestLine(string[] requestLine)
        {
            return requestLine.Length == 3 && requestLine[2] == GlobalConstants.HttpOneProtocolFragment;
            //return requestLine.Length == 3 && requestLine[2].ToLower() != GlobalConstants.HttpOneProtocolFragment; //защо?
        }

        private bool isValidRequestQueryString(string queryString, string[] queryParameteres)
        {
            //return !String.IsNullOrEmpty(queryString) && queryParameteres.Length > 0; //Същото?
            return !(String.IsNullOrEmpty(queryString) || queryParameteres.Length < 1); 
        }

        private void ParseRequestMethod(string [] requestLine)
        {
            //this.RequestMethod = (HttpRequestMethod)Enum.Parse(typeof(HttpRequestMethod), requestLine[0]);
            bool success = Enum.TryParse<HttpRequestMethod>(requestLine[0].Capitalize(), out HttpRequestMethod parsedMethod);
            if(!success)
            {
                throw new BadRequestException();
            }

            this.RequestMethod = parsedMethod;
        }

        private void ParseRequestUrl(string[] requestLine)
        {
            this.Url = requestLine[1];
        }

        private void ParseRequestPath()
        {
            this.Path = this.Url.Split(new[] {'?', '#'}, StringSplitOptions.RemoveEmptyEntries)[0];
        }

        private void ParseHeaders(string[] requestContent)
        {
            foreach  (string line in requestContent)
            {
                if(line == Environment.NewLine)
                {
                    break;
                }

                string[] headerArgs = line.Split(new[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
                if(headerArgs.Length == 2)
                {
                    //What about Cookie: sas=33; adsd=231 etc.
                    HttpHeader header = new HttpHeader(headerArgs[0], headerArgs[1]);
                    this.Headers.Add(header);
                }
            }

            if (!this.Headers.ContainsHeader(GlobalConstants.HostHeaderKey))
            {
                throw new BadRequestException();
            }
        }

        private void ParseCookies()
        {
            if (!this.Headers.ContainsHeader("Cookie"))
            {
                return;
            }

            string cookieString = this.Headers.GetHeader("Cookie").Value;
            if (string.IsNullOrEmpty(cookieString))
            {
                return;
            }

            string[] allCookies = cookieString.Split("; ", StringSplitOptions.RemoveEmptyEntries);
            foreach (string cookie in allCookies)
            {
                string[] cookieParts = cookie.Split("=", 2, StringSplitOptions.RemoveEmptyEntries);
                if(cookieParts.Length != 2)
                {
                    continue;
                }

                string key = cookieParts[0];
                string value = cookieParts[1];
                HttpCookie newCookie = new HttpCookie(key, value, false);
                this.Cookies.Add(newCookie);
            }
        }

        private void ParseQueryParameters()
        {
            if (this.Url.Contains('?'))
            {
                string queryString = this.Url.Split(new[] { '?', '#' }, StringSplitOptions.None)[1];
                if (string.IsNullOrWhiteSpace(queryString))
                {
                    return;
                }

                string[] queryParameteres = queryString.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                if(!this.isValidRequestQueryString(queryString, queryParameteres))
                {
                    throw new BadRequestException();
                }

                foreach (string query in queryParameteres)
                {
                    string[] queryArgs = query.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (queryArgs.Length == 2)
                    {
                        string key = queryArgs[0];
                        string value = queryArgs[1];
                        if (!this.QueryData.ContainsKey(key))
                        {
                            this.QueryData.Add(key, value);
                        }
                        else
                        {
                            this.QueryData[key] = value;
                        }
                    }
                }
            }
        }

        private void ParseFormDataParameters(string formData)
        {
            if (string.IsNullOrEmpty(formData))
            {
                return;
            }

            string[] allParams = formData.Split(new[] { '&' });
            foreach (string pair in allParams)
            {
                string[] pairArgs = pair.Split(new[] { '=' });
                if(pairArgs.Length == 2)
                {
                    string paramKey = pairArgs[0];
                    string paramValue = pairArgs[1];
                    if (!this.FormData.ContainsKey(paramKey))
                    {
                        this.FormData.Add(paramKey, paramValue);
                    }
                    else
                    {
                        this.FormData[paramKey] = paramValue;
                    }
                }
            }
        }

        private void ParseRequestParameters(string formData)
        {
            this.ParseQueryParameters();
            this.ParseFormDataParameters(formData);
        }

        private void ParseRequest(string requestString)
        {
            string[] splitRequestContent = requestString.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            string[] requestLine = splitRequestContent[0].Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (!this.isValidRequestLine(requestLine))
            {
                throw new BadRequestException();
            }

            this.ParseRequestMethod(requestLine);
            this.ParseRequestUrl(requestLine);
            this.ParseRequestPath();

            this.ParseHeaders(splitRequestContent.Skip(1).ToArray());
            this.ParseCookies();
            this.ParseRequestParameters(splitRequestContent[splitRequestContent.Length - 1]);
        }
    }
}