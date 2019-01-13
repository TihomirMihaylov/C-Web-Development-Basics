using SIS.HTTP.Enums;
using SIS.HTTP.Headers;
using SIS.HTTP.Requests;
using SIS.HTTP.Responses;
using SIS.MvcFramework.Loggers;
using SIS.MvcFramework.Services;
using SIS.WebServer.Results;
using SIS.WebServer.Routing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SIS.MvcFramework.Routing
{
    public class RoutingEngine
    {
        public void RegisterRoutes(ServerRoutingTable routingTable, IMvcApplication application, MvcFrameworkSettings settings, IServiceCollection serviceCollection)
        {
            RegisterStaticFiles(routingTable, settings);
            RegisterActions(routingTable, application,settings, serviceCollection);
            RegisterDefaultRoute(routingTable);
        }

        private void RegisterStaticFiles(ServerRoutingTable routingTable, MvcFrameworkSettings settings)
        {
            string path = settings.WwwrootPath;
            if (!Directory.Exists(path))
            {
                return;
            }

            string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories); // * --> all files
            foreach (string file in files)
            {
                string url = file.Replace("\\", "/").Replace(path, string.Empty);
                routingTable.Add(HttpRequestMethod.Get, url, (request) =>
                {
                    string content = File.ReadAllText(file);
                    string contentType = "text/plain";
                    if (file.EndsWith(".css"))
                    {
                        contentType = "text/css";
                    }
                    else if (file.EndsWith(".js"))
                    {
                        contentType = "application/javascript";
                    }
                    else if (file.EndsWith(".bmp"))
                    {
                        contentType = "image/bmp";
                    }
                    else if (file.EndsWith(".png"))
                    {
                        contentType = "image/png";
                    }
                    else if (file.EndsWith(".jpg") || file.EndsWith(".jpeg"))
                    {
                        contentType = "image/jpeg";
                    }
                    else if (file.EndsWith(".ico")) //Не е ли .icon ?
                    {
                        contentType = "image/x-icon";
                    }

                    return new TextResult(content, HttpResponseStatusCode.Ok, contentType );
                });

                Console.WriteLine($"Content registered: {file} => {HttpRequestMethod.Get} => {url}");
            }
        }

        private static void RegisterDefaultRoute(ServerRoutingTable routingTable)
        {
            if (!routingTable.Contains(HttpRequestMethod.Get, "/")
                && routingTable.Contains(HttpRequestMethod.Get, "/Home/Index"))
            {
                routingTable.Add(HttpRequestMethod.Get, "/", (request) =>
                    routingTable.Get(HttpRequestMethod.Get, "/Home/Index")(request));

                Console.WriteLine($"Route registered: reuse Home/Index => {HttpRequestMethod.Get} => /");
            }
        }

        private static void RegisterActions(ServerRoutingTable routingTable, IMvcApplication application, MvcFrameworkSettings settings, IServiceCollection serviceCollection)
        {
            IUserCookieService userCookieService = serviceCollection.CreateInstance<IUserCookieService>();
            IEnumerable<Type> controllers = application.GetType().Assembly.GetTypes() //Взимам всички контролери
                .Where(myType => myType.IsClass
                                && !myType.IsAbstract
                                && myType.IsSubclassOf(typeof(Controller)));

            foreach (Type controller in controllers)
            {
                IEnumerable<MethodInfo> getMethods = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                foreach (MethodInfo methodInfo in getMethods)
                {
                    HttpAttribute httpAttribute = (HttpAttribute)methodInfo.GetCustomAttributes(true).FirstOrDefault(ca => //Взимам първия атрибут
                    ca.GetType().IsSubclassOf(typeof(HttpAttribute)));

                    HttpRequestMethod method = HttpRequestMethod.Get;
                    string path = null;
                    if (httpAttribute != null)
                    {
                        method = httpAttribute.Method;
                        path = httpAttribute.Path;
                    }

                    if (path == null)
                    {
                        string controlerName = controller.Name;
                        if (controlerName.EndsWith("Controller"))
                        {
                            controlerName = controlerName.Substring(0, controlerName.Length - "Controller".Length);
                        }

                        string actionName = methodInfo.Name;
                        path = $"/{controlerName}/{actionName}";
                    }
                    else if (!path.StartsWith("/"))
                    {
                        path = "/" + path;
                    }

                    AuthorizeAttribute authorizeArrtibute = methodInfo.GetCustomAttributes(true)
                        .FirstOrDefault(ca => ca.GetType() == typeof(AuthorizeAttribute)) as AuthorizeAttribute;
                    routingTable.Add(method, path, (request) =>
                    {
                        if (authorizeArrtibute != null)
                        {
                            MvcUserInfo userData = Controller.GetUserData(request.Cookies, userCookieService);
                            if(userData == null || !userData.IsLoggedIn
                                || (authorizeArrtibute.RoleName != null 
                                && authorizeArrtibute.RoleName != userData.Role))
                            {
                                HttpResponse response = new HttpResponse();
                                response.Headers.Add(new HttpHeader("Location", settings.LoginPageUrl)); // == Redirect
                                response.StatusCode = HttpResponseStatusCode.SeeOther;
                                return response;
                            }
                        }

                        return ExecuteAction(controller, methodInfo, request, serviceCollection);
                    });

                    Console.WriteLine($"Route registered: {controller.Name}.{methodInfo.Name} => {method} => {path}");
                }
            }
        }

        private static IHttpResponse ExecuteAction(Type controllerType, MethodInfo methodInfo, IHttpRequest request, IServiceCollection serviceCollection)
        {
            //1. Create instance of controllerName
            Controller controllerInstance = serviceCollection.CreateInstance(controllerType) as Controller;

            //2. Set request
            if (controllerInstance == null)
            {
                return new TextResult("Controller not found!", HTTP.Enums.HttpResponseStatusCode.InternalServerError);
            }

            controllerInstance.Request = request;
            controllerInstance.ViewEngine = new ViewEngine.ViewEngine(); //TODO: use service collection
            controllerInstance.CookieService = serviceCollection.CreateInstance<IUserCookieService>();

            //3.
            List<object> actionParametersList = GetActionParameterObjects(methodInfo, request, serviceCollection);

            IHttpResponse response = methodInfo.Invoke(controllerInstance, actionParametersList.ToArray()) as IHttpResponse; //Върху коя инстанция ще се извика и с какви параметри
            //4. Return action result
            return response;
        }


        private static List<object> GetActionParameterObjects(MethodInfo methodInfo, IHttpRequest request, IServiceCollection serviceCollection)
        {
            //3. Invoke actionName
            ParameterInfo[] actionParameteres = methodInfo.GetParameters();
            List<object> actionParametersList = new List<object>();
            foreach (ParameterInfo parameter in actionParameteres)
            {
                //TODO: improve this check
                if (parameter.ParameterType.IsValueType || Type.GetTypeCode(parameter.ParameterType) == TypeCode.String)
                {
                    string stringValue = GetRequestDate(request, parameter.Name);
                    actionParametersList.Add(ObjectMapper.TryParse(stringValue, parameter.ParameterType));
                }
                else
                {
                    object instance = serviceCollection.CreateInstance(parameter.ParameterType);
                    //Populate instance properties from request
                    PropertyInfo[] properties = parameter.ParameterType.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        //TO DO: support IEnumerable
                        string stringValue = GetRequestDate(request, property.Name);
                        //Check type of property
                        // -> decimal set --> decimal.TryParse() etc.int, double, char, long, DateTime
                        object value = ObjectMapper.TryParse(stringValue, property.PropertyType);

                        property.SetMethod.Invoke(instance, new object[] { value });
                    }

                    actionParametersList.Add(instance);
                }
            }

            return actionParametersList;
        }

        private static string GetRequestDate(IHttpRequest request, string key)
        {
            key = key.ToLower();
            string stringValue = null;
            if (request.FormData.Any(x => x.Key.ToLower() == key))
            {
                stringValue = request.FormData.First(x => x.Key.ToLower() == key).Value.ToString().UrlDecode();
            }
            else if (request.QueryData.Any(x => x.Key.ToLower() == key))
            {
                stringValue = request.QueryData.First(x => x.Key.ToLower() == key).Value.ToString().UrlDecode();
            }

            return stringValue;
        }
    }
}