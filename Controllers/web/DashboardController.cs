using Microsoft.AspNetCore.Mvc;

namespace DRCS.Controllers.web
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var role = HttpContext.Items["role"]?.ToString() ?? "User";

            if (role.StartsWith("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (role == "Volunteer")
            {
                return RedirectToAction("Index", "Volunteer");
            }
            else // Normal User
            {
                return RedirectToAction("Index", "User");
            }
        }
    }
}
