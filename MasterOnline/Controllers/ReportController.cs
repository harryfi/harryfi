using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using MasterOnline.Models;
using MasterOnline.Services;
using MasterOnline.ViewModels;
//add by nurul 29/7/2019
using PagedList;
//end add by nurul 29/7/2019

namespace MasterOnline.Controllers
{
    [SessionCheck]
    public class ReportController : Controller
    {
        public MoDbContext MoDbContext { get; set; }
        public ErasoftContext ErasoftDbContext { get; set; }
        string dbSourceEra = "";

        public ReportController()
        {
            MoDbContext = new MoDbContext("");
            var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            if (sessionData?.Account != null)
            {
                if (sessionData.Account.UserId == "admin_manage")
                {
                    ErasoftDbContext = new ErasoftContext();
                }
                else
                {
#if (Debug_AWS)
                    dbSourceEra = sessionData.Account.DataSourcePathDebug;
#else
                    dbSourceEra = sessionData.Account.DataSourcePath;
#endif
                    ErasoftDbContext = new ErasoftContext(dbSourceEra, sessionData.Account.DatabasePathErasoft);
                }
                    
            }
            else
            {
                if (sessionData?.User != null)
                {
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
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


        [Route("reports/1")]
        public ActionResult MutasiPiutang()
        {
            return View();
        }

        [HttpPost]
        public string Preview1(ReportViewModel.Report1 data)
        {
            //13.250.232.74
            //https://masteronline.co.id
            //13.251.222.53
            //https://devreport.masteronline.co.id
            //return string.Format("http://13.251.222.53:3535/MOReport/Report/Form/frmLaporanMutasiPiutangTanpaPosting.aspx?UserID={0}&From={1}&To={2}&CutOff={3}",
#if AWS
            return string.Format("https://report.masteronline.co.id/Report/Form/frmLaporanMutasiPiutangTanpaPosting.aspx?UserID={0}&From={1}&To={2}&CutOff={3}",
            Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromCust),
                Uri.EscapeDataString(data.ToCust),
                Uri.EscapeDataString(data.CutOffDate));
#else
            return string.Format("https://devreport.masteronline.co.id/Report/Form/frmLaporanMutasiPiutangTanpaPosting.aspx?UserID={0}&From={1}&To={2}&CutOff={3}",
            Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromCust),
                Uri.EscapeDataString(data.ToCust),
                Uri.EscapeDataString(data.CutOffDate));
#endif
        }

        [Route("reports/2")]
        public ActionResult MutasiHutang()
        {
            return View();
        }

        [HttpPost]
        public string Preview2(ReportViewModel.Report2 data)
        {
#if AWS
            return string.Format("https://report.masteronline.co.id/Report/Form/frmLaporanMutasiTanpaPosting.aspx?UserID={0}&From={1}&To={2}&CutOff={3}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromSupp),
                Uri.EscapeDataString(data.ToSupp),
                Uri.EscapeDataString(data.CutOffDate));
#else
            return string.Format("https://devreport.masteronline.co.id/Report/Form/frmLaporanMutasiTanpaPosting.aspx?UserID={0}&From={1}&To={2}&CutOff={3}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromSupp),
                Uri.EscapeDataString(data.ToSupp),
                Uri.EscapeDataString(data.CutOffDate));
#endif
        }

        [Route("reports/3")]
        public ActionResult KartuPiutang()
        {
            return View();
        }

        [HttpPost]
        public string Preview3(ReportViewModel.Report3 data)
        {
#if AWS
            return string.Format("https://report.masteronline.co.id/Report/Form/KartuPiutangtanpaposting.aspx?UserID={0}&From={1}&To={2}&FromMonth={3}&CutOff={4}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromCust),
                Uri.EscapeDataString(data.ToCust),
                Uri.EscapeDataString(data.FromMonth),
                Uri.EscapeDataString(data.CutOffDate));
#else
            return string.Format("https://devreport.masteronline.co.id/Report/Form/KartuPiutangtanpaposting.aspx?UserID={0}&From={1}&To={2}&FromMonth={3}&CutOff={4}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromCust),
                Uri.EscapeDataString(data.ToCust),
                Uri.EscapeDataString(data.FromMonth),
                Uri.EscapeDataString(data.CutOffDate));
#endif
        }

        [Route("reports/4")]
        public ActionResult KartuHutang()
        {
            return View();
        }

        [HttpPost]
        public string Preview4(ReportViewModel.Report4 data)
        {
#if AWS
            return string.Format("https://report.masteronline.co.id/Report/Form/KartuHutang_Tanpa_Posting.aspx?UserID={0}&From={1}&To={2}&FromMonth={3}&CutOff={4}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromSupp),
                Uri.EscapeDataString(data.ToSupp),
                Uri.EscapeDataString(data.FromMonth),
                Uri.EscapeDataString(data.CutOffDate));
#else
            return string.Format("https://devreport.masteronline.co.id/Report/Form/KartuHutang_Tanpa_Posting.aspx?UserID={0}&From={1}&To={2}&FromMonth={3}&CutOff={4}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromSupp),
                Uri.EscapeDataString(data.ToSupp),
                Uri.EscapeDataString(data.FromMonth),
                Uri.EscapeDataString(data.CutOffDate));
#endif

        }

        [Route("reports/5")]
        public ActionResult AnalisaPembelian()
        {
            return View();
        }

        [HttpPost]
        public string Preview5(ReportViewModel.Report5 data)
        {
#if AWS
            return string.Format("https://report.masteronline.co.id/Report/Form/frm_LAnalisaPem.aspx?UserID={0}&FromSupp={1}&ToSupp={2}&FromBrg={3}&ToBrg={4}&DrTanggal={5}&SdTanggal={6}&Order={7}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromSupp),
                Uri.EscapeDataString(data.ToSupp),
                Uri.EscapeDataString(data.FromBrg),
                Uri.EscapeDataString(data.ToBrg),
                Uri.EscapeDataString(data.DrTanggal),
                Uri.EscapeDataString(data.SdTanggal),
                Uri.EscapeDataString(data.Order));
#else
            //change by nurul 11/1/2019 -- return string.Format("https://devreport.masteronline.co.id/Report/Form/frm_LAnalisaPem.aspx?UserID={0}&FromSupp={1}&ToSupp={2}&FromBrg={3}&ToBrg={4}&DrTanggal={5}&SdTanggal={6}",
            return string.Format("https://devreport.masteronline.co.id/Report/Form/frm_LAnalisaPem.aspx?UserID={0}&FromSupp={1}&ToSupp={2}&FromBrg={3}&ToBrg={4}&DrTanggal={5}&SdTanggal={6}&Order={7}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromSupp),
                Uri.EscapeDataString(data.ToSupp),
                Uri.EscapeDataString(data.FromBrg),
                Uri.EscapeDataString(data.ToBrg),
                Uri.EscapeDataString(data.DrTanggal),
                Uri.EscapeDataString(data.SdTanggal),
                //add by nurul 11/1/2019
                Uri.EscapeDataString(data.Order));
                //end add
#endif

        }

        [Route("reports/6")]
        public ActionResult AnalisaRugiLabaPenjualan()
        {
            return View();
        }

        [HttpPost]
        public string Preview6(ReportViewModel.Report6 data)
        {
#if AWS
            return string.Format("https://report.masteronline.co.id/Report/Form/frm_LAnalisaRLPenj_SP.aspx?UserID={0}&FromCust={1}&ToCust={2}&FromBrg={3}&ToBrg={4}&DrTanggal={5}&SdTanggal={6}&Order={7}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromCust),
                Uri.EscapeDataString(data.ToCust),
                Uri.EscapeDataString(data.FromBrg),
                Uri.EscapeDataString(data.ToBrg),
                Uri.EscapeDataString(data.DrTanggal),
                Uri.EscapeDataString(data.SdTanggal),
                Uri.EscapeDataString(data.Order));
#else
            //change by nurul 11/1/2019 -- return string.Format("https://devreport.masteronline.co.id/Report/Form/frm_LAnalisaRLPenj_SP.aspx?UserID={0}&FromCust={1}&ToCust={2}&FromBrg={3}&ToBrg={4}&DrTanggal={5}&SdTanggal={6}",
            //return string.Format("https://devreport.masteronline.co.id/Report/Form/frm_LAnalisaRLPenj_SP.aspx?UserID={0}&FromCust={1}&ToCust={2}&FromBrg={3}&ToBrg={4}&DrTanggal={5}&SdTanggal={6}&Order={7}&FromBuyer={8}&ToBuyer={9}",
            return string.Format("https://devreport.masteronline.co.id/Report/Form/frm_LAnalisaRLPenj_SP.aspx?UserID={0}&FromCust={1}&ToCust={2}&FromBrg={3}&ToBrg={4}&DrTanggal={5}&SdTanggal={6}&Order={7}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromCust),
                Uri.EscapeDataString(data.ToCust),
                Uri.EscapeDataString(data.FromBrg),
                Uri.EscapeDataString(data.ToBrg),
                Uri.EscapeDataString(data.DrTanggal),
                Uri.EscapeDataString(data.SdTanggal),
                //add by nurul 11/1/2019
                Uri.EscapeDataString(data.Order));
                //Uri.EscapeDataString(data.FromBuyer),
                //Uri.EscapeDataString(data.ToBuyer));
                //end add 
#endif
        }

        [Route("reports/7")]
        public ActionResult KartuStok()
        {
            return View();
        }

        [HttpPost]
        public string Preview7(ReportViewModel.Report7 data)
        {
#if AWS
            return string.Format("https://report.masteronline.co.id/Report/Form/FrmLapSTT09_TanpaPosting.aspx?iJenisForm=SETELAH_POSTING&UserID={0}&Gudang={1}&FromBrg={2}&ToBrg={3}&FromMonth={4}&CutOff={5}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.Gudang),
                Uri.EscapeDataString(data.FromBrg),
                Uri.EscapeDataString(data.ToBrg),
                Uri.EscapeDataString(data.FromMonth),
                Uri.EscapeDataString(data.CutOffDate));
#else
            return string.Format("https://devreport.masteronline.co.id/Report/Form/FrmLapSTT09_TanpaPosting.aspx?iJenisForm=SETELAH_POSTING&UserID={0}&Gudang={1}&FromBrg={2}&ToBrg={3}&FromMonth={4}&CutOff={5}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.Gudang),
                Uri.EscapeDataString(data.FromBrg),
                Uri.EscapeDataString(data.ToBrg),
                Uri.EscapeDataString(data.FromMonth),
                Uri.EscapeDataString(data.CutOffDate));
#endif

        }

        [Route("reports/8")]
        public ActionResult MutasiStokBulanan()
        {
            return View();
        }

        [HttpPost]
        public string Preview8(ReportViewModel.Report8 data)
        {
#if AWS
            return string.Format("https://report.masteronline.co.id/Report/Form/FrmLapSTT08_Drilldown_TanpaPosting2.aspx?UserID={0}&Gudang={1}&FromBrg={2}&ToBrg={3}&Tahun={4}&DrBulan={5}&SdBulan={6}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.Gudang),
                Uri.EscapeDataString(data.FromBrg),
                Uri.EscapeDataString(data.ToBrg),
                Uri.EscapeDataString(data.Tahun),
                Uri.EscapeDataString(data.DrBulan),
                Uri.EscapeDataString(data.SdBulan));
#else
            return string.Format("https://devreport.masteronline.co.id/Report/Form/FrmLapSTT08_Drilldown_TanpaPosting2.aspx?UserID={0}&Gudang={1}&FromBrg={2}&ToBrg={3}&Tahun={4}&DrBulan={5}&SdBulan={6}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.Gudang),
                Uri.EscapeDataString(data.FromBrg),
                Uri.EscapeDataString(data.ToBrg),
                Uri.EscapeDataString(data.Tahun),
                Uri.EscapeDataString(data.DrBulan),
                Uri.EscapeDataString(data.SdBulan));
#endif

        }

        [Route("reports/9")]
        public ActionResult Neraca()
        {
            return View();
        }

        [HttpPost]
        public string Preview9(ReportViewModel.Report9 data)
        {
#if AWS
            return string.Format("https://report.masteronline.co.id/Report/Form/frmCetakLaporanNeraca.aspx?UserID={0}&KdLap={1}&Tahun={2}&Bulan={3}&Print=Yes",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.KdLap),
                Uri.EscapeDataString(data.Tahun),
                Uri.EscapeDataString(data.Bulan));
#else
            // change by nurul 12/10/2018   return string.Format("https://devreport.masteronline.co.id/Report/Form/frmCetakLaporanNeraca.aspx?UserID={0}&KdLap={1}&Tahun={2}&Bulan={3}&Print={4}",
            return string.Format("https://devreport.masteronline.co.id/Report/Form/frmCetakLaporanNeraca.aspx?UserID={0}&KdLap={1}&Tahun={2}&Bulan={3}&Print=Yes",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.KdLap),
                Uri.EscapeDataString(data.Tahun),
                Uri.EscapeDataString(data.Bulan));
                //Uri.EscapeDataString(data.Print));
#endif

        }

        [Route("reports/10")]
        public ActionResult RugiLaba()
        {
            return View();
        }

        [HttpPost]
        public string Preview10(ReportViewModel.Report10 data)
        {
#if AWS
            return string.Format("https://report.masteronline.co.id/Report/Form/frmCetakLapRugiLaba.aspx?UserID={0}&KdLap={1}&Tahun={2}&Bulan={3}&Print=Yes",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.KdLap),
                Uri.EscapeDataString(data.Tahun),               
                Uri.EscapeDataString(data.Bulan));
#else
            // change by nurul 12/10/2018   return string.Format("https://devreport.masteronline.co.id/Report/Form/frmCetakLapRugiLaba.aspx?UserID={0}&KdLap={1}&Bulan={2}&Print={3}",
            return string.Format("https://devreport.masteronline.co.id/Report/Form/frmCetakLapRugiLaba.aspx?UserID={0}&KdLap={1}&Tahun={2}&Bulan={3}&Print=Yes",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.KdLap),
                Uri.EscapeDataString(data.Tahun),
                Uri.EscapeDataString(data.Bulan));
                //Uri.EscapeDataString(data.Print));
#endif

        }

        [Route("reports/11")]
        public ActionResult CetakBukuBesar()
        {
            return View();
        }

        [HttpPost]
        public string Preview11(ReportViewModel.Report11 data)
        {
#if AWS
            return string.Format("https://report.masteronline.co.id/Report/Form/frmCetakBukuBesar.aspx?UserID={0}&DrRek={1}&SdRek={2}&Type={3}&Print={4}&Posting={5}&Tahun={6}&DrBulan={7}&SdBulan={8}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.DrRek),
                Uri.EscapeDataString(data.SdRek),
                Uri.EscapeDataString(data.Type),
                Uri.EscapeDataString(data.Print),
                Uri.EscapeDataString(data.Posting),
                Uri.EscapeDataString(data.Tahun),
                Uri.EscapeDataString(data.DrBulan),
                Uri.EscapeDataString(data.SdBulan));
#else
            return string.Format("https://devreport.masteronline.co.id/Report/Form/frmCetakBukuBesar.aspx?UserID={0}&DrRek={1}&SdRek={2}&Type={3}&Print={4}&Posting={5}&Tahun={6}&DrBulan={7}&SdBulan={8}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.DrRek),
                Uri.EscapeDataString(data.SdRek),
                Uri.EscapeDataString(data.Type),
                Uri.EscapeDataString(data.Print),
                Uri.EscapeDataString(data.Posting),
                Uri.EscapeDataString(data.Tahun),
                Uri.EscapeDataString(data.DrBulan),
                Uri.EscapeDataString(data.SdBulan));
#endif

        }

        [Route("reports/12")]
        public ActionResult CetakRekapBukuBesar()
        {
            return View();
        }

        [HttpPost]
        public string Preview12(ReportViewModel.Report12 data)
        {
#if AWS
            return string.Format("https://report.masteronline.co.id/Report/Form/frmCetakRekapBukuBesar.aspx?UserID={0}&DrRek={1}&SdRek={2}&Type={3}&Nol={4}&Posting={5}&Tahun={6}&DrBulan={7}&SdBulan={8}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.DrRek),
                Uri.EscapeDataString(data.SdRek),
                Uri.EscapeDataString(data.Type),
                Uri.EscapeDataString(data.Nol),
                Uri.EscapeDataString(data.Posting),
                Uri.EscapeDataString(data.Tahun),
                Uri.EscapeDataString(data.DrBulan),
                Uri.EscapeDataString(data.SdBulan));
#else
            return string.Format("https://devreport.masteronline.co.id/Report/Form/frmCetakRekapBukuBesar.aspx?UserID={0}&DrRek={1}&SdRek={2}&Type={3}&Nol={4}&Posting={5}&Tahun={6}&DrBulan={7}&SdBulan={8}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.DrRek),
                Uri.EscapeDataString(data.SdRek),
                Uri.EscapeDataString(data.Type),
                Uri.EscapeDataString(data.Nol),
                Uri.EscapeDataString(data.Posting),
                Uri.EscapeDataString(data.Tahun),
                Uri.EscapeDataString(data.DrBulan),
                Uri.EscapeDataString(data.SdBulan));
#endif

        }

        [Route("reports/13")]
        public ActionResult ListFakturPenjualan()
        {
            return View();
        }

        [HttpPost]
        public string Preview13(ReportViewModel.Report13 data)
        {
#if AWS
            return string.Format("https://report.masteronline.co.id/Report/Form/frm_rpt_FA.aspx?UserID={0}&FromCust={1}&ToCust={2}&DrTanggal={3}&SdTanggal={4}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromCust),
                Uri.EscapeDataString(data.ToCust),
                Uri.EscapeDataString(data.DrTanggal),
                Uri.EscapeDataString(data.SdTanggal));
#else
            return string.Format("https://devreport.masteronline.co.id/Report/Form/frm_rpt_FA.aspx?UserID={0}&FromCust={1}&ToCust={2}&DrTanggal={3}&SdTanggal={4}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromCust),
                Uri.EscapeDataString(data.ToCust),
                Uri.EscapeDataString(data.DrTanggal),
                Uri.EscapeDataString(data.SdTanggal));
#endif

        }

        [Route("reports/14")]
        public ActionResult ListInvPembelian()
        {
            return View();
        }

        [HttpPost]
        public string Preview14(ReportViewModel.Report14 data)
        {
#if AWS
            return string.Format("https://report.masteronline.co.id/Report/Form/frm_rptinv_1.aspx?UserID={0}&FromSupp={1}&ToSupp={2}&DrTanggal={3}&SdTanggal={4}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromSupp),
                Uri.EscapeDataString(data.ToSupp),
                Uri.EscapeDataString(data.DrTanggal),
                Uri.EscapeDataString(data.SdTanggal));
#else
            return string.Format("https://devreport.masteronline.co.id/Report/Form/frm_rptinv_1.aspx?UserID={0}&FromSupp={1}&ToSupp={2}&DrTanggal={3}&SdTanggal={4}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromSupp),
                Uri.EscapeDataString(data.ToSupp),
                Uri.EscapeDataString(data.DrTanggal),
                Uri.EscapeDataString(data.SdTanggal));
#endif
        }

        [Route("reports/15")]
        public ActionResult KartuStokDenganHarga()
        {
            return View();
        }
        [HttpPost]
        public string Preview15(ReportViewModel.Report15 data)
        {
#if AWS
            return string.Format("https://report.masteronline.co.id/Report/Form/FrmLapSTT09.aspx?iJenisForm=SETELAH_POSTING&UserID={0}&Gudang={1}&FromBrg={2}&ToBrg={3}&FromMonth={4}&CutOff={5}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.Gudang),
                Uri.EscapeDataString(data.FromBrg),
                Uri.EscapeDataString(data.ToBrg),
                Uri.EscapeDataString(data.FromMonth),
                Uri.EscapeDataString(data.CutOffDate));
#else
            return string.Format("https://devreport.masteronline.co.id/Report/Form/FrmLapSTT09.aspx?iJenisForm=SETELAH_POSTING&UserID={0}&Gudang={1}&FromBrg={2}&ToBrg={3}&FromMonth={4}&CutOff={5}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.Gudang),
                Uri.EscapeDataString(data.FromBrg),
                Uri.EscapeDataString(data.ToBrg),
                Uri.EscapeDataString(data.FromMonth),
                Uri.EscapeDataString(data.CutOffDate));
#endif

        }

        [Route("reports/16")]
        public ActionResult PosisiStok()
        {
            return View();
        }
        [HttpPost]
        public string Preview16(ReportViewModel.Report16 data)
        {
#if AWS
            return string.Format("https://report.masteronline.co.id/Report/Form/frmLapSTT02C1_TanpaPosting.aspx?iSort=KODE_BARANG&iPeriode=HARI_INI&UserID={0}&CutOff={1}&PilihStok={2}&From={3}&To={4}&Gudang1={5}&Gudang2={6}&Gudang3={7}&Gudang4={8}&Gudang5={9}&Gudang6={10}&Gudang7={11}&Gudang8={12}&Gudang9={13}&Gudang10={14}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.CutOffDate),
                Uri.EscapeDataString(data.PilihStok),
                Uri.EscapeDataString(data.FromBrg),
                Uri.EscapeDataString(data.ToBrg),
                Uri.EscapeDataString(data.Gudang1),
                Uri.EscapeDataString(data.Gudang2),
                Uri.EscapeDataString(data.Gudang3),
                Uri.EscapeDataString(data.Gudang4),
                Uri.EscapeDataString(data.Gudang5),
                Uri.EscapeDataString(data.Gudang6),
                Uri.EscapeDataString(data.Gudang7),
                Uri.EscapeDataString(data.Gudang8),
                Uri.EscapeDataString(data.Gudang9),
                Uri.EscapeDataString(data.Gudang10));
#else
            return string.Format("https://devreport.masteronline.co.id/Report/Form/frmLapSTT02C1_TanpaPosting.aspx?iSort=KODE_BARANG&iPeriode=HARI_INI&UserID={0}&CutOff={1}&PilihStok={2}&From={3}&To={4}&Gudang1={5}&Gudang2={6}&Gudang3={7}&Gudang4={8}&Gudang5={9}&Gudang6={10}&Gudang7={11}&Gudang8={12}&Gudang9={13}&Gudang10={14}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.CutOffDate),
                Uri.EscapeDataString(data.PilihStok),
                Uri.EscapeDataString(data.FromBrg),
                Uri.EscapeDataString(data.ToBrg),
                Uri.EscapeDataString(data.Gudang1),
                Uri.EscapeDataString(data.Gudang2),
                Uri.EscapeDataString(data.Gudang3),
                Uri.EscapeDataString(data.Gudang4),
                Uri.EscapeDataString(data.Gudang5),
                Uri.EscapeDataString(data.Gudang6),
                Uri.EscapeDataString(data.Gudang7),
                Uri.EscapeDataString(data.Gudang8),
                Uri.EscapeDataString(data.Gudang9),
                Uri.EscapeDataString(data.Gudang10));
#endif

        }

        public ActionResult PromptCustomer()
        {
            //CHANGE BY NURUL 29/7/2019
            ////var listCust = ErasoftDbContext.ARF01.ToList();
            ////var listCust = ErasoftDbContext.ARF01.
            ////                            Join(
            ////                                MoDbContext.Marketplaces,
            ////                                kode => kode.NAMA,
            ////                                nama => Convert.ToString(nama.IdMarket),
            ////                                (kode, nama) => new { Nama = nama.NamaMarket, Kode = kode.PERSO, Id = kode.CUST }
            ////                            ).ToList();

            ////var IDs = (from a in db1.Table1
            ////            join b in db1.Table2 on a.Id equals b.Id
            ////            orderby a.Status
            ////            where b.Id == 1 && a.Status == "new"
            ////            select new a.Id).ToArray();

            //// var query = from c in db2.Company
            ////             join a in IDs on c.Id equals a.Id
            ////             select new { Id = a.Id, CompanyId = c.CompanyId };

            //var ListMarketplaces = (from c in MoDbContext.Marketplaces
            //                        select new { Id = c.IdMarket, Nama = c.NamaMarket }).ToList();

            //var ListARF01 = (from a in ErasoftDbContext.ARF01
            //                 select new { Cust = a.CUST, Id_market = a.NAMA, Perso = a.PERSO }).ToList();

            //var listCust = (from a in ListARF01
            //                join c in ListMarketplaces on a.Id_market equals c.Id.ToString()
            //                select new mdlPromptCust { CUST = a.Cust, NAMA = c.Nama, PERSO = a.Perso }).ToList();

            //var listCust2 = ErasoftDbContext.ARF01.ToList();

            ////listCust.ForEach(
            ////    delegate (String CUST)
            ////    {
            ////        Console.WriteLine(CUST);
            ////        Console.WriteLine(NAMA);
            ////    });

            //return View("PromptCustomer", listCust);
            return View();
        }
        public class mdlPromptCust
        {
            public string CUST { get; set; }
            public string NAMA { get; set; }
            public string PERSO { get; set; }

        }
        public ActionResult PromptSupplier()
        {
            //ADD BY NURUL 29/7/2019
            //var listSupp = ErasoftDbContext.APF01.ToList();

            //return View("PromptSupplier", listSupp);
            return View();
            //END ADD BY NURUL 29/7/2019
        }
        public ActionResult PromptBarang()
        {
            //CHANGE BY NURUL 29/7/2019, PAGING 
            //change by nurul 18/1/2019var listBrg = ErasoftDbContext.STF02.ToList();
            //var listBrg = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList();

            //return View("PromptBarang", listBrg);
            return View();
            //END CHANGE BY NURUL 29/7/2019, PAGING 
        }
        //add by nurul 29/7/2019
        public class getTotalCount
        {
            public int JUMLAH { get; set; }
        }
        protected JsonResult JsonErrorMessage(string message)
        {
            var vmError = new InvoiceViewModel()
            {
            };
            vmError.Errors.Add(message);
            return Json(vmError, JsonRequestBehavior.AllowGet);
        }
        public ActionResult TablePromptBarangPartial(int? page, string search = "")
        {
            try
            {
                int pagenumber = (page ?? 1) - 1;
                ViewData["searchParam"] = search;
                ViewData["LastPage"] = page;

                //ADD BY NURUL 3/10/2019
                string[] getkata = search.Split(' ');
                string sSQLkode = "";
                string sSQLnama = "";
                if (getkata.Length > 0)
                {
                    if (search != "")
                    {
                        for (int i = 0; i < getkata.Length; i++)
                        {
                            if (getkata.Length == 1)
                            {
                                sSQLkode += "( BRG like '%" + getkata[i] + "%' )";
                                sSQLnama += " ( (ISNULL(NAMA,'') + ' ' + ISNULL(NAMA2,'')) like '%" + getkata[i] + "%' )";
                            }
                            else
                            {
                                if (getkata[i] == getkata.First())
                                {
                                    sSQLkode += " ( BRG like '%" + getkata[i] + "%'";
                                    sSQLnama += " ( (ISNULL(NAMA,'') + ' ' + ISNULL(NAMA2,'')) like '%" + getkata[i] + "%'";
                                }
                                else if (getkata[i] == getkata.Last())
                                {
                                    sSQLkode += " and BRG like '%" + getkata[i] + "%' )";
                                    sSQLnama += " and (ISNULL(NAMA,'') + ' ' + ISNULL(NAMA2,'')) like '%" + getkata[i] + "%' )";
                                }
                                else
                                {
                                    sSQLkode += " and BRG like '%" + getkata[i] + "%' ";
                                    sSQLnama += " and (ISNULL(NAMA,'') + ' ' + ISNULL(NAMA2,'')) like '%" + getkata[i] + "%' ";
                                }
                            }
                        }
                    }
                }
                //END ADD BY NURUL 3/10/2019

                string sSQLSelect = "";
                sSQLSelect += "SELECT BRG AS KODE, ISNULL(NAMA,'') + ' ' + ISNULL(NAMA2,'') AS NAMA  ";
                string sSQLCount = "";
                sSQLCount += "SELECT COUNT(ID) AS JUMLAH ";
                string sSQL2 = "";
                sSQL2 += "FROM STF02 ";
                sSQL2 += "WHERE TYPE = '3'";
                if (search != "")
                {
                    //sSQL2 += "AND (BRG LIKE '%" + search + "%' OR (ISNULL(NAMA, '') + ' ' + ISNULL(NAMA2, '')) LIKE '%" + search + "%' ) ";
                    sSQL2 += " and ( " + sSQLkode + " or " + sSQLnama + " ) ";
                }
                string sSQLSelect2 = "";
                sSQLSelect2 += "ORDER BY NAMA ASC, BRG ASC ";
                sSQLSelect2 += "OFFSET " + Convert.ToString(pagenumber * 10) + " ROWS ";
                sSQLSelect2 += "FETCH NEXT 10 ROWS ONLY ";

                var listBarang = ErasoftDbContext.Database.SqlQuery<PromptBarangViewModel>(sSQLSelect + sSQL2 + sSQLSelect2).ToList();

                var totalCount = ErasoftDbContext.Database.SqlQuery<getTotalCount>(sSQLCount + sSQL2).Single();

                IPagedList<PromptBarangViewModel> pageOrders = new StaticPagedList<PromptBarangViewModel>(listBarang, pagenumber + 1, 10, totalCount.JUMLAH);
                return PartialView("TablePromptBarangPartial", pageOrders);
            }
            catch (Exception ex)
            {
                return JsonErrorMessage("Prompt gagal");
            }
        }
        public ActionResult TablePromptBuyerPartial(int? page, string search = "")
        {
            try
            {
                int pagenumber = (page ?? 1) - 1;
                ViewData["searchParam"] = search;
                ViewData["LastPage"] = page;

                //ADD BY NURUL 3/10/2019
                string[] getkata = search.Split(' ');
                string sSQLkode = "";
                string sSQLnama = "";
                if (getkata.Length > 0)
                {
                    if (search != "")
                    {
                        for (int i = 0; i < getkata.Length; i++)
                        {
                            if (getkata.Length == 1)
                            {
                                sSQLkode += "( BUYER_CODE like '%" + getkata[i] + "%' )";
                                sSQLnama += " ( ISNULL(NAMA,'') like '%" + getkata[i] + "%' )";
                            }
                            else
                            {
                                if (getkata[i] == getkata.First())
                                {
                                    sSQLkode += " ( BUYER_CODE like '%" + getkata[i] + "%'";
                                    sSQLnama += " ( ISNULL(NAMA,'') like '%" + getkata[i] + "%'";
                                }
                                else if (getkata[i] == getkata.Last())
                                {
                                    sSQLkode += " and BUYER_CODE like '%" + getkata[i] + "%' )";
                                    sSQLnama += " and ISNULL(NAMA,'') like '%" + getkata[i] + "%' )";
                                }
                                else
                                {
                                    sSQLkode += " and BUYER_CODE like '%" + getkata[i] + "%' ";
                                    sSQLnama += " and ISNULL(NAMA,'') like '%" + getkata[i] + "%' ";
                                }
                            }
                        }
                    }
                }
                //END ADD BY NURUL 3/10/2019

                string sSQLSelect = "";
                sSQLSelect += "SELECT BUYER_CODE AS KODE, ISNULL(NAMA,'') AS NAMA  ";
                string sSQLCount = "";
                sSQLCount += "SELECT COUNT(RECNUM) AS JUMLAH ";
                string sSQL2 = "";
                sSQL2 += "FROM ARF01C ";
                if (search != "")
                {
                    //sSQL2 += " WHERE (BUYER_CODE LIKE '%" + search + "%' OR ISNULL(NAMA, '') LIKE '%" + search + "%' ) ";
                    sSQL2 += " where ( " + sSQLkode + " or " + sSQLnama + " ) ";
                }
                string sSQLSelect2 = "";
                sSQLSelect2 += "ORDER BY NAMA ASC, BUYER_CODE ASC ";
                sSQLSelect2 += "OFFSET " + Convert.ToString(pagenumber * 10) + " ROWS ";
                sSQLSelect2 += "FETCH NEXT 10 ROWS ONLY ";

                var listData = ErasoftDbContext.Database.SqlQuery<PromptBarangViewModel>(sSQLSelect + sSQL2 + sSQLSelect2).ToList();

                var totalCount = ErasoftDbContext.Database.SqlQuery<getTotalCount>(sSQLCount + sSQL2).Single();

                IPagedList<PromptBarangViewModel> pageOrders = new StaticPagedList<PromptBarangViewModel>(listData, pagenumber + 1, 10, totalCount.JUMLAH);
                return PartialView("TablePromptBuyerPartial", pageOrders);
            }
            catch (Exception ex)
            {
                return JsonErrorMessage("Prompt gagal");
            }
        }
        public ActionResult TablePromptCustomerPartial(int? page, string search = "")
        {
            try
            {
                int pagenumber = (page ?? 1) - 1;
                ViewData["searchParam"] = search;
                ViewData["LastPage"] = page;

                //ADD BY NURUL 3/10/2019
                string[] getkata = search.Split(' ');
                string sSQLkode = "";
                string sSQLnama = "";
                if (getkata.Length > 0)
                {
                    if (search != "")
                    {
                        for (int i = 0; i < getkata.Length; i++)
                        {
                            if (getkata.Length == 1)
                            {
                                sSQLkode += "( A.CUST like '%" + getkata[i] + "%' )";
                                sSQLnama += " ( (ISNULL(C.NamaMarket,'') + ' - ' + ISNULL(A.PERSO,'')) like '%" + getkata[i] + "%' )";
                            }
                            else
                            {
                                if (getkata[i] == getkata.First())
                                {
                                    sSQLkode += " ( A.CUST like '%" + getkata[i] + "%'";
                                    sSQLnama += " ( (ISNULL(C.NamaMarket,'') + ' - ' + ISNULL(A.PERSO,'')) like '%" + getkata[i] + "%'";
                                }
                                else if (getkata[i] == getkata.Last())
                                {
                                    sSQLkode += " and A.CUST like '%" + getkata[i] + "%' )";
                                    sSQLnama += " and (ISNULL(C.NamaMarket,'') + ' - ' + ISNULL(A.PERSO,'')) like '%" + getkata[i] + "%' )";
                                }
                                else
                                {
                                    sSQLkode += " and A.CUST like '%" + getkata[i] + "%' ";
                                    sSQLnama += " and (ISNULL(C.NamaMarket,'') + ' - ' + ISNULL(A.PERSO,'')) like '%" + getkata[i] + "%' ";
                                }
                            }
                        }
                    }
                }
                //END ADD BY NURUL 3/10/2019

                string sSQLSelect = "";
                sSQLSelect += "SELECT A.CUST AS KODE, ISNULL(C.NamaMarket,'') AS NAMA, ISNULL(A.PERSO,'') AS PERSO ";
                string sSQLCount = "";
                sSQLCount += "SELECT COUNT(RECNUM) AS JUMLAH ";
                string sSQL2 = "";
                sSQL2 += "FROM ARF01 A ";
                sSQL2 += "LEFT JOIN MO.dbo.MARKETPLACE C ON A.NAMA = C.IdMarket ";
                if (search != "")
                {
                    //sSQL2 += " WHERE (A.CUST LIKE '%" + search + "%' OR ISNULL(C.NamaMarket, '') LIKE '%" + search + "%' OR ISNULL(A.PERSO, '') LIKE '%" + search + "%' ) ";
                    sSQL2 += " where ( " + sSQLkode + " or " + sSQLnama + " ) ";
                }
                string sSQLSelect2 = "";
                sSQLSelect2 += "ORDER BY A.CUST ASC ";
                sSQLSelect2 += "OFFSET " + Convert.ToString(pagenumber * 10) + " ROWS ";
                sSQLSelect2 += "FETCH NEXT 10 ROWS ONLY ";

                var listData = ErasoftDbContext.Database.SqlQuery<mdlCustomer>(sSQLSelect + sSQL2 + sSQLSelect2).ToList();

                var totalCount = ErasoftDbContext.Database.SqlQuery<getTotalCount>(sSQLCount + sSQL2).Single();

                IPagedList<mdlCustomer> pageOrders = new StaticPagedList<mdlCustomer>(listData, pagenumber + 1, 10, totalCount.JUMLAH);
                return PartialView("TablePromptCustomerPartial", pageOrders);
            }
            catch (Exception ex)
            {
                return JsonErrorMessage("Prompt gagal");
            }
        }
        public ActionResult TablePromptGudangPartial(int? page, string search = "")
        {
            try
            {
                int pagenumber = (page ?? 1) - 1;
                ViewData["searchParam"] = search;
                ViewData["LastPage"] = page;

                //ADD BY NURUL 3/10/2019
                string[] getkata = search.Split(' ');
                string sSQLkode = "";
                string sSQLnama = "";
                if (getkata.Length > 0)
                {
                    if (search != "")
                    {
                        for (int i = 0; i < getkata.Length; i++)
                        {
                            if (getkata.Length == 1)
                            {
                                sSQLkode += "( KODE_GUDANG like '%" + getkata[i] + "%' )";
                                sSQLnama += " ( ISNULL(NAMA_GUDANG,'') like '%" + getkata[i] + "%' )";
                            }
                            else
                            {
                                if (getkata[i] == getkata.First())
                                {
                                    sSQLkode += " ( KODE_GUDANG like '%" + getkata[i] + "%'";
                                    sSQLnama += " ( ISNULL(NAMA_GUDANG,'') like '%" + getkata[i] + "%'";
                                }
                                else if (getkata[i] == getkata.Last())
                                {
                                    sSQLkode += " and KODE_GUDANG like '%" + getkata[i] + "%' )";
                                    sSQLnama += " and ISNULL(NAMA_GUDANG,'') like '%" + getkata[i] + "%' )";
                                }
                                else
                                {
                                    sSQLkode += " and KODE_GUDANG like '%" + getkata[i] + "%' ";
                                    sSQLnama += " and ISNULL(NAMA_GUDANG,'') like '%" + getkata[i] + "%' ";
                                }
                            }
                        }
                    }
                }
                //END ADD BY NURUL 3/10/2019

                string sSQLSelect = "";
                sSQLSelect += "SELECT KODE_GUDANG AS KODE, ISNULL(NAMA_GUDANG,'') AS NAMA  ";
                string sSQLCount = "";
                sSQLCount += "SELECT COUNT(ID) AS JUMLAH ";
                string sSQL2 = "";
                sSQL2 += "FROM STF18 ";
                if (search != "")
                {
                    //sSQL2 += " WHERE (KODE_GUDANG LIKE '%" + search + "%' OR ISNULL(NAMA_GUDANG, '') LIKE '%" + search + "%' ) ";
                    sSQL2 += " where ( " + sSQLkode + " or " + sSQLnama + " ) ";
                }
                string sSQLSelect2 = "";
                sSQLSelect2 += "ORDER BY KODE_GUDANG ASC ";
                sSQLSelect2 += "OFFSET " + Convert.ToString(pagenumber * 10) + " ROWS ";
                sSQLSelect2 += "FETCH NEXT 10 ROWS ONLY ";

                var listData = ErasoftDbContext.Database.SqlQuery<PromptBarangViewModel>(sSQLSelect + sSQL2 + sSQLSelect2).ToList();

                var totalCount = ErasoftDbContext.Database.SqlQuery<getTotalCount>(sSQLCount + sSQL2).Single();

                IPagedList<PromptBarangViewModel> pageOrders = new StaticPagedList<PromptBarangViewModel>(listData, pagenumber + 1, 10, totalCount.JUMLAH);
                return PartialView("TablePromptGudangPartial", pageOrders);
            }
            catch (Exception ex)
            {
                return JsonErrorMessage("Prompt gagal");
            }
        }
        public ActionResult TablePromptKodeRekPartial(int? page, string search = "")
        {
            try
            {
                int pagenumber = (page ?? 1) - 1;
                ViewData["searchParam"] = search;
                ViewData["LastPage"] = page;

                //ADD BY NURUL 3/10/2019
                string[] getkata = search.Split(' ');
                string sSQLkode = "";
                string sSQLnama = "";
                if (getkata.Length > 0)
                {
                    if (search != "")
                    {
                        for (int i = 0; i < getkata.Length; i++)
                        {
                            if (getkata.Length == 1)
                            {
                                sSQLkode += "( KODE like '%" + getkata[i] + "%' )";
                                sSQLnama += " ( ISNULL(NAMA,'') like '%" + getkata[i] + "%' )";
                            }
                            else
                            {
                                if (getkata[i] == getkata.First())
                                {
                                    sSQLkode += " ( KODE like '%" + getkata[i] + "%'";
                                    sSQLnama += " ( ISNULL(NAMA,'') like '%" + getkata[i] + "%'";
                                }
                                else if (getkata[i] == getkata.Last())
                                {
                                    sSQLkode += " and KODE like '%" + getkata[i] + "%' )";
                                    sSQLnama += " and ISNULL(NAMA,'') like '%" + getkata[i] + "%' )";
                                }
                                else
                                {
                                    sSQLkode += " and KODE like '%" + getkata[i] + "%' ";
                                    sSQLnama += " and ISNULL(NAMA,'') like '%" + getkata[i] + "%' ";
                                }
                            }
                        }
                    }
                }
                //END ADD BY NURUL 3/10/2019

                string sSQLSelect = "";
                sSQLSelect += "SELECT KODE AS KODE, ISNULL(NAMA,'') AS NAMA  ";
                string sSQLCount = "";
                sSQLCount += "SELECT COUNT(RECNUM) AS JUMLAH ";
                string sSQL2 = "";
                sSQL2 += "FROM GLFREK ";
                if (search != "")
                {
                    //sSQL2 += " WHERE (KODE LIKE '%" + search + "%' OR ISNULL(NAMA, '') LIKE '%" + search + "%' ) ";
                    sSQL2 += " where ( " + sSQLkode + " or " + sSQLnama + " ) ";
                }
                string sSQLSelect2 = "";
                sSQLSelect2 += "ORDER BY KODE ASC ";
                sSQLSelect2 += "OFFSET " + Convert.ToString(pagenumber * 10) + " ROWS ";
                sSQLSelect2 += "FETCH NEXT 10 ROWS ONLY ";

                var listData = ErasoftDbContext.Database.SqlQuery<PromptBarangViewModel>(sSQLSelect + sSQL2 + sSQLSelect2).ToList();

                var totalCount = ErasoftDbContext.Database.SqlQuery<getTotalCount>(sSQLCount + sSQL2).Single();

                IPagedList<PromptBarangViewModel> pageOrders = new StaticPagedList<PromptBarangViewModel>(listData, pagenumber + 1, 10, totalCount.JUMLAH);
                return PartialView("TablePromptKodeRekPartial", pageOrders);
            }
            catch (Exception ex)
            {
                return JsonErrorMessage("Prompt gagal");
            }
        }
        public ActionResult TablePromptSupplierPartial(int? page, string search = "")
        {
            try
            {
                int pagenumber = (page ?? 1) - 1;
                ViewData["searchParam"] = search;
                ViewData["LastPage"] = page;

                //ADD BY NURUL 3/10/2019
                string[] getkata = search.Split(' ');
                string sSQLkode = "";
                string sSQLnama = "";
                if (getkata.Length > 0)
                {
                    if (search != "")
                    {
                        for (int i = 0; i < getkata.Length; i++)
                        {
                            if (getkata.Length == 1)
                            {
                                sSQLkode += "( SUPP like '%" + getkata[i] + "%' )";
                                sSQLnama += " ( ISNULL(NAMA,'') like '%" + getkata[i] + "%' )";
                            }
                            else
                            {
                                if (getkata[i] == getkata.First())
                                {
                                    sSQLkode += " ( SUPP like '%" + getkata[i] + "%'";
                                    sSQLnama += " ( ISNULL(NAMA,'') like '%" + getkata[i] + "%'";
                                }
                                else if (getkata[i] == getkata.Last())
                                {
                                    sSQLkode += " and SUPP like '%" + getkata[i] + "%' )";
                                    sSQLnama += " and ISNULL(NAMA,'') like '%" + getkata[i] + "%' )";
                                }
                                else
                                {
                                    sSQLkode += " and SUPP like '%" + getkata[i] + "%' ";
                                    sSQLnama += " and ISNULL(NAMA,'') like '%" + getkata[i] + "%' ";
                                }
                            }
                        }
                    }
                }
                //END ADD BY NURUL 3/10/2019

                string sSQLSelect = "";
                sSQLSelect += "SELECT SUPP AS KODE, ISNULL(NAMA,'') AS NAMA  ";
                string sSQLCount = "";
                sSQLCount += "SELECT COUNT(RECNUM) AS JUMLAH ";
                string sSQL2 = "";
                sSQL2 += "FROM APF01 ";
                if (search != "")
                {
                    //sSQL2 += " WHERE (SUPP LIKE '%" + search + "%' OR ISNULL(NAMA, '') LIKE '%" + search + "%' ) ";
                    sSQL2 += " where ( " + sSQLkode + " or " + sSQLnama + " ) ";
                }
                string sSQLSelect2 = "";
                sSQLSelect2 += "ORDER BY NAMA ASC, SUPP ASC ";
                sSQLSelect2 += "OFFSET " + Convert.ToString(pagenumber * 10) + " ROWS ";
                sSQLSelect2 += "FETCH NEXT 10 ROWS ONLY ";

                var listData = ErasoftDbContext.Database.SqlQuery<PromptBarangViewModel>(sSQLSelect + sSQL2 + sSQLSelect2).ToList();

                var totalCount = ErasoftDbContext.Database.SqlQuery<getTotalCount>(sSQLCount + sSQL2).Single();

                IPagedList<PromptBarangViewModel> pageOrders = new StaticPagedList<PromptBarangViewModel>(listData, pagenumber + 1, 10, totalCount.JUMLAH);
                return PartialView("TablePromptSupplierPartial", pageOrders);
            }
            catch (Exception ex)
            {
                return JsonErrorMessage("Prompt gagal");
            }
        }
        //end add by nurul 29/7/2019
        public ActionResult PromptGudang()
        {
            //CHANGE BY NURUL 29/7/2019
            //var listGudang = ErasoftDbContext.STF18.ToList();

            //return View("PromptGudang", listGudang);
            return View();
            //END ADD BY NURUL 29/7/2019
        }
        public ActionResult PromptKodeLR()
        {
            var listKodeLR = ErasoftDbContext.GLFRLA1.ToList();

            return View("PromptKodeLapRL", listKodeLR);
        }
        public ActionResult PromptKodeNeraca()
        {
            var listKodeNeraca = ErasoftDbContext.GLFNER1.ToList();

            return View("PromptKodeLapNeraca", listKodeNeraca);
        }
        public ActionResult PromptKodeRekening()
        {
            //ADD BY NURUL 29/7/2019
            //var listKodeRek = ErasoftDbContext.GLFREKs.Where(a => a.type.ToUpper() != "L").ToList();

            //return View("PromptKodeRek", listKodeRek);
            return View("PromptKodeRek");
            //END ADD BY NURUL 29/7/2019
        }

        //add by nurul 31/1/2019
        public ActionResult PromptBuyer()
        {
            //change by nurul 29/7/2019
            //var listBuyer = ErasoftDbContext.ARF01C.ToList();

            //return View("PromptBuyer", listBuyer);
            return View();
            //end change by nurul 29/7/2019
        }

    }
}