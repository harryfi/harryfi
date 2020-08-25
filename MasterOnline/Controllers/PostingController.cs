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
                    ErasoftDbContext = new ErasoftContext(accFromUser.DataSourcePath, accFromUser.DatabasePathErasoft);
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



#if AWS
            //return string.Format("http://localhost/masteronline/Proses/FormProsesPembelian.aspx?UserID={0}&Month={1}&Year={2}&From={3}&To={4}",
            //return string.Format("http://13.251.222.53:3535/MOReport/Proses/FormProsesPembelian.aspx?UserID={0}&Month={1}&Year={2}&From={3}&To={4}",
            return string.Format("https://report.masteronline.co.id/Proses/FormProsesPembelian.aspx?UserID={0}&Month={1}&Year={2}&From={3}&To={4}",
				Uri.EscapeDataString(data.UserID),
				Uri.EscapeDataString(data.Month),
				Uri.EscapeDataString(data.Year),
				Uri.EscapeDataString(data.From),
				Uri.EscapeDataString(data.To));
#else
            //return string.Format("http://localhost/masteronline/Proses/FormProsesPembelian.aspx?UserID={0}&Month={1}&Year={2}&From={3}&To={4}",
            //return string.Format("http://13.251.222.53:3535/MOReport/Proses/FormProsesPembelian.aspx?UserID={0}&Month={1}&Year={2}&From={3}&To={4}",
            return string.Format("https://devreport.masteronline.co.id/Proses/FormProsesPembelian.aspx?UserID={0}&Month={1}&Year={2}&From={3}&To={4}",
                Uri.EscapeDataString(data.UserID),
                Uri.EscapeDataString(data.Month),
                Uri.EscapeDataString(data.Year),
                Uri.EscapeDataString(data.From),
                Uri.EscapeDataString(data.To));
#endif

        }

        //add by nurul 4/8/2020
        public class lastPosting
        {
            public string no_bukti { get; set; }
            public DateTime tanggal { get; set; }
            public string tgl { get; set; }
            public string bulan { get; set; }
            public string tahun { get; set; }
        }
        public ActionResult getLastTglPosting()
        {
            var sSQL = "select top 1 * from ( ";
            sSQL += "select * from (SELECT top 1 no_bukti,tgl as tanggal FROM sit01a where ISNULL(st_POSTING,'') <> 'Y' AND STATUS NOT IN ('2','3')  order by tanggal asc )a ";
            sSQL += "union ";
            sSQL += "select * from (SELECT top 1 inv as no_bukti,tgl as tanggal FROM pbt01a where ISNULL(POSTING,'') <> 'Y' AND STATUS NOT IN ('2','3')  order by tanggal asc )a ";
            sSQL += "union ";
            sSQL += "select * from (SELECT top 1 nobuk as no_bukti,tgl as tanggal FROM stt01a where ISNULL(st_POSTING,'') <> 'Y'  order by tanggal asc )a ";
            sSQL += "union ";
            sSQL += "select * from (SELECT top 1 faktur as no_bukti,tgl as tanggal FROM art01a where ISNULL(POST,'') <> 'Y'  order by tanggal asc )a ";
            sSQL += "union ";
            sSQL += "select * from (SELECT top 1 inv as no_bukti,tgl as tanggal FROM apt01a where ISNULL(POSTING,'') <> 'Y' order by tanggal asc )a ";
            sSQL += "union ";
            sSQL += "select * from (SELECT top 1 bukti as no_bukti,tgl as tanggal FROM art03a where ISNULL(POSTING,'') <> 'Y'  order by tanggal asc )a ";
            sSQL += "union ";
            sSQL += "select * from (SELECT top 1 bukti as no_bukti,tgl as tanggal FROM apt03a where ISNULL(POSTING,'') <> 'Y' order by tanggal asc )a ";
            sSQL += ")b order by tanggal asc ";
            var tempLastPosting = ErasoftDbContext.Database.SqlQuery<lastPosting>(sSQL).SingleOrDefault();

            var Vm = new lastPosting()
            {
            };
            if (tempLastPosting != null)
            {
                Vm.no_bukti = tempLastPosting.no_bukti;
                Vm.tanggal = tempLastPosting.tanggal;
                Vm.tgl = tempLastPosting.tanggal.Day.ToString();
                System.Globalization.DateTimeFormatInfo mfi = new System.Globalization.DateTimeFormatInfo();
                Vm.bulan = mfi.GetMonthName(tempLastPosting.tanggal.Month).ToString();
                //Vm.bulan = tempLastPosting.tanggal.Month.ToString();
                Vm.tahun = tempLastPosting.tanggal.Year.ToString();
            }

            return Json(Vm, JsonRequestBehavior.AllowGet);
        }
        //end add by nurul 4/8/2020
    }
}