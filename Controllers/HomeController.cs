using Microsoft.AspNetCore.Mvc;
using MobilePhone.Models;
using System.Diagnostics;

namespace MobilePhone.Controllers
{
    public class HomeController : Controller
    {
        private MyDbContext _dbContext;
        
        public HomeController(MyDbContext _db)
        {
            _dbContext = _db;
        }

        public IActionResult Index()
        {
            ViewData["name"] = HttpContext.Session.GetString("username");
            var data = _dbContext.Products.ToList();
            return View(data);

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}