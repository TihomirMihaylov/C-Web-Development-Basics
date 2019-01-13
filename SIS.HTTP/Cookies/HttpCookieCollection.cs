using System.Collections;
using System.Collections.Generic;

namespace SIS.HTTP.Cookies
{
    public class HttpCookieCollection : IHttpCookieCollection
    {
        private readonly Dictionary<string, HttpCookie> cookies;

        public HttpCookieCollection()
        {
            this.cookies = new Dictionary<string, HttpCookie>();
        }

        public void Add(HttpCookie cookie)
        {
            if (!this.cookies.ContainsKey(cookie.Key)) //?
            {
                this.cookies.Add(cookie.Key, cookie);
            }
        }

        public bool ContainsCookie(string key)
        {
            return this.cookies.ContainsKey(key);
        }

        public HttpCookie GetCookie(string key)
        {
            return this.cookies.GetValueOrDefault(key, null);
        }

        public bool HasCookies()
        {
            return this.cookies.Count > 0;
        }

        public IEnumerator<HttpCookie> GetEnumerator()
        {
            foreach (KeyValuePair<string, HttpCookie> cookie in this.cookies)
            {
                yield return cookie.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join("; ", this.cookies.Values);
        }
    }
}