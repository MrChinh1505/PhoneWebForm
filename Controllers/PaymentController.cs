using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using MobilePhone.Helpers;
using MobilePhone.Models;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Text.Json.Nodes;
using System.Security.Cryptography;
using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MobilePhone.Controllers
{
    public enum Unit
    {
        VND = 23000
    }

    public class PaymentController : Controller
    {
        private readonly IConfiguration Configuration;

        public PaymentController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private string Signature(string secretKey,string raw)
        {

            var h256 = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));

            return h256.ComputeHash(Encoding.UTF8.GetBytes(raw)).ToString();

        }


        [HttpGet]
        public IActionResult PayCOD()
        {
            List<CartItem> carts = HttpContext.Session.Get<List<CartItem>>("Cart");
            
            Agent a = new Agent
            {
                id = HttpContext.Session.GetString("username"),
                name = HttpContext.Session.GetString("name"),
                address = HttpContext.Session.GetString("address"),
                contact = HttpContext.Session.GetString("contact"),
            };
            
            if (carts != null)
            {
                ViewBag.OrderID = DateTime.Now.ToString("yyMMddHHss");
                ViewBag.AgentID = a.id;
                ViewBag.Name = a.name;
                ViewBag.Address = a.address;
                ViewBag.Contact = a.contact;
                ViewBag.TotalAmount = (carts.Sum(c => c.price * c.quantity)).ToString();
            }
            else
            {
                return RedirectToAction("Index","Cart");
            }
            return View(carts);
        }
        [HttpGet]
        public async Task<IActionResult> PayMomo()
        {
            List<CartItem> cart = HttpContext.Session.Get<List<CartItem>>("Cart");

            if (cart == null)
            {
                ViewBag.Message = "Cart empty";
                return RedirectToAction("Fail","Order");
            }

            else
            {
                HttpClient client = new HttpClient();

                string endpoint = Configuration["Momo:endpoint"];
                string partnerCode = Configuration["Momo:partnerCode"];
                string accessKey = Configuration["Momo:accessKey"];
                string secretKey = Configuration["Momo:secretKey"];
                string requestId = "";

                // order information
                string orderId = Guid.NewGuid().ToString();
                string amount = (cart.Sum(c => c.price * c.quantity)*((int)Unit.VND)).ToString() ?? "0";
                string returnUrl = "localhost:/Payemnt/ReturnUrl";
                string notifyUrl = "localhost:/Payemnt/NotifyUrl";
                string extraData = "";
                string orderInfo = "Thanh toan Momo";

                // rawHash for signature
                string raw = "partnerCode=" + partnerCode +
                                 "&accessKey=" + accessKey + 
                                 "&requestId=" + requestId + 
                                 "&amount=" + amount + 
                                 "&orderId=" +  orderId + 
                                 "&orderInfo=" + orderInfo + 
                                 "&returnUrl=" + returnUrl + 
                                 "&notifyUrl=" + notifyUrl + 
                                 "&extraData=" + extraData;


                // hash raw with rsa and private keys
                string signature = this.Signature(secretKey, raw);
                
                // string signature = new HMACSHA256().ComputeHash(Encoding.UTF8.GetBytes(raw)).ToString(); 

                var content = new Dictionary<string, string>
                {
                    {"partnerCode", partnerCode },
                    {"accessKey", accessKey },
                    {"requestId",requestId },
                    {"amount", amount },
                    {"orderId", orderId },
                    {"orderInfo", orderInfo},
                    {"returnUrl", returnUrl  },
                    {"notifyUrl", notifyUrl },
                    { "extraData", ""},
                    {"requestType", "captureMoMoWallet" },
                    {"signature", signature },
                };

                // send request then receive a response
                /*
                    example:

                    response {
                      "requestId": "MM1540456472575",
                      "errorCode": 0,
                      "orderId": "MM1540456472575",
                      "message": "Success",
                      "localMessage": "Thành công",
                      "requestType": "captureMoMoWallet",
                      "payUrl": "https://test-payment.momo.vn/gw_payment/payment/qr?partnerCode=MOMO&accessKey=F8BBA842ECF85&requestId=MM1540456472575&amount=150000&orderId=MM1540456472575&signature=df2a347519abb91e9c1bd1bee80e675f4108cb6dbcac531979e805857293d486&requestType=captureMoMoWallet",
                      "signature": "ee6a01b85ffc48a2b5d3df473da88c75cc5e879d1543d9e76ced279c10bcd646",
                      "qrCodeUrl": "https://test-payment.momo.vn/gw_payment/s/zoVKZd",
                      "deeplink": "momo://?action=payWithAppToken&amount=150000&fee=0&requestType=payment&orderLabel=M%C3%A3+%C4%91%C6%A1n+h%C3%A0ng&orderId=MM1540456472575&requestId=MM1540456472575&merchantnamelabel=Nh%C3%A0+cung+c%E1%BA%A5p&description=SDK+team.&partnerCode=MOMO&merchantcode=MOMO&language=vi&merchantname=MoMo+Payment&packageId=&extras=&extraData=email=abc@gmail.com&deeplinkCallback=https%3A%2F%2Ftest-payment.momo.vn%2Fgw_payment%2Fm2%3Fid%3DM7EWVy&callbackUrl=https%3A%2F%2Ftest-payment.momo.vn%2Fgw_payment%2Fm2%3Fid%3DM7EWVy&urlSubmitToken=https%3A%2F%2Ftest-payment.momo.vn%2Fgw_payment%2Fpayment_with_app%3FpartnerCode%3DMOMO%26accessKey%3DF8BBA842ECF85%26requestId%3DMM1540456472575%26orderId%3DMM1540456472575%26orderInfo%3DSDK%2Bteam.%26amount%3D150000%26signature%3Ddf2a347519abb91e9c1bd1bee80e675f4108cb6dbcac531979e805857293d486%26requestType%3DcaptureMoMoWallet%26payType%3Dapp-in-app&appScheme=",
                      "deeplinkWebInApp": "http://momo//?type=webinapp&action=payment&requestId=MM1540456472575&billId=MM1540456472575&partnerCode=MOMO&partnerName=MoMo Payment&amount=150000&description=SDK team.¬ifyUrl=https://momo.vn&returnUrl=https://momo.vn&code=momo&extraData=eyJzaWduYXR1cmUiOiI0OWUzMTZhNTVkN2UxM2Q0ZjEwNGFjZjM2YTM5MzllZjg0NDk3NWU2OTJiMWU1OGM3MDFjYWUyM2ZiM2QxNDY5In0=&signature=49e316a55d7e13d4f104acf36a3939ef844975e692b1e58c701cae23fb3d1469"
                    }
                 */

                var request = new FormUrlEncodedContent(content);

                var response = await client.PostAsync(endpoint, request);

                // var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    // If the request is successful, return the generated QR Code
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // if response content return null it will be redirected to fail order view
                    if (!string.IsNullOrEmpty(responseContent))
                    {
                        return RedirectToAction("Fail", "Order");

                    }

                    var responseJson = JObject.Parse(responseContent);
                    var qrCode = responseJson["payUrl"].ToString();
                    
                    return Redirect(qrCode); 
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    
                    return RedirectToAction("Fail", "Order");
                }
            }
        }
        
        // remember that you will be verify with private key when processing status 

        public ActionResult ReturnUrl()
        {
            // process if success 

            // process if fail
            
            return View();
        }

        public ActionResult NotifyUrl()
        {
            // update in database, this action will be run in the background
            // process if success
            // process if fail
            return View();
        }


        [HttpGet]
        public IActionResult Index()
        {
            ViewData["name"] = HttpContext.Session.GetString("username");
            return View();
        }
    }
}
