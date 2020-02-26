using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Mvc;
using MasterOnline.Models;
using MasterOnline.Services;
using MasterOnline.ViewModels;

using Hangfire;
using Hangfire.Server;
using Hangfire.Storage;
using Hangfire.SqlServer;
using Erasoft.Function;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using PagedList;

using System.Globalization;

namespace MasterOnline.Controllers
{
    public class AdminController : Controller
    {
        private readonly MoDbContext MoDbContext;

        public AdminController()
        {
            MoDbContext = new MoDbContext("");
        }

        protected override void Dispose(bool disposing)
        {
            MoDbContext.Dispose();
        }

        // =============================================== Bagian Login / Logout (START)

        // Halaman login
        [Route("admin/login")]
        public ActionResult Login()
        {
            return View();
        }


        //Proses login admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LoggingIn(Admin admin)
        {
            ModelState.Remove("AdminId");
            ModelState.Remove("Username");

            if (!ModelState.IsValid)
            {
                return View("Login", admin);
            }

            var adminFromDb = MoDbContext.Admins.SingleOrDefault(a => a.Email == admin.Email);

            if (adminFromDb == null)
            {
                ModelState.AddModelError("", @"Admin tidak ditemukan!");
                return View("Login", admin);
            }

            //if (admin.Password != adminFromDb.Password)
            //{
            //    ModelState.AddModelError("", @"Password salah!");
            //    return View("Login", admin);
            //}

            //Session["SessionAdmin"] = adminFromDb;

            ////return RedirectToAction("AccountMenu");
            //return RedirectToAction("DashboardAdmin");
            var result = "";
            if (adminFromDb.Email == "admin@masteronline.co.id")
            {
                if (admin.Password != adminFromDb.Password)
                {
                    ModelState.AddModelError("", @"Password salah!");
                    return View("Login", admin);
                }

                Session["SessionAdmin"] = adminFromDb;

                result = "DashboardAdmin";
            }
            else if (adminFromDb.Email == "csmasteronline@gmail.com")
            {
                if (admin.Password != adminFromDb.Password)
                {
                    ModelState.AddModelError("", @"Password salah!");
                    return View("Login", admin);
                }

                Session["SessionAdmin"] = adminFromDb;

                result = "DashboardAdm";

            }
            return RedirectToAction(result);

        }

        public ActionResult LoggingOut()
        {
            Session["SessionAdmin"] = null;
            return RedirectToAction("Login", "Admin");
        }

        // =============================================== Bagian Login / Logout (END)

        // =============================================== Bagian User & Account (START)

        [Route("admin/account/detail/{accId}")]
        public ActionResult AccountDetail(int? accId)
        {
            var accInDb = MoDbContext.Account.SingleOrDefault(a => a.AccountId == accId);

            if (accInDb == null)
                return View("Error");

            //add by nurul 20/2/2019
            var vm = new MenuAccount()
            {
                Account = accInDb
            };
            //end add by nurul 20/2/2019

            //return View(accInDb);
            return PartialView("AccountDetail", vm);
        }

        // Mengubah status user
        public ActionResult ChangeStatusUser(int? userId)
        {
            var userInDb = MoDbContext.User.Single(u => u.UserId == userId);
            userInDb.Status = !userInDb.Status;

            MoDbContext.SaveChanges();

            ViewData["SuccessMessage"] = $"User {userInDb.Username} berhasil diubah statusnya.";

            var vm = new UserViewModel()
            {
                ListUser = MoDbContext.User.ToList(),
                ListAccount = MoDbContext.Account.ToList()
            };

            return View("UserMenu", vm);
        }

        // Mengubah status akun utama
        //public ActionResult ChangeStatusAcc(int? accId)
        //[HttpGet]
        //public async Task<ActionResult> ChangeStatusAcc(int? accId, string stat)
        public async Task<ActionResult> ChangeStatusAcc(MenuAccount dataVm)
        {
            bool statusAcc = dataVm.Account.Status;
            var accId = dataVm.Account.AccountId;
            var accInDb = MoDbContext.Account.Single(a => a.AccountId == accId);
            if (accInDb.Status == statusAcc)
            {
                var Tempvm = new MenuAccount()
                {
                    Account = MoDbContext.Account.SingleOrDefault(a => a.AccountId == accId),
                    ListPartner = MoDbContext.Partner.ToList()
                };

                return PartialView("FormAccountPartialNew", Tempvm);

            }
            accInDb.Status = !accInDb.Status;


            if (accInDb.Status == true && accInDb.DatabasePathErasoft == null || accInDb.Status == true && accInDb.DatabasePathErasoft == "")
            {
                string sql = "";
                var userId = Convert.ToString(accInDb.AccountId);
                //var tujuan = "54.179.169.195\\SQLEXPRESS";
                var tujuan = "13.250.232.74\\SQLEXPRESS, 1433";
                //var tujuan = "13.251.222.53\\SQLEXPRESS, 1433";

                accInDb.DatabasePathErasoft = "ERASOFT_" + userId;
                accInDb.DataSourcePath = tujuan;

                //var path = Server.MapPath("~/Content/admin/");
                var path = "C:\\BAK_new_user\\";
                var pathRestore = "C:\\DB_user";
                sql = $"RESTORE DATABASE {accInDb.DatabasePathErasoft} FROM DISK = '{path + "ERASOFT_backup_for_new_account.bak"}'" +
                      $" WITH MOVE 'erasoft' TO '{pathRestore}\\{accInDb.DatabasePathErasoft}.mdf'," +
                      $" MOVE 'erasoft_log' TO '{pathRestore}\\{accInDb.DatabasePathErasoft}.ldf';";

                SqlConnection con = new SqlConnection("Server="+ tujuan +";Initial Catalog=master;persist security info=True;" +
                                                      "user id=sa;password=admin123^;");
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

                //add by Tri, set free trials 14 hari
                if (accInDb.Status)
                {
                    if (accInDb.KODE_SUBSCRIPTION == "01")
                    {
                        accInDb.TGL_SUBSCRIPTION = DateTime.Today.AddDays(14);
                    }
                    //add by nurul 12/3/2019
                    accInDb.tgl_approve = DateTime.Today;
                    //end add 
                }
                //end add by Tri, set free trials 14 hari

                ViewData["SuccessMessage"] = $"Akun {accInDb.Username} berhasil diubah statusnya dan dibuatkan database baru.";
                MoDbContext.SaveChanges();

                //add by nurul 5/3/2019
                var email = new MailAddress(accInDb.Email);
                var originPassword = accInDb.Password;
                var nama = accInDb.Username;
                //var body = "<p><img src=\"https://s3-ap-southeast-1.amazonaws.com//masteronlinebucket/uploaded-image/ee23b210-cb3b-4796-9ad1-9ddf936a8e26.jpg\"  width=\"200\" height=\"150\"></p>" +
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
                //end change by nurul 5/3/2019

                var message = new MailMessage();
                message.To.Add(email);
                message.From = new MailAddress("csmasteronline@gmail.com");
                message.Subject = "Akun Master Online Anda sudah aktif!";
                message.Body = string.Format(body, accInDb.Email, originPassword, nama);
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
            else
            {
                if (accInDb.Status == false)
                {
                    var listUserPerAcc = MoDbContext.User.Where(u => u.AccountId == accId).ToList();
                    foreach (var user in listUserPerAcc)
                    {
                        user.Status = false;
                    }
                }
                ViewData["SuccessMessage"] = $"Akun {accInDb.Username} berhasil dinonaktifkan.";
                MoDbContext.SaveChanges();

            }
            //end add by nurul 5/3/2019

            //change by nurul 5/3/2019
            //var listAcc = MoDbContext.Account.ToList();

            //return View("AccountMenu", listAcc);

            //var vm = new MenuAccount()
            //{
            //    ListAccount = MoDbContext.Account.ToList(),
            //    ListPartner = MoDbContext.Partner.ToList()
            //};
            var vm = new MenuAccount()
            {
                Account = MoDbContext.Account.SingleOrDefault(a => a.AccountId == accId),
                ListPartner = MoDbContext.Partner.ToList()
            };

            return PartialView("FormAccountPartialNew", vm);
            //end change by nurul 5/3/2019
        }

        public ActionResult TambahHapusDatabaseAcc(int? accId)
        {
            var accInDb = MoDbContext.Account.FirstOrDefault(a => a.AccountId == accId);

            if (accInDb != null)
            {
                if (accInDb.DatabasePathErasoft != null)
                {
                    if (accInDb.DatabasePathErasoft != "")
                    {
                        try
                        {
#if AWS
                    System.Data.Entity.Database.Delete($"Server=localhost;Initial Catalog={accInDb.DatabasePathErasoft};persist security info=True;" +
                                                       "user id=masteronline;password=M@ster123;");
#elif Debug_AWS
                    System.Data.Entity.Database.Delete($"Server=13.250.232.74\\SQLEXPRESS,1433;Initial Catalog={accInDb.DatabasePathErasoft};persist security info=True;" +
                                                       "user id=masteronline;password=M@ster123;");
#else
                            System.Data.Entity.Database.Delete($"Server=13.251.222.53\\SQLEXPRESS,1433;Initial Catalog={accInDb.DatabasePathErasoft};persist security info=True;" +
                                                               "user id=masteronline;password=M@ster123;");
#endif

                            accInDb.DatabasePathErasoft = null;
                            ViewData["SuccessMessage"] = $"Akun {accInDb.Username} berhasil dihapus databasenya.";
                        }
                        catch (Exception e)
                        {
                            return Content(e.Message);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", @"Database tidak ditemukan!");
                    }
                }
                else
                {
                    ModelState.AddModelError("", @"Database tidak ditemukan!");
                }
            }

            MoDbContext.SaveChanges();

            var listAcc = MoDbContext.Account.ToList();

            return View("DatabaseMenu", listAcc);
        }

        public ActionResult TambahHapusAcc(int? accId)
        {
            var accInDb = MoDbContext.Account.FirstOrDefault(a => a.AccountId == accId);

            if (accInDb != null)
            {
                try
                {
                    if (accInDb.DatabasePathErasoft != null)
                    {
                        if (accInDb.DatabasePathErasoft != "")
                        {

#if AWS
                                        System.Data.Entity.Database.Delete($"Server=localhost;Initial Catalog={accInDb.DatabasePathErasoft};persist security info=True;" +
                                                                           "user id=masteronline;password=M@ster123;");
#elif Debug_AWS
                                        System.Data.Entity.Database.Delete($"Server=13.250.232.74\\SQLEXPRESS,1433;Initial Catalog={accInDb.DatabasePathErasoft};persist security info=True;" +
                                                                           "user id=masteronline;password=M@ster123;");
#else

                            System.Data.Entity.Database.Delete($"Server=13.251.222.53\\SQLEXPRESS,1433;Initial Catalog={accInDb.DatabasePathErasoft};persist security info=True;" +
                                                               "user id=masteronline;password=M@ster123;");
#endif
                        }
                    }

                    var uname = accInDb.Username;
                    MoDbContext.Account.Remove(accInDb);
                    MoDbContext.SaveChanges();
                    var cekUlangAcc = MoDbContext.Account.FirstOrDefault(a => a.AccountId == accId);
                    //accInDb.DatabasePathErasoft = null;
                    if (cekUlangAcc == null)
                    {
                        ViewData["SuccessMessage"] = $"Database dan Akun dari {uname} berhasil dihapus.";
                    }

                }

                catch (Exception e)
                {
                    return Content(e.Message);
                }
            }

            //MoDbContext.SaveChanges();

            var listAcc = MoDbContext.Account.ToList();

            return View("DatabaseMenu", listAcc);
        }

        // =============================================== Bagian User & Account (END)

        // =============================================== Bagian Promo (START)

        public ActionResult Promo()
        {
            var vm = new PromoViewModel()
            {
                ListPromo = MoDbContext.Promo.ToList()
            };

            return View(vm);
        }

        // =============================================== Bagian Promo (END)

        // =============================================== Bagian Subs (START)

        public ActionResult Subscription()
        {
            var vm = new SubsViewModel()
            {
                ListSubs = MoDbContext.Subscription.ToList()
            };

            return View(vm);
        }

        public ActionResult EditSubs(int? idSub)
        {
            var vm = new SubsViewModel()
            {
                Subs = MoDbContext.Subscription.SingleOrDefault(m => m.RecNum == idSub),
            };

            ViewData["Editing"] = 1;

            return View("Subscription", vm);
        }

        public ActionResult DeleteSubscription(int? subsId)
        {
            var subsVm = new SubsViewModel()
            {
                Subs = MoDbContext.Subscription.Single(m => m.RecNum == subsId),
                ListSubs = MoDbContext.Subscription.ToList()
            };

            MoDbContext.Subscription.Remove(subsVm.Subs);
            MoDbContext.SaveChanges();

            return RedirectToAction("Subscription");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveSubscription(SubsViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View("Subscription", vm);
            }

            if (vm.Subs.RecNum == null)
            {
                var subsInDb = MoDbContext.Subscription.SingleOrDefault(m => m.KODE == vm.Subs.KODE);

                if (subsInDb != null)
                {
                    ModelState.AddModelError("", @"Kode subscription sudah terdaftar!");
                    return View("Subscription", vm);
                }

                MoDbContext.Subscription.Add(vm.Subs);
            }
            else
            {
                var subsInDb = MoDbContext.Subscription.Single(m => m.RecNum == vm.Subs.RecNum);
                subsInDb.KETERANGAN = vm.Subs.KETERANGAN;
                subsInDb.JUMLAH_PESANAN = vm.Subs.JUMLAH_PESANAN;
                subsInDb.JUMLAH_MP = vm.Subs.JUMLAH_MP;
                subsInDb.HARGA = vm.Subs.HARGA;
            }

            MoDbContext.SaveChanges();
            ModelState.Clear();

            return RedirectToAction("Subscription");
        }

        // =============================================== Bagian Subs (END)

        // =============================================== Bagian History Pembayaran (START)
        [Route("admin/manage/AktivitasSubscription")]
        [SessionAdminCheck]
        public ActionResult AktivitasSubscription()
        {
            var vm = new SubsViewModel()
            {
                ListAktivitasSubs = MoDbContext.AktivitasSubscription.ToList(),
                //ADD BY NURUL 22/2/2019
                ListSubs = MoDbContext.Subscription.ToList(),
                ListAccount = MoDbContext.Account.ToList()
                //END ADD BY NURUL 22/2/2019
            };

            return View(vm);
        }

        public ActionResult EditPayment(int? paymentId)
        {
            var vm = new SubsViewModel()
            {
                Payment = MoDbContext.AktivitasSubscription.SingleOrDefault(m => m.RecNum == paymentId),
            };

            //ViewData["Editing"] = 1;

            //return View("FormHistoryPembayaranPartial", vm);
            return PartialView("FormHistoryPembayaranPartial", vm);
        }

        public ActionResult DeletePayment(int? paymentId)
        {
            var subsVm = new SubsViewModel()
            {
                Payment = MoDbContext.AktivitasSubscription.Single(m => m.RecNum == paymentId),
                ListAktivitasSubs = MoDbContext.AktivitasSubscription.ToList()
            };

            MoDbContext.AktivitasSubscription.Remove(subsVm.Payment);
            MoDbContext.SaveChanges();

            return RedirectToAction("AktivitasSubscription");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SavePayment(SubsViewModel vm)
        {
            var newPayment = false;
            if (!ModelState.IsValid)
            {
                //return View("FormHistoryPembayaranPartial", vm);
                return PartialView("FormHistoryPembayaranPartial", vm);
            }

            if (vm.Payment.RecNum == null)
            {
                //var subsInDb = MoDbContext.AktivitasSubscription.SingleOrDefault(m => m.KODE == vm.Subs.KODE);

                //if (subsInDb != null)
                //{
                //    ModelState.AddModelError("", @"Kode history pembayaran sudah terdaftar!");
                //    return View("AktivitasSubscription", vm);
                //}
                if (vm.Payment.TipePembayaran == null)
                {
                    vm.Payment.TipePembayaran = "Manual_Transfer";
                }
                MoDbContext.AktivitasSubscription.Add(vm.Payment);
                newPayment = true;
            }
            else
            {
                var subsInDb = MoDbContext.AktivitasSubscription.Single(m => m.RecNum == vm.Payment.RecNum);
                subsInDb.Email = vm.Payment.Email;
                subsInDb.Account = vm.Payment.Account;
                subsInDb.TipeSubs = vm.Payment.TipeSubs;
                subsInDb.TanggalBayar = vm.Payment.TanggalBayar;
                subsInDb.Nilai = vm.Payment.Nilai;
                subsInDb.TipePembayaran = vm.Payment.TipePembayaran;
                subsInDb.DrTGL = vm.Payment.DrTGL;
                subsInDb.SdTGL = vm.Payment.SdTGL;
                subsInDb.jumlahUser = vm.Payment.jumlahUser;

                //vm.newPayment = false;

            }

            //change by Tri, 25 Nov 2019
            //var akun = MoDbContext.Account.Single(m => m.Email == vm.Payment.Email && m.Username == vm.Payment.Account);
            var akun = MoDbContext.Account.Single(m => m.Email == vm.Payment.Email);
            //end change by Tri, 25 Nov 2019
            akun.KODE_SUBSCRIPTION = vm.Payment.TipeSubs;
            akun.jumlahUser = vm.Payment.jumlahUser;
            akun.TGL_SUBSCRIPTION = vm.Payment.SdTGL;
            MoDbContext.SaveChanges();
            ModelState.Clear();
            if (newPayment == true)
            {
                var cekPayment = MoDbContext.AktivitasSubscription.Where(a => a.Email == vm.Payment.Email && a.TanggalBayar == vm.Payment.TanggalBayar && a.Nilai == vm.Payment.Nilai).ToList();
                if (cekPayment != null)
                {
                    await SendInvoice(Convert.ToString(cekPayment.Single().RecNum), "0");
                }
            }

            return PartialView("FormHistoryPembayaranPartial", vm);
        }
        [HttpGet]
        public ActionResult GetAccount()
        {
            var account = MoDbContext.Account.ToList();

            return Json(account, JsonRequestBehavior.AllowGet);
        }

        //add by nurul 12/8/2019, kirim invoice lewat email 
        //public async Task<ActionResult> SendInvoice(int? aktSubID, string btnKirim)
        public async Task<ActionResult> SendInvoice(string aktSubID, string btnKirim)
        {
            try
            {
                var ambilUlangAktSub = new AktivitasSubscription();
                bool succes = false;
                var aktSubId = Convert.ToInt32(aktSubID);
                var aktSub = MoDbContext.AktivitasSubscription.Single(u => u.RecNum == aktSubId);
                if (aktSub.Invoice_No == null || aktSub.Invoice_No.Substring(3, 4) == "2019") //kalo Invoice_No null/masih format lama
                {
                    var sub = MoDbContext.Subscription.Single(u => u.KODE == aktSub.TipeSubs).KETERANGAN;

                    var listAktSubInDb = MoDbContext.AktivitasSubscription.OrderBy(p => p.RecNum).ToList();
                    var cekListSudahAdaNoInv = MoDbContext.AktivitasSubscription.Where(a => a.Invoice_No != null && a.Invoice_No.Substring(3, 4) != "2019").OrderBy(p => p.RecNum).ToList();
                    var digitAkhir = "";
                    var noInv = "";
                    //if (listAktSubInDb.Count == 0)
                    if (cekListSudahAdaNoInv.Count() == 0)
                    {
                        digitAkhir = "0001";
                        noInv = $"MO/{DateTime.Now.Year.ToString().Substring(2, 2)}/{digitAkhir}";
                    }
                    else 
                    {
                        //var lastRecNum = listAktSubInDb.Last().RecNum;
                        //lastRecNum++;
                        //var NoUrut = listAktSubInDb.Count();
                        var NoUrut = Convert.ToInt32(listAktSubInDb.Where(a => a.Invoice_No != null && a.Invoice_No.Substring(3, 4) != "2019").OrderByDescending(a => a.Invoice_No).FirstOrDefault().Invoice_No.Substring(6, 4));
                        NoUrut++;

                        digitAkhir = NoUrut.ToString().PadLeft(4, '0');
                        noInv = $"MO/{DateTime.Now.Year.ToString().Substring(2, 2)}/{digitAkhir}";
                    }

                    if (digitAkhir != "" && noInv != "")
                    {
                        aktSub.Invoice_No = noInv;
                        aktSub.tgl_email = DateTime.Today;
                    }

                    var email = new MailAddress(aktSub.Email);
                    //add
                    var today = DateTime.Today.ToString("dd/MM/yyyy");
                    var nama = aktSub.Account;
                    var tglBayar = aktSub.TanggalBayar?.ToString("dd/MM/yyyy");
                    var subs = sub;
                    var nilai = $"Rp. {String.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:N}", aktSub.Nilai)}";
                    var jmlUser = aktSub.jumlahUser.ToString();
                    var drTgl = aktSub.DrTGL?.ToString("dd/MM/yyyy");
                    var sdTgl = aktSub.SdTGL?.ToString("dd/MM/yyyy");
                    var inv = noInv;
                    //end add

                    var message = new MailMessage();
                    message.To.Add(email);
                    message.From = new MailAddress("csmasteronline@gmail.com");
                    message.Subject = "Email Payment Subscription";
                    message.Body = System.IO.File.ReadAllText(Server.MapPath("~/Content/admin/PaymentSubscription.html"))
                        .Replace("EMAIL", Convert.ToString(email))
                        .Replace("TODAY", today)
                        .Replace("NAMA", nama)
                        .Replace("TGLBAYAR", tglBayar)
                        .Replace("SUBS", subs)
                        .Replace("NILAI", nilai)
                        .Replace("JMLUSER", jmlUser)
                        .Replace("DRTGL", drTgl)
                        .Replace("SDTGL", sdTgl)
                        .Replace("NOINV", inv);
                    message.IsBodyHtml = true;

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
                        try
                        {
                            await smtp.SendMailAsync(message);
                            succes = true;
                            MoDbContext.SaveChanges();
                            ModelState.Clear();
                        }
                        catch (Exception ex)
                        {
                            ViewData["SuccessMessage"] = $"Pembayaran {ambilUlangAktSub.Account} gagal kirim email.";
                        }
                    }
                }
                ambilUlangAktSub = MoDbContext.AktivitasSubscription.Single(u => u.RecNum == aktSubId);
                if (btnKirim == "1" && succes == true)
                {
                    ViewData["SuccessMessage"] = $"Pembayaran {ambilUlangAktSub.Account} berhasil dikirim email.";

                    //var vm = new SubsViewModel()
                    //{
                    //    ListAktivitasSubs = MoDbContext.AktivitasSubscription.ToList(),
                    //    ListSubs = MoDbContext.Subscription.ToList(),
                    //    ListAccount = MoDbContext.Account.ToList()
                    //};

                    //return PartialView("AktivitasSubscription", vm);
                    return RedirectToAction("AktivitasSubscription");
                }
                else if (succes == false)
                {
                    ViewData["SuccessMessage"] = $"Pembayaran {ambilUlangAktSub.Account} gagal kirim email.";
                    return RedirectToAction("AktivitasSubscription");
                }
                return new EmptyResult();
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }
        }
        //end add by nurul 12/8/2019

        // =============================================== Bagian History Pembayaran (END)

        // =============================================== Bagian Marketplace (START)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveMarketplace(MarketplaceMenuViewModel marketVm)
        {
            if (!ModelState.IsValid)
            {
                return View("MarketplaceMenu", marketVm);
            }

            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/Uploaded/"), fileName);
                    marketVm.Marketplace.LokasiLogo = "~/Content/Uploaded/" + fileName;
                    file.SaveAs(path);
                }
            }

            if (marketVm.Marketplace.IdMarket == null)
            {
                var marketInDb = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket == marketVm.Marketplace.NamaMarket);

                if (marketInDb != null)
                {
                    ModelState.AddModelError("", @"Marketplace sudah terdaftar!");
                    return View("MarketplaceMenu", marketVm);
                }

                MoDbContext.Marketplaces.Add(marketVm.Marketplace);
            }
            else
            {
                var marketInDb = MoDbContext.Marketplaces.Single(m => m.IdMarket == marketVm.Marketplace.IdMarket);
                marketInDb.NamaMarket = marketVm.Marketplace.NamaMarket;
                marketInDb.Status = marketVm.Marketplace.Status;

                if (!String.IsNullOrWhiteSpace(marketVm.Marketplace.LokasiLogo))
                {
                    marketInDb.LokasiLogo = marketVm.Marketplace.LokasiLogo;
                }
            }

            MoDbContext.SaveChanges();
            ModelState.Clear();

            return RedirectToAction("MarketplaceMenu");
        }

        public ActionResult EditMarket(int? marketId)
        {
            var marketVm = new MarketplaceMenuViewModel()
            {
                Marketplace = MoDbContext.Marketplaces.Single(m => m.IdMarket == marketId),
            };

            ViewData["Editing"] = 1;

            return View("MarketplaceMenu", marketVm);
        }

        public ActionResult DeleteMarket(int? marketId)
        {
            var marketVm = new MarketplaceMenuViewModel()
            {
                Marketplace = MoDbContext.Marketplaces.Single(m => m.IdMarket == marketId),
                ListMarket = MoDbContext.Marketplaces.ToList()
            };

            string fullPath = Request.MapPath(marketVm.Marketplace.LokasiLogo);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            MoDbContext.Marketplaces.Remove(marketVm.Marketplace);
            MoDbContext.SaveChanges();

            return RedirectToAction("MarketplaceMenu");
        }

        // =============================================== Bagian Marketplace (END)

        // =============================================== Bagian Ekpedisi (START)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveEkspedisi(CourierViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View("CourierMenu", vm);
            }

            if (vm.Ekspedisi.RecNum == null)
            {
                var eksInDb = MoDbContext.Ekspedisi.SingleOrDefault(e => e.NamaEkspedisi == vm.Ekspedisi.NamaEkspedisi);

                if (eksInDb != null)
                {
                    ModelState.AddModelError("", @"Ekspedisi sudah terdaftar!");
                    return View("CourierMenu", vm);
                }

                MoDbContext.Ekspedisi.Add(vm.Ekspedisi);
            }
            else
            {
                var eksInDb = MoDbContext.Ekspedisi.Single(e => e.RecNum == vm.Ekspedisi.RecNum);
                eksInDb.NamaEkspedisi = vm.Ekspedisi.NamaEkspedisi;
                eksInDb.Status = vm.Ekspedisi.Status;
            }

            MoDbContext.SaveChanges();
            ModelState.Clear();

            return RedirectToAction("CourierMenu");
        }

        public ActionResult EditEkspedisi(int? eksId)
        {
            var vm = new CourierViewModel()
            {
                Ekspedisi = MoDbContext.Ekspedisi.Single(e => e.RecNum == eksId)
            };

            ViewData["Editing"] = 1;

            return View("CourierMenu", vm);
        }

        public ActionResult DeleteEkspedisi(int? eksId)
        {
            var vm = new CourierViewModel()
            {
                Ekspedisi = MoDbContext.Ekspedisi.Single(e => e.RecNum == eksId),
                ListEkspedisi = MoDbContext.Ekspedisi.ToList()
            };

            MoDbContext.Ekspedisi.Remove(vm.Ekspedisi);
            MoDbContext.SaveChanges();

            return RedirectToAction("CourierMenu");
        }

        // =============================================== Bagian Ekpedisi (END)

        // =============================================== Bagian Form (START)

        public ActionResult ShowToggle(int formId)
        {
            var formInDb = MoDbContext.FormMoses.Single(m => m.ScrId == formId);

            formInDb.Show = !formInDb.Show;

            if (formInDb.HaveChild)
            {
                var childs = MoDbContext.FormMoses.Where(c => c.ParentId == formId).ToList();

                foreach (var child in childs)
                {
                    child.Show = formInDb.Show;
                }
            }

            MoDbContext.SaveChanges();

            return RedirectToAction("FormMenu");
        }

        // =============================================== Bagian Form (END)
        // =============================================== Bagian Editor Account (START)
        [Route("admin/manage/editor-account")]
        [SessionAdminCheck]
        public ActionResult AccountMenuEdit()
        {
            //change by nurul 13/2/2019
            //var listAcc = MoDbContext.Account.ToList();
            //return View(listAcc);
            var vm = new MenuAccount()
            {
                ListAccount = MoDbContext.Account.OrderByDescending(a => a.TGL_DAFTAR).ToList(),
                ListPartner = MoDbContext.Partner.ToList()
            };
            return View(vm);
            //end change by nurul 13/2/2019
        }

        public ActionResult AccountDetailEdit(int? accId)
        {
            var accInDb = MoDbContext.Account.SingleOrDefault(a => a.AccountId == accId);

            if (accInDb == null)
                return View("Error");

            //add by nurul 20/2/2019
            var vm = new MenuAccount()
            {
                Account = accInDb
            };
            //end add by nurul 20/2/2019

            //return View(accInDb);
            return PartialView("AccountDetailEdit", vm);
        }

        //add by nurul 13/2/2019
        public ActionResult RefreshAccountMenuEdit()
        {
            var vm = new MenuAccount()
            {
                ListAccount = MoDbContext.Account.OrderByDescending(a => a.TGL_DAFTAR).ToList(),
                ListPartner = MoDbContext.Partner.ToList()
            };
            return PartialView("TableAccountEdit", vm);
        }


        public ActionResult EditAccount(int? accountId)
        {
            var vm = new MenuAccount()
            {
                Account = MoDbContext.Account.SingleOrDefault(m => m.AccountId == accountId),
                ListSubs = MoDbContext.Subscription.ToList()
            };

            //ViewData["Editing"] = 1;

            //return View("AccountMenuEdit", vm);
            return PartialView("FormAccountPartial", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveAccount(MenuAccount data)
        {
            if (!ModelState.IsValid)
            {
                return View("AccountMenuEdit", data);
            }

            if (data.Account.AccountId == 0)
            {
                var accInDb = MoDbContext.Account.SingleOrDefault(m => m.AccountId == data.Account.AccountId);

                if (accInDb != null)
                {
                    ModelState.AddModelError("", @"Kode account sudah terdaftar!");
                    return View("AccountMenuEdit", data);
                }

                MoDbContext.Account.Add(data.Account);
            }
            else
            {
                var accInDb = MoDbContext.Account.Single(m => m.AccountId == data.Account.AccountId);
                //if (accInDb.Status != vm.Account.Status)
                //{
                //    Task<ActionResult> x = ChangeStatusPartner(Convert.ToString(vm.Account.AccountId));
                //}
                accInDb.Email = data.Account.Email;
                accInDb.KODE_SUBSCRIPTION = data.Account.KODE_SUBSCRIPTION;
                accInDb.jumlahUser = data.Account.jumlahUser;
                accInDb.TGL_SUBSCRIPTION = data.Account.TGL_SUBSCRIPTION;
                accInDb.NoHp = data.Account.NoHp;

            }

            MoDbContext.SaveChanges();
            ModelState.Clear();

            var vm = new MenuAccount()
            {
                Account = MoDbContext.Account.SingleOrDefault(a => a.AccountId == data.Account.AccountId)
            };

            return PartialView("FormAccountPartial", vm);
        }
        //add by nurul 12/3/2019
        public ActionResult EditAccountNew(int? accountId)
        {
            var vm = new MenuAccount()
            {
                Account = MoDbContext.Account.SingleOrDefault(m => m.AccountId == accountId),
                ListSubs = MoDbContext.Subscription.ToList()
            };

            //ViewData["Editing"] = 1;

            //return View("AccountMenuEdit", vm);
            return PartialView("FormAccountPartialNew", vm);
        }

        //end add by nurul 12/3/2019
        // =============================================== Bagian Editor Account (END)

        // =============================================== Menu-menu pada halaman admin (START)

        [Route("admin/manage/account")]
        [SessionAdminCheck]
        public ActionResult AccountMenu()
        {
            //change by nurul 13/2/2019
            //var listAcc = MoDbContext.Account.ToList();
            //return View(listAcc);
            var vm = new MenuAccount()
            {
                ListAccount = MoDbContext.Account.OrderByDescending(a => a.TGL_DAFTAR).ToList(),
                ListPartner = MoDbContext.Partner.ToList()
            };
            return View(vm);
            //end change by nurul 13/2/2019
        }

        //add by nurul 13/2/2019
        public ActionResult RefreshAccountMenu()
        {
            var vm = new MenuAccount()
            {
                ListAccount = MoDbContext.Account.OrderByDescending(a => a.TGL_DAFTAR).ToList(),
                ListPartner = MoDbContext.Partner.ToList()
            };
            return PartialView("TableAccount", vm);
        }


        //add by Iman 15/04/2019
        [Route("admin/manage/reminder-expired")]
        [SessionAdminCheck]
        public ActionResult AccountReminderExpired(string param)
        {
            //if (param != null)
            //{
            //    string dr = (param.Split(';')[param.Split(';').Length - 2]);
            //    string sd = (param.Split(';')[param.Split(';').Length - 1]);
            //    string tgl1 = (dr.Split('/')[dr.Split('/').Length - 3]);
            //    string bln1 = (dr.Split('/')[dr.Split('/').Length - 2]);
            //    string thn1 = (dr.Split('/')[dr.Split('/').Length - 1]);
            //    string drtanggal = tgl1 + '/' + bln1 + '/' + thn1;
            //    string tgl2 = (sd.Split('/')[sd.Split('/').Length - 3]);
            //    string bln2 = (sd.Split('/')[sd.Split('/').Length - 2]);
            //    string thn2 = (sd.Split('/')[sd.Split('/').Length - 1]);
            //    string sdtanggal = tgl2 + '/' + bln2 + '/' + thn2;
            //    var drTgl = DateTime.ParseExact(drtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            //    var sdTgl = DateTime.ParseExact(sdtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);


            //    var vm = new MenuAccount()
            //    {
            //        ListAccount = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION >= drTgl && a.TGL_SUBSCRIPTION <= sdTgl && a.Status == true).ToList(),
            //        //ListAccount = MoDbContext.Account.OrderByDescending(a => a.TGL_SUBSCRIPTION >= DateTime.Today && a.TGL_SUBSCRIPTION <= date).ToList(),
            //        ListPartner = MoDbContext.Partner.ToList()
            //    };
            //    return PartialView("TableAccountReminderExpired", vm);
            //    //return View(vm);
            //}
            //else
            //{

            DateTime dateTime = DateTime.UtcNow.Date;
            DateTime Nextmonth = dateTime.AddMonths(1);
            param = dateTime.ToString("dd/MM/yyyy") + ";" + Nextmonth.ToString("dd/MM/yyyy");
            //param = "29/04/2019;29/05/2019";
            string dr = (param.Split(';')[param.Split(';').Length - 2]);
            string sd = (param.Split(';')[param.Split(';').Length - 1]);
            string tgl1 = (dr.Split('/')[dr.Split('/').Length - 3]);
            string bln1 = (dr.Split('/')[dr.Split('/').Length - 2]);
            string thn1 = (dr.Split('/')[dr.Split('/').Length - 1]);
            string drtanggal = tgl1 + '/' + bln1 + '/' + thn1;
            string tgl2 = (sd.Split('/')[sd.Split('/').Length - 3]);
            string bln2 = (sd.Split('/')[sd.Split('/').Length - 2]);
            string thn2 = (sd.Split('/')[sd.Split('/').Length - 1]);
            string sdtanggal = tgl2 + '/' + bln2 + '/' + thn2;
            var drTgl = DateTime.ParseExact(drtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var sdTgl = DateTime.ParseExact(sdtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var vm = new MenuAccount
            {
                //ListAccount = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION >= drTgl && a.TGL_SUBSCRIPTION <= sdTgl && a.Status == true).ToList(),
                ListPartner = MoDbContext.Partner.ToList()
            };

            return View("AccountReminderExpired", vm);

            //var date = DateTime.Today.AddMonths(+1);
            //var vm = new MenuAccount()
            //{                    
            //    ListAccount = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION >= DateTime.Today && a.TGL_SUBSCRIPTION <= date && a.Status == true).ToList(),
            //    ListPartner = MoDbContext.Partner.ToList()
            //};
            //return View(vm);
            //}
        }
        public ActionResult TAccountReminderExpired(string param)
        {
            if (param != null)
            {
                if (param.Length > 1)
                {

                    string dr = (param.Split(';')[param.Split(';').Length - 2]);
                    string sd = (param.Split(';')[param.Split(';').Length - 1]);
                    string tgl1 = (dr.Split('/')[dr.Split('/').Length - 3]);
                    string bln1 = (dr.Split('/')[dr.Split('/').Length - 2]);
                    string thn1 = (dr.Split('/')[dr.Split('/').Length - 1]);
                    string drtanggal = tgl1 + '/' + bln1 + '/' + thn1;
                    string tgl2 = (sd.Split('/')[sd.Split('/').Length - 3]);
                    string bln2 = (sd.Split('/')[sd.Split('/').Length - 2]);
                    string thn2 = (sd.Split('/')[sd.Split('/').Length - 1]);
                    string sdtanggal = tgl2 + '/' + bln2 + '/' + thn2;
                    var drTgl = DateTime.ParseExact(drtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    var sdTgl = DateTime.ParseExact(sdtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    var vm = new MenuAccount()
                    {
                        ListAccount = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION >= drTgl && a.TGL_SUBSCRIPTION <= sdTgl && a.Status == true).ToList(),
                        ListPartner = MoDbContext.Partner.ToList()
                    };
                    return PartialView("TableAccountReminderExpired", vm);
                    //return View(vm);
                }
                else
                {
                    DateTime dateTime = DateTime.UtcNow.Date;
                    DateTime Nextmonth = dateTime.AddMonths(1);
                    param = dateTime.ToString("dd/MM/yyyy") + ";" + Nextmonth.ToString("dd/MM/yyyy");
                    string dr = (param.Split(';')[param.Split(';').Length - 2]);
                    string sd = (param.Split(';')[param.Split(';').Length - 1]);
                    string tgl1 = (dr.Split('/')[dr.Split('/').Length - 3]);
                    string bln1 = (dr.Split('/')[dr.Split('/').Length - 2]);
                    string thn1 = (dr.Split('/')[dr.Split('/').Length - 1]);
                    string drtanggal = tgl1 + '/' + bln1 + '/' + thn1;
                    string tgl2 = (sd.Split('/')[sd.Split('/').Length - 3]);
                    string bln2 = (sd.Split('/')[sd.Split('/').Length - 2]);
                    string thn2 = (sd.Split('/')[sd.Split('/').Length - 1]);
                    string sdtanggal = tgl2 + '/' + bln2 + '/' + thn2;
                    var drTgl = DateTime.ParseExact(drtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    var sdTgl = DateTime.ParseExact(sdtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    var vm = new MenuAccount()
                    {
                        ListAccount = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION >= drTgl && a.TGL_SUBSCRIPTION <= sdTgl && a.Status == true).ToList(),
                        ListPartner = MoDbContext.Partner.ToList()
                    };
                    return PartialView("TableAccountReminderExpired", vm);
                    return View(vm);
                }

            }
            else
            {

                DateTime dateTime = DateTime.UtcNow.Date;
                DateTime Nextmonth = dateTime.AddMonths(1);
                param = dateTime.ToString("dd/MM/yyyy") + ";" + Nextmonth.ToString("dd/MM/yyyy");
                string dr = (param.Split(';')[param.Split(';').Length - 2]);
                string sd = (param.Split(';')[param.Split(';').Length - 1]);
                string tgl1 = (dr.Split('/')[dr.Split('/').Length - 3]);
                string bln1 = (dr.Split('/')[dr.Split('/').Length - 2]);
                string thn1 = (dr.Split('/')[dr.Split('/').Length - 1]);
                string drtanggal = tgl1 + '/' + bln1 + '/' + thn1;
                string tgl2 = (sd.Split('/')[sd.Split('/').Length - 3]);
                string bln2 = (sd.Split('/')[sd.Split('/').Length - 2]);
                string thn2 = (sd.Split('/')[sd.Split('/').Length - 1]);
                string sdtanggal = tgl2 + '/' + bln2 + '/' + thn2;
                var drTgl = DateTime.ParseExact(drtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                var sdTgl = DateTime.ParseExact(sdtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                var vm = new MenuAccount()
                {
                    ListAccount = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION >= drTgl && a.TGL_SUBSCRIPTION <= sdTgl && a.Status == true).ToList(),
                    ListPartner = MoDbContext.Partner.ToList()
                };
                return PartialView("TableAccountReminderExpired", vm);
            }
        }



        [SessionAdminCheck]
        public ActionResult AccountMenuSudahExpired(string param)
        {
            string tgl1 = (param.Split('/')[param.Split('/').Length - 3]);
            string bln1 = (param.Split('/')[param.Split('/').Length - 2]);
            string thn1 = (param.Split('/')[param.Split('/').Length - 1]);
            string tanggal = tgl1 + '/' + bln1 + '/' + thn1;
            var perTgl = DateTime.ParseExact(tanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var vm = new MenuAccount()
            {
                ListAccount = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION <= perTgl && a.Status == true).ToList(),
                ListPartner = MoDbContext.Partner.ToList()
            };
            return PartialView("TableAccountSudahExpired", vm);
            //return View(vm);
        }
        //end add by nurul Iman 15/04/2019




        //add by nurul 1/4/2019
        [SessionAdminCheck]
        public ActionResult AccountMenuAktif()
        {
            var vm = new MenuAccount()
            {
                ListAccount = MoDbContext.Account.Where(a => a.Status == true).ToList(),
                ListPartner = MoDbContext.Partner.ToList()
            };
            return PartialView("TableAccountAktif", vm);
        }
        [SessionAdminCheck]
        public ActionResult AccountMenuNonaktif(string param)
        {
            var vm = new MenuAccount()
            {
                ListAccount = MoDbContext.Account.Where(a => a.Status == false).ToList(),
                ListPartner = MoDbContext.Partner.ToList()
            };
            return PartialView("TableAccountNonaktif", vm);
        }
        //end add by nurul 1/4/2019

        [SessionAdminCheck]
        public ActionResult AccountMenuWillExpired(string param)
        {
            string dr = (param.Split(';')[param.Split(';').Length - 2]);
            string sd = (param.Split(';')[param.Split(';').Length - 1]);
            string tgl1 = (dr.Split('/')[dr.Split('/').Length - 3]);
            string bln1 = (dr.Split('/')[dr.Split('/').Length - 2]);
            string thn1 = (dr.Split('/')[dr.Split('/').Length - 1]);
            string drtanggal = tgl1 + '/' + bln1 + '/' + thn1;
            string tgl2 = (sd.Split('/')[sd.Split('/').Length - 3]);
            string bln2 = (sd.Split('/')[sd.Split('/').Length - 2]);
            string thn2 = (sd.Split('/')[sd.Split('/').Length - 1]);
            string sdtanggal = tgl2 + '/' + bln2 + '/' + thn2;
            var drTgl = DateTime.ParseExact(drtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var sdTgl = DateTime.ParseExact(sdtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var vm = new MenuAccount()
            {
                ListAccount = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION >= drTgl && a.TGL_SUBSCRIPTION <= sdTgl && a.Status == true).ToList(),
                ListPartner = MoDbContext.Partner.ToList()
            };
            return PartialView("TableAccountWillExpired", vm);
        }
        [SessionAdminCheck]
        public ActionResult AccountMenuExpired(string param)
        {
            string tgl1 = (param.Split('/')[param.Split('/').Length - 3]);
            string bln1 = (param.Split('/')[param.Split('/').Length - 2]);
            string thn1 = (param.Split('/')[param.Split('/').Length - 1]);
            string tanggal = tgl1 + '/' + bln1 + '/' + thn1;
            var perTgl = DateTime.ParseExact(tanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var vm = new MenuAccount()
            {
                ListAccount = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION <= perTgl && a.Status == true).ToList(),
                ListPartner = MoDbContext.Partner.ToList()
            };
            return PartialView("TableAccountExpired", vm);
        }
        //end add by nurul 13/2/2019

        [Route("admin/manage/user")]
        [SessionAdminCheck]
        public ActionResult UserMenu()
        {
            var vm = new UserViewModel()
            {
                ListUser = MoDbContext.User.ToList(),
                ListAccount = MoDbContext.Account.ToList()
            };

            return View(vm);
        }

        [Route("admin/manage/marketplace")]
        [SessionAdminCheck]
        public ActionResult MarketplaceMenu()
        {
            var marketVm = new MarketplaceMenuViewModel()
            {
                ListMarket = MoDbContext.Marketplaces.ToList()
            };

            return View(marketVm);
        }

        [Route("admin/manage/courier")]
        [SessionAdminCheck]
        public ActionResult CourierMenu()
        {
            var vm = new CourierViewModel()
            {
                ListEkspedisi = MoDbContext.Ekspedisi.ToList()
            };

            return View(vm);
        }

        [Route("admin/manage/form")]
        [SessionAdminCheck]
        public ActionResult FormMenu()
        {
            var forms = MoDbContext.FormMoses.ToList();

            return View(forms);
        }

        [Route("admin/manage/partner")]
        [SessionAdminCheck]
        public ActionResult PartnerMenu()
        {
            var vm = new PartnerViewModel()
            {
                ListPartner = MoDbContext.Partner.OrderByDescending(a => a.TGL_DAFTAR).ToList()
            };

            return View(vm);
        }

        //add by nurul 15/2/2019
        // =============================================== Menu Partner
        //add by nurul 4/10/2019
        public class getTotalCount
        {
            public int JUMLAH { get; set; }
        }
        public ActionResult RefreshTablePartner(int? page, string search = "")
        {
            int pagenumber = (page ?? 1) - 1;
            ViewData["searchParam"] = search;
            ViewData["LastPage"] = page;
            string[] getkata = search.Split(' ');
            string sSQLnama = "";
            string sSQLemail = "";
            string sSQLhp = "";
            string sSQLtipe = "";
            string sSQLref = "";
            if (getkata.Length > 0)
            {
                if (search != "")
                {
                    for (int i = 0; i < getkata.Length; i++)
                    {
                        if (getkata.Length == 1)
                        {
                            sSQLnama += "( Username like '%" + getkata[i] + "%' )";
                            sSQLemail += " ( Email like '%" + getkata[i] + "%' )";
                            sSQLhp += " ( NoHp like '%" + getkata[i] + "%' )";
                            sSQLtipe += " ( NamaTipe like '%" + getkata[i] + "%' )";
                            sSQLref += " ( isnull(KodeRefPilihan,'') like '%" + getkata[i] + "%' )";
                        }
                        else
                        {
                            if (getkata[i] == getkata.First())
                            {
                                sSQLnama += " ( Username like '%" + getkata[i] + "%'";
                                sSQLemail += " ( Email like '%" + getkata[i] + "%'";
                                sSQLhp += " ( NoHp like '%" + getkata[i] + "%'";
                                sSQLtipe += "( NamaTipe like '%" + getkata[i] + "%'";
                                sSQLref += " ( isnull(KodeRefPilihan,'') like '%" + getkata[i] + "%' ";
                            }
                            else if (getkata[i] == getkata.Last())
                            {
                                sSQLnama += " and Username like '%" + getkata[i] + "%' )";
                                sSQLemail += " and Email like '%" + getkata[i] + "%' )";
                                sSQLhp += " and NoHp like '%" + getkata[i] + "%' )";
                                sSQLtipe += " and NamaTipe like '%" + getkata[i] + "%' )";
                                sSQLref += " and isnull(KodeRefPilihan,'') like '%" + getkata[i] + "%' )";
                            }
                            else
                            {
                                sSQLnama += " and Username like '%" + getkata[i] + "%' ";
                                sSQLemail += " and Email like '%" + getkata[i] + "%' ";
                                sSQLhp += " and NoHp like '%" + getkata[i] + "%' ";
                                sSQLtipe += " and NamaTipe like '%" + getkata[i] + "%' ";
                                sSQLref += " and isnull(KodeRefPilihan,'') like '%" + getkata[i] + "%' ";
                            }
                        }
                    }
                }
            }
            //var partners = (from p in MoDbContext.Partner
            //             where (p.Username.Contains(search) || p.Email.Contains(search))
            //             orderby p.TGL_DAFTAR descending
            //             select p);
            string sSQLSelect = "";
            sSQLSelect += "SELECT * ";
            string sSQLCount = "";
            sSQLCount += "SELECT COUNT(partnerid) AS JUMLAH ";
            string sSQL2 = "";
            sSQL2 += "FROM partner ";
            if (search != "")
            {
                sSQL2 += " WHERE ( " + sSQLnama + " or " + sSQLemail + " or " + sSQLhp + " or " + sSQLtipe + " or " + sSQLref + " ) ";
            }

            var minimal_harus_ada_item_untuk_current_page = (page * 10) - 9;
            var totalCount = MoDbContext.Database.SqlQuery<getTotalCount>(sSQLCount + sSQL2).Single();
            if (minimal_harus_ada_item_untuk_current_page > totalCount.JUMLAH)
            {
                pagenumber = pagenumber - 1;
            }

            string sSQLSelect2 = "";
            sSQLSelect2 += "ORDER BY TGL_DAFTAR DESC, Username asc ";
            sSQLSelect2 += "OFFSET " + Convert.ToString(pagenumber * 10) + " ROWS ";
            sSQLSelect2 += "FETCH NEXT 10 ROWS ONLY ";

            var Listpartners = MoDbContext.Database.SqlQuery<mdlPartner>(sSQLSelect + sSQL2 + sSQLSelect2).ToList();

            IPagedList<mdlPartner> pageOrders = new StaticPagedList<mdlPartner>(Listpartners, pagenumber + 1, 10, totalCount.JUMLAH);
            return PartialView("TablePartnerPartial", pageOrders);
        }
        //end add by nurul 4/10/2019

        public ActionResult EditKomisi(int? partnerid)
        {
            //var vm = new PartnerViewModel()
            //{
            //    partner = MoDbContext.Partner.SingleOrDefault(m => m.PartnerId == partnerid),
            //};

            //ViewData["Editing"] = 1;

            //return View("PartnerMenu", vm);
            try
            {

                var vm = new PartnerViewModel()
                {
                    partner = MoDbContext.Partner.SingleOrDefault(m => m.PartnerId == partnerid),
                };

                return PartialView("DetailPartnerPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveKomisi(PartnerViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View("PartnerMenu", vm);
            }

            if (vm.partner.PartnerId == 0)
            {
                var partInDb = MoDbContext.Partner.SingleOrDefault(m => m.PartnerId == vm.partner.PartnerId);

                if (partInDb != null)
                {
                    ModelState.AddModelError("", @"Kode partner sudah terdaftar!");
                    return View("PartnerMenu", vm);
                }

                MoDbContext.Partner.Add(vm.partner);
            }
            else
            {
                var partInDb = MoDbContext.Partner.Single(m => m.PartnerId == vm.partner.PartnerId);
                //if (partInDb.Status != vm.partner.Status)
                //{
                //    //Task<ActionResult> x = ChangeStatusPartner(Convert.ToString(vm.partner.PartnerId));
                //}
                partInDb.komisi_subscribe = vm.partner.komisi_subscribe;
                partInDb.komisi_subscribe_gold = vm.partner.komisi_subscribe_gold;
                partInDb.komisi_support = vm.partner.komisi_support;
            }

            MoDbContext.SaveChanges();
            ModelState.Clear();

            var dm = new PartnerViewModel()
            {
                partner= MoDbContext.Partner.SingleOrDefault(m => m.PartnerId == vm.partner.PartnerId)
            };

            return PartialView("DetailPartnerPartial", dm);
        }
        // =============================================== Menu Partner (END)
        //end add by nurul 15/2/2019

        // =============================================== Menu-menu pada halaman admin (END)

        public async Task<ActionResult> ChangeStatusPartner(PartnerViewModel dataVm)
        {
            var partnerId = Convert.ToInt64(dataVm.partner.PartnerId);
            var partnerInDb = MoDbContext.Partner.Single(u => u.PartnerId == partnerId);
            if (partnerInDb.Status == dataVm.partner.Status)
            {
                var Tempvm = new PartnerViewModel()
                {
                    partner = MoDbContext.Partner.SingleOrDefault(a => a.PartnerId == partnerId),
                };

                return PartialView("DetailPartnerPartial", Tempvm);
            }
            if (partnerInDb.Status && !partnerInDb.StatusSetuju)
                partnerInDb.StatusSetuju = true;
            else
                partnerInDb.StatusSetuju = false;

            partnerInDb.Status = !partnerInDb.Status;

            MoDbContext.SaveChanges();

            if (partnerInDb.Status)
            {
                var email = new MailAddress(partnerInDb.Email);
                var message = new MailMessage();
                message.To.Add(email);
                message.From = new MailAddress("csmasteronline@gmail.com");
                message.Subject = "Pendaftaran Partner MasterOnline berhasil!";
                message.Body = System.IO.File.ReadAllText(Server.MapPath("~/Content/admin/AffiliateTerms.html"))
                    .Replace("LINKPERSETUJUAN", Request.Url.GetLeftPart(UriPartial.Authority) + Url.Action("PartnerApproval", "Account", new { partnerId }));
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

            ViewData["SuccessMessage"] = $"Partner {partnerInDb.Username} berhasil diubah statusnya.";
            //var vm = new PartnerViewModel()
            //{
            //    ListPartner = MoDbContext.Partner.ToList()
            //};

            //return View("PartnerMenu", vm);
            return new EmptyResult();
        }

        public ActionResult GeneratorSqlMenu()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetResultQuery(DataForQuery data)
        {
            string resultQuery = "";
            string insertDataQuery = "";

            if (data.MigrationHistoryInsertQuery != null)
            {
                insertDataQuery = data.MigrationHistoryInsertQuery
                    .Replace("[dbo]", "['+ @db_name +'].")
                    .Replace("(N'", "(N''")
                    .Replace("',", "'',")
                    .Replace(", N'", ", N''")
                    .Replace("')", "'')");
                insertDataQuery = $"EXEC('{insertDataQuery}') \n";
            }

            string addColumnQuery = "";

            if (data.AdditionalQuery != null)
            {
                addColumnQuery = data.AdditionalQuery
                    .Replace("[dbo]", "['+ @db_name +'].");
                addColumnQuery = $"EXEC('{addColumnQuery}') \n";
            }

            resultQuery = "DECLARE @db_name NVARCHAR (MAX) \n" +
                          "DECLARE c_db_names CURSOR FOR \n" +
                          "SELECT name FROM sys.databases \n" +
                          "WHERE name NOT IN('master', 'tempdb', 'model', 'msdb', 'activity', 'ReportServer$SQLEXPRESS', 'mo', " +
                          "'ReportServer$SQLEXPRESSTempDB', 'ReportServer', 'ReportServerTempDB', 'SCREEN_ACTIVITY', 'REPORTSI', 'REPORTST', 'erasoft', 'AP_NET', 'AR_NET', " +
                          "'MD_NET', 'SI_NET', 'ST_NET', 'SCREEN-NET2', 'REPORTAP', 'REPORTAR', 'REPORTMD') \n" +
                          "OPEN c_db_names \n" +
                          "FETCH c_db_names INTO @db_name \n" +
                          "WHILE @@Fetch_Status = 0 \n" +
                          "BEGIN \n" +
                          insertDataQuery +
                          addColumnQuery +
                          "FETCH c_db_names INTO @db_name \n" +
                          "END \n" +
                          "CLOSE c_db_names \n" +
                          "DEALLOCATE c_db_names";

            return Json(resultQuery, JsonRequestBehavior.AllowGet);
        }

        // =============================================== Dashboard (START)

        [Route("admin/home")]
        [SessionAdminCheck]
        public ActionResult DashboardAdmin()
        {
            var x = MoDbContext.AktivitasSubscription.ToList();
            var fromDate = new DateTime();
            var toDate = new DateTime();
            var getMonth = new Int32();
            List<String> cekThree = new List<String>();
            List<String> cekTwelve = new List<String>();
            foreach (var item in x)
            {
                if (item.DrTGL != null && item.SdTGL != null)
                {
                    fromDate = Convert.ToDateTime(item.DrTGL);
                    toDate = Convert.ToDateTime(item.SdTGL);
                    //date = ((TimeSpan)(toDate - fromDate)).Days;
                    getMonth = GetMonthDifference(fromDate, toDate);
                    if (getMonth == 3)
                    {
                        cekThree.Add(item.Account);
                    }
                    else if (getMonth == 12)
                    {
                        cekTwelve.Add(item.Account);
                    }
                }
            }
            var date = DateTime.Today.AddMonths(-1);
            var Sale = MoDbContext.AktivitasSubscription.Where(a => a.TanggalBayar >= date && a.TanggalBayar <= DateTime.Today).ToList();
            double lengthSum = Sale.Select(a => a.Nilai).Sum();
            //add by nurul 24/4/2019
            //var dateCS = DateTime.Today.AddMonths(1);
            var accCS = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION > DateTime.Today).ToList();
            var accex = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION <= DateTime.Today).ToList();
            //end add by nurul 24/4/2019
            var vm = new DashboardAdminViewModel()
            {
                ListAccount = MoDbContext.Account.ToList(),
                ListSales = Sale,
                Three = cekThree.Count(),
                Twelve = cekTwelve.Count(),
                Bayar = lengthSum,
                //add by nurul 24/4/2019
                ListAccountCS = accCS,
                ListAccounteX = accex,
                //end add by nurul 24/4/2019
            };
            return View(vm);
        }

        public static int GetMonthDifference(DateTime fromDate, DateTime toDate)
        {
            int monthsApart = 12 * (fromDate.Year - toDate.Year) + fromDate.Month - toDate.Month;
            return Math.Abs(monthsApart);
        }

        [SessionAdminCheck]
        public ActionResult RefreshDashboard(string param)
        {
            string dr = (param.Split(';')[param.Split(';').Length - 2]);
            string sd = (param.Split(';')[param.Split(';').Length - 1]);
            string tgl1 = (dr.Split('/')[dr.Split('/').Length - 3]);
            string bln1 = (dr.Split('/')[dr.Split('/').Length - 2]);
            string thn1 = (dr.Split('/')[dr.Split('/').Length - 1]);
            string drtanggal = tgl1 + '/' + bln1 + '/' + thn1;
            string tgl2 = (sd.Split('/')[sd.Split('/').Length - 3]);
            string bln2 = (sd.Split('/')[sd.Split('/').Length - 2]);
            string thn2 = (sd.Split('/')[sd.Split('/').Length - 1]);
            string sdtanggal = tgl2 + '/' + bln2 + '/' + thn2;
            var drTgl = DateTime.ParseExact(drtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var sdTgl = DateTime.ParseExact(sdtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

            var x = MoDbContext.AktivitasSubscription.ToList();
            var fromDate = new DateTime();
            var toDate = new DateTime();
            var getMonth = new Int32();
            List<String> cekThree = new List<String>();
            List<String> cekTwelve = new List<String>();
            foreach (var item in x)
            {
                if (item.DrTGL != null && item.SdTGL != null)
                {
                    fromDate = Convert.ToDateTime(item.DrTGL);
                    toDate = Convert.ToDateTime(item.SdTGL);
                    getMonth = GetMonthDifference(fromDate, toDate);
                    if (getMonth == 3)
                    {
                        cekThree.Add(item.Account);
                    }
                    else if (getMonth == 12)
                    {
                        cekTwelve.Add(item.Account);
                    }
                }
            }
            var Sale = MoDbContext.AktivitasSubscription.Where(b => b.TanggalBayar >= drTgl && b.TanggalBayar <= sdTgl).ToList();
            double lengthSum = Sale.Select(a => a.Nilai).Sum();
            var vm = new DashboardAdminViewModel()
            {
                ListAccount = MoDbContext.Account.ToList(),
                ListSales = Sale,
                Three = cekThree.Count(),
                Twelve = cekTwelve.Count(),
                Bayar = lengthSum
            };
            return PartialView("TableDashboard", vm);
        }

        //add by nurul 24/4/2019
        [SessionAdminCheck]
        public ActionResult RefreshDashboardCS(string param)
        {
            var x = MoDbContext.AktivitasSubscription.ToList();
            var fromDate = new DateTime();
            var toDate = new DateTime();
            var getMonth = new Int32();
            List<String> cekThree = new List<String>();
            List<String> cekTwelve = new List<String>();
            foreach (var item in x)
            {
                if (item.DrTGL != null && item.SdTGL != null)
                {
                    fromDate = Convert.ToDateTime(item.DrTGL);
                    toDate = Convert.ToDateTime(item.SdTGL);
                    getMonth = GetMonthDifference(fromDate, toDate);
                    if (getMonth == 3)
                    {
                        cekThree.Add(item.Account);
                    }
                    else if (getMonth == 12)
                    {
                        cekTwelve.Add(item.Account);
                    }
                }
            }

            string tgl1 = (param.Split('/')[param.Split('/').Length - 3]);
            string bln1 = (param.Split('/')[param.Split('/').Length - 2]);
            string thn1 = (param.Split('/')[param.Split('/').Length - 1]);
            string drtanggal = tgl1 + '/' + bln1 + '/' + thn1;

            var drTgl = DateTime.ParseExact(drtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

            var accCS = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION > drTgl).ToList();
            var accex = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION <= drTgl).ToList();
            var vm = new DashboardAdminViewModel()
            {
                ListAccount = MoDbContext.Account.ToList(),
                Three = cekThree.Count(),
                Twelve = cekTwelve.Count(),
                ListAccountCS = accCS,
                ListAccounteX = accex,
            };
            return PartialView("TableDashboardCS", vm);
        }
        //end add by nurul 24/4/2019

        // =============================================== Dashboard (END)

        //====================================================================    BAGIAN ADMINCS   ===========================================================================================================

        // =============================================== Bagian Account (START)
        [Route("adminCS/manage/account")]
        [SessionAdminCheck]
        public ActionResult AccMenu()
        {
            var vm = new MenuAccount()
            {
                ListAccount = MoDbContext.Account.OrderByDescending(a => a.TGL_DAFTAR).ToList(),
                ListPartner = MoDbContext.Partner.ToList()
            };
            return View(vm);
        }

        public ActionResult RefreshAccMenu()
        {
            var vm = new MenuAccount()
            {
                ListAccount = MoDbContext.Account.OrderByDescending(a => a.TGL_DAFTAR).ToList(),
                ListPartner = MoDbContext.Partner.ToList()
            };
            return PartialView("TableAcc", vm);
        }

        [SessionAdminCheck]
        public ActionResult AccMenuAktif()
        {
            var vm = new MenuAccount()
            {
                ListAccount = MoDbContext.Account.Where(a => a.Status == true).ToList(),
                ListPartner = MoDbContext.Partner.ToList()
            };
            return PartialView("TableAccAktif", vm);
        }
        [SessionAdminCheck]
        public ActionResult AccMenuNonaktif(string param)
        {
            var vm = new MenuAccount()
            {
                ListAccount = MoDbContext.Account.Where(a => a.Status == false).ToList(),
                ListPartner = MoDbContext.Partner.ToList()
            };
            return PartialView("TableAccNonaktif", vm);
        }

        [SessionAdminCheck]
        public ActionResult AccMenuWillExpired(string param)
        {
            string dr = (param.Split(';')[param.Split(';').Length - 2]);
            string sd = (param.Split(';')[param.Split(';').Length - 1]);
            string tgl1 = (dr.Split('/')[dr.Split('/').Length - 3]);
            string bln1 = (dr.Split('/')[dr.Split('/').Length - 2]);
            string thn1 = (dr.Split('/')[dr.Split('/').Length - 1]);
            string drtanggal = tgl1 + '/' + bln1 + '/' + thn1;
            string tgl2 = (sd.Split('/')[sd.Split('/').Length - 3]);
            string bln2 = (sd.Split('/')[sd.Split('/').Length - 2]);
            string thn2 = (sd.Split('/')[sd.Split('/').Length - 1]);
            string sdtanggal = tgl2 + '/' + bln2 + '/' + thn2;
            var drTgl = DateTime.ParseExact(drtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var sdTgl = DateTime.ParseExact(sdtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var vm = new MenuAccount()
            {
                ListAccount = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION >= drTgl && a.TGL_SUBSCRIPTION <= sdTgl && a.Status == true).ToList(),
                ListPartner = MoDbContext.Partner.ToList()
            };
            return PartialView("TableAccWillExpired", vm);
        }
        [SessionAdminCheck]
        public ActionResult AccMenuExpired(string param)
        {
            string tgl1 = (param.Split('/')[param.Split('/').Length - 3]);
            string bln1 = (param.Split('/')[param.Split('/').Length - 2]);
            string thn1 = (param.Split('/')[param.Split('/').Length - 1]);
            string tanggal = tgl1 + '/' + bln1 + '/' + thn1;
            var perTgl = DateTime.ParseExact(tanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var vm = new MenuAccount()
            {
                ListAccount = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION <= perTgl && a.Status == true).ToList(),
                ListPartner = MoDbContext.Partner.ToList()
            };
            return PartialView("TableAccExpired", vm);
        }
        [Route("adminCS/account/detail/{accId}")]
        public ActionResult AccDetail(int? accId)
        {
            var accInDb = MoDbContext.Account.SingleOrDefault(a => a.AccountId == accId);

            if (accInDb == null)
                return View("Error");

            var vm = new MenuAccount()
            {
                Account = accInDb
            };
            return PartialView("AccDetail", vm);
        }

        // Mengubah status akun utama
        //public async Task<ActionResult> ChangeStatusAccount(int? accId, string stat)
        public async Task<ActionResult> ChangeStatusAccount(MenuAccount dataVm)
        {
            bool statusAcc = dataVm.Account.Status;
            var accId = dataVm.Account.AccountId;
            var accInDb = MoDbContext.Account.Single(a => a.AccountId == accId);
            if (accInDb.Status == statusAcc)
            {
                var Tempvm = new MenuAccount()
                {
                    Account = MoDbContext.Account.SingleOrDefault(a => a.AccountId == accId),
                    ListPartner = MoDbContext.Partner.ToList()
                };

                return PartialView("FormAccountPartialNew", Tempvm);
            }
            accInDb.Status = !accInDb.Status;

            if (accInDb.Status == true && accInDb.DatabasePathErasoft == null || accInDb.Status == true && accInDb.DatabasePathErasoft == "")
            {
                string sql = "";
                var userId = Convert.ToString(accInDb.AccountId);

                accInDb.DatabasePathErasoft = "ERASOFT_" + userId;

                //remark by fauzi 29 Januari 2020
                //var path = Server.MapPath("~/Content/admin/");
                var path = "C:\\BAK_new_user\\";
                var pathRestore = "C:\\DB_user";
                sql = $"RESTORE DATABASE {accInDb.DatabasePathErasoft} FROM DISK = '{path + "ERASOFT_backup_for_new_account.bak"}'" +
                      $" WITH MOVE 'erasoft' TO '{pathRestore}\\{accInDb.DatabasePathErasoft}.mdf'," +
                      $" MOVE 'erasoft_log' TO '{pathRestore}\\{accInDb.DatabasePathErasoft}.ldf';";
#if AWS
                //add by fauzi 29 Januari 2020
                accInDb.DataSourcePath = "13.250.232.74\\SQLEXPRESS, 1433";
                SqlConnection con = new SqlConnection("Server=localhost;Initial Catalog=master;persist security info=True;" +
                                "user id=masteronline;password=M@ster123;");
#elif Debug_AWS
                //add by fauzi 29 Januari 2020
                accInDb.DataSourcePath = "13.250.232.74\\SQLEXPRESS, 1433";
                SqlConnection con = new SqlConnection("Server=13.250.232.74\\SQLEXPRESS,1433;Initial Catalog=master;persist security info=True;" +
                                                      "user id=masteronline;password=M@ster123;");
#else
                //add by fauzi 29 Januari 2020
                accInDb.DataSourcePath = "13.251.222.53\\SQLEXPRESS, 1433";
                SqlConnection con = new SqlConnection("Server=13.251.222.53\\SQLEXPRESS,1433;Initial Catalog=master;persist security info=True;" +
                                                      "user id=masteronline;password=M@ster123;");
#endif
                SqlCommand command = new SqlCommand(sql, con);

                con.Open();
                command.ExecuteNonQuery();
                con.Close();
                con.Dispose();

                ErasoftContext ErasoftDbContext = new ErasoftContext(accInDb.DataSourcePath, accInDb.DatabasePathErasoft);
                var dataPerusahaan = ErasoftDbContext.SIFSYS.FirstOrDefault();
                if (string.IsNullOrEmpty(dataPerusahaan.NAMA_PT))
                {
                    dataPerusahaan.NAMA_PT = accInDb.NamaTokoOnline;
                    ErasoftDbContext.SaveChanges();
                }
                //end add by Tri 20-09-2018, save nama toko ke SIFSYS


                //add by Tri, set free trials 14 hari
                if (accInDb.Status)
                {
                    if (accInDb.KODE_SUBSCRIPTION == "01")
                    {
                        accInDb.TGL_SUBSCRIPTION = DateTime.Today.AddDays(14);
                    }
                    accInDb.tgl_approve = DateTime.Today;
                }
                //end add by Tri, set free trials 14 hari

                ViewData["SuccessMessage"] = $"Akun {accInDb.Username} berhasil diubah statusnya dan dibuatkan database baru.";
                MoDbContext.SaveChanges();

                var email = new MailAddress(accInDb.Email);
                var originPassword = accInDb.Password;
                var nama = accInDb.Username;
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
                message.Body = string.Format(body, accInDb.Email, originPassword, nama);
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
            else
            {
                if (accInDb.Status == false)
                {
                    var listUserPerAcc = MoDbContext.User.Where(u => u.AccountId == accId).ToList();
                    foreach (var user in listUserPerAcc)
                    {
                        user.Status = false;
                    }
                }
                ViewData["SuccessMessage"] = $"Akun {accInDb.Username} berhasil dinonaktifkan.";
                MoDbContext.SaveChanges();
            }
            var vm = new MenuAccount()
            {
                Account = MoDbContext.Account.SingleOrDefault(a => a.AccountId == accId),
                ListPartner = MoDbContext.Partner.ToList()
            };

            return PartialView("FormAccPartial", vm);
        }
        public ActionResult EditAccNew(int? accountId)
        {
            var vm = new MenuAccount()
            {
                Account = MoDbContext.Account.SingleOrDefault(m => m.AccountId == accountId),
                ListSubs = MoDbContext.Subscription.ToList()
            };

            return PartialView("FormAccPartial", vm);
        }
        // =============================================== Bagian Account (END)

        // =============================================== Menu Partner
        [Route("adminCS/manage/partner")]
        [SessionAdminCheck]
        public ActionResult PartMenu()
        {
            var vm = new PartnerViewModel()
            {
                ListPartner = MoDbContext.Partner.ToList()
            };

            return View(vm);
        }

        public ActionResult RefreshTablePart(int? page, string search = "")
        {
            int pagenumber = (page ?? 1) - 1;
            ViewData["searchParam"] = search;
            ViewData["LastPage"] = page;
            string[] getkata = search.Split(' ');
            string sSQLnama = "";
            string sSQLemail = "";
            string sSQLhp = "";
            string sSQLtipe = "";
            string sSQLref = "";
            if (getkata.Length > 0)
            {
                if (search != "")
                {
                    for (int i = 0; i < getkata.Length; i++)
                    {
                        if (getkata.Length == 1)
                        {
                            sSQLnama += "( Username like '%" + getkata[i] + "%' )";
                            sSQLemail += " ( Email like '%" + getkata[i] + "%' )";
                            sSQLhp += " ( NoHp like '%" + getkata[i] + "%' )";
                            sSQLtipe += " ( NamaTipe like '%" + getkata[i] + "%' )";
                            sSQLref += " ( isnull(KodeRefPilihan,'') like '%" + getkata[i] + "%' )";
                        }
                        else
                        {
                            if (getkata[i] == getkata.First())
                            {
                                sSQLnama += " ( Username like '%" + getkata[i] + "%'";
                                sSQLemail += " ( Email like '%" + getkata[i] + "%'";
                                sSQLhp += " ( NoHp like '%" + getkata[i] + "%'";
                                sSQLtipe += "( NamaTipe like '%" + getkata[i] + "%'";
                                sSQLref += " ( isnull(KodeRefPilihan,'') like '%" + getkata[i] + "%' ";
                            }
                            else if (getkata[i] == getkata.Last())
                            {
                                sSQLnama += " and Username like '%" + getkata[i] + "%' )";
                                sSQLemail += " and Email like '%" + getkata[i] + "%' )";
                                sSQLhp += " and NoHp like '%" + getkata[i] + "%' )";
                                sSQLtipe += " and NamaTipe like '%" + getkata[i] + "%' )";
                                sSQLref += " and isnull(KodeRefPilihan,'') like '%" + getkata[i] + "%' )";
                            }
                            else
                            {
                                sSQLnama += " and Username like '%" + getkata[i] + "%' ";
                                sSQLemail += " and Email like '%" + getkata[i] + "%' ";
                                sSQLhp += " and NoHp like '%" + getkata[i] + "%' ";
                                sSQLtipe += " and NamaTipe like '%" + getkata[i] + "%' ";
                                sSQLref += " and isnull(KodeRefPilihan,'') like '%" + getkata[i] + "%' ";
                            }
                        }
                    }
                }
            }
            //var partners = (from p in MoDbContext.Partner
            //             where (p.Username.Contains(search) || p.Email.Contains(search))
            //             orderby p.TGL_DAFTAR descending
            //             select p);
            string sSQLSelect = "";
            sSQLSelect += "SELECT * ";
            string sSQLCount = "";
            sSQLCount += "SELECT COUNT(partnerid) AS JUMLAH ";
            string sSQL2 = "";
            sSQL2 += "FROM partner ";
            if (search != "")
            {
                sSQL2 += " WHERE ( " + sSQLnama + " or " + sSQLemail + " or " + sSQLhp + " or " + sSQLtipe + " or " + sSQLref + " ) ";
            }

            var minimal_harus_ada_item_untuk_current_page = (page * 10) - 9;
            var totalCount = MoDbContext.Database.SqlQuery<getTotalCount>(sSQLCount + sSQL2).Single();
            if (minimal_harus_ada_item_untuk_current_page > totalCount.JUMLAH)
            {
                pagenumber = pagenumber - 1;
            }

            string sSQLSelect2 = "";
            sSQLSelect2 += "ORDER BY TGL_DAFTAR DESC, Username asc ";
            sSQLSelect2 += "OFFSET " + Convert.ToString(pagenumber * 10) + " ROWS ";
            sSQLSelect2 += "FETCH NEXT 10 ROWS ONLY ";

            var Listpartners = MoDbContext.Database.SqlQuery<mdlPartner>(sSQLSelect + sSQL2 + sSQLSelect2).ToList();

            IPagedList<mdlPartner> pageOrders = new StaticPagedList<mdlPartner>(Listpartners, pagenumber + 1, 10, totalCount.JUMLAH);
            return PartialView("TablePartPartial", pageOrders);
        }
        //end add by nurul 4/10/2019

        public async Task<ActionResult> ChangeStatusPart(PartnerViewModel dataVm)
        {
            var partnerId = Convert.ToInt64(dataVm.partner.PartnerId);
            var partnerInDb = MoDbContext.Partner.Single(u => u.PartnerId == partnerId);
            if (partnerInDb.Status == dataVm.partner.Status)
            {
                var Tempvm = new PartnerViewModel()
                {
                    partner = MoDbContext.Partner.SingleOrDefault(a => a.PartnerId == partnerId),
                };

                return PartialView("DetailPartPartial", Tempvm);
            }
            if (partnerInDb.Status && !partnerInDb.StatusSetuju)
                partnerInDb.StatusSetuju = true;
            else
                partnerInDb.StatusSetuju = false;

            partnerInDb.Status = !partnerInDb.Status;

            MoDbContext.SaveChanges();

            if (partnerInDb.Status)
            {
                var email = new MailAddress(partnerInDb.Email);
                var message = new MailMessage();
                message.To.Add(email);
                message.From = new MailAddress("csmasteronline@gmail.com");
                message.Subject = "Pendaftaran Partner MasterOnline berhasil!";
                message.Body = System.IO.File.ReadAllText(Server.MapPath("~/Content/admin/AffiliateTerms.html"))
                    .Replace("LINKPERSETUJUAN", Request.Url.GetLeftPart(UriPartial.Authority) + Url.Action("PartnerApproval", "Account", new { partnerId }));
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

            ViewData["SuccessMessage"] = $"Partner {partnerInDb.Username} berhasil diubah statusnya.";
            return new EmptyResult();
        }
        public ActionResult EditKomisiCS(int? partnerid)
        {
            try
            {

                var vm = new PartnerViewModel()
                {
                    partner = MoDbContext.Partner.SingleOrDefault(m => m.PartnerId == partnerid),
                };

                return PartialView("DetailPartPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveKomisiCS(PartnerViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View("PartMenu", vm);
            }

            if (vm.partner.PartnerId == 0)
            {
                var partInDb = MoDbContext.Partner.SingleOrDefault(m => m.PartnerId == vm.partner.PartnerId);

                if (partInDb != null)
                {
                    ModelState.AddModelError("", @"Kode partner sudah terdaftar!");
                    return View("PartMenu", vm);
                }

                MoDbContext.Partner.Add(vm.partner);
            }
            else
            {
                var partInDb = MoDbContext.Partner.Single(m => m.PartnerId == vm.partner.PartnerId);
                partInDb.komisi_subscribe = vm.partner.komisi_subscribe;
                partInDb.komisi_subscribe_gold = vm.partner.komisi_subscribe_gold;
                partInDb.komisi_support = vm.partner.komisi_support;
            }

            MoDbContext.SaveChanges();
            ModelState.Clear();

            var dm = new PartnerViewModel()
            {
                partner = MoDbContext.Partner.SingleOrDefault(m => m.PartnerId == vm.partner.PartnerId)
            };

            return PartialView("DetailPartnerPartial", dm);
        }
        // =============================================== Menu Partner (END)

        // =============================================== Dashboard (START)

        [Route("adminCS/home")]
        [SessionAdminCheck]
        public ActionResult DashboardAdm()
        {
            var x = MoDbContext.AktivitasSubscription.ToList();
            var fromDate = new DateTime();
            var toDate = new DateTime();
            var getMonth = new Int32();
            List<String> cekThree = new List<String>();
            List<String> cekTwelve = new List<String>();
            foreach (var item in x)
            {
                if (item.DrTGL != null && item.SdTGL != null)
                {
                    fromDate = Convert.ToDateTime(item.DrTGL);
                    toDate = Convert.ToDateTime(item.SdTGL);
                    getMonth = GetMonthDifference(fromDate, toDate);
                    if (getMonth == 3)
                    {
                        cekThree.Add(item.Account);
                    }
                    else if (getMonth == 12)
                    {
                        cekTwelve.Add(item.Account);
                    }
                }
            }
            var date = DateTime.Today.AddMonths(-1);
            var Sale = MoDbContext.AktivitasSubscription.Where(a => a.TanggalBayar >= date && a.TanggalBayar <= DateTime.Today).ToList();
            double lengthSum = Sale.Select(a => a.Nilai).Sum();
            //add by nurul 24/4/2019
            //var dateCS = DateTime.Today.AddMonths(1);
            var accCS = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION > DateTime.Today).ToList();
            var accex = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION <= DateTime.Today).ToList();
            //end add by nurul 24/4/2019
            var vm = new DashboardAdminViewModel()
            {
                ListAccount = MoDbContext.Account.ToList(),
                ListSales = Sale,
                Three = cekThree.Count(),
                Twelve = cekTwelve.Count(),
                Bayar = lengthSum,
                ListAccountCS = accCS,
                ListAccounteX = accex,
            };
            return View(vm);
        }

        [SessionAdminCheck]
        public ActionResult RefreshDashboardSalesCS(string param)
        {
            string dr = (param.Split(';')[param.Split(';').Length - 2]);
            string sd = (param.Split(';')[param.Split(';').Length - 1]);
            string tgl1 = (dr.Split('/')[dr.Split('/').Length - 3]);
            string bln1 = (dr.Split('/')[dr.Split('/').Length - 2]);
            string thn1 = (dr.Split('/')[dr.Split('/').Length - 1]);
            string drtanggal = tgl1 + '/' + bln1 + '/' + thn1;
            string tgl2 = (sd.Split('/')[sd.Split('/').Length - 3]);
            string bln2 = (sd.Split('/')[sd.Split('/').Length - 2]);
            string thn2 = (sd.Split('/')[sd.Split('/').Length - 1]);
            string sdtanggal = tgl2 + '/' + bln2 + '/' + thn2;
            var drTgl = DateTime.ParseExact(drtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var sdTgl = DateTime.ParseExact(sdtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

            var x = MoDbContext.AktivitasSubscription.ToList();
            var fromDate = new DateTime();
            var toDate = new DateTime();
            var getMonth = new Int32();
            List<String> cekThree = new List<String>();
            List<String> cekTwelve = new List<String>();
            foreach (var item in x)
            {
                if (item.DrTGL != null && item.SdTGL != null)
                {
                    fromDate = Convert.ToDateTime(item.DrTGL);
                    toDate = Convert.ToDateTime(item.SdTGL);
                    getMonth = GetMonthDifference(fromDate, toDate);
                    if (getMonth == 3)
                    {
                        cekThree.Add(item.Account);
                    }
                    else if (getMonth == 12)
                    {
                        cekTwelve.Add(item.Account);
                    }
                }
            }
            var Sale = MoDbContext.AktivitasSubscription.Where(b => b.TanggalBayar >= drTgl && b.TanggalBayar <= sdTgl).ToList();
            double lengthSum = Sale.Select(a => a.Nilai).Sum();
            var vm = new DashboardAdminViewModel()
            {
                ListAccount = MoDbContext.Account.ToList(),
                ListSales = Sale,
                Three = cekThree.Count(),
                Twelve = cekTwelve.Count(),
                Bayar = lengthSum
            };
            return PartialView("TableDashboardAdm", vm);
        }

        //add by nurul 24/4/2019
        [SessionAdminCheck]
        public ActionResult RefreshDashboardCust(string param)
        {
            var x = MoDbContext.AktivitasSubscription.ToList();
            var fromDate = new DateTime();
            var toDate = new DateTime();
            var getMonth = new Int32();
            List<String> cekThree = new List<String>();
            List<String> cekTwelve = new List<String>();
            foreach (var item in x)
            {
                if (item.DrTGL != null && item.SdTGL != null)
                {
                    fromDate = Convert.ToDateTime(item.DrTGL);
                    toDate = Convert.ToDateTime(item.SdTGL);
                    getMonth = GetMonthDifference(fromDate, toDate);
                    if (getMonth == 3)
                    {
                        cekThree.Add(item.Account);
                    }
                    else if (getMonth == 12)
                    {
                        cekTwelve.Add(item.Account);
                    }
                }
            }

            string tgl1 = (param.Split('/')[param.Split('/').Length - 3]);
            string bln1 = (param.Split('/')[param.Split('/').Length - 2]);
            string thn1 = (param.Split('/')[param.Split('/').Length - 1]);
            string drtanggal = tgl1 + '/' + bln1 + '/' + thn1;

            var drTgl = DateTime.ParseExact(drtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

            var accCS = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION > drTgl).ToList();
            var accex = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION <= drTgl).ToList();
            var vm = new DashboardAdminViewModel()
            {
                ListAccount = MoDbContext.Account.ToList(),
                Three = cekThree.Count(),
                Twelve = cekTwelve.Count(),
                ListAccountCS = accCS,
                ListAccounteX = accex,
            };
            return PartialView("TableDashboardAdmCS", vm);
        }
        //end add by nurul 24/4/2019

        // =============================================== Dashboard (END)

        // =============================================== Bagian History Pembayaran (START)

        public ActionResult AktivitasSubs()
        {
            var vm = new SubsViewModel()
            {
                ListAktivitasSubs = MoDbContext.AktivitasSubscription.ToList(),
                ListSubs = MoDbContext.Subscription.ToList(),
                ListAccount = MoDbContext.Account.ToList()
            };

            return View(vm);
        }

        public ActionResult EditPaymentCS(int? paymentId)
        {
            var vm = new SubsViewModel()
            {
                Payment = MoDbContext.AktivitasSubscription.SingleOrDefault(m => m.RecNum == paymentId),
            };

            return PartialView("FormHistoryPembPartial", vm);
        }

        public ActionResult DeletePaymentCS(int? paymentId)
        {
            var subsVm = new SubsViewModel()
            {
                Payment = MoDbContext.AktivitasSubscription.Single(m => m.RecNum == paymentId),
                ListAktivitasSubs = MoDbContext.AktivitasSubscription.ToList()
            };

            MoDbContext.AktivitasSubscription.Remove(subsVm.Payment);
            MoDbContext.SaveChanges();

            return RedirectToAction("AktivitasSubs");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SavePaymentCS(SubsViewModel vm)
        {
            var newPayment = false;
            if (!ModelState.IsValid)
            {
                return PartialView("FormHistoryPembPartial", vm);
            }

            if (vm.Payment.RecNum == null)
            {

                if (vm.Payment.TipePembayaran == null)
                {
                    vm.Payment.TipePembayaran = "Manual_Transfer";
                }
                MoDbContext.AktivitasSubscription.Add(vm.Payment);
                newPayment = true;
            }
            else
            {
                var subsInDb = MoDbContext.AktivitasSubscription.Single(m => m.RecNum == vm.Payment.RecNum);
                subsInDb.Email = vm.Payment.Email;
                subsInDb.Account = vm.Payment.Account;
                subsInDb.TipeSubs = vm.Payment.TipeSubs;
                subsInDb.TanggalBayar = vm.Payment.TanggalBayar;
                subsInDb.Nilai = vm.Payment.Nilai;
                subsInDb.TipePembayaran = vm.Payment.TipePembayaran;
                subsInDb.DrTGL = vm.Payment.DrTGL;
                subsInDb.SdTGL = vm.Payment.SdTGL;
                subsInDb.jumlahUser = vm.Payment.jumlahUser;
            }
            //change by Tri, 25 Nov 2019
            //var akun = MoDbContext.Account.Single(m => m.Email == vm.Payment.Email && m.Username == vm.Payment.Account);
            var akun = MoDbContext.Account.Single(m => m.Email == vm.Payment.Email);
            //end change by Tri, 25 Nov 2019
            akun.KODE_SUBSCRIPTION = vm.Payment.TipeSubs;
            akun.jumlahUser = vm.Payment.jumlahUser;
            akun.TGL_SUBSCRIPTION = vm.Payment.SdTGL;
            MoDbContext.SaveChanges();
            ModelState.Clear();
            if (newPayment == true)
            {
                var cekPayment = MoDbContext.AktivitasSubscription.Where(a => a.Email == vm.Payment.Email && a.TanggalBayar == vm.Payment.TanggalBayar && a.Nilai == vm.Payment.Nilai).ToList();
                if (cekPayment != null)
                {
                    await SendInvoiceCS(Convert.ToString(cekPayment.Single().RecNum), "0");
                }
            }
            return PartialView("FormHistoryPembPartial", vm);
        }
        [HttpGet]
        public ActionResult GetAccountCS()
        {
            var account = MoDbContext.Account.ToList();

            return Json(account, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> SendInvoiceCS(string aktSubID, string btnKirim)
        {
            try
            {
                var ambilUlangAktSub = new AktivitasSubscription();
                bool succes = false;
                var aktSubId = Convert.ToInt32(aktSubID);
                var aktSub = MoDbContext.AktivitasSubscription.Single(u => u.RecNum == aktSubId);
                if (aktSub.Invoice_No == null || aktSub.Invoice_No.Substring(3, 4) == "2019") //kalo Invoice_No null/masih format lama
                {
                    var sub = MoDbContext.Subscription.Single(u => u.KODE == aktSub.TipeSubs).KETERANGAN;

                    var listAktSubInDb = MoDbContext.AktivitasSubscription.OrderBy(p => p.RecNum).ToList();
                    var cekListSudahAdaNoInv = MoDbContext.AktivitasSubscription.Where(a => a.Invoice_No != null && a.Invoice_No.Substring(3, 4) != "2019").OrderBy(p => p.RecNum).ToList();
                    var digitAkhir = "";
                    var noInv = "";
                    if (cekListSudahAdaNoInv.Count() == 0)
                    {
                        digitAkhir = "0001";
                        noInv = $"MO/{DateTime.Now.Year.ToString().Substring(2, 2)}/{digitAkhir}";
                    }
                    else
                    {
                        var NoUrut = Convert.ToInt32(listAktSubInDb.Where(a => a.Invoice_No != null && a.Invoice_No.Substring(3, 4) != "2019").OrderByDescending(a => a.Invoice_No).FirstOrDefault().Invoice_No.Substring(6, 4));
                        NoUrut++;

                        digitAkhir = NoUrut.ToString().PadLeft(4, '0');
                        noInv = $"MO/{DateTime.Now.Year.ToString().Substring(2, 2)}/{digitAkhir}";
                    }

                    if (digitAkhir != "" && noInv != "")
                    {
                        aktSub.Invoice_No = noInv;
                        aktSub.tgl_email = DateTime.Today;
                    }

                    var email = new MailAddress(aktSub.Email);
                    //add
                    var today = DateTime.Today.ToString("dd/MM/yyyy");
                    var nama = aktSub.Account;
                    var tglBayar = aktSub.TanggalBayar?.ToString("dd/MM/yyyy");
                    var subs = sub;
                    var nilai = $"Rp. {String.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:N}", aktSub.Nilai)}";
                    var jmlUser = aktSub.jumlahUser.ToString();
                    var drTgl = aktSub.DrTGL?.ToString("dd/MM/yyyy");
                    var sdTgl = aktSub.SdTGL?.ToString("dd/MM/yyyy");
                    var inv = noInv;
                    //end add

                    var message = new MailMessage();
                    message.To.Add(email);
                    message.From = new MailAddress("csmasteronline@gmail.com");
                    message.Subject = "Email Payment Subscription";
                    message.Body = System.IO.File.ReadAllText(Server.MapPath("~/Content/admin/PaymentSubscription.html"))
                        .Replace("EMAIL", Convert.ToString(email))
                        .Replace("TODAY", today)
                        .Replace("NAMA", nama)
                        .Replace("TGLBAYAR", tglBayar)
                        .Replace("SUBS", subs)
                        .Replace("NILAI", nilai)
                        .Replace("JMLUSER", jmlUser)
                        .Replace("DRTGL", drTgl)
                        .Replace("SDTGL", sdTgl)
                        .Replace("NOINV", inv);
                    message.IsBodyHtml = true;

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
                        try
                        {
                            await smtp.SendMailAsync(message);
                            succes = true;
                            MoDbContext.SaveChanges();
                            ModelState.Clear();
                        }
                        catch (Exception ex)
                        {
                            ViewData["SuccessMessage"] = $"Pembayaran {ambilUlangAktSub.Account} gagal kirim email.";
                        }
                    }
                }
                ambilUlangAktSub = MoDbContext.AktivitasSubscription.Single(u => u.RecNum == aktSubId);
                if (btnKirim == "1" && succes == true)
                {
                    ViewData["SuccessMessage"] = $"Pembayaran {ambilUlangAktSub.Account} berhasil dikirim email.";

                    return RedirectToAction("AktivitasSubs");
                }
                else if (succes == false)
                {
                    ViewData["SuccessMessage"] = $"Pembayaran {ambilUlangAktSub.Account} gagal kirim email.";
                    return RedirectToAction("AktivitasSubs");
                }
                return new EmptyResult();
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }
        }

        // =============================================== Bagian History Pembayaran (END)



        // status hangfire server

        [Route("admin/manage/hangfirestatus")]
        [SessionAdminCheck]
        public ActionResult HangfireServerStatus()
        {
            return View();
        }
        [SessionAdminCheck]
        public ActionResult RefreshHangfireServerStatus(int? page = 1, string search = "")
        {
            try
            {
                int pagenumber = (page ?? 1) - 1;
                ViewData["LastPage"] = page;
                ViewData["searchParam"] = search;
                var accountInDb = (from a in MoDbContext.Account
                                   where (a.Email.Contains(search) || a.Username.Contains(search) || a.NamaTokoOnline.Contains(search) || a.DataSourcePath.Contains(search))
                                   orderby a.LAST_LOGIN_DATE descending
                                   select a);

                var accountinDbPaging = accountInDb.Skip(pagenumber * 5).Take(5).ToList();

                var pageContent = new List<HANGFIRE_SERVER_STATUS>();
                foreach (var item in accountinDbPaging)
                {
                    if (item.DatabasePathErasoft != null && item.DatabasePathErasoft != "")
                    {
                        var EDB = new DatabaseSQL(item.DatabasePathErasoft);

                        string EDBConnID = EDB.GetConnectionString("ConnID");
                        var sqlStorage = new SqlServerStorage(EDBConnID);

                        var monitoringApi = sqlStorage.GetMonitoringApi();
                        var serverList = monitoringApi.Servers();

                        var lastHeartbeat = new DateTime?();
                        var getfirstserver = serverList.FirstOrDefault();
                        if (getfirstserver != null)
                        {
                            lastHeartbeat = getfirstserver.Heartbeat;
                        }
                        var data = new HANGFIRE_SERVER_STATUS()
                        {
                            Email = item.Email,
                            LAST_LOGIN_DATE = item.LAST_LOGIN_DATE,
                            Username = item.Username,
                            DatabasePathErasoft = item.DatabasePathErasoft,
                            DatabaseSourceErasoft = item.DataSourcePath,
                            NamaTokoOnline = item.NamaTokoOnline,
                            TGL_SUBSCRIPTION = item.TGL_SUBSCRIPTION,
                            HangfireServerCount = serverList.Count(),
                            PesananJobEnqueued = 0,
                            CreateProductJobEnqueued = 0,
                            StokJobEnqueued = 0,
                            LAST_HEARTBEAT = lastHeartbeat
                        };

                        string sSQL = "select 'Stok' as tipe,count(*) jumlah from hangfire.job (nolock) where statename='Enqueued' and InvocationData like '%StokControllerJob%' " + System.Environment.NewLine;
                        sSQL += "union all" + System.Environment.NewLine;
                        sSQL += "select 'Order' as tipe,count(*) jumlah from hangfire.job (nolock) where statename='Enqueued' and InvocationData like '%Order%'" + System.Environment.NewLine;
                        sSQL += "union all" + System.Environment.NewLine;
                        sSQL += "select 'Product' as tipe,count(*) jumlah from hangfire.job (nolock) where statename='Enqueued' and InvocationData like '%Product%'" + System.Environment.NewLine;
                        var dsCekQueue = EDB.GetDataSet("sCon", "QUEUE_COUNT", sSQL);
                        if (dsCekQueue.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsCekQueue.Tables[0].Rows.Count; i++)
                            {
                                switch (Convert.ToString(dsCekQueue.Tables[0].Rows[i]["tipe"]))
                                {
                                    case "Stok":
                                        data.StokJobEnqueued = Convert.ToInt32(dsCekQueue.Tables[0].Rows[i]["jumlah"]);
                                        break;
                                    case "Order":
                                        data.PesananJobEnqueued = Convert.ToInt32(dsCekQueue.Tables[0].Rows[i]["jumlah"]);
                                        break;
                                    case "Product":
                                        data.CreateProductJobEnqueued = Convert.ToInt32(dsCekQueue.Tables[0].Rows[i]["jumlah"]);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }

                        pageContent.Add(data);
                    }
                    else
                    {
                        var data = new HANGFIRE_SERVER_STATUS()
                        {
                            Email = item.Email,
                            LAST_LOGIN_DATE = item.LAST_LOGIN_DATE,
                            Username = item.Username,
                            DatabasePathErasoft = item.DatabasePathErasoft,
                            DatabaseSourceErasoft = item.DataSourcePath,
                            NamaTokoOnline = item.NamaTokoOnline,
                            TGL_SUBSCRIPTION = item.TGL_SUBSCRIPTION,
                            HangfireServerCount = 0,
                            PesananJobEnqueued = 0,
                            CreateProductJobEnqueued = 0,
                            StokJobEnqueued = 0,
                            LAST_HEARTBEAT = null
                        };
                        pageContent.Add(data);
                    }
                }

                var totalAccountInDb = accountInDb.Count();
                IPagedList<HANGFIRE_SERVER_STATUS> pageOrders = new StaticPagedList<HANGFIRE_SERVER_STATUS>(pageContent, pagenumber + 1, 5, totalAccountInDb);

                return PartialView("TableHangfireServerStatus", pageOrders);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = "Terjadi Kesalahan, mohon hubungi support." }, JsonRequestBehavior.AllowGet);
            }

        }

        [SessionAdminCheck]
        public ActionResult DeactivateRecentActiveUsers()
        {
            var lastYear = DateTime.UtcNow.AddYears(-1);
            var last2Week = DateTime.UtcNow.AddHours(7).AddDays(-14);
            var datenow = DateTime.UtcNow.AddHours(7);

            var accountInDb = (from a in MoDbContext.Account
                               where
                               (a.LAST_LOGIN_DATE ?? lastYear) >= last2Week
                               &&
                               (a.TGL_SUBSCRIPTION ?? lastYear) >= datenow
                               orderby a.LAST_LOGIN_DATE descending
                               select a).ToList();
            foreach (var item in accountInDb)
            {
                AdminStopHangfireServer(item.DatabasePathErasoft);
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }

        [SessionAdminCheck]
        public ActionResult AdminStopHangfireServer(string nourut = "")
        {
            try
            {
                var vsuccess = false;
                var vstatus = "";
                var EDB = new DatabaseSQL(nourut);

                string EDBConnID = EDB.GetConnectionString("ConnID");
                var sqlStorage = new SqlServerStorage(EDBConnID);

                var monitoringApi = sqlStorage.GetMonitoringApi();
                var serverList = monitoringApi.Servers();

                if (serverList.Count() > 0)
                {
                    foreach (var server in serverList)
                    {
                        //var serverConnection = sqlStorage.GetConnection().RemoveServer(server.Name);
                        sqlStorage.GetMonitoringApi().Servers().Remove(server);
                        sqlStorage.GetMonitoringApi().Servers().Clear();
                        sqlStorage.GetConnection().RemoveServer(server.Name);
                        sqlStorage.GetConnection().Dispose();

                        vsuccess = true;
                        vstatus = "Hangfire berhasil dinonaktifkan.";
                    }
                }
                return Json(new { success = vsuccess, status = vstatus }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = ex.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [SessionAdminCheck]
        public ActionResult ActivateRecentActiveUsers()
        {

            var lastYear = DateTime.UtcNow.AddYears(-1);
            var last2Week = DateTime.UtcNow.AddHours(7).AddDays(-14);
            var datenow = DateTime.UtcNow.AddHours(7);

            var accountInDb = (from a in MoDbContext.Account
                               where
                               (a.LAST_LOGIN_DATE ?? lastYear) >= last2Week
                               &&
                               (a.TGL_SUBSCRIPTION ?? lastYear) >= datenow
                               orderby a.LAST_LOGIN_DATE descending
                               select a).ToList();
            foreach (var item in accountInDb)
            {
                AdminStartHangfireServer(item.DataSourcePath, item.DatabasePathErasoft);
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }

        [SessionAdminCheck]
        public ActionResult AdminStartHangfireServer(string dbsource, string nourut = "", string timer = "")
        {
            int interval = 30;
            var vstatus = "";
            var vsuccess = false;
            try
            {
                interval = Convert.ToInt32(timer);
                var lastYear = DateTime.UtcNow.AddYears(-1);
                var datenow = DateTime.UtcNow.AddHours(7);

                var accountInDb = (from a in MoDbContext.Account
                                   where
                                   (a.DatabasePathErasoft == nourut)
                                   &&
                                   (a.TGL_SUBSCRIPTION ?? lastYear) >= datenow
                                   orderby a.LAST_LOGIN_DATE descending
                                   select a).ToList();
                if (accountInDb.Count() > 0)
                {
                    foreach (var item in accountInDb)
                    {
                        var EDB = new DatabaseSQL(nourut);

                        string EDBConnID = EDB.GetConnectionString("ConnID");
                        var sqlStorage = new SqlServerStorage(EDBConnID);

                        var monitoringApi = sqlStorage.GetMonitoringApi();
                        var serverList = monitoringApi.Servers();

                        if (serverList.Count() == 0)
                        {
                            var optionsStatusResiServer = new BackgroundJobServerOptions
                            {
                                ServerName = "StatusResiPesanan",
                                Queues = new[] { "1_manage_pesanan" },
                                WorkerCount = 1,

                            };
                            var newStatusResiServer = new BackgroundJobServer(optionsStatusResiServer, sqlStorage);

                            var options = new BackgroundJobServerOptions
                            {
                                ServerName = "Account",
                                Queues = new[] { "1_critical", "2_get_token", "3_general", "4_tokped_cek_pending" },
                                WorkerCount = 1,
                            };
                            var newserver = new BackgroundJobServer(options, sqlStorage);

                            var optionsStokServer = new BackgroundJobServerOptions
                            {
                                ServerName = "Stok",
                                Queues = new[] { "1_update_stok" },
                                WorkerCount = 2,
                            };
                            var newStokServer = new BackgroundJobServer(optionsStokServer, sqlStorage);

                            var optionsBarangServer = new BackgroundJobServerOptions
                            {
                                ServerName = "Product",
                                Queues = new[] { "1_create_product" },
                                WorkerCount = 1,
                            };
                            var newProductServer = new BackgroundJobServer(optionsBarangServer, sqlStorage);

                            RecurringJobManager recurJobM = new RecurringJobManager(sqlStorage);
                            RecurringJobOptions recurJobOpt = new RecurringJobOptions()
                            {
                                QueueName = "3_general"
                            };
                            using (var connection = sqlStorage.GetConnection())
                            {
                                //remove semua recurring job
                                foreach (var recurringJob in connection.GetRecurringJobs())
                                {
                                    recurJobM.RemoveIfExists(recurringJob.Id);
                                }
                                //run semua recurring job seperti user login
                                //remark by fauzi 28 Januari 2020
                                //var sifsys_jtranretur = Convert.ToString(EDB.GetFieldValue("ConnID", "SIFSYS", "1=1", "JTRAN_RETUR"));
                                Task.Run(() => new AccountController().SyncMarketplace(dbsource, nourut, EDBConnID, "", "auto_start", interval, null)).Wait();
                            }
                            using (var connection = sqlStorage.GetConnection())
                            {
                                //update semua recurring job dengan interval sesuai setting timer
                                foreach (var recurringJob in connection.GetRecurringJobs())
                                {
                                    recurJobM.AddOrUpdate(recurringJob.Id, recurringJob.Job, Cron.MinuteInterval(interval), recurJobOpt);
                                }
                            }
                            vsuccess = true;
                            vstatus = "Hangfire untuk account " + item.Email.ToString() + " telah diaktifkan.";
                        }
                        else
                        {
                            vstatus = "Hangfire untuk account " + item.Email.ToString() + " sudah aktif.";
                        }
                    }
                }
                else
                {
                    vsuccess = false;
                    vstatus = "Hangfire tidak dapat diaktifkan dikarenakan account sudah expired.";
                }
                return Json(new { success = vsuccess, status = vstatus }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = ex.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [SessionAdminCheck]
        public ActionResult AdminBroadcastMessage(string pesan)
        {
            var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
            contextNotif.Clients.All.broadcastmessage(pesan);
            return Json("", JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult ProsesAkhirTahunPrepare(string tahun)
        {
            try
            {
                var lastYear = DateTime.UtcNow.AddYears(-1);
                var last2Week = DateTime.UtcNow.AddHours(7).AddDays(-14);
                var datenow = DateTime.UtcNow.AddHours(7);

                var MoDbContext = new MoDbContext("");

                var accountInDb = (from a in MoDbContext.Account
                                   where
                                   (a.LAST_LOGIN_DATE ?? lastYear) >= last2Week
                                   &&
                                   (a.TGL_SUBSCRIPTION ?? lastYear) >= datenow
                                   orderby a.LAST_LOGIN_DATE descending
                                   select new { db_name = a.DatabasePathErasoft, db_source = a.DataSourcePath, onlineshopname = a.NamaTokoOnline }).ToList();

                return new JsonResult { Data = new { arraydbname = accountInDb }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            catch (Exception ex)
            {
                return new JsonResult { Data = new { mo_error = "Gagal memproses akhir tahun. Internal Server Error." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [Queue("3_general")]
        public ActionResult ProsesAkhirTahun(string db_source, string db_name,string tahun)
        {
            try
            {
                //change by fauzi 24 Januari 2020
                //var RemoteMODbContext = new MoDbContext(db_source);
                var RemoteMODbContext = new MoDbContext("");
                //end
                RemoteMODbContext.Database.ExecuteSqlCommand("exec [PROSES_AKHIR_TAHUN] @db_name, @tahun", new SqlParameter("@db_name", db_name), new SqlParameter("@tahun", tahun));

                return new JsonResult { Data = new { mo_message = "Sukses memproses akhir tahun." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            catch (Exception ex)
            {
                return new JsonResult { Data = new { mo_error = "Gagal memproses akhir tahun. Internal Server Error." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        //add by fauzi 21 Februari 2020
        [Queue("3_general")]
        public async Task<ActionResult> ReminderEmailExpiredAccountMP(string dbPathEra, string susername, string semail, string snamatoko, string smarketplace, string expired_date)
        {
            try
            {
                var dateExpired = Convert.ToDateTime(expired_date);
                var dateExpiredEmail = Convert.ToDateTime(expired_date).ToString("dd MMMM yyyy HH:mm tt");
                var countDays = DateTime.UtcNow.AddHours(7).Subtract(dateExpired);
                var countResult = countDays.Days;

                if ((countResult == -7) || (countResult == -3) || (countResult == -1))
                {
                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                    contextNotif.Clients.Group(dbPathEra).notifAccountExpired("Reminder: Status akun marketplace " + snamatoko + " (" + smarketplace + ") akan expired " + countResult.ToString().Replace("-", "") + " hari lagi.");

                    var email = new MailAddress(semail);
                    var body = "<p><img src=\"https://s3-ap-southeast-1.amazonaws.com//masteronlinebucket/uploaded-image/efd0f5b3-7862-4ee6-b796-6c5fc9c63d5f.jpeg\"  width=\"250\" height=\"100\"></p>" +
                    "<p>Hi Kak {0},</p>" +
                    "<p>Untuk menjaga keamanan data Toko, pihak Marketplace secara berkala melakukan pembaharuan / Refresh API Token. Integrasi Marketplace akan otomatis terputus saat API Token Marketplace tersebut sudah expired.</p>" +
                    "<p>Akun Marketplace Kakak yang akan expired adalah sebagai berikut, mohon lakukan link ulang di Master Online :</p>" +
                    "<p><span style='background-color: #FFFF00;text-decoration: underline;'>- Nama akun: {2} {1} akan expired pada {3} atau {4} hari lagi</span></p>" +
                    "<p>Cara melakukan link ulang di Master Online:</p>" +
                    "<p>1. Masuk menu Pengaturan > Link ke Marketplace</p>" +
                    "<p>2. Edit akun Marketplace Anda</p>" +
                    "<p>3. Lengkapi data Toko</p>" +
                    "<p>4. Klik Simpan.</p>" +
                    "<p>Terima kasih atas perhatian dan kerjasama nya. Sukses selalu bersama Master Online.</p>" +
                    "<p>&nbsp;</p>" +
                    "<p>Best regards,</p>" +
                    "<p>&nbsp;</p>" +
                    "<p>Master Online.</p>";

                var message = new MailMessage();
                message.To.Add(email);
                message.From = new MailAddress("csmasteronline@gmail.com");
                message.Subject = "Master Online x "+ smarketplace + " Announcement";
                message.Body = string.Format(body, susername, smarketplace, snamatoko, expired_date.ToString(), countResult.ToString().Replace("-", ""));
                message.IsBodyHtml = true;

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
                }
                return new JsonResult { Data = new { mo_message = "Reminder expired running." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            catch (Exception ex)
            {
                return new JsonResult { Data = new { mo_error = "Reminder expired not running. Because internal Server Error." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        public ActionResult ReminderNotifyExpiredAccountMP(string dbPathEra,  string snamatoko, string smarketplace, string expired_date)
        {
            var dateExpired = Convert.ToDateTime(expired_date);
            var dateExpiredEmail = Convert.ToDateTime(expired_date).ToString("dd MMMM yyyy HH:mm tt");
            var countDays = DateTime.UtcNow.AddHours(7).Subtract(dateExpired);
            var countResult = countDays.Days;

            if ((countResult == -7) || (countResult == -3) || (countResult == -1))
            {
                var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                contextNotif.Clients.Group(dbPathEra).notifAccountExpired("Reminder: Status akun marketplace " + snamatoko + " (" + smarketplace + ") akan expired " + countResult.ToString().Replace("-", "") + " hari lagi.");
            }
            return new JsonResult { Data = new { mo_message = "Notify expired running." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult PromptAccount()
        {
            return View("PromptAccount");
        }
        public ActionResult TablePromptAkunPartial(int? page, string search = "")
        {
            try
            {
                int pagenumber = (page ?? 1) - 1;
                ViewData["searchParam"] = search;
                ViewData["LastPage"] = page;

                //ADD by nurul 2/10/2019, contain search 
                string[] getkata = search.Split(' ');
                string sSQLnama = "";
                string sSQLusername = "";
                string sSQLemail = "";
                if (getkata.Length > 0)
                {
                    for (int i = 0; i < getkata.Length; i++)
                    {
                        if (i > 0)
                        {
                            sSQLnama += " AND ";
                            sSQLusername += " AND ";
                            sSQLemail += " AND ";
                        }

                        sSQLnama += " A.NAMATOKOONLINE like '%" + getkata[i] + "%' ";
                        sSQLusername += "  A.USERNAME like '%" + getkata[i] + "%' ";
                        sSQLemail += "  A.[EMAIL] like '%" + getkata[i] + "%' ";
                    }
                }
                string sSQLSelect = "";
                sSQLSelect += "SELECT A.DATABASEPATHERASOFT AS era_db_path, A.[DATASOURCEPATH] AS data_source_path, A.[EMAIL] AS email, A.[USERNAME] AS NAMA, A.[NAMATOKOONLINE] AS namatoko  ";
                string sSQLCount = "";
                sSQLCount += "SELECT COUNT(*) AS JUMLAH ";
                string sSQL2 = "";
                sSQL2 += "FROM ACCOUNT A ";
                if (search != "")
                {
                    sSQL2 += " WHERE ( " + sSQLnama + " or " + sSQLusername + " or " + sSQLemail + " ) ";
                }
                string sSQLSelect2 = "";
                sSQLSelect2 += "ORDER BY namatoko ASC ";
                sSQLSelect2 += "OFFSET " + Convert.ToString(pagenumber * 10) + " ROWS ";
                sSQLSelect2 += "FETCH NEXT 10 ROWS ONLY ";

                var listSelect = MoDbContext.Database.SqlQuery<PromptAccountViewModel>(sSQLSelect + sSQL2 + sSQLSelect2).ToList();

                var pageContent = new List<PromptAccountViewModel>();
                foreach (var item in listSelect)
                {
                    pageContent.Add(new PromptAccountViewModel()
                    {
                        data_source_path = item.data_source_path,
                        email = item.email,
                        era_db_path = item.era_db_path,
                        nama = item.nama,
                        namatoko = item.namatoko
                    });
                }
                var totalCount = MoDbContext.Database.SqlQuery<getTotalCount>(sSQLCount + sSQL2).Single();


                IPagedList<PromptAccountViewModel> pageOrders = new StaticPagedList<PromptAccountViewModel>(pageContent, pagenumber + 1, 10, totalCount.JUMLAH);
                return PartialView("TablePromptAkunPartial", pageOrders);
            }
            catch (Exception ex)
            {
                return new JsonResult { Data = new { mo_error = "Prompt Gagal" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }
    }
}