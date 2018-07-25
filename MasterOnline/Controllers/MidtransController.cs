using MasterOnline.Models;
using MasterOnline.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace MasterOnline.Controllers
{
    public class MidtransController : Controller
    {
        AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        public MoDbContext MoDbContext { get; set; }
        // GET: Midtrans
        [System.Web.Mvc.HttpGet]
        public async System.Threading.Tasks.Task<ActionResult> PaymentMidtrans(string code)
        {
            MoDbContext = new MoDbContext();
            PaymentMidtransViewModel dataClass = new PaymentMidtransViewModel();
            if (!string.IsNullOrEmpty(code))
            {
                dataClass.price = MoDbContext.Subscription.SingleOrDefault(s => s.KODE == code).HARGA.ToString();
                dataClass.typeSubscription = MoDbContext.Subscription.SingleOrDefault(s => s.KODE == code).KETERANGAN.ToString();
                dataClass.subDesc = "Jumlah marketplace :" + MoDbContext.Subscription.SingleOrDefault(s => s.KODE == code).JUMLAH_MP.ToString() + " \nJumlah pesanan :" + MoDbContext.Subscription.SingleOrDefault(s => s.KODE == code).JUMLAH_PESANAN.ToString();
            }

            dataClass.urlView = "http://localhost:50108/midtrans/PaymentMidtrans";
            BindReqSnap data = new BindReqSnap
            {
                //transaction_details = new TransactionDetail(),
                //{
                //    gross_amount = Convert.ToInt64(dataClass.price),
                //    order_id = DateTime.Now.ToString("yyyyMMddHHmmss"),
                //},
                //credit_card = new CreditCard(),
                //{
                //    secure = true,
                //    save_card = true,
                //},
                //customer_details = new CustomerDetail(),
                //{
                //    first_name = sessionData.User.Username,
                //    //email = email,
                //    //phone = hp,
                //},
                //user_id = sessionData.User.NoHp,
            };
            data.transaction_details = new TransactionDetail();
            data.transaction_details.gross_amount = Convert.ToInt64(dataClass.price);
            data.transaction_details.order_id = DateTime.Now.ToString("yyyyMMddHHmmss");
            data.credit_card = new CreditCard();
            data.credit_card.secure = true;
            data.credit_card.save_card = true;
            data.customer_details = new CustomerDetail();
            //data.customer_details.first_name = sessionData.User.Username;
            //data.user_id = sessionData.User.NoHp;

            string dataPost = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            Utils.HttpRequest req = new Utils.HttpRequest();
            System.Net.Http.HttpContent content = new System.Net.Http.StringContent(dataPost);
            BindResSnap bindTransferCharge = await req.RequestJSONObject(Utils.HttpRequest.RESTServices.v1, "transactions", content, typeof(BindResSnap), Base64Encode()) as BindResSnap;
            if (bindTransferCharge != null)
            {
                if (!string.IsNullOrEmpty(bindTransferCharge.token))
                {
                    return Json(bindTransferCharge.token, JsonRequestBehavior.AllowGet);
                }
                else {
                    return Json(bindTransferCharge.error_messages, JsonRequestBehavior.AllowGet);

                }
            }
            //return View(data);
            return Json(data, JsonRequestBehavior.AllowGet);

        }

        //[System.Web.Mvc.Route("midtrans/transaction")]
        //public void PostReceive([FromBody]MidtransTransactionData notification_data)
        //{
        //    MoDbContext = new MoDbContext();
        //    if (notification_data != null) {
        //        if( notification_data.order_id)
        //    }
        //}
        public static string Base64Encode()
        {

            string plainText = "SB-Mid-server-RSxNraBOqtiTba9MSz1SpHx0";
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}