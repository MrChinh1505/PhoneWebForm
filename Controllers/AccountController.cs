using Microsoft.AspNetCore.Mvc;
using MobilePhone.Models;

namespace MobilePhone.Controllers
{
    public class AccountController : Controller
    {

        private MyDbContext _db;
        public AccountController(MyDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Auth() {
            if (HttpContext.Session.GetString("username") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpPost]
        public IActionResult Auth(string username, string password)
        {
            var result = _db.Agents.FirstOrDefault(a => a.id == username);

            if(result != null && result.pwd == password)
            {
                // save session 
                HttpContext.Session.SetString("username", username);
                HttpContext.Session.SetString("name", result.name);
                HttpContext.Session.SetString("contact", result.contact);
                HttpContext.Session.SetString("address", result.address);
                // redirect to Home
                return RedirectToAction("Index","Home");
            }
            else
            {
                ViewBag.msg = "Invalid password!";
            }
            return View();
        }

        public IActionResult Logout()
        {
            // clear session
            HttpContext.Session.Clear();
            // redirect to home with action index
            return RedirectToAction("Index", "Home");
        }
    }
}
