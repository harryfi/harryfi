using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
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
                return HttpNotFound();

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

            if (accInDb.Status == false)
            {
                var listUserPerAcc = MoDbContext.User.Where(u => u.AccountId == accId).ToList();
                foreach (var user in listUserPerAcc)
                {
                    user.Status = false;
                }
            }

            try
            {
                var userId = accInDb.UserId;
                string sql = "";

                if (accInDb.Status)
                {
                    var path = Server.MapPath("~/Content/admin/");
                    sql = $"RESTORE DATABASE ERASOFT_{userId} FROM DISK = '{path + "ERASOFT_backup_for_new_account.bak"}'" +
                                 $" WITH MOVE 'erasoft' TO '{path}/ERASOFT_{userId}.mdf'," +
                                 $" MOVE 'erasoft_log' TO '{path}/ERASOFT_{userId}.ldf';";
#if AWS
                    SqlConnection con = new SqlConnection("Server=localhost;Initial Catalog=master;persist security info=True;" +
                                                          "user id=sa;password=admin123^;");
#else
                    SqlConnection con = new SqlConnection("Server=202.67.14.92\\SQLEXPRESS,1433;Initial Catalog=master;persist security info=True;" +
                                                          "user id=masteronline;password=M@ster123;");
#endif
                    SqlCommand command = new SqlCommand(sql, con);

                    con.Open();
                    command.ExecuteNonQuery();
                    con.Close();
                    con.Dispose();

                    //add by Tri 20-09-2018, save nama toko ke SIFSYS
                    ErasoftContext ErasoftDbContext = new ErasoftContext(userId);
                    var dataPerusahaan = ErasoftDbContext.SIFSYS.FirstOrDefault();
                    if (string.IsNullOrEmpty(dataPerusahaan.NAMA_PT))
                    {
                        dataPerusahaan.NAMA_PT = accInDb.NamaTokoOnline;
                        ErasoftDbContext.SaveChanges();
                    }                    
                    //end add by Tri 20-09-2018, save nama toko ke SIFSYS

                }
                else
                {
#if AWS
                    System.Data.Entity.Database.Delete($"Server=localhost;Initial Catalog=ERASOFT_{userId};persist security info=True;" +
                                                       "user id=sa;password=admin123^;");
#else
                    System.Data.Entity.Database.Delete($"Server=202.67.14.92\\SQLEXPRESS,1433;Initial Catalog=ERASOFT_{userId};persist security info=True;" +
                                                       "user id=masteronline;password=M@ster123;");
#endif
                }
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }

            ViewData["SuccessMessage"] = $"Akun {accInDb.Username} berhasil diubah statusnya.";
            MoDbContext.SaveChanges();

            var listAcc = MoDbContext.Account.ToList();

            return View("AccountMenu", listAcc);
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

        // =============================================== Menu-menu pada halaman admin (END)
    }
}