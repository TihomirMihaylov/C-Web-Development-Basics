using ChushkaWebApp.ViewModels.Orders;
using SIS.HTTP.Responses;
using SIS.MvcFramework;
using System.Collections.Generic;
using System.Linq;

namespace ChushkaWebApp.Controllers
{
    public class OrdersController : BaseController
    {
        [Authorize("Admin")]
        public IHttpResponse All()
        {
            List<OrderViewModel> orders = this.Db.Orders.Select(x => new OrderViewModel
            {
                Id = x.Id,
                Username = x.User.Username,
                ProductName = x.Product.Name,
                OrderedOn = x.OrderedOn
            }).ToList();

            AllOrdersViewModel model = new AllOrdersViewModel { Orders = orders };
            return this.View(model);
        }
    }
}