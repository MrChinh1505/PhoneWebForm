using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MobilePhone.Models;

namespace MobilePhone.Controllers
{
    public class History : Controller
    {
        private MyDbContext _db;
        public History(MyDbContext db) {
            _db = db;
        }
        // GET: History
        public ActionResult Index()
        {
            ViewData["name"] = HttpContext.Session.GetString("username");
            string agentID = HttpContext.Session.GetString("username");
            List<Delivery> deliveryList = _db.Deliveries.Where(d => d.AgentID == agentID).ToList();
            return View(deliveryList);
        }
    }
}
