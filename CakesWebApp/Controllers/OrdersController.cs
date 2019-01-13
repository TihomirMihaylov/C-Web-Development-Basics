using CakesWebApp.Models;
using CakesWebApp.ViewModels.Cakes;
using CakesWebApp.ViewModels.Orders;
using SIS.HTTP.Responses;
using SIS.MvcFramework;
using System.Linq;

namespace CakesWebApp.Controllers
{
    public class OrdersController : BaseController
    {
        //Add to order
        [HttpPost("/orders/add")]
        public IHttpResponse Add(int productId)
        {
            int? userId = this.Db.Users.FirstOrDefault(x => x.Username == this.User.Username)?.Id;
            if (userId == null)
            {
                return this.BadRequestErrorWithView("Please login first.");
            }

            Order lastUserOrder = this.Db.Orders.Where(x => x.UserId == userId)
                            .OrderByDescending(x => x.Id)
                            .FirstOrDefault();

            if (lastUserOrder == null) //първа поръчка за потребителя
            {
                lastUserOrder = new Order
                {
                    UserId = userId.Value //nullable int
                };

                this.Db.Orders.Add(lastUserOrder);
                this.Db.SaveChanges();
            }

            OrderProduct orderProduct = new OrderProduct
            {
                OrderId = lastUserOrder.Id,
                ProductId = productId
            };

            this.Db.OrderProducts.Add(orderProduct);
            this.Db.SaveChanges();

            return this.Redirect("/orders/byid?id=" + lastUserOrder.Id);
        }

        //Order by id (Order details)
        [HttpGet("/orders/byid")]
        public IHttpResponse GetById(int id)
        {
            Order order = this.Db.Orders.FirstOrDefault(x => x.Id == id //трябва да съществува
                                    && x.User.Username == this.User.Username);//и да е на същия потребител

            if (order == null)
            {
                return this.BadRequestErrorWithView("Invalid order Id.");
            }

            int lastOrderId = this.Db.Orders.Where(x => x.User.Username == this.User.Username)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefault().Id;

            GetByIdViewModel viewModel = new GetByIdViewModel
            {
                Id = id,
                Products = this.Db.OrderProducts.Where(x => x.OrderId == id)
                            .Select(x => new CakeViewModel
                            {
                                Id = x.Product.Id,
                                Name = x.Product.Name,
                                ImageUrl = x.Product.ImageUrl,
                                Price = x.Product.Price
                            }).ToList(),
                IsShoppingCart = lastOrderId == id
            };

            return this.View("OrderById", viewModel);
        }

        //List of orders
        [HttpGet("/orders/list")]
        public IHttpResponse List()
        {
            var orders = this.Db.Orders.Where(x => x.User.Username == this.User.Username)
                .Select(x => new OrderInListViewModel
                {
                    Id = x.Id,
                    CreatedOn = x.DateOfCreation,
                    NumberOfProducts = x.Products.Count(),
                    SumOfProductPrices = x.Products.Sum(p => p.Product.Price)
                });

            return this.View("OrdersList", orders.ToArray());
        }

        //Finish order (shopping cart => order)
        [HttpPost("/orders/finish")]
        public IHttpResponse Finish(int orderId)
        {
            int? userId = this.Db.Users.FirstOrDefault(x => x.Username == this.User.Username)?.Id;
            if (userId == null)
            {
                return this.BadRequestErrorWithView("Please login first.");
            }

            //Validate that the current user has mermission to finish this order
            if(!this.Db.Orders.Any(x => x.Id == orderId //поръчката трябва да съществува
                && x.User.Username == this.User.Username)) //и д е на текущия юзър
            {
                return this.BadRequestErrorWithView("Order not found.");
            }

            Order newEmptyOrder = new Order
            {
                UserId = userId.Value //nullable int
            };

            this.Db.Orders.Add(newEmptyOrder);
            this.Db.SaveChanges();

            return this.Redirect("/orders/list");
        }
    }
}