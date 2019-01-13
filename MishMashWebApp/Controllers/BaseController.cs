using MishMashWebApp.Data;
using SIS.MvcFramework;

namespace MishMashWebApp.Controllers
{
    public abstract class BaseController : Controller
    {
        public BaseController()
        {
            this.Db = new ApplicationDbContext();
        }

        protected ApplicationDbContext Db { get; }
    }
}