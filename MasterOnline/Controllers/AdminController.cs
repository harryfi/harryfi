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
using System.Web.Script.Serialization;
using MasterOnline.Utils;

namespace MasterOnline.Controllers
{
    public class AdminController : Controller
    {

        private readonly MoDbContext MoDbContext;

        //add for support MO by fauzi 24 November 2020
        public ErasoftContext ErasoftDbContext { get; set; }
        DatabaseSQL EDB;
        //end for support MO by fauzi 24 November 2020

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

                //Session["SessionAdmin"] = adminFromDb;
                Session["SessionAdmin"] = adminFromDb.Email;
                Session["SessionAdminUsername"] = adminFromDb.Username;

                result = "DashboardAdmin";
            }
            else if (adminFromDb.Email == "csmasteronline@gmail.com")
            {
                if (admin.Password != adminFromDb.Password)
                {
                    ModelState.AddModelError("", @"Password salah!");
                    return View("Login", admin);
                }

                //Session["SessionAdmin"] = adminFromDb;
                Session["SessionAdmin"] = adminFromDb.Email;
                Session["SessionAdminUsername"] = adminFromDb.Username;

                result = "DashboardAdm";

            }
            return RedirectToAction(result);

        }

        public ActionResult LoggingOut()
        {
            Session["SessionAdmin"] = null;
            Session["SessionAdminUsername"] = null;
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
#if AWS
                //var tujuan = "13.250.232.74\\SQLEXPRESS, 1433";
                //var tujuan = "13.251.222.53\\SQLEXPRESS, 1433";
                //var tujuan = "172.31.1.127\\SQLEXPRESS, 1433"; // T3.LARGE DB FOR REGISTER NEW ACCOUNT  // IP PRIVATE STATIC AWS
                
                //var tujuan = "172.31.20.73\\SQLEXPRESS, 1433"; // TUJUAN PINDAH KE SERVER REGISTER STAGING by Fauzi 28/01/2021
                var tujuan = "172.31.20.192\\SQLEXPRESS, 1433"; // TUJUAN PINDAH KE SERVER REGISTER AUTO SCALE by Fauzi 20/04/2021

#else
                //var tujuan = "54.179.169.195\\SQLEXPRESS, 1444";
                //var tujuan = "13.251.222.53\\SQLEXPRESS, 1433";
                //var tujuan = "13.251.222.53\\SQLEXPRESS, 1433";
                //var tujuan = "172.31.29.78\\SQLEXPRESS, 1433";
                var tujuan = "172.31.20.73\\SQLEXPRESS, 1433"; // TUJUAN PINDAH KE SERVER REGISTER STAGING by Fauzi 28/01/2021
#endif

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
                string dbSourceEra = "";
#if (Debug_AWS)
                dbSourceEra = accInDb.DataSourcePathDebug;
#else
                dbSourceEra = accInDb.DataSourcePath;
#endif
                ErasoftContext ErasoftDbContext = new ErasoftContext(dbSourceEra, accInDb.DatabasePathErasoft);
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
                    //System.Data.Entity.Database.Delete($"Server=localhost;Initial Catalog={accInDb.DatabasePathErasoft};persist security info=True;" +
                    //                                   "user id=masteronline;password=M@ster123;");
                            // IP PRIVATE T3 REGISTER
                            System.Data.Entity.Database.Delete($"Server=172.31.20.192\\SQLEXPRESS,1433;Initial Catalog={accInDb.DatabasePathErasoft};persist security info=True;" +
                                                       "user id=masteronline;password=M@ster123;");
#elif Debug_AWS
                            System.Data.Entity.Database.Delete($"Server=172.31.20.192\\SQLEXPRESS,1433;Initial Catalog={accInDb.DatabasePathErasoft};persist security info=True;" +
                                                               "user id=masteronline;password=M@ster123;");

                            //System.Data.Entity.Database.Delete($"Server=172.31.20.192\\SQLEXPRESS,1433;Initial Catalog={accInDb.DatabasePathErasoft};persist security info=True;" +
                            //                           "user id=masteronline;password=M@ster123;");
#else
                            //System.Data.Entity.Database.Delete($"Server=13.251.222.53\\SQLEXPRESS,1433;Initial Catalog={accInDb.DatabasePathErasoft};persist security info=True;" +
                            //                                   "user id=masteronline;password=M@ster123;");

                            // ADD BY FAUZI 04/12/2020
                            // IP PRIVATE DEV
                            System.Data.Entity.Database.Delete($"Server=172.31.20.73\\SQLEXPRESS,1433;Initial Catalog={accInDb.DatabasePathErasoft};persist security info=True;" +
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
                                        //System.Data.Entity.Database.Delete($"Server=localhost;Initial Catalog={accInDb.DatabasePathErasoft};persist security info=True;" +
                                        //                                   "user id=masteronline;password=M@ster123;");
                            //System.Data.Entity.Database.Delete($"Server=13.250.232.74\\SQLEXPRESS,1433;Initial Catalog={accInDb.DatabasePathErasoft};persist security info=True;" +
                            //                                               "user id=masteronline;password=M@ster123;");

                            // ADD BY FAUZI 04/12/2020
                            // IP PRIVATE LIVE T3 REGIS
                            System.Data.Entity.Database.Delete($"Server=172.31.20.192\\SQLEXPRESS,1433;Initial Catalog={accInDb.DatabasePathErasoft};persist security info=True;" +
                                                                           "user id=masteronline;password=M@ster123;");
#elif Debug_AWS
                            System.Data.Entity.Database.Delete($"Server=172.31.20.192\\SQLEXPRESS,1433;Initial Catalog={accInDb.DatabasePathErasoft};persist security info=True;" +
                                                               "user id=masteronline;password=M@ster123;");

                            //System.Data.Entity.Database.Delete($"Server=172.31.20.192\\SQLEXPRESS,1433;Initial Catalog={accInDb.DatabasePathErasoft};persist security info=True;" +
                            //                                               "user id=masteronline;password=M@ster123;");
#else

                            //System.Data.Entity.Database.Delete($"Server=13.251.222.53\\SQLEXPRESS,1433;Initial Catalog={accInDb.DatabasePathErasoft};persist security info=True;" +
                            //                                   "user id=masteronline;password=M@ster123;");

                            // ADD BY FAUZI 04/12/2020
                            // IP PRIVATE DEV
                            System.Data.Entity.Database.Delete($"Server=172.31.20.73\\SQLEXPRESS,1433;Initial Catalog={accInDb.DatabasePathErasoft};persist security info=True;" +
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
            //add by nurul 7/1/2021
            var addOnCust = MoDbContext.Addons_Customer.Where(a => a.Account == vm.Payment.Email).ToList();
            if (addOnCust.Count() > 0)
            {
                var sSQLUpdateAddOn = "update a set TglSubscription=b.Tgl_Subscription from Addons_Customer a inner join Account b on a.account=b.email where a.account='" + vm.Payment.Email + "'";
                MoDbContext.Database.ExecuteSqlCommand(sSQLUpdateAddOn);
                MoDbContext.SaveChanges();
            }
            //end add by nurul 7/1/2021
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

            return new JsonResult { Data = account, MaxJsonLength = Int32.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
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
                    //message.Body = System.IO.File.ReadAllText("~/Content/admin/PaymentSubscription.html")
                    message.Body = System.IO.File.ReadAllText("C://inetpub//wwwroot//MasterOnline//Content//admin//PaymentSubscription.html")
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
                    //var fileName = Path.GetFileName(file.FileName);
                    //var path = Path.Combine(IPServerLocation + @"Content\Uploaded\", fileName);
                    //marketVm.Marketplace.LokasiLogo = IPServerLocation + @"Content\Uploaded\" + fileName;
                    //file.SaveAs(path);
                    
                    var pathLoc = UploadFileServices.UploadFile_KTP(file, marketVm.Marketplace.NamaMarket);
                    if (pathLoc != null)
                    {
                        marketVm.Marketplace.LokasiLogo = pathLoc;
                    }
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

        // =============================================== Bagian Addons (START)
#region Bagian Addons (START)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveAddons(AddonsViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View("AddonsMenu", vm);
            }

            if (vm.Addons.RecNum == null)
            {
                var eksInDb = MoDbContext.Addons.SingleOrDefault(e => e.Fitur == vm.Addons.Fitur);

                if (eksInDb != null)
                {
                    ModelState.AddModelError("", @"Ekspedisi sudah terdaftar!");
                    return View("AddonsMenu", vm);
                }

                MoDbContext.Addons.Add(vm.Addons);
            }
            else
            {
                var eksInDb = MoDbContext.Addons.Single(e => e.RecNum == vm.Addons.RecNum);
                eksInDb.Fitur = vm.Addons.Fitur;
                eksInDb.Harga = vm.Addons.Harga;
            }

            MoDbContext.SaveChanges();
            ModelState.Clear();

            return RedirectToAction("AddonsMenu");
        }

        public ActionResult EditAddons(int? eksId)
        {
            var vm = new AddonsViewModel()
            {
                Addons = MoDbContext.Addons.Single(e => e.RecNum == eksId)
            };

            ViewData["Editing"] = 1;

            return View("AddonsMenu", vm);
        }

        public ActionResult DeleteAddons(int? eksId)
        {
            var vm = new AddonsViewModel()
            {
                Addons = MoDbContext.Addons.Single(e => e.RecNum == eksId),
                ListAddons = MoDbContext.Addons.ToList()
            };

            MoDbContext.Addons.Remove(vm.Addons);
            MoDbContext.SaveChanges();

            return RedirectToAction("AddonsMenu");
        }

#endregion
        // =============================================== Bagian Addons (END)

        // =============================================== Bagian CustomerAddons (START)
#region Bagian CustomerAddons (START)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCustAddons(AddonsCustomerViewModel vm)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View("AddonsCustMenu", vm);
            //}
            DateTime tglDaftar = DateTime.Now;
            if (vm.Addons_Customer.Account == null)
            {
                var eksInDb = MoDbContext.Addons_Customer.SingleOrDefault(e => e.Account == vm.Accounts.Email);

                if (eksInDb != null)
                {
                    ModelState.AddModelError("", @"Email masih kosong!");
                    return View("AddonsCustMenu", vm);
                }
            }
            else
            {
                var idAddon = Convert.ToInt32(vm.Addons_Customer.ID_ADDON);
                var MasterAddon = MoDbContext.Addons.SingleOrDefault(a => a.RecNum == idAddon);
                var eksInDb = MoDbContext.Addons_Customer.SingleOrDefault(e => e.RecNum == vm.Addons_Customer.RecNum);
                if (eksInDb == null)
                {
                    eksInDb = new Addons_Customer
                    {
                        //change by nurul 21/10/2020
                        //TglSubscription = vm.Accounts.TGL_SUBSCRIPTION,
                        //Account = vm.Accounts.Email,
                        //NamaTokoOnline = vm.Accounts.NamaTokoOnline,
                        //Harga = vm.Addons.Harga,
                        Harga = vm.Addons_Customer.Harga,
                        TglSubscription = vm.Addons_Customer.TglSubscription,
                        Account = vm.Addons_Customer.Account,
                        NamaTokoOnline = vm.Addons_Customer.NamaTokoOnline,
                        //end change by nurul 21/10/2020
                        NamaAddons = vm.Addons_Customer.NamaAddons,
                        //add by nurul 21/10/2020                   
                        TGL_DAFTAR = DateTime.Now,
                        ID_ADDON = vm.Addons_Customer.ID_ADDON
                        //end add by nurul 21/10/2020
                    };
                    //add by nurul 21/10/2020
                    tglDaftar = Convert.ToDateTime(eksInDb.TGL_DAFTAR);
                    if (vm.Addons_Customer.ID_ADDON == "2") //82cart FREE
                    {
                        eksInDb.STATUS = "1";
                    }
                    else
                    {
                        eksInDb.STATUS = "0";
                    }
                    if(eksInDb.Harga <= 0)
                    {
                        if(MasterAddon != null)
                        {
                            eksInDb.Harga = MasterAddon.Harga;
                            if (eksInDb.NamaAddons == "" || eksInDb.NamaAddons == null)
                            {
                                eksInDb.NamaAddons = MasterAddon.Fitur;
                            }
                        }
                    }
                    //end add by nurul 21/10/2020
                    MoDbContext.Addons_Customer.Add(eksInDb);
                }
                else
                {
                    eksInDb.TglSubscription = vm.Addons_Customer.TglSubscription;
                    eksInDb.Account = vm.Addons_Customer.Account;
                    eksInDb.NamaTokoOnline = vm.Addons_Customer.NamaTokoOnline;
                    eksInDb.Harga = vm.Addons_Customer.Harga;
                    eksInDb.NamaAddons = vm.Addons_Customer.NamaAddons;
                }
            }

            MoDbContext.SaveChanges();
            ModelState.Clear();

            return new EmptyResult(); 
            //var vmNew = new AddonsCustomerViewModel()
            //{
            //    Addons_Customer = MoDbContext.Addons_Customer.SingleOrDefault(e => e.TGL_DAFTAR == tglDaftar)
            //};

            ////return Json(vmNew, JsonRequestBehavior.AllowGet);
            //return PartialView("FormAddonsCustPartial", vmNew);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCustAddonsCS(AddonsCustomerViewModel vm)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View("AddonsCustMenu", vm);
            //}
            DateTime tglDaftar = DateTime.Now;
            if (vm.Addons_Customer.Account == null)
            {
                var eksInDb = MoDbContext.Addons_Customer.SingleOrDefault(e => e.Account == vm.Accounts.Email);

                if (eksInDb != null)
                {
                    ModelState.AddModelError("", @"Email masih kosong!");
                    return View("AddonsCustMenuCS", vm);
                }
            }
            else
            {
                var idAddon = Convert.ToInt32(vm.Addons_Customer.ID_ADDON);
                var MasterAddon = MoDbContext.Addons.SingleOrDefault(a => a.RecNum == idAddon);
                var eksInDb = MoDbContext.Addons_Customer.SingleOrDefault(e => e.RecNum == vm.Addons_Customer.RecNum);
                if (eksInDb == null)
                {
                    eksInDb = new Addons_Customer
                    {
                        //change by nurul 21/10/2020
                        //TglSubscription = vm.Accounts.TGL_SUBSCRIPTION,
                        //Account = vm.Accounts.Email,
                        //NamaTokoOnline = vm.Accounts.NamaTokoOnline,
                        //Harga = vm.Addons.Harga,
                        Harga = vm.Addons_Customer.Harga,
                        TglSubscription = vm.Addons_Customer.TglSubscription,
                        Account = vm.Addons_Customer.Account,
                        NamaTokoOnline = vm.Addons_Customer.NamaTokoOnline,
                        //end change by nurul 21/10/2020
                        NamaAddons = vm.Addons_Customer.NamaAddons,
                        //add by nurul 21/10/2020                   
                        TGL_DAFTAR = DateTime.Now,
                        ID_ADDON = vm.Addons_Customer.ID_ADDON
                        //end add by nurul 21/10/2020
                    };
                    //add by nurul 21/10/2020
                    tglDaftar = Convert.ToDateTime(eksInDb.TGL_DAFTAR);
                    if (vm.Addons_Customer.ID_ADDON == "2") //82cart FREE
                    {
                        eksInDb.STATUS = "1";
                    }
                    else
                    {
                        eksInDb.STATUS = "0";
                    }
                    if (eksInDb.Harga <= 0)
                    {
                        if (MasterAddon != null)
                        {
                            eksInDb.Harga = MasterAddon.Harga;
                            if (eksInDb.NamaAddons == "" || eksInDb.NamaAddons == null)
                            {
                                eksInDb.NamaAddons = MasterAddon.Fitur;
                            }
                        }
                    }
                    //end add by nurul 21/10/2020
                    MoDbContext.Addons_Customer.Add(eksInDb);
                }
                else
                {
                    eksInDb.TglSubscription = vm.Addons_Customer.TglSubscription;
                    eksInDb.Account = vm.Addons_Customer.Account;
                    eksInDb.NamaTokoOnline = vm.Addons_Customer.NamaTokoOnline;
                    eksInDb.Harga = vm.Addons_Customer.Harga;
                    eksInDb.NamaAddons = vm.Addons_Customer.NamaAddons;
                }
            }

            MoDbContext.SaveChanges();
            ModelState.Clear();

            return new EmptyResult();
            //var vmNew = new AddonsCustomerViewModel()
            //{
            //    Addons_Customer = MoDbContext.Addons_Customer.SingleOrDefault(e => e.TGL_DAFTAR == tglDaftar)
            //};

            ////return Json(vmNew, JsonRequestBehavior.AllowGet);
            //return PartialView("FormAddonsCustPartial", vmNew);
        }

        [HttpGet]
        public ActionResult GetAddons()
        {
            var addons = MoDbContext.Addons.ToList();

            return Json(addons, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult EditCustAddons(int? eksId)
        {
            var vm = new AddonsCustomerViewModel()
            {
                Addons_Customer = MoDbContext.Addons_Customer.SingleOrDefault(e => e.RecNum == eksId)
            };

            //ViewData["Editing"] = 1;

            //return Json(vm, JsonRequestBehavior.AllowGet);
            return PartialView("FormAddonsCustPartial", vm);
        }

        public ActionResult DeleteCustAddons(int? eksId)
        {
            var vm = new AddonsCustomerViewModel()
            {
                Addons_Customer = MoDbContext.Addons_Customer.Single(e => e.RecNum == eksId),
                ListCustAddons = MoDbContext.Addons_Customer.ToList()
            };

            MoDbContext.Addons_Customer.Remove(vm.Addons_Customer);
            MoDbContext.SaveChanges();

            return RedirectToAction("AddonsCustMenu");
        }

        public ActionResult DeleteCustAddonsCS(int? eksId)
        {
            var vm = new AddonsCustomerViewModel()
            {
                Addons_Customer = MoDbContext.Addons_Customer.Single(e => e.RecNum == eksId),
                ListCustAddons = MoDbContext.Addons_Customer.ToList()
            };

            MoDbContext.Addons_Customer.Remove(vm.Addons_Customer);
            MoDbContext.SaveChanges();

            return RedirectToAction("AddonsCustMenuCS");
        }

        #endregion
        // =============================================== Bagian CustomerAddons (END)

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
                //ListAccount = MoDbContext.Account.OrderByDescending(a => a.TGL_DAFTAR).ToList(),
                //ListPartner = MoDbContext.Partner.ToList()
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
        //public ActionResult RefreshAccountMenuEdit()
        //{
        //    var vm = new MenuAccount()
        //    {
        //        ListAccount = MoDbContext.Account.OrderByDescending(a => a.TGL_DAFTAR).ToList(),
        //        ListPartner = MoDbContext.Partner.ToList()
        //    };
        //    return PartialView("TableAccountEdit", vm);
        //}
        public ActionResult RefreshAccountMenuEdit(int? page, string search = "")
        {
            int pagenumber = (page ?? 1) - 1;
            ViewData["searchParam"] = search;
            ViewData["LastPage"] = page;

            string[] getkata = search.Split(' ');
            string sSQLemail = "";
            string sSQLnama = "";
            string sSQLusername = "";
            string sSQLpartner = "";
            if (getkata.Length > 0)
            {
                if (search != "")
                {
                    for (int i = 0; i < getkata.Length; i++)
                    {
                        if (i > 0)
                        {
                            sSQLemail += " and ";
                            sSQLnama += " and ";
                            sSQLusername += " and ";
                            sSQLpartner += " and ";
                        }

                        sSQLemail += " ( a.Email like '%" + getkata[i] + "%' ) ";
                        sSQLnama += "  ( a.NamaTokoOnline like '%" + getkata[i] + "%' ) ";
                        sSQLusername += " ( a.Username like '%" + getkata[i] + "%' ) ";
                        sSQLpartner += "  ( isnull(b.Username,'') like '%" + getkata[i] + "%' ) ";

                    }
                }
            }

            string sSQLSelect = "";
            sSQLSelect += "select a.*,isnull(b.Username,'') as nmPartner,c.KETERANGAN as Subscription ";
            string sSQLCount = "";
            sSQLCount += "SELECT COUNT(AccountId) AS JUMLAH ";
            string sSQL2 = "";
            sSQL2 += "FROM account a(nolock) ";
            sSQL2 += "left join partner b (nolock) on a.KODE_REFERRAL=convert(nvarchar,b.PartnerId) left join Subscription c(nolock) on a.KODE_SUBSCRIPTION=c.kode ";
            //sSQL2 += "where LEVEL = '2' ";
            if (search != "")
            {
                sSQL2 += " where ( " + sSQLemail + " or " + sSQLnama + " or " + sSQLusername + " or " + sSQLpartner + " ) ";
            }

            var minimal_harus_ada_item_untuk_current_page = (page * 10) - 9;
            var totalCount = MoDbContext.Database.SqlQuery<getTotalCount>(sSQLCount + sSQL2).Single();
            if (minimal_harus_ada_item_untuk_current_page > totalCount.JUMLAH)
            {
                pagenumber = pagenumber - 1;
            }

            string sSQLSelect2 = "";
            sSQLSelect2 += "ORDER BY TGL_DAFTAR desc ";
            sSQLSelect2 += "OFFSET " + Convert.ToString(pagenumber * 10) + " ROWS ";
            sSQLSelect2 += "FETCH NEXT 10 ROWS ONLY ";

            var ListAkun = MoDbContext.Database.SqlQuery<mdlAkun>(sSQLSelect + sSQL2 + sSQLSelect2).ToList();

            IPagedList<mdlAkun> pageOrders = new StaticPagedList<mdlAkun>(ListAkun, pagenumber + 1, 10, totalCount.JUMLAH);
            return PartialView("TableAccountEdit", pageOrders);
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
            //add by nurul 7/1/2021
            var addOnCust = MoDbContext.Addons_Customer.Where(a => a.Account == data.Account.Email).ToList();
            if (addOnCust.Count() > 0)
            {
                var sSQLUpdateAddOn = "update a set TglSubscription=b.Tgl_Subscription from Addons_Customer a inner join Account b on a.account=b.email where a.account='" + data.Account.Email + "'";
                MoDbContext.Database.ExecuteSqlCommand(sSQLUpdateAddOn);
                MoDbContext.SaveChanges();
            }
            //end add by nurul 7/1/2021
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
                //ListAccount = MoDbContext.Account.OrderByDescending(a => a.TGL_DAFTAR).ToList(),
                //ListPartner = MoDbContext.Partner.ToList()
            };
            return View(vm);
            //end change by nurul 13/2/2019
        }

        //add by nurul 13/2/2019
        //public ActionResult RefreshAccountMenu()
        //{
        //    var vm = new MenuAccount()
        //    {
        //        ListAccount = MoDbContext.Account.OrderByDescending(a => a.TGL_DAFTAR).ToList(),
        //        ListPartner = MoDbContext.Partner.ToList()
        //    };
        //    return PartialView("TableAccount", vm);
        //}
        public ActionResult RefreshAccountMenu(int? page, string search = "")
        {
            int pagenumber = (page ?? 1) - 1;
            ViewData["searchParam"] = search;
            ViewData["LastPage"] = page;
            
            string[] getkata = search.Split(' ');
            string sSQLemail = "";
            string sSQLnama = "";
            string sSQLusername = "";
            string sSQLpartner = "";
            if (getkata.Length > 0)
            {
                if (search != "")
                {
                    for (int i = 0; i < getkata.Length; i++)
                    {
                        if (i > 0)
                        {
                            sSQLemail += " and ";
                            sSQLnama += " and ";
                            sSQLusername += " and ";
                            sSQLpartner += " and ";
                        }
                        
                        sSQLemail += " ( a.Email like '%" + getkata[i] + "%' ) ";
                        sSQLnama += "  ( a.NamaTokoOnline like '%" + getkata[i] + "%' ) ";
                        sSQLusername += " ( a.Username like '%" + getkata[i] + "%' ) ";
                        sSQLpartner += "  ( isnull(b.Username,'') like '%" + getkata[i] + "%' ) ";

                    }
                }
            }

            string sSQLSelect = "";
            sSQLSelect += "select a.*,isnull(b.Username,'') as nmPartner,c.KETERANGAN as Subscription ";
            string sSQLCount = "";
            sSQLCount += "SELECT COUNT(AccountId) AS JUMLAH ";
            string sSQL2 = "";
            sSQL2 += "FROM account a(nolock) ";
            sSQL2 += "left join partner b (nolock) on a.KODE_REFERRAL=convert(nvarchar,b.PartnerId) left join Subscription c(nolock) on a.KODE_SUBSCRIPTION=c.kode ";
            //sSQL2 += "where LEVEL = '2' ";
            if (search != "")
            {
                sSQL2 += " where ( " + sSQLemail + " or " + sSQLnama + " or " + sSQLusername + " or " + sSQLpartner + " ) ";
            }

            var minimal_harus_ada_item_untuk_current_page = (page * 10) - 9;
            var totalCount = MoDbContext.Database.SqlQuery<getTotalCount>(sSQLCount + sSQL2).Single();
            if (minimal_harus_ada_item_untuk_current_page > totalCount.JUMLAH)
            {
                pagenumber = pagenumber - 1;
            }

            string sSQLSelect2 = "";
            sSQLSelect2 += "ORDER BY TGL_DAFTAR desc ";
            sSQLSelect2 += "OFFSET " + Convert.ToString(pagenumber * 10) + " ROWS ";
            sSQLSelect2 += "FETCH NEXT 10 ROWS ONLY ";

            var ListAkun = MoDbContext.Database.SqlQuery<mdlAkun>(sSQLSelect + sSQL2 + sSQLSelect2).ToList();

            IPagedList<mdlAkun> pageOrders = new StaticPagedList<mdlAkun>(ListAkun, pagenumber + 1, 10, totalCount.JUMLAH);
            return PartialView("TableAccount", pageOrders);
        }


        //add by Iman 15/04/2019
        [Route("admin/manage/reminder-expired")]
        [SessionAdminCheck]
        public ActionResult AccountReminderExpired(int? page = 1, string search = "", string filter = "14")
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

            //DateTime dateTime = DateTime.UtcNow.Date;
            //DateTime Nextmonth = dateTime.AddMonths(1);
            //param = dateTime.ToString("dd/MM/yyyy") + ";" + Nextmonth.ToString("dd/MM/yyyy");
            ////param = "29/04/2019;29/05/2019";
            //string dr = (param.Split(';')[param.Split(';').Length - 2]);
            //string sd = (param.Split(';')[param.Split(';').Length - 1]);
            //string tgl1 = (dr.Split('/')[dr.Split('/').Length - 3]);
            //string bln1 = (dr.Split('/')[dr.Split('/').Length - 2]);
            //string thn1 = (dr.Split('/')[dr.Split('/').Length - 1]);
            //string drtanggal = tgl1 + '/' + bln1 + '/' + thn1;
            //string tgl2 = (sd.Split('/')[sd.Split('/').Length - 3]);
            //string bln2 = (sd.Split('/')[sd.Split('/').Length - 2]);
            //string thn2 = (sd.Split('/')[sd.Split('/').Length - 1]);
            //string sdtanggal = tgl2 + '/' + bln2 + '/' + thn2;
            //var drTgl = DateTime.ParseExact(drtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            //var sdTgl = DateTime.ParseExact(sdtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            //var vm = new MenuAccount
            //{
            //    //ListAccount = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION >= drTgl && a.TGL_SUBSCRIPTION <= sdTgl && a.Status == true).ToList(),
            //    ListPartner = MoDbContext.Partner.ToList()
            //};

            //return View("AccountReminderExpired", vm);

            return View("AccountReminderExpired");

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

        public ActionResult RefreshTableReminderWillExpired(string take, int? page, string drTgl, string search = "", string filter = "0")//, string sdTgl
        {
            int pagenumber = (page ?? 1) - 1;
            ViewData["takeRecord"] = take;
            ViewData["LastPage"] = page;
            ViewData["getDrTgl"] = drTgl;
            //ViewData["getSdTgl"] = sdTgl;
            //ViewData["flag"] = flag;
            ViewData["searchParam"] = search;
            ViewData["filter"] = filter;

            
            if (filter == "" || filter == null)
                filter = "0";
            int filterDays = Convert.ToInt32(filter);

            DateTime Drtgl;
            DateTime Sdtgl;

            if (!(String.IsNullOrEmpty(drTgl)))
            {
                Drtgl = DateTime.ParseExact(drTgl, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture).AddDays(filterDays);
            }
            else
            {
                Drtgl = DateTime.UtcNow.AddHours(7);
            }

            if (!(String.IsNullOrEmpty(drTgl)))
            {
                Sdtgl = DateTime.ParseExact(drTgl, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture).AddDays(filterDays);
            }
            else
            {
                Sdtgl = DateTime.UtcNow.AddHours(7).AddMonths(1);
            }
            //DateTime Drtgl = ((drTgl != "" || drTgl == null) ? DateTime.ParseExact(drTgl, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture) : DateTime.UtcNow.AddHours(7));
            //DateTime Sdtgl = ((sdTgl != "" || sdTgl == null) ? DateTime.ParseExact(sdTgl, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture) : DateTime.UtcNow.AddHours(7).AddMonths(1));
            //var Drtgl = DateTime.UtcNow.AddHours(7);
            //var Sdtgl = DateTime.UtcNow.AddHours(7).AddMonths(1);
            string tempDrtgl = Drtgl.ToString("yyyy-MM-dd");
            string tempSdtgl = Sdtgl.ToString("yyyy-MM-dd");

            string[] getkata = search.Split(' ');
            string sSQLEmail = "";
            string sSQLUsername = "";
            string sSQLNohp = "";
            string sSQLNamatoko = "";
            string sSQLNamapartner = "";
            string sSQLTglsubs = "";
            string sSQLTipesubs = "";
            string sSQLJumlahsubs = "";
            string sSQLTglemail1 = "";
            string sSQLTglemail2 = "";

            if (getkata.Length > 0)
            {
                if (search != "")
                {
                    for (int i = 0; i < getkata.Length; i++)
                    {
                        if (i > 0)
                        {
                            sSQLEmail += " and ";
                            sSQLUsername += " and ";
                            sSQLNohp += " and ";
                            sSQLNamatoko += " and ";
                            sSQLNamapartner += " and ";
                            sSQLTglsubs += " and ";
                            sSQLTipesubs += " and ";
                            sSQLJumlahsubs += " and ";
                            sSQLTglemail1 += " and ";
                            sSQLTglemail2 += " and ";
                        }

                        sSQLEmail += " ( A.Email like '%" + getkata[i] + "%' )";
                        sSQLUsername += " ( A.Username like '%" + getkata[i] + "%' )";
                        sSQLNohp += " ( A.NoHp like '%" + getkata[i] + "%' )";
                        sSQLNamatoko += " ( A.NamaTokoOnline like '%" + getkata[i] + "%' )";
                        sSQLNamapartner += " ( B.Username like '%" + getkata[i] + "%' )";
                        sSQLTglsubs += " ( A.TGL_SUBSCRIPTION like '%" + getkata[i] + "%' )";
                        sSQLTipesubs += " ( C.KETERANGAN like '%" + getkata[i] + "%' )";
                        sSQLJumlahsubs += " ( A.jumlahUser like '%" + getkata[i] + "%' )";
                        sSQLTglemail1 += " ( A.tgl_email1 like '%" + getkata[i] + "%' )";
                        sSQLTglemail2 += " ( A.tgl_email2 like '%" + getkata[i] + "%' )";

                    }
                }
            }

            string sSQLSelect = "";
            sSQLSelect += "SELECT A.AccountId, A.Email, A.Username, A.NoHp, A.NamaTokoOnline, ISNULL(B.Username, '') [NamaPartner], ";
            sSQLSelect += "A.TGL_SUBSCRIPTION , C.KETERANGAN, ISNULL(A.jumlahUser, 0) [JumlahUser], A.Status, A.tgl_email1, A.tgl_email2 ";
            string sSQLCount = "";
            sSQLCount += "SELECT COUNT(A.Email) AS JUMLAH ";
            string sSQL2 = "";
            sSQL2 += "FROM Account A (NOLOCK) LEFT JOIN Partner B (NOLOCK) ON A.KODE_REFERRAL = B.PartnerId ";
            sSQL2 += "LEFT JOIN Subscription C (NOLOCK) ON A.KODE_SUBSCRIPTION = C.KODE ";
            sSQL2 += "WHERE A.TGL_SUBSCRIPTION >= '" + tempDrtgl + "' AND A.TGL_SUBSCRIPTION <= '" + tempSdtgl + "'";
            //if (flag == "1")
            //{
            //    sSQL2 += "WHERE A.TGL_SUBSCRIPTION >= '" + tempDrtgl + "' AND A.TGL_SUBSCRIPTION <= '" + tempSdtgl + "'";
            //}
            //else
            //{
            //    sSQL2 += "WHERE A.TGL_SUBSCRIPTION <= '" + tempSdtgl + "'";
            //}


            if (search != "")
            {
                sSQL2 += " AND ( " + sSQLEmail + " or " + sSQLUsername + " or " + sSQLNohp + " or " + sSQLNamatoko + "  or " + sSQLNamapartner + " or "
                                    + sSQLTglsubs + " or " + sSQLTipesubs + "   or " + sSQLJumlahsubs + " or " + sSQLTglemail1 + " or " + sSQLTglemail2 + " ) ";
            }

            var minimal_harus_ada_item_untuk_current_page = (page * 10) - 9;
            var totalReminderExpired = MoDbContext.Database.SqlQuery<int>(sSQLCount + sSQL2).Single();
            if (minimal_harus_ada_item_untuk_current_page > totalReminderExpired)
            {
                pagenumber = pagenumber - 1;
            }

            string sSQLSelect2 = "";
            sSQLSelect2 += "ORDER BY TGL_SUBSCRIPTION ASC ";
            sSQLSelect2 += "OFFSET " + Convert.ToString(pagenumber * Convert.ToInt32(take)) + " ROWS ";
            sSQLSelect2 += "FETCH NEXT " + take + " ROWS ONLY ";

            var reminderExpired = MoDbContext.Database.SqlQuery<REMINDER_EXPIRED_VIEW>(sSQLSelect + sSQL2 + sSQLSelect2).ToList();

            IPagedList<REMINDER_EXPIRED_VIEW> pageOrders = new StaticPagedList<REMINDER_EXPIRED_VIEW>(reminderExpired, pagenumber + 1, Convert.ToInt32(take), totalReminderExpired); //Convert.ToInt32(take) //10

            return PartialView("TableAccountReminderExpired", pageOrders);
        }

        public ActionResult RefreshTableReminderAlreadyExpired(string take, int? page, string drTgl, string sdTgl, string search = "", string filter = "0")
        {
            int pagenumber = (page ?? 1) - 1;
            ViewData["takeRecord"] = take;
            ViewData["LastPage"] = page;
            ViewData["getDrTgl"] = drTgl;
            ViewData["getSdTgl"] = sdTgl;
            //ViewData["flag"] = flag;
            ViewData["searchParam"] = search;
            ViewData["filter"] = filter;


            if (filter == "" || filter == null)
                filter = "0";
            int filterDays = Convert.ToInt32(filter);

            DateTime Drtgl;
            DateTime Sdtgl;

            if (!(String.IsNullOrEmpty(drTgl)))
            {
                Drtgl = DateTime.ParseExact(drTgl, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture).AddDays(filterDays);
            }
            else
            {
                Drtgl = DateTime.UtcNow.AddHours(7);
            }

            if (!(String.IsNullOrEmpty(sdTgl)))
            {
                Sdtgl = DateTime.ParseExact(drTgl, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture).AddDays(filterDays);
            }
            else
            {
                Sdtgl = DateTime.UtcNow.AddHours(7).AddMonths(1);
            }
            //DateTime Drtgl = ((drTgl != "" || drTgl == null) ? DateTime.ParseExact(drTgl, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture) : DateTime.UtcNow.AddHours(7));
            //DateTime Sdtgl = ((sdTgl != "" || sdTgl == null) ? DateTime.ParseExact(sdTgl, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture) : DateTime.UtcNow.AddHours(7).AddMonths(1));
            //var Drtgl = DateTime.UtcNow.AddHours(7);
            //var Sdtgl = DateTime.UtcNow.AddHours(7).AddMonths(1);
            string tempDrtgl = Drtgl.ToString("yyyy-MM-dd");
            string tempSdtgl = Sdtgl.ToString("yyyy-MM-dd");

            string[] getkata = search.Split(' ');
            string sSQLEmail = "";
            string sSQLUsername = "";
            string sSQLNohp = "";
            string sSQLNamatoko = "";
            string sSQLNamapartner = "";
            string sSQLTglsubs = "";
            string sSQLTipesubs = "";
            string sSQLJumlahsubs = "";
            string sSQLTglemail1 = "";
            string sSQLTglemail2 = "";

            if (getkata.Length > 0)
            {
                if (search != "")
                {
                    for (int i = 0; i < getkata.Length; i++)
                    {
                        if (i > 0)
                        {
                            sSQLEmail += " and ";
                            sSQLUsername += " and ";
                            sSQLNohp += " and ";
                            sSQLNamatoko += " and ";
                            //sSQLNamapartner += " and ";
                            sSQLTglsubs += " and ";
                            sSQLTipesubs += " and ";
                            sSQLJumlahsubs += " and ";
                            sSQLTglemail1 += " and ";
                            sSQLTglemail2 += " and ";
                        }

                        sSQLEmail += " ( A.Email like '%" + getkata[i] + "%' )";
                        sSQLUsername += " ( A.Username like '%" + getkata[i] + "%' )";
                        sSQLNohp += " ( A.NoHp like '%" + getkata[i] + "%' )";
                        sSQLNamatoko += " ( A.NamaTokoOnline like '%" + getkata[i] + "%' )";
                        //sSQLNamapartner += " ( B.Username like '%" + getkata[i] + "%' )";
                        sSQLTglsubs += " ( A.TGL_SUBSCRIPTION like '%" + getkata[i] + "%' )";
                        sSQLTipesubs += " ( C.KETERANGAN like '%" + getkata[i] + "%' )";
                        sSQLJumlahsubs += " ( A.jumlahUser like '%" + getkata[i] + "%' )";
                        sSQLTglemail1 += " ( A.tgl_email1 like '%" + getkata[i] + "%' )";
                        sSQLTglemail2 += " ( A.tgl_email2 like '%" + getkata[i] + "%' )";

                    }
                }
            }

            string sSQLSelect = "";
            sSQLSelect += "SELECT A.AccountId, A.Email, A.Username, A.NoHp, A.NamaTokoOnline, ISNULL(B.Username, '') [NamaPartner], ";
            sSQLSelect += "A.TGL_SUBSCRIPTION , C.KETERANGAN, ISNULL(A.jumlahUser, 0) [JumlahUser], A.Status, A.tgl_email1, A.tgl_email2 ";
            string sSQLCount = "";
            sSQLCount += "SELECT COUNT(A.Email) AS JUMLAH ";
            string sSQL2 = "";
            sSQL2 += "FROM Account A (NOLOCK) LEFT JOIN Partner B (NOLOCK) ON A.KODE_REFERRAL = B.PartnerId ";
            sSQL2 += "LEFT JOIN Subscription C (NOLOCK) ON A.KODE_SUBSCRIPTION = C.KODE ";
            sSQL2 += "WHERE A.TGL_SUBSCRIPTION <= '" + tempDrtgl + "'";

            //if (flag == "1")
            //{
            //    sSQL2 += "WHERE A.TGL_SUBSCRIPTION >= '" + tempDrtgl + "' AND A.TGL_SUBSCRIPTION <= '" + tempSdtgl + "'";
            //}
            //else
            //{
            //    sSQL2 += "WHERE A.TGL_SUBSCRIPTION <= '" + tempSdtgl + "'";
            //}


            if (search != "")
            {
                sSQL2 += " AND ( " + sSQLEmail + " or " + sSQLUsername + " or " + sSQLNohp + " or " + sSQLNamatoko + "  or "
                                    + sSQLTglsubs + " or " + sSQLTipesubs + "   or " + sSQLJumlahsubs + " or " + sSQLTglemail1 + " or " + sSQLTglemail2 + " ) "; // " + sSQLNamapartner + " or
            }

            var minimal_harus_ada_item_untuk_current_page = (page * 10) - 9;
            var totalReminderExpired = MoDbContext.Database.SqlQuery<int>(sSQLCount + sSQL2).Single();
            if (minimal_harus_ada_item_untuk_current_page > totalReminderExpired)
            {
                pagenumber = pagenumber - 1;
            }

            string sSQLSelect2 = "";
            sSQLSelect2 += "ORDER BY TGL_SUBSCRIPTION DESC ";
            sSQLSelect2 += "OFFSET " + Convert.ToString(pagenumber * Convert.ToInt32(take)) + " ROWS ";
            sSQLSelect2 += "FETCH NEXT " + take + " ROWS ONLY ";

            var reminderExpired = MoDbContext.Database.SqlQuery<REMINDER_EXPIRED_VIEW>(sSQLSelect + sSQL2 + sSQLSelect2).ToList();

            IPagedList<REMINDER_EXPIRED_VIEW> pageOrders = new StaticPagedList<REMINDER_EXPIRED_VIEW>(reminderExpired, pagenumber + 1, Convert.ToInt32(take), totalReminderExpired); //Convert.ToInt32(take) //10

            return PartialView("TableAccountSudahExpired", pageOrders);
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
        //public ActionResult AccountMenuAktif()
        //{
        //    var vm = new MenuAccount()
        //    {
        //        ListAccount = MoDbContext.Account.Where(a => a.Status == true).ToList(),
        //        ListPartner = MoDbContext.Partner.ToList()
        //    };
        //    return PartialView("TableAccountAktif", vm);
        //}
        public ActionResult RefreshTableAccountMenuAktif(string type, int? page, string search = "")
        {
            int pagenumber = (page ?? 1) - 1;
            ViewData["searchParam"] = search;
            ViewData["LastPage"] = page;

            string[] getkata = search.Split(' ');
            string sSQLemail = "";
            string sSQLnama = "";
            string sSQLusername = "";
            string sSQLpartner = "";
            if (getkata.Length > 0)
            {
                if (search != "")
                {
                    for (int i = 0; i < getkata.Length; i++)
                    {
                        if (i > 0)
                        {
                            sSQLemail += " and ";
                            sSQLnama += " and ";
                            sSQLusername += " and ";
                            sSQLpartner += " and ";
                        }

                        sSQLemail += " ( a.Email like '%" + getkata[i] + "%' ) ";
                        sSQLnama += "  ( a.NamaTokoOnline like '%" + getkata[i] + "%' ) ";
                        sSQLusername += " ( a.Username like '%" + getkata[i] + "%' ) ";
                        sSQLpartner += "  ( isnull(b.Username,'') like '%" + getkata[i] + "%' ) ";

                    }
                }
            }

            string sSQLSelect = "";
            sSQLSelect += "select a.*,isnull(b.Username,'') as nmPartner,c.KETERANGAN as Subscription ";
            string sSQLCount = "";
            sSQLCount += "SELECT COUNT(AccountId) AS JUMLAH ";
            string sSQL2 = "";
            sSQL2 += "FROM account a(nolock) ";
            sSQL2 += "left join partner b (nolock) on a.KODE_REFERRAL=convert(nvarchar,b.PartnerId) left join Subscription c(nolock) on a.KODE_SUBSCRIPTION=c.kode ";
            sSQL2 += "where a.Status = '1' ";
            if (search != "")
            {
                sSQL2 += " and ( " + sSQLemail + " or " + sSQLnama + " or " + sSQLusername + " or " + sSQLpartner + " ) ";
            }

            var minimal_harus_ada_item_untuk_current_page = (page * 10) - 9;
            var totalCount = MoDbContext.Database.SqlQuery<getTotalCount>(sSQLCount + sSQL2).Single();
            if (minimal_harus_ada_item_untuk_current_page > totalCount.JUMLAH)
            {
                pagenumber = pagenumber - 1;
            }

            string sSQLSelect2 = "";
            sSQLSelect2 += "ORDER BY TGL_DAFTAR desc ";
            sSQLSelect2 += "OFFSET " + Convert.ToString(pagenumber * 10) + " ROWS ";
            sSQLSelect2 += "FETCH NEXT 10 ROWS ONLY ";

            var ListAkun = MoDbContext.Database.SqlQuery<mdlAkun>(sSQLSelect + sSQL2 + sSQLSelect2).ToList();

            IPagedList<mdlAkun> pageOrders = new StaticPagedList<mdlAkun>(ListAkun, pagenumber + 1, 10, totalCount.JUMLAH);
            if(type == "2")
            {
                return PartialView("TableAccAktif", pageOrders);
            }
            return PartialView("TableAccountAktif", pageOrders);
        }

        [SessionAdminCheck]
        //public ActionResult AccountMenuNonaktif(string param)
        //{
        //    var vm = new MenuAccount()
        //    {
        //        ListAccount = MoDbContext.Account.Where(a => a.Status == false).ToList(),
        //        ListPartner = MoDbContext.Partner.ToList()
        //    };
        //    return PartialView("TableAccountNonaktif", vm);
        //}
        public ActionResult RefreshTableAccountMenuNonaktif(string type, int? page, string search = "")
        {
            int pagenumber = (page ?? 1) - 1;
            ViewData["searchParam"] = search;
            ViewData["LastPage"] = page;

            string[] getkata = search.Split(' ');
            string sSQLemail = "";
            string sSQLnama = "";
            string sSQLusername = "";
            string sSQLpartner = "";
            if (getkata.Length > 0)
            {
                if (search != "")
                {
                    for (int i = 0; i < getkata.Length; i++)
                    {
                        if (i > 0)
                        {
                            sSQLemail += " and ";
                            sSQLnama += " and ";
                            sSQLusername += " and ";
                            sSQLpartner += " and ";
                        }

                        sSQLemail += " ( a.Email like '%" + getkata[i] + "%' ) ";
                        sSQLnama += "  ( a.NamaTokoOnline like '%" + getkata[i] + "%' ) ";
                        sSQLusername += " ( a.Username like '%" + getkata[i] + "%' ) ";
                        sSQLpartner += "  ( isnull(b.Username,'') like '%" + getkata[i] + "%' ) ";

                    }
                }
            }

            string sSQLSelect = "";
            sSQLSelect += "select a.*,isnull(b.Username,'') as nmPartner,c.KETERANGAN as Subscription ";
            string sSQLCount = "";
            sSQLCount += "SELECT COUNT(AccountId) AS JUMLAH ";
            string sSQL2 = "";
            sSQL2 += "FROM account a(nolock) ";
            sSQL2 += "left join partner b (nolock) on a.KODE_REFERRAL=convert(nvarchar,b.PartnerId) left join Subscription c(nolock) on a.KODE_SUBSCRIPTION=c.kode ";
            sSQL2 += "where a.Status = '0' ";
            if (search != "")
            {
                sSQL2 += " and ( " + sSQLemail + " or " + sSQLnama + " or " + sSQLusername + " or " + sSQLpartner + " ) ";
            }

            var minimal_harus_ada_item_untuk_current_page = (page * 10) - 9;
            var totalCount = MoDbContext.Database.SqlQuery<getTotalCount>(sSQLCount + sSQL2).Single();
            if (minimal_harus_ada_item_untuk_current_page > totalCount.JUMLAH)
            {
                pagenumber = pagenumber - 1;
            }

            string sSQLSelect2 = "";
            sSQLSelect2 += "ORDER BY TGL_DAFTAR desc ";
            sSQLSelect2 += "OFFSET " + Convert.ToString(pagenumber * 10) + " ROWS ";
            sSQLSelect2 += "FETCH NEXT 10 ROWS ONLY ";

            var ListAkun = MoDbContext.Database.SqlQuery<mdlAkun>(sSQLSelect + sSQL2 + sSQLSelect2).ToList();

            IPagedList<mdlAkun> pageOrders = new StaticPagedList<mdlAkun>(ListAkun, pagenumber + 1, 10, totalCount.JUMLAH);
            if (type == "2")
            {
                return PartialView("TableAccNonaktif", pageOrders);
            }
            return PartialView("TableAccountNonaktif", pageOrders);
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
        public ActionResult RefreshTableAccountMenuWillExpired(string type, string param, int? page, string search = "")
        {
            int pagenumber = (page ?? 1) - 1;
            ViewData["searchParam"] = search;
            ViewData["LastPage"] = page;

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

            string[] getkata = search.Split(' ');
            string sSQLemail = "";
            string sSQLnama = "";
            string sSQLusername = "";
            string sSQLpartner = "";
            if (getkata.Length > 0)
            {
                if (search != "")
                {
                    for (int i = 0; i < getkata.Length; i++)
                    {
                        if (i > 0)
                        {
                            sSQLemail += " and ";
                            sSQLnama += " and ";
                            sSQLusername += " and ";
                            sSQLpartner += " and ";
                        }

                        sSQLemail += " ( a.Email like '%" + getkata[i] + "%' ) ";
                        sSQLnama += "  ( a.NamaTokoOnline like '%" + getkata[i] + "%' ) ";
                        sSQLusername += " ( a.Username like '%" + getkata[i] + "%' ) ";
                        sSQLpartner += "  ( isnull(b.Username,'') like '%" + getkata[i] + "%' ) ";

                    }
                }
            }

            string sSQLSelect = "";
            sSQLSelect += "select a.*,isnull(b.Username,'') as nmPartner,c.KETERANGAN as Subscription ";
            string sSQLCount = "";
            sSQLCount += "SELECT COUNT(AccountId) AS JUMLAH ";
            string sSQL2 = "";
            sSQL2 += "FROM account a(nolock) ";
            sSQL2 += "left join partner b (nolock) on a.KODE_REFERRAL=convert(nvarchar,b.PartnerId) left join Subscription c(nolock) on a.KODE_SUBSCRIPTION=c.kode ";
            sSQL2 += "where a.TGL_SUBSCRIPTION >= '" + drTgl.ToString("yyyy-MM-dd") + "' and a.TGL_SUBSCRIPTION <= '" + sdTgl.ToString("yyyy-MM-dd") + "' and a.Status='1' ";
            if (search != "")
            {
                sSQL2 += " and ( " + sSQLemail + " or " + sSQLnama + " or " + sSQLusername + " or " + sSQLpartner + " ) ";
            }

            var minimal_harus_ada_item_untuk_current_page = (page * 10) - 9;
            var totalCount = MoDbContext.Database.SqlQuery<getTotalCount>(sSQLCount + sSQL2).Single();
            if (minimal_harus_ada_item_untuk_current_page > totalCount.JUMLAH)
            {
                pagenumber = pagenumber - 1;
            }

            string sSQLSelect2 = "";
            sSQLSelect2 += "ORDER BY TGL_DAFTAR desc ";
            sSQLSelect2 += "OFFSET " + Convert.ToString(pagenumber * 10) + " ROWS ";
            sSQLSelect2 += "FETCH NEXT 10 ROWS ONLY ";

            var ListAkun = MoDbContext.Database.SqlQuery<mdlAkun>(sSQLSelect + sSQL2 + sSQLSelect2).ToList();

            IPagedList<mdlAkun> pageOrders = new StaticPagedList<mdlAkun>(ListAkun, pagenumber + 1, 10, totalCount.JUMLAH);
            if (type == "2")
            {
                return PartialView("TableAccWillExpired", pageOrders);
            }
            return PartialView("TableAccountWillExpired", pageOrders);
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
        public ActionResult RefreshTableAccountMenuExpired(string type, string param, int? page, string search = "")
        {
            int pagenumber = (page ?? 1) - 1;
            ViewData["searchParam"] = search;
            ViewData["LastPage"] = page;

            string tgl1 = (param.Split('/')[param.Split('/').Length - 3]);
            string bln1 = (param.Split('/')[param.Split('/').Length - 2]);
            string thn1 = (param.Split('/')[param.Split('/').Length - 1]);
            string tanggal = tgl1 + '/' + bln1 + '/' + thn1;
            var perTgl = DateTime.ParseExact(tanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

            string[] getkata = search.Split(' ');
            string sSQLemail = "";
            string sSQLnama = "";
            string sSQLusername = "";
            string sSQLpartner = "";
            if (getkata.Length > 0)
            {
                if (search != "")
                {
                    for (int i = 0; i < getkata.Length; i++)
                    {
                        if (i > 0)
                        {
                            sSQLemail += " and ";
                            sSQLnama += " and ";
                            sSQLusername += " and ";
                            sSQLpartner += " and ";
                        }

                        sSQLemail += " ( a.Email like '%" + getkata[i] + "%' ) ";
                        sSQLnama += "  ( a.NamaTokoOnline like '%" + getkata[i] + "%' ) ";
                        sSQLusername += " ( a.Username like '%" + getkata[i] + "%' ) ";
                        sSQLpartner += "  ( isnull(b.Username,'') like '%" + getkata[i] + "%' ) ";

                    }
                }
            }

            string sSQLSelect = "";
            sSQLSelect += "select a.*,isnull(b.Username,'') as nmPartner,c.KETERANGAN as Subscription ";
            string sSQLCount = "";
            sSQLCount += "SELECT COUNT(AccountId) AS JUMLAH ";
            string sSQL2 = "";
            sSQL2 += "FROM account a(nolock) ";
            sSQL2 += "left join partner b (nolock) on a.KODE_REFERRAL=convert(nvarchar,b.PartnerId) left join Subscription c(nolock) on a.KODE_SUBSCRIPTION=c.kode ";
            sSQL2 += "where a.TGL_SUBSCRIPTION <= '" + perTgl.ToString("yyyy-MM-dd") + "' and a.Status='1' ";
            if (search != "")
            {
                sSQL2 += " and ( " + sSQLemail + " or " + sSQLnama + " or " + sSQLusername + " or " + sSQLpartner + " ) ";
            }

            var minimal_harus_ada_item_untuk_current_page = (page * 10) - 9;
            var totalCount = MoDbContext.Database.SqlQuery<getTotalCount>(sSQLCount + sSQL2).Single();
            if (minimal_harus_ada_item_untuk_current_page > totalCount.JUMLAH)
            {
                pagenumber = pagenumber - 1;
            }

            string sSQLSelect2 = "";
            sSQLSelect2 += "ORDER BY TGL_DAFTAR desc ";
            sSQLSelect2 += "OFFSET " + Convert.ToString(pagenumber * 10) + " ROWS ";
            sSQLSelect2 += "FETCH NEXT 10 ROWS ONLY ";

            var ListAkun = MoDbContext.Database.SqlQuery<mdlAkun>(sSQLSelect + sSQL2 + sSQLSelect2).ToList();

            IPagedList<mdlAkun> pageOrders = new StaticPagedList<mdlAkun>(ListAkun, pagenumber + 1, 10, totalCount.JUMLAH);
            if (type == "2")
            {
                return PartialView("TableAccExpired", pageOrders);
            }
            return PartialView("TableAccountExpired", pageOrders);
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

        [Route("admin/manage/addons")]
        [SessionAdminCheck]
        public ActionResult AddonsMenu()
        {
            var vm = new AddonsViewModel()
            {
                ListAddons = MoDbContext.Addons.ToList()
            };

            return View(vm);
        }

        [Route("admin/manage/custaddons")]
        [SessionAdminCheck]
        public ActionResult AddonsCustMenu()
        {
            var vm = new AddonsCustomerViewModel()
            {
                ListCustAddons = MoDbContext.Addons_Customer.ToList()
                //,
                //ListAddons = MoDbContext.Addons.ToList()
            };

            return View(vm);
        }

        [Route("admin/manage/custaddonscs")]
        [SessionAdminCheck]
        public ActionResult AddonsCustMenuCS()
        {
            var vm = new AddonsCustomerViewModel()
            {
                ListCustAddons = MoDbContext.Addons_Customer.ToList()
                //,
                //ListAddons = MoDbContext.Addons.ToList()
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
                message.Body = System.IO.File.ReadAllText("~/Content/admin/AffiliateTerms.html")
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
                //ListAccount = MoDbContext.Account.OrderByDescending(a => a.TGL_DAFTAR).ToList(),
                //ListPartner = MoDbContext.Partner.ToList()
            };
            return View(vm);
        }

        //public ActionResult RefreshAccMenu()
        //{
        //    var vm = new MenuAccount()
        //    {
        //        ListAccount = MoDbContext.Account.OrderByDescending(a => a.TGL_DAFTAR).ToList(),
        //        ListPartner = MoDbContext.Partner.ToList()
        //    };
        //    return PartialView("TableAcc", vm);
        //}
        public ActionResult RefreshAccMenu(int? page, string search = "")
        {
            int pagenumber = (page ?? 1) - 1;
            ViewData["searchParam"] = search;
            ViewData["LastPage"] = page;

            string[] getkata = search.Split(' ');
            string sSQLemail = "";
            string sSQLnama = "";
            string sSQLusername = "";
            string sSQLpartner = "";
            if (getkata.Length > 0)
            {
                if (search != "")
                {
                    for (int i = 0; i < getkata.Length; i++)
                    {
                        if (i > 0)
                        {
                            sSQLemail += " and ";
                            sSQLnama += " and ";
                            sSQLusername += " and ";
                            sSQLpartner += " and ";
                        }

                        sSQLemail += " ( a.Email like '%" + getkata[i] + "%' ) ";
                        sSQLnama += "  ( a.NamaTokoOnline like '%" + getkata[i] + "%' ) ";
                        sSQLusername += " ( a.Username like '%" + getkata[i] + "%' ) ";
                        sSQLpartner += "  ( isnull(b.Username,'') like '%" + getkata[i] + "%' ) ";

                    }
                }
            }

            string sSQLSelect = "";
            sSQLSelect += "select a.*,isnull(b.Username,'') as nmPartner,c.KETERANGAN as Subscription ";
            string sSQLCount = "";
            sSQLCount += "SELECT COUNT(AccountId) AS JUMLAH ";
            string sSQL2 = "";
            sSQL2 += "FROM account a(nolock) ";
            sSQL2 += "left join partner b (nolock) on a.KODE_REFERRAL=convert(nvarchar,b.PartnerId) left join Subscription c(nolock) on a.KODE_SUBSCRIPTION=c.kode ";
            //sSQL2 += "where LEVEL = '2' ";
            if (search != "")
            {
                sSQL2 += " where ( " + sSQLemail + " or " + sSQLnama + " or " + sSQLusername + " or " + sSQLpartner + " ) ";
            }

            var minimal_harus_ada_item_untuk_current_page = (page * 10) - 9;
            var totalCount = MoDbContext.Database.SqlQuery<getTotalCount>(sSQLCount + sSQL2).Single();
            if (minimal_harus_ada_item_untuk_current_page > totalCount.JUMLAH)
            {
                pagenumber = pagenumber - 1;
            }

            string sSQLSelect2 = "";
            sSQLSelect2 += "ORDER BY TGL_DAFTAR desc ";
            sSQLSelect2 += "OFFSET " + Convert.ToString(pagenumber * 10) + " ROWS ";
            sSQLSelect2 += "FETCH NEXT 10 ROWS ONLY ";

            var ListAkun = MoDbContext.Database.SqlQuery<mdlAkun>(sSQLSelect + sSQL2 + sSQLSelect2).ToList();

            IPagedList<mdlAkun> pageOrders = new StaticPagedList<mdlAkun>(ListAkun, pagenumber + 1, 10, totalCount.JUMLAH);
            return PartialView("TableAcc", pageOrders);
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

        // =============================================== Bagian SUPPORT (START)
        [Route("adminCS/manage/support")]
        [SessionAdminCheck]
        public ActionResult SupportMenu()
        {
            var vm = new SupportMenu()
            {
                AccountList = MoDbContext.Account.Where(p => p.Status).Select(p => p.Email).ToList(),
            };

            return View(vm);
        }

        public async Task<ActionResult> GetMarketplaceAccount(string emailAccount)
        {
            var vm = new SupportMenu()
            {
                ListTokoMPCustomers = new List<ListMarketplaces>()
            };

            if (!string.IsNullOrEmpty(emailAccount))
            {
                var accountlist = MoDbContext.Account.Where(p => p.Email == emailAccount).SingleOrDefault();
                string dbSourceEra = "";
#if (Debug_AWS)
                dbSourceEra = accountlist.DataSourcePathDebug;
#else
                dbSourceEra = accountlist.DataSourcePath;
#endif
                ErasoftDbContext = new ErasoftContext(dbSourceEra, accountlist.DatabasePathErasoft);
                
                var customer = ErasoftDbContext.ARF01.Where(m => m.NAMA != "18").OrderBy(m => m.NAMA).ToList();
                var mp = MoDbContext.Marketplaces.ToList();
                if (customer.Count > 0)
                {
                    foreach (var tbl in customer)
                    {
                        var data = new ListMarketplaces
                        {
                            cust = Convert.ToInt32(tbl.RecNum),
                            namaCust = tbl.PERSO,
                        };
                        data.namaMarket = mp.Where(m => m.IdMarket.ToString() == tbl.NAMA).FirstOrDefault().NamaMarket;

                        vm.ListTokoMPCustomers.Add(data);
                    }
                }

                //return View(vm);
                return new JsonResult { Data = new { success = true , result = vm }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            else
            {
                //return View("Error");
                return new JsonResult { Data = new { success = false }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }


        public async Task<ActionResult> ProsesUnlinkMP(string listTokoMP)
        {
            bool resultUnlink = false;
            
            if (!string.IsNullOrEmpty(listTokoMP))
            {
                string[] dataSplitToko = listTokoMP.Split('|');
                string accountEmail = dataSplitToko[0];
                string listToko = dataSplitToko[1];
                string listkodeBRG = dataSplitToko[2];
                string[] splitlistToko = listToko.Split(',');
                string[] splitlistkodeBRG = listkodeBRG.Split('^');
                List<string> listBrg = new List<string>();
                var sqlListKode = "";
                var sqlListKodeNotFound = "";

                if (!string.IsNullOrEmpty(listToko) && !string.IsNullOrEmpty(listkodeBRG))
                {
                    try
                    {
                        var accountlist = MoDbContext.Account.Where(p => p.Email == accountEmail).SingleOrDefault();
                        DatabaseSQL EDB = new DatabaseSQL(accountlist.DatabasePathErasoft);
                        string dbSourceEra = "";
#if (Debug_AWS)
                        dbSourceEra = accountlist.DataSourcePathDebug;
#else
                        dbSourceEra = accountlist.DataSourcePath;
#endif
                        ErasoftDbContext = new ErasoftContext(dbSourceEra, accountlist.DatabasePathErasoft);

                        var listdataKodeBRG = ErasoftDbContext.STF02H.Select(p => p.BRG).ToList();

                        foreach (var listKode in splitlistkodeBRG)
                        {
                            var kodeUpper = listKode.ToUpper();
                            listdataKodeBRG = listdataKodeBRG.ConvertAll(d => d.ToUpper());
                            var kodeBRGCheck = listdataKodeBRG.Contains(kodeUpper);
                            if (kodeBRGCheck)
                            {
                                sqlListKode += "'" + listKode + "',";
                                resultUnlink = true;
                                listBrg.Add(listKode);
                            }
                            else
                            {
                                sqlListKodeNotFound += listKode + ",";
                                resultUnlink = false;
                            }
                            
                        }

                        sqlListKode = sqlListKode.Substring(0, sqlListKode.Length - 1).Replace(" ", "").ToUpper();

                        foreach (var dataToko in splitlistToko)
                        {
                            EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET DISPLAY = 0, BRG_MP = '', AVALUE_34 = '', LINK_STATUS = '', LINK_ERROR = '' WHERE UPPER(BRG) IN (" + sqlListKode + ") AND IDMARKET = '" + dataToko + "' ");
                            if(listBrg.Count > 0)
                            {
                                foreach (var item in listBrg)
                                {
                                    EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TABEL_HISTORI_UNLINK (BRG, TGL, USERNAME, IDMARKET) VALUES ('"+item+"', '"+DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss")+ "', 'SUPPORT_MO', " + dataToko + ")");
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        resultUnlink = false;
                    }
                    
                }

                if (!string.IsNullOrEmpty(sqlListKodeNotFound))
                {
                    return new JsonResult { Data = new { success = resultUnlink, kodenotfound = sqlListKodeNotFound }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
                else
                {
                    return new JsonResult { Data = new { success = resultUnlink, kodenotfound = "" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
                
            }
            else
            {
                //return View("Error");
                return new JsonResult { Data = new { success = false }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }


        public async Task<ActionResult> ProsesEditKode(string listData)
        {
            bool resultEdit = false;
            var vkodebarangsudahada = "";

            if (!string.IsNullOrEmpty(listData))
            {
                string[] dataSplitToko = listData.Split('|');
                string accountEmail = dataSplitToko[0];
                string listkodeBRGBaru = dataSplitToko[1];
                string listkodeBRGLama = dataSplitToko[2];
                string[] splitlistBRGBaru = listkodeBRGBaru.Split('^');
                string[] splitlistBRGLama = listkodeBRGLama.Split('^');

                var sqlListKodeLama = "";
                var sqlListKodeBaru = "";
                int iurutan = 0;
                var vlistKodeSudahPosting = "";
                var vlistKodeMultiSKU = "";
                var vlistKodeBundling = "";
                var vkodebarangLamaTidakAda = "";
                //add by nurul 27/12/2021
                var vlistKodeLamaBaruSama = "";
                //end add by nurul 27/12/2021
                //add by nurul 22/4/2022
                var vlistKodeBrgHarusUpdateStok = "";
                //end add by nurul 22/4/2022

                if (!string.IsNullOrEmpty(listkodeBRGBaru) && !string.IsNullOrEmpty(listkodeBRGLama))
                {
                    if (splitlistBRGBaru.Length == splitlistBRGLama.Length)
                    {
                        try
                        {
                            var accountlist = MoDbContext.Account.Where(p => p.Email == accountEmail).SingleOrDefault();
                            DatabaseSQL EDB = new DatabaseSQL(accountlist.DatabasePathErasoft);
                            string dbSourceEra = "";
#if (Debug_AWS || DEBUG)
                            dbSourceEra = accountlist.DataSourcePathDebug;
#else
                            dbSourceEra = accountlist.DataSourcePath;
#endif
                            ErasoftDbContext = new ErasoftContext(dbSourceEra, accountlist.DatabasePathErasoft);



                            foreach (var listKodeBaru in splitlistBRGBaru)
                            {
                                var checkBarangBaru = ErasoftDbContext.STF02.Where(p => p.BRG.ToLower() == listKodeBaru.ToLower()).ToList();
                                var kodeBrgLamaCheck = splitlistBRGLama[iurutan].ToString();
                                var checkBarangLama = ErasoftDbContext.STF02.Where(p => p.BRG == kodeBrgLamaCheck).ToList();
                                var checkBarangVariant = ErasoftDbContext.STF02.Where(p => p.PART.ToLower() == kodeBrgLamaCheck.ToLower()).ToList();

                                //add by nurul 27/12/2021
                                if (listKodeBaru.ToUpper() != kodeBrgLamaCheck.ToUpper())
                                {
                                    //end add by nurul 27/12/2021
                                    if (checkBarangLama.Count() > 0)
                                    {
                                        if (checkBarangBaru.Count() == 0)
                                        {
                                            //var checkSI = ErasoftDbContext.SIT01B.Where(p => p.BRG == kodeBrgLamaCheck).SingleOrDefault();

                                            var resultCekSI = (from a in ErasoftDbContext.SIT01B
                                                               join b in ErasoftDbContext.SIT01A on a.NO_BUKTI equals b.NO_BUKTI
                                                               where a.BRG.ToLower() == kodeBrgLamaCheck.ToLower()
                                                               select new
                                                               {
                                                                   a.NO_BUKTI,
                                                                   a.BRG,
                                                                   b.ST_POSTING,
                                                                   //add by nurul 22/4/2022
                                                                   b.TGL,
                                                                   b.CUST
                                                                   //end add by nurul 22/4/2022
                                                               }
                                                ).ToList();

                                            var resultCekST = (from a in ErasoftDbContext.STT01B
                                                               join b in ErasoftDbContext.STT01A on a.Nobuk equals b.Nobuk
                                                               where a.Kobar.ToLower() == kodeBrgLamaCheck.ToLower()
                                                               select new
                                                               {
                                                                   a.Nobuk,
                                                                   a.Kobar,
                                                                   b.ST_Posting,
                                                                   //add by nurul 22/4/2022
                                                                   b.Tgl
                                                                   //end add by nurul 22/4/2022
                                                               }
                                                ).ToList();

                                            //add by nurul 25/5/2021
                                            var resultCekPB = (from a in ErasoftDbContext.PBT01B
                                                               join b in ErasoftDbContext.PBT01A on a.INV equals b.INV
                                                               where a.BRG.ToLower() == kodeBrgLamaCheck.ToLower()
                                                               select new
                                                               {
                                                                   a.INV,
                                                                   a.BRG,
                                                                   b.POSTING,
                                                                   //add by nurul 22/4/2022
                                                                   b.TGL,
                                                                   b.SUPP
                                                                   //end add by nurul 22/4/2022
                                                               }
                                                ).ToList();
                                            var checkResultPB = resultCekPB.Where(p => p.POSTING.Contains("Y")).ToList();

                                            var resultCekMultiSKU = (from a in ErasoftDbContext.STF03C
                                                                     where a.BRG.ToLower() == kodeBrgLamaCheck.ToLower() || a.BRG_ACUAN.ToLower() == kodeBrgLamaCheck.ToLower()
                                                                     //add by nurul 27/12/2021
                                                                    || a.BRG.ToLower() == listKodeBaru.ToLower() || a.BRG_ACUAN.ToLower() == listKodeBaru.ToLower()
                                                                     //end add by nurul 27/12/2021
                                                                     select new
                                                                     {
                                                                         a.BRG_ACUAN,
                                                                         a.BRG
                                                                     }
                                                ).ToList();

                                            var resultCekBundling = (from a in ErasoftDbContext.STF03
                                                                     where a.Brg.ToLower() == kodeBrgLamaCheck.ToLower() || a.Unit.ToLower() == kodeBrgLamaCheck.ToLower()
                                                                     //add by nurul 27/12/2021
                                                                    || a.Brg.ToLower() == listKodeBaru.ToLower() || a.Unit.ToLower() == listKodeBaru.ToLower()
                                                                     //end add by nurul 27/12/2021
                                                                     select new
                                                                     {
                                                                         a.Unit,
                                                                         a.Brg
                                                                     }
                                                ).ToList();
                                            //end add by nurul 25/5/2021

                                            //var checkPostingSI = ErasoftDbContext.SIT01A.Where(p => p.NO_BUKTI == checkSI.NO_BUKTI).SingleOrDefault();
                                            //var checkST = ErasoftDbContext.STT01B.Where(p => p.Kobar == kodeBrgLamaCheck).Select(p => p.Nobuk).SingleOrDefault();
                                            //var checkPostingST = ErasoftDbContext.STT01A.Where(p => p.Nobuk == checkST).SingleOrDefault();
                                            var checkResultSI = resultCekSI.Where(p => p.ST_POSTING.Contains("Y")).ToList();
                                            var checkResultST = resultCekST.Where(p => p.ST_Posting.Contains("Y")).ToList();

                                            //if (checkResultSI.Count() == 0 && checkResultST.Count() == 0)
                                            if (checkResultSI.Count() == 0 && checkResultST.Count() == 0 && checkResultST.Count() == 0)
                                            {
                                                if (resultCekMultiSKU.Count() == 0)
                                                {
                                                    if (resultCekBundling.Count() == 0)
                                                    {
                                                        //add by nurul 22/4/2022
                                                        var listBrg__ = new List<tempProsesAkhirTahun>();
                                                        var listCust__ = new List<tempProsesAkhirTahun>();
                                                        var listSupp__ = new List<tempProsesAkhirTahun>();
                                                        var tahunProses = DateTime.UtcNow.AddHours(7).Year;
                                                        var cekTglSI = resultCekSI.Where(a => a.TGL.Year < DateTime.UtcNow.AddHours(7).Year).ToList();
                                                        if (cekTglSI.Count() > 0)
                                                        {
                                                            for (int i = 2018; i < tahunProses; i++)
                                                            {
                                                                var cekPerTahun = cekTglSI.Where(a => a.TGL.Year == i).ToList();
                                                                if (cekPerTahun.Count() > 0)
                                                                {
                                                                    var getBrg = cekPerTahun.Select(a => a.BRG).Distinct().ToList();
                                                                    if (getBrg.Count() > 0)
                                                                    {
                                                                        foreach (var brg in getBrg)
                                                                        {
                                                                            listBrg__.Add(new tempProsesAkhirTahun
                                                                            {
                                                                                kode = listKodeBaru,
                                                                                tahun = i
                                                                            });
                                                                        }
                                                                    }
                                                                    var getCust = cekPerTahun.Select(a => a.CUST).Distinct().ToList();
                                                                    if (getCust.Count() > 0)
                                                                    {
                                                                        foreach (var cust in getCust)
                                                                        {
                                                                            listCust__.Add(new tempProsesAkhirTahun
                                                                            {
                                                                                kode = cust,
                                                                                tahun = i
                                                                            });
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        var cekTglPB = resultCekPB.Where(a => a.TGL.Value.Year < DateTime.UtcNow.AddHours(7).Year).ToList();
                                                        if (cekTglSI.Count() > 0)
                                                        {
                                                            for (int i = 2018; i < tahunProses; i++)
                                                            {
                                                                var cekPerTahun = cekTglPB.Where(a => a.TGL.Value.Year == i).ToList();
                                                                if (cekPerTahun.Count() > 0)
                                                                {
                                                                    var getBrg = cekPerTahun.Select(a => a.BRG).Distinct().ToList();
                                                                    if (getBrg.Count() > 0)
                                                                    {
                                                                        foreach (var brg in getBrg)
                                                                        {
                                                                            listBrg__.Add(new tempProsesAkhirTahun
                                                                            {
                                                                                kode = listKodeBaru,
                                                                                tahun = i
                                                                            });
                                                                        }
                                                                    }
                                                                    var getSupp = cekPerTahun.Select(a => a.SUPP).Distinct().ToList();
                                                                    if (getSupp.Count() > 0)
                                                                    {
                                                                        foreach (var supp in getSupp)
                                                                        {
                                                                            listSupp__.Add(new tempProsesAkhirTahun
                                                                            {
                                                                                kode = supp,
                                                                                tahun = i
                                                                            });
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        var cekTglST = resultCekST.Where(a => a.Tgl.Value.Year < DateTime.UtcNow.AddHours(7).Year).ToList();
                                                        if (cekTglSI.Count() > 0)
                                                        {
                                                            for (int i = 2018; i < tahunProses; i++)
                                                            {
                                                                var cekPerTahun = cekTglST.Where(a => a.Tgl.Value.Year == i).ToList();
                                                                if (cekPerTahun.Count() > 0)
                                                                {
                                                                    var getBrg = cekPerTahun.Select(a => a.Kobar).Distinct().ToList();
                                                                    if (getBrg.Count() > 0)
                                                                    {
                                                                        foreach (var brg in getBrg)
                                                                        {
                                                                            listBrg__.Add(new tempProsesAkhirTahun
                                                                            {
                                                                                kode = listKodeBaru,
                                                                                tahun = i
                                                                            });
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        //end add by nurul 22/4/2022

                                                        // kondisi kalau belum posting
                                                        sqlListKodeLama += "'" + listKodeBaru + "',";

                                                        EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, " " +
                                                            "update stf02 set brg='" + listKodeBaru + "' where brg ='" + kodeBrgLamaCheck + "'; " +
                                                            "update stf02h set brg ='" + listKodeBaru + "' where brg ='" + kodeBrgLamaCheck + "'; " +
                                                            "update sot01b set brg ='" + listKodeBaru + "' where brg ='" + kodeBrgLamaCheck + "'; " +
                                                            "update sit01b set brg ='" + listKodeBaru + "' where brg ='" + kodeBrgLamaCheck + "'; " +
                                                            "update stt01b set kobar ='" + listKodeBaru + "' where kobar ='" + kodeBrgLamaCheck + "'; " +
                                                            "update stt04b set brg ='" + listKodeBaru + "' where brg ='" + kodeBrgLamaCheck + "'; " +
                                                            "update pbt01b set brg ='" + listKodeBaru + "' where brg ='" + kodeBrgLamaCheck + "'; " +
                                                            "update detailpromosis set KODE_BRG ='" + listKodeBaru + "' where KODE_BRG ='" + kodeBrgLamaCheck + "'; " +
                                                            "update sot03c set brg ='" + listKodeBaru + "' where brg ='" + kodeBrgLamaCheck + "';");

                                                        string sSQL = "SELECT GD,BRG,TAHUN, ISNULL(SUM(QAWAL+(QM1+QM2+QM3+QM4+QM5+QM6+QM7+QM8+QM9+QM10+QM11+QM12)-(QK1+QK2+QK3+QK4+QK5+QK6+QK7+QK8+QK9+QK10+QK11+QK12)), 0) as JUMLAH " +
                                                                        " FROM STF08A WHERE BRG = '" + listKodeBaru + "' GROUP BY GD,BRG,TAHUN ORDER BY GD ASC";
                                                        var ListQOHPerGD = ErasoftDbContext.Database.SqlQuery<STOCK_AKHIRTAHUN>(sSQL).ToList();
                                                        double dqtyTemp = 0;
                                                        var vTahun = Convert.ToInt16(DateTime.UtcNow.AddHours(7).ToString("yyyy").ToString());
                                                        foreach (var dataStock in ListQOHPerGD)
                                                        {
                                                            if (dataStock.TAHUN != vTahun)
                                                            {
                                                                if (dataStock.TAHUN < vTahun)
                                                                {
                                                                    dqtyTemp += dataStock.JUMLAH;
                                                                }
                                                                else
                                                                {
                                                                    EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF08A SET QAWAL = '0' " +
                                                                    "WHERE BRG = '" + listKodeBaru + "' AND GD = '" + dataStock.GD + "' AND TAHUN = '" + dataStock.TAHUN + "'");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF08A SET QAWAL = '" + dqtyTemp + "' " +
                                                                    "WHERE BRG = '" + listKodeBaru + "' AND GD = '" + dataStock.GD + "' AND TAHUN = '" + vTahun + "'");

                                                                EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF08A SET QAWAL = '0' " +
                                                                    "WHERE BRG = '" + kodeBrgLamaCheck + "' AND GD = '" + dataStock.GD + "' AND TAHUN = '" + vTahun + "'");

                                                                dqtyTemp = 0;
                                                            }
                                                        }

                                                        //add by nurul 22/4/2022
                                                        for (int xy = 2018; xy < tahunProses; xy++)
                                                        {
                                                            var cekPerTahun = listBrg__.Where(a => a.tahun == xy).ToList();
                                                            if (cekPerTahun.Count() > 0)
                                                            {
                                                                ProsesAkhirTahunPerBarang(dbSourceEra, accountlist.DatabasePathErasoft, Convert.ToString(xy), listKodeBaru, "", "");
                                                                ProsesAkhirTahunPerBarang(dbSourceEra, accountlist.DatabasePathErasoft, Convert.ToString(xy), kodeBrgLamaCheck, "", "");
                                                            }
                                                            var cekPerTahunCust = listCust__.Where(a => a.tahun == xy).ToList();
                                                            var getCust = cekPerTahunCust.Select(a => a.kode).Distinct().ToList();
                                                            if (getCust.Count() > 0)
                                                            {
                                                                foreach (var kode in getCust)
                                                                {
                                                                    ProsesAkhirTahunPerBarang(dbSourceEra, accountlist.DatabasePathErasoft, Convert.ToString(xy), "", "", kode);
                                                                }
                                                            }
                                                            var cekPerTahunSupp = listSupp__.Where(a => a.tahun == xy).ToList();
                                                            var getSupp = cekPerTahunSupp.Select(a => a.kode).Distinct().ToList();
                                                            if (getSupp.Count() > 0)
                                                            {
                                                                foreach (var kode in getSupp)
                                                                {
                                                                    ProsesAkhirTahunPerBarang(dbSourceEra, accountlist.DatabasePathErasoft, Convert.ToString(xy), "", kode, "");
                                                                }
                                                            }
                                                        }
                                                        if (listBrg__.Count() > 0)
                                                        {
                                                            if (listBrg__.Count() > 0)
                                                            {
                                                                vlistKodeBrgHarusUpdateStok += "" + listKodeBaru + ", " + kodeBrgLamaCheck + ", ";
                                                            }
                                                        }
                                                        //end add by nurul 22/4/2022

                                                        resultEdit = true;
                                                    }
                                                    else
                                                    {
                                                        //kondisi kalau sudah dijadikan barang bundling/komponen 
                                                        vlistKodeBundling += "" + kodeBrgLamaCheck + ",";
                                                    }
                                                }
                                                else
                                                {
                                                    //kondisi kalau sudah dijadikan barang multi sku/brg acuan 
                                                    vlistKodeMultiSKU += "" + kodeBrgLamaCheck + ",";
                                                }
                                            }
                                            else
                                            {
                                                // kondisi kalau sudah posting
                                                vlistKodeSudahPosting += "" + kodeBrgLamaCheck + ",";
                                            }
                                        }
                                        else
                                        {
                                            // alert jika kode barang sudah ada lakukan Merge bukan Edit Kode Barang!.
                                            //vkodebarangsudahada += kodeBrgLamaCheck + " *** " + listKodeBaru + "  | ";
                                            vkodebarangsudahada += "" + listKodeBaru + ",";
                                            //return new JsonResult { Data = new { success = resultEdit, dataposting = "kode barang sudah ada lakukan Merge bukan Edit Kode Barang!." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                                        }
                                    }
                                    else
                                    {
                                        vkodebarangLamaTidakAda += "" + kodeBrgLamaCheck + ",";
                                    }

                                    if (checkBarangVariant.Count() > 0)
                                    {
                                        foreach (var barangvariant in checkBarangVariant)
                                        {
                                            //remark by nurul 21/1/2022, remark cek posting varian karna yg d ubah hanya part nya saja 
                                            //var resultCekSIVarian = (from a in ErasoftDbContext.SIT01B
                                            //                         join b in ErasoftDbContext.SIT01A on a.NO_BUKTI equals b.NO_BUKTI
                                            //                         where a.BRG.ToUpper() == barangvariant.BRG.ToString().ToUpper()
                                            //                         select new
                                            //                         {
                                            //                             a.NO_BUKTI,
                                            //                             a.BRG,
                                            //                             b.ST_POSTING
                                            //                         }
                                            //                                        ).ToList();

                                            //var resultCekSTVarian = (from a in ErasoftDbContext.STT01B
                                            //                         join b in ErasoftDbContext.STT01A on a.Nobuk equals b.Nobuk
                                            //                         where a.Kobar.ToUpper() == barangvariant.BRG.ToString().ToUpper()
                                            //                         select new
                                            //                         {
                                            //                             a.Nobuk,
                                            //                             a.Kobar,
                                            //                             b.ST_Posting
                                            //                         }
                                            //    ).ToList();

                                            ////add by nurul 25/5/2021
                                            //var resultCekPBVarian = (from a in ErasoftDbContext.PBT01B
                                            //                         join b in ErasoftDbContext.PBT01A on a.INV equals b.INV
                                            //                         where a.BRG.ToUpper() == barangvariant.BRG.ToString().ToUpper()
                                            //                         select new
                                            //                         {
                                            //                             a.INV,
                                            //                             a.BRG,
                                            //                             b.POSTING
                                            //                         }
                                            //    ).ToList();
                                            ////var resultCekMultiSKUVarian = (from a in ErasoftDbContext.STF03C
                                            ////                         where a.BRG.ToLower() == barangvariant.BRG.ToString().ToLower() || a.BRG_ACUAN.ToLower() == barangvariant.BRG.ToString().ToLower()
                                            ////                         select new
                                            ////                         {
                                            ////                             a.BRG_ACUAN,
                                            ////                             a.BRG
                                            ////                         }
                                            ////    ).ToList();

                                            ////var resultCekBundlingVarian = (from a in ErasoftDbContext.STF03
                                            ////                         where a.Brg.ToLower() == barangvariant.BRG.ToString().ToLower() || a.Unit.ToLower() == barangvariant.BRG.ToString().ToLower()
                                            ////                         select new
                                            ////                         {
                                            ////                             a.Unit,
                                            ////                             a.Brg
                                            ////                         }
                                            ////    ).ToList();

                                            //var checkResultPBVarian = resultCekPBVarian.Where(p => p.POSTING.Contains("Y")).ToList();
                                            ////end add by nurul 25/5/2021

                                            //var checkResultSIVarian = resultCekSIVarian.Where(p => p.ST_POSTING.Contains("Y")).ToList();
                                            //var checkResultSTVarian = resultCekSTVarian.Where(p => p.ST_Posting.Contains("Y")).ToList();

                                            ////if (checkResultSIVarian.Count() == 0 && checkResultSTVarian.Count() == 0)
                                            //if (checkResultSIVarian.Count() == 0 && checkResultSTVarian.Count() == 0 && checkResultPBVarian.Count() == 0)
                                            //{
                                                //end remark by nurul 21/1/2022, remark cek posting varian karna yg d ubah hanya part nya saja 

                                                //if (resultCekMultiSKUVarian.Count() == 0)
                                                //{
                                                //    if (resultCekBundlingVarian.Count() == 0)
                                                //    {
                                                // kondisi kalau belum posting
                                                sqlListKodeLama += "'" + listKodeBaru + "',";

                                                EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, " " +
                                                    "update stf02 set part='" + listKodeBaru + "' where brg ='" + barangvariant.BRG.ToString() + "'; "
                                                    );

                                                resultEdit = true;
                                            //    }
                                            //    else
                                            //    {
                                            //        //kondisi kalau sudah dijadikan barang bundling/komponen 
                                            //        vlistKodeBundling += "" + barangvariant.BRG.ToString() + ",";
                                            //    }
                                            //}
                                            //else
                                            //{
                                            //    //kondisi kalau sudah dijadikan barang multi sku/brg acuan 
                                            //    vlistKodeMultiSKU += "" + barangvariant.BRG.ToString() + ",";
                                            //}

                                            //remark by nurul 21/1/2022, remark cek posting varian karna yg d ubah hanya part nya saja 
                                            //}
                                            //else
                                            //{
                                            //    // kondisi kalau sudah posting
                                            //    vlistKodeSudahPosting += "" + barangvariant.BRG.ToString() + ",";
                                            //}
                                            //end remark by nurul 21/1/2022, remark cek posting varian karna yg d ubah hanya part nya saja 
                                        }
                                    }
                                }
                                else
                                {
                                    //alert jika kode lama dan baru sama 
                                    vlistKodeLamaBaruSama += "" + kodeBrgLamaCheck + ",";
                                }


                                iurutan += 1;
                            }

                        }
                        catch (Exception ex)
                        {
                            resultEdit = false;
                        }
                    }
                    else
                    {
                        // alert bahwa jumlah list kode tidak sama.
                        return new JsonResult { Data = new { success = resultEdit, dataposting = "Jumlah list kode barang tidak sama." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }

                }

                //return View(vm);

                //change by nurul 25/5/2021
                //if (!string.IsNullOrEmpty(vlistKodeSudahPosting))
                //{
                //    return new JsonResult { Data = new { success = resultEdit, dataposting = "Terdapat kode barang yang sudah posting : " + vlistKodeSudahPosting }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                //}
                //else if (!string.IsNullOrEmpty(vkodebarangsudahada))
                //{
                //    return new JsonResult { Data = new { success = resultEdit, dataposting = "Terdapat kode barang sudah ada." + vkodebarangsudahada }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                //}
                //else
                //{
                //    return new JsonResult { Data = new { success = resultEdit, dataposting = "" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                //}
                var errors = "";
                //add by nurul 27/12/2021
                if (!string.IsNullOrEmpty(vlistKodeLamaBaruSama))
                {
                    errors = errors + " Terdapat kode barang yang sama awal dan tujuannya : " + vlistKodeLamaBaruSama + System.Environment.NewLine;
                }
                //end add by nurul 27/12/2021
                if (!string.IsNullOrEmpty(vkodebarangLamaTidakAda))
                {
                    errors = errors + " Terdapat kode barang lama yang tidak ada." + vkodebarangLamaTidakAda + System.Environment.NewLine;
                }
                if (!string.IsNullOrEmpty(vkodebarangsudahada))
                {
                    errors = errors + " Terdapat kode barang baru yang sudah ada." + vkodebarangsudahada + System.Environment.NewLine;
                }
                if (!string.IsNullOrEmpty(vlistKodeSudahPosting))
                {
                    errors = errors + " Terdapat kode barang yang sudah posting : " + vlistKodeSudahPosting + System.Environment.NewLine;
                }
                
                if (!string.IsNullOrEmpty(vlistKodeMultiSKU))
                {
                    errors = errors + " Terdapat kode barang yang merupakan barang acuan / multi sku : " + vlistKodeMultiSKU + System.Environment.NewLine;
                }
                if (!string.IsNullOrEmpty(vlistKodeBundling))
                {
                    errors = errors + " Terdapat kode barang yang merupakan barang bundling / komponen : " + vlistKodeBundling + System.Environment.NewLine;
                }
                //add by nurul 22/4/2022
                if (!string.IsNullOrEmpty(vlistKodeBrgHarusUpdateStok))
                {
                    errors = errors + "***Note : Terdapat kode barang yang harus dilakukan update stok : " + vlistKodeBrgHarusUpdateStok + System.Environment.NewLine;
                }
                //end add by nurul 22/4/2022

                return new JsonResult { Data = new { success = resultEdit, dataposting = errors }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                //end change by nurul 25/5/2021
            }
            else
            {
                //return View("Error");
                return new JsonResult { Data = new { success = false }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        //add by nurul 22/2/2022
        public class ListBarangEditMerge
        {
            public int no { get; set; }
            public string brgLama { get; set; }
            public string brgBaru { get; set; }
        }
        public ActionResult checkIndukSupportMenu(string listData)
        {
            var adaInduk = false;
            var Label = "";
            var result = false;
            var errors = "";
            try
            {
                if (!string.IsNullOrEmpty(listData))
                {
                    string[] dataSplitToko = listData.Split('|');
                    string accountEmail = dataSplitToko[0];
                    string listkodeBRGBaru = dataSplitToko[1];
                    string listkodeBRGLama = dataSplitToko[2];
                    string[] splitlistBRGBaru = listkodeBRGBaru.Split('^');
                    string[] splitlistBRGLama = listkodeBRGLama.Split('^');
                    if (!string.IsNullOrEmpty(listkodeBRGBaru) && !string.IsNullOrEmpty(listkodeBRGLama))
                    {
                        if (splitlistBRGBaru.Length == splitlistBRGLama.Length)
                        {
                            try
                            {
                                var accountlist = MoDbContext.Account.Where(p => p.Email == accountEmail).SingleOrDefault();
                                DatabaseSQL EDB = new DatabaseSQL(accountlist.DatabasePathErasoft);
                                string dbSourceEra = "";
#if (Debug_AWS || DEBUG)
                                dbSourceEra = accountlist.DataSourcePathDebug;
#else
                                dbSourceEra = accountlist.DataSourcePath;
#endif
                                ErasoftDbContext = new ErasoftContext(dbSourceEra, accountlist.DatabasePathErasoft);


                                var cekBrgIndukExist = ErasoftDbContext.STF02.Where(a => splitlistBRGLama.Contains(a.BRG) && a.TYPE == "4").Select(a => a.BRG).ToList();
                                if(cekBrgIndukExist.Count() > 0)
                                {
                                    var hitung = 0;
                                    var listBrgNew = new List<ListBarangEditMerge>();
                                    foreach(var barang in splitlistBRGLama)
                                    {
                                        hitung = hitung + 1;
                                        var barangtemp = new ListBarangEditMerge
                                        {
                                            no = hitung,
                                            brgBaru = splitlistBRGBaru[hitung - 1],
                                            brgLama = splitlistBRGLama[hitung - 1]
                                        };
                                        listBrgNew.Add(barangtemp);
                                    }
                                    adaInduk = true;
                                    var cekVarian = ErasoftDbContext.STF02.Where(a => cekBrgIndukExist.Contains(a.PART)).Select(s => new { s.BRG, s.PART }).ToList();
                                    
                                    foreach (var induk in cekBrgIndukExist)
                                    {
                                        var cekVarianPerInduk = cekVarian.Where(a => a.PART == induk).ToList();
                                        var cekBarangBaru = listBrgNew.Where(a => a.brgLama == induk).FirstOrDefault();
                                        var barangbaru = "";
                                        if(cekBarangBaru != null)
                                        {
                                            barangbaru = cekBarangBaru.brgBaru;
                                        }
                                        Label = Label + "Merge / edit barang induk \"" + induk + "\" ke barang induk \"" + barangbaru + "\" akan ikut mengubah struktur varian barang induk \"" + barangbaru + "\" menjadi: " + System.Environment.NewLine;
                                        //Label = Label + "Barang Induk \"" + induk + "\" jika diubah kode barangnya maka akan mengubah mapping induk pada variannya sebagai berikut : " + System.Environment.NewLine;
                                        if (cekVarianPerInduk.Count() > 0)
                                        {
                                            var nomer = 0;
                                            foreach (var varian in cekVarianPerInduk)
                                            {
                                                nomer = nomer + 1;
                                                Label = Label + nomer + ". " + varian.BRG + System.Environment.NewLine;
                                            }
                                        }
                                        Label = Label + System.Environment.NewLine;
                                    }
                                    result = true;
                                }
                                else
                                {
                                    result = true;
                                    return new JsonResult { Data = new { success = result, induk = adaInduk, label = Label, error = errors }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                                }
                            }
                            catch (Exception ex)
                            {
                                result = false;
                                errors = errors + ex.Message + System.Environment.NewLine;
                            }
                        }
                        else
                        {
                            result = false;
                            // alert bahwa jumlah list kode tidak sama.
                            errors = errors + "Jumlah list kode barang tidak sama." + System.Environment.NewLine;
                            return new JsonResult { Data = new { success = result, error = errors }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                        }
                    }
                    else
                    {
                        result = false;
                        // alert bahwa List kode barang kosong.
                        errors = errors + "List kode barang kosong" + System.Environment.NewLine;
                        return new JsonResult { Data = new { success = result, error = errors }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }
                }
                else
                {
                    result = false;
                    // alert bahwa List kode barang kosong.
                    errors = errors + "List kode barang kosong" + System.Environment.NewLine;
                    return new JsonResult { Data = new { success = result, error = errors }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
            }
            catch(Exception ex2)
            {
                result = false;
                errors = errors + ex2.Message + System.Environment.NewLine;
                return new JsonResult { Data = new { success = result, error = errors }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            return new JsonResult { Data = new { success = true, induk = adaInduk, label = Label, error= errors }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        //end add by nurul 22/2/2022

        //add by nurul 22/4/2022
        public class tempProsesAkhirTahun
        {
            public string kode { get; set; }
            public int tahun { get; set; }
        }
        //[Queue("3_general")]
        public ActionResult ProsesAkhirTahunPerBarang(string db_source, string db_name, string tahun, string brg, string supp, string cust)
        {
            try
            {
                var getIP = "";
                var getPort = "1433";
                var getFullIP = "";
                if (db_source != "" && db_source != null)
                {
                    getIP = db_source.Split('\'').First();
                    getPort = db_source.Split(new string[] { ", " }, StringSplitOptions.None).Last();
                    if (db_source.Contains("\\SQLEXPRESS,"))
                    {
                        getFullIP = db_source.Replace("\\SQLEXPRESS, ", ", ");
                    }
                    else
                    {
                        getFullIP = db_source.Replace("\"SQLEXPRESS, ", ", ");
                    }
                }
                if (db_source != "" && db_source != null)
                {
                    var RemoteMODbContext = new MoDbContext(getPort, getFullIP);

                    try
                    {
                        var cekExist = RemoteMODbContext.Database.ExecuteSqlCommand("use " + db_name);

                        //var tahunProses = Convert.ToInt16(tahun);

                        //object[] spParams = {
                        //new SqlParameter("@db_name", db_name),
                        //new SqlParameter("@THN", tahunProses)
                        //};
                        //RemoteMODbContext.Database.ExecuteSqlCommand("exec [PROSES_AKHIR_TAHUN] @db_name, @THN", spParams);


                        var tahunProses = Convert.ToInt16(tahun);
                        var brg_ = "";
                        var cust_ = "";
                        var supp_ = "";
                        if (!string.IsNullOrEmpty(brg))
                        {
                            brg_ = brg;
                        }
                        if (!string.IsNullOrEmpty(supp))
                        {
                            supp_ = supp;
                        }
                        if (!string.IsNullOrEmpty(cust))
                        {
                            cust_ = cust;
                        }

                        object[] spParams = {
                        new SqlParameter("@db_name", db_name),
                        new SqlParameter("@THN", tahunProses),
                        new SqlParameter("@BRG", brg_),
                        new SqlParameter("@SUPP", supp_),
                        new SqlParameter("@CUST", cust_),
                        };
                        //MoDbContext.Database.ExecuteSqlCommand("exec [PROSES_AKHIR_TAHUN_PERBRG] @db_name, @THN, @BRG, @SUPP, @CUST", spParams);
                        RemoteMODbContext.Database.ExecuteSqlCommand("exec [PROSES_AKHIR_TAHUN_PERBRG] @db_name, @THN, @BRG, @SUPP, @CUST", spParams);
                    }
                    catch
                    {
                        //return new JsonResult { Data = new { mo_error = "Gagal memproses akhir tahun. Database " + db_name + " tidak ditemukan." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }

                    //return new JsonResult { Data = new { mo_message = "Sukses memproses akhir tahun." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
                //return new JsonResult { Data = new { mo_error = "Gagal memproses akhir tahun. Internal Server Error." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            catch (Exception ex)
            {
                //return new JsonResult { Data = new { mo_error = "Gagal memproses akhir tahun. Internal Server Error." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            return new EmptyResult();
        }
        //end add by nurul 22/4/2022
        public async Task<ActionResult> ProsesMergeKode(string listData)
        {
            bool resultMerge = false;
            var vkodebarangtidakada = "";
            var vkodebarangharussynculang = "";

            if (!string.IsNullOrEmpty(listData))
            {
                string[] dataSplitToko = listData.Split('|');
                string accountEmail = dataSplitToko[0];
                string listkodeBRGBaru = dataSplitToko[1];
                string listkodeBRGLama = dataSplitToko[2];
                string[] splitlistBRGBaru = listkodeBRGBaru.Split('^');
                string[] splitlistBRGLama = listkodeBRGLama.Split('^');

                var sqlListKodeLama = "";
                var sqlListKodeBaru = "";
                int iurutan = 0;
                var vlistKodeSudahPosting = "";
                var vlistKodeMultiSKU = "";
                var vlistKodeBundling = "";
                //add by nurul 27/12/2021
                var vlistKodeLamaBaruSama = "";
                //end add by nurul 27/12/2021
                //add by nurul 22/4/2022
                var vlistKodeBrgHarusUpdateStok = "";
                //end add by nurul 22/4/2022

                if (!string.IsNullOrEmpty(listkodeBRGBaru) && !string.IsNullOrEmpty(listkodeBRGLama))
                {
                    if (splitlistBRGBaru.Length == splitlistBRGLama.Length)
                    {
                        try
                        {
                            var accountlist = MoDbContext.Account.Where(p => p.Email == accountEmail).SingleOrDefault();
                            DatabaseSQL EDB = new DatabaseSQL(accountlist.DatabasePathErasoft);
                            string dbSourceEra = "";
#if (Debug_AWS)
                            dbSourceEra = accountlist.DataSourcePathDebug;
#else
                            dbSourceEra = accountlist.DataSourcePath;
#endif
                            ErasoftDbContext = new ErasoftContext(dbSourceEra, accountlist.DatabasePathErasoft);


                            var checkToko = ErasoftDbContext.ARF01.ToList();
                            var checkMP = MoDbContext.Marketplaces.ToList();

                            
                            foreach (var listKodeBaru in splitlistBRGBaru)
                            {
                                var checkBarangBaru = ErasoftDbContext.STF02.Where(p => p.BRG.ToUpper() == listKodeBaru.ToUpper()).ToList();
                                var kodeBrgLamaCheck = splitlistBRGLama[iurutan].ToString();
                                var checkBarangLama = ErasoftDbContext.STF02.Where(p => p.BRG.ToUpper() == kodeBrgLamaCheck.ToUpper()).ToList();

                                //add by nurul 27/12/2021
                                if (listKodeBaru.ToUpper() != kodeBrgLamaCheck.ToUpper())
                                {
                                    //end add by nurul 27/12/2021

                                    var checkBarangMPBaru = ErasoftDbContext.STF02H.Where(p => p.BRG.ToUpper() == listKodeBaru.ToUpper()).ToList();
                                    var checkBarangMPLama = ErasoftDbContext.STF02H.Where(p => p.BRG.ToUpper() == kodeBrgLamaCheck.ToUpper()).ToList();

                                    var checkBarangVariantLama = ErasoftDbContext.STF02.Where(p => p.PART.ToUpper() == kodeBrgLamaCheck.ToUpper()).ToList();
                                    var checkBarangVariantBaru = ErasoftDbContext.STF02.Where(p => p.PART.ToUpper() == listKodeBaru.ToUpper()).ToList();

                                    if (checkBarangBaru.Count() > 0 && checkBarangLama.Count() > 0)
                                    {
                                        //var checkSI = ErasoftDbContext.SIT01B.Where(p => p.BRG == kodeBrgLamaCheck).SingleOrDefault();

                                        var resultCekSI = (from a in ErasoftDbContext.SIT01B
                                                           join b in ErasoftDbContext.SIT01A on a.NO_BUKTI equals b.NO_BUKTI
                                                           where a.BRG.ToUpper() == kodeBrgLamaCheck.ToUpper()
                                                           select new
                                                           {
                                                               a.NO_BUKTI,
                                                               a.BRG,
                                                               b.ST_POSTING,
                                                               //add by nurul 22/4/2022
                                                               b.TGL,
                                                               b.CUST
                                                               //end add by nurul 22/4/2022
                                                           }
                                            ).ToList();

                                        var resultCekST = (from a in ErasoftDbContext.STT01B
                                                           join b in ErasoftDbContext.STT01A on a.Nobuk equals b.Nobuk
                                                           where a.Kobar.ToUpper() == kodeBrgLamaCheck.ToUpper()
                                                           select new
                                                           {
                                                               a.Nobuk,
                                                               a.Kobar,
                                                               b.ST_Posting,
                                                               //add by nurul 22/4/2022
                                                               b.Tgl
                                                               //end add by nurul 22/4/2022
                                                           }
                                            ).ToList();

                                        var resultCekPB = (from a in ErasoftDbContext.PBT01B
                                                           join b in ErasoftDbContext.PBT01A on a.INV equals b.INV
                                                           where a.BRG.ToUpper() == kodeBrgLamaCheck.ToUpper()
                                                           select new
                                                           {
                                                               a.INV,
                                                               a.BRG,
                                                               b.POSTING,
                                                               //add by nurul 22/4/2022
                                                               b.TGL,
                                                               b.SUPP
                                                               //end add by nurul 22/4/2022
                                                           }
                                            ).ToList();

                                        //add by nurul 25/5/2021
                                        var resultCekMultiSKU = (from a in ErasoftDbContext.STF03C
                                                                 where a.BRG.ToLower() == kodeBrgLamaCheck.ToLower() || a.BRG_ACUAN.ToLower() == kodeBrgLamaCheck.ToLower()
                                                                 //add by nurul 27/12/2021
                                                                 || a.BRG.ToLower() == listKodeBaru.ToLower() || a.BRG_ACUAN.ToLower() == listKodeBaru.ToLower()
                                                                 //end add by nurul 27/12/2021
                                                                 select new
                                                                 {
                                                                     a.BRG_ACUAN,
                                                                     a.BRG
                                                                 }
                                            ).ToList();

                                        var resultCekBundling = (from a in ErasoftDbContext.STF03
                                                                 where a.Brg.ToLower() == kodeBrgLamaCheck.ToLower() || a.Unit.ToLower() == kodeBrgLamaCheck.ToLower()
                                                                 //add by nurul 27/12/2021
                                                                 || a.Brg.ToLower() == listKodeBaru.ToLower() || a.Unit.ToLower() == listKodeBaru.ToLower()
                                                                 //end add by nurul 27/12/2021
                                                                 select new
                                                                 {
                                                                     a.Unit,
                                                                     a.Brg
                                                                 }
                                            ).ToList();
                                        //end add by nurul 25/5/2021

                                        //var checkPostingSI = ErasoftDbContext.SIT01A.Where(p => p.NO_BUKTI == checkSI.NO_BUKTI).SingleOrDefault();
                                        //var checkST = ErasoftDbContext.STT01B.Where(p => p.Kobar == kodeBrgLamaCheck).Select(p => p.Nobuk).SingleOrDefault();
                                        //var checkPostingST = ErasoftDbContext.STT01A.Where(p => p.Nobuk == checkST).SingleOrDefault();
                                        var checkResultSI = resultCekSI.Where(p => p.ST_POSTING.Contains("Y")).ToList();
                                        var checkResultST = resultCekST.Where(p => p.ST_Posting.Contains("Y")).ToList();
                                        var checkResultPB = resultCekPB.Where(p => p.POSTING.Contains("Y")).ToList();

                                        if (checkResultSI.Count() == 0 && checkResultST.Count() == 0 && checkResultPB.Count() == 0
                                            //&& resultCekPB.Count() == 0
                                            )
                                        {
                                            if (resultCekMultiSKU.Count() == 0)
                                            {
                                                if (resultCekBundling.Count() == 0)
                                                {
                                                    // kondisi kalau belum posting
                                                    sqlListKodeLama += "'" + listKodeBaru + "',";
                                                    //var checkBarangLamaLagi = ErasoftDbContext.STF02.Where(p => p.BRG == kodeBrgLamaCheck).ToList();
                                                    //if (checkBarangLamaLagi.Count() > 0)
                                                    //{
                                                    if (checkBarangMPBaru.Count() > 0)
                                                    {
                                                        //foreach(var brgMpBaru in checkBarangMPBaru)
                                                        //{
                                                        //    if(!brgMpBaru.DISPLAY && string.IsNullOrEmpty(brgMpBaru.BRG_MP))
                                                        //    {
                                                        //        EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "DELETE FROM STF02H WHERE BRG ='" + brgMpBaru.BRG + "'");
                                                        //    }
                                                        //}                                            
                                                    }

                                                    if (checkBarangMPLama.Count() > 0)
                                                    {
                                                        foreach (var brgMpLama in checkBarangMPLama)
                                                        {
                                                            //if (!brgMpLama.DISPLAY && string.IsNullOrEmpty(brgMpLama.BRG_MP))
                                                            if (string.IsNullOrEmpty(brgMpLama.BRG_MP))
                                                            {
                                                                EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "DELETE FROM STF02H WHERE BRG ='" + brgMpLama.BRG + "' AND Recnum = " + brgMpLama.RecNum + "");
                                                            }
                                                            else if (checkBarangMPBaru.Count() > 0)
                                                            {
                                                                var dupe = checkBarangMPBaru.Where(m => m.IDMARKET == brgMpLama.IDMARKET).FirstOrDefault();
                                                                if (dupe != null)
                                                                {
                                                                    if (!string.IsNullOrEmpty(dupe.BRG_MP))
                                                                    {
                                                                        var dataToko = checkToko.Where(m => m.RecNum == dupe.IDMARKET).FirstOrDefault();
                                                                        int recm = Convert.ToInt32(dataToko.NAMA);
                                                                        var dataMp = checkMP.Where(p => p.IdMarket == recm).FirstOrDefault();
                                                                        EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "DELETE FROM STF02H WHERE Recnum = " + brgMpLama.RecNum + "");
                                                                        vkodebarangharussynculang += "kodebrg: " + listKodeBaru + " ,akun :" + dataMp.NamaMarket + "(" + dataToko.PERSO + ")  |  ";

                                                                    }
                                                                    EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "DELETE FROM STF02H WHERE Recnum = " + dupe.RecNum + "");
                                                                }
                                                            }
                                                        }

                                                    }

                                                    //}

                                                    //EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, " " +
                                                    //    "update stf02 set brg='" + listKodeBaru + "' where brg ='" + kodeBrgLamaCheck + "'; " +
                                                    //    "update stf02h set brg ='" + listKodeBaru + "' where brg ='" + kodeBrgLamaCheck + "'; " +
                                                    //    "update sot01b set brg ='" + listKodeBaru + "' where brg ='" + kodeBrgLamaCheck + "'; " +
                                                    //    "update sit01b set brg ='" + listKodeBaru + "' where brg ='" + kodeBrgLamaCheck + "'; " +
                                                    //    "update stt01b set kobar ='" + listKodeBaru + "' where kobar ='" + kodeBrgLamaCheck + "'; " +
                                                    //    "update stt04b set brg ='" + listKodeBaru + "' where brg ='" + kodeBrgLamaCheck + "'; " +
                                                    //    "update pbt01b set brg ='" + listKodeBaru + "' where brg ='" + kodeBrgLamaCheck + "'; " +
                                                    //    "update detailpromosis set KODE_BRG ='" + listKodeBaru + "' where KODE_BRG ='" + kodeBrgLamaCheck + "'; " +
                                                    //    "update sot03c set brg ='" + listKodeBaru + "' where brg ='" + kodeBrgLamaCheck + "';");

                                                    //add by nurul 22/4/2022
                                                    var listBrg__ = new List<tempProsesAkhirTahun>();
                                                    var listCust__ = new List<tempProsesAkhirTahun>();
                                                    var listSupp__ = new List<tempProsesAkhirTahun>();
                                                    var tahunProses = DateTime.UtcNow.AddHours(7).Year;
                                                    var cekTglSI = resultCekSI.Where(a => a.TGL.Year < DateTime.UtcNow.AddHours(7).Year).ToList();
                                                    if(cekTglSI.Count() > 0)
                                                    {
                                                        for (int i = 2018; i < tahunProses; i++)
                                                        {
                                                            var cekPerTahun = cekTglSI.Where(a => a.TGL.Year == i).ToList();
                                                            if(cekPerTahun.Count() > 0)
                                                            {
                                                                var getBrg = cekPerTahun.Select(a => a.BRG).Distinct().ToList();
                                                                if(getBrg.Count() > 0)
                                                                {
                                                                    foreach (var brg in getBrg)
                                                                    {
                                                                        listBrg__.Add(new tempProsesAkhirTahun
                                                                        {
                                                                            kode = listKodeBaru,
                                                                            tahun=i
                                                                        });
                                                                    }
                                                                }
                                                                var getCust = cekPerTahun.Select(a => a.CUST).Distinct().ToList();
                                                                if (getCust.Count() > 0)
                                                                {
                                                                    foreach (var cust in getCust)
                                                                    {
                                                                        listCust__.Add(new tempProsesAkhirTahun
                                                                        {
                                                                            kode = cust,
                                                                            tahun = i
                                                                        });
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    var cekTglPB = resultCekPB.Where(a => a.TGL.Value.Year < DateTime.UtcNow.AddHours(7).Year).ToList();
                                                    if (cekTglSI.Count() > 0)
                                                    {
                                                        for (int i = 2018; i < tahunProses; i++)
                                                        {
                                                            var cekPerTahun = cekTglPB.Where(a => a.TGL.Value.Year == i).ToList();
                                                            if (cekPerTahun.Count() > 0)
                                                            {
                                                                var getBrg = cekPerTahun.Select(a => a.BRG).Distinct().ToList();
                                                                if (getBrg.Count() > 0)
                                                                {
                                                                    foreach (var brg in getBrg)
                                                                    {
                                                                        listBrg__.Add(new tempProsesAkhirTahun
                                                                        {
                                                                            kode = listKodeBaru,
                                                                            tahun = i
                                                                        });
                                                                    }
                                                                }
                                                                var getSupp = cekPerTahun.Select(a => a.SUPP).Distinct().ToList();
                                                                if (getSupp.Count() > 0)
                                                                {
                                                                    foreach (var supp in getSupp)
                                                                    {
                                                                        listSupp__.Add(new tempProsesAkhirTahun
                                                                        {
                                                                            kode = supp,
                                                                            tahun = i
                                                                        });
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    var cekTglST = resultCekST.Where(a => a.Tgl.Value.Year < DateTime.UtcNow.AddHours(7).Year).ToList();
                                                    if (cekTglSI.Count() > 0)
                                                    {
                                                        for (int i = 2018; i < tahunProses; i++)
                                                        {
                                                            var cekPerTahun = cekTglST.Where(a => a.Tgl.Value.Year == i).ToList();
                                                            if (cekPerTahun.Count() > 0)
                                                            {
                                                                var getBrg = cekPerTahun.Select(a => a.Kobar).Distinct().ToList();
                                                                if (getBrg.Count() > 0)
                                                                {
                                                                    foreach (var brg in getBrg)
                                                                    {
                                                                        listBrg__.Add(new tempProsesAkhirTahun
                                                                        {
                                                                            kode = listKodeBaru,
                                                                            tahun = i
                                                                        });
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    //end add by nurul 22/4/2022

                                                    EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "update stf02h set brg ='" + listKodeBaru + "' where brg ='" + kodeBrgLamaCheck + "';");
                                                    EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "update stf02 set brg='" + listKodeBaru + "' where brg ='" + kodeBrgLamaCheck + "';");
                                                    EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "update sot01b set brg ='" + listKodeBaru + "' where brg ='" + kodeBrgLamaCheck + "';");
                                                    EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "update sit01b set brg ='" + listKodeBaru + "' where brg ='" + kodeBrgLamaCheck + "';");
                                                    EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "update stt01b set kobar ='" + listKodeBaru + "' where kobar ='" + kodeBrgLamaCheck + "';");
                                                    EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "update stt04b set brg ='" + listKodeBaru + "' where brg ='" + kodeBrgLamaCheck + "';");
                                                    EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "update pbt01b set brg ='" + listKodeBaru + "' where brg ='" + kodeBrgLamaCheck + "';");
                                                    EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "update detailpromosis set KODE_BRG ='" + listKodeBaru + "' where KODE_BRG ='" + kodeBrgLamaCheck + "';");
                                                    EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "update sot03c set brg ='" + listKodeBaru + "' where brg ='" + kodeBrgLamaCheck + "';");

                                                    //if (checkBarangMPBaru.Count() >= checkBarangMPLama.Count())
                                                    //{
                                                    var checkBarangLama2 = ErasoftDbContext.STF02.Where(p => p.BRG.ToUpper() == kodeBrgLamaCheck.ToUpper()).ToList();
                                                    if (checkBarangLama2.Count() > 0)
                                                    {
                                                        if (listKodeBaru != kodeBrgLamaCheck)
                                                        {
                                                            EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "DELETE FROM STF02 WHERE BRG ='" + kodeBrgLamaCheck + "';");
                                                        }
                                                    }
                                                    //}
                                                    //else
                                                    //{
                                                    //    EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "DELETE FROM STF02 WHERE BRG ='" + listKodeBaru + "';");
                                                    //}

                                                    string sSQL = "SELECT GD,BRG,TAHUN, ISNULL(SUM(QAWAL+(QM1+QM2+QM3+QM4+QM5+QM6+QM7+QM8+QM9+QM10+QM11+QM12)-(QK1+QK2+QK3+QK4+QK5+QK6+QK7+QK8+QK9+QK10+QK11+QK12)), 0) as JUMLAH " +
                                                                    " FROM STF08A(NOLOCK) WHERE BRG = '" + listKodeBaru + "' GROUP BY GD,BRG,TAHUN ORDER BY GD ASC";
                                                    var ListQOHPerGD = ErasoftDbContext.Database.SqlQuery<STOCK_AKHIRTAHUN>(sSQL).ToList();
                                                    double dqtyTemp = 0;
                                                    var vTahun = Convert.ToInt16(DateTime.UtcNow.AddHours(7).ToString("yyyy").ToString());
                                                    foreach (var dataStock in ListQOHPerGD)
                                                    {
                                                        if (dataStock.TAHUN != vTahun)
                                                        {
                                                            if (dataStock.TAHUN < vTahun)
                                                            {
                                                                dqtyTemp += dataStock.JUMLAH;
                                                            }
                                                            else
                                                            {
                                                                EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF08A SET QAWAL = '0' " +
                                                                "WHERE BRG = '" + listKodeBaru + "' AND GD = '" + dataStock.GD + "' AND TAHUN = '" + dataStock.TAHUN + "'");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF08A SET QAWAL = '" + dqtyTemp + "' " +
                                                                "WHERE BRG = '" + listKodeBaru + "' AND GD = '" + dataStock.GD + "' AND TAHUN = '" + vTahun + "'");

                                                            EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF08A SET QAWAL = '0' " +
                                                                "WHERE BRG = '" + kodeBrgLamaCheck + "' AND GD = '" + dataStock.GD + "' AND TAHUN = '" + vTahun + "'");

                                                            dqtyTemp = 0;
                                                        }
                                                    }

                                                    //add by nurul 22/4/2022
                                                    for (int xy = 2018; xy < tahunProses; xy++)
                                                    {
                                                        var cekPerTahun = listBrg__.Where(a => a.tahun == xy).ToList();
                                                        if (cekPerTahun.Count() > 0)
                                                        {
                                                            ProsesAkhirTahunPerBarang(dbSourceEra, accountlist.DatabasePathErasoft, Convert.ToString(xy), listKodeBaru, "", "");
                                                            ProsesAkhirTahunPerBarang(dbSourceEra, accountlist.DatabasePathErasoft, Convert.ToString(xy), kodeBrgLamaCheck, "", "");
                                                        }
                                                        var cekPerTahunCust = listCust__.Where(a => a.tahun == xy).ToList();
                                                        var getCust = cekPerTahunCust.Select(a => a.kode).Distinct().ToList();
                                                        if (getCust.Count() > 0)
                                                        {
                                                            foreach (var kode in getCust)
                                                            {
                                                                ProsesAkhirTahunPerBarang(dbSourceEra, accountlist.DatabasePathErasoft, Convert.ToString(xy), "", "", kode);
                                                            }
                                                        }
                                                        var cekPerTahunSupp = listSupp__.Where(a => a.tahun == xy).ToList();
                                                        var getSupp = cekPerTahunSupp.Select(a => a.kode).Distinct().ToList();
                                                        if (getSupp.Count() > 0)
                                                        {
                                                            foreach (var kode in getSupp)
                                                            {
                                                                ProsesAkhirTahunPerBarang(dbSourceEra, accountlist.DatabasePathErasoft, Convert.ToString(xy), "", kode, "");
                                                            }
                                                        }
                                                    }
                                                    if (listBrg__.Count() > 0)
                                                    {
                                                        if (listBrg__.Count() > 0)
                                                        {
                                                            vlistKodeBrgHarusUpdateStok += "" + listKodeBaru + ", " + kodeBrgLamaCheck + ", ";
                                                        }
                                                    }
                                                    //end add by nurul 22/4/2022

                                                    //var checkDuplicateBrgMP = ErasoftDbContext.STF02H.Where(p => p.BRG.ToUpper() == listKodeBaru.ToUpper()).GroupBy(x => x.IDMARKET).Where(p => p.Count() > 1).ToList();
                                                    ////foreach(var tes in checkDuplicateBrgMP)
                                                    //if (checkDuplicateBrgMP.Count() > 0)
                                                    //{
                                                    //    //var checkToko = ErasoftDbContext.ARF01.ToList();
                                                    //    //var checkMP = MoDbContext.Marketplaces.ToList();
                                                    //    foreach (var checkDup in checkDuplicateBrgMP)
                                                    //    {
                                                    //        int idmarket = Convert.ToInt32(checkDup.Key);
                                                    //        var listDuplicate = ErasoftDbContext.STF02H.Where(p => p.BRG.ToUpper() == listKodeBaru.ToUpper() && p.IDMARKET == idmarket).OrderByDescending(p => p.LINK_DATETIME).ToList();
                                                    //        if (listDuplicate.Count() > 0)
                                                    //        {
                                                    //            //bool deleted = false;
                                                    //            foreach (var itemDup in listDuplicate)
                                                    //            {
                                                    //                int recn = itemDup.IDMARKET;
                                                    //                var dataToko = checkToko.Where(p => p.RecNum == recn).SingleOrDefault();
                                                    //                int recm = Convert.ToInt32(dataToko.NAMA);
                                                    //                var dataMp = checkMP.Where(p => p.IdMarket == recm).SingleOrDefault();
                                                    //                vkodebarangharussynculang += "kodebrg: " + listKodeBaru + " (toko:" + dataToko.PERSO + " mp:" + dataMp.NamaMarket + ")  |  ";
                                                    //                //    if (deleted)
                                                    //                //    {
                                                    //                //EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "DELETE FROM STF02H WHERE BRG ='" + itemDup.BRG + "' AND Recnum = " + itemDup.RecNum + ";");
                                                    //                //    }
                                                    //                //    deleted = true;
                                                    //            }
                                                    //        }
                                                    //    }
                                                    //}

                                                    resultMerge = true;
                                                }
                                                else
                                                {
                                                    //kondisi kalau sudah dijadikan barang bundling/brg komponen 
                                                    vlistKodeBundling += "" + kodeBrgLamaCheck + ", ";
                                                }
                                            }
                                            else
                                            {
                                                //kondisi kalau sudah dijadikan barang multi sku/brg acuan 
                                                vlistKodeMultiSKU += "" + kodeBrgLamaCheck + ", ";
                                            }
                                        }
                                        else
                                        {
                                            // kondisi kalau sudah posting
                                            vlistKodeSudahPosting += kodeBrgLamaCheck + ", ";
                                        }
                                    }
                                    else
                                    {
                                        // alert jika kode barang sudah ada lakukan Merge bukan Edit Kode Barang!.
                                        vkodebarangtidakada += kodeBrgLamaCheck + " *** " + listKodeBaru + "  | ";
                                        //return new JsonResult { Data = new { success = resultMerge, dataposting = "kode barang tidak ada, lakukan Edit Kode Barang bukan Merge Kode Barang!." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                                    }

                                    if (checkBarangVariantLama.Count() > 0)
                                    {
                                        int iurutanVariant = 0;
                                        foreach (var barangvariant in checkBarangVariantLama)
                                        {
                                            //var checkBarangMPLamaVariant = ErasoftDbContext.STF02H.Where(p => p.BRG == barangvariant.BRG).ToList();
                                            //var kodeBrgBaruVariantCheck = checkBarangVariantBaru[iurutanVariant].BRG.ToString();
                                            //var checkBarangMPBaruVariant = ErasoftDbContext.STF02H.Where(p => p.BRG == kodeBrgBaruVariantCheck).ToList();

                                            //remark by nurul 21/1/2022, remark cek posting varian karna yg d ubah hanya part nya saja 
                                            //var resultCekSIVarian = (from a in ErasoftDbContext.SIT01B
                                            //                         join b in ErasoftDbContext.SIT01A on a.NO_BUKTI equals b.NO_BUKTI
                                            //                         where a.BRG.ToUpper() == barangvariant.BRG.ToString().ToUpper()
                                            //                         select new
                                            //                         {
                                            //                             a.NO_BUKTI,
                                            //                             a.BRG,
                                            //                             b.ST_POSTING
                                            //                         }
                                            //                                        ).ToList();

                                            //var resultCekSTVarian = (from a in ErasoftDbContext.STT01B
                                            //                         join b in ErasoftDbContext.STT01A on a.Nobuk equals b.Nobuk
                                            //                         where a.Kobar.ToUpper() == barangvariant.BRG.ToString().ToUpper()
                                            //                         select new
                                            //                         {
                                            //                             a.Nobuk,
                                            //                             a.Kobar,
                                            //                             b.ST_Posting
                                            //                         }
                                            //    ).ToList();

                                            ////add by nurul 25/5/2021
                                            //var resultCekPBVarian = (from a in ErasoftDbContext.PBT01B
                                            //                         join b in ErasoftDbContext.PBT01A on a.INV equals b.INV
                                            //                         where a.BRG.ToUpper() == barangvariant.BRG.ToString().ToUpper()
                                            //                         select new
                                            //                         {
                                            //                             a.INV,
                                            //                             a.BRG,
                                            //                             b.POSTING
                                            //                         }
                                            //    ).ToList();

                                            ////var resultCekMultiSKUVarian = (from a in ErasoftDbContext.STF03C
                                            ////                               where a.BRG.ToLower() == barangvariant.BRG.ToString().ToLower() || a.BRG_ACUAN.ToLower() == barangvariant.BRG.ToString().ToLower()
                                            ////                               select new
                                            ////                               {
                                            ////                                   a.BRG_ACUAN,
                                            ////                                   a.BRG
                                            ////                               }
                                            ////    ).ToList();

                                            ////var resultCekBundlingVarian = (from a in ErasoftDbContext.STF03
                                            ////                               where a.Brg.ToLower() == barangvariant.BRG.ToString().ToLower() || a.Unit.ToLower() == barangvariant.BRG.ToString().ToLower()
                                            ////                               select new
                                            ////                               {
                                            ////                                   a.Unit,
                                            ////                                   a.Brg
                                            ////                               }
                                            ////    ).ToList();

                                            //var checkResultPBVarian = resultCekPBVarian.Where(p => p.POSTING.Contains("Y")).ToList();
                                            ////end add by nurul 25/5/2021

                                            //var checkResultSIVarian = resultCekSIVarian.Where(p => p.ST_POSTING.Contains("Y")).ToList();
                                            //var checkResultSTVarian = resultCekSTVarian.Where(p => p.ST_Posting.Contains("Y")).ToList();

                                            ////if (checkResultSIVarian.Count() == 0 && checkResultSTVarian.Count() == 0)
                                            //if (checkResultSIVarian.Count() == 0 && checkResultSTVarian.Count() == 0 && checkResultPBVarian.Count() == 0)
                                            //{
                                            //end remark by nurul 21/1/2022, remark cek posting varian karna yg d ubah hanya part nya saja 

                                            //if (resultCekMultiSKUVarian.Count() == 0)
                                            //{
                                            //    if (resultCekBundlingVarian.Count() == 0)
                                            //    {
                                            // kondisi kalau belum posting
                                            sqlListKodeLama += "'" + barangvariant.BRG.ToString() + "',";

                                                //if (checkBarangMPBaruVariant.Count() >= checkBarangMPLamaVariant.Count()) {
                                                //EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "DELETE FROM STF02 WHERE BRG ='" + barangvariant.BRG + "'; DELETE FROM STF02H WHERE BRG ='" + barangvariant.BRG + "'");
                                                //}
                                                //else
                                                //{
                                                //    EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "DELETE FROM STF02 WHERE BRG ='" + kodeBrgBaruVariantCheck + "'; DELETE FROM STF02H WHERE BRG ='" + kodeBrgBaruVariantCheck + "'");
                                                //}

                                                EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, " " +
                                                    "update stf02 set part='" + listKodeBaru + "' where brg ='" + barangvariant.BRG.ToString() + "'; "
                                                    );

                                                resultMerge = true;
                                            //    }
                                            //    else
                                            //    {
                                            //        //kondisi kalau sudah dijadikan barang bundling/komponen 
                                            //        vlistKodeBundling += "" + barangvariant.BRG.ToString() + ",";
                                            //    }
                                            //}
                                            //else
                                            //{
                                            //    //kondisi kalau sudah dijadikan barang multi sku/brg acuan 
                                            //    vlistKodeMultiSKU += "" + barangvariant.BRG.ToString() + ",";
                                            //}

                                            //remark by nurul 21/1/2022, remark cek posting varian karna yg d ubah hanya part nya saja 
                                            //}
                                            //else
                                            //{
                                            //    // kondisi kalau sudah posting
                                            //    vlistKodeSudahPosting += "" + barangvariant.BRG.ToString() + ",";
                                            //}
                                            //end remark by nurul 21/1/2022, remark cek posting varian karna yg d ubah hanya part nya saja 

                                            iurutanVariant += 1;
                                        }
                                    }
                                }
                                else
                                {
                                    //alert jika kode lama dan baru sama 
                                    vlistKodeLamaBaruSama += "" + kodeBrgLamaCheck + ", ";
                                }

                                iurutan += 1;
                            }

                        }
                        catch (Exception ex)
                        {
                            resultMerge = false;
                        }
                    }
                    else
                    {
                        // alert bahwa jumlah list kode tidak sama.
                        return new JsonResult { Data = new { success = resultMerge, dataposting = "Jumlah list kode barang tidak sama." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }

                }


                //change by nurul 25/5/2021
                ////return View(vm);
                //if (!string.IsNullOrEmpty(vlistKodeSudahPosting) && !string.IsNullOrEmpty(vkodebarangharussynculang))
                //{
                //    return new JsonResult { Data = new { success = resultMerge, dataposting = "Terdapat kode barang yang sudah posting : " + vlistKodeSudahPosting  + " ||| Dan terdapat juga kode barang yang harus di sync ulang yaitu:" + vkodebarangharussynculang }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                //}
                //else if (!string.IsNullOrEmpty(vkodebarangtidakada) && !string.IsNullOrEmpty(vkodebarangharussynculang))
                //{
                //    return new JsonResult { Data = new { success = resultMerge, dataposting = " Terdapat kode barang yang tidak ada : " + vkodebarangtidakada + " |||  Dan terdapat juga kode barang yang harus di sync ulang yaitu:" + vkodebarangharussynculang }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                //}
                //else if (!string.IsNullOrEmpty(vlistKodeSudahPosting) && string.IsNullOrEmpty(vkodebarangharussynculang))
                //{
                //    return new JsonResult { Data = new { success = resultMerge, dataposting = "Terdapat kode barang yang sudah posting : " + vlistKodeSudahPosting }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                //}
                //else if (!string.IsNullOrEmpty(vkodebarangtidakada) && string.IsNullOrEmpty(vkodebarangharussynculang))
                //{
                //    return new JsonResult { Data = new { success = resultMerge, dataposting = " Terdapat kode barang yang tidak ada : " + vkodebarangtidakada }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                //}
                //else
                //{
                //    return new JsonResult { Data = new { success = resultMerge, dataposting = "" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                //}


                var errors = "";
                //add by nurul 27/12/2021
                if (!string.IsNullOrEmpty(vlistKodeLamaBaruSama))
                {
                    errors = errors + " Terdapat kode barang yang sama awal dan tujuannya : " + vlistKodeLamaBaruSama + System.Environment.NewLine;
                }
                //end add by nurul 27/12/2021
                if (!string.IsNullOrEmpty(vlistKodeSudahPosting))
                {
                    errors = errors + " Terdapat kode barang yang sudah posting : " + vlistKodeSudahPosting + System.Environment.NewLine;
                }
                if (!string.IsNullOrEmpty(vkodebarangharussynculang))
                {
                    errors = errors + " Terdapat kode barang yang harus di sync ulang : " + vkodebarangharussynculang + System.Environment.NewLine;
                }
                if (!string.IsNullOrEmpty(vkodebarangtidakada))
                {
                    errors = errors + " Terdapat kode barang yang tidak ada : " + vkodebarangtidakada + System.Environment.NewLine;
                }
                if (!string.IsNullOrEmpty(vlistKodeMultiSKU))
                {
                    errors = errors + " Terdapat kode barang yang merupakan barang acuan / multi sku : " + vlistKodeMultiSKU + System.Environment.NewLine;
                }
                if (!string.IsNullOrEmpty(vlistKodeBundling))
                {
                    errors = errors + " Terdapat kode barang yang merupakan barang bundling / komponen : " + vlistKodeBundling + System.Environment.NewLine;
                }
                //add by nurul 22/4/2022
                if (!string.IsNullOrEmpty(vlistKodeBrgHarusUpdateStok))
                {
                    errors = errors + "***Note : Terdapat kode barang yang harus dilakukan update stok : " + vlistKodeBrgHarusUpdateStok + System.Environment.NewLine;
                }
                //end add by nurul 22/4/2022

                return new JsonResult { Data = new { success = resultMerge, dataposting = errors }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                //end change by nurul 25/5/2021
            }
            else
            {
                //return View("Error");
                return new JsonResult { Data = new { success = false }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }


        public async Task<ActionResult> ProsesDeleteKode(string listData)
        {
            bool resultDelete = false;
            var vkodebarangtidakada = "";

            if (!string.IsNullOrEmpty(listData))
            {
                string[] dataSplitToko = listData.Split('|');
                string accountEmail = dataSplitToko[0];
                string listkodeBRGBaru = dataSplitToko[1];
                //string listkodeBRGLama = dataSplitToko[2];
                string[] splitlistBRGBaru = listkodeBRGBaru.Split('^');
                //string[] splitlistBRGLama = listkodeBRGLama.Split('^');

                //var sqlListKodeLama = "";
                var sqlListKodeBaru = "";
                int iurutan = 0;
                var vlistKodeSudahPosting = "";
                var vlistKodeMultiSKU = "";
                var vlistKodeBundling = "";
                //add by nurul 22/4/2022
                var vlistKodeBrgHarusUpdateStok = "";
                //end add by nurul 22/4/2022

                if (!string.IsNullOrEmpty(listkodeBRGBaru))
                {
                    if (splitlistBRGBaru.Length > 0 )
                    {
                        try
                        {
                            var accountlist = MoDbContext.Account.Where(p => p.Email == accountEmail).SingleOrDefault();
                            DatabaseSQL EDB = new DatabaseSQL(accountlist.DatabasePathErasoft);
                            string dbSourceEra = "";
#if (Debug_AWS)
                            dbSourceEra = accountlist.DataSourcePathDebug;
#else
                            dbSourceEra = accountlist.DataSourcePath;
#endif
                            ErasoftDbContext = new ErasoftContext(dbSourceEra, accountlist.DatabasePathErasoft);


                            foreach (var listKodeBaru in splitlistBRGBaru)
                            {
                                var checkBarangBaru = ErasoftDbContext.STF02.Where(p => p.BRG.ToUpper() == listKodeBaru.ToUpper()).ToList();
                                //var kodeBrgLamaCheck = splitlistBRGLama[iurutan].ToString();
                                //var checkBarangLama = ErasoftDbContext.STF02.Where(p => p.BRG == kodeBrgLamaCheck).ToList();

                                //var checkBarangMPBaru = ErasoftDbContext.STF02H.Where(p => p.BRG == listKodeBaru).ToList();
                                //var checkBarangMPLama = ErasoftDbContext.STF02H.Where(p => p.BRG == kodeBrgLamaCheck).ToList();

                                //var checkBarangVariantLama = ErasoftDbContext.STF02.Where(p => p.PART == kodeBrgLamaCheck).ToList();
                                //var checkBarangVariantBaru = ErasoftDbContext.STF02.Where(p => p.PART == listKodeBaru).ToList();

                                if (checkBarangBaru.Count() > 0)
                                {
                                    //var checkSI = ErasoftDbContext.SIT01B.Where(p => p.BRG == kodeBrgLamaCheck).SingleOrDefault();

                                    var resultCekSI = (from a in ErasoftDbContext.SIT01B
                                                       join b in ErasoftDbContext.SIT01A on a.NO_BUKTI equals b.NO_BUKTI
                                                       where a.BRG.ToUpper() == listKodeBaru.ToUpper()
                                                       select new
                                                       {
                                                           a.NO_BUKTI,
                                                           a.BRG,
                                                           b.ST_POSTING,
                                                           //add by nurul 22/4/2022
                                                           b.TGL,
                                                           b.CUST
                                                           //end add by nurul 22/4/2022
                                                       }
                                        ).ToList();

                                    var resultCekST = (from a in ErasoftDbContext.STT01B
                                                       join b in ErasoftDbContext.STT01A on a.Nobuk equals b.Nobuk
                                                       where a.Kobar.ToUpper() == listKodeBaru.ToUpper()
                                                       select new
                                                       {
                                                           a.Nobuk,
                                                           a.Kobar,
                                                           b.ST_Posting,
                                                           //add by nurul 22/4/2022
                                                           b.Tgl
                                                           //end add by nurul 22/4/2022
                                                       }
                                        ).ToList();

                                    var resultCekPB = (from a in ErasoftDbContext.PBT01B
                                                       join b in ErasoftDbContext.PBT01A on a.INV equals b.INV
                                                       where a.BRG.ToUpper() == listKodeBaru.ToUpper()
                                                       select new
                                                       {
                                                           a.INV,
                                                           a.BRG,
                                                           b.POSTING,
                                                           //add by nurul 22/4/2022
                                                           b.TGL,
                                                           b.SUPP
                                                           //end add by nurul 22/4/2022
                                                       }
                                        ).ToList();

                                    //add by nurul 25/5/2021
                                    var resultCekMultiSKU = (from a in ErasoftDbContext.STF03C
                                                             where a.BRG.ToLower() == listKodeBaru.ToLower() || a.BRG_ACUAN.ToLower() == listKodeBaru.ToLower()
                                                             select new
                                                             {
                                                                 a.BRG_ACUAN,
                                                                 a.BRG
                                                             }
                                        ).ToList();

                                    var resultCekBundling = (from a in ErasoftDbContext.STF03
                                                             where a.Brg.ToLower() == listKodeBaru.ToLower() || a.Unit.ToLower() == listKodeBaru.ToLower()
                                                             select new
                                                             {
                                                                 a.Unit,
                                                                 a.Brg
                                                             }
                                        ).ToList();
                                    //end add by nurul 25/5/2021

                                    var checkResultSI = resultCekSI.Where(p => p.ST_POSTING.Contains("Y")).ToList();
                                    var checkResultST = resultCekST.Where(p => p.ST_Posting.Contains("Y")).ToList();
                                    var checkResultPB = resultCekPB.Where(p => p.POSTING.Contains("Y")).ToList();

                                    if (checkResultSI.Count() == 0 && checkResultST.Count() == 0 && checkResultPB.Count() == 0 &&
                                        resultCekSI.Count() == 0 && resultCekST.Count() == 0 && resultCekPB.Count() == 0)
                                    {
                                        if (resultCekMultiSKU.Count() == 0)
                                        {
                                            if (resultCekBundling.Count() == 0)
                                            {
                                                //add by nurul 22/4/2022
                                                var listBrg__ = new List<tempProsesAkhirTahun>();
                                                var listCust__ = new List<tempProsesAkhirTahun>();
                                                var listSupp__ = new List<tempProsesAkhirTahun>();
                                                var tahunProses = DateTime.UtcNow.AddHours(7).Year;
                                                var cekTglSI = resultCekSI.Where(a => a.TGL.Year < DateTime.UtcNow.AddHours(7).Year).ToList();
                                                if (cekTglSI.Count() > 0)
                                                {
                                                    for (int i = 2018; i < tahunProses; i++)
                                                    {
                                                        var cekPerTahun = cekTglSI.Where(a => a.TGL.Year == i).ToList();
                                                        if (cekPerTahun.Count() > 0)
                                                        {
                                                            var getBrg = cekPerTahun.Select(a => a.BRG).Distinct().ToList();
                                                            if (getBrg.Count() > 0)
                                                            {
                                                                foreach (var brg in getBrg)
                                                                {
                                                                    listBrg__.Add(new tempProsesAkhirTahun
                                                                    {
                                                                        kode = listKodeBaru,
                                                                        tahun = i
                                                                    });
                                                                }
                                                            }
                                                            var getCust = cekPerTahun.Select(a => a.CUST).Distinct().ToList();
                                                            if (getCust.Count() > 0)
                                                            {
                                                                foreach (var cust in getCust)
                                                                {
                                                                    listCust__.Add(new tempProsesAkhirTahun
                                                                    {
                                                                        kode = cust,
                                                                        tahun = i
                                                                    });
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                var cekTglPB = resultCekPB.Where(a => a.TGL.Value.Year < DateTime.UtcNow.AddHours(7).Year).ToList();
                                                if (cekTglSI.Count() > 0)
                                                {
                                                    for (int i = 2018; i < tahunProses; i++)
                                                    {
                                                        var cekPerTahun = cekTglPB.Where(a => a.TGL.Value.Year == i).ToList();
                                                        if (cekPerTahun.Count() > 0)
                                                        {
                                                            var getBrg = cekPerTahun.Select(a => a.BRG).Distinct().ToList();
                                                            if (getBrg.Count() > 0)
                                                            {
                                                                foreach (var brg in getBrg)
                                                                {
                                                                    listBrg__.Add(new tempProsesAkhirTahun
                                                                    {
                                                                        kode = listKodeBaru,
                                                                        tahun = i
                                                                    });
                                                                }
                                                            }
                                                            var getSupp = cekPerTahun.Select(a => a.SUPP).Distinct().ToList();
                                                            if (getSupp.Count() > 0)
                                                            {
                                                                foreach (var supp in getSupp)
                                                                {
                                                                    listSupp__.Add(new tempProsesAkhirTahun
                                                                    {
                                                                        kode = supp,
                                                                        tahun = i
                                                                    });
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                var cekTglST = resultCekST.Where(a => a.Tgl.Value.Year < DateTime.UtcNow.AddHours(7).Year).ToList();
                                                if (cekTglSI.Count() > 0)
                                                {
                                                    for (int i = 2018; i < tahunProses; i++)
                                                    {
                                                        var cekPerTahun = cekTglST.Where(a => a.Tgl.Value.Year == i).ToList();
                                                        if (cekPerTahun.Count() > 0)
                                                        {
                                                            var getBrg = cekPerTahun.Select(a => a.Kobar).Distinct().ToList();
                                                            if (getBrg.Count() > 0)
                                                            {
                                                                foreach (var brg in getBrg)
                                                                {
                                                                    listBrg__.Add(new tempProsesAkhirTahun
                                                                    {
                                                                        kode = listKodeBaru,
                                                                        tahun = i
                                                                    });
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                //end add by nurul 22/4/2022

                                                // kondisi kalau belum posting
                                                sqlListKodeBaru += "'" + listKodeBaru + "',";
                                                EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, " " +
                                                    "delete from stf02h where brg ='" + listKodeBaru + "'; " +
                                                    "delete from stf02 where brg ='" + listKodeBaru + "'; " +
                                                    "delete from stt01b where kobar ='" + listKodeBaru + "'; " +
                                                    "delete from stt04b where brg ='" + listKodeBaru + "'; " +
                                                    "delete from pbt01b where brg ='" + listKodeBaru + "'; " +
                                                    "delete from detailpromosis where KODE_BRG ='" + listKodeBaru + "'; " +
                                                    "delete from sot03c where brg ='" + listKodeBaru + "';");

                                                //add by nurul 22/4/2022
                                                for (int xy = 2018; xy < tahunProses; xy++)
                                                {
                                                    var cekPerTahun = listBrg__.Where(a => a.tahun == xy).ToList();
                                                    if (cekPerTahun.Count() > 0)
                                                    {
                                                        ProsesAkhirTahunPerBarang(dbSourceEra, accountlist.DatabasePathErasoft, Convert.ToString(xy), listKodeBaru, "", "");
                                                    }
                                                    var cekPerTahunCust = listCust__.Where(a => a.tahun == xy).ToList();
                                                    var getCust = cekPerTahunCust.Select(a => a.kode).Distinct().ToList();
                                                    if (getCust.Count() > 0)
                                                    {
                                                        foreach (var kode in getCust)
                                                        {
                                                            ProsesAkhirTahunPerBarang(dbSourceEra, accountlist.DatabasePathErasoft, Convert.ToString(xy), "", "", kode);
                                                        }
                                                    }
                                                    var cekPerTahunSupp = listSupp__.Where(a => a.tahun == xy).ToList();
                                                    var getSupp = cekPerTahunSupp.Select(a => a.kode).Distinct().ToList();
                                                    if (getSupp.Count() > 0)
                                                    {
                                                        foreach (var kode in getSupp)
                                                        {
                                                            ProsesAkhirTahunPerBarang(dbSourceEra, accountlist.DatabasePathErasoft, Convert.ToString(xy), "", kode, "");
                                                        }
                                                    }
                                                }
                                                if (listBrg__.Count() > 0)
                                                {
                                                    if (listBrg__.Count() > 0)
                                                    {
                                                        vlistKodeBrgHarusUpdateStok += "" + listKodeBaru + ", ";
                                                    }
                                                }
                                                //end add by nurul 22/4/2022

                                                resultDelete = true;
                                            }
                                            else
                                            {
                                                //kondisi kalau sudah dijadikan barang bundling/brg komponen 
                                                vlistKodeBundling += "" + listKodeBaru + ",";
                                            }
                                        }
                                        else
                                        {
                                            //kondisi kalau sudah dijadikan barang multi sku/brg acuan 
                                            vlistKodeMultiSKU += "" + listKodeBaru + ",";
                                        }
                                    }
                                    else
                                    {
                                        // kondisi kalau sudah posting
                                        vlistKodeSudahPosting += listKodeBaru + " ,";
                                    }
                                }
                                else
                                {
                                    // alert jika kode barang sudah ada lakukan Merge bukan Edit Kode Barang!.
                                    vkodebarangtidakada += listKodeBaru + "| ";
                                    //return new JsonResult { Data = new { success = resultMerge, dataposting = "kode barang tidak ada, lakukan Edit Kode Barang bukan Merge Kode Barang!." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                                }


                                //if (checkBarangVariantLama.Count() > 0)
                                //{
                                //    int iurutanVariant = 0;
                                //    foreach (var barangvariant in checkBarangVariantLama)
                                //    {
                                //        //var checkBarangMPLamaVariant = ErasoftDbContext.STF02H.Where(p => p.BRG == barangvariant.BRG).ToList();
                                //        //var kodeBrgBaruVariantCheck = checkBarangVariantBaru[iurutanVariant].BRG.ToString();
                                //        //var checkBarangMPBaruVariant = ErasoftDbContext.STF02H.Where(p => p.BRG == kodeBrgBaruVariantCheck).ToList();

                                //        var resultCekSIVarian = (from a in ErasoftDbContext.SIT01B
                                //                                 join b in ErasoftDbContext.SIT01A on a.NO_BUKTI equals b.NO_BUKTI
                                //                                 where a.BRG == barangvariant.BRG.ToString()
                                //                                 select new
                                //                                 {
                                //                                     a.NO_BUKTI,
                                //                                     a.BRG,
                                //                                     b.ST_POSTING
                                //                                 }
                                //                                                ).ToList();

                                //        var resultCekSTVarian = (from a in ErasoftDbContext.STT01B
                                //                                 join b in ErasoftDbContext.STT01A on a.Nobuk equals b.Nobuk
                                //                                 where a.Kobar == barangvariant.BRG.ToString()
                                //                                 select new
                                //                                 {
                                //                                     a.Nobuk,
                                //                                     a.Kobar,
                                //                                     b.ST_Posting
                                //                                 }
                                //            ).ToList();

                                //        var checkResultSIVarian = resultCekSIVarian.Where(p => p.ST_POSTING.Contains("Y")).ToList();
                                //        var checkResultSTVarian = resultCekSTVarian.Where(p => p.ST_Posting.Contains("Y")).ToList();

                                //        if (checkResultSIVarian.Count() == 0 && checkResultSTVarian.Count() == 0)
                                //        {
                                //            // kondisi kalau belum posting
                                //            sqlListKodeBaru += "'" + barangvariant.BRG.ToString() + "',";

                                //            //if (checkBarangMPBaruVariant.Count() >= checkBarangMPLamaVariant.Count()) {
                                //            //EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "DELETE FROM STF02 WHERE BRG ='" + barangvariant.BRG + "'; DELETE FROM STF02H WHERE BRG ='" + barangvariant.BRG + "'");
                                //            //}
                                //            //else
                                //            //{
                                //            //    EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "DELETE FROM STF02 WHERE BRG ='" + kodeBrgBaruVariantCheck + "'; DELETE FROM STF02H WHERE BRG ='" + kodeBrgBaruVariantCheck + "'");
                                //            //}

                                //            EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, " " +
                                //                "update stf02 set part='" + listKodeBaru + "' where brg ='" + barangvariant.BRG.ToString() + "'; "
                                //                );

                                //            resultDelete = true;
                                //        }
                                //        else
                                //        {
                                //            // kondisi kalau sudah posting
                                //            vlistKodeSudahPosting += "" + barangvariant.BRG.ToString() + ",";
                                //        }

                                //        iurutanVariant += 1;
                                //    }
                                //}


                                iurutan += 1;
                            }

                        }
                        catch (Exception ex)
                        {
                            resultDelete = false;
                        }
                    }
                    else
                    {
                        // alert bahwa jumlah list kode tidak sama.
                        return new JsonResult { Data = new { success = resultDelete, dataposting = "Jumlah list kode barang tidak ada." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }

                }

                ////return View(vm);
                //if (!string.IsNullOrEmpty(vlistKodeSudahPosting))
                //{
                //    return new JsonResult { Data = new { success = resultDelete, dataposting = "Terdapat kode barang yang sudah posting / ada transaksi. mohon konfirmasi ulang dengan customer. : " + vlistKodeSudahPosting }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                //}
                //else if (!string.IsNullOrEmpty(vkodebarangtidakada))
                //{
                //    return new JsonResult { Data = new { success = resultDelete, dataposting = " Terdapat kode barang yang tidak ada : " + vkodebarangtidakada }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                //}
                //else
                //{
                //    return new JsonResult { Data = new { success = resultDelete, dataposting = "" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

                //}

                var errors = "";
                if (!string.IsNullOrEmpty(vlistKodeSudahPosting))
                {
                    errors = errors + " Terdapat kode barang yang sudah posting / ada transaksi. mohon konfirmasi ulang dengan customer. : " + vlistKodeSudahPosting + System.Environment.NewLine;
                }
                if (!string.IsNullOrEmpty(vkodebarangtidakada))
                {
                    errors = errors + " Terdapat kode barang yang tidak ada : " + vkodebarangtidakada + System.Environment.NewLine;
                }
                if (!string.IsNullOrEmpty(vlistKodeMultiSKU))
                {
                    errors = errors + " Terdapat kode barang yang merupakan barang acuan / multi sku : " + vlistKodeMultiSKU + System.Environment.NewLine;
                }
                if (!string.IsNullOrEmpty(vlistKodeBundling))
                {
                    errors = errors + " Terdapat kode barang yang merupakan barang bundling / komponen : " + vlistKodeBundling + System.Environment.NewLine;
                }
                //add by nurul 22/4/2022
                if (!string.IsNullOrEmpty(vlistKodeBrgHarusUpdateStok))
                {
                    errors = errors + "***Note : Terdapat kode barang yang harus dilakukan update stok : " + vlistKodeBrgHarusUpdateStok + System.Environment.NewLine;
                }
                //end add by nurul 22/4/2022

                return new JsonResult { Data = new { success = resultDelete, dataposting = errors }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                //end change by nurul 25/5/2021

            }
            else
            {
                //return View("Error");
                return new JsonResult { Data = new { success = false }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        public async Task<ActionResult> ProsesCekVariantKode(string listData)
        {
            bool resultCekVariant = false;
            var vmessage = "";

            if (!string.IsNullOrEmpty(listData))
            {
                string[] dataSplitToko = listData.Split('|');
                string accountEmail = dataSplitToko[0];
                string listkodeBRGBaru = dataSplitToko[1].ToUpper();

                var dataInduk = "";
                var dataVar = "";

                if (!string.IsNullOrEmpty(listkodeBRGBaru))
                {
                    try
                    {
                        var accountlist = MoDbContext.Account.Where(p => p.Email == accountEmail).SingleOrDefault();
                        DatabaseSQL EDB = new DatabaseSQL(accountlist.DatabasePathErasoft);
                        string dbSourceEra = "";
#if (Debug_AWS)
                            dbSourceEra = accountlist.DataSourcePathDebug;
#else
                        dbSourceEra = accountlist.DataSourcePath;
#endif
                        ErasoftDbContext = new ErasoftContext(dbSourceEra, accountlist.DatabasePathErasoft);

                        //listdataKodeBRG = listdataKodeBRG.ConvertAll(d => d.ToUpper());
                        //var kodeBRGCheck = listdataKodeBRG.Contains(kodeUpper);

                        var checkBrgStrukturInduk = ErasoftDbContext.STF02.Where(p => p.BRG.ToUpper() == listkodeBRGBaru).ToList();

                        if(checkBrgStrukturInduk.Count() > 0)
                        {
                            resultCekVariant = true;

                            if (checkBrgStrukturInduk[0].TYPE == "4")
                            {
                                dataInduk = checkBrgStrukturInduk[0].BRG.ToString();
                                var checkBrgVariant = ErasoftDbContext.STF02.Where(p => p.PART.ToUpper() == listkodeBRGBaru).ToList();
                                if (checkBrgVariant.Count() > 0)
                                {
                                    foreach (var datavariant in checkBrgVariant)
                                    {
                                        dataVar = dataVar + datavariant.BRG.ToString() + "\n";
                                    }
                                }
                                else
                                {
                                    dataVar = "";
                                }
                            }
                            else if(checkBrgStrukturInduk[0].TYPE == "3")
                            {
                                var checkBrgInduk = ErasoftDbContext.STF02.Where(p => p.BRG.ToUpper() == listkodeBRGBaru).SingleOrDefault();
                                if (!string.IsNullOrEmpty(checkBrgInduk.PART))
                                {
                                    dataInduk = checkBrgInduk.PART.ToString();
                                }
                                else
                                {
                                    dataInduk = checkBrgInduk.BRG.ToString();
                                }

                                var checkBrgVariant = ErasoftDbContext.STF02.Where(p => p.PART.ToUpper() == dataInduk.ToUpper()).ToList();
                                if (checkBrgVariant.Count() > 0)
                                {
                                    foreach (var datavariant in checkBrgVariant)
                                    {
                                        dataVar = dataVar + datavariant.BRG.ToString() + "\n";
                                    }
                                }
                                else
                                {
                                    vmessage = "Kode barang variant tidak ada.";
                                    dataVar = "";
                                }
                            }
                            else
                            {
                                dataInduk = checkBrgStrukturInduk[0].BRG;
                                vmessage = "Kode barang tidak diatur sebagai induk ataupun variant. Harap hubungi IT Support.";
                            }
                        }
                        else
                        {
                            vmessage = "Kode barang tidak ada.";
                        }                

                    }
                    catch (Exception ex)
                    {
                        resultCekVariant = false;
                    }
                }

                if (!string.IsNullOrEmpty(vmessage))
                {
                    return new JsonResult { Data = new { success = resultCekVariant, message = vmessage, datainduk = dataInduk, datavar = dataVar }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
                else
                {
                    return new JsonResult { Data = new { success = resultCekVariant, message = "", datainduk = dataInduk , datavar = dataVar }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }

            }
            else
            {
                return new JsonResult { Data = new { success = false }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        // =============================================== Bagian SUPPORT (END)

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
                //accInDb.DataSourcePath = "13.250.232.74\\SQLEXPRESS, 1433";
                //SqlConnection con = new SqlConnection("Server=localhost;Initial Catalog=master;persist security info=True;" +
                //                "user id=masteronline;password=M@ster123;");

                //add by fauzi 04/12/2020
                // IP PRIVATE STATIC SERVER T3 REGISTER
                accInDb.DataSourcePath = "172.31.20.192\\SQLEXPRESS, 1433";
                SqlConnection con = new SqlConnection("Server=172.31.20.192\\SQLEXPRESS,1433;Initial Catalog=master;persist security info=True;" +
                                                      "user id=masteronline;password=M@ster123;");
#elif Debug_AWS
                //add by fauzi 29 Januari 2020
                //accInDb.DataSourcePath = "13.250.232.74\\SQLEXPRESS, 1433";
                //SqlConnection con = new SqlConnection("Server=13.250.232.74\\SQLEXPRESS,1433;Initial Catalog=master;persist security info=True;" +
                //                                      "user id=masteronline;password=M@ster123;");

                //add by fauzi 29 Januari 2020
                // IP PRIVATE STATIC DEV
                accInDb.DataSourcePath = "172.31.20.192\\SQLEXPRESS, 1433";
                SqlConnection con = new SqlConnection("Server=172.31.20.192\\SQLEXPRESS,1433;Initial Catalog=master;persist security info=True;" +
                                                      "user id=masteronline;password=M@ster123;");
#else
                //add by fauzi 29 Januari 2020
                //accInDb.DataSourcePath = "13.251.222.53\\SQLEXPRESS, 1433";
                //SqlConnection con = new SqlConnection("Server=13.251.222.53\\SQLEXPRESS,1433;Initial Catalog=master;persist security info=True;" +
                //                                      "user id=masteronline;password=M@ster123;");

                //add by fauzi 04/12/2020
                //IP PRIVATE DEV 
                accInDb.DataSourcePath = "172.31.20.73\\SQLEXPRESS, 1433";
                SqlConnection con = new SqlConnection("Server=172.31.20.73\\SQLEXPRESS,1433;Initial Catalog=master;persist security info=True;" +
                                                      "user id=masteronline;password=M@ster123;");

#endif
                SqlCommand command = new SqlCommand(sql, con);

                con.Open();
                command.ExecuteNonQuery();
                con.Close();
                con.Dispose();
                string dbSourceEra = "";
#if (Debug_AWS)
                dbSourceEra = accInDb.DataSourcePathDebug;
#else
                dbSourceEra = accInDb.DataSourcePath;
#endif
                ErasoftContext ErasoftDbContext = new ErasoftContext(dbSourceEra, accInDb.DatabasePathErasoft);
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
                message.Body = System.IO.File.ReadAllText("~/Content/admin/AffiliateTerms.html")
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

            //add by nurul 7/1/2021
            var addOnCust = MoDbContext.Addons_Customer.Where(a => a.Account == vm.Payment.Email).ToList();
            if(addOnCust.Count() > 0)
            {
                var sSQLUpdateAddOn = "update a set TglSubscription=b.Tgl_Subscription from Addons_Customer a inner join Account b on a.account=b.email where a.account='" + vm.Payment.Email +"'";
                MoDbContext.Database.ExecuteSqlCommand(sSQLUpdateAddOn);
                MoDbContext.SaveChanges();
            }
            //end add by nurul 7/1/2021
            
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

            //return Json(account, JsonRequestBehavior.AllowGet);
            return new JsonResult { Data = account, MaxJsonLength = Int32.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
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
                    //message.Body = System.IO.File.ReadAllText("~/Content/admin/PaymentSubscription.html")
                    message.Body = System.IO.File.ReadAllText("C://inetpub//wwwroot//MasterOnline//Content//admin//PaymentSubscription.html")
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

                        try
                        {
                            var sqlStorage = new SqlServerStorage(EDBConnID);
                            var monitoringApi = sqlStorage.GetMonitoringApi();
                            var serverList = monitoringApi.Servers();

                            var lastHeartbeat = new DateTime?();
                            var getfirstserver = serverList.FirstOrDefault();
                            if (getfirstserver != null)
                            {
                                lastHeartbeat = getfirstserver.Heartbeat;
                                // add by fauzi 11 Februari 2020
                                DateTime vDatetime = lastHeartbeat.Value;
                                lastHeartbeat = vDatetime.AddHours(7);
                                // end
                            }
                            // add by fauzi 11 Februari 2020
                            DateTime vDtlastlogin = item.LAST_LOGIN_DATE.Value;
                            vDtlastlogin = vDtlastlogin.AddHours(7);
                            // end
                            var data = new HANGFIRE_SERVER_STATUS()
                            {
                                Email = item.Email,
                                //LAST_LOGIN_DATE = item.LAST_LOGIN_DATE, // remark by fauzi 11 Februari 2020
                                LAST_LOGIN_DATE = vDtlastlogin,
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
                            #region function lama
                            ////string sSQL = "select 'Stok' as tipe,count(*) jumlah from hangfire.job (nolock) where statename='Enqueued' and InvocationData like '%StokControllerJob%' " + System.Environment.NewLine;
                            //string sSQL = "select 'Stok' as tipe,count(*) jumlah from hangfire.job (nolock) where statename='Enqueued' and InvocationData like '%StokControllerJob%' and InvocationData not like '%updateBruto%'" + System.Environment.NewLine;
                            //sSQL += "union all" + System.Environment.NewLine;
                            ////change 2 aug 2021, pisahkan job get order complete shopee dan lzd
                            ////sSQL += "select 'Order' as tipe,count(*) jumlah from hangfire.job (nolock) where statename='Enqueued' and InvocationData like '%Order%'" + System.Environment.NewLine;
                            ////change by nurul 9/8/2021, pisahkan job get order complete jdid
                            ////sSQL += "select 'Order' as tipe,count(*) jumlah from hangfire.job (nolock) where statename='Enqueued' and InvocationData like '%Order%' and InvocationData not like '%GetOrdersToUpdateMO%' and InvocationData not like '%ShopeeControllerJob%GetOrderByStatusCompleted%'" + System.Environment.NewLine;
                            //sSQL += "select 'Order' as tipe,count(*) jumlah from hangfire.job (nolock) where statename='Enqueued' and InvocationData like '%Order%' and InvocationData not like '%GetOrdersToUpdateMO%' and InvocationData not like '%ShopeeControllerJob%GetOrderByStatusCompleted%' and InvocationData not like '%JD_GetOrderByStatusComplete%'" + System.Environment.NewLine;
                            //sSQL += "union all" + System.Environment.NewLine;
                            ////sSQL += "select 'Manage' as tipe,count(*) jumlah from hangfire.job (nolock) where statename='Enqueued' and (InvocationData like '%GetOrdersToUpdateMO%' or InvocationData like '%ShopeeControllerJob%GetOrderByStatusCompleted%')" + System.Environment.NewLine;
                            //sSQL += "select 'Manage' as tipe,count(*) jumlah from hangfire.job (nolock) where statename='Enqueued' and (InvocationData like '%GetOrdersToUpdateMO%' or InvocationData like '%ShopeeControllerJob%GetOrderByStatusCompleted%' or InvocationData like '%JD_GetOrderByStatusComplete%')" + System.Environment.NewLine;
                            ////end change by nurul 9/8/2021, pisahkan job get order complete jdid
                            ////change 2 aug 2021, pisahkan job get order complete shopee dan lzd
                            //sSQL += "union all" + System.Environment.NewLine;
                            ////sSQL += "select 'Product' as tipe,count(*) jumlah from hangfire.job (nolock) where statename='Enqueued' and InvocationData like '%Product%'" + System.Environment.NewLine;
                            //sSQL += "select 'Product' as tipe,count(*) jumlah from hangfire.job (nolock) where statename='Enqueued' and InvocationData like '%Product%' and InvocationData not like '%stok%'" + System.Environment.NewLine;
                            //var dsCekQueue = EDB.GetDataSet("sCon", "QUEUE_COUNT", sSQL);
                            //if (dsCekQueue.Tables[0].Rows.Count > 0)
                            //{
                            //    for (int i = 0; i < dsCekQueue.Tables[0].Rows.Count; i++)
                            //    {
                            //        switch (Convert.ToString(dsCekQueue.Tables[0].Rows[i]["tipe"]))
                            //        {
                            //            case "Stok":
                            //                data.StokJobEnqueued = Convert.ToInt32(dsCekQueue.Tables[0].Rows[i]["jumlah"]);
                            //                break;
                            //            case "Order":
                            //                data.PesananJobEnqueued = Convert.ToInt32(dsCekQueue.Tables[0].Rows[i]["jumlah"]);
                            //                break;
                            //            case "Manage":
                            //                data.ManageJobEnqueued = Convert.ToInt32(dsCekQueue.Tables[0].Rows[i]["jumlah"]);
                            //                break;
                            //            case "Product":
                            //                data.CreateProductJobEnqueued = Convert.ToInt32(dsCekQueue.Tables[0].Rows[i]["jumlah"]);
                            //                break;
                            //            default:
                            //                break;
                            //        }
                            //    }
                            //}
                            #endregion
                            string sSQL = "select 'Stok' as tipe,count(*) jumlah from hangfire.job (nolock) where statename='Enqueued' and InvocationData like '%StokControllerJob%' and InvocationData not like '%updateBruto%'" + System.Environment.NewLine;
                            var dsCekQueue = EDB.GetDataSet("sCon", "QUEUE_COUNT_1", sSQL);
                            if (dsCekQueue.Tables["QUEUE_COUNT_1"].Rows.Count > 0)
                            {
                                data.StokJobEnqueued = Convert.ToInt32(dsCekQueue.Tables["QUEUE_COUNT_1"].Rows[0]["jumlah"]);
                            }
                            //sSQL = "select 'Order' as tipe,count(*) jumlah from hangfire.job (nolock) where statename='Enqueued' and InvocationData like '%Order%' and InvocationData not like '%GetOrdersToUpdateMO%' and InvocationData not like '%ShopeeControllerJob%GetOrderByStatusCompleted%' and InvocationData not like '%JD_GetOrderByStatusComplete%'" + System.Environment.NewLine;
                            sSQL = "select 'Order' as tipe,count(*) jumlah from hangfire.job (nolock) where statename='Enqueued' and InvocationData like '%getOrder%' and InvocationData not like '%GetOrdersToUpdateMO%' and InvocationData not like '%ShopeeControllerJob%GetOrderByStatusCompleted%' and InvocationData not like '%JD_GetOrderByStatusComplete%' and InvocationData not like '%GetOrderLogistics%'" + System.Environment.NewLine;
                            dsCekQueue = EDB.GetDataSet("sCon", "QUEUE_COUNT_2", sSQL);
                            if (dsCekQueue.Tables["QUEUE_COUNT_2"].Rows.Count > 0)
                            {
                                data.PesananJobEnqueued = Convert.ToInt32(dsCekQueue.Tables["QUEUE_COUNT_2"].Rows[0]["jumlah"]);
                            }
                            sSQL = "select 'Manage' as tipe,count(*) jumlah from hangfire.job (nolock) where statename='Enqueued' and (InvocationData like '%GetOrdersToUpdateMO%' or InvocationData like '%ShopeeControllerJob%GetOrderByStatusCompleted%' or InvocationData like '%JD_GetOrderByStatusComplete%' or InvocationData like '%updatekurirshopee%' or InvocationData like '%GetTrackNoShopee%')" + System.Environment.NewLine;
                            dsCekQueue = EDB.GetDataSet("sCon", "QUEUE_COUNT_3", sSQL);
                            if (dsCekQueue.Tables["QUEUE_COUNT_3"].Rows.Count > 0)
                            {
                                data.ManageJobEnqueued = Convert.ToInt32(dsCekQueue.Tables["QUEUE_COUNT_3"].Rows[0]["jumlah"]);
                            }
                            sSQL = "select 'Product' as tipe,count(*) jumlah from hangfire.job (nolock) where statename='Enqueued' and InvocationData like '%Product%' and InvocationData not like '%stok%'" + System.Environment.NewLine;
                            dsCekQueue = EDB.GetDataSet("sCon", "QUEUE_COUNT_4", sSQL);
                            if (dsCekQueue.Tables["QUEUE_COUNT_4"].Rows.Count > 0)
                            {
                                data.CreateProductJobEnqueued = Convert.ToInt32(dsCekQueue.Tables["QUEUE_COUNT_4"].Rows[0]["jumlah"]);
                            }
                            pageContent.Add(data);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.ToString().Contains("Cannot open database")) {
                                DateTime vDtlastlogin = item.LAST_LOGIN_DATE.Value;
                                vDtlastlogin = vDtlastlogin.AddHours(7);
                                var data = new HANGFIRE_SERVER_STATUS()
                                {
                                    Email = item.Email,
                                    //LAST_LOGIN_DATE = item.LAST_LOGIN_DATE, // remark by fauzi 11 Februari 2020
                                    LAST_LOGIN_DATE = vDtlastlogin,
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
        public ActionResult RenewWebhook()
        {
            var getAkunBaim = MoDbContext.Account.FirstOrDefault(a => a.Email == "baimsky@gmail.com");
            ErasoftDbContextNew = new ErasoftContext(getAkunBaim.DatabasePathErasoft);
            var partnerDb = ErasoftDbContextNew.PARTNER_API.FirstOrDefault(p => p.PartnerId == 20007);
            string access_token = partnerDb.Access_Token;

            var datenow = DateTime.UtcNow.AddHours(7);
            var session = ErasoftDbContextNew.PARTNER_API.FirstOrDefault(p => datenow < p.Session_ExpiredDate && p.PartnerId == 20007);
            if (session == null)
            {
                string refreshTokenExc = RefreshToken("baimsky@gmail.com", partnerDb.Access_Token, partnerDb.Refresh_Token, partnerDb.Session);
                if (refreshTokenExc.Contains("Error : "))
                {
                    return Json(refreshTokenExc, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    access_token = refreshTokenExc;
                }
            }

            string currentSession = dbCheckSession(access_token, partnerDb.Session);
            if (currentSession.Contains("Error : "))
            {
                return Json(currentSession, JsonRequestBehavior.AllowGet);
            }

            string url = "https://account.accurate.id/api/webhook-renew.do";

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);

            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", "Bearer " + access_token);
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
            myReq.ContentLength = 0;

            string responseFromServer = "";

            try
            {
                using (WebResponse response = myReq.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseFromServer = reader.ReadToEnd();
                    }
                }
                return Json(responseFromServer, JsonRequestBehavior.AllowGet);

            }
            catch (WebException e)
            {
                string err = "";
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        err = sr.ReadToEnd();
                        string error = Newtonsoft.Json.JsonConvert.SerializeObject(err);

                        return Json(error, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(e.Message, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public string dbRefreshSession(string access_token, string session_token, string id_db)
        {
            var getAkunBaim = MoDbContext.Account.FirstOrDefault(a => a.Email == "baimsky@gmail.com");
            ErasoftDbContextNew = new ErasoftContext(getAkunBaim.DatabasePathErasoft);

            string ret = "";
            string url = "https://account.accurate.id/api/db-refresh-session.do?id=" + id_db + "&session=" + session_token;

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);

            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", "Bearer " + access_token);
            myReq.Accept = "*/*";
            myReq.ContentType = "application/json";
            myReq.ContentLength = 0;

            string responseFromServer = "";

            try
            {
                using (WebResponse response = myReq.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseFromServer = reader.ReadToEnd();
                    }
                }
            }
            catch (WebException e)
            {
                string err = "";
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        err = sr.ReadToEnd();
                        return "Error : " + err;
                    }
                }
                else
                {
                    err = "Error : " + e.Message + " | " + e.Source + " | " + e.StackTrace;
                    return err;
                }
            }
            catch (Exception ex)
            {
                string error = "Error : " + ex.Message + ". " + ex.Source + ". " + ex.StackTrace;
                return error;
            }

            var response_session = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ResponseSession)) as ResponseSession;
            ErasoftDbContextNew.Database.ExecuteSqlCommand("UPDATE PARTNER_API SET Session = '" + response_session.d.session + "' WHERE PartnerId = 20007 ");//fs_id = 1

            var partnerDb = ErasoftDbContextNew.PARTNER_API.FirstOrDefault(p => p.PartnerId == 20007);
            return partnerDb.Session;

            //return ret;
        }

        public string dbCheckSession(string access_token, string session_token)
        {
            var getAkunBaim = MoDbContext.Account.FirstOrDefault(a => a.Email == "baimsky@gmail.com");
            ErasoftDbContextNew = new ErasoftContext(getAkunBaim.DatabasePathErasoft);
            var partnerDb = ErasoftDbContextNew.PARTNER_API.FirstOrDefault(p => p.PartnerId == 20007);

            string ret = "";
            string url = "https://account.accurate.id/api/db-check-session.do?session=" + session_token;

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);

            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", "Bearer " + access_token);
            myReq.Accept = "*/*";
            myReq.ContentType = "application/json";

            string responseFromServer = "";

            try
            {
                using (WebResponse response = myReq.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseFromServer = reader.ReadToEnd();
                    }
                }
            }
            catch (WebException e)
            {
                string err = "";
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        err = sr.ReadToEnd();
                        return "Error : " + err;
                    }
                }
                else
                {
                    err = "Error : " + e.Message + " | " + e.Source + " | " + e.StackTrace;
                    return err;
                }
            }
            catch (Exception ex)
            {
                string error = "Error : " + ex.Message + ". " + ex.Source + ". " + ex.StackTrace;
                return error;
            }

            var response_session = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(CheckSession)) as CheckSession;
            if (response_session.s == false || response_session.d == false)
            {
                string db_id = partnerDb.DatabaseId;
                string newSession = dbRefreshSession(access_token, session_token, db_id);
                return newSession;
            }
            else
            {
                return session_token;
            }

            //return ret;
        }

        public string RefreshToken(string email, string access_token, string refresh_token, string session_token)
        {
            var getAkunBaim = MoDbContext.Account.FirstOrDefault(a => a.Email == "baimsky@gmail.com");
            ErasoftDbContextNew = new ErasoftContext(getAkunBaim.DatabasePathErasoft);
            var partnerDb = ErasoftDbContextNew.PARTNER_API.FirstOrDefault(p => p.PartnerId == 20007);

            string ret = "";
            string url = "https://account.accurate.id/oauth/token";

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);

#if AWS || Debug_AWS
            var encodedData = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("b9e575d6-e1de-405a-86bd-a18a0e5c19d9" + ":" + "b3217ae03620c4403a9587023fa03ed0")); //IdSec[0].ClientId //IdSec[0].ClientSecret
#elif DEBUG || DEV
            var encodedData = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("9ba2291f-6407-4948-be90-955fa8e78f7e" + ":" + "54e4d8110aba0dd922a325658fc1599d"));
#endif

            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", "Basic " + encodedData);
            myReq.Accept = "*/*";
            myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.ContentLength = 0;

            string myData = "grant_type=refresh_token&refresh_token=" + refresh_token;
            string responseFromServer = "";

            try
            {
                myReq.ContentLength = myData.Length;
                using (var dataStream = myReq.GetRequestStream())
                {
                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
                }
                using (WebResponse response = myReq.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseFromServer = reader.ReadToEnd();
                    }
                }
            }
            catch (WebException e)
            {
                string err = "";
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        err = sr.ReadToEnd();
                        return "Error : " + err;
                    }
                }
                else
                {
                    err = "Error : " + e.Message + " | " + e.Source + " | " + e.StackTrace;
                    return err;
                }
            }
            catch (Exception ex)
            {
                string error = "Error : " + ex.Message + ". " + ex.Source + ". " + ex.StackTrace;
                return error;
            }

            try
            {
                var response_token = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(Token)) as Token;
                var date_expires = DateTime.Now.AddDays((response_token.expires_in) / 86400).ToString("yyyy-MM-dd HH:mm:ss");
                ErasoftDbContextNew.Database.ExecuteSqlCommand("UPDATE PARTNER_API SET Access_Token = '" + response_token.access_token + "', Refresh_Token = '" + response_token.refresh_token + "', Session_ExpiredDate = '" + date_expires + "' WHERE PartnerId = 20007 ");
                string currentSession = dbCheckSession(response_token.access_token, session_token);
                if (currentSession.Contains("Error : "))
                {
                    return currentSession;
                }
                return response_token.access_token;
            }
            catch (Exception ex)
            {
                string error = ex.Message + ". " + ex.Source + ". " + ex.StackTrace;
                return error;
            }
            return ret;
        }

        public class Token
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string refresh_token { get; set; }
            public int expires_in { get; set; }
            public string scope { get; set; }
        }

        public class CheckSession
        {
            public bool s { get; set; }
            public bool d { get; set; }
        }

        public class ResponseSession
        {
            public bool s { get; set; }
            public D d { get; set; }
        }

        public class D
        {
            public long dataVersion { get; set; }
            public string licenseEnd { get; set; }
            public string session { get; set; }
            public string host { get; set; }
            public string errorMessage { get; set; }
            public bool admin { get; set; }
            public int errorCode { get; set; }
            public string accessibleUntil { get; set; }
            public bool trial { get; set; }
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
                    EDB.ExecuteSQL("", System.Data.CommandType.Text, "DELETE FROM HANGFIRE.JOB WHERE STATENAME LIKE 'PROC%'");
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
        public ActionResult ActivateRecentActiveUsersV2()
        {

            //var lastYear = DateTime.UtcNow.AddYears(-1);
            //var last2Week = DateTime.UtcNow.AddHours(7).AddDays(-14);
            //var datenow = DateTime.UtcNow.AddHours(7);

            //var accountInDb = (from a in MoDbContext.Account
            //                   where
            //                   //(a.LAST_LOGIN_DATE ?? lastYear) >= last2Week
            //                   //&&
            //                   (a.TGL_SUBSCRIPTION ?? lastYear) >= datenow
            //                   orderby a.LAST_LOGIN_DATE descending
            //                   select a).ToList();
            //foreach (var item in accountInDb)
            //{
            //    AdminStartHangfireServer(item.DataSourcePath, item.DatabasePathErasoft);
            //}
            HangfireBootstrapper.Instance.ActivateHangfireServer();

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
                            //using (var connection = sqlStorage.GetConnection())
                            //{
                            //    //update semua recurring job dengan interval sesuai setting timer
                            //    foreach (var recurringJob in connection.GetRecurringJobs())
                            //    {
                            //        recurJobM.AddOrUpdate(recurringJob.Id, recurringJob.Job, Cron.MinuteInterval(interval), recurJobOpt);
                            //    }
                            //}
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
        public ActionResult AdminCheckHangfire()
        {
            string dbpathString = "";
            string ListJobStuck = "";
            //var tgl1 = DateTime.UtcNow.AddHours(7);
            try
            {
#if DEBUG || Debug_AWS
                dbpathString = "datasourcepathdebug";
#else
            dbpathString = "datasourcepath";
#endif
                var ListServer = MoDbContext.Database.SqlQuery<mo_account>("SELECT DISTINCT " + dbpathString + " FROM ACCOUNT WHERE ISNULL(" + dbpathString + ", '') <> ''").ToList();
                MoDbContext.TABEL_CHECK_HANGFIRE.RemoveRange(MoDbContext.TABEL_CHECK_HANGFIRE.ToList());
                MoDbContext.SaveChanges();
                foreach (var db in ListServer)
                {
                    var currentTime = DateTime.UtcNow;
                    //string con = "Data Source="+db+";Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#if DEBUG || Debug_AWS
                    string con = "Data Source="+db.datasourcepathdebug+";Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#else
            string con = "Data Source="+db.datasourcepath+";Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#endif
                    string sSQL = "declare @count as integer; set @count = 0; DECLARE @db_name NVARCHAR (MAX) DECLARE c_db_names CURSOR FOR SELECT name FROM sys.databases ";
                    sSQL += "WHERE name like ('erasoft_%') OPEN c_db_names FETCH c_db_names INTO @db_name WHILE @@Fetch_Status = 0 BEGIN set @count = 0;";
                    sSQL += "EXEC(' use '+ @db_name +' ; Declare @var3 int = 0; ";
                    sSQL += "select @var3 = count(*) from hangfire.job nolock where statename like ''proc%'' and createdat <= ''" + currentTime.AddHours(-6).ToString("yyyy-MM-dd HH:mm:ss") + "'' ";
                    //sSQL += "select @var3 = count(*) from hangfire.job nolock where statename like ''proc%'' ";
                    sSQL += "if @var3 > 0 begin ";
                    sSQL += "Declare @var1 int = 0; select @var1 = count(*) from hangfire.server where lastheartbeat >= ''" + currentTime.AddMinutes(-5).ToString("yyyy-MM-dd HH:mm:ss") + "'' ";
                    sSQL += "if @var1 > 0 begin ";
#if Debug_AWS || AWS
                    sSQL += "insert into [REGIS1_MO].MO.dbo.tabel_check_hangfire (DBPATHERA, ARG, TGL) ";
#else
                    sSQL += "insert into [DEV_DB].MO.dbo.tabel_check_hangfire (DBPATHERA, ARG, TGL) ";
#endif
                    sSQL += "select '''+ @db_name +''', invocationdata, max(b.CreatedAt) from hangfire.job (nolock) a inner join hangfire.state (nolock) b on a.id = b.jobid ";
                    sSQL += "where statename like ''proc%'' and name = ''Processing'' group by invocationdata; end end ')";
                    sSQL += "FETCH c_db_names INTO @db_name END CLOSE c_db_names DEALLOCATE c_db_names";
                    //MoDbContext.Database.ExecuteSqlCommand(sSQL);
                    using (SqlConnection oConnection = new SqlConnection(con))
                    {
                        oConnection.Open();
                        //oConnection.ConnectionTimeout = 300;
                        using (SqlCommand oCommand = oConnection.CreateCommand())
                        {
                            oCommand.CommandTimeout = 300;
                            oCommand.CommandText = sSQL;
                            oCommand.ExecuteNonQuery();
                        }
                    }
                }
                var listData = MoDbContext.TABEL_CHECK_HANGFIRE.ToList();
                if (listData.Count > 0)
                {
                    var ListHf = MoDbContext.Database.SqlQuery<TABEL_CHECK_HANGFIRE>("SELECT EMAIL AS DBPATHERA, ARG, B.TGL, B.RECNUM  FROM ACCOUNT A INNER JOIN TABEL_CHECK_HANGFIRE B ON A.DATABASEPATHERASOFT = B.DBPATHERA ORDER BY B.DBPATHERA").ToList();
                    var currentacc = "";
                    foreach (var hfjob in ListHf)
                    {
                        if(currentacc != hfjob.DBPATHERA)
                        {
                            ListJobStuck += "EMAIL : " + hfjob.DBPATHERA + "\n";
                        }
                        ListJobStuck += "TGL PROCESSING : " + hfjob.TGL.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "\n";
                        var resultConvertInvocation = Newtonsoft.Json.JsonConvert.DeserializeObject(hfjob.ARG, typeof(FieldInvocationData)) as FieldInvocationData;
                        var sType = resultConvertInvocation.Type.Split(',');
                        ListJobStuck += "JOB : " + sType[0] + " , " + resultConvertInvocation.Method + "\n";
                        if (currentacc != hfjob.DBPATHERA)
                        {
                            if(currentacc != "")
                            ListJobStuck += "\n===========================================================\n\n";
                            currentacc = hfjob.DBPATHERA;
                        }
                        else
                        {

                        }
                    }
                }
            }
            catch(Exception ex)
            {
                return Json(new { success = false, status = "error : " + ex.Message }, JsonRequestBehavior.AllowGet);

            }
            //var tgl2 = DateTime.UtcNow.AddHours(7);
            return Json(new { success = true, status = ListJobStuck }, JsonRequestBehavior.AllowGet);

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

                //var akun = MoDbContext.Account.Count();
                //var user = MoDbContext.User.Count();
#if (DEBUG || Debug_AWS)
                var accountInDb = (from a in MoDbContext.Account
                                   where
                                   //(a.LAST_LOGIN_DATE ?? lastYear) >= last2Week
                                   //&&
                                   (a.TGL_SUBSCRIPTION ?? lastYear) >= datenow
                                   && !string.IsNullOrEmpty(a.DatabasePathErasoft)
                                   orderby a.LAST_LOGIN_DATE descending
                                   select new { db_name = a.DatabasePathErasoft, db_source = a.DataSourcePathDebug, onlineshopname = a.NamaTokoOnline }).ToList();
#else
                var accountInDb = (from a in MoDbContext.Account
                                   where
                                   //(a.LAST_LOGIN_DATE ?? lastYear) >= last2Week
                                   //&&
                                   (a.TGL_SUBSCRIPTION ?? lastYear) >= datenow
                                   && !string.IsNullOrEmpty(a.DatabasePathErasoft)
                                   orderby a.LAST_LOGIN_DATE descending
                                   select new { db_name = a.DatabasePathErasoft, db_source = a.DataSourcePath, onlineshopname = a.NamaTokoOnline }).ToList();
#endif
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
                //change by nurul 21/12/2020
                //var RemoteMODbContext = new MoDbContext("");
                var getIP = "";
                var getPort = "1433";
                var getFullIP = "";
                if (db_source != "" && db_source != null)
                {
                    getIP = db_source.Split('\'').First();
                    getPort = db_source.Split(new string[] { ", " }, StringSplitOptions.None).Last();
                    if (db_source.Contains("\\SQLEXPRESS,"))
                    {
                        getFullIP = db_source.Replace("\\SQLEXPRESS, ", ", ");
                    }
                    else
                    {
                        getFullIP = db_source.Replace("\"SQLEXPRESS, ", ", ");
                    }
                }
                //var getIP = db_source.Split(new string[] { "\"" }, StringSplitOptions.None).First();
                //var getPort = db_source.Split(new string[] { ", " }, StringSplitOptions.None).Last();
                if (db_source != "" && db_source != null)
                {
                    var RemoteMODbContext = new MoDbContext(getPort, getFullIP);

                    //var akun = MoDbContext.Account.Count();
                    //var user = MoDbContext.User.Count();
                    //end change by nurul 21/12/2020
                    //end
                    //remark dulu biar ga keproses 
                    //RemoteMODbContext.Database.ExecuteSqlCommand("exec [PROSES_AKHIR_TAHUN] @db_name, @tahun", new SqlParameter("@db_name", db_name), new SqlParameter("@tahun", tahun));
                    try
                    {
                        var cekExist = RemoteMODbContext.Database.ExecuteSqlCommand("use " + db_name);

                        var tahunProses = Convert.ToInt16(tahun);

                        object[] spParams = {
                        new SqlParameter("@db_name", db_name),
                        new SqlParameter("@THN", tahunProses)
                        };
                        RemoteMODbContext.Database.ExecuteSqlCommand("exec [PROSES_AKHIR_TAHUN] @db_name, @THN", spParams);
                    }
                    catch
                    {
                        return new JsonResult { Data = new { mo_error = "Gagal memproses akhir tahun. Database " + db_name + " tidak ditemukan." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }

                    return new JsonResult { Data = new { mo_message = "Sukses memproses akhir tahun." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
                return new JsonResult { Data = new { mo_error = "Gagal memproses akhir tahun. Internal Server Error." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            catch (Exception ex)
            {
                return new JsonResult { Data = new { mo_error = "Gagal memproses akhir tahun. Internal Server Error." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        //add by nurul 21/12/2020
        public ActionResult ProsesAkhirTahunPreparePerServer(string tahun, string server, string[] db_name)
        {
            try
            {
                var lastYear = DateTime.UtcNow.AddYears(-1);
                var last2Week = DateTime.UtcNow.AddHours(7).AddDays(-14);
                var datenow = DateTime.UtcNow.AddHours(7);

                //var MoDbContext = new MoDbContext("");
                var getIP = "";
                var getPort = "1433";
                var getFullIP = "";
                if (server != "" && server != null)
                {
                    getIP = server.Split('\'').First();
                    getPort = server.Split(new string[] { ", " }, StringSplitOptions.None).Last();
                    if (server.Contains("\\SQLEXPRESS,"))
                    {
                        getFullIP = server.Replace("\\SQLEXPRESS, ", ", ");
                    }
                    else
                    {
                        getFullIP = server.Replace("\"SQLEXPRESS, ", ", ");
                    }
                }
                
                //var getIP = server.Split(new string[] { "\"" }, StringSplitOptions.None).First();
                //var getPort = server.Split(new string[] { ", " }, StringSplitOptions.None).Last();
                var listDB = new List<string>();
                if(db_name != null  && db_name.Count() > 0)
                {
                    listDB = db_name.ToList();
                }
                if (getIP != "")
                {
                    //var MoDbContext = new MoDbContext(getPort, getIP);
                    var MoDbContext = new MoDbContext("");
                    //var akun = MoDbContext.Account.Count();
                    //var user = MoDbContext.User.Count();

#if (DEBUG || Debug_AWS)
                    var accountInDb = (from a in MoDbContext.Account
                                       where
                                       //(a.LAST_LOGIN_DATE ?? lastYear) >= last2Week
                                       //&&
                                       (a.TGL_SUBSCRIPTION ?? lastYear) >= datenow
                                       && !string.IsNullOrEmpty(a.DatabasePathErasoft)
                                       &&
                                       listDB.Contains(a.DatabasePathErasoft)
                                       orderby a.LAST_LOGIN_DATE descending
                                       select new { db_name = a.DatabasePathErasoft, db_source = a.DataSourcePathDebug, onlineshopname = a.NamaTokoOnline }).ToList();
#else
                    var accountInDb = (from a in MoDbContext.Account
                                       where
                                       //(a.LAST_LOGIN_DATE ?? lastYear) >= last2Week
                                       //&&
                                       (a.TGL_SUBSCRIPTION ?? lastYear) >= datenow
                                       && !string.IsNullOrEmpty(a.DatabasePathErasoft)
                                       &&
                                       listDB.Contains(a.DatabasePathErasoft)
                                       orderby a.LAST_LOGIN_DATE descending
                                       select new { db_name = a.DatabasePathErasoft, db_source = a.DataSourcePath, onlineshopname = a.NamaTokoOnline }).ToList();
#endif
                    return new JsonResult { Data = new { arraydbname = accountInDb }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
                return new JsonResult { Data = new { mo_error = "Server tidak ditemukan." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            catch (Exception ex)
            {
                return new JsonResult { Data = new { mo_error = "Gagal memproses akhir tahun. Internal Server Error." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }
        public class listServer
        {
            public string IP { get; set; }
        }
        public ActionResult GetServer()
        {
            var MoDbContext = new MoDbContext("");
            //listServer = MoDbContext.Account.Where(a => !string.IsNullOrEmpty(a.DataSourcePath)).Select(a => a.DataSourcePath).Distinct().ToList();
            var listServer = new List<listServer>();
#if (AWS || DEV)
            listServer = (from a in MoDbContext.Account
                         where !string.IsNullOrEmpty(a.DataSourcePath)
                         select new listServer { IP = a.DataSourcePath }).Distinct().ToList();
#else
            listServer = (from a in MoDbContext.Account
                          where !string.IsNullOrEmpty(a.DataSourcePathDebug)
                          select new listServer { IP = a.DataSourcePathDebug }).Distinct().ToList();
#endif
            return Json(listServer, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PromptAccountUserPerServer(string server)
        {
            var vm = new PromptAccountServerAkhirTahunViewModel()
            {
                
            };
            if(server != "" && server != null)
            {
                var lastYear = DateTime.UtcNow.AddYears(-1);
                var last2Week = DateTime.UtcNow.AddHours(7).AddDays(-14);
                var datenow = DateTime.UtcNow.AddHours(7);
                //var MoDbContext = new MoDbContext("");

                var getIP = "";
                var getPort = "1433";
                var getIPPrivate = "";
                var getFullIP = "";
                if (server != "" && server != null)
                {
                    getIP = server.Split('\'').First();
                    getPort = server.Split(new string[] { ", " }, StringSplitOptions.None).Last();
                    if (server.Contains("\\SQLEXPRESS,"))
                    {
                        getFullIP = server.Replace("\\SQLEXPRESS, ", ", ");
                    }
                    else
                    {
                        getFullIP = server.Replace("\"SQLEXPRESS, ", ", ");
                    }
                }
                if (getIP != "")
                {
                    //var MoDbContext = new MoDbContext(getPort, getIP);
                    var MoDbContext = new MoDbContext("");
                    //var akun = MoDbContext.Account.Count();
                    //var user = MoDbContext.User.Count();

                    var listAkun = (from a in MoDbContext.Account
                                    where
                                    //(a.LAST_LOGIN_DATE ?? lastYear) >= last2Week
                                    //&&
                                    (a.TGL_SUBSCRIPTION ?? lastYear) >= datenow
                                    && !string.IsNullOrEmpty(a.DatabasePathErasoft)
                                    &&
                                    //a.DataSourcePath.Contains(getIPPrivate)
                                    (a.DataSourcePath.Contains(getIP) || a.DataSourcePathDebug.Contains(getIP))
                                    orderby a.LAST_LOGIN_DATE descending
                                    select new listAkunPerServer { db_name = a.DatabasePathErasoft, db_source = a.DataSourcePath, onlineshopname = a.NamaTokoOnline, email = a.Email, accountid = a.AccountId }).ToList();
                    vm.listAkun = listAkun;
                }
            }
            return PartialView("TablePromptAkunProsesAkhirTahun", vm);
        }
        //[Queue("3_general")]
        //public ActionResult ProsesAkhirTahunPerServer(string db_source, string db_name, string tahun)
        //{
        //    try
        //    {
        //        //change by fauzi 24 Januari 2020
        //        //var RemoteMODbContext = new MoDbContext(db_source);
        //        //change by nurul 21/12/2020
        //        //var RemoteMODbContext = new MoDbContext("");
        //        var getIP = "";
        //        var getPort = "1433";
        //        if (db_source != "" && db_source != null)
        //        {
        //            if (db_source.Contains("172.31.20.197") || db_source.Contains("13.250.232.74"))
        //            {
        //                getIP = "13.250.232.74";
        //            }
        //            else if ((db_source.Contains("172.31.20.200") || db_source.Contains("54.179.169.195")) && db_source.Contains("1433"))
        //            {
        //                getIP = "54.179.169.195";
        //            }
        //            else if (db_source.Contains("172.31.17.194") || db_source.Contains("52.76.44.100"))
        //            {
        //                getIP = "52.76.44.100";
        //            }
        //            else if (db_source.Contains("172.31.26.111") || db_source.Contains("54.254.98.21"))
        //            {
        //                getIP = "54.254.98.21";
        //            }
        //            else if (db_source.Contains("172.31.14.140") || db_source.Contains("18.141.161.81"))
        //            {
        //                getIP = "18.141.161.81";
        //            }
        //            else if (db_source.Contains("172.31.1.127") || db_source.Contains("13.251.64.77"))
        //            {
        //                getIP = "13.251.64.77";
        //            }
        //            else if (db_source.Contains("172.31.40.234") || db_source.Contains("54.179.0.52"))
        //            {
        //                getIP = "54.179.0.52";
        //            }
        //            else if (db_source.Contains("13.251.222.53") || db_source.Contains("13.251.222.53"))
        //            {
        //                getIP = "13.251.222.53";
        //            }
        //            else if ((db_source.Contains("54.179.169.195") || db_source.Contains("54.179.169.195")) && db_source.Contains("1444"))
        //            {
        //                getIP = "54.179.169.195";
        //                getPort = "1444";
        //            }
        //        }
        //        //var getIP = db_source.Split(new string[] { "\"" }, StringSplitOptions.None).First();
        //        //var getPort = db_source.Split(new string[] { ", " }, StringSplitOptions.None).Last();
        //        if (getIP != "")
        //        {
        //            var RemoteMODbContext = new MoDbContext(getPort, getIP);
        //            //var akun = RemoteMODbContext.Account.Count();
        //            //var user = RemoteMODbContext.User.Count();
        //            //end change by nurul 21/12/2020
        //            //end
        //            //remark dulu biar ga keproses 
        //            var tahunProses = Convert.ToInt16(tahun);

        //            object[] spParams = {
        //            new SqlParameter("@db_name", db_name),
        //            new SqlParameter("@THN", tahunProses)
        //            };
        //            RemoteMODbContext.Database.ExecuteSqlCommand("exec [PROSES_AKHIR_TAHUN] @db_name, @THN", spParams);
        //            //RemoteMODbContext.Database.ExecuteSqlCommand("exec [PROSES_AKHIR_TAHUN] @db_name, @THN", new SqlParameter("@db_name", db_name), new SqlParameter("@THN", tahunProses));
        //        }
        //        return new JsonResult { Data = new { mo_message = "Sukses memproses akhir tahun." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new JsonResult { Data = new { mo_error = "Gagal memproses akhir tahun. Internal Server Error." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        //    }
        //}
        //end add by nurul 21/12/2020

        //add by nurul 5/1/2021
        [Queue("3_general")]
        public ActionResult ProsesAkhirTahunGL(string db_source, string db_name, string tahun)
        {
            try
            {
                var getIP = "";
                var getPort = "1433";
                var getFullIP = "";
                if (db_source != "" && db_source != null)
                {
                    getIP = db_source.Split('\'').First();
                    getPort = db_source.Split(new string[] { ", " }, StringSplitOptions.None).Last();
                    if (db_source.Contains("\\SQLEXPRESS,"))
                    {
                        getFullIP = db_source.Replace("\\SQLEXPRESS, ", ", ");
                    }
                    else
                    {
                        getFullIP = db_source.Replace("\"SQLEXPRESS, ", ", ");
                    }
                }
                if (db_source != "" && db_source != null)
                {
                    var RemoteMODbContext = new MoDbContext(getPort, getFullIP);
                    
                    try
                    {
                        var cekExist = RemoteMODbContext.Database.ExecuteSqlCommand("use " + db_name);

                        var tahunProses = Convert.ToInt16(tahun);

                        object[] spParams = {
                        new SqlParameter("@db_name", db_name),
                        new SqlParameter("@THN", tahunProses)
                        };
                        RemoteMODbContext.Database.ExecuteSqlCommand("exec [PROSES_AKHIR_TAHUN_GL] @db_name, @THN", spParams);

#region REMARK
                        //short tahunProses1 = Convert.ToInt16(tahunProses + 1);

                        //ErasoftContext ErasoftDbContext = new ErasoftContext(getIP, db_name);
                        //var test = ErasoftDbContext.SOT01A.OrderByDescending(A => A.TGL).FirstOrDefault();

                        //#region GLFMUT
                        //string sSQL = "";
                        //sSQL += "DECLARE @THN_PROSES AS INT; " + Environment.NewLine + Environment.NewLine +
                        //"SET @THN_PROSES = " + tahunProses + "; " + Environment.NewLine + Environment.NewLine +
                        //"SELECT LKS, KODE, @THN_PROSES +1 THN, " +
                        //"JUMLAH = ISNULL(SUM(SA + (DEBET1 + DEBET2 + DEBET3 + DEBET4 + DEBET5 + DEBET6 + DEBET7 + DEBET8 + DEBET9 + DEBET10 + DEBET11 + DEBET12) - (KREDIT1 + KREDIT2 + KREDIT3 + KREDIT4 + KREDIT5 + KREDIT6 + KREDIT7 + KREDIT8 + KREDIT9 + KREDIT10 + KREDIT11 + KREDIT12)), 0) " +
                        //"INTO #TEMP_GL " +
                        //"FROM GLFMUT(NOLOCK) " +
                        //"WHERE THN = @THN_PROSES " +
                        //"GROUP BY LKS,KODE; " + Environment.NewLine + Environment.NewLine +

                        //"UPDATE A SET SA = TEMP.JUMLAH " +
                        //"FROM GLFMUT A " +
                        //"INNER JOIN #TEMP_GL TEMP ON A.LKS = TEMP.LKS AND A.KODE = TEMP.KODE AND A.THN = TEMP.THN; " + Environment.NewLine + Environment.NewLine +

                        //"INSERT INTO GLFMUT(THN, LKS, KODE, SA, " +
                        //"DEBET1, DEBET2, DEBET3, DEBET4, DEBET5, DEBET6, DEBET7, DEBET8, DEBET9, DEBET10, DEBET11, DEBET12, " +
                        //"KREDIT1, KREDIT2, KREDIT3, KREDIT4, KREDIT5, KREDIT6, KREDIT7, KREDIT8, KREDIT9, KREDIT10, KREDIT11, KREDIT12) " +
                        //"SELECT TEMP.THN,TEMP.LKS,TEMP.KODE,TEMP.JUMLAH, " +
                        //"0,0,0,0,0,0,0,0,0,0,0,0, " +
                        //"0,0,0,0,0,0,0,0,0,0,0,0 " +
                        //"FROM #TEMP_GL TEMP  " +
                        //"LEFT JOIN GLFMUT GL ON GL.KODE = TEMP.KODE AND GL.LKS = TEMP.LKS AND GL.THN = @THN_PROSES + 1 " +
                        //"WHERE ISNULL(GL.KODE,'') = ''; " + Environment.NewLine + Environment.NewLine +

                        //"DROP TABLE #TEMP_GL; " + Environment.NewLine;
                        ////var resultProsesAkhirTahunGLFMUT = EDB.ExecuteSQL("CString", CommandType.Text, sSQL);
                        //var resultProsesAkhirTahunGLFMUT = ErasoftDbContext.Database.ExecuteSqlCommand(sSQL);
                        //ErasoftDbContext.SaveChanges();
                        //#endregion GLFMUT

                        //#region STF08
                        //string sSQL2 = "";
                        //sSQL2 += "DECLARE @THN_PROSES_ST AS INT; " + Environment.NewLine + Environment.NewLine +
                        //"SET @THN_PROSES_ST = " + tahunProses + "; " + Environment.NewLine + Environment.NewLine +
                        //"SELECT GD,BRG,@THN_PROSES_ST + 1 TAHUN, " +
                        //"JUMLAH = ISNULL(SUM(QAWAL + (QM1 + QM2 + QM3 + QM4 + QM5 + QM6 + QM7 + QM8 + QM9 + QM10 + QM11 + QM12) - (QK1 + QK2 + QK3 + QK4 + QK5 + QK6 + QK7 + QK8 + QK9 + QK10 + QK11 + QK12)), 0) " +
                        //"INTO #TEMP_ST " +
                        //"FROM STF08(NOLOCK) " +
                        //"INNER JOIN STF18(NOLOCK) ON STF08.GD = STF18.KODE_GUDANG " +
                        //"WHERE STF08.TAHUN = @THN_PROSES_ST " +
                        //"GROUP BY GD,BRG; " + Environment.NewLine + Environment.NewLine +

                        //"UPDATE A SET QAWAL = TEMP.JUMLAH " +
                        //"FROM STF08 A " +
                        //"INNER JOIN #TEMP_ST TEMP ON A.GD = TEMP.GD AND A.BRG = TEMP.BRG AND A.TAHUN = TEMP.TAHUN; " + Environment.NewLine + Environment.NewLine +

                        //"INSERT INTO STF08(GD, BRG, TAHUN, QAWAL, NAWAL, " +
                        //"QM1, QM2, QM3, QM4, QM5, QM6, QM7, QM8, QM9, QM10, QM11, QM12, " +
                        //"QK1, QK2, QK3, QK4, QK5, QK6, QK7, QK8, QK9, QK10, QK11, QK12, " +
                        //"NM1, NM2, NM3, NM4, NM5, NM6, NM7, NM8, NM9, NM10, NM11, NM12, " +
                        //"NK1, NK2, NK3, NK4, NK5, NK6, NK7, NK8, NK9, NK10, NK11, NK12) " +
                        //"SELECT TEMP.GD,TEMP.BRG,TEMP.TAHUN,TEMP.JUMLAH,0, " +
                        //"0,0,0,0,0,0,0,0,0,0,0,0, " +
                        //"0,0,0,0,0,0,0,0,0,0,0,0, " +
                        //"0,0,0,0,0,0,0,0,0,0,0,0, " +
                        //"0,0,0,0,0,0,0,0,0,0,0,0 " +
                        //"FROM #TEMP_ST TEMP  " +
                        //"LEFT JOIN STF08 ST ON ST.BRG = TEMP.BRG AND ST.GD = TEMP.GD AND ST.TAHUN = @THN_PROSES_ST + 1 " +
                        //"WHERE ISNULL(ST.BRG,'') = ''; " + Environment.NewLine + Environment.NewLine +

                        //"DROP TABLE #TEMP_ST; " + Environment.NewLine;

                        ////var resultProsesAkhirTahunSTF08 = EDB.ExecuteSQL("CString", CommandType.Text, sSQL2);
                        //var resultProsesAkhirTahunSTF08 = ErasoftDbContext.Database.ExecuteSqlCommand(sSQL2);
                        //ErasoftDbContext.SaveChanges();
                        //#endregion STF08

                        //#region GLFMTL
                        //string sSQL3 = "";
                        //sSQL2 += "DECLARE @THN_PROSES_GLFMTL AS INT; " + Environment.NewLine + Environment.NewLine +
                        //"SET @THN_PROSES_GLFMTL = " + tahunProses + "; " + Environment.NewLine + Environment.NewLine +

                        //"SELECT @THN_PROSES_GLFMTL +1 TAHUN,LKS,KODE, " +
                        //"THI1 AS THL1, THI2 AS THL2, THI3 AS THL3, THI4 AS THL4, THI5 AS THL5, THI6 AS THL6, THI7 AS THL7, THI8 AS THL8, THI9 AS THL9, THI10 AS THL10, THI11 AS THL11, THI12 AS THL12 " +
                        //"INTO #TEMP_GLFMTL " +
                        //"FROM GLFMTL " +
                        //"WHERE Thn = @THN_PROSES_GLFMTL; " + Environment.NewLine + Environment.NewLine +


                        //"UPDATE A SET THL1 = TEMP.THL1, THL2 = TEMP.THL2, THL3 = TEMP.THL3, THL4 = TEMP.THL4, THL5 = TEMP.THL5, THL6 = TEMP.THL6, THL7 = TEMP.THL7, THL8 = TEMP.THL8, THL9 = TEMP.THL9, THL10 = TEMP.THL10, THL11 = TEMP.THL11, THL12 = TEMP.THL12 " +
                        //"FROM GLFMTL A " +
                        //"INNER JOIN #TEMP_GLFMTL TEMP ON A.KODE = TEMP.KODE AND A.LKS = TEMP.LKS AND A.THN = TEMP.TAHUN; " + Environment.NewLine + Environment.NewLine +

                        //"INSERT INTO GLFMTL(THN, LKS, KODE, " +
                        //"THI1, THI2, THI3, THI4, THI5, THI6, THI7, THI8, THI9, THI10, THI11, THI12, " +
                        //"THL1, THL2, THL3, THL4, THL5, THL6, THL7, THL8, THL9, THL10, THL11, THL12) " +
                        //"SELECT TEMP.TAHUN,TEMP.LKS,TEMP.KODE, " +
                        //"0,0,0,0,0,0,0,0,0,0,0,0, " +
                        //"TEMP.THL1,TEMP.THL2,TEMP.THL3,TEMP.THL4,TEMP.THL5,TEMP.THL6,TEMP.THL7,TEMP.THL8,TEMP.THL9,TEMP.THL10,TEMP.THL11,TEMP.THL12 " +
                        //"FROM #TEMP_GLFMTL TEMP  " +
                        //"LEFT JOIN GLFMTL MTL ON MTL.KODE = TEMP.KODE AND MTL.LKS = TEMP.LKS AND MTL.THN = @THN_PROSES_GLFMTL + 1 " +
                        //"WHERE ISNULL(MTL.KODE,'') = ''; " + Environment.NewLine + Environment.NewLine +

                        //"DROP TABLE #TEMP_GLFMTL; " + Environment.NewLine;

                        ////var resultProsesAkhirTahunGLFMTL = EDB.ExecuteSQL("CString", CommandType.Text, sSQL3);
                        //var resultProsesAkhirTahunGLFMTL = ErasoftDbContext.Database.ExecuteSqlCommand(sSQL3);
                        //ErasoftDbContext.SaveChanges();
                        //#endregion GLFMTL
#endregion REMARK
                    }
                    catch (Exception ex)
                    {
                        return new JsonResult { Data = new { mo_error = "Gagal memproses akhir tahun. Database " + db_name + " tidak ditemukan." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }

                    return new JsonResult { Data = new { mo_message = "Sukses memproses akhir tahun." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
                return new JsonResult { Data = new { mo_error = "Gagal memproses akhir tahun. Internal Server Error." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            catch (Exception ex)
            {
                return new JsonResult { Data = new { mo_error = "Gagal memproses akhir tahun. Internal Server Error." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }
        //end add by nurul 5/1/2021

        //add by fauzi 21 Februari 2020
        [Queue("3_general")]
        public async Task<ActionResult> ReminderEmailExpiredAccountMP(string dbPathEra, string susername, string semail, string snamatoko, string smarketplace, DateTime? expired_date)
        {
            var currentTimeRequest = (long)DateTimeOffset.UtcNow.AddHours(7).ToUnixTimeSeconds();
            try
            {
                DateTime dateExpired = Convert.ToDateTime(expired_date);
                var dateExpiredEmail = Convert.ToDateTime(expired_date).ToString("dd MMMM yyyy HH:mm tt");
                var countDays = DateTime.UtcNow.AddHours(7).Subtract(dateExpired).Days.ToString();

                if ((countDays == "-7") || (countDays == "-3") || (countDays == "-1"))
                {
                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                    contextNotif.Clients.Group(dbPathEra).notifAccountExpired("Reminder: Status akun marketplace " + snamatoko + " (" + smarketplace + ") akan expired " + countDays.ToString().Replace("-", "") + " hari lagi.");

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
                    message.Subject = "Master Online x " + smarketplace + " Announcement";
                    message.Body = string.Format(body, susername, smarketplace, snamatoko, dateExpiredEmail.ToString(), countDays.ToString().Replace("-", ""));
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
                return new JsonResult { Data = new { mo_message = "Success: Reminder expired running. Remaining days : " + countDays.ToString() + " timestamp:" + currentTimeRequest.ToString() }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            catch (Exception ex)
            {
                return new JsonResult { Data = new { mo_error = "Failed: Reminder expired not running. Because internal Server Error. " + ex.Message.ToString() + " timestamp:" + currentTimeRequest.ToString() }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        public ActionResult ReminderNotifyExpiredAccountMP(string dbPathEra, string snamatoko, string smarketplace, DateTime? expired_date)
        {
            DateTime dateExpired = Convert.ToDateTime(expired_date);
            var countDays = DateTime.UtcNow.AddHours(7).Subtract(dateExpired).Days.ToString();

            if ((countDays == "-7") || (countDays == "-3") || (countDays == "-1"))
            {
                var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                contextNotif.Clients.Group(dbPathEra).notifAccountExpired("Reminder: Status akun marketplace " + snamatoko + " (" + smarketplace + ") akan expired " + countDays.ToString().Replace("-", "") + " hari lagi.");
            }
            return new JsonResult { Data = new { mo_message = "Notify expired running." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult RefreshTablePesananHangfireJob(string db_customer = "", string email_customer = "")
        {
            try
            {
                string viewName = "TablePesananHangfireJob";
                ViewData["EmailCustomer"] = email_customer;
                var EDB = new DatabaseSQL(db_customer);

                var listTable = new List<PesananHangfireJob>();

                string sSQLSelectGroupMethod = "";
                sSQLSelectGroupMethod += "SELECT statename, invocationdata, arguments " +
                    "FROM hangfire.[job] " +
                    "WHERE invocationdata LIKE '%getorder%' and statename LIKE '%succ%' " +
                    //add by nurul 2/9/2021
                    "and invocationdata not like '%GetOrderLogistics%' " +
                    //end add by nurul 2/9/2021
                    "GROUP BY statename, invocationdata, arguments " +
                    "ORDER BY invocationdata ASC ";
                //sSQLSelectGroupMethod += "SELECT statename, invocationdata " +
                //   "FROM hangfire.[job] " +
                //   "WHERE invocationdata LIKE '%getorder%' and statename LIKE '%succ%' " +
                //   "GROUP BY statename, invocationdata " +
                //   "ORDER BY invocationdata ASC ";

                var resultDataJobGroup = EDB.GetDataSet("SCon", "QUEUE_GROUP_JOB", sSQLSelectGroupMethod);

                var TotalHari = 0;
                var TotalJam = 0;
                var TotalMenit = 0;
                var TotalDetik = 0;

                if (resultDataJobGroup.Tables[0].Rows.Count > 0)
                {
                    for (int k = 0; k < resultDataJobGroup.Tables[0].Rows.Count; k++)
                    {
                        string sSQLSelect = "";

                        sSQLSelect += "SELECT TOP 1 a.Id, a.statename, a.invocationdata, a.arguments, a.createdat, " +
                            "(SELECT TOP 1 b.createdat FROM hangfire.[state] b WHERE b.jobid = a.id AND b.[name] LIKE '%proc%' ORDER BY b.id DESC) AS LASTCREATEJOBPROCESS, " +
                            "(SELECT TOP 1 b.createdat FROM hangfire.[state] b WHERE b.jobid = a.id AND b.[name] LIKE '%succ%' ORDER BY b.id DESC) AS LASTCREATEJOBSUCCESS " +
                            "FROM hangfire.[job] a " +
                            "WHERE " +
                            "a.invocationdata = '" + resultDataJobGroup.Tables[0].Rows[k]["invocationdata"] + "' AND " +
                            "a.arguments = '" + resultDataJobGroup.Tables[0].Rows[k]["arguments"] + "' AND " +
                            "a.statename LIKE '%succ%' ORDER BY a.Id DESC ";


                        var resultDataJob = EDB.GetDataSet("SCon", "QUEUE_JOB", sSQLSelect);
                        if (resultDataJob.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < resultDataJob.Tables[0].Rows.Count; i++)
                            {
                                var checkApprove = false;
                                var resultConvertInvocation = Newtonsoft.Json.JsonConvert.DeserializeObject(resultDataJob.Tables[0].Rows[i]["INVOCATIONDATA"].ToString(), typeof(FieldInvocationData)) as FieldInvocationData;
                                string[] splitMarketplace = resultConvertInvocation.Type.Split(',');

                                var tglProcess = Convert.ToDateTime(resultDataJob.Tables[0].Rows[i]["LASTCREATEJOBPROCESS"]).AddHours(7);
                                var tglSuccess = Convert.ToDateTime(resultDataJob.Tables[0].Rows[i]["LASTCREATEJOBSUCCESS"]).AddHours(7);
                                TimeSpan selisih = tglSuccess.Subtract(tglProcess);

                                var resultSelisih = "";
                                if (selisih.Days > 0)
                                {
                                    TotalHari += selisih.Days;
                                    TotalJam += selisih.Hours;
                                    TotalMenit += selisih.Minutes;
                                    TotalDetik += selisih.Seconds;
                                    resultSelisih = "(" + selisih.Days + ") hari, (" + selisih.Hours + ") jam, (" + selisih.Minutes + ") menit, (" + selisih.Seconds + ") detik";
                                }
                                else if (selisih.Hours > 0)
                                {
                                    TotalJam += selisih.Hours;
                                    TotalMenit += selisih.Minutes;
                                    TotalDetik += selisih.Seconds;
                                    resultSelisih = "(" + selisih.Hours + ") jam, (" + selisih.Minutes + ") menit, (" + selisih.Seconds + ") detik";
                                }
                                else if (selisih.Minutes > 0)
                                {
                                    TotalMenit += selisih.Minutes;
                                    TotalDetik += selisih.Seconds;
                                    resultSelisih = "(" + selisih.Minutes + ") menit, (" + selisih.Seconds + ") detik";
                                }
                                else if (selisih.Seconds > 0)
                                {
                                    TotalDetik += selisih.Seconds;
                                    resultSelisih = "(" + selisih.Seconds + ") detik";
                                }
                                else
                                {
                                    resultSelisih = "(" + selisih.Seconds + ") detik";
                                }

                                var statusOrder = "";
                                var namaToko = "";
                                var marketplace = splitMarketplace[0].Replace("MasterOnline.Controllers.", "").Replace("ControllerJob", "");

                                if (marketplace.ToUpper() == "LAZADA")
                                {
                                    string[] splitArguments = resultDataJob.Tables[0].Rows[i]["ARGUMENTS"].ToString().Replace("\"", "").Replace("\\", "").Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "").Split(',');

                                    if (splitArguments.Length > 0)
                                    {
                                        var no_custLazada = splitArguments[0].ToString();
                                        var tokenLazada = splitArguments[1].ToString();
                                        var usernameLazada = splitArguments[3].ToString();
                                        string queryCheckToko = "SELECT PERSO FROM ARF01 WHERE TOKEN = '" + tokenLazada + "' AND CUST = '" + no_custLazada + "'; ";
                                        var resultDataToko = EDB.GetDataSet("SCon", "QUEUE_TOKO_LAZADA", queryCheckToko);
                                        if (resultDataToko.Tables[0].Rows.Count > 0)
                                        {
                                            //if(resultDataToko.Tables[0].Rows[0]["PERSO"].ToString() == usernameLazada)
                                            //{
                                                checkApprove = true;
                                            namaToko = resultDataToko.Tables[0].Rows[0]["PERSO"].ToString() + " user:" + usernameLazada;
                                            //namaToko = resultDataToko.Tables[0].Rows[0]["PERSO"].ToString();
                                            //}
                                        }
                                    }
                                }
                                else if (marketplace.ToUpper() == "BLIBLI")
                                {
                                    if (resultConvertInvocation.Method.ToUpper() == "GETORDERLIST" &&
                                    resultDataJob.Tables[0].Rows[i]["arguments"].ToString().Contains("blibli_pesanan_paid"))
                                    {
                                        statusOrder = " (Paid)";
                                    }
                                    else if (resultConvertInvocation.Method.ToUpper() == "GETORDERLIST" &&
                                    resultDataJob.Tables[0].Rows[i]["arguments"].ToString().Contains("blibli_pesanan_complete"))
                                    {
                                        statusOrder = " (Complete)";
                                    }

                                    string[] splitArguments = resultDataJob.Tables[0].Rows[i]["ARGUMENTS"].ToString().Replace("\"", "").Replace("\\", "").Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "").Split(',');

                                    if (splitArguments.Length > 0)
                                    {
                                        var no_custBlibli = splitArguments[13].ToString();
                                        string[] tokenBlibli = splitArguments[6].ToString().Split(':');
                                        string[] usernameBlibli = splitArguments[9].ToString().Split(':');
                                        string queryCheckToko = "SELECT PERSO FROM ARF01 WHERE TOKEN = '" + tokenBlibli[1] + "' AND CUST = '" + no_custBlibli + "'; ";
                                        var resultDataToko = EDB.GetDataSet("SCon", "QUEUE_TOKO_BLIBLI", queryCheckToko);
                                        if (resultDataToko.Tables[0].Rows.Count > 0)
                                        {
                                            //if (resultDataToko.Tables[0].Rows[0]["PERSO"].ToString() == usernameBlibli[1])
                                            //{
                                                checkApprove = true;
                                            namaToko = resultDataToko.Tables[0].Rows[0]["PERSO"].ToString() + " user:" + usernameBlibli[1];
                                            //namaToko = resultDataToko.Tables[0].Rows[0]["PERSO"].ToString();
                                            //}
                                        }
                                    }
                                }
                                else if (marketplace.ToUpper() == "TOKOPEDIA")
                                {
                                    if (resultConvertInvocation.Method.ToUpper() == "GETORDERLIST" &&
                                    resultDataJob.Tables[0].Rows[i]["arguments"].ToString().Contains("\\\"}\"" + "," + "\"2\"" + "," + "\"\\\""))
                                    {
                                        statusOrder = " (Paid)";
                                    }

                                    string[] splitArguments = resultDataJob.Tables[0].Rows[i]["ARGUMENTS"].ToString().Replace("\"", "").Replace("\\", "").Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "").Split(',');

                                    if (splitArguments.Length > 0)
                                    {
                                        var no_custTokped = splitArguments[12].ToString();
                                        if (resultConvertInvocation.Method.ToUpper() == "GETORDERLISTCANCEL" || resultConvertInvocation.Method.ToUpper() == "GETORDERLIST_CANCEL_WEBHOOK")
                                        {
                                            no_custTokped = splitArguments[11].ToString();
                                        }
                                        string[] apiClientUsernameTokped = splitArguments[1].ToString().Split(':');
                                        string[] usernameTokped = splitArguments[9].ToString().Split(':');
                                        string queryCheckToko = "SELECT PERSO FROM ARF01 WHERE API_CLIENT_U = '" + apiClientUsernameTokped[1] + "' AND CUST = '" + no_custTokped + "' ";
                                        var resultDataToko = EDB.GetDataSet("SCon", "QUEUE_TOKO_TOKOPEDIA", queryCheckToko);
                                        if (resultDataToko.Tables[0].Rows.Count > 0)
                                        {
                                            //if (resultDataToko.Tables[0].Rows[0]["PERSO"].ToString() == usernameTokped[1])
                                            //{
                                                checkApprove = true;
                                            namaToko = resultDataToko.Tables[0].Rows[0]["PERSO"].ToString() + " user:" + usernameTokped[1];
                                            //namaToko = resultDataToko.Tables[0].Rows[0]["PERSO"].ToString();
                                            //}
                                        }
                                        else
                                        {

                                        }
                                    }
                                }
                                else if (marketplace.ToUpper() == "SHOPEE")
                                {
                                    if (resultConvertInvocation.Method.ToUpper() == "GETORDERBYSTATUS" &&
                                    resultDataJob.Tables[0].Rows[i]["arguments"].ToString().Contains("}\"" + "," + "\"6\"" + "," + "\"\\\""))
                                    {
                                        statusOrder = " (Unpaid)";
                                    }
                                    else if (resultConvertInvocation.Method.ToUpper() == "GETORDERBYSTATUS" &&
                                    resultDataJob.Tables[0].Rows[i]["arguments"].ToString().Contains("}\"" + "," + "\"3\"" + "," + "\"\\\""))
                                    {
                                        statusOrder = " (Ready To Ship)";
                                    }

                                    string[] splitArguments = resultDataJob.Tables[0].Rows[i]["ARGUMENTS"].ToString().Replace("\"", "").Replace("\\", "").Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "").Split(',');

                                    if (splitArguments.Length > 0)
                                    {
                                        //var no_custShopee = splitArguments[12].ToString();
                                        var usernameShopee = splitArguments[8].ToString().Split(':');
                                        var no_custShopee = splitArguments[14].ToString();
                                        string[] merchantCode = splitArguments[0].ToString().Split(':');
                                        string queryCheckToko = "SELECT PERSO FROM ARF01 WHERE SORT1_CUST = '" + merchantCode[1] + "' AND CUST = '" + no_custShopee + "'; ";
                                        var resultDataToko = EDB.GetDataSet("SCon", "QUEUE_TOKO_SHOPEE", queryCheckToko);
                                        if (resultDataToko.Tables[0].Rows.Count > 0)
                                        {
                                            ////if (resultDataToko.Tables[0].Rows[0]["PERSO"].ToString() == usernameShopee[1])
                                            ////{
                                            //    checkApprove = true;
                                            //namaToko = resultDataToko.Tables[0].Rows[0]["PERSO"].ToString() + " user:" + usernameShopee[1];
                                            ////namaToko = resultDataToko.Tables[0].Rows[0]["PERSO"].ToString();
                                            ////}
                                            namaToko = resultDataToko.Tables[0].Rows[0]["PERSO"].ToString() + " user:" + usernameShopee[1];
                                            var sMETHOD = resultConvertInvocation.Method + statusOrder;
                                            var sMARKETPLACE = marketplace + " (" + namaToko + ")";
                                            if (listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).ToList().Count == 0)
                                            {
                                                checkApprove = true;
                                            }
                                            else
                                            {
                                                var createJobSuccess = Convert.ToDateTime(resultDataJob.Tables[0].Rows[i]["LASTCREATEJOBSUCCESS"]).AddHours(7);
                                                if (listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).FirstOrDefault().LASTCREATEJOBSUCCESS < createJobSuccess)
                                                {
                                                    listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).FirstOrDefault().LASTCREATEJOBPROCESS = Convert.ToDateTime(resultDataJob.Tables[0].Rows[i]["LASTCREATEJOBPROCESS"]).AddHours(7);
                                                    listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).FirstOrDefault().LASTCREATEJOBSUCCESS = createJobSuccess;
                                                    listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).FirstOrDefault().SELISIH = resultSelisih;
                                                    listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).FirstOrDefault().ID = Convert.ToInt32(resultDataJob.Tables[0].Rows[i]["ID"].ToString());
                                                    listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).FirstOrDefault().CREATEDAT = Convert.ToDateTime(resultDataJob.Tables[0].Rows[i]["CREATEDAT"]).AddHours(7);
                                                    listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).FirstOrDefault().STATENAME = resultDataJob.Tables[0].Rows[i]["STATENAME"].ToString();
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (marketplace.ToUpper() == "EIGHTTWOCART")
                                {
                                    if (resultConvertInvocation.Method.ToUpper() == "E2CART_GETORDERBYSTATUS" &&
                                    resultDataJob.Tables[0].Rows[i]["arguments"].ToString().Contains("\\\"}\"" + "," + "\"23\"" + "," + "\"\\\""))
                                    {
                                        statusOrder = " (Unpaid)";
                                    }
                                    else if (resultConvertInvocation.Method.ToUpper() == "E2CART_GETORDERBYSTATUS" &&
                                    resultDataJob.Tables[0].Rows[i]["arguments"].ToString().Contains("\\\"}\"" + "," + "\"2\"" + "," + "\"\\\""))
                                    {
                                        statusOrder = " (Paid)";
                                    }

                                    string[] splitArguments = resultDataJob.Tables[0].Rows[i]["ARGUMENTS"].ToString().Replace("\"", "").Replace("\\", "").Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "").Split(',');

                                    if (splitArguments.Length > 0)
                                    {
                                        var no_cust82Cart = splitArguments[12].ToString();
                                        string[] apiKey82Cart = splitArguments[3].ToString().Split(':');
                                        string[] username82Cart = splitArguments[1].ToString().Split(':');
                                        string queryCheckToko = "SELECT PERSO FROM ARF01 WHERE API_KEY = '" + apiKey82Cart[1] + "' AND CUST = '" + no_cust82Cart + "'; ";
                                        var resultDataToko = EDB.GetDataSet("SCon", "QUEUE_TOKO_82CART", queryCheckToko);
                                        if (resultDataToko.Tables[0].Rows.Count > 0)
                                        {
                                            //if (resultDataToko.Tables[0].Rows[0]["PERSO"].ToString() == username82Cart[1])
                                            //{
                                                checkApprove = true;
                                            namaToko = resultDataToko.Tables[0].Rows[0]["PERSO"].ToString() + " user:" + username82Cart[1];
                                            //namaToko = resultDataToko.Tables[0].Rows[0]["PERSO"].ToString();
                                            //}
                                        }
                                    }
                                }
                                else if (marketplace.ToUpper() == "SHOPIFY")
                                {

                                    string[] splitArguments = resultDataJob.Tables[0].Rows[i]["ARGUMENTS"].ToString().Replace("\"", "").Replace("\\", "").Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "").Split(',');

                                    if (splitArguments.Length > 0)
                                    {
                                        var no_custShopify = splitArguments[9].ToString();
                                        string[] apiKeyShopify = splitArguments[3].ToString().Split(':');
                                        string[] usernameShopify = splitArguments[1].ToString().Split(':');
                                        string queryCheckToko = "SELECT PERSO FROM ARF01 WHERE API_KEY = '" + apiKeyShopify[1] + "' AND CUST = '" + no_custShopify + "'; ";
                                        var resultDataToko = EDB.GetDataSet("SCon", "QUEUE_TOKO_SHOPIFY", queryCheckToko);
                                        if (resultDataToko.Tables[0].Rows.Count > 0)
                                        {
                                            //if (resultDataToko.Tables[0].Rows[0]["PERSO"].ToString() == username82Cart[1])
                                            //{
                                            checkApprove = true;
                                            namaToko = resultDataToko.Tables[0].Rows[0]["PERSO"].ToString() + " user:" + usernameShopify[1];
                                            //namaToko = resultDataToko.Tables[0].Rows[0]["PERSO"].ToString();
                                            //}
                                        }
                                    }
                                }
                                else if (marketplace.ToUpper() == "JDID")
                                {

                                    string[] splitArguments = resultDataJob.Tables[0].Rows[i]["ARGUMENTS"].ToString().Replace("\"", "").Replace("\\", "").Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "").Split(',');

                                    if (splitArguments.Length > 0)
                                    {
                                        string[] no_custJDID = splitArguments[3].ToString().Split(':');
                                        string[] appKeyJDID = splitArguments[0].ToString().Split(':');
                                        string[] usernameJDID = splitArguments[4].ToString().Split(':');
                                        string queryCheckToko = "SELECT PERSO FROM ARF01 WHERE API_KEY = '" + appKeyJDID[1] + "' AND CUST = '" + no_custJDID[1] + "'; ";
                                        var resultDataToko = EDB.GetDataSet("SCon", "QUEUE_TOKO_JDID", queryCheckToko);
                                        if (resultDataToko.Tables[0].Rows.Count > 0)
                                        {
                                            //if (resultDataToko.Tables[0].Rows[0]["PERSO"].ToString() == username82Cart[1])
                                            //{
                                            checkApprove = true;
                                            namaToko = resultDataToko.Tables[0].Rows[0]["PERSO"].ToString() + " user:" + usernameJDID[1];
                                            //namaToko = resultDataToko.Tables[0].Rows[0]["PERSO"].ToString();
                                            //}
                                        }
                                    }
                                }
                                else if (marketplace.ToUpper() == "BUKALAPAK")
                                {
                                    string[] splitArguments = resultDataJob.Tables[0].Rows[i]["ARGUMENTS"].ToString().Replace("\"", "").Replace("\\", "").Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "").Split(',');

                                    if (splitArguments.Length > 0)
                                    {
                                        var no_custBL = splitArguments[6].ToString();
                                        var usernameBL = splitArguments[8].ToString();
                                        string queryCheckToko = "SELECT PERSO FROM ARF01 WHERE CUST = '" + no_custBL + "'; ";
                                        var resultDataToko = EDB.GetDataSet("SCon", "QUEUE_TOKO_BL", queryCheckToko);
                                        if (resultDataToko.Tables[0].Rows.Count > 0)
                                        {
                                            namaToko = resultDataToko.Tables[0].Rows[0]["PERSO"].ToString() + " user:" + usernameBL;
                                            var sMETHOD = resultConvertInvocation.Method + statusOrder;
                                            var sMARKETPLACE = marketplace + " (" + namaToko + ")";
                                            if (listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).ToList().Count == 0)
                                            {
                                                checkApprove = true;
                                            }
                                            else
                                            {
                                                var createJobSuccess = Convert.ToDateTime(resultDataJob.Tables[0].Rows[i]["LASTCREATEJOBSUCCESS"]).AddHours(7);
                                                if(listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).FirstOrDefault().LASTCREATEJOBSUCCESS < createJobSuccess)
                                                {
                                                    listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).FirstOrDefault().LASTCREATEJOBPROCESS = Convert.ToDateTime(resultDataJob.Tables[0].Rows[i]["LASTCREATEJOBPROCESS"]).AddHours(7);
                                                    listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).FirstOrDefault().LASTCREATEJOBSUCCESS = createJobSuccess;
                                                    listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).FirstOrDefault().SELISIH = resultSelisih;
                                                    listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).FirstOrDefault().ID = Convert.ToInt32(resultDataJob.Tables[0].Rows[i]["ID"].ToString());
                                                    listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).FirstOrDefault().CREATEDAT = Convert.ToDateTime(resultDataJob.Tables[0].Rows[i]["CREATEDAT"]).AddHours(7);
                                                    listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).FirstOrDefault().STATENAME = resultDataJob.Tables[0].Rows[i]["STATENAME"].ToString();
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (marketplace.ToUpper() == "TIKTOK")
                                {
                                    string[] splitArguments = resultDataJob.Tables[0].Rows[i]["ARGUMENTS"].ToString().Replace("\"", "").Replace("\\", "").Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "").Split(',');

                                    if (splitArguments.Length > 0)
                                    {
                                        var no_custTik = splitArguments[4].ToString().Split(':');
                                        var usernameTik = splitArguments[5].ToString().Split(':');
                                        string queryCheckToko = "SELECT PERSO FROM ARF01 WHERE CUST = '" + no_custTik[1] + "'; ";
                                        var resultDataToko = EDB.GetDataSet("SCon", "QUEUE_TOKO_TIKTOK", queryCheckToko);
                                        if (resultDataToko.Tables[0].Rows.Count > 0)
                                        {
                                            namaToko = resultDataToko.Tables[0].Rows[0]["PERSO"].ToString() + " user:" + usernameTik[1];
                                            var sMETHOD = resultConvertInvocation.Method + statusOrder;
                                            var sMARKETPLACE = marketplace + " (" + namaToko + ")";
                                            if (listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).ToList().Count == 0)
                                            {
                                                checkApprove = true;
                                            }
                                            else
                                            {
                                                var createJobSuccess = Convert.ToDateTime(resultDataJob.Tables[0].Rows[i]["LASTCREATEJOBSUCCESS"]).AddHours(7);
                                                if (listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).FirstOrDefault().LASTCREATEJOBSUCCESS < createJobSuccess)
                                                {
                                                    listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).FirstOrDefault().LASTCREATEJOBPROCESS = Convert.ToDateTime(resultDataJob.Tables[0].Rows[i]["LASTCREATEJOBPROCESS"]).AddHours(7);
                                                    listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).FirstOrDefault().LASTCREATEJOBSUCCESS = createJobSuccess;
                                                    listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).FirstOrDefault().SELISIH = resultSelisih;
                                                    listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).FirstOrDefault().ID = Convert.ToInt32(resultDataJob.Tables[0].Rows[i]["ID"].ToString());
                                                    listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).FirstOrDefault().CREATEDAT = Convert.ToDateTime(resultDataJob.Tables[0].Rows[i]["CREATEDAT"]).AddHours(7);
                                                    listTable.Where(m => m.METHOD == sMETHOD && m.MARKETPLACE == sMARKETPLACE).FirstOrDefault().STATENAME = resultDataJob.Tables[0].Rows[i]["STATENAME"].ToString();
                                                }
                                            }
                                        }
                                    }
                                }
                                if (checkApprove)
                                {
                                    listTable.Add(new PesananHangfireJob
                                    {
                                        CREATEDAT = Convert.ToDateTime(resultDataJob.Tables[0].Rows[i]["CREATEDAT"]).AddHours(7),
                                        ID = Convert.ToInt32(resultDataJob.Tables[0].Rows[i]["ID"].ToString()),
                                        STATENAME = resultDataJob.Tables[0].Rows[i]["STATENAME"].ToString(),
                                        METHOD = resultConvertInvocation.Method + statusOrder,
                                        MARKETPLACE = marketplace + " (" + namaToko + ")",
                                        LASTCREATEJOBPROCESS = Convert.ToDateTime(resultDataJob.Tables[0].Rows[i]["LASTCREATEJOBPROCESS"]).AddHours(7),
                                        LASTCREATEJOBSUCCESS = Convert.ToDateTime(resultDataJob.Tables[0].Rows[i]["LASTCREATEJOBSUCCESS"]).AddHours(7),
                                        SELISIH = resultSelisih
                                    });
                                }
                            }
                        }
                    }
                }

                var myDateTime = new DateTime(2000, 01, 01);
                myDateTime = myDateTime.AddDays(TotalHari);
                myDateTime = myDateTime.AddHours(TotalJam);
                myDateTime = myDateTime.AddMinutes(TotalMenit);
                myDateTime = myDateTime.AddSeconds(TotalDetik);
                var resultAddTime = Convert.ToDateTime(myDateTime).ToString("HH:mm:ss");

                ViewData["TotalSelisih"] = resultAddTime.ToString();
                var pageContent = listTable;

                IPagedList<PesananHangfireJob> pageOrders = new StaticPagedList<PesananHangfireJob>(pageContent, 0 + 1, 10, 0);
                return PartialView(viewName, pageOrders);

            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = "Terjadi Kesalahan, mohon hubungi support." }, JsonRequestBehavior.AllowGet);
            }
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

        //add by nurul 17/5/2021
        public ErasoftContext ErasoftDbContextNew { get; set; }
        public async System.Threading.Tasks.Task<string> GetCategoryBlibli(string email)
        {
            try
            {
                var sessionAccountUserID = System.Web.HttpContext.Current.Session["SessionAccountUserID"];

                //            dbPathEra = sessionAccountDatabasePathErasoft.ToString();
                //            //dbSourceEra = accFromUser.DataSourcePath;
                //#if (Debug_AWS)
                //            dbSourceEra = sessionAccountDataSourcePathDebug.ToString();
                //#else
                //                dbSourceEra = sessionAccountDataSourcePath.ToString();
                //#endif
#if (AWS || Debug_AWS)
            var getAkunRahma = MoDbContext.Account.Where(a => a.Email == "aniesuprijati@gmail.com").FirstOrDefault();
#elif (DEV || DEBUG)
                var getAkunRahma = MoDbContext.Account.Where(a => a.Email == "rahmamk@gmail.com").FirstOrDefault();
#endif
                if (getAkunRahma != null)
                {
                    //if (sessionAccountUserID.ToString() == "admin_manage")
                    //    ErasoftDbContextNew = new ErasoftContext();
                    //else
                    ErasoftDbContextNew = new ErasoftContext(getAkunRahma.DatabasePathErasoft);

                    var idmarket = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BLIBLI").IdMarket.ToString();
                    //var custInDb = ErasoftDbContextNew.ARF01.Where(c => c.NAMA == idmarket).ToList();
                    //foreach (var customer in custInDb)
                    var customer = ErasoftDbContextNew.ARF01.Where(c => c.NAMA == idmarket && c.EMAIL == email).FirstOrDefault();
                    if (customer != null)
                    {
#region BLIBLI get token
                        if (!string.IsNullOrEmpty(customer.API_CLIENT_P) && !string.IsNullOrEmpty(customer.API_CLIENT_U))
                        {
                            var BliApi = new BlibliControllerJob();
                            BlibliControllerJob.BlibliAPIData data = new BlibliControllerJob.BlibliAPIData()
                            {
                                API_client_username = customer.API_CLIENT_U,
                                API_client_password = customer.API_CLIENT_P,
                                API_secret_key = customer.API_KEY,
                                mta_username_email_merchant = customer.EMAIL,
                                mta_password_password_merchant = customer.PASSWORD,
                                merchant_code = customer.Sort1_Cust,
                                token = customer.TOKEN,
                                idmarket = customer.RecNum.Value,
                                versiToken = customer.KD_ANALISA,
                                DatabasePathErasoft = getAkunRahma.DatabasePathErasoft,
                                username = getAkunRahma.Username

                            };
                            var result = await BliApi.GetCategoryTreeV2(data);
                            //Task.Run(() => BliApi.GetCategoryTreeV2(data)).Wait();
                            //BliApi.GetCategoryTree(data);
                            //if (result == "1")
                            //{
                            //    //return "Get Category & Attribute Blibli berhasil.";
                            //    return Json("Get Category & Attribute Blibli berhasil.", JsonRequestBehavior.AllowGet);
                            //}
                            //else
                            //{
                            //    //return "Get Category & Attribute Blibli gagal.";
                            //    return Json("Get Category & Attribute Blibli gagal.", JsonRequestBehavior.AllowGet);
                            //}
                            //return Json("Get Category & Attribute Blibli sedang berlangsung.", JsonRequestBehavior.AllowGet);
                            return "Get Category & Attribute Blibli sedang berlangsung.";
                        }
#endregion
                    }
                }
                return "Akun tidak ditemukan.";
                //return Json("Akun tidak ditemukan.", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return "Get Category & Attribute Blibli gagal.";
            }
        }
        //end add by nurul 17/5/2021

        //add by nurul 27/5/2021
        public async System.Threading.Tasks.Task<string> GetCategoryJDID(string email)
        {
            try
            {
                var sessionAccountUserID = System.Web.HttpContext.Current.Session["SessionAccountUserID"];

#if (AWS || Debug_AWS)
            var getAkunRahma = MoDbContext.Account.Where(a => a.Email == "aniesuprijati@gmail.com").FirstOrDefault();
#elif (DEV || DEBUG)
                var getAkunRahma = MoDbContext.Account.Where(a => a.Email == "rahmamk@gmail.com").FirstOrDefault();
#endif
                if (getAkunRahma != null)
                {
                    ErasoftDbContextNew = new ErasoftContext(getAkunRahma.DatabasePathErasoft);

                    var idmarket = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "JD.ID").IdMarket.ToString();
                    var customer = ErasoftDbContextNew.ARF01.Where(c => c.NAMA == idmarket && c.EMAIL == email).FirstOrDefault();
                    if (customer != null)
                    {
                        DateTime dateNow = DateTime.UtcNow.AddHours(7);
                        if (dateNow >= customer.TGL_EXPIRED)
                        {
                            JDIDControllerJob.JDIDAPIDataJob iden = new JDIDControllerJob.JDIDAPIDataJob();
                            iden.merchant_code = customer.Sort1_Cust;
                            iden.DatabasePathErasoft = getAkunRahma.DatabasePathErasoft;
                            iden.username = getAkunRahma.Username;
                            iden.no_cust = customer.CUST;
                            iden.tgl_expired = customer.TGL_EXPIRED;
                            iden.appKey = customer.API_KEY;
                            iden.appSecret = customer.API_CLIENT_U;
                            iden.accessToken = customer.TOKEN;
                            iden.refreshToken = customer.REFRESH_TOKEN;
                            //add by nurul 4/5/2021, JDID versi 2
                            iden.versi = customer.KD_ANALISA;
                            iden.merchant_code = customer.Sort1_Cust;
                            //end add by nurul 4/5/2021, JDID versi 2

                            // proses cek dan get token
                            //#if (AWS || DEV)
                            //                    client.Enqueue<JDIDControllerJob>(x => x.GetTokenJDID(iden, false, false));
                            //#else
                            //                                                Task.Run(() => new JDIDControllerJob().GetTokenJDID(iden, false, false)).Wait();
                            //#endif
                            await new JDIDControllerJob().GetTokenJDID(iden, false, false);

                            var arf01_2 = ErasoftDbContext.ARF01.Where(t => t.CUST == customer.CUST).FirstOrDefault();
                            if (arf01_2 != null)
                            {
                                if (!string.IsNullOrEmpty(arf01_2.TOKEN) && !string.IsNullOrEmpty(arf01_2.API_CLIENT_U) && !string.IsNullOrEmpty(arf01_2.API_KEY))
                                {
                                    var jdAPI2 = new JDIDController();
                                    JDIDAPIData data = new JDIDAPIData()
                                    {
                                        no_cust = arf01_2.CUST,
                                        email = arf01_2.EMAIL,
                                        accessToken = arf01_2.TOKEN,
                                        appKey = arf01_2.API_KEY,
                                        appSecret = arf01_2.API_CLIENT_U,
                                        DatabasePathErasoft = getAkunRahma.DatabasePathErasoft,
                                        merchant_code = arf01_2.Sort1_Cust,
                                        tgl_expired = arf01_2.TGL_EXPIRED,
                                        versi = arf01_2.KD_ANALISA,
                                        username = getAkunRahma.Username,
                                        //add by nurul 6/6/2021
                                        refreshToken = customer.REFRESH_TOKEN
                                        //add by nurul 6/6/2021
                                    };
                                    var result = await jdAPI2.JDID_getCategoryV2(data);
                                    return "Get Category & Attribute JDID sedang berlangsung.";
                                }
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(customer.TOKEN) && !string.IsNullOrEmpty(customer.API_CLIENT_U) && !string.IsNullOrEmpty(customer.API_KEY))
                            {
                                var jdAPI2 = new JDIDController();
                                JDIDAPIData data = new JDIDAPIData()
                                {
                                    no_cust = customer.CUST,
                                    email = customer.EMAIL,
                                    accessToken = customer.TOKEN,
                                    appKey = customer.API_KEY,
                                    appSecret = customer.API_CLIENT_U,
                                    DatabasePathErasoft = getAkunRahma.DatabasePathErasoft,
                                    merchant_code = customer.Sort1_Cust,
                                    tgl_expired = customer.TGL_EXPIRED,
                                    versi = customer.KD_ANALISA,
                                    username = getAkunRahma.Username,
                                    //add by nurul 6/6/2021
                                    refreshToken = customer.REFRESH_TOKEN
                                    //add by nurul 6/6/2021
                                };
                                var result = await jdAPI2.JDID_getCategoryV2(data);
                                return "Get Category & Attribute JDID sedang berlangsung.";
                            }
                        }
                    }
                }
                return "Akun tidak ditemukan.";
            }
            catch (Exception ex)
            {
                return "Get Category & Attribute JDID gagal.";
            }
        }

        public async System.Threading.Tasks.Task<string> GetAttributeJDID(string email)
        {
            try
            {
                var sessionAccountUserID = System.Web.HttpContext.Current.Session["SessionAccountUserID"];

#if (AWS || Debug_AWS)
            var getAkunRahma = MoDbContext.Account.Where(a => a.Email == "aniesuprijati@gmail.com").FirstOrDefault();
#elif (DEV || DEBUG)
                var getAkunRahma = MoDbContext.Account.Where(a => a.Email == "rahmamk@gmail.com").FirstOrDefault();
#endif
                if (getAkunRahma != null)
                {
                    ErasoftDbContextNew = new ErasoftContext(getAkunRahma.DatabasePathErasoft);

                    var idmarket = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "JD.ID").IdMarket.ToString();
                    var customer = ErasoftDbContextNew.ARF01.Where(c => c.NAMA == idmarket && c.EMAIL == email).FirstOrDefault();
                    if (customer != null)
                    {
                        DateTime dateNow = DateTime.UtcNow.AddHours(7);
                        if (dateNow >= customer.TGL_EXPIRED)
                        {
                            JDIDControllerJob.JDIDAPIDataJob iden = new JDIDControllerJob.JDIDAPIDataJob();
                            iden.merchant_code = customer.Sort1_Cust;
                            iden.DatabasePathErasoft = getAkunRahma.DatabasePathErasoft;
                            iden.username = getAkunRahma.Username;
                            iden.no_cust = customer.CUST;
                            iden.tgl_expired = customer.TGL_EXPIRED;
                            iden.appKey = customer.API_KEY;
                            iden.appSecret = customer.API_CLIENT_U;
                            iden.accessToken = customer.TOKEN;
                            iden.refreshToken = customer.REFRESH_TOKEN;
                            //add by nurul 4/5/2021, JDID versi 2
                            iden.versi = customer.KD_ANALISA;
                            //end add by nurul 4/5/2021, JDID versi 2

                            // proses cek dan get token
                            //#if (AWS || DEV)
                            //                    client.Enqueue<JDIDControllerJob>(x => x.GetTokenJDID(iden, false, false));
                            //#else
                            //                                                Task.Run(() => new JDIDControllerJob().GetTokenJDID(iden, false, false)).Wait();
                            //#endif
                            await new JDIDControllerJob().GetTokenJDID(iden, false, false);

                            var arf01_2 = ErasoftDbContext.ARF01.Where(t => t.CUST == customer.CUST).FirstOrDefault();
                            if (arf01_2 != null)
                            {
                                if (!string.IsNullOrEmpty(arf01_2.TOKEN) && !string.IsNullOrEmpty(arf01_2.API_CLIENT_U) && !string.IsNullOrEmpty(arf01_2.API_KEY))
                                {
                                    var jdAPI2 = new JDIDController();
                                    JDIDAPIData data = new JDIDAPIData()
                                    {
                                        no_cust = arf01_2.CUST,
                                        email = arf01_2.EMAIL,
                                        accessToken = arf01_2.TOKEN,
                                        appKey = arf01_2.API_KEY,
                                        appSecret = arf01_2.API_CLIENT_U,
                                        DatabasePathErasoft = getAkunRahma.DatabasePathErasoft,
                                        merchant_code = arf01_2.Sort1_Cust,
                                        tgl_expired = arf01_2.TGL_EXPIRED,
                                        versi = arf01_2.KD_ANALISA,
                                        username = getAkunRahma.Username,
                                        //add by nurul 6/6/2021
                                        refreshToken = customer.REFRESH_TOKEN
                                        //add by nurul 6/6/2021
                                    };
                                    var getCat = MoDbContext.CATEGORY_JDID.Where(a => a.TYPE == "3").ToList();
                                    if (getCat.Count() > 0)
                                    {
                                        foreach (var cat in getCat)
                                        {
                                            var listAttributeJDID = await new JDIDController().getAttributeV2(data, cat.CATEGORY_CODE, cat.CATEGORY_NAME);
                                        }
                                        return "Get Attribute JDID sedang berlangsung.";
                                    }
                                    else
                                    {
                                        return "Get Attribute JDID gagal. Category kosong.";
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(customer.TOKEN) && !string.IsNullOrEmpty(customer.API_CLIENT_U) && !string.IsNullOrEmpty(customer.API_KEY))
                            {
                                var jdAPI2 = new JDIDController();
                                JDIDAPIData data = new JDIDAPIData()
                                {
                                    no_cust = customer.CUST,
                                    email = customer.EMAIL,
                                    accessToken = customer.TOKEN,
                                    appKey = customer.API_KEY,
                                    appSecret = customer.API_CLIENT_U,
                                    DatabasePathErasoft = getAkunRahma.DatabasePathErasoft,
                                    merchant_code = customer.Sort1_Cust,
                                    tgl_expired = customer.TGL_EXPIRED,
                                    versi = customer.KD_ANALISA,
                                    username = getAkunRahma.Username,
                                    //add by nurul 6/6/2021
                                    refreshToken = customer.REFRESH_TOKEN
                                    //add by nurul 6/6/2021
                                };
                                var getCat = MoDbContext.CATEGORY_JDID.Where(a => a.TYPE == "3").ToList();
                                if (getCat.Count() > 0)
                                {
                                    foreach (var cat in getCat)
                                    {
                                        var listAttributeJDID = await new JDIDController().getAttributeV2(data, cat.CATEGORY_CODE, cat.CATEGORY_NAME);
                                    }
                                    return "Get Attribute JDID sedang berlangsung.";
                                }
                                else
                                {
                                    return "Get Attribute JDID gagal. Category kosong.";
                                }
                            }
                        }
                    }
                }
                return "Akun tidak ditemukan.";
            }
            catch (Exception ex)
            {
                return "Get Category & Attribute JDID gagal.";
            }
        }
        //end add by nurul 27/5/2021
    }
    public class mo_account
    {
        public string datasourcepathdebug { get; set; }
        public string datasourcepath { get; set; }
    }
}