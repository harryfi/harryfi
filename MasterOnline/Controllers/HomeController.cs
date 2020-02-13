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

        public HomeController()
        {
            MoDbContext = new MoDbContext("");
            var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            if (sessionData?.Account != null)
            {
                if (sessionData.Account.UserId == "admin_manage")
                    ErasoftDbContext = new ErasoftContext();
                else
                    ErasoftDbContext = new ErasoftContext(sessionData.Account.DataSourcePath, sessionData.Account.DatabasePathErasoft);
            }
            else
            {
                if (sessionData?.User != null)
                {
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                    ErasoftDbContext = new ErasoftContext(accFromUser.DataSourcePath , accFromUser.DatabasePathErasoft);
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