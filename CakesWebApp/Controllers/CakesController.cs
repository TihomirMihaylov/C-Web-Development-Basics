using CakesWebApp.Models;
using CakesWebApp.ViewModels.Cakes;
using SIS.HTTP.Responses;
using SIS.MvcFramework;
using SIS.MvcFramework.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CakesWebApp.Controllers
{
    public class CakesController : BaseController
    {
        private readonly ILogger logger;

        public CakesController(ILogger logger)
        {
            this.logger = logger;
        }

        [HttpGet("/cakes/add")]
        public IHttpResponse AddCakes()
        {
            return this.View("AddCakes");
        }

        [HttpPost("/cakes/add")]
        public IHttpResponse DoAddCakes(DoAddCakesInputModel model)
        {
            //Create entry in DB
            Product product = model.To<Product>(); //Мапвам наобратно от вю модел към модел
            //Product cake = new Product()
            //{
            //    Name = model.Name,
            //    Price = model.Price,
            //    ImageUrl = model.Picture
            //};

            this.Db.Products.Add(product);
            try
            {
                this.Db.SaveChanges();
            }
            catch (Exception e)
            {
                return this.ServerError(e.Message);
            }

            //Redirect to home page
            return this.Redirect("/cakes/view?id=" + product.Id);
        }

        // cakes/view?id=1
        [HttpGet("/cakes/view")]
        public IHttpResponse ById(int id)
        {
            Product product = this.Db.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return this.BadRequestErrorWithView("Cake not found.");
            }

            CakeViewModel viewModel = product.To<CakeViewModel>(); //така викам ауто-мапъра

            //CakeViewModel viewModel = new CakeViewModel
            //{
            //    Name = product.Name,
            //    Price = product.Price,
            //    ImageUrl = product.ImageUrl
            //};

            return this.View("CakeById", viewModel); //тук подавам кой Layout искам да зареди!
        }

        // cakes/search?searchText=cake
        [HttpGet("/cakes/search")]
        public IHttpResponse Search(string searchText)
        {
            List<CakeViewModel> cakes = this.Db.Products.Where(x => x.Name.Contains(searchText))
                                .Select(x => new CakeViewModel
                                {
                                    Id = x.Id,
                                    Name = x.Name,
                                    Price = x.Price,
                                    ImageUrl = x.ImageUrl
                                }).ToList();

            SearchViewModel cakesViewModel = new SearchViewModel
            {
                Cakes = cakes, //Колекциите да не ги подавам на Вю-то като Лист - или като масив или в друг вю модел
                SearchText = searchText
            };

            return this.View("Search", cakesViewModel);
        }
    }
}