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
using System.Collections.Specialized;
using Microsoft.AspNetCore.Http;
using System.Web;
using Azure;

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


        public ActionResult PayVNPay()
        {
            List<CartItem> cart = HttpContext.Session.Get<List<CartItem>>("Cart");

            if (cart == null)
            {
                ViewBag.Message = "Cart empty";
                return RedirectToAction("Fail", "Order");
            }

            else
            {
                HttpClient client = new HttpClient();

                //string endpoint = Configuration["Momo:endpoint"];
                //string partnerCode = Configuration["Momo:partnerCode"];
                //string accessKey = Configuration["Momo:accessKey"];
                //string secretKey = Configuration["Momo:secretKey"];
                //string requestId = "";

                // order information
                string orderId = Guid.NewGuid().ToString();
                string amount = (cart.Sum(c => c.price * c.quantity) * ((int)Unit.VND)).ToString() ?? "0";


                // rawHash for signature
                string url = Configuration["VNPay:Url"];
                string returnUrl = Configuration["VNPay:ReturnUrl"];
                string tmnCode = Configuration["VNPay:TmnCode"];
                string hashSecret = Configuration["VNPay:HashSecret"];

                PayLib pay = new PayLib();

                pay.AddRequestData("vnp_Version", "2.1.0"); //Phiên bản api mà merchant kết nối. Phiên bản hiện tại là 2.1.0
                pay.AddRequestData("vnp_Command", "pay"); //Mã API sử dụng, mã cho giao dịch thanh toán là 'pay'
                pay.AddRequestData("vnp_TmnCode", tmnCode); //Mã website của merchant trên hệ thống của VNPAY (khi đăng ký tài khoản sẽ có trong mail VNPAY gửi về)
                pay.AddRequestData("vnp_Amount", amount); //số tiền cần thanh toán, công thức: số tiền * 100 - ví dụ 10.000 (mười nghìn đồng) --> 1000000
                pay.AddRequestData("vnp_BankCode", ""); //Mã Ngân hàng thanh toán (tham khảo: https://sandbox.vnpayment.vn/apis/danh-sach-ngan-hang/), có thể để trống, người dùng có thể chọn trên cổng thanh toán VNPAY
                pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")); //ngày thanh toán theo định dạng yyyyMMddHHmmss
                pay.AddRequestData("vnp_CurrCode", "VND"); //Đơn vị tiền tệ sử dụng thanh toán. Hiện tại chỉ hỗ trợ VND
                pay.AddRequestData("vnp_IpAddr", Util.GetIpAddress()); //Địa chỉ IP của khách hàng thực hiện giao dịch
                pay.AddRequestData("vnp_Locale", "en"); //Ngôn ngữ giao diện hiển thị - Tiếng Việt (vn), Tiếng Anh (en)
                pay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang"); //Thông tin mô tả nội dung thanh toán
                pay.AddRequestData("vnp_OrderType", "other"); //topup: Nạp tiền điện thoại - billpayment: Thanh toán hóa đơn - fashion: Thời trang - other: Thanh toán trực tuyến
                pay.AddRequestData("vnp_ReturnUrl", returnUrl); //URL thông báo kết quả giao dịch khi Khách hàng kết thúc thanh toán
                pay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString()); //mã hóa đơn

                string paymentUrl = pay.CreateRequestUrl(url, hashSecret);

                return Redirect(paymentUrl);
            }
        }
        public ActionResult PaymentConfirm()
        {
            if (Request.QueryString.HasValue == true)
            {
                string hashSecret = Configuration["VNPay:HashSecret"]; //Chuỗi bí mật
                var vnpayData1 = Request.QueryString;
                PayLib pay = new PayLib();
                NameValueCollection vnpayData = HttpUtility.ParseQueryString(vnpayData1.ToString());
                //lấy toàn bộ dữ liệu được trả về
                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        pay.AddResponseData(s, vnpayData[s]);
                    }
                }

                long orderId = Convert.ToInt64(pay.GetResponseData("vnp_TxnRef")); //mã hóa đơn
                long vnpayTranId = Convert.ToInt64(pay.GetResponseData("vnp_TransactionNo")); //mã giao dịch tại hệ thống VNPAY
                string vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode"); //response code: 00 - thành công, khác 00 - xem thêm https://sandbox.vnpayment.vn/apis/docs/bang-ma-loi/
                //string vnp_SecureHash = Request.QueryString["vnp_SecureHash"]; //hash của dữ liệu trả về
                string vnp_SecureHash = Request.Query["vnp_SecureHash"];

                bool checkSignature = pay.ValidateSignature(vnp_SecureHash, hashSecret); //check chữ ký đúng hay không?

                string amount = Convert.ToString(pay.GetResponseData("vnp_Amount"));

                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00")
                    {
                        //Thanh toán thành công
                        ViewBag.Message = "Thanh toán thành công hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId;
                        return RedirectToAction("Success", "Order"); ;
                    }
                    else
                    {
                        //Thanh toán không thành công. Mã lỗi: vnp_ResponseCode
                        ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId + " | Mã lỗi: " + vnp_ResponseCode;
                        return RedirectToAction("Fail", "Order");
                    }
                }
                else
                {

                    return RedirectToAction("Fail", "Order");
                }
            }
            else
            {
                return RedirectToAction("Fail", "Order");
            }

            return View();
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
