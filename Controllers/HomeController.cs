using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Task4.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace Task4.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        UserContext db;
        public HomeController(ILogger<HomeController> logger, UserContext context)
        {
            _logger = logger;
            db = context;
        }
        
        [Authorize]        
        public IActionResult Index()
        {
            User user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null || user.IsBlocked)
            {
                return RedirectToAction("Logout", "Account");
            }
            var users = db.Users;
            ViewBag.Users = users;
            return View();
        }        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
