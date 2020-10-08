using MasterOnline.Models;
using MasterOnline.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
        public async System.Threading.Tasks.Task<ActionResult> PaymentMidtrans(string code, string bulan, string addon, int accId, int? accCount)
        {
            MoDbContext = new MoDbContext("");
            var accInDB = new Account();
            var dtNow = DateTime.Now;
            var retError = new bindMidtrans();
            //PaymentMidtransViewModel dataClass = new PaymentMidtransViewModel();
            if (!string.IsNullOrEmpty(code))
            {
                var price = MoDbContext.Subscription.SingleOrDefault(s => s.KODE == code).HARGA.ToString();
                if (!price.Equals("0"))
                {
                    //dataClass.typeSubscription = MoDbContext.Subscription.SingleOrDefault(s => s.KODE == code).KETERANGAN.ToString();
                    //dataClass.subDesc = "Jumlah marketplace :" + MoDbContext.Subscription.SingleOrDefault(s => s.KODE == code).JUMLAH_MP.ToString() + " \nJumlah pesanan :" + MoDbContext.Subscription.SingleOrDefault(s => s.KODE == code).JUMLAH_PESANAN.ToString();


                    //dataClass.urlView = "http://localhost:50108/midtrans/PaymentMidtrans";
                    string currentYear = DateTime.Today.ToString("yy");

                    #region FITUR ADDON by fauzi
                    var priceAddon = 0;
                    var emailAddon = "";
                    var idAccount = "";
                    if (!string.IsNullOrEmpty(addon))
                    {
                        string[] splitAddon = addon.Split(',');                        
                        foreach(var dataAddon in splitAddon)
                        {
                            int idAddon = Convert.ToInt32(dataAddon);
                            var hargaAddon = MoDbContext.Addons.Where(p => p.RecNum == idAddon).Select(p => p.Harga).SingleOrDefault();
                            priceAddon += hargaAddon;
                        }
                    }
                    #endregion

                    #region auto number no_transaksi
                    var listTrans = MoDbContext.TransaksiMidtrans.Where(t => t.NO_TRANSAKSI.Substring(2, 2).Equals(currentYear)).OrderBy(t => t.RECNUM).ToList();
                    int lastNum = 0;
                    if (listTrans.Count > 0)
                    {
                        lastNum = listTrans.Last().RECNUM.Value;
                    }
                    lastNum = lastNum + 1;
                    //string noTrans = currentYear + lastNum.ToString().PadLeft(10, '0'); // remark add prefix MT for ID auto number Midtrans by fauzi 07-10-2020
                    string noTrans = "MD" + currentYear + lastNum.ToString().PadLeft(8, '0');
                    #endregion

                    int bln = string.IsNullOrEmpty(bulan) ? 3 : Convert.ToInt32(bulan);
                    if (bln == 12)
                        bln = 10;
                    if (bln == 6)
                        bln = 5;

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
                    data.transaction_details.gross_amount = (Convert.ToInt64(price) + Convert.ToInt64(priceAddon)) * bln;
                    //add 3 Maret 2019, handle jumlah user
                    if(code == "03" && accCount > 5)
                    {
                        data.transaction_details.gross_amount = ((Convert.ToInt64(price) + Convert.ToInt64(priceAddon) + 100000 * (accCount - 5)) * bln) ?? 0;
                    }
                    //add change 3 Maret 2019, handle jumlah user
                    data.transaction_details.order_id = noTrans;
                    data.credit_card = new CreditCard();
                    data.credit_card.secure = true;
                    data.credit_card.save_card = true;
                    data.credit_card.save_token_id = true;
                    data.customer_details = new CustomerDetail();

                    if (accId > 0)
                    {
                        accInDB = MoDbContext.Account.Where(a => a.AccountId == accId).FirstOrDefault();
                        if (accInDB != null)
                        {
                            data.customer_details.email = accInDB.Email;
                            data.customer_details.phone = accInDB.NoHp;
                            data.user_id = accId.ToString();
                            emailAddon = accInDB.Email;
                            idAccount = accId.ToString();
                        }
                    }
                    else if (sessionData?.Account != null)
                    {

                        //EDB = new DatabaseSQL(sessionData.Account.UserId);
                        data.customer_details.email = sessionData.Account.Email;
                        data.customer_details.phone = sessionData.Account.NoHp;
                        data.user_id = sessionData.Account.AccountId.ToString();
                        emailAddon = sessionData.Account.Email;
                        idAccount = sessionData.Account.AccountId.ToString();
                    }
                    else
                    {
                        if (sessionData?.User != null)
                        {
                            //var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                            data.customer_details.email = sessionData.User.Email;
                            data.customer_details.phone = sessionData.User.NoHp;
                            data.user_id = sessionData.User.UserId.ToString();
                            emailAddon = sessionData.User.Email;
                            idAccount = sessionData.User.UserId.ToString();
                        }
                    }

                    string dataPost = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                    Utils.HttpRequest req = new Utils.HttpRequest();
                    System.Net.Http.HttpContent content = new System.Net.Http.StringContent(dataPost);
                    BindResSnap bindTransferCharge = await req.RequestJSONObject(Utils.HttpRequest.RESTServices.v1, "transactions", content, typeof(BindResSnap), Base64Encode()) as BindResSnap;
                    if (bindTransferCharge != null)
                    {
                        if (!string.IsNullOrEmpty(bindTransferCharge.token))
                        {
                            MoDbContext = new MoDbContext("");

                            var dataTrans = new TransaksiMidtrans();
                            dataTrans.NO_TRANSAKSI = noTrans;
                            dataTrans.TGL_INPUT = dtNow;
                            dataTrans.TYPE = code;
                            //dataTrans.VALUE = MoDbContext.Subscription.SingleOrDefault(s => s.KODE == code).HARGA; // remark by fauzi for add price addon
                            dataTrans.VALUE = MoDbContext.Subscription.SingleOrDefault(s => s.KODE == code).HARGA + Convert.ToInt64(priceAddon);
                            dataTrans.BULAN = string.IsNullOrEmpty(bulan) ? 0 : Convert.ToInt32(bulan);
                            //add 1 Maret 2019, jumlah user
                            if (code == "03")
                            {
                                dataTrans.jumlahUser = accCount;
                            }
                            else if (code == "02")
                            {
                                dataTrans.jumlahUser = 2;
                            }
                            else
                            {
                                dataTrans.jumlahUser = 0;
                            }
                            //end add 1 Maret 2019, jumlah user
                            //dataTrans.ACCOUNT_ID = sessionData?.Account != null ? sessionData.Account.AccountId : sessionData.User.AccountId;
                            if (accId > 0)
                            {
                                dataTrans.ACCOUNT_ID = accId;
                            }
                            else
                            {
                                dataTrans.ACCOUNT_ID = sessionData?.Account != null ? sessionData.Account.AccountId : sessionData.User.AccountId;
                            }

                            MoDbContext.TransaksiMidtrans.Add(dataTrans);

                            #region save to table ADDON_CUSTOMER for fiture ADDON by fauzi 07/10/2020
                            if (!string.IsNullOrEmpty(addon))
                            {
                                long idAc = Convert.ToInt64(idAccount);
                                var dataAccountInDB = MoDbContext.Account.Where(a => a.AccountId == idAc).FirstOrDefault();
                                string[] splitAddon = addon.Split(',');

                                    DateTime? drTgl = DateTime.Today.AddHours(7);
                                    DateTime? sdTgl = DateTime.Today.AddHours(7);

                                    if (dataAccountInDB.TGL_SUBSCRIPTION > DateTime.Today.AddHours(7))
                                    {
                                        drTgl = dataAccountInDB?.TGL_SUBSCRIPTION;
                                    }
                                    sdTgl = drTgl.Value.AddMonths(bln);

                                foreach (var dataAddon in splitAddon)
                                {
                                    int idAddon = Convert.ToInt32(dataAddon);
                                    var dataDBAddon = MoDbContext.Addons.Where(p => p.RecNum == idAddon).SingleOrDefault();

                                    var dataAddonCheck = MoDbContext.Addons_Customer.Where(p => p.Account == emailAddon && p.ID_ADDON == dataAddon).SingleOrDefault();
                                    if (dataAddonCheck != null)
                                    {
                                        dataAddonCheck.NamaAddons = dataDBAddon.Fitur.ToString();
                                        dataAddonCheck.TGL_DAFTAR = dtNow.AddHours(7);
                                        dataAddonCheck.TglSubscription = sdTgl;
                                        dataAddonCheck.Harga = dataDBAddon.Harga;
                                        dataAddonCheck.ID_TRANS_MIDTRANS = noTrans;
                                    }
                                    else
                                    {
                                        var dataAddCust = new Addons_Customer();
                                        dataAddCust.NamaAddons = dataDBAddon.Fitur.ToString();
                                        dataAddCust.Account = emailAddon.ToString();
                                        dataAddCust.NamaTokoOnline = dataAccountInDB.NamaTokoOnline.ToString();
                                        dataAddCust.TGL_DAFTAR = dtNow.AddHours(7);
                                        dataAddCust.TglSubscription = sdTgl;
                                        dataAddCust.Harga = dataDBAddon.Harga;
                                        dataAddCust.ID_ADDON = dataDBAddon.RecNum.ToString();
                                        dataAddCust.ID_TRANS_MIDTRANS = noTrans;
                                        if(dataDBAddon.RecNum == 2) //82cart FREE
                                        {
                                            dataAddCust.STATUS = "1";
                                        }
                                        else
                                        {
                                            dataAddCust.STATUS = "0";
                                        }
                                        MoDbContext.Addons_Customer.Add(dataAddCust);
                                    }

                                    
                                }
                            }

                            #endregion

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
                            retError.token = bindTransferCharge.token;
                            return Json(retError, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            retError.error = bindTransferCharge.error_messages;
                            return Json(retError, JsonRequestBehavior.AllowGet);

                        }
                    }
                    //return View(data);
                    retError.error = "failed to connect to midtrans";
                    return Json(retError, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    retError.error = "free account";
                    return Json(retError, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                retError.error = "code is empty";
                return Json(retError, JsonRequestBehavior.AllowGet);
            }
        }

        [System.Web.Mvc.Route("midtrans/transaction")]
        public void PostReceive([FromBody]MidtransTransactionData notification_data)
        {
            try
            {
                MoDbContext = new MoDbContext("");
                if (notification_data != null)
                {
                    var dataMidtrans = MoDbContext.MidtransData.SingleOrDefault(m => m.TRANSACTION_ID == notification_data.transaction_id && m.STATUS_CODE == notification_data.status_code);
                    if (dataMidtrans == null)
                    {
                        var newData = new MIDTRANS_DATA();
                        newData.BANK = notification_data.bank;
                        if (string.IsNullOrEmpty(notification_data.bank))
                            if (notification_data.va_numbers != null)
                            {
                                if (notification_data.va_numbers.Length > 0)
                                {
                                    newData.BANK = notification_data.va_numbers[0].bank;
                                }
                            }
                        newData.GROSS_AMOUNT = notification_data.gross_amount;
                        newData.ORDER_ID = notification_data.order_id;
                        newData.PAYMENT_TYPE = notification_data.payment_type;
                        newData.SIGNATURE_KEY = notification_data.signature_key;
                        newData.STATUS_CODE = notification_data.status_code;
                        newData.TRANSACTION_ID = notification_data.transaction_id;
                        newData.TRANSACTION_STATUS = notification_data.transaction_status;
                        newData.TRANSACTION_TIME = notification_data.transaction_time;

                        MoDbContext.MidtransData.Add(newData);

                        if (notification_data.status_code == "200" && (notification_data.transaction_status == "settlement" || notification_data.transaction_status == "capture"))
                        {
                            //transaction complete
                            var tranMidtrans = MoDbContext.TransaksiMidtrans.Where(t => t.NO_TRANSAKSI == notification_data.order_id).SingleOrDefault();
                            if (tranMidtrans != null)
                            {
                                //transaksi sudah ada di tabel transaksi midtrans
                                var insertTrans = new AktivitasSubscription();

                                var userData = MoDbContext.Account.SingleOrDefault(p => p.AccountId == tranMidtrans.ACCOUNT_ID);
                                if (userData != null)
                                {
                                    if (userData.KODE_SUBSCRIPTION == "01" && userData.Status == false)
                                    {
                                        //user baru daftar, langsung subscribe -> activate account
                                        //change 14 may 2019, move function to midtranscontroller
                                        //var accAPI = new AccountController();
                                        //var retActivate = accAPI.ChangeStatusAcc(Convert.ToInt32(userData.AccountId));
                                        var retActivate = ChangeStatusAcc(Convert.ToInt32(userData.AccountId));
                                        //end change 14 may 2019, move function to midtranscontroller
                                        if (retActivate.status == 0)
                                        {
                                            string path = @"C:\logs\MidtransErrorLog.txt";
                                            if (!System.IO.File.Exists(path))
                                            {
                                                var createFile = System.IO.File.Create(path);
                                                createFile.Close();
                                                TextWriter tw = new StreamWriter(path);
                                                tw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : " + retActivate.message);
                                                tw.Close();
                                            }
                                            else if (System.IO.File.Exists(path))
                                            {
                                                TextWriter tw = new StreamWriter(path);
                                                //tw.WriteLine("The next line!");
                                                tw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : " + retActivate.message);
                                                tw.Close();
                                            }
                                        }
                                        else
                                        {
                                            userData.tgl_approve = DateTime.Now;
                                        }

                                    }
                                    //add 18 Feb 2019, penambahan field periode
                                    DateTime? drTgl = DateTime.Today;
                                    DateTime? sdTgl = DateTime.Today;

                                    if (userData.TGL_SUBSCRIPTION > DateTime.Today)
                                    {
                                        drTgl = userData?.TGL_SUBSCRIPTION;
                                    }
                                    sdTgl = drTgl.Value.AddMonths(tranMidtrans.BULAN);
                                    //end add 18 Feb 2019, penambahan field periode

                                    userData.KODE_SUBSCRIPTION = tranMidtrans.TYPE;
                                    //userData.TGL_SUBSCRIPTION = Convert.ToDateTime(notification_data.transaction_time);
                                    //change  18 Feb 2019
                                    //userData.TGL_SUBSCRIPTION = userData.TGL_SUBSCRIPTION.Value.AddMonths(tranMidtrans.BULAN);
                                    userData.TGL_SUBSCRIPTION = sdTgl;
                                    //end change  18 Feb 2019

                                    if (!string.IsNullOrEmpty(notification_data.saved_token_id))
                                        userData.TOKEN_CC = notification_data.saved_token_id;

                                    insertTrans.Account = userData.Username;
                                    if (insertTrans.Account.Length > 20)
                                        insertTrans.Account = insertTrans.Account.Substring(0, 17) + "...";
                                    insertTrans.Email = userData.Email;
                                    //insertTrans.Nilai = tranMidtrans.VALUE * (tranMidtrans.BULAN > 0 ? tranMidtrans.BULAN : 1);
                                    insertTrans.Nilai = Convert.ToDouble(notification_data.gross_amount);
                                    insertTrans.TanggalBayar = Convert.ToDateTime(notification_data.transaction_time);
                                    insertTrans.TipeSubs = tranMidtrans.TYPE;
                                    insertTrans.TipePembayaran = notification_data.payment_type + " " + newData.BANK;
                                    insertTrans.DrTGL = drTgl;
                                    insertTrans.SdTGL = sdTgl;
                                    //add 1 Maret 2019, jumlah user
                                    if (tranMidtrans.TYPE == "03")
                                    {
                                        insertTrans.jumlahUser = tranMidtrans.jumlahUser;
                                    }
                                    else
                                    {
                                        insertTrans.jumlahUser = 2;
                                    }
                                    userData.jumlahUser = insertTrans.jumlahUser;
                                    //end add 1 Maret 2019, jumlah user
                                    MoDbContext.AktivitasSubscription.Add(insertTrans);

                                    #region Save Active for Addon Fiture
                                    var dataAddonCheck = MoDbContext.Addons_Customer.Where(p => p.Account == userData.Email && p.ID_TRANS_MIDTRANS == tranMidtrans.NO_TRANSAKSI).ToList();
                                    if(dataAddonCheck != null)
                                    {
                                        dataAddonCheck.ForEach(p => p.STATUS = "1");
                                    }
                                    #endregion
                                }

                            }
                        }

                        MoDbContext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                string path = @"C:\logs\MidtransErrorLog.txt";
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
        //ADD 14 MAY 2019, ACTIVATE ACCOUNT
        public BindingBase ChangeStatusAcc(int? accId)
        {
            var ret = new BindingBase
            {
                status = 0
            };
            try
            {
                var accInDb = MoDbContext.Account.Single(a => a.AccountId == accId);
                accInDb.Status = !accInDb.Status;
                string sql = "";
                var userId = Convert.ToString(accInDb.AccountId);

                accInDb.DatabasePathErasoft = "ERASOFT_" + userId;
                //var path = "C:\\inetpub\\wwwroot\\MasterOnline\\Content\\admin\\";
                var path = Server.MapPath("~/Content/admin/");
                //var path = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/admin/");

                sql = $"RESTORE DATABASE {accInDb.DatabasePathErasoft} FROM DISK = '{path + "ERASOFT_backup_for_new_account.bak"}'" +
                      $" WITH MOVE 'erasoft' TO '{path}/{accInDb.DatabasePathErasoft}.mdf'," +
                      $" MOVE 'erasoft_log' TO '{path}/{accInDb.DatabasePathErasoft}.ldf';";
#if AWS
            SqlConnection con = new SqlConnection("Server=localhost;Initial Catalog=master;persist security info=True;" +
                                "user id=masteronline;password=M@ster123;");
#elif Debug_AWS
                SqlConnection con = new SqlConnection("Server=13.250.232.74\\SQLEXPRESS,1433;Initial Catalog=master;persist security info=True;" +
                                                      "user id=masteronline;password=M@ster123;");
#else
            SqlConnection con = new SqlConnection("Server=13.251.222.53\\SQLEXPRESS,1433;Initial Catalog=master;persist security info=True;" +
                                                  "user id=masteronline;password=M@ster123;");
#endif
                SqlCommand command = new SqlCommand(sql, con);

                con.Open();
                command.ExecuteNonQuery();
                con.Close();
                con.Dispose();


                //add by Tri 20-09-2018, save nama toko ke SIFSYS
                //change by calvin 3 oktober 2018
                //ErasoftContext ErasoftDbContext = new ErasoftContext(userId);
                ErasoftContext ErasoftDbContext = new ErasoftContext(accInDb.DataSourcePath, accInDb.DatabasePathErasoft);
                //end change by calvin 3 oktober 2018
                var dataPerusahaan = ErasoftDbContext.SIFSYS.FirstOrDefault();
                if (string.IsNullOrEmpty(dataPerusahaan.NAMA_PT))
                {
                    dataPerusahaan.NAMA_PT = accInDb.NamaTokoOnline;
                    ErasoftDbContext.SaveChanges();
                }
                //end add by Tri 20-09-2018, save nama toko ke SIFSYS

                if (accInDb.Status == false)
                {
                    var listUserPerAcc = MoDbContext.User.Where(u => u.AccountId == accId).ToList();
                    foreach (var user in listUserPerAcc)
                    {
                        user.Status = false;
                    }
                }

                //ViewData["SuccessMessage"] = $"Akun {accInDb.Username} berhasil diubah statusnya dan dibuatkan database baru.";
                MoDbContext.SaveChanges();

                //var listAcc = MoDbContext.Account.ToList();
                sendEmail(accInDb.Email, accInDb.Password, accInDb.Username);
                //return View("AccountMenu", listAcc);
                ret.status = 1;
            }
            catch (Exception ex)
            {
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }

            return ret;
        }

        public async void sendEmail(string emailUser, string pass, string user)
        {
            var email = new MailAddress(emailUser);
            var originPassword = pass ;
            var nama = user;
            var body = "<p><img src=\"https://s3-ap-southeast-1.amazonaws.com//masteronlinebucket/uploaded-image/efd0f5b3-7862-4ee6-b796-6c5fc9c63d5f.jpeg\"  width=\"250\" height=\"100\"></p>" +
                "<p>Hi {2},</p>" +
                "<p>Selamat akun anda telah berhasil kami daftarkan.</p>" +
                "<p>Login sekarang &nbsp;<b><a class=\"user-link\" href=\"https://masteronline.co.id/login\">Di Sini</a></b> dan kembangkan bisnis online anda bersama Master Online.</p>" +
                "<p>Email akun anda ialah sebagai berikut :</p>" +
                "<p>Email: {0}</p>" +
                "<p>Fitur utama kami:</p>" +
                "<p>1. Kelola pesanan di semua marketplace secara realtime di Master Online.</p>" +
                "<p>2. Upload dan kelola inventory di semua marketplace real time.</p>" +
                "<p>3. Analisa penjualan di semua marketplace.</p>" +
                "<p>Nantikan perkembangan fitur - fitur kami berikut nya &nbsp;<img src=\"https://html-online.com/editor/tinymce4_6_5/plugins/emoticons/img/smiley-laughing.gif\" alt=\"laughing\" /></p>" +
                "<p>Untuk informasi lebih detail dapat menghubungi customer service kami melalui telp +6221 6349318 atau email support@masteronline.co.id atau chat melalui website kami www.masteronline.co.id.</p>" +
                "<p>Semoga sukses selalu dalam bisnis anda bersama Master Online.</p>" +
                "<p>&nbsp;</p>" +
                "<p>Best regards,</p>" +
                "<p>CS Master Online.</p>";

            var message = new MailMessage();
            message.To.Add(email);
            message.From = new MailAddress("csmasteronline@gmail.com");
            message.Subject = "Akun Master Online Anda sudah aktif!";
            message.Body = string.Format(body, email, originPassword, nama);
            message.IsBodyHtml = true;
#if AWS
            //using (var smtp = new SmtpClient())
            //{
            //    var credential = new NetworkCredential
            //    {
            //        UserName = "AKIAIXN2D33JPSDL7WEQ",
            //        Password = "ApBddkFZF8hwJtbo+s4Oq31MqDtWOpzYKDhyVGSHGCEl"
            //    };
            //    smtp.Credentials = credential;
            //    smtp.Host = "email-smtp.us-east-1.amazonaws.com";
            //    smtp.Port = 587;
            //    smtp.EnableSsl = true;
            //    await smtp.SendMailAsync(message);
            //}
            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = "csmasteronline@gmail.com",
                    Password = "kmblwexkeretrwxv"
                };
                smtp.Credentials = credential;
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(message);
            }
#else
            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = "csmasteronline@gmail.com",
                    Password = "kmblwexkeretrwxv"
                };
                smtp.Credentials = credential;
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(message);
            }
#endif
        }
        //END ADD 14 MAY 2019, ACTIVATE ACCOUNT

        //public void AutoChargeCC()
        //{
        //    string url = "https://api.sandbox.midtrans.com/v2/charge";
        //    string serverKey = "";
        //    System.Net.WebRequest myReq = System.Net.WebRequest.Create(url);
        //    myReq.ContentType = "application/json";
        //    myReq.Headers.Add("Accept", "application/json");
        //    myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(serverKey + ":"))));

        //    var bindData = new AutoDebetCC();
        //    bindData.payment_type = "credit_card";
        //    bindData.credit_card = new AutoCC
        //    {
        //        token_id = ""
        //    };
        //    bindData.transaction_details = new TransactionDetail
        //    {
        //        gross_amount = 20000,
        //        order_id = "test-auto-1"
        //    };
        //    string myData = Newtonsoft.Json.JsonConvert.SerializeObject(bindData);

        //    Stream dataStream;
        //    dataStream = myReq.GetRequestStream();
        //    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, System.Text.Encoding.UTF8.GetBytes(myData).Length);
        //    dataStream.Close();

        //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

        //    System.Net.WebResponse response = myReq.GetResponse();
        //    dataStream = response.GetResponseStream();

        //    StreamReader reader = new StreamReader(dataStream);
        //    string responseFromServer = reader.ReadToEnd();

        //    reader.Close();
        //    dataStream.Close();
        //    response.Close();
        //}

        public static string Base64Encode()
        {
            //production : Mid-client-sMzViq24qWRlPdPu Mid-server-brKgVeWZt89aotXTI8DDPkfY
            //sandbox : SB-Mid-client-AyzcvZKcwAlD_0QY SB-Mid-server-GAojYLM-zNP6Ik_HzyqBzaGb

            string plainText = "Mid-server-brKgVeWZt89aotXTI8DDPkfY";//SB-Mid-server-RSxNraBOqtiTba9MSz1SpHx0 Mid-server-OB_-aJie9ELUo3pDnZSj0vYq
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);

            //string plainText = "SB-Mid-server-GAojYLM-zNP6Ik_HzyqBzaGb";//SB-Mid-server-RSxNraBOqtiTba9MSz1SpHx0 Mid-server-OB_-aJie9ELUo3pDnZSj0vYq
            //var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);

            return Convert.ToBase64String(plainTextBytes);
        }

    }

    public class bindMidtrans
    {
        public string token { get; set; }
        public string error { get; set; }
    }
}