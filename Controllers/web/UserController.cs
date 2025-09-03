using Microsoft.AspNetCore.Mvc;

namespace DRCS.Controllers.web
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            var role = HttpContext.Items["role"]?.ToString() ?? "";
            if (role != "User")
                return Unauthorized();

            return View(); // Views/Dashboard/User/Index.cshtml
        }
    }
}
