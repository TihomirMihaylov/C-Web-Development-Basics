﻿using SIS.HTTP.Enums;
using SIS.HTTP.Headers;
using SIS.HTTP.Responses;
using System.Text;

namespace SIS.WebServer.Results
{
    public class TextResult : HttpResponse
    {
        public TextResult(string content, HttpResponseStatusCode responseStatusCode, string contentType= "text/plain; charset=utf-8")
            : base(responseStatusCode)
        {
            this.Headers.Add(new HttpHeader("Content-Type", contentType));
            this.Content = Encoding.UTF8.GetBytes(content);
        }
    }
}