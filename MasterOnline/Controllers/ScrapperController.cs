using System;
using System.Collections.Generic;
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
        private static MoDbContext _moDbContext;
        private static ErasoftContext _erasoftDbContext;
        private static AccountUserViewModel _viewModel;

        public ScrapperController()
        {
            _moDbContext = new MoDbContext();
            _viewModel = new AccountUserViewModel();
        }

        protected override void Dispose(bool disposing)
        {
            _moDbContext.Dispose();
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

            var accFromDb = _moDbContext.Account.SingleOrDefault(a => a.Email == account.Email);

            if (accFromDb == null)
            {
                var userFromDb = _moDbContext.User.SingleOrDefault(a => a.Email == account.Email);

                if (userFromDb == null)
                {
                    ModelState.AddModelError(string.Empty, @"Username / Email tidak ditemukan!");
                    return View("LoginScrapper", account);
                }

                var accInDb = _moDbContext.Account.Single(ac => ac.AccountId == userFromDb.AccountId);
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

            if (_viewModel?.Account != null)
            {
                _erasoftDbContext = _viewModel.Account.UserId == "admin_manage" ? new ErasoftContext() : new ErasoftContext(_viewModel.Account.DatabasePathErasoft);
            }
            else
            {
                var accFromUser = _moDbContext.Account.Single(a => a.AccountId == _viewModel.User.AccountId);
                _erasoftDbContext = new ErasoftContext(accFromUser.DatabasePathErasoft);
            }

            return RedirectToAction("DataBarang");
        }

        // GET: Scrapper
        [Route("scrapper/databarang")]
        public ActionResult DataBarang()
        {
            if (_erasoftDbContext == null)
            {
                return Content("Null");
            }

            var barangVm = new BarangViewModel()
            {
                ListStf02S = _erasoftDbContext.STF02.ToList(),
                ListMarket = _erasoftDbContext.ARF01.OrderBy(p => p.RecNum).ToList(),
                ListHargaJualPermarketView = _erasoftDbContext.STF02H.OrderBy(p => p.IDMARKET).ToList(),
                //ListCategoryBlibli = MoDbContext.CategoryBlibli.Where(p => string.IsNullOrEmpty(p.PARENT_CODE)).ToList(),
                DataUsaha = _erasoftDbContext.SIFSYS.Single(p => p.BLN == 1)
            };

            return View(barangVm);
        }
    }
}