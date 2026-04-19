using Microsoft.AspNetCore.Mvc;

namespace MegaProject.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}