using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MasterOnline.Models;
using MasterOnline.Utils;
using MasterOnline.ViewModels;

namespace MasterOnline.Controllers
{
    public class ScrapperController : Controller
    {
        //set parameter network location server IP Private
        public string IPServerLocation = @"\\172.31.20.73\MasterOnline\";
        //public string IPServerLocation = "\\\\127.0.0.1\\MasterOnline\\"; // \\127.0.0.1\MasterOnline

        public MoDbContext MoDbContext { get; set; }
        public ErasoftContext ErasoftDbContext { get; set; }
        private AccountUserViewModel _viewModel;
        string dbSourceEra = "";

        public ScrapperController()
        {
            MoDbContext = new MoDbContext("");
            _viewModel = new AccountUserViewModel();

            //            var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            //            if (sessionData?.Account != null)
            //            {
            //                if (sessionData.Account.UserId == "admin_manage")
            //                {
            //                    ErasoftDbContext = new ErasoftContext();
            //                }
            //                else
            //                {
            //#if (Debug_AWS)
            //                    dbSourceEra = sessionData.Account.DataSourcePathDebug;
            //#else
            //                    dbSourceEra = sessionData.Account.DataSourcePath;
            //#endif
            //                    ErasoftDbContext = new ErasoftContext(dbSourceEra, sessionData.Account.DatabasePathErasoft);
            //                }
            //            }
            //            else
            //            {
            //                if (sessionData?.User != null)
            //                {
            //                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
            //#if (Debug_AWS)
            //                    dbSourceEra = accFromUser.DataSourcePathDebug;
            //#else
            //                    dbSourceEra = accFromUser.DataSourcePath;
            //#endif
            //                    //ErasoftDbContext = new ErasoftContext(accFromUser.DataSourcePath, accFromUser.DatabasePathErasoft);
            //                    ErasoftDbContext = new ErasoftContext(dbSourceEra, accFromUser.DatabasePathErasoft);
            //                }
            //            }

            var sessionAccount = System.Web.HttpContext.Current.Session["SessionAccount"];
            var sessionAccountUserID = System.Web.HttpContext.Current.Session["SessionAccountUserID"];
            var sessionAccountUserName = System.Web.HttpContext.Current.Session["SessionAccountUserName"];
            var sessionAccountEmail = System.Web.HttpContext.Current.Session["SessionAccountEmail"];
            var sessionAccountTglSub = System.Web.HttpContext.Current.Session["SessionAccountTglSub"];
            var sessionAccountKodeSub = System.Web.HttpContext.Current.Session["SessionAccountKodeSub"];
            var sessionAccountDataSourcePathDebug = System.Web.HttpContext.Current.Session["SessionAccountDataSourcePathDebug"];
            var sessionAccountDataSourcePath = System.Web.HttpContext.Current.Session["SessionAccountDataSourcePath"];
            var sessionAccountDatabasePathErasoft = System.Web.HttpContext.Current.Session["SessionAccountDatabasePathErasoft"];

            var sessionUser = System.Web.HttpContext.Current.Session["SessionUser"];
            var sessionUserUserID = System.Web.HttpContext.Current.Session["SessionUserUserID"];
            var sessionUserUsername = System.Web.HttpContext.Current.Session["SessionUserUsername"];
            var sessionUserEmail = System.Web.HttpContext.Current.Session["SessionUserEmail"];
            var sessionUserAccountID = System.Web.HttpContext.Current.Session["SessionUserAccountID"];

            if (sessionAccount != null)
            {
                if (sessionAccountUserID.ToString() == "admin_manage")
                {
                    ErasoftDbContext = new ErasoftContext();
                }
                else
                {
#if (Debug_AWS)
                    dbSourceEra = sessionAccountDataSourcePathDebug.ToString();
#else
                    dbSourceEra = sessionAccountDataSourcePath.ToString();
#endif
                    ErasoftDbContext = new ErasoftContext(dbSourceEra, sessionAccountDatabasePathErasoft.ToString());
                }

            }
            else
            {
                if (sessionUser != null)
                {
                    var userAccID = Convert.ToInt64(sessionUserAccountID);
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == userAccID);
#if (Debug_AWS)
                    dbSourceEra = accFromUser.DataSourcePathDebug;
#else
                    dbSourceEra = accFromUser.DataSourcePath;
#endif
                    //ErasoftDbContext = new ErasoftContext(accFromUser.DataSourcePath, accFromUser.DatabasePathErasoft);
                    ErasoftDbContext = new ErasoftContext(dbSourceEra, accFromUser.DatabasePathErasoft);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            MoDbContext.Dispose();
            ErasoftDbContext?.Dispose();
        }

        [Route("scrapper/login")]
        public ActionResult LoginScrapper()
        {
            return View();
        }

        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        [Route("scrapper/loggingin")]
        public ActionResult LoggingIn(Account account)
        {
            ModelState.Remove("UserId");
            ModelState.Remove("Username");
            ModelState.Remove("NoHp");
            ModelState.Remove("NamaTokoOnline");

            if (!ModelState.IsValid)
                return View("LoginScrapper", account);

            var accFromDb = MoDbContext.Account.SingleOrDefault(a => a.Email == account.Email);

            if (accFromDb == null)
            {
                var userFromDb = MoDbContext.User.SingleOrDefault(a => a.Email == account.Email);

                if (userFromDb == null)
                {
                    ModelState.AddModelError(string.Empty, @"Username / Email tidak ditemukan!");
                    return View("LoginScrapper", account);
                }

                var accInDb = MoDbContext.Account.Single(ac => ac.AccountId == userFromDb.AccountId);
                var key = accInDb.VCode;
                var originPassword = account.Password;
                var encodedPassword = Helper.EncodePassword(originPassword, key);
                var pass = userFromDb.Password;

                if (!encodedPassword.Equals(pass))
                {
                    ModelState.AddModelError(string.Empty, @"Password salah!");
                    return View("LoginScrapper", account);
                }

                if (!userFromDb.Status)
                {
                    ModelState.AddModelError(string.Empty, @"Akun tidak aktif!");
                    return View("LoginScrapper", account);
                }

                _viewModel.User = userFromDb;
            }
            else
            {
                if (!accFromDb.Status)
                {
                    ModelState.AddModelError(string.Empty, @"Akun tidak aktif!");
                    return View("LoginScrapper", account);
                }

                _viewModel.Account = accFromDb;
            }

            Session["SessionInfo"] = _viewModel;

            if (_viewModel?.Account != null)
            {
#if (Debug_AWS)
                    dbSourceEra = _viewModel.Account.DataSourcePathDebug;
#else
                    dbSourceEra = _viewModel.Account.DataSourcePath;
#endif
                ErasoftDbContext = _viewModel.Account.UserId == "admin_manage" ? new ErasoftContext() : new ErasoftContext(dbSourceEra,  _viewModel.Account.DatabasePathErasoft);
            }
            else
            {
                var accFromUser = MoDbContext.Account.Single(a => a.AccountId == _viewModel.User.AccountId);
#if (Debug_AWS)
                    dbSourceEra = accFromUser.DataSourcePathDebug;
#else
                    dbSourceEra = accFromUser.DataSourcePath;
#endif
                ErasoftDbContext = new ErasoftContext(dbSourceEra, accFromUser.DatabasePathErasoft);
            }

            return RedirectToAction("DataBarang");
        }

        // GET: Scrapper
        [Route("scrapper/databarang")]
        public ActionResult DataBarang()
        {
            var barangVm = new BarangViewModel()
            {
                ListStf02S = ErasoftDbContext.STF02.ToList(),
                ListMarket = ErasoftDbContext.ARF01.OrderBy(p => p.RecNum).ToList(),
                ListHargaJualPermarketView = ErasoftDbContext.STF02H.OrderBy(p => p.IDMARKET).ToList(),
                //ListCategoryBlibli = MoDbContext.CategoryBlibli.Where(p => string.IsNullOrEmpty(p.PARENT_CODE)).ToList(),
                DataUsaha = ErasoftDbContext.SIFSYS.Single(p => p.BLN == 1)
            };

            return View(barangVm);
        }

        [Route("scrapper/opencmd")]
        public void RunCmd(string moe, string mop, string moai)
        {
            Process si = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = IPServerLocation + @"Services\thzalyvuspulzjyhwwlymvskly\",
                    UseShellExecute = false,
                    FileName = IPServerLocation + @"Services\thzalyvuspulzjyhwwlymvskly\thzalyvuspulzjyhwwlyiha.bat",
                    Arguments = $"{moe} {mop} {$"databarang_{moai}.csv"}",
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
            };

            si.OutputDataReceived += p_OutputDataReceived;
            si.ErrorDataReceived += p_ErrorDataReceived;
            si.Start();
            si.BeginOutputReadLine();
            si.BeginErrorReadLine();
            si.WaitForExit();
        }

        void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Response.Write("Received from standard out: " + e.Data);
        }

        void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Response.Write("Received from standard error: " + e.Data);
        }
    }
}