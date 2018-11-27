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
        public MoDbContext MoDbContext { get; set; }
        public ErasoftContext ErasoftDbContext { get; set; }
        private AccountUserViewModel _viewModel;

        public ScrapperController()
        {
            MoDbContext = new MoDbContext();
            _viewModel = new AccountUserViewModel();

            var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            if (sessionData?.Account != null)
            {
                if (sessionData.Account.UserId == "admin_manage")
                    ErasoftDbContext = new ErasoftContext();
                else
                    ErasoftDbContext = new ErasoftContext(sessionData.Account.DatabasePathErasoft);

            }
            else
            {
                if (sessionData?.User != null)
                {
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                    ErasoftDbContext = new ErasoftContext(accFromUser.DatabasePathErasoft);
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
                var pass = accFromDb.Password;
                var hashCode = accFromDb.VCode;
                var encodingPassString = Helper.EncodePassword(account.Password, hashCode);

                if (!encodingPassString.Equals(pass))
                {
                    ModelState.AddModelError(string.Empty, @"Password salah!");
                    return View("LoginScrapper", account);
                }

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
                ErasoftDbContext = _viewModel.Account.UserId == "admin_manage" ? new ErasoftContext() : new ErasoftContext(_viewModel.Account.DatabasePathErasoft);
            }
            else
            {
                var accFromUser = MoDbContext.Account.Single(a => a.AccountId == _viewModel.User.AccountId);
                ErasoftDbContext = new ErasoftContext(accFromUser.DatabasePathErasoft);
            }

            return RedirectToAction("DataBarang");
        }

        // GET: Scrapper
        [Route("scrapper/databarang")]
        public ActionResult DataBarang()
        {
            if (ErasoftDbContext == null)
            {
                return Content("Null");
            }

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
        public void RunCmd()
        {
            System.Diagnostics.Process si = new System.Diagnostics.Process();
            //si.StartInfo.WorkingDirectory = "c:\\";
            si.StartInfo.UseShellExecute = false;
            si.StartInfo.FileName = Server.MapPath("~/Services/Batch/test.bat");
            //si.StartInfo.Arguments = "/c cd Users dir";
            si.StartInfo.CreateNoWindow = true;
            si.StartInfo.RedirectStandardInput = true;
            si.StartInfo.RedirectStandardOutput = true;
            si.StartInfo.RedirectStandardError = true;
            si.Start();
            string output = si.StandardOutput.ReadToEnd();
            si.Close();
            Response.Write(output);
        }
    }
}