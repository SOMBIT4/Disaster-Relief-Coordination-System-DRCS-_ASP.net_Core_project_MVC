using Microsoft.AspNetCore.Mvc;

namespace DRCS.Controllers.web
{
    public class AuthApiController : Controller
    {

        public IActionResult Login()
        {
            return View();
        }


        public IActionResult Register()
        {
            return View();
        }
    }
}