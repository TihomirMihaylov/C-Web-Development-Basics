using MishMashWebApp.Models;
using MishMashWebApp.ViewModels.Users;
using SIS.HTTP.Cookies;
using SIS.HTTP.Responses;
using SIS.MvcFramework;
using SIS.MvcFramework.Services;
using System;
using System.Linq;

namespace MishMashWebApp.Controllers
{
    public class UsersController : BaseController
    {
        private readonly IHashService hashService;

        public UsersController(IHashService hashService)
        {
            this.hashService = hashService;
        }

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

        public IHttpResponse Login()
        {
            return this.View();
        }

        [HttpPost]
        public IHttpResponse Login(DoLoginInputModel model)
        {
            string hashedPassword = this.hashService.Hash(model.Password);

            User user = this.Db.Users.FirstOrDefault(u =>
                u.Username == model.Username.Trim() &&
                u.Password == hashedPassword);

            if(user == null)
            {
                return this.BadRequestErrorWithView("Invalid username or password.");
            }

            MvcUserInfo mvcUser = new MvcUserInfo { Username = user.Username, Role = user.Role.ToString(), Info = user.Email};
            string cookieContent = this.CookieService.GetUserCookie(mvcUser);
            HttpCookie authCookie = new HttpCookie(".auth-cakes", cookieContent, 7) { HttpOnly = true };
            this.Response.Cookies.Add(authCookie);

            return this.Redirect("/");
        }

        public IHttpResponse Register()
        {
            return this.View();
        }

        [HttpPost]
        public IHttpResponse Register(DoRegisterInputModel model)
        {
            //1.Validate data!!!
            if (string.IsNullOrWhiteSpace(model.Username) || model.Username.Trim().Length < 4)
            {
                return this.BadRequestErrorWithView("Please enter a valid username with length 4 or more characters!");
            }
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return this.BadRequestErrorWithView("Please enter a valid email");
            }
            if (this.Db.Users.Any(u => u.Username == model.Username.Trim()))
            {
                return this.BadRequestErrorWithView("User with the same name already exists!");
            }
            if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 6)
            {
                return this.BadRequestErrorWithView("Please enter a valid password with length 6 or more characters!");
            }
            if (model.Password != model.ConfirmPassword)
            {
                return this.BadRequestErrorWithView("Passwords do not match!");
            }

            //2.Generate password hash
            string hashedPassword = this.hashService.Hash(model.Password);

            Role role = Role.User;
            if (!this.Db.Users.Any()) //Първия регистриран потребител става админ
            {
                role = Role.Admin;
            }

            //3.Create user in DB
            User user = new User()
            {
                Username = model.Username.Trim(),
                Email = model.Email.Trim(),
                Password = hashedPassword,
                Role = role
            };

            this.Db.Users.Add(user);
            try
            {
                this.Db.SaveChanges();
            }
            catch (Exception e)
            {
                return this.ServerError(e.Message);
            }

            return this.Redirect("/Users/Login");
        }
    }
}