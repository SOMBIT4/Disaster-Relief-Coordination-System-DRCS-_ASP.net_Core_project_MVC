using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DRCS.Controllers.web
{
    
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            //var role = HttpContext.Items["role"]?.ToString() ?? "";
            //if (!role.StartsWith("Admin"))
            //    return Unauthorized();
            var userId = User.FindFirst("userId")?.Value;
            var role = User.FindFirst("role")?.Value;

            Console.WriteLine($"UserId={userId}, Role={role}");
            return Json(new { userId, role });
            return View(); // Views/Dashboard/Admin/Index.cshtml
        }
    }
}
