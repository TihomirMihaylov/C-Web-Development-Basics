using System.Collections.Concurrent;

namespace SIS.HTTP.Sessions
{
    public class HttpSessionStorage
    {
        public const string SessionCookieKey = "SIS_ID";

        private static readonly ConcurrentDictionary<string, IHttpSession> sessions = new ConcurrentDictionary<string, IHttpSession>();

        public static IHttpSession GetSession(string id)
        {
            //retrieves a Session from the Session Storage collection if it exists, or adds it and then retrieves it, if it does NOT exist
            return sessions.GetOrAdd(id, _ => new HttpSession(id));
        }
    }
}