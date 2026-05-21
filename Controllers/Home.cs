using Microsoft.AspNetCore.Mvc;

namespace MegaProject.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}