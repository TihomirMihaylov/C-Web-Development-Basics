using ChushkaWebApp.ViewModels.Home;
using SIS.HTTP.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChushkaWebApp.Controllers
{
    public class HomeController : BaseController
    {
        public IHttpResponse Index()
        {
            if (this.User.IsLoggedIn)
            {
                List<ProductViewModel> products = this.Db.Products.Where(x => !x.IsDeleted).Select(x => new ProductViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Price = x.Price
                }).ToList();
                IndexViewModel model = new IndexViewModel
                {
                    Products = products
                };

                return this.View("Home/IndexLoggedIn", model);
            }
                       
            return this.View();
        }
    }
}