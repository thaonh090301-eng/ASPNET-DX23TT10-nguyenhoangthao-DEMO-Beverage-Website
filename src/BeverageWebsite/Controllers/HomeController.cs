using System.Web.Mvc;

namespace BeverageWebsite
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
