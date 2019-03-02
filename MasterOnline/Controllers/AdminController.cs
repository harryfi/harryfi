﻿using System;
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

            //add by nurul 20/2/2019
            var vm = new MenuAccount()
            {
                Account = accInDb
            };
            //end add by nurul 20/2/2019

            //return View(accInDb);
            return PartialView("AccountDetail",vm);
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
            //add by Tri, set free trials 14 hari
            if (accInDb.Status)
            {
                if (accInDb.KODE_SUBSCRIPTION == "01")
                {
                    accInDb.TGL_SUBSCRIPTION = DateTime.Today.AddDays(14);
                }
            }
            //end add by Tri, set free trials 14 hari

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
                ListAktivitasSubs = MoDbContext.AktivitasSubscription.ToList(),
                //ADD BY NURUL 22/2/2019
                ListSubs=MoDbContext.Subscription.ToList()
                //END ADD BY NURUL 22/2/2019
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

        [SessionAdminCheck]
        public ActionResult AccountMenuWillExpiredEdit(string param)
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
                ListAccount = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION >= drTgl && a.TGL_SUBSCRIPTION <= sdTgl).ToList(),
                ListPartner = MoDbContext.Partner.ToList()
            };
            return PartialView("TableAccountWillExpiredEdit", vm);
        }
        [SessionAdminCheck]
        public ActionResult AccountMenuExpiredEdit(string param)
        {
            string tgl1 = (param.Split('/')[param.Split('/').Length - 3]);
            string bln1 = (param.Split('/')[param.Split('/').Length - 2]);
            string thn1 = (param.Split('/')[param.Split('/').Length - 1]);
            string tanggal = tgl1 + '/' + bln1 + '/' + thn1;
            var perTgl = DateTime.ParseExact(tanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var vm = new MenuAccount()
            {
                ListAccount = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION <= perTgl).ToList(),
                ListPartner = MoDbContext.Partner.ToList()
            };
            return PartialView("TableAccountExpiredEdit", vm);
        }

        public ActionResult EditAccount(int? accountId)
        {
            var vm = new MenuAccount()
            {
                Account = MoDbContext.Account.SingleOrDefault(m => m.AccountId == accountId),
                ListSubs=MoDbContext.Subscription.ToList()
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
                accInDb.TGL_SUBSCRIPTION = data.Account.TGL_SUBSCRIPTION;
                accInDb.NoHp = data.Account.NoHp;
                accInDb.jumlahUser = data.Account.jumlahUser;
            }

            MoDbContext.SaveChanges();
            ModelState.Clear();

            //return RedirectToAction("AccountMenuEdit");
            //return RedirectToAction("FormAccountPartial");
            var vm = new MenuAccount()
            {
                Account = MoDbContext.Account.SingleOrDefault(a => a.AccountId == data.Account.AccountId)
            };

            return PartialView("FormAccountPartial", vm);
        }
        // =============================================== Bagian Account (END)
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
                ListAccount = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION >= drTgl && a.TGL_SUBSCRIPTION <= sdTgl).ToList(),
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
                ListAccount = MoDbContext.Account.Where(a => a.TGL_SUBSCRIPTION <= perTgl).ToList(),
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
                ListPartner = MoDbContext.Partner.ToList()
            };

            return View(vm);
        }

        //add by nurul 15/2/2019
        // =============================================== Menu Partner
        public ActionResult EditKomisi(int? partnerid)
        {
            var vm = new PartnerViewModel()
            {
                partner = MoDbContext.Partner.SingleOrDefault(m => m.PartnerId == partnerid),
            };

            ViewData["Editing"] = 1;

            return View("PartnerMenu", vm);
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
                if (partInDb.Status != vm.partner.Status)
                {
                   Task<ActionResult> x = ChangeStatusPartner(Convert.ToString(vm.partner.PartnerId));
                }
                partInDb.komisi_subscribe = vm.partner.komisi_subscribe;
                partInDb.komisi_subscribe_gold = vm.partner.komisi_subscribe_gold;
                partInDb.komisi_support = vm.partner.komisi_support;
            }

            MoDbContext.SaveChanges();
            ModelState.Clear();

            return RedirectToAction("PartnerMenu");
        }
        // =============================================== Menu Partner (END)
        //end add by nurul 15/2/2019

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
    }
}