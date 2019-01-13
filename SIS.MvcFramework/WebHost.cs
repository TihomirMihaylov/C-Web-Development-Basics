using SIS.MvcFramework.Loggers;
using SIS.MvcFramework.Routing;
using SIS.MvcFramework.Services;
using SIS.WebServer;
using SIS.WebServer.Routing;
using System.Globalization;

namespace SIS.MvcFramework
{
    public static class WebHost
    {
        public static void Start(IMvcApplication application)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            IServiceCollection dependencyContainer = new ServiceCollection(); //това е мапинг между интерфейсните и класовете
            dependencyContainer.AddService<IHashService, HashService>();
            dependencyContainer.AddService<IUserCookieService, UserCookieService>();
            //dependencyContainer.AddService<ILogger, ConsoleLogger>();
            //Регистриране на сървис чрез подаване на метод, който да го създава.
            dependencyContainer.AddService<ILogger>(() => new FileLogger($"log.txt"));

            application.ConfigureServices(dependencyContainer);
            MvcFrameworkSettings settings = application.Configure();

            ServerRoutingTable serverRoutingTable = new ServerRoutingTable();
            RoutingEngine routingEngine = new RoutingEngine();
            routingEngine.RegisterRoutes(serverRoutingTable, application,settings, dependencyContainer);

            Server server = new Server(settings.PortNumber, serverRoutingTable);
            server.Run();
        }
    }
}