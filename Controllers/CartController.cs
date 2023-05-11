using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobilePhone.Models;
using System.Web;
using MobilePhone.Helpers;
using Microsoft.CodeAnalysis;
using System.Configuration;

namespace MobilePhone.Controllers
{
    public class CartController : Controller
    {
        private MyDbContext _dbContext;
        private readonly IConfiguration Configuration;

        public CartController(MyDbContext _db, IConfiguration _configuration)
        {
            _dbContext = _db;
            Configuration = _configuration;
        }

        [HttpPost]
        [ActionName("AddToCart")]
        public ActionResult AddToCart(string productId, int quantity = 1) {
            
            List<CartItem> cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
            
            CartItem item = cart.FirstOrDefault(i => i.productID == productId);

            if (item != null)
            {
                item.quantity += quantity;
            }
            else
            {
                Product pro = _dbContext.Products.FirstOrDefault(i => i.ProductID == productId);
                if (pro != null)
                {
                    cart.Add(new CartItem
                    {
                        productID = pro.ProductID,
                        productName = pro.ProductName,
                        price = pro.Price,
                        quantity = quantity
                    });
                }
            }

            HttpContext.Session.Set("Cart", cart);

            return Json(new { success = true });
        }

        [HttpPost]
        [ActionName("UpdateCart")]
        public ActionResult UpdateCart(string productID, int quantity)
        {
            List<CartItem> cart = HttpContext.Session.Get<List<CartItem>>("Cart");

            CartItem item = cart.FirstOrDefault(i => i.productID == productID);

            item.quantity = quantity;

            HttpContext.Session.Set("Cart", cart);

            return Json(new { success = true });
        }
        public IActionResult Index()
        {

            if (HttpContext.Session.GetString("username") == null)
            {
                TempData["Message"] = "Login to continue!";
                return RedirectToAction("Auth", "Account");
            }

            ViewBag.AgentID = HttpContext.Session.GetString("username");
            ViewBag.AgentName = HttpContext.Session.GetString("name");
            ViewBag.Address = HttpContext.Session.GetString("address");
            ViewBag.Contact = HttpContext.Session.GetString("contact");

            ViewData["endpoint"] = Configuration["Momo:endpoint"];
            ViewData["name"] = HttpContext.Session.GetString("username");

            var myCart = HttpContext.Session.Get<List<CartItem>>("Cart");
            return View(myCart);
        }
    }
}
