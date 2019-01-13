using CakesWebApp.ViewModels.User;
using SIS.HTTP.Responses;
using SIS.MvcFramework;
using System.Linq;

namespace CakesWebApp.Controllers
{
    public class UserController : BaseController
    {
        [HttpGet("/user/profile")]
        public IHttpResponse Profile()
        {
            //read profile data
            ProfileViewModel viewModel = this.Db.Users.Where(x => x.Username == this.User.Username)
                             .Select(x => new ProfileViewModel
                                          {
                                              Username = x.Username,
                                              RegisteredOn = x.DateOfRegistration,
                                              OrdersCount = x.Orders.Count
                                          }).FirstOrDefault();

            if(viewModel == null)
            {
                return this.BadRequestErrorWithView("Profile page not accessible for this user.");
            }

            if(viewModel.OrdersCount > 0)
            {
                viewModel.OrdersCount--; //Винаги ще има една незавършена поръчка, която е текущата му кошница
            }

            return this.View("Profile", viewModel);
        }
    }
}