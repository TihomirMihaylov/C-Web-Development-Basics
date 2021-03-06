﻿using SIS.HTTP.Common;
using System;
using System.Collections.Generic;

namespace SIS.HTTP.Headers
{
    public class HttpHeaderCollection : IHttpHeaderCollection
    {
        private readonly Dictionary<string, HttpHeader> headers;

        public HttpHeaderCollection()
        {
            this.headers = new Dictionary<string, HttpHeader>();
        }

        public void Add(HttpHeader header)
        {
            CoreValidator.ThrowIfNull(header, nameof(header));
            this.headers[header.Key] = header;
        }

        public bool ContainsHeader(string key)
        {
            return this.headers.ContainsKey(key);
        }

        public HttpHeader GetHeader(string key)
        {
            //return this.headers[key]; //Би трябвало да е същото
            return this.headers.GetValueOrDefault(key, null);
        }

        public override string ToString()
        {
            return String.Join(Environment.NewLine, this.headers.Values);
        }
    }
}