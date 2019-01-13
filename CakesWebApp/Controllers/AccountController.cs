using CakesWebApp.Models;
using CakesWebApp.ViewModels.Accounts;
using SIS.HTTP.Cookies;
using SIS.HTTP.Responses;
using SIS.MvcFramework;
using SIS.MvcFramework.Services;
using System;
using System.Linq;

namespace CakesWebApp.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IHashService hashService;

        public AccountController(IHashService hashService) //Вече го подавам през конструктора. Не го инициализирам вътре.
        {
            this.hashService = hashService;
        }

        [HttpGet("/account/register")]
        public IHttpResponse Register()
        {
            return this.View("Register");
        }

        //Този метод ще приема нашата заявка когато тя се прати от формата
        [HttpPost("/account/register")]
        public IHttpResponse DoRegister(DoRegisterInputModel model)
        {
            //any validation for null ? FormData
            //string username = this.Request.FormData["username"].ToString().Trim();
            //string password = this.Request.FormData["password"].ToString();
            //string confirmPassword = this.Request.FormData["confirmPassword"].ToString();

            //1.Validate data!!!
            if (string.IsNullOrWhiteSpace(model.Username) || model.Username.Trim().Length < 4)
            {
                return this.BadRequestErrorWithView("Please enter a valid username with length 4 or more characters!");
            }
            if(this.Db.Users.Any(u => u.Username == model.Username.Trim()))
            {
                return this.BadRequestErrorWithView("User with the same name already exists!");
            }
            if(string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 6)
            {
                return this.BadRequestErrorWithView("Please enter a valid password with length 6 or more characters!");
            }
            if (model.Password != model.ConfirmPassword)
            {
                return this.BadRequestErrorWithView("Passwords do not match!");
            }

            //2.Generate password hash
            string hashedPassword = this.hashService.Hash(model.Password);

            //3.Create user in DB
            User user = new User()
            {
                Name = model.Username.Trim(),
                Username = model.Username.Trim(),
                Password = hashedPassword //!
            };

            this.Db.Users.Add(user);
            try
            {
                this.Db.SaveChanges(); //await не го слагам понеже не ни е асинхронен сървъра.
            }
            catch (Exception e)
            {
                //TODO: log error - така се прави в истинските приложения
                return this.ServerError(e.Message);
            }

            //4.Login
            //TO DO

            //5.Redirect to home page
            return this.Redirect("/");
        }

        [HttpGet("/account/login")]
        public IHttpResponse Login()
        {
            return this.View("Login");
        }

        [HttpPost("/account/login")]
        public IHttpResponse DoLogin(DoLoginInputModel model)
        {
            //string username = this.Request.FormData["username"].ToString().Trim();
            //string password = this.Request.FormData["password"].ToString();

            string hashedPassword = this.hashService.Hash(model.Password);

            //1.Validate user and password
            User user = this.Db.Users.FirstOrDefault(u => u.Username == model.Username.Trim() && u.Password == hashedPassword);
            if(user == null)
            {
                return this.BadRequestErrorWithView("Invalid username or password!");
            }

            MvcUserInfo mvcUser = new MvcUserInfo { Username = user.Username };
            //2.Save cookie / session
            string cookieContent = this.CookieService.GetUserCookie(mvcUser); //.GetUserCookie(user.Username); ?
            HttpCookie authCookie = new HttpCookie(".auth-cakes", cookieContent, 7) { HttpOnly = true};
            this.Response.Cookies.Add(authCookie);

            //3.Redirect to home page
            return this.Redirect("/");
        }

        [HttpGet("/account/logout")]
        public IHttpResponse Logout()
        {
            if (!this.Request.Cookies.ContainsCookie(".auth-cakes"))
            {
                return this.Redirect("/");
            }

            HttpCookie cookie = this.Request.Cookies.GetCookie(".auth-cakes");
            cookie.Delete();
            //трябва да я запишем в респонса
            this.Response.Cookies.Add(cookie);
            //Redirect to home page
            return this.Redirect("/");
        }
    }
}