﻿using System;
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
    public class ReportController : Controller
    {
        public MoDbContext MoDbContext { get; set; }
        public ErasoftContext ErasoftDbContext { get; set; }

        public ReportController()
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


        [Route("reports/1")]
        public ActionResult MutasiPiutang()
        {
            return View();
        }

        [HttpPost]
        public string Preview1(ReportViewModel.Report1 data)
        {
            return string.Format("http://202.67.14.92:3535/MOReport/Report/Form/frmLaporanMutasiPiutangTanpaPosting.aspx?UserID={0}&From={1}&To={2}&CutOff={3}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromCust),
                Uri.EscapeDataString(data.ToCust),
                Uri.EscapeDataString(data.CutOffDate));
        }

        [Route("reports/2")]
        public ActionResult MutasiHutang()
        {
            return View();
        }

        [HttpPost]
        public string Preview2(ReportViewModel.Report2 data)
        {
            return string.Format("http://202.67.14.92:3535/MOReport/Report/Form/frmLaporanMutasiTanpaPosting.aspx?UserID={0}&From={1}&To={2}&CutOff={3}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromSupp),
                Uri.EscapeDataString(data.ToSupp),
                Uri.EscapeDataString(data.CutOffDate));
        }

        [Route("reports/3")]
        public ActionResult KartuPiutang()
        {
            return View();
        }

        [HttpPost]
        public string Preview3(ReportViewModel.Report3 data)
        {
            return string.Format("http://202.67.14.92:3535/MOReport/Report/Form/KartuPiutangtanpaposting.aspx?UserID={0}&From={1}&To={2}&FromMonth={3}&CutOff={4}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromCust),
                Uri.EscapeDataString(data.ToCust),
                Uri.EscapeDataString(data.FromMonth),
                Uri.EscapeDataString(data.CutOffDate));
        }

        [Route("reports/4")]
        public ActionResult KartuHutang()
        {
            return View();
        }

        [HttpPost]
        public string Preview4(ReportViewModel.Report4 data)
        {
            return string.Format("http://202.67.14.92:3535/MOReport/Report/Form/KartuHutang_Tanpa_Posting.aspx?UserID={0}&From={1}&To={2}&FromMonth={3}&CutOff={4}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromSupp),
                Uri.EscapeDataString(data.ToSupp),
                Uri.EscapeDataString(data.FromMonth),
                Uri.EscapeDataString(data.CutOffDate));
        }

        [Route("reports/5")]
        public ActionResult AnalisaPembelian()
        {
            return View();
        }

        [HttpPost]
        public string Preview5(ReportViewModel.Report5 data)
        {
            return string.Format("http://202.67.14.92:3535/MOReport/Report/Form/frm_LAnalisaPem.aspx?UserID={0}&FromSupp={1}&ToSupp={2}&FromBrg={3}&ToBrg={4}&DrTanggal={5}&SdTanggal={6}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromSupp),
                Uri.EscapeDataString(data.ToSupp),
                Uri.EscapeDataString(data.FromBrg),
                Uri.EscapeDataString(data.ToBrg),
                Uri.EscapeDataString(data.DrTanggal),
                Uri.EscapeDataString(data.SdTanggal));
        }

        [Route("reports/6")]
        public ActionResult AnalisaRugiLabaPenjualan()
        {
            return View();
        }

        [HttpPost]
        public string Preview6(ReportViewModel.Report6 data)
        {
            return string.Format("http://202.67.14.92:3535/MOReport/Report/Form/frm_LAnalisaRLPenj_SP.aspx?UserID={0}&FromCust={1}&ToCust={2}&FromBrg={3}&ToBrg={4}&DrTanggal={5}&SdTanggal={6}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromCust),
                Uri.EscapeDataString(data.ToCust),
                Uri.EscapeDataString(data.FromBrg),
                Uri.EscapeDataString(data.ToBrg),
                Uri.EscapeDataString(data.DrTanggal),
                Uri.EscapeDataString(data.SdTanggal));
        }

        [Route("reports/7")]
        public ActionResult KartuStok()
        {
            return View();
        }

        [HttpPost]
        public string Preview7(ReportViewModel.Report7 data)
        {
            return string.Format("http://202.67.14.92:3535/MOReport/Report/Form/FrmLapSTT09_TanpaPosting.aspx?iJenisForm=SETELAH_POSTING&UserID={0}&Gudang={1}&From={2}&To={3}&FromMonth={4}&CutOff={5}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.Gudang),
                Uri.EscapeDataString(data.FromBrg),
                Uri.EscapeDataString(data.ToBrg),
                Uri.EscapeDataString(data.FromMonth),
                Uri.EscapeDataString(data.CutOffDate));
        }

        [Route("reports/8")]
        public ActionResult MutasiStokBulanan()
        {
            return View();
        }

        [HttpPost]
        public string Preview8(ReportViewModel.Report8 data)
        {
            return string.Format("http://202.67.14.92:3535/MOReport/Report/Form/FrmLapSTT08_Drilldown_TanpaPosting2.aspx?UserID={0}&Gudang={1}&FromBrg={2}&ToBrg={3}&Tahun={4}&DrBulan={5}&SdBulan={6}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.Gudang),
                Uri.EscapeDataString(data.FromBrg),
                Uri.EscapeDataString(data.ToBrg),
                Uri.EscapeDataString(data.Tahun),
                Uri.EscapeDataString(data.DrBulan),
                Uri.EscapeDataString(data.SdBulan));
        }

        [Route("reports/9")]
        public ActionResult Neraca()
        {
            return View();
        }

        [HttpPost]
        public string Preview9(ReportViewModel.Report9 data)
        {
            return string.Format("http://202.67.14.92:3535/MOReport/Report/Form/frmCetakLaporanNeraca.aspx?UserID={0}&KdLap={1}&Tahun={2}&Bulan={3}&Print={4}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.KdLap),
                Uri.EscapeDataString(data.Tahun),
                Uri.EscapeDataString(data.Bulan),
                Uri.EscapeDataString(data.Print));
        }

        [Route("reports/10")]
        public ActionResult RugiLaba()
        {
            return View();
        }

        [HttpPost]
        public string Preview10(ReportViewModel.Report10 data)
        {
            return string.Format("http://202.67.14.92:3535/MOReport/Report/Form/frmCetakLapRugiLaba.aspx?UserID={0}&KdLap={1}&Bulan={2}&Print={3}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.KdLap),
                Uri.EscapeDataString(data.Bulan),
                Uri.EscapeDataString(data.Print));
        }

        [Route("reports/11")]
        public ActionResult CetakBukuBesar()
        {
            return View();
        }

        [HttpPost]
        public string Preview11(ReportViewModel.Report11 data)
        {
            return string.Format("http://202.67.14.92:3535/MOReport/Report/Form/frmCetakBukuBesar.aspx?UserID={0}&DrRek={1}&SdRek={2}&Type={3}&Print={4}&Posting={5}&Tahun={6}&DrBulan={7}&SdBulan={8}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.DrRek),
                Uri.EscapeDataString(data.SdRek),
                Uri.EscapeDataString(data.Type),
                Uri.EscapeDataString(data.Print),
                Uri.EscapeDataString(data.Posting),
                Uri.EscapeDataString(data.Tahun),
                Uri.EscapeDataString(data.DrBulan),
                Uri.EscapeDataString(data.SdBulan));
        }

        [Route("reports/12")]
        public ActionResult CetakRekapBukuBesar()
        {
            return View();
        }

        [HttpPost]
        public string Preview12(ReportViewModel.Report12 data)
        {
            return string.Format("http://202.67.14.92:3535/MOReport/Report/Form/frmCetakRekapBukuBesar.aspx?UserID={0}&DrRek={1}&SdRek={2}&Type={3}&Nol={4}&Posting={5}&Tahun={6}&DrBulan={7}&SdBulan={8}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.DrRek),
                Uri.EscapeDataString(data.SdRek),
                Uri.EscapeDataString(data.Type),
                Uri.EscapeDataString(data.Nol),
                Uri.EscapeDataString(data.Posting),
                Uri.EscapeDataString(data.Tahun),
                Uri.EscapeDataString(data.DrBulan),
                Uri.EscapeDataString(data.SdBulan));
        }

        [Route("reports/13")]
        public ActionResult ListFakturPenjualan()
        {
            return View();
        }

        [HttpPost]
        public string Preview13(ReportViewModel.Report13 data)
        {
            return string.Format("http://202.67.14.92:3535/MOReport/Report/Form/frm_rpt_FA.aspx?UserID={0}&FromCust={1}&ToCust={2}&DrTanggal={3}&SdTanggal={4}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromCust),
                Uri.EscapeDataString(data.ToCust),
                Uri.EscapeDataString(data.DrTanggal),
                Uri.EscapeDataString(data.SdTanggal));
        }

        [Route("reports/14")]
        public ActionResult ListInvPembelian()
        {
            return View();
        }

        [HttpPost]
        public string Preview14(ReportViewModel.Report14 data)
        {
            return string.Format("http://202.67.14.92:3535/MOReport/Report/Form/frm_rptinv_1.aspx?UserID={0}&FromSupp={1}&ToSupp={2}&DrTanggal={3}&SdTanggal={4}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.FromSupp),
                Uri.EscapeDataString(data.ToSupp),
                Uri.EscapeDataString(data.DrTanggal),
                Uri.EscapeDataString(data.SdTanggal));
        }

        [Route("reports/15")]
        public ActionResult KartuStokDenganHarga()
        {
            return View();
        }
        [HttpPost]
        public string Preview15(ReportViewModel.Report15 data)
        {
            return string.Format("http://202.67.14.92:3535/MOReport/Report/Form/FrmLapSTT09.aspx?iJenisForm=SETELAH_POSTING&UserID={0}&Gudang={1}&From={2}&To={3}&FromMonth={4}&CutOff={5}",
                Uri.EscapeDataString(data.UserId),
                Uri.EscapeDataString(data.Gudang),
                Uri.EscapeDataString(data.FromBrg),
                Uri.EscapeDataString(data.ToBrg),
                Uri.EscapeDataString(data.FromMonth),
                Uri.EscapeDataString(data.CutOffDate));
        }

        [Route("reports/16")]
        public ActionResult PosisiStok()
        {
            return View();
        }
        [HttpPost]
        public string Preview16(ReportViewModel.Report16 data)
        {
            return string.Format("http://202.67.14.92:3535/MOReport/Report/Form/frmLapSTT02C1_TanpaPosting.aspx?iSort=KODE_BARANG&iPeriode=HARI_INI&UserID={0}&CutOff={1}&PilihStok={2}&From={3}&To={4}&Gudang1={5}&Gudang2={6}&Gudang3={7}&Gudang4={8}&Gudang5={9}&Gudang6={10}&Gudang7={11}&Gudang8={12}&Gudang9={13}&Gudang10={14}",
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
        }

        public ActionResult PromptCustomer()
        {
            var listCust = ErasoftDbContext.ARF01.ToList();

            return View("PromptCustomer", listCust);
        }
        public ActionResult PromptSupplier()
        {
            var listSupp = ErasoftDbContext.APF01.ToList();

            return View("PromptSupplier", listSupp);
        }
        public ActionResult PromptBarang()
        {
            var listBrg = ErasoftDbContext.STF02.ToList();

            return View("PromptBarang", listBrg);
        }
        public ActionResult PromptGudang()
        {
            var listGudang = ErasoftDbContext.STF18.ToList();

            return View("PromptGudang", listGudang);
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
            var listKodeRek = ErasoftDbContext.GLFREKs.Where(a => a.type.ToUpper() != "L").ToList();

            return View("PromptKodeRek", listKodeRek);
        }

    }
}