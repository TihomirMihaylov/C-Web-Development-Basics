using CakesWebApp.Data;
using SIS.MvcFramework;

namespace CakesWebApp.Controllers
{
    public abstract class BaseController : Controller //Добре е да имам един базов клас, който всички наследяват.
    {
        //Тук оставяме само нещата, които ползваме в нашето приложение конкретно. Всичко друго пренасяме във фреймуърка.
        //Когато имаме много функционалности си има сървиси и всеки върши определена работа. Сега работим директно с базата
        protected BaseController() //В ASP.NET Core базата се подава в коструктора чрез dependency injection
        {
            this.Db = new CakesDbContext();
        }

        protected CakesDbContext Db { get; }
    }
}