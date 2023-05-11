using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobilePhone.Helpers;
using MobilePhone.Models;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using ZaloDotNetSDK;

namespace MobilePhone.Controllers
{
    public class OrderController : Controller
    {
        private MyDbContext _db;
        public OrderController(MyDbContext db) {
            _db = db;
        }

        [HttpPost]
        public IActionResult ConfirmOrder(string OrderID, string agentID, string total)
        {
            ViewData["name"] = HttpContext.Session.GetString("username");
            try
            {
                List<CartItem> carts = HttpContext.Session.Get<List<CartItem>>("Cart");

                // Create new order
                Order order = new Order
                {
                    OrderID = OrderID,
                    OrderDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    AgentID = agentID,
                    Total = int.Parse(total),
                };

                _db.Orders.Add(order);

                // save into database
                _db.SaveChanges();
                // save in OrderDetail
                foreach (var item in carts)
                {
                    OrderDetail orderDetail = new OrderDetail
                    {
                        
                        OrderID = OrderID,
                        ProductID = item.productID,
                        Quantity = item.quantity,
                        Total = item.price * item.quantity,
                    };
                    _db.OrderDetails.Add(orderDetail);
                }

                _db.SaveChangesAsync();

                // remove Cart in session
                HttpContext.Session.Remove("Cart");

                // return view success with orderID instead of JSON

                TempData["Message"] = "Success!";
                TempData["orderID"] = order.OrderID;
                TempData["Date"] = order.OrderDate;




                return RedirectToAction("Success");
            }
            catch (Exception e)
            {
                TempData["Message"] = "Fail!";
                return RedirectToAction("Fail");
            }   
        }

        public ActionResult Success() {

            return View();
        }

        public ActionResult Fail()
        {
            return View();
        }

    }
}
