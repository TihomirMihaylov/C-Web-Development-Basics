using SIS.HTTP.Cookies;
using SIS.HTTP.Enums;
using SIS.HTTP.Headers;
using SIS.HTTP.Requests;
using SIS.HTTP.Responses;
using SIS.MvcFramework.Services;
using SIS.MvcFramework.ViewEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.MvcFramework
{
    public abstract class Controller
    {
        protected Controller()
        {
            this.Response = new HttpResponse {  StatusCode = HttpResponseStatusCode.Ok};
        }

        public IHttpRequest Request { get; set; }

        public IHttpResponse Response { get; set; }

        public IViewEngine ViewEngine { get; set; }

        public IUserCookieService CookieService { get; internal set; }

        public static MvcUserInfo GetUserData(IHttpCookieCollection cookieCollection, IUserCookieService cookieService)
        {
            //Без да бъркам в базата само от бисквитката вадя кой е юзъра
            if (!cookieCollection.ContainsCookie(".auth-cakes"))
            {
                return new MvcUserInfo();
            }

            HttpCookie cookie = cookieCollection.GetCookie(".auth-cakes");
            string cookieContent = cookie.Value;

            try
            {
                return cookieService.GetUserData(cookieContent);
            }
            catch (Exception)
            {
                return new MvcUserInfo();
            }
        }

        protected MvcUserInfo User => GetUserData(this.Request.Cookies, this.CookieService);

        protected IHttpResponse View(string viewName = null, string layoutName = "_Layout")
        {
            return this.View(viewName,(object)null, layoutName); // (object)null -> обект от тип null
        }

        protected IHttpResponse View<T>(string viewName = null, T model = null, string layoutName = "_Layout")
            where T : class
        {
            if(viewName == null)
            {
                viewName = this.Request.Path.Trim('/', '\\');
                if (string.IsNullOrWhiteSpace(viewName))
                {
                    viewName = "Home/Index";
                }
            }

            string allContent = this.GetViewContent(viewName, model, layoutName);
            this.PrepareHtmlResult(allContent);
            return this.Response;
        }

        protected IHttpResponse View<T>(T model = null, string layoutName = "_Layout")
            where T : class
        {
            return this.View(null, model, layoutName);
        }

        protected IHttpResponse File(byte[] content)
        {
            this.Response.Headers.Add(new HttpHeader("Content-Length", content.Length.ToString()));
            this.Response.Headers.Add(new HttpHeader("Content-Disposition", "inline"));
            this.Response.Content = content;
            return this.Response;
        }

        protected IHttpResponse Redirect(string location)
        {
            this.Response.Headers.Add(new HttpHeader("Location", location));
            this.Response.StatusCode = HttpResponseStatusCode.SeeOther;
            return this.Response;
        }

        protected IHttpResponse Text(string content)
        {
            this.Response.Headers.Add(new HttpHeader("Content-Type", "text/plain; charset=utf-8"));
            this.Response.Content = Encoding.UTF8.GetBytes(content);
            return this.Response;
        }

        protected IHttpResponse BadRequestError(string errorMessage)
        {
            ErrorViewModel viewModel = new ErrorViewModel { Error = errorMessage };
            string allContent = this.GetViewContent("Error", viewModel);
            this.PrepareHtmlResult(allContent);
            this.Response.StatusCode = HttpResponseStatusCode.BadRequest;
            return this.Response;
        }

        protected IHttpResponse BadRequestErrorWithView(string errorMessage)
        {
            return this.BadRequestErrorWithView(errorMessage, (object)null);
        }

        protected IHttpResponse BadRequestErrorWithView<T>(string errorMessage, T model, string layoutName = "_Layout")
        {
            ErrorViewModel viewModel = new ErrorViewModel { Error = errorMessage };
            string errorContent = this.GetViewContent("Error", viewModel, null);

            string viewName = this.Request.Path.Trim('/', '\\');
            if (string.IsNullOrWhiteSpace(viewName))
            {
                viewName = "Home/Index";
            }

            string viewContent = this.GetViewContent(viewName, model, null);
            string allViewContent = errorContent + Environment.NewLine + viewContent;
            string errorAndViewContent = this.ViewEngine.GetHtml(viewName, allViewContent, model, this.User);

            string layoutFileContent = System.IO.File.ReadAllText($"Views/{layoutName}.html");
            string allContent = layoutFileContent.Replace("@RenderBody()", errorAndViewContent);
            string layoutContent = this.ViewEngine.GetHtml("_Layout", allContent, model, this.User);

            this.PrepareHtmlResult(layoutContent);
            this.Response.StatusCode = HttpResponseStatusCode.BadRequest;
            return this.Response;
        }

        protected IHttpResponse ServerError(string errorMessage)
        {
            ErrorViewModel viewModel = new ErrorViewModel { Error = errorMessage };
            string allContent = this.GetViewContent("Error", viewModel);
            this.PrepareHtmlResult(allContent);
            this.Response.StatusCode = HttpResponseStatusCode.InternalServerError;
            return this.Response;
        }

        private string GetViewContent<T>(string viewName, T model, string layoutName = "_Layout") //Така поддържаме повече от един начин за визуализиране на layout
        {
            string content = this.ViewEngine.GetHtml(viewName, System.IO.File.ReadAllText("Views/" + viewName + ".html"), model, this.User);

            if(layoutName != null)
            {
                string layoutFileContent = System.IO.File.ReadAllText($"Views/{layoutName}.html");
                string allContent = layoutFileContent.Replace("@RenderBody()", content);
                string layoutContent = this.ViewEngine.GetHtml("_Layout", allContent, model, this.User);
                return layoutContent;
            }

            return content;
        }

        private void PrepareHtmlResult(string content)
        {
            this.Response.Headers.Add(new HttpHeader("Content-Type", "text/html; charset=utf-8"));
            this.Response.Content = Encoding.UTF8.GetBytes(content);
        }
    }
}