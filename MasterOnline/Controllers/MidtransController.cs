using MasterOnline.Models;
using MasterOnline.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
            var dtNow = DateTime.Now;
            PaymentMidtransViewModel dataClass = new PaymentMidtransViewModel();
            if (!string.IsNullOrEmpty(code))
            {
                dataClass.price = MoDbContext.Subscription.SingleOrDefault(s => s.KODE == code).HARGA.ToString();
                if (!dataClass.price.Equals("0"))
                {
                    dataClass.typeSubscription = MoDbContext.Subscription.SingleOrDefault(s => s.KODE == code).KETERANGAN.ToString();
                    dataClass.subDesc = "Jumlah marketplace :" + MoDbContext.Subscription.SingleOrDefault(s => s.KODE == code).JUMLAH_MP.ToString() + " \nJumlah pesanan :" + MoDbContext.Subscription.SingleOrDefault(s => s.KODE == code).JUMLAH_PESANAN.ToString();


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
                    data.transaction_details.order_id = dtNow.ToString("yyyyMMddHHmmss");
                    data.credit_card = new CreditCard();
                    data.credit_card.secure = true;
                    data.credit_card.save_card = true;
                    data.customer_details = new CustomerDetail();
                    //data.customer_details.first_name = sessionData.User.Username;
                    //data.user_id = sessionData.User.NoHp;
                    string currentYear = DateTime.Today.ToString("yy");
                    string dataPost = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                    Utils.HttpRequest req = new Utils.HttpRequest();
                    System.Net.Http.HttpContent content = new System.Net.Http.StringContent(dataPost);
                    BindResSnap bindTransferCharge = await req.RequestJSONObject(Utils.HttpRequest.RESTServices.v1, "transactions", content, typeof(BindResSnap), Base64Encode()) as BindResSnap;
                    if (bindTransferCharge != null)
                    {
                        if (!string.IsNullOrEmpty(bindTransferCharge.token))
                        {
                            MoDbContext = new MoDbContext();

                            #region auto number no_transaksi
                            var listTrans = MoDbContext.TransaksiMidtrans.Where(t => t.NO_TRANSAKSI.Substring(0, 2).Equals(currentYear)).OrderBy(t => t.RECNUM).ToList();
                            int lastNum = 0;
                            if (listTrans.Count > 0)
                            {
                                lastNum = listTrans.Last().RECNUM.Value;
                            }
                            lastNum = lastNum + 1;
                            string noTrans = currentYear + lastNum.ToString().PadLeft(10, '0');
                            #endregion
                            var dataTrans = new TransaksiMidtrans();
                            dataTrans.NO_TRANSAKSI = noTrans;
                            dataTrans.TGL_INPUT = dtNow;
                            dataTrans.TYPE = code;
                            dataTrans.VALUE = MoDbContext.Subscription.SingleOrDefault(s => s.KODE == code).HARGA;
                            dataTrans.ACCOUNT_ID = sessionData?.Account != null ? sessionData.Account.AccountId : sessionData.User.AccountId;

                            MoDbContext.TransaksiMidtrans.Add(dataTrans);

                            //var dataSub = new AktivitasSubscription();
                            //dataSub.Account = sessionData?.Account != null ? sessionData.Account.Username : sessionData.User.Username;
                            //dataSub.Email = sessionData?.Account != null ? sessionData.Account.Email : sessionData.User.Email;
                            //dataSub.Nilai = dataTrans.VALUE;
                            //dataSub.TanggalBayar = dtNow;
                            //dataSub.TipeSubs = code;
                            //MoDbContext.AktivitasSubscription.Add(dataSub);

                            //if (sessionData?.Account != null)
                            //{
                            //    var dataAccount = MoDbContext.Account.SingleOrDefault(m => m.AccountId == sessionData.Account.AccountId);
                            //    if (dataAccount != null)
                            //    {
                            //        dataAccount.KODE_SUBSCRIPTION = code;
                            //        dataAccount.TGL_SUBSCRIPTION = dtNow;
                            //    }
                            //}
                            //else
                            //{
                            //    var dataAccount = MoDbContext.Account.SingleOrDefault(m => m.AccountId == sessionData.User.AccountId);
                            //    if (dataAccount != null)
                            //    {
                            //        dataAccount.KODE_SUBSCRIPTION = code;
                            //        dataAccount.TGL_SUBSCRIPTION = dtNow;
                            //    }
                            //}
                            MoDbContext.SaveChanges();
                            return Json(bindTransferCharge.token, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(bindTransferCharge.error_messages, JsonRequestBehavior.AllowGet);

                        }
                    }
                    //return View(data);
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("free account", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("code is empty", JsonRequestBehavior.AllowGet);
            }
        }

        [System.Web.Mvc.Route("midtrans/transaction")]
        public void PostReceive([FromBody]MidtransTransactionData notification_data)
        {
            try
            {
                MoDbContext = new MoDbContext();
                if (notification_data != null)
                {
                    var dataMidtrans = MoDbContext.MidtransData.SingleOrDefault(m => m.TRANSACTION_ID.Equals(notification_data.transaction_id) && m.STATUS_CODE.Equals(notification_data.status_code));
                    if (dataMidtrans == null)
                    {
                        var newData = new MIDTRANS_DATA();
                        newData.BANK = notification_data.bank;
                        newData.GROSS_AMOUNT = notification_data.gross_amount;
                        newData.ORDER_ID = notification_data.order_id;
                        newData.PAYMENT_TYPE = notification_data.payment_type;
                        newData.SIGNATURE_KEY = notification_data.signature_key;
                        newData.STATUS_CODE = notification_data.status_code;
                        newData.TRANSACTION_ID = notification_data.transaction_id;
                        newData.TRANSACTION_STATUS = notification_data.transaction_status;
                        newData.TRANSACTION_TIME = notification_data.transaction_time;

                        MoDbContext.MidtransData.Add(newData);
                    }

                    if (notification_data.status_code.Equals("200") && notification_data.transaction_status.Equals("settlement"))
                    {
                        //transaction complete
                        var tranMidtrans = MoDbContext.TransaksiMidtrans.Where(t => t.NO_TRANSAKSI == notification_data.order_id).SingleOrDefault();
                        if (tranMidtrans != null)
                        {
                            //transaksi sudah ada di tabel transaksi midtrans
                            var insertTrans = new AktivitasSubscription();

                            var userData = MoDbContext.Account.SingleOrDefault(p => p.AccountId == tranMidtrans.ACCOUNT_ID);
                            userData.KODE_SUBSCRIPTION = tranMidtrans.TYPE;
                            userData.TGL_SUBSCRIPTION = Convert.ToDateTime(notification_data.transaction_time);
                            if (!string.IsNullOrEmpty(notification_data.saved_token_id))
                                userData.TOKEN_CC = notification_data.saved_token_id;

                            insertTrans.Account = userData.Username;
                            insertTrans.Email = userData.Email;
                            insertTrans.Nilai = tranMidtrans.VALUE;
                            insertTrans.TanggalBayar = Convert.ToDateTime(notification_data.transaction_time);
                            insertTrans.TipeSubs = tranMidtrans.TYPE;
                            
                            MoDbContext.AktivitasSubscription.Add(insertTrans);
                        }
                    }

                    MoDbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string path = @"C:\MasterOnline\MidtransErrorLog.txt";
                if (!System.IO.File.Exists(path))
                {
                    var createFile = System.IO.File.Create(path);
                    createFile.Close();
                    TextWriter tw = new StreamWriter(path);
                    tw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : " + ex.ToString());
                    tw.Close();
                }
                else if (System.IO.File.Exists(path))
                {
                    TextWriter tw = new StreamWriter(path);
                    //tw.WriteLine("The next line!");
                    tw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : " + ex.ToString());
                    tw.Close();
                }
            }
        }

        public void AutoChargeCC()
        {
            string url = "https://api.sandbox.midtrans.com/v2/charge";
            string serverKey = "";
            System.Net.WebRequest myReq = System.Net.WebRequest.Create(url);
            myReq.ContentType = "application/json";
            myReq.Headers.Add("Accept", "application/json");
            myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(serverKey + ":"))));

            var bindData = new AutoDebetCC();
            bindData.payment_type = "credit_card";
            bindData.credit_card = new AutoCC
            {
                token_id = ""
            };
            bindData.transaction_details = new TransactionDetail
            {
                gross_amount = 20000,
                order_id = "test-auto-1"
            };
            string myData = Newtonsoft.Json.JsonConvert.SerializeObject(bindData);

            Stream dataStream;
            dataStream = myReq.GetRequestStream();
            dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, System.Text.Encoding.UTF8.GetBytes(myData).Length);
            dataStream.Close();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            System.Net.WebResponse response = myReq.GetResponse();
            dataStream = response.GetResponseStream();

            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            reader.Close();
            dataStream.Close();
            response.Close();
        }

        public static string Base64Encode()
        {

            string plainText = "SB-Mid-server-RSxNraBOqtiTba9MSz1SpHx0";
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

    }
}