using SIS.MvcFramework;
using SIS.MvcFramework.Loggers;
using SIS.MvcFramework.Services;

namespace CakesWebApp
{
    public class Startup : IMvcApplication
    {
        public MvcFrameworkSettings Configure()
        {
            return new MvcFrameworkSettings(); //тук задваме път до wwwroot папката
        }

        public void ConfigureServices(IServiceCollection collection) //Тук е контейнера и регистрациите
        {
            collection.AddService<IHashService, HashService>();
            collection.AddService<IUserCookieService, UserCookieService>();
            //collection.AddService<ILogger, ConsoleLogger>();
            //Регистриране на сървис чрез подаване на метод, който да го създава.
            collection.AddService<ILogger>(() => new FileLogger($"log.txt"));
        }
    }
}