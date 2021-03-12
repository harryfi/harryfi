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
        string dbSourceEra = "";

        public PostingController()
        {
            MoDbContext = new MoDbContext("");
            //            var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            //            if (sessionData?.Account != null)
            //			{
            //				if (sessionData.Account.UserId == "admin_manage")
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

            //			}
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


        [Route("manage/posting")]
        public ActionResult frmPosting()
        {
            return View();
        }

        [HttpPost]
        public string doPosting(PostingViewModel.DataPosting data)
        {

            updateHarsat0();

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
            //return string.Format("https://devreport.masteronline.co.id/Proses/FormProsesPembelian.aspx?UserID={0}&Month={1}&Year={2}&From={3}&To={4}",
            return string.Format("https://devreport.masteronline.my.id/Proses/FormProsesPembelian.aspx?UserID={0}&Month={1}&Year={2}&From={3}&To={4}",
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
            sSQL += "union ";
            sSQL += "select * from (SELECT top 1 bukti as no_bukti,tgl as tanggal FROM GLFTRAN1 where ISNULL(POSTING,'') <> 'Y' order by tanggal asc )a ";
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

        //add by nurul 29/1/2021
        public string updateHarsat0()
        {
            var ret = "";
            try
            {
                var sSQL = "select b.no from stt01a a (nolock) inner join stt01b b (nolock) on a.nobuk=b.nobuk where harsat =0 and harga > 0 and qty > 0 and isnull(st_POSTING,'') <> 'Y' and year(a.tgl) > 2019 order by a.tgl";
                var cekHarsatST0 = ErasoftDbContext.Database.SqlQuery<int>(sSQL).ToList();
                if (cekHarsatST0.Count() > 0)
                {
                    string listRec = "";
                    foreach (var rec in cekHarsatST0)
                    {
                        listRec += "'" + rec + "',";
                    }
                    listRec = listRec.Substring(0, listRec.Length - 1);
                    var sSQLUpdate = "update b set harsat=harga/qty from stt01a a (nolock) inner join stt01b b (nolock) on a.nobuk=b.nobuk where isnull(a.st_posting,'') <>'Y' and year(a.tgl) > 2019 and harsat =0 and harga > 0 and qty > 0 and b.no in (" + listRec + ") ";
                    var updateHarsat = ErasoftDbContext.Database.ExecuteSqlCommand(sSQLUpdate);
                    ErasoftDbContext.SaveChanges();
                }
            }catch(Exception ex)
            {

            }
            return ret;
        }
        //end add by nurul 29/1/2021
    }
}