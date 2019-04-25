﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Security;
using MasterOnline.Models;
using MasterOnline.Models.Api;
using MasterOnline.Utils;
using MasterOnline.ViewModels;

using System.Web;
using System.Collections.Generic;
using System.Security.Claims;
using Hangfire;
using Hangfire.Server;
using Hangfire.Storage;
using Hangfire.SqlServer;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity;
using Erasoft.Function;

namespace MasterOnline.Controllers
{
    public class AccountController : Controller
    {
        private MoDbContext MoDbContext;
        private AccountUserViewModel _viewModel;
        //add by calvin 1 april 2019
        public void IdentitySignin(string userId, string name, string providerKey = null, bool isPersistent = false)
        {
            var claims = new List<Claim>();

            // create *required* claims
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            claims.Add(new Claim(ClaimTypes.Name, name));

            var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

            // add to user here!
            AuthenticationManager.SignIn(new AuthenticationProperties()
            {
                AllowRefresh = true,
                IsPersistent = isPersistent,
                ExpiresUtc = DateTime.UtcNow.AddDays(7)
            }, identity);
        }

        public void IdentitySignout()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie,
                                          DefaultAuthenticationTypes.ExternalCookie);
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
        //end add by calvin 1 april 2019

        public AccountController()
        {
            MoDbContext = new MoDbContext();
            _viewModel = new AccountUserViewModel();
        }

        protected override void Dispose(bool disposing)
        {
            MoDbContext.Dispose();
        }

        // Route ke halaman login
        [System.Web.Mvc.Route("login")]
        public ActionResult Login(string Ref)
        {
            var partnerInDb = MoDbContext.Partner.FirstOrDefault(p => p.KodeRefPilihan == Ref);

            if (Ref != null && partnerInDb == null)
            {
                return View("Error");
            }

            if (partnerInDb != null)
            {
                if (!partnerInDb.Status || !partnerInDb.StatusSetuju)
                {
                    return View("Error");
                }
            }

            return View();
        }

        [System.Web.Mvc.Route("loginSubs")]
        public ActionResult LoginSubs(string Ref, string kode, string bln, int jmlUser)
        {
            var partnerInDb = MoDbContext.Partner.FirstOrDefault(p => p.KodeRefPilihan == Ref);

            if (!string.IsNullOrEmpty(Ref) && partnerInDb == null)
            {
                return View("Error");
            }

            if (partnerInDb != null)
            {
                if (!partnerInDb.Status || !partnerInDb.StatusSetuju)
                {
                    return View("Error");
                }
            }

            var vm = new Account
            {
                DatabasePathMo = bln,
                KODE_SUBSCRIPTION = kode,
                jumlahUser = jmlUser
            };
            return View("Register", vm);
        }


        // Halaman login
        [System.Web.Mvc.Route("support/login")]
        public ActionResult SupportLogin()
        {
            return View();
        }

        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SupportPickAccount(SupportLogin admin)
        {
            ModelState.Remove("AdminId");
            ModelState.Remove("Username");

            if (!ModelState.IsValid)
            {
                return View("SupportLogin", admin);
            }
            {
                System.Collections.Generic.List<string> ListSupport = new System.Collections.Generic.List<string>();

                ListSupport.Add("marlakhy@yahoo.com");
                ListSupport.Add("calvintes2@email.com");
                ListSupport.Add("rahmamk@gmail.com");

                if (!ListSupport.Contains(admin.Email.ToLower()))
                {
                    ModelState.AddModelError("", @"Support tidak ditemukan!");
                    return View("SupportLogin", admin);
                }

                var accInDb = MoDbContext.Account.SingleOrDefault(a => a.Email == admin.Email);

                if (accInDb == null)
                {
                    ModelState.AddModelError(string.Empty, @"Username / Email tidak ditemukan!");
                    return View("SupportLogin", admin);
                }

                var key = accInDb.VCode;
                var originPassword = admin.Password;
                var encodedPassword = MasterOnline.Utils.Helper.EncodePassword(originPassword, key);
                var pass = accInDb.Password;

                if (!encodedPassword.Equals(pass))
                {
                    ModelState.AddModelError(string.Empty, @"Password salah!");
                    ModelState.AddModelError(string.Empty, @"SUCCESS");
                    return View("SupportLogin", admin);
                }

                if (!accInDb.Status)
                {
                    ModelState.AddModelError(string.Empty, @"Akun tidak aktif!");
                    ModelState.AddModelError(string.Empty, @"SUCCESS");
                    return View("SupportLogin", admin);
                }

            }

            var accSelected = MoDbContext.Account.SingleOrDefault(a => a.Email == admin.SelectedAccount);

            if (accSelected == null)
            {
                var userFromDb = MoDbContext.User.SingleOrDefault(a => a.Email == admin.SelectedAccount);


                if (userFromDb == null)
                {
                    ModelState.AddModelError(string.Empty, @"Username / Email tidak ditemukan!");
                    ModelState.AddModelError(string.Empty, @"SUCCESS");
                    return View("SupportLogin", admin);
                }

                var accInDb = MoDbContext.Account.Single(ac => ac.AccountId == userFromDb.AccountId);

                if (!userFromDb.Status)
                {
                    ModelState.AddModelError(string.Empty, @"Akun tidak aktif!");
                    ModelState.AddModelError(string.Empty, @"SUCCESS");
                    return View("SupportLogin", admin);
                }

                _viewModel.User = userFromDb;
            }
            else
            {
                if (!accSelected.Status)
                {
                    ModelState.AddModelError(string.Empty, @"Akun tidak aktif!");
                    ModelState.AddModelError(string.Empty, @"SUCCESS");
                    return View("SupportLogin", admin);
                }

                _viewModel.Account = accSelected;
            }

            Session["SessionInfo"] = _viewModel;

            DatabaseSQL EDB; //add by calvin 1 april 2019
            ErasoftContext erasoftContext = null;
            string dbPathEra = "";
            if (_viewModel?.Account != null)
            {
                erasoftContext = _viewModel.Account.UserId == "admin_manage" ? new ErasoftContext() : new ErasoftContext(_viewModel.Account.DatabasePathErasoft);
                //add by calvin 1 april 2019
                EDB = new DatabaseSQL(_viewModel.Account.DatabasePathErasoft);
                dbPathEra = _viewModel.Account.DatabasePathErasoft;
                //IdentitySignin(_viewModel.Account.Email, _viewModel.Account.Username);
                //end add by calvin 1 april 2019
            }
            else
            {
                var accFromUser = MoDbContext.Account.Single(a => a.AccountId == _viewModel.User.AccountId);
                erasoftContext = new ErasoftContext(accFromUser.DatabasePathErasoft);
                //add by calvin 1 april 2019
                EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
                dbPathEra = accFromUser.DatabasePathErasoft;
                //IdentitySignin(accFromUser.Email, accFromUser.Username);
                //end add by calvin 1 april 2019
            }

            var dataUsahaInDb = erasoftContext.SIFSYS.Single(p => p.BLN == 1);
            var jumlahAkunMarketplace = erasoftContext.ARF01.Count();

            if (dataUsahaInDb?.NAMA_PT != "PT ERAKOMP INFONUSA" && jumlahAkunMarketplace > 0)
            {
                //change by calvin 1 april 2019
                //SyncMarketplace(erasoftContext, dataUsahaInDb.JTRAN_RETUR);
                string username = _viewModel.Account != null ? _viewModel.Account.Username : _viewModel.User.Username;
                bool cekSyncMarketplace = false;
                if (cekSyncMarketplace)
                {
                    Task.Run(() => SyncMarketplace(dbPathEra, EDB.GetConnectionString("ConnID"), dataUsahaInDb.JTRAN_RETUR, username, 5).Wait());
                }
                //end change by calvin 1 april 2019
                return RedirectToAction("Index", "Manage", "SyncMarketplace");
            }

            return RedirectToAction("Bantuan", "Manage");
        }

        //Proses login support
        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SupportLoggingIn(SupportLogin admin)
        {
            ModelState.Remove("AdminId");
            ModelState.Remove("Username");

            if (!ModelState.IsValid)
            {
                return View("SupportLogin", admin);
            }

            System.Collections.Generic.List<string> ListSupport = new System.Collections.Generic.List<string>();
            ListSupport.Add("marlakhy@yahoo.com");
            ListSupport.Add("calvintes2@email.com");
            ListSupport.Add("rahmamk@gmail.com");

            if (!ListSupport.Contains(admin.Email.ToLower()))
            {
                ModelState.AddModelError("", @"Support tidak ditemukan!");
                return View("SupportLogin", admin);
            }

            var accFromDb = MoDbContext.Account.SingleOrDefault(a => a.Email == admin.Email);

            if (accFromDb == null)
            {
                ModelState.AddModelError(string.Empty, @"Username / Email tidak ditemukan!");
                return View("SupportLogin", admin);
            }
            var accInDb = MoDbContext.Account.Single(ac => ac.AccountId == accFromDb.AccountId);
            var key = accInDb.VCode;
            var originPassword = admin.Password;
            var encodedPassword = MasterOnline.Utils.Helper.EncodePassword(originPassword, key);
            var pass = accFromDb.Password;

            if (!encodedPassword.Equals(pass))
            {
                ModelState.AddModelError(string.Empty, @"Password salah!");
                return View("SupportLogin", admin);
            }

            if (!accFromDb.Status)
            {
                ModelState.AddModelError(string.Empty, @"Akun tidak aktif!");
                return View("SupportLogin", admin);
            }

            var accList = MoDbContext.Account.Where(p => p.Status).Select(p => p.Email).ToList();
            admin.AccountList = accList;

            //_viewModel.User = userFromDb;

            //Session["SessionInfo"] = _viewModel;

            //ErasoftContext erasoftContext = null;

            //if (_viewModel?.Account != null)
            //{
            //    erasoftContext = _viewModel.Account.UserId == "admin_manage" ? new ErasoftContext() : new ErasoftContext(_viewModel.Account.DatabasePathErasoft);
            //}
            //else
            //{
            //    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == _viewModel.User.AccountId);
            //    erasoftContext = new ErasoftContext(accFromUser.DatabasePathErasoft);
            //}

            //var dataUsahaInDb = erasoftContext.SIFSYS.Single(p => p.BLN == 1);
            //var jumlahAkunMarketplace = erasoftContext.ARF01.Count();

            //if (dataUsahaInDb?.NAMA_PT != "PT ERAKOMP INFONUSA" && jumlahAkunMarketplace > 0)
            //{
            //    SyncMarketplace(erasoftContext);
            //    return RedirectToAction("Index", "Manage", "SyncMarketplace");
            //}
            //return RedirectToAction("Bantuan", "Manage");

            ModelState.AddModelError(string.Empty, @"SUCCESS");
            return View("SupportLogin", admin);
        }

        // Proses Logging In dari Acc / User
        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LoggingIn(Account account)
        {
            ModelState.Remove("NamaTokoOnline");

            if (!ModelState.IsValid)
                return View("Login", account);

            //Configuration connectionConfiguration = WebConfigurationManager.OpenWebConfiguration("~");
            var accFromDb = MoDbContext.Account.SingleOrDefault(a => a.Email == account.Email);

            if (accFromDb == null)
            {
                var userFromDb = MoDbContext.User.SingleOrDefault(a => a.Email == account.Email);


                if (userFromDb == null)
                {
                    ModelState.AddModelError(string.Empty, @"Username / Email tidak ditemukan!");
                    return View("Login", account);
                }

                var accInDb = MoDbContext.Account.Single(ac => ac.AccountId == userFromDb.AccountId);
                var key = accInDb.VCode;
                var originPassword = account.Password;
                var encodedPassword = Helper.EncodePassword(originPassword, key);
                var pass = userFromDb.Password;

                if (!encodedPassword.Equals(pass))
                {
                    ModelState.AddModelError(string.Empty, @"Password salah!");
                    return View("Login", account);
                }

                if (!userFromDb.Status)
                {
                    ModelState.AddModelError(string.Empty, @"Akun tidak aktif!");
                    return View("Login", account);
                }

                _viewModel.User = userFromDb;
                //var accByUser = MoDbContext.Account.Single(a => a.AccountId == userFromDb.AccountId);
                //connectionConfiguration.ConnectionStrings.ConnectionStrings["PerAccContext"].ConnectionString = $"Server=13.251.222.53\\SQLEXPRESS, 1433;initial catalog=ERASOFT_{accByUser.UserId};user id=masteronline;password=M@ster123;multipleactiveresultsets=True;application name=EntityFramework";
            }
            else
            {
                var pass = accFromDb.Password;
                var hashCode = accFromDb.VCode;
                var encodingPassString = Helper.EncodePassword(account.Password, hashCode);

                if (!encodingPassString.Equals(pass))
                {
                    ModelState.AddModelError(string.Empty, @"Password salah!");
                    return View("Login", account);
                }

                if (!accFromDb.Status)
                {
                    ModelState.AddModelError(string.Empty, @"Akun tidak aktif!");
                    return View("Login", account);
                }

                _viewModel.Account = accFromDb;
                //connectionConfiguration.ConnectionStrings.ConnectionStrings["PerAccContext"].ConnectionString = $"Server=13.251.222.53\\SQLEXPRESS, 1433;initial catalog=ERASOFT_{accFromDb.UserId};user id=masteronline;password=M@ster123;multipleactiveresultsets=True;application name=EntityFramework";
            }

            Session["SessionInfo"] = _viewModel;

            DatabaseSQL EDB; //add by calvin 1 april 2019
            ErasoftContext erasoftContext = null;
            string dbPathEra = "";

            if (_viewModel?.Account != null)
            {
                erasoftContext = _viewModel.Account.UserId == "admin_manage" ? new ErasoftContext() : new ErasoftContext(_viewModel.Account.DatabasePathErasoft);
                //add by calvin 1 april 2019
                EDB = new DatabaseSQL(_viewModel.Account.DatabasePathErasoft);
                dbPathEra = _viewModel.Account.DatabasePathErasoft;
                accFromDb.LAST_LOGIN_DATE = DateTime.UtcNow;
                MoDbContext.SaveChanges();
                //IdentitySignin(_viewModel.Account.Email, _viewModel.Account.Username);
                //end add by calvin 1 april 2019
            }
            else
            {
                var accFromUser = MoDbContext.Account.Single(a => a.AccountId == _viewModel.User.AccountId);
                erasoftContext = new ErasoftContext(accFromUser.DatabasePathErasoft);
                //add by calvin 1 april 2019
                EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
                dbPathEra = accFromUser.DatabasePathErasoft;
                accFromUser.LAST_LOGIN_DATE = DateTime.UtcNow;
                MoDbContext.SaveChanges();
                //IdentitySignin(accFromUser.Email, accFromUser.Username);
                //end add by calvin 1 april 2019
            }

            var dataUsahaInDb = erasoftContext.SIFSYS.Single(p => p.BLN == 1);
            var jumlahAkunMarketplace = erasoftContext.ARF01.Count();

            if (dataUsahaInDb?.NAMA_PT != "PT ERAKOMP INFONUSA" && jumlahAkunMarketplace > 0)
            {
                //change by calvin 1 april 2019
                //SyncMarketplace(erasoftContext, dataUsahaInDb.JTRAN_RETUR);
                string username = _viewModel.Account != null ? _viewModel.Account.Username : _viewModel.User.Username;
                Task.Run(() => SyncMarketplace(dbPathEra, EDB.GetConnectionString("ConnID"), dataUsahaInDb.JTRAN_RETUR, username, 5).Wait());

                //var jdapi = new JDIDController();
                //var data3 = new JDIDAPIData
                //{
                //    accessToken = "4304bd28315728067f7db7e6ff8cc015",
                //    appKey = "86b082cb8d3436bb340739a90d953ec7",
                //    appSecret = "1bcda1dca02339e049cb26c5b4c7da12"
                //};
                //jdapi.Order_JD(data3, "000025");

                //end change by calvin 1 april 2019

                return RedirectToAction("Index", "Manage", "SyncMarketplace");
            }

            return RedirectToAction("Bantuan", "Manage");
        }

        //change by calvin 1 april 2019
        //protected void SyncMarketplace(ErasoftContext LocalErasoftDbContext, string jtran_retur)
        public async Task<string> SyncMarketplace(string dbPathEra, string EDBConnID, string sync_pesanan_stok, string username, int recurr_interval)
        //end change by calvin 1 april 2019
        {
            //catatan by calvin : jika developer sedang mau mengecek API, tidak perlu menggunakan backgroundjob untuk memanggil API
            //-jika terjadi jobs nyangkut ( enqueued dan tidak di proses )
            //maka lakukan :
            //dari sisi developer: lakukanHapusServer = true;
            //lalu pada masteronline.co.id lakukan login dengan login support
            //atau minta user login ulang

            //MoDbContext = new MoDbContext();
            bool lakukanHapusServer = false;
            ErasoftContext LocalErasoftDbContext = new ErasoftContext(dbPathEra);
            MoDbContext = new MoDbContext();

            var sqlStorage = new SqlServerStorage(EDBConnID);
            var monitoringApi = sqlStorage.GetMonitoringApi();
            var serverList = monitoringApi.Servers();

            if (serverList.Count() > 0)
            {
#if Debug_AWS
                if (lakukanHapusServer)
                {
                    foreach (var server in serverList)
                    {
                        var serverConnection = sqlStorage.GetConnection();
                        serverConnection.RemoveServer(server.Name);
                        serverConnection.Dispose();
                    }
                }
#else
                
#endif
            }
            if (serverList.Count() == 0)
            {
#if Debug_AWS

#else
                var optionsStatusResiServer = new BackgroundJobServerOptions
                {
                    ServerName = "StatusResiPesanan",
                    Queues = new[] { "1_manage_pesanan" },
                    WorkerCount = 2,
                    
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
                    WorkerCount = 3,
                };
                var newStokServer = new BackgroundJobServer(optionsStokServer, sqlStorage);
#endif

            }

            var client = new BackgroundJobClient(sqlStorage);
            RecurringJobManager recurJobM = new RecurringJobManager(sqlStorage);
            RecurringJobOptions recurJobOpt = new RecurringJobOptions()
            {
                QueueName = "3_general"
            };

            //using (var connection = sqlStorage.GetConnection())
            //{
            //    foreach (var recurringJob in connection.GetRecurringJobs())
            //    {
            //        recurJobM.AddOrUpdate(recurringJob.Id,recurringJob.Job, Cron.MinuteInterval(30),recurJobOpt);
            //    }
            //}

            //testFailedNotif
            //recurJobM.AddOrUpdate("calvintesfailed", Hangfire.Common.Job.FromExpression<StokControllerJob>(x => x.testFailedNotif(dbPathEra, "Failed 1 min")), Cron.MinuteInterval(1), recurJobOpt);
            //recurJobM.AddOrUpdate("calvintesfailed2", Hangfire.Common.Job.FromExpression<StokControllerJob>(x => x.testFailedNotif(dbPathEra, "Failed 2 min")), Cron.MinuteInterval(1), recurJobOpt);
            //recurJobM.RemoveIfExists("calvintesfailed");
            //recurJobM.RemoveIfExists("calvintesfailed2");

            //var delaay = new TimeSpan(0, 0, 0);
            //client.Schedule<Hubs.MasterOnlineHub>(x => x.Announcement("Maaf, MasterOnline akan ditutup untuk sementara waktu untuk dilakukan maintenance pada jam 14:40 WIB"), delaay);

            //var test = new JDIDController();
            //var categoryJD = LocalErasoftDbContext.CATEGORY_JDID.Where(m => m.LEAF == "1").ToList();
            //if (categoryJD.Count > 0)
            //{
            //    var jdCode = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "JD.ID");
            //    var listJD = LocalErasoftDbContext.ARF01.Where(m => m.NAMA == jdCode.IdMarket.ToString()).ToList();
            //    var data = new JDIDAPIData
            //    {
            //        accessToken = listJD[0].TOKEN,
            //        appKey = listJD[0].API_CLIENT_P,
            //        appSecret = listJD[0].API_CLIENT_U
            //    };
            //    var a = test.getAttribute(data, categoryJD[0].CATEGORY_CODE);
            //    var b = test.getAttributeOpt(data, categoryJD[0].CATEGORY_CODE, a.ACODE_1, 1);
            //}
            //test.getCategory();
            //var mid = new MidtransController();
            //var dataMid = new MidtransTransactionData
            //{
            //    va_numbers = new BCAVA[1],
            //    transaction_time = "2019-03-06 15:07:50",
            //    transaction_status = "settlement",
            //    transaction_id = "19b283b2-ed3b-405e-8cb2-c4488a61c98e",
            //    status_message = "midtrans payment notification",
            //    status_code = "200",
            //    signature_key = "143cc0e7ccd2b92d20afd1d865c858e6c9735dfa40f390d5864ef363549ff345b7080dab88dc9d900fdf964c2fad7fcbaa1c526359b8791d1b32c5dbb1cec45d",
            //    //settlement_time = "2019-03-06 15:14:45",
            //    payment_type = "bank_transfer",
            //    order_id = "190000008027",
            //    gross_amount = "2400000.00",
            //    fraud_status = "accept"
            //};
            //dataMid.va_numbers[0] = new BCAVA
            //{
            //    bank = "bca",
            //    va_number = "644537235209833"
            //};
            //mid.PostReceive(dataMid);
            //add by calvin 9 oktober 2018
            //delete log API older than 7 days
            var deleteOldLogs = (from p in LocalErasoftDbContext.API_LOG_MARKETPLACE where p.REQUEST_DATETIME.Day.CompareTo(DateTime.Now.Day) > 7 select p).ToList();
            LocalErasoftDbContext.API_LOG_MARKETPLACE.RemoveRange(deleteOldLogs);
            LocalErasoftDbContext.SaveChanges();
            //end add by calvin 9 oktober 2018

            //if (sessionData?.Account != null)
            //{
            //    if (sessionData.Account.UserId == "admin_manage")
            //        ErasoftDbContext = new ErasoftContext();
            //    else
            //        ErasoftDbContext = new ErasoftContext(sessionData.Account.UserId);
            //}
            //else
            //{
            //    if (sessionData?.User != null)
            //    {
            //        var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
            //        ErasoftDbContext = new ErasoftContext(accFromUser.UserId);
            //    }
            //}
            //string username = sessionData.Account.Username;

            #region bukalapak
            var kdBL = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BUKALAPAK");
            var listBLShop = LocalErasoftDbContext.ARF01.Where(m => m.NAMA == kdBL.IdMarket.ToString()).ToList();
            if (listBLShop.Count > 0)
            {
                foreach (ARF01 tblCustomer in listBLShop)
                {
                    if (!string.IsNullOrEmpty(tblCustomer.TOKEN))
                    {
                        string connId_JobId = dbPathEra + "_bukalapak_pesanan_" + Convert.ToString(tblCustomer.RecNum.Value);
                        if (sync_pesanan_stok == "1")
                        {
                            recurJobM.AddOrUpdate(connId_JobId, Hangfire.Common.Job.FromExpression<BukaLapakControllerJob>(x => x.cekTransaksi(tblCustomer.CUST, tblCustomer.EMAIL, tblCustomer.API_KEY, tblCustomer.TOKEN, dbPathEra, username)), Cron.MinuteInterval(5), recurJobOpt);
                            //new BukaLapakControllerJob().cekTransaksi(tblCustomer.CUST, tblCustomer.EMAIL, tblCustomer.API_KEY, tblCustomer.TOKEN, dbPathEra, username);
                        }
                        else
                        {
                            recurJobM.RemoveIfExists(connId_JobId);
                        }
                    }
                }
            }
            #endregion

            #region lazada
            var kdLazada = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "LAZADA");
            var listLazadaShop = LocalErasoftDbContext.ARF01.Where(m => m.NAMA == kdLazada.IdMarket.ToString()).ToList();
            //var lzdApi = new LazadaController();
            if (listLazadaShop.Count > 0)
            {
                foreach (ARF01 tblCustomer in listLazadaShop)
                {
                    if (!string.IsNullOrEmpty(tblCustomer.TOKEN))
                    {
                        #region refresh token lazada
                        //change by calvin 4 april 2019
                        //lzdApi.GetRefToken(tblCustomer.CUST, tblCustomer.REFRESH_TOKEN);
                        //lzdApi.GetShipment(tblCustomer.CUST, tblCustomer.TOKEN);
                        client.Enqueue<LazadaControllerJob>(x => x.GetRefToken(tblCustomer.CUST, tblCustomer.REFRESH_TOKEN, dbPathEra, username));
                        //end change by calvin 4 april 2019
                        #endregion
                        //var stf02hinDB = LocalErasoftDbContext.STF02H.Where(p => !string.IsNullOrEmpty(p.BRG_MP) && p.IDMARKET == tblCustomer.RecNum).ToList();
                        //foreach (var item in stf02hinDB)
                        //{
                        //    var barangInDb = LocalErasoftDbContext.STF02.SingleOrDefault(b => b.BRG == item.BRG);
                        //    if (barangInDb != null)
                        //    {
                        //        var qtyOnHand = 0d;
                        //        {
                        //            object[] spParams = {
                        //                    new SqlParameter("@BRG", barangInDb.BRG),
                        //                    new SqlParameter("@GD","ALL"),
                        //                    new SqlParameter("@Satuan", "2"),
                        //                    new SqlParameter("@THN", Convert.ToInt16(DateTime.Now.ToString("yyyy"))),
                        //                    new SqlParameter("@QOH", SqlDbType.Decimal) {Direction = ParameterDirection.Output}
                        //                };

                        //            LocalErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);
                        //            qtyOnHand = Convert.ToDouble(((SqlParameter)spParams[4]).Value);
                        //        }
                        //        lzdApi.UpdatePriceQuantity(item.BRG_MP, "", (qtyOnHand > 0 ? qtyOnHand.ToString() : "1"), tblCustomer.TOKEN);
                        //    }

                        //}
                        if (sync_pesanan_stok == "1")
                        {
                            string connId_JobId = dbPathEra + "_lazada_pesanan_" + Convert.ToString(tblCustomer.RecNum.Value);
                            //new LazadaControllerJob().GetOrders(tblCustomer.CUST, tblCustomer.TOKEN, dbPathEra, username);
                            recurJobM.AddOrUpdate(connId_JobId, Hangfire.Common.Job.FromExpression<LazadaControllerJob>(x => x.GetOrders(tblCustomer.CUST, tblCustomer.TOKEN, dbPathEra, username)), Cron.MinuteInterval(5), recurJobOpt);
                        }
                        else
                        {
                            string connId_JobId = dbPathEra + "_lazada_pesanan_" + Convert.ToString(tblCustomer.RecNum.Value);
                            recurJobM.RemoveIfExists(connId_JobId);
                        }
                    }
                }
            }
            #endregion

            #region Blibli
            var kdBli = MoDbContext.Marketplaces.Single(m => m.NamaMarket.ToUpper() == "BLIBLI");
            var listBLIShop = LocalErasoftDbContext.ARF01.Where(m => m.NAMA == kdBli.IdMarket.ToString()).ToList();
            if (listBLIShop.Count > 0)
            {
                //remark by calvin 1 april 2019
                //var BliApi = new BlibliController();
                foreach (ARF01 tblCustomer in listBLIShop)
                {
                    if (!string.IsNullOrEmpty(tblCustomer.API_CLIENT_P) && !string.IsNullOrEmpty(tblCustomer.API_CLIENT_U))
                    {
                        //change by calvin 1 april 2019
                        //BlibliController.BlibliAPIData data = new BlibliController.BlibliAPIData()
                        //{
                        //    API_client_username = tblCustomer.API_CLIENT_U,
                        //    API_client_password = tblCustomer.API_CLIENT_P,
                        //    API_secret_key = tblCustomer.API_KEY,
                        //    mta_username_email_merchant = tblCustomer.EMAIL,
                        //    mta_password_password_merchant = tblCustomer.PASSWORD,
                        //    merchant_code = tblCustomer.Sort1_Cust,
                        //    token = tblCustomer.TOKEN,
                        //    idmarket = tblCustomer.RecNum.Value
                        //};
                        //BliApi.GetToken(data, true, false);
                        ////Task.Run(() => BliApi.GetCategoryTree(data)).Wait();
                        BlibliControllerJob.BlibliAPIData data = new BlibliControllerJob.BlibliAPIData()
                        {
                            API_client_username = tblCustomer.API_CLIENT_U,
                            API_client_password = tblCustomer.API_CLIENT_P,
                            API_secret_key = tblCustomer.API_KEY,
                            mta_username_email_merchant = tblCustomer.EMAIL,
                            mta_password_password_merchant = tblCustomer.PASSWORD,
                            merchant_code = tblCustomer.Sort1_Cust,
                            token = tblCustomer.TOKEN,
                            idmarket = tblCustomer.RecNum.Value,
                            DatabasePathErasoft = dbPathEra,
                            username = username
                        };

                        client.Enqueue<BlibliControllerJob>(x => x.GetToken(data, true, false));

                        string connId_JobId = dbPathEra + "_blibli_get_queue_feed_detail_" + Convert.ToString(tblCustomer.RecNum.Value);
                        recurJobM.AddOrUpdate(connId_JobId, Hangfire.Common.Job.FromExpression<BlibliControllerJob>(x => x.GetQueueFeedDetail(data, null)), Cron.MinuteInterval(recurr_interval), recurJobOpt);

                        if (sync_pesanan_stok == "1")
                        {
                            connId_JobId = dbPathEra + "_blibli_pesanan_paid_" + Convert.ToString(tblCustomer.RecNum.Value);
                            recurJobM.AddOrUpdate(connId_JobId, Hangfire.Common.Job.FromExpression<BlibliControllerJob>(x => x.GetOrderList(data, BlibliControllerJob.StatusOrder.Paid, connId_JobId, tblCustomer.CUST, tblCustomer.NAMA)), Cron.MinuteInterval(recurr_interval), recurJobOpt);

                            connId_JobId = dbPathEra + "_blibli_pesanan_complete_" + Convert.ToString(tblCustomer.RecNum.Value);
                            recurJobM.AddOrUpdate(connId_JobId, Hangfire.Common.Job.FromExpression<BlibliControllerJob>(x => x.GetOrderList(data, BlibliControllerJob.StatusOrder.Completed, connId_JobId, tblCustomer.CUST, tblCustomer.NAMA)), Cron.MinuteInterval(recurr_interval), recurJobOpt);
                        }
                        else
                        {
                            connId_JobId = dbPathEra + "_blibli_pesanan_paid_" + Convert.ToString(tblCustomer.RecNum.Value);
                            recurJobM.RemoveIfExists(connId_JobId);

                            connId_JobId = dbPathEra + "_blibli_pesanan_complete_" + Convert.ToString(tblCustomer.RecNum.Value);
                            recurJobM.RemoveIfExists(connId_JobId);
                        }
                    }
                }
            }
            #endregion

            #region elevenia
            var kdEL = MoDbContext.Marketplaces.Single(m => m.NamaMarket.ToUpper() == "ELEVENIA");
            var listELShop = LocalErasoftDbContext.ARF01.Where(m => m.NAMA == kdEL.IdMarket.ToString()).ToList();
            if (listELShop.Count > 0)
            {
                //var elApi = new EleveniaController();
                foreach (ARF01 tblCustomer in listELShop)
                {
                    //isi delivery temp
                    //change by calvin 4 april 2019
                    //elApi.GetDeliveryTemp(Convert.ToString(tblCustomer.RecNum), Convert.ToString(tblCustomer.API_KEY));
                    var parentid = client.Enqueue<EleveniaControllerJob>(x => x.GetDeliveryTemp(Convert.ToString(tblCustomer.RecNum), Convert.ToString(tblCustomer.API_KEY), dbPathEra, username));
                    //end change by calvin 4 april 2019

                    //add by calvin 2 april 2019
                    string connId_JobId = "";
                    if (sync_pesanan_stok == "1")
                    {
                        connId_JobId = dbPathEra + "_elevenia_pesanan_paid_" + Convert.ToString(tblCustomer.RecNum.Value);
                        recurJobM.AddOrUpdate(connId_JobId, Hangfire.Common.Job.FromExpression<EleveniaControllerJob>(x => x.GetOrder(tblCustomer.API_KEY, EleveniaControllerJob.StatusOrder.Paid, tblCustomer.CUST, tblCustomer.PERSO, dbPathEra, username)), Cron.MinuteInterval(recurr_interval), recurJobOpt);

                        connId_JobId = dbPathEra + "_elevenia_pesanan_completed_" + Convert.ToString(tblCustomer.RecNum.Value);
                        recurJobM.AddOrUpdate(connId_JobId, Hangfire.Common.Job.FromExpression<EleveniaControllerJob>(x => x.GetOrder(tblCustomer.API_KEY, EleveniaControllerJob.StatusOrder.Paid, tblCustomer.CUST, tblCustomer.PERSO, dbPathEra, username)), Cron.MinuteInterval(recurr_interval), recurJobOpt);

                        connId_JobId = dbPathEra + "_elevenia_pesanan_confirmpurchase_" + Convert.ToString(tblCustomer.RecNum.Value);
                        recurJobM.AddOrUpdate(connId_JobId, Hangfire.Common.Job.FromExpression<EleveniaControllerJob>(x => x.GetOrder(tblCustomer.API_KEY, EleveniaControllerJob.StatusOrder.Paid, tblCustomer.CUST, tblCustomer.PERSO, dbPathEra, username)), Cron.MinuteInterval(recurr_interval), recurJobOpt);
                    }
                    else
                    {
                        connId_JobId = dbPathEra + "_elevenia_pesanan_paid_" + Convert.ToString(tblCustomer.RecNum.Value);
                        recurJobM.RemoveIfExists(connId_JobId);

                        connId_JobId = dbPathEra + "_elevenia_pesanan_completed_" + Convert.ToString(tblCustomer.RecNum.Value);
                        recurJobM.RemoveIfExists(connId_JobId);

                        connId_JobId = dbPathEra + "_elevenia_pesanan_confirmpurchase_" + Convert.ToString(tblCustomer.RecNum.Value);
                        recurJobM.RemoveIfExists(connId_JobId);
                    }
                    //end add by calvin 2 april 2019
                }
            }
            #endregion

            #region Tokopedia
            var kdTokped = MoDbContext.Marketplaces.Single(m => m.NamaMarket.ToUpper() == "TOKOPEDIA");
            var lisTokpedShop = LocalErasoftDbContext.ARF01.Where(m => m.NAMA == kdTokped.IdMarket.ToString()).ToList();
            if (lisTokpedShop.Count > 0)
            {
                //var tokopediaApi = new TokopediaController();
                foreach (var tblCustomer in lisTokpedShop)
                {
                    if (tblCustomer.Sort1_Cust != "")
                    {
                        if (!string.IsNullOrEmpty(tblCustomer.API_CLIENT_P) && !string.IsNullOrEmpty(tblCustomer.API_CLIENT_U))
                        {
                            TokopediaControllerJob.TokopediaAPIData data = new TokopediaControllerJob.TokopediaAPIData
                            {
                                merchant_code = tblCustomer.Sort1_Cust, //FSID
                                API_client_password = tblCustomer.API_CLIENT_P, //Client Secret
                                API_client_username = tblCustomer.API_CLIENT_U, //Client ID
                                API_secret_key = tblCustomer.API_KEY, //Shop ID 
                                idmarket = tblCustomer.RecNum.Value,
                                DatabasePathErasoft = dbPathEra,
                                username = username
                            };
                            //tokopediaApi.GetToken(iden);
                            var parentid = client.Enqueue<TokopediaControllerJob>(x => x.GetToken(data));

                            //add by calvin 2 april 2019
                            string connId_JobId = "";
                            if (sync_pesanan_stok == "1")
                            {
                                connId_JobId = dbPathEra + "_tokopedia_pesanan_paid_" + Convert.ToString(tblCustomer.RecNum.Value);
                                recurJobM.AddOrUpdate(connId_JobId, Hangfire.Common.Job.FromExpression<TokopediaControllerJob>(x => x.GetOrderList(data, TokopediaControllerJob.StatusOrder.Paid, tblCustomer.CUST, tblCustomer.PERSO, 1, 0)), Cron.MinuteInterval(recurr_interval), recurJobOpt);
                            }
                            else
                            {
                                connId_JobId = dbPathEra + "_tokopedia_pesanan_paid_" + Convert.ToString(tblCustomer.RecNum.Value);
                                recurJobM.RemoveIfExists(connId_JobId);
                            }
                            //end add by calvin 2 april 2019
                            //new TokopediaControllerJob().GetToken(data);
                            //await new TokopediaControllerJob().GetOrderList(data, TokopediaControllerJob.StatusOrder.Paid, tblCustomer.CUST, tblCustomer.PERSO, 1, 0);
                        }
                    }
                }
            }
            #endregion

            #region Shopee
            //debug
            //ShopeeController.ShopeeAPIData dataaa = new ShopeeController.ShopeeAPIData()
            //{
            //    merchant_code = "6297330",

            //};
            //var ShopeeApi = new ShopeeController();
            //var resultShopee = ShopeeApi.GetItemDetail(dataaa, 470836261);
            var kdShopee = MoDbContext.Marketplaces.Single(m => m.NamaMarket.ToUpper() == "SHOPEE");
            var listShopeeShop = LocalErasoftDbContext.ARF01.Where(m => m.NAMA == kdShopee.IdMarket.ToString()).ToList();
            if (listShopeeShop.Count > 0)
            {
                //var shopeeApi = new ShopeeController();
                foreach (ARF01 tblCustomer in listShopeeShop)
                {
                    ShopeeControllerJob.ShopeeAPIData iden = new ShopeeControllerJob.ShopeeAPIData();
                    iden.merchant_code = tblCustomer.Sort1_Cust;
                    iden.DatabasePathErasoft = dbPathEra;
                    iden.username = username;

                    string connId_JobId = "";
                    if (sync_pesanan_stok == "1")
                    {
                        connId_JobId = dbPathEra + "_shopee_pesanan_rts_" + Convert.ToString(tblCustomer.RecNum.Value);
                        recurJobM.AddOrUpdate(connId_JobId, Hangfire.Common.Job.FromExpression<ShopeeControllerJob>(x => x.GetOrderByStatus(iden, ShopeeControllerJob.StatusOrder.READY_TO_SHIP, tblCustomer.CUST, tblCustomer.PERSO, 0, 0)), Cron.MinuteInterval(recurr_interval), recurJobOpt);

                        connId_JobId = dbPathEra + "_shopee_pesanan_complete_" + Convert.ToString(tblCustomer.RecNum.Value);
                        recurJobM.AddOrUpdate(connId_JobId, Hangfire.Common.Job.FromExpression<ShopeeControllerJob>(x => x.GetOrderByStatusCompleted(iden, ShopeeControllerJob.StatusOrder.COMPLETED, tblCustomer.CUST, tblCustomer.PERSO, 0, 0)), Cron.MinuteInterval(recurr_interval), recurJobOpt);

                        await new ShopeeControllerJob().GetOrderByStatusCompleted(iden, ShopeeControllerJob.StatusOrder.COMPLETED, tblCustomer.CUST, tblCustomer.PERSO, 0, 0);
                    }
                    else
                    {
                        connId_JobId = dbPathEra + "_shopee_pesanan_rts_" + Convert.ToString(tblCustomer.RecNum.Value);
                        recurJobM.RemoveIfExists(connId_JobId);

                        connId_JobId = dbPathEra + "_shopee_pesanan_complete_" + Convert.ToString(tblCustomer.RecNum.Value);
                        recurJobM.RemoveIfExists(connId_JobId);
                    }
                }
            }
            #endregion
            return "";
        }

        // Route ke halaman register
        [System.Web.Mvc.Route("register")]
        public ActionResult Register(string Ref)
        {
            var partnerInDb = MoDbContext.Partner.SingleOrDefault(p => p.KodeRefPilihan == Ref);

            if (Ref != null && partnerInDb == null)
            {
                return View("Error");
            }

            if (partnerInDb != null)
            {
                if (!partnerInDb.Status || !partnerInDb.StatusSetuju)
                {
                    return View("Error");
                }
            }

            return View();
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult CekEmailPengguna(string emailPengguna)
        {
            var res = new CekKetersediaanData()
            {
                Email = emailPengguna
            };

            var accInDb = MoDbContext.Account.SingleOrDefault(a => a.Email == emailPengguna);
            if (accInDb != null)
            {
                res.Available = false;
                res.CekNull = accInDb.Username;
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        // Proses saving data account
        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveAccount(Account account)
        {
            if (!ModelState.IsValid)
            {
                return View("Register", account);
            }

            if (account.Password != account.ConfirmPassword)
            {
                ModelState.AddModelError("", @"Password konfirmasi tidak sama");
                return View("Register", account);
            }

            var accInDb = MoDbContext.Account.SingleOrDefault(a => a.Email == account.Email);
            var userInDb = MoDbContext.User.SingleOrDefault(a => a.Email == account.Email);

            if (accInDb != null || userInDb != null)
            {
                ModelState.AddModelError("", @"Email sudah terdaftar!");
                return View("Register", account);
            }

            var keyNew = Helper.GeneratePassword(10);
            var originPassword = account.Password;
            var password = Helper.EncodePassword(account.Password, keyNew);

            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {
                    //var fileName = Path.GetFileName(file.FileName);
                    var fileName = account.Email.Replace(".", "_");
                    var path = Path.Combine(Server.MapPath("~/Content/Uploaded/"), fileName);
                    account.PhotoKtpUrl = "~/Content/Uploaded/" + fileName;
                    file.SaveAs(path);
                }
            }

            if (String.IsNullOrWhiteSpace(account.PhotoKtpUrl))
            {
                ModelState.AddModelError("", @"Harap sertakan foto / scan KTP Anda!");
                return View("Register", account);
            }

            var email = new MailAddress(account.Email);
            var nama = account.Username;
            account.UserId = email.User + "_" + email.Host.Replace(".", "_");
            account.Status = false; //User tidak aktif untuk pertama kali
            //change back to set user to free user
            string userSubs = "01";
            if (!string.IsNullOrEmpty(account.KODE_SUBSCRIPTION))
                userSubs = account.KODE_SUBSCRIPTION;
            //change by Tri, 7 Feb 2019 handle user pilih subscription sebelum register
            account.KODE_SUBSCRIPTION = "01"; //Free account
            account.TGL_SUBSCRIPTION = DateTime.Today.Date; //First time subs
            //if (string.IsNullOrEmpty(account.KODE_SUBSCRIPTION))
            //{
            //    account.KODE_SUBSCRIPTION = "01";
            //    account.TGL_SUBSCRIPTION = DateTime.Today.Date;
            //}
            //else
            //{
            //    account.TGL_SUBSCRIPTION = DateTime.Today.Date.AddDays(-1); //user buy subscription while register, set subscription to expire
            //}
            //end change by Tri, 7 Feb 2019 handle user pilih subscription sebeelum register
            //end change back to set user to free user
            account.Password = password;
            account.ConfirmPassword = password;
            account.VCode = keyNew;
            //add by Tri 13 Feb 2019, tambah tanggal daftar
            account.TGL_DAFTAR = DateTime.Now;
            //end add by Tri 13 Feb 2019, tambah tanggal daftar
            //add by Tri 4 Mar 2019, tambah jumlah user
            if (userSubs == "02")
            {
                account.jumlahUser = 2;
            }
            else if (userSubs == "01")
            {
                account.jumlahUser = 0;
            }
            //end add by Tri 4 Mar 2019, tambah jumlah user
            MoDbContext.Account.Add(account);
            MoDbContext.SaveChanges();
            ModelState.Clear();

            //remark by calvin 2 oktober 2018, untuk testing dlu
            //change by nurul 5/3/2019
            //var body = "<p>Selamat, akun Anda berhasil didaftarkan pada sistem kami&nbsp;<img src=\"https://html-online.com/editor/tinymce4_6_5/plugins/emoticons/img/smiley-laughing.gif\" alt=\"laughing\" /></p>" +
            //    "<p>&nbsp;</p>" +
            //    "<p>Detail akun Anda ialah sebagai berikut,</p>" +
            //    "<p>Email: {0}</p>" +
            //    "<p>Password: {1}</p>" +
            //    "<p>Semoga sukses selalu dalam bisnis Anda di MasterOnline.</p><p>&nbsp;</p>" +
            //    "<p>Best regards,</p>" +
            //    "<p>CS MasterOnline.</p>";
            //var body = "<p><img src=\"https://s3-ap-southeast-1.amazonaws.com//masteronlinebucket/uploaded-image/ee23b210-cb3b-4796-9ad1-9ddf936a8e26.jpg\"  width=\"200\" height=\"150\"></p>" +

            //remark by calvin 16 april 2019, pindah ke hangfire untuk kirim email nya
            //            var body = "<p><img src=\"https://s3-ap-southeast-1.amazonaws.com//masteronlinebucket/uploaded-image/efd0f5b3-7862-4ee6-b796-6c5fc9c63d5f.jpeg\"  width=\"250\" height=\"100\"></p>" +
            //                "<p>Hi {2},</p>" +
            //                "<p>Selamat bergabung di Master Online.</p>" +
            //                "<p>Master Online adalah Software Omnichannel management dimana anda dapat mengontrol dan mengelola bisnis anda di semua marketplace Indonesia dari 1 platfrom.</p>" +
            //                "<p>Tunggu aktivasi akun anda dalam 1-2 hari ke depan.</p>" +
            //                "<p>Cek Email anda dan Stay Tuned !&nbsp;<img src=\"https://html-online.com/editor/tinymce4_6_5/plugins/emoticons/img/smiley-laughing.gif\" alt=\"laughing\" /></p>" +
            //                "<p>&nbsp;</p>" +
            //                "<p>Best regards,</p>" +
            //                "<p>CS Master Online.</p>";
            //            //end change by nurul 5/3/2019

            //            var message = new MailMessage();
            //            message.To.Add(email);
            //            message.From = new MailAddress("csmasteronline@gmail.com");
            //            message.Subject = "Pendaftaran Master Online berhasil!";
            //            message.Body = string.Format(body, account.Email, originPassword, nama);
            //            message.IsBodyHtml = true;
#if AWS
            //            //using (var smtp = new SmtpClient())
            //            //{
            //            //    var credential = new NetworkCredential
            //            //    {
            //            //        UserName = "AKIAIXN2D33JPSDL7WEQ",
            //            //        Password = "ApBddkFZF8hwJtbo+s4Oq31MqDtWOpzYKDhyVGSHGCEl"
            //            //    };
            //            //    smtp.Credentials = credential;
            //            //    smtp.Host = "email-smtp.us-east-1.amazonaws.com";
            //            //    smtp.Port = 587;
            //            //    smtp.EnableSsl = true;
            //            //    await smtp.SendMailAsync(message);
            //            //}
            //            using (var smtp = new SmtpClient())
            //            {
            //                var credential = new NetworkCredential
            //                {
            //                    UserName = "csmasteronline@gmail.com",
            //                    Password = "erasoft123"
            //                };
            //                smtp.Credentials = credential;
            //                smtp.Host = "smtp.gmail.com";
            //                smtp.Port = 587;
            //                smtp.EnableSsl = true;
            //                await smtp.SendMailAsync(message);
            //            }
#else
            //            using (var smtp = new SmtpClient())
            //            {
            //                var credential = new NetworkCredential
            //                {
            //                    UserName = "csmasteronline@gmail.com",
            //                    Password = "erasoft123"
            //                };
            //                smtp.Credentials = credential;
            //                smtp.Host = "smtp.gmail.com";
            //                smtp.Port = 587;
            //                smtp.EnableSsl = true;
            //                await smtp.SendMailAsync(message);
            //            }
            //end remark by calvin 16 april 2019, pindah ke hangfire untuk kirim email nya
#endif

            //var sqlStorage = new SqlServerStorage(System.Configuration.ConfigurationManager.ConnectionStrings["MoDbContext"].ConnectionString);
            //var clientKirimEmail = new BackgroundJobClient(sqlStorage);
            //var monitoringApi = sqlStorage.GetMonitoringApi();
            //var serverList = monitoringApi.Servers();
#if Debug_AWS
            //if (serverList.Count() > 0)
            //{
            //    bool lakukanHapusServer = false;
            //    if (lakukanHapusServer)
            //    {
            //        foreach (var server in serverList)
            //        {
            //            var serverConnection = sqlStorage.GetConnection();
            //            serverConnection.RemoveServer(server.Name);
            //            serverConnection.Dispose();
            //        }

            //        var options = new BackgroundJobServerOptions
            //        {
            //            ServerName = "Admin_Email",
            //            Queues = new[] { "1_critical", "2_general" },
            //            WorkerCount = 1,
            //        };
            //        var newserver = new BackgroundJobServer(options, sqlStorage);
            //    }
            //}
#else
#endif
            //if (serverList.Count() == 0)
            //{
            //    var options = new BackgroundJobServerOptions
            //    {
            //        ServerName = "Admin_Email",
            //        Queues = new[] { "1_critical", "2_general" },
            //        WorkerCount = 1,
            //    };
            //    var newserver = new BackgroundJobServer(options, sqlStorage);
            //}

            //clientKirimEmail.Enqueue(() => TesSendEmail(email, account.Email, originPassword, nama));
            Task.Run(() => TesSendEmail(email, account.Email, originPassword, nama));

            //ViewData["SuccessMessage"] = $"Selamat, akun Anda berhasil didaftarkan! Klik <a href=\"{Url.Action("Login")}\">di sini</a> untuk login!";
            ViewData["SuccessMessage"] = $"Kami telah menerima pendaftaran Anda. Silakan menunggu <i>approval</i> melalui email dari admin kami, terima kasih.";

            //if (account.KODE_SUBSCRIPTION != "01")
            //{
            //    var ret = ChangeStatusAcc(Convert.ToInt32(account.AccountId));
            //    if (ret.status == 1)
            //    {
            //        var midtrans = new MidtransController();
            //        return await midtrans.PaymentMidtrans(account.KODE_SUBSCRIPTION, account.DatabasePathMo, Convert.ToInt32(account.AccountId));
            //    }
            //    else
            //    {
            //        var errorRet = new bindMidtrans();
            //        errorRet.error = ret.message;
            //        return Json(errorRet, JsonRequestBehavior.AllowGet);
            //    }
            //}
            if (userSubs != "01")
            {
                var midtrans = new MidtransController();
                return await midtrans.PaymentMidtrans(userSubs, account.DatabasePathMo, Convert.ToInt32(account.AccountId), account.jumlahUser);
            }

            ViewData["SuccessMessage"] = $"Kami telah menerima pendaftaran Anda. Silakan menunggu <i>approval</i> melalui email dari admin kami, terima kasih.";
            return View("Register");

        }

        //[AutomaticRetry(Attempts = 2)]
        //[Queue("2_general")]
        protected async Task<string> TesSendEmail(MailAddress email, string account_Email, string originPassword, string nama)
        {
            var body = "<p><img src=\"https://s3-ap-southeast-1.amazonaws.com//masteronlinebucket/uploaded-image/efd0f5b3-7862-4ee6-b796-6c5fc9c63d5f.jpeg\"  width=\"250\" height=\"100\"></p>" +
               "<p>Hi {2},</p>" +
               "<p>Selamat bergabung di Master Online.</p>" +
               "<p>Master Online adalah Software Omnichannel management dimana anda dapat mengontrol dan mengelola bisnis anda di semua marketplace Indonesia dari 1 platfrom.</p>" +
               "<p>Tunggu aktivasi akun anda dalam 1-2 hari ke depan.</p>" +
               "<p>Cek Email anda dan Stay Tuned !&nbsp;<img src=\"https://html-online.com/editor/tinymce4_6_5/plugins/emoticons/img/smiley-laughing.gif\" alt=\"laughing\" /></p>" +
               "<p>&nbsp;</p>" +
               "<p>Best regards,</p>" +
               "<p>CS Master Online.</p>";
            //end change by nurul 5/3/2019

            var message = new MailMessage();
            message.To.Add(email);
            message.From = new MailAddress("csmasteronline@gmail.com");
            message.Subject = "Pendaftaran Master Online berhasil!";
            message.Body = string.Format(body, account_Email, originPassword, nama);
            message.IsBodyHtml = true;

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
            return "";
        }
        //function activate account
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

                //ViewData["SuccessMessage"] = $"Akun {accInDb.Username} berhasil diubah statusnya dan dibuatkan database baru.";
                MoDbContext.SaveChanges();

                //var listAcc = MoDbContext.Account.ToList();

                //return View("AccountMenu", listAcc);
                ret.status = 1;
            }
            catch (Exception ex)
            {
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }

            return ret;
        }

        // Proses Logging Out + Hapus Session
        public ActionResult LoggingOut()
        {
            var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            string dbPathEra = "";
            if (sessionData?.Account != null)
            {
                dbPathEra = sessionData.Account.DatabasePathErasoft;
            }
            else
            {
                var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                dbPathEra = accFromUser.DatabasePathErasoft;
            }
            if (dbPathEra != "")
            {
                var EDB = new DatabaseSQL(dbPathEra);

                string EDBConnID = EDB.GetConnectionString("ConnID");
                var sqlStorage = new SqlServerStorage(EDBConnID);
                //CHANGE by calvin 15 april 2019
                //var monitoringApi = sqlStorage.GetMonitoringApi();
                //var serverList = monitoringApi.Servers();
                //if (serverList.Count() > 0)
                //{
                //    foreach (var server in serverList)
                //    {
                //        var serverConnection = sqlStorage.GetConnection();
                //        serverConnection.RemoveServer(server.Name);
                //        serverConnection.Dispose();
                //    }
                //}

                RecurringJobManager recurJobM = new RecurringJobManager(sqlStorage);
                RecurringJobOptions recurJobOpt = new RecurringJobOptions()
                {
                    QueueName = "3_general"
                };

                using (var connection = sqlStorage.GetConnection())
                {
                    foreach (var recurringJob in connection.GetRecurringJobs())
                    {
                        recurJobM.AddOrUpdate(recurringJob.Id, recurringJob.Job, Cron.MinuteInterval(30), recurJobOpt);
                    }
                }
                //end CHANGE by calvin 15 april 2019
            }

            Session["SessionInfo"] = null;
            //add by Tri, clear session id from cookies
            Session.Abandon();
            Response.Cookies.Add(new System.Web.HttpCookie("ASP.NET_SessionId", ""));
            //end add by Tri, clear session id from cookies
            //IdentitySignout();//add by calvin 1 april 2019
            return RedirectToAction("Index", "Home");
        }

        // Route ke halaman lupa password
        [System.Web.Mvc.Route("remind")]
        public ActionResult Remind()
        {
            return View();
        }

        [System.Web.Mvc.Route("partner")]
        public ActionResult Partner(string Ref)
        {
            var partnerInDb = MoDbContext.Partner.SingleOrDefault(p => p.KodeRefPilihan == Ref);

            if (Ref != null && partnerInDb == null)
            {
                return View("Error");
            }

            if (partnerInDb != null)
            {
                if (!partnerInDb.Status || !partnerInDb.StatusSetuju)
                {
                    return View("Error");
                }
            }

            return View();
        }

        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SavePartner(Partner partner)
        {
            if (!ModelState.IsValid)
            {
                return View("Partner", partner);
            }

            var partnerInDb = MoDbContext.Partner.SingleOrDefault(a => a.Email == partner.Email);
            var cekKodeRefPilihan = MoDbContext.Partner.SingleOrDefault(a => a.KodeRefPilihan.ToUpper() == partner.KodeRefPilihan.ToUpper());

            if (partnerInDb != null)
            {
                ModelState.AddModelError("", @"Email sudah terdaftar!");
                return View("Partner", partner);
            }

            if (cekKodeRefPilihan != null)
            {
                ModelState.AddModelError("", @"Kode Referal sudah terdaftar!");
                return View("Partner", partner);
            }

            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {
                    //var fileName = Path.GetFileName(file.FileName);
                    var fileName = partner.Email.Replace(".", "_");
                    var path = Path.Combine(Server.MapPath("~/Content/Uploaded/"), fileName);
                    partner.PhotoKtpUrl = "~/Content/Uploaded/" + fileName;
                    file.SaveAs(path);
                }
            }

            if (partner.TipePartner == 1 && String.IsNullOrWhiteSpace(partner.PhotoKtpUrl))
            {
                ModelState.AddModelError("", @"Harap sertakan foto / scan KTP Anda!");
                return View("Partner", partner);
            }

            partner.Status = false; //Partner tidak aktif untuk pertama kali
            partner.StatusSetuju = false; //Partner tidak setuju untuk pertama kali

            //add by nurul 15/2/2019
            partner.komisi_subscribe = 0;
            partner.komisi_support = 0;
            partner.komisi_subscribe_gold = 0;
            //end add by nurul 15/2/2019
            partner.TGL_DAFTAR = DateTime.Now;//add 18 Feb 2019, tambah tgl daftar partner

            MoDbContext.Partner.Add(partner);
            MoDbContext.SaveChanges();
            ModelState.Clear();

            ViewData["SuccessMessage"] = $"Terima kasih, pengajuan Partner Anda akan segera kami proses. Silakan tunggu email konfirmasi.";

            return View("Partner");
        }

        [System.Web.Mvc.Route("partner/approval")]
        public async Task<ActionResult> PartnerApproval(long? partnerId)
        {
            var partnerInDb = MoDbContext.Partner.SingleOrDefault(u => u.PartnerId == partnerId);
            if (partnerInDb == null) return View("Error");

            var approvalData = new PartnerApprovalViewModel();
            approvalData.KodeReferalPilihan = partnerInDb.KodeRefPilihan;
            approvalData.NamaTipe = partnerInDb.NamaTipe;

            if (partnerInDb.StatusSetuju)
            {
                approvalData.SudahDaftar = true;
                return View(approvalData);
            }

            partnerInDb.StatusSetuju = true;

            MoDbContext.SaveChanges();

            if (partnerInDb.Status)
            {
                var email = new MailAddress(partnerInDb.Email);
                var message = new MailMessage();
                message.To.Add(email);
                message.From = new MailAddress("csmasteronline@gmail.com");
                message.Subject = "SELAMAT! Anda telah menjadi partner dari MasterOnline!";
                message.Body = System.IO.File.ReadAllText(Server.MapPath("~/Content/admin/PartnerApproval.html"))
                    .Replace("LINKREF", Request.Url.GetLeftPart(UriPartial.Authority) + Url.Action("Index", "Home", new { @ref = partnerInDb.KodeRefPilihan }))
                    .Replace("TIPEPARTNER", partnerInDb.NamaTipe);
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

            return View(approvalData);
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult CekEmailPartner(string emailPengguna)
        {
            var res = new CekKetersediaanData()
            {
                Email = emailPengguna
            };

            var partnerInDb = MoDbContext.Partner.SingleOrDefault(a => a.Email == emailPengguna);
            if (partnerInDb != null)
            {
                res.Available = false;
                res.CekNull = partnerInDb.Username;
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult CekHpPartner(string noHp)
        {
            var res = new CekKetersediaanData()
            {
                MobileNo = noHp
            };

            var partnerInDb = MoDbContext.Partner.SingleOrDefault(a => a.NoHp == noHp);
            if (partnerInDb != null)
            {
                res.Available = false;
                res.CekNull = partnerInDb.Username;
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult CekKodeRefPartner(string kodeRef)
        {
            var res = new CekKetersediaanData()
            {
                KodeRef = kodeRef
            };

            var partnerInDb = MoDbContext.Partner.SingleOrDefault(a => a.KodeRefPilihan == kodeRef);
            if (partnerInDb != null)
            {
                res.Available = false;
                res.CekNull = partnerInDb.Username;
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Mvc.Route("HomePricing")]
        public ActionResult HomePricing(string Ref)
        {
            var partnerInDb = MoDbContext.Partner.SingleOrDefault(p => p.KodeRefPilihan == Ref);

            if (Ref != null && partnerInDb == null)
            {
                return View("Error");
            }

            if (partnerInDb != null)
            {
                if (!partnerInDb.Status || !partnerInDb.StatusSetuju)
                {
                    return View("Error");
                }
            }

            var vm = new SubsViewModel()
            {
                ListSubs = MoDbContext.Subscription.ToList(),
                loggedin = true
            };

            return View(vm);
        }
    }
}