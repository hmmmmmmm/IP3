using System.Web.Mvc;

namespace IP3_Group4.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index() // Action for home page
        {
            return View(); // returns home page
        }
    }
}