using SIS.MvcFramework;
using SIS.MvcFramework.Services;

namespace MishMashWebApp
{
    public class Startup : IMvcApplication
    {
        public MvcFrameworkSettings Configure()
        {
            return new MvcFrameworkSettings(); //тук задваме път до wwwroot папката
        }

        public void ConfigureServices(IServiceCollection collection)
        {
            //Тези неща ги има конфигуриране за всеки случай във фреймуърка в WebHost.cs
        }
    }
}