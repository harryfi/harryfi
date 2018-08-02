using MasterOnline.Models;
using MasterOnline.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
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
                    data.transaction_details.order_id = DateTime.Now.ToString("yyyyMMddHHmmss");
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
                            dataTrans.TGL_INPUT = DateTime.Now;
                            dataTrans.TYPE = code;
                            dataTrans.VALUE = MoDbContext.Subscription.SingleOrDefault(s => s.KODE == code).HARGA;
                            dataTrans.ACCOUNT_ID = sessionData?.Account != null ? sessionData.Account.AccountId : sessionData.User.AccountId;

                            MoDbContext.TransaksiMidtrans.Add(dataTrans);
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
                        dataMidtrans = new MIDTRANS_DATA();
                        dataMidtrans.BANK = notification_data.bank;
                        dataMidtrans.GROSS_AMOUNT = notification_data.gross_amount;
                        dataMidtrans.ORDER_ID = notification_data.order_id;
                        dataMidtrans.PAYMENT_TYPE = notification_data.payment_type;
                        dataMidtrans.SIGNATURE_KEY = notification_data.signature_key;
                        dataMidtrans.STATUS_CODE = notification_data.status_code;
                        dataMidtrans.TRANSACTION_ID = notification_data.transaction_id;
                        dataMidtrans.TRANSACTION_STATUS = notification_data.transaction_status;
                        dataMidtrans.TRANSACTION_TIME = notification_data.transaction_time;

                        MoDbContext.MidtransData.Add(dataMidtrans);
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
                            insertTrans.Account = userData.Username;
                            insertTrans.Email = userData.Email;
                            insertTrans.Nilai = tranMidtrans.VALUE;
                            insertTrans.TanggalBayar = tranMidtrans.TGL_INPUT;
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
                    tw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +" : " + ex.ToString());
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
        public static string Base64Encode()
        {

            string plainText = "SB-Mid-server-RSxNraBOqtiTba9MSz1SpHx0";
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}