using System;
using System.Text;

namespace SIS.HTTP.Cookies
{
    public class HttpCookie
    {
        private const int HttpCookieDefaultExpirationDays = 3;
        private const string HttpCookieDefaultPath = "/"; //ЕТО ТОВА СЪМ ПРОПУСНАЛ ПРЕДНИЯ ПЪТ!!!

        public HttpCookie(string key, string value, int expires = HttpCookieDefaultExpirationDays, string path = HttpCookieDefaultPath)
        {
            this.Key = key;
            this.Value = value;
            this.IsNew = true;
            this.Path = path;
            this.Expires = DateTime.UtcNow.AddDays(expires);
        }

        public HttpCookie(string key, string value, bool isNew, int expires = HttpCookieDefaultExpirationDays, string path = HttpCookieDefaultPath)
            :this(key, value, expires)
        {
            this.IsNew = isNew;
        }

        public string Key { get; }

        public string Value { get; set; }

        public DateTime Expires { get; private set; }

        public string Path { get; set; }

        public bool IsNew { get; }

        public bool HttpOnly { get; set; } = true; //Важно е от гледна точка на секюрити

        public void Delete()
        {
            this.Expires = DateTime.UtcNow.AddDays(-1);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{this.Key}={this.Value}; Expires={this.Expires:R}"); //R за да е в правилния формат .ToString("R") е същото
            if (this.HttpOnly)
            {
                sb.Append("; HttpOnly");
            }

            sb.Append($"; Path={this.Path}");
            return sb.ToString();  
        }
    }
}