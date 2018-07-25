using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using MasterOnline.Models;
using MasterOnline.Services;
using MasterOnline.ViewModels;

namespace MasterOnline.Controllers
{
    [SessionCheck]
    public class PostingController : Controller
    {
        public MoDbContext MoDbContext { get; set; }
        public ErasoftContext ErasoftDbContext { get; set; }

        public PostingController()
        {
            MoDbContext = new MoDbContext();
            var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            if (sessionData?.Account != null)
			{
				if (sessionData.Account.UserId == "admin_manage")
					ErasoftDbContext = new ErasoftContext();
				else
					ErasoftDbContext = new ErasoftContext(sessionData.Account.UserId);
			}
            else
            {
                if (sessionData?.User != null)
                {
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                    ErasoftDbContext = new ErasoftContext(accFromUser.UserId);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            MoDbContext.Dispose();
            ErasoftDbContext?.Dispose();
        }


        [Route("manage/posting")]
        public ActionResult frmPosting()
        {
            return View();
        }

        [HttpPost]
        public string doPosting(PostingViewModel.DataPosting data)
        {
            //return string.Format("http://localhost/masteronline/Proses/FormProsesPembelian.aspx?UserID={0}&Month={1}&Year={2}&From={3}&To={4}",
            return string.Format("http://202.67.14.92:3535/MOReport/Proses/FormProsesPembelian.aspx?UserID={0}&Month={1}&Year={2}&From={3}&To={4}",
				Uri.EscapeDataString(data.UserID),
				Uri.EscapeDataString(data.Month),
				Uri.EscapeDataString(data.Year),
				Uri.EscapeDataString(data.From),
				Uri.EscapeDataString(data.To));
		}
	}
}