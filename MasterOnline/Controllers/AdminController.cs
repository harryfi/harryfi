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

namespace MasterOnline.Controllers
{
    public class AdminController : Controller
    {
        private readonly MoDbContext MoDbContext;

        public AdminController()
        {
            MoDbContext = new MoDbContext();
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

            if (admin.Password != adminFromDb.Password)
            {
                ModelState.AddModelError("", @"Password salah!");
                return View("Login", admin);
            }

            Session["SessionAdmin"] = adminFromDb;

            return RedirectToAction("AccountMenu");
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

            return View(accInDb);
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
        public ActionResult ChangeStatusAcc(int? accId)
        {
            var accInDb = MoDbContext.Account.Single(a => a.AccountId == accId);
            accInDb.Status = !accInDb.Status;
            string sql = "";
            var userId = Convert.ToString(accInDb.AccountId);

            accInDb.DatabasePathErasoft = "ERASOFT_" + userId;

            var path = Server.MapPath("~/Content/admin/");
            sql = $"RESTORE DATABASE {accInDb.DatabasePathErasoft} FROM DISK = '{path + "ERASOFT_backup_for_new_account.bak"}'" +
                  $" WITH MOVE 'erasoft' TO '{path}/{accInDb.DatabasePathErasoft}.mdf'," +
                  $" MOVE 'erasoft_log' TO '{path}/{accInDb.DatabasePathErasoft}.ldf';";
#if AWS
            SqlConnection con = new SqlConnection("Server=localhost;Initial Catalog=master;persist security info=True;" +
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
            ErasoftContext ErasoftDbContext = new ErasoftContext(accInDb.DatabasePathErasoft);
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

            ViewData["SuccessMessage"] = $"Akun {accInDb.Username} berhasil diubah statusnya dan dibuatkan database baru.";
            MoDbContext.SaveChanges();

            var listAcc = MoDbContext.Account.ToList();

            return View("AccountMenu", listAcc);
        }

        public ActionResult TambahHapusDatabaseAcc(int? accId)
        {
            var accInDb = MoDbContext.Account.FirstOrDefault(a => a.AccountId == accId);

            if (accInDb != null)
            {
                try
                {
#if AWS
                    System.Data.Entity.Database.Delete($"Server=localhost;Initial Catalog={accInDb.DatabasePathErasoft};persist security info=True;" +
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

            MoDbContext.SaveChanges();

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

        public ActionResult AktivitasSubscription()
        {
            var vm = new SubsViewModel()
            {
                ListAktivitasSubs = MoDbContext.AktivitasSubscription.ToList()
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

        // =============================================== Menu-menu pada halaman admin (START)

        [Route("admin/manage/account")]
        [SessionAdminCheck]
        public ActionResult AccountMenu()
        {
            var listAcc = MoDbContext.Account.ToList();
            return View(listAcc);
        }

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
                ListPartner = MoDbContext.Partner.ToList()
            };

            return View(vm);
        }

        // =============================================== Menu-menu pada halaman admin (END)

        public async Task<ActionResult> ChangeStatusPartner(string partnerid)
        {
            var partnerId = Convert.ToInt64(partnerid);
            var partnerInDb = MoDbContext.Partner.Single(u => u.PartnerId == partnerId);
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
                    .Replace("LINKPERSETUJUAN", Request.Url.GetLeftPart(UriPartial.Authority) + Url.Action("PartnerApproval", "Account", new {partnerId}));
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
                    Password = "erasoft123"
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
                        Password = "erasoft123"
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
            var vm = new PartnerViewModel()
            {
                ListPartner = MoDbContext.Partner.ToList()
            };

            return View("PartnerMenu", vm);
        }

        public ActionResult GeneratorSqlMenu()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetResultQuery(DataForQuery data)
        {
            string resultQuery = "";
            string insertDataQuery = data.MigrationHistoryInsertQuery;

            resultQuery = "DECLARE @db_name NVARCHAR (MAX) \n" +
                          "DECLARE c_db_names CURSOR FOR \n" +
                          "SELECT name FROM sys.databases \n" +
                          "WHERE name NOT IN('master', 'tempdb', 'model', 'msdb', 'activity', 'ReportServer$SQLEXPRESS', 'mo', " +
                          "'ReportServer$SQLEXPRESSTempDB', 'SCREEN_ACTIVITY', 'REPORTSI', 'REPORTST', 'erasoft', 'AP_NET', 'AR_NET', " +
                          "'MD_NET', 'SI_NET', 'ST_NET', 'SCREEN-NET2', 'REPORTAP', 'REPORTAR', 'REPORTMD') \n" +
                          "OPEN c_db_names \n" +
                          "FETCH c_db_names INTO @db_name \n" +
                          "WHILE @@Fetch_Status = 0 \n" +
                          "BEGIN \n" +
                          $"EXEC('{insertDataQuery}') \n" +
                          "FETCH c_db_names INTO @db_name \n" +
                          "END \n" +
                          "CLOSE c_db_names \n" +
                          "DEALLOCATE c_db_names";

            return Json(resultQuery, JsonRequestBehavior.AllowGet);
        }
    }
}