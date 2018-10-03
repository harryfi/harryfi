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
            MoDbContext = new MoDbContext();
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

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
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