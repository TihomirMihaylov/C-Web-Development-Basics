using SIS.MvcFramework;

namespace CakesWebApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var mvcApplication = new Startup();
            WebHost.Start(mvcApplication);
        }
    }
}