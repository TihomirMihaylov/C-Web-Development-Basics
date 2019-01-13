using SIS.HTTP.Responses;
using SIS.MvcFramework;
using System.Collections.Generic;

namespace CakesWebApp.Controllers
{
    public class HomeController : BaseController
    {
        [HttpGet("/")]
        public IHttpResponse Index()
        {
            return this.View("Index");
        }

        [HttpGet("/hello")]
        public IHttpResponse HelloUser()
        {
            HelloUserViewModel viewModel = new HelloUserViewModel { Username = this.User.Username };
            return this.View("HelloUser", viewModel);
        }
    }

    public class HelloUserViewModel
    {
        public string Username { get; set; }
    }
}