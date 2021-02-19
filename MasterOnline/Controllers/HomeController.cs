using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MasterOnline.ViewModels;

namespace MasterOnline.Controllers
{
    public class HomeController : Controller
    {
        public MoDbContext MoDbContext { get; set; }
        public ErasoftContext ErasoftDbContext { get; set; }
        string dbSourceEra = "";

        public HomeController()
        {
            MoDbContext = new MoDbContext("");
            //var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
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
            //                    ErasoftDbContext = new ErasoftContext(dbSourceEra, accFromUser.DatabasePathErasoft);
            //                }
            //            }

            var sessionAccount = System.Web.HttpContext.Current.Session["SessionAccount"];
            var sessionAccountUserID = System.Web.HttpContext.Current.Session["SessionAccountUserID"];
            var sessionAccountDataSourcePathDebug = System.Web.HttpContext.Current.Session["SessionAccountDataSourcePathDebug"];
            var sessionAccountDataSourcePath = System.Web.HttpContext.Current.Session["SessionAccountDataSourcePath"];
            var sessionAccountDatabasePathErasoft = System.Web.HttpContext.Current.Session["SessionAccountDatabasePathErasoft"];

            var sessionUser = System.Web.HttpContext.Current.Session["SessionUser"];
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
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == Convert.ToInt64(sessionUserAccountID));
#if (Debug_AWS)
                    dbSourceEra = accFromUser.DataSourcePathDebug;
#else
                    dbSourceEra = accFromUser.DataSourcePath;
#endif
                    ErasoftDbContext = new ErasoftContext(dbSourceEra, accFromUser.DatabasePathErasoft);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            MoDbContext.Dispose();
            ErasoftDbContext?.Dispose();
        }

        public ActionResult Index(string Ref)
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

        [Route("home/testing")]
        public ActionResult testing()
        {
            var vm = new SubsViewModel()
            {
                ListSubs = MoDbContext.Subscription.ToList()
            };
            return View(vm);
        }

        public ActionResult About(string Ref)
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
        public ActionResult FAQ(string Ref)
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

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult Promo()
        {
            return View();
        }

        public ActionResult SavePromo(PromoViewModel vm)
        {
            foreach (var promo in vm.ListPromo)
            {
                promo.EMAIL_REF = vm.Promo.EMAIL_REF;
                promo.HP_REF = vm.Promo.HP_REF;
                promo.TGL = DateTime.Today.Date;
                promo.PAID = false;
                MoDbContext.Promo.Add(promo);
            }

            MoDbContext.SaveChanges();
            ModelState.Clear();

            return RedirectToAction("Promo");
        }
    }
}