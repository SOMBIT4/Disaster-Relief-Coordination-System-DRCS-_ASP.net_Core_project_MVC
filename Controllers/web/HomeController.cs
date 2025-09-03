using Microsoft.AspNetCore.Mvc;

namespace DRCS.Controllers.web
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
