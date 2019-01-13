using ChushkaWebApp.Models;
using ChushkaWebApp.ViewModels.Products;
using SIS.HTTP.Responses;
using SIS.MvcFramework;
using System;
using System.Linq;

namespace ChushkaWebApp.Controllers
{
    public class ProductsController : BaseController
    {
        [Authorize]
        public IHttpResponse Details(int id)
        {
            ProductDetailsViewModel viewModel = this.Db.Products.Where(x => !x.IsDeleted)
                .Select(x => new ProductDetailsViewModel
                {
                    Type = x.Type,
                    Name = x.Name,
                    Description = x.Description,
                    Price = x.Price,
                    Id = x.Id
                })
                .FirstOrDefault(x => x.Id == id);
            if(viewModel == null)
            {
                return this.BadRequestError("Invalid product Id");
            }

            //ProductDetailsViewModel viewModel = dbModel.To<ProductDetailsViewModel>(); //Не става с аутомапера защото мапва енумерацията като инт
            return this.View(viewModel);
        }

        [Authorize]
        public IHttpResponse Order(int id)
        {
            User user = this.Db.Users.FirstOrDefault(x => x.Username == this.User.Username);
            if(user == null)
            {
                return this.BadRequestError("Invalid user.");
            }

            Order order = new Order
            {
                UserId = user.Id,
                ProductId = id
            };

            this.Db.Orders.Add(order);
            this.Db.SaveChanges();

            return this.Redirect("/");
        }

        [Authorize("Admin")]
        public IHttpResponse Create() => this.View();

        [Authorize("Admin")]
        [HttpPost]
        public IHttpResponse Create(CreateProductInputModel model)
        {
            if(!Enum.TryParse(model.Type, out ProductType type))
            {
                return this.BadRequestErrorWithView("Invalid type.");
            }

            Product product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                Type = type
            };

            this.Db.Products.Add(product);
            this.Db.SaveChanges();

            return this.Redirect("/Products/Details?id=" + product.Id);
        }

        [Authorize("Admin")]
        public IHttpResponse Edit(int id)
        {
            UpdateDeleteProductInputModel viewModel = this.Db.Products.Where(x => !x.IsDeleted)
                .Select(x => new UpdateDeleteProductInputModel
                {
                    Type = x.Type.ToString(),
                    Name = x.Name,
                    Description = x.Description,
                    Price = x.Price,
                    Id = x.Id
                })
                .FirstOrDefault(x => x.Id == id);
            if (viewModel == null)
            {
                return this.BadRequestError("Invalid product Id");
            }

            return this.View(viewModel);
        }

        [Authorize("Admin")]
        [HttpPost]
        public IHttpResponse Edit(UpdateDeleteProductInputModel model)
        {
            Product product = this.Db.Products.FirstOrDefault(x => x.Id == model.Id);
            if(product == null || product.IsDeleted)
            {
                return this.BadRequestErrorWithView("Product not found.");
            }
            if (!Enum.TryParse(model.Type, out ProductType type))
            {
                return this.BadRequestErrorWithView("Invalid type.");
            }

            product.Type = type;
            product.Name = model.Name;
            product.Price = model.Price;
            product.Description = model.Description;
            this.Db.SaveChanges();

            return this.Redirect("/Products/Details?id=" + product.Id);
        }

        [Authorize("Admin")]
        public IHttpResponse Delete(int id)
        {
            UpdateDeleteProductInputModel viewModel = this.Db.Products.Where(x => !x.IsDeleted)
                .Select(x => new UpdateDeleteProductInputModel
                {
                    Type = x.Type.ToString(),
                    Name = x.Name,
                    Description = x.Description,
                    Price = x.Price,
                    Id = x.Id
                })
                .FirstOrDefault(x => x.Id == id);
            if (viewModel == null)
            {
                return this.BadRequestError("Invalid product Id");
            }

            return this.View(viewModel);
        }

        [Authorize("Admin")]
        [HttpPost]
        public IHttpResponse Delete(int id, string name) //name го слагам само за да има различна сигнатура
        {
            Product product = this.Db.Products.FirstOrDefault(x => x.Id == id);
            if (product == null || product.IsDeleted)
            {
                return this.Redirect("/");
            }

            //this.Db.Products.Remove(product);
            product.IsDeleted = true; //Soft deletion
            this.Db.SaveChanges();

            return this.Redirect("/");
        }
    }
}