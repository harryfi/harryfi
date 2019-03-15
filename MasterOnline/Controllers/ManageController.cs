using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Services.Protocols;
using System.Xml.Schema;
using EntityFramework.Extensions;
using System.Threading;
using System.Threading.Tasks;
using Erasoft.Function;

using MasterOnline.Models;
using MasterOnline.Services;
using MasterOnline.Utils;

using MasterOnline.ViewModels;
using PagedList;

//ADD BY NURUL 29/1/2019
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
//END ADD BY NURUL 29/1/2019 

namespace MasterOnline.Controllers
{
    [SessionCheck]

    public class ManageController : Controller
    {
        public MoDbContext MoDbContext { get; set; }
        public ErasoftContext ErasoftDbContext { get; set; }
        DatabaseSQL EDB;

        public ManageController()
        {
            MoDbContext = new MoDbContext();
            var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            if (sessionData?.Account != null)
            {
                if (sessionData.Account.UserId == "admin_manage")
                    ErasoftDbContext = new ErasoftContext();
                else
                    ErasoftDbContext = new ErasoftContext(sessionData.Account.DatabasePathErasoft);

                EDB = new DatabaseSQL(sessionData.Account.DatabasePathErasoft);

            }
            else
            {
                if (sessionData?.User != null)
                {
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                    ErasoftDbContext = new ErasoftContext(accFromUser.DatabasePathErasoft);

                    EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
                }
            }
        }

        [HttpGet]
        [Route("manage/keepsession")]
        public JsonResult KeepSessionAlive()
        {
            return new JsonResult { Data = "Success", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        protected override void Dispose(bool disposing)
        {
            MoDbContext.Dispose();
            ErasoftDbContext?.Dispose();
        }

        // Function Tambahan (START)

        int? ParseInt(string val)
        {
            int i;
            return int.TryParse(val, out i) ? (int?)i : null;
        }

        // Function Tambahan (END)

        // =============================================== Dashboard (START)

        [Route("manage/home")]
        public ActionResult Index()
        {
            var vm = new SubsViewModel()
            {
                ListSubs = MoDbContext.Subscription.ToList()
            };
            return View(vm);
        }

        public ActionResult DashboardPartial(string selDate)
        {
            var selectedDate = (selDate != "" ? DateTime.ParseExact(selDate, "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture) : DateTime.Today.Date);

            var selectedMonth = (selDate != "" ? DateTime.ParseExact(selDate, "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture).Month : DateTime.Today.Month);

            var vm = new DashboardViewModel()
            {
                ListPesanan = ErasoftDbContext.SOT01A.ToList(),
                ListPesananDetail = ErasoftDbContext.SOT01B.ToList(),
                ListFaktur = ErasoftDbContext.SIT01A.ToList(),
                ListFakturDetail = ErasoftDbContext.SIT01B.ToList(),
                //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListAkunMarketplace = ErasoftDbContext.ARF01.ToList(),
                ListMarket = MoDbContext.Marketplaces.ToList(),
                ListBarangUntukCekQty = ErasoftDbContext.STF08A.ToList(),
                ListStok = ErasoftDbContext.STT01B.ToList()
            };

            // Pesanan
            vm.JumlahPesananHariIni = vm.ListPesanan?.Where(p => p.TGL?.Date == selectedDate).Count();
            // change by nurul 12/10/2018   vm.NilaiPesananHariIni = vm.ListPesanan?.Where(p => p.TGL?.Date == selectedDate).Sum(p => p.BRUTO - p.NILAI_DISC);
            vm.NilaiPesananHariIni = vm.ListPesanan?.Where(p => p.TGL?.Date == selectedDate).Sum(p => p.NETTO);
            vm.JumlahPesananBulanIni = vm.ListPesanan?.Where(p => p.TGL?.Month == selectedMonth).Count();
            // change by nurul 12/10/2018   vm.NilaiPesananBulanIni = vm.ListPesanan?.Where(p => p.TGL?.Month == selectedMonth).Sum(p => p.BRUTO - p.NILAI_DISC);
            vm.NilaiPesananBulanIni = vm.ListPesanan?.Where(p => p.TGL?.Month == selectedMonth).Sum(p => p.NETTO);

            // Faktur
            vm.JumlahFakturHariIni = vm.ListFaktur?.Where(p => p.TGL == selectedDate && p.JENIS_FORM == "2").Count();
            // change by nurul 12/10/2018   vm.NilaiFakturHariIni = vm.ListFaktur?.Where(p => p.TGL == selectedDate && p.JENIS_FORM == "2").Sum(p => p.BRUTO - p.NILAI_DISC);
            vm.NilaiFakturHariIni = vm.ListFaktur?.Where(p => p.TGL == selectedDate && p.JENIS_FORM == "2").Sum(p => p.NETTO);
            vm.JumlahFakturBulanIni = vm.ListFaktur?.Where(p => p.TGL.Month == selectedMonth && p.JENIS_FORM == "2").Count();
            // change by nurul 12/10/2018   vm.NilaiFakturBulanIni = vm.ListFaktur?.Where(p => p.TGL.Month == selectedMonth && p.JENIS_FORM == "2").Sum(p => p.BRUTO - p.NILAI_DISC);
            vm.NilaiFakturBulanIni = vm.ListFaktur?.Where(p => p.TGL.Month == selectedMonth && p.JENIS_FORM == "2").Sum(p => p.NETTO);


            // Retur
            vm.JumlahReturHariIni = vm.ListFaktur?.Where(p => p.TGL == selectedDate && p.JENIS_FORM == "3").Count();
            // change by nurul 12/10/2018   vm.NilaiReturHariIni = vm.ListFaktur?.Where(p => p.TGL == selectedDate && p.JENIS_FORM == "3").Sum(p => p.BRUTO - p.NILAI_DISC);
            vm.NilaiReturHariIni = vm.ListFaktur?.Where(p => p.TGL == selectedDate && p.JENIS_FORM == "3").Sum(p => p.NETTO);
            vm.JumlahReturBulanIni = vm.ListFaktur?.Where(p => p.TGL.Month == selectedMonth && p.JENIS_FORM == "3").Count();
            // change by nurul 12/10/2018   vm.NilaiReturBulanIni = vm.ListFaktur?.Where(p => p.TGL.Month == selectedMonth && p.JENIS_FORM == "3").Sum(p => p.BRUTO - p.NILAI_DISC);
            vm.NilaiReturBulanIni = vm.ListFaktur?.Where(p => p.TGL.Month == selectedMonth && p.JENIS_FORM == "3").Sum(p => p.NETTO);


            if (vm.ListAkunMarketplace.Count > 0)
            {
                foreach (var marketplace in vm.ListAkunMarketplace)
                {
                    var idMarket = Convert.ToInt32(marketplace.NAMA);
                    var namaMarket = vm.ListMarket.Single(m => m.IdMarket == idMarket).NamaMarket;

                    var jumlahPesananToday = vm.ListPesanan?
                        .Where(p => p.CUST == marketplace.CUST && p.TGL == selectedDate).Count();
                    // change by nurul 12/10/2018   var nilaiPesananToday = $"Rp {String.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:N}", vm.ListPesanan?.Where(p => p.CUST == marketplace.CUST && p.TGL == selectedDate).Sum(p => p.BRUTO - p.NILAI_DISC))}";
                    var nilaiPesananToday = $"Rp {String.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:N}", vm.ListPesanan?.Where(p => p.CUST == marketplace.CUST && p.TGL == selectedDate).Sum(p => p.NETTO))}";


                    var jumlahPesananMonth = vm.ListPesanan?

                        .Where(p => p.CUST == marketplace.CUST && p.TGL?.Month == selectedMonth).Count();
                    // change by nurul 12/10/2018   var nilaiPesananMonth = $"Rp {String.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:N}", vm.ListPesanan?.Where(p => p.CUST == marketplace.CUST && p.TGL?.Month == selectedMonth).Sum(p => p.BRUTO - p.NILAI_DISC))}";
                    var nilaiPesananMonth = $"Rp {String.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:N}", vm.ListPesanan?.Where(p => p.CUST == marketplace.CUST && p.TGL?.Month == selectedMonth).Sum(p => p.NETTO))}";


                    vm.ListPesananPerMarketplace.Add(new PesananPerMarketplaceModel()
                    {
                        NamaMarket = $"{namaMarket} ({marketplace.PERSO})",
                        JumlahPesananHariIni = jumlahPesananToday.ToString(),
                        NilaiPesananHariIni = nilaiPesananToday,
                        JumlahPesananBulanIni = jumlahPesananMonth.ToString(),
                        NilaiPesananBulanIni = nilaiPesananMonth
                    });
                }
            }

            foreach (var barang in vm.ListBarang)
            {
                var listBarangTerpesan = vm.ListPesananDetail.Where(b => b.BRG == barang.BRG).ToList();

                if (listBarangTerpesan.Count > 0)
                {
                    var qtyBarang = listBarangTerpesan.Where(b => b.TGL_INPUT?.Month >= (selectedMonth - 3) &&
                                                                  b.TGL_INPUT?.Month <= selectedMonth).Sum(b => b.QTY);
                    vm.ListBarangLaku.Add(new PenjualanBarang
                    {
                        KodeBrg = barang.BRG,
                        NamaBrg = $"{barang.NAMA} {barang.NAMA2}",
                        Qty = qtyBarang,
                        Laku = true
                    });
                }
            }

            foreach (var barang in vm.ListBarang.Where(b => b.Tgl_Input?.Month >= (selectedMonth - 3) && b.Tgl_Input?.Month <= selectedMonth))
            {
                var barangTerpesan = vm.ListPesananDetail.FirstOrDefault(b => b.BRG == barang.BRG);
                var stokBarang = vm.ListStok.FirstOrDefault(b => b.Kobar == barang.BRG);

                if (barangTerpesan == null)
                {
                    vm.ListBarangTidakLaku.Add(new PenjualanBarang
                    {
                        KodeBrg = barang.BRG,
                        NamaBrg = $"{barang.NAMA} {barang.NAMA2}",
                        Qty = Convert.ToDouble(stokBarang?.Qty),
                        Laku = false
                    });
                }
            }

            return PartialView(vm);
        }

        // =============================================== Dashboard (END)

        // =============================================== Menu Manage (START)

        public async void GetPesanan(string connectionID, string username)
        {

            int bliAcc = 0;
            var kdBli = MoDbContext.Marketplaces.Single(m => m.NamaMarket.ToUpper() == "BLIBLI");
            var listBliShop = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdBli.IdMarket.ToString()).ToList();
            if (listBliShop.Count > 0)
            {
                bliAcc = 1;
                foreach (ARF01 tblCustomer in listBliShop)
                {
                    if (!string.IsNullOrEmpty(tblCustomer.Sort1_Cust))
                    {
                        var bliApi = new BlibliController();

                        BlibliController.BlibliAPIData iden = new BlibliController.BlibliAPIData
                        {
                            merchant_code = tblCustomer.Sort1_Cust,
                            API_client_password = tblCustomer.API_CLIENT_P,
                            API_client_username = tblCustomer.API_CLIENT_U,
                            API_secret_key = tblCustomer.API_KEY,
                            token = tblCustomer.TOKEN,
                            mta_username_email_merchant = tblCustomer.EMAIL,
                            mta_password_password_merchant = tblCustomer.PASSWORD,
                            idmarket = tblCustomer.RecNum.Value
                        };

                        await bliApi.GetOrderList(iden, BlibliController.StatusOrder.Paid, connectionID, tblCustomer.CUST, tblCustomer.PERSO);
                    }
                }
            }
        }
        public async System.Threading.Tasks.Task<ActionResult> RefreshPesananDibayarMarketplace()
        {
            //add by Tri call market place api getorder
            var connectionID = Guid.NewGuid().ToString();
            AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            string username = "";
            if (sessionData?.User != null)
            {
                var accId = MoDbContext.User.Single(u => u.Username == sessionData.User.Username).AccountId;
                username = MoDbContext.Account.Single(a => a.AccountId == accId).Username;
            }
            else
            {
                username = sessionData?.Account?.Username;
            }
            var Marketplaces = MoDbContext.Marketplaces.ToList();

            //remark by calvin 13 desember 2018, testing
            var kdBli = Marketplaces.Single(m => m.NamaMarket.ToUpper() == "BLIBLI");
            var listBliShop = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdBli.IdMarket.ToString()).ToList();
            if (listBliShop.Count > 0)
            {
                foreach (ARF01 tblCustomer in listBliShop)
                {
                    if (!string.IsNullOrEmpty(tblCustomer.Sort1_Cust))
                    {
                        var bliApi = new BlibliController();

                        BlibliController.BlibliAPIData iden = new BlibliController.BlibliAPIData
                        {
                            merchant_code = tblCustomer.Sort1_Cust,
                            API_client_password = tblCustomer.API_CLIENT_P,
                            API_client_username = tblCustomer.API_CLIENT_U,
                            API_secret_key = tblCustomer.API_KEY,
                            token = tblCustomer.TOKEN,
                            mta_username_email_merchant = tblCustomer.EMAIL,
                            mta_password_password_merchant = tblCustomer.PASSWORD,
                            idmarket = tblCustomer.RecNum.Value
                        };

                        await bliApi.GetOrderList(iden, BlibliController.StatusOrder.Paid, connectionID, tblCustomer.CUST, tblCustomer.PERSO);

                        //add by calvin 8 nov 2018, update status so di MO jika sudah ada order complete dari blibli
                        await bliApi.GetOrderList(iden, BlibliController.StatusOrder.Completed, connectionID, tblCustomer.CUST, tblCustomer.PERSO);
                        //end add by calvin 8 nov 2018
                    }
                }
            }
            var kdEL = Marketplaces.Single(m => m.NamaMarket.ToUpper() == "ELEVENIA");
            var listELShop = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdEL.IdMarket.ToString()).ToList();
            if (listELShop.Count > 0)
            {
                foreach (ARF01 tblCustomer in listELShop)
                {
                    var elApi = new EleveniaController();
                    await elApi.GetOrder(tblCustomer.API_KEY, EleveniaController.StatusOrder.Paid, connectionID, tblCustomer.CUST, tblCustomer.PERSO);

                    //add by calvin 8 nov 2018, update status so di MO jika sudah ada order complete dari elevenia
                    await elApi.GetOrder(tblCustomer.API_KEY, EleveniaController.StatusOrder.Completed, connectionID, tblCustomer.CUST, tblCustomer.PERSO);
                    await elApi.GetOrder(tblCustomer.API_KEY, EleveniaController.StatusOrder.ConfirmPurchase, connectionID, tblCustomer.CUST, tblCustomer.PERSO);
                    //end add by calvin 8 nov 2018
                }
            }
            var kdBL = Marketplaces.Single(m => m.NamaMarket.ToUpper() == "BUKALAPAK");
            var listBLShop = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdBL.IdMarket.ToString()).ToList();
            if (listBLShop.Count > 0)
            {
                foreach (ARF01 tblCustomer in listBLShop)
                {
                    var blApi = new BukaLapakController();
                    blApi.cekTransaksi(tblCustomer.CUST, tblCustomer.EMAIL, tblCustomer.API_KEY, tblCustomer.TOKEN, connectionID);
                }

            }

            var kdLzd = Marketplaces.Single(m => m.NamaMarket.ToUpper() == "LAZADA");
            var listLzdShop = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdLzd.IdMarket.ToString()).ToList();
            if (listLzdShop.Count > 0)
            {
                foreach (ARF01 tblCustomer in listLzdShop)
                {
                    var lzdApi = new LazadaController();
                    lzdApi.GetOrders(tblCustomer.CUST, tblCustomer.TOKEN, connectionID);
                }
            }
            //end remark by calvin 13 desember 2018, testing

            var kdTokped = Marketplaces.Single(m => m.NamaMarket.ToUpper() == "TOKOPEDIA");
            var listTokPed = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdTokped.IdMarket.ToString()).ToList();
            if (listTokPed.Count > 0)
            {
                foreach (ARF01 tblCustomer in listTokPed)
                {
                    if (!string.IsNullOrEmpty(tblCustomer.Sort1_Cust))
                    {
                        var tokopediaApi = new TokopediaController();

                        TokopediaController.TokopediaAPIData iden = new TokopediaController.TokopediaAPIData
                        {
                            merchant_code = tblCustomer.Sort1_Cust, //FSID
                            API_client_password = tblCustomer.API_CLIENT_P, //Client ID
                            API_client_username = tblCustomer.API_CLIENT_U, //Client Secret
                            API_secret_key = tblCustomer.API_KEY, //Shop ID 
                            token = tblCustomer.TOKEN,
                            idmarket = tblCustomer.RecNum.Value
                        };
                        //TokopediaController.TokopediaAPIData idenTest = new TokopediaController.TokopediaAPIData
                        //{
                        //    merchant_code = "13072", //FSID
                        //    API_client_username = "36bc3d7bcc13404c9e670a84f0c61676", //Client ID
                        //    API_client_password = "8a76adc52d144a9fa1ef4f96b59b7419", //Client Secret
                        //    API_secret_key = "2619296", //Shop ID 
                        //    token = "pmgdpFANTcC0PM9tVzrwmw"
                        //};
                        //await tokopediaApi.GetActiveItemList(iden, connectionID, tblCustomer.CUST, tblCustomer.PERSO, tblCustomer.RecNum ?? 0);
                        await tokopediaApi.GetOrderList(iden, TokopediaController.StatusOrder.Paid, connectionID, tblCustomer.CUST, tblCustomer.PERSO);
                        //await tokopediaApi.GetOrderList(idenTest, TokopediaController.StatusOrder.Paid, connectionID, tblCustomer.CUST, tblCustomer.PERSO);
                        //await tokopediaApi.GetCategoryTree(idenTest);
                        //await tokopediaApi.GetOrderList(iden, TokopediaController.StatusOrder.Completed, connectionID, tblCustomer.CUST, tblCustomer.PERSO);
                        //await tokopediaApi.GetOrderList(idenTest, TokopediaController.StatusOrder.Completed, connectionID, "", "");
                    }
                }
            }

            var kdShopee = Marketplaces.Single(m => m.NamaMarket.ToUpper() == "SHOPEE");
            var listShopeeShop = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdShopee.IdMarket.ToString()).ToList();
            if (listShopeeShop.Count > 0)
            {
                var shopeeApi = new ShopeeController();
                foreach (ARF01 tblCustomer in listShopeeShop)
                {
                    ShopeeController.ShopeeAPIData iden = new ShopeeController.ShopeeAPIData();
                    iden.merchant_code = tblCustomer.Sort1_Cust;
                    await shopeeApi.GetOrderByStatus(iden, ShopeeController.StatusOrder.READY_TO_SHIP, connectionID, tblCustomer.CUST, tblCustomer.PERSO, 0);
                }
            }

            //add by calvin 14 nov 2018, update qoh setelah get pesanan
            var TEMP_ALL_MP_ORDER_ITEMs = ErasoftDbContext.Database.SqlQuery<TEMP_ALL_MP_ORDER_ITEM>("SELECT * FROM TEMP_ALL_MP_ORDER_ITEM WHERE CONN_ID = '" + connectionID + "'").ToList();


            List<string> listBrg = new List<string>();
            foreach (var item in TEMP_ALL_MP_ORDER_ITEMs)
            {
                listBrg.Add(item.BRG);
            }
            updateStockMarketPlace(listBrg);
            ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_ALL_MP_ORDER_ITEM WHERE CONN_ID = '" + connectionID + "'");
            //end add by calvin 14 nov 2018, update qoh setelah get pesanan

            var vm = new PesananViewModel()
            {
                ListPesanan = ErasoftDbContext.SOT01A.AsNoTracking().Where(p => p.STATUS_TRANSAKSI == "01").ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = Marketplaces,
            };

            return PartialView("TablePesananSudahDibayarPartial", vm);
        }
        [Route("manage/order")]
        //public ActionResult Pesanan()
        public async System.Threading.Tasks.Task<ActionResult> Pesanan()
        {
            //add by Tri call market place api getorder
            var connectionID = Guid.NewGuid().ToString();
            AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            string username = "";
            if (sessionData?.User != null)
            {
                var accId = MoDbContext.User.Single(u => u.Username == sessionData.User.Username).AccountId;
                username = MoDbContext.Account.Single(a => a.AccountId == accId).Username;
            }
            else
            {
                username = sessionData?.Account?.Username;
            }
            var Marketplaces = MoDbContext.Marketplaces.ToList();
            var List_ARF01 = ErasoftDbContext.ARF01.ToList();
            //remark by calvin 13 desember 2018, testing
            var kdBli = Marketplaces.Single(m => m.NamaMarket.ToUpper() == "BLIBLI");
            var listBliShop = List_ARF01.Where(m => m.NAMA == kdBli.IdMarket.ToString()).ToList();
            if (listBliShop.Count > 0)
            {
                foreach (ARF01 tblCustomer in listBliShop)
                {
                    if (!string.IsNullOrEmpty(tblCustomer.Sort1_Cust))
                    {
                        var bliApi = new BlibliController();

                        BlibliController.BlibliAPIData iden = new BlibliController.BlibliAPIData
                        {
                            merchant_code = tblCustomer.Sort1_Cust,
                            API_client_password = tblCustomer.API_CLIENT_P,
                            API_client_username = tblCustomer.API_CLIENT_U,
                            API_secret_key = tblCustomer.API_KEY,
                            token = tblCustomer.TOKEN,
                            mta_username_email_merchant = tblCustomer.EMAIL,
                            mta_password_password_merchant = tblCustomer.PASSWORD,
                            idmarket = tblCustomer.RecNum.Value
                        };

                        await bliApi.GetOrderList(iden, BlibliController.StatusOrder.Paid, connectionID, tblCustomer.CUST, tblCustomer.PERSO);

                        //add by calvin 8 nov 2018, update status so di MO jika sudah ada order complete dari blibli
                        await bliApi.GetOrderList(iden, BlibliController.StatusOrder.Completed, connectionID, tblCustomer.CUST, tblCustomer.PERSO);
                        //end add by calvin 8 nov 2018
                    }
                }
            }
            var kdEL = Marketplaces.Single(m => m.NamaMarket.ToUpper() == "ELEVENIA");
            var listELShop = List_ARF01.Where(m => m.NAMA == kdEL.IdMarket.ToString()).ToList();
            if (listELShop.Count > 0)
            {
                foreach (ARF01 tblCustomer in listELShop)
                {
                    var elApi = new EleveniaController();
                    await elApi.GetOrder(tblCustomer.API_KEY, EleveniaController.StatusOrder.Paid, connectionID, tblCustomer.CUST, tblCustomer.PERSO);

                    //add by calvin 8 nov 2018, update status so di MO jika sudah ada order complete dari elevenia
                    await elApi.GetOrder(tblCustomer.API_KEY, EleveniaController.StatusOrder.Completed, connectionID, tblCustomer.CUST, tblCustomer.PERSO);
                    await elApi.GetOrder(tblCustomer.API_KEY, EleveniaController.StatusOrder.ConfirmPurchase, connectionID, tblCustomer.CUST, tblCustomer.PERSO);
                    //end add by calvin 8 nov 2018
                }
            }
            var kdBL = Marketplaces.Single(m => m.NamaMarket.ToUpper() == "BUKALAPAK");
            var listBLShop = List_ARF01.Where(m => m.NAMA == kdBL.IdMarket.ToString()).ToList();
            if (listBLShop.Count > 0)
            {
                foreach (ARF01 tblCustomer in listBLShop)
                {
                    var blApi = new BukaLapakController();
                    if (!string.IsNullOrEmpty(tblCustomer.TOKEN))
                        blApi.cekTransaksi(tblCustomer.CUST, tblCustomer.EMAIL, tblCustomer.API_KEY, tblCustomer.TOKEN, connectionID);
                }

            }

            var kdLzd = Marketplaces.Single(m => m.NamaMarket.ToUpper() == "LAZADA");
            var listLzdShop = List_ARF01.Where(m => m.NAMA == kdLzd.IdMarket.ToString()).ToList();
            if (listLzdShop.Count > 0)
            {
                foreach (ARF01 tblCustomer in listLzdShop)
                {
                    var lzdApi = new LazadaController();
                    if (!string.IsNullOrEmpty(tblCustomer.TOKEN))
                        lzdApi.GetOrders(tblCustomer.CUST, tblCustomer.TOKEN, connectionID);
                }
            }
            //end remark by calvin 13 desember 2018, testing

            var kdTokped = Marketplaces.Single(m => m.NamaMarket.ToUpper() == "TOKOPEDIA");
            var listTokPed = List_ARF01.Where(m => m.NAMA == kdTokped.IdMarket.ToString()).ToList();
            if (listTokPed.Count > 0)
            {
                foreach (ARF01 tblCustomer in listTokPed)
                {
                    if (!string.IsNullOrEmpty(tblCustomer.Sort1_Cust))
                    {
                        var tokopediaApi = new TokopediaController();

                        TokopediaController.TokopediaAPIData iden = new TokopediaController.TokopediaAPIData
                        {
                            merchant_code = tblCustomer.Sort1_Cust, //FSID
                            API_client_password = tblCustomer.API_CLIENT_P, //Client ID
                            API_client_username = tblCustomer.API_CLIENT_U, //Client Secret
                            API_secret_key = tblCustomer.API_KEY, //Shop ID 
                            token = tblCustomer.TOKEN,
                            idmarket = tblCustomer.RecNum.Value
                        };
                        //TokopediaController.TokopediaAPIData idenTest = new TokopediaController.TokopediaAPIData
                        //{
                        //    merchant_code = "13072", //FSID
                        //    API_client_username = "36bc3d7bcc13404c9e670a84f0c61676", //Client ID
                        //    API_client_password = "8a76adc52d144a9fa1ef4f96b59b7419", //Client Secret
                        //    API_secret_key = "2619296", //Shop ID 
                        //    token = "pmgdpFANTcC0PM9tVzrwmw"
                        //};
                        //await tokopediaApi.GetActiveItemList(iden, connectionID, tblCustomer.CUST, tblCustomer.PERSO, tblCustomer.RecNum ?? 0);
                        await tokopediaApi.GetOrderList(iden, TokopediaController.StatusOrder.Paid, connectionID, tblCustomer.CUST, tblCustomer.PERSO);
                        //await tokopediaApi.GetOrderList(idenTest, TokopediaController.StatusOrder.Paid, connectionID, tblCustomer.CUST, tblCustomer.PERSO);
                        //await tokopediaApi.GetCategoryTree(idenTest);
                        //await tokopediaApi.GetOrderList(iden, TokopediaController.StatusOrder.Completed, connectionID, tblCustomer.CUST, tblCustomer.PERSO);
                        //await tokopediaApi.GetOrderList(idenTest, TokopediaController.StatusOrder.Completed, connectionID, "", "");
                    }
                }
            }

            var kdShopee = Marketplaces.Single(m => m.NamaMarket.ToUpper() == "SHOPEE");
            var listShopeeShop = List_ARF01.Where(m => m.NAMA == kdShopee.IdMarket.ToString()).ToList();
            if (listShopeeShop.Count > 0)
            {
                var shopeeApi = new ShopeeController();
                foreach (ARF01 tblCustomer in listShopeeShop)
                {
                    ShopeeController.ShopeeAPIData iden = new ShopeeController.ShopeeAPIData();
                    iden.merchant_code = tblCustomer.Sort1_Cust;
                    await shopeeApi.GetOrderByStatus(iden, ShopeeController.StatusOrder.READY_TO_SHIP, connectionID, tblCustomer.CUST, tblCustomer.PERSO, 0);
                }
            }

            //add by calvin 14 nov 2018, update qoh setelah get pesanan
            var TEMP_ALL_MP_ORDER_ITEMs = ErasoftDbContext.Database.SqlQuery<TEMP_ALL_MP_ORDER_ITEM>("SELECT * FROM TEMP_ALL_MP_ORDER_ITEM WHERE CONN_ID = '" + connectionID + "'").ToList();


            List<string> listBrg = new List<string>();
            foreach (var item in TEMP_ALL_MP_ORDER_ITEMs)
            {
                listBrg.Add(item.BRG);
            }
            updateStockMarketPlace(listBrg);
            ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_ALL_MP_ORDER_ITEM WHERE CONN_ID = '" + connectionID + "'");
            //end add by calvin 14 nov 2018, update qoh setelah get pesanan

            var vm = new PesananViewModel()
            {
                //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = List_ARF01,
                ListMarketplace = Marketplaces,
                ListSubs = MoDbContext.Subscription.ToList(),
                //add by nurul 26/9/2018
                //ListBarangMarket = ErasoftDbContext.STF02H.ToList()
                //end add 
            };

            return View(vm);
        }

        [Route("manage/penjualan/faktur")]
        public ActionResult Faktur()
        {
            var vm = new FakturViewModel()
            {
                ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2").OrderByDescending(f => f.TGL).ToList(),
                //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListPesanan = ErasoftDbContext.SOT01A.ToList(),
                ListNFaktur = ErasoftDbContext.ART03B.ToList(),
                ListSubs = MoDbContext.Subscription.ToList(),
                ListImportFaktur = ErasoftDbContext.LOG_IMPORT_FAKTUR.Where(a => 0 == 1).ToList()
            };

            return View(vm);
        }

        [Route("manage/penjualan/retur")]
        public ActionResult ReturFaktur()
        {
            var vm = new FakturViewModel()
            {
                ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "3").OrderByDescending(f => f.TGL).ToList(),
                //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList()
            };

            return View(vm);
        }

        [Route("manage/pembelian/invoice")]
        public ActionResult Invoice()
        {
            var vm = new InvoiceViewModel()
            {
                ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "1").OrderByDescending(f => f.TGL).ToList(),
                //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListSubs = MoDbContext.Subscription.ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListNInvoice = ErasoftDbContext.APT03B.ToList()
            };

            return View(vm);
        }

        [Route("manage/pembelian/retur")]
        public ActionResult ReturInvoice()
        {
            var vm = new InvoiceViewModel()
            {
                ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "2").OrderByDescending(f => f.TGL).ToList(),
                //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList()
            };

            return View(vm);
        }

        [Route("manage/akunting/jurnal")]
        public ActionResult Jurnal()
        {
            var vm = new JurnalViewModel()
            {
                ListJurnal = ErasoftDbContext.GLFTRAN1.ToList(),
                ListRekening = ErasoftDbContext.GLFREKs.ToList(),
                ListJurnalDetail = ErasoftDbContext.GLFTRAN2.ToList()
            };

            return View(vm);
        }

        [Route("manage/bantuan")]
        public ActionResult Bantuan()
        {
            return View();
        }

        [Route("manage/master/barang")]
        public ActionResult Barang()
        {
            var barangVm = new BarangViewModel()
            {
                //ListStf02S = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                //ListStf02S = ErasoftDbContext.STF02.Where(a => a.SUP == "").ToList(),

                //ingat ganti saat publish, by calvin
                //ListStf02S = ErasoftDbContext.STF02.Where(p => (p.PART == null ? "" : p.PART) == "" && (p.BRG == "01.CMO00.00" || p.BRG == "01.LIP00.00" || p.BRG == "JPTTEST2")).ToList(),
                ListStf02S = ErasoftDbContext.STF02.Where(p => (p.PART == null ? "" : p.PART) == "").ToList(),

                ListMarket = ErasoftDbContext.ARF01.OrderBy(p => p.RecNum).ToList(),
                ListHargaJualPermarketView = ErasoftDbContext.STF02H.Where(p => 0 == 1).OrderBy(p => p.IDMARKET).ToList(),
                //ListCategoryBlibli = MoDbContext.CategoryBlibli.Where(p => string.IsNullOrEmpty(p.PARENT_CODE)).ToList(),
                DataUsaha = ErasoftDbContext.SIFSYS.Single(p => p.BLN == 1)
            };

            return View(barangVm);
        }

        ////add by calvin 21 desember 2018, shopee 
        //public ActionResult MenuBarang()
        //{
        //    var barangVm = new BarangViewModel()
        //    {
        //        ListStf02S = ErasoftDbContext.STF02.ToList(),
        //        ListMarket = ErasoftDbContext.ARF01.OrderBy(p => p.RecNum).ToList(),
        //        ListHargaJualPermarketView = ErasoftDbContext.STF02H.OrderBy(p => p.IDMARKET).ToList(),
        //        //ListCategoryBlibli = MoDbContext.CategoryBlibli.Where(p => string.IsNullOrEmpty(p.PARENT_CODE)).ToList(),
        //        DataUsaha = ErasoftDbContext.SIFSYS.Single(p => p.BLN == 1),
        //        StatusLog = ErasoftDbContext.Database.SqlQuery<API_LOG_MARKETPLACE_PER_ITEM>("SELECT * FROM API_LOG_MARKETPLACE_PER_ITEM WHERE 0 = 1").ToList(),
        //        Stok = ErasoftDbContext.Database.SqlQuery<QOH_QOO_ALL_ITEM>("SELECT * FROM [QOH_QOO_ALL_ITEM]").ToList()
        //    };

        //    return View(barangVm);
        //}

        //add by nurul 29/8/2018
        [Route("manage/master/Menubarang")]
        public ActionResult MenuBarang()
        {
            var barangVm = new BarangViewModel()
            {
                //change by nurul 18/1/2019 -- ListStf02S = ErasoftDbContext.STF02.ToList(),
                ListStf02S = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListMarket = ErasoftDbContext.ARF01.OrderBy(p => p.RecNum).ToList(),
                ListHargaJualPermarketView = ErasoftDbContext.STF02H.Where(p => 0 == 1).OrderBy(p => p.IDMARKET).ToList(),
                //ListCategoryBlibli = MoDbContext.CategoryBlibli.Where(p => string.IsNullOrEmpty(p.PARENT_CODE)).ToList(),
                DataUsaha = ErasoftDbContext.SIFSYS.Single(p => p.BLN == 1),
                StatusLog = ErasoftDbContext.Database.SqlQuery<API_LOG_MARKETPLACE_PER_ITEM>("SELECT * FROM API_LOG_MARKETPLACE_PER_ITEM WHERE 0 = 1").ToList(),
                Stok = ErasoftDbContext.Database.SqlQuery<QOH_QOO_ALL_ITEM>("SELECT * FROM [QOH_QOO_ALL_ITEM]").ToList()
            };

            return View(barangVm);
        }
        //end add by nurul

        //add by calvin 4 september 2018
        [Route("manage/master/marketplacelog")]
        public ActionResult MarketPlaceLog()
        {
            var barangVm = new BarangViewModel()
            {
                ListMarket = ErasoftDbContext.ARF01.OrderBy(p => p.RecNum).ToList(),
                ListStf02S = ErasoftDbContext.STF02.ToList(),
                StatusLog = ErasoftDbContext.Database.SqlQuery<API_LOG_MARKETPLACE_PER_ITEM>("SELECT * FROM API_LOG_MARKETPLACE_PER_ITEM").ToList()
            };

            return View(barangVm);
        }
        //end add by calvin

        [Route("manage/ganti-password")]
        public ActionResult GantiPassword()
        {
            return View();
        }

        [Route("manage/security/user")]
        public ActionResult SecUserMenu(int? userId)
        {
            var secuserVm = new SecurityUserViewModel()
            {
                User = MoDbContext.User.SingleOrDefault(u => u.UserId == userId),
                ListForms = MoDbContext.FormMoses.Where(f => f.Show).ToList(),
                ListSec = MoDbContext.SecUser.Where(s => s.UserId == userId).ToList(),
                ListUser = MoDbContext.User.ToList(),
            };

            Session["UserId"] = secuserVm.User?.UserId;

            return View(secuserVm);
        }

        [Route("manage/master/kategori-barang")]
        public ActionResult KategoriBarang(int? page = 1, string search = "")
        {
            var kategoriVm = new KategoriBarangViewModel()
            {
                ListKategori = ErasoftDbContext.STF02E.Where(k => k.LEVEL == "1" && (k.KET.Contains(search) || k.KODE.Contains(search))).OrderByDescending(k => k.RecNum).ToList()
            };

            ViewData["currentPage"] = page;
            ViewData["searchParam"] = search;

            return View(kategoriVm);
        }

        [Route("manage/master/merk-barang")]
        public ActionResult MerkBarang(int? page = 1, string search = "")
        {
            var merkVm = new MerkBarangViewModel()
            {
                ListMerk = ErasoftDbContext.STF02E.Where(m => m.LEVEL == "2" && (m.KET.Contains(search) || m.KODE.Contains(search))).OrderByDescending(m => m.RecNum).ToList()
            };

            ViewData["searchParam"] = search;
            ViewData["currentPage"] = page;

            return View(merkVm);
        }

        [Route("manage/master/marketplace")]
        public ActionResult Pelanggan()
        {
            var custVm = new CustomerViewModel()
            {
                ListCustomer = ErasoftDbContext.ARF01.ToList(),
                ListSubs = MoDbContext.Subscription.ToList()
            };

            return View(custVm);
        }

        [Route("manage/master/pembeli")]
        public ActionResult Buyer()
        {
            var vm = new BuyerViewModel()
            {
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList()
            };

            return View(vm);
        }

        //public ActionResult BuyerPopup()
        //{
        //    return View();
        //}
        public ActionResult BuyerPopUp1()
        {
            var vm = new BuyerViewModel()
            {
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList()
            };

            return View(vm);
        }

        // =============================================== Menu Manage (END)

        // =============================================== Bagian Pembeli (START)

        [HttpPost]
        public ActionResult SavePembeli(BuyerViewModel dataBuyer)
        {
            if (!ModelState.IsValid)
            {
                dataBuyer.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(dataBuyer, JsonRequestBehavior.AllowGet);
            }

            if (dataBuyer.Pembeli.RecNum == null)
            {
                var listPembeli = ErasoftDbContext.ARF01C.OrderBy(m => m.RecNum).ToList();
                var noPembeli = "";

                if (listPembeli.Count == 0)
                {
                    noPembeli = "000001";
                    ErasoftDbContext.Database.ExecuteSqlCommand("DBCC CHECKIDENT (ARF01C, RESEED, 0)");
                }
                else
                {
                    var lastRecNum = listPembeli.Last().RecNum;
                    lastRecNum++;

                    noPembeli = lastRecNum.ToString().PadLeft(6, '0');
                }

                dataBuyer.Pembeli.BUYER_CODE = noPembeli;
                ErasoftDbContext.ARF01C.Add(dataBuyer.Pembeli);
            }
            else
            {
                var buyerInDb = ErasoftDbContext.ARF01C.Single(c => c.RecNum == dataBuyer.Pembeli.RecNum);

                buyerInDb.NAMA = dataBuyer.Pembeli.NAMA;
                buyerInDb.AL = dataBuyer.Pembeli.AL;
                buyerInDb.KODEPROV = dataBuyer.Pembeli.KODEPROV;
                buyerInDb.KODEKABKOT = dataBuyer.Pembeli.KODEKABKOT;
                buyerInDb.KODEPOS = dataBuyer.Pembeli.KODEPOS;
                buyerInDb.PERSO = dataBuyer.Pembeli.PERSO;
                buyerInDb.EMAIL = dataBuyer.Pembeli.EMAIL;
                buyerInDb.TLP = dataBuyer.Pembeli.TLP;
            }

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            var partialVm = new BuyerViewModel()
            {
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ThenByDescending(x => x.TGL_INPUT).ToList()
            };

            return PartialView("TableBuyerPartial", partialVm);
        }
        //add by nurul 5/12/2018
        [HttpPost]
        public ActionResult SaveBuyerPopUp(BuyerViewModel dataBuyer)
        {
            if (!ModelState.IsValid)
            {
                //dataBuyer.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                //return Json(dataBuyer, JsonRequestBehavior.AllowGet);
                return View("BuyerPopup1", dataBuyer);
            }

            if (dataBuyer.Pembeli.RecNum == null)
            {
                var listPembeli = ErasoftDbContext.ARF01C.OrderBy(m => m.RecNum).ToList();
                var noPembeli = "";

                if (listPembeli.Count == 0)
                {
                    noPembeli = "000001";
                    ErasoftDbContext.Database.ExecuteSqlCommand("DBCC CHECKIDENT (ARF01C, RESEED, 0)");
                }
                else
                {
                    var lastRecNum = listPembeli.Last().RecNum;
                    lastRecNum++;

                    noPembeli = lastRecNum.ToString().PadLeft(6, '0');
                }

                dataBuyer.Pembeli.BUYER_CODE = noPembeli;
                ErasoftDbContext.ARF01C.Add(dataBuyer.Pembeli);
            }
            else
            {
                var buyerInDb = ErasoftDbContext.ARF01C.Single(c => c.RecNum == dataBuyer.Pembeli.RecNum);

                buyerInDb.NAMA = dataBuyer.Pembeli.NAMA;
                buyerInDb.AL = dataBuyer.Pembeli.AL;
                buyerInDb.KODEPROV = dataBuyer.Pembeli.KODEPROV;
                buyerInDb.KODEKABKOT = dataBuyer.Pembeli.KODEKABKOT;
                buyerInDb.KODEPOS = dataBuyer.Pembeli.KODEPOS;
                buyerInDb.PERSO = dataBuyer.Pembeli.PERSO;
                buyerInDb.EMAIL = dataBuyer.Pembeli.EMAIL;
                buyerInDb.TLP = dataBuyer.Pembeli.TLP;
            }

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            var partialVm = new BuyerViewModel()
            {
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ThenByDescending(x => x.TGL_INPUT).ToList()
            };

            return PartialView("TableBuyerPopUp", partialVm);
        }
        //end add

        [HttpPost]
        public ActionResult SavePembeliPopup(BuyerViewModel dataBuyer)
        {
            if (!ModelState.IsValid)
            {
                return View("BuyerPopup", dataBuyer);
            }

            if (dataBuyer.Pembeli.RecNum == null)
            {
                ErasoftDbContext.ARF01C.Add(dataBuyer.Pembeli);
            }
            else
            {
                var buyerInDb = ErasoftDbContext.ARF01C.Single(c => c.BUYER_CODE == dataBuyer.Pembeli.BUYER_CODE);

                buyerInDb.NAMA = dataBuyer.Pembeli.NAMA;
                buyerInDb.AL = dataBuyer.Pembeli.AL;
                buyerInDb.KODEPROV = dataBuyer.Pembeli.KODEPROV;
                buyerInDb.KODEKABKOT = dataBuyer.Pembeli.KODEKABKOT;
                buyerInDb.KODEPOS = dataBuyer.Pembeli.KODEPOS;
                buyerInDb.PERSO = dataBuyer.Pembeli.PERSO;
                buyerInDb.EMAIL = dataBuyer.Pembeli.EMAIL;
                buyerInDb.TLP = dataBuyer.Pembeli.TLP;
            }

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            return new EmptyResult();
        }

        public ActionResult EditPembeli(int recNum)
        {
            try
            {
                var buyerVm = new BuyerViewModel()
                {
                    Pembeli = ErasoftDbContext.ARF01C.Single(c => c.RecNum == recNum),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList()
                };

                return Json(buyerVm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult EditPembeliPopup(string kodePembeli)
        {
            try
            {
                var buyerVm = new BuyerViewModel()
                {
                    Pembeli = ErasoftDbContext.ARF01C.SingleOrDefault(c => c.BUYER_CODE == kodePembeli),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList()
                };

                ViewData["Editing"] = 1;

                return View("BuyerPopup", buyerVm);
            }
            catch (Exception e)
            {
                return JsonErrorMessage(e.Message);
            }
        }

        public ActionResult DeletePembeli(int buyerId)
        {
            try
            {
                var buyerInDb = ErasoftDbContext.ARF01C.Single(c => c.RecNum == buyerId);
                //ADD BY NURUL 30/7/2018
                var vmError = new StokViewModel() { };

                var cekFaktur = ErasoftDbContext.SIT01A.Count(k => k.PEMESAN == buyerInDb.BUYER_CODE);
                var cekPesanan = ErasoftDbContext.SOT01A.Count(k => k.PEMESAN == buyerInDb.BUYER_CODE);

                if (cekFaktur > 0 || cekPesanan > 0)
                {
                    vmError.Errors.Add("Pembeli sudah dipakai di transaksi !");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
                //END ADD                                
                ErasoftDbContext.ARF01C.Remove(buyerInDb);
                ErasoftDbContext.SaveChanges();

                var partialVm = new BuyerViewModel()
                {
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ThenByDescending(x => x.TGL_INPUT).ToList()
                };

                return PartialView("TableBuyerPartial", partialVm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        // =============================================== Bagian Pembeli (END)

        // =============================================== Bagian Customer (START)

        [HttpGet]
        //change by nurul 21/2/2019
        //public ActionResult CekJumlahMarketplace(string uname)
        public ActionResult CekJumlahMarketplace(long accId)
        {
            var jumlahAkunMarketplace = ErasoftDbContext.ARF01
                .GroupBy(m => m.NAMA)
                .Select(g => new
                {
                    NamaMarket = g.FirstOrDefault().NAMA,
                    Jumlah = g.Select(o => o.NAMA).Distinct().Count()
                });

            var accInDb = MoDbContext.Account.FirstOrDefault(a => a.AccountId == accId);

            if (accInDb == null)
            {
                var accIdByUser = MoDbContext.User.FirstOrDefault(u => u.AccountId == accId)?.AccountId;
                accInDb = MoDbContext.Account.FirstOrDefault(a => a.AccountId == accIdByUser);
            }

            var accSubs = MoDbContext.Subscription.FirstOrDefault(s => s.KODE == accInDb.KODE_SUBSCRIPTION);
            var jumlahSemuaAkun = 0;
            var namaMarketTerpakai = new List<int>();

            foreach (var market in jumlahAkunMarketplace)
            {
                namaMarketTerpakai.Add(Convert.ToInt32(market.NamaMarket));
                jumlahSemuaAkun += market.Jumlah;
            }

            var valSubs = new ValidasiSubs()
            {
                JumlahMarketplace = jumlahSemuaAkun,
                JumlahMarketplaceMax = accSubs?.JUMLAH_MP,
                ListNamaMarketTerpakai = namaMarketTerpakai,
                //add by nurul 12/2/2019
                SudahSampaiBatasTanggal = (accInDb?.TGL_SUBSCRIPTION <= DateTime.Today.Date)
                //end add by nurul 12/2/2019
            };

            return Json(valSubs, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<string> GetCategoryBlibli()
        {
            var idmarket = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BLIBLI").IdMarket.ToString();
            var custInDb = ErasoftDbContext.ARF01.Where(c => c.NAMA == idmarket).ToList();
            foreach (var customer in custInDb)
            {
                #region BLIBLI get token
                if (!string.IsNullOrEmpty(customer.API_CLIENT_P) && !string.IsNullOrEmpty(customer.API_CLIENT_U))
                {
                    var BliApi = new BlibliController();
                    BlibliController.BlibliAPIData data = new BlibliController.BlibliAPIData()
                    {
                        API_client_username = customer.API_CLIENT_U,
                        API_client_password = customer.API_CLIENT_P,
                        API_secret_key = customer.API_KEY,
                        mta_username_email_merchant = customer.EMAIL,
                        mta_password_password_merchant = customer.PASSWORD,
                        merchant_code = customer.Sort1_Cust,
                        token = customer.TOKEN,
                        idmarket = customer.RecNum.Value
                    };
                    await BliApi.GetCategoryTree(data);
                    //BliApi.GetCategoryTree(data);
                }
                #endregion
            }
            return "";
        }
        [HttpGet]
        public async System.Threading.Tasks.Task<string> GetMasterCategoryElevenia()
        {
            var idmarket = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "ELEVENIA").IdMarket.ToString();
            var listELShop = ErasoftDbContext.ARF01.Where(m => m.NAMA == idmarket).ToList();
            if (listELShop.Count > 0)
            {
                var elApi = new EleveniaController();
                foreach (ARF01 tblCustomer in listELShop)
                {
                    if (Convert.ToString(tblCustomer.API_KEY) != "")
                    {
                        await elApi.GetCategoryElevenia(Convert.ToString(tblCustomer.API_KEY));
                        break;
                    }
                }
            }
            return "";
        }
        [HttpGet]
        public async System.Threading.Tasks.Task<string> GetMasterAttributeElevenia()
        {
            var idmarket = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "ELEVENIA").IdMarket.ToString();
            var listELShop = ErasoftDbContext.ARF01.Where(m => m.NAMA == idmarket).ToList();
            if (listELShop.Count > 0)
            {
                var elApi = new EleveniaController();
                foreach (ARF01 tblCustomer in listELShop)
                {
                    if (Convert.ToString(tblCustomer.API_KEY) != "")
                    {
                        await elApi.GetAttribute(Convert.ToString(tblCustomer.API_KEY));
                        break;
                    }
                }
            }
            return "";
        }


        [HttpPost]
        public ActionResult SaveCustomer(CustomerViewModel customer)
        {
            if (!ModelState.IsValid)
            {
                customer.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(customer, JsonRequestBehavior.AllowGet);
            }
            ////add by nurul 15/8/2018
            //if (customer.Customers.NAMA.Equals(MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "TOKOPEDIA").IdMarket.ToString()))
            //{
            //    customer.Errors.Add("Akun anda harus official store di Tokopedia. Silahkan hubungi kami apabila anda sudah official store!");
            //    return Json(customer, JsonRequestBehavior.AllowGet);
            //}
            ////end add
            string kdCustomer = "";
            if (customer.Customers.RecNum == null)
            {
                var listCustomer = ErasoftDbContext.ARF01.ToList();
                var noCust = "";

                if (listCustomer.Count == 0)
                {
                    noCust = "000001";
                    ErasoftDbContext.Database.ExecuteSqlCommand("DBCC CHECKIDENT (ARF01, RESEED, 0)");
                }
                else
                {
                    var lastRecNum = listCustomer.Last().RecNum;
                    lastRecNum++;

                    noCust = lastRecNum.ToString().PadLeft(6, '0');
                }
                kdCustomer = noCust;
                customer.Customers.CUST = noCust;
                //add by Tri, not null hidden field > blank
                //customer.Customers.AL = "";
                //customer.Customers.KODEKABKOT = "";
                //customer.Customers.KODEPOS = "";
                //customer.Customers.KODEPROV = "";
                //customer.Customers.TLP = "";
                //end add by Tri, not null hidden field > blank

                //add by nurul 14/1/2019 'validasi email dan marketplace sama 
                var vmError = new CustomerViewModel() { };

                var cekEmailMP = ErasoftDbContext.ARF01.Where(a => a.NAMA == customer.Customers.NAMA && a.EMAIL == customer.Customers.EMAIL).ToList();
                int nm = Convert.ToInt32(customer.Customers.NAMA);
                var getMP = MoDbContext.Marketplaces.SingleOrDefault(a => a.IdMarket == nm).NamaMarket;
                if (cekEmailMP.Count > 0)
                {
                    vmError.Errors.Add("Email sudah digunakan untuk Marketplace ( " + getMP + " ) !");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
                //end add by nurul 14/1/2019

                ErasoftDbContext.ARF01.Add(customer.Customers);
                ErasoftDbContext.SaveChanges();

                var listBarang = ErasoftDbContext.STF02.ToList();
                var cust = ErasoftDbContext.ARF01.Single(c => c.CUST == noCust);

                foreach (var barang in listBarang)
                {
                    var dataHarga = new STF02H()
                    {
                        BRG = barang.BRG,
                        IDMARKET = Convert.ToInt32(cust.RecNum),
                        AKUNMARKET = cust.PERSO,
                        HJUAL = 0,
                        USERNAME = barang.USERNAME
                    };

                    ErasoftDbContext.STF02H.Add(dataHarga);
                }


            }
            else
            {
                var custInDb = ErasoftDbContext.ARF01.Single(c => c.RecNum == customer.Customers.RecNum);

                //add by nurul 14/1/2019 'validasi email dan marketplace sama 
                var vmError = new CustomerViewModel() { };

                var cekEmailMP = ErasoftDbContext.ARF01.Where(a => a.NAMA == customer.Customers.NAMA && a.EMAIL == customer.Customers.EMAIL && a.RecNum != customer.Customers.RecNum).ToList();
                int nm = Convert.ToInt32(customer.Customers.NAMA);
                var getMP = MoDbContext.Marketplaces.SingleOrDefault(a => a.IdMarket == nm).NamaMarket;
                if (cekEmailMP.Count > 0)
                {
                    vmError.Errors.Add("Email sudah digunakan untuk Marketplace ( " + getMP + " ) !");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
                //end add by nurul 14/1/2019

                custInDb.TOP = customer.Customers.TOP;
                custInDb.AL = customer.Customers.AL;
                custInDb.KODEPROV = customer.Customers.KODEPROV;
                custInDb.KODEKABKOT = customer.Customers.KODEKABKOT;
                custInDb.KODEPOS = customer.Customers.KODEPOS;
                custInDb.PERSO = customer.Customers.PERSO;
                custInDb.EMAIL = customer.Customers.EMAIL;
                custInDb.PASSWORD = customer.Customers.PASSWORD;
                custInDb.TLP = customer.Customers.TLP;
                //add by Tri, add api key
                custInDb.API_KEY = customer.Customers.API_KEY;
                custInDb.Sort1_Cust = customer.Customers.Sort1_Cust;
                kdCustomer = custInDb.CUST;
                //end add by Tri, add api key
                custInDb.API_CLIENT_U = customer.Customers.API_CLIENT_U;
                custInDb.API_CLIENT_P = customer.Customers.API_CLIENT_P;

                kdCustomer = custInDb.CUST;
            }

            ErasoftDbContext.SaveChanges();
            var Marketplaces = MoDbContext.Marketplaces.ToList();
            //add by Tri call bl/lzd api get access key
            if (customer.Customers.NAMA.Equals(Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BUKALAPAK").IdMarket.ToString()))
            {
                var getKey = new BukaLapakController().GetAccessKey(kdCustomer, customer.Customers.EMAIL, customer.Customers.PASSWORD);
            }
            //else if (customer.Customers.NAMA.Equals(MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "LAZADA").IdMarket.ToString()))
            //{
            //    var getToken = new LazadaController().GetToken(kdCustomer, customer.Customers.API_KEY);
            //}
            #region Elevenia get deliveryTemp
            else if (customer.Customers.NAMA.Equals(Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "ELEVENIA").IdMarket.ToString()))
            {
                var elApi = new EleveniaController();
                elApi.GetDeliveryTemp(Convert.ToString(customer.Customers.RecNum), Convert.ToString(customer.Customers.API_KEY));
            }
            #endregion
            #region BLIBLI get category dan attribute
            else if (customer.Customers.NAMA.Equals(Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BLIBLI").IdMarket.ToString()))
            {
                if (!string.IsNullOrEmpty(customer.Customers.API_CLIENT_P) && !string.IsNullOrEmpty(customer.Customers.API_CLIENT_U))
                {
                    var BliApi = new BlibliController();
                    BlibliController.BlibliAPIData data = new BlibliController.BlibliAPIData()
                    {
                        API_client_username = customer.Customers.API_CLIENT_U,
                        API_client_password = customer.Customers.API_CLIENT_P,
                        API_secret_key = customer.Customers.API_KEY,
                        mta_username_email_merchant = customer.Customers.EMAIL,
                        mta_password_password_merchant = customer.Customers.PASSWORD,
                        idmarket = customer.Customers.RecNum.Value
                    };
                    BliApi.GetToken(data, true, true);
                    //BliApi.GetPickupPoint(data);
                }
            }
            #endregion

            //end add by Tri call bl/lzd api get access key
            ModelState.Clear();

            var partialVm = new CustomerViewModel()
            {
                ListCustomer = ErasoftDbContext.ARF01.AsNoTracking().ToList(),
                kodeCust = kdCustomer
            };
            if (customer.Customers.NAMA.Equals(Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "LAZADA").IdMarket.ToString()))
            {
                partialVm.marketplace = "LAZADA";
                return Json(partialVm, JsonRequestBehavior.AllowGet);
            }
            else if (customer.Customers.NAMA.Equals(Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "SHOPEE").IdMarket.ToString()))
            {
                partialVm.marketplace = "SHOPEE";
                return Json(partialVm, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return PartialView("TableCustomerPartial", partialVm);
            }
        }

        public ActionResult EditCustomer(int recNum)
        {
            try
            {
                var custVm = new CustomerViewModel()
                {
                    Customers = ErasoftDbContext.ARF01.Single(c => c.RecNum == recNum),
                    ListCustomer = ErasoftDbContext.ARF01.ToList()
                };

                return Json(custVm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult DeleteCustomer(int recNum)
        {
            var custInDb = ErasoftDbContext.ARF01.Single(c => c.RecNum == recNum);

            ErasoftDbContext.ARF01.Remove(custInDb);
            ErasoftDbContext.STF02H.RemoveRange(ErasoftDbContext.STF02H.Where(h => h.IDMARKET == recNum));

            ErasoftDbContext.SaveChanges();

            var partialVm = new CustomerViewModel()
            {
                ListCustomer = ErasoftDbContext.ARF01.ToList()
            };

            return PartialView("TableCustomerPartial", partialVm);
        }

        [HttpGet]
        public ActionResult GetProvinsi()
        {
            var prov = MoDbContext.Provinsi.ToList();

            return Json(prov, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetKabKot(string kodeProv)
        {
            var kabkot = MoDbContext.KabupatenKota.Where(k => k.KodeProv == kodeProv).ToList();

            return Json(kabkot, JsonRequestBehavior.AllowGet);
        }

        // =============================================== Bagian Customer (END)

        // =============================================== Bagian Barang (START)

        public ActionResult RefreshTableBarang()
        {
            var barangVm = new BarangViewModel()
            {
                //change by nurul 18/1/2019 -- ListStf02S = ErasoftDbContext.STF02.ToList(),
                ListStf02S = ErasoftDbContext.STF02.Where(p => (p.PART == null ? "" : p.PART) == "").ToList(),
            };

            return PartialView("TableBarang1Partial", barangVm);
        }

        public ActionResult RefreshTableBarangKosong()
        {
            var listBarangMiniStok = new List<PenjualanBarang>();
            var qohqoo = ErasoftDbContext.Database.SqlQuery<QOH_QOO_ALL_ITEM>("SELECT * FROM [QOH_QOO_ALL_ITEM]").ToList();

            //change by nurul 18/1/2019 -- foreach (var barang in ErasoftDbContext.STF02.ToList())
            foreach (var barang in ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList())
            {
                var barangUtkCek = ErasoftDbContext.STF08A.ToList().FirstOrDefault(b => b.BRG == barang.BRG);
                var qtyOnHand = 0d;
                var getQoh = 0d;
                var getQoo = 0d;
                var cekQoh = qohqoo.FirstOrDefault(p => p.BRG == barang.BRG && p.JENIS == "QOH");
                var cekQoo = qohqoo.FirstOrDefault(p => p.BRG == barang.BRG && p.JENIS == "QOO");
                if (cekQoh != null)
                {
                    getQoh = cekQoh.JUMLAH;
                }
                else
                {
                    getQoh = 0;
                }
                if (cekQoo != null)
                {
                    getQoo = cekQoo.JUMLAH;
                }
                else
                {
                    getQoo = 0;
                }

                if (barangUtkCek != null)
                {
                    //qtyOnHand = barangUtkCek.QAwal + barangUtkCek.QM1 + barangUtkCek.QM2 + barangUtkCek.QM3 + barangUtkCek.QM4
                    //            + barangUtkCek.QM5 + barangUtkCek.QM6 + barangUtkCek.QM7 + barangUtkCek.QM8 + barangUtkCek.QM9
                    //            + barangUtkCek.QM10 + barangUtkCek.QM11 + barangUtkCek.QM12 - barangUtkCek.QK1 - barangUtkCek.QK2
                    //            - barangUtkCek.QK3 - barangUtkCek.QK4 - barangUtkCek.QK5 - barangUtkCek.QK6 - barangUtkCek.QK7
                    //            - barangUtkCek.QK8 - barangUtkCek.QK9 - barangUtkCek.QK10 - barangUtkCek.QK11 - barangUtkCek.QK12;
                    qtyOnHand = GetQOHSTF08A(barang.BRG, "ALL");

                    //if (qtyOnHand == 0) //change by nurul 10/1/2019 -- yang minus jg d tampilin 
                    if (qtyOnHand <= 0)
                    {
                        listBarangMiniStok.Add(new PenjualanBarang
                        {
                            KodeBrg = barang.BRG,
                            NamaBrg = $"{barang.NAMA} {barang.NAMA2}",
                            Kategori = barang.KET_SORT1,
                            Merk = barang.KET_SORT2,
                            HJual = barang.HJUAL,
                            Qty = qtyOnHand,
                            //add by nurul 21/11/2018
                            //Stok = ErasoftDbContext.Database.SqlQuery<QOH_QOO_ALL_ITEM>("SELECT * FROM [QOH_QOO_ALL_ITEM]").ToList(),
                            //Qoh = qohqoo.FirstOrDefault(p => p.BRG == barang.BRG && p.JENIS == "QOH").JUMLAH,
                            //Qoo = qohqoo.FirstOrDefault(p => p.BRG == barang.BRG && p.JENIS == "QOO").JUMLAH,
                            Qoh = getQoh,
                            Qoo = getQoo
                        });
                    }
                }
            }

            return PartialView("TableBarangKosongPartial", listBarangMiniStok.OrderBy(b => b.NamaBrg).ToList());
        }

        public ActionResult RefreshTableBarangTidakLaku(string param)
        {
            //add by nurul 16/1/2019
            string dr = (param.Split(';')[param.Split(';').Length - 2]);
            string sd = (param.Split(';')[param.Split(';').Length - 1]);
            string tgl1 = (dr.Split('/')[dr.Split('/').Length - 3]);
            string bln1 = (dr.Split('/')[dr.Split('/').Length - 2]);
            string thn1 = (dr.Split('/')[dr.Split('/').Length - 1]);
            string drtanggal = thn1 + '-' + bln1 + '-' + tgl1;
            string tgl2 = (sd.Split('/')[sd.Split('/').Length - 3]);
            string bln2 = (sd.Split('/')[sd.Split('/').Length - 2]);
            string thn2 = (sd.Split('/')[sd.Split('/').Length - 1]);
            string sdtanggal = thn2 + '-' + bln2 + '-' + tgl2;
            //end add by nurul 
            var listBarangTidakLaku = new List<PenjualanBarang>();
            var qohqoo = ErasoftDbContext.Database.SqlQuery<QOH_QOO_ALL_ITEM>("SELECT * FROM [QOH_QOO_ALL_ITEM]").ToList();
            //change by nurul 16/1/2019 -- stf02Filter = ErasoftDbContext.Database.SqlQuery<PenjualanBarang>("select c.brg as KodeBrg,isnull(c.nama, '') + ' ' + isnull(c.nama2, '') as NamaBrg,c.KET_SORT1 as Kategori,c.KET_SORT2 as Merk, c.HJUAL as HJual from stf02 c left join (select distinct brg from sot01a a inner join sot01b b on a.no_bukti = b.no_bukti where a.tgl >= dateadd(month, -3, getdate())) b on c.brg = b.brg where isnull(b.brg, '') = ''").ToList();
            //change by nurul 18/1/2019 -- var stf02Filter = ErasoftDbContext.Database.SqlQuery<PenjualanBarang>("select c.brg as KodeBrg,isnull(c.nama, '') + ' ' + isnull(c.nama2, '') as NamaBrg,c.KET_SORT1 as Kategori,c.KET_SORT2 as Merk, c.HJUAL as HJual from stf02 c left join (select distinct brg from sot01a a inner join sot01b b on a.no_bukti = b.no_bukti where a.tgl between '" + drtanggal + "' and '" + sdtanggal + "') b on c.brg = b.brg where isnull(b.brg, '') = ''").ToList();
            var stf02Filter = ErasoftDbContext.Database.SqlQuery<PenjualanBarang>("select c.brg as KodeBrg,isnull(c.nama, '') + ' ' + isnull(c.nama2, '') as NamaBrg,c.KET_SORT1 as Kategori,c.KET_SORT2 as Merk, c.HJUAL as HJual from stf02 c left join (select distinct brg from sot01a a inner join sot01b b on a.no_bukti = b.no_bukti where a.tgl between '" + drtanggal + "' and '" + sdtanggal + "') b on c.brg = b.brg where isnull(b.brg, '') = '' and c.[type] = '3'").ToList();
            //end change 
            foreach (var barang in stf02Filter)
            {
                var getQoh = 0d;
                var getQoo = 0d;
                var cekQoh = qohqoo.FirstOrDefault(p => p.BRG == barang.KodeBrg && p.JENIS == "QOH");
                var cekQoo = qohqoo.FirstOrDefault(p => p.BRG == barang.KodeBrg && p.JENIS == "QOO");
                if (cekQoh != null)
                {
                    getQoh = cekQoh.JUMLAH;
                }
                else
                {
                    getQoh = 0;
                }
                if (cekQoo != null)
                {
                    getQoo = cekQoo.JUMLAH;
                }
                else
                {
                    getQoo = 0;
                }
                listBarangTidakLaku.Add(new PenjualanBarang
                {

                    KodeBrg = barang.KodeBrg,
                    NamaBrg = barang.NamaBrg,
                    Kategori = barang.Kategori,
                    Merk = barang.Merk,
                    HJual = barang.HJual,
                    Qoh = getQoh,
                    Qoo = getQoo
                });
            }
            //foreach (var barang in ErasoftDbContext.STF02.ToList())
            //{
            //    var barangTerpesan = ErasoftDbContext.SOT01B.FirstOrDefault(b => b.BRG == barang.BRG);

            //    // Kalo barangTerpesan == null tandanya ga laku
            //    if (barangTerpesan == null)
            //    {
            //        listBarangTidakLaku.Add(new PenjualanBarang
            //        {
            //            KodeBrg = barang.BRG,
            //            NamaBrg = $"{barang.NAMA} {barang.NAMA2}",
            //            Kategori = barang.KET_SORT1,
            //            Merk = barang.KET_SORT2,
            //            HJual = barang.HJUAL,
            //            Laku = false,
            //            //add by nurul 21/11/2018
            //            //Stok = ErasoftDbContext.Database.SqlQuery<QOH_QOO_ALL_ITEM>("SELECT * FROM [QOH_QOO_ALL_ITEM]").ToList(),
            //        });
            //    }
            //}

            return PartialView("TableBarangTidakLakuPartial", listBarangTidakLaku.OrderBy(b => b.NamaBrg).ToList());
        }

        public ActionResult RefreshTableBarangDibawahMinimumStok()
        {
            var listBarangMiniStok = new List<PenjualanBarang>();
            var qohqoo = ErasoftDbContext.Database.SqlQuery<QOH_QOO_ALL_ITEM>("SELECT * FROM [QOH_QOO_ALL_ITEM]").ToList();

            //change by nurul 18/1/2019 -- foreach (var barang in ErasoftDbContext.STF02.ToList())
            foreach (var barang in ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList())
            {

                var barangUtkCek = ErasoftDbContext.STF08A.ToList().FirstOrDefault(b => b.BRG == barang.BRG);
                var qtyOnHand = 0d;
                var getQoh = 0d;
                var getQoo = 0d;
                var cekQoh = qohqoo.FirstOrDefault(p => p.BRG == barang.BRG && p.JENIS == "QOH");
                var cekQoo = qohqoo.FirstOrDefault(p => p.BRG == barang.BRG && p.JENIS == "QOO");
                if (cekQoh != null)
                {
                    getQoh = cekQoh.JUMLAH;
                }
                else
                {
                    getQoh = 0;
                }
                if (cekQoo != null)
                {
                    getQoo = cekQoo.JUMLAH;
                }
                else
                {
                    getQoo = 0;
                }

                if (barangUtkCek != null)
                {
                    //qtyOnHand = barangUtkCek.QAwal + barangUtkCek.QM1 + barangUtkCek.QM2 + barangUtkCek.QM3 + barangUtkCek.QM4
                    //            + barangUtkCek.QM5 + barangUtkCek.QM6 + barangUtkCek.QM7 + barangUtkCek.QM8 + barangUtkCek.QM9
                    //            + barangUtkCek.QM10 + barangUtkCek.QM11 + barangUtkCek.QM12 - barangUtkCek.QK1 - barangUtkCek.QK2
                    //            - barangUtkCek.QK3 - barangUtkCek.QK4 - barangUtkCek.QK5 - barangUtkCek.QK6 - barangUtkCek.QK7
                    //            - barangUtkCek.QK8 - barangUtkCek.QK9 - barangUtkCek.QK10 - barangUtkCek.QK11 - barangUtkCek.QK12;
                    qtyOnHand = GetQOHSTF08A(barang.BRG, "ALL");

                    if (qtyOnHand < barang.MINI)
                    {
                        listBarangMiniStok.Add(new PenjualanBarang
                        {
                            KodeBrg = barang.BRG,
                            NamaBrg = $"{barang.NAMA} {barang.NAMA2}",
                            Kategori = barang.KET_SORT1,
                            Merk = barang.KET_SORT2,
                            HJual = barang.HJUAL,
                            //add by nurul 21/11/2018
                            //Stok = ErasoftDbContext.Database.SqlQuery<QOH_QOO_ALL_ITEM>("SELECT * FROM [QOH_QOO_ALL_ITEM]").ToList(),
                            Qoh = getQoh,
                            Qoo = getQoo,
                            Min = barang.MINI
                        });
                    }
                }
            }

            return PartialView("TableBarangDibawahMinimumStokPartial", listBarangMiniStok.OrderBy(b => b.NamaBrg).ToList());
        }

        public ActionResult RefreshTableBarangPalingLaku(string param)
        {
            //add by nurul 16/1/2019
            string dr = (param.Split(';')[param.Split(';').Length - 2]);
            string sd = (param.Split(';')[param.Split(';').Length - 1]);
            string tgl1 = (dr.Split('/')[dr.Split('/').Length - 3]);
            string bln1 = (dr.Split('/')[dr.Split('/').Length - 2]);
            string thn1 = (dr.Split('/')[dr.Split('/').Length - 1]);
            string drtanggal = thn1 + '-' + bln1 + '-' + tgl1;
            string tgl2 = (sd.Split('/')[sd.Split('/').Length - 3]);
            string bln2 = (sd.Split('/')[sd.Split('/').Length - 2]);
            string thn2 = (sd.Split('/')[sd.Split('/').Length - 1]);
            string sdtanggal = thn2 + '-' + bln2 + '-' + tgl2;
            //end add by nurul 
            var listBarangLaku = new List<PenjualanBarang>();
            var qohqoo = ErasoftDbContext.Database.SqlQuery<QOH_QOO_ALL_ITEM>("SELECT * FROM [QOH_QOO_ALL_ITEM]").ToList();
            //change by nurul 16/1/2019 -- var stf02Filter = ErasoftDbContext.Database.SqlQuery<PenjualanBarang>("select c.brg as KodeBrg,isnull(c.nama, '') + ' ' + isnull(c.nama2, '') as NamaBrg,c.KET_SORT1 as Kategori,c.KET_SORT2 as Merk, c.HJUAL as HJual from stf02 c left join (select distinct brg from sot01a a inner join sot01b b on a.no_bukti = b.no_bukti where a.tgl >= dateadd(month, -3, getdate())) b on c.brg = b.brg where isnull(b.brg, '') <> ''").ToList();
            //change by nurul 18/1/2019 -- var stf02Filter = ErasoftDbContext.Database.SqlQuery<PenjualanBarang>("select c.brg as KodeBrg,isnull(c.nama, '') + ' ' + isnull(c.nama2, '') as NamaBrg,c.KET_SORT1 as Kategori,c.KET_SORT2 as Merk, c.HJUAL as HJual from stf02 c left join (select distinct brg from sot01a a inner join sot01b b on a.no_bukti = b.no_bukti where a.tgl between '" + drtanggal + "' and '" + sdtanggal + "') b on c.brg = b.brg where isnull(b.brg, '') <> ''").ToList();
            var stf02Filter = ErasoftDbContext.Database.SqlQuery<PenjualanBarang>("select c.brg as KodeBrg,isnull(c.nama, '') + ' ' + isnull(c.nama2, '') as NamaBrg,c.KET_SORT1 as Kategori,c.KET_SORT2 as Merk, c.HJUAL as HJual from stf02 c left join (select distinct brg from sot01a a inner join sot01b b on a.no_bukti = b.no_bukti where a.tgl between '" + drtanggal + "' and '" + sdtanggal + "') b on c.brg = b.brg where isnull(b.brg, '') <> '' and c.[type] = '3'").ToList();
            //end change 
            foreach (var barang in stf02Filter)
            {
                var getQoh = 0d;
                var getQoo = 0d;
                var cekQoh = qohqoo.FirstOrDefault(p => p.BRG == barang.KodeBrg && p.JENIS == "QOH");
                var cekQoo = qohqoo.FirstOrDefault(p => p.BRG == barang.KodeBrg && p.JENIS == "QOO");
                if (cekQoh != null)
                {
                    getQoh = cekQoh.JUMLAH;
                }
                else
                {
                    getQoh = 0;
                }
                if (cekQoo != null)
                {
                    getQoo = cekQoo.JUMLAH;
                }
                else
                {
                    getQoo = 0;
                }
                listBarangLaku.Add(new PenjualanBarang
                {

                    KodeBrg = barang.KodeBrg,
                    NamaBrg = barang.NamaBrg,
                    Kategori = barang.Kategori,
                    Merk = barang.Merk,
                    HJual = barang.HJual,
                    Qoh = getQoh,
                    Qoo = getQoo
                });
            }

            //foreach (var barang in ErasoftDbContext.STF02.ToList())
            //{
            //    //change by nurul 10/12/2018 (periode 3 bulan terakhir)
            //    //var listBarangTerpesan = ErasoftDbContext.SOT01B.Where(b => b.BRG == barang.BRG).ToList();
            //    var month = DateTime.Now.AddMonths(-3);
            //    var listBarangTerpesan = (from a in ErasoftDbContext.SOT01A
            //                              join b in ErasoftDbContext.SOT01B on a.NO_BUKTI equals b.NO_BUKTI
            //                              where b.BRG == barang.BRG && a.TGL >= month
            //                              select new { BRG = b.BRG, NO_BUKTI = a.NO_BUKTI, TGL = a.TGL }).ToList();
            //    //end change 

            //    if (listBarangTerpesan.Count > 0)
            //    {
            //        listBarangLaku.Add(new PenjualanBarang
            //        {
            //            KodeBrg = barang.BRG,
            //            //KodeBrg = listBarangTerpesan.SingleOrDefault().BRG,
            //            NamaBrg = $"{barang.NAMA} {barang.NAMA2}",
            //            Kategori = barang.KET_SORT1,
            //            Merk = barang.KET_SORT2,
            //            HJual = barang.HJUAL,
            //            //add by nurul 21/11/2018
            //            Stok = ErasoftDbContext.Database.SqlQuery<QOH_QOO_ALL_ITEM>("SELECT * FROM [QOH_QOO_ALL_ITEM]").ToList(),
            //        });
            //    }
            //}


            return PartialView("TableBarangPalingLakuPartial", listBarangLaku.OrderBy(b => b.NamaBrg).ToList());
        }

        [HttpGet]
        public ActionResult GetKategoriBarang()
        {
            var listKategori = ErasoftDbContext.STF02E.Where(k => k.LEVEL == "1").OrderBy(m => m.KET).ToList();

            return Json(listKategori, JsonRequestBehavior.AllowGet);
        }
        #region Kategori Elevenia

        [HttpGet]
        public ActionResult GetKategoriEleveniaByCode(string code)
        {
            //string[] codelist = code.Split(';');
            var listKategoriEle = MoDbContext.CategoryElevenia.Where(k => k.PARENT_CODE == "").OrderBy(k => k.CATEGORY_NAME).ToList();

            return Json(listKategoriEle, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetKategoriEleveniaByParentCode(string code)
        {
            string[] codelist = code.Split(';');
            var listKategoriEle = MoDbContext.CategoryElevenia.Where(k => codelist.Contains(k.PARENT_CODE)).OrderBy(k => k.CATEGORY_NAME).ToList();

            return Json(listKategoriEle, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetKategoriEleveniaByChildCode(string code)
        {
            string[] codelist = code.Split(';');
            List<CATEGORY_ELEVENIA> listKategoriEle = new List<CATEGORY_ELEVENIA>();
            if (!string.IsNullOrEmpty(code))
            {
                var category = MoDbContext.CategoryElevenia.Where(k => codelist.Contains(k.CATEGORY_CODE)).FirstOrDefault();
                listKategoriEle.Add(category);

                if (category.PARENT_CODE != "")
                {
                    bool TopParent = false;
                    while (!TopParent)
                    {
                        category = MoDbContext.CategoryElevenia.Where(k => k.CATEGORY_CODE.Equals(category.PARENT_CODE)).FirstOrDefault();
                        listKategoriEle.Add(category);
                        if (string.IsNullOrEmpty(category.PARENT_CODE))
                        {
                            TopParent = true;
                        }
                    }
                }
            }

            return Json(listKategoriEle.OrderBy(p => p.RecNum), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetAttributeElevenia(string code)
        {
            string[] codelist = code.Split(';');
            var listAttributeEle = MoDbContext.AttributeElevenia.Where(k => codelist.Contains(k.CATEGORY_CODE)).ToList();
            return Json(listAttributeEle, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Kategori Blibli
        [HttpGet]
        public ActionResult GetKategoriBlibliByCode(string code)
        {
            string[] codelist = code.Split(';');
            var listKategoriBlibli = MoDbContext.CategoryBlibli.Where(k => codelist.Contains(k.CATEGORY_CODE)).OrderBy(k => k.CATEGORY_NAME).ToList();

            return Json(listKategoriBlibli, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetKategoriBlibliByParentCode(string code)
        {
            string[] codelist = code.Split(';');
            var listKategoriBlibli = MoDbContext.CategoryBlibli.Where(k => codelist.Contains(k.PARENT_CODE)).OrderBy(k => k.CATEGORY_NAME).ToList();

            return Json(listKategoriBlibli, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetKategoriBlibliByChildCode(string code)
        {
            string[] codelist = code.Split(';');
            List<CATEGORY_BLIBLI> listKategoriBlibli = new List<CATEGORY_BLIBLI>();
            var category = MoDbContext.CategoryBlibli.Where(k => codelist.Contains(k.CATEGORY_CODE)).FirstOrDefault();
            listKategoriBlibli.Add(category);

            if (category.PARENT_CODE != "")
            {
                bool TopParent = false;
                while (!TopParent)
                {
                    category = MoDbContext.CategoryBlibli.Where(k => k.CATEGORY_CODE.Equals(category.PARENT_CODE)).FirstOrDefault();
                    listKategoriBlibli.Add(category);
                    if (string.IsNullOrEmpty(category.PARENT_CODE))
                    {
                        TopParent = true;
                    }
                }
            }

            return Json(listKategoriBlibli.OrderBy(p => p.RecNum), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        //public ActionResult GetAttributeBlibli(string code)
        public async Task<ActionResult> GetAttributeBlibli(string code)
        {
            string[] codelist = code.Split(';');
            //var listAttributeBlibli = MoDbContext.AttributeBlibli.Where(k => codelist.Contains(k.CATEGORY_CODE)).ToList();
            string categoryCode = codelist[0];
            var marketrecnum_int = Convert.ToInt32(codelist[1]);
            var CategoryBlibli = MoDbContext.CategoryBlibli.Where(k => k.CATEGORY_CODE == categoryCode).FirstOrDefault();
            var tblCustomer = ErasoftDbContext.ARF01.Where(p => p.RecNum == marketrecnum_int).FirstOrDefault();

            var BliAPI = new BlibliController();

            BlibliController.BlibliAPIData data = new BlibliController.BlibliAPIData
            {
                merchant_code = tblCustomer.Sort1_Cust,
                API_client_password = tblCustomer.API_CLIENT_P,
                API_client_username = tblCustomer.API_CLIENT_U,
                API_secret_key = tblCustomer.API_KEY,
                token = tblCustomer.TOKEN,
                mta_username_email_merchant = tblCustomer.EMAIL,
                mta_password_password_merchant = tblCustomer.PASSWORD,
                idmarket = tblCustomer.RecNum.Value
            };

            var listAttributeBlibli = await BliAPI.GetAttributeToList(data, CategoryBlibli);

            return Json(listAttributeBlibli, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetAttributeOptBlibli(string code)
        {
            string[] codelist = code.Split(';');
            var listAttributeOptBlibli = MoDbContext.AttributeOptBlibli.Where(k => codelist.Contains(k.ACODE)).ToList();
            return Json(listAttributeOptBlibli, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> GetAttributeBlibliVar(string code)
        {
            string[] codelist = code.Split(';');
            //var listAttributeBlibli = MoDbContext.AttributeBlibli.Where(k => codelist.Contains(k.CATEGORY_CODE)).ToList();
            string categoryCode = codelist[0];
            var marketrecnum_int = Convert.ToInt32(codelist[1]);
            var CategoryBlibli = MoDbContext.CategoryBlibli.Where(k => k.CATEGORY_CODE == categoryCode).FirstOrDefault();
            var tblCustomer = ErasoftDbContext.ARF01.Where(p => p.RecNum == marketrecnum_int).FirstOrDefault();

            var BliAPI = new BlibliController();

            BlibliController.BlibliAPIData data = new BlibliController.BlibliAPIData
            {
                merchant_code = tblCustomer.Sort1_Cust,
                API_client_password = tblCustomer.API_CLIENT_P,
                API_client_username = tblCustomer.API_CLIENT_U,
                API_secret_key = tblCustomer.API_KEY,
                token = tblCustomer.TOKEN,
                mta_username_email_merchant = tblCustomer.EMAIL,
                mta_password_password_merchant = tblCustomer.PASSWORD,
                idmarket = tblCustomer.RecNum.Value
            };

            var listAttributeBlibli = await BliAPI.GetAttributeToList(data, CategoryBlibli);

            return Json(listAttributeBlibli, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetAttributeOptBlibliVar(string code)
        {
            string[] codelist = code.Split(';');
            var listAttributeOptBlibli = MoDbContext.AttributeOptBlibli.Where(k => codelist.Contains(k.ACODE)).ToList();
            return Json(listAttributeOptBlibli, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region lzd
        [HttpGet]
        public ActionResult GetKategoriLazadaByCode(/*string code*/)
        {
            //string[] codelist = code.Split(';');
            //var listKategoriLazada = MoDbContext.CATEGORY_LAZADA.Where(k => codelist.Contains(k.CATEGORY_ID)).OrderBy(k => k.NAME).ToList();
            var listKategoriLazada = MoDbContext.CATEGORY_LAZADA.Where(k => string.IsNullOrEmpty(k.PARENT_ID)).OrderBy(k => k.NAME).ToList();

            return Json(listKategoriLazada, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetKategoriLazadaByParentCode(string code)
        {
            string[] codelist = code.Split(';');
            var listKategoriLazada = MoDbContext.CATEGORY_LAZADA.Where(k => codelist.Contains(k.PARENT_ID)).OrderBy(k => k.NAME).ToList();

            return Json(listKategoriLazada, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetKategoriLazadaByChildCode(string code)
        {
            string[] codelist = code.Split(';');
            List<CATEGORY_LAZADA> listKategoriLazada = new List<CATEGORY_LAZADA>();
            var category = MoDbContext.CATEGORY_LAZADA.Where(k => codelist.Contains(k.CATEGORY_ID)).FirstOrDefault();
            listKategoriLazada.Add(category);

            if (category.PARENT_ID != "")
            {
                bool TopParent = false;
                while (!TopParent)
                {
                    category = MoDbContext.CATEGORY_LAZADA.Where(k => k.CATEGORY_ID.Equals(category.PARENT_ID)).FirstOrDefault();
                    listKategoriLazada.Add(category);
                    if (string.IsNullOrEmpty(category.PARENT_ID))
                    {
                        TopParent = true;
                    }
                }
            }

            return Json(listKategoriLazada.OrderBy(p => p.RecNum), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetAttributeLazada(string code)
        {
            string[] codelist = code.Split(';');
            var listAttributeLazada = MoDbContext.ATTRIBUTE_LAZADA.Where(k => codelist.Contains(k.CATEGORY_CODE)).ToList();
            return Json(listAttributeLazada, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetAttributeOptLazada(string code, string kategoryCode)
        {
            string[] codelist = code.Split(';');
            //var listAttributeOptLazada = MoDbContext.ATTRIBUTE_OPT_LAZADA.Where(k => codelist.Contains(k.A_NAME) && k.CATEGORY_CODE.ToUpper() == kategoryCode.ToUpper()).ToList();
            //var listAttributeOptLazada = MoDbContext.Database.SqlQuery<ATTRIBUTE_OPT_LAZADA>("SELECT * FROM ATTRIBUTE_OPT_LAZADA WHERE UPPER(CATEGORY_CODE)=UPPER('" + kategoryCode + "') AND A_NAME='" + codelist[0] + "'").ToList();
            var lzdApi = new LazadaController();
            var listAttributeOptLazada = lzdApi.getAttrLzd(kategoryCode, codelist[0]);
            return Json(listAttributeOptLazada, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetAttributeLazadaVar(string code)
        {
            string[] codelist = code.Split(';');
            var listAttributeLazada = MoDbContext.ATTRIBUTE_LAZADA.Where(k => codelist.Contains(k.CATEGORY_CODE)).ToList();
            return Json(listAttributeLazada, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetAttributeOptLazadaVar(string code)
        {
            string[] codelist = code.Split(';');
            var lzdApi = new LazadaController();
            var listAttributeOptLazada = lzdApi.getAttrLzd(codelist[1], codelist[0]);
            return Json(listAttributeOptLazada, JsonRequestBehavior.AllowGet);
        }
        #endregion
        //add by calvin 18 desember 2018
        #region Kategori Shopee
        [HttpGet]
        public ActionResult GetKategoriShopeeByCode(string code)
        {
            string[] codelist = code.Split(';');
            var listKategoriShopee = MoDbContext.CategoryShopee.Where(k => string.IsNullOrEmpty(k.PARENT_CODE)).OrderBy(k => k.CATEGORY_NAME).ToList();

            return Json(listKategoriShopee, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetKategoriShopeeByParentCode(string code)
        {
            string[] codelist = code.Split(';');
            var listKategoriShopee = MoDbContext.CategoryShopee.Where(k => codelist.Contains(k.PARENT_CODE)).OrderBy(k => k.CATEGORY_NAME).ToList();

            return Json(listKategoriShopee, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetKategoriShopeeByChildCode(string code)
        {
            string[] codelist = code.Split(';');
            List<CATEGORY_SHOPEE> listKategoriShopee = new List<CATEGORY_SHOPEE>();
            if (!string.IsNullOrEmpty(code))
            {
                var category = MoDbContext.CategoryShopee.Where(k => codelist.Contains(k.CATEGORY_CODE)).FirstOrDefault();
                listKategoriShopee.Add(category);

                if (category.PARENT_CODE != "")
                {
                    bool TopParent = false;
                    while (!TopParent)
                    {
                        category = MoDbContext.CategoryShopee.Where(k => k.CATEGORY_CODE.Equals(category.PARENT_CODE)).FirstOrDefault();
                        listKategoriShopee.Add(category);
                        if (string.IsNullOrEmpty(category.PARENT_CODE))
                        {
                            TopParent = true;
                        }
                    }
                }
            }
            return Json(listKategoriShopee.OrderBy(p => p.RecNum), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> GetAttributeShopee(string code)
        //public ActionResult GetAttributeShopee(string code)
        {
            string[] codelist = code.Split(';');
            //var listAttributeShopee = MoDbContext.AttributeShopee.Where(k => codelist.Contains(k.CATEGORY_CODE)).ToList();
            string categoryCode = codelist[0];
            var marketrecnum_int = Convert.ToInt32(codelist[1]);
            var CategoryShopee = MoDbContext.CategoryShopee.Where(k => k.CATEGORY_CODE == categoryCode).FirstOrDefault();
            var sort1_cust = ErasoftDbContext.ARF01.Where(p => p.RecNum == marketrecnum_int).FirstOrDefault().Sort1_Cust;

            var ShopeeApi = new ShopeeController();

            ShopeeController.ShopeeAPIData data = new ShopeeController.ShopeeAPIData()
            {
                merchant_code = sort1_cust,
            };

            var listAttributeShopee = await ShopeeApi.GetAttributeToList(data, CategoryShopee);

            return Json(listAttributeShopee, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetAttributeOptShopee(string code)
        {
            string[] codelist = code.Split(';');
            var listAttributeOptShopee = MoDbContext.AttributeOptShopee.Where(k => codelist.Contains(k.ACODE)).ToList();
            return Json(listAttributeOptShopee, JsonRequestBehavior.AllowGet);
        }
        #endregion
        //end add by calvin 18 desember 2018

        //add by calvin 6 februari 2019
        #region Kategori Tokped
        [HttpGet]
        public ActionResult GetKategoriTokpedByCode(string code)
        {
            string[] codelist = code.Split(';');
            var listKategoriTokped = MoDbContext.CategoryTokped.Where(k => string.IsNullOrEmpty(k.PARENT_CODE)).OrderBy(k => k.CATEGORY_NAME).ToList();

            return Json(listKategoriTokped, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetKategoriTokpedByParentCode(string code)
        {
            string[] codelist = code.Split(';');
            var listKategoriTokped = MoDbContext.CategoryTokped.Where(k => codelist.Contains(k.PARENT_CODE)).OrderBy(k => k.CATEGORY_NAME).ToList();

            return Json(listKategoriTokped, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetKategoriTokpedByChildCode(string code)
        {
            string[] codelist = code.Split(';');
            List<CATEGORY_TOKPED> listKategoriTokped = new List<CATEGORY_TOKPED>();
            if (!string.IsNullOrEmpty(code))
            {
                var category = MoDbContext.CategoryTokped.Where(k => codelist.Contains(k.CATEGORY_CODE)).FirstOrDefault();
                listKategoriTokped.Add(category);

                if (category.PARENT_CODE != "")
                {
                    bool TopParent = false;
                    while (!TopParent)
                    {
                        category = MoDbContext.CategoryTokped.Where(k => k.CATEGORY_CODE.Equals(category.PARENT_CODE)).FirstOrDefault();
                        listKategoriTokped.Add(category);
                        if (string.IsNullOrEmpty(category.PARENT_CODE))
                        {
                            TopParent = true;
                        }
                    }
                }
            }
            return Json(listKategoriTokped.OrderBy(p => p.MASTER_CATEGORY_CODE).ThenBy(p => p.IS_LAST_NODE), JsonRequestBehavior.AllowGet);
        }
        public class GetAttributeTokpedReturn
        {
            public List<ATTRIBUTE_TOKPED> attribute { get; set; }
            public List<ATTRIBUTE_UNIT_TOKPED> attribute_unit { get; set; }
        }
        [HttpGet]
        public ActionResult GetAttributeTokped(string code)
        {
            string[] codelist = code.Split(';');
            var listAttributeTokped = MoDbContext.AttributeTokped.Where(k => codelist.Contains(k.CATEGORY_CODE)).ToList();
            var listAttributeUnitTokped = MoDbContext.AttributeUnitTokped.ToList();
            var returnList = new GetAttributeTokpedReturn()
            {
                attribute = listAttributeTokped,
                attribute_unit = listAttributeUnitTokped
            };
            return Json(returnList, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetAttributeOptTokped(string code)
        {
            string[] codelist = code.Split(';');
            try
            {
                int VariantID = Convert.ToInt32(codelist[0]);
                int UnitID = Convert.ToInt32(codelist[1]);
                var listAttributeOptTokped = MoDbContext.AttributeOptTokped.Where(p => p.VARIANT_ID == VariantID && p.UNIT_ID == UnitID).ToList();
                return Json(listAttributeOptTokped, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var listAttributeOptTokped = MoDbContext.AttributeOptTokped.Where(p => 0 == 1).ToList();
                return Json(listAttributeOptTokped, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        //end add by calvin 6 februari 2019
        [HttpGet]
        public ActionResult GetMerkBarang()
        {
            var listMerk = ErasoftDbContext.STF02E.Where(m => m.LEVEL == "2").OrderBy(m => m.KET).ToList();
            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };

            var result = new ContentResult
            {
                Content = serializer.Serialize(listMerk),
                ContentType = "application/json"
            };

            return result;
        }

        public ActionResult DeleteFotoProduk(string kodeBarang, int urutan)
        {
            try
            {
                var barangInDb = ErasoftDbContext.STF02.FirstOrDefault(b => b.BRG == kodeBarang);

                if (barangInDb != null)
                {
                    switch (urutan)
                    {
                        case 1:
                            barangInDb.LINK_GAMBAR_1 = null;
                            barangInDb.Sort5 = null;
                            break;
                        case 2:
                            barangInDb.LINK_GAMBAR_2 = null;
                            barangInDb.Sort6 = null;
                            break;
                        case 3:
                            barangInDb.LINK_GAMBAR_3 = null;
                            barangInDb.Sort7 = null;
                            break;
                    }

                    ErasoftDbContext.SaveChanges();
                }

                return Json("Sukses hapus url foto produk dari tabel", JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult DeleteFotoProdukBlibli(string brg, string recnum, int urutan)
        {
            try
            {
                int recnum_int = Convert.ToInt32(recnum);
                var barangInDb = ErasoftDbContext.STF02H.FirstOrDefault(b => b.BRG == brg && b.IDMARKET == recnum_int);

                if (barangInDb != null)
                {
                    switch (urutan)
                    {
                        case 1:
                            barangInDb.AVALUE_50 = null;
                            barangInDb.ACODE_50 = null;
                            break;
                        case 2:
                            barangInDb.AVALUE_49 = null;
                            barangInDb.ACODE_49 = null;
                            break;
                        case 3:
                            barangInDb.AVALUE_48 = null;
                            barangInDb.ACODE_48 = null;
                            break;
                    }

                    ErasoftDbContext.SaveChanges();
                }

                return Json("Sukses hapus url foto produk dari tabel", JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult DeleteFotoProdukBliVar(string recnum, int urutan)
        {
            try
            {
                int recnum_int = Convert.ToInt32(recnum);
                var barangInDb = ErasoftDbContext.STF02H.FirstOrDefault(b => b.RecNum.Value == recnum_int);

                if (barangInDb != null)
                {
                    switch (urutan)
                    {
                        case 1:
                            barangInDb.AVALUE_50 = null;
                            barangInDb.ACODE_50 = null;
                            break;
                    }

                    ErasoftDbContext.SaveChanges();
                }

                return Json("Sukses hapus url foto produk dari tabel", JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
        [HttpPost]
        //public ActionResult SaveBarang(BarangViewModel dataBarang, IEnumerable<HttpPostedFileBase> fotoProdukBlibli)
        public ActionResult SaveBarang(BarangViewModel dataBarang)
        {
            if (!ModelState.IsValid)
            {
                dataBarang.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(dataBarang, JsonRequestBehavior.AllowGet);
            }

            bool insert = false;//add by Tri
            bool updateHarga = false;//add by Tri
            bool updateDisplay = false;//add by Tri
            bool updateGambar = false;//add by Tri
            var Marketplaces = MoDbContext.Marketplaces.ToList();
            var kdBL = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BUKALAPAK");
            var kdLazada = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "LAZADA");
            var kdBlibli = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BLIBLI");
            var kdElevenia = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "ELEVENIA");
            var validPrice = true;

            string[] imgPath = new string[Request.Files.Count];
            if (dataBarang.Stf02.ID == null)
            {
                insert = true;

                if (dataBarang.ListHargaJualPermarket?.Count > 0)
                {
                    List<string> listError = new List<string>();
                    int i = 0;
                    foreach (var hargaPerMarket in dataBarang.ListHargaJualPermarket)
                    {
                        var kdMarket = ErasoftDbContext.ARF01.Where(m => m.RecNum == hargaPerMarket.IDMARKET).SingleOrDefault().NAMA;

                        if (kdMarket == kdLazada.IdMarket.ToString())
                        {
                            if (hargaPerMarket.HJUAL < 3000)
                            {
                                validPrice = false;
                                listError.Add(i + "_errortext_" + "Harga Jual harus lebih dari 3000.");
                            }
                            else if (hargaPerMarket.HJUAL % 100 != 0)
                            {
                                validPrice = false;
                                listError.Add(i + "_errortext_" + "Harga Jual harus kelipatan 100.");

                            }
                        }
                        else if (kdMarket == kdBlibli.IdMarket.ToString())
                        {
                            if (hargaPerMarket.HJUAL < 1100)
                            {
                                validPrice = false;
                                listError.Add(i + "_errortext_" + "Harga Jual minimal 1100.");
                            }
                        }
                        else if (kdMarket == kdBL.IdMarket.ToString() || kdMarket == kdElevenia.IdMarket.ToString())
                        {
                            if (hargaPerMarket.HJUAL < 100)
                            {
                                validPrice = false;
                                listError.Add(i + "_errortext_" + "Harga Jual harus lebih dari 100.");
                            }
                            else if (hargaPerMarket.HJUAL % 100 != 0)
                            {
                                validPrice = false;
                                listError.Add(i + "_errortext_" + "Harga Jual harus kelipatan 100.");

                            }
                        }
                        i++;
                    }
                    if (validPrice)
                    {
                        //add by calvin 1 maret 2019
                        Dictionary<string, string> extra_image_uploaded = new Dictionary<string, string>();
                        Dictionary<int, string> same_uploaded = new Dictionary<int, string>();
                        if (Request.Files.Count > 0)
                        {
                            for (int file_index = 0; file_index < Request.Files.Count; file_index++)
                            {
                                string key = Request.Files.GetKey(file_index);
                                string[] key_split = key.Split(';');
                                if (key_split.Count() > 1)
                                {
                                    #region Extra Image
                                    int urutan = Convert.ToInt32(key_split[0]);
                                    int idmarket = Convert.ToInt32(key_split[1]);
                                    var file = Request.Files[file_index];

                                    if (file != null && file.ContentLength > 0)
                                    {
                                        if (!same_uploaded.ContainsKey(file.ContentLength))
                                        {
                                            ImgurImageResponse image = UploadImageService.UploadSingleImageToImgur(file, "uploaded-image");
                                            same_uploaded.Add(file.ContentLength, image.data.link_l);
                                            extra_image_uploaded.Add(Convert.ToString(urutan) + ";" + Convert.ToString(idmarket) + ";" + Convert.ToString(file.ContentLength), image.data.link_l);
                                        }
                                        else
                                        {
                                            extra_image_uploaded.Add(Convert.ToString(urutan) + ";" + Convert.ToString(idmarket) + ";" + Convert.ToString(file.ContentLength), same_uploaded.Where(p => p.Key == file.ContentLength).FirstOrDefault().Value);
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }
                        //end add by calvin 1 maret 2019

                        foreach (var hargaPerMarket in dataBarang.ListHargaJualPermarket)
                        {
                            hargaPerMarket.BRG = dataBarang.Stf02.BRG;

                            //add by calvin 1 maret 2019
                            if (extra_image_uploaded.Count() > 0)
                            {
                                foreach (var extra_image in extra_image_uploaded)
                                {
                                    string[] key_split = extra_image.Key.Split(';');
                                    int urutan = Convert.ToInt32(key_split[0]);
                                    int idmarket = Convert.ToInt32(key_split[1]);
                                    string idGambar = Convert.ToString(key_split[2]);
                                    if (idmarket == hargaPerMarket.IDMARKET)
                                    {
                                        switch (urutan)
                                        {
                                            case 1:
                                                hargaPerMarket.ACODE_50 = idGambar;
                                                hargaPerMarket.AVALUE_50 = extra_image.Value;
                                                break;
                                            case 2:
                                                hargaPerMarket.ACODE_49 = idGambar;
                                                hargaPerMarket.AVALUE_49 = extra_image.Value;
                                                break;
                                            case 3:
                                                hargaPerMarket.ACODE_48 = idGambar;
                                                hargaPerMarket.AVALUE_48 = extra_image.Value;
                                                break;
                                        }
                                    }
                                }
                            }
                            //end add by calvin 1 maret 2019
                            ErasoftDbContext.STF02H.Add(hargaPerMarket);
                        }
                    }
                    else
                    {
                        dataBarang.errorHargaPerMP = "1";
                        dataBarang.Errors = listError;
                        return Json(dataBarang, JsonRequestBehavior.AllowGet);
                    }

                }

                var listMarket = dataBarang.ListMarket.ToList();

                //remark by Tri, moved to top
                //add by tri
                //string[] imgPath = new string[Request.Files.Count];
                //end add by tri
                //end remark by Tri, moved to top

                if (Request.Files.Count > 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var file = Request.Files[i];

                        if (file != null && file.ContentLength > 0)
                        {
                            //var namaFile = $"FotoProduk-{dataBarang.Stf02.USERNAME}-BRG{dataBarang.Stf02.BRG}-foto-{i + 1}";
                            ImgurImageResponse image = UploadImageService.UploadSingleImageToImgur(file, "uploaded-image");

                            //var fileExtension = Path.GetExtension(file.FileName);
                            //var path = Path.Combine(Server.MapPath("~/Content/Uploaded/"), namaFile);
                            //try
                            //{
                            //    file.SaveAs(path);
                            //}
                            //catch (Exception ex)
                            //{

                            //}
                            //add by tri

                            imgPath[i] = image.data.link;

                            switch (i)
                            {
                                case 0:
                                    dataBarang.Stf02.LINK_GAMBAR_1 = image.data.link_l;
                                    dataBarang.Stf02.Sort5 = Convert.ToString(file.ContentLength);
                                    break;
                                case 1:
                                    dataBarang.Stf02.LINK_GAMBAR_2 = image.data.link_l;
                                    dataBarang.Stf02.Sort6 = Convert.ToString(file.ContentLength);
                                    break;
                                case 2:
                                    dataBarang.Stf02.LINK_GAMBAR_3 = image.data.link_l;
                                    dataBarang.Stf02.Sort7 = Convert.ToString(file.ContentLength);
                                    break;
                            }
                        }
                    }
                }

                for (int i = 0; i < Request.Files.Count; i++)
                {
                    switch (i)
                    {
                        case 0:
                            imgPath[0] = dataBarang.Stf02.LINK_GAMBAR_1;
                            break;
                        case 1:
                            imgPath[1] = dataBarang.Stf02.LINK_GAMBAR_2;
                            break;
                        case 2:
                            imgPath[2] = dataBarang.Stf02.LINK_GAMBAR_3;
                            break;
                    }
                }

                if (string.IsNullOrWhiteSpace(dataBarang.Stf02.TYPE))
                {
                    dataBarang.Stf02.TYPE = "3";
                }
                ErasoftDbContext.STF02.Add(dataBarang.Stf02);
            }
            else
            {
                var barangInDb = ErasoftDbContext.STF02.SingleOrDefault(b => b.ID == dataBarang.Stf02.ID);

                if (barangInDb != null)
                {
                    barangInDb.NAMA = dataBarang.Stf02.NAMA;
                    barangInDb.NAMA2 = dataBarang.Stf02.NAMA2;
                    barangInDb.MINI = dataBarang.Stf02.MINI;
                    barangInDb.MAXI = dataBarang.Stf02.MAXI;
                    barangInDb.Sort1 = dataBarang.Stf02.Sort1;
                    barangInDb.Sort2 = dataBarang.Stf02.Sort2;
                    barangInDb.KET_SORT1 = dataBarang.Stf02.KET_SORT1;
                    barangInDb.KET_SORT2 = dataBarang.Stf02.KET_SORT2;
                    barangInDb.STN = dataBarang.Stf02.STN;
                    barangInDb.STN2 = dataBarang.Stf02.STN2;
                    barangInDb.ISI = dataBarang.Stf02.ISI;
                    barangInDb.Metoda = dataBarang.Stf02.Metoda;
                    barangInDb.Deskripsi = dataBarang.Stf02.Deskripsi;
                    barangInDb.BERAT = dataBarang.Stf02.BERAT;
                    barangInDb.PANJANG = dataBarang.Stf02.PANJANG;
                    barangInDb.LEBAR = dataBarang.Stf02.LEBAR;
                    barangInDb.TINGGI = dataBarang.Stf02.TINGGI;
                    barangInDb.HJUAL = dataBarang.Stf02.HJUAL;
                    barangInDb.TYPE = "3";

                    if (dataBarang.ListHargaJualPermarket?.Count > 0)
                    {
                        List<string> listError = new List<string>();
                        int i = 0;
                        foreach (var dataBaru in dataBarang.ListHargaJualPermarket)
                        {
                            //add validasi harga per marketplace
                            var kdMarket = ErasoftDbContext.ARF01.Where(m => m.RecNum == dataBaru.IDMARKET).SingleOrDefault().NAMA;
                            //add by nurul 31/1/2019
                            //var getpromosi1 = ErasoftDbContext.Database.SqlQuery<>("SELECT * FROM API_LOG_MARKETPLACE_PER_ITEM WHERE REQUEST_ATTRIBUTE_1 = '" + barangId + "' AND REQUEST_ACTION IN ('Create Product','create brg','create Produk')").ToList()
                            var getpromo1 = (from a in ErasoftDbContext.PROMOSI
                                             join b in ErasoftDbContext.DETAILPROMOSI on a.RecNum equals b.RecNumPromosi
                                             join c in ErasoftDbContext.ARF01 on a.NAMA_MARKET equals c.CUST
                                             select new { brg = b.KODE_BRG, mulai = a.TGL_MULAI, akhir = a.TGL_AKHIR, nama = c.NAMA }).ToList();
                            var getpromo2 = (from d in MoDbContext.Marketplaces
                                             select new { market = d.IdMarket }).ToList();
                            var getpromosi = (from a in getpromo1
                                              join d in getpromo2 on a.nama equals Convert.ToString(d.market)
                                              where a.brg == barangInDb.BRG && Convert.ToString(d.market) == kdMarket
                                              select new BarangViewModel { BRG = a.brg, MULAI = Convert.ToString(a.mulai), AKHIR = Convert.ToString(a.akhir), MARKET = Convert.ToInt32(d.market) }).ToList();
                            var drtanggal = "";
                            var sdtanggal = "";
                            if (getpromosi.Count() > 0)
                            {
                                string tgl1 = (getpromosi.FirstOrDefault().MULAI.Split('-')[getpromosi.FirstOrDefault().MULAI.Split('-').Length - 3]);
                                string bln1 = (getpromosi.FirstOrDefault().MULAI.Split('-')[getpromosi.FirstOrDefault().MULAI.Split('-').Length - 2]);
                                string thn10 = (getpromosi.FirstOrDefault().MULAI.Split('-')[getpromosi.FirstOrDefault().MULAI.Split('-').Length - 1]);
                                string thn1 = (thn10.Split(' ')[thn10.Split(' ').Length - 3]);
                                drtanggal = tgl1 + '/' + bln1 + '/' + thn1;
                            }
                            else
                            {
                                drtanggal = "01/01/1000";
                            }
                            if (getpromosi.Count() > 0)
                            {
                                string tgl2 = (getpromosi.FirstOrDefault().AKHIR.Split('-')[getpromosi.FirstOrDefault().AKHIR.Split('-').Length - 3]);
                                string bln2 = (getpromosi.FirstOrDefault().AKHIR.Split('-')[getpromosi.FirstOrDefault().AKHIR.Split('-').Length - 2]);
                                string thn20 = (getpromosi.FirstOrDefault().AKHIR.Split('-')[getpromosi.FirstOrDefault().AKHIR.Split('-').Length - 1]);
                                string thn2 = (thn20.Split(' ')[thn20.Split(' ').Length - 3]);
                                sdtanggal = tgl2 + '/' + bln2 + '/' + thn2;
                            }
                            else
                            {
                                sdtanggal = "01/01/1000";
                            }
                            var tglmulai = DateTime.ParseExact(drtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                            var tglakhir = DateTime.ParseExact(sdtanggal, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                            //end add by nurul 31/1/2019
                            if (kdMarket == kdLazada.IdMarket.ToString())
                            {
                                if (dataBaru.HJUAL < 3000)
                                {
                                    validPrice = false;
                                    listError.Add(i + "_errortext_" + "Harga Jual harus lebih dari 3000.");
                                }
                                else if (dataBaru.HJUAL % 100 != 0)
                                {
                                    validPrice = false;
                                    listError.Add(i + "_errortext_" + "Harga Jual harus kelipatan 100.");

                                }
                                //add by nurul 31/1/2019
                                if (DateTime.Now >= tglmulai && DateTime.Now <= tglakhir)
                                {
                                    validPrice = false;
                                    listError.Add(i + "_errortext_" + "Harga barang tidak dapat di update, karena sedang dalam masa promosi !");
                                }
                                //end add by nurul 31/1/2019
                            }
                            else if (kdMarket == kdBlibli.IdMarket.ToString())
                            {
                                if (dataBaru.HJUAL < 1100)
                                {
                                    validPrice = false;
                                    listError.Add(i + "_errortext_" + "Harga Jual minimal 1100.");
                                }
                                //add by nurul 31/1/2019
                                if (DateTime.Now >= tglmulai && DateTime.Now <= tglakhir)
                                {
                                    validPrice = false;
                                    listError.Add(i + "_errortext_" + "Harga barang tidak dapat di update, karena sedang dalam masa promosi !");
                                }
                                //end add by nurul 31/1/2019
                            }
                            else if (kdMarket == kdBL.IdMarket.ToString() || kdMarket == kdElevenia.IdMarket.ToString())
                            {
                                if (dataBaru.HJUAL < 100)
                                {
                                    validPrice = false;
                                    listError.Add(i + "_errortext_" + "Harga Jual harus lebih dari 100.");
                                }
                                else if (dataBaru.HJUAL % 100 != 0)
                                {
                                    validPrice = false;
                                    listError.Add(i + "_errortext_" + "Harga Jual harus kelipatan 100.");

                                }
                                //add by nurul 31/1/2019
                                if (DateTime.Now >= tglmulai && DateTime.Now <= tglakhir)
                                {
                                    validPrice = false;
                                    listError.Add(i + "_errortext_" + "Harga barang tidak dapat di update, karena sedang dalam masa promosi !");
                                }
                                //end add by nurul 31/1/2019
                            }
                            i++;
                            //end add validasi harga per marketplace
                        }
                        if (validPrice)
                        {

                            //add by calvin 1 maret 2019
                            Dictionary<string, string> extra_image_uploaded = new Dictionary<string, string>();
                            Dictionary<int, string> same_uploaded = new Dictionary<int, string>();
                            if (Request.Files.Count > 0)
                            {
                                for (int file_index = 0; file_index < Request.Files.Count; file_index++)
                                {
                                    string key = Request.Files.GetKey(file_index);
                                    string[] key_split = key.Split(';');
                                    if (key_split.Count() > 1)
                                    {
                                        #region Extra Image
                                        int urutan = Convert.ToInt32(key_split[0]);
                                        int idmarket = Convert.ToInt32(key_split[1]);
                                        var file = Request.Files[file_index];

                                        if (file != null && file.ContentLength > 0)
                                        {
                                            if (!same_uploaded.ContainsKey(file.ContentLength))
                                            {
                                                ImgurImageResponse image = UploadImageService.UploadSingleImageToImgur(file, "uploaded-image");
                                                same_uploaded.Add(file.ContentLength, image.data.link_l);
                                                extra_image_uploaded.Add(Convert.ToString(urutan) + ";" + Convert.ToString(idmarket) + ";" + Convert.ToString(file.ContentLength), image.data.link_l);
                                            }
                                            else
                                            {
                                                extra_image_uploaded.Add(Convert.ToString(urutan) + ";" + Convert.ToString(idmarket) + ";" + Convert.ToString(file.ContentLength), same_uploaded.Where(p => p.Key == file.ContentLength).FirstOrDefault().Value);
                                            }
                                        }
                                        #endregion
                                    }
                                }
                            }
                            //end add by calvin 1 maret 2019

                            foreach (var dataBaru in dataBarang.ListHargaJualPermarket)
                            {
                                var dataHarga = ErasoftDbContext.STF02H.SingleOrDefault(h => h.RecNum == dataBaru.RecNum);
                                if (dataHarga == null)
                                {
                                    dataBaru.BRG = barangInDb.BRG;

                                    //add by calvin 1 maret 2019
                                    if (extra_image_uploaded.Count() > 0)
                                    {
                                        foreach (var extra_image in extra_image_uploaded)
                                        {
                                            string[] key_split = extra_image.Key.Split(';');
                                            int urutan = Convert.ToInt32(key_split[0]);
                                            int idmarket = Convert.ToInt32(key_split[1]);
                                            string idGambar = Convert.ToString(key_split[2]);
                                            if (idmarket == dataBaru.IDMARKET)
                                            {
                                                switch (urutan)
                                                {
                                                    case 1:
                                                        dataBaru.ACODE_50 = idGambar;
                                                        dataBaru.AVALUE_50 = extra_image.Value;
                                                        break;
                                                    case 2:
                                                        dataBaru.ACODE_49 = idGambar;
                                                        dataBaru.AVALUE_49 = extra_image.Value;
                                                        break;
                                                    case 3:
                                                        dataBaru.ACODE_48 = idGambar;
                                                        dataBaru.AVALUE_48 = extra_image.Value;
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                    //end add by calvin 1 maret 2019

                                    ErasoftDbContext.STF02H.Add(dataBaru);
                                }
                                else
                                {
                                    //add by Tri update harga di marketplace
                                    if (dataHarga.HJUAL != dataBaru.HJUAL)
                                    {
                                        updateHarga = true;
                                    }
                                    //end add by Tri update harga di marketplace
                                    dataHarga.HJUAL = dataBaru.HJUAL;

                                    if (dataHarga.DISPLAY != dataBaru.DISPLAY)
                                    {
                                        updateDisplay = true;
                                    }
                                    dataHarga.DISPLAY = dataBaru.DISPLAY;
                                    #region Category && Attribute
                                    dataHarga.CATEGORY_CODE = dataBaru.CATEGORY_CODE;
                                    dataHarga.CATEGORY_NAME = dataBaru.CATEGORY_NAME;
                                    dataHarga.DeliveryTempElevenia = dataBaru.DeliveryTempElevenia;
                                    dataHarga.PICKUP_POINT = dataBaru.PICKUP_POINT;
                                    dataHarga.ACODE_1 = dataBaru.ACODE_1;
                                    dataHarga.ACODE_2 = dataBaru.ACODE_2;
                                    dataHarga.ACODE_3 = dataBaru.ACODE_3;
                                    dataHarga.ACODE_4 = dataBaru.ACODE_4;
                                    dataHarga.ACODE_5 = dataBaru.ACODE_5;
                                    dataHarga.ACODE_6 = dataBaru.ACODE_6;
                                    dataHarga.ACODE_7 = dataBaru.ACODE_7;
                                    dataHarga.ACODE_8 = dataBaru.ACODE_8;
                                    dataHarga.ACODE_9 = dataBaru.ACODE_9;
                                    dataHarga.ACODE_10 = dataBaru.ACODE_10;
                                    dataHarga.ACODE_11 = dataBaru.ACODE_11;
                                    dataHarga.ACODE_12 = dataBaru.ACODE_12;
                                    dataHarga.ACODE_13 = dataBaru.ACODE_13;
                                    dataHarga.ACODE_14 = dataBaru.ACODE_14;
                                    dataHarga.ACODE_15 = dataBaru.ACODE_15;
                                    dataHarga.ACODE_16 = dataBaru.ACODE_16;
                                    dataHarga.ACODE_17 = dataBaru.ACODE_17;
                                    dataHarga.ACODE_18 = dataBaru.ACODE_18;
                                    dataHarga.ACODE_19 = dataBaru.ACODE_19;
                                    dataHarga.ACODE_20 = dataBaru.ACODE_20;
                                    dataHarga.ACODE_21 = dataBaru.ACODE_21;
                                    dataHarga.ACODE_22 = dataBaru.ACODE_22;
                                    dataHarga.ACODE_23 = dataBaru.ACODE_23;
                                    dataHarga.ACODE_24 = dataBaru.ACODE_24;
                                    dataHarga.ACODE_25 = dataBaru.ACODE_25;
                                    dataHarga.ACODE_26 = dataBaru.ACODE_26;
                                    dataHarga.ACODE_27 = dataBaru.ACODE_27;
                                    dataHarga.ACODE_28 = dataBaru.ACODE_28;
                                    dataHarga.ACODE_29 = dataBaru.ACODE_29;
                                    dataHarga.ACODE_30 = dataBaru.ACODE_30;
                                    dataHarga.ACODE_31 = dataBaru.ACODE_31;
                                    dataHarga.ACODE_32 = dataBaru.ACODE_32;
                                    dataHarga.ACODE_33 = dataBaru.ACODE_33;
                                    dataHarga.ACODE_34 = dataBaru.ACODE_34;
                                    dataHarga.ACODE_35 = dataBaru.ACODE_35;
                                    dataHarga.ACODE_36 = dataBaru.ACODE_36;
                                    dataHarga.ACODE_37 = dataBaru.ACODE_37;
                                    dataHarga.ACODE_38 = dataBaru.ACODE_38;
                                    dataHarga.ACODE_39 = dataBaru.ACODE_39;
                                    dataHarga.ACODE_40 = dataBaru.ACODE_40;
                                    dataHarga.ACODE_41 = dataBaru.ACODE_41;
                                    dataHarga.ACODE_42 = dataBaru.ACODE_42;
                                    dataHarga.ACODE_43 = dataBaru.ACODE_43;
                                    dataHarga.ACODE_44 = dataBaru.ACODE_44;
                                    dataHarga.ACODE_45 = dataBaru.ACODE_45;
                                    dataHarga.ACODE_46 = dataBaru.ACODE_46;
                                    dataHarga.ACODE_47 = dataBaru.ACODE_47;
                                    //remark by calvin 1 maret 2019, dipakai untuk gambar
                                    //dataHarga.ACODE_48 = dataBaru.ACODE_48;
                                    //dataHarga.ACODE_49 = dataBaru.ACODE_49;
                                    //dataHarga.ACODE_50 = dataBaru.ACODE_50;
                                    //remark by calvin 1 maret 2019, dipakai untuk gambar

                                    dataHarga.ANAME_1 = dataBaru.ANAME_1;
                                    dataHarga.ANAME_2 = dataBaru.ANAME_2;
                                    dataHarga.ANAME_3 = dataBaru.ANAME_3;
                                    dataHarga.ANAME_4 = dataBaru.ANAME_4;
                                    dataHarga.ANAME_5 = dataBaru.ANAME_5;
                                    dataHarga.ANAME_6 = dataBaru.ANAME_6;
                                    dataHarga.ANAME_7 = dataBaru.ANAME_7;
                                    dataHarga.ANAME_8 = dataBaru.ANAME_8;
                                    dataHarga.ANAME_9 = dataBaru.ANAME_9;
                                    dataHarga.ANAME_10 = dataBaru.ANAME_10;
                                    dataHarga.ANAME_11 = dataBaru.ANAME_11;
                                    dataHarga.ANAME_12 = dataBaru.ANAME_12;
                                    dataHarga.ANAME_13 = dataBaru.ANAME_13;
                                    dataHarga.ANAME_14 = dataBaru.ANAME_14;
                                    dataHarga.ANAME_15 = dataBaru.ANAME_15;
                                    dataHarga.ANAME_16 = dataBaru.ANAME_16;
                                    dataHarga.ANAME_17 = dataBaru.ANAME_17;
                                    dataHarga.ANAME_18 = dataBaru.ANAME_18;
                                    dataHarga.ANAME_19 = dataBaru.ANAME_19;
                                    dataHarga.ANAME_20 = dataBaru.ANAME_20;
                                    dataHarga.ANAME_21 = dataBaru.ANAME_21;
                                    dataHarga.ANAME_22 = dataBaru.ANAME_22;
                                    dataHarga.ANAME_23 = dataBaru.ANAME_23;
                                    dataHarga.ANAME_24 = dataBaru.ANAME_24;
                                    dataHarga.ANAME_25 = dataBaru.ANAME_25;
                                    dataHarga.ANAME_26 = dataBaru.ANAME_26;
                                    dataHarga.ANAME_27 = dataBaru.ANAME_27;
                                    dataHarga.ANAME_28 = dataBaru.ANAME_28;
                                    dataHarga.ANAME_29 = dataBaru.ANAME_29;
                                    dataHarga.ANAME_30 = dataBaru.ANAME_30;
                                    dataHarga.ANAME_31 = dataBaru.ANAME_31;
                                    dataHarga.ANAME_32 = dataBaru.ANAME_32;
                                    dataHarga.ANAME_33 = dataBaru.ANAME_33;
                                    dataHarga.ANAME_34 = dataBaru.ANAME_34;
                                    dataHarga.ANAME_35 = dataBaru.ANAME_35;
                                    dataHarga.ANAME_36 = dataBaru.ANAME_36;
                                    dataHarga.ANAME_37 = dataBaru.ANAME_37;
                                    dataHarga.ANAME_38 = dataBaru.ANAME_38;
                                    dataHarga.ANAME_39 = dataBaru.ANAME_39;
                                    dataHarga.ANAME_40 = dataBaru.ANAME_40;
                                    dataHarga.ANAME_41 = dataBaru.ANAME_41;
                                    dataHarga.ANAME_42 = dataBaru.ANAME_42;
                                    dataHarga.ANAME_43 = dataBaru.ANAME_43;
                                    dataHarga.ANAME_44 = dataBaru.ANAME_44;
                                    dataHarga.ANAME_45 = dataBaru.ANAME_45;
                                    dataHarga.ANAME_46 = dataBaru.ANAME_46;
                                    dataHarga.ANAME_47 = dataBaru.ANAME_47;
                                    //remark by calvin 1 maret 2019, dipakai untuk gambar
                                    //dataHarga.ANAME_48 = dataBaru.ANAME_48;
                                    //dataHarga.ANAME_49 = dataBaru.ANAME_49;
                                    //dataHarga.ANAME_50 = dataBaru.ANAME_50;
                                    //remark by calvin 1 maret 2019, dipakai untuk gambar

                                    dataHarga.AVALUE_1 = dataBaru.AVALUE_1;
                                    dataHarga.AVALUE_2 = dataBaru.AVALUE_2;
                                    dataHarga.AVALUE_3 = dataBaru.AVALUE_3;
                                    dataHarga.AVALUE_4 = dataBaru.AVALUE_4;
                                    dataHarga.AVALUE_5 = dataBaru.AVALUE_5;
                                    dataHarga.AVALUE_6 = dataBaru.AVALUE_6;
                                    dataHarga.AVALUE_7 = dataBaru.AVALUE_7;
                                    dataHarga.AVALUE_8 = dataBaru.AVALUE_8;
                                    dataHarga.AVALUE_9 = dataBaru.AVALUE_9;
                                    dataHarga.AVALUE_10 = dataBaru.AVALUE_10;
                                    dataHarga.AVALUE_11 = dataBaru.AVALUE_11;
                                    dataHarga.AVALUE_12 = dataBaru.AVALUE_12;
                                    dataHarga.AVALUE_13 = dataBaru.AVALUE_13;
                                    dataHarga.AVALUE_14 = dataBaru.AVALUE_14;
                                    dataHarga.AVALUE_15 = dataBaru.AVALUE_15;
                                    dataHarga.AVALUE_16 = dataBaru.AVALUE_16;
                                    dataHarga.AVALUE_17 = dataBaru.AVALUE_17;
                                    dataHarga.AVALUE_18 = dataBaru.AVALUE_18;
                                    dataHarga.AVALUE_19 = dataBaru.AVALUE_19;
                                    dataHarga.AVALUE_20 = dataBaru.AVALUE_20;
                                    dataHarga.AVALUE_21 = dataBaru.AVALUE_21;
                                    dataHarga.AVALUE_22 = dataBaru.AVALUE_22;
                                    dataHarga.AVALUE_23 = dataBaru.AVALUE_23;
                                    dataHarga.AVALUE_24 = dataBaru.AVALUE_24;
                                    dataHarga.AVALUE_25 = dataBaru.AVALUE_25;
                                    dataHarga.AVALUE_26 = dataBaru.AVALUE_26;
                                    dataHarga.AVALUE_27 = dataBaru.AVALUE_27;
                                    dataHarga.AVALUE_28 = dataBaru.AVALUE_28;
                                    dataHarga.AVALUE_29 = dataBaru.AVALUE_29;
                                    dataHarga.AVALUE_30 = dataBaru.AVALUE_30;
                                    dataHarga.AVALUE_31 = dataBaru.AVALUE_31;
                                    dataHarga.AVALUE_32 = dataBaru.AVALUE_32;
                                    dataHarga.AVALUE_33 = dataBaru.AVALUE_33;
                                    dataHarga.AVALUE_34 = dataBaru.AVALUE_34;
                                    dataHarga.AVALUE_35 = dataBaru.AVALUE_35;
                                    dataHarga.AVALUE_36 = dataBaru.AVALUE_36;
                                    dataHarga.AVALUE_37 = dataBaru.AVALUE_37;
                                    dataHarga.AVALUE_38 = dataBaru.AVALUE_38;
                                    dataHarga.AVALUE_39 = dataBaru.AVALUE_39;
                                    dataHarga.AVALUE_40 = dataBaru.AVALUE_40;
                                    dataHarga.AVALUE_41 = dataBaru.AVALUE_41;
                                    dataHarga.AVALUE_42 = dataBaru.AVALUE_42;
                                    dataHarga.AVALUE_43 = dataBaru.AVALUE_43;
                                    dataHarga.AVALUE_44 = dataBaru.AVALUE_44;
                                    dataHarga.AVALUE_45 = dataBaru.AVALUE_45;
                                    dataHarga.AVALUE_46 = dataBaru.AVALUE_46;
                                    dataHarga.AVALUE_47 = dataBaru.AVALUE_47;
                                    //remark by calvin 1 maret 2019, dipakai untuk gambar
                                    //dataHarga.AVALUE_48 = dataBaru.AVALUE_48;
                                    //dataHarga.AVALUE_49 = dataBaru.AVALUE_49;
                                    //dataHarga.AVALUE_50 = dataBaru.AVALUE_50;
                                    //end remark by calvin 1 maret 2019
                                    #endregion

                                    //add by calvin 1 maret 2019
                                    if (extra_image_uploaded.Count() > 0)
                                    {
                                        foreach (var extra_image in extra_image_uploaded)
                                        {
                                            string[] key_split = extra_image.Key.Split(';');
                                            int urutan = Convert.ToInt32(key_split[0]);
                                            int idmarket = Convert.ToInt32(key_split[1]);
                                            string idGambar = Convert.ToString(key_split[2]);
                                            if (idmarket == dataHarga.IDMARKET)
                                            {
                                                switch (urutan)
                                                {
                                                    case 1:
                                                        dataHarga.ACODE_50 = idGambar;
                                                        dataHarga.AVALUE_50 = extra_image.Value;
                                                        break;
                                                    case 2:
                                                        dataHarga.ACODE_49 = idGambar;
                                                        dataHarga.AVALUE_49 = extra_image.Value;
                                                        break;
                                                    case 3:
                                                        dataHarga.ACODE_48 = idGambar;
                                                        dataHarga.AVALUE_48 = extra_image.Value;
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                    //end add by calvin 1 maret 2019
                                }
                            }
                        }
                        else
                        {
                            dataBarang.errorHargaPerMP = "1";
                            dataBarang.Errors = listError;
                            return Json(dataBarang, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (Request.Files.Count > 0)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            var file = Request.Files[i];

                            if (file != null && file.ContentLength > 0)
                            {
                                //var namaFile = $"FotoProduk-{dataBarang.Stf02.USERNAME}-BRG{barangInDb.BRG}-foto-{i + 1}";
                                ImgurImageResponse image = UploadImageService.UploadSingleImageToImgur(file, "uploaded-image");

                                //updateGambar = true;
                                //var fileExtension = Path.GetExtension(file.FileName);
                                //var namaFile = $"FotoProduk-{barangInDb.USERNAME}-{barangInDb.BRG}-foto-{i + 1}{fileExtension}";
                                //var path = Path.Combine(Server.MapPath("~/Content/Uploaded/"), namaFile);
                                //file.SaveAs(path);
                                ////add by tri
                                //imgPath[i] = path;

                                imgPath[i] = image.data.link;

                                switch (i)
                                {
                                    case 0:
                                        barangInDb.LINK_GAMBAR_1 = image.data.link_l;
                                        barangInDb.Sort5 = Convert.ToString(file.ContentLength);
                                        break;
                                    case 1:
                                        barangInDb.LINK_GAMBAR_2 = image.data.link_l;
                                        barangInDb.Sort6 = Convert.ToString(file.ContentLength);
                                        break;
                                    case 2:
                                        barangInDb.LINK_GAMBAR_3 = image.data.link_l;
                                        barangInDb.Sort7 = Convert.ToString(file.ContentLength);
                                        break;
                                }
                            }
                        }
                    }
                    //add by calvin 16 nov 2018, imgpath saat update
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                imgPath[0] = barangInDb.LINK_GAMBAR_1;
                                break;
                            case 1:
                                imgPath[1] = barangInDb.LINK_GAMBAR_2;
                                break;
                            case 2:
                                imgPath[2] = barangInDb.LINK_GAMBAR_3;
                                break;
                        }
                    }
                    //end add by calvin
                }
            }

            ErasoftDbContext.SaveChanges();
            bool doSync = true;
            if (doSync)
            {
                #region Sync ke Marketplace
                //var kdBL = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BUKALAPAK");//moved to top
                var listBLShop = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdBL.IdMarket.ToString()).ToList();
                //var kdLazada = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "LAZADA");//moved to top
                var listLazadaShop = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdLazada.IdMarket.ToString()).ToList();
                string[] imageUrl = new string[Request.Files.Count];//variabel penampung url image hasil upload ke markeplace
                var lzdApi = new LazadaController();
                var blApi = new BukaLapakController();

                //add by tri call marketplace api to create product
                if (insert)
                {
                    var barangInDb = ErasoftDbContext.STF02.SingleOrDefault(b => b.ID == dataBarang.Stf02.ID || b.BRG == dataBarang.Stf02.BRG);
                    #region lazada
                    if (listLazadaShop.Count > 0)
                    {
                        foreach (ARF01 tblCustomer in listLazadaShop)
                        {
                            createBarangLazada(dataBarang, imgPath, tblCustomer);

                            //        var productMarketPlace = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == barangInDb.BRG && m.IDMARKET == tblCustomer.RecNum);
                            //        if (!string.IsNullOrEmpty(tblCustomer.TOKEN) && productMarketPlace.DISPLAY)
                            //        {
                            //            //string[] imageUrl = new string[Request.Files.Count];
                            //            for (int i = 0; i < imgPath.Length; i++)
                            //            {
                            //                if (!string.IsNullOrEmpty(imgPath[i]))
                            //                {
                            //                    var uploadImg = lzdApi.UploadImage(imgPath[i], tblCustomer.TOKEN);
                            //                    if (uploadImg.status == 1)
                            //                        imageUrl[i] = uploadImg.message;
                            //                }
                            //            }

                            //            //string[] imgID = new string[3];
                            //            //for (int i = 0; i < 3; i++)
                            //            //{
                            //            //    //    if (!string.IsNullOrEmpty(imgPath[i]))
                            //            //    //    {
                            //            //    imageUrl[i] = "http://masteronline.co.id/ele/image?id=" + $"FotoProduk-{barangInDb.USERNAME}-{barangInDb.BRG}-foto-{i + 1}.jpg";
                            //            //    imageUrl[i] = Convert.ToString(imageUrl[i]).Replace(" ", "%20");
                            //            //    //    }
                            //            //}

                            //            BrgViewModel dataLazada = new BrgViewModel
                            //            {
                            //                deskripsi = dataBarang.Stf02.Deskripsi,
                            //                harga = dataBarang.Stf02.HJUAL.ToString(),
                            //                height = dataBarang.Stf02.TINGGI.ToString(),
                            //                kdBrg = barangInDb.BRG,
                            //                length = dataBarang.Stf02.PANJANG.ToString(),
                            //                nama = dataBarang.Stf02.NAMA,
                            //                nama2 = dataBarang.Stf02.NAMA2,
                            //                weight = dataBarang.Stf02.BERAT.ToString(),
                            //                width = dataBarang.Stf02.LEBAR.ToString(),
                            //                user = tblCustomer.EMAIL,
                            //                key = tblCustomer.API_KEY,
                            //                qty = "1",
                            //                token = tblCustomer.TOKEN,
                            //                idMarket = tblCustomer.RecNum.ToString(),
                            //            };

                            //            ////string[] imgID = new string[3];
                            //            ////if (Request.Files.Count > 0)
                            //            ////{
                            //            //for (int i = 0; i < 3; i++)
                            //            //{
                            //            //    //var file = Request.Files[i];

                            //            //    //if (file != null && file.ContentLength > 0)
                            //            //    //{
                            //            //    //    var fileExtension = Path.GetExtension(file.FileName);
                            //            //    imageUrl[i] = "http://masteronline.co.id/ele/image?id=" + $"FotoProduk-{dataBarang.Username}-{dataBarang.Stf02.BRG}-foto-{i + 1}.jpg";
                            //            //    imageUrl[i] = Convert.ToString(imageUrl[i]).Replace(" ", "%20");
                            //            //    //}
                            //            //}

                            //            dataLazada.merk = ErasoftDbContext.STF02E.SingleOrDefault(m => m.KODE == dataBarang.Stf02.Sort2 && m.LEVEL == "2").KET;
                            //            //var productMarketPlace = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == barangInDb.BRG && m.IDMARKET == tblCustomer.RecNum);
                            //            dataLazada.harga = productMarketPlace.HJUAL.ToString();
                            //            dataLazada.activeProd = productMarketPlace.DISPLAY;

                            //            if (!string.IsNullOrEmpty(imageUrl[2]))
                            //            {
                            //                dataLazada.imageUrl3 = imageUrl[2];
                            //            }
                            //            if (!string.IsNullOrEmpty(imageUrl[1]))
                            //            {
                            //                dataLazada.imageUrl2 = imageUrl[1];
                            //            }
                            //            if (!string.IsNullOrEmpty(imageUrl[0]))
                            //            {
                            //                dataLazada.imageUrl = imageUrl[0];
                            //            }
                            //            var result = lzdApi.CreateProduct(dataLazada);
                            //        }

                        }
                    }
                    #endregion
                    #region Bukalapak
                    //var kdBL = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BUKALAPAK");
                    //var listBLShop = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdBL.IdMarket.ToString()).ToList();
                    if (listBLShop.Count > 0)
                    {
                        foreach (ARF01 tblCustomer in listBLShop)
                        {
                            var productMarketPlace = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == barangInDb.BRG && m.IDMARKET == tblCustomer.RecNum);
                            if (productMarketPlace.DISPLAY)
                            {
                                createBarangBukaLapak(dataBarang, imgPath, tblCustomer);
                                //string[] imgID = new string[Request.Files.Count];
                                //for (int i = 0; i < imgPath.Length; i++)
                                //{
                                //    if (!string.IsNullOrEmpty(imgPath[i]))
                                //    {
                                //        var uploadImg = blApi.uploadGambar(imgPath[i], tblCustomer.API_KEY, tblCustomer.TOKEN);
                                //        if (uploadImg.status == 1)
                                //            imgID[i] = uploadImg.message;
                                //    }
                                //}
                                //BrgViewModel data = new BrgViewModel
                                //{
                                //    deskripsi = dataBarang.Stf02.Deskripsi,
                                //    harga = dataBarang.Stf02.HJUAL.ToString(),
                                //    height = dataBarang.Stf02.TINGGI.ToString(),
                                //    kdBrg = barangInDb.BRG,
                                //    length = dataBarang.Stf02.PANJANG.ToString(),
                                //    nama = dataBarang.Stf02.NAMA,
                                //    nama2 = dataBarang.Stf02.NAMA2,
                                //    weight = dataBarang.Stf02.BERAT.ToString(),
                                //    width = dataBarang.Stf02.LEBAR.ToString(),
                                //    user = tblCustomer.EMAIL,
                                //    key = tblCustomer.API_KEY,
                                //    qty = "1",
                                //    token = tblCustomer.TOKEN,
                                //    idMarket = tblCustomer.RecNum.ToString(),
                                //    //merk = ErasoftDbContext.STF02E.SingleOrDefault(m => m.KODE == dataBarang.Stf02.Sort2 && m.LEVEL == "2").KET
                                //};
                                //data.merk = ErasoftDbContext.STF02E.SingleOrDefault(m => m.KODE == dataBarang.Stf02.Sort2 && m.LEVEL == "2").KET;
                                ////var productMarketPlace = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == barangInDb.BRG && m.IDMARKET == tblCustomer.RecNum);
                                //data.harga = productMarketPlace.HJUAL.ToString();
                                //if (!string.IsNullOrEmpty(imgID[2]))
                                //{
                                //    data.imageId3 = imgID[2];
                                //}
                                //if (!string.IsNullOrEmpty(imgID[1]))
                                //{
                                //    data.imageId2 = imgID[1];
                                //}
                                //if (!string.IsNullOrEmpty(imgID[0]))
                                //{
                                //    data.imageId = imgID[0];
                                //}

                                //var result = blApi.CreateProduct(data);
                                //if (result.status == 1)
                                //    if (!productMarketPlace.DISPLAY)
                                //    {
                                //        //panggil api utk non-aktif barang yg baru di insert
                                //        result = blApi.prodNonAktif(barangInDb.BRG, result.message, tblCustomer.API_KEY, tblCustomer.TOKEN);
                                //    }
                            }

                        }
                    }
                    #endregion
                    #region Elevenia
                    saveBarangElevenia(1, dataBarang, imgPath);
                    #endregion
                    #region Blibli
                    saveBarangBlibli(1, dataBarang);
                    #endregion
                    saveBarangShopee(1, dataBarang, false);
                    saveBarangTokpedVariant(1, barangInDb.BRG, false);
                }
                //end add by tri call marketplace api to create product
                else
                {
                    //saveBarangBlibli(1, dataBarang);
                    //update harga, qty, dll

                    saveBarangBlibli(2, dataBarang);
                    saveBarangElevenia(2, dataBarang, imgPath);
                    saveBarangShopee(2, dataBarang, updateHarga);


                    //get image
                    var barang = ErasoftDbContext.STF02.SingleOrDefault(b => b.ID == dataBarang.Stf02.ID);
                    //string[] picPath = new string[3];
                    //for (int i = 0; i < 3; i++)
                    //{
                    //    string picName = $"FotoProduk-{barang.USERNAME}-{barang.BRG}-foto-{i + 1}.jpg";
                    //    if (System.IO.File.Exists(Server.MapPath("/Content/Uploaded/" + picName)))
                    //    {
                    //        picPath[i] = Server.MapPath("/Content/Uploaded/" + picName);
                    //    }
                    //}
                    //end get image

                    saveBarangTokpedVariant(2, barang.BRG, false);

                    if (updateDisplay)
                    {
                        #region lazada
                        if (listLazadaShop.Count > 0)
                        {
                            foreach (ARF01 tblCustomer in listLazadaShop)
                            {
                                if (!string.IsNullOrEmpty(tblCustomer.TOKEN))
                                {
                                    var tokoLazada = ErasoftDbContext.STF02H.SingleOrDefault(h => h.IDMARKET == tblCustomer.RecNum && h.BRG == barang.BRG);
                                    if (tokoLazada.DISPLAY && string.IsNullOrEmpty(tokoLazada.BRG_MP))//display = true and brg_mp = null -> create product
                                    {
                                        createBarangLazada(dataBarang, imgPath, tblCustomer);
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(tokoLazada.BRG_MP))
                                        {
                                            var resultLazada = lzdApi.setDisplay(tokoLazada.BRG_MP, tokoLazada.DISPLAY, tblCustomer.TOKEN);
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        #region Elevenia
                        saveBarangElevenia(3, dataBarang, imgPath);
                        #endregion
                        #region Bukalapak
                        if (listBLShop.Count > 0)
                        {
                            foreach (ARF01 tblCustomer in listBLShop)
                            {
                                var tokoBl = ErasoftDbContext.STF02H.SingleOrDefault(h => h.IDMARKET == tblCustomer.RecNum && h.BRG == barang.BRG);
                                if (tokoBl.DISPLAY)
                                {
                                    if (string.IsNullOrEmpty(tokoBl.BRG_MP))
                                    {
                                        createBarangBukaLapak(dataBarang, imgPath, tblCustomer);
                                    }
                                    else
                                    {
                                        var result = blApi.prodAktif(barang.BRG, tokoBl.BRG_MP, tblCustomer.API_KEY, tblCustomer.TOKEN);
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(tokoBl.BRG_MP))
                                    {
                                        var result = blApi.prodNonAktif(barang.BRG, tokoBl.BRG_MP, tblCustomer.API_KEY, tblCustomer.TOKEN);
                                    }

                                }

                            }
                        }
                        #endregion
                    }
                    if (updateHarga)
                    {
                        #region lazada
                        if (listLazadaShop.Count > 0)
                        {
                            foreach (ARF01 tblCustomer in listLazadaShop)
                            {
                                if (!string.IsNullOrEmpty(tblCustomer.TOKEN))
                                {
                                    //var barang = ErasoftDbContext.STF02.SingleOrDefault(b => b.ID == dataBarang.Stf02.ID);
                                    var tokoLazada = ErasoftDbContext.STF02H.SingleOrDefault(h => h.IDMARKET == tblCustomer.RecNum && h.BRG == barang.BRG);
                                    var resultLazada = lzdApi.UpdatePriceQuantity(tokoLazada.BRG_MP, tokoLazada.HJUAL.ToString(), "", tblCustomer.TOKEN);
                                }
                            }
                        }
                        #endregion
                        #region Bukalapak
                        if (listBLShop.Count > 0)
                        {
                            foreach (ARF01 tblCustomer in listBLShop)
                            {
                                //var barang = ErasoftDbContext.STF02.SingleOrDefault(b => b.ID == dataBarang.Stf02.ID);
                                var tokoBl = ErasoftDbContext.STF02H.SingleOrDefault(h => h.IDMARKET == tblCustomer.RecNum && h.BRG == barang.BRG);
                                var resultBL = blApi.updateProduk(barang.BRG, tokoBl.BRG_MP, tokoBl.HJUAL.ToString(), "", tblCustomer.API_KEY, tblCustomer.TOKEN);
                            }
                        }

                        #endregion
                    }
                }
                #endregion
            }
            ModelState.Clear();

            var partialVm = new BarangViewModel()
            {
                //change by nurul 18/1/2019 -- ListStf02S = ErasoftDbContext.STF02.ToList(),
                ListStf02S = ErasoftDbContext.STF02.Where(p => (p.PART == null ? "" : p.PART) == "").ToList(),
                ListHargaJualPermarketView = ErasoftDbContext.STF02H.Where(p => 0 == 1).OrderBy(p => p.IDMARKET).ToList(),
            };

            return PartialView("TableBarang1Partial", partialVm);
        }

        [HttpPost]
        public ActionResult SaveBarangInduk(BarangViewModel dataBarang)
        {
            if (!ModelState.IsValid)
            {
                dataBarang.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(dataBarang, JsonRequestBehavior.AllowGet);
            }
            string KodeBarang = "";
            bool insert = false;//add by Tri
            bool updateHarga = false;//add by Tri
            bool updateDisplay = false;//add by Tri
            bool updateGambar = false;//add by Tri
            var Marketplaces = MoDbContext.Marketplaces.ToList();
            var kdBL = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BUKALAPAK");
            var kdLazada = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "LAZADA");
            var kdBlibli = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BLIBLI");
            var kdElevenia = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "ELEVENIA");
            var kdShopee = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "SHOPEE");
            var kdTokped = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "TOKOPEDIA");
            var validPrice = true;

            string[] imgPath = new string[Request.Files.Count];
            if (dataBarang.Stf02.ID == null)
            {
                insert = true;
                KodeBarang = dataBarang.Stf02.BRG;
                if (dataBarang.ListHargaJualPermarket?.Count > 0)
                {
                    List<string> listError = new List<string>();
                    int i = 0;
                    foreach (var hargaPerMarket in dataBarang.ListHargaJualPermarket)
                    {
                        var kdMarket = ErasoftDbContext.ARF01.Where(m => m.RecNum == hargaPerMarket.IDMARKET).SingleOrDefault().NAMA;
                        if (kdMarket == kdLazada.IdMarket.ToString())
                        {
                            if (hargaPerMarket.HJUAL < 3000)
                            {
                                validPrice = false;
                                listError.Add(i + "_errortext_" + "Harga Jual harus lebih dari 3000.");
                            }
                            else if (hargaPerMarket.HJUAL % 100 != 0)
                            {
                                validPrice = false;
                                listError.Add(i + "_errortext_" + "Harga Jual harus kelipatan 100.");

                            }
                        }
                        else if (kdMarket == kdBlibli.IdMarket.ToString())
                        {
                            if (hargaPerMarket.HJUAL < 1100)
                            {
                                validPrice = false;
                                listError.Add(i + "_errortext_" + "Harga Jual minimal 1100.");
                            }
                        }
                        else if (kdMarket == kdBL.IdMarket.ToString() || kdMarket == kdElevenia.IdMarket.ToString())
                        {
                            if (hargaPerMarket.HJUAL < 100)
                            {
                                validPrice = false;
                                listError.Add(i + "_errortext_" + "Harga Jual harus lebih dari 100.");
                            }
                            else if (hargaPerMarket.HJUAL % 100 != 0)
                            {
                                validPrice = false;
                                listError.Add(i + "_errortext_" + "Harga Jual harus kelipatan 100.");

                            }
                        }
                        i++;
                    }
                    if (validPrice)
                    {
                        foreach (var hargaPerMarket in dataBarang.ListHargaJualPermarket)
                        {
                            hargaPerMarket.BRG = dataBarang.Stf02.BRG;
                            ErasoftDbContext.STF02H.Add(hargaPerMarket);
                        }
                    }
                    else
                    {
                        dataBarang.errorHargaPerMP = "1";
                        dataBarang.Errors = listError;
                        return Json(dataBarang, JsonRequestBehavior.AllowGet);
                    }

                }

                var listMarket = dataBarang.ListMarket.ToList();

                //remark by Tri, moved to top
                //add by tri
                //string[] imgPath = new string[Request.Files.Count];
                //end add by tri
                //end remark by Tri, moved to top

                if (Request.Files.Count > 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var file = Request.Files[i];

                        if (file != null && file.ContentLength > 0)
                        {
                            //var namaFile = $"FotoProduk-{dataBarang.Stf02.USERNAME}-BRG{dataBarang.Stf02.BRG}-foto-{i + 1}";
                            ImgurImageResponse image = UploadImageService.UploadSingleImageToImgur(file, "uploaded-image");

                            //var fileExtension = Path.GetExtension(file.FileName);
                            //var path = Path.Combine(Server.MapPath("~/Content/Uploaded/"), namaFile);
                            //try
                            //{
                            //    file.SaveAs(path);
                            //}
                            //catch (Exception ex)
                            //{

                            //}
                            //add by tri

                            imgPath[i] = image.data.link;

                            switch (i)
                            {
                                case 0:
                                    dataBarang.Stf02.LINK_GAMBAR_1 = image.data.link_l;
                                    dataBarang.Stf02.Sort5 = Convert.ToString(file.ContentLength);
                                    break;
                                case 1:
                                    dataBarang.Stf02.LINK_GAMBAR_2 = image.data.link_l;
                                    dataBarang.Stf02.Sort6 = Convert.ToString(file.ContentLength);
                                    break;
                                case 2:
                                    dataBarang.Stf02.LINK_GAMBAR_3 = image.data.link_l;
                                    dataBarang.Stf02.Sort7 = Convert.ToString(file.ContentLength);
                                    break;
                            }
                        }
                    }
                }

                for (int i = 0; i < Request.Files.Count; i++)
                {
                    switch (i)
                    {
                        case 0:
                            imgPath[0] = dataBarang.Stf02.LINK_GAMBAR_1;
                            break;
                        case 1:
                            imgPath[1] = dataBarang.Stf02.LINK_GAMBAR_2;
                            break;
                        case 2:
                            imgPath[2] = dataBarang.Stf02.LINK_GAMBAR_3;
                            break;
                    }
                }

                if (string.IsNullOrWhiteSpace(dataBarang.Stf02.TYPE))
                {
                    dataBarang.Stf02.TYPE = "4";
                }
                ErasoftDbContext.STF02.Add(dataBarang.Stf02);
            }
            else
            {
                var barangInDb = ErasoftDbContext.STF02.SingleOrDefault(b => b.ID == dataBarang.Stf02.ID);

                if (barangInDb != null)
                {
                    KodeBarang = barangInDb.BRG;
                    barangInDb.NAMA = dataBarang.Stf02.NAMA;
                    barangInDb.NAMA2 = dataBarang.Stf02.NAMA2;
                    barangInDb.MINI = dataBarang.Stf02.MINI;
                    barangInDb.MAXI = dataBarang.Stf02.MAXI;
                    barangInDb.Sort1 = dataBarang.Stf02.Sort1;
                    barangInDb.Sort2 = dataBarang.Stf02.Sort2;
                    barangInDb.KET_SORT1 = dataBarang.Stf02.KET_SORT1;
                    barangInDb.KET_SORT2 = dataBarang.Stf02.KET_SORT2;
                    barangInDb.STN = dataBarang.Stf02.STN;
                    barangInDb.STN2 = dataBarang.Stf02.STN2;
                    barangInDb.ISI = dataBarang.Stf02.ISI;
                    barangInDb.Metoda = dataBarang.Stf02.Metoda;
                    barangInDb.Deskripsi = dataBarang.Stf02.Deskripsi;
                    barangInDb.BERAT = dataBarang.Stf02.BERAT;
                    barangInDb.PANJANG = dataBarang.Stf02.PANJANG;
                    barangInDb.LEBAR = dataBarang.Stf02.LEBAR;
                    barangInDb.TINGGI = dataBarang.Stf02.TINGGI;
                    barangInDb.HJUAL = dataBarang.Stf02.HJUAL;
                    barangInDb.TYPE = "4";
                    if (dataBarang.ListHargaJualPermarket?.Count > 0)
                    {
                        List<string> listError = new List<string>();
                        int i = 0;
                        foreach (var dataBaru in dataBarang.ListHargaJualPermarket)
                        {
                            //add validasi harga per marketplace
                            var kdMarket = ErasoftDbContext.ARF01.Where(m => m.RecNum == dataBaru.IDMARKET).SingleOrDefault().NAMA;
                            if (kdMarket == kdLazada.IdMarket.ToString())
                            {
                                if (dataBaru.HJUAL < 3000)
                                {
                                    validPrice = false;
                                    listError.Add(i + "_errortext_" + "Harga Jual harus lebih dari 3000.");
                                }
                                else if (dataBaru.HJUAL % 100 != 0)
                                {
                                    validPrice = false;
                                    listError.Add(i + "_errortext_" + "Harga Jual harus kelipatan 100.");

                                }
                            }
                            else if (kdMarket == kdBlibli.IdMarket.ToString())
                            {
                                if (dataBaru.HJUAL < 1100)
                                {
                                    validPrice = false;
                                    listError.Add(i + "_errortext_" + "Harga Jual minimal 1100.");
                                }
                            }
                            else if (kdMarket == kdBL.IdMarket.ToString() || kdMarket == kdElevenia.IdMarket.ToString())
                            {
                                if (dataBaru.HJUAL < 100)
                                {
                                    validPrice = false;
                                    listError.Add(i + "_errortext_" + "Harga Jual harus lebih dari 100.");
                                }
                                else if (dataBaru.HJUAL % 100 != 0)
                                {
                                    validPrice = false;
                                    listError.Add(i + "_errortext_" + "Harga Jual harus kelipatan 100.");

                                }
                            }
                            i++;
                            //end add validasi harga per marketplace
                        }
                        if (validPrice)
                        {
                            foreach (var dataBaru in dataBarang.ListHargaJualPermarket)
                            {
                                var dataHarga = ErasoftDbContext.STF02H.SingleOrDefault(h => h.BRG == barangInDb.BRG && h.IDMARKET == dataBaru.IDMARKET);
                                if (dataHarga == null)
                                {
                                    dataBaru.BRG = barangInDb.BRG;
                                    ErasoftDbContext.STF02H.Add(dataBaru);
                                }
                                else
                                {
                                    //add by Tri update harga di marketplace
                                    if (dataHarga.HJUAL != dataBaru.HJUAL)
                                    {
                                        updateHarga = true;
                                    }
                                    //end add by Tri update harga di marketplace
                                    dataHarga.HJUAL = dataBaru.HJUAL;
                                    bool updateKategori = false;
                                    if (dataHarga.DISPLAY != dataBaru.DISPLAY)
                                    {
                                        updateDisplay = true;
                                    }
                                    dataHarga.DISPLAY = dataBaru.DISPLAY;
                                    if (dataHarga.CATEGORY_CODE != dataBaru.CATEGORY_CODE)
                                    {
                                        updateKategori = true;
                                    }
                                    #region Category && Attribute
                                    dataHarga.CATEGORY_CODE = dataBaru.CATEGORY_CODE;
                                    dataHarga.CATEGORY_NAME = dataBaru.CATEGORY_NAME;
                                    dataHarga.DeliveryTempElevenia = dataBaru.DeliveryTempElevenia;
                                    dataHarga.PICKUP_POINT = dataBaru.PICKUP_POINT;
                                    dataHarga.ACODE_1 = dataBaru.ACODE_1;
                                    dataHarga.ACODE_2 = dataBaru.ACODE_2;
                                    dataHarga.ACODE_3 = dataBaru.ACODE_3;
                                    dataHarga.ACODE_4 = dataBaru.ACODE_4;
                                    dataHarga.ACODE_5 = dataBaru.ACODE_5;
                                    dataHarga.ACODE_6 = dataBaru.ACODE_6;
                                    dataHarga.ACODE_7 = dataBaru.ACODE_7;
                                    dataHarga.ACODE_8 = dataBaru.ACODE_8;
                                    dataHarga.ACODE_9 = dataBaru.ACODE_9;
                                    dataHarga.ACODE_10 = dataBaru.ACODE_10;
                                    dataHarga.ACODE_11 = dataBaru.ACODE_11;
                                    dataHarga.ACODE_12 = dataBaru.ACODE_12;
                                    dataHarga.ACODE_13 = dataBaru.ACODE_13;
                                    dataHarga.ACODE_14 = dataBaru.ACODE_14;
                                    dataHarga.ACODE_15 = dataBaru.ACODE_15;
                                    dataHarga.ACODE_16 = dataBaru.ACODE_16;
                                    dataHarga.ACODE_17 = dataBaru.ACODE_17;
                                    dataHarga.ACODE_18 = dataBaru.ACODE_18;
                                    dataHarga.ACODE_19 = dataBaru.ACODE_19;
                                    dataHarga.ACODE_20 = dataBaru.ACODE_20;
                                    dataHarga.ACODE_21 = dataBaru.ACODE_21;
                                    dataHarga.ACODE_22 = dataBaru.ACODE_22;
                                    dataHarga.ACODE_23 = dataBaru.ACODE_23;
                                    dataHarga.ACODE_24 = dataBaru.ACODE_24;
                                    dataHarga.ACODE_25 = dataBaru.ACODE_25;
                                    dataHarga.ACODE_26 = dataBaru.ACODE_26;
                                    dataHarga.ACODE_27 = dataBaru.ACODE_27;
                                    dataHarga.ACODE_28 = dataBaru.ACODE_28;
                                    dataHarga.ACODE_29 = dataBaru.ACODE_29;
                                    dataHarga.ACODE_30 = dataBaru.ACODE_30;
                                    dataHarga.ACODE_31 = dataBaru.ACODE_31;
                                    dataHarga.ACODE_32 = dataBaru.ACODE_32;
                                    dataHarga.ACODE_33 = dataBaru.ACODE_33;
                                    dataHarga.ACODE_34 = dataBaru.ACODE_34;
                                    dataHarga.ACODE_35 = dataBaru.ACODE_35;
                                    dataHarga.ACODE_36 = dataBaru.ACODE_36;
                                    dataHarga.ACODE_37 = dataBaru.ACODE_37;
                                    dataHarga.ACODE_38 = dataBaru.ACODE_38;
                                    dataHarga.ACODE_39 = dataBaru.ACODE_39;
                                    dataHarga.ACODE_30 = dataBaru.ACODE_40;
                                    dataHarga.ACODE_41 = dataBaru.ACODE_41;
                                    dataHarga.ACODE_42 = dataBaru.ACODE_42;
                                    dataHarga.ACODE_43 = dataBaru.ACODE_43;
                                    dataHarga.ACODE_44 = dataBaru.ACODE_44;
                                    dataHarga.ACODE_45 = dataBaru.ACODE_45;
                                    dataHarga.ACODE_46 = dataBaru.ACODE_46;
                                    dataHarga.ACODE_47 = dataBaru.ACODE_47;
                                    dataHarga.ACODE_48 = dataBaru.ACODE_48;
                                    dataHarga.ACODE_49 = dataBaru.ACODE_49;
                                    dataHarga.ACODE_50 = dataBaru.ACODE_50;

                                    dataHarga.ANAME_1 = dataBaru.ANAME_1;
                                    dataHarga.ANAME_2 = dataBaru.ANAME_2;
                                    dataHarga.ANAME_3 = dataBaru.ANAME_3;
                                    dataHarga.ANAME_4 = dataBaru.ANAME_4;
                                    dataHarga.ANAME_5 = dataBaru.ANAME_5;
                                    dataHarga.ANAME_6 = dataBaru.ANAME_6;
                                    dataHarga.ANAME_7 = dataBaru.ANAME_7;
                                    dataHarga.ANAME_8 = dataBaru.ANAME_8;
                                    dataHarga.ANAME_9 = dataBaru.ANAME_9;
                                    dataHarga.ANAME_10 = dataBaru.ANAME_10;
                                    dataHarga.ANAME_11 = dataBaru.ANAME_11;
                                    dataHarga.ANAME_12 = dataBaru.ANAME_12;
                                    dataHarga.ANAME_13 = dataBaru.ANAME_13;
                                    dataHarga.ANAME_14 = dataBaru.ANAME_14;
                                    dataHarga.ANAME_15 = dataBaru.ANAME_15;
                                    dataHarga.ANAME_16 = dataBaru.ANAME_16;
                                    dataHarga.ANAME_17 = dataBaru.ANAME_17;
                                    dataHarga.ANAME_18 = dataBaru.ANAME_18;
                                    dataHarga.ANAME_19 = dataBaru.ANAME_19;
                                    dataHarga.ANAME_20 = dataBaru.ANAME_20;
                                    dataHarga.ANAME_21 = dataBaru.ANAME_21;
                                    dataHarga.ANAME_22 = dataBaru.ANAME_22;
                                    dataHarga.ANAME_23 = dataBaru.ANAME_23;
                                    dataHarga.ANAME_24 = dataBaru.ANAME_24;
                                    dataHarga.ANAME_25 = dataBaru.ANAME_25;
                                    dataHarga.ANAME_26 = dataBaru.ANAME_26;
                                    dataHarga.ANAME_27 = dataBaru.ANAME_27;
                                    dataHarga.ANAME_28 = dataBaru.ANAME_28;
                                    dataHarga.ANAME_29 = dataBaru.ANAME_29;
                                    dataHarga.ANAME_30 = dataBaru.ANAME_30;
                                    dataHarga.ANAME_31 = dataBaru.ANAME_31;
                                    dataHarga.ANAME_32 = dataBaru.ANAME_32;
                                    dataHarga.ANAME_33 = dataBaru.ANAME_33;
                                    dataHarga.ANAME_34 = dataBaru.ANAME_34;
                                    dataHarga.ANAME_35 = dataBaru.ANAME_35;
                                    dataHarga.ANAME_36 = dataBaru.ANAME_36;
                                    dataHarga.ANAME_37 = dataBaru.ANAME_37;
                                    dataHarga.ANAME_38 = dataBaru.ANAME_38;
                                    dataHarga.ANAME_39 = dataBaru.ANAME_39;
                                    dataHarga.ANAME_40 = dataBaru.ANAME_40;
                                    dataHarga.ANAME_41 = dataBaru.ANAME_41;
                                    dataHarga.ANAME_42 = dataBaru.ANAME_42;
                                    dataHarga.ANAME_43 = dataBaru.ANAME_43;
                                    dataHarga.ANAME_44 = dataBaru.ANAME_44;
                                    dataHarga.ANAME_45 = dataBaru.ANAME_45;
                                    dataHarga.ANAME_46 = dataBaru.ANAME_46;
                                    dataHarga.ANAME_47 = dataBaru.ANAME_47;
                                    dataHarga.ANAME_48 = dataBaru.ANAME_48;
                                    dataHarga.ANAME_49 = dataBaru.ANAME_49;
                                    dataHarga.ANAME_50 = dataBaru.ANAME_50;

                                    dataHarga.AVALUE_1 = dataBaru.AVALUE_1;
                                    dataHarga.AVALUE_2 = dataBaru.AVALUE_2;
                                    dataHarga.AVALUE_3 = dataBaru.AVALUE_3;
                                    dataHarga.AVALUE_4 = dataBaru.AVALUE_4;
                                    dataHarga.AVALUE_5 = dataBaru.AVALUE_5;
                                    dataHarga.AVALUE_6 = dataBaru.AVALUE_6;
                                    dataHarga.AVALUE_7 = dataBaru.AVALUE_7;
                                    dataHarga.AVALUE_8 = dataBaru.AVALUE_8;
                                    dataHarga.AVALUE_9 = dataBaru.AVALUE_9;
                                    dataHarga.AVALUE_10 = dataBaru.AVALUE_10;
                                    dataHarga.AVALUE_11 = dataBaru.AVALUE_11;
                                    dataHarga.AVALUE_12 = dataBaru.AVALUE_12;
                                    dataHarga.AVALUE_13 = dataBaru.AVALUE_13;
                                    dataHarga.AVALUE_14 = dataBaru.AVALUE_14;
                                    dataHarga.AVALUE_15 = dataBaru.AVALUE_15;
                                    dataHarga.AVALUE_16 = dataBaru.AVALUE_16;
                                    dataHarga.AVALUE_17 = dataBaru.AVALUE_17;
                                    dataHarga.AVALUE_18 = dataBaru.AVALUE_18;
                                    dataHarga.AVALUE_19 = dataBaru.AVALUE_19;
                                    dataHarga.AVALUE_20 = dataBaru.AVALUE_20;
                                    dataHarga.AVALUE_21 = dataBaru.AVALUE_21;
                                    dataHarga.AVALUE_22 = dataBaru.AVALUE_22;
                                    dataHarga.AVALUE_23 = dataBaru.AVALUE_23;
                                    dataHarga.AVALUE_24 = dataBaru.AVALUE_24;
                                    dataHarga.AVALUE_25 = dataBaru.AVALUE_25;
                                    dataHarga.AVALUE_26 = dataBaru.AVALUE_26;
                                    dataHarga.AVALUE_27 = dataBaru.AVALUE_27;
                                    dataHarga.AVALUE_28 = dataBaru.AVALUE_28;
                                    dataHarga.AVALUE_29 = dataBaru.AVALUE_29;
                                    dataHarga.AVALUE_30 = dataBaru.AVALUE_30;
                                    dataHarga.AVALUE_31 = dataBaru.AVALUE_31;
                                    dataHarga.AVALUE_32 = dataBaru.AVALUE_32;
                                    dataHarga.AVALUE_33 = dataBaru.AVALUE_33;
                                    dataHarga.AVALUE_34 = dataBaru.AVALUE_34;
                                    dataHarga.AVALUE_35 = dataBaru.AVALUE_35;
                                    dataHarga.AVALUE_36 = dataBaru.AVALUE_36;
                                    dataHarga.AVALUE_37 = dataBaru.AVALUE_37;
                                    dataHarga.AVALUE_38 = dataBaru.AVALUE_38;
                                    dataHarga.AVALUE_39 = dataBaru.AVALUE_39;
                                    dataHarga.AVALUE_40 = dataBaru.AVALUE_40;
                                    dataHarga.AVALUE_41 = dataBaru.AVALUE_41;
                                    dataHarga.AVALUE_42 = dataBaru.AVALUE_42;
                                    dataHarga.AVALUE_43 = dataBaru.AVALUE_43;
                                    dataHarga.AVALUE_44 = dataBaru.AVALUE_44;
                                    dataHarga.AVALUE_45 = dataBaru.AVALUE_45;
                                    dataHarga.AVALUE_46 = dataBaru.AVALUE_46;
                                    dataHarga.AVALUE_47 = dataBaru.AVALUE_47;
                                    dataHarga.AVALUE_48 = dataBaru.AVALUE_48;
                                    dataHarga.AVALUE_49 = dataBaru.AVALUE_49;
                                    dataHarga.AVALUE_50 = dataBaru.AVALUE_50;
                                    #endregion
                                    if (updateKategori)
                                    {
                                        var kdMarket = ErasoftDbContext.ARF01.Where(m => m.RecNum == dataBaru.IDMARKET).SingleOrDefault().NAMA;
                                        string namaMarket = "";
                                        if (kdMarket == kdTokped.IdMarket.ToString())
                                        {
                                            namaMarket = "TOKPED";
                                        }
                                        else if (kdMarket == kdShopee.IdMarket.ToString())
                                        {
                                            namaMarket = "SHOPEE";
                                        }
                                        else if (kdMarket == kdBlibli.IdMarket.ToString())
                                        {
                                            namaMarket = "BLIBLI";
                                        }
                                        if (kdMarket == kdLazada.IdMarket.ToString())
                                        {
                                            namaMarket = "LAZADA";
                                        }
                                        else if (kdMarket == kdBL.IdMarket.ToString())
                                        {
                                            namaMarket = "BUKALAPAK";
                                        }
                                        else if (kdMarket == kdElevenia.IdMarket.ToString())
                                        {
                                            namaMarket = "ELEVENIA";
                                        }
                                        if (namaMarket != "")
                                        {
                                            EDB.ExecuteSQL("CString", CommandType.Text, "UPDATE STF02I SET CATEGORY_MO = '" + barangInDb.Sort1 + "', MP_CATEGORY_CODE='" + dataHarga.CATEGORY_CODE + "' WHERE BRG = '" + barangInDb.BRG + "' AND MARKET='" + namaMarket + "' ");
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            dataBarang.errorHargaPerMP = "1";
                            dataBarang.Errors = listError;
                            return Json(dataBarang, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (Request.Files.Count > 0)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            var file = Request.Files[i];

                            if (file != null && file.ContentLength > 0)
                            {
                                ImgurImageResponse image = UploadImageService.UploadSingleImageToImgur(file, "uploaded-image");

                                imgPath[i] = image.data.link;

                                switch (i)
                                {
                                    case 0:
                                        barangInDb.LINK_GAMBAR_1 = image.data.link_l;
                                        barangInDb.Sort5 = Convert.ToString(file.ContentLength);
                                        break;
                                    case 1:
                                        barangInDb.LINK_GAMBAR_2 = image.data.link_l;
                                        barangInDb.Sort6 = Convert.ToString(file.ContentLength);
                                        break;
                                    case 2:
                                        barangInDb.LINK_GAMBAR_3 = image.data.link_l;
                                        barangInDb.Sort7 = Convert.ToString(file.ContentLength);
                                        break;
                                }
                            }
                        }
                    }
                    //add by calvin 16 nov 2018, imgpath saat update
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                imgPath[0] = barangInDb.LINK_GAMBAR_1;
                                break;
                            case 1:
                                imgPath[1] = barangInDb.LINK_GAMBAR_2;
                                break;
                            case 2:
                                imgPath[2] = barangInDb.LINK_GAMBAR_3;
                                break;
                        }
                    }
                    //end add by calvin
                }
            }

            ErasoftDbContext.SaveChanges();

            ModelState.Clear();

            var kategori = ErasoftDbContext.STF02E.Single(k => k.KODE == dataBarang.Stf02.Sort1);
            var stf20 = ErasoftDbContext.STF20.Where(m => m.CATEGORY_MO == kategori.KODE).ToList();
            var vm = new BarangStrukturVarViewModel()
            {
                Barang = ErasoftDbContext.STF02.Where(p => p.BRG == KodeBarang).FirstOrDefault(),
                Kategori = kategori,
                Variant_Level_1 = new STF20()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_JUDUL_VAR = 1,
                    VALUE_JUDUL_VAR = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(1)).FirstOrDefault()?.VALUE_JUDUL_VAR
                },
                Variant_Level_2 = new STF20()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_JUDUL_VAR = 2,
                    VALUE_JUDUL_VAR = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(2)).FirstOrDefault()?.VALUE_JUDUL_VAR
                },
                Variant_Level_3 = new STF20()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_JUDUL_VAR = 3,
                    VALUE_JUDUL_VAR = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(3)).FirstOrDefault()?.VALUE_JUDUL_VAR
                },
                ListMarket = ErasoftDbContext.ARF01.OrderBy(p => p.RecNum).ToList(),
                VariantPerMP = ErasoftDbContext.STF02I.AsNoTracking().Where(p => p.BRG == KodeBarang).ToList(),
                VariantOptMaster = ErasoftDbContext.STF20B.Where(p => p.CATEGORY_MO == kategori.KODE).ToList()
            };
            return PartialView("BarangVarPartial", vm);
        }

        protected void createBarangLazada(BarangViewModel dataBarang, string[] imgPath, ARF01 tblCustomer)
        {
            //var kdLazada = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "LAZADA");
            //var listLazadaShop = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdLazada.IdMarket.ToString()).ToList();
            var barangInDb = ErasoftDbContext.STF02.SingleOrDefault(b => b.ID == dataBarang.Stf02.ID || b.BRG == dataBarang.Stf02.BRG);
            var lzdApi = new LazadaController();
            string[] imageUrl = new string[imgPath.Length];
            //if (listLazadaShop.Count > 0)
            //{
            //    foreach (ARF01 tblCustomer in listLazadaShop)
            //    {
            var productMarketPlace = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == barangInDb.BRG && m.IDMARKET == tblCustomer.RecNum);
            if (!string.IsNullOrEmpty(tblCustomer.TOKEN) && productMarketPlace.DISPLAY)
            {
                for (int i = 0; i < imgPath.Length; i++)
                {
                    if (!string.IsNullOrEmpty(imgPath[i]))
                    {
                        var uploadImg = lzdApi.UploadImage(imgPath[i], tblCustomer.TOKEN);
                        if (uploadImg.status == 1)
                            imageUrl[i] = uploadImg.message;
                    }
                }

                BrgViewModel dataLazada = new BrgViewModel
                {
                    deskripsi = dataBarang.Stf02.Deskripsi,
                    harga = dataBarang.Stf02.HJUAL.ToString(),
                    height = dataBarang.Stf02.TINGGI.ToString(),
                    kdBrg = barangInDb.BRG,
                    length = dataBarang.Stf02.PANJANG.ToString(),
                    nama = dataBarang.Stf02.NAMA,
                    nama2 = dataBarang.Stf02.NAMA2,
                    weight = dataBarang.Stf02.BERAT.ToString(),
                    width = dataBarang.Stf02.LEBAR.ToString(),
                    user = tblCustomer.EMAIL,
                    key = tblCustomer.API_KEY,
                    qty = "1",
                    token = tblCustomer.TOKEN,
                    idMarket = tblCustomer.RecNum.ToString(),
                };

                dataLazada.merk = ErasoftDbContext.STF02E.SingleOrDefault(m => m.KODE == dataBarang.Stf02.Sort2 && m.LEVEL == "2").KET;
                dataLazada.harga = productMarketPlace.HJUAL.ToString();
                dataLazada.activeProd = productMarketPlace.DISPLAY;

                if (!string.IsNullOrEmpty(imageUrl[2]))
                {
                    dataLazada.imageUrl3 = imageUrl[2];
                }
                if (!string.IsNullOrEmpty(imageUrl[1]))
                {
                    dataLazada.imageUrl2 = imageUrl[1];
                }
                if (!string.IsNullOrEmpty(imageUrl[0]))
                {
                    dataLazada.imageUrl = imageUrl[0];
                }
                //if (!string.IsNullOrEmpty(barangInDb.LINK_GAMBAR_3))
                //{
                //    dataLazada.imageUrl3 = barangInDb.LINK_GAMBAR_3;
                //}
                //if (!string.IsNullOrEmpty(barangInDb.LINK_GAMBAR_2))
                //{
                //    dataLazada.imageUrl2 = barangInDb.LINK_GAMBAR_2;
                //}
                //if (!string.IsNullOrEmpty(barangInDb.LINK_GAMBAR_1))
                //{
                //    dataLazada.imageUrl = barangInDb.LINK_GAMBAR_1;
                //}
                var result = lzdApi.CreateProduct(dataLazada);
            }
            //    }
            //}
        }
        protected void createBarangBukaLapak(BarangViewModel dataBarang, string[] imgPath, ARF01 tblCustomer)
        {
            var barangInDb = ErasoftDbContext.STF02.SingleOrDefault(b => b.ID == dataBarang.Stf02.ID || b.BRG == dataBarang.Stf02.BRG);
            var productMarketPlace = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == barangInDb.BRG && m.IDMARKET == tblCustomer.RecNum);
            var blApi = new BukaLapakController();
            string[] imgID = new string[imgPath.Length];
            for (int i = 0; i < imgPath.Length; i++)
            {
                if (!string.IsNullOrEmpty(imgPath[i]))
                {
                    var uploadImg = blApi.uploadGambar(imgPath[i], tblCustomer.API_KEY, tblCustomer.TOKEN);
                    if (uploadImg.status == 1)
                        imgID[i] = uploadImg.message;
                }
            }
            BrgViewModel data = new BrgViewModel
            {
                deskripsi = dataBarang.Stf02.Deskripsi,
                harga = dataBarang.Stf02.HJUAL.ToString(),
                height = dataBarang.Stf02.TINGGI.ToString(),
                kdBrg = barangInDb.BRG,
                length = dataBarang.Stf02.PANJANG.ToString(),
                nama = dataBarang.Stf02.NAMA,
                nama2 = dataBarang.Stf02.NAMA2,
                weight = dataBarang.Stf02.BERAT.ToString(),
                width = dataBarang.Stf02.LEBAR.ToString(),
                user = tblCustomer.EMAIL,
                key = tblCustomer.API_KEY,
                qty = "1",
                token = tblCustomer.TOKEN,
                idMarket = tblCustomer.RecNum.ToString(),
                //merk = ErasoftDbContext.STF02E.SingleOrDefault(m => m.KODE == dataBarang.Stf02.Sort2 && m.LEVEL == "2").KET
            };
            data.merk = ErasoftDbContext.STF02E.SingleOrDefault(m => m.KODE == dataBarang.Stf02.Sort2 && m.LEVEL == "2").KET;
            //var productMarketPlace = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == barangInDb.BRG && m.IDMARKET == tblCustomer.RecNum);
            data.harga = productMarketPlace.HJUAL.ToString();
            if (!string.IsNullOrEmpty(imgID[2]))
            {
                data.imageId3 = imgID[2];
            }
            if (!string.IsNullOrEmpty(imgID[1]))
            {
                data.imageId2 = imgID[1];
            }
            if (!string.IsNullOrEmpty(imgID[0]))
            {
                data.imageId = imgID[0];
            }

            var result = blApi.CreateProduct(data);
            //if (result.status == 1)
            //    if (!productMarketPlace.DISPLAY)
            //    {
            //        //panggil api utk non-aktif barang yg baru di insert
            //        result = blApi.prodNonAktif(barangInDb.BRG, result.message, tblCustomer.API_KEY, tblCustomer.TOKEN);
            //    }
        }
        protected void saveBarangTokpedVariant(int mode, string dataBarang_Stf02_BRG, bool updateHarga)
        {
            var barangInDb = ErasoftDbContext.STF02.SingleOrDefault(b => b.BRG == dataBarang_Stf02_BRG);
            var kdTokped = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "TOKOPEDIA");
            if (barangInDb != null && kdTokped != null)
            {
                var listTokped = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdTokped.IdMarket.ToString()).ToList();
                if (listTokped.Count > 0)
                {
                    switch (mode)
                    {
                        case 1:
                            {
                                foreach (ARF01 tblCustomer in listTokped)
                                {
                                    if (!string.IsNullOrEmpty(tblCustomer.Sort1_Cust))
                                    {
                                        var display = Convert.ToBoolean(ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang_Stf02_BRG) ? barangInDb.BRG : dataBarang_Stf02_BRG) && m.IDMARKET == tblCustomer.RecNum).DISPLAY);
                                        if (display)
                                        {
                                            TokopediaController.TokopediaAPIData iden = new TokopediaController.TokopediaAPIData()
                                            {
                                                merchant_code = tblCustomer.Sort1_Cust, //FSID
                                                API_client_password = tblCustomer.API_CLIENT_P, //Client ID
                                                API_client_username = tblCustomer.API_CLIENT_U, //Client Secret
                                                API_secret_key = tblCustomer.API_KEY, //Shop ID 
                                                token = tblCustomer.TOKEN,
                                                idmarket = tblCustomer.RecNum.Value
                                            };
                                            TokopediaController tokoAPI = new TokopediaController();
                                            Task.Run(() => tokoAPI.CreateProduct(iden, (string.IsNullOrEmpty(dataBarang_Stf02_BRG) ? barangInDb.BRG : dataBarang_Stf02_BRG)).Wait());
                                        }
                                    }
                                }
                            }
                            break;
                        case 2:
                            {
                                foreach (ARF01 tblCustomer in listTokped)
                                {
                                    if (!string.IsNullOrEmpty(tblCustomer.Sort1_Cust))
                                    {
                                        var stf02h = ErasoftDbContext.STF02H.Where(p => p.BRG == barangInDb.BRG && p.IDMARKET == tblCustomer.RecNum).FirstOrDefault();
                                        if (stf02h != null)
                                        {
                                            if (!string.IsNullOrEmpty(stf02h.BRG_MP))
                                            {
                                                TokopediaController tokoAPI = new TokopediaController();
                                                TokopediaController.TokopediaAPIData iden = new TokopediaController.TokopediaAPIData()
                                                {
                                                    merchant_code = tblCustomer.Sort1_Cust, //FSID
                                                    API_client_password = tblCustomer.API_CLIENT_P, //Client ID
                                                    API_client_username = tblCustomer.API_CLIENT_U, //Client Secret
                                                    API_secret_key = tblCustomer.API_KEY, //Shop ID 
                                                    token = tblCustomer.TOKEN,
                                                    idmarket = tblCustomer.RecNum.Value
                                                };
                                                if (stf02h.BRG_MP.Contains("PENDING"))
                                                {
                                                    var cekPendingCreate = ErasoftDbContext.STF02H.Where(p => p.IDMARKET == tblCustomer.RecNum && p.BRG_MP == stf02h.BRG_MP).ToList();
                                                    if (cekPendingCreate.Count > 0)
                                                    {
                                                        foreach (var item in cekPendingCreate)
                                                        {
                                                            Task.Run(() => tokoAPI.CreateProductGetStatus(iden, item.BRG, Convert.ToInt32(item.BRG_MP.Split(';')[1]), item.BRG_MP.Split(';')[2]).Wait());
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    Task.Run(() => tokoAPI.EditProduct(iden, (string.IsNullOrEmpty(dataBarang_Stf02_BRG) ? barangInDb.BRG : dataBarang_Stf02_BRG), stf02h.BRG_MP).Wait());
                                                }
                                            }
                                            else
                                            {
                                                if (stf02h.DISPLAY)
                                                {
                                                    var display = Convert.ToBoolean(ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang_Stf02_BRG) ? barangInDb.BRG : dataBarang_Stf02_BRG) && m.IDMARKET == tblCustomer.RecNum).DISPLAY);
                                                    if (display)
                                                    {
                                                        TokopediaController.TokopediaAPIData iden = new TokopediaController.TokopediaAPIData()
                                                        {
                                                            merchant_code = tblCustomer.Sort1_Cust, //FSID
                                                            API_client_password = tblCustomer.API_CLIENT_P, //Client ID
                                                            API_client_username = tblCustomer.API_CLIENT_U, //Client Secret
                                                            API_secret_key = tblCustomer.API_KEY, //Shop ID 
                                                            token = tblCustomer.TOKEN,
                                                            idmarket = tblCustomer.RecNum.Value
                                                        };
                                                        TokopediaController tokoAPI = new TokopediaController();
                                                        Task.Run(() => tokoAPI.CreateProduct(iden, (string.IsNullOrEmpty(dataBarang_Stf02_BRG) ? barangInDb.BRG : dataBarang_Stf02_BRG)).Wait());
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        protected void saveBarangShopee(int mode, BarangViewModel dataBarang, bool updateHarga)
        {
            var barangInDb = ErasoftDbContext.STF02.SingleOrDefault(b => b.ID == dataBarang.Stf02.ID || b.BRG == dataBarang.Stf02.BRG);
            var kdShopee = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "SHOPEE");
            if (barangInDb != null && kdShopee != null)
            {
                var listShopee = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdShopee.IdMarket.ToString()).ToList();
                if (listShopee.Count > 0)
                {
                    switch (mode)
                    {
                        #region Create Product lalu Hide Item
                        case 1:
                            {
                                foreach (ARF01 tblCustomer in listShopee)
                                {
                                    if (!string.IsNullOrEmpty(tblCustomer.Sort1_Cust))
                                    {
                                        var display = Convert.ToBoolean(ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).DISPLAY);
                                        if (display)
                                        {
                                            ShopeeController.ShopeeAPIData iden = new ShopeeController.ShopeeAPIData
                                            {
                                                merchant_code = tblCustomer.Sort1_Cust,
                                            };
                                            ShopeeController shoAPI = new ShopeeController();
                                            Task.Run(() => shoAPI.CreateProduct(iden, (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG), tblCustomer.CUST, new List<ShopeeController.ShopeeLogisticsClass>()).Wait());
                                        }
                                    }
                                }
                            }
                            break;
                        #endregion
                        case 2:
                            {
                                foreach (ARF01 tblCustomer in listShopee)
                                {
                                    if (!string.IsNullOrEmpty(tblCustomer.Sort1_Cust))
                                    {
                                        var stf02h = ErasoftDbContext.STF02H.Where(p => p.BRG == barangInDb.BRG && p.IDMARKET == tblCustomer.RecNum).FirstOrDefault();
                                        if (stf02h != null)
                                        {
                                            if (!string.IsNullOrEmpty(stf02h.BRG_MP))
                                            {
                                                ShopeeController.ShopeeAPIData iden = new ShopeeController.ShopeeAPIData
                                                {
                                                    merchant_code = tblCustomer.Sort1_Cust,
                                                };
                                                ShopeeController shoAPI = new ShopeeController();

                                                //remark by calvin 26 februari 2019, ini untuk update deskripsi dll
                                                //Task.Run(() => shoAPI.UpdateProduct(iden, (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG), tblCustomer.CUST, new List<ShopeeController.ShopeeLogisticsClass>()).Wait());
                                                //end remark by calvin 26 februari 2019
                                                Task.Run(() => shoAPI.UpdateImage(iden, (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG), stf02h.BRG_MP).Wait());
                                                string[] brg_mp = stf02h.BRG_MP.Split(';');
                                                if (updateHarga)
                                                {
                                                    if (brg_mp.Count() == 2)
                                                    {
                                                        if (brg_mp[1] == "0")
                                                        {
                                                            Task.Run(() => shoAPI.UpdatePrice(iden, stf02h.BRG_MP, (float)stf02h.HJUAL)).Wait();
                                                        }
                                                        else if (brg_mp[1] != "")
                                                        {
                                                            Task.Run(() => shoAPI.UpdateVariationPrice(iden, stf02h.BRG_MP, (float)stf02h.HJUAL)).Wait();
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (stf02h.DISPLAY)
                                                {
                                                    ShopeeController.ShopeeAPIData iden = new ShopeeController.ShopeeAPIData
                                                    {
                                                        merchant_code = tblCustomer.Sort1_Cust,
                                                    };
                                                    ShopeeController shoAPI = new ShopeeController();
                                                    Task.Run(() => shoAPI.CreateProduct(iden, (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG), tblCustomer.CUST, new List<ShopeeController.ShopeeLogisticsClass>()).Wait());
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        protected void saveBarangShopeeVariant(int mode, string dataBarang_Stf02_BRG, bool updateHarga)
        {
            var barangInDb = ErasoftDbContext.STF02.SingleOrDefault(b => b.BRG == dataBarang_Stf02_BRG);
            var kdShopee = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "SHOPEE");
            if (barangInDb != null && kdShopee != null)
            {
                var listShopee = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdShopee.IdMarket.ToString()).ToList();
                if (listShopee.Count > 0)
                {
                    switch (mode)
                    {
                        #region Create Product lalu Hide Item
                        case 1:
                            {
                                foreach (ARF01 tblCustomer in listShopee)
                                {
                                    if (!string.IsNullOrEmpty(tblCustomer.Sort1_Cust))
                                    {
                                        var display = Convert.ToBoolean(ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang_Stf02_BRG) ? barangInDb.BRG : dataBarang_Stf02_BRG) && m.IDMARKET == tblCustomer.RecNum).DISPLAY);
                                        if (display)
                                        {
                                            ShopeeController.ShopeeAPIData iden = new ShopeeController.ShopeeAPIData
                                            {
                                                merchant_code = tblCustomer.Sort1_Cust,
                                            };
                                            ShopeeController shoAPI = new ShopeeController();
                                            Task.Run(() => shoAPI.CreateProduct(iden, (string.IsNullOrEmpty(dataBarang_Stf02_BRG) ? barangInDb.BRG : dataBarang_Stf02_BRG), tblCustomer.CUST, new List<ShopeeController.ShopeeLogisticsClass>()).Wait());
                                        }
                                    }
                                }
                            }
                            break;
                        #endregion
                        case 2:
                            {
                                foreach (ARF01 tblCustomer in listShopee)
                                {
                                    if (!string.IsNullOrEmpty(tblCustomer.Sort1_Cust))
                                    {
                                        var stf02h = ErasoftDbContext.STF02H.Where(p => p.BRG == barangInDb.BRG && p.IDMARKET == tblCustomer.RecNum).FirstOrDefault();
                                        if (stf02h != null)
                                        {
                                            if (!string.IsNullOrEmpty(stf02h.BRG_MP))
                                            {
                                                ShopeeController.ShopeeAPIData iden = new ShopeeController.ShopeeAPIData
                                                {
                                                    merchant_code = tblCustomer.Sort1_Cust,
                                                };
                                                ShopeeController shoAPI = new ShopeeController();

                                                //remark by calvin 26 februari 2019, ini untuk update deskripsi dll
                                                //Task.Run(() => shoAPI.UpdateProduct(iden, (string.IsNullOrEmpty(dataBarang_Stf02_BRG) ? barangInDb.BRG : dataBarang_Stf02_BRG), tblCustomer.CUST, new List<ShopeeController.ShopeeLogisticsClass>()).Wait());
                                                //end remark by calvin 26 februari 2019

                                                //Task.Run(() => shoAPI.GetVariation(iden, barangInDb, Convert.ToInt64(stf02h.BRG_MP.Split(';')[0]), tblCustomer).Wait());
                                                Task.Run(() => shoAPI.InitTierVariation(iden, barangInDb, Convert.ToInt64(stf02h.BRG_MP.Split(';')[0]), tblCustomer).Wait());

                                                Task.Run(() => shoAPI.UpdateImage(iden, (string.IsNullOrEmpty(dataBarang_Stf02_BRG) ? barangInDb.BRG : dataBarang_Stf02_BRG), stf02h.BRG_MP).Wait());
                                                string[] brg_mp = stf02h.BRG_MP.Split(';');
                                                if (updateHarga)
                                                {
                                                    if (brg_mp.Count() == 2)
                                                    {
                                                        if (brg_mp[1] == "0")
                                                        {
                                                            Task.Run(() => shoAPI.UpdatePrice(iden, stf02h.BRG_MP, (float)stf02h.HJUAL)).Wait();
                                                        }
                                                        else if (brg_mp[1] != "")
                                                        {
                                                            Task.Run(() => shoAPI.UpdateVariationPrice(iden, stf02h.BRG_MP, (float)stf02h.HJUAL)).Wait();
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (stf02h.DISPLAY)
                                                {
                                                    ShopeeController.ShopeeAPIData iden = new ShopeeController.ShopeeAPIData
                                                    {
                                                        merchant_code = tblCustomer.Sort1_Cust,
                                                    };
                                                    ShopeeController shoAPI = new ShopeeController();
                                                    Task.Run(() => shoAPI.CreateProduct(iden, (string.IsNullOrEmpty(dataBarang_Stf02_BRG) ? barangInDb.BRG : dataBarang_Stf02_BRG), tblCustomer.CUST, new List<ShopeeController.ShopeeLogisticsClass>()).Wait());
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        protected void saveBarangBlibli(int mode, BarangViewModel dataBarang)
        {
            var barangInDb = ErasoftDbContext.STF02.AsNoTracking().SingleOrDefault(b => b.ID == dataBarang.Stf02.ID || b.BRG == dataBarang.Stf02.BRG);
            var kdBlibli = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BLIBLI");
            if (barangInDb != null && kdBlibli != null)
            {
                var listBlibli = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdBlibli.IdMarket.ToString()).ToList();
                if (listBlibli.Count > 0)
                {
                    switch (mode)
                    {
                        #region Create Product lalu Hide Item
                        case 1:
                            {
                                foreach (ARF01 tblCustomer in listBlibli)
                                {
                                    if (!string.IsNullOrEmpty(tblCustomer.Kode))
                                    {
                                        //if (string.IsNullOrEmpty(ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).BRG_MP))
                                        //{

                                        //var fileExtension = Path.GetExtension(file.FileName);
                                        //var namaFile = $"FotoProduk-{dataBarang.Stf02.USERNAME}-{dataBarang.Stf02.BRG}-foto-{i + 1}{fileExtension}";
                                        //var path = Path.Combine(Server.MapPath("~/Content/Uploaded/"), namaFile);
                                        var display = Convert.ToBoolean(ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).DISPLAY);
                                        if (display)
                                        {
                                            BlibliController.BlibliAPIData iden = new BlibliController.BlibliAPIData
                                            {
                                                merchant_code = tblCustomer.Sort1_Cust,
                                                API_client_password = tblCustomer.API_CLIENT_P,
                                                API_client_username = tblCustomer.API_CLIENT_U,
                                                API_secret_key = tblCustomer.API_KEY,
                                                token = tblCustomer.TOKEN,
                                                mta_username_email_merchant = tblCustomer.EMAIL,
                                                mta_password_password_merchant = tblCustomer.PASSWORD,
                                                idmarket = tblCustomer.RecNum.Value
                                            };
                                            BlibliController.BlibliProductData data = new BlibliController.BlibliProductData
                                            {
                                                kode = string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG,
                                                nama = dataBarang.Stf02.NAMA + ' ' + dataBarang.Stf02.NAMA2 + ' ' + dataBarang.Stf02.NAMA3,
                                                berat = (dataBarang.Stf02.BERAT).ToString(),//MO save dalam Gram, Elevenia dalam Kilogram
                                                Keterangan = dataBarang.Stf02.Deskripsi,
                                                Qty = "0",
                                                MinQty = "0",
                                                PickupPoint = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).PICKUP_POINT.ToString(),
                                                IDMarket = tblCustomer.RecNum.ToString(),
                                                Length = Convert.ToString(dataBarang.Stf02.PANJANG),
                                                Width = Convert.ToString(dataBarang.Stf02.LEBAR),
                                                Height = Convert.ToString(dataBarang.Stf02.TINGGI),
                                                type = Convert.ToString(dataBarang.Stf02.TYPE),
                                                dataBarangInDb = barangInDb
                                            };

                                            data.Brand = ErasoftDbContext.STF02E.SingleOrDefault(m => m.KODE == dataBarang.Stf02.Sort2 && m.LEVEL == "2").KET;
                                            data.Price = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).HJUAL.ToString();
                                            data.MarketPrice = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).HJUAL.ToString();
                                            data.CategoryCode = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).CATEGORY_CODE.ToString();

                                            data.display = display ? "true" : "false";
                                            BlibliController bliAPI = new BlibliController();
                                            Task.Run(() => bliAPI.CreateProduct(iden, data).Wait());

                                        }
                                        //new BlibliController().GetQueueFeedDetail(iden, null);
                                        //}
                                    }
                                }
                            }
                            break;
                        #endregion
                        case 2:
                            {
                                var qtyOnHand = GetQOHSTF08A(string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG, "ALL");
                                foreach (ARF01 tblCustomer in listBlibli)
                                {
                                    if (!string.IsNullOrEmpty(tblCustomer.Kode))
                                    {
                                        var stf02h = ErasoftDbContext.STF02H.Where(p => p.BRG == barangInDb.BRG && p.IDMARKET == tblCustomer.RecNum).FirstOrDefault();
                                        if (stf02h != null)
                                        {
                                            if (!string.IsNullOrEmpty(stf02h.BRG_MP))
                                            {
                                                var BliApi = new BlibliController();
                                                BlibliController.BlibliAPIData iden = new BlibliController.BlibliAPIData
                                                {
                                                    merchant_code = tblCustomer.Sort1_Cust,
                                                    API_client_password = tblCustomer.API_CLIENT_P,
                                                    API_client_username = tblCustomer.API_CLIENT_U,
                                                    API_secret_key = tblCustomer.API_KEY,
                                                    token = tblCustomer.TOKEN,
                                                    mta_username_email_merchant = tblCustomer.EMAIL,
                                                    mta_password_password_merchant = tblCustomer.PASSWORD,
                                                    idmarket = tblCustomer.RecNum.Value
                                                };
                                                if (stf02h.BRG_MP == "PENDING")
                                                {
                                                    BliApi.GetQueueFeedDetail(iden, null);
                                                }
                                                else
                                                {
                                                    #region update
                                                    BlibliController.BlibliProductData data = new BlibliController.BlibliProductData
                                                    {
                                                        kode = barangInDb.BRG,
                                                        kode_mp = stf02h.BRG_MP,
                                                        Qty = Convert.ToString(qtyOnHand),
                                                        MinQty = "0"
                                                    };
                                                    data.Price = stf02h.HJUAL.ToString();
                                                    data.MarketPrice = stf02h.HJUAL.ToString();
                                                    var display = Convert.ToBoolean(stf02h.DISPLAY);
                                                    data.display = display ? "true" : "false";
                                                    BliApi.UpdateProdukQOH_Display(iden, data);
                                                    #endregion
                                                }
                                            }
                                            else
                                            {
                                                var display = Convert.ToBoolean(stf02h.DISPLAY);
                                                if (display)
                                                {
                                                    #region insert
                                                    BlibliController.BlibliAPIData iden = new BlibliController.BlibliAPIData
                                                    {
                                                        merchant_code = tblCustomer.Sort1_Cust,
                                                        API_client_password = tblCustomer.API_CLIENT_P,
                                                        API_client_username = tblCustomer.API_CLIENT_U,
                                                        API_secret_key = tblCustomer.API_KEY,
                                                        token = tblCustomer.TOKEN,
                                                        mta_username_email_merchant = tblCustomer.EMAIL,
                                                        mta_password_password_merchant = tblCustomer.PASSWORD,
                                                        idmarket = tblCustomer.RecNum.Value
                                                    };
                                                    BlibliController.BlibliProductData data = new BlibliController.BlibliProductData
                                                    {
                                                        kode = string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG,
                                                        nama = dataBarang.Stf02.NAMA + ' ' + dataBarang.Stf02.NAMA2 + ' ' + dataBarang.Stf02.NAMA3,
                                                        berat = (dataBarang.Stf02.BERAT).ToString(),//MO save dalam Gram, Elevenia dalam Kilogram
                                                        Keterangan = dataBarang.Stf02.Deskripsi,
                                                        Qty = "0",
                                                        MinQty = "0",
                                                        PickupPoint = stf02h.PICKUP_POINT,
                                                        IDMarket = tblCustomer.RecNum.ToString(),
                                                        Length = Convert.ToString(dataBarang.Stf02.PANJANG),
                                                        Width = Convert.ToString(dataBarang.Stf02.LEBAR),
                                                        Height = Convert.ToString(dataBarang.Stf02.TINGGI),
                                                        type = Convert.ToString(dataBarang.Stf02.TYPE),
                                                        dataBarangInDb = barangInDb
                                                    };
                                                    data.Brand = ErasoftDbContext.STF02E.SingleOrDefault(m => m.KODE == dataBarang.Stf02.Sort2 && m.LEVEL == "2").KET;
                                                    data.Price = Convert.ToString(stf02h.HJUAL);
                                                    data.MarketPrice = Convert.ToString(stf02h.HJUAL);
                                                    data.CategoryCode = Convert.ToString(stf02h.CATEGORY_CODE);

                                                    data.display = display ? "true" : "false";
                                                    BlibliController bliAPI = new BlibliController();
                                                    Task.Run(() => bliAPI.CreateProduct(iden, data).Wait());

                                                    #endregion
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        protected void saveBarangBlibliVariant(int mode, string dataBarang_Stf02_BRG)
        {
            var barangInDb = ErasoftDbContext.STF02.SingleOrDefault(b => b.BRG == dataBarang_Stf02_BRG);
            var kdBlibli = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BLIBLI");
            if (barangInDb != null && kdBlibli != null)
            {
                var listBlibli = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdBlibli.IdMarket.ToString()).ToList();
                if (listBlibli.Count > 0)
                {
                    switch (mode)
                    {
                        #region Create Product lalu Hide Item
                        case 1:
                            {
                                foreach (ARF01 tblCustomer in listBlibli)
                                {
                                    if (!string.IsNullOrEmpty(tblCustomer.Kode))
                                    {
                                        //if (string.IsNullOrEmpty(ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).BRG_MP))
                                        //{

                                        //var fileExtension = Path.GetExtension(file.FileName);
                                        //var namaFile = $"FotoProduk-{dataBarang.Stf02.USERNAME}-{dataBarang.Stf02.BRG}-foto-{i + 1}{fileExtension}";
                                        //var path = Path.Combine(Server.MapPath("~/Content/Uploaded/"), namaFile);
                                        var display = Convert.ToBoolean(ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang_Stf02_BRG) ? barangInDb.BRG : dataBarang_Stf02_BRG) && m.IDMARKET == tblCustomer.RecNum).DISPLAY);
                                        if (display)
                                        {
                                            BlibliController.BlibliAPIData iden = new BlibliController.BlibliAPIData
                                            {
                                                merchant_code = tblCustomer.Sort1_Cust,
                                                API_client_password = tblCustomer.API_CLIENT_P,
                                                API_client_username = tblCustomer.API_CLIENT_U,
                                                API_secret_key = tblCustomer.API_KEY,
                                                token = tblCustomer.TOKEN,
                                                mta_username_email_merchant = tblCustomer.EMAIL,
                                                mta_password_password_merchant = tblCustomer.PASSWORD,
                                                idmarket = tblCustomer.RecNum.Value
                                            };
                                            BlibliController.BlibliProductData data = new BlibliController.BlibliProductData
                                            {
                                                kode = string.IsNullOrEmpty(dataBarang_Stf02_BRG) ? barangInDb.BRG : dataBarang_Stf02_BRG,
                                                nama = barangInDb.NAMA + ' ' + barangInDb.NAMA2 + ' ' + barangInDb.NAMA3,
                                                berat = (barangInDb.BERAT).ToString(),//MO save dalam Gram, Elevenia dalam Kilogram
                                                Keterangan = barangInDb.Deskripsi,
                                                Qty = "0",
                                                MinQty = "0",
                                                PickupPoint = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang_Stf02_BRG) ? barangInDb.BRG : dataBarang_Stf02_BRG) && m.IDMARKET == tblCustomer.RecNum).PICKUP_POINT.ToString(),
                                                IDMarket = tblCustomer.RecNum.ToString(),
                                                Length = Convert.ToString(barangInDb.PANJANG),
                                                Width = Convert.ToString(barangInDb.LEBAR),
                                                Height = Convert.ToString(barangInDb.TINGGI),
                                                dataBarangInDb = barangInDb
                                            };
                                            data.Brand = ErasoftDbContext.STF02E.SingleOrDefault(m => m.KODE == barangInDb.Sort2 && m.LEVEL == "2").KET;
                                            data.Price = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang_Stf02_BRG) ? barangInDb.BRG : dataBarang_Stf02_BRG) && m.IDMARKET == tblCustomer.RecNum).HJUAL.ToString();
                                            data.MarketPrice = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang_Stf02_BRG) ? barangInDb.BRG : dataBarang_Stf02_BRG) && m.IDMARKET == tblCustomer.RecNum).HJUAL.ToString();
                                            data.CategoryCode = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang_Stf02_BRG) ? barangInDb.BRG : dataBarang_Stf02_BRG) && m.IDMARKET == tblCustomer.RecNum).CATEGORY_CODE.ToString();

                                            data.display = display ? "true" : "false";
                                            BlibliController bliAPI = new BlibliController();
                                            Task.Run(() => bliAPI.CreateProduct(iden, data).Wait());

                                        }
                                        //new BlibliController().GetQueueFeedDetail(iden, null);
                                        //}
                                    }
                                }
                            }
                            break;
                        #endregion
                        case 2:
                            {
                                var qtyOnHand = GetQOHSTF08A(string.IsNullOrEmpty(dataBarang_Stf02_BRG) ? barangInDb.BRG : dataBarang_Stf02_BRG, "ALL");
                                foreach (ARF01 tblCustomer in listBlibli)
                                {
                                    if (!string.IsNullOrEmpty(tblCustomer.Kode))
                                    {
                                        var stf02h = ErasoftDbContext.STF02H.Where(p => p.BRG == barangInDb.BRG && p.IDMARKET == tblCustomer.RecNum).FirstOrDefault();
                                        if (stf02h != null)
                                        {
                                            if (!string.IsNullOrEmpty(stf02h.BRG_MP))
                                            {
                                                var BliApi = new BlibliController();
                                                BlibliController.BlibliAPIData iden = new BlibliController.BlibliAPIData
                                                {
                                                    merchant_code = tblCustomer.Sort1_Cust,
                                                    API_client_password = tblCustomer.API_CLIENT_P,
                                                    API_client_username = tblCustomer.API_CLIENT_U,
                                                    API_secret_key = tblCustomer.API_KEY,
                                                    token = tblCustomer.TOKEN,
                                                    mta_username_email_merchant = tblCustomer.EMAIL,
                                                    mta_password_password_merchant = tblCustomer.PASSWORD,
                                                    idmarket = tblCustomer.RecNum.Value
                                                };
                                                if (stf02h.BRG_MP == "PENDING")
                                                {
                                                    BliApi.GetQueueFeedDetail(iden, null);
                                                }
                                                else
                                                {
                                                    #region update
                                                    BlibliController.BlibliProductData data = new BlibliController.BlibliProductData
                                                    {
                                                        kode = barangInDb.BRG,
                                                        kode_mp = stf02h.BRG_MP,
                                                        Qty = Convert.ToString(qtyOnHand),
                                                        MinQty = "0"
                                                    };
                                                    data.Price = stf02h.HJUAL.ToString();
                                                    data.MarketPrice = stf02h.HJUAL.ToString();
                                                    var display = Convert.ToBoolean(stf02h.DISPLAY);
                                                    data.display = display ? "true" : "false";
                                                    BliApi.UpdateProdukQOH_Display(iden, data);
                                                    #endregion
                                                }
                                            }
                                            else
                                            {
                                                var display = Convert.ToBoolean(stf02h.DISPLAY);
                                                if (display)
                                                {
                                                    #region insert
                                                    BlibliController.BlibliAPIData iden = new BlibliController.BlibliAPIData
                                                    {
                                                        merchant_code = tblCustomer.Sort1_Cust,
                                                        API_client_password = tblCustomer.API_CLIENT_P,
                                                        API_client_username = tblCustomer.API_CLIENT_U,
                                                        API_secret_key = tblCustomer.API_KEY,
                                                        token = tblCustomer.TOKEN,
                                                        mta_username_email_merchant = tblCustomer.EMAIL,
                                                        mta_password_password_merchant = tblCustomer.PASSWORD,
                                                        idmarket = tblCustomer.RecNum.Value
                                                    };
                                                    BlibliController.BlibliProductData data = new BlibliController.BlibliProductData
                                                    {
                                                        kode = string.IsNullOrEmpty(dataBarang_Stf02_BRG) ? barangInDb.BRG : dataBarang_Stf02_BRG,
                                                        nama = barangInDb.NAMA + ' ' + barangInDb.NAMA2 + ' ' + barangInDb.NAMA3,
                                                        berat = (barangInDb.BERAT).ToString(),//MO save dalam Gram, Elevenia dalam Kilogram
                                                        Keterangan = barangInDb.Deskripsi,
                                                        Qty = "0",
                                                        MinQty = "0",
                                                        PickupPoint = stf02h.PICKUP_POINT,
                                                        IDMarket = tblCustomer.RecNum.ToString(),
                                                        Length = Convert.ToString(barangInDb.PANJANG),
                                                        Width = Convert.ToString(barangInDb.LEBAR),
                                                        Height = Convert.ToString(barangInDb.TINGGI),
                                                        dataBarangInDb = barangInDb
                                                    };
                                                    data.Brand = ErasoftDbContext.STF02E.SingleOrDefault(m => m.KODE == barangInDb.Sort2 && m.LEVEL == "2").KET;
                                                    data.Price = Convert.ToString(stf02h.HJUAL);
                                                    data.MarketPrice = Convert.ToString(stf02h.HJUAL);
                                                    data.CategoryCode = Convert.ToString(stf02h.CATEGORY_CODE);

                                                    data.display = display ? "true" : "false";
                                                    BlibliController bliAPI = new BlibliController();
                                                    Task.Run(() => bliAPI.CreateProduct(iden, data).Wait());

                                                    #endregion
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        protected void saveBarangElevenia(int mode, BarangViewModel dataBarang, string[] imgPath)
        {
            //mode 1 Create Product - Hide Item, 2 Update, 3 Display / Hide Item
            var barangInDb = ErasoftDbContext.STF02.SingleOrDefault(b => b.ID == dataBarang.Stf02.ID || b.BRG == dataBarang.Stf02.BRG);
            var kdEl = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "ELEVENIA");
            if (barangInDb != null && kdEl != null)
            {
                var listElShop = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdEl.IdMarket.ToString()).ToList();
                if (listElShop.Count > 0)
                {
                    switch (mode)
                    {
                        #region Create Product lalu Hide Item
                        case 1:
                            {
                                #region getUrlImage, remark by calvin 19 nov 2018
                                //                                //string[] imgID = new string[Request.Files.Count];
                                //                                string[] imgID = new string[3];
                                //                                //if (Request.Files.Count > 0)
                                //                                //{
                                //                                for (int i = 0; i < 3; i++)
                                //                                {
                                //                                    //var file = Request.Files[i];

                                //                                    //if (file != null && file.ContentLength > 0)
                                //                                    //{
                                //                                    //    var fileExtension = Path.GetExtension(file.FileName);

                                //                                    //imgID[i] = "https://masteronline.co.id/ele/image?id=" + $"FotoProduk-{barangInDb.USERNAME}-{barangInDb.BRG}-foto-{i + 1}.jpg";
                                //#if AWS
                                //                                    imgID[i] = "https://masteronline.co.id/ele/image/" + $"FotoProduk-{barangInDb.USERNAME}-{barangInDb.BRG}-foto-{i + 1}";
                                //#else
                                //                                    imgID[i] = "https://dev.masteronline.co.id/ele/image/" + $"FotoProduk-{barangInDb.USERNAME}-{barangInDb.BRG}-foto-{i + 1}";
                                //#endif
                                //                                    //imgID[i] = Convert.ToString(imgID[i]).Replace(" ", "%20");

                                //                                    //}
                                //                                }
                                //                                //}
                                #endregion
                                foreach (ARF01 tblCustomer in listElShop)
                                {
                                    EleveniaController.EleveniaProductData data = new EleveniaController.EleveniaProductData
                                    {
                                        api_key = tblCustomer.API_KEY,
                                        kode = string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG,
                                        nama = dataBarang.Stf02.NAMA + ' ' + dataBarang.Stf02.NAMA2 + ' ' + dataBarang.Stf02.NAMA3,
                                        berat = (dataBarang.Stf02.BERAT / 1000).ToString(),//MO save dalam Gram, Elevenia dalam Kilogram
                                        imgUrl = imgPath,
                                        Keterangan = dataBarang.Stf02.Deskripsi,
                                        Qty = "1",
                                        DeliveryTempNo = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).DeliveryTempElevenia.ToString(),
                                        IDMarket = tblCustomer.RecNum.ToString(),
                                    };
                                    data.Brand = ErasoftDbContext.STF02E.SingleOrDefault(m => m.KODE == dataBarang.Stf02.Sort2 && m.LEVEL == "2").KET;
                                    data.Price = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).HJUAL.ToString();
                                    var display = Convert.ToBoolean(ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).DISPLAY);
                                    if (display)
                                    {
                                        var result = new EleveniaController().CreateProduct(data, display);
                                    }
                                }
                            }
                            break;
                        #endregion
                        #region Update Product
                        case 2:
                            {
                                #region getUrlImage, remark by calvin 19 nov 2018
                                //                                //string[] imgID = new string[Request.Files.Count];
                                //                                string[] imgID = new string[3];
                                //                                //if (Request.Files.Count > 0)
                                //                                //{
                                //                                for (int i = 0; i < 3; i++)
                                //                                {
                                //                                    //var file = Request.Files[i];

                                //                                    //if (file != null && file.ContentLength > 0)
                                //                                    //{
                                //                                    //    var fileExtension = Path.GetExtension(file.FileName);

                                //                                    //imgID[i] = "https://masteronline.co.id/ele/image?id=" + $"FotoProduk-{barangInDb.USERNAME}-{barangInDb.BRG}-foto-{i + 1}.jpg";
                                //#if AWS
                                //                                    imgID[i] = "https://masteronline.co.id/ele/image/" + $"FotoProduk-{barangInDb.USERNAME}-{barangInDb.BRG}-foto-{i + 1}";
                                //#else
                                //                                    imgID[i] = "https://dev.masteronline.co.id/ele/image/" + $"FotoProduk-{barangInDb.USERNAME}-{barangInDb.BRG}-foto-{i + 1}";
                                //#endif
                                //                                    //imgID[i] = Convert.ToString(imgID[i]).Replace(" ", "%20");

                                //                                    //}
                                //                                }
                                //                                //}
                                #endregion
                                foreach (ARF01 tblCustomer in listElShop)
                                {
                                    var qtyOnHand = GetQOHSTF08A(string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG, "ALL");

                                    EleveniaController.EleveniaProductData data = new EleveniaController.EleveniaProductData
                                    {
                                        api_key = tblCustomer.API_KEY,
                                        kode = string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG,
                                        nama = dataBarang.Stf02.NAMA + ' ' + dataBarang.Stf02.NAMA2 + ' ' + dataBarang.Stf02.NAMA3,
                                        berat = (dataBarang.Stf02.BERAT / 1000).ToString(),//MO save dalam Gram, Elevenia dalam Kilogram
                                        imgUrl = imgPath,
                                        Keterangan = dataBarang.Stf02.Deskripsi,
                                        Qty = Convert.ToString(qtyOnHand),
                                        DeliveryTempNo = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).DeliveryTempElevenia.ToString(),
                                        IDMarket = tblCustomer.RecNum.ToString(),
                                    };
                                    data.Brand = ErasoftDbContext.STF02E.SingleOrDefault(m => m.KODE == dataBarang.Stf02.Sort2 && m.LEVEL == "2").KET;
                                    data.Price = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).HJUAL.ToString();
                                    data.kode_mp = Convert.ToString(ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).BRG_MP);

                                    var display = Convert.ToBoolean(ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).DISPLAY);
                                    if (string.IsNullOrEmpty(data.kode_mp) && display)
                                    {
                                        var result = new EleveniaController().CreateProduct(data, display);
                                    }
                                    else if (!string.IsNullOrEmpty(data.kode_mp))
                                    {
                                        var result = new EleveniaController().UpdateProduct(data);
                                    }
                                    //if (result.resultCode.Equals("200"))
                                    //{
                                    //    #region Hide Item
                                    //    EleveniaController.EleveniaProductData data2 = new EleveniaController.EleveniaProductData
                                    //    {
                                    //        api_key = tblCustomer.TOKEN,
                                    //        kode = Convert.ToString(result.productNo)
                                    //    };
                                    //    var resultHide = new EleveniaController().HideItem(data2);
                                    //    #endregion
                                    //}
                                }
                            }
                            break;
                        #endregion
                        #region Display/Hide Item
                        case 3:
                            foreach (ARF01 tblCustomer in listElShop)
                            {
                                EleveniaController.EleveniaProductData data = new EleveniaController.EleveniaProductData
                                {
                                    api_key = tblCustomer.API_KEY,
                                    kode = Convert.ToString(ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).BRG_MP)
                                };
                                if (Convert.ToBoolean(ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).DISPLAY))
                                {
                                    var result = new EleveniaController().DisplayItem(data);
                                }
                                else if (!string.IsNullOrEmpty(data.kode_mp))//add by Tri, tidak perlu panggil api jika kode_mp == null
                                {
                                    var result = new EleveniaController().HideItem(data);
                                }
                            }
                            break;
                        #endregion
                        default:
                            break;
                    }
                }
            }
        }

        [Route("manage/EditBarang")]
        public ActionResult EditBarang(string barangId)
        {
            try
            {
                //add by calvin 9 nov 2018
                #region Blibli, get queue feed
                var kdBli = MoDbContext.Marketplaces.Single(m => m.NamaMarket.ToUpper() == "BLIBLI");
                var listBLIShop = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdBli.IdMarket.ToString()).ToList();
                if (listBLIShop.Count > 0)
                {
                    var BliApi = new BlibliController();
                    foreach (ARF01 tblCustomer in listBLIShop)
                    {
                        if (!string.IsNullOrEmpty(tblCustomer.API_CLIENT_P) && !string.IsNullOrEmpty(tblCustomer.API_CLIENT_U))
                        {
                            BlibliController.BlibliAPIData data = new BlibliController.BlibliAPIData()
                            {
                                API_client_username = tblCustomer.API_CLIENT_U,
                                API_client_password = tblCustomer.API_CLIENT_P,
                                API_secret_key = tblCustomer.API_KEY,
                                mta_username_email_merchant = tblCustomer.EMAIL,
                                mta_password_password_merchant = tblCustomer.PASSWORD,
                                merchant_code = tblCustomer.Sort1_Cust,
                                token = tblCustomer.TOKEN,
                                idmarket = tblCustomer.RecNum.Value
                            };
                            BliApi.GetQueueFeedDetail(data, null);
                        }
                    }
                }
                #endregion
                //end add by calvin 9 nov 2018

                var vm = new BarangViewModel()
                {
                    //change by nurul 18/1/2019 -- Stf02 = ErasoftDbContext.STF02.Single(b => b.BRG == barangId),
                    Stf02 = ErasoftDbContext.STF02.Where(p => (p.PART == null ? "" : p.PART) == "").Single(b => b.BRG == barangId),
                    //change by nurul 18/1/2019 -- ListStf02S = ErasoftDbContext.STF02.ToList(),
                    ListStf02S = ErasoftDbContext.STF02.Where(p => (p.PART == null ? "" : p.PART) == "").ToList(),
                    //ListCategoryBlibli = MoDbContext.CategoryBlibli.Where(p => string.IsNullOrEmpty(p.PARENT_CODE)).ToList(),
                    ListMarket = ErasoftDbContext.ARF01.OrderBy(p => p.RecNum).ToList(),
                    ListHargaJualPermarketView = ErasoftDbContext.STF02H.AsNoTracking().Where(h => h.BRG == barangId).OrderBy(p => p.IDMARKET).ToList(),
                    StatusLog = ErasoftDbContext.Database.SqlQuery<API_LOG_MARKETPLACE_PER_ITEM>("SELECT * FROM API_LOG_MARKETPLACE_PER_ITEM WHERE REQUEST_ATTRIBUTE_1 = '" + barangId + "' AND REQUEST_ACTION IN ('Create Product','create brg','create Produk')").ToList()
                };

                return PartialView("FormBarangPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult RefreshFormBarang()
        {
            var vm = new BarangViewModel()
            {
                ListKategoriMerk = ErasoftDbContext.STF02E.ToList(),
                ListMarket = ErasoftDbContext.ARF01.OrderBy(p => p.RecNum).ToList(),
                ListHargaJualPermarketView = ErasoftDbContext.STF02H.Where(p => 0 == 1).OrderBy(p => p.IDMARKET).ToList(),
                //ListCategoryBlibli = MoDbContext.CategoryBlibli.Where(p => string.IsNullOrEmpty(p.PARENT_CODE)).ToList(),
                DataUsaha = ErasoftDbContext.SIFSYS.Single(p => p.BLN == 1),
                StatusLog = ErasoftDbContext.Database.SqlQuery<API_LOG_MARKETPLACE_PER_ITEM>("SELECT * FROM API_LOG_MARKETPLACE_PER_ITEM WHERE 0 = 1").ToList()
            };

            return PartialView("FormBarangPartial", vm);
        }

        public ActionResult DeleteBarang(string barangId)
        {
            //change by nurul 18/1/2019 -- var barangInDb = ErasoftDbContext.STF02.Single(b => b.BRG == barangId);
            var barangInDb = ErasoftDbContext.STF02.Where(p => (p.PART == null ? "" : p.PART) == "").Single(b => b.BRG == barangId);

            //add by nurul 30/7/2018
            var vmError = new StokViewModel() { };

            var cekFaktur = ErasoftDbContext.SIT01B.Count(k => k.BRG == barangInDb.BRG);
            var cekPembelian = ErasoftDbContext.PBT01B.Count(k => k.BRG == barangInDb.BRG);
            var cekTransaksi = ErasoftDbContext.STT01B.Count(k => k.Kobar == barangInDb.BRG);
            var cekPesanan = ErasoftDbContext.SOT01B.Count(k => k.BRG == barangInDb.BRG);
            var cekPromosi = ErasoftDbContext.DETAILPROMOSI.Count(k => k.KODE_BRG == barangInDb.BRG);

            if (cekFaktur > 0 || cekPembelian > 0 || cekTransaksi > 0 || cekPesanan > 0 || cekPromosi > 0)
            {
                vmError.Errors.Add("Barang sudah dipakai di transaksi !");
                return Json(vmError, JsonRequestBehavior.AllowGet);
            }
            //end add

            ErasoftDbContext.STF02H.RemoveRange(ErasoftDbContext.STF02H.Where(h => h.BRG == barangId));
            ErasoftDbContext.STF02.Remove(barangInDb);
            ErasoftDbContext.SaveChanges();

            var partialVm = new BarangViewModel()
            {
                //change by nurul 18/1/2019 -- ListStf02S = ErasoftDbContext.STF02.ToList()
                ListStf02S = ErasoftDbContext.STF02.Where(p => (p.PART == null ? "" : p.PART) == "").ToList()
            };

            return PartialView("TableBarang1Partial", partialVm);
        }

        [Route("manage/promptdeliverytempelevenia")]
        public ActionResult PromptDeliveryTempElevenia(string recnum)
        {
            try
            {
                var PromptModel = ErasoftDbContext.DeliveryTemplateElevenia.Where(a => a.RECNUM_ARF01.ToString() == recnum).ToList();
                return View("PromptDeliveryTempElevenia", PromptModel);
            }
            catch (Exception ex)
            {
                return JsonErrorMessage("Prompt gagal");
            }
        }

        [Route("manage/promptetalasetokped")]
        public ActionResult PromptEtalaseTokped(string recnum)
        {
            try
            {
                int recnum_int = Convert.ToInt32(recnum);
                var tblCustomer = ErasoftDbContext.ARF01.Where(m => m.RecNum == recnum_int).FirstOrDefault();
                var tokopediaApi = new TokopediaController();

                TokopediaController.TokopediaAPIData iden = new TokopediaController.TokopediaAPIData
                {
                    merchant_code = tblCustomer.Sort1_Cust, //FSID
                    API_client_password = tblCustomer.API_CLIENT_P, //Client ID
                    API_client_username = tblCustomer.API_CLIENT_U, //Client Secret
                    API_secret_key = tblCustomer.API_KEY, //Shop ID 
                    token = tblCustomer.TOKEN,
                    idmarket = tblCustomer.RecNum.Value
                };
                var PromptModel = tokopediaApi.GetEtalase(iden);
                return View("PromptEtalaseTokopedia", PromptModel);
            }
            catch (Exception ex)
            {
                return JsonErrorMessage("Prompt gagal");
            }
        }


        [Route("manage/PromptDeliveryProviderLazada")]
        public ActionResult PromptDeliveryProviderLazada(string cust)
        {
            try
            {
                var PromptModel = ErasoftDbContext.DELIVERY_PROVIDER_LAZADA.Where(a => a.CUST == cust).ToList();
                return View("PromptDeliveryProviderLazada", PromptModel);
            }
            catch (Exception ex)
            {
                return JsonErrorMessage("Prompt gagal");
            }
        }

        public ActionResult PromptPickupPointBlibli(string merchant_code)
        {
            try
            {
                var PromptModel = ErasoftDbContext.PICKUP_POINT_BLIBLI.Where(a => a.MERCHANT_CODE.ToString() == merchant_code).ToList();
                return View("PromptPickupPointBlibli", PromptModel);
            }
            catch (Exception ex)
            {
                return JsonErrorMessage("Prompt gagal");
            }
        }


        protected JsonResult JsonErrorMessage(string message)
        {
            var vmError = new InvoiceViewModel()
            {
            };
            vmError.Errors.Add(message);
            return Json(vmError, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveKategoriBarang(KategoriBarangViewModel dataKategori)
        {
            if (!ModelState.IsValid)
            {
                var vm = new KategoriBarangViewModel()
                {
                    Kategori = dataKategori.Kategori,
                    ListKategori = ErasoftDbContext.STF02E.Where(k => k.LEVEL == "1").ToList()
                };

                return View("KategoriBarang", vm);
            }

            if (dataKategori.Kategori.RecNum == null)
            {
                var checkData = ErasoftDbContext.STF02E.SingleOrDefault(k => k.KODE == dataKategori.Kategori.KODE);

                if (checkData == null)
                {
                    ErasoftDbContext.STF02E.Add(dataKategori.Kategori);
                }
                else
                {
                    ModelState.AddModelError("", $@"Kategori dengan kode {dataKategori.Kategori.KODE} sudah dipakai oleh Anda / orang lain! Coba kode yang lain!");

                    var kategoriVm = new KategoriBarangViewModel()
                    {
                        Kategori = dataKategori.Kategori,
                        ListKategori = ErasoftDbContext.STF02E.Where(k => k.LEVEL == "1").ToList()
                    };

                    return View("KategoriBarang", kategoriVm);
                }
            }
            else
            {
                //var katInDb = ErasoftDbContext.STF02E.Single(k => k.KODE == dataKategori.Kategori.KODE);
                var katInDb = ErasoftDbContext.STF02E.Single(k => k.RecNum == dataKategori.Kategori.RecNum);

                //katInDb.KODE = dataKategori.Kategori.KODE;
                katInDb.KET = dataKategori.Kategori.KET;
            }

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            return RedirectToAction("RefreshTableKategori");
        }

        public ActionResult RefreshTableKategori()
        {
            var listKategori = ErasoftDbContext.STF02E.Where(k => k.LEVEL == "1").OrderByDescending(k => k.RecNum).ToList();

            return PartialView("TableKategoriPartial", listKategori.ToPagedList(1, 10));
        }

        public ActionResult EditKategori(int? recNum)
        {
            var vm = new KategoriBarangViewModel()
            {
                Kategori = ErasoftDbContext.STF02E.Single(k => k.RecNum == recNum)
            };

            return Json(vm, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult CekStrukturVar(string kode)
        {
            int validd = 0;
            var cekVariasi = ErasoftDbContext.STF20.Where(p => p.CATEGORY_MO == kode).FirstOrDefault();
            var cekOpsiVariasi = ErasoftDbContext.STF20B.Where(p => p.CATEGORY_MO == kode).FirstOrDefault();
            if (cekVariasi != null && cekOpsiVariasi != null)
            {
                validd = 1;
            }
            var data = new
            {
                adaVariasi = validd
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetStrukturVar(string kode, string brg)
        {
            var kategori = ErasoftDbContext.STF02E.Single(k => k.KODE == kode);
            var stf20 = ErasoftDbContext.STF20.Where(m => m.CATEGORY_MO == kategori.KODE).ToList();
            var vm = new BarangStrukturVarViewModel()
            {
                Barang = ErasoftDbContext.STF02.Where(p => p.BRG == brg).FirstOrDefault(),
                Kategori = kategori,
                Variant_Level_1 = new STF20()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_JUDUL_VAR = 1,
                    VALUE_JUDUL_VAR = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(1)).FirstOrDefault()?.VALUE_JUDUL_VAR
                },
                Variant_Level_2 = new STF20()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_JUDUL_VAR = 2,
                    VALUE_JUDUL_VAR = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(2)).FirstOrDefault()?.VALUE_JUDUL_VAR
                },
                Variant_Level_3 = new STF20()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_JUDUL_VAR = 3,
                    VALUE_JUDUL_VAR = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(3)).FirstOrDefault()?.VALUE_JUDUL_VAR
                },
                ListMarket = ErasoftDbContext.ARF01.OrderBy(p => p.RecNum).ToList(),
                VariantPerMP = ErasoftDbContext.STF02I.Where(p => p.BRG == brg).ToList(),
                VariantOptMaster = ErasoftDbContext.STF20B.Where(p => p.CATEGORY_MO == kategori.KODE).ToList()
            };
            return PartialView("BarangVarPartial", vm);
        }
        [HttpPost]
        public ActionResult GetOptVariantBarang(string brg, string code, int level)
        {
            var VariantOptInDb = ErasoftDbContext.STF20B.Where(m => m.CATEGORY_MO == code && m.LEVEL_VAR == level).ToList();
            var VariantSelected = ErasoftDbContext.STF02I.Where(m => m.BRG == brg && m.CATEGORY_MO == code && m.LEVEL_VAR == level).ToList();
            string selectedValues = "";
            foreach (var item in VariantSelected)
            {
                selectedValues += item.KODE_VAR + ",";
            }
            var data = new
            {
                selected = selectedValues,
                options = VariantOptInDb
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveOptVariantBarang(string brg, string shopee_code, string tokped_code, string blibli_code, string lazada_code, string code, string[] opt_selected_1, string[] opt_selected_2, string[] opt_selected_3)
        {
            var kategori = ErasoftDbContext.STF02E.Single(k => k.KODE == code);
            var stf20 = ErasoftDbContext.STF20.Where(m => m.CATEGORY_MO == kategori.KODE).ToList();
            var VariantOptMaster = ErasoftDbContext.STF20B.Where(p => p.CATEGORY_MO == kategori.KODE).ToList();
            List<STF02I> listNewData = new List<STF02I>();
            {
                var Histori_Shopee_stf02i = ErasoftDbContext.STF02I.Where(p => p.MARKET == "SHOPEE" && p.CATEGORY_MO == code && p.MP_CATEGORY_CODE == shopee_code).OrderByDescending(p => p.RECNUM).ToList();
                var Histori_Tokped_stf02i = ErasoftDbContext.STF02I.Where(p => p.MARKET == "TOKPED" && p.CATEGORY_MO == code && p.MP_CATEGORY_CODE == tokped_code).OrderByDescending(p => p.RECNUM).ToList();
                var Histori_Blibli_stf02i = ErasoftDbContext.STF02I.Where(p => p.MARKET == "BLIBLI" && p.CATEGORY_MO == code && p.MP_CATEGORY_CODE == blibli_code).OrderByDescending(p => p.RECNUM).ToList();
                var Histori_Lazada_stf02i = ErasoftDbContext.STF02I.Where(p => p.MARKET == "LAZADA" && p.CATEGORY_MO == code && p.MP_CATEGORY_CODE == lazada_code).OrderByDescending(p => p.RECNUM).ToList();

                if (opt_selected_1 != null)
                {
                    var Histori_Shopee = Histori_Shopee_stf02i.Where(p => p.LEVEL_VAR == 1).OrderByDescending(p => p.RECNUM).ToList();
                    var Histori_Tokped = Histori_Tokped_stf02i.Where(p => p.LEVEL_VAR == 1).OrderByDescending(p => p.RECNUM).ToList();
                    var Histori_Blibli = Histori_Blibli_stf02i.Where(p => p.LEVEL_VAR == 1).OrderByDescending(p => p.RECNUM).ToList();
                    var Histori_Lazada = Histori_Lazada_stf02i.Where(p => p.LEVEL_VAR == 1).OrderByDescending(p => p.RECNUM).ToList();
                    foreach (var item in opt_selected_1)
                    {
                        if (item != "")
                        {
                            string JudulShopee = "";
                            var FoundHistoriJudulShopee = Histori_Shopee.FirstOrDefault();
                            if (FoundHistoriJudulShopee != null)
                            {
                                JudulShopee = FoundHistoriJudulShopee.MP_JUDUL_VAR;
                                if (string.IsNullOrWhiteSpace(JudulShopee))
                                {
                                    JudulShopee = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(1)).FirstOrDefault()?.VALUE_JUDUL_VAR;
                                }
                            }
                            else
                            {
                                JudulShopee = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(1)).FirstOrDefault()?.VALUE_JUDUL_VAR;
                            }
                            string ValueShopee = "";
                            var FoundHistoriValueShopee = Histori_Shopee.FirstOrDefault(p => p.KODE_VAR == item);
                            if (FoundHistoriValueShopee != null)
                            {
                                ValueShopee = FoundHistoriValueShopee.MP_VALUE_VAR;
                                if (string.IsNullOrWhiteSpace(ValueShopee))
                                {
                                    ValueShopee = VariantOptMaster.Where(m => m.LEVEL_VAR.Equals(1) && m.KODE_VAR == item).FirstOrDefault()?.KET_VAR;
                                }
                            }
                            else
                            {
                                ValueShopee = VariantOptMaster.Where(m => m.LEVEL_VAR.Equals(1) && m.KODE_VAR == item).FirstOrDefault()?.KET_VAR;
                            }

                            STF02I newdata = new STF02I()
                            {
                                MARKET = "SHOPEE",
                                BRG = brg,
                                CATEGORY_MO = code,
                                KODE_VAR = item,
                                LEVEL_VAR = 1,
                                MP_JUDUL_VAR = JudulShopee,
                                MP_VALUE_VAR = ValueShopee,
                                MP_CATEGORY_CODE = shopee_code
                            };
                            listNewData.Add(newdata);

                            STF02I newdataTokped = new STF02I()
                            {
                                MARKET = "TOKPED",
                                BRG = brg,
                                CATEGORY_MO = code,
                                KODE_VAR = item,
                                LEVEL_VAR = 1,
                                MP_JUDUL_VAR = Histori_Tokped.FirstOrDefault()?.MP_JUDUL_VAR,
                                MP_VALUE_VAR = Histori_Tokped.FirstOrDefault(p => p.KODE_VAR == item)?.MP_VALUE_VAR,
                                MP_CATEGORY_CODE = tokped_code
                            };
                            listNewData.Add(newdataTokped);

                            STF02I newdataBlibli = new STF02I()
                            {
                                MARKET = "BLIBLI",
                                BRG = brg,
                                CATEGORY_MO = code,
                                KODE_VAR = item,
                                LEVEL_VAR = 1,
                                MP_JUDUL_VAR = Histori_Blibli.FirstOrDefault()?.MP_JUDUL_VAR,
                                MP_VALUE_VAR = Histori_Blibli.FirstOrDefault(p => p.KODE_VAR == item)?.MP_VALUE_VAR,
                                MP_CATEGORY_CODE = blibli_code
                            };
                            listNewData.Add(newdataBlibli);

                            STF02I newdataLazada = new STF02I()
                            {
                                MARKET = "LAZADA",
                                BRG = brg,
                                CATEGORY_MO = code,
                                KODE_VAR = item,
                                LEVEL_VAR = 1,
                                MP_JUDUL_VAR = Histori_Lazada.FirstOrDefault()?.MP_JUDUL_VAR,
                                MP_VALUE_VAR = Histori_Lazada.FirstOrDefault(p => p.KODE_VAR == item)?.MP_VALUE_VAR,
                                MP_CATEGORY_CODE = lazada_code
                            };
                            listNewData.Add(newdataLazada);
                        }
                    }
                }
                if (opt_selected_2 != null)
                {
                    var Histori_Shopee = Histori_Shopee_stf02i.Where(p => p.LEVEL_VAR == 2).OrderByDescending(p => p.RECNUM).ToList();
                    var Histori_Tokped = Histori_Tokped_stf02i.Where(p => p.LEVEL_VAR == 2).OrderByDescending(p => p.RECNUM).ToList();
                    var Histori_Blibli = Histori_Blibli_stf02i.Where(p => p.LEVEL_VAR == 2).OrderByDescending(p => p.RECNUM).ToList();
                    var Histori_Lazada = Histori_Lazada_stf02i.Where(p => p.LEVEL_VAR == 2).OrderByDescending(p => p.RECNUM).ToList();
                    foreach (var item in opt_selected_2)
                    {
                        if (item != "")
                        {
                            string JudulShopee = "";
                            var FoundHistoriJudulShopee = Histori_Shopee.FirstOrDefault();
                            if (FoundHistoriJudulShopee != null)
                            {
                                JudulShopee = FoundHistoriJudulShopee.MP_JUDUL_VAR;
                                if (string.IsNullOrWhiteSpace(JudulShopee))
                                {
                                    JudulShopee = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(2)).FirstOrDefault()?.VALUE_JUDUL_VAR;
                                }
                            }
                            else
                            {
                                JudulShopee = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(2)).FirstOrDefault()?.VALUE_JUDUL_VAR;
                            }
                            string ValueShopee = "";
                            var FoundHistoriValueShopee = Histori_Shopee.FirstOrDefault(p => p.KODE_VAR == item);
                            if (FoundHistoriValueShopee != null)
                            {
                                ValueShopee = FoundHistoriValueShopee.MP_VALUE_VAR;
                                if (string.IsNullOrWhiteSpace(ValueShopee))
                                {
                                    ValueShopee = VariantOptMaster.Where(m => m.LEVEL_VAR.Equals(2) && m.KODE_VAR == item).FirstOrDefault()?.KET_VAR;
                                }
                            }
                            else
                            {
                                ValueShopee = VariantOptMaster.Where(m => m.LEVEL_VAR.Equals(2) && m.KODE_VAR == item).FirstOrDefault()?.KET_VAR;
                            }

                            STF02I newdata = new STF02I()
                            {
                                MARKET = "SHOPEE",
                                BRG = brg,
                                CATEGORY_MO = code,
                                KODE_VAR = item,
                                LEVEL_VAR = 2,
                                MP_JUDUL_VAR = JudulShopee,
                                MP_VALUE_VAR = ValueShopee,
                                MP_CATEGORY_CODE = shopee_code
                            };
                            listNewData.Add(newdata);

                            STF02I newdataTokped = new STF02I()
                            {
                                MARKET = "TOKPED",
                                BRG = brg,
                                CATEGORY_MO = code,
                                KODE_VAR = item,
                                LEVEL_VAR = 2,
                                MP_JUDUL_VAR = Histori_Tokped.FirstOrDefault()?.MP_JUDUL_VAR,
                                MP_VALUE_VAR = Histori_Tokped.FirstOrDefault(p => p.KODE_VAR == item)?.MP_VALUE_VAR,
                                MP_CATEGORY_CODE = tokped_code
                            };
                            listNewData.Add(newdataTokped);

                            STF02I newdataBlibli = new STF02I()
                            {
                                MARKET = "BLIBLI",
                                BRG = brg,
                                CATEGORY_MO = code,
                                KODE_VAR = item,
                                LEVEL_VAR = 2,
                                MP_JUDUL_VAR = Histori_Blibli.FirstOrDefault()?.MP_JUDUL_VAR,
                                MP_VALUE_VAR = Histori_Blibli.FirstOrDefault(p => p.KODE_VAR == item)?.MP_VALUE_VAR,
                                MP_CATEGORY_CODE = blibli_code
                            };
                            listNewData.Add(newdataBlibli);

                            STF02I newdataLazada = new STF02I()
                            {
                                MARKET = "LAZADA",
                                BRG = brg,
                                CATEGORY_MO = code,
                                KODE_VAR = item,
                                LEVEL_VAR = 2,
                                MP_JUDUL_VAR = Histori_Lazada.FirstOrDefault()?.MP_JUDUL_VAR,
                                MP_VALUE_VAR = Histori_Lazada.FirstOrDefault(p => p.KODE_VAR == item)?.MP_VALUE_VAR,
                                MP_CATEGORY_CODE = lazada_code
                            };
                            listNewData.Add(newdataLazada);
                        }
                    }
                }
                if (opt_selected_3 != null)
                {
                    var Histori_Shopee = Histori_Shopee_stf02i.Where(p => p.LEVEL_VAR == 3).OrderByDescending(p => p.RECNUM).ToList();
                    var Histori_Tokped = Histori_Tokped_stf02i.Where(p => p.LEVEL_VAR == 3).OrderByDescending(p => p.RECNUM).ToList();
                    var Histori_Blibli = Histori_Blibli_stf02i.Where(p => p.LEVEL_VAR == 3).OrderByDescending(p => p.RECNUM).ToList();
                    var Histori_Lazada = Histori_Lazada_stf02i.Where(p => p.LEVEL_VAR == 3).OrderByDescending(p => p.RECNUM).ToList();
                    foreach (var item in opt_selected_3)
                    {
                        if (item != "")
                        {
                            string JudulShopee = "";
                            var FoundHistoriJudulShopee = Histori_Shopee.FirstOrDefault();
                            if (FoundHistoriJudulShopee != null)
                            {
                                JudulShopee = FoundHistoriJudulShopee.MP_JUDUL_VAR;
                                if (string.IsNullOrWhiteSpace(JudulShopee))
                                {
                                    JudulShopee = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(3)).FirstOrDefault()?.VALUE_JUDUL_VAR;
                                }
                            }
                            else
                            {
                                JudulShopee = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(3)).FirstOrDefault()?.VALUE_JUDUL_VAR;
                            }
                            string ValueShopee = "";
                            var FoundHistoriValueShopee = Histori_Shopee.FirstOrDefault(p => p.KODE_VAR == item);
                            if (FoundHistoriValueShopee != null)
                            {
                                ValueShopee = FoundHistoriValueShopee.MP_VALUE_VAR;
                                if (string.IsNullOrWhiteSpace(ValueShopee))
                                {
                                    ValueShopee = VariantOptMaster.Where(m => m.LEVEL_VAR.Equals(3) && m.KODE_VAR == item).FirstOrDefault()?.KET_VAR;
                                }
                            }
                            else
                            {
                                ValueShopee = VariantOptMaster.Where(m => m.LEVEL_VAR.Equals(3) && m.KODE_VAR == item).FirstOrDefault()?.KET_VAR;
                            }

                            STF02I newdata = new STF02I()
                            {
                                MARKET = "SHOPEE",
                                BRG = brg,
                                CATEGORY_MO = code,
                                KODE_VAR = item,
                                LEVEL_VAR = 3,
                                MP_JUDUL_VAR = JudulShopee,
                                MP_VALUE_VAR = ValueShopee,
                                MP_CATEGORY_CODE = shopee_code
                            };
                            listNewData.Add(newdata);

                            STF02I newdataTokped = new STF02I()
                            {
                                MARKET = "TOKPED",
                                BRG = brg,
                                CATEGORY_MO = code,
                                KODE_VAR = item,
                                LEVEL_VAR = 3,
                                MP_JUDUL_VAR = Histori_Tokped.FirstOrDefault()?.MP_JUDUL_VAR,
                                MP_VALUE_VAR = Histori_Tokped.FirstOrDefault(p => p.KODE_VAR == item)?.MP_VALUE_VAR,
                                MP_CATEGORY_CODE = tokped_code
                            };
                            listNewData.Add(newdataTokped);

                            STF02I newdataBlibli = new STF02I()
                            {
                                MARKET = "BLIBLI",
                                BRG = brg,
                                CATEGORY_MO = code,
                                KODE_VAR = item,
                                LEVEL_VAR = 3,
                                MP_JUDUL_VAR = Histori_Blibli.FirstOrDefault()?.MP_JUDUL_VAR,
                                MP_VALUE_VAR = Histori_Blibli.FirstOrDefault(p => p.KODE_VAR == item)?.MP_VALUE_VAR,
                                MP_CATEGORY_CODE = blibli_code
                            };
                            listNewData.Add(newdataBlibli);

                            STF02I newdataLazada = new STF02I()
                            {
                                MARKET = "LAZADA",
                                BRG = brg,
                                CATEGORY_MO = code,
                                KODE_VAR = item,
                                LEVEL_VAR = 3,
                                MP_JUDUL_VAR = Histori_Lazada.FirstOrDefault()?.MP_JUDUL_VAR,
                                MP_VALUE_VAR = Histori_Lazada.FirstOrDefault(p => p.KODE_VAR == item)?.MP_VALUE_VAR,
                                MP_CATEGORY_CODE = lazada_code
                            };
                            listNewData.Add(newdataLazada);
                        }
                    }
                }
            }
            if (listNewData.Count() > 0)
            {
                var listStf02IinDb = ErasoftDbContext.STF02I.Where(p => p.BRG == brg).ToList();
                ErasoftDbContext.STF02I.RemoveRange(listStf02IinDb);
                ErasoftDbContext.SaveChanges();

                ErasoftDbContext.STF02I.AddRange(listNewData);
                ErasoftDbContext.SaveChanges();
            }

            var vm = new BarangStrukturVarViewModel()
            {
                Barang = ErasoftDbContext.STF02.Where(p => p.BRG == brg).FirstOrDefault(),
                Kategori = kategori,
                Variant_Level_1 = new STF20()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_JUDUL_VAR = 1,
                    VALUE_JUDUL_VAR = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(1)).FirstOrDefault()?.VALUE_JUDUL_VAR
                },
                Variant_Level_2 = new STF20()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_JUDUL_VAR = 2,
                    VALUE_JUDUL_VAR = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(2)).FirstOrDefault()?.VALUE_JUDUL_VAR
                },
                Variant_Level_3 = new STF20()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_JUDUL_VAR = 3,
                    VALUE_JUDUL_VAR = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(3)).FirstOrDefault()?.VALUE_JUDUL_VAR
                },
                ListMarket = ErasoftDbContext.ARF01.OrderBy(p => p.RecNum).ToList(),
                VariantPerMP = ErasoftDbContext.STF02I.Where(p => p.BRG == brg).ToList(),
                VariantOptMaster = VariantOptMaster
            };
            return PartialView("BarangVarPartial", vm);
        }
        public class UpdateBatchHjualVariant
        {
            public int recnum { get; set; }
            public int hjual { get; set; }
        }
        public ActionResult UpdateHjualVariantBarang(string brg, List<UpdateBatchHjualVariant> newhjual)
        {
            List<int> ids = new List<int>();
            foreach (var item in newhjual)
            {
                ids.Add(item.recnum);
            }
            foreach (var record in ErasoftDbContext.STF02H.Where(x => ids.Contains(x.RecNum.HasValue ? x.RecNum.Value : 0)).ToList())
            {
                record.HJUAL = newhjual.Where(p => p.recnum == record.RecNum).SingleOrDefault().hjual;
            }
            ErasoftDbContext.SaveChanges();

            ModelState.Clear();

            //ingat ganti saat publish, by calvin
            saveBarangShopeeVariant(2, brg, false);
            saveBarangBlibliVariant(2, brg);
            saveBarangTokpedVariant(2, brg, false);

            var partialVm = new BarangViewModel()
            {
                ListStf02S = ErasoftDbContext.STF02.Where(p => (p.PART == null ? "" : p.PART) == "").ToList(),
                ListHargaJualPermarketView = ErasoftDbContext.STF02H.Where(p => 0 == 1).OrderBy(p => p.IDMARKET).ToList(),
            };

            return PartialView("TableBarang1Partial", partialVm);
        }
        public ActionResult GetDetailBarangVar(string kode, string brg)
        {
            var VariantMO = ErasoftDbContext.STF02.Where(p => (p.PART == null ? "" : p.PART) == brg).ToList();
            var listBrgVariantMO = VariantMO.Select(p => p.BRG).ToList();
            var VariantMO_H = ErasoftDbContext.STF02H.Where(p => listBrgVariantMO.Contains(p.BRG)).ToList();

            var vm = new BarangDetailVarViewModel()
            {
                VariantMO = VariantMO,
                VariantMO_H = VariantMO_H,
                ListMarket = ErasoftDbContext.ARF01.OrderBy(p => p.RecNum).ToList()
            };

            return PartialView("BarangDetailVarPartial", vm);
        }
        public class StrukturVariantMp
        {
            public string code { get; set; }
            public StrukturVariantMPJudul var_judul { get; set; }
            public StrukturVariantMPOpt var_detail { get; set; }
        }
        public class StrukturVariantMPJudul
        {
            public string lv_1 { get; set; }
            public string lv_2 { get; set; }
            public string lv_3 { get; set; }
        }
        public class StrukturVariantMPOpt
        {
            public string[] lv_1 { get; set; }
            public string[] lv_2 { get; set; }
            public string[] lv_3 { get; set; }
        }
        protected STF02 CopyStf02(STF02 source)
        {
            STF02 newCopy = new STF02()
            {
                BRG = source.BRG,
                ANTIBIOTIK = source.ANTIBIOTIK,
                BERAT = source.BERAT,
                BRG_NON_OS = source.BRG_NON_OS,
                BSK = source.BSK,
                DEFAULT_STN_HRG_JUAL = source.DEFAULT_STN_HRG_JUAL,
                DEFAULT_STN_JUAL = source.DEFAULT_STN_JUAL,
                Deskripsi = source.Deskripsi,
                DISPLAY_MARKET = source.DISPLAY_MARKET,
                FORMULARIUM = source.FORMULARIUM,
                GENERIC = source.GENERIC,
                HBELI = source.HBELI,
                HBESAR = source.HBESAR,
                HJUAL = source.HJUAL,
                HKECIL = source.HKECIL,
                HNA_PPN = source.HNA_PPN,
                HPP = source.HPP,
                HP_STD = source.HP_STD,
                H_STN_3 = source.H_STN_3,
                H_STN_4 = source.H_STN_4,
                ISI = source.ISI,
                ISI3 = source.ISI3,
                ISI4 = source.ISI4,
                JENIS = source.JENIS,
                KET_SORT1 = source.KET_SORT1,
                Ket_Sort10 = source.Ket_Sort10,
                KET_SORT2 = source.KET_SORT2,
                KET_SORT3 = source.KET_SORT3,
                KET_SORT4 = source.KET_SORT4,
                KET_SORT5 = source.KET_SORT5,
                Ket_Sort6 = source.Ket_Sort6,
                Ket_Sort7 = source.Ket_Sort7,
                Ket_Sort8 = source.Ket_Sort8,
                Ket_Sort9 = source.Ket_Sort9,
                KET_STN = source.KET_STN,
                KET_STN2 = source.KET_STN2,
                KET_STN3 = source.KET_STN3,
                KET_STN4 = source.KET_STN4,
                KLINK = source.KLINK,
                KUBILASI = source.KUBILASI,
                LABA = source.LABA,
                LEBAR = source.LEBAR,
                LINK_GAMBAR_1 = source.LINK_GAMBAR_1,
                LINK_GAMBAR_2 = source.LINK_GAMBAR_2,
                LINK_GAMBAR_3 = source.LINK_GAMBAR_3,
                LKS = source.LKS,
                LT = source.LT,
                MAXI = source.MAXI,
                MEREK = source.MEREK,
                Metoda = source.Metoda,
                METODA_HPP_PER_SN = source.METODA_HPP_PER_SN,
                MINI = source.MINI,
                MVC = source.MVC,
                NAMA = source.NAMA,
                NAMA2 = source.NAMA2,
                NAMA3 = source.NAMA3,
                NARKOTIK = source.NARKOTIK,
                OC = source.OC,
                PANJANG = source.PANJANG,
                PART = source.PART,
                Photo = source.Photo,
                PHOTO2 = source.PHOTO2,
                PSIKOTROPIK = source.PSIKOTROPIK,
                QPROD = source.QPROD,
                QSALES = source.QSALES,
                Qty_berat = source.Qty_berat,
                Sort1 = source.Sort1,
                Sort10 = source.Sort10,
                Sort2 = source.Sort2,
                Sort3 = source.Sort3,
                Sort4 = source.Sort4,
                Sort5 = source.Sort5,
                Sort6 = source.Sort6,
                Sort7 = source.Sort7,
                Sort8 = source.Sort8,
                Sort9 = source.Sort9,
                SS = source.SS,
                STN = source.STN,
                STN2 = source.STN2,
                STN3 = source.STN3,
                STN4 = source.STN4,
                Stn_berat = source.Stn_berat,
                SUP = source.SUP,
                Tgl_Input = source.Tgl_Input,
                TGL_KLR = source.TGL_KLR,
                TINGGI = source.TINGGI,
                TOLERANSI = source.TOLERANSI,
                TYPE = source.TYPE,
                USERNAME = source.USERNAME,
                WARNA = source.WARNA
            };
            return newCopy;
        }
        protected STF02H CopyStf02h(STF02H source)
        {
            STF02H newCopy = new STF02H()
            {
                BRG = source.BRG,
                DISPLAY = source.DISPLAY,
                AKUNMARKET = source.AKUNMARKET,
                BRG_MP = source.BRG_MP,
                HJUAL = source.HJUAL,
                IDMARKET = source.IDMARKET,
                USERNAME = source.USERNAME,
                #region Category && Attribute
                CATEGORY_CODE = source.CATEGORY_CODE,
                CATEGORY_NAME = source.CATEGORY_NAME,
                DeliveryTempElevenia = source.DeliveryTempElevenia,
                PICKUP_POINT = source.PICKUP_POINT,
                ACODE_1 = source.ACODE_1,
                ACODE_2 = source.ACODE_2,
                ACODE_3 = source.ACODE_3,
                ACODE_4 = source.ACODE_4,
                ACODE_5 = source.ACODE_5,
                ACODE_6 = source.ACODE_6,
                ACODE_7 = source.ACODE_7,
                ACODE_8 = source.ACODE_8,
                ACODE_9 = source.ACODE_9,
                ACODE_10 = source.ACODE_10,
                ACODE_11 = source.ACODE_11,
                ACODE_12 = source.ACODE_12,
                ACODE_13 = source.ACODE_13,
                ACODE_14 = source.ACODE_14,
                ACODE_15 = source.ACODE_15,
                ACODE_16 = source.ACODE_16,
                ACODE_17 = source.ACODE_17,
                ACODE_18 = source.ACODE_18,
                ACODE_19 = source.ACODE_19,
                ACODE_20 = source.ACODE_20,
                ACODE_21 = source.ACODE_21,
                ACODE_22 = source.ACODE_22,
                ACODE_23 = source.ACODE_23,
                ACODE_24 = source.ACODE_24,
                ACODE_25 = source.ACODE_25,
                ACODE_26 = source.ACODE_26,
                ACODE_27 = source.ACODE_27,
                ACODE_28 = source.ACODE_28,
                ACODE_29 = source.ACODE_29,
                ACODE_30 = source.ACODE_30,
                ACODE_31 = source.ACODE_31,
                ACODE_32 = source.ACODE_32,
                ACODE_33 = source.ACODE_33,
                ACODE_34 = source.ACODE_34,
                ACODE_35 = source.ACODE_35,
                ACODE_36 = source.ACODE_36,
                ACODE_37 = source.ACODE_37,
                ACODE_38 = source.ACODE_38,
                ACODE_39 = source.ACODE_39,
                ACODE_40 = source.ACODE_40,
                ACODE_41 = source.ACODE_41,
                ACODE_42 = source.ACODE_42,
                ACODE_43 = source.ACODE_43,
                ACODE_44 = source.ACODE_44,
                ACODE_45 = source.ACODE_45,
                ACODE_46 = source.ACODE_46,
                ACODE_47 = source.ACODE_47,
                ACODE_48 = source.ACODE_48,
                ACODE_49 = source.ACODE_49,
                ACODE_50 = source.ACODE_50,

                ANAME_1 = source.ANAME_1,
                ANAME_2 = source.ANAME_2,
                ANAME_3 = source.ANAME_3,
                ANAME_4 = source.ANAME_4,
                ANAME_5 = source.ANAME_5,
                ANAME_6 = source.ANAME_6,
                ANAME_7 = source.ANAME_7,
                ANAME_8 = source.ANAME_8,
                ANAME_9 = source.ANAME_9,
                ANAME_10 = source.ANAME_10,
                ANAME_11 = source.ANAME_11,
                ANAME_12 = source.ANAME_12,
                ANAME_13 = source.ANAME_13,
                ANAME_14 = source.ANAME_14,
                ANAME_15 = source.ANAME_15,
                ANAME_16 = source.ANAME_16,
                ANAME_17 = source.ANAME_17,
                ANAME_18 = source.ANAME_18,
                ANAME_19 = source.ANAME_19,
                ANAME_20 = source.ANAME_20,
                ANAME_21 = source.ANAME_21,
                ANAME_22 = source.ANAME_22,
                ANAME_23 = source.ANAME_23,
                ANAME_24 = source.ANAME_24,
                ANAME_25 = source.ANAME_25,
                ANAME_26 = source.ANAME_26,
                ANAME_27 = source.ANAME_27,
                ANAME_28 = source.ANAME_28,
                ANAME_29 = source.ANAME_29,
                ANAME_30 = source.ANAME_30,
                ANAME_31 = source.ANAME_31,
                ANAME_32 = source.ANAME_32,
                ANAME_33 = source.ANAME_33,
                ANAME_34 = source.ANAME_34,
                ANAME_35 = source.ANAME_35,
                ANAME_36 = source.ANAME_36,
                ANAME_37 = source.ANAME_37,
                ANAME_38 = source.ANAME_38,
                ANAME_39 = source.ANAME_39,
                ANAME_40 = source.ANAME_40,
                ANAME_41 = source.ANAME_41,
                ANAME_42 = source.ANAME_42,
                ANAME_43 = source.ANAME_43,
                ANAME_44 = source.ANAME_44,
                ANAME_45 = source.ANAME_45,
                ANAME_46 = source.ANAME_46,
                ANAME_47 = source.ANAME_47,
                ANAME_48 = source.ANAME_48,
                ANAME_49 = source.ANAME_49,
                ANAME_50 = source.ANAME_50,

                AVALUE_1 = source.AVALUE_1,
                AVALUE_2 = source.AVALUE_2,
                AVALUE_3 = source.AVALUE_3,
                AVALUE_4 = source.AVALUE_4,
                AVALUE_5 = source.AVALUE_5,
                AVALUE_6 = source.AVALUE_6,
                AVALUE_7 = source.AVALUE_7,
                AVALUE_8 = source.AVALUE_8,
                AVALUE_9 = source.AVALUE_9,
                AVALUE_10 = source.AVALUE_10,
                AVALUE_11 = source.AVALUE_11,
                AVALUE_12 = source.AVALUE_12,
                AVALUE_13 = source.AVALUE_13,
                AVALUE_14 = source.AVALUE_14,
                AVALUE_15 = source.AVALUE_15,
                AVALUE_16 = source.AVALUE_16,
                AVALUE_17 = source.AVALUE_17,
                AVALUE_18 = source.AVALUE_18,
                AVALUE_19 = source.AVALUE_19,
                AVALUE_20 = source.AVALUE_20,
                AVALUE_21 = source.AVALUE_21,
                AVALUE_22 = source.AVALUE_22,
                AVALUE_23 = source.AVALUE_23,
                AVALUE_24 = source.AVALUE_24,
                AVALUE_25 = source.AVALUE_25,
                AVALUE_26 = source.AVALUE_26,
                AVALUE_27 = source.AVALUE_27,
                AVALUE_28 = source.AVALUE_28,
                AVALUE_29 = source.AVALUE_29,
                AVALUE_30 = source.AVALUE_30,
                AVALUE_31 = source.AVALUE_31,
                AVALUE_32 = source.AVALUE_32,
                AVALUE_33 = source.AVALUE_33,
                AVALUE_34 = source.AVALUE_34,
                AVALUE_35 = source.AVALUE_35,
                AVALUE_36 = source.AVALUE_36,
                AVALUE_37 = source.AVALUE_37,
                AVALUE_38 = source.AVALUE_38,
                AVALUE_39 = source.AVALUE_39,
                AVALUE_40 = source.AVALUE_40,
                AVALUE_41 = source.AVALUE_41,
                AVALUE_42 = source.AVALUE_42,
                AVALUE_43 = source.AVALUE_43,
                AVALUE_44 = source.AVALUE_44,
                AVALUE_45 = source.AVALUE_45,
                AVALUE_46 = source.AVALUE_46,
                AVALUE_47 = source.AVALUE_47,
                AVALUE_48 = source.AVALUE_48,
                AVALUE_49 = source.AVALUE_49,
                AVALUE_50 = source.AVALUE_50,
                #endregion
            };
            return newCopy;
        }
        [HttpPost]
        public ActionResult UpdateGambarVariantBarang()
        {
            bool first = true;
            Dictionary<int, string> same_uploaded = new Dictionary<int, string>();
            foreach (var item in Request.Files.AllKeys)
            {
                int stf02_id = Convert.ToInt32(item);
                var itemVar = ErasoftDbContext.STF02.Where(p => p.ID == stf02_id).SingleOrDefault();
                if (itemVar != null)
                {
                    var file = Request.Files[item];

                    if (file != null && file.ContentLength > 0)
                    {
                        if (!same_uploaded.ContainsKey(file.ContentLength))
                        {
                            ImgurImageResponse image = UploadImageService.UploadSingleImageToImgur(file, "uploaded-image");
                            itemVar.LINK_GAMBAR_1 = image.data.link_l;
                            same_uploaded.Add(file.ContentLength, image.data.link_l);
                        }
                        else
                        {
                            itemVar.LINK_GAMBAR_1 = same_uploaded.Where(p => p.Key == file.ContentLength).FirstOrDefault().Value;
                        }

                        //add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat
                        itemVar.Sort5 = Convert.ToString(file.ContentLength);

                        if (first)
                        {
                            var itemInduk = ErasoftDbContext.STF02.Where(p => p.BRG == itemVar.PART).SingleOrDefault();
                            if (itemInduk != null)
                            {
                                if (string.IsNullOrWhiteSpace(itemInduk.Sort5))
                                {
                                    itemInduk.Sort5 = Convert.ToString(file.ContentLength);
                                    itemInduk.LINK_GAMBAR_1 = itemVar.LINK_GAMBAR_1;
                                }
                            }
                        }
                    }
                    ErasoftDbContext.SaveChanges();
                }
                first = false;
            }
            return Json($"Update Gambar Variant Berhasil.", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateGambarBlibli()
        {
            if (Request.Files.AllKeys.Count() > 0)
            {
                Dictionary<int, string> same_uploaded = new Dictionary<int, string>();
                string first_key = Convert.ToString(Request.Files.AllKeys.FirstOrDefault());
                string brg = first_key.Split(';')[2];

                var itemVarAllMarket = ErasoftDbContext.STF02H.Where(p => p.BRG == brg).ToList();
                foreach (var item in Request.Files.AllKeys)
                {
                    string[] key_split = item.Split(';');
                    int urutan = Convert.ToInt32(key_split[0]);
                    int idmarket = Convert.ToInt32(key_split[1]);
                    var itemVar = itemVarAllMarket.Where(p => p.IDMARKET == idmarket).SingleOrDefault();
                    if (itemVar != null)
                    {
                        var file = Request.Files[item];

                        if (file != null && file.ContentLength > 0)
                        {
                            switch (urutan)
                            {
                                case 1:
                                    {
                                        if (!same_uploaded.ContainsKey(file.ContentLength))
                                        {
                                            ImgurImageResponse image = UploadImageService.UploadSingleImageToImgur(file, "uploaded-image");
                                            itemVar.AVALUE_50 = image.data.link_l;
                                            same_uploaded.Add(file.ContentLength, image.data.link_l);
                                        }
                                        else
                                        {
                                            itemVar.AVALUE_50 = same_uploaded.Where(p => p.Key == file.ContentLength).FirstOrDefault().Value;
                                        }

                                        itemVar.ACODE_50 = Convert.ToString(file.ContentLength);
                                        ErasoftDbContext.SaveChanges();
                                    }
                                    break;
                                case 2:
                                    {
                                        if (!same_uploaded.ContainsKey(file.ContentLength))
                                        {
                                            ImgurImageResponse image = UploadImageService.UploadSingleImageToImgur(file, "uploaded-image");
                                            itemVar.AVALUE_49 = image.data.link_l;
                                            same_uploaded.Add(file.ContentLength, image.data.link_l);
                                        }
                                        else
                                        {
                                            itemVar.AVALUE_49 = same_uploaded.Where(p => p.Key == file.ContentLength).FirstOrDefault().Value;
                                        }

                                        itemVar.ACODE_49 = Convert.ToString(file.ContentLength);
                                        ErasoftDbContext.SaveChanges();
                                    }
                                    break;
                                case 3:
                                    {
                                        if (!same_uploaded.ContainsKey(file.ContentLength))
                                        {
                                            ImgurImageResponse image = UploadImageService.UploadSingleImageToImgur(file, "uploaded-image");
                                            itemVar.AVALUE_48 = image.data.link_l;
                                            same_uploaded.Add(file.ContentLength, image.data.link_l);
                                        }
                                        else
                                        {
                                            itemVar.AVALUE_48 = same_uploaded.Where(p => p.Key == file.ContentLength).FirstOrDefault().Value;
                                        }

                                        itemVar.ACODE_48 = Convert.ToString(file.ContentLength);
                                        ErasoftDbContext.SaveChanges();
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            return Json($"Update Gambar Variant Berhasil.", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateGambarVariantBlibli()
        {
            Dictionary<int, string> same_uploaded = new Dictionary<int, string>();
            foreach (var item in Request.Files.AllKeys)
            {
                int stf02h_recnum = Convert.ToInt32(item);
                var itemVar = ErasoftDbContext.STF02H.Where(p => p.RecNum == stf02h_recnum).SingleOrDefault();
                if (itemVar != null)
                {
                    var file = Request.Files[item];

                    if (file != null && file.ContentLength > 0)
                    {
                        if (!same_uploaded.ContainsKey(file.ContentLength))
                        {
                            ImgurImageResponse image = UploadImageService.UploadSingleImageToImgur(file, "uploaded-image");
                            itemVar.AVALUE_50 = image.data.link_l;
                            same_uploaded.Add(file.ContentLength, image.data.link_l);
                        }
                        else
                        {
                            itemVar.AVALUE_50 = same_uploaded.Where(p => p.Key == file.ContentLength).FirstOrDefault().Value;
                        }

                        itemVar.ACODE_50 = Convert.ToString(file.ContentLength);
                    }
                    ErasoftDbContext.SaveChanges();
                }
            }
            return Json($"Update Gambar Variant Berhasil.", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AutoloadVariantBarang(string brg, string code, string[] opt_selected_1, string[] opt_selected_2, string[] opt_selected_3)
        {
            var kategori = ErasoftDbContext.STF02E.Single(k => k.KODE == code);
            var stf20 = ErasoftDbContext.STF20.Where(m => m.CATEGORY_MO == kategori.KODE).ToList();

            //Autoload (Overwrite) STF02
            List<STF02> ListNewVariantData_Stf02 = new List<STF02>();
            List<STF02H> ListNewVariantData_Stf02H = new List<STF02H>();
            var STF02_Induk = ErasoftDbContext.STF02.Where(p => p.BRG == brg).SingleOrDefault();
            var List_STF02H_Induk = ErasoftDbContext.STF02H.Where(p => p.BRG == brg).ToList();

            //cek stf02h yg sudah berhasil link ke marketplace
            var listStf02inDbCek = ErasoftDbContext.STF02.Where(p => (p.PART == null ? "" : p.PART) == brg).ToList();
            var listBrgStf02inDbCek = listStf02inDbCek.Select(p => p.BRG).ToList();
            var listStf02HinDbCekDuplikat = ErasoftDbContext.STF02H.Where(p => listBrgStf02inDbCek.Contains(p.BRG) && ((p.BRG_MP == null ? "" : p.BRG_MP) != "")).ToList();
            //var listBrgStf02hinDbCekDuplikat = listStf02HinDbCekDuplikat.Select(p => p.BRG).ToList();
            //cek stf02h yg sudah berhasil link ke marketplace

            var stf20b = ErasoftDbContext.STF20B.Where(m => m.CATEGORY_MO == kategori.KODE).ToList();
            if (STF02_Induk != null)
            {
                if (opt_selected_1 != null)
                {
                    foreach (var item in opt_selected_1.Where(p => p.Trim() != "").ToList())
                    {
                        if (opt_selected_2 != null) //jika tidak ada varian level 2 di STF20B, maka akan menjadi null
                        {
                            if (opt_selected_2.Where(p => p.Trim() != "").ToList().Count() > 0) // jika ada varian lv 2, tapi tidak dipakai, maka akan ada isi count 1 dengan nilai blank
                            {
                                foreach (var item2 in opt_selected_2.Where(p => p.Trim() != "").ToList())
                                {
                                    if (opt_selected_3 != null) //jika tidak ada varian level 3 di STF20B, maka akan menjadi null
                                    {
                                        if (opt_selected_3.Where(p => p.Trim() != "").ToList().Count() > 0) // jika ada varian lv 3, tapi tidak dipakai, maka akan ada isi count 1 dengan nilai blank
                                        {
                                            foreach (var item3 in opt_selected_3.Where(p => p.Trim() != "").ToList())
                                            {
                                                if (!listBrgStf02inDbCek.Contains(STF02_Induk.BRG + "." + item + "." + item2 + "." + item3))
                                                {
                                                    STF02 newVariantData = new STF02();
                                                    newVariantData = CopyStf02(STF02_Induk);
                                                    newVariantData.BRG = newVariantData.BRG + "." + item + "." + item2 + "." + item3;
                                                    string ket_varlv1 = stf20b.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == item).FirstOrDefault()?.KET_VAR;
                                                    string ket_varlv2 = stf20b.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == item2).FirstOrDefault()?.KET_VAR;
                                                    string ket_varlv3 = stf20b.Where(p => p.LEVEL_VAR == 3 && p.KODE_VAR == item3).FirstOrDefault()?.KET_VAR;
                                                    newVariantData.NAMA2 += " " + ket_varlv1 + " " + ket_varlv2 + " " + ket_varlv3;
                                                    newVariantData.Sort8 = item;
                                                    newVariantData.Sort9 = item2;
                                                    newVariantData.Sort10 = item3;
                                                    newVariantData.Ket_Sort8 = ket_varlv1;
                                                    newVariantData.Ket_Sort9 = ket_varlv2;
                                                    newVariantData.Ket_Sort10 = ket_varlv3;
                                                    newVariantData.PART = STF02_Induk.BRG;
                                                    newVariantData.TYPE = "3";
                                                    ListNewVariantData_Stf02.Add(newVariantData);
                                                }
                                                else
                                                {
                                                    var UpdateStf02Sorts = ErasoftDbContext.STF02.Where(p => (p.PART == null ? "" : p.PART) == brg && p.BRG == (STF02_Induk.BRG + "." + item + "." + item2 + "." + item3)).SingleOrDefault();
                                                    if (UpdateStf02Sorts != null)
                                                    {
                                                        string ket_varlv1 = stf20b.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == item).FirstOrDefault()?.KET_VAR;
                                                        string ket_varlv2 = stf20b.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == item2).FirstOrDefault()?.KET_VAR;
                                                        string ket_varlv3 = stf20b.Where(p => p.LEVEL_VAR == 3 && p.KODE_VAR == item3).FirstOrDefault()?.KET_VAR;
                                                        UpdateStf02Sorts.Sort8 = item;
                                                        UpdateStf02Sorts.Sort9 = item2;
                                                        UpdateStf02Sorts.Sort10 = item3;
                                                        UpdateStf02Sorts.Ket_Sort8 = ket_varlv1;
                                                        UpdateStf02Sorts.Ket_Sort9 = ket_varlv2;
                                                        UpdateStf02Sorts.Ket_Sort10 = ket_varlv3;
                                                        ErasoftDbContext.SaveChanges();
                                                    }
                                                }

                                                foreach (var stf02h_induk in List_STF02H_Induk)
                                                {
                                                    var cekAdaSTF02HVariasi = listStf02HinDbCekDuplikat.Where(p => p.BRG == STF02_Induk.BRG + "." + item + "." + item2 + "." + item3 && p.IDMARKET == stf02h_induk.IDMARKET).FirstOrDefault();
                                                    if (cekAdaSTF02HVariasi == null)
                                                    {
                                                        STF02H newVariantDataStf02H = new STF02H();
                                                        newVariantDataStf02H = CopyStf02h(stf02h_induk);
                                                        newVariantDataStf02H.BRG = newVariantDataStf02H.BRG + "." + item + "." + item2 + "." + item3;
                                                        ListNewVariantData_Stf02H.Add(newVariantDataStf02H);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (!listBrgStf02inDbCek.Contains(STF02_Induk.BRG + "." + item + "." + item2))
                                            {
                                                STF02 newVariantData = new STF02();
                                                newVariantData = CopyStf02(STF02_Induk);
                                                newVariantData.BRG = newVariantData.BRG + "." + item + "." + item2;
                                                string ket_varlv1 = stf20b.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == item).FirstOrDefault()?.KET_VAR;
                                                string ket_varlv2 = stf20b.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == item2).FirstOrDefault()?.KET_VAR;
                                                newVariantData.NAMA2 += " " + ket_varlv1 + " " + ket_varlv2;
                                                newVariantData.Sort8 = item;
                                                newVariantData.Sort9 = item2;
                                                newVariantData.Ket_Sort8 = ket_varlv1;
                                                newVariantData.Ket_Sort9 = ket_varlv2;
                                                newVariantData.PART = STF02_Induk.BRG;
                                                newVariantData.TYPE = "3";
                                                ListNewVariantData_Stf02.Add(newVariantData);
                                            }
                                            else
                                            {
                                                var UpdateStf02Sorts = ErasoftDbContext.STF02.Where(p => (p.PART == null ? "" : p.PART) == brg && p.BRG == (STF02_Induk.BRG + "." + item + "." + item2)).SingleOrDefault();
                                                if (UpdateStf02Sorts != null)
                                                {
                                                    string ket_varlv1 = stf20b.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == item).FirstOrDefault()?.KET_VAR;
                                                    string ket_varlv2 = stf20b.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == item2).FirstOrDefault()?.KET_VAR;
                                                    UpdateStf02Sorts.Sort8 = item;
                                                    UpdateStf02Sorts.Sort9 = item2;
                                                    UpdateStf02Sorts.Ket_Sort8 = ket_varlv1;
                                                    UpdateStf02Sorts.Ket_Sort9 = ket_varlv2;
                                                    ErasoftDbContext.SaveChanges();
                                                }
                                            }

                                            foreach (var stf02h_induk in List_STF02H_Induk)
                                            {
                                                var cekAdaSTF02HVariasi = listStf02HinDbCekDuplikat.Where(p => p.BRG == STF02_Induk.BRG + "." + item + "." + item2 && p.IDMARKET == stf02h_induk.IDMARKET).FirstOrDefault();
                                                if (cekAdaSTF02HVariasi == null)
                                                {
                                                    STF02H newVariantDataStf02H = new STF02H();
                                                    newVariantDataStf02H = CopyStf02h(stf02h_induk);
                                                    newVariantDataStf02H.BRG = newVariantDataStf02H.BRG + "." + item + "." + item2;
                                                    ListNewVariantData_Stf02H.Add(newVariantDataStf02H);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!listBrgStf02inDbCek.Contains(STF02_Induk.BRG + "." + item + "." + item2))
                                        {
                                            STF02 newVariantData = new STF02();
                                            newVariantData = CopyStf02(STF02_Induk);
                                            newVariantData.BRG = newVariantData.BRG + "." + item + "." + item2;
                                            string ket_varlv1 = stf20b.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == item).FirstOrDefault()?.KET_VAR;
                                            string ket_varlv2 = stf20b.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == item2).FirstOrDefault()?.KET_VAR;
                                            newVariantData.NAMA2 += " " + ket_varlv1 + " " + ket_varlv2;
                                            newVariantData.Sort8 = item;
                                            newVariantData.Sort9 = item2;
                                            newVariantData.Ket_Sort8 = ket_varlv1;
                                            newVariantData.Ket_Sort9 = ket_varlv2;
                                            newVariantData.PART = STF02_Induk.BRG;
                                            newVariantData.TYPE = "3";
                                            ListNewVariantData_Stf02.Add(newVariantData);
                                        }
                                        else
                                        {
                                            var UpdateStf02Sorts = ErasoftDbContext.STF02.Where(p => (p.PART == null ? "" : p.PART) == brg && p.BRG == (STF02_Induk.BRG + "." + item + "." + item2)).SingleOrDefault();
                                            if (UpdateStf02Sorts != null)
                                            {
                                                string ket_varlv1 = stf20b.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == item).FirstOrDefault()?.KET_VAR;
                                                string ket_varlv2 = stf20b.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == item2).FirstOrDefault()?.KET_VAR;
                                                UpdateStf02Sorts.Sort8 = item;
                                                UpdateStf02Sorts.Sort9 = item2;
                                                UpdateStf02Sorts.Ket_Sort8 = ket_varlv1;
                                                UpdateStf02Sorts.Ket_Sort9 = ket_varlv2;
                                                ErasoftDbContext.SaveChanges();
                                            }
                                        }

                                        foreach (var stf02h_induk in List_STF02H_Induk)
                                        {
                                            var cekAdaSTF02HVariasi = listStf02HinDbCekDuplikat.Where(p => p.BRG == STF02_Induk.BRG + "." + item + "." + item2 && p.IDMARKET == stf02h_induk.IDMARKET).FirstOrDefault();
                                            if (cekAdaSTF02HVariasi == null)
                                            {
                                                STF02H newVariantDataStf02H = new STF02H();
                                                newVariantDataStf02H = CopyStf02h(stf02h_induk);
                                                newVariantDataStf02H.BRG = newVariantDataStf02H.BRG + "." + item + "." + item2;
                                                ListNewVariantData_Stf02H.Add(newVariantDataStf02H);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (!listBrgStf02inDbCek.Contains(STF02_Induk.BRG + "." + item))
                                {
                                    STF02 newVariantData = new STF02();
                                    newVariantData = CopyStf02(STF02_Induk);
                                    newVariantData.BRG = newVariantData.BRG + "." + item;
                                    string ket_varlv1 = stf20b.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == item).FirstOrDefault()?.KET_VAR;
                                    newVariantData.NAMA2 += " " + ket_varlv1;
                                    newVariantData.Sort8 = item;
                                    newVariantData.Ket_Sort8 = ket_varlv1;
                                    newVariantData.PART = STF02_Induk.BRG;
                                    newVariantData.TYPE = "3";
                                    ListNewVariantData_Stf02.Add(newVariantData);

                                }
                                else
                                {
                                    var UpdateStf02Sorts = ErasoftDbContext.STF02.Where(p => (p.PART == null ? "" : p.PART) == brg && p.BRG == (STF02_Induk.BRG + "." + item)).SingleOrDefault();
                                    if (UpdateStf02Sorts != null)
                                    {
                                        string ket_varlv1 = stf20b.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == item).FirstOrDefault()?.KET_VAR;
                                        UpdateStf02Sorts.Sort8 = item;
                                        UpdateStf02Sorts.Ket_Sort8 = ket_varlv1;
                                        ErasoftDbContext.SaveChanges();
                                    }
                                }

                                foreach (var stf02h_induk in List_STF02H_Induk)
                                {
                                    var cekAdaSTF02HVariasi = listStf02HinDbCekDuplikat.Where(p => p.BRG == STF02_Induk.BRG + "." + item && p.IDMARKET == stf02h_induk.IDMARKET).FirstOrDefault();
                                    if (cekAdaSTF02HVariasi == null)
                                    {
                                        STF02H newVariantDataStf02H = new STF02H();
                                        newVariantDataStf02H = CopyStf02h(stf02h_induk);
                                        newVariantDataStf02H.BRG = newVariantDataStf02H.BRG + "." + item;
                                        ListNewVariantData_Stf02H.Add(newVariantDataStf02H);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!listBrgStf02inDbCek.Contains(STF02_Induk.BRG + "." + item))
                            {
                                STF02 newVariantData = new STF02();
                                newVariantData = CopyStf02(STF02_Induk);
                                newVariantData.BRG = newVariantData.BRG + "." + item;
                                string ket_varlv1 = stf20b.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == item).FirstOrDefault()?.KET_VAR;
                                newVariantData.NAMA2 += " " + ket_varlv1;
                                newVariantData.Sort8 = item;
                                newVariantData.Ket_Sort8 = ket_varlv1;
                                newVariantData.PART = STF02_Induk.BRG;
                                newVariantData.TYPE = "3";
                                ListNewVariantData_Stf02.Add(newVariantData);

                            }
                            else
                            {
                                var UpdateStf02Sorts = ErasoftDbContext.STF02.Where(p => (p.PART == null ? "" : p.PART) == brg && p.BRG == (STF02_Induk.BRG + "." + item)).SingleOrDefault();
                                if (UpdateStf02Sorts != null)
                                {
                                    string ket_varlv1 = stf20b.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == item).FirstOrDefault()?.KET_VAR;
                                    UpdateStf02Sorts.Sort8 = item;
                                    UpdateStf02Sorts.Ket_Sort8 = ket_varlv1;
                                    ErasoftDbContext.SaveChanges();
                                }
                            }

                            foreach (var stf02h_induk in List_STF02H_Induk)
                            {
                                var cekAdaSTF02HVariasi = listStf02HinDbCekDuplikat.Where(p => p.BRG == STF02_Induk.BRG + "." + item && p.IDMARKET == stf02h_induk.IDMARKET).FirstOrDefault();
                                if (cekAdaSTF02HVariasi == null)
                                {
                                    STF02H newVariantDataStf02H = new STF02H();
                                    newVariantDataStf02H = CopyStf02h(stf02h_induk);
                                    newVariantDataStf02H.BRG = newVariantDataStf02H.BRG + "." + item;
                                    ListNewVariantData_Stf02H.Add(newVariantDataStf02H);
                                }
                            }
                        }
                    }
                }
            }
            #region Save STF02 Variant
            if (ListNewVariantData_Stf02.Count() > 0)
            {
                var listStf02inDb = ErasoftDbContext.STF02.Where(p => (p.PART == null ? "" : p.PART) == brg).ToList();
                if (listStf02inDb.Count() > 0)
                {
                    var listBrgStf02inDb = listStf02inDb.Select(p => p.BRG).ToList();
                    var listStf02HinDb = ErasoftDbContext.STF02H.Where(p => listBrgStf02inDb.Contains(p.BRG) && ((p.BRG_MP == null ? "" : p.BRG_MP) == "")).ToList();
                    var listStf02HinDbDelete = listStf02HinDb.Select(p => p.BRG).ToList();
                    if (listStf02HinDb.Count() > 0)
                    {
                        ErasoftDbContext.STF02H.RemoveRange(listStf02HinDb);
                    }
                    var listStf02inDbDelete = listStf02inDb.Where(p => listStf02HinDbDelete.Contains(p.BRG)).ToList();
                    ErasoftDbContext.STF02.RemoveRange(listStf02inDbDelete);
                    ErasoftDbContext.SaveChanges();
                }

                ErasoftDbContext.STF02.AddRange(ListNewVariantData_Stf02);
                ErasoftDbContext.SaveChanges();
            }
            #endregion
            #region Save STF02H Variant
            if (ListNewVariantData_Stf02H.Count() > 0)
            {
                var listStf02inDb = ErasoftDbContext.STF02.Where(p => (p.PART == null ? "" : p.PART) == brg).ToList();
                if (listStf02inDb.Count() > 0)
                {
                    var listBrgStf02inDb = listStf02inDb.Select(p => p.BRG).ToList();
                    var listStf02HinDb = ErasoftDbContext.STF02H.Where(p => listBrgStf02inDb.Contains(p.BRG) && ((p.BRG_MP == null ? "" : p.BRG_MP) == "")).ToList();
                    if (listStf02HinDb.Count() > 0)
                    {
                        ErasoftDbContext.STF02H.RemoveRange(listStf02HinDb);
                    }
                    ErasoftDbContext.SaveChanges();
                }

                ErasoftDbContext.STF02H.AddRange(ListNewVariantData_Stf02H);
                ErasoftDbContext.SaveChanges();
            }
            #endregion
            //end Autoload (Overwrite) STF02

            var VariantMO = ErasoftDbContext.STF02.Where(p => (p.PART == null ? "" : p.PART) == brg).ToList();
            var listBrgVariantMO = VariantMO.Select(p => p.BRG).ToList();
            var VariantMO_H = ErasoftDbContext.STF02H.Where(p => listBrgVariantMO.Contains(p.BRG)).ToList();

            var vm = new BarangDetailVarViewModel()
            {
                VariantMO = VariantMO,
                VariantMO_H = VariantMO_H,
                ListMarket = ErasoftDbContext.ARF01.OrderBy(p => p.RecNum).ToList()
            };

            return PartialView("BarangDetailVarPartial", vm);
        }
        public ActionResult SaveMappingVarShopee(string brg, string code, string[] opt_selected_1, string[] opt_selected_2, string[] opt_selected_3, StrukturVariantMp shopee)
        {
            var kategori = ErasoftDbContext.STF02E.Single(k => k.KODE == code);
            var stf20 = ErasoftDbContext.STF20.Where(m => m.CATEGORY_MO == kategori.KODE).ToList();
            List<STF02I> listNewData = new List<STF02I>();
            #region Create Ulang STF02I
            {
                if (opt_selected_1 != null)
                {
                    var i = 0;
                    foreach (var item in opt_selected_1)
                    {
                        if (item != "")
                        {
                            try
                            {
                                STF02I newdata = new STF02I()
                                {
                                    MARKET = "SHOPEE",
                                    BRG = brg,
                                    CATEGORY_MO = code,
                                    KODE_VAR = item,
                                    LEVEL_VAR = 1,
                                    MP_JUDUL_VAR = shopee.var_judul.lv_1,
                                    MP_VALUE_VAR = shopee.var_detail.lv_1[i],
                                    MP_CATEGORY_CODE = shopee.code
                                };
                                listNewData.Add(newdata);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        i++;
                    }
                }
                if (opt_selected_2 != null)
                {
                    var i = 0;
                    foreach (var item in opt_selected_2)
                    {
                        if (item != "")
                        {
                            try
                            {
                                STF02I newdata = new STF02I()
                                {
                                    MARKET = "SHOPEE",
                                    BRG = brg,
                                    CATEGORY_MO = code,
                                    KODE_VAR = item,
                                    LEVEL_VAR = 2,
                                    MP_JUDUL_VAR = shopee.var_judul.lv_2,
                                    MP_VALUE_VAR = shopee.var_detail.lv_2[i],
                                    MP_CATEGORY_CODE = shopee.code
                                };
                                listNewData.Add(newdata);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        i++;
                    }
                }
                if (opt_selected_3 != null)
                {
                    var i = 0;
                    foreach (var item in opt_selected_3)
                    {
                        if (item != "")
                        {
                            try
                            {
                                STF02I newdata = new STF02I()
                                {
                                    MARKET = "SHOPEE",
                                    BRG = brg,
                                    CATEGORY_MO = code,
                                    KODE_VAR = item,
                                    LEVEL_VAR = 3,
                                    MP_JUDUL_VAR = shopee.var_judul.lv_3,
                                    MP_VALUE_VAR = shopee.var_detail.lv_3[i],
                                    MP_CATEGORY_CODE = shopee.code
                                };
                                listNewData.Add(newdata);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        i++;
                    }
                }
            }
            #endregion

            #region Save STF02I
            if (listNewData.Count() > 0)
            {
                var listStf02IinDb = ErasoftDbContext.STF02I.Where(p => p.BRG == brg && p.MARKET == "SHOPEE").ToList();
                ErasoftDbContext.STF02I.RemoveRange(listStf02IinDb);
                ErasoftDbContext.SaveChanges();

                ErasoftDbContext.STF02I.AddRange(listNewData);
                ErasoftDbContext.SaveChanges();
            }
            #endregion
            var vm = new BarangDetailVarViewModel()
            {

            };

            return Json(vm, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveMappingVarTokped(string brg, string code, string[] opt_selected_1, string[] opt_selected_2, string[] opt_selected_3, StrukturVariantMp tokped)
        {

            var kategori = ErasoftDbContext.STF02E.Single(k => k.KODE == code);
            var stf20 = ErasoftDbContext.STF20.Where(m => m.CATEGORY_MO == kategori.KODE).ToList();
            List<STF02I> listNewData = new List<STF02I>();
            #region Create Ulang STF02I
            {
                if (opt_selected_1 != null)
                {
                    var i = 0;
                    foreach (var item in opt_selected_1)
                    {
                        if (item != "")
                        {
                            try
                            {
                                STF02I newdataTokped = new STF02I()
                                {
                                    MARKET = "TOKPED",
                                    BRG = brg,
                                    CATEGORY_MO = code,
                                    KODE_VAR = item,
                                    LEVEL_VAR = 1,
                                    MP_JUDUL_VAR = tokped.var_judul.lv_1,
                                    MP_VALUE_VAR = tokped.var_detail.lv_1[i],
                                    MP_CATEGORY_CODE = tokped.code
                                };
                                listNewData.Add(newdataTokped);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        i++;
                    }
                }
                if (opt_selected_2 != null)
                {
                    var i = 0;
                    foreach (var item in opt_selected_2)
                    {
                        if (item != "")
                        {
                            try
                            {
                                STF02I newdataTokped = new STF02I()
                                {
                                    MARKET = "TOKPED",
                                    BRG = brg,
                                    CATEGORY_MO = code,
                                    KODE_VAR = item,
                                    LEVEL_VAR = 2,
                                    MP_JUDUL_VAR = tokped.var_judul.lv_2,
                                    MP_VALUE_VAR = tokped.var_detail.lv_2[i],
                                    MP_CATEGORY_CODE = tokped.code
                                };
                                listNewData.Add(newdataTokped);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        i++;
                    }
                }
                if (opt_selected_3 != null)
                {
                    var i = 0;
                    foreach (var item in opt_selected_3)
                    {
                        if (item != "")
                        {
                            try
                            {
                                STF02I newdataTokped = new STF02I()
                                {
                                    MARKET = "TOKPED",
                                    BRG = brg,
                                    CATEGORY_MO = code,
                                    KODE_VAR = item,
                                    LEVEL_VAR = 3,
                                    MP_JUDUL_VAR = tokped.var_judul.lv_3,
                                    MP_VALUE_VAR = tokped.var_detail.lv_3[i],
                                    MP_CATEGORY_CODE = tokped.code
                                };
                                listNewData.Add(newdataTokped);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        i++;
                    }
                }
            }
            #endregion

            #region Save STF02I
            if (listNewData.Count() > 0)
            {
                var listStf02IinDb = ErasoftDbContext.STF02I.Where(p => p.BRG == brg && p.MARKET == "TOKPED").ToList();
                ErasoftDbContext.STF02I.RemoveRange(listStf02IinDb);
                ErasoftDbContext.SaveChanges();

                ErasoftDbContext.STF02I.AddRange(listNewData);
                ErasoftDbContext.SaveChanges();
            }
            #endregion

            var vm = new BarangDetailVarViewModel()
            {

            };

            return Json(vm, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveMappingVarBlibli(string brg, string code, string[] opt_selected_1, string[] opt_selected_2, string[] opt_selected_3, StrukturVariantMp blibli)
        {

            var kategori = ErasoftDbContext.STF02E.Single(k => k.KODE == code);
            var stf20 = ErasoftDbContext.STF20.Where(m => m.CATEGORY_MO == kategori.KODE).ToList();
            List<STF02I> listNewData = new List<STF02I>();
            #region Create Ulang STF02I
            {
                if (opt_selected_1 != null)
                {
                    var i = 0;
                    foreach (var item in opt_selected_1)
                    {
                        if (item != "")
                        {
                            try
                            {
                                STF02I newdataBlibli = new STF02I()
                                {
                                    MARKET = "BLIBLI",
                                    BRG = brg,
                                    CATEGORY_MO = code,
                                    KODE_VAR = item,
                                    LEVEL_VAR = 1,
                                    MP_JUDUL_VAR = blibli.var_judul.lv_1,
                                    MP_VALUE_VAR = blibli.var_detail.lv_1[i],
                                    MP_CATEGORY_CODE = blibli.code
                                };
                                listNewData.Add(newdataBlibli);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        i++;
                    }
                }
                if (opt_selected_2 != null)
                {
                    var i = 0;
                    foreach (var item in opt_selected_2)
                    {
                        if (item != "")
                        {
                            try
                            {
                                STF02I newdataBlibli = new STF02I()
                                {
                                    MARKET = "BLIBLI",
                                    BRG = brg,
                                    CATEGORY_MO = code,
                                    KODE_VAR = item,
                                    LEVEL_VAR = 2,
                                    MP_JUDUL_VAR = blibli.var_judul.lv_2,
                                    MP_VALUE_VAR = blibli.var_detail.lv_2[i],
                                    MP_CATEGORY_CODE = blibli.code
                                };
                                listNewData.Add(newdataBlibli);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        i++;
                    }
                }
                if (opt_selected_3 != null)
                {
                    var i = 0;
                    foreach (var item in opt_selected_3)
                    {
                        if (item != "")
                        {
                            try
                            {
                                STF02I newdataBlibli = new STF02I()
                                {
                                    MARKET = "BLIBLI",
                                    BRG = brg,
                                    CATEGORY_MO = code,
                                    KODE_VAR = item,
                                    LEVEL_VAR = 3,
                                    MP_JUDUL_VAR = blibli.var_judul.lv_3,
                                    MP_VALUE_VAR = blibli.var_detail.lv_3[i],
                                    MP_CATEGORY_CODE = blibli.code
                                };
                                listNewData.Add(newdataBlibli);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        i++;
                    }
                }
            }
            #endregion

            #region Save STF02I
            if (listNewData.Count() > 0)
            {
                var listStf02IinDb = ErasoftDbContext.STF02I.Where(p => p.BRG == brg && p.MARKET == "BLIBLI").ToList();
                ErasoftDbContext.STF02I.RemoveRange(listStf02IinDb);
                ErasoftDbContext.SaveChanges();

                ErasoftDbContext.STF02I.AddRange(listNewData);
                ErasoftDbContext.SaveChanges();
            }
            #endregion

            var vm = new BarangDetailVarViewModel()
            {

            };

            return Json(vm, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveMappingVarLazada(string brg, string code, string[] opt_selected_1, string[] opt_selected_2, string[] opt_selected_3, StrukturVariantMp lazada)
        {

            var kategori = ErasoftDbContext.STF02E.Single(k => k.KODE == code);
            var stf20 = ErasoftDbContext.STF20.Where(m => m.CATEGORY_MO == kategori.KODE).ToList();
            List<STF02I> listNewData = new List<STF02I>();
            #region Create Ulang STF02I
            {
                if (opt_selected_1 != null)
                {
                    var i = 0;
                    foreach (var item in opt_selected_1)
                    {
                        if (item != "")
                        {
                            try
                            {
                                STF02I newdataLazada = new STF02I()
                                {
                                    MARKET = "LAZADA",
                                    BRG = brg,
                                    CATEGORY_MO = code,
                                    KODE_VAR = item,
                                    LEVEL_VAR = 1,
                                    MP_JUDUL_VAR = lazada.var_judul.lv_1,
                                    MP_VALUE_VAR = lazada.var_detail.lv_1[i],
                                    MP_CATEGORY_CODE = lazada.code
                                };
                                listNewData.Add(newdataLazada);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        i++;
                    }
                }
                if (opt_selected_2 != null)
                {
                    var i = 0;
                    foreach (var item in opt_selected_2)
                    {
                        if (item != "")
                        {
                            try
                            {
                                STF02I newdataLazada = new STF02I()
                                {
                                    MARKET = "LAZADA",
                                    BRG = brg,
                                    CATEGORY_MO = code,
                                    KODE_VAR = item,
                                    LEVEL_VAR = 2,
                                    MP_JUDUL_VAR = lazada.var_judul.lv_2,
                                    MP_VALUE_VAR = lazada.var_detail.lv_2[i],
                                    MP_CATEGORY_CODE = lazada.code
                                };
                                listNewData.Add(newdataLazada);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        i++;
                    }
                }
                if (opt_selected_3 != null)
                {
                    var i = 0;
                    foreach (var item in opt_selected_3)
                    {
                        if (item != "")
                        {
                            try
                            {
                                STF02I newdataLazada = new STF02I()
                                {
                                    MARKET = "LAZADA",
                                    BRG = brg,
                                    CATEGORY_MO = code,
                                    KODE_VAR = item,
                                    LEVEL_VAR = 3,
                                    MP_JUDUL_VAR = lazada.var_judul.lv_3,
                                    MP_VALUE_VAR = lazada.var_detail.lv_3[i],
                                    MP_CATEGORY_CODE = lazada.code
                                };
                                listNewData.Add(newdataLazada);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        i++;
                    }
                }
            }
            #endregion

            #region Save STF02I
            if (listNewData.Count() > 0)
            {
                var listStf02IinDb = ErasoftDbContext.STF02I.Where(p => p.BRG == brg && p.MARKET == "LAZADA").ToList();
                ErasoftDbContext.STF02I.RemoveRange(listStf02IinDb);
                ErasoftDbContext.SaveChanges();

                ErasoftDbContext.STF02I.AddRange(listNewData);
                ErasoftDbContext.SaveChanges();
            }
            #endregion

            var vm = new BarangDetailVarViewModel()
            {

            };

            return Json(vm, JsonRequestBehavior.AllowGet);
        }
        public ActionResult EditStrukturVar(int? recNum)
        {
            var kategori = ErasoftDbContext.STF02E.Single(k => k.RecNum == recNum);
            var stf20 = ErasoftDbContext.STF20.Where(m => m.CATEGORY_MO == kategori.KODE).ToList();
            var vm = new MasterStrukturVarViewModel()
            {
                Kategori = kategori,
                Variant_Level_1 = new STF20()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_JUDUL_VAR = 1,
                    VALUE_JUDUL_VAR = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(1)).FirstOrDefault()?.VALUE_JUDUL_VAR
                },
                VariantOpt_Level_1 = new STF20B()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_VAR = 1
                },
                Variant_Level_2 = new STF20()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_JUDUL_VAR = 2,
                    VALUE_JUDUL_VAR = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(2)).FirstOrDefault()?.VALUE_JUDUL_VAR
                },
                VariantOpt_Level_2 = new STF20B()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_VAR = 2
                },
                Variant_Level_3 = new STF20()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_JUDUL_VAR = 3,
                    VALUE_JUDUL_VAR = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(3)).FirstOrDefault()?.VALUE_JUDUL_VAR
                },
                VariantOpt_Level_3 = new STF20B()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_VAR = 3
                },
                VariantOptInDb = ErasoftDbContext.STF20B.Where(m => m.CATEGORY_MO == kategori.KODE).OrderBy(m => m.LEVEL_VAR).ToList()
            };

            return PartialView("StrukturVarPartial", vm);
        }

        public ActionResult UpdateStrukturVar(int? recNum, string JUDUL_VAR_1, string JUDUL_VAR_2, string JUDUL_VAR_3)
        {
            var kategori = ErasoftDbContext.STF02E.SingleOrDefault(k => k.RecNum == recNum);
            if (kategori != null)
            {
                List<STF20> batchNewStf20 = new List<STF20>();
                var updateStf20_1 = ErasoftDbContext.STF20.Where(m => m.CATEGORY_MO == kategori.KODE && m.LEVEL_JUDUL_VAR == 1).SingleOrDefault();
                if (updateStf20_1 != null)
                {
                    updateStf20_1.VALUE_JUDUL_VAR = JUDUL_VAR_1;
                }
                else
                {
                    STF20 newStf20 = new STF20()
                    {
                        CATEGORY_MO = kategori.KODE,
                        LEVEL_JUDUL_VAR = 1,
                        VALUE_JUDUL_VAR = JUDUL_VAR_1
                    };
                    batchNewStf20.Add(newStf20);
                }
                var updateStf20_2 = ErasoftDbContext.STF20.Where(m => m.CATEGORY_MO == kategori.KODE && m.LEVEL_JUDUL_VAR == 2).SingleOrDefault();
                if (updateStf20_2 != null)
                {
                    updateStf20_2.VALUE_JUDUL_VAR = JUDUL_VAR_2;
                }
                else
                {
                    STF20 newStf20 = new STF20()
                    {
                        CATEGORY_MO = kategori.KODE,
                        LEVEL_JUDUL_VAR = 2,
                        VALUE_JUDUL_VAR = JUDUL_VAR_2
                    };
                    batchNewStf20.Add(newStf20);
                }
                var updateStf20_3 = ErasoftDbContext.STF20.Where(m => m.CATEGORY_MO == kategori.KODE && m.LEVEL_JUDUL_VAR == 3).SingleOrDefault();
                if (updateStf20_3 != null)
                {
                    updateStf20_3.VALUE_JUDUL_VAR = JUDUL_VAR_3;
                }
                else
                {
                    STF20 newStf20 = new STF20()
                    {
                        CATEGORY_MO = kategori.KODE,
                        LEVEL_JUDUL_VAR = 3,
                        VALUE_JUDUL_VAR = JUDUL_VAR_3
                    };
                    batchNewStf20.Add(newStf20);
                }
                if (batchNewStf20.Count() > 0)
                {
                    ErasoftDbContext.STF20.AddRange(batchNewStf20);
                }
                ErasoftDbContext.SaveChanges();
            }
            var stf20 = ErasoftDbContext.STF20.Where(m => m.CATEGORY_MO == kategori.KODE).ToList();
            var vm = new MasterStrukturVarViewModel()
            {
                Kategori = kategori,
                Variant_Level_1 = new STF20()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_JUDUL_VAR = 1,
                    VALUE_JUDUL_VAR = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(1)).FirstOrDefault()?.VALUE_JUDUL_VAR
                },
                VariantOpt_Level_1 = new STF20B()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_VAR = 1
                },
                Variant_Level_2 = new STF20()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_JUDUL_VAR = 2,
                    VALUE_JUDUL_VAR = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(2)).FirstOrDefault()?.VALUE_JUDUL_VAR
                },
                VariantOpt_Level_2 = new STF20B()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_VAR = 2
                },
                Variant_Level_3 = new STF20()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_JUDUL_VAR = 3,
                    VALUE_JUDUL_VAR = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(3)).FirstOrDefault()?.VALUE_JUDUL_VAR
                },
                VariantOpt_Level_3 = new STF20B()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_VAR = 3
                },
                VariantOptInDb = ErasoftDbContext.STF20B.Where(m => m.CATEGORY_MO == kategori.KODE).OrderBy(m => m.LEVEL_VAR).ToList()
            };

            return PartialView("StrukturVarPartial", vm);
        }

        public ActionResult SaveVariantOptLevel(string JUDUL_VAR, STF20B data)
        {
            //add by nurul 18/2/2019
            var stf20b = ErasoftDbContext.STF20B.Where(m => m.CATEGORY_MO == data.CATEGORY_MO && m.LEVEL_VAR == data.LEVEL_VAR && m.KODE_VAR == data.KODE_VAR).FirstOrDefault();
            var vmError = new MasterStrukturVarViewModel() { };
            if (data.KODE_VAR == null || data.KODE_VAR == "" || data.KET_VAR == null || data.KET_VAR == "")
            {
                vmError.Errors.Add("Mohon lengkapi Opsi Variasi " + data.LEVEL_VAR + " !");
                return Json(vmError, JsonRequestBehavior.AllowGet);
            }
            if (stf20b != null)
            {
                if (stf20b.KODE_VAR.ToUpper() == data.KODE_VAR.ToUpper())
                {
                    vmError.Errors.Add("Kode Opsi Variasi " + data.LEVEL_VAR + " '" + data.KODE_VAR.ToUpper() + "' sudah ada !");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
            }
            //end add by nurul 18/2/2019
            var updateStf20 = ErasoftDbContext.STF20.Where(m => m.CATEGORY_MO == data.CATEGORY_MO && m.LEVEL_JUDUL_VAR == data.LEVEL_VAR).SingleOrDefault();
            if (updateStf20 != null)
            {
                updateStf20.VALUE_JUDUL_VAR = JUDUL_VAR;
            }
            else
            {
                STF20 newStf20 = new STF20()
                {
                    CATEGORY_MO = data.CATEGORY_MO,
                    LEVEL_JUDUL_VAR = data.LEVEL_VAR,
                    VALUE_JUDUL_VAR = JUDUL_VAR
                };
                ErasoftDbContext.STF20.Add(newStf20);
            }
            ErasoftDbContext.SaveChanges();

            //var stf20b = ErasoftDbContext.STF20B.Where(m => m.CATEGORY_MO == data.CATEGORY_MO && m.LEVEL_VAR == data.LEVEL_VAR && m.KODE_VAR == data.KODE_VAR).FirstOrDefault();
            if (stf20b == null)
            {
                ErasoftDbContext.STF20B.Add(data);
                ErasoftDbContext.SaveChanges();
            }

            var kategori = ErasoftDbContext.STF02E.Single(k => k.KODE == data.CATEGORY_MO);
            var stf20 = ErasoftDbContext.STF20.Where(m => m.CATEGORY_MO == kategori.KODE).ToList();
            var vm = new MasterStrukturVarViewModel()
            {
                Kategori = kategori,
                Variant_Level_1 = new STF20()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_JUDUL_VAR = 1,
                    VALUE_JUDUL_VAR = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(1)).FirstOrDefault()?.VALUE_JUDUL_VAR
                },
                VariantOpt_Level_1 = new STF20B()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_VAR = 1
                },
                Variant_Level_2 = new STF20()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_JUDUL_VAR = 2,
                    VALUE_JUDUL_VAR = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(2)).FirstOrDefault()?.VALUE_JUDUL_VAR
                },
                VariantOpt_Level_2 = new STF20B()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_VAR = 2
                },
                Variant_Level_3 = new STF20()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_JUDUL_VAR = 3,
                    VALUE_JUDUL_VAR = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(3)).FirstOrDefault()?.VALUE_JUDUL_VAR
                },
                VariantOpt_Level_3 = new STF20B()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_VAR = 3
                },
                VariantOptInDb = ErasoftDbContext.STF20B.Where(m => m.CATEGORY_MO == kategori.KODE).OrderBy(m => m.LEVEL_VAR).ToList()
            };

            return PartialView("StrukturVarPartial", vm);
        }

        public ActionResult DeleteVariantOptLevel(int? recNum, int? recNumVariantOpt)
        {
            var deleteStf20b = ErasoftDbContext.STF20B.Where(m => m.RECNUM == recNumVariantOpt.Value).SingleOrDefault();
            if (deleteStf20b != null)
            {
                ErasoftDbContext.STF20B.Remove(deleteStf20b);
                ErasoftDbContext.SaveChanges();
            }

            var kategori = ErasoftDbContext.STF02E.Single(k => k.RecNum == recNum.Value);
            var stf20 = ErasoftDbContext.STF20.Where(m => m.CATEGORY_MO == kategori.KODE).ToList();
            var vm = new MasterStrukturVarViewModel()
            {
                Kategori = kategori,
                Variant_Level_1 = new STF20()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_JUDUL_VAR = 1,
                    VALUE_JUDUL_VAR = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(1)).FirstOrDefault()?.VALUE_JUDUL_VAR
                },
                VariantOpt_Level_1 = new STF20B()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_VAR = 1
                },
                Variant_Level_2 = new STF20()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_JUDUL_VAR = 2,
                    VALUE_JUDUL_VAR = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(2)).FirstOrDefault()?.VALUE_JUDUL_VAR
                },
                VariantOpt_Level_2 = new STF20B()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_VAR = 2
                },
                Variant_Level_3 = new STF20()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_JUDUL_VAR = 3,
                    VALUE_JUDUL_VAR = stf20.Where(m => m.LEVEL_JUDUL_VAR.Equals(3)).FirstOrDefault()?.VALUE_JUDUL_VAR
                },
                VariantOpt_Level_3 = new STF20B()
                {
                    CATEGORY_MO = kategori.KODE,
                    LEVEL_VAR = 3
                },
                VariantOptInDb = ErasoftDbContext.STF20B.Where(m => m.CATEGORY_MO == kategori.KODE).OrderBy(m => m.LEVEL_VAR).ToList()
            };

            return PartialView("StrukturVarPartial", vm);
        }

        public ActionResult DeleteKategori(int? recNum)
        {
            var kategoriInDb = ErasoftDbContext.STF02E.Single(k => k.RecNum == recNum);

            ErasoftDbContext.STF02E.Remove(kategoriInDb);
            ErasoftDbContext.SaveChanges();

            return RedirectToAction("RefreshTableKategori");
        }

        [HttpPost]
        public ActionResult SaveMerkBarang(MerkBarangViewModel dataMerk)
        {
            if (!ModelState.IsValid)
            {
                var vm = new MerkBarangViewModel()
                {
                    Merk = dataMerk.Merk,
                    ListMerk = ErasoftDbContext.STF02E.Where(m => m.LEVEL == "2").ToList()
                };

                return View("MerkBarang", vm);
            }

            var checkData = ErasoftDbContext.STF02E.SingleOrDefault(m => m.KODE == dataMerk.Merk.KODE);

            ////add by nurul 3/10/2018
            //var vmError = new StokViewModel() { };
            //var check = ErasoftDbContext.STF02E.SingleOrDefault(m => m.KET == dataMerk.Merk.KET && m.USERNAME == dataMerk.Merk.USERNAME);
            //if (check != null)
            //{
            //    vmError.Errors.Add("Nama Merk ini sudah digunakan !");
            //    return Json(vmError, JsonRequestBehavior.AllowGet);
            //}
            ////end add

            if (dataMerk.Merk.RecNum == null)
            {
                if (checkData == null)
                {
                    ErasoftDbContext.STF02E.Add(dataMerk.Merk);
                }
                else
                {
                    ModelState.AddModelError("", $@"Merk dengan kode {dataMerk.Merk.KODE} sudah ada! Coba kode yang lain!");

                    var merkVm = new MerkBarangViewModel()
                    {
                        Merk = dataMerk.Merk,
                        ListMerk = ErasoftDbContext.STF02E.Where(m => m.LEVEL == "2").ToList()
                    };

                    return View("MerkBarang", merkVm);
                }
            }
            else
            {
                var merkInDb = ErasoftDbContext.STF02E.Single(m => m.RecNum == dataMerk.Merk.RecNum);

                //merkInDb.KODE = dataMerk.Merk.KODE;
                merkInDb.KET = dataMerk.Merk.KET;
            }

            ErasoftDbContext.SaveChanges();

            return RedirectToAction("RefreshTableMerk");
        }

        public ActionResult RefreshTableMerk()
        {
            var listKategori = ErasoftDbContext.STF02E.Where(k => k.LEVEL == "2").ToList();

            return PartialView("TableMerkPartial", listKategori.ToPagedList(1, 10));
        }

        public ActionResult EditMerk(int? recNum)
        {
            var vm = new MerkBarangViewModel()
            {
                Merk = ErasoftDbContext.STF02E.Single(m => m.RecNum == recNum)
            };

            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteMerk(int? recNum)
        {
            var merkInDb = ErasoftDbContext.STF02E.Single(m => m.RecNum == recNum);

            ErasoftDbContext.STF02E.Remove(merkInDb);
            ErasoftDbContext.SaveChanges();

            return RedirectToAction("RefreshTableMerk");
        }

        // =============================================== Bagian Barang (START)

        // =============================================== Bagian User (START)

        [Route("manage/master/akun")]
        public ActionResult Akun()
        {
            var dataSession = Session["SessionInfo"] as AccountUserViewModel;
            //change by nurul 8/2/2019
            //if (dataSession?.User != null)
            //    return View("NoPermission");
            var userAc = new List<User>();
            var accountId = new long();
            if (dataSession?.User != null)
            {
                accountId = MoDbContext.Account.SingleOrDefault(a => a.AccountId == dataSession.User.AccountId).AccountId;
                userAc = MoDbContext.User.Where(a => a.AccountId == accountId).ToList();
            }
            else if (dataSession?.Account != null)
            {
                accountId = dataSession.Account.AccountId;
                userAc = MoDbContext.User.Where(a => a.AccountId == accountId).ToList();
            }
            //end change by nurul 8/2/2019

            var vm = new AccountUserViewModel()
            {
                //change by nurul 8/2/2019
                //ListUser = MoDbContext.User.Where(u => u.AccountId == dataSession.Account.AccountId).ToList(),
                ListUser = userAc,
                //end change by nurul 8/2/2019
                ListSec = MoDbContext.SecUser.ToList(),

                //add by nurul 1/3/2019
                ListSubs = MoDbContext.Subscription.ToList()
                //end add by nurul 1/3/2019
            };

            return View(vm);
        }

        public ActionResult RefreshTableAkun()
        {
            var dataSession = Session["SessionInfo"] as AccountUserViewModel;

            //change by nurul 8/2/2019
            //if (dataSession?.User != null)
            //    return View("NoPermission");
            var userAc = new List<User>();
            var accountId = new long();
            if (dataSession?.User != null)
            {
                accountId = MoDbContext.Account.SingleOrDefault(a => a.AccountId == dataSession.User.AccountId).AccountId;
                userAc = MoDbContext.User.Where(a => a.AccountId == accountId).ToList();
            }
            else if (dataSession?.Account != null)
            {
                accountId = dataSession.Account.AccountId;
                userAc = MoDbContext.User.Where(a => a.AccountId == accountId).ToList();
            }
            //end change by nurul 8/2/2019

            var vm = new AccountUserViewModel()
            {
                //change by nurul 8/2/2019
                //ListUser = MoDbContext.User.Where(u => u.AccountId == dataSession.Account.AccountId).ToList(),
                ListUser = userAc,
                //end change by nurul 8/2/2019
                ListSec = MoDbContext.SecUser.ToList()
            };

            return PartialView("TableAkunPartial", vm);
        }

        public ActionResult RefreshAkunForm()
        {
            try
            {
                var vm = new AccountUserViewModel();

                return PartialView("FormAkunPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpPost]
        [Route("manage/submit/user")]
        public ActionResult SaveUser(AccountUserViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                if (!ModelState.IsValid)
                {
                    viewModel.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                    return Json(viewModel, JsonRequestBehavior.AllowGet);
                }
            }

            if (viewModel.User.UserId == null)
            {
                var checkUser = MoDbContext.User.SingleOrDefault(u => u.Email == viewModel.User.Email);
                var checkAkun = MoDbContext.Account.SingleOrDefault(u => u.Email == viewModel.User.Email);

                if (checkUser == null && checkAkun == null)
                {
                    var accInDb = MoDbContext.Account.Single(ac => ac.AccountId == viewModel.User.AccountId);

                    var key = accInDb.VCode;
                    var originPassword = viewModel.User.Password;
                    var encodedPassword = Helper.EncodePassword(originPassword, key);

                    viewModel.User.Password = encodedPassword;
                    viewModel.User.KonfirmasiPassword = encodedPassword;
                    viewModel.User.Status = true; // Otomatis aktif
                    MoDbContext.User.Add(viewModel.User);
                }
                else
                {
                    viewModel.Errors.Add("Email sudah terdaftar!");
                    return Json(viewModel, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var userInDb = MoDbContext.User.Single(c => c.UserId == viewModel.User.UserId);
                var accInDb = MoDbContext.Account.Single(ac => ac.AccountId == viewModel.User.AccountId);

                var key = accInDb.VCode;
                var originPassword = viewModel.User.Password;
                var encodedPassword = Helper.EncodePassword(originPassword, key);

                userInDb.Email = viewModel.User.Email;
                userInDb.Username = viewModel.User.Username;
                userInDb.NoHp = viewModel.User.NoHp;

                if (userInDb.Password != encodedPassword)
                {
                    userInDb.Password = encodedPassword;
                    userInDb.KonfirmasiPassword = encodedPassword;
                }
            }

            MoDbContext.SaveChanges();
            ModelState.Clear();

            var dataSession = Session["SessionInfo"] as AccountUserViewModel;

            //change by nurul 8/2/2019
            //if (dataSession?.User != null)
            //    return View("NoPermission");
            var userAc = new List<User>();
            var accountId = new long();
            if (dataSession?.User != null)
            {
                accountId = MoDbContext.Account.SingleOrDefault(a => a.AccountId == dataSession.User.AccountId).AccountId;
                userAc = MoDbContext.User.Where(a => a.AccountId == accountId).ToList();
            }
            else if (dataSession?.Account != null)
            {
                accountId = dataSession.Account.AccountId;
                userAc = MoDbContext.User.Where(a => a.AccountId == accountId).ToList();
            }
            //end change by nurul 8/2/2019

            var vm = new AccountUserViewModel()
            {
                //change by nurul 8/2/2019
                //ListUser = MoDbContext.User.Where(u => u.AccountId == dataSession.Account.AccountId).ToList(),
                ListUser = userAc,
                //end change by nurul 8/2/2019
                ListSec = MoDbContext.SecUser.ToList()
            };

            return PartialView("TableAkunPartial", vm);
        }

        public ActionResult EditUser(int? userId)
        {
            try
            {
                var vm = new AccountUserViewModel()
                {
                    User = MoDbContext.User.Single(u => u.UserId == userId)
                };

                return PartialView("FormAkunPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult DeleteUser(int? userId)
        {
            var user = MoDbContext.User.Single(u => u.UserId == userId);

            MoDbContext.User.Remove(user);
            MoDbContext.SaveChanges();

            var dataSession = Session["SessionInfo"] as AccountUserViewModel;

            //change by nurul 8/2/2019
            //if (dataSession?.User != null)
            //    return View("NoPermission");
            var userAc = new List<User>();
            var accountId = new long();
            if (dataSession?.User != null)
            {
                accountId = MoDbContext.Account.SingleOrDefault(a => a.AccountId == dataSession.User.AccountId).AccountId;
                userAc = MoDbContext.User.Where(a => a.AccountId == accountId).ToList();
            }
            else if (dataSession?.Account != null)
            {
                accountId = dataSession.Account.AccountId;
                userAc = MoDbContext.User.Where(a => a.AccountId == accountId).ToList();
            }
            //end change by nurul 8/2/2019

            var vm = new AccountUserViewModel()
            {
                //change by nurul 8/2/2019
                //ListUser = MoDbContext.User.Where(u => u.AccountId == dataSession.Account.AccountId).ToList(),
                ListUser = userAc,
                //end change by nurul 8/2/2019
                ListSec = MoDbContext.SecUser.ToList()
            };

            return PartialView("TableAkunPartial", vm);
        }

        //add by nurul 1/3/2019
        public ActionResult CekJumlahUser(long accId)
        {
            var accInDb = MoDbContext.Account.FirstOrDefault(a => a.AccountId == accId);

            if (accInDb == null)
            {
                var accIdByUser = MoDbContext.User.FirstOrDefault(u => u.AccountId == accId)?.AccountId;
                accInDb = MoDbContext.Account.FirstOrDefault(a => a.AccountId == accIdByUser);
            }

            var accSubs = MoDbContext.Subscription.FirstOrDefault(s => s.KODE == accInDb.KODE_SUBSCRIPTION);
            var cekuser = MoDbContext.User.Where(a => a.AccountId == accId).Count();
            var jmluser = false;
            if (accSubs.KODE == "03")
            {
                if (cekuser >= accInDb.jumlahUser) //basic dan gold
                {
                    jmluser = true;
                }
            } else if (accSubs.KODE == "02") { 
                if (cekuser >= 2) //silver
                {
                    jmluser = true;
                }
            }else if (accSubs.KODE == "01")
            {
                if (cekuser >= 0)
                {
                    jmluser = true;
                }
            }


            var valSubs = new ValidasiSubs()
            {
                //JumlahUserLebih = (cekuser >= accInDb.jumlahUser)
                JumlahUserLebih = jmluser
            };

            return Json(valSubs, JsonRequestBehavior.AllowGet);
        }
        //end add by nurul 1/3/2019

        // =============================================== Bagian User (END)

        // =============================================== Bagian Security (START)

        [HttpPost]
        public ActionResult SaveSecUser(DataSecUser dataSec)
        {
            var userId = Session["UserId"] as long?;

            foreach (var entity in MoDbContext.SecUser.Where(s => s.UserId == userId).ToList())
                MoDbContext.SecUser.Remove(entity);

            var dataSession = Session["SessionInfo"] as AccountUserViewModel;
            var counter = 0;
            //List<SecUser> _testList = new List<SecUser>();

            foreach (var form in dataSec.FormArray)
            {
                var secUser = new SecUser
                {
                    AccountId = dataSession?.Account.AccountId,
                    UserId = userId,
                    FormId = Convert.ToInt32(form),
                    ParentId = Convert.ToInt32(dataSec.ParentArray[counter]),
                    Permission = true
                };
                counter++;
                //_testList.Add(secUser);
                MoDbContext.SecUser.Add(secUser);
            }

            MoDbContext.SaveChanges();
            ModelState.Clear();

            var secuserVm = new SecurityUserViewModel()
            {
                User = MoDbContext.User.SingleOrDefault(u => u.UserId == userId),
                ListForms = MoDbContext.FormMoses.Where(f => f.Show).ToList(),
                ListSec = MoDbContext.SecUser.Where(s => s.UserId == userId).ToList(),
                ListUser = MoDbContext.User.ToList(),
            };

            return Json($"Settingan security untuk user {secuserVm.User.Username} berhasil disimpan.", JsonRequestBehavior.AllowGet);
        }

        // =============================================== Bagian Security (END)

        // =============================================== Bagian Faktur Penjualan (START)

        [HttpGet]
        public ActionResult GetFaktur()
        {
            var listFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2").ToList();
            var listKodeFaktur = new List<FakturJson>();

            foreach (var faktur in listFaktur)
            {
                listKodeFaktur.Add(new FakturJson()
                {
                    RecNum = faktur.RecNum,
                    NO_BUKTI = faktur.NO_BUKTI
                });
            }

            return Json(listKodeFaktur, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetFakturByMarketplace(string kodeMarket)
        {
            var listFaktur = ErasoftDbContext.SIT01A
                            // change by ega 9/8/2018 , katanya calvin dibikin kaya gini aja, jadi tanya dia aja
                            //.Where(f => f.JENIS_FORM == "2" && f.CUST == kodeMarket && String.IsNullOrEmpty(f.NO_REF))
                            .Where(f => f.JENIS_FORM == "2" && f.CUST == kodeMarket && (String.IsNullOrEmpty(f.NO_REF) || f.NO_REF == "-"))
                            .OrderBy(f => f.NO_BUKTI).ToList();
            var listKodeFaktur = new List<FakturJson>();

            foreach (var faktur in listFaktur)
            {
                listKodeFaktur.Add(new FakturJson()
                {
                    RecNum = faktur.RecNum,
                    NO_BUKTI = faktur.NO_BUKTI
                });
            }

            return Json(listKodeFaktur, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveFaktur(FakturViewModel dataVm)
        {
            if (!ModelState.IsValid)
            {
                dataVm.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(dataVm, JsonRequestBehavior.AllowGet);
            }

            if (dataVm.Faktur.RecNum == null)
            {
                var listFakturInDb = ErasoftDbContext.SIT01A.OrderBy(p => p.RecNum).ToList();
                var digitAkhir = "";
                var noOrder = "";

                if (listFakturInDb.Count == 0)
                {
                    digitAkhir = "000001";
                    noOrder = $"SI{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                    ErasoftDbContext.Database.ExecuteSqlCommand("DBCC CHECKIDENT (SIT01A, RESEED, 0)");
                }
                else
                {
                    var lastRecNum = listFakturInDb.Last().RecNum;
                    lastRecNum++;

                    digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                    noOrder = $"SI{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                }

                //add by calvin, validasi QOH
                var qtyOnHand = GetQOHSTF08A(dataVm.FakturDetail.BRG, dataVm.FakturDetail.GUDANG);
                if (qtyOnHand < dataVm.FakturDetail.QTY)
                {
                    dataVm.Errors.Add("Qty penjualan melebihi qty yang ada di gudang ( " + Convert.ToString(qtyOnHand) + " )");
                    return Json(dataVm, JsonRequestBehavior.AllowGet);
                }
                //end add by calvin, validasi QOH

                dataVm.Faktur.NO_BUKTI = noOrder;
                dataVm.Faktur.NO_F_PAJAK = "";
                dataVm.Faktur.NAMA_CUST = ErasoftDbContext.ARF01.Single(p => p.CUST == dataVm.Faktur.CUST).NAMA;
                dataVm.Faktur.AL = ErasoftDbContext.ARF01.Single(p => p.CUST == dataVm.Faktur.CUST).AL;
                dataVm.Faktur.AL2 = ErasoftDbContext.ARF01.Single(p => p.CUST == dataVm.Faktur.CUST).AL2;
                dataVm.Faktur.AL3 = ErasoftDbContext.ARF01.Single(p => p.CUST == dataVm.Faktur.CUST).AL3;
                dataVm.Faktur.PPN_Bln_Lapor = Convert.ToByte(dataVm.Faktur.TGL.ToString("MM"));
                dataVm.Faktur.PPN_Thn_Lapor = Convert.ToByte(dataVm.Faktur.TGL.ToString("yyyy").Substring(2, 2));

                if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.NILAI_PPNBM)))
                {
                    dataVm.Faktur.NILAI_PPNBM = 0;
                }
                #region add by calvin 6 juni 2018, agar sit01a field yang penting tidak null
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.NO_SO)))
                {
                    dataVm.Faktur.NO_SO = "";
                }
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.NO_REF)))
                {
                    dataVm.Faktur.NO_REF = "";
                }
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.DISCOUNT)))
                {
                    dataVm.Faktur.DISCOUNT = 0;
                }
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.CUST_QQ)))
                {
                    dataVm.Faktur.CUST_QQ = "";
                }
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.NAMA_CUST_QQ)))
                {
                    dataVm.Faktur.NAMA_CUST_QQ = "";
                }
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.STATUS_LOADING)))
                {
                    dataVm.Faktur.STATUS_LOADING = "";
                }
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.NO_PO_CUST)))
                {
                    dataVm.Faktur.NO_PO_CUST = "";
                }
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.PENGIRIM)))
                {
                    dataVm.Faktur.PENGIRIM = "";
                }
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.NAMAPENGIRIM)))
                {
                    dataVm.Faktur.NAMAPENGIRIM = "";
                }
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.ZONA)))
                {
                    dataVm.Faktur.ZONA = "";
                }
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.UCAPAN)))
                {
                    dataVm.Faktur.UCAPAN = "";
                }
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.N_UCAPAN)))
                {
                    dataVm.Faktur.N_UCAPAN = "";
                }
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.PEMESAN)))
                {
                    dataVm.Faktur.PEMESAN = "";
                }
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.SUPP)))
                {
                    dataVm.Faktur.SUPP = "";
                }
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.KOMISI)))
                {
                    dataVm.Faktur.KOMISI = 0;
                }
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.N_KOMISI)))
                {
                    dataVm.Faktur.N_KOMISI = 0;
                }
                #endregion

                dataVm.FakturDetail.NO_BUKTI = noOrder;

                if (string.IsNullOrEmpty(Convert.ToString(dataVm.FakturDetail.NILAI_DISC_1)))
                {
                    dataVm.FakturDetail.NILAI_DISC_1 = 0;
                }
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.FakturDetail.NILAI_DISC_2)))
                {
                    dataVm.FakturDetail.NILAI_DISC_2 = 0;
                }
                dataVm.FakturDetail.NILAI_DISC = dataVm.FakturDetail.NILAI_DISC_1 + dataVm.FakturDetail.NILAI_DISC_2;

                if (string.IsNullOrEmpty(Convert.ToString(dataVm.FakturDetail.QTY_KIRIM)))
                {
                    dataVm.FakturDetail.QTY_KIRIM = 0;
                }
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.FakturDetail.QTY_RETUR)))
                {
                    dataVm.FakturDetail.QTY_RETUR = 0;
                }
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.FakturDetail.DISCOUNT_3)))
                {
                    dataVm.FakturDetail.DISCOUNT_3 = 0;
                }
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.FakturDetail.DISCOUNT_4)))
                {
                    dataVm.FakturDetail.DISCOUNT_4 = 0;
                }
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.FakturDetail.DISCOUNT_5)))
                {
                    dataVm.FakturDetail.DISCOUNT_5 = 0;
                }
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.FakturDetail.NILAI_DISC_3)))
                {
                    dataVm.FakturDetail.NILAI_DISC_3 = 0;
                }
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.FakturDetail.NILAI_DISC_4)))
                {
                    dataVm.FakturDetail.NILAI_DISC_4 = 0;
                }
                if (string.IsNullOrEmpty(Convert.ToString(dataVm.FakturDetail.NILAI_DISC_5)))
                {
                    dataVm.FakturDetail.NILAI_DISC_5 = 0;
                }

                ErasoftDbContext.SIT01A.Add(dataVm.Faktur);
                ErasoftDbContext.SaveChanges();

                if (dataVm.FakturDetail.NO_URUT == null)
                {
                    ErasoftDbContext.SIT01B.Add(dataVm.FakturDetail);
                    ErasoftDbContext.SIT01A.Where(p => p.NO_BUKTI == noOrder && p.JENIS_FORM == "2").Update(p => new SIT01A() { BRUTO = dataVm.Faktur.BRUTO });
                }
            }
            else
            {
                var fakturInDb = ErasoftDbContext.SIT01A.Single(p => p.NO_BUKTI == dataVm.Faktur.NO_BUKTI && p.JENIS_FORM == "2");

                //add by calvin, 22 juni 2018 validasi QOH
                var qtyOnHand = GetQOHSTF08A(dataVm.FakturDetail.BRG, dataVm.FakturDetail.GUDANG);

                if (qtyOnHand < dataVm.FakturDetail.QTY)
                {
                    dataVm.Errors.Add("Qty penjualan melebihi qty yang ada di gudang ( " + Convert.ToString(qtyOnHand) + " )");
                    return Json(dataVm, JsonRequestBehavior.AllowGet);
                }
                //end add by calvin, validasi QOH

                fakturInDb.NETTO = dataVm.Faktur.NETTO;
                fakturInDb.BRUTO = dataVm.Faktur.BRUTO;
                fakturInDb.DISCOUNT = dataVm.Faktur.DISCOUNT;
                fakturInDb.PPN = dataVm.Faktur.PPN;
                fakturInDb.NILAI_PPN = dataVm.Faktur.NILAI_PPN;

                dataVm.FakturDetail.NO_BUKTI = dataVm.Faktur.NO_BUKTI;
                dataVm.FakturDetail.NILAI_DISC = dataVm.FakturDetail.NILAI_DISC_1 + dataVm.FakturDetail.NILAI_DISC_2;

                if (dataVm.FakturDetail.NO_URUT == null)
                {
                    ErasoftDbContext.SIT01B.Add(dataVm.FakturDetail);
                }
            }

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            //add by calvin 8 nov 2018, update stok marketplace
            List<string> listBrg = new List<string>();
            listBrg.Add(dataVm.FakturDetail.BRG);
            updateStockMarketPlace(listBrg);
            //end add by calvin 8 nov 2018

            var vm = new FakturViewModel()
            {
                Faktur = ErasoftDbContext.SIT01A.Single(p => p.NO_BUKTI == dataVm.Faktur.NO_BUKTI && p.JENIS_FORM == "2"),
                ListFakturDetail = ErasoftDbContext.SIT01B.Where(pd => pd.NO_BUKTI == dataVm.Faktur.NO_BUKTI && pd.JENIS_FORM == "2").ToList(),
                //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/12019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListNFaktur = ErasoftDbContext.ART03B.ToList(),
            };

            return PartialView("BarangFakturPartial", vm);
        }

        public ActionResult SaveReturFaktur(FakturViewModel dataVm)
        {
            if (!ModelState.IsValid)
            {
                dataVm.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(dataVm, JsonRequestBehavior.AllowGet);
            }

            bool returBaru = false;

            if (dataVm.Faktur.RecNum == null)
            {

                var cekREf = ErasoftDbContext.SIT01A.SingleOrDefault(f => f.NO_REF == dataVm.Faktur.NO_REF);
                if (cekREf == null)
                {
                    var listFakturInDb = ErasoftDbContext.SIT01A.OrderBy(p => p.RecNum).ToList();
                    var digitAkhir = "";
                    var noOrder = "";

                    if (listFakturInDb.Count == 0)
                    {
                        digitAkhir = "000001";
                        noOrder = $"RJ{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                        ErasoftDbContext.Database.ExecuteSqlCommand("DBCC CHECKIDENT (SIT01A, RESEED, 0)");
                    }
                    else
                    {
                        var lastRecNum = listFakturInDb.Last().RecNum;
                        lastRecNum++;

                        digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                        noOrder = $"RJ{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                    }

                    var fakturInDb = ErasoftDbContext.SIT01A.SingleOrDefault(f => f.NO_BUKTI == dataVm.Faktur.NO_REF);

                    if (fakturInDb != null)
                    {
                        fakturInDb.NO_REF = noOrder;
                        dataVm.Faktur.PEMESAN = fakturInDb.PEMESAN;
                        dataVm.Faktur.NAMAPEMESAN = fakturInDb.NAMAPEMESAN;
                    }

                    //var recNumCust = ParseInt(dataVm.Faktur.CUST);
                    var CustInDb = ErasoftDbContext.ARF01.SingleOrDefault(p => p.CUST == dataVm.Faktur.CUST);
                    if (CustInDb != null)
                    {
                        dataVm.Faktur.NAMA_CUST = CustInDb.NAMA;
                        dataVm.Faktur.AL = CustInDb.AL;
                        dataVm.Faktur.AL2 = CustInDb.AL2;
                        dataVm.Faktur.AL3 = CustInDb.AL3;
                    }
                    dataVm.Faktur.NO_BUKTI = noOrder;
                    dataVm.Faktur.NO_F_PAJAK = "";
                    //dataVm.Faktur.NAMA_CUST = ErasoftDbContext.ARF01.Single(p => p.CUST == dataVm.Faktur.CUST).NAMA;
                    //dataVm.Faktur.AL = ErasoftDbContext.ARF01.Single(p => p.RecNum == recNumCust).AL;
                    //dataVm.Faktur.AL2 = ErasoftDbContext.ARF01.Single(p => p.RecNum == recNumCust).AL2;
                    //dataVm.Faktur.AL3 = ErasoftDbContext.ARF01.Single(p => p.RecNum == recNumCust).AL3;
                    dataVm.Faktur.PPN_Bln_Lapor = Convert.ToByte(dataVm.Faktur.TGL.ToString("MM"));
                    dataVm.Faktur.PPN_Thn_Lapor = Convert.ToByte(dataVm.Faktur.TGL.ToString("yyyy").Substring(2, 2));
                    ErasoftDbContext.SIT01A.Add(dataVm.Faktur);

                    ErasoftDbContext.SaveChanges();

                    //add by ega
                    returBaru = true;
                    //end add by ega
                }
                else
                {
                    dataVm.Faktur.NO_BUKTI = cekREf.NO_BUKTI;
                }
            }
            else
            {
                //add by calvin 16 nov 2018, cek jika tidak ada detail, autoload
                var cekdetail = ErasoftDbContext.SIT01B.FirstOrDefault(p => p.NO_BUKTI == dataVm.Faktur.NO_BUKTI);
                if (cekdetail != null)
                {
                    //UPDATE ANAK
                    var FakturDetailDB = ErasoftDbContext.SIT01B.Single(p => p.NO_BUKTI == dataVm.Faktur.NO_BUKTI && p.BRG == dataVm.FakturDetail.BRG);

                    //add by calvin, validasi QOH
                    var qtyOnHand = GetQOHSTF08A(FakturDetailDB.BRG, FakturDetailDB.GUDANG);

                    if (qtyOnHand - FakturDetailDB.QTY + dataVm.FakturDetail.QTY < 0)
                    {
                        var vmError = new InvoiceViewModel()
                        {

                        };
                        vmError.Errors.Add("Tidak bisa retur, Qty untuk barang ( " + FakturDetailDB.BRG + " ) di gudang " + FakturDetailDB.GUDANG + " sisa ( " + Convert.ToString(qtyOnHand) + " ).");
                        return Json(vmError, JsonRequestBehavior.AllowGet);
                    }
                    //end add by calvin, validasi QOH

                    FakturDetailDB.QTY = dataVm.FakturDetail.QTY;
                    FakturDetailDB.DISCOUNT = dataVm.FakturDetail.DISCOUNT;
                    FakturDetailDB.DISCOUNT_2 = dataVm.FakturDetail.DISCOUNT_2;
                    FakturDetailDB.NILAI_DISC_1 = dataVm.FakturDetail.NILAI_DISC_1;
                    FakturDetailDB.NILAI_DISC_2 = dataVm.FakturDetail.NILAI_DISC_2;
                    FakturDetailDB.NILAI_DISC = dataVm.FakturDetail.NILAI_DISC_1 + dataVm.FakturDetail.NILAI_DISC_2;
                    FakturDetailDB.HARGA = (dataVm.FakturDetail.QTY) * (FakturDetailDB.H_SATUAN) - (FakturDetailDB.NILAI_DISC_1 + FakturDetailDB.NILAI_DISC_2);
                    ErasoftDbContext.SaveChanges();

                    //UPDATE BAPAK
                    var fakturInDb = ErasoftDbContext.SIT01A.Single(p => p.NO_BUKTI == dataVm.Faktur.NO_BUKTI && p.JENIS_FORM == "3");
                    double bruto_ = (double)ErasoftDbContext.SIT01B.Where(p => p.NO_BUKTI == dataVm.Faktur.NO_BUKTI).Sum(p => p.HARGA);
                    //vm.NilaiPesananHariIni = vm.ListPesanan?.Where(p => p.TGL == selectedDate).Sum(p => p.BRUTO - p.NILAI_DISC);

                    fakturInDb.BRUTO = bruto_;
                    fakturInDb.NILAI_DISC = dataVm.Faktur.NILAI_DISC;
                    fakturInDb.PPN = dataVm.Faktur.PPN;
                    fakturInDb.NILAI_PPN = dataVm.Faktur.NILAI_PPN;
                    fakturInDb.MATERAI = dataVm.Faktur.MATERAI;
                    fakturInDb.NETTO = fakturInDb.BRUTO - fakturInDb.NILAI_DISC + fakturInDb.NILAI_PPN + fakturInDb.MATERAI;
                    ErasoftDbContext.SaveChanges();

                    returBaru = false;

                    //add by calvin 8 nov 2018, update stok marketplace
                    List<string> listBrg = new List<string>();
                    listBrg.Add(FakturDetailDB.BRG);
                    updateStockMarketPlace(listBrg);
                    //end add by calvin 8 nov 2018
                }
                else
                {
                    returBaru = true;
                }
            }

            // autoload detail item, jika buat retur baru
            if (returBaru)
            {
                object[] spParams = {
                    new SqlParameter("@NOBUK",dataVm.Faktur.NO_BUKTI),
                    new SqlParameter("@NO_REF",dataVm.Faktur.NO_REF)
                };

                ErasoftDbContext.Database.ExecuteSqlCommand("exec [SP_AUTOLOADRETUR_PENJUALAN] @NOBUK, @NO_REF", spParams);

                //add by calvin 8 nov 2018, update stok marketplace
                List<string> listBrg = new List<string>();
                var detailReturFakturInDb = ErasoftDbContext.SIT01B.AsNoTracking().Where(pd => pd.NO_BUKTI == dataVm.Faktur.NO_BUKTI && pd.JENIS_FORM == "3").ToList();
                foreach (var item in detailReturFakturInDb)
                {
                    listBrg.Add(item.BRG);
                }
                updateStockMarketPlace(listBrg);
                //end add by calvin 8 nov 2018
            }
            ModelState.Clear();

            var vm = new FakturViewModel()
            {
                Faktur = ErasoftDbContext.SIT01A.AsNoTracking().Single(p => p.NO_BUKTI == dataVm.Faktur.NO_BUKTI && p.JENIS_FORM == "3"),
                //Faktur = ErasoftDbContext.SIT01A.Single(p => p.NO_BUKTI == dataVm.Faktur.NO_BUKTI && p.JENIS_FORM == "3"),
                ListFakturDetail = ErasoftDbContext.SIT01B.AsNoTracking().Where(pd => pd.NO_BUKTI == dataVm.Faktur.NO_BUKTI && pd.JENIS_FORM == "3").ToList(),
                //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
            };


            return PartialView("BarangReturPartial", vm);
        }

        public ActionResult RefreshTableFaktur1()
        {
            var vm = new FakturViewModel()
            {
                ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2").ToList(),
                //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListNFaktur = ErasoftDbContext.ART03B.ToList(),
                ListPesanan = ErasoftDbContext.SOT01A.ToList(),


            };

            return PartialView("TableFakturPartial", vm);
        }

        public ActionResult RefreshTableFakturLunas()
        {
            IEnumerable<ART01D> FakturSudahLunas = ErasoftDbContext.ART01D.Where(a => a.NETTO.Value - a.KREDIT.Value > 0);
            var vm = new FakturViewModel()
            {
                ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2" && FakturSudahLunas.Any(a => a.FAKTUR == f.NO_BUKTI))
                            .ToList(),
                //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListNFaktur = ErasoftDbContext.ART03B.ToList(),
                ListPesanan = ErasoftDbContext.SOT01A.ToList(),

            };

            return PartialView("TableFakturLunasPartial", vm);
        }
        public ActionResult RefreshTableFakturTempo(string tgl)
        {
            //IEnumerable<ART01D> FakturJatuhTempo = ErasoftDbContext.ART01D.Where(a => a.NETTO.Value - a.KREDIT.Value > 0);
            //add by nurul 10/1/2019
            var tanggal = DateTime.ParseExact(tgl, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            //end add
            var vm = new FakturViewModel()
            {
                //change by nurul 10/1/2019 -- ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2" && f.TGL_JT_TEMPO <= DateTime.Now).ToList(),
                ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2" && f.TGL_JT_TEMPO <= tanggal).ToList(),
                //end change 
                //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListNFaktur = ErasoftDbContext.ART03B.ToList(),
                ListPesanan = ErasoftDbContext.SOT01A.ToList(),

            };

            return PartialView("TableFakturLunasPartial", vm);
        }


        public ActionResult RefreshTableRetur1()
        {
            var vm = new FakturViewModel()
            {
                ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "3").ToList(),
                //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListNFaktur = ErasoftDbContext.ART03B.ToList()
            };

            return PartialView("TableReturPartial", vm);
        }

        public ActionResult RefreshTableReturInvoice()
        {
            var vm = new InvoiceViewModel()
            {
                ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "2").ToList(),
                //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList()
            };

            return PartialView("TableReturInvoicePartial", vm);
        }

        public ActionResult RefreshFakturForm()
        {
            try
            {
                var vm = new FakturViewModel()
                {
                    ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2").ToList(),
                    //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList(),
                    ListNFaktur = ErasoftDbContext.ART03B.ToList()
                };

                return PartialView("BarangFakturPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult RefreshReturFakturForm()
        {
            try
            {
                var vm = new FakturViewModel()
                {
                    ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "3").ToList(),
                    //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList()
                };

                return PartialView("BarangReturPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult GetDataFaktur(int? recNum)
        {
            try
            {
                var fakturInDb = ErasoftDbContext.SIT01A.Single(p => p.RecNum == recNum);

                var vm = new FakturViewModel()
                {
                    Faktur = fakturInDb,
                    ListFaktur = ErasoftDbContext.SIT01A.ToList(),
                    ListFakturDetail = ErasoftDbContext.SIT01B.Where(pd => pd.NO_BUKTI == fakturInDb.NO_BUKTI && pd.JENIS_FORM == "2").ToList(),
                    //ListBarang = ErasoftDbContext.STF02.ToList() 'change by nurul 21/1/2019
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList()
                };

                return PartialView("BarangReturPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult EditFaktur(int? orderId)
        {
            var fakturInDb = ErasoftDbContext.SIT01A.Single(p => p.RecNum == orderId && p.JENIS_FORM == "2");

            var vm = new FakturViewModel()
            {
                Faktur = fakturInDb,
                ListFaktur = ErasoftDbContext.SIT01A.ToList(),
                ListFakturDetail = ErasoftDbContext.SIT01B.Where(pd => pd.NO_BUKTI == fakturInDb.NO_BUKTI && pd.JENIS_FORM == "2").ToList(),
                //ListBarang = ErasoftDbContext.STF02.ToList() 'change by nurul 21/1/2019 
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList()
            };

            return PartialView("BarangFakturPartial", vm);
        }

        public ActionResult EditReturFaktur(int? orderId)
        {
            var fakturInDb = ErasoftDbContext.SIT01A.Single(p => p.RecNum == orderId && p.JENIS_FORM == "3");

            var vm = new FakturViewModel()
            {
                Faktur = fakturInDb,
                ListFaktur = ErasoftDbContext.SIT01A.ToList(),
                ListFakturDetail = ErasoftDbContext.SIT01B.Where(pd => pd.NO_BUKTI == fakturInDb.NO_BUKTI && pd.JENIS_FORM == "3").ToList(),
                //ListBarang = ErasoftDbContext.STF02.ToList() 'change by nurul 21/1/2019 
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList()
            };

            return PartialView("BarangReturPartial", vm);
        }

        public ActionResult DeleteFaktur(int? orderId)
        {
            var fakturInDb = ErasoftDbContext.SIT01A.Single(p => p.RecNum == orderId && p.JENIS_FORM == "2");

            //add by calvin 8 nov 2018, update stok marketplace
            List<string> listBrg = new List<string>();
            var detailFakturInDb = ErasoftDbContext.SIT01B.Where(p => p.NO_BUKTI == fakturInDb.NO_BUKTI && p.JENIS_FORM == "2").ToList();
            foreach (var item in detailFakturInDb)
            {
                listBrg.Add(item.BRG);
            }
            //end add by calvin 8 nov 2018

            ErasoftDbContext.SIT01A.Remove(fakturInDb);
            ErasoftDbContext.SaveChanges();

            //add by calvin 8 nov 2018, update stok marketplace
            updateStockMarketPlace(listBrg);
            //end add by calvin 8 nov 2018

            var vm = new FakturViewModel()
            {
                ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2").ToList(),
                //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListPesanan = ErasoftDbContext.SOT01A.ToList(),
                ListNFaktur = ErasoftDbContext.ART03B.ToList(),

            };

            return PartialView("TableFakturPartial", vm);
        }

        public ActionResult DeleteReturFaktur(int? orderId)
        {
            var returFakturInDb = ErasoftDbContext.SIT01A.Single(p => p.RecNum == orderId && p.JENIS_FORM == "3");
            var fakturInDbWithRef = ErasoftDbContext.SIT01A.Single(p => p.NO_BUKTI == returFakturInDb.NO_REF && p.JENIS_FORM == "2");
            fakturInDbWithRef.NO_REF = "";

            //add by calvin 8 nov 2018, update stok marketplace
            List<string> listBrg = new List<string>();
            //end add by calvin 8 nov 2018

            //add by calvin, validasi QOH
            var returFakturDetailInDb = ErasoftDbContext.SIT01B.Where(b => b.NO_BUKTI == returFakturInDb.NO_BUKTI && b.JENIS_FORM == "3").ToList();
            foreach (var item in returFakturDetailInDb)
            {
                var qtyOnHand = GetQOHSTF08A(item.BRG, item.GUDANG);

                if (qtyOnHand - item.QTY < 0)
                {
                    var vmError = new InvoiceViewModel()
                    {

                    };
                    vmError.Errors.Add("Tidak bisa delete, Qty di gudang sisa ( " + Convert.ToString(qtyOnHand) + " )");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
                //add by calvin 8 nov 2018, update stok marketplace
                listBrg.Add(item.BRG);
                //end add by calvin 8 nov 2018
            }
            //end add by calvin, validasi QOH

            ErasoftDbContext.SIT01A.Remove(returFakturInDb);
            ErasoftDbContext.SaveChanges();

            //add by calvin 8 nov 2018, update stok marketplace
            updateStockMarketPlace(listBrg);
            //end add by calvin 8 nov 2018

            var vm = new FakturViewModel()
            {
                ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "3").ToList(),
                //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListNFaktur = ErasoftDbContext.ART03B.ToList()
            };

            return PartialView("TableReturPartial", vm);
        }

        [HttpGet]
        public ActionResult DeleteBarangFaktur(int noUrut)
        {
            try
            {
                var barangFakturInDb = ErasoftDbContext.SIT01B.Single(b => b.NO_URUT == noUrut && b.JENIS_FORM == "2");
                var fakturInDb = ErasoftDbContext.SIT01A.Single(p => p.NO_BUKTI == barangFakturInDb.NO_BUKTI && p.JENIS_FORM == "2");

                fakturInDb.BRUTO -= barangFakturInDb.HARGA;
                fakturInDb.NILAI_PPN = Math.Ceiling((double)fakturInDb.PPN * (double)fakturInDb.BRUTO / 100);
                //change by nurul 8/10/2018  fakturInDb.NETTO = fakturInDb.BRUTO - fakturInDb.NILAI_DISC + fakturInDb.NILAI_PPN;
                fakturInDb.NETTO = fakturInDb.BRUTO - fakturInDb.NILAI_DISC + fakturInDb.NILAI_PPN + fakturInDb.MATERAI;

                ErasoftDbContext.SIT01B.Remove(barangFakturInDb);
                ErasoftDbContext.SaveChanges();

                //add by calvin 8 nov 2018, update stok marketplace
                List<string> listBrg = new List<string>();
                listBrg.Add(barangFakturInDb.BRG);
                updateStockMarketPlace(listBrg);
                //end add by calvin 8 nov 2018

                var vm = new FakturViewModel()
                {
                    Faktur = ErasoftDbContext.SIT01A.Single(p => p.NO_BUKTI == fakturInDb.NO_BUKTI && p.JENIS_FORM == "2"),
                    ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2").ToList(),
                    ListFakturDetail = ErasoftDbContext.SIT01B.Where(pd => pd.NO_BUKTI == fakturInDb.NO_BUKTI && pd.JENIS_FORM == "2").ToList(),
                    //ListBarang = ErasoftDbContext.STF02.ToList() 'change by nurul 21/1/2019 
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList()
                };

                return PartialView("BarangFakturPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpGet]
        public ActionResult DeleteBarangReturFaktur(int noUrut)
        {
            try
            {
                var barangFakturInDb = ErasoftDbContext.SIT01B.Single(b => b.NO_URUT == noUrut && b.JENIS_FORM == "3");
                var fakturInDb = ErasoftDbContext.SIT01A.Single(p => p.NO_BUKTI == barangFakturInDb.NO_BUKTI && p.JENIS_FORM == "3");

                //add by calvin, validasi QOH
                var qtyOnHand = GetQOHSTF08A(barangFakturInDb.BRG, barangFakturInDb.GUDANG);

                if (qtyOnHand - barangFakturInDb.QTY < 0)
                {
                    var vmError = new InvoiceViewModel()
                    {

                    };
                    vmError.Errors.Add("Tidak bisa delete, Qty di gudang sisa ( " + Convert.ToString(qtyOnHand) + " )");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
                //end add by calvin, validasi QOH

                fakturInDb.BRUTO -= barangFakturInDb.HARGA;
                fakturInDb.NILAI_PPN = Math.Ceiling((double)fakturInDb.PPN * (double)fakturInDb.BRUTO / 100);
                fakturInDb.NETTO = fakturInDb.BRUTO - fakturInDb.NILAI_DISC + fakturInDb.NILAI_PPN;

                ErasoftDbContext.SIT01B.Remove(barangFakturInDb);
                ErasoftDbContext.SaveChanges();

                //add by calvin 8 nov 2018, update stok marketplace
                List<string> listBrg = new List<string>();
                listBrg.Add(barangFakturInDb.BRG);
                updateStockMarketPlace(listBrg);
                //end add by calvin 8 nov 2018

                var vm = new FakturViewModel()
                {
                    Faktur = ErasoftDbContext.SIT01A.AsNoTracking().Single(p => p.NO_BUKTI == fakturInDb.NO_BUKTI && p.JENIS_FORM == "3"),
                    ListFaktur = ErasoftDbContext.SIT01A.AsNoTracking().Where(f => f.JENIS_FORM == "2").ToList(),
                    ListFakturDetail = ErasoftDbContext.SIT01B.AsNoTracking().Where(pd => pd.NO_BUKTI == fakturInDb.NO_BUKTI && pd.JENIS_FORM == "3").ToList(),
                    //ListBarang = ErasoftDbContext.STF02.ToList() 'change by nurul 21/1/2019 
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList()
                };

                return PartialView("BarangReturPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult UpdateFaktur(UpdateData dataUpdate)
        {
            var fakturInDb = ErasoftDbContext.SIT01A.Single(p => p.NO_BUKTI == dataUpdate.OrderId && p.JENIS_FORM == "2");
            fakturInDb.BRUTO = dataUpdate.Bruto;
            fakturInDb.NILAI_DISC = dataUpdate.NilaiDisc;
            fakturInDb.PPN = dataUpdate.Ppn;
            fakturInDb.NILAI_PPN = dataUpdate.NilaiPpn;
            fakturInDb.MATERAI = dataUpdate.OngkosKirim;
            fakturInDb.TGL = DateTime.ParseExact(dataUpdate.Tgl, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            fakturInDb.CUST = dataUpdate.Cust;
            fakturInDb.TERM = dataUpdate.Term;
            fakturInDb.PEMESAN = dataUpdate.Buyer;
            fakturInDb.TGL_JT_TEMPO = DateTime.ParseExact(dataUpdate.Tempo, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            fakturInDb.NETTO = fakturInDb.BRUTO - fakturInDb.NILAI_DISC + fakturInDb.NILAI_PPN + fakturInDb.MATERAI;

            ErasoftDbContext.SaveChanges();

            return new EmptyResult();
        }


        [HttpPost]
        public ActionResult UpdateReturFaktur(UpdateData dataUpdate)
        {
            var fakturInDb = ErasoftDbContext.SIT01A.Single(p => p.NO_BUKTI == dataUpdate.OrderId && p.JENIS_FORM == "3");
            fakturInDb.BRUTO = dataUpdate.Bruto;
            fakturInDb.NILAI_DISC = dataUpdate.NilaiDisc;
            fakturInDb.PPN = dataUpdate.Ppn;
            fakturInDb.NILAI_PPN = dataUpdate.NilaiPpn;
            fakturInDb.MATERAI = dataUpdate.OngkosKirim;
            fakturInDb.NETTO = fakturInDb.BRUTO - fakturInDb.NILAI_DISC + fakturInDb.NILAI_PPN + fakturInDb.MATERAI;

            ErasoftDbContext.SaveChanges();

            return new EmptyResult();
        }

        //add by nurul 16/11/2018 FakturViewModel dataVm
        [HttpGet]
        public ActionResult GetRecnumReturFaktur(string noUrut)
        {
            string a = (noUrut.Split('-')[noUrut.Split('-').Length - 1]);
            int urut = Convert.ToInt32(a);
            //var Recnum = ErasoftDbContext.SIT01B.Single(p => p.NO_BUKTI == dataVm.Faktur.NO_BUKTI).TRANS_NO_URUT;
            var Recnum = ErasoftDbContext.SIT01B.Single(b => b.NO_URUT == urut && b.JENIS_FORM == "3").TRANS_NO_URUT;

            return Json(Recnum, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetQtyReturFaktur(string param)
        {
            string order = (param.Split(';')[param.Split(';').Length - 3]);
            string brg = (param.Split(';')[param.Split(';').Length - 2]);
            Int32 recnumBrg = Convert.ToInt32(param.Split(';')[param.Split(';').Length - 1]);

            var res = new mdlGetQty()
            {
                OrderId = order,
                BrgId = brg,
                Recnum = recnumBrg
            };

            var spQTY = ErasoftDbContext.SIT01B.Single(p => p.NO_BUKTI == order && p.BRG == brg && p.NO_URUT == recnumBrg).QTY;

            return Json(spQTY, JsonRequestBehavior.AllowGet);
        }
        public class mdlGetQty
        {
            public string OrderId { get; set; }
            public string BrgId { get; set; }
            public Int32 Recnum { get; set; }
        }
        public ActionResult GetReturFaktur(string orderId)
        {
            var listDetail = ErasoftDbContext.SIT01B.Where(b => b.NO_BUKTI == orderId).ToList();
            var detail = listDetail.Count();

            return Json(detail, JsonRequestBehavior.AllowGet);
        }
        //end add 

        // =============================================== Bagian Faktur Penjualan (END)

        // =============================================== Bagian Pembelian Invoice (START)

        [HttpGet]
        public ActionResult GetInvoice()
        {
            var listInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "1").ToList();
            var listKodeInvoice = new List<InvoiceJson>();

            foreach (var invoice in listInvoice)
            {
                listKodeInvoice.Add(new InvoiceJson()
                {
                    RecNum = invoice.RecNum,
                    INV = invoice.INV
                });
            }

            return Json(listKodeInvoice, JsonRequestBehavior.AllowGet);
        }

        //[HttpGet]
        //public ActionResult GetInvoiceBySupp(string kodeSupplier)
        //{
        //    //change by nurul 5 / 11 / 2018
        //    var listInvoice = ErasoftDbContext.PBT01A
        //                        //change by nurul 5/11/2018  --  
        //                        .Where(f => f.JENISFORM == "1" && f.SUPP == kodeSupplier)
        //                        //.Where(f => f.JENISFORM == "1" && f.SUPP == kodeSupplier && (String.IsNullOrEmpty(f.REF) || f.REF == "-"))
        //                        .OrderBy(f => f.INV).ThenByDescending(f => f.TGLINPUT).ToList();

        //    //string sSQL = "";
        //    //sSQL += "SELECT * ";
        //    //sSQL += "FROM PBT01A A LEFT JOIN PBT01A B ON ";
        //    //sSQL += "A.JENISFORM = '1' ";
        //    //sSQL += "AND B.JENISFORM = '2' ";
        //    //sSQL += "AND A.INV = B.REF ";
        //    //sSQL += "WHERE ISNULL(B.INV, '') = '' ";
        //    //sSQL += "AND A.JENISFORM = '1' ";
        //    //sSQL += "AND A.SUPP = '" + kodeSupplier + "' ";
        //    //sSQL += "ORDER BY A.INV ASC, A.TGLINPUT DESC ";
        //    //var listInvoice = ErasoftDbContext.Database.SqlQuery<PBT01A>(sSQL).ToList();
        //    //end change 
        //    var listKodeInvoice = new List<InvoiceJson>();

        //    foreach (var invoice in listInvoice)
        //    {
        //        listKodeInvoice.Add(new InvoiceJson()
        //        {
        //            RecNum = invoice.RecNum,
        //            INV = invoice.INV
        //        });
        //    }

        //    return Json(listKodeInvoice, JsonRequestBehavior.AllowGet);
        //}

        [HttpGet]
        public ActionResult GetInvoiceBySupp(string kodeSupplier)
        {
            //change by nurul 5/11/2018
            var listInvoice = ErasoftDbContext.PBT01A
                                .Where(f => f.JENISFORM == "1" && f.SUPP == kodeSupplier)
                                .OrderBy(f => f.INV).ThenByDescending(f => f.TGLINPUT).ToList();
            ////end change 
            var listKodeInvoice = new List<InvoiceJson>();

            foreach (var invoice in listInvoice)
            {
                listKodeInvoice.Add(new InvoiceJson()
                {
                    RecNum = invoice.RecNum,
                    INV = invoice.INV
                });
            }

            return Json(listKodeInvoice, JsonRequestBehavior.AllowGet);
        }

        //add by nurul 5/11/2018
        [HttpGet]
        public ActionResult GetInvoiceBySuppNew(string kodeSupplier)
        {

            string sSQL = "";
            sSQL += "SELECT * ";
            sSQL += "FROM PBT01A A LEFT JOIN PBT01A B ON ";
            sSQL += "A.JENISFORM = '1' ";
            sSQL += "AND B.JENISFORM = '2' ";
            sSQL += "AND A.INV = B.REF ";
            sSQL += "WHERE ISNULL(B.INV, '') = '' ";
            sSQL += "AND A.JENISFORM = '1' ";
            sSQL += "AND A.SUPP = '" + kodeSupplier + "' ";
            sSQL += "ORDER BY A.INV ASC, A.TGLINPUT DESC ";
            var listInvoice = ErasoftDbContext.Database.SqlQuery<PBT01A>(sSQL).ToList();
            //end change 
            var listKodeInvoice = new List<InvoiceJson>();

            foreach (var invoice in listInvoice)
            {
                listKodeInvoice.Add(new InvoiceJson()
                {
                    RecNum = invoice.RecNum,
                    INV = invoice.INV
                });
            }

            return Json(listKodeInvoice, JsonRequestBehavior.AllowGet);
        }
        //end add

        [HttpGet]
        public ActionResult GetListPesanan()
        {
            var listPesanan = ErasoftDbContext.SOT01A.ToList();

            return Json(listPesanan, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveInvoice(InvoiceViewModel dataVm)
        {
            if (!ModelState.IsValid)
            {
                dataVm.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(dataVm, JsonRequestBehavior.AllowGet);
            }

            if (dataVm.Invoice.RecNum == null)
            {
                var listInvoiceInDb = ErasoftDbContext.PBT01A.OrderBy(p => p.RecNum).ToList();
                var digitAkhir = "";
                var noOrder = "";

                if (listInvoiceInDb.Count == 0)
                {
                    digitAkhir = "000001";
                    noOrder = $"PB{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                    ErasoftDbContext.Database.ExecuteSqlCommand("DBCC CHECKIDENT (PBT01A, RESEED, 0)");
                }
                else
                {
                    var lastRecNum = listInvoiceInDb.Last().RecNum;
                    lastRecNum++;

                    digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                    noOrder = $"PB{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                }

                dataVm.Invoice.INV = noOrder;
                dataVm.Invoice.F_PAJAK = "";
                dataVm.Invoice.NAMA = ErasoftDbContext.APF01.Single(p => p.SUPP == dataVm.Invoice.SUPP).NAMA;
                dataVm.Invoice.PPN_Bln_Lapor = Convert.ToByte(dataVm.Invoice.TGL?.ToString("MM") ?? "0");
                dataVm.Invoice.PPN_Thn_Lapor = Convert.ToByte(dataVm.Invoice.TGL?.ToString("yyyy").Substring(2, 2) ?? "0");

                dataVm.InvoiceDetail.INV = noOrder;

                ErasoftDbContext.PBT01A.Add(dataVm.Invoice);
                ErasoftDbContext.SaveChanges();

                if (dataVm.InvoiceDetail.NO == null)
                {
                    ErasoftDbContext.PBT01B.Add(dataVm.InvoiceDetail);
                    ErasoftDbContext.PBT01A.Where(p => p.INV == noOrder && p.JENISFORM == "1").Update(p => new PBT01A() { BRUTO = dataVm.Invoice.BRUTO });
                }
            }
            else
            {
                var invoiceInDb = ErasoftDbContext.PBT01A.Single(p => p.INV == dataVm.Invoice.INV && p.JENISFORM == "1");

                invoiceInDb.NETTO = dataVm.Invoice.NETTO;
                invoiceInDb.BRUTO = dataVm.Invoice.BRUTO;
                invoiceInDb.NDISC1 = dataVm.Invoice.NDISC1;
                invoiceInDb.PPN = dataVm.Invoice.PPN;
                invoiceInDb.NPPN = dataVm.Invoice.NPPN;
                //ADD BY NURUL 7/12/2018
                invoiceInDb.BIAYA_LAIN = dataVm.Invoice.BIAYA_LAIN;
                //END ADD
                invoiceInDb.NILAI_PPN = dataVm.Invoice.NILAI_PPN;
                invoiceInDb.KODE_REF_PESANAN = dataVm.Invoice.KODE_REF_PESANAN;

                dataVm.InvoiceDetail.INV = dataVm.Invoice.INV;

                if (dataVm.InvoiceDetail.NO == null)
                {
                    ErasoftDbContext.PBT01B.Add(dataVm.InvoiceDetail);
                }
            }

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            //add by calvin 8 nov 2018, update stok marketplace
            List<string> listBrg = new List<string>();
            listBrg.Add(dataVm.InvoiceDetail.BRG);
            updateStockMarketPlace(listBrg);
            //end add by calvin 8 nov 2018

            var vm = new InvoiceViewModel()
            {
                Invoice = ErasoftDbContext.PBT01A.Single(p => p.INV == dataVm.Invoice.INV && p.JENISFORM == "1"),
                ListInvoiceDetail = ErasoftDbContext.PBT01B.Where(pd => pd.INV == dataVm.Invoice.INV && pd.JENISFORM == "1").ToList(),
                //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList()
            };

            return PartialView("BarangInvoicePartial", vm);
        }

        public ActionResult SaveReturInvoice(InvoiceViewModel dataVm)
        {

            //if (!ModelState.IsValid)
            //{
            //    dataVm.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
            //    return Json(dataVm, JsonRequestBehavior.AllowGet);
            //}

            bool returBaru = false;

            if (dataVm.Invoice.RecNum == null)
            {
                var cekREf = ErasoftDbContext.PBT01A.SingleOrDefault(f => f.REF == dataVm.Invoice.REF);
                if (cekREf == null)
                {
                    var listInvoiceInDb = ErasoftDbContext.PBT01A.OrderBy(p => p.RecNum).ToList();
                    var digitAkhir = "";
                    var noOrder = "";

                    if (listInvoiceInDb.Count == 0)
                    {
                        digitAkhir = "000001";
                        noOrder = $"RB{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                        ErasoftDbContext.Database.ExecuteSqlCommand("DBCC CHECKIDENT (PBT01A, RESEED, 0)");
                    }
                    else
                    {
                        var lastRecNum = listInvoiceInDb.Last().RecNum;
                        lastRecNum++;

                        digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                        noOrder = $"RB{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                    }

                    //add by calvin, validasi QOH
                    var invoiceDetailInDb = ErasoftDbContext.PBT01B.Where(b => b.INV == dataVm.Invoice.REF).ToList();
                    foreach (var item in invoiceDetailInDb)
                    {
                        var qtyOnHand = GetQOHSTF08A(item.BRG, item.GD);

                        if (qtyOnHand - item.QTY < 0)
                        {
                            var vmError = new InvoiceViewModel()
                            {

                            };
                            vmError.Errors.Add("Tidak bisa retur, Qty untuk barang ( " + item.BRG + " ) di gudang " + item.GD + " sisa ( " + Convert.ToString(qtyOnHand) + " ).");
                            return Json(vmError, JsonRequestBehavior.AllowGet);
                        }
                    }
                    //end add by calvin, validasi QOH

                    var returInDb = ErasoftDbContext.PBT01A.SingleOrDefault(f => f.INV == dataVm.Invoice.REF);
                    if (returInDb != null)
                    {
                        dataVm.Invoice.TERM = returInDb.TERM;
                        dataVm.Invoice.TGJT = returInDb.TGJT;
                        dataVm.Invoice.BRUTO = returInDb.BRUTO;
                        dataVm.Invoice.NETTO = returInDb.NETTO;
                        //add by nurul 10/12/2018
                        dataVm.Invoice.PPN = returInDb.PPN;
                        dataVm.Invoice.NPPN = returInDb.NPPN;
                        dataVm.Invoice.NDISC1 = returInDb.NDISC1;
                        dataVm.Invoice.BIAYA_LAIN = returInDb.BIAYA_LAIN;
                        //end add 
                    }

                    //var recNumCust = ParseInt(dataVm.Invoice.SUPP);
                    dataVm.Invoice.INV = noOrder;
                    dataVm.Invoice.F_PAJAK = "";
                    dataVm.Invoice.NAMA = ErasoftDbContext.APF01.Single(p => p.SUPP == dataVm.Invoice.SUPP).NAMA;
                    dataVm.Invoice.PPN_Bln_Lapor = Convert.ToByte(dataVm.Invoice.TGL?.ToString("MM") ?? "0");
                    dataVm.Invoice.PPN_Thn_Lapor = Convert.ToByte(dataVm.Invoice.TGL?.ToString("yyyy").Substring(2, 2) ?? "0");

                    //dataVm.InvoiceDetail.INV = noOrder;

                    ErasoftDbContext.PBT01A.Add(dataVm.Invoice);

                    //if (dataVm.InvoiceDetail.NO == null)
                    //{
                    //    ErasoftDbContext.PBT01B.Add(dataVm.InvoiceDetail);
                    //}
                    returBaru = true;
                }
            }
            else
            {
                //add by calvin 16 nov 2018, cek jika tidak ada detail, autoload
                var cekdetail = ErasoftDbContext.PBT01B.FirstOrDefault(p => p.INV == dataVm.Invoice.INV);
                if (cekdetail != null)
                {
                    var invoiceInDb = ErasoftDbContext.PBT01A.Single(p => p.INV == dataVm.Invoice.INV && p.JENISFORM == "2");

                    //UPDATE ANAK
                    var invDetailDb = ErasoftDbContext.PBT01B.Single(p => p.INV == dataVm.Invoice.INV && p.BRG == dataVm.InvoiceDetail.BRG);

                    //add by calvin, validasi QOH
                    var qtyOnHand = GetQOHSTF08A(invDetailDb.BRG, invDetailDb.GD);

                    if (qtyOnHand + invDetailDb.QTY - dataVm.InvoiceDetail.QTY < 0)
                    {
                        var vmError = new InvoiceViewModel()
                        {

                        };
                        vmError.Errors.Add("Tidak bisa retur, Qty untuk barang ( " + invDetailDb.BRG + " ) di gudang " + invDetailDb.GD + " sisa ( " + Convert.ToString(qtyOnHand + invDetailDb.QTY) + " ).");
                        return Json(vmError, JsonRequestBehavior.AllowGet);
                    }
                    //end add by calvin, validasi QOH

                    invDetailDb.QTY = dataVm.InvoiceDetail.QTY;
                    invDetailDb.NILAI_DISC_1 = dataVm.InvoiceDetail.NILAI_DISC_1;
                    invDetailDb.NILAI_DISC_2 = dataVm.InvoiceDetail.NILAI_DISC_2;
                    invDetailDb.THARGA = (dataVm.InvoiceDetail.QTY) * (invDetailDb.HBELI) - (invDetailDb.NILAI_DISC_1 + invDetailDb.NILAI_DISC_2);

                    //UPDATE BAPAK
                    invoiceInDb.NETTO = dataVm.Invoice.NETTO;
                    invoiceInDb.BRUTO = dataVm.Invoice.BRUTO;
                    invoiceInDb.NDISC1 = dataVm.Invoice.NDISC1;
                    invoiceInDb.PPN = dataVm.Invoice.PPN;
                    //invoiceInDb.NILAI_PPN = dataVm.Invoice.NILAI_PPN;
                    invoiceInDb.NPPN = dataVm.Invoice.NPPN;
                    //add by nurul 10/12/2018
                    invoiceInDb.BIAYA_LAIN = dataVm.Invoice.BIAYA_LAIN;
                    //end add

                    //dataVm.InvoiceDetail.INV = dataVm.Invoice.INV;
                    //if (dataVm.InvoiceDetail.NO == null)
                    //{
                    //    ErasoftDbContext.PBT01B.Add(dataVm.InvoiceDetail);
                    //}

                    returBaru = false;
                }
                else
                {
                    returBaru = true;
                }
            }

            ErasoftDbContext.SaveChanges();

            // autoload detail item, jika buat retur baru
            if (returBaru)
            {
                object[] spParams = {
                new SqlParameter("@NOBUK",dataVm.Invoice.INV),
                new SqlParameter("@NO_REF",dataVm.Invoice.REF)
                };

                ErasoftDbContext.Database.ExecuteSqlCommand("exec [SP_AUTOLOADRETUR_PEMBELIAN] @NOBUK, @NO_REF", spParams);

                //add by calvin 8 nov 2018, update stok marketplace
                List<string> listBrg = new List<string>();
                var detailReturInvoiceInDb = ErasoftDbContext.PBT01B.AsNoTracking().Where(pd => pd.INV == dataVm.Invoice.INV && pd.JENISFORM == "2").ToList();
                foreach (var item in detailReturInvoiceInDb)
                {
                    listBrg.Add(item.BRG);
                }
                updateStockMarketPlace(listBrg);
                //end add by calvin 8 nov 2018
            }
            else
            {
                //add by calvin 8 nov 2018, update stok marketplace
                List<string> listBrg = new List<string>();
                listBrg.Add(dataVm.InvoiceDetail.BRG);
                updateStockMarketPlace(listBrg);
                //end add by calvin 8 nov 2018
            }
            ModelState.Clear();

            var vm = new InvoiceViewModel()
            {
                //Invoice = ErasoftDbContext.PBT01A.Single(p => p.INV == dataVm.Invoice.INV && p.JENISFORM == "2"),
                Invoice = ErasoftDbContext.PBT01A.AsNoTracking().Single(p => p.INV == dataVm.Invoice.INV && p.JENISFORM == "2"),
                ListInvoiceDetail = ErasoftDbContext.PBT01B.AsNoTracking().Where(pd => pd.INV == dataVm.Invoice.INV && pd.JENISFORM == "2").ToList(),
                //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList()
            };

            return PartialView("BarangReturInvoicePartial", vm);
        }

        public ActionResult EditReturInvoice(int? orderId)
        {
            var InvoiceInDB = ErasoftDbContext.PBT01A.Single(p => p.RecNum == orderId && p.JENISFORM == "2");

            var vm = new InvoiceViewModel()
            {
                Invoice = ErasoftDbContext.PBT01A.Single(p => p.INV == InvoiceInDB.INV && p.JENISFORM == "2"),
                ListInvoiceDetail = ErasoftDbContext.PBT01B.Where(pd => pd.INV == InvoiceInDB.INV && pd.JENISFORM == "2").ToList(),
                //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList()
            };

            return PartialView("BarangReturInvoicePartial", vm);
        }
        public ActionResult RefreshTableInvoice1()
        {
            var vm = new InvoiceViewModel()
            {
                ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "1").ToList(),
                //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListNInvoice = ErasoftDbContext.APT03B.ToList()
            };

            return PartialView("TableInvoicePartial", vm);
        }


        public ActionResult RefreshTableInvoiceLunas()
        {
            IEnumerable<APT01D> InvBelumLunas = ErasoftDbContext.APT01D.Where(a => a.NETTO - a.DEBET > 0);
            var vm = new InvoiceViewModel()
            {
                ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "1" && InvBelumLunas.Any(a => a.INV == f.INV)).ToList(),
                //ListBarang = ErasoftDbContext.STF02.ToList(), change by nurul 21/1/2019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListNInvoice = ErasoftDbContext.APT03B.ToList()
            };

            return PartialView("TableInvoiceLunasPartial", vm);
        }

        public ActionResult RefreshTableInvoiceTempo(string tgl)
        {
            //add by nurul 10/1/2019
            var tanggal = DateTime.ParseExact(tgl, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            //end add 
            var vm = new InvoiceViewModel()
            {
                //change by nurul 10/1/2019 -- ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "1" && f.TGJT <= DateTime.Now).ToList(),
                ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "1" && f.TGJT <= tanggal).ToList(),
                //end change 
                //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListNInvoice = ErasoftDbContext.APT03B.ToList()
            };

            return PartialView("TableInvoiceLunasPartial", vm);
        }

        public ActionResult RefreshTableReturInvoice1()
        {
            var vm = new InvoiceViewModel()
            {
                ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "2").ToList(),
                //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListNInvoice = ErasoftDbContext.APT03B.ToList()
            };

            return PartialView("TableReturInvoicePartial", vm);
        }

        public ActionResult RefreshInvoiceForm()
        {
            try
            {
                var vm = new InvoiceViewModel()
                {
                    ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "1").ToList(),
                    //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList()
                };

                return PartialView("BarangInvoicePartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult RefreshReturInvoiceForm()
        {
            try
            {
                var vm = new InvoiceViewModel()
                {
                    ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "2").ToList(),
                    //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList()
                };

                return PartialView("BarangReturInvoicePartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult GetDataInvoice(int? recNum)
        {
            try
            {
                var invoiceInDb = ErasoftDbContext.PBT01A.Single(p => p.RecNum == recNum);

                var vm = new InvoiceViewModel()
                {
                    Invoice = invoiceInDb,
                    ListInvoice = ErasoftDbContext.PBT01A.ToList(),
                    ListInvoiceDetail = ErasoftDbContext.PBT01B.Where(pd => pd.INV == invoiceInDb.INV && pd.JENISFORM == "1").ToList(),
                    //ListBarang = ErasoftDbContext.STF02.ToList() 'change by nurul 21/1/2019 
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList()
                };

                return PartialView("BarangReturInvoicePartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult EditInvoice(int? orderId)
        {
            try
            {
                var invoiceInDb = ErasoftDbContext.PBT01A.Single(p => p.RecNum == orderId && p.JENISFORM == "1");

                var vm = new InvoiceViewModel()
                {
                    Invoice = invoiceInDb,
                    ListInvoice = ErasoftDbContext.PBT01A.ToList(),
                    ListInvoiceDetail = ErasoftDbContext.PBT01B.Where(pd => pd.INV == invoiceInDb.INV && pd.JENISFORM == "1").ToList(),
                    //ListBarang = ErasoftDbContext.STF02.ToList() 'change by nurul 21/1/2019
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList()
                };

                return PartialView("BarangInvoicePartial", vm);
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        public ActionResult DeleteInvoice(int? orderId)
        {
            var invoiceInDb = ErasoftDbContext.PBT01A.Single(p => p.RecNum == orderId && p.JENISFORM == "1");

            //add by calvin 8 nov 2018, update stok marketplace
            List<string> listBrg = new List<string>();
            //end add by calvin 8 nov 2018

            //add by calvin, validasi QOH
            var invoiceDetailInDb = ErasoftDbContext.PBT01B.Where(b => b.INV == invoiceInDb.INV && b.JENISFORM == "1").ToList();
            foreach (var item in invoiceDetailInDb)
            {
                var qtyOnHand = GetQOHSTF08A(item.BRG, item.GD);

                if (qtyOnHand - item.QTY < 0)
                {
                    var vmError = new InvoiceViewModel()
                    {

                    };
                    vmError.Errors.Add("Tidak bisa delete, Qty di gudang sisa ( " + Convert.ToString(qtyOnHand) + " )");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
                //add by calvin 8 nov 2018, update stok marketplace
                listBrg.Add(item.BRG);
                //end add by calvin 8 nov 2018
            }
            //end add by calvin, validasi QOH

            ErasoftDbContext.PBT01A.Remove(invoiceInDb);
            ErasoftDbContext.SaveChanges();

            //add by calvin 8 nov 2018, update stok marketplace
            updateStockMarketPlace(listBrg);
            //end add by calvin 8 nov 2018

            var vm = new InvoiceViewModel()
            {
                ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "1").ToList(),
                //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListNInvoice = ErasoftDbContext.APT03B.ToList()
            };

            return PartialView("TableInvoicePartial", vm);
        }

        public ActionResult DeleteReturInvoice(int? orderId)
        {
            var invoiceInDb = ErasoftDbContext.PBT01A.Single(p => p.RecNum == orderId && p.JENISFORM == "2");

            //add by calvin 8 nov 2018, update stok marketplace
            List<string> listBrg = new List<string>();
            var detailReturInvoiceInDb = ErasoftDbContext.PBT01B.Where(pd => pd.INV == invoiceInDb.INV && pd.JENISFORM == "2").ToList();
            foreach (var item in detailReturInvoiceInDb)
            {
                listBrg.Add(item.BRG);
            }
            //end add by calvin 8 nov 2018

            ErasoftDbContext.PBT01A.Remove(invoiceInDb);
            ErasoftDbContext.SaveChanges();

            //add by calvin 8 nov 2018, update stok marketplace
            updateStockMarketPlace(listBrg);
            //end add by calvin 8 nov 2018

            var vm = new InvoiceViewModel()
            {
                ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "2").ToList(),
                //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListNInvoice = ErasoftDbContext.APT03B.ToList()
            };

            return PartialView("TableReturInvoicePartial", vm);
        }

        [HttpGet]
        public ActionResult DeleteBarangInvoice(int noUrut)
        {
            try
            {
                var barangInvoiceInDb = ErasoftDbContext.PBT01B.Single(b => b.NO == noUrut && b.JENISFORM == "1");
                var invoiceInDb = ErasoftDbContext.PBT01A.Single(p => p.INV == barangInvoiceInDb.INV && p.JENISFORM == "1");

                //add by calvin, validasi QOH
                var qtyOnHand = GetQOHSTF08A(barangInvoiceInDb.BRG, barangInvoiceInDb.GD);

                if (qtyOnHand - barangInvoiceInDb.QTY < 0)
                {
                    var vmError = new InvoiceViewModel()
                    {

                    };
                    vmError.Errors.Add("Tidak bisa delete, Qty di gudang sisa ( " + Convert.ToString(qtyOnHand) + " )");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
                //end add by calvin, validasi QOH

                invoiceInDb.BRUTO -= barangInvoiceInDb.THARGA;
                //invoiceInDb.NILAI_PPN = Math.Ceiling((double)invoiceInDb.PPN * (double)invoiceInDb.BRUTO / 100);
                invoiceInDb.NPPN = Math.Ceiling((double)invoiceInDb.PPN * (double)invoiceInDb.BRUTO / 100);
                //change by nurul 10/12/2018 -- invoiceInDb.NETTO = invoiceInDb.BRUTO - invoiceInDb.NDISC1 + invoiceInDb.NILAI_PPN;
                invoiceInDb.NETTO = invoiceInDb.BRUTO - invoiceInDb.NDISC1 + invoiceInDb.NPPN + invoiceInDb.BIAYA_LAIN;

                ErasoftDbContext.PBT01B.Remove(barangInvoiceInDb);
                ErasoftDbContext.SaveChanges();

                var vm = new InvoiceViewModel()
                {
                    Invoice = ErasoftDbContext.PBT01A.Single(p => p.INV == invoiceInDb.INV && p.JENISFORM == "1"),
                    ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "2").ToList(),
                    ListInvoiceDetail = ErasoftDbContext.PBT01B.Where(pd => pd.INV == invoiceInDb.INV && pd.JENISFORM == "1").ToList(),
                    //ListBarang = ErasoftDbContext.STF02.ToList() 'change by nurul 21/1/2019
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList()
                };

                //add by calvin 8 nov 2018, update stok marketplace
                List<string> listBrg = new List<string>();
                listBrg.Add(barangInvoiceInDb.BRG);
                updateStockMarketPlace(listBrg);
                //end add by calvin 8 nov 2018

                return PartialView("BarangInvoicePartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpGet]
        public ActionResult DeleteBarangReturInvoice(int noUrut)
        {
            try
            {
                var barangInvoiceInDb = ErasoftDbContext.PBT01B.Single(b => b.NO == noUrut && b.JENISFORM == "2");
                var invoiceInDb = ErasoftDbContext.PBT01A.Single(p => p.INV == barangInvoiceInDb.INV && p.JENISFORM == "2");

                invoiceInDb.BRUTO -= barangInvoiceInDb.THARGA;
                //invoiceInDb.NILAI_PPN = Math.Ceiling((double)invoiceInDb.PPN * (double)invoiceInDb.BRUTO / 100);
                invoiceInDb.NPPN = Math.Ceiling((double)invoiceInDb.PPN * (double)invoiceInDb.BRUTO / 100);
                //change by nurul 10/12/2018 -- invoiceInDb.NETTO = invoiceInDb.BRUTO - invoiceInDb.NDISC1 + invoiceInDb.NILAI_PPN;
                invoiceInDb.NETTO = invoiceInDb.BRUTO - invoiceInDb.NDISC1 + invoiceInDb.NPPN + invoiceInDb.BIAYA_LAIN;

                ErasoftDbContext.PBT01B.Remove(barangInvoiceInDb);
                ErasoftDbContext.SaveChanges();

                //add by calvin 8 nov 2018, update stok marketplace
                List<string> listBrg = new List<string>();
                listBrg.Add(barangInvoiceInDb.BRG);
                updateStockMarketPlace(listBrg);
                //end add by calvin 8 nov 2018

                var vm = new InvoiceViewModel()
                {
                    Invoice = ErasoftDbContext.PBT01A.AsNoTracking().Single(p => p.INV == invoiceInDb.INV && p.JENISFORM == "2"),
                    ListInvoice = ErasoftDbContext.PBT01A.AsNoTracking().Where(f => f.JENISFORM == "2").ToList(),
                    ListInvoiceDetail = ErasoftDbContext.PBT01B.AsNoTracking().Where(pd => pd.INV == invoiceInDb.INV && pd.JENISFORM == "2").ToList(),
                    //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList()

                };

                return PartialView("BarangReturInvoicePartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult UpdateInvoice(UpdateData dataUpdate)
        {
            var invoiceInDb = ErasoftDbContext.PBT01A.Single(p => p.INV == dataUpdate.OrderId);
            invoiceInDb.BRUTO = dataUpdate.Bruto;
            invoiceInDb.NDISC1 = dataUpdate.NilaiDisc;
            invoiceInDb.PPN = dataUpdate.Ppn;
            //change by nurul 16/11/2018 -- invoiceInDb.NPPN = dataUpdate.Bruto * (invoiceInDb.PPN / 100);
            invoiceInDb.NPPN = dataUpdate.NilaiPpn;
            //end change 
            //ADD BY NURUL 7/12/2018
            invoiceInDb.BIAYA_LAIN = dataUpdate.OngkosKirim;
            //END ADD
            invoiceInDb.KODE_REF_PESANAN = dataUpdate.KodeRefPesanan;
            invoiceInDb.TGL = DateTime.ParseExact(dataUpdate.Tgl.Substring(0, 10), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            invoiceInDb.SUPP = dataUpdate.Supp;
            invoiceInDb.TERM = dataUpdate.TermInvoice;
            invoiceInDb.NAMA = ErasoftDbContext.APF01.Single(s => s.SUPP == dataUpdate.Supp).NAMA;
            invoiceInDb.TGJT = DateTime.ParseExact(dataUpdate.Tempo, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            //CHANGE BY NURUL 7/12/2018 -- invoiceInDb.NETTO = invoiceInDb.BRUTO - invoiceInDb.NDISC1 + invoiceInDb.NPPN;
            invoiceInDb.NETTO = invoiceInDb.BRUTO - invoiceInDb.NDISC1 + invoiceInDb.NPPN + invoiceInDb.BIAYA_LAIN;
            //END CHANGE 

            ErasoftDbContext.SaveChanges();

            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult UpdateReturInvoice(UpdateData dataUpdate)
        {
            var invoiceInDb = ErasoftDbContext.PBT01A.Single(p => p.INV == dataUpdate.OrderId);
            invoiceInDb.BRUTO = dataUpdate.Bruto;
            invoiceInDb.NDISC1 = dataUpdate.NilaiDisc;
            invoiceInDb.PPN = dataUpdate.Ppn;
            //change by nurul 6/11/2018 -- invoiceInDb.NPPN = dataUpdate.Bruto * (invoiceInDb.PPN / 100);
            invoiceInDb.NPPN = ((dataUpdate.Bruto - invoiceInDb.NDISC1) * invoiceInDb.PPN / 100);
            //invoiceInDb.KODE_REF_PESANAN = dataUpdate.KodeRefPesanan;
            //add by nurul 10/12/2018
            invoiceInDb.BIAYA_LAIN = dataUpdate.OngkosKirim;
            //end add
            //change by nurul 10/12/2018 -- invoiceInDb.NETTO = invoiceInDb.BRUTO - invoiceInDb.NDISC1 + invoiceInDb.NPPN;
            invoiceInDb.NETTO = invoiceInDb.BRUTO - invoiceInDb.NDISC1 + invoiceInDb.NPPN + invoiceInDb.BIAYA_LAIN;
            //end change 

            ErasoftDbContext.SaveChanges();

            return new EmptyResult();
        }

        //add by nurul 16/11/2018
        [HttpGet]
        public ActionResult GetRecnumReturInvoice(string noUrut)
        {
            string a = (noUrut.Split('-')[noUrut.Split('-').Length - 1]);
            int urut = Convert.ToInt32(a);
            var Recnum = ErasoftDbContext.PBT01B.Single(p => p.NO == urut && p.JENISFORM == "2").NO_URUT_PO;

            return Json(Recnum, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetQtyReturInvoice(string param)
        {
            string order = (param.Split(';')[param.Split(';').Length - 3]);
            string brg = (param.Split(';')[param.Split(';').Length - 2]);
            Int32 recnumBrg = Convert.ToInt32(param.Split(';')[param.Split(';').Length - 1]);

            var res = new mdlGetQty()
            {
                OrderId = order,
                BrgId = brg,
                Recnum = recnumBrg
            };

            var spQTY = ErasoftDbContext.PBT01B.Single(p => p.INV == order && p.BRG == brg && p.NO == recnumBrg).QTY;

            return Json(spQTY, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetReturInvoice(string orderId)
        {
            var listDetail = ErasoftDbContext.PBT01B.Where(b => b.INV == orderId).ToList();
            var detail = listDetail.Count();

            return Json(detail, JsonRequestBehavior.AllowGet);
        }
        //end add 

        // =============================================== Bagian Pembelian Invoice (END)

        // =============================================== Bagian Pesanan (START)

        [HttpGet]
        public ActionResult GetPelanggan()
        {
            var listPelanggan = MoDbContext.Marketplaces.ToList();

            return Json(listPelanggan, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPelangganAkun()
        {
            var listPelanggan = ErasoftDbContext.ARF01.OrderBy(m => m.NAMA).ToList();

            return Json(listPelanggan, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetPelangganAkunTokpedShopee()
        {
            var listPelanggan = ErasoftDbContext.ARF01.OrderBy(m => m.NAMA).Where(m => m.NAMA == "15" || m.NAMA == "17").ToList();

            return Json(listPelanggan, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPembeli()
        {
            var listPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList();

            return Json(listPembeli, JsonRequestBehavior.AllowGet);
        }

        //add by nurul 4/12/2018
        [HttpGet]
        public ActionResult GetPembeliPesanan(string kode)
        {
            //var listPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList();
            var pembeli = ErasoftDbContext.ARF01C.Single(x => x.BUYER_CODE == kode);

            return Json(pembeli, JsonRequestBehavior.AllowGet);
        }
        //end add 

        [HttpGet]
        public ActionResult GetDataBarangPesanan(string code)
        {
            //var listBarang = ErasoftDbContext.STF02.ToList();
            var listBarang = (from a in ErasoftDbContext.STF02
                              join b in ErasoftDbContext.STF02H on a.BRG equals b.BRG
                              join c in ErasoftDbContext.ARF01 on b.IDMARKET equals c.RecNum
                              //change by nurul 21/1/2019 -- where c.CUST == code
                              where c.CUST == code && a.TYPE == "3"
                              select new { BRG = a.BRG, NAMA = a.NAMA, NAMA2 = a.NAMA2 == null ? "" : a.NAMA2, STN2 = a.STN2, HJUAL = b.HJUAL });

            return Json(listBarang, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDataBarang(string code)
        {
            //var listBarang = ErasoftDbContext.STF02.ToList(); 'change by nurul 21/1/2019 
            var listBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList();

            return Json(listBarang, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetDataBarangPromosi(int? promoId)
        {
            //var listBarang = ErasoftDbContext.STF02.ToList(); 'change by nurul 21/1/2019 
            var listBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList();

            if (promoId == null)
            {
                return Json(listBarang, JsonRequestBehavior.AllowGet);
            }

            var listBarangSesuaiPromo = ErasoftDbContext.DETAILPROMOSI.Where(dp => dp.RecNumPromosi == promoId).ToList();
            List<STF02> listBarangUntukPromo = null;

            if (listBarangSesuaiPromo != null && listBarangSesuaiPromo.Count > 0)
            {
                listBarangUntukPromo = listBarang.Where(b => !listBarangSesuaiPromo.Any(bp => bp.KODE_BRG == b.BRG)).ToList();
            }
            else
            {
                return Json(listBarang, JsonRequestBehavior.AllowGet);
            }

            return Json(listBarangUntukPromo, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetEkspedisi()
        {
            var listEkspedisi = MoDbContext.Ekspedisi.ToList();

            return Json(listEkspedisi, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        //change by nurul 21/2/2019
        //public ActionResult CekJumlahPesananBulanIni(string uname)
        public ActionResult CekJumlahPesananBulanIni(long accId)
        {
            var listPesanan = ErasoftDbContext.SOT01A.ToList();
            var jumlahPesananBulanIni = listPesanan.Count(p => p.TGL?.Month == DateTime.Today.Month);
            var accInDb = MoDbContext.Account.FirstOrDefault(a => a.AccountId == accId);

            if (accInDb == null)
            {
                var accIdByUser = MoDbContext.User.FirstOrDefault(u => u.AccountId == accId)?.AccountId;
                accInDb = MoDbContext.Account.FirstOrDefault(a => a.AccountId == accIdByUser);
            }

            var accSubs = MoDbContext.Subscription.FirstOrDefault(s => s.KODE == accInDb.KODE_SUBSCRIPTION);

            var valSubs = new ValidasiSubs()
            {
                JumlahPesananBulanIni = jumlahPesananBulanIni,
                JumlahPesananMax = accSubs?.JUMLAH_PESANAN,
                //change by nurul 8/2/2019
                //SudahSampaiBatasTanggal = (accInDb?.TGL_SUBSCRIPTION <= DateTime.Today.Date && accInDb.KODE_SUBSCRIPTION != "01")
                SudahSampaiBatasTanggal = (accInDb?.TGL_SUBSCRIPTION <= DateTime.Today.Date)
                //en change by nurul 8/2/2019
            };

            return Json(valSubs, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPesananInfo(string nobuk)
        {
            var pesananInDb = ErasoftDbContext.SOT01A.Single(p => p.NO_BUKTI == nobuk);
            var pesananDetailInDb = ErasoftDbContext.SOT01B.FirstOrDefault(p => p.NO_BUKTI == nobuk && p.BRG == "NOT_FOUND");
            var marketInDb = ErasoftDbContext.ARF01.Single(m => m.CUST == pesananInDb.CUST);
            var idMarket = Convert.ToInt32(marketInDb.NAMA);
            var namaMarketplace = MoDbContext.Marketplaces.Single(m => m.IdMarket == idMarket).NamaMarket;
            var namaAkunMarket = $"{namaMarketplace} ({marketInDb.PERSO})";
            var namaBuyer = ErasoftDbContext.ARF01C.SingleOrDefault(b => b.BUYER_CODE == pesananInDb.PEMESAN).NAMA;

            var infoPesanan = new InfoPesanan()
            {
                NoPesanan = pesananInDb.NO_BUKTI,
                TglPesanan = pesananInDb.TGL?.ToString("dd/MM/yyyy"),
                Marketplace = namaAkunMarket,
                Pembeli = namaBuyer,
                Total = String.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:N}", pesananInDb.NETTO),
                allowContinue = pesananDetailInDb == null ? 1 : 0
            };

            return Json(infoPesanan, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetAutoloadFaktur(int recNum)
        {
            //var fakturInDb = ErasoftDbContext.SIT01A.Single(p => p.RecNum == recNum);
            var listData = ErasoftDbContext.SIT01B.Where(pd => pd.SIT01A.RecNum == recNum && pd.JENIS_FORM == "2").ToList();

            return Json(listData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SavePesanan(PesananViewModel dataVm)
        {
            if (!ModelState.IsValid)
            {
                dataVm.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(dataVm, JsonRequestBehavior.AllowGet);
            }

            if (dataVm.Pesanan.RecNum == null)
            {
                var listPesananInDb = ErasoftDbContext.SOT01A.OrderBy(p => p.RecNum).ToList();
                var digitAkhir = "";
                var noOrder = "";

                if (listPesananInDb.Count == 0)
                {
                    digitAkhir = "000001";
                    noOrder = $"SO{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                    ErasoftDbContext.Database.ExecuteSqlCommand("DBCC CHECKIDENT (SOT01A, RESEED, 0)");
                }
                else
                {
                    var lastRecNum = listPesananInDb.Last().RecNum;
                    lastRecNum++;

                    digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                    noOrder = $"SO{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                }

                dataVm.Pesanan.NO_BUKTI = noOrder;
                dataVm.Pesanan.STATUS_TRANSAKSI = "0";
                dataVm.PesananDetail.NO_BUKTI = noOrder;
                dataVm.PesananDetail.NILAI_DISC = dataVm.PesananDetail.NILAI_DISC_1 + dataVm.PesananDetail.NILAI_DISC_2;

                ErasoftDbContext.SOT01A.Add(dataVm.Pesanan);

                if (dataVm.PesananDetail.NO_URUT == null)
                {
                    ErasoftDbContext.SOT01B.Add(dataVm.PesananDetail);
                }
            }
            else
            {
                var pesananInDb = ErasoftDbContext.SOT01A.Single(p => p.NO_BUKTI == dataVm.Pesanan.NO_BUKTI);

                pesananInDb.NETTO = dataVm.Pesanan.NETTO;
                pesananInDb.BRUTO = dataVm.Pesanan.BRUTO;
                pesananInDb.DISCOUNT = dataVm.Pesanan.DISCOUNT;
                pesananInDb.PPN = dataVm.Pesanan.PPN;
                pesananInDb.NILAI_PPN = dataVm.Pesanan.NILAI_PPN;
                pesananInDb.ONGKOS_KIRIM = dataVm.Pesanan.ONGKOS_KIRIM;
                pesananInDb.ALAMAT_KIRIM = dataVm.Pesanan.ALAMAT_KIRIM;
                pesananInDb.TERM = dataVm.Pesanan.TERM;
                pesananInDb.TGL_JTH_TEMPO = dataVm.Pesanan.TGL_JTH_TEMPO;
                pesananInDb.CUST = dataVm.Pesanan.CUST;
                pesananInDb.PEMESAN = dataVm.Pesanan.PEMESAN;
                pesananInDb.NAMAPEMESAN = dataVm.Pesanan.NAMAPEMESAN;

                dataVm.PesananDetail.NO_BUKTI = dataVm.Pesanan.NO_BUKTI;
                dataVm.PesananDetail.NILAI_DISC = dataVm.PesananDetail.NILAI_DISC_1 + dataVm.PesananDetail.NILAI_DISC_2;

                if (dataVm.PesananDetail.NO_URUT == null)
                {
                    ErasoftDbContext.SOT01B.Add(dataVm.PesananDetail);
                }
            }

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            //add by calvin 8 nov 2018, update stok marketplace
            List<string> listBrg = new List<string>();
            listBrg.Add(dataVm.PesananDetail.BRG);
            updateStockMarketPlace(listBrg);
            //end add by calvin 8 nov 2018

            var vm = new PesananViewModel()
            {
                Pesanan = ErasoftDbContext.SOT01A.Single(p => p.NO_BUKTI == dataVm.Pesanan.NO_BUKTI),
                ListPesananDetail = ErasoftDbContext.SOT01B.Where(pd => pd.NO_BUKTI == dataVm.Pesanan.NO_BUKTI).ToList(),
                //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
            };

            return PartialView("BarangPesananPartial", vm);
        }

        public ActionResult UbahStatusPesanan(int? recNum, string tipeStatus)
        {
            var pesananInDb = ErasoftDbContext.SOT01A.Single(p => p.RecNum == recNum);
            if (tipeStatus == "04") // validasi di tab Siap dikirim
            {
                var dataVm = new PesananViewModel()
                {
                    Pesanan = pesananInDb
                };

                //if (pesananInDb.TRACKING_SHIPMENT.Trim() == "")
                //remark by nurul 23/11/2018 no resi boleh kosong 
                //if (dataVm.Pesanan.TRACKING_SHIPMENT == null || pesananInDb.TRACKING_SHIPMENT.Trim() == "")
                //{

                //    var vmError = new StokViewModel();
                //    vmError.Errors.Add("Resi belum diisi");
                //    return Json(vmError, JsonRequestBehavior.AllowGet);
                //}
                //end remark by nurul 23/11/2018 no resi boleh kosong 

                var pesananDetailInDb = ErasoftDbContext.SOT01B.Where(p => p.NO_BUKTI == pesananInDb.NO_BUKTI).ToList();
                bool valid = true;
                foreach (var item in pesananDetailInDb)
                {
                    if (item.LOKASI.Trim() == "")
                    {
                        valid = false;
                    }
                }

                if (!valid)
                {
                    var vmError = new StokViewModel();
                    vmError.Errors.Add("Gd & Qty belum lengkap");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
            }

            //add by nurul 4/1/2019 (tambah validasi jika gudang belum diisi)
            if (tipeStatus == "03")
            {
                var pesananDetailInDb = ErasoftDbContext.SOT01B.Where(p => p.NO_BUKTI == pesananInDb.NO_BUKTI).ToList();
                bool valid = true;
                foreach (var item in pesananDetailInDb)
                {
                    if (item.LOKASI.Trim() == "")
                    {
                        valid = false;
                    }
                }

                if (!valid)
                {
                    var vmError = new StokViewModel();
                    vmError.Errors.Add("Isi semua gudang / qty terlebih dahulu!");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
            }
            //end add

            pesananInDb.STATUS_TRANSAKSI = tipeStatus;
            ErasoftDbContext.SaveChanges();

            //add by calvin 29 nov 2018
            if (tipeStatus == "11") // cancel, update qoh
            {
                var pesananDetailInDb = ErasoftDbContext.SOT01B.Where(p => p.NO_BUKTI == pesananInDb.NO_BUKTI).ToList();

                List<string> listBrg = new List<string>();
                foreach (var item in pesananDetailInDb)
                {
                    listBrg.Add(item.BRG);
                }
                updateStockMarketPlace(listBrg);
            }
            //end add by calvin 29 nov 2018

            //add by Tri, call marketplace api to update order status
            ChangeStatusPesanan(pesananInDb.NO_BUKTI, pesananInDb.STATUS_TRANSAKSI, false);
            //end add by Tri, call marketplace api to update order status
            return new EmptyResult();
        }

        public ActionResult RefreshTablePesanan()
        {
            var vm = new PesananViewModel()
            {
                //change by nurul 22/1/2019 -- ListPesanan = ErasoftDbContext.SOT01A.Where(p => p.STATUS_TRANSAKSI == "0").ToList(),
                ListPesanan = ErasoftDbContext.SOT01A.ToList(),
                //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList()
            };

            return PartialView("TablePesananPartial", vm);
        }
        //add by calvin 17 desember 2018
        public ActionResult FillModalFixNotFound(string recNum)
        {
            var intRecnum = Convert.ToInt64(recNum);
            var PesananDetail = ErasoftDbContext.SOT01B.Where(b => b.NO_URUT == intRecnum).FirstOrDefault();

            var pesananInDb = ErasoftDbContext.SOT01A.Single(p => p.NO_BUKTI == PesananDetail.NO_BUKTI);
            var marketInDb = ErasoftDbContext.ARF01.Single(m => m.CUST == pesananInDb.CUST);
            var idMarket = Convert.ToInt32(marketInDb.RecNum);
            var ListBarangMarket = ErasoftDbContext.STF02H.Where(p => p.IDMARKET == idMarket).ToList();
            var ListKodeBarangMarket = ListBarangMarket.Select(p => p.BRG).ToList();
            //var ListBarang = ErasoftDbContext.STF02.Where(p => ListKodeBarangMarket.Contains(p.BRG)).ToList(); 'change by nurul 21/1/2019
            var ListBarang = ErasoftDbContext.STF02.Where(p => ListKodeBarangMarket.Contains(p.BRG) && p.TYPE == "3").ToList();
            var vm = new PesananViewModel()
            {
                PesananDetail = PesananDetail,
                ListBarangMarket = ListBarangMarket,
                ListBarang = ListBarang
            };
            return PartialView("BarangFixNotFoundPartial", vm);
        }
        public ActionResult UpdateFixNotFound(string nourut)
        {
            try
            {
                var no_urut = nourut.Split(';');
                int no_urut_sot01b = Convert.ToInt32(no_urut[0]);
                int recnum_stf02h = Convert.ToInt32(no_urut[1]);
                var PesananDetail = ErasoftDbContext.SOT01B.Where(b => b.NO_URUT == no_urut_sot01b).SingleOrDefault();
                var dataStf02h = ErasoftDbContext.STF02H.Where(b => b.RecNum == recnum_stf02h).SingleOrDefault();

                var pesananInDb = ErasoftDbContext.SOT01A.SingleOrDefault(p => p.NO_BUKTI == PesananDetail.NO_BUKTI);
                PesananDetail.BRG = dataStf02h.BRG;

                if (string.IsNullOrWhiteSpace(dataStf02h.BRG_MP))
                {
                    var catatan_split = PesananDetail.CATATAN.Split(new string[] { "_;_" }, StringSplitOptions.None);

                    if (catatan_split.Count() > 2) //OrderNo_;_NamaBarang_;_IdBarang
                    {
                        dataStf02h.BRG_MP = catatan_split[2];
                    }
                }
                ErasoftDbContext.SaveChanges();

                var vm = new PesananViewModel()
                {
                    Pesanan = pesananInDb,
                    ListPesanan = ErasoftDbContext.SOT01A.ToList(),
                    ListPesananDetail = ErasoftDbContext.SOT01B.Where(pd => pd.NO_BUKTI == pesananInDb.NO_BUKTI).ToList(),
                    //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListEkspedisi = MoDbContext.Ekspedisi.ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList()
                };

                //add by calvin 21 Desember 2018, update stok marketplace
                List<string> listBrg = new List<string>();
                listBrg.Add(PesananDetail.BRG);
                updateStockMarketPlace(listBrg);
                //end add by calvin 21 Desember 2018

                return PartialView("BarangPesananSelesaiPartial", vm);
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        //end add by calvn 17 desember 2018
        public ActionResult RefreshGudangQtyPesanan(string noBuk)
        {
            //add by calvin 27 nov 2018, munculkan QOH di combobox gudang
            var ListPesananDetail = ErasoftDbContext.SOT01B.Where(b => b.NO_BUKTI == noBuk).ToList();

            List<string> items = new List<string>();
            string brg = "";
            foreach (var item in ListPesananDetail)
            {
                items.Add(item.BRG);
                if (brg != "")
                {
                    brg += "','";
                }
                brg += item.BRG;
            }

            //var ListBarang = ErasoftDbContext.STF02.Where(p => items.Contains(p.BRG)).ToList(); 'change by nurul 21/1/2019
            var ListBarang = ErasoftDbContext.STF02.Where(p => items.Contains(p.BRG) && p.TYPE == "3").ToList();
            string sSQL = "SELECT A.BRG, A.GD, B.Nama_Gudang, QOH = ISNULL(SUM(QAWAL+(QM1+QM2+QM3+QM4+QM5+QM6+QM7+QM8+QM9+QM10+QM11+QM12)-(QK1+QK2+QK3+QK4+QK5+QK6+QK7+QK8+QK9+QK10+QK11+QK12)),0) ";
            sSQL += "FROM STF08A A LEFT JOIN STF18 B ON A.GD = B.Kode_Gudang WHERE A.TAHUN=" + DateTime.Now.ToString("yyyy") + " AND A.BRG IN ('" + brg + "') GROUP BY A.BRG, A.GD, B.Nama_Gudang";
            var ListQOHPerGD = ErasoftDbContext.Database.SqlQuery<QOH_PER_GD>(sSQL).ToList();
            //end add by calvin 27 nov 2018, munculkan QOH di combobox gudang
            sSQL = "SELECT BRG,GD = B.LOKASI, QSO = ISNULL(SUM(ISNULL(QTY,0)),0) FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI LEFT JOIN SIT01A C ON A.NO_BUKTI = C.NO_SO WHERE A.STATUS_TRANSAKSI IN ('0', '01', '02', '03', '04')  AND ISNULL(C.NO_BUKTI,'') = '' AND B.BRG IN ('" + brg + "') AND A.NO_BUKTI <> '" + noBuk + "' GROUP BY BRG, B.LOKASI";
            var ListQOOPerBRG = ErasoftDbContext.Database.SqlQuery<QOO_PER_BRG>(sSQL).ToList();
            //add by nurul 11/3/2019
            var cekgudang = ErasoftDbContext.STF18.Where(a => a.Kode_Gudang == ErasoftDbContext.SIFSYS.FirstOrDefault().GUDANG).ToList();
            var gudang = "";
            if (cekgudang.Count() > 0)
            {
                gudang = ErasoftDbContext.SIFSYS.SingleOrDefault().GUDANG;
            }
            else
            {
                gudang = ErasoftDbContext.STF18.FirstOrDefault().Kode_Gudang;
            }
            //end add by nurul 11/3/2019
            var vm = new PesananViewModel()
            {
                //add by nurul 23/11/2018
                Pesanan = ErasoftDbContext.SOT01A.SingleOrDefault(b => b.NO_BUKTI == noBuk),
                //end add 
                ListPesananDetail = ListPesananDetail,
                ListBarang = ListBarang,
                ListQOHPerGD = ListQOHPerGD,
                ListQOOPerBRG = ListQOOPerBRG,
                //add by nurul 11/3/2019
                setGd = gudang
                //end add by nurul 11/3/2019
            };

            return PartialView("GudangQtyPartial", vm);
        }

        public ActionResult RefreshTablePesananSudahDibayar()
        {
            var vm = new PesananViewModel()
            {
                ListPesanan = ErasoftDbContext.SOT01A.Where(p => p.STATUS_TRANSAKSI == "01").ToList(),
                //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
            };

            return PartialView("TablePesananSudahDibayarPartial", vm);
        }

        public ActionResult RefreshTablePesananSiapKirim()
        {
            var vm = new PesananViewModel()
            {
                ListPesanan = ErasoftDbContext.SOT01A.Where(p => p.STATUS_TRANSAKSI == "02").ToList(),
                //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList()
            };

            return PartialView("TablePesananSiapKirimPartial", vm);
        }

        public ActionResult RefreshTablePesananSudahKirim()
        {
            var vm = new PesananViewModel()
            {
                ListPesanan = ErasoftDbContext.SOT01A.Where(p => p.STATUS_TRANSAKSI == "03").ToList(),
                //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListFaktur = ErasoftDbContext.SIT01A.ToList()
            };

            return PartialView("TablePesananSudahKirimPartial", vm);
        }

        public ActionResult RefreshTablePesananSelesai()
        {
            var vm = new PesananViewModel()
            {
                ListPesanan = ErasoftDbContext.SOT01A.Where(p => p.STATUS_TRANSAKSI == "04").ToList(),
                //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListFaktur = ErasoftDbContext.SIT01A.ToList()
            };

            return PartialView("TablePesananSelesaiPartial", vm);
        }

        public ActionResult RefreshTablePesananCancel()
        {
            var vm = new PesananViewModel()
            {
                ListPesanan = ErasoftDbContext.SOT01A.Where(p => p.STATUS_TRANSAKSI == "11").ToList(),
                //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList()
            };

            return PartialView("TablePesananCancelPartial", vm);
        }

        public ActionResult RefreshPesananForm()
        {
            try
            {
                var vm = new PesananViewModel()
                {
                    ListPesanan = ErasoftDbContext.SOT01A.ToList(),
                    //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList()
                };

                return PartialView("BarangPesananPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult EditPesanan(int? orderId)
        {
            try
            {
                var pesananInDb = ErasoftDbContext.SOT01A.Single(p => p.RecNum == orderId);

                var vm = new PesananViewModel()
                {
                    Pesanan = pesananInDb,
                    ListPesanan = ErasoftDbContext.SOT01A.ToList(),
                    ListPesananDetail = ErasoftDbContext.SOT01B.Where(pd => pd.NO_BUKTI == pesananInDb.NO_BUKTI).ToList(),
                    //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList()
                };

                return PartialView("BarangPesananPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        //add by Tri, call marketplace api to change status
        [HttpGet]
        public void ChangeStatusPesanan(string nobuk, string status, bool lazadaPickup)
        {
            var pesanan = ErasoftDbContext.SOT01A.Single(p => p.NO_BUKTI == nobuk);
            var marketPlace = ErasoftDbContext.ARF01.Single(p => p.CUST == pesanan.CUST);
            var mp = MoDbContext.Marketplaces.Single(p => p.IdMarket.ToString() == marketPlace.NAMA);
            var blAPI = new BukaLapakController();
            var lzdAPI = new LazadaController();
            switch (status)
            {
                case "11"://cancel
                    {
                        if (mp.NamaMarket.ToUpper().Contains("SHOPEE"))
                        {
                            var shoAPI = new ShopeeController();
                            ShopeeController.ShopeeAPIData data = new ShopeeController.ShopeeAPIData()
                            {
                                merchant_code = marketPlace.Sort1_Cust,
                            };
                            Task.Run(() => shoAPI.AcceptBuyerCancellation(data, pesanan.NO_REFERENSI).Wait());
                        }

                        if (mp.NamaMarket.ToUpper().Contains("LAZADA"))
                        {
                            var sot01b = ErasoftDbContext.SOT01B.Where(p => p.NO_BUKTI == nobuk).ToList();
                            if (sot01b.Count > 0)
                            {
                                foreach (var tbl in sot01b)
                                {
                                    lzdAPI.SetStatusToCanceled(tbl.ORDER_ITEM_ID, marketPlace.TOKEN);

                                }
                            }
                        }
                    }
                    break;
                case "02":
                    if (mp.NamaMarket.ToUpper().Contains("BUKALAPAK"))
                    {

                    }
                    if (mp.NamaMarket.ToUpper().Contains("ELEVENIA"))
                    {
                        DataSet dsTEMP_ELV_ORDERS = new DataSet();
                        dsTEMP_ELV_ORDERS = EDB.GetDataSet("Con", "TEMP_ELV_ORDERS", "SELECT ORDER_NO,ORDER_PROD_NO FROM TEMP_ELV_ORDERS WHERE DELIVERY_NO='" + Convert.ToString(pesanan.NO_REFERENSI) + "' GROUP BY ORDER_NO,ORDER_PROD_NO");
                        if (dsTEMP_ELV_ORDERS.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsTEMP_ELV_ORDERS.Tables[0].Rows.Count; i++)
                            {
                                string ordNo = Convert.ToString(dsTEMP_ELV_ORDERS.Tables[0].Rows[i]["ORDER_NO"]);
                                string ordPrdSeq = Convert.ToString(dsTEMP_ELV_ORDERS.Tables[0].Rows[i]["ORDER_PROD_NO"]);
                                var elApi = new EleveniaController();
                                elApi.AcceptOrder(marketPlace.API_KEY, ordNo, ordPrdSeq);
                            }
                        }
                    }
                    if (mp.NamaMarket.ToUpper().Contains("LAZADA"))
                    {
                        //List<string> orderItemIds = new List<string>();
                        //var sot01b = ErasoftDbContext.SOT01B.Where(p => p.NO_BUKTI == nobuk).ToList();
                        //if(sot01b.Count > 0)
                        //{
                        //    foreach(var tbl in sot01b)
                        //    {
                        //        orderItemIds.Add(tbl.ORDER_ITEM_ID);
                        //    }
                        //    lzdAPI.GetToPacked(orderItemIds, "JNE", marketPlace.TOKEN);
                        //}
                    }
                    if (mp.NamaMarket.ToUpper().Contains("TOKOPEDIA"))
                    {
                        var TokoAPI = new TokopediaController();
                        if (!string.IsNullOrEmpty(marketPlace.Sort1_Cust))
                        {
                            TokopediaController.TokopediaAPIData iden = new TokopediaController.TokopediaAPIData()
                            {
                                merchant_code = marketPlace.Sort1_Cust, //FSID
                                API_client_password = marketPlace.API_CLIENT_P, //Client ID
                                API_client_username = marketPlace.API_CLIENT_U, //Client Secret
                                API_secret_key = marketPlace.API_KEY, //Shop ID 
                                token = marketPlace.TOKEN,
                                idmarket = marketPlace.RecNum.Value
                            };
                            Task.Run(() => TokoAPI.PostAckOrder(iden, pesanan.NO_BUKTI, pesanan.NO_REFERENSI)).Wait();
                        }
                    }
                    break;
                case "03":
                    if (mp.NamaMarket.ToUpper().Contains("BUKALAPAK"))
                    {
                        if (!string.IsNullOrEmpty(pesanan.TRACKING_SHIPMENT))
                            blAPI.KonfirmasiPengiriman(/*nobuk,*/ pesanan.TRACKING_SHIPMENT, pesanan.NO_REFERENSI, pesanan.SHIPMENT, marketPlace.API_KEY, marketPlace.TOKEN);
                    }
                    else if (mp.NamaMarket.ToUpper().Contains("LAZADA"))
                    {
                        //if (!string.IsNullOrEmpty(pesanan.TRACKING_SHIPMENT) && !string.IsNullOrEmpty(pesanan.SHIPMENT))
                        //{
                        if (lazadaPickup)
                        {
                            var pesananChild = ErasoftDbContext.SOT01B.Where(p => p.NO_BUKTI == nobuk).ToList();
                            if (pesananChild.Count > 0)
                            {
                                List<string> ordItemId = new List<string>();
                                foreach (SOT01B item in pesananChild)
                                {
                                    ordItemId.Add(item.ORDER_ITEM_ID);
                                }
                                //if(typeDelivery == "0")//dropship
                                //{
                                //    lzdAPI.GetToDeliver(ordItemId, pesanan.SHIPMENT, pesanan.TRACKING_SHIPMENT, marketPlace.TOKEN);
                                //}
                                //else//pick up
                                //{
                                //    lzdAPI.GetToPacked(ordItemId, pesanan.SHIPMENT, marketPlace.TOKEN);
                                //}
                                //lzdAPI.GetToPacked(ordItemId, pesanan.SHIPMENT, marketPlace.TOKEN);
                                lzdAPI.GetToDeliver(ordItemId, pesanan.SHIPMENT, pesanan.TRACKING_SHIPMENT, marketPlace.TOKEN);
                            }


                        }
                        //}
                    }
                    else if (mp.NamaMarket.ToUpper().Contains("ELEVENIA"))
                    {
                        if (!string.IsNullOrEmpty(pesanan.TRACKING_SHIPMENT))
                        {

                            DataSet dsTEMP_ELV_ORDERS = new DataSet();
                            dsTEMP_ELV_ORDERS = EDB.GetDataSet("Con", "TEMP_ELV_ORDERS", "SELECT DELIVERY_MTD_CD,DELIVERY_ETR_CD,ORDER_NO,DELIVERY_ETR_NAME,ORDER_PROD_NO FROM TEMP_ELV_ORDERS WHERE DELIVERY_NO='" + Convert.ToString(pesanan.NO_REFERENSI) + "' GROUP BY DELIVERY_MTD_CD,DELIVERY_ETR_CD,ORDER_NO,DELIVERY_ETR_NAME,ORDER_PROD_NO");
                            if (dsTEMP_ELV_ORDERS.Tables[0].Rows.Count > 0)
                            {
                                for (int i = 0; i < dsTEMP_ELV_ORDERS.Tables[0].Rows.Count; i++)
                                {
                                    string awb = Convert.ToString(pesanan.TRACKING_SHIPMENT);
                                    string dlvNo = Convert.ToString(pesanan.NO_REFERENSI);
                                    string dlvMthdCd = Convert.ToString(dsTEMP_ELV_ORDERS.Tables[0].Rows[i]["DELIVERY_MTD_CD"]);
                                    string dlvEtprsCd = Convert.ToString(dsTEMP_ELV_ORDERS.Tables[0].Rows[i]["DELIVERY_ETR_CD"]);
                                    string ordNo = Convert.ToString(dsTEMP_ELV_ORDERS.Tables[0].Rows[i]["ORDER_NO"]);
                                    string dlvEtprsNm = Convert.ToString(dsTEMP_ELV_ORDERS.Tables[0].Rows[i]["DELIVERY_ETR_NAME"]);
                                    string ordPrdSeq = Convert.ToString(dsTEMP_ELV_ORDERS.Tables[0].Rows[i]["ORDER_PROD_NO"]);
                                    var elApi = new EleveniaController();
                                    elApi.UpdateAWBNumber(marketPlace.API_KEY, awb, dlvNo, dlvMthdCd, dlvEtprsCd, ordNo, dlvEtprsNm, ordPrdSeq);
                                }
                            }
                        }
                    }
                    else if (mp.NamaMarket.ToUpper().Contains("BLIBLI"))
                    {
                        if (!string.IsNullOrEmpty(pesanan.TRACKING_SHIPMENT))
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(pesanan.NO_REFERENSI)))
                            {
                                var bliAPI = new BlibliController();
                                var listDetail = ErasoftDbContext.SOT01B.Where(p => p.NO_BUKTI == pesanan.NO_BUKTI).ToList();
                                foreach (var item in listDetail)
                                {
                                    BlibliController.BlibliAPIData iden = new BlibliController.BlibliAPIData
                                    {
                                        merchant_code = marketPlace.Sort1_Cust,
                                        API_client_password = marketPlace.API_CLIENT_P,
                                        API_client_username = marketPlace.API_CLIENT_U,
                                        API_secret_key = marketPlace.API_KEY,
                                        token = marketPlace.TOKEN,
                                        mta_username_email_merchant = marketPlace.EMAIL,
                                        mta_password_password_merchant = marketPlace.PASSWORD,
                                        idmarket = marketPlace.RecNum.Value
                                    };
                                    bliAPI.fillOrderAWB(iden, pesanan.TRACKING_SHIPMENT, pesanan.NO_REFERENSI, item.ORDER_ITEM_ID);
                                }
                            }
                        }
                    }
                    break;
            }

        }
        //end add by Tri, call marketplace api to change status
        public ActionResult LazadaLabel(int recnum)
        {
            var pesanan = ErasoftDbContext.SOT01A.Single(p => p.RecNum == recnum);
            var marketPlace = ErasoftDbContext.ARF01.Single(p => p.CUST == pesanan.CUST);
            var mp = MoDbContext.Marketplaces.Single(p => p.IdMarket.ToString() == marketPlace.NAMA);
            if (mp.NamaMarket.ToUpper().Contains("LAZADA"))
            {
                var lzdApi = new LazadaController();
                List<string> orderItemIds = new List<string>();
                var sot01b = ErasoftDbContext.SOT01B.Where(p => p.NO_BUKTI == pesanan.NO_BUKTI).ToList();
                if (sot01b.Count > 0)
                {
                    foreach (var tbl in sot01b)
                    {
                        orderItemIds.Add(tbl.ORDER_ITEM_ID);
                    }
                    var retApi = lzdApi.GetLabel(orderItemIds, marketPlace.TOKEN);
                    if (retApi.code == "0")
                    {
                        var htmlString = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(retApi.data.document.file));
                        #region add button cetak
                        htmlString += "<button id='print-btn' >Cetak</button>";
                        htmlString += "<script>";
                        htmlString += " function run() { document.getElementById('print-btn').onclick = function () {";
                        htmlString += "document.getElementById('print-btn').style.visibility = 'hidden';";
                        htmlString += "window.print(); }; window.onafterprint = function () {";
                        htmlString += "document.getElementById('print-btn').style.visibility = 'visible'; } }";
                        htmlString += " if (document.readyState!='loading') run();";
                        htmlString += " else if (document.addEventListener) document.addEventListener('DOMContentLoaded', run);";
                        htmlString += "else document.attachEvent('onreadystatechange', function(){ if (document.readyState=='complete') run(); });";
                        htmlString += "</script>";
                        #endregion
                        return Json(htmlString, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return JsonErrorMessage(retApi.message);
                    }

                }
                else
                {
                    return JsonErrorMessage("Detail Order not found.");
                }
            }
            return JsonErrorMessage("This Function is for Lazada only");
        }

        public ActionResult LazadaGetResi(int recnum, string DeliveryProvider)
        {
            var pesanan = ErasoftDbContext.SOT01A.Single(p => p.RecNum == recnum);
            var marketPlace = ErasoftDbContext.ARF01.Single(p => p.CUST == pesanan.CUST);
            var mp = MoDbContext.Marketplaces.Single(p => p.IdMarket.ToString() == marketPlace.NAMA);
            if (mp.NamaMarket.ToUpper().Contains("LAZADA"))
            {
                var lzdApi = new LazadaController();
                List<string> orderItemIds = new List<string>();
                var sot01b = ErasoftDbContext.SOT01B.Where(p => p.NO_BUKTI == pesanan.NO_BUKTI).ToList();
                if (sot01b.Count > 0)
                {
                    List<string> ordItemId = new List<string>();
                    foreach (SOT01B item in sot01b)
                    {
                        ordItemId.Add(item.ORDER_ITEM_ID);
                    }

                    var retApi = lzdApi.GetToPacked(ordItemId, DeliveryProvider, marketPlace.TOKEN);
                    if (retApi.code == "0")
                    {
                        var ret = new LazadaGetResiObj();
                        //{
                        if (retApi.data != null)
                            ret.NoResi = retApi.data.order_items[0].tracking_number;
                        //};
                        return Json(ret, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return JsonErrorMessage(retApi.message);
                    }

                }
                else
                {
                    return JsonErrorMessage("Detail Order not found.");
                }
            }
            return JsonErrorMessage("This Function is for Lazada only");
        }

        public ActionResult LihatPesanan(int? orderId)
        {
            try
            {
                var pesananInDb = ErasoftDbContext.SOT01A.Single(p => p.RecNum == orderId);

                var vm = new PesananViewModel()
                {
                    Pesanan = pesananInDb,
                    ListPesanan = ErasoftDbContext.SOT01A.ToList(),
                    ListPesananDetail = ErasoftDbContext.SOT01B.Where(pd => pd.NO_BUKTI == pesananInDb.NO_BUKTI).ToList(),
                    //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListEkspedisi = MoDbContext.Ekspedisi.ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList()
                };

                return PartialView("BarangPesananSelesaiPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult DeletePesanan(int? orderId)
        {
            var pesananInDb = ErasoftDbContext.SOT01A.Single(p => p.RecNum == orderId);

            // ========== Hapus Barang =============
            /*var listPesananDetail = ErasoftDbContext.SOT01B.Where(pd => pd.NO_BUKTI == pesananInDb.NO_BUKTI).ToList();

            if (listPesananDetail.Count > 0)
            {
                foreach (var pesananDetail in listPesananDetail)
                {
                    ErasoftDbContext.SOT01B.Remove(pesananDetail);
                }
            }*/

            //add by calvin 8 nov 2018, update stok marketplace
            List<string> listBrg = new List<string>();
            var detailPesananInDb = ErasoftDbContext.SOT01B.Where(p => p.NO_BUKTI == pesananInDb.NO_BUKTI).ToList();
            foreach (var item in detailPesananInDb)
            {
                listBrg.Add(item.BRG);
            }
            //end add by calvin 8 nov 2018

            ErasoftDbContext.SOT01A.Remove(pesananInDb);
            ErasoftDbContext.SaveChanges();

            //add by calvin 8 nov 2018, update stok marketplace
            updateStockMarketPlace(listBrg);
            //end add by calvin 8 nov 2018

            var vm = new PesananViewModel()
            {
                //change by nurul 22/1/2019 -- ListPesanan = ErasoftDbContext.SOT01A.Where(p => p.STATUS_TRANSAKSI == "00").ToList(),
                ListPesanan = ErasoftDbContext.SOT01A.ToList(),
                //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList()
            };

            return PartialView("TablePesananPartial", vm);
        }

        [HttpGet]
        public ActionResult DeleteBarangPesanan(int noUrut)
        {
            try
            {
                var barangPesananInDb = ErasoftDbContext.SOT01B.Single(b => b.NO_URUT == noUrut);
                var pesananInDb = ErasoftDbContext.SOT01A.Single(p => p.NO_BUKTI == barangPesananInDb.NO_BUKTI);

                pesananInDb.BRUTO -= barangPesananInDb.HARGA;
                pesananInDb.NILAI_PPN = Math.Ceiling(pesananInDb.PPN * pesananInDb.BRUTO / 100);
                pesananInDb.NETTO = pesananInDb.BRUTO - pesananInDb.DISCOUNT + pesananInDb.NILAI_PPN +
                                    pesananInDb.ONGKOS_KIRIM;

                ErasoftDbContext.SOT01B.Remove(barangPesananInDb);
                ErasoftDbContext.SaveChanges();

                //add by calvin 8 nov 2018, update stok marketplace
                List<string> listBrg = new List<string>();
                listBrg.Add(barangPesananInDb.BRG);
                updateStockMarketPlace(listBrg);
                //end add by calvin 8 nov 2018
                var vm = new PesananViewModel()
                {
                    Pesanan = ErasoftDbContext.SOT01A.Single(p => p.NO_BUKTI == pesananInDb.NO_BUKTI),
                    ListPesanan = ErasoftDbContext.SOT01A.ToList(),
                    ListPesananDetail = ErasoftDbContext.SOT01B.Where(pd => pd.NO_BUKTI == pesananInDb.NO_BUKTI).ToList(),
                    //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList()
                };

                return PartialView("BarangPesananPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult UpdatePesanan(UpdateData dataUpdate)
        {
            var pesananInDb = ErasoftDbContext.SOT01A.Single(p => p.NO_BUKTI == dataUpdate.OrderId);
            pesananInDb.NILAI_DISC = dataUpdate.NilaiDisc;
            pesananInDb.ONGKOS_KIRIM = dataUpdate.OngkosKirim;
            pesananInDb.PPN = dataUpdate.Ppn;
            pesananInDb.NILAI_PPN = dataUpdate.NilaiPpn;
            pesananInDb.TGL = DateTime.ParseExact(dataUpdate.Tgl, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            pesananInDb.CUST = dataUpdate.Cust;
            pesananInDb.TERM = dataUpdate.Term;
            pesananInDb.EXPEDISI = dataUpdate.Exp;
            pesananInDb.PEMESAN = dataUpdate.Buyer;
            var buyer = ErasoftDbContext.ARF01C.FirstOrDefault(k => k.BUYER_CODE == dataUpdate.Buyer);
            pesananInDb.NAMAPEMESAN = buyer.NAMA;
            pesananInDb.TGL_JTH_TEMPO = DateTime.ParseExact(dataUpdate.Tempo, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            pesananInDb.NETTO = pesananInDb.BRUTO - pesananInDb.NILAI_DISC + pesananInDb.NILAI_PPN +
                                pesananInDb.ONGKOS_KIRIM;

            ErasoftDbContext.SaveChanges();

            return new EmptyResult();
        }

        [HttpGet]
        public ActionResult GetResi(int? recNum)
        {
            var pesananInDb = ErasoftDbContext.SOT01A.Single(p => p.RecNum == recNum);

            //change by nurul 22/11/2018
            //string[] shipment = new string[2];
            //shipment[0] = pesananInDb.TRACKING_SHIPMENT;
            //shipment[1] = pesananInDb.SHIPMENT;
            string[] shipment = new string[5];
            shipment[0] = pesananInDb.TRACKING_SHIPMENT;
            shipment[1] = pesananInDb.SHIPMENT;
            shipment[2] = pesananInDb.NO_BUKTI;
            shipment[3] = pesananInDb.NAMAPEMESAN;
            shipment[4] = pesananInDb.NAMAPENGIRIM;
            //end change 


            return Json(shipment, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public async Task<ActionResult> GetResiTokped(int? recNum)
        {
            var pesananInDb = ErasoftDbContext.SOT01A.Single(p => p.RecNum == recNum);
            //var marketPlace = ErasoftDbContext.ARF01.Single(p => p.CUST == pesananInDb.CUST);

            string[] shipment = new string[6];
            shipment[0] = pesananInDb.TRACKING_SHIPMENT;
            shipment[1] = pesananInDb.SHIPMENT;
            shipment[2] = pesananInDb.NO_BUKTI;
            shipment[3] = pesananInDb.NAMAPEMESAN;
            shipment[4] = pesananInDb.NAMAPENGIRIM;

            string parameters = "";
            shipment[5] = "";
            if (string.IsNullOrWhiteSpace(pesananInDb.TRACKING_SHIPMENT))
            {
                //var shoAPI = new ShopeeController();
                //ShopeeController.ShopeeAPIData data = new ShopeeController.ShopeeAPIData()
                //{
                //    merchant_code = marketPlace.Sort1_Cust,
                //};
                //ShopeeController.ShopeeGetParameterForInitLogisticResult InitParam;
                //InitParam = await shoAPI.GetParameterForInitLogistic(data, pesananInDb.NO_REFERENSI);

                //if (InitParam.dropoff != null)
                //{
                //    parameters += "DROPOFF;";

                //    if (InitParam.dropoff.Contains("branch_id"))
                //    {
                //        parameters += "BRANCH_ID;";
                //    }
                //    if (InitParam.dropoff.Contains("sender_real_name"))
                //    {
                //        parameters += "SENDER;";
                //    }
                //    if (InitParam.dropoff.Contains("tracking_no"))
                //    {
                //        parameters += "DROPOFF_TRACKING_NO;";
                //    }
                //}
                if (pesananInDb.SHIPMENT.Contains("Instant"))
                {
                    parameters += "PICKUP;";

                    //if (InitParam.pickup.Contains("address_id"))
                    //{
                    //    parameters += "ADDRESS_ID;";
                    //}
                    //if (InitParam.pickup.Contains("pickup_time_id"))
                    //{
                    //    parameters += "PICKUP_TIME;";
                    //}
                }
                //if (InitParam.non_integrated != null)
                //{
                //    parameters += "NON;";
                //    if (InitParam.non_integrated.Contains("tracking_no"))
                //    {
                //        parameters += "TRACKING_NO;";
                //    }
                //}
            }
            shipment[5] = parameters;
            return Json(shipment, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetResiShopee(int? recNum)
        {
            var pesananInDb = ErasoftDbContext.SOT01A.Single(p => p.RecNum == recNum);
            var marketPlace = ErasoftDbContext.ARF01.Single(p => p.CUST == pesananInDb.CUST);


            string[] shipment = new string[6];
            shipment[0] = pesananInDb.TRACKING_SHIPMENT;
            shipment[1] = pesananInDb.SHIPMENT;
            shipment[2] = pesananInDb.NO_BUKTI;
            shipment[3] = pesananInDb.NAMAPEMESAN;
            shipment[4] = pesananInDb.NAMAPENGIRIM;

            string parameters = "";
            shipment[5] = "";
            if (string.IsNullOrWhiteSpace(pesananInDb.TRACKING_SHIPMENT))
            {
                var shoAPI = new ShopeeController();
                ShopeeController.ShopeeAPIData data = new ShopeeController.ShopeeAPIData()
                {
                    merchant_code = marketPlace.Sort1_Cust,
                };
                ShopeeController.ShopeeGetParameterForInitLogisticResult InitParam;
                InitParam = await shoAPI.GetParameterForInitLogistic(data, pesananInDb.NO_REFERENSI);

                if (InitParam.dropoff != null)
                {
                    parameters += "DROPOFF;";

                    if (InitParam.dropoff.Contains("branch_id"))
                    {
                        parameters += "BRANCH_ID;";
                    }
                    if (InitParam.dropoff.Contains("sender_real_name"))
                    {
                        parameters += "SENDER;";
                    }
                    if (InitParam.dropoff.Contains("tracking_no"))
                    {
                        parameters += "DROPOFF_TRACKING_NO;";
                    }
                }
                if (InitParam.pickup != null)
                {
                    parameters += "PICKUP;";

                    if (InitParam.pickup.Contains("address_id"))
                    {
                        parameters += "ADDRESS_ID;";
                    }
                    if (InitParam.pickup.Contains("pickup_time_id"))
                    {
                        parameters += "PICKUP_TIME;";
                    }
                }
                if (InitParam.non_integrated != null)
                {
                    parameters += "NON;";
                    if (InitParam.non_integrated.Contains("tracking_no"))
                    {
                        parameters += "TRACKING_NO;";
                    }
                }
            }
            shipment[5] = parameters;
            return Json(shipment, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SaveResi(int? recNum, string noResi, string deliveryProv/*, string typeDelivery*/)
        {
            var pesananInDb = ErasoftDbContext.SOT01A.Single(p => p.RecNum == recNum);
            //remark 15-02-2019, agar user tidak perlu kosongkan nmr resi yg didapan langsung dr api
            //add by Tri, check if user input new resi
            //bool changeStat = false;
            //if (string.IsNullOrEmpty(pesananInDb.TRACKING_SHIPMENT))
            //    changeStat = true;
            //end add by Tri, check if user input new resi
            //end remark 15-02-2019, agar user tidak perlu kosongkan nmr resi yg didapan langsung dr api

            //add by Tri, delivery provider lazada
            if (!string.IsNullOrEmpty(deliveryProv))
                pesananInDb.SHIPMENT = deliveryProv;
            //end add by Tri, delivery provider lazada
            pesananInDb.TRACKING_SHIPMENT = noResi;
            ErasoftDbContext.SaveChanges();

            //add by Tri, call mp api if user input new resi
            //remark 15-02-2019, agar user tidak perlu kosongkan nmr resi yg didapan langsung dr api
            //if (changeStat)
            //end remark 15-02-2019, agar user tidak perlu kosongkan nmr resi yg didapan langsung dr api
            ChangeStatusPesanan(pesananInDb.NO_BUKTI, "03", true/*, typeDelivery*/);
            //end add by Tri, call mp api if user input new resi

            return new EmptyResult();
        }

        [HttpGet]
        public async Task<ActionResult> SaveResiTokped(int? recNum)
        {
            var pesananInDb = ErasoftDbContext.SOT01A.SingleOrDefault(p => p.RecNum == recNum);
            var marketPlace = ErasoftDbContext.ARF01.Single(p => p.CUST == pesananInDb.CUST);
            if (!string.IsNullOrEmpty(marketPlace.Sort1_Cust))
            {
                TokopediaController.TokopediaAPIData iden = new TokopediaController.TokopediaAPIData()
                {
                    merchant_code = marketPlace.Sort1_Cust, //FSID
                    API_client_password = marketPlace.API_CLIENT_P, //Client ID
                    API_client_username = marketPlace.API_CLIENT_U, //Client Secret
                    API_secret_key = marketPlace.API_KEY, //Shop ID 
                    token = marketPlace.TOKEN,
                    idmarket = marketPlace.RecNum.Value
                };
                var TokoAPI = new TokopediaController();
                string[] referensi = pesananInDb.NO_REFERENSI.Split(';');
                if (referensi.Count() > 0)
                {
                    await TokoAPI.PostRequestPickup(iden, pesananInDb.NO_BUKTI, referensi[0]);
                }
            }
            
            return new EmptyResult();
        }

        [HttpGet]
        public async Task<ActionResult> SaveResiShopee(int? recNum, string metode,
            string dBranch, string dSender, string dTrackNo,
            string pAddress, string pTime,
            string nTrackNo)
        {
            var pesananInDb = ErasoftDbContext.SOT01A.SingleOrDefault(p => p.RecNum == recNum);
            bool changeStat = false;
            if (string.IsNullOrEmpty(pesananInDb.TRACKING_SHIPMENT))
                changeStat = true;

            string nilaiTRACKING_SHIPMENT = "";
            if (metode == "0") // DROPOFF
            {
                //format : D[;]BRANCH_ID[;]SENDER_REAL_NAME[;]TRACKING_NO
                nilaiTRACKING_SHIPMENT = "D[;]" + dBranch + "[;]" + dSender + "[;]" + dTrackNo;
            }
            if (metode == "1") // PICKUP
            {
                //format : P[;]ADDRESS_ID[;]PICKUP_TIME
                nilaiTRACKING_SHIPMENT = "P[;]" + pAddress + "[;]" + pTime;
            }
            if (metode == "2") // NON INTEGRATED
            {
                //format : N[;]TRACKING_NO
                nilaiTRACKING_SHIPMENT = "N[;]" + nTrackNo;
            }

            //pesananInDb.TRACKING_SHIPMENT = nilaiTRACKING_SHIPMENT;
            //ErasoftDbContext.SaveChanges();

            if (changeStat)
            {
                var marketPlace = ErasoftDbContext.ARF01.Single(p => p.CUST == pesananInDb.CUST);
                var shoAPI = new ShopeeController();
                ShopeeController.ShopeeAPIData data = new ShopeeController.ShopeeAPIData()
                {
                    merchant_code = marketPlace.Sort1_Cust,
                };
                if (metode == "0") // DROPOFF
                {
                    ShopeeController.ShopeeInitLogisticDropOffDetailData detail = new ShopeeController.ShopeeInitLogisticDropOffDetailData()
                    {
                        branch_id = 0,
                        sender_real_name = "",
                        tracking_no = ""
                    };
                    if (dBranch != "")
                    {
                        detail.branch_id = Convert.ToInt64(dBranch);
                    }
                    if (dSender != "")
                    {
                        detail.sender_real_name = dSender;
                    }
                    if (dTrackNo != "")
                    {
                        detail.tracking_no = dTrackNo;
                    }
                    await shoAPI.InitLogisticDropOff(data, pesananInDb.NO_REFERENSI, detail, recNum.Value, dBranch, dSender, dTrackNo);
                }
                else if (metode == "1") // PICKUP
                {
                    ShopeeController.ShopeeInitLogisticPickupDetailData detail = new ShopeeController.ShopeeInitLogisticPickupDetailData()
                    {
                        address_id = 0,
                        pickup_time_id = ""
                    };
                    if (pAddress != "")
                    {
                        detail.address_id = Convert.ToInt64(pAddress);
                    }
                    if (pTime != "")
                    {
                        detail.pickup_time_id = pTime;
                    }
                    await shoAPI.InitLogisticPickup(data, pesananInDb.NO_REFERENSI, detail, recNum.Value, nilaiTRACKING_SHIPMENT);
                }
                else if (metode == "2") // NON INTEGRATED
                {
                    ShopeeController.ShopeeInitLogisticNotIntegratedDetailData detail = new ShopeeController.ShopeeInitLogisticNotIntegratedDetailData()
                    {
                        tracking_no = ""
                    };
                    if (nTrackNo != "")
                    {
                        detail.tracking_no = nTrackNo;
                    }
                    await shoAPI.InitLogisticNonIntegrated(data, pesananInDb.NO_REFERENSI, detail, recNum.Value, nilaiTRACKING_SHIPMENT);
                }
            }
            return new EmptyResult();
        }

        [HttpGet]
        public async Task<ActionResult> GetShopeeDropoffBranch(int? recNum)
        {
            var pesananInDb = ErasoftDbContext.SOT01A.Single(p => p.RecNum == recNum);
            var marketPlace = ErasoftDbContext.ARF01.Single(p => p.CUST == pesananInDb.CUST);
            var shoAPI = new ShopeeController();
            ShopeeController.ShopeeAPIData data = new ShopeeController.ShopeeAPIData()
            {
                merchant_code = marketPlace.Sort1_Cust,
            };
            var result = await shoAPI.GetBranch(data, pesananInDb.NO_REFERENSI);
            return new EmptyResult();
        }

        [HttpGet]
        public async Task<ActionResult> GetShopeePickupAddress(int? recNum)
        {
            var pesananInDb = ErasoftDbContext.SOT01A.Single(p => p.RecNum == recNum);
            var marketPlace = ErasoftDbContext.ARF01.Single(p => p.CUST == pesananInDb.CUST);
            var shoAPI = new ShopeeController();
            ShopeeController.ShopeeAPIData data = new ShopeeController.ShopeeAPIData()
            {
                merchant_code = marketPlace.Sort1_Cust,
            };
            var result = await shoAPI.GetAddress(data);
            return Json(result.address_list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetShopeePickupTime(int? recNum, long address_id)
        {
            var pesananInDb = ErasoftDbContext.SOT01A.Single(p => p.RecNum == recNum);
            var marketPlace = ErasoftDbContext.ARF01.Single(p => p.CUST == pesananInDb.CUST);
            var shoAPI = new ShopeeController();
            ShopeeController.ShopeeAPIData data = new ShopeeController.ShopeeAPIData()
            {
                merchant_code = marketPlace.Sort1_Cust,
            };
            var result = await shoAPI.GetTimeSlot(data, address_id, pesananInDb.NO_REFERENSI);
            return Json(result.pickup_time, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GenerateFaktur(int? recNumPesanan, string uname)
        {
            try
            {
                var pesananInDb = ErasoftDbContext.SOT01A.Single(p => p.RecNum == recNumPesanan);
                var listBarangPesananInDb = ErasoftDbContext.SOT01B.Where(p => p.NO_BUKTI == pesananInDb.NO_BUKTI).ToList();
                var dataVm = new FakturViewModel()
                {
                    Faktur = new SIT01A(),
                    FakturDetail = new SIT01B()
                };
                var cekNoSOExist = ErasoftDbContext.SIT01A.Where(p => p.NO_SO == pesananInDb.NO_BUKTI).FirstOrDefault();
                if (cekNoSOExist == null)
                {
                    // Bagian Save Faktur Generated

                    var digitAkhir = "";
                    var noOrder = "";

                    var listFakturInDb = ErasoftDbContext.SIT01A.Max(p => p.RecNum);

                    if (!listFakturInDb.HasValue)
                    {
                        digitAkhir = "000001";
                        noOrder = $"SI{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                        ErasoftDbContext.Database.ExecuteSqlCommand("DBCC CHECKIDENT (SIT01A, RESEED, 0)");
                    }
                    else
                    {
                        //change by calvin 4 maret 2019
                        //var lastRecNum = listFakturInDb.Last().RecNum;
                        var lastRecNum = listFakturInDb.Value;
                        //end change by calvin 4 maret 2019

                        if (lastRecNum == 0)
                        {
                            lastRecNum = 1;
                        }
                        lastRecNum++;

                        digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                        noOrder = $"SI{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                    }
                    #region add by calvin 31 okt 2018, hitung ulang sesuai dengan qty_n, bukan qty
                    var pesanan_bruto = 0d;
                    var pesanan_netto = 0d;
                    var pesanan_nilai_ppn = 0d;
                    foreach (var item in listBarangPesananInDb)
                    {
                        double nilai_disc_1 = 0d;
                        double nilai_disc_2 = 0d;
                        double harga = 0d;
                        if (Math.Abs(item.DISCOUNT) > 0)
                        {
                            nilai_disc_1 = (item.DISCOUNT * item.H_SATUAN * (item.QTY_N.HasValue ? item.QTY_N.Value : 0)) / 100;
                        }
                        else
                        {
                            //req by pak dani, dibuat proporsional jika discount bukan persen, tapi nilai discount, karena bisa lebih besar daripada harga * qty_n
                            nilai_disc_1 = (item.NILAI_DISC_1 / item.QTY) * (item.QTY_N.HasValue ? item.QTY_N.Value : 0);
                        }

                        if (Math.Abs(item.DISCOUNT_2) > 0)
                        {
                            nilai_disc_2 = (item.DISCOUNT * (item.H_SATUAN - nilai_disc_1) * (item.QTY_N.HasValue ? item.QTY_N.Value : 0)) / 100;
                        }
                        else
                        {
                            nilai_disc_2 = (item.NILAI_DISC_2 / item.QTY) * (item.QTY_N.HasValue ? item.QTY_N.Value : 0);
                        }

                        harga = item.H_SATUAN * (item.QTY_N.HasValue ? item.QTY_N.Value : 0) - nilai_disc_1 -
                                                  nilai_disc_2;
                        pesanan_bruto += harga;
                    }

                    pesanan_nilai_ppn = (pesananInDb.PPN * pesanan_bruto) / 100;

                    pesanan_netto = pesanan_bruto - pesananInDb.NILAI_DISC + pesanan_nilai_ppn + pesananInDb.ONGKOS_KIRIM;
                    #endregion

                    dataVm.Faktur.NO_BUKTI = noOrder;
                    dataVm.Faktur.NO_F_PAJAK = "-";
                    dataVm.Faktur.NO_SO = pesananInDb.NO_BUKTI;
                    dataVm.Faktur.CUST = pesananInDb.CUST;
                    dataVm.Faktur.NAMAPEMESAN = (pesananInDb.NAMAPEMESAN.Length > 20 ? pesananInDb.NAMAPEMESAN.Substring(0, 17) + "..." : pesananInDb.NAMAPEMESAN);
                    dataVm.Faktur.PEMESAN = pesananInDb.PEMESAN;
                    dataVm.Faktur.NAMA_CUST = ErasoftDbContext.ARF01.Single(p => p.CUST == dataVm.Faktur.CUST).PERSO;
                    //dataVm.Faktur.AL = ErasoftDbContext.ARF01.Single(p => p.CUST == dataVm.Faktur.CUST).AL;
                    dataVm.Faktur.AL = pesananInDb.ALAMAT_KIRIM;
                    dataVm.Faktur.AL2 = ErasoftDbContext.ARF01.Single(p => p.CUST == dataVm.Faktur.CUST).AL2;
                    dataVm.Faktur.AL3 = ErasoftDbContext.ARF01.Single(p => p.CUST == dataVm.Faktur.CUST).AL3;
                    dataVm.Faktur.TGL = DateTime.Now;
                    dataVm.Faktur.PPN_Bln_Lapor = Convert.ToByte(dataVm.Faktur.TGL.ToString("MM"));
                    dataVm.Faktur.PPN_Thn_Lapor = Convert.ToByte(dataVm.Faktur.TGL.ToString("yyyy").Substring(2, 2));
                    dataVm.Faktur.USERNAME = uname;
                    dataVm.Faktur.JENIS_RETUR = "-";
                    dataVm.Faktur.JENIS_FORM = "2";
                    dataVm.Faktur.STATUS = "1";
                    dataVm.Faktur.ST_POSTING = "T";
                    dataVm.Faktur.VLT = "IDR";
                    dataVm.Faktur.NO_FA_OUTLET = "-";
                    dataVm.Faktur.NO_LPB = "-";
                    dataVm.Faktur.GROUP_LIMIT = "-";
                    dataVm.Faktur.KODE_ANGKUTAN = "-";
                    dataVm.Faktur.JENIS_MOBIL = "-";
                    dataVm.Faktur.JTRAN = "SI";
                    dataVm.Faktur.JENIS = "1";
                    dataVm.Faktur.NAMA_CUST = "-";
                    dataVm.Faktur.TUKAR = 1;
                    dataVm.Faktur.TUKAR_PPN = 1;
                    dataVm.Faktur.SOPIR = "-";
                    dataVm.Faktur.KET = "-";
                    dataVm.Faktur.PPNBM = 0;
                    dataVm.Faktur.NILAI_PPNBM = 0;
                    dataVm.Faktur.KODE_SALES = "-";
                    dataVm.Faktur.KODE_WIL = "-";
                    dataVm.Faktur.U_MUKA = 0;
                    dataVm.Faktur.U_MUKA_FA = 0;
                    dataVm.Faktur.TERM = pesananInDb.TERM;
                    dataVm.Faktur.TGL_JT_TEMPO = pesananInDb.TGL_JTH_TEMPO;

                    //change by calvin 31 okt 2018
                    //dataVm.Faktur.BRUTO = pesananInDb.BRUTO;
                    dataVm.Faktur.BRUTO = pesanan_bruto;
                    //end change by calvin 31 okt 2018

                    dataVm.Faktur.PPN = pesananInDb.PPN;

                    //change by calvin 31 okt 2018
                    //dataVm.Faktur.NILAI_PPN = pesananInDb.NILAI_PPN;
                    dataVm.Faktur.NILAI_PPN = pesanan_nilai_ppn;
                    //end change by calvin 31 okt 2018

                    dataVm.Faktur.DISCOUNT = pesananInDb.DISCOUNT;
                    dataVm.Faktur.NILAI_DISC = pesananInDb.NILAI_DISC;
                    dataVm.Faktur.MATERAI = pesananInDb.ONGKOS_KIRIM;

                    //change by calvin 31 okt 2018
                    //dataVm.Faktur.NETTO = pesananInDb.NETTO;
                    dataVm.Faktur.NETTO = pesanan_netto;
                    //end change by calvin 31 okt 2018

                    dataVm.Faktur.TGLINPUT = DateTime.Now;

                    #region add by calvin 6 juni 2018, agar sit01a field yang penting tidak null
                    if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.NILAI_DISC)))
                    {
                        dataVm.Faktur.NILAI_DISC = 0;
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.NO_SO)))
                    {
                        dataVm.Faktur.NO_SO = "-";
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.NO_REF)))
                    {
                        dataVm.Faktur.NO_REF = "-";
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.DISCOUNT)))
                    {
                        dataVm.Faktur.DISCOUNT = 0;
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.CUST_QQ)))
                    {
                        dataVm.Faktur.CUST_QQ = "-";
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.NAMA_CUST_QQ)))
                    {
                        dataVm.Faktur.NAMA_CUST_QQ = "-";
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.STATUS_LOADING)))
                    {
                        dataVm.Faktur.STATUS_LOADING = "-";
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.NO_PO_CUST)))
                    {
                        dataVm.Faktur.NO_PO_CUST = "-";
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.PENGIRIM)))
                    {
                        dataVm.Faktur.PENGIRIM = "-";
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.NAMAPENGIRIM)))
                    {
                        dataVm.Faktur.NAMAPENGIRIM = "-";
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.ZONA)))
                    {
                        dataVm.Faktur.ZONA = "-";
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.UCAPAN)))
                    {
                        dataVm.Faktur.UCAPAN = "-";
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.N_UCAPAN)))
                    {
                        dataVm.Faktur.N_UCAPAN = "-";
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.PEMESAN)))
                    {
                        dataVm.Faktur.PEMESAN = "-";
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.SUPP)))
                    {
                        dataVm.Faktur.SUPP = "-";
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.KOMISI)))
                    {
                        dataVm.Faktur.KOMISI = 0;
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(dataVm.Faktur.N_KOMISI)))
                    {
                        dataVm.Faktur.N_KOMISI = 0;
                    }
                    #endregion

                    ErasoftDbContext.SIT01A.Add(dataVm.Faktur);
                    ErasoftDbContext.SaveChanges();

                    dataVm.FakturDetail.NO_BUKTI = noOrder;
                    dataVm.FakturDetail.USERNAME = uname;
                    dataVm.FakturDetail.CATATAN = "-";
                    dataVm.FakturDetail.JENIS_FORM = "2";
                    dataVm.FakturDetail.TGLINPUT = DateTime.Now;

                    //add by calvin 8 nov 2018, update stok marketplace
                    List<string> listBrg = new List<string>();
                    //end add by calvin 8 nov 2018

                    foreach (var pesananDetail in listBarangPesananInDb)
                    {
                        #region add by calvin 31 okt 2018, hitung ulang sesuai dengan qty_n, bukan qty
                        double nilai_disc_1 = 0d;
                        double nilai_disc_2 = 0d;
                        double harga = 0d;
                        if (Math.Abs(pesananDetail.DISCOUNT) > 0)
                        {
                            nilai_disc_1 = (pesananDetail.DISCOUNT * pesananDetail.H_SATUAN * (pesananDetail.QTY_N.HasValue ? pesananDetail.QTY_N.Value : 0)) / 100;
                        }
                        else
                        {
                            //req by pak dani, dibuat proporsional jika discount bukan persen, tapi nilai discount, karena bisa lebih besar daripada harga * qty_n
                            nilai_disc_1 = (pesananDetail.NILAI_DISC_1 / pesananDetail.QTY) * (pesananDetail.QTY_N.HasValue ? pesananDetail.QTY_N.Value : 0);
                        }

                        if (Math.Abs(pesananDetail.DISCOUNT_2) > 0)
                        {
                            nilai_disc_2 = (pesananDetail.DISCOUNT * (pesananDetail.H_SATUAN - nilai_disc_1) * (pesananDetail.QTY_N.HasValue ? pesananDetail.QTY_N.Value : 0)) / 100;
                        }
                        else
                        {
                            nilai_disc_2 = (pesananDetail.NILAI_DISC_2 / pesananDetail.QTY) * (pesananDetail.QTY_N.HasValue ? pesananDetail.QTY_N.Value : 0);
                        }

                        harga = pesananDetail.H_SATUAN * (pesananDetail.QTY_N.HasValue ? pesananDetail.QTY_N.Value : 0) - nilai_disc_1 -
                                                  nilai_disc_2;
                        #endregion

                        //change by calvin 31 okt 2018
                        //dataVm.FakturDetail.NILAI_DISC = pesananDetail.NILAI_DISC_1 + pesananDetail.NILAI_DISC_2;
                        dataVm.FakturDetail.NILAI_DISC = nilai_disc_1 + nilai_disc_2;
                        //end change by calvin 31 okt 2018


                        dataVm.FakturDetail.BRG = pesananDetail.BRG;
                        dataVm.FakturDetail.SATUAN = pesananDetail.SATUAN;
                        dataVm.FakturDetail.H_SATUAN = pesananDetail.H_SATUAN;
                        dataVm.FakturDetail.GUDANG = pesananDetail.LOKASI;

                        //change by calvin 31 okt 2018
                        //dataVm.FakturDetail.QTY = pesananDetail.QTY;
                        dataVm.FakturDetail.QTY = pesananDetail.QTY_N.HasValue ? pesananDetail.QTY_N.Value : 0;
                        //end change by calvin 31 okt 2018

                        dataVm.FakturDetail.DISCOUNT = pesananDetail.DISCOUNT;
                        dataVm.FakturDetail.DISCOUNT_2 = pesananDetail.DISCOUNT_2;

                        //change by calvin 31 okt 2018
                        //dataVm.FakturDetail.NILAI_DISC_1 = pesananDetail.NILAI_DISC_1;
                        //dataVm.FakturDetail.NILAI_DISC_2 = pesananDetail.NILAI_DISC_2;
                        //dataVm.FakturDetail.HARGA = pesananDetail.HARGA;
                        dataVm.FakturDetail.NILAI_DISC_1 = nilai_disc_1;
                        dataVm.FakturDetail.NILAI_DISC_2 = nilai_disc_2;
                        dataVm.FakturDetail.HARGA = harga;
                        //end change by calvin 31 okt 2018

                        if (string.IsNullOrEmpty(Convert.ToString(dataVm.FakturDetail.QTY_KIRIM)))
                        {
                            dataVm.FakturDetail.QTY_KIRIM = 0;
                        }
                        if (string.IsNullOrEmpty(Convert.ToString(dataVm.FakturDetail.QTY_RETUR)))
                        {
                            dataVm.FakturDetail.QTY_RETUR = 0;
                        }
                        if (string.IsNullOrEmpty(Convert.ToString(dataVm.FakturDetail.DISCOUNT_3)))
                        {
                            dataVm.FakturDetail.DISCOUNT_3 = 0;
                        }
                        if (string.IsNullOrEmpty(Convert.ToString(dataVm.FakturDetail.DISCOUNT_4)))
                        {
                            dataVm.FakturDetail.DISCOUNT_4 = 0;
                        }
                        if (string.IsNullOrEmpty(Convert.ToString(dataVm.FakturDetail.DISCOUNT_5)))
                        {
                            dataVm.FakturDetail.DISCOUNT_5 = 0;
                        }
                        if (string.IsNullOrEmpty(Convert.ToString(dataVm.FakturDetail.NILAI_DISC_3)))
                        {
                            dataVm.FakturDetail.NILAI_DISC_3 = 0;
                        }
                        if (string.IsNullOrEmpty(Convert.ToString(dataVm.FakturDetail.NILAI_DISC_4)))
                        {
                            dataVm.FakturDetail.NILAI_DISC_4 = 0;
                        }
                        if (string.IsNullOrEmpty(Convert.ToString(dataVm.FakturDetail.NILAI_DISC_5)))
                        {
                            dataVm.FakturDetail.NILAI_DISC_5 = 0;
                        }

                        ErasoftDbContext.SIT01B.Add(dataVm.FakturDetail);
                        ErasoftDbContext.SIT01A.Where(p => p.NO_BUKTI == noOrder && p.JENIS_FORM == "2").Update(p => new SIT01A() { BRUTO = dataVm.Faktur.BRUTO });
                        ErasoftDbContext.SaveChanges();

                        //add by calvin 8 nov 2018, update stok marketplace
                        listBrg.Add(pesananDetail.BRG);
                        //end add by calvin 8 nov 2018
                    }

                    //add by calvin 8 nov 2018, update stok marketplace
                    updateStockMarketPlace(listBrg);
                    //end add by calvin 8 nov 2018

                    // End Bagian Save Faktur Generated
                }


                return Json(pesananInDb.NO_BUKTI, JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    System.Diagnostics.Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
        }

        [HttpGet]
        public ActionResult SaveGudangQty(int? recNum, string gd, int qty)
        {
            var barangPesananInDb = ErasoftDbContext.SOT01B.Single(b => b.NO_URUT == recNum);

            //add by calvin, 22 juni 2018 validasi QOH
            var qtyOnHand = GetQOHSTF08A(barangPesananInDb.BRG, gd);

            if (qtyOnHand + (barangPesananInDb.QTY_N.HasValue ? (barangPesananInDb.LOKASI == gd ? barangPesananInDb.QTY_N.Value : 0) : 0) - qty < 0)
            {
                var vmError = new StokViewModel()
                {

                };
                vmError.Errors.Add("Tidak bisa save, Qty item ( " + barangPesananInDb.BRG + " ) di gudang ( " + gd + " ) sisa ( " + Convert.ToString(qtyOnHand) + " )");
                return Json(vmError, JsonRequestBehavior.AllowGet);
            }
            //}
            //end add by calvin, validasi QOH

            //change by calvin 31 okt 2018, req by pak dani, harusnya update ke qty_n, bukan qty, dan so tidak dihitung ulang
            //barangPesananInDb.QTY = qty;
            barangPesananInDb.LOKASI = gd;
            barangPesananInDb.QTY_N = qty;


            #region remark by calvin 31 okt 2018, req by pak dani, harusnya update ke qty_n, bukan qty, dan so tidak dihitung ulang
            //if (Math.Abs(barangPesananInDb.DISCOUNT) > 0)
            //{
            //    barangPesananInDb.NILAI_DISC_1 = (barangPesananInDb.DISCOUNT * barangPesananInDb.H_SATUAN * qty) / 100;
            //}

            //if (Math.Abs(barangPesananInDb.DISCOUNT_2) > 0)
            //{
            //    barangPesananInDb.NILAI_DISC_2 = (barangPesananInDb.DISCOUNT * (barangPesananInDb.H_SATUAN - barangPesananInDb.NILAI_DISC_1) * qty) / 100;
            //}

            //barangPesananInDb.HARGA = barangPesananInDb.H_SATUAN * qty - barangPesananInDb.NILAI_DISC_1 -
            //                          barangPesananInDb.NILAI_DISC_2;

            //var pesananInDb = ErasoftDbContext.SOT01A.Single(p => p.NO_BUKTI == barangPesananInDb.NO_BUKTI);
            //var listBarangPesanan = ErasoftDbContext.SOT01B.Where(b => b.NO_BUKTI == pesananInDb.NO_BUKTI).ToList();
            //var brutoPesanan = 0d;

            //foreach (var barang in listBarangPesanan)
            //{
            //    brutoPesanan += barang.HARGA;
            //}

            //pesananInDb.BRUTO = brutoPesanan;

            ////add by nurul 6/8/2018
            ////var ppnBaru = 0d;
            //pesananInDb.NILAI_PPN = (pesananInDb.PPN * pesananInDb.BRUTO) / 100;
            ////end add

            //pesananInDb.NETTO = pesananInDb.BRUTO - pesananInDb.NILAI_DISC + pesananInDb.NILAI_PPN + pesananInDb.ONGKOS_KIRIM;
            #endregion
            ErasoftDbContext.SaveChanges();

            return new EmptyResult();
        }

        public ActionResult LihatFakturBarcode(string resi)
        {
            var cekCust = "";
            var cekMP = "";
            if (resi != "-")
            {
                cekCust = ErasoftDbContext.SOT01A.SingleOrDefault(a => a.TRACKING_SHIPMENT == resi).CUST;
                cekMP = ErasoftDbContext.ARF01.SingleOrDefault(a => a.CUST == cekCust).NAMA;
            }
            var resiBr = "";
            if (cekMP == "17")
            {
                resiBr = (resi.Split(']')[resi.Split(']').Length - 1]);
            }
            else
            {
                resiBr = resi;
            }
            return new BarcodeResult(resiBr);
        }

        [HttpGet]
        public ActionResult LihatFaktur(string noBukPesanan)
        {
            string nobuk = noBukPesanan.Substring(0, 2);
            //string nobuk = noBukPesanan.Substring(1,1);
            try
            {
                //change by nurul 3/12/2018
                //var fakturInDb = ErasoftDbContext.SIT01A.Single(f => f.NO_SO == noBukPesanan);
                if (nobuk == "SO")
                {
                    var fakturInDb = ErasoftDbContext.SIT01A.Single(f => f.NO_SO == noBukPesanan);
                    var namaToko = "";

                    var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
                    if (sessionData?.Account != null)
                    {
                        namaToko = sessionData.Account.NamaTokoOnline;
                    }
                    else
                    {
                        if (sessionData?.User != null)
                        {
                            var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                            namaToko = accFromUser.NamaTokoOnline;
                        }
                    }


                    var cust = ErasoftDbContext.ARF01.Single(c => c.CUST == fakturInDb.CUST);
                    var idMarket = Convert.ToInt32(cust.NAMA);
                    var urlLogoMarket = MoDbContext.Marketplaces.Single(m => m.IdMarket == idMarket).LokasiLogo;
                    var namaPT = ErasoftDbContext.SIFSYS.Single(p => p.BLN == 1).NAMA_PT;
                    //add by nurul 29/11/2018 (modiv cetak faktur)
                    var alamat = ErasoftDbContext.SIFSYS.Single(a => a.BLN == 1).ALAMAT_PT;
                    var tlp = ErasoftDbContext.SIFSYS_TAMBAHAN.Single().TELEPON;
                    //end add
                    //add by nurul 2/1/2019 (tambah no referensi)
                    var noRef = ErasoftDbContext.SOT01A.SingleOrDefault(a => a.NO_BUKTI == noBukPesanan).NO_REFERENSI;
                    //end add 
                    //add by nurul 28/1/2019 
                    var market = MoDbContext.Marketplaces.Single(a => a.IdMarket == idMarket).NamaMarket;
                    var kurir = ErasoftDbContext.SOT01A.SingleOrDefault(a => a.NO_BUKTI == noBukPesanan).NAMAPENGIRIM;
                    var resi = ErasoftDbContext.SOT01A.SingleOrDefault(a => a.NO_BUKTI == noBukPesanan).TRACKING_SHIPMENT;

                    //end add by nurul 28/1/2019 

                    var vm = new FakturViewModel()
                    {
                        NamaToko = namaToko,
                        NamaPerusahaan = namaPT,
                        LogoMarket = urlLogoMarket,
                        Faktur = fakturInDb,
                        ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                        //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                        ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                        ListFakturDetail = ErasoftDbContext.SIT01B.Where(fd => fd.NO_BUKTI == fakturInDb.NO_BUKTI).ToList(),
                        //add by nurul nurul 29/11/2018 (modiv cetak faktur)
                        AlamatToko = alamat,
                        TlpToko = tlp,
                        //end add
                        //add by nurul 2/1/2019 (tambah no referensi)
                        noRef = noRef,
                        //end add 
                        //add by nurul 28/1/2019 
                        Kurir = kurir,
                        Marketplace = market,
                        NoResi = resi
                        //end add by nurul 28/1/2019 

                    };

                    return View(vm);
                }
                else
                {
                    var fakturInDb = ErasoftDbContext.SIT01A.Single(f => f.NO_BUKTI == noBukPesanan);
                    var namaToko = "";

                    var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
                    if (sessionData?.Account != null)
                    {
                        namaToko = sessionData.Account.NamaTokoOnline;
                    }
                    else
                    {
                        if (sessionData?.User != null)
                        {
                            var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                            namaToko = accFromUser.NamaTokoOnline;
                        }
                    }


                    var cust = ErasoftDbContext.ARF01.Single(c => c.CUST == fakturInDb.CUST);
                    var idMarket = Convert.ToInt32(cust.NAMA);
                    var urlLogoMarket = MoDbContext.Marketplaces.Single(m => m.IdMarket == idMarket).LokasiLogo;
                    var namaPT = ErasoftDbContext.SIFSYS.Single(p => p.BLN == 1).NAMA_PT;
                    //add by nurul 29/11/2018 (modiv cetak faktur)
                    var alamat = ErasoftDbContext.SIFSYS.Single(a => a.BLN == 1).ALAMAT_PT;
                    var tlp = ErasoftDbContext.SIFSYS_TAMBAHAN.Single().TELEPON;
                    //end add
                    //add by nurul 2/1/2019 (tambah no referensi)
                    var noRef = "";
                    var kurir = "";
                    var resi = "";
                    if (fakturInDb.NO_SO == null || fakturInDb.NO_SO == "" || fakturInDb.NO_SO == "-")
                    {
                        noRef = "-";
                        kurir = "-";
                        resi = "-";
                    }
                    else
                    {
                        noRef = ErasoftDbContext.SOT01A.SingleOrDefault(a => a.NO_BUKTI == fakturInDb.NO_SO).NO_REFERENSI;
                        kurir = ErasoftDbContext.SOT01A.SingleOrDefault(a => a.NO_BUKTI == fakturInDb.NO_SO).NAMAPENGIRIM;
                        resi = ErasoftDbContext.SOT01A.SingleOrDefault(a => a.NO_BUKTI == fakturInDb.NO_SO).TRACKING_SHIPMENT;
                    }
                    //end add 
                    //add by nurul 28/1/2019 
                    var market = MoDbContext.Marketplaces.Single(a => a.IdMarket == idMarket).NamaMarket;
                    //var barcode = new Object();
                    //BarcodeLib.Barcode b;
                    //b = new BarcodeLib.Barcode();
                    //b.Alignment = BarcodeLib.AlignmentPositions.CENTER;
                    //BarcodeLib.TYPE type = BarcodeLib.TYPE.CODE128;
                    //try
                    //{
                    //    if (type != BarcodeLib.TYPE.UNSPECIFIED)
                    //    {
                    //        b.IncludeLabel = true;
                    //        //b.RotateFlipType = (RotateFlipType)Enum.Parse(typeof(RotateFlipType), "rotatenonflipnone", true);
                    //        if (resi != null || resi != "")
                    //        {
                    //            barcode = b.Encode(type, resi.Trim(), 300, 60);
                    //        }
                    //    }
                    //}
                    ////catch (Exception ex)
                    ////{
                    ////    Erasoft.Function.MessageBox.Show(ex.Message);
                    ////}
                    //catch (Exception e)
                    //{
                    //    return JsonErrorMessage(e.Message);
                    //}
                    //end add by nurul 28/1/2019 

                    var vm = new FakturViewModel()
                    {
                        NamaToko = namaToko,
                        NamaPerusahaan = namaPT,
                        LogoMarket = urlLogoMarket,
                        Faktur = fakturInDb,
                        ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                        //ListBarang = ErasoftDbContext.STF02.ToList(), 'change by nurul 21/1/2019
                        ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                        ListFakturDetail = ErasoftDbContext.SIT01B.Where(fd => fd.NO_BUKTI == fakturInDb.NO_BUKTI).ToList(),
                        //add by nurul nurul 29/11/2018 (modiv cetak faktur)
                        AlamatToko = alamat,
                        TlpToko = tlp,
                        //end add
                        //add by nurul 2/1/2019 (tambah no referensi)
                        noRef = noRef,
                        //end add 
                        //add by nurul 28/1/2019 
                        Kurir = kurir,
                        Marketplace = market,
                        NoResi = resi
                        //end add by nurul 28/1/2019 
                    };

                    return View(vm);
                }
                //var fakturInDb = a;
                //end change 

                //var namaToko = "";

                //var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
                //if (sessionData?.Account != null)
                //{
                //    namaToko = sessionData.Account.NamaTokoOnline;
                //}
                //else
                //{
                //    if (sessionData?.User != null)
                //    {
                //        var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                //        namaToko = accFromUser.NamaTokoOnline;
                //    }
                //}


                //var cust = ErasoftDbContext.ARF01.Single(c => c.CUST == fakturInDb.CUST);
                //var idMarket = Convert.ToInt32(cust.NAMA);
                //var urlLogoMarket = MoDbContext.Marketplaces.Single(m => m.IdMarket == idMarket).LokasiLogo;
                //var namaPT = ErasoftDbContext.SIFSYS.Single(p => p.BLN == 1).NAMA_PT;
                ////add by nurul 29/11/2018 (modiv cetak faktur)
                //var alamat = ErasoftDbContext.SIFSYS.Single(a => a.BLN == 1).ALAMAT_PT;
                //var tlp = ErasoftDbContext.SIFSYS_TAMBAHAN.Single().TELEPON;
                ////end add 

                //var vm = new FakturViewModel()
                //{
                //    NamaToko = namaToko,
                //    NamaPerusahaan = namaPT,
                //    LogoMarket = urlLogoMarket,
                //    Faktur = fakturInDb,
                //    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                //    ListBarang = ErasoftDbContext.STF02.ToList(),
                //    ListFakturDetail = ErasoftDbContext.SIT01B.Where(fd => fd.NO_BUKTI == fakturInDb.NO_BUKTI).ToList(),
                //    //add by nurul nurul 29/11/2018 (modiv cetak faktur)
                //    AlamatToko = alamat,
                //    TlpToko=tlp
                //    //end add 
                //};

                //return View(vm);
            }
            catch (Exception)
            {
                return View("NotFoundPage");
            }
        }

        //add by nurul 3/12/2018
        //[HttpGet]
        //public ActionResult CetakFaktur(string noBukPesanan)
        //{
        //    string nobuk = Convert.ToString(noBukPesanan.Split(2));
        //    try
        //    {
        //        if (nobuk == "SO") {

        //        }
        //        var fakturInDb = ErasoftDbContext.SIT01A.Single(f => f.NO_BUKTI == noBukPesanan);
        //        var namaToko = "";

        //        var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        //        if (sessionData?.Account != null)
        //        {
        //            namaToko = sessionData.Account.NamaTokoOnline;
        //        }
        //        else
        //        {
        //            if (sessionData?.User != null)
        //            {
        //                var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
        //                namaToko = accFromUser.NamaTokoOnline;
        //            }
        //        }


        //        var cust = ErasoftDbContext.ARF01.Single(c => c.CUST == fakturInDb.CUST);
        //        var idMarket = Convert.ToInt32(cust.NAMA);
        //        var urlLogoMarket = MoDbContext.Marketplaces.Single(m => m.IdMarket == idMarket).LokasiLogo;
        //        var namaPT = ErasoftDbContext.SIFSYS.Single(p => p.BLN == 1).NAMA_PT;
        //        //add by nurul 29/11/2018 (modiv cetak faktur)
        //        var alamat = ErasoftDbContext.SIFSYS.Single(a => a.BLN == 1).ALAMAT_PT;
        //        var tlp = ErasoftDbContext.SIFSYS_TAMBAHAN.Single().TELEPON;
        //        //end add 

        //        var vm = new FakturViewModel()
        //        {
        //            NamaToko = namaToko,
        //            NamaPerusahaan = namaPT,
        //            LogoMarket = urlLogoMarket,
        //            Faktur = fakturInDb,
        //            ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
        //            ListBarang = ErasoftDbContext.STF02.ToList(),
        //            ListFakturDetail = ErasoftDbContext.SIT01B.Where(fd => fd.NO_BUKTI == fakturInDb.NO_BUKTI).ToList(),
        //            //add by nurul nurul 29/11/2018 (modiv cetak faktur)
        //            AlamatToko = alamat,
        //            TlpToko = tlp
        //            //end add 
        //        };

        //        return View(vm);
        //    }
        //    catch (Exception)
        //    {
        //        return View("NotFoundPage");
        //    }
        //}
        //end add 

        // =============================================== Bagian Pesanan (END)

        // =============================================== Bagian Supplier (START)

        [Route("manage/master/supplier")]
        public ActionResult SupplierMenu()
        {
            var vm = new SupplierViewModel()
            {
                ListSupplier = ErasoftDbContext.APF01.ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public ActionResult SaveSupplier(SupplierViewModel dataVm)
        {
            if (!ModelState.IsValid)
            {
                return View("SupplierMenu", dataVm);
            }

            if (dataVm.Supplier.RecNum == null)
            {
                var suppInDb = ErasoftDbContext.APF01.SingleOrDefault(s => s.SUPP == dataVm.Supplier.SUPP);

                if (suppInDb != null)
                {
                    ModelState.AddModelError("", $@"Supplier dengan kode {dataVm.Supplier.SUPP} sudah ada! Coba kode yang lain!");

                    var vm = new SupplierViewModel()
                    {
                        Supplier = dataVm.Supplier,
                        ListSupplier = ErasoftDbContext.APF01.ToList()
                    };

                    return View("SupplierMenu", vm);
                }

                ErasoftDbContext.APF01.Add(dataVm.Supplier);
            }
            else
            {
                var suppInDb = ErasoftDbContext.APF01.Single(s => s.RecNum == dataVm.Supplier.RecNum);

                suppInDb.NAMA = dataVm.Supplier.NAMA;
                suppInDb.AL = dataVm.Supplier.AL;
                suppInDb.AL2 = dataVm.Supplier.AL2;
                suppInDb.AL3 = dataVm.Supplier.AL3;
                suppInDb.AL4 = dataVm.Supplier.AL4;
                suppInDb.PERSO = dataVm.Supplier.PERSO;
                suppInDb.NPWP = dataVm.Supplier.NPWP;
                suppInDb.TLP = dataVm.Supplier.TLP;
                suppInDb.TERM = dataVm.Supplier.TERM;
            }

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            var partialVm = new SupplierViewModel()
            {
                ListSupplier = ErasoftDbContext.APF01.OrderBy(f => f.NAMA).ToList()
            };

            return PartialView("TableSupplierPartial", partialVm);
        }

        public ActionResult EditSupplier(int? recNum)
        {
            try
            {
                var supVm = new SupplierViewModel()
                {
                    Supplier = ErasoftDbContext.APF01.Single(c => c.RecNum == recNum),
                    ListSupplier = ErasoftDbContext.APF01.ToList()
                };

                return Json(supVm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult DeleteSupplier(int? recNum)
        {
            var suppInDb = ErasoftDbContext.APF01.Single(c => c.RecNum == recNum);

            //add by nurul 30/7/2018
            var vmError = new StokViewModel() { };

            var cekFaktur = ErasoftDbContext.SIT01A.Count(k => k.SUPP == suppInDb.SUPP);
            var cekPembelian = ErasoftDbContext.PBT01A.Count(k => k.SUPP == suppInDb.SUPP);
            var cekTransaksi = ErasoftDbContext.STT01A.Count(k => k.Supp == suppInDb.SUPP);
            var cekPesanan = ErasoftDbContext.SOT01A.Count(k => k.SUPP == suppInDb.SUPP);

            if (cekFaktur > 0 || cekPembelian > 0 || cekTransaksi > 0 || cekPesanan > 0)
            {
                vmError.Errors.Add("Supplier sudah dipakai di transaksi !");
                return Json(vmError, JsonRequestBehavior.AllowGet);
            }
            //end add

            ErasoftDbContext.APF01.Remove(suppInDb);
            ErasoftDbContext.SaveChanges();

            var partialVm = new SupplierViewModel()
            {
                ListSupplier = ErasoftDbContext.APF01.OrderBy(s => s.NAMA).ToList()
            };

            return PartialView("TableSupplierPartial", partialVm);
        }

        // =============================================== Bagian Supplier (END)

        // =============================================== Bagian SA. Hutang (START)

        [Route("manage/sa/hutang")]
        public ActionResult HutangMenu()
        {
            var vm = new SaHutangViewModel()
            {
                ListHutang = ErasoftDbContext.APT01A.Where(b => b.RANGKA == "1").ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public ActionResult GetSupplier()
        {
            var supplier = ErasoftDbContext.APF01.ToList();

            return Json(supplier, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveHutang(SaHutangViewModel dataVm)
        {
            if (!ModelState.IsValid)
            {
                return View("HutangMenu", dataVm);
            }

            if (dataVm.Hutang.RECNUM == null)
            {
                var listPesananInDb = ErasoftDbContext.APT01A.OrderBy(p => p.RECNUM).ToList();
                var digitAkhir = "";
                var noHutang = "";

                if (listPesananInDb.Count == 0)
                {
                    digitAkhir = "000001";
                    noHutang = $"AP{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                    ErasoftDbContext.Database.ExecuteSqlCommand("DBCC CHECKIDENT (APT01A, RESEED, 0)");
                }
                else
                {
                    var lastRecNum = listPesananInDb.Last().RECNUM;
                    lastRecNum++;

                    digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                    noHutang = $"AP{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                }

                dataVm.Hutang.INV = noHutang;
                dataVm.Hutang.NETTO = dataVm.Hutang.TOTAL;

                ErasoftDbContext.APT01A.Add(dataVm.Hutang);
            }
            else
            {
                var hutangInDb = ErasoftDbContext.APT01A.Single(h => h.RECNUM == dataVm.Hutang.RECNUM);

                hutangInDb.TGL = dataVm.Hutang.TGL;
                hutangInDb.SUPP = dataVm.Hutang.SUPP;
                hutangInDb.NSUPP = dataVm.Hutang.NSUPP;
                hutangInDb.TERM = dataVm.Hutang.TERM;
                hutangInDb.NETTO = dataVm.Hutang.TOTAL;
                hutangInDb.TOTAL = dataVm.Hutang.TOTAL;
                hutangInDb.JTGL = dataVm.Hutang.JTGL;
            }

            dataVm.Hutang.KET = "-";
            dataVm.Hutang.PO = "";
            dataVm.Hutang.SATUAN = "";
            dataVm.Hutang.F_PAJAK = "";
            dataVm.Hutang.INV_2 = "-";
            dataVm.Hutang.RANGKA = "1";
            dataVm.Hutang.MESIN = "";
            dataVm.Hutang.TAHUN = 0;

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            return RedirectToAction("HutangMenu");
        }

        public ActionResult RefreshTableHutang()
        {
            var vm = new SaHutangViewModel()
            {
                ListHutang = ErasoftDbContext.APT01A.Where(b => b.RANGKA == "1").ToList()
            };

            return PartialView("TableHutangPartial", vm);
        }

        public ActionResult RefreshHutangForm()
        {
            try
            {
                var vm = new SaHutangViewModel();

                return PartialView("FormHutangPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult EditHutang(int? recNum)
        {
            try
            {
                var hutVm = new SaHutangViewModel()
                {
                    Hutang = ErasoftDbContext.APT01A.Where(b => b.RANGKA == "1").Single(h => h.RECNUM == recNum)
                };

                return PartialView("FormHutangPartial", hutVm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult DeleteHutang(int? recNum)
        {
            var hutangInDb = ErasoftDbContext.APT01A.Where(b => b.RANGKA == "1").Single(h => h.RECNUM == recNum);

            ErasoftDbContext.APT01A.Remove(hutangInDb);
            ErasoftDbContext.SaveChanges();

            var vm = new SaHutangViewModel()
            {
                ListHutang = ErasoftDbContext.APT01A.Where(b => b.RANGKA == "1").ToList()
            };

            return PartialView("TableHutangPartial", vm);
        }

        // =============================================== Bagian SA. Hutang (END)

        // =============================================== Bagian SA. Piutang (START)

        [Route("manage/sa/piutang")]
        public ActionResult PiutangMenu()
        {
            var vm = new SaPiutangViewModel()
            {
                ListPiutang = ErasoftDbContext.ART01A.Where(b => b.RANGKA == "1").ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public ActionResult GetCustomer()
        {
            var supplier = ErasoftDbContext.ARF01.ToList();

            return Json(supplier, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SavePiutang(SaPiutangViewModel dataVm)
        {
            if (!ModelState.IsValid)
            {
                return View("PiutangMenu", dataVm);
            }

            if (dataVm.Piutang.RecNum == null)
            {
                var listPiutangInDb = ErasoftDbContext.ART01A.OrderBy(p => p.RecNum).ToList();
                var digitAkhir = "";
                var noHutang = "";

                if (listPiutangInDb.Count == 0)
                {
                    digitAkhir = "000001";
                    noHutang = $"AR{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                    ErasoftDbContext.Database.ExecuteSqlCommand("DBCC CHECKIDENT (ART01A, RESEED, 0)");
                }
                else
                {
                    var lastRecNum = listPiutangInDb.Last().RecNum;
                    lastRecNum++;

                    digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                    noHutang = $"AR{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                }

                dataVm.Piutang.FAKTUR = noHutang;

                ErasoftDbContext.ART01A.Add(dataVm.Piutang);

            }
            else
            {

                var piutangInDb = ErasoftDbContext.ART01A.Single(h => h.RecNum == dataVm.Piutang.RecNum);

                piutangInDb.TGL = dataVm.Piutang.TGL;
                piutangInDb.CUST = dataVm.Piutang.CUST;
                piutangInDb.NCUST = dataVm.Piutang.NCUST;
                piutangInDb.TERM = dataVm.Piutang.TERM;
                piutangInDb.TOTAL = dataVm.Piutang.TOTAL;
                piutangInDb.JTGL = dataVm.Piutang.JTGL;
            }

            //add by nurul 27/9/2018
            var vmError = new StokViewModel() { };
            var date1 = dataVm.Piutang.TGL.Value.Year;
            if (date1 > 2078)
            {
                vmError.Errors.Add("Maximum Year is 2078 !");
                return Json(vmError, JsonRequestBehavior.AllowGet);
            }
            if (dataVm.Piutang.CUST == null)
            {
                vmError.Errors.Add("Customer is null !");
                return Json(vmError, JsonRequestBehavior.AllowGet);
            }

            //end add 

            dataVm.Piutang.KET = "-";
            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            return RedirectToAction("PiutangMenu");
        }

        public ActionResult RefreshTablePiutang()
        {
            var vm = new SaPiutangViewModel()
            {
                //ListPiutang = ErasoftDbContext.ART01A.ToList()
                ListPiutang = ErasoftDbContext.ART01A.Where(b => b.RANGKA == "1").ToList()
            };

            return PartialView("TablePiutangPartial", vm);
        }

        public ActionResult RefreshPiutangForm()
        {
            try
            {
                var vm = new SaPiutangViewModel();

                return PartialView("FormPiutangPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult EditPiutang(int? recNum)
        {
            try
            {
                var piuVm = new SaPiutangViewModel()
                {
                    Piutang = ErasoftDbContext.ART01A.Where(b => b.RANGKA == "1").Single(h => h.RecNum == recNum)
                };

                return PartialView("FormPiutangPartial", piuVm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult DeletePiutang(int? recNum)
        {
            var piutangInDb = ErasoftDbContext.ART01A.Where(b => b.RANGKA == "1").Single(h => h.RecNum == recNum);

            ErasoftDbContext.ART01A.Remove(piutangInDb);
            ErasoftDbContext.SaveChanges();

            var vm = new SaPiutangViewModel()
            {
                ListPiutang = ErasoftDbContext.ART01A.Where(b => b.RANGKA == "1").ToList()
            };

            return PartialView("TablePiutangPartial", vm);
        }

        // =============================================== Bagian SA. Piutang (END)

        // =============================================== Bagian Rekening (START)

        [Route("manage/master/rekening")]
        public ActionResult RekeningMenu()
        {
            var vm = new RekeningViewModel()
            {
                ListRekening = ErasoftDbContext.GLFREKs.ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public ActionResult SaveRekening(RekeningViewModel dataVm)
        {
            if (!ModelState.IsValid)
            {
                return View("RekeningMenu", dataVm);
            }

            if (dataVm.Rekening.RecNum == null)
            {
                var checkData = ErasoftDbContext.GLFREKs.SingleOrDefault(r => r.KODE == dataVm.Rekening.KODE);

                if (checkData == null)
                {
                    ErasoftDbContext.GLFREKs.Add(dataVm.Rekening);
                }
                else
                {
                    ModelState.AddModelError("", $@"Rekening dengan kode {dataVm.Rekening.KODE} sudah dipakai oleh Anda / orang lain! Coba kode yang lain!");

                    var rekVm = new RekeningViewModel()
                    {
                        Rekening = dataVm.Rekening,
                        ListRekening = ErasoftDbContext.GLFREKs.ToList()
                    };

                    return View("RekeningMenu", rekVm);
                }
            }
            else
            {
                var rekInDb = ErasoftDbContext.GLFREKs.Single(r => r.RecNum == dataVm.Rekening.RecNum);

                rekInDb.NAMA = dataVm.Rekening.NAMA;
                rekInDb.JR = dataVm.Rekening.JR;
                rekInDb.KATEGORY = dataVm.Rekening.KATEGORY;
            }

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            var partialVm = new RekeningViewModel()
            {
                ListRekening = ErasoftDbContext.GLFREKs.ToList()
            };

            return PartialView("TableRekeningPartial", partialVm);
        }

        public ActionResult EditRekening(int? recNum)
        {
            try
            {
                var rekVm = new RekeningViewModel()
                {
                    Rekening = ErasoftDbContext.GLFREKs.Single(r => r.RecNum == recNum),
                    ListRekening = ErasoftDbContext.GLFREKs.ToList(),
                };

                return Json(rekVm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult DeleteRekening(int? recNum)
        {
            var rekInDb = ErasoftDbContext.GLFREKs.Single(r => r.RecNum == recNum);

            ErasoftDbContext.GLFREKs.Remove(rekInDb);
            ErasoftDbContext.SaveChanges();

            var partialVm = new RekeningViewModel()
            {
                ListRekening = ErasoftDbContext.GLFREKs.ToList()
            };

            return PartialView("TableRekeningPartial", partialVm);
        }

        // =============================================== Bagian Rekening (END)

        // =============================================== Bagian SA. Stock (START)

        public ActionResult GetGudang()
        {
            var listGudang = ErasoftDbContext.STF18.ToList();

            return Json(listGudang, JsonRequestBehavior.AllowGet);
        }
        //add by nurul 13/12/2018
        [HttpGet]
        public ActionResult GetGudangBarang(string brgId)
        {

            if (brgId != "")
            {
                string sSQL = "SELECT A.BRG, A.GD, B.Nama_Gudang, QOH = ISNULL(SUM(QAWAL+(QM1+QM2+QM3+QM4+QM5+QM6+QM7+QM8+QM9+QM10+QM11+QM12)-(QK1+QK2+QK3+QK4+QK5+QK6+QK7+QK8+QK9+QK10+QK11+QK12)),0) ";
                sSQL += "FROM STF08A A LEFT JOIN STF18 B ON A.GD = B.Kode_Gudang WHERE A.TAHUN=" + DateTime.Now.ToString("yyyy") + " AND A.BRG IN ('" + brgId + "') GROUP BY A.BRG, A.GD, B.Nama_Gudang";
                //change by nurul 8/3/2019 set default gudang dr sifsys
                //var ListQOHPerGD = ErasoftDbContext.Database.SqlQuery<QOH_PER_GD>(sSQL).ToList();
                //return Json(ListQOHPerGD, JsonRequestBehavior.AllowGet);
                var cekgudang = ErasoftDbContext.STF18.Where(a => a.Kode_Gudang == ErasoftDbContext.SIFSYS.FirstOrDefault().GUDANG).ToList();
                var gudang = "";
                if (cekgudang.Count() > 0)
                {
                    gudang = ErasoftDbContext.SIFSYS.SingleOrDefault().GUDANG;
                }
                else
                {
                    gudang = ErasoftDbContext.STF18.FirstOrDefault().Kode_Gudang;
                }
                var vm = new FakturViewModel()
                {
                    ListQOHPerGD = ErasoftDbContext.Database.SqlQuery<QOH_PER_GD>(sSQL).ToList(),
                    //setGd = ErasoftDbContext.SIFSYS.SingleOrDefault().GUDANG
                    setGd = gudang
                };   
                return Json(vm, JsonRequestBehavior.AllowGet);
                //end change by nurul 8/3/2019 set default gudang dr sifsys
            }
            else
            {
                var listGudang = ErasoftDbContext.STF18.ToList();
                return Json(listGudang, JsonRequestBehavior.AllowGet);
            }


            //return Json(ListQOHPerGD, JsonRequestBehavior.AllowGet);
        }
        //end add by nurul

        //add by nurul 11/3/2019 set default gudang dr sifsys
        public ActionResult GetGudangBarangStok(string brgId)
        {

            if (brgId != "")
            {
                string sSQL = "SELECT A.BRG, A.GD, B.Nama_Gudang, QOH = ISNULL(SUM(QAWAL+(QM1+QM2+QM3+QM4+QM5+QM6+QM7+QM8+QM9+QM10+QM11+QM12)-(QK1+QK2+QK3+QK4+QK5+QK6+QK7+QK8+QK9+QK10+QK11+QK12)),0) ";
                sSQL += "FROM STF08A A LEFT JOIN STF18 B ON A.GD = B.Kode_Gudang WHERE A.TAHUN=" + DateTime.Now.ToString("yyyy") + " AND A.BRG IN ('" + brgId + "') GROUP BY A.BRG, A.GD, B.Nama_Gudang";
                var cekgudang = ErasoftDbContext.STF18.Where(a => a.Kode_Gudang == ErasoftDbContext.SIFSYS.FirstOrDefault().GUDANG).ToList();
                var gudang = "";
                if (cekgudang != null)
                {
                    gudang = ErasoftDbContext.SIFSYS.SingleOrDefault().GUDANG;
                }
                else
                {
                    gudang = ErasoftDbContext.STF18.FirstOrDefault().Kode_Gudang;
                }
                var vm = new StokViewModel()
                {
                    ListQOHPerGD = ErasoftDbContext.Database.SqlQuery<QOH_PER_GD>(sSQL).ToList(),
                    //setGd = ErasoftDbContext.SIFSYS.SingleOrDefault().GUDANG
                    setGd = gudang
                };
                return Json(vm, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var listGudang = ErasoftDbContext.STF18.ToList();
                return Json(listGudang, JsonRequestBehavior.AllowGet);
            }
        }
        //end add by nurul 11/3/2019

        [Route("manage/sa/stok")]
        public ActionResult StokMenu()
        {
            var vm = new StokViewModel()
            {
                ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("ST") && a.JAM == 1).ToList(),
                //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListGudang = ErasoftDbContext.STF18.ToList()
            };

            return View(vm);
        }

        public ActionResult SaveStok(StokViewModel dataVm)
        {
            if (!ModelState.IsValid)
            {
                dataVm.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(dataVm, JsonRequestBehavior.AllowGet);
            }

            if (dataVm.Stok.ID == null)
            {
                var listStokInDb = ErasoftDbContext.STT01A.OrderBy(p => p.ID).ToList();
                var digitAkhir = "";
                var noStok = "";

                if (listStokInDb.Count == 0)
                {
                    digitAkhir = "000001";
                    noStok = $"ST{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                    ErasoftDbContext.Database.ExecuteSqlCommand("DBCC CHECKIDENT (STT01A, RESEED, 0)");
                }
                else
                {
                    var lastRecNum = listStokInDb.Last().ID;
                    var lastKode = listStokInDb.Last().Nobuk;
                    lastRecNum++;

                    digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                    noStok = $"ST{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";

                    if (noStok == lastKode)
                    {
                        lastRecNum++;
                        digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                        noStok = $"ST{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                    }
                }

                dataVm.Stok.Nobuk = noStok;
                dataVm.Stok.STATUS_LOADING = "0";
                dataVm.BarangStok.Nobuk = noStok;

                #region add by calvin 14 juni 2018, agar field yg penting di stt01a tidak null
                dataVm.Stok.Satuan = "";
                dataVm.Stok.Ket = "";
                dataVm.Stok.ST_Posting = "";
                dataVm.Stok.MK = "M";
                dataVm.Stok.JTran = "M";
                dataVm.Stok.Ref = "";
                dataVm.Stok.WORK_CENTER = "";
                dataVm.Stok.KLINE = "";
                dataVm.Stok.KODE_ANGKUTAN = "";
                dataVm.Stok.JENIS_MOBIL = "";
                dataVm.Stok.NO_POLISI = "";
                dataVm.Stok.NAMA_SOPIR = "";
                dataVm.Stok.No_PP = "";
                dataVm.Stok.CATATAN_1 = "";
                dataVm.Stok.CATATAN_2 = "";
                dataVm.Stok.CATATAN_3 = "";
                dataVm.Stok.CATATAN_4 = "";
                dataVm.Stok.CATATAN_5 = "";
                dataVm.Stok.CATATAN_6 = "";
                dataVm.Stok.CATATAN_7 = "";
                dataVm.Stok.CATATAN_8 = "";
                dataVm.Stok.CATATAN_9 = "";
                dataVm.Stok.CATATAN_10 = "";
                dataVm.Stok.NOBUK_POQC = "";
                dataVm.Stok.Supp = "";
                dataVm.Stok.NAMA_SUPP = "";
                dataVm.Stok.NO_PL = "";
                dataVm.Stok.NO_FAKTUR = "";
                #endregion

                ErasoftDbContext.STT01A.Add(dataVm.Stok);

                if (dataVm.BarangStok.No == null)
                {
                    #region add by calvin 14 juni 2018, agar field yg penting di stt01b tidak null
                    dataVm.BarangStok.Dr_Gd = "";
                    dataVm.BarangStok.WO = "";
                    dataVm.BarangStok.Rak = "";
                    dataVm.BarangStok.JTran = "M";
                    dataVm.BarangStok.KLINK = "";
                    dataVm.BarangStok.NO_WO = "";
                    dataVm.BarangStok.KET = "";
                    dataVm.BarangStok.BRG_ORIGINAL = "";
                    dataVm.BarangStok.QTY3 = 0;
                    dataVm.BarangStok.BUKTI_DS = "";
                    dataVm.BarangStok.BUKTI_REFF = "";
                    #endregion

                    ErasoftDbContext.STT01B.Add(dataVm.BarangStok);
                }
            }
            else
            {
                var stokInDb = ErasoftDbContext.STT01A.Single(p => p.Nobuk == dataVm.Stok.Nobuk);

                stokInDb.Tgl = dataVm.Stok.Tgl;
                dataVm.BarangStok.Nobuk = dataVm.Stok.Nobuk;

                if (dataVm.BarangStok.No == null)
                {
                    #region add by calvin 14 juni 2018, agar field yg penting di stt01b tidak null
                    dataVm.BarangStok.Dr_Gd = "";
                    dataVm.BarangStok.WO = "";
                    dataVm.BarangStok.Rak = "";
                    dataVm.BarangStok.JTran = "M";
                    dataVm.BarangStok.KLINK = "";
                    dataVm.BarangStok.NO_WO = "";
                    dataVm.BarangStok.KET = "";
                    dataVm.BarangStok.BRG_ORIGINAL = "";
                    dataVm.BarangStok.QTY3 = 0;
                    dataVm.BarangStok.BUKTI_DS = "";
                    dataVm.BarangStok.BUKTI_REFF = "";
                    #endregion

                    ErasoftDbContext.STT01B.Add(dataVm.BarangStok);
                }
            }

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();


            //add by calvin 8 nov 2018, update stok marketplace
            List<string> listBrg = new List<string>();
            listBrg.Add(dataVm.BarangStok.Kobar);
            updateStockMarketPlace(listBrg);
            //end add by calvin 8 nov 2018

            var vm = new StokViewModel()
            {
                Stok = ErasoftDbContext.STT01A.Single(p => p.Nobuk == dataVm.Stok.Nobuk),
                ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("ST") && a.JAM == 1).ToList(),
                ListBarangStok = ErasoftDbContext.STT01B.Where(bs => bs.Nobuk == dataVm.Stok.Nobuk).ToList(),
                //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListGudang = ErasoftDbContext.STF18.ToList()
            };

            return PartialView("BarangStokPartial", vm);
        }

        public ActionResult RefreshTableStok()
        {
            var vm = new StokViewModel()
            {
                ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("ST") && a.JAM == 1).ToList()
            };

            return PartialView("TableStokPartial", vm);
        }

        public ActionResult RefreshStokForm()
        {
            try
            {
                var vm = new StokViewModel()
                {
                    ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("ST") && a.JAM == 1).ToList(),
                    //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                return PartialView("BarangStokPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult EditStok(int? stokId)
        {
            try
            {
                var stokInDb = ErasoftDbContext.STT01A.Where(a => a.JAM == 1).Single(p => p.ID == stokId);

                var vm = new StokViewModel()
                {
                    Stok = stokInDb,
                    ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("ST") && a.JAM == 1).ToList(),
                    ListBarangStok = ErasoftDbContext.STT01B.Where(bs => bs.Nobuk == stokInDb.Nobuk).ToList(),
                    //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                return PartialView("BarangStokPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult DeleteStok(int? stokId)
        {
            var stokInDb = ErasoftDbContext.STT01A.Where(a => a.JAM == 1).Single(p => p.ID == stokId);

            //add by calvin 8 nov 2018, update stok marketplace
            List<string> listBrg = new List<string>();
            //end add by calvin 8 nov 2018

            //add by calvin, 22 juni 2018 validasi QOH
            var stokDetailInDb = ErasoftDbContext.STT01B.Where(b => b.Jenis_Form == stokInDb.Jenis_Form && b.Nobuk == stokInDb.Nobuk).ToList();
            foreach (var item in stokDetailInDb)
            {
                var qtyOnHand = GetQOHSTF08A(item.Kobar, item.Ke_Gd);
                if (qtyOnHand - item.Qty < 0)
                {
                    var vmError = new StokViewModel()
                    {

                    };

                    //change by nurul 18/1/2019 -- var namaItem = ErasoftDbContext.STF02.Where(b => b.BRG == item.Kobar).FirstOrDefault();
                    var namaItem = ErasoftDbContext.STF02.Where(b => b.BRG == item.Kobar && b.TYPE == "3").FirstOrDefault();
                    vmError.Errors.Add("Tidak bisa delete, Qty Barang ( " + item.Kobar + " ) di gudang " + item.Ke_Gd + " sisa ( " + Convert.ToString(qtyOnHand) + " ) untuk item " + namaItem.NAMA + "");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
                //add by calvin 8 nov 2018, update stok marketplace
                listBrg.Add(item.Kobar);
                //end add by calvin 8 nov 2018
            }
            //end add by calvin, validasi QOH

            //add by nurul 18/10/2018
            ErasoftDbContext.STT01B.RemoveRange(stokDetailInDb);
            //end add 

            ErasoftDbContext.STT01A.Remove(stokInDb);
            ErasoftDbContext.SaveChanges();

            //add by calvin 8 nov 2018, update stok marketplace
            updateStockMarketPlace(listBrg);
            //end add by calvin 8 nov 2018

            var vm = new StokViewModel()
            {
                ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("ST") && a.JAM == 1).ToList()
            };

            return PartialView("TableStokPartial", vm);
        }

        [HttpGet]
        public ActionResult DeleteBarangStok(int noUrut)
        {
            try
            {
                var barangStokInDb = ErasoftDbContext.STT01B.Single(b => b.No == noUrut);
                var stokInDb = ErasoftDbContext.STT01A.Where(a => a.JAM == 1).Single(p => p.Nobuk == barangStokInDb.Nobuk);

                //add by calvin, 22 juni 2018 validasi QOH
                var qtyOnHand = GetQOHSTF08A(barangStokInDb.Kobar, barangStokInDb.Ke_Gd);

                if (qtyOnHand - barangStokInDb.Qty < 0)
                {
                    var vmError = new StokViewModel()
                    {

                    };

                    //change by nurul 18/1/2019 -- var namaItem = ErasoftDbContext.STF02.Where(b => b.BRG == barangStokInDb.Kobar).FirstOrDefault();
                    var namaItem = ErasoftDbContext.STF02.Where(b => b.BRG == barangStokInDb.Kobar && b.TYPE == "3").FirstOrDefault();
                    vmError.Errors.Add("Tidak bisa delete, Qty di gudang sisa ( " + Convert.ToString(qtyOnHand) + " ) untuk item " + namaItem.NAMA + "");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
                //end add by calvin, validasi QOH

                ErasoftDbContext.STT01B.Remove(barangStokInDb);
                ErasoftDbContext.SaveChanges();

                //add by calvin 8 nov 2018, update stok marketplace
                List<string> listBrg = new List<string>();
                listBrg.Add(barangStokInDb.Kobar);
                updateStockMarketPlace(listBrg);
                //end add by calvin 8 nov 2018

                var vm = new StokViewModel()
                {
                    Stok = ErasoftDbContext.STT01A.Where(a => a.JAM == 1).Single(p => p.Nobuk == stokInDb.Nobuk),
                    ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("ST") && a.JAM == 1).ToList(),
                    ListBarangStok = ErasoftDbContext.STT01B.Where(bs => bs.Nobuk == stokInDb.Nobuk).ToList(),
                    //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                return PartialView("BarangStokPartial", vm);
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult UpdateStok(UpdateData dataUpdate)
        {
            var stokInDb = ErasoftDbContext.STT01A.Single(p => p.Nobuk == dataUpdate.NoBuktiStok);
            //remark by nurul 25/9/2018
            //stokInDb.TglInput = DateTime.ParseExact(dataUpdate.TglInput, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            stokInDb.Tgl = DateTime.ParseExact(dataUpdate.TglInput, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            ErasoftDbContext.SaveChanges();

            return new EmptyResult();
        }

        // =============================================== Bagian SA. Stock (END)

        // =============================================== Bagian Report (START)

        [Route("manage/reports")]
        public async Task<ActionResult> Reports()
        {
            //BlibliController bliAPI = new BlibliController();
            //BlibliController.BlibliAPIData iden = new BlibliController.BlibliAPIData
            //{
            //    merchant_code = "",
            //    API_client_password = "Serayu112pwt",
            //    API_client_username = "mta-api-ses-60077",
            //    API_secret_key = "Serayu112pwt",
            //    token = "2f7f7d61-d4c9-4e2e-8dc6-07cd0bca06be",
            //    mta_username_email_merchant = "mochhazam@gmail.com",
            //    mta_password_password_merchant = "Serayu112pwt",
            //    idmarket = 12
            //};
            //List<string> listCategory = new List<string>();
            //listCategory.Add("SA-1000049");

            //var Updatecategory = MoDbContext.CategoryBlibli.Where(p => listCategory.Contains(p.CATEGORY_CODE)).ToList();
            //Task.Run(() => bliAPI.UpdateAttributeList(iden, Updatecategory)).Wait();

            //ingat ganti saat publish, by calvin
            //string brgtes = "01.SMKR00.00.3m";
            //List<string> listBrg = new List<string>();
            //listBrg.Add(brgtes);

            //listBrg.Add("01.DCTR00.00.3m");
            //listBrg.Add("03.WNB00.04");

            //updateStockMarketPlace(listBrg);

            //add by calvin 1 maret 2019, tes resize image
            //string urlGambar = "https://s3-ap-southeast-1.amazonaws.com//masteronlinebucket/uploaded-image/7331b819-34d8-4056-9adb-a6ff695092b6.jpg";
            //using (var client = new System.Net.Http.HttpClient())
            //{
            //    var bytes = await client.GetByteArrayAsync(urlGambar);

            //    using (var stream = new MemoryStream(bytes, true))
            //    {
            //        var img = Image.FromStream(stream);
            //        float newResolution = img.Height;
            //        if (img.Width < newResolution)
            //        {
            //            newResolution = img.Width;
            //        }

            //        System.Drawing.Imaging.ImageCodecInfo jpgEncoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);

            //        // Create an Encoder object based on the GUID  
            //        // for the Quality parameter category.  
            //        System.Drawing.Imaging.Encoder myEncoder =
            //            System.Drawing.Imaging.Encoder.Quality;

            //        // Create an EncoderParameters object.  
            //        // An EncoderParameters object has an array of EncoderParameter  
            //        // objects. In this case, there is only one  
            //        // EncoderParameter object in the array.  
            //        System.Drawing.Imaging.EncoderParameters myEncoderParameters = new System.Drawing.Imaging.EncoderParameters(1);

            //        System.Drawing.Imaging.EncoderParameter myEncoderParameter = new System.Drawing.Imaging.EncoderParameter(myEncoder, 90L);
            //        myEncoderParameters.Param[0] = myEncoderParameter;

            //        //img.Save(@"D:\TesResize\img.jpg");
            //        var resizedImage = (Image)BlibliResizeImage(img, Convert.ToInt32(newResolution), Convert.ToInt32(newResolution));
            //        //var resizedImage = (Image)BlibliResizeImageFromStream(stream);
            //        //resizedImage.Save(@"D:\TesResize\resizedImage.jpg", jpgEncoder, myEncoderParameters);
            //        resizedImage.Save(stream, jpgEncoder, myEncoderParameters);

            //        //ImageConverter _imageConverter = new ImageConverter();
            //        //byte[] resizedByteArr = (byte[])_imageConverter.ConvertTo(resizedImage, typeof(byte[]));
            //        stream.ToArray();

            //        //using (var ms = new MemoryStream(resizedByteArr))
            //        //{
            //        //    var img2 = Image.FromStream(ms);
            //        //    img2.Save(@"D:\TesResize\resizedByte.jpg", jpgEncoder, myEncoderParameters);
            //        //}
            //        //resizedImage.Save(@"D:\TesResize\resizedImage.jpg", jpgEncoder, myEncoderParameters);

            //    }
            //}
            //end add by calvin 1 maret 2019, tes resize image

            return View();
        }

        public Bitmap BlibliResizeImage(System.Drawing.Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
        private System.Drawing.Imaging.ImageCodecInfo GetEncoder(System.Drawing.Imaging.ImageFormat format)
        {
            System.Drawing.Imaging.ImageCodecInfo[] codecs = System.Drawing.Imaging.ImageCodecInfo.GetImageDecoders();
            foreach (System.Drawing.Imaging.ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
        public static Bitmap BlibliResizeImageFromStream(MemoryStream stream)
        {
            using (var img = Image.FromStream(stream))
            {
                float newResolution = img.Height;
                if (img.Width < newResolution)
                {
                    newResolution = img.Width;
                }
                var destRect = new Rectangle(0, 0, Convert.ToInt32(newResolution), Convert.ToInt32(newResolution));
                var destImage = new Bitmap(Convert.ToInt32(newResolution), Convert.ToInt32(newResolution));

                //var newWidth = (int)(srcImage.Width * scaleFactor);
                //var newHeight = (int)(srcImage.Height * scaleFactor);
                var newWidth = (int)(newResolution);
                var newHeight = (int)(newResolution);
                using (var newImage = new Bitmap(newWidth, newHeight))
                using (var graphics = Graphics.FromImage(newImage))
                {
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    graphics.DrawImage(img, destRect);
                    //newImage.Save("","Test", System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                return destImage;
            }
        }

        //[Route("manage/report/test")]
        //public ActionResult TestReport()
        //{
        //    return View();
        //}

        //public ActionResult LihatListPembeliPopup()
        //{
        //    var listPembeli = ErasoftDbContext.ARF01C.ToList();

        //    return View("TableListPembeli", listPembeli);
        //}

        // =============================================== Bagian Report (END)

        // =============================================== Bagian Jurnal (START)

        [HttpGet]
        public ActionResult GetRekening()
        {
            var listBarang = ErasoftDbContext.GLFREKs.ToList();

            return Json(listBarang, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveJurnal(JurnalViewModel dataVm)
        {
            if (!ModelState.IsValid)
            {
                dataVm.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(dataVm, JsonRequestBehavior.AllowGet);
            }

            if (dataVm.Jurnal.RecNum == null)
            {
                var listJurnalInDb = ErasoftDbContext.GLFTRAN1.OrderBy(p => p.RecNum).ToList();
                var digitAkhir = "";
                var noOrder = "";

                if (listJurnalInDb.Count == 0)
                {
                    digitAkhir = "000001";
                    noOrder = $"JUR{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                    ErasoftDbContext.Database.ExecuteSqlCommand("DBCC CHECKIDENT (GLFTRAN1, RESEED, 0)");
                }
                else
                {
                    var lastRecNum = listJurnalInDb.Last().RecNum;
                    lastRecNum++;

                    digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                    noOrder = $"JUR{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                }

                dataVm.Jurnal.bukti = noOrder;
                dataVm.JurnalDetail.bukti = noOrder;

                ErasoftDbContext.GLFTRAN1.Add(dataVm.Jurnal);

                if (dataVm.JurnalDetail.no == null)
                {
                    ErasoftDbContext.GLFTRAN2.Add(dataVm.JurnalDetail);
                }
            }
            else
            {
                var jurnalInDb = ErasoftDbContext.GLFTRAN1.Single(p => p.bukti == dataVm.Jurnal.bukti);

                jurnalInDb.tgl = dataVm.Jurnal.tgl;

                dataVm.JurnalDetail.bukti = dataVm.Jurnal.bukti;

                if (dataVm.JurnalDetail.no == null)
                {
                    ErasoftDbContext.GLFTRAN2.Add(dataVm.JurnalDetail);
                }
            }

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            var vm = new JurnalViewModel()
            {
                Jurnal = ErasoftDbContext.GLFTRAN1.Single(p => p.bukti == dataVm.Jurnal.bukti),
                ListJurnalDetail = ErasoftDbContext.GLFTRAN2.Where(pd => pd.bukti == dataVm.Jurnal.bukti).ToList(),
                ListRekening = ErasoftDbContext.GLFREKs.ToList()
            };

            return PartialView("DetailJurnalPartial", vm);
        }

        [HttpPost]
        public ActionResult UpdateJurnal(UpdateData dataUpdate)
        {
            var jurnalInDb = ErasoftDbContext.GLFTRAN1.Single(p => p.bukti == dataUpdate.OrderId);
            jurnalInDb.tdebet = dataUpdate.Debet;
            jurnalInDb.tkredit = dataUpdate.Kredit;
            jurnalInDb.tgl = DateTime.ParseExact(dataUpdate.Tgl, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

            ErasoftDbContext.SaveChanges();

            return new EmptyResult();
        }

        public ActionResult EditJurnal(int? orderId)
        {
            try
            {
                var jurnalInDb = ErasoftDbContext.GLFTRAN1.Single(p => p.RecNum == orderId);

                var vm = new JurnalViewModel()
                {
                    Jurnal = jurnalInDb,
                    ListJurnal = ErasoftDbContext.GLFTRAN1.ToList(),
                    ListJurnalDetail = ErasoftDbContext.GLFTRAN2.Where(pd => pd.bukti == jurnalInDb.bukti).ToList(),
                    ListRekening = ErasoftDbContext.GLFREKs.ToList()
                };

                return PartialView("DetailJurnalPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult DeleteJurnal(int? orderId)
        {
            var jurnalInDb = ErasoftDbContext.GLFTRAN1.Single(p => p.RecNum == orderId);

            ErasoftDbContext.GLFTRAN1.Remove(jurnalInDb);
            ErasoftDbContext.SaveChanges();

            var vm = new JurnalViewModel()
            {
                ListJurnal = ErasoftDbContext.GLFTRAN1.ToList(),
                ListJurnalDetail = ErasoftDbContext.GLFTRAN2.ToList()
            };

            return PartialView("TableJurnalPartial", vm);
        }

        [HttpGet]
        public ActionResult DeleteRekeningJurnal(int noUrut)
        {
            try
            {
                var barangJurnalInDb = ErasoftDbContext.GLFTRAN2.Single(b => b.no == noUrut);
                var jurnalInDb = ErasoftDbContext.GLFTRAN1.Single(p => p.bukti == barangJurnalInDb.bukti);

                if (barangJurnalInDb.dk == "D")
                {
                    jurnalInDb.tdebet -= barangJurnalInDb.nilai;
                }
                else
                {
                    jurnalInDb.tkredit -= barangJurnalInDb.nilai;
                }

                ErasoftDbContext.GLFTRAN2.Remove(barangJurnalInDb);
                ErasoftDbContext.SaveChanges();

                var vm = new JurnalViewModel()
                {
                    Jurnal = ErasoftDbContext.GLFTRAN1.Single(p => p.bukti == jurnalInDb.bukti),
                    ListJurnal = ErasoftDbContext.GLFTRAN1.ToList(),
                    ListJurnalDetail = ErasoftDbContext.GLFTRAN2.Where(pd => pd.bukti == jurnalInDb.bukti).ToList(),
                    ListRekening = ErasoftDbContext.GLFREKs.ToList()
                };

                return PartialView("DetailJurnalPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult RefreshTableJurnal1()
        {
            var vm = new JurnalViewModel()
            {
                ListJurnal = ErasoftDbContext.GLFTRAN1.ToList(),
                ListRekening = ErasoftDbContext.GLFREKs.ToList(),
                ListJurnalDetail = ErasoftDbContext.GLFTRAN2.ToList()
            };

            return PartialView("TableJurnalPartial", vm);
        }

        public ActionResult RefreshJurnalForm()
        {
            try
            {
                var vm = new JurnalViewModel()
                {
                    ListJurnal = ErasoftDbContext.GLFTRAN1.ToList(),
                    ListRekening = ErasoftDbContext.GLFREKs.ToList()
                };

                return PartialView("DetailJurnalPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        // =============================================== Bagian Jurnal (END)

        // =============================================== Bagian Pembayaran Piutang (START)

        [Route("manage/penjualan/piutang")]
        public ActionResult PembayaranPiutang()
        {
            var vm = new BayarPiutangViewModel()
            {
                ListPiutang = ErasoftDbContext.ART03A.ToList(),
                ListPiutangDetail = ErasoftDbContext.ART03B.ToList(),
                ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2").ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public ActionResult GetFakturBelumLunas(string noCust)
        {
            var listFakturBelumLunas = ErasoftDbContext.ART01D.Where(f => f.CUST == noCust && (f.NETTO + f.DEBET - f.KREDIT - f.BAYAR) > 0).ToList();
            var listKodeFaktur = new List<FakturJson>();

            foreach (var faktur in listFakturBelumLunas)
            {
                listKodeFaktur.Add(new FakturJson()
                {
                    RecNum = faktur.RecNum,
                    NO_BUKTI = faktur.FAKTUR,
                    Sisa = (faktur.NETTO + faktur.DEBET - faktur.KREDIT - faktur.BAYAR) ?? 0
                });
            }

            return Json(listKodeFaktur, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveBayarPiutang(BayarPiutangViewModel dataVm)
        {

            if (!ModelState.IsValid)
            {
                dataVm.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(dataVm, JsonRequestBehavior.AllowGet);
            }

            //var piutangBaru = false;

            if (dataVm.Piutang.RecNum == null)
            {
                var listBayarPiutangInDb = ErasoftDbContext.ART03A.OrderBy(p => p.RecNum).ToList();
                var digitAkhir = "";
                var noOrder = "";

                if (listBayarPiutangInDb.Count == 0)
                {
                    digitAkhir = "000001";
                    noOrder = $"CR{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                    ErasoftDbContext.Database.ExecuteSqlCommand("DBCC CHECKIDENT (ART03A, RESEED, 0)");
                }
                else
                {
                    var lastRecNum = listBayarPiutangInDb.Last().RecNum;
                    lastRecNum++;

                    digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                    noOrder = $"CR{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                }

                dataVm.Piutang.BUKTI = noOrder;
                dataVm.PiutangDetail.BUKTI = noOrder;

                ErasoftDbContext.ART03A.Add(dataVm.Piutang);

                //if (dataVm.PiutangDetail.NO == null)
                if (!string.IsNullOrEmpty(dataVm.PiutangDetail.NFAKTUR))
                {
                    ErasoftDbContext.ART03B.Add(dataVm.PiutangDetail);
                }
                //else
                //{
                //    piutangBaru = true;
                //}
            }
            else
            {
                dataVm.PiutangDetail.BUKTI = dataVm.Piutang.BUKTI;
                var piutangInDb = ErasoftDbContext.ART03A.Single(p => p.BUKTI == dataVm.Piutang.BUKTI);

                //if (dataVm.PiutangDetail.NO == null)
                if (!string.IsNullOrEmpty(dataVm.PiutangDetail.NFAKTUR))
                {
                    ErasoftDbContext.ART03B.Add(dataVm.PiutangDetail);
                    piutangInDb.TPOT = piutangInDb.TPOT + dataVm.PiutangDetail.POT;
                    piutangInDb.TBAYAR = piutangInDb.TBAYAR + dataVm.PiutangDetail.BAYAR;
                }
                //else
                //{
                //    //    var detPiutang = ErasoftDbContext.ART03B.Where(p => p.BUKTI == dataVm.Piutang.BUKTI && p.NO == dataVm.PiutangDetail.NO).Single();
                //    //    var oldRecordPot = detPiutang.POT;
                //    //    var oldRecordBayar = detPiutang.BAYAR;

                //    //    detPiutang.POT = dataVm.PiutangDetail.POT;
                //    //    detPiutang.BAYAR = dataVm.PiutangDetail.BAYAR;

                //    //    piutangInDb.TPOT = piutangInDb.TPOT + dataVm.PiutangDetail.POT - oldRecordPot;
                //    //    piutangInDb.TBAYAR = piutangInDb.TBAYAR + dataVm.PiutangDetail.BAYAR - oldRecordBayar;

                //    //}

                //    //add by nurul 10/10/2018
                //    //var piutangInDb = ErasoftDbContext.ART03A.Single(p => p.BUKTI == dataVm.Piutang.BUKTI);
                //    var detailPiutang = ErasoftDbContext.ART03B.Where(p => p.BUKTI == piutangInDb.BUKTI).ToList();
                //    if (detailPiutang.Count == 0)
                //    {
                //        piutangBaru = true;
                //    }
                //    //else
                //    //{
                //    //    piutangInDb.TPOT = piutangInDb.TPOT + dataVm.PiutangDetail.POT;
                //    //    piutangInDb.TBAYAR = piutangInDb.TBAYAR + dataVm.PiutangDetail.BAYAR;
                //}

                //end add
            }

            ErasoftDbContext.SaveChanges();

            if (dataVm.bayarPiutang > 0)
            {
                var piutangInDb = ErasoftDbContext.ART03A.Single(p => p.BUKTI == dataVm.Piutang.BUKTI);
                if (piutangInDb != null)
                {
                    //delete detail
                    var oldDetail = ErasoftDbContext.ART03B.Where(m => m.BUKTI == piutangInDb.BUKTI).ToList();
                    if (oldDetail.Count > 0)
                    {
                        ErasoftDbContext.ART03B.Where(m => m.BUKTI == piutangInDb.BUKTI).Delete();
                        piutangInDb.TBAYAR = 0;
                        ErasoftDbContext.SaveChanges();
                    }

                    var listFaktur = ErasoftDbContext.ART01D.Where(p => p.CUST == dataVm.Piutang.CUST
                                                                && (p.NETTO - p.BAYAR - p.KREDIT + p.DEBET) > 0
                                                                //&& p.TGL <= dataVm.sdTgl && p.TGL >= dataVm.drTgl
                                                                ).OrderBy(p => p.TGL).ToList();

                    if (listFaktur.Count > 0)
                    {
                        var totalSisa = ErasoftDbContext.ART01D.Where(p => p.CUST == dataVm.Piutang.CUST && (p.NETTO - p.BAYAR - p.KREDIT + p.DEBET) > 0)
                            .Sum(p => p.NETTO - p.BAYAR - p.KREDIT + p.DEBET).Value;
                        if (totalSisa >= dataVm.bayarPiutang)
                        {
                            foreach (var faktur in listFaktur)
                            {
                                var detailPembayaran = new ART03B();
                                detailPembayaran.BUKTI = dataVm.Piutang.BUKTI;
                                detailPembayaran.NFAKTUR = faktur.FAKTUR;
                                double sisa = faktur.NETTO.Value - faktur.BAYAR.Value - faktur.KREDIT.Value + faktur.DEBET.Value;
                                detailPembayaran.SISA = sisa;
                                detailPembayaran.BAYAR = dataVm.bayarPiutang >= sisa ? sisa : dataVm.bayarPiutang;
                                detailPembayaran.POT = 0;
                                detailPembayaran.USERNAME = "AUTOLOAD_FAKTUR";
                                try
                                {
                                    ErasoftDbContext.ART03B.Add(detailPembayaran);

                                    //piutangInDb.TPOT = piutangInDb.TPOT + dataVm.PiutangDetail.POT;
                                    piutangInDb.TBAYAR = piutangInDb.TBAYAR + detailPembayaran.BAYAR;

                                    ErasoftDbContext.SaveChanges();

                                    dataVm.bayarPiutang = dataVm.bayarPiutang - detailPembayaran.BAYAR;
                                    if (dataVm.bayarPiutang == 0)
                                        break;
                                }
                                catch (Exception ex)
                                {
                                    return JsonErrorMessage(ex.Message);
                                }
                            }
                        }
                        else
                        {
                            return JsonErrorMessage("Anda tidak dapat melakukan pembayaran dengan Nilai Rp." + String.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:N}", dataVm.bayarPiutang) + "\nNilai sisa faktur untuk customer ini adalah Rp." + String.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:N}", totalSisa));
                        }

                    }
                    else
                    {
                        return JsonErrorMessage("Tidak ditemukan Faktur yang belum lunas");
                    }
                }
                else
                {
                    return JsonErrorMessage("Pembayaran Piutan tidak ditemukan.");
                }

            }

            ModelState.Clear();

            var vm = new BayarPiutangViewModel()
            {
                //Piutang = ErasoftDbContext.ART03A.AsNoTracking().Single(p => p.BUKTI == dataVm.PiutangDetail.BUKTI),
                Piutang = ErasoftDbContext.ART03A.AsNoTracking().Single(p => p.BUKTI == dataVm.Piutang.BUKTI),
                ListPiutangDetail = ErasoftDbContext.ART03B.AsNoTracking().Where(pd => pd.BUKTI == dataVm.Piutang.BUKTI).ToList(),
                ListFaktur = ErasoftDbContext.SIT01A.ToList(),
                ListSisa = ErasoftDbContext.ART01D.Where(s => s.CUST == dataVm.Piutang.CUST).ToList()
            };

            return PartialView("DetailBayarPiutangPartial", vm);
        }

        public ActionResult SaveEditDetail(string bukti, string no, double bayar, double pot)
        {
            var piutangInDB = ErasoftDbContext.ART03A.Where(a => a.BUKTI == bukti).SingleOrDefault();
            if (piutangInDB != null)
            {
                var sNmr = no.Split('-');
                int iNmr = Convert.ToInt32(sNmr[sNmr.Length - 1]);
                var detPiutang = ErasoftDbContext.ART03B.Where(b => b.BUKTI == bukti && b.NO == iNmr).SingleOrDefault();
                if (detPiutang != null)
                {
                    var oldRecordPot = detPiutang.POT;
                    var oldRecordBayar = detPiutang.BAYAR;

                    detPiutang.POT = pot;
                    detPiutang.BAYAR = bayar;

                    piutangInDB.TPOT = piutangInDB.TPOT + pot - oldRecordPot;
                    piutangInDB.TBAYAR = piutangInDB.TBAYAR + bayar - oldRecordBayar;

                    ErasoftDbContext.SaveChanges();

                    var vm = new BayarPiutangViewModel()
                    {
                        Piutang = ErasoftDbContext.ART03A.AsNoTracking().Single(p => p.BUKTI == bukti),
                        ListPiutangDetail = ErasoftDbContext.ART03B.AsNoTracking().Where(pd => pd.BUKTI == bukti).ToList(),
                        ListFaktur = ErasoftDbContext.SIT01A.ToList(),
                        ListSisa = ErasoftDbContext.ART01D.Where(s => s.CUST == piutangInDB.CUST).ToList()
                    };

                    return PartialView("DetailBayarPiutangPartial", vm);
                }
                else
                {
                    return JsonErrorMessage("Piutang detail not found");
                }
            }
            return JsonErrorMessage("Piutang not found");
        }

        [HttpPost]
        public ActionResult UpdateBayarPiutang(UpdateData dataUpdate)
        {
            var piutangInDb = ErasoftDbContext.ART03A.Single(p => p.BUKTI == dataUpdate.OrderId);
            piutangInDb.TGL = DateTime.ParseExact(dataUpdate.Tgl, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

            ErasoftDbContext.SaveChanges();

            return new EmptyResult();
        }

        public ActionResult EditBayarPiutang(int? orderId)
        {
            try
            {
                var piutangInDb = ErasoftDbContext.ART03A.Single(p => p.RecNum == orderId);

                var vm = new BayarPiutangViewModel()
                {
                    Piutang = piutangInDb,
                    ListPiutang = ErasoftDbContext.ART03A.ToList(),
                    ListPiutangDetail = ErasoftDbContext.ART03B.Where(pd => pd.BUKTI == piutangInDb.BUKTI).ToList(),
                    ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2").ToList()
                };

                return PartialView("DetailBayarPiutangPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult DeleteBayarPiutang(int? orderId)
        {
            var piutangInDb = ErasoftDbContext.ART03A.Single(p => p.RecNum == orderId);

            ErasoftDbContext.ART03A.Remove(piutangInDb);
            ErasoftDbContext.SaveChanges();

            var vm = new BayarPiutangViewModel()
            {
                ListPiutang = ErasoftDbContext.ART03A.ToList(),
                ListPiutangDetail = ErasoftDbContext.ART03B.ToList()
            };

            return PartialView("TableBayarPiutangPartial", vm);
        }

        [HttpGet]
        public ActionResult DeleteDetailBayarPiutang(int noUrut)
        {
            try
            {
                var detailPiutangInDb = ErasoftDbContext.ART03B.Single(b => b.NO == noUrut);
                var piutangInDb = ErasoftDbContext.ART03A.Single(p => p.BUKTI == detailPiutangInDb.BUKTI);

                piutangInDb.TPOT -= detailPiutangInDb.POT;
                piutangInDb.TBAYAR -= detailPiutangInDb.BAYAR;
                ErasoftDbContext.ART03B.Remove(detailPiutangInDb);
                ErasoftDbContext.SaveChanges();

                var vm = new BayarPiutangViewModel()
                {
                    Piutang = ErasoftDbContext.ART03A.Single(p => p.BUKTI == piutangInDb.BUKTI),
                    ListPiutang = ErasoftDbContext.ART03A.ToList(),
                    ListPiutangDetail = ErasoftDbContext.ART03B.Where(pd => pd.BUKTI == piutangInDb.BUKTI).ToList(),
                    ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2").ToList()
                };

                return PartialView("DetailBayarPiutangPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult RefreshTableBayarPiutang1()
        {
            var vm = new BayarPiutangViewModel()
            {
                ListPiutang = ErasoftDbContext.ART03A.ToList()
            };

            return PartialView("TableBayarPiutangPartial", vm);
        }

        public ActionResult RefreshBayarPiutangForm()
        {
            try
            {
                var vm = new BayarPiutangViewModel();

                return PartialView("DetailBayarPiutangPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        // =============================================== Bagian Pembayaran Piutang (END)

        // =============================================== Bagian Pembayaran Hutang (START)

        [Route("manage/pembelian/hutang")]
        public ActionResult PembayaranHutang()
        {
            var vm = new BayarHutangViewModel()
            {
                ListHutang = ErasoftDbContext.APT03A.ToList(),
                ListHutangDetail = ErasoftDbContext.APT03B.ToList(),
                ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2").ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public ActionResult GetInvoiceBelumLunas(string noSupp)
        {
            var listInvoiceBelumLunas = ErasoftDbContext.APT01D.Where(f => f.SUPP == noSupp && (f.NETTO + f.DEBET - f.KREDIT - f.BAYAR) > 0).ToList();
            var listKodeInvoice = new List<InvoiceJson>();

            foreach (var invoice in listInvoiceBelumLunas)
            {
                listKodeInvoice.Add(new InvoiceJson()
                {
                    RecNum = invoice.RECNUM,
                    INV = invoice.INV,
                    Sisa = (invoice.NETTO - invoice.DEBET + invoice.KREDIT - invoice.BAYAR)
                });
            }

            return Json(listKodeInvoice, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveBayarHutang(BayarHutangViewModel dataVm)
        {
            if (!ModelState.IsValid)
            {
                dataVm.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(dataVm, JsonRequestBehavior.AllowGet);
            }

            if (dataVm.Hutang.RecNum == null)
            {
                var listBayarHutangInDb = ErasoftDbContext.APT03A.OrderBy(p => p.RecNum).ToList();
                var digitAkhir = "";
                var noOrder = "";

                if (listBayarHutangInDb.Count == 0)
                {
                    digitAkhir = "000001";
                    noOrder = $"DR{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                    ErasoftDbContext.Database.ExecuteSqlCommand("DBCC CHECKIDENT (APT03A, RESEED, 0)");
                }
                else
                {
                    var lastRecNum = listBayarHutangInDb.Last().RecNum;
                    lastRecNum++;

                    digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                    noOrder = $"DR{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                }

                dataVm.Hutang.BUKTI = noOrder;
                dataVm.HutangDetail.BUKTI = noOrder;

                ErasoftDbContext.APT03A.Add(dataVm.Hutang);

                if (dataVm.HutangDetail.NO == null)
                {
                    ErasoftDbContext.APT03B.Add(dataVm.HutangDetail);
                }
            }
            else
            {
                dataVm.HutangDetail.BUKTI = dataVm.Hutang.BUKTI;

                if (dataVm.HutangDetail.NO == null)
                {
                    ErasoftDbContext.APT03B.Add(dataVm.HutangDetail);
                }

                var hutangInDb = ErasoftDbContext.APT03A.Single(p => p.BUKTI == dataVm.Hutang.BUKTI);

                hutangInDb.TPOT = hutangInDb.TPOT + dataVm.HutangDetail.POT;
                hutangInDb.TBAYAR = hutangInDb.TBAYAR + dataVm.HutangDetail.BAYAR;

            }

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            var vm = new BayarHutangViewModel()
            {
                Hutang = ErasoftDbContext.APT03A.Single(p => p.BUKTI == dataVm.HutangDetail.BUKTI),
                ListHutangDetail = ErasoftDbContext.APT03B.Where(pd => pd.BUKTI == dataVm.Hutang.BUKTI).ToList(),
                ListFaktur = ErasoftDbContext.SIT01A.ToList(),
                ListSisa = ErasoftDbContext.APT01D.Where(s => s.SUPP == dataVm.Hutang.SUPP).ToList()
            };

            return PartialView("DetailBayarHutangPartial", vm);
        }

        [HttpPost]
        public ActionResult UpdateBayarHutang(UpdateData dataUpdate)
        {
            var hutangInDb = ErasoftDbContext.APT03A.Single(p => p.BUKTI == dataUpdate.OrderId);
            hutangInDb.TGL = DateTime.ParseExact(dataUpdate.Tgl, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

            ErasoftDbContext.SaveChanges();

            return new EmptyResult();
        }

        public ActionResult EditBayarHutang(int? orderId)
        {
            try
            {
                var hutangInDb = ErasoftDbContext.APT03A.Single(p => p.RecNum == orderId);

                var vm = new BayarHutangViewModel()
                {
                    Hutang = hutangInDb,
                    ListHutang = ErasoftDbContext.APT03A.ToList(),
                    ListHutangDetail = ErasoftDbContext.APT03B.Where(pd => pd.BUKTI == hutangInDb.BUKTI).ToList(),
                    ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2").ToList()
                };

                return PartialView("DetailBayarHutangPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult DeleteBayarHutang(int? orderId)
        {
            var hutangInDb = ErasoftDbContext.APT03A.Single(p => p.RecNum == orderId);

            ErasoftDbContext.APT03A.Remove(hutangInDb);
            ErasoftDbContext.SaveChanges();

            var vm = new BayarHutangViewModel()
            {
                ListHutang = ErasoftDbContext.APT03A.ToList(),
                ListHutangDetail = ErasoftDbContext.APT03B.ToList()
            };

            return PartialView("TableBayarHutangPartial", vm);
        }

        [HttpGet]
        public ActionResult DeleteDetailBayarHutang(int noUrut)
        {
            try
            {
                var detailHutangInDb = ErasoftDbContext.APT03B.Single(b => b.NO == noUrut);
                var hutangInDb = ErasoftDbContext.APT03A.Single(p => p.BUKTI == detailHutangInDb.BUKTI);

                hutangInDb.TPOT -= detailHutangInDb.POT;
                hutangInDb.TBAYAR -= detailHutangInDb.BAYAR;

                ErasoftDbContext.APT03B.Remove(detailHutangInDb);
                ErasoftDbContext.SaveChanges();

                var vm = new BayarHutangViewModel()
                {
                    Hutang = ErasoftDbContext.APT03A.Single(p => p.BUKTI == hutangInDb.BUKTI),
                    ListHutang = ErasoftDbContext.APT03A.ToList(),
                    ListHutangDetail = ErasoftDbContext.APT03B.Where(pd => pd.BUKTI == hutangInDb.BUKTI).ToList(),
                    ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2").ToList()
                };

                return PartialView("DetailBayarHutangPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult RefreshTableBayarHutang1()
        {
            var vm = new BayarHutangViewModel()
            {
                ListHutang = ErasoftDbContext.APT03A.ToList()
            };

            return PartialView("TableBayarHutangPartial", vm);
        }

        public ActionResult RefreshBayarHutangForm()
        {
            try
            {
                var vm = new BayarHutangViewModel();

                return PartialView("DetailBayarHutangPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        // =============================================== Bagian Pembayaran Hutang (END)

        // =============================================== Bagian Gudang (START)

        [Route("manage/master/gudang")]
        public ActionResult Gudang()
        {
            var vm = new GudangViewModel()
            {
                ListGudang = ErasoftDbContext.STF18.ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public ActionResult SaveGudang(GudangViewModel dataGudang)
        {
            if (!ModelState.IsValid)
            {
                dataGudang.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(dataGudang, JsonRequestBehavior.AllowGet);
            }

            if (dataGudang.Gudang.ID == null)
            {
                var checkData = ErasoftDbContext.STF18.SingleOrDefault(k => k.Kode_Gudang == dataGudang.Gudang.Kode_Gudang);

                if (checkData == null)
                {
                    ErasoftDbContext.STF18.Add(dataGudang.Gudang);
                }
                else
                {
                    dataGudang.Errors.Add($@"Gudang dengan kode {dataGudang.Gudang.Kode_Gudang} sudah dipakai oleh Anda / orang lain! Coba kode yang lain!");
                    return Json(dataGudang, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                //var katInDb = ErasoftDbContext.STF18.Single(k => k.Kode_Gudang == dataGudang.Gudang.Kode_Gudang);
                var katInDb = ErasoftDbContext.STF18.Single(k => k.ID == dataGudang.Gudang.ID);

                katInDb.Nama_Gudang = dataGudang.Gudang.Nama_Gudang;
            }

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            var partialVm = new GudangViewModel()
            {
                ListGudang = ErasoftDbContext.STF18.ToList()
            };

            return PartialView("TableGudangPartial", partialVm);
        }

        public ActionResult EditGudang(int? gudangId)
        {
            try
            {
                var vm = new GudangViewModel()
                {
                    Gudang = ErasoftDbContext.STF18.Single(k => k.ID == gudangId),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                return Json(vm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult DeleteGudang(int? gudangId)
        {
            var gudangInDb = ErasoftDbContext.STF18.Single(k => k.ID == gudangId);

            //ADD BY NURUL 27/7/2018
            var vmError = new StokViewModel() { };

            var cekFaktur = ErasoftDbContext.SIT01B.Count(k => k.GUDANG == gudangInDb.Kode_Gudang);
            var cekPembelian = ErasoftDbContext.PBT01B.Count(k => k.GD == gudangInDb.Kode_Gudang);
            var cekTransaksi = ErasoftDbContext.STT01B.Count(k => k.Dr_Gd == gudangInDb.Kode_Gudang || k.Ke_Gd == gudangInDb.Kode_Gudang);
            var cekPesanan = ErasoftDbContext.SOT01B.Count(k => k.LOKASI == gudangInDb.Kode_Gudang);

            if (cekFaktur > 0 || cekPembelian > 0 || cekTransaksi > 0 || cekPesanan > 0)
            {
                vmError.Errors.Add("Gudang sudah dipakai di transaksi !");
                return Json(vmError, JsonRequestBehavior.AllowGet);
            }
            //END ADD

            ErasoftDbContext.STF18.Remove(gudangInDb);
            ErasoftDbContext.SaveChanges();

            var partialVm = new GudangViewModel()
            {
                ListGudang = ErasoftDbContext.STF18.ToList()
            };

            return PartialView("TableGudangPartial", partialVm);
        }

        // =============================================== Bagian Gudang (END)

        // =============================================== Bagian Transaksi Masuk Barang (START)

        [Route("manage/persediaan/masuk")]
        public ActionResult TransaksiMasukMenu()
        {
            var vm = new StokViewModel()
            {
                ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("IN")).ToList(),
                //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListGudang = ErasoftDbContext.STF18.ToList()
            };

            return View(vm);
        }

        public ActionResult SaveTransaksiMasuk(StokViewModel dataVm)
        {
            if (!ModelState.IsValid)
            {
                dataVm.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(dataVm, JsonRequestBehavior.AllowGet);
            }

            if (dataVm.Stok.ID == null)
            {
                var listStokInDb = ErasoftDbContext.STT01A.OrderBy(p => p.ID).ToList();
                var digitAkhir = "";
                var noStok = "";

                if (listStokInDb.Count == 0)
                {
                    digitAkhir = "000001";
                    noStok = $"IN{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                    ErasoftDbContext.Database.ExecuteSqlCommand("DBCC CHECKIDENT (STT01A, RESEED, 0)");
                }
                else
                {
                    var lastRecNum = listStokInDb.Last().ID;
                    var lastKode = listStokInDb.Last().Nobuk;
                    lastRecNum++;

                    digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                    noStok = $"IN{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";

                    if (noStok == lastKode)
                    {
                        lastRecNum++;
                        digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                        noStok = $"IN{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                    }
                }

                dataVm.Stok.Nobuk = noStok;
                dataVm.Stok.STATUS_LOADING = "0";
                dataVm.BarangStok.Nobuk = noStok;

                ErasoftDbContext.STT01A.Add(dataVm.Stok);

                if (dataVm.BarangStok.No == null)
                {
                    //change by nurul 3/10/2018
                    //ErasoftDbContext.STT01B.Add(dataVm.BarangStok);
                    var vmError = new StokViewModel() { };

                    if (dataVm.BarangStok.Ke_Gd == null || dataVm.BarangStok.Qty == 0 || dataVm.BarangStok.Harga == 0)
                    {
                        vmError.Errors.Add("Silahkan isi semua field terlebih dahulu !");
                        return Json(vmError, JsonRequestBehavior.AllowGet);
                    }

                    ErasoftDbContext.STT01B.Add(dataVm.BarangStok);

                    //end change 
                }
            }
            else
            {
                var stokInDb = ErasoftDbContext.STT01A.Single(p => p.Nobuk == dataVm.Stok.Nobuk);

                stokInDb.Tgl = dataVm.Stok.Tgl;
                dataVm.BarangStok.Nobuk = dataVm.Stok.Nobuk;

                if (dataVm.BarangStok.No == null)
                {
                    //change by nurul 3/10/2018
                    //ErasoftDbContext.STT01B.Add(dataVm.BarangStok);
                    var vmError = new StokViewModel() { };

                    if (dataVm.BarangStok.Ke_Gd == null || dataVm.BarangStok.Qty == 0 || dataVm.BarangStok.Harga == 0)
                    {
                        vmError.Errors.Add("Silahkan isi semua field terlebih dahulu !");
                        return Json(vmError, JsonRequestBehavior.AllowGet);
                    }

                    ErasoftDbContext.STT01B.Add(dataVm.BarangStok);

                    //end change
                }
            }

            #region add by calvin 14 juni 2018, agar field yg penting di stt01b tidak null
            dataVm.BarangStok.Dr_Gd = "";
            dataVm.BarangStok.WO = "";
            dataVm.BarangStok.Rak = "";
            dataVm.BarangStok.JTran = "M";
            dataVm.BarangStok.KLINK = "";
            dataVm.BarangStok.NO_WO = "";
            dataVm.BarangStok.KET = "";
            dataVm.BarangStok.BRG_ORIGINAL = "";
            dataVm.BarangStok.QTY3 = 0;
            dataVm.BarangStok.BUKTI_DS = "";
            dataVm.BarangStok.BUKTI_REFF = "";
            #endregion

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            var vm = new StokViewModel()
            {
                Stok = ErasoftDbContext.STT01A.Single(p => p.Nobuk == dataVm.Stok.Nobuk),
                ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("IN")).ToList(),
                ListBarangStok = ErasoftDbContext.STT01B.Where(bs => bs.Nobuk == dataVm.Stok.Nobuk).ToList(),
                //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListGudang = ErasoftDbContext.STF18.ToList()
            };

            //add by Tri, panggil api marketplace to change stock
            List<string> listBrg = new List<string>();
            //foreach (var brg in vm.ListBarang)
            //{
            listBrg.Add(dataVm.BarangStok.Kobar);
            //}
            updateStockMarketPlace(listBrg);
            //end add by Tri, panggil api marketplace to change stock

            return PartialView("BarangTransaksiMasukPartial", vm);
        }

        public ActionResult RefreshTableTransaksiMasuk()
        {
            var vm = new StokViewModel()
            {
                ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("IN")).ToList()
            };

            return PartialView("TableTransaksiMasukPartial", vm);
        }

        public ActionResult RefreshTransaksiMasukForm()
        {
            try
            {
                var vm = new StokViewModel()
                {
                    ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("IN")).ToList(),
                    //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                return PartialView("BarangTransaksiMasukPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult EditTransaksiMasuk(int? stokId)
        {
            try
            {
                var stokInDb = ErasoftDbContext.STT01A.Single(p => p.ID == stokId);

                var vm = new StokViewModel()
                {
                    Stok = stokInDb,
                    ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("IN")).ToList(),
                    ListBarangStok = ErasoftDbContext.STT01B.Where(bs => bs.Nobuk == stokInDb.Nobuk).ToList(),
                    //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                return PartialView("BarangTransaksiMasukPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult DeleteTransaksiMasuk(int? stokId)
        {
            var stokInDb = ErasoftDbContext.STT01A.Single(p => p.ID == stokId);
            List<string> brg = new List<string>();//add by Tri, 21 agustus 2018
            //add by calvin, 22 juni 2018 validasi QOH
            var stokDetailInDb = ErasoftDbContext.STT01B.Where(b => b.Nobuk == stokInDb.Nobuk).ToList();
            foreach (var item in stokDetailInDb)
            {
                var qtyOnHand = GetQOHSTF08A(item.Kobar, item.Ke_Gd);

                if (qtyOnHand - item.Qty < 0)
                {
                    var vmError = new StokViewModel()
                    {

                    };
                    vmError.Errors.Add("Tidak bisa delete, Qty barang ( " + item.Kobar + " ) di gudang " + item.Ke_Gd + " sisa ( " + Convert.ToString(qtyOnHand) + " )");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
                brg.Add(item.Kobar);//add by Tri, 21 agustus 2018
                //add by nurul 13/9/2018
                ErasoftDbContext.STT01B.Remove(item);
                ErasoftDbContext.SaveChanges();
                //end add by nurul 13/9/2018
            }
            //end add by calvin, validasi QOH

            ErasoftDbContext.STT01A.Remove(stokInDb);
            ErasoftDbContext.SaveChanges();

            var vm = new StokViewModel()
            {
                ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("IN")).ToList()
            };

            //add by Tri, panggil api marketplace to change stock            
            updateStockMarketPlace(brg);
            //end add by Tri, panggil api marketplace to change stock

            return PartialView("TableTransaksiMasukPartial", vm);
        }

        [HttpGet]
        public ActionResult DeleteBarangTransaksiMasuk(int noUrut)
        {
            try
            {
                var barangStokInDb = ErasoftDbContext.STT01B.Single(b => b.No == noUrut);
                var stokInDb = ErasoftDbContext.STT01A.Single(p => p.Nobuk == barangStokInDb.Nobuk);

                //add by calvin, 22 juni 2018 validasi QOH
                var qtyOnHand = GetQOHSTF08A(barangStokInDb.Kobar, barangStokInDb.Ke_Gd);

                if (qtyOnHand - barangStokInDb.Qty < 0)
                {
                    var vmError = new StokViewModel()
                    {

                    };
                    vmError.Errors.Add("Tidak bisa delete, Qty di gudang sisa ( " + Convert.ToString(qtyOnHand) + " )");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
                //end add by calvin, validasi QOH

                //add by Tri, panggil api marketplace to change stock
                List<string> brg = new List<string>();
                brg.Add(barangStokInDb.Kobar);
                //end add by Tri, panggil api marketplace to change stock

                ErasoftDbContext.STT01B.Remove(barangStokInDb);
                ErasoftDbContext.SaveChanges();

                var vm = new StokViewModel()
                {
                    Stok = ErasoftDbContext.STT01A.Single(p => p.Nobuk == stokInDb.Nobuk),
                    ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("IN")).ToList(),
                    ListBarangStok = ErasoftDbContext.STT01B.Where(bs => bs.Nobuk == stokInDb.Nobuk).ToList(),
                    //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                //add by Tri, panggil api marketplace to change stock
                updateStockMarketPlace(brg);
                //end add by Tri, panggil api marketplace to change stock

                return PartialView("BarangTransaksiMasukPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult UpdateTransaksiMasuk(UpdateData dataUpdate)
        {
            var stokInDb = ErasoftDbContext.STT01A.Single(p => p.Nobuk == dataUpdate.NoBuktiStok);
            //remark by nurul 25/9/2018
            //stokInDb.TglInput = DateTime.ParseExact(dataUpdate.TglInput, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            stokInDb.Tgl = DateTime.ParseExact(dataUpdate.TglInput, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            ErasoftDbContext.SaveChanges();

            return new EmptyResult();
        }

        // =============================================== Bagian Transaksi Masuk Barang (END)

        public void updateStockMarketPlace(List<string> listBrg)
        {
            var kdBL = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BUKALAPAK").IdMarket;
            var kdLazada = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "LAZADA").IdMarket;
            var kdBli = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BLIBLI").IdMarket;
            var kdElevenia = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "ELEVENIA").IdMarket;
            var kdShopee = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "SHOPEE").IdMarket;
            var kdTokped = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "TOKOPEDIA").IdMarket;
            var blApi = new BukaLapakController();
            var lzdApi = new LazadaController();
            var eleApi = new EleveniaController();
            foreach (string kdBrg in listBrg)
            {
                var qtyOnHand = GetQOHSTF08A(kdBrg, "ALL");

                var brgMarketplace = ErasoftDbContext.STF02H.Where(p => p.BRG == kdBrg && !string.IsNullOrEmpty(p.BRG_MP)).ToList();
                foreach (var stf02h in brgMarketplace)
                {
                    var marketPlace = ErasoftDbContext.ARF01.SingleOrDefault(p => p.RecNum == stf02h.IDMARKET);
                    if (marketPlace.NAMA.Equals(kdBL.ToString()))
                    {
                        blApi.updateProduk(kdBrg, stf02h.BRG_MP, "", (qtyOnHand > 0) ? qtyOnHand.ToString() : "0", marketPlace.API_KEY, marketPlace.TOKEN);
                    }
                    else if (marketPlace.NAMA.Equals(kdLazada.ToString()))
                    {
                        lzdApi.UpdatePriceQuantity(stf02h.BRG_MP, "", (qtyOnHand > 0) ? qtyOnHand.ToString() : "0", marketPlace.TOKEN);
                    }
                    else if (marketPlace.NAMA.Equals(kdElevenia.ToString()))
                    {
                        var barangInDb = ErasoftDbContext.STF02.SingleOrDefault(b => b.BRG == kdBrg);
                        string[] imgID = new string[3];
                        //change by calvin 4 desember 2018
                        //                        for (int i = 0; i < 3; i++)
                        //                        {
                        //#if AWS
                        //                            imgID[i] = "https://masteronline.co.id/ele/image/" + $"FotoProduk-{barangInDb.USERNAME}-{barangInDb.BRG}-foto-{i + 1}";
                        //#else
                        //                            imgID[i] = "https://dev.masteronline.co.id/ele/image/" + $"FotoProduk-{barangInDb.USERNAME}-{barangInDb.BRG}-foto-{i + 1}";
                        //#endif
                        //                        }
                        for (int i = 0; i < 3; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    imgID[0] = barangInDb.LINK_GAMBAR_1;
                                    break;
                                case 1:
                                    imgID[1] = barangInDb.LINK_GAMBAR_2;
                                    break;
                                case 2:
                                    imgID[2] = barangInDb.LINK_GAMBAR_3;
                                    break;
                            }
                        }
                        //end change by calvin 4 desember 2018

                        EleveniaController.EleveniaProductData data = new EleveniaController.EleveniaProductData
                        {
                            api_key = marketPlace.API_KEY,
                            kode = barangInDb.BRG,
                            nama = barangInDb.NAMA + ' ' + barangInDb.NAMA2 + ' ' + barangInDb.NAMA3,
                            berat = (barangInDb.BERAT / 1000).ToString(),//MO save dalam Gram, Elevenia dalam Kilogram
                            imgUrl = imgID,
                            Keterangan = barangInDb.Deskripsi,
                            Qty = Convert.ToString(qtyOnHand),
                            DeliveryTempNo = stf02h.DeliveryTempElevenia,
                            IDMarket = marketPlace.RecNum.ToString(),
                        };
                        data.Brand = ErasoftDbContext.STF02E.SingleOrDefault(m => m.KODE == barangInDb.Sort2 && m.LEVEL == "2").KET;
                        data.Price = stf02h.HJUAL.ToString();
                        data.kode_mp = stf02h.BRG_MP;
                        eleApi.UpdateProductQOH_Price(data);
                    }
                    else if (marketPlace.NAMA.Equals(kdBli.ToString()))
                    {
                        if (!string.IsNullOrEmpty(marketPlace.Kode))
                        {
                            var barangInDb = ErasoftDbContext.STF02.SingleOrDefault(b => b.BRG == kdBrg);

                            BlibliController.BlibliAPIData iden = new BlibliController.BlibliAPIData
                            {
                                merchant_code = marketPlace.Sort1_Cust,
                                API_client_password = marketPlace.API_CLIENT_P,
                                API_client_username = marketPlace.API_CLIENT_U,
                                API_secret_key = marketPlace.API_KEY,
                                token = marketPlace.TOKEN,
                                mta_username_email_merchant = marketPlace.EMAIL,
                                mta_password_password_merchant = marketPlace.PASSWORD,
                                idmarket = marketPlace.RecNum.Value
                            };
                            BlibliController.BlibliProductData data = new BlibliController.BlibliProductData
                            {
                                kode = kdBrg,
                                kode_mp = stf02h.BRG_MP,
                                Qty = Convert.ToString(qtyOnHand),
                                MinQty = "0"
                            };
                            data.Price = stf02h.HJUAL.ToString();
                            data.MarketPrice = stf02h.HJUAL.ToString();
                            var display = Convert.ToBoolean(stf02h.DISPLAY);
                            data.display = display ? "true" : "false";
                            new BlibliController().UpdateProdukQOH_Display(iden, data);
                        }
                    }
                    //add by calvin 18 desember 2018
                    else if (marketPlace.NAMA.Equals(kdTokped.ToString()))
                    {
                        var TokoAPI = new TokopediaController();
                        if (!string.IsNullOrEmpty(marketPlace.Sort1_Cust))
                        {
                            if (!string.IsNullOrEmpty(stf02h.BRG_MP))
                            {
                                TokopediaController.TokopediaAPIData iden = new TokopediaController.TokopediaAPIData()
                                {
                                    merchant_code = marketPlace.Sort1_Cust, //FSID
                                    API_client_password = marketPlace.API_CLIENT_P, //Client ID
                                    API_client_username = marketPlace.API_CLIENT_U, //Client Secret
                                    API_secret_key = marketPlace.API_KEY, //Shop ID 
                                    token = marketPlace.TOKEN,
                                    idmarket = marketPlace.RecNum.Value
                                };
                                if (stf02h.BRG_MP.Contains("PENDING"))
                                {
                                    var cekPendingCreate = ErasoftDbContext.STF02H.Where(p => p.IDMARKET == marketPlace.RecNum && p.BRG_MP == stf02h.BRG_MP).ToList();
                                    if (cekPendingCreate.Count > 0)
                                    {
                                        foreach (var item in cekPendingCreate)
                                        {
                                            Task.Run(() => TokoAPI.CreateProductGetStatus(iden, item.BRG, Convert.ToInt32(item.BRG_MP.Split(';')[1]), item.BRG_MP.Split(';')[2]).Wait());
                                        }
                                    }
                                }
                                else
                                {
                                    Task.Run(() => TokoAPI.UpdateStock(iden, Convert.ToInt32(stf02h.BRG_MP), Convert.ToInt32(qtyOnHand))).Wait();
                                }
                            }
                        }
                    }
                    else if (marketPlace.NAMA.Equals(kdShopee.ToString()))
                    {
                        var ShopeeApi = new ShopeeController();

                        ShopeeController.ShopeeAPIData data = new ShopeeController.ShopeeAPIData()
                        {
                            merchant_code = marketPlace.Sort1_Cust,
                        };
                        if (stf02h.BRG_MP != "")
                        {
                            string[] brg_mp = stf02h.BRG_MP.Split(';');
                            if (brg_mp.Count() == 2)
                            {
                                if (brg_mp[1] == "0")
                                {
                                    Task.Run(() => ShopeeApi.UpdateStock(data, stf02h.BRG_MP, Convert.ToInt32(qtyOnHand))).Wait();
                                }
                                else if (brg_mp[1] != "")
                                {
                                    Task.Run(() => ShopeeApi.UpdateVariationStock(data, stf02h.BRG_MP, Convert.ToInt32(qtyOnHand))).Wait();
                                }
                            }
                        }
                    }
                    //end add by calvin 18 desember 2018
                }
            }
        }

        // =============================================== Bagian Transaksi Keluar Barang (START)

        [Route("manage/persediaan/keluar")]
        public ActionResult TransaksiKeluarMenu()
        {
            var vm = new StokViewModel()
            {
                ListStok = ErasoftDbContext.STT01A.ToList(),
                //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListGudang = ErasoftDbContext.STF18.ToList()
            };

            return View(vm);
        }

        public ActionResult SaveTransaksiKeluar(StokViewModel dataVm)
        {
            if (!ModelState.IsValid)
            {
                dataVm.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(dataVm, JsonRequestBehavior.AllowGet);
            }

            if (dataVm.Stok.ID == null)
            {
                var listStokInDb = ErasoftDbContext.STT01A.OrderBy(p => p.ID).ToList();
                var digitAkhir = "";
                var noStok = "";

                if (listStokInDb.Count == 0)
                {
                    digitAkhir = "000001";
                    noStok = $"KS{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                    ErasoftDbContext.Database.ExecuteSqlCommand("DBCC CHECKIDENT (STT01A, RESEED, 0)");
                }
                else
                {
                    var lastRecNum = listStokInDb.Last().ID;
                    var lastKode = listStokInDb.Last().Nobuk;
                    lastRecNum++;

                    digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                    noStok = $"KS{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";

                    if (noStok == lastKode)
                    {
                        lastRecNum++;
                        digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                        noStok = $"KS{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                    }
                }

                var year = Convert.ToInt16(DateTime.Now.ToString("yyyy"));
                var barangForCheck = ErasoftDbContext.STF08A.SingleOrDefault(b =>
                    b.BRG == dataVm.BarangStok.Kobar && b.GD == dataVm.BarangStok.Dr_Gd && b.Tahun == year);

                //add by calvin, 22 juni 2018 validasi QOH
                var qtyOnHand = GetQOHSTF08A(dataVm.BarangStok.Kobar, dataVm.BarangStok.Dr_Gd);

                if (qtyOnHand < dataVm.BarangStok.Qty)
                {
                    var vmError = new StokViewModel()
                    {

                    };
                    vmError.Errors.Add("Tidak bisa dikeluarkan, Qty di gudang sisa ( " + Convert.ToString(qtyOnHand) + " )");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
                //end add by calvin, validasi QOH

                dataVm.Stok.Nobuk = noStok;
                dataVm.Stok.STATUS_LOADING = "0";
                dataVm.BarangStok.Nobuk = noStok;

                ErasoftDbContext.STT01A.Add(dataVm.Stok);

                if (dataVm.BarangStok.No == null)
                {
                    //change by nurul 3/10/2018
                    //ErasoftDbContext.STT01B.Add(dataVm.BarangStok);
                    var vmError = new StokViewModel() { };

                    if (dataVm.BarangStok.Dr_Gd == null || dataVm.BarangStok.Qty == 0)
                    {
                        vmError.Errors.Add("Silahkan isi semua field terlebih dahulu !");
                        return Json(vmError, JsonRequestBehavior.AllowGet);
                    }

                    ErasoftDbContext.STT01B.Add(dataVm.BarangStok);

                    //end change 
                }
            }
            else
            {
                var stokInDb = ErasoftDbContext.STT01A.Single(p => p.Nobuk == dataVm.Stok.Nobuk);

                var year = Convert.ToInt16(DateTime.Now.ToString("yyyy"));
                var barangForCheck = ErasoftDbContext.STF08A.SingleOrDefault(b =>
                    b.BRG == dataVm.BarangStok.Kobar && b.GD == dataVm.BarangStok.Dr_Gd && b.Tahun == year);

                //add by calvin, 22 juni 2018 validasi QOH
                var qtyOnHand = GetQOHSTF08A(dataVm.BarangStok.Kobar, dataVm.BarangStok.Dr_Gd);

                if (qtyOnHand < dataVm.BarangStok.Qty)
                {
                    var vmError = new StokViewModel()
                    {

                    };
                    vmError.Errors.Add("Tidak bisa dikeluarkan, Qty di gudang sisa ( " + Convert.ToString(qtyOnHand) + " )");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
                //end add by calvin, validasi QOH

                stokInDb.Tgl = dataVm.Stok.Tgl;
                dataVm.BarangStok.Nobuk = dataVm.Stok.Nobuk;

                if (dataVm.BarangStok.No == null)
                {
                    //change by nurul 3/10/2018
                    //ErasoftDbContext.STT01B.Add(dataVm.BarangStok);
                    var vmError = new StokViewModel() { };

                    if (dataVm.BarangStok.Dr_Gd == null || dataVm.BarangStok.Qty == 0)
                    {
                        vmError.Errors.Add("Silahkan isi semua field terlebih dahulu !");
                        return Json(vmError, JsonRequestBehavior.AllowGet);
                    }

                    ErasoftDbContext.STT01B.Add(dataVm.BarangStok);

                    //end change 
                }
            }

            #region add by calvin 14 juni 2018, agar field yg penting di stt01b tidak null
            dataVm.BarangStok.Ke_Gd = "";
            dataVm.BarangStok.WO = "";
            dataVm.BarangStok.Rak = "";
            dataVm.BarangStok.JTran = "K";
            dataVm.BarangStok.KLINK = "";
            dataVm.BarangStok.NO_WO = "";
            dataVm.BarangStok.KET = "";
            dataVm.BarangStok.BRG_ORIGINAL = "";
            dataVm.BarangStok.QTY3 = 0;
            dataVm.BarangStok.BUKTI_DS = "";
            dataVm.BarangStok.BUKTI_REFF = "";
            #endregion

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            var vm = new StokViewModel()
            {
                Stok = ErasoftDbContext.STT01A.Single(p => p.Nobuk == dataVm.Stok.Nobuk),
                ListStok = ErasoftDbContext.STT01A.ToList(),
                ListBarangStok = ErasoftDbContext.STT01B.Where(bs => bs.Nobuk == dataVm.Stok.Nobuk).ToList(),
                //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListGudang = ErasoftDbContext.STF18.ToList()
            };

            //add by Tri, panggil api marketplace to change stock
            List<string> listBrg = new List<string>();
            //foreach (var brg in vm.ListBarang)
            //{
            listBrg.Add(dataVm.BarangStok.Kobar);
            //}
            updateStockMarketPlace(listBrg);
            //end add by Tri, panggil api marketplace to change stock

            return PartialView("BarangTransaksiKeluarPartial", vm);
        }

        public ActionResult RefreshTableTransaksiKeluar()
        {
            var vm = new StokViewModel()
            {
                ListStok = ErasoftDbContext.STT01A.ToList()
            };

            return PartialView("TableTransaksiKeluarPartial", vm);
        }

        public ActionResult RefreshTransaksiKeluarForm()
        {
            try
            {
                var vm = new StokViewModel()
                {
                    ListStok = ErasoftDbContext.STT01A.ToList(),
                    //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                return PartialView("BarangTransaksiKeluarPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult EditTransaksiKeluar(int? stokId)
        {
            try
            {
                var stokInDb = ErasoftDbContext.STT01A.Single(p => p.ID == stokId);

                var vm = new StokViewModel()
                {
                    Stok = stokInDb,
                    ListStok = ErasoftDbContext.STT01A.ToList(),
                    ListBarangStok = ErasoftDbContext.STT01B.Where(bs => bs.Nobuk == stokInDb.Nobuk).ToList(),
                    //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                return PartialView("BarangTransaksiKeluarPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult DeleteTransaksiKeluar(int? stokId)
        {
            var stokInDb = ErasoftDbContext.STT01A.Single(p => p.ID == stokId);

            //add by Tri, 21 agustus 2018
            List<string> brg = new List<string>();
            var stokDetailInDb = ErasoftDbContext.STT01B.Where(b => b.Nobuk == stokInDb.Nobuk).ToList();
            foreach (var item in stokDetailInDb)
            {
                brg.Add(item.Kobar);
                //add by nurul 13/9/2018
                ErasoftDbContext.STT01B.Remove(item);
                ErasoftDbContext.SaveChanges();
                //end add by nurul 13/9/2018
            }
            //end add by Tri, 21 agustus 2018

            ErasoftDbContext.STT01A.Remove(stokInDb);
            ErasoftDbContext.SaveChanges();

            var vm = new StokViewModel()
            {
                ListStok = ErasoftDbContext.STT01A.ToList()
            };

            //add by Tri, panggil api marketplace to change stock
            updateStockMarketPlace(brg);
            //end add by Tri, panggil api marketplace to change stock

            return PartialView("TableTransaksiKeluarPartial", vm);
        }

        [HttpGet]
        public ActionResult DeleteBarangTransaksiKeluar(int noUrut)
        {
            try
            {
                var barangStokInDb = ErasoftDbContext.STT01B.Single(b => b.No == noUrut);
                var stokInDb = ErasoftDbContext.STT01A.Single(p => p.Nobuk == barangStokInDb.Nobuk);

                //add by Tri, panggil api marketplace to change stock
                List<string> brg = new List<string>();
                brg.Add(barangStokInDb.Kobar);
                //end add by Tri, panggil api marketplace to change stock

                ErasoftDbContext.STT01B.Remove(barangStokInDb);
                ErasoftDbContext.SaveChanges();

                var vm = new StokViewModel()
                {
                    Stok = ErasoftDbContext.STT01A.Single(p => p.Nobuk == stokInDb.Nobuk),
                    ListStok = ErasoftDbContext.STT01A.ToList(),
                    ListBarangStok = ErasoftDbContext.STT01B.Where(bs => bs.Nobuk == stokInDb.Nobuk).ToList(),
                    //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                //add by Tri, panggil api marketplace to change stock
                updateStockMarketPlace(brg);
                //end add by Tri, panggil api marketplace to change stock

                return PartialView("BarangTransaksiKeluarPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult UpdateTransaksiKeluar(UpdateData dataUpdate)
        {
            var stokInDb = ErasoftDbContext.STT01A.Single(p => p.Nobuk == dataUpdate.NoBuktiStok);
            //remark by nurul 25/9/2018
            //stokInDb.TglInput = DateTime.ParseExact(dataUpdate.TglInput, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            stokInDb.Tgl = DateTime.ParseExact(dataUpdate.TglInput, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            ErasoftDbContext.SaveChanges();

            return new EmptyResult();
        }

        // =============================================== Bagian Transaksi Keluar Barang (END)

        // =============================================== Bagian Transaksi Pindah Barang (START)

        [Route("manage/persediaan/pindah")]
        public ActionResult TransaksiPindahMenu()
        {
            var vm = new StokViewModel()
            {
                ListStok = ErasoftDbContext.STT01A.ToList(),
                //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListGudang = ErasoftDbContext.STF18.ToList()
            };

            return View(vm);
        }

        public ActionResult SaveTransaksiPindah(StokViewModel dataVm)
        {
            if (!ModelState.IsValid)
            {
                dataVm.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(dataVm, JsonRequestBehavior.AllowGet);
            }

            if (dataVm.Stok.ID == null)
            {
                //add by calvin, 22 juni 2018 validasi QOH
                var qtyOnHand = GetQOHSTF08A(dataVm.BarangStok.Kobar, dataVm.BarangStok.Dr_Gd);

                if (qtyOnHand < dataVm.BarangStok.Qty)
                {
                    var vmError = new StokViewModel()
                    {

                    };
                    vmError.Errors.Add("Tidak bisa dikeluarkan, Qty di gudang " + Convert.ToString(dataVm.BarangStok.Dr_Gd) + " sisa ( " + Convert.ToString(qtyOnHand) + " )");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
                //end add by calvin, validasi QOH

                var listStokInDb = ErasoftDbContext.STT01A.OrderBy(p => p.ID).ToList();
                var digitAkhir = "";
                var noStok = "";

                if (listStokInDb.Count == 0)
                {
                    digitAkhir = "000001";
                    noStok = $"PG{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                    ErasoftDbContext.Database.ExecuteSqlCommand("DBCC CHECKIDENT (STT01A, RESEED, 0)");
                }
                else
                {
                    var lastRecNum = listStokInDb.Last().ID;
                    var lastKode = listStokInDb.Last().Nobuk;
                    lastRecNum++;

                    digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                    noStok = $"PG{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";

                    if (noStok == lastKode)
                    {
                        lastRecNum++;
                        digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                        noStok = $"PG{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                    }
                }

                dataVm.Stok.Nobuk = noStok;
                dataVm.Stok.STATUS_LOADING = "0";
                dataVm.BarangStok.Nobuk = noStok;



                ErasoftDbContext.STT01A.Add(dataVm.Stok);

                if (dataVm.BarangStok.No == null)
                {
                    //change by nurul 3/10/2018
                    //ErasoftDbContext.STT01B.Add(dataVm.BarangStok);
                    var vmError = new StokViewModel() { };

                    if (dataVm.BarangStok.Dr_Gd == null || dataVm.BarangStok.Ke_Gd == null || dataVm.BarangStok.Qty == 0)
                    {
                        vmError.Errors.Add("Silahkan isi semua field terlebih dahulu !");
                        return Json(vmError, JsonRequestBehavior.AllowGet);
                    }

                    ErasoftDbContext.STT01B.Add(dataVm.BarangStok);

                    //end change 
                }
            }
            else
            {
                //add by calvin, 22 juni 2018 validasi QOH
                var qtyOnHand = GetQOHSTF08A(dataVm.BarangStok.Kobar, dataVm.BarangStok.Dr_Gd);

                if (qtyOnHand < dataVm.BarangStok.Qty)
                {
                    var vmError = new StokViewModel()
                    {

                    };
                    vmError.Errors.Add("Tidak bisa dikeluarkan, Qty di gudang " + Convert.ToString(dataVm.BarangStok.Dr_Gd) + " sisa ( " + Convert.ToString(qtyOnHand) + " )");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
                //end add by calvin, validasi QOH

                var stokInDb = ErasoftDbContext.STT01A.Single(p => p.Nobuk == dataVm.Stok.Nobuk);

                stokInDb.Tgl = dataVm.Stok.Tgl;
                dataVm.BarangStok.Nobuk = dataVm.Stok.Nobuk;

                if (dataVm.BarangStok.No == null)
                {
                    //change by nurul 3/10/2018
                    //ErasoftDbContext.STT01B.Add(dataVm.BarangStok);
                    var vmError = new StokViewModel() { };

                    if (dataVm.BarangStok.Dr_Gd == null || dataVm.BarangStok.Ke_Gd == null || dataVm.BarangStok.Qty == 0)
                    {
                        vmError.Errors.Add("Silahkan isi semua field terlebih dahulu !");
                        return Json(vmError, JsonRequestBehavior.AllowGet);
                    }

                    ErasoftDbContext.STT01B.Add(dataVm.BarangStok);

                    //end change 
                }
            }

            #region add by calvin 14 juni 2018, agar field yg penting di stt01b tidak null
            dataVm.BarangStok.WO = "";
            dataVm.BarangStok.Rak = "";
            dataVm.BarangStok.JTran = "P";
            dataVm.BarangStok.KLINK = "";
            dataVm.BarangStok.NO_WO = "";
            dataVm.BarangStok.KET = "";
            dataVm.BarangStok.BRG_ORIGINAL = "";
            dataVm.BarangStok.QTY3 = 0;
            dataVm.BarangStok.BUKTI_DS = "";
            dataVm.BarangStok.BUKTI_REFF = "";
            #endregion
            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            var vm = new StokViewModel()
            {
                Stok = ErasoftDbContext.STT01A.Single(p => p.Nobuk == dataVm.Stok.Nobuk),
                ListStok = ErasoftDbContext.STT01A.ToList(),
                ListBarangStok = ErasoftDbContext.STT01B.Where(bs => bs.Nobuk == dataVm.Stok.Nobuk).ToList(),
                //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListGudang = ErasoftDbContext.STF18.ToList()
            };

            return PartialView("BarangTransaksiPindahPartial", vm);
        }

        public ActionResult RefreshTableTransaksiPindah()
        {
            var vm = new StokViewModel()
            {
                ListStok = ErasoftDbContext.STT01A.ToList()
            };

            return PartialView("TableTransaksiPindahPartial", vm);
        }

        public ActionResult RefreshTransaksiPindahForm()
        {
            try
            {
                var vm = new StokViewModel()
                {
                    ListStok = ErasoftDbContext.STT01A.ToList(),
                    //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                return PartialView("BarangTransaksiPindahPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult EditTransaksiPindah(int? stokId)
        {
            try
            {
                var stokInDb = ErasoftDbContext.STT01A.Single(p => p.ID == stokId);

                var vm = new StokViewModel()
                {
                    Stok = stokInDb,
                    ListStok = ErasoftDbContext.STT01A.ToList(),
                    ListBarangStok = ErasoftDbContext.STT01B.Where(bs => bs.Nobuk == stokInDb.Nobuk).ToList(),
                    //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                return PartialView("BarangTransaksiPindahPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult DeleteTransaksiPindah(int? stokId)
        {
            var stokInDb = ErasoftDbContext.STT01A.Single(p => p.ID == stokId);

            //add by calvin, 25 juni 2018 validasi QOH
            var stokDetailInDb = ErasoftDbContext.STT01B.Where(b => b.Nobuk == stokInDb.Nobuk).ToList();
            foreach (var item in stokDetailInDb)
            {
                var qtyOnHand = GetQOHSTF08A(item.Kobar, item.Ke_Gd);

                if (qtyOnHand - item.Qty < 0)
                {
                    var vmError = new StokViewModel()
                    {

                    };
                    vmError.Errors.Add("Tidak bisa dihapus, Qty Barang ( " + item.Kobar + " ) di gudang " + Convert.ToString(item.Ke_Gd) + " sisa ( " + Convert.ToString(qtyOnHand) + " )");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
                //add by nurul 13/9/2018
                ErasoftDbContext.STT01B.Remove(item);
                ErasoftDbContext.SaveChanges();
                //end add by nurul 13/9/2018
            }
            //end add by calvin, validasi QOH

            ErasoftDbContext.STT01A.Remove(stokInDb);
            ErasoftDbContext.SaveChanges();

            var vm = new StokViewModel()
            {
                ListStok = ErasoftDbContext.STT01A.ToList()
            };

            return PartialView("TableTransaksiPindahPartial", vm);
        }

        [HttpGet]
        public ActionResult DeleteBarangTransaksiPindah(int noUrut)
        {
            try
            {
                var barangStokInDb = ErasoftDbContext.STT01B.Single(b => b.No == noUrut);
                var stokInDb = ErasoftDbContext.STT01A.Single(p => p.Nobuk == barangStokInDb.Nobuk);

                //add by calvin, 22 juni 2018 validasi QOH
                var qtyOnHand = GetQOHSTF08A(barangStokInDb.Kobar, barangStokInDb.Ke_Gd);

                if (qtyOnHand - barangStokInDb.Qty < 0)
                {
                    var vmError = new StokViewModel()
                    {

                    };
                    vmError.Errors.Add("Tidak bisa dihapus, Qty di gudang " + Convert.ToString(barangStokInDb.Ke_Gd) + " sisa ( " + Convert.ToString(qtyOnHand) + " )");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
                //end add by calvin, validasi QOH

                ErasoftDbContext.STT01B.Remove(barangStokInDb);
                ErasoftDbContext.SaveChanges();

                var vm = new StokViewModel()
                {
                    Stok = ErasoftDbContext.STT01A.Single(p => p.Nobuk == stokInDb.Nobuk),
                    ListStok = ErasoftDbContext.STT01A.ToList(),
                    ListBarangStok = ErasoftDbContext.STT01B.Where(bs => bs.Nobuk == stokInDb.Nobuk).ToList(),
                    //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                return PartialView("BarangTransaksiPindahPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult UpdateTransaksiPindah(UpdateData dataUpdate)
        {
            var stokInDb = ErasoftDbContext.STT01A.Single(p => p.Nobuk == dataUpdate.NoBuktiStok);
            //remark by nurul 25/9/2018
            //stokInDb.TglInput = DateTime.ParseExact(dataUpdate.TglInput, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            stokInDb.Tgl = DateTime.ParseExact(dataUpdate.TglInput, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            ErasoftDbContext.SaveChanges();

            return new EmptyResult();
        }

        // =============================================== Bagian Transaksi Pindah Barang (END)

        // =============================================== Bagian Ubah Password (START)

        public ActionResult UbahPassword(UpdateData dataPassBaru)
        {
            var accInDb = MoDbContext.Account.SingleOrDefault(a => a.Username == dataPassBaru.Username);

            if (accInDb == null)
            {
                var userInDb = MoDbContext.User.Single(u => u.Username == dataPassBaru.Username);

                if (userInDb.Password == dataPassBaru.OldPass)
                {
                    userInDb.Password = dataPassBaru.NewPass;
                    userInDb.KonfirmasiPassword = dataPassBaru.NewPass;
                }
                else
                {
                    dataPassBaru.WrongOldPass = true;
                    return Json(dataPassBaru, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var pass = accInDb.Password;
                var hashCode = accInDb.VCode;
                var encodingPassString = Helper.EncodePassword(dataPassBaru.OldPass, hashCode);

                if (pass == encodingPassString)
                {
                    var encodingPassNewString = Helper.EncodePassword(dataPassBaru.NewPass, hashCode);

                    accInDb.Password = encodingPassNewString;
                    accInDb.ConfirmPassword = encodingPassNewString;
                }
                else
                {
                    dataPassBaru.WrongOldPass = true;
                    return Json(dataPassBaru, JsonRequestBehavior.AllowGet);
                }
            }

            MoDbContext.SaveChanges();
            ModelState.Clear();

            return Json(dataPassBaru, JsonRequestBehavior.AllowGet);
        }

        // =============================================== Bagian Ubah Password (END)

        // =============================================== Bagian Cek Kode (START)

        [HttpGet]
        public ActionResult CekKodeGudang(string kode)
        {
            var res = new CekKode()
            {
                Kode = kode
            };

            var gudangInDb = ErasoftDbContext.STF18.FirstOrDefault(g => g.Kode_Gudang == kode);
            if (gudangInDb != null) res.Available = false;

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CekKodeKategori(string kode)
        {
            var res = new CekKode()
            {
                Kode = kode
            };

            var gudangInDb = ErasoftDbContext.STF02E.FirstOrDefault(k => k.LEVEL == "1" && k.KODE == kode);
            if (gudangInDb != null) res.Available = false;

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CekKodeMerk(string kode)
        {
            var res = new CekKode()
            {
                Kode = kode
            };

            var gudangInDb = ErasoftDbContext.STF02E.FirstOrDefault(k => k.LEVEL == "2" && k.KODE == kode);
            if (gudangInDb != null) res.Available = false;

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        //change by nurul 9/11/2018
        //[HttpGet]
        //public ActionResult CekKetMerk(string kode)
        //{
        //    var res = new CekKode()
        //    {
        //        Kode = kode
        //    };

        //    var gudangInDb = ErasoftDbContext.STF02E.FirstOrDefault(k => k.LEVEL == "2" && k.KET == kode);
        //    if (gudangInDb != null) res.Available = false;

        //    return Json(res, JsonRequestBehavior.AllowGet);
        //}

        //[HttpGet]
        [HttpGet]
        //public ActionResult CekKetMerk(string ket, string kodemerk)
        public ActionResult CekKetMerk(string param)
        {
            string kodemerk = (param.Split(';')[param.Split(';').Length - 1]);
            string ket = (param.Split(';')[param.Split(';').Length - 2]);

            var res = new CekMerk()
            {
                Kode = kodemerk,
                Nama = ket
            };

            //var gudangInDb = ErasoftDbContext.STF02E.FirstOrDefault(k => k.LEVEL == "2" && k.KET == kode);
            var gudangInDb = ErasoftDbContext.STF02E.FirstOrDefault(k => k.LEVEL == "2" && k.KET == ket && k.KODE != kodemerk);
            if (gudangInDb != null) res.Available = false;

            return Json(res, JsonRequestBehavior.AllowGet);
        }
        //end change by nurul

        public ActionResult CekNmGudang(string kode)
        {
            var res = new CekKode()
            {
                Kode = kode
            };

            var gudangInDb = ErasoftDbContext.STF18.FirstOrDefault(g => g.Nama_Gudang == kode);
            if (gudangInDb != null) res.Available = false;

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CekKodeBarang(string kode)
        {
            var res = new CekKode()
            {
                Kode = kode
            };

            var gudangInDb = ErasoftDbContext.STF02.FirstOrDefault(k => k.BRG == kode);
            if (gudangInDb != null) res.Available = false;

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CekKodeSupplier(string kode)
        {
            var res = new CekKode()
            {
                Kode = kode
            };

            var gudangInDb = ErasoftDbContext.APF01.FirstOrDefault(k => k.SUPP == kode);
            if (gudangInDb != null) res.Available = false;

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CekNmSupplier(string kode)
        {
            var res = new CekKode()
            {
                Kode = kode
            };

            var gudangInDb = ErasoftDbContext.APF01.FirstOrDefault(k => k.NAMA == kode);
            if (gudangInDb != null) res.Available = false;

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CekKodeRekening(string kode)
        {
            var res = new CekKode()
            {
                Kode = kode
            };

            var gudangInDb = ErasoftDbContext.GLFREKs.FirstOrDefault(k => k.KODE == kode);
            if (gudangInDb != null) res.Available = false;

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CekKodePembeli(string kode)
        {
            var res = new CekKode()
            {
                Kode = kode
            };

            var gudangInDb = ErasoftDbContext.ARF01C.FirstOrDefault(k => k.BUYER_CODE == kode);
            if (gudangInDb != null) res.Available = false;

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        // =============================================== Bagian Cek Kode (END)

        // =============================================== Bagian Data Perusahaan (START)

        public ActionResult DataPerusahaanMenu()
        {
            var dataPerusahaanVm = new DataPerusahaanViewModel()
            {
                DataUsaha = ErasoftDbContext.SIFSYS.SingleOrDefault(p => p.BLN == 1),
                DataUsahaTambahan = ErasoftDbContext.SIFSYS_TAMBAHAN.First()
            };

            return View(dataPerusahaanVm);
        }

        [HttpGet]
        public ActionResult GetDataPengusaha(string userId)
        {
            var accInDb = MoDbContext.Account.SingleOrDefault(a => a.UserId == userId);

            if (accInDb == null)
                return Json("No Data Found!", JsonRequestBehavior.AllowGet);

            var res = new DataPengusaha()
            {
                NamaLengkap = accInDb.Username,
                Email = accInDb.Email,
                Telepon = accInDb.NoHp
            };

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteLogoPerusahaan(string namaPT, string uname)
        {
            try
            {
                namaPT = namaPT.Trim();
                uname = uname.Trim();
                var namaFile = $"LogoUsaha-{uname}-{namaPT}.jpg";
                var path = Path.Combine(Server.MapPath("~/Content/Logo_Perusahaan/"), namaFile);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }

                return new EmptyResult();
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult SaveDataUsaha(DataPerusahaanViewModel dataVm)
        {
            if (!ModelState.IsValid)
            {
                dataVm.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(dataVm, JsonRequestBehavior.AllowGet);
            }

            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {
                    var fileExtension = Path.GetExtension(file.FileName);
                    string namaPT = dataVm.DataUsaha.USERNAME.Trim();
                    string uname = dataVm.DataUsaha.NAMA_PT.Trim();
                    var namaFile = $"LogoUsaha-{namaPT}-{uname}{fileExtension}";
                    var path = Path.Combine(Server.MapPath("~/Content/Logo_Perusahaan/"), namaFile);
                    file.SaveAs(path);
                }
            }

            var dataPerusahaanInDb = ErasoftDbContext.SIFSYS.Single(p => p.BLN == 1);
            dataPerusahaanInDb.NAMA_PT = dataVm.DataUsaha.NAMA_PT;
            dataPerusahaanInDb.ALAMAT_PT = dataVm.DataUsaha.ALAMAT_PT;
            dataPerusahaanInDb.NPWP = dataVm.DataUsaha.NPWP;
            dataPerusahaanInDb.METODA_NO = dataVm.DataUsaha.METODA_NO;
            dataPerusahaanInDb.KODE_BRG_STYLE = dataVm.DataUsaha.KODE_BRG_STYLE;
            //add by nurul 11/3/2019
            dataPerusahaanInDb.GUDANG = dataVm.DataUsaha.GUDANG;
            //end add by nurul 11/3/2019
            //dataPerusahaanInDb.BCA_API_KEY = dataVm.DataUsaha.BCA_API_KEY;
            //dataPerusahaanInDb.BCA_API_SECRET = dataVm.DataUsaha.BCA_API_SECRET;
            //dataPerusahaanInDb.BCA_CLIENT_ID = dataVm.DataUsaha.BCA_CLIENT_ID;
            //dataPerusahaanInDb.BCA_CLIENT_SECRET = dataVm.DataUsaha.BCA_CLIENT_SECRET;

            var dataPerusahaanTambahanInDb = ErasoftDbContext.SIFSYS_TAMBAHAN.SingleOrDefault();
            var accInDb = MoDbContext.Account.SingleOrDefault(ac => ac.Email == dataPerusahaanTambahanInDb.EMAIL);

            if (accInDb != null) accInDb.Email = dataVm.DataUsahaTambahan.EMAIL;
            MoDbContext.SaveChanges();

            dataPerusahaanTambahanInDb.KODEPOS = dataVm.DataUsahaTambahan.KODEPOS;
            dataPerusahaanTambahanInDb.KODEPROV = dataVm.DataUsahaTambahan.KODEPROV;
            dataPerusahaanTambahanInDb.KODEKABKOT = dataVm.DataUsahaTambahan.KODEKABKOT;
            dataPerusahaanTambahanInDb.PERSON = dataVm.DataUsahaTambahan.PERSON;
            dataPerusahaanTambahanInDb.EMAIL = dataVm.DataUsahaTambahan.EMAIL;
            dataPerusahaanTambahanInDb.TELEPON = dataVm.DataUsahaTambahan.TELEPON;

            ErasoftDbContext.SaveChanges();

            return new EmptyResult();
        }

        // =============================================== Bagian Data Perusahaan (END)

        // =============================================== ADD BY NURUL 24/8/2018 -- Bagian Data APIBCA (START)

        public ActionResult APIBCA()
        {
            var APIBCAVm = new DataPerusahaanViewModel()
            {
                DataUsaha = ErasoftDbContext.SIFSYS.Single(p => p.BLN == 1)
            };

            return View(APIBCAVm);
        }


        [HttpPost]
        public ActionResult SaveAPIBCA(DataPerusahaanViewModel dataVm)
        {
            if (!ModelState.IsValid)
            {
                dataVm.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(dataVm, JsonRequestBehavior.AllowGet);
            }

            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {
                    var fileExtension = Path.GetExtension(file.FileName);
                    string namaPT = dataVm.DataUsaha.USERNAME.Trim();
                }
            }

            var dataPerusahaanInDb = ErasoftDbContext.SIFSYS.Single(p => p.BLN == 1);
            dataPerusahaanInDb.BCA_API_KEY = dataVm.DataUsaha.BCA_API_KEY;
            dataPerusahaanInDb.BCA_API_SECRET = dataVm.DataUsaha.BCA_API_SECRET;
            dataPerusahaanInDb.BCA_CLIENT_ID = dataVm.DataUsaha.BCA_CLIENT_ID;
            dataPerusahaanInDb.BCA_CLIENT_SECRET = dataVm.DataUsaha.BCA_CLIENT_SECRET;

            ErasoftDbContext.SaveChanges();

            return new EmptyResult();
        }

        // =============================================== END ADD BY NURUL -- Bagian Data API BCA (END)
        // =============================================== ADD BY CALVIN -- Bagian Import Data Faktur
        public class UploadFakturResult
        {
            public string success { get; set; }
            public string resultMessage { get; set; }

        }
        [HttpGet]
        public FileResult DownloadLogUploadFaktur(string filename)
        {
            AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            var path = Path.Combine(Server.MapPath("~/Content/Uploaded/" + sessionData.Account.DatabasePathErasoft + "/"), filename);

            byte[] data = System.IO.File.ReadAllBytes(path);
            string contentType = MimeMapping.GetMimeMapping(path);
            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = filename,
                Inline = true,
            };
            //Response.AppendHeader("Content-Disposition", cd.ToString());

            return File(data, contentType, filename);
        }

        [HttpGet]
        public ActionResult ListImportFaktur(string cust)
        {
            var partialVm = new FakturViewModel()
            {
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListImportFaktur = ErasoftDbContext.LOG_IMPORT_FAKTUR.Where(a => a.CUST == cust).OrderByDescending(a => a.UPLOAD_DATETIME).ToList()
            };

            return PartialView("UploadFakturView", partialVm);
        }
        [HttpPost]
        public ActionResult UploadFakturTokped(UploadFakturTokpedDataDetail[] data, string cust, string nama_cust, string perso)
        {
            AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            string uname = sessionData.Account.Username;
            UploadFakturResult result = new UploadFakturResult
            {
                success = "0",
                resultMessage = ""
            };

            #region Logging
            string message = "";
            string filename = "Log_Upload_Inv_Tokopedia_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".txt";
            var path = Path.Combine(Server.MapPath("~/Content/Uploaded/" + sessionData.Account.DatabasePathErasoft + "/"), filename);

            LOG_IMPORT_FAKTUR newLogImportFaktur = new LOG_IMPORT_FAKTUR
            {
                CUST = cust,
                UPLOADER = uname,
                UPLOAD_DATETIME = DateTime.Now,
                LOG_FILE = filename,
            };
            string lastFakturInUpload = "";
            DateTime lastFakturDateInUpload = DateTime.Now;
            #endregion

            if (data == null)
            {
                return JsonErrorMessage("Format data tidak sesuai");
            }
            else
            {
                if (!System.IO.File.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(Path.Combine(Server.MapPath("~/Content/Uploaded/" + sessionData.Account.DatabasePathErasoft + "/"), ""));
                    var asd = System.IO.File.Create(path);
                    asd.Close();
                }
                StreamWriter tw = new StreamWriter(path);

                #region Proses Upload
                var lastRecnumARF01C = ErasoftDbContext.ARF01C.Max(p => p.RecNum);
                var listFakturInDb = ErasoftDbContext.SIT01A.OrderBy(p => p.RecNum).ToList();
                //var listItem = ErasoftDbContext.STF02.ToList(); 'change by nurul 21/1/2019
                var listItem = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList();

                var digitAkhir = "";
                var noOrder = "";
                var lastRecNum = 0;
                if (listFakturInDb.Count == 0)
                {
                    digitAkhir = "000001";
                    noOrder = $"SI{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                    ErasoftDbContext.Database.ExecuteSqlCommand("DBCC CHECKIDENT (SIT01A, RESEED, 0)");
                }
                else
                {
                    lastRecNum = listFakturInDb.Last().RecNum.HasValue ? Convert.ToInt32(listFakturInDb.Last().RecNum) : 0;
                    if (lastRecNum == 0)
                    {
                        lastRecNum = 1;
                    }
                }
                string buyercode = "";
                string al2 = "";
                string al3 = "";

                bool adaWarning = false;
                bool masihFakturYangSama = true;
                bool fakturLolosValidasi = true;
                bool barangFakturLolosValidasi = true;
                string messageWarning = "";
                string faktur_invoice = "";

                List<ARF01C> newARF01Cs = new List<ARF01C>();
                List<SIT01A> newFakturs = new List<SIT01A>();
                List<SIT01B> newFaktursDetails = new List<SIT01B>();
                for (int i = 0; i < data.Count(); i++)
                {
                    UploadFakturTokpedDataDetail faktur = data[i];

                    #region  validasi
                    //cek faktur sudah pernah di upload
                    if (!string.IsNullOrWhiteSpace(faktur.Invoice))
                    {
                        if (i > 0)
                        {
                            masihFakturYangSama = false;
                        }
                        faktur_invoice = faktur.Invoice;
                        message = "";
                        messageWarning = "";
                        adaWarning = false;
                        fakturLolosValidasi = true;
                        var cekFakturExists = listFakturInDb.Where(p => p.JENIS_FORM == "2" && p.NO_REF == faktur_invoice).FirstOrDefault();
                        if (cekFakturExists != null)
                        {
                            fakturLolosValidasi = false;
                            //log faktur sudah pernah di upload
                            message = "Faktur [" + faktur_invoice + "] sudah pernah diupload, dengan nomor faktur : [" + cekFakturExists.NO_BUKTI + "]." + System.Environment.NewLine;
                            tw.WriteLine(message);
                        }
                    }
                    else
                    {
                        masihFakturYangSama = true;
                        messageWarning = "";
                    }
                    if (fakturLolosValidasi)
                    {
                        barangFakturLolosValidasi = true;
                        //cek barang sudah ada di master
                        var cekItem = listItem.Where(p => p.BRG == (string.IsNullOrWhiteSpace(faktur.StockKeepingUnitSKU) ? faktur.ProductID : faktur.StockKeepingUnitSKU)).FirstOrDefault();
                        if (cekItem == null)
                        {
                            //log item belum ada di master
                            barangFakturLolosValidasi = false;
                            adaWarning = true;
                            if (message == "")
                            {
                                message = "Faktur Tokopedia [" + faktur_invoice + "] gagal diupload." + System.Environment.NewLine;
                                message += "Masalah pada nomor faktur [" + faktur_invoice + "] :" + System.Environment.NewLine;
                                tw.WriteLine(message);
                            }
                            messageWarning = "- Item [" + (string.IsNullOrWhiteSpace(faktur.StockKeepingUnitSKU) ? faktur.ProductID : faktur.StockKeepingUnitSKU) + "] belum ada di Master Barang MasterOnline." + System.Environment.NewLine;
                            tw.WriteLine(messageWarning);
                        }
                    }
                    #endregion

                    if (fakturLolosValidasi && barangFakturLolosValidasi)
                    {
                        buyercode = "";
                        if (!string.IsNullOrWhiteSpace(faktur.Invoice))
                        {
                            lastRecNum++;
                            digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                            noOrder = $"SI{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                        }

                        #region insert pembeli
                        if (!string.IsNullOrWhiteSpace(faktur.Invoice))
                        {
                            string kabupaten = (faktur.RecipientAddress.Split(',')[faktur.RecipientAddress.Split(',').Length - 3]);
                            string provinsi = ((faktur.RecipientAddress.Split(',')[faktur.RecipientAddress.Split(',').Length - 1]).Substring(6, (faktur.RecipientAddress.Split(',')[faktur.RecipientAddress.Split(',').Length - 1]).Length - 6));
                            var cekPembeli = (from p in ErasoftDbContext.ARF01C
                                              where p.EMAIL == (faktur.CustomerName.Replace(" ", "").Length > 36 ? faktur.CustomerName.Replace(" ", "").Substring(0, 36) + "@tokopedia.com" : faktur.CustomerName.Replace(" ", "") + "@tokopedia.com")
                                              select new { p.BUYER_CODE, p.AL2, p.AL3 }).SingleOrDefault();
                            if (cekPembeli == null)
                            {
                                lastRecnumARF01C++;

                                ARF01C newPembeli = new ARF01C
                                {
                                    BUYER_CODE = lastRecnumARF01C.ToString().PadLeft(10, '0'),
                                    NAMA = faktur.CustomerName.Length > 30 ? faktur.CustomerName.Substring(0, 27) + "..." : faktur.CustomerName,
                                    AL = faktur.RecipientAddress,
                                    TLP = faktur.CustomerPhone,
                                    PERSO = perso,
                                    TERM = 0,
                                    LIMIT = 0,
                                    PKP = "0",
                                    KLINK = "01",
                                    KODE_CABANG = 1,
                                    VLT = "IDR",
                                    KDHARGA = "01",
                                    AL_KIRIM1 = faktur.RecipientAddress.Length > 30 ? faktur.RecipientAddress.Substring(0, 30) : faktur.RecipientAddress,
                                    AL_KIRIM2 = faktur.RecipientAddress.Length > 60 ? faktur.RecipientAddress.Substring(30, 30) : faktur.RecipientAddress.Substring(30, faktur.RecipientAddress.Length - 30),
                                    AL_KIRIM3 = faktur.RecipientAddress.Length > 90 ? faktur.RecipientAddress.Substring(60, 27) + "..." : faktur.RecipientAddress.Substring(60, faktur.RecipientAddress.Length - 60),
                                    DISC_NOTA = 0,
                                    NDISC_NOTA = 0,
                                    DISC_ITEM = 0,
                                    NDISC_ITEM = 0,
                                    STATUS = "1",
                                    LABA = 0,
                                    TIDAK_HIT_UANG_R = false,
                                    No_Seri_Pajak = "FP",
                                    TGL_INPUT = DateTime.Now,
                                    USERNAME = faktur.CustomerName.Replace(" ", "").Length > 30 ? faktur.CustomerName.Replace(" ", "").Substring(0, 27) + "..." : faktur.CustomerName.Replace(" ", ""),
                                    KODEPOS = faktur.RecipientAddress.Split(',')[faktur.RecipientAddress.Split(',').Length - 1].Substring(1, 5),
                                    EMAIL = faktur.CustomerName.Replace(" ", "").Length > 36 ? faktur.CustomerName.Replace(" ", "").Substring(0, 36) + "@tokopedia.com" : faktur.CustomerName.Replace(" ", "") + "@tokopedia.com",
                                    KODEKABKOT = "3174",
                                    KODEPROV = "31",
                                    NAMA_KABKOT = kabupaten.Length > 50 ? kabupaten.Substring(0, 47) + "..." : kabupaten,
                                    NAMA_PROV = provinsi.Length > 50 ? provinsi.Substring(0, 47) + "..." : provinsi,
                                };
                                newARF01Cs.Add(newPembeli);
                                //ErasoftDbContext.ARF01C.Add(newPembeli);

                                buyercode = newPembeli.BUYER_CODE;
                                al2 = newPembeli.AL2;
                                al3 = newPembeli.AL3;
                            }
                            else
                            {
                                buyercode = cekPembeli.BUYER_CODE;
                                al2 = cekPembeli.AL2;
                                al3 = cekPembeli.AL3;
                            }
                        }
                        #endregion
                        #region insert sit01a
                        if (!string.IsNullOrWhiteSpace(faktur.Invoice))
                        {
                            //jika blank berarti masih faktur yang sama, item ke dua
                            SIT01A newfaktur = new SIT01A
                            {
                                JENIS_FORM = "2",
                                NO_BUKTI = noOrder,
                                NO_F_PAJAK = "-",
                                NO_SO = "-",
                                CUST = cust,
                                NAMAPEMESAN = faktur.Recipient.Length > 30 ? faktur.Recipient.Substring(0, 27) + "..." : faktur.Recipient,
                                PEMESAN = buyercode,
                                NAMA_CUST = nama_cust,
                                AL = faktur.RecipientAddress,
                                TGL = Convert.ToDateTime(faktur.PaymentDate),
                                PPN_Bln_Lapor = Convert.ToByte(Convert.ToDateTime(faktur.PaymentDate).ToString("MM")),
                                PPN_Thn_Lapor = Convert.ToByte(Convert.ToDateTime(faktur.PaymentDate).ToString("yyyy").Substring(2, 2)),
                                USERNAME = uname,
                                JENIS_RETUR = "-",
                                STATUS = "1",
                                ST_POSTING = "T",
                                VLT = "IDR",
                                NO_FA_OUTLET = "-",
                                NO_LPB = "-",
                                GROUP_LIMIT = "-",
                                KODE_ANGKUTAN = "-",
                                JENIS_MOBIL = "-",
                                JTRAN = "SI",
                                JENIS = "1",
                                TUKAR = 1,
                                TUKAR_PPN = 1,
                                SOPIR = "-",
                                KET = "Catatan Dari Pembeli : " + faktur.Notes,
                                PPNBM = 0,
                                NILAI_PPNBM = 0,
                                KODE_SALES = "-",
                                KODE_WIL = "-",
                                U_MUKA = 0,
                                U_MUKA_FA = 0,
                                TERM = 0,
                                TGL_JT_TEMPO = Convert.ToDateTime(faktur.PaymentDate),
                                BRUTO = Convert.ToDouble(faktur.TotalAmountRp.Replace("Rp ", "").Replace(".", "")) - Convert.ToDouble(faktur.TotalShippingFeeRp.Replace("Rp ", "").Replace(".", "")),
                                PPN = 0,
                                NILAI_PPN = 0,
                                DISCOUNT = 0,
                                NILAI_DISC = 0,
                                MATERAI = Convert.ToDouble(faktur.TotalShippingFeeRp.Replace("Rp ", "").Replace(".", "")),
                                NETTO = Convert.ToDouble(faktur.TotalAmountRp.Replace("Rp ", "").Replace(".", "")),
                                TGLINPUT = DateTime.Now,
                                NO_REF = faktur_invoice,
                                NAMA_CUST_QQ = "-",
                                STATUS_LOADING = "-",
                                NO_PO_CUST = "-",
                                PENGIRIM = "-",
                                NAMAPENGIRIM = "-",
                                ZONA = "-",
                                UCAPAN = "-",
                                N_UCAPAN = "-",
                                SUPP = "-",
                                KOMISI = 0,
                                N_KOMISI = 0
                            };
                            newFakturs.Add(newfaktur);
                            //ErasoftDbContext.SIT01A.Add(newfaktur);
                            lastFakturInUpload = faktur_invoice;
                            lastFakturDateInUpload = Convert.ToDateTime(faktur.PaymentDate);
                        }
                        #endregion
                        #region insert sit01b
                        SIT01B newfakturdetail = new SIT01B
                        {
                            JENIS_FORM = "2",
                            NO_BUKTI = noOrder,
                            USERNAME = uname,
                            CATATAN = "-",
                            TGLINPUT = DateTime.Now,
                            //NILAI_DISC = Convert.ToDouble(faktur.DiskonDariPenjual.Replace("Rp ", "").Replace(".", "")),
                            NILAI_DISC = 0,
                            DISCOUNT = 0,
                            //NILAI_DISC_1 = Convert.ToDouble(faktur.DiskonDariPenjual.Replace("Rp ", "").Replace(".", "")),
                            NILAI_DISC_1 = 0,
                            DISCOUNT_2 = 0,
                            NILAI_DISC_2 = 0,
                            DISCOUNT_3 = 0,
                            NILAI_DISC_3 = 0,
                            DISCOUNT_4 = 0,
                            NILAI_DISC_4 = 0,
                            DISCOUNT_5 = 0,
                            NILAI_DISC_5 = 0,
                            DISC_TITIPAN = 0,
                            BRG = string.IsNullOrWhiteSpace(faktur.StockKeepingUnitSKU) ? faktur.ProductID : faktur.StockKeepingUnitSKU,
                            SATUAN = "2",
                            H_SATUAN = Convert.ToDouble(faktur.PriceRp.Replace("Rp ", "").Replace(".", "")),
                            QTY = Convert.ToDouble(faktur.Quantity),
                            HARGA = Convert.ToDouble(faktur.PriceRp.Replace("Rp ", "").Replace(".", "")),
                            QTY_KIRIM = 0,
                            QTY_RETUR = 0,
                            GUDANG = "001" //buat default gudang 001, untuk semua akun baru
                        };
                        //ErasoftDbContext.SIT01B.Add(newfakturdetail);
                        newFaktursDetails.Add(newfakturdetail);
                        #endregion
                    }
                    else
                    {
                        var fakturPerluDiRemove = (from p in newFakturs where p.NO_REF == faktur_invoice select p).FirstOrDefault();
                        if (fakturPerluDiRemove != null)
                        {
                            newFakturs.RemoveAll(a => a.NO_REF == faktur_invoice);
                            var detailFakturPerluDiRemove = (from p in newFaktursDetails where p.NO_BUKTI == fakturPerluDiRemove.NO_BUKTI select p).FirstOrDefault();
                            if (detailFakturPerluDiRemove != null)
                            {
                                newFaktursDetails.RemoveAll(a => a.NO_BUKTI == fakturPerluDiRemove.NO_BUKTI);
                            }
                        }
                    }
                }
                #endregion

                #region commit insert

                //record terakhir
                using (System.Data.Entity.DbContextTransaction transaction = ErasoftDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        ErasoftDbContext.ARF01C.AddRange(newARF01Cs);
                        ErasoftDbContext.SaveChanges();
                        if (newFakturs.Count == 0)
                        {
                            lastFakturInUpload = "";
                            lastFakturDateInUpload = DateTime.Now;
                        }
                        ErasoftDbContext.SIT01A.AddRange(newFakturs);
                        ErasoftDbContext.SaveChanges();
                        ErasoftDbContext.SIT01B.AddRange(newFaktursDetails);
                        ErasoftDbContext.SaveChanges();

                        newLogImportFaktur.LAST_FAKTUR_UPLOADED = lastFakturInUpload;
                        newLogImportFaktur.LAST_FAKTUR_UPLOADED_DATETIME = lastFakturDateInUpload;
                        ErasoftDbContext.LOG_IMPORT_FAKTUR.Add(newLogImportFaktur);
                        ErasoftDbContext.SaveChanges();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            ErasoftDbContext.ARF01C.RemoveRange(newARF01Cs);
                        }
                        catch (Exception)
                        { }
                        try
                        {
                            ErasoftDbContext.SIT01A.RemoveRange(newFakturs);
                        }
                        catch (Exception)
                        { }
                        try
                        {
                            ErasoftDbContext.SIT01B.RemoveRange(newFaktursDetails);
                        }
                        catch (Exception)
                        { }

                        try
                        {
                            message = "Faktur Tokopedia gagal diupload, terjadi error." + System.Environment.NewLine;
                            message += "Error : " + (ex.InnerException == null ? ex.Message : (ex.InnerException.InnerException == null ? ex.InnerException.Message : ex.InnerException.InnerException.Message));
                            tw.WriteLine(message);

                            newLogImportFaktur.LAST_FAKTUR_UPLOADED = "Error. Gagal Upload.";
                            newLogImportFaktur.LAST_FAKTUR_UPLOADED_DATETIME = DateTime.Now;
                            ErasoftDbContext.LOG_IMPORT_FAKTUR.Add(newLogImportFaktur);
                            ErasoftDbContext.SaveChanges();

                            transaction.Commit();
                        }
                        catch (Exception ex2)
                        {
                            transaction.Rollback();
                        }
                    }
                }
                #endregion

                tw.Close();
            }


            var partialVm = new FakturViewModel()
            {
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListImportFaktur = ErasoftDbContext.LOG_IMPORT_FAKTUR.Where(a => a.CUST == cust).OrderByDescending(a => a.UPLOAD_DATETIME).ToList()
            };

            return PartialView("UploadFakturView", partialVm);
            //return new EmptyResult();
            //return File(path, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(path));
        }
        [HttpPost]
        public ActionResult UploadFakturShopee(UploadFakturShopeeDataDetail[] data, string cust, string nama_cust, string perso)
        {
            AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            string uname = sessionData.Account.Username;
            UploadFakturResult result = new UploadFakturResult
            {
                success = "0",
                resultMessage = ""
            };

            #region Logging
            string message = "";
            string filename = "Log_Upload_Inv_Shopee_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".txt";
            var path = Path.Combine(Server.MapPath("~/Content/Uploaded/" + sessionData.Account.DatabasePathErasoft + "/"), filename);

            LOG_IMPORT_FAKTUR newLogImportFaktur = new LOG_IMPORT_FAKTUR
            {
                CUST = cust,
                UPLOADER = uname,
                UPLOAD_DATETIME = DateTime.Now,
                LOG_FILE = filename,
            };
            string lastFakturInUpload = "";
            DateTime lastFakturDateInUpload = DateTime.Now;
            #endregion

            if (data == null)
            {
                return JsonErrorMessage("Format data tidak sesuai");
            }
            else
            {
                if (!System.IO.File.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(Path.Combine(Server.MapPath("~/Content/Uploaded/" + sessionData.Account.DatabasePathErasoft + "/"), ""));
                    var asd = System.IO.File.Create(path);
                    asd.Close();
                }
                StreamWriter tw = new StreamWriter(path);

                #region Proses Upload
                var lastRecnumARF01C = ErasoftDbContext.ARF01C.Max(p => p.RecNum);
                var listFakturInDb = ErasoftDbContext.SIT01A.OrderBy(p => p.RecNum).ToList();
                //var listItem = ErasoftDbContext.STF02.ToList();
                var listItem = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList();

                var digitAkhir = "";
                var noOrder = "";
                var lastRecNum = 0;
                if (listFakturInDb.Count == 0)
                {
                    digitAkhir = "000001";
                    noOrder = $"SI{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                    ErasoftDbContext.Database.ExecuteSqlCommand("DBCC CHECKIDENT (SIT01A, RESEED, 0)");
                }
                else
                {
                    lastRecNum = listFakturInDb.Last().RecNum.HasValue ? Convert.ToInt32(listFakturInDb.Last().RecNum) : 0;
                    if (lastRecNum == 0)
                    {
                        lastRecNum = 1;
                    }
                }
                string buyercode = "";
                string al2 = "";
                string al3 = "";

                bool adaWarning = false;
                bool masihFakturYangSama = true;
                bool fakturLolosValidasi = true;
                bool barangFakturLolosValidasi = true;
                string messageWarning = "";
                string faktur_invoice = "";

                List<ARF01C> newARF01Cs = new List<ARF01C>();
                List<SIT01A> newFakturs = new List<SIT01A>();
                List<SIT01B> newFaktursDetails = new List<SIT01B>();
                for (int i = 0; i < data.Count(); i++)
                {
                    UploadFakturShopeeDataDetail faktur = data[i];

                    #region  validasi
                    //cek faktur sudah pernah di upload
                    if (!string.IsNullOrWhiteSpace(faktur.NoPesanan))
                    {
                        if (i > 0)
                        {
                            masihFakturYangSama = false;
                        }
                        faktur_invoice = faktur.NoPesanan;
                        message = "";
                        messageWarning = "";
                        adaWarning = false;
                        fakturLolosValidasi = true;
                        var cekFakturExists = listFakturInDb.Where(p => p.JENIS_FORM == "2" && p.NO_REF == faktur_invoice).FirstOrDefault();
                        if (cekFakturExists != null)
                        {
                            fakturLolosValidasi = false;
                            //log faktur sudah pernah di upload
                            message = "Faktur [" + faktur_invoice + "] sudah pernah diupload, dengan nomor faktur : [" + cekFakturExists.NO_BUKTI + "]." + System.Environment.NewLine;
                            tw.WriteLine(message);
                        }
                    }
                    else
                    {
                        masihFakturYangSama = true;
                        messageWarning = "";
                    }
                    if (fakturLolosValidasi)
                    {
                        barangFakturLolosValidasi = true;
                        //cek barang sudah ada di master
                        var cekItem = listItem.Where(p => p.BRG == (string.IsNullOrWhiteSpace(faktur.NomorReferensiSKU) ? faktur.SKUInduk : faktur.NomorReferensiSKU)).FirstOrDefault();
                        if (cekItem == null)
                        {
                            //log item belum ada di master
                            barangFakturLolosValidasi = false;
                            adaWarning = true;
                            if (message == "")
                            {
                                message = "Faktur Shopee [" + faktur_invoice + "] gagal diupload." + System.Environment.NewLine;
                                message += "Masalah pada nomor faktur [" + faktur_invoice + "] :" + System.Environment.NewLine;
                                tw.WriteLine(message);
                            }
                            messageWarning = "- Item [" + (string.IsNullOrWhiteSpace(faktur.NomorReferensiSKU) ? faktur.SKUInduk : faktur.NomorReferensiSKU) + "] belum ada di Master Barang MasterOnline." + System.Environment.NewLine;
                            tw.WriteLine(messageWarning);
                        }
                    }
                    #endregion

                    if (fakturLolosValidasi && barangFakturLolosValidasi)
                    {
                        buyercode = "";
                        if (!string.IsNullOrWhiteSpace(faktur.NoPesanan))
                        {
                            lastRecNum++;
                            digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                            noOrder = $"SI{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                        }


                        #region insert pembeli
                        if (!string.IsNullOrWhiteSpace(faktur.NoPesanan))
                        {
                            var cekPembeli = (from p in ErasoftDbContext.ARF01C
                                              where p.EMAIL == (faktur.UsernamePembeli.Length > 39 ? faktur.UsernamePembeli.Substring(0, 39) + "@shopee.com" : faktur.UsernamePembeli + "@shopee.com")
                                              select new { p.BUYER_CODE, p.AL2, p.AL3 }).SingleOrDefault();
                            if (cekPembeli == null)
                            {
                                lastRecnumARF01C++;

                                ARF01C newPembeli = new ARF01C
                                {
                                    BUYER_CODE = lastRecnumARF01C.ToString().PadLeft(10, '0'),
                                    NAMA = faktur.NamaPenerima.Length > 30 ? faktur.NamaPenerima.Substring(0, 27) + "..." : faktur.NamaPenerima,
                                    AL = faktur.AlamatPengiriman,
                                    TLP = faktur.NoTelepon,
                                    PERSO = perso,
                                    TERM = 0,
                                    LIMIT = 0,
                                    PKP = "0",
                                    KLINK = "01",
                                    KODE_CABANG = 1,
                                    VLT = "IDR",
                                    KDHARGA = "01",
                                    AL_KIRIM1 = faktur.AlamatPengiriman.Length > 30 ? faktur.AlamatPengiriman.Substring(0, 30) : faktur.AlamatPengiriman,
                                    AL_KIRIM2 = faktur.AlamatPengiriman.Length > 60 ? faktur.AlamatPengiriman.Substring(30, 30) : faktur.AlamatPengiriman.Substring(30, faktur.AlamatPengiriman.Length - 30),
                                    AL_KIRIM3 = faktur.AlamatPengiriman.Length > 90 ? faktur.AlamatPengiriman.Substring(60, 27) + "..." : faktur.AlamatPengiriman.Substring(60, faktur.AlamatPengiriman.Length - 60),
                                    DISC_NOTA = 0,
                                    NDISC_NOTA = 0,
                                    DISC_ITEM = 0,
                                    NDISC_ITEM = 0,
                                    STATUS = "1",
                                    LABA = 0,
                                    TIDAK_HIT_UANG_R = false,
                                    No_Seri_Pajak = "FP",
                                    TGL_INPUT = DateTime.Now,
                                    USERNAME = faktur.UsernamePembeli.Length > 30 ? faktur.UsernamePembeli.Substring(0, 27) + "..." : faktur.UsernamePembeli,
                                    KODEPOS = faktur.AlamatPengiriman.Substring(faktur.AlamatPengiriman.Length - 5, 5),
                                    EMAIL = faktur.UsernamePembeli.Length > 39 ? faktur.UsernamePembeli.Substring(0, 39) + "@shopee.com" : faktur.UsernamePembeli + "@shopee.com",
                                    KODEKABKOT = "3174",
                                    KODEPROV = "31",
                                    NAMA_KABKOT = faktur.KotaKabupaten.Length > 50 ? faktur.KotaKabupaten.Substring(0, 47) + "..." : faktur.KotaKabupaten,
                                    NAMA_PROV = faktur.Provinsi.Length > 50 ? faktur.Provinsi.Substring(0, 47) + "..." : faktur.Provinsi,
                                };
                                newARF01Cs.Add(newPembeli);

                                buyercode = newPembeli.BUYER_CODE;
                                al2 = newPembeli.AL2;
                                al3 = newPembeli.AL3;
                            }
                            else
                            {
                                buyercode = cekPembeli.BUYER_CODE;
                                al2 = cekPembeli.AL2;
                                al3 = cekPembeli.AL3;
                            }
                        }
                        #endregion
                        #region insert sit01a
                        if (!string.IsNullOrWhiteSpace(faktur.NoPesanan))
                        {
                            SIT01A newfaktur = new SIT01A
                            {
                                JENIS_FORM = "2",
                                NO_BUKTI = noOrder,
                                NO_F_PAJAK = "-",
                                NO_SO = "-",
                                CUST = cust,
                                NAMAPEMESAN = faktur.NamaPenerima.Length > 30 ? faktur.NamaPenerima.Substring(0, 27) + "..." : faktur.NamaPenerima,
                                PEMESAN = buyercode,
                                NAMA_CUST = nama_cust,
                                AL = faktur.AlamatPengiriman,
                                TGL = Convert.ToDateTime(faktur.WaktuPembayaranDilakukan),
                                PPN_Bln_Lapor = Convert.ToByte(Convert.ToDateTime(faktur.WaktuPembayaranDilakukan).ToString("MM")),
                                PPN_Thn_Lapor = Convert.ToByte(Convert.ToDateTime(faktur.WaktuPembayaranDilakukan).ToString("yyyy").Substring(2, 2)),
                                USERNAME = uname,
                                JENIS_RETUR = "-",
                                STATUS = "1",
                                ST_POSTING = "T",
                                VLT = "IDR",
                                NO_FA_OUTLET = "-",
                                NO_LPB = "-",
                                GROUP_LIMIT = "-",
                                KODE_ANGKUTAN = "-",
                                JENIS_MOBIL = "-",
                                JTRAN = "SI",
                                JENIS = "1",
                                TUKAR = 1,
                                TUKAR_PPN = 1,
                                SOPIR = "-",
                                KET = "Catatan Dari Pembeli : " + faktur.CatatandariPembeli + ". Catatan : " + faktur.Catatan,
                                PPNBM = 0,
                                NILAI_PPNBM = 0,
                                KODE_SALES = "-",
                                KODE_WIL = "-",
                                U_MUKA = 0,
                                U_MUKA_FA = 0,
                                TERM = 0,
                                TGL_JT_TEMPO = Convert.ToDateTime(faktur.PesananHarusDikirimkanSebelumMenghindariketerlambatan),
                                BRUTO = Convert.ToDouble(faktur.TotalHargaProduk.Replace("Rp ", "").Replace(".", "")),
                                PPN = 0,
                                NILAI_PPN = 0,
                                DISCOUNT = 0,
                                NILAI_DISC = 0,
                                MATERAI = Convert.ToDouble(faktur.PerkiraanOngkosKirim.Replace("Rp ", "").Replace(".", "")),
                                NETTO = Convert.ToDouble(faktur.TotalHargaProduk.Replace("Rp ", "").Replace(".", "")) + Convert.ToDouble(faktur.PerkiraanOngkosKirim.Replace("Rp ", "").Replace(".", "")),
                                TGLINPUT = DateTime.Now,
                                NO_REF = faktur.NoPesanan,
                                NAMA_CUST_QQ = "-",
                                STATUS_LOADING = "-",
                                NO_PO_CUST = "-",
                                PENGIRIM = "-",
                                NAMAPENGIRIM = "-",
                                ZONA = "-",
                                UCAPAN = "-",
                                N_UCAPAN = "-",
                                SUPP = "-",
                                KOMISI = 0,
                                N_KOMISI = 0
                            };
                            newFakturs.Add(newfaktur);
                        }
                        #endregion
                        #region insert sit01b
                        SIT01B newfakturdetail = new SIT01B
                        {
                            JENIS_FORM = "2",
                            NO_BUKTI = noOrder,
                            USERNAME = uname,
                            CATATAN = "-",
                            TGLINPUT = DateTime.Now,
                            NILAI_DISC = Convert.ToDouble(faktur.DiskonDariPenjual.Replace("Rp ", "").Replace(".", "")),
                            DISCOUNT = 0,
                            NILAI_DISC_1 = Convert.ToDouble(faktur.DiskonDariPenjual.Replace("Rp ", "").Replace(".", "")),
                            DISCOUNT_2 = 0,
                            NILAI_DISC_2 = 0,
                            DISCOUNT_3 = 0,
                            NILAI_DISC_3 = 0,
                            DISCOUNT_4 = 0,
                            NILAI_DISC_4 = 0,
                            DISCOUNT_5 = 0,
                            NILAI_DISC_5 = 0,
                            DISC_TITIPAN = 0,
                            BRG = string.IsNullOrWhiteSpace(faktur.NomorReferensiSKU) ? faktur.SKUInduk : faktur.NomorReferensiSKU,
                            SATUAN = "2",
                            H_SATUAN = Convert.ToDouble(faktur.HargaSebelumDiskon.Replace("Rp ", "").Replace(".", "")),
                            QTY = faktur.Jumlah,
                            HARGA = Convert.ToDouble(faktur.TotalHargaProduk.Replace("Rp ", "").Replace(".", "")),
                            QTY_KIRIM = 0,
                            QTY_RETUR = 0,
                            GUDANG = "001" //buat default gudang 001, untuk semua akun baru
                        };
                        newFaktursDetails.Add(newfakturdetail);

                        #endregion
                    }
                    else
                    {
                        var fakturPerluDiRemove = (from p in newFakturs where p.NO_REF == faktur_invoice select p).FirstOrDefault();
                        if (fakturPerluDiRemove != null)
                        {
                            newFakturs.RemoveAll(a => a.NO_REF == faktur_invoice);
                            var detailFakturPerluDiRemove = (from p in newFaktursDetails where p.NO_BUKTI == fakturPerluDiRemove.NO_BUKTI select p).FirstOrDefault();
                            if (detailFakturPerluDiRemove != null)
                            {
                                newFaktursDetails.RemoveAll(a => a.NO_BUKTI == fakturPerluDiRemove.NO_BUKTI);
                            }
                        }
                    }
                }
                #endregion

                #region commit insert

                //record terakhir
                using (System.Data.Entity.DbContextTransaction transaction = ErasoftDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        ErasoftDbContext.ARF01C.AddRange(newARF01Cs);
                        ErasoftDbContext.SaveChanges();
                        if (newFakturs.Count == 0)
                        {
                            lastFakturInUpload = "";
                            lastFakturDateInUpload = DateTime.Now;
                        }
                        ErasoftDbContext.SIT01A.AddRange(newFakturs);
                        ErasoftDbContext.SaveChanges();
                        ErasoftDbContext.SIT01B.AddRange(newFaktursDetails);
                        ErasoftDbContext.SaveChanges();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        message = "Faktur Shopee gagal diupload, terjadi error." + System.Environment.NewLine;
                        message += "Error : " + (ex.InnerException.Message == null ? ex.Message : ex.InnerException.Message);
                        tw.WriteLine(message);
                    }
                }
                #endregion

                tw.Close();
            }

            newLogImportFaktur.LAST_FAKTUR_UPLOADED = lastFakturInUpload;
            newLogImportFaktur.LAST_FAKTUR_UPLOADED_DATETIME = lastFakturDateInUpload;
            ErasoftDbContext.LOG_IMPORT_FAKTUR.Add(newLogImportFaktur);
            ErasoftDbContext.SaveChanges();

            var partialVm = new FakturViewModel()
            {
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListImportFaktur = ErasoftDbContext.LOG_IMPORT_FAKTUR.Where(a => a.CUST == cust).OrderByDescending(a => a.UPLOAD_DATETIME).ToList()
            };

            return PartialView("UploadFakturView", partialVm);
        }

        // =============================================== END ADD BY CALVIN -- Bagian Import Data Faktur
        // =============================================== Bagian Promosi (START)

        [Route("manage/master/promosi-barang")]
        public ActionResult Promosi()
        {
            var vm = new PromosiViewModel()
            {
                ListPromosi = ErasoftDbContext.PROMOSI.ToList(),
                //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList()
            };

            return View("PromosiMenu", vm);
        }

        public ActionResult RefreshPromosiForm()
        {
            try
            {
                var vm = new PromosiViewModel()
                {
                    ListPromosi = ErasoftDbContext.PROMOSI.ToList(),
                    //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList()
                };

                return PartialView("BarangPromosiPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult RefreshTablePromosi()
        {
            var vm = new PromosiViewModel()
            {
                ListPromosi = ErasoftDbContext.PROMOSI.ToList(),
                //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
            };

            return PartialView("TablePromosiPartial", vm);
        }

        public ActionResult EditPromosi(int? orderId)
        {
            var promosiInDb = ErasoftDbContext.PROMOSI.Single(p => p.RecNum == orderId);

            var vm = new PromosiViewModel()
            {
                Promosi = promosiInDb,
                ListPromosi = ErasoftDbContext.PROMOSI.ToList(),
                ListPromosiDetail = ErasoftDbContext.DETAILPROMOSI.Where(pd => pd.RecNumPromosi == promosiInDb.RecNum).ToList(),
                //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList()
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList()
            };

            return PartialView("BarangPromosiPartial", vm);
        }

        public ActionResult DeletePromosi(int? orderId)
        {
            var promosiInDb = ErasoftDbContext.PROMOSI.Single(p => p.RecNum == orderId);
            var detailPromosiInDb = ErasoftDbContext.DETAILPROMOSI.Where(dp => dp.RecNumPromosi == promosiInDb.RecNum).ToList();

            foreach (var barang in detailPromosiInDb)
            {
                ErasoftDbContext.DETAILPROMOSI.Remove(barang);
            }

            ErasoftDbContext.PROMOSI.Remove(promosiInDb);
            ErasoftDbContext.SaveChanges();

            //add by calvin 26 desember 2018
            //var customer = ErasoftDbContext.ARF01.SingleOrDefault(c => c.Kode == promosiInDb.NAMA_MARKET);
            var customer = ErasoftDbContext.ARF01.SingleOrDefault(c => c.CUST == promosiInDb.NAMA_MARKET);
            var kdShopee = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "SHOPEE").IdMarket.ToString();
            var kdLazada = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "LAZADA").IdMarket.ToString();

            if (customer.NAMA.Equals(kdShopee))
            {
                if (!string.IsNullOrWhiteSpace(customer.Sort1_Cust))
                {
                    if (!string.IsNullOrWhiteSpace(promosiInDb.MP_PROMO_ID))
                    {
                        var ShopeeApi = new ShopeeController();

                        ShopeeController.ShopeeAPIData data = new ShopeeController.ShopeeAPIData()
                        {
                            merchant_code = customer.Sort1_Cust,
                        };
                        Task.Run(() => ShopeeApi.DeleteDiscount(data, Convert.ToInt64(promosiInDb.MP_PROMO_ID))).Wait();
                    }
                }
            }
            else if (customer.NAMA.Equals(kdLazada))
            {
                if (!string.IsNullOrWhiteSpace(customer.TOKEN))
                {
                    var lazadaApi = new LazadaController();
                    foreach (var promo in detailPromosiInDb)
                    {
                        var brgInDB = ErasoftDbContext.STF02H.Where(m => m.BRG == promo.KODE_BRG && m.IDMARKET == customer.RecNum).FirstOrDefault();
                        if (brgInDB != null)
                        {
                            if (!string.IsNullOrEmpty(brgInDB.BRG_MP))
                            {
                                var promoPrice = brgInDB.HJUAL;
                                lazadaApi.UpdatePromoPrice(brgInDB.BRG_MP, promoPrice, DateTime.Today, DateTime.Today, customer.TOKEN);
                            }
                        }
                    }
                }
            }
            //end add by calvin 26 desember 2018

            var vm = new PromosiViewModel()
            {
                ListPromosi = ErasoftDbContext.PROMOSI.ToList(),
                //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
            };

            return PartialView("TablePromosiPartial", vm);
        }

        [HttpGet]
        public ActionResult DeleteBarangPromosi(int noUrut)
        {
            try
            {
                var barangPromosiInDb = ErasoftDbContext.DETAILPROMOSI.Single(b => b.RecNum == noUrut);
                var promosiInDb = ErasoftDbContext.PROMOSI.Single(p => p.RecNum == barangPromosiInDb.RecNumPromosi);

                ErasoftDbContext.DETAILPROMOSI.Remove(barangPromosiInDb);
                ErasoftDbContext.SaveChanges();

                //add by calvin 26 desember 2018
                //var customer = ErasoftDbContext.ARF01.SingleOrDefault(c => c.Kode == promosiInDb.NAMA_MARKET);
                var customer = ErasoftDbContext.ARF01.SingleOrDefault(c => c.CUST == promosiInDb.NAMA_MARKET);
                var kdShopee = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "SHOPEE").IdMarket.ToString();
                var kdLazada = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "LAZADA").IdMarket.ToString();

                if (customer.NAMA.Equals(kdShopee))
                {
                    if (!string.IsNullOrWhiteSpace(customer.Sort1_Cust))
                    {
                        if (!string.IsNullOrWhiteSpace(promosiInDb.MP_PROMO_ID))
                        {
                            var ShopeeApi = new ShopeeController();

                            ShopeeController.ShopeeAPIData data = new ShopeeController.ShopeeAPIData()
                            {
                                merchant_code = customer.Sort1_Cust,
                            };
                            Task.Run(() => ShopeeApi.DeleteDiscountItem(data, Convert.ToInt64(promosiInDb.MP_PROMO_ID), barangPromosiInDb)).Wait();
                        }
                    }
                }
                else if (customer.NAMA.Equals(kdLazada))
                {
                    if (!string.IsNullOrWhiteSpace(customer.TOKEN))
                    {
                        var lazadaApi = new LazadaController();
                        var brgInDB = ErasoftDbContext.STF02H.Where(m => m.BRG == barangPromosiInDb.KODE_BRG && m.IDMARKET == customer.RecNum).FirstOrDefault();
                        if (brgInDB != null)
                        {
                            if (!string.IsNullOrEmpty(brgInDB.BRG_MP))
                            {
                                var promoPrice = brgInDB.HJUAL;
                                lazadaApi.UpdatePromoPrice(brgInDB.BRG_MP, promoPrice, DateTime.Today, DateTime.Today, customer.TOKEN);
                            }
                        }

                    }
                }
                //end add by calvin 26 desember 2018

                var vm = new PromosiViewModel()
                {
                    Promosi = ErasoftDbContext.PROMOSI.Single(p => p.RecNum == promosiInDb.RecNum),
                    ListPromosi = ErasoftDbContext.PROMOSI.ToList(),
                    ListPromosiDetail = ErasoftDbContext.DETAILPROMOSI.Where(pd => pd.RecNumPromosi == promosiInDb.RecNum).ToList(),
                    //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList()
                    ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList()
                };

                return PartialView("BarangPromosiPartial", vm);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult SavePromosi(PromosiViewModel dataVm)
        {
            if (!ModelState.IsValid)
            {
                dataVm.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(dataVm, JsonRequestBehavior.AllowGet);
            }

            if (dataVm.Promosi.RecNum == null)
            {
                var listFakturInDb = ErasoftDbContext.PROMOSI.OrderBy(p => p.RecNum).ToList();
                int? lastRecNum = 0;

                if (listFakturInDb.Count == 0)
                {
                    ErasoftDbContext.Database.ExecuteSqlCommand("DBCC CHECKIDENT (Promosis, RESEED, 0)");
                    lastRecNum++;
                }
                else
                {
                    lastRecNum = listFakturInDb.Last().RecNum;
                    lastRecNum++;
                }

                ErasoftDbContext.PROMOSI.Add(dataVm.Promosi);
                ErasoftDbContext.SaveChanges();

                if (dataVm.PromosiDetail.RecNum == null)
                {
                    //change by nurul 3/1/2019 -- dataVm.PromosiDetail.RecNumPromosi = lastRecNum;
                    dataVm.PromosiDetail.RecNumPromosi = dataVm.Promosi.RecNum;
                    ErasoftDbContext.DETAILPROMOSI.Add(dataVm.PromosiDetail);
                    ErasoftDbContext.SaveChanges();
                }

                //add by calvin 26 desember 2018
                //change by nurul 3/1/2019 -- var customer = ErasoftDbContext.ARF01.SingleOrDefault(c => c.Kode == dataVm.Promosi.NAMA_MARKET);
                var customer = ErasoftDbContext.ARF01.SingleOrDefault(c => c.CUST == dataVm.Promosi.NAMA_MARKET);
                var kdShopee = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "SHOPEE").IdMarket.ToString();
                var kdLazada = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "LAZADA").IdMarket.ToString();

                if (customer.NAMA.Equals(kdShopee))
                {
                    if (!string.IsNullOrWhiteSpace(customer.Sort1_Cust))
                    {
                        var ShopeeApi = new ShopeeController();

                        ShopeeController.ShopeeAPIData data = new ShopeeController.ShopeeAPIData()
                        {
                            merchant_code = customer.Sort1_Cust,
                        };
                        //Task.Run(() => ShopeeApi.AddDiscount(data, lastRecNum.HasValue ? lastRecNum.Value : 0)).Wait();
                        Task.Run(() => ShopeeApi.AddDiscount(data, dataVm.Promosi.RecNum.HasValue ? dataVm.Promosi.RecNum.Value : 0)).Wait();

                    }
                }
                else if (customer.NAMA.Equals(kdLazada))
                {
                    if (!string.IsNullOrWhiteSpace(customer.TOKEN))
                    {
                        var lazadaApi = new LazadaController();
                        var brgInDB = ErasoftDbContext.STF02H.Where(m => m.BRG == dataVm.PromosiDetail.KODE_BRG && m.IDMARKET == customer.RecNum).FirstOrDefault();
                        if (brgInDB != null)
                        {
                            if (!string.IsNullOrEmpty(brgInDB.BRG_MP))
                            {
                                var promoPrice = dataVm.PromosiDetail.HARGA_PROMOSI;
                                if (promoPrice == 0)
                                {
                                    promoPrice = brgInDB.HJUAL - (brgInDB.HJUAL * dataVm.PromosiDetail.PERSEN_PROMOSI / 100);
                                }
                                lazadaApi.UpdatePromoPrice(brgInDB.BRG_MP, promoPrice, dataVm.Promosi.TGL_MULAI ?? DateTime.Today, dataVm.Promosi.TGL_AKHIR ?? DateTime.Today, customer.TOKEN);
                            }
                        }

                    }
                }
                //end add by calvin 26 desember 2018
            }
            else
            {
                var promosiInDb = ErasoftDbContext.PROMOSI.Single(p => p.RecNum == dataVm.Promosi.RecNum);

                promosiInDb.NAMA_PROMOSI = dataVm.Promosi.NAMA_PROMOSI;
                promosiInDb.NAMA_MARKET = dataVm.Promosi.NAMA_MARKET;
                promosiInDb.TGL_MULAI = dataVm.Promosi.TGL_MULAI;
                promosiInDb.TGL_AKHIR = dataVm.Promosi.TGL_AKHIR;

                if (dataVm.PromosiDetail.RecNum == null)
                {
                    dataVm.PromosiDetail.RecNumPromosi = promosiInDb.RecNum;
                    ErasoftDbContext.DETAILPROMOSI.Add(dataVm.PromosiDetail);
                    ErasoftDbContext.SaveChanges();

                    //add by calvin 26 desember 2018
                    //change by nurul 3/1/2019 -- var customer = ErasoftDbContext.ARF01.SingleOrDefault(c => c.Kode == dataVm.Promosi.NAMA_MARKET);
                    var customer = ErasoftDbContext.ARF01.SingleOrDefault(c => c.CUST == dataVm.Promosi.NAMA_MARKET);
                    var kdShopee = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "SHOPEE").IdMarket.ToString();
                    var kdLazada = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "LAZADA").IdMarket.ToString();

                    if (customer.NAMA.Equals(kdShopee))
                    {
                        if (!string.IsNullOrWhiteSpace(customer.Sort1_Cust))
                        {
                            if (!string.IsNullOrWhiteSpace(dataVm.Promosi.MP_PROMO_ID))
                            {
                                var ShopeeApi = new ShopeeController();

                                ShopeeController.ShopeeAPIData data = new ShopeeController.ShopeeAPIData()
                                {
                                    merchant_code = customer.Sort1_Cust,
                                };
                                Task.Run(() => ShopeeApi.AddDiscountItem(data, Convert.ToInt64(dataVm.Promosi.MP_PROMO_ID), dataVm.PromosiDetail)).Wait();
                            }
                        }
                    }
                    else if (customer.NAMA.Equals(kdLazada))
                    {
                        if (!string.IsNullOrWhiteSpace(customer.TOKEN))
                        {
                            var lazadaApi = new LazadaController();
                            var brgInDB = ErasoftDbContext.STF02H.Where(m => m.BRG == dataVm.PromosiDetail.KODE_BRG && m.IDMARKET == customer.RecNum).FirstOrDefault();
                            if (brgInDB != null)
                            {
                                if (!string.IsNullOrEmpty(brgInDB.BRG_MP))
                                {
                                    var promoPrice = dataVm.PromosiDetail.HARGA_PROMOSI;
                                    if (promoPrice == 0)
                                    {
                                        promoPrice = brgInDB.HJUAL - (brgInDB.HJUAL * dataVm.PromosiDetail.PERSEN_PROMOSI / 100);
                                    }
                                    lazadaApi.UpdatePromoPrice(brgInDB.BRG_MP, promoPrice, dataVm.Promosi.TGL_MULAI ?? DateTime.Today, dataVm.Promosi.TGL_AKHIR ?? DateTime.Today, customer.TOKEN);
                                }
                            }

                        }
                    }
                    //end add by calvin 26 desember 2018
                }
            }

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            var vm = new PromosiViewModel()
            {
                Promosi = ErasoftDbContext.PROMOSI.Single(p => p.RecNum == dataVm.Promosi.RecNum),
                ListPromosiDetail = ErasoftDbContext.DETAILPROMOSI.Where(pd => pd.RecNumPromosi == dataVm.Promosi.RecNum).ToList(),
                //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList()
            };

            return PartialView("BarangPromosiPartial", vm);
        }

        [HttpPost]
        public ActionResult UpdatePromosi(UpdateData dataUpdate)
        {
            var promosiInDb = ErasoftDbContext.PROMOSI.Single(p => p.RecNum == dataUpdate.RecNumPromosi);
            promosiInDb.NAMA_MARKET = dataUpdate.NamaMarket;
            promosiInDb.TGL_MULAI = DateTime.ParseExact(dataUpdate.TglMulai, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            promosiInDb.TGL_AKHIR = DateTime.ParseExact(dataUpdate.TglAkhir, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

            ErasoftDbContext.SaveChanges();

            return new EmptyResult();
        }

        // =============================================== Bagian Promosi (END)

        // =============================================== Bagian Harga Jual Barang (START)

        [Route("manage/master/harga-jual-barang")]
        public ActionResult HargaJual()
        {
            var vm = new HargaJualViewModel()
            {
                //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListHargaJualPerMarket = ErasoftDbContext.STF02H.ToList(),
                ListHargaTerakhir = ErasoftDbContext.STF10.ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),

            };

            return View("HargaJualMenu", vm);
        }

        [HttpGet]
        public ActionResult UbahHargaJual(int? recNum, double hargaJualBaru)
        {
            var ret = new ReturnJson();
            var hJualInDb = ErasoftDbContext.STF02H.SingleOrDefault(h => h.RecNum == recNum);
            //change by nurul 18/1/2019 -- var brg = ErasoftDbContext.STF02.SingleOrDefault(b => b.BRG == hJualInDb.BRG);
            var brg = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").SingleOrDefault(b => b.BRG == hJualInDb.BRG);
            if (hJualInDb == null)
            {
                ret.message = "No Data Found!";
                return Json(ret, JsonRequestBehavior.AllowGet);
            }

            //add by Tri, validasi harga per marketplace            
            var kdBL = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BUKALAPAK").IdMarket.ToString();
            var kdLazada = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "LAZADA").IdMarket.ToString();
            var kdBlibli = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BLIBLI").IdMarket.ToString();
            var kdElevenia = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "ELEVENIA").IdMarket.ToString();
            var kdShopee = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "SHOPEE").IdMarket.ToString();
            var customer = ErasoftDbContext.ARF01.SingleOrDefault(c => c.RecNum == hJualInDb.IDMARKET);
            if (customer.NAMA.Equals(kdLazada))
            {
                if (hargaJualBaru < 3000)
                {
                    ret.message = "Harga Jual harus lebih dari 3000.";
                    return Json(ret, JsonRequestBehavior.AllowGet);
                }
                else if (hargaJualBaru % 100 != 0)
                {
                    ret.message = "Harga Jual harus kelipatan 100.";
                    return Json(ret, JsonRequestBehavior.AllowGet);

                }
            }
            else if (customer.NAMA.Equals(kdBlibli))
            {
                if (hargaJualBaru < 1100)
                {
                    ret.message = "Harga Jual minimal 1100.";
                    return Json(ret, JsonRequestBehavior.AllowGet);
                }
            }
            else if (customer.NAMA.Equals(kdBL) || customer.NAMA.Equals(kdElevenia))
            {
                if (hargaJualBaru < 100)
                {
                    ret.message = "Harga Jual harus lebih dari 100.";
                    return Json(ret, JsonRequestBehavior.AllowGet);
                }
                else if (hargaJualBaru % 100 != 0)
                {
                    ret.message = "Harga Jual harus kelipatan 100.";
                    return Json(ret, JsonRequestBehavior.AllowGet);
                }
            }
            //end add by Tri, validasi harga per marketplace

            hJualInDb.HJUAL = hargaJualBaru;
            ErasoftDbContext.SaveChanges();

            var qtyOnHand = GetQOHSTF08A(hJualInDb.BRG, "ALL");

            //add by Tri, update harga ke marketplace
            if (customer.NAMA.Equals(kdLazada))
            {
                var lzdApi = new LazadaController();
                lzdApi.UpdatePriceQuantity(hJualInDb.BRG_MP, hargaJualBaru.ToString(), "", customer.TOKEN);
            }
            else if (customer.NAMA.Equals(kdBL))
            {
                var blApi = new BukaLapakController();
                blApi.updateProduk(hJualInDb.BRG, hJualInDb.BRG_MP, hargaJualBaru.ToString(), "", customer.API_KEY, customer.TOKEN);
            }
            else if (customer.NAMA.Equals(kdBlibli))
            {
                BlibliController.BlibliAPIData iden = new BlibliController.BlibliAPIData
                {
                    merchant_code = customer.Sort1_Cust,
                    API_client_password = customer.API_CLIENT_P,
                    API_client_username = customer.API_CLIENT_U,
                    API_secret_key = customer.API_KEY,
                    token = customer.TOKEN,
                    mta_username_email_merchant = customer.EMAIL,
                    mta_password_password_merchant = customer.PASSWORD,
                    idmarket = customer.RecNum.Value
                };
                BlibliController.BlibliProductData data = new BlibliController.BlibliProductData
                {
                    kode = brg.BRG,
                    kode_mp = hJualInDb.BRG_MP,
                    Qty = Convert.ToString(qtyOnHand),
                    MinQty = "0",
                    nama = brg.NAMA
                };
                data.Price = hargaJualBaru.ToString();
                data.MarketPrice = hJualInDb.HJUAL.ToString();
                var display = Convert.ToBoolean(hJualInDb.DISPLAY);
                data.display = display ? "true" : "false";
                new BlibliController().UpdateProdukQOH_Display(iden, data);
            }
            else if (customer.NAMA.Equals(kdElevenia))
            {
                string[] imgID = new string[3];
                //change by calvin 4 desember 2018
                //                for (int i = 0; i < 3; i++)
                //                {
                //#if AWS
                //                    imgID[i] = "https://masteronline.co.id/ele/image/" + $"FotoProduk-{brg.USERNAME}-{brg.BRG}-foto-{i + 1}";
                //#else
                //                    imgID[i] = "https://dev.masteronline.co.id/ele/image/" + $"FotoProduk-{brg.USERNAME}-{brg.BRG}-foto-{i + 1}";
                //#endif
                //                }
                for (int i = 0; i < 3; i++)
                {
                    switch (i)
                    {
                        case 0:
                            imgID[0] = brg.LINK_GAMBAR_1;
                            break;
                        case 1:
                            imgID[1] = brg.LINK_GAMBAR_2;
                            break;
                        case 2:
                            imgID[2] = brg.LINK_GAMBAR_3;
                            break;
                    }
                }
                //end change by calvin 4 desember 2018
                EleveniaController.EleveniaProductData data = new EleveniaController.EleveniaProductData
                {
                    api_key = customer.API_KEY,
                    kode = hJualInDb.BRG,
                    nama = brg.NAMA + ' ' + brg.NAMA2 + ' ' + brg.NAMA3,
                    berat = (brg.BERAT / 1000).ToString(),//MO save dalam Gram, Elevenia dalam Kilogram
                    imgUrl = imgID,
                    Keterangan = brg.Deskripsi,
                    Qty = Convert.ToString(qtyOnHand),
                    DeliveryTempNo = hJualInDb.DeliveryTempElevenia.ToString(),
                    IDMarket = customer.RecNum.ToString(),
                };
                data.Brand = ErasoftDbContext.STF02E.SingleOrDefault(m => m.KODE == brg.Sort2 && m.LEVEL == "2").KET;
                data.Price = hargaJualBaru.ToString();
                data.kode_mp = hJualInDb.BRG_MP;

                var display = Convert.ToBoolean(hJualInDb.DISPLAY);
                if (!string.IsNullOrEmpty(data.kode_mp))
                {
                    var result = new EleveniaController().UpdateProduct(data);
                }
            }
            //end add by Tri, update harga ke marketplace
            //add by calvin 18 desember 2018
            else if (customer.NAMA.Equals(kdShopee))
            {
                if (!string.IsNullOrWhiteSpace(customer.Sort1_Cust))
                {
                    var ShopeeApi = new ShopeeController();

                    ShopeeController.ShopeeAPIData data = new ShopeeController.ShopeeAPIData()
                    {
                        merchant_code = customer.Sort1_Cust,
                    };
                    if (hJualInDb.BRG_MP != "")
                    {
                        string[] brg_mp = hJualInDb.BRG_MP.Split(';');
                        if (brg_mp.Count() == 2)
                        {
                            if (brg_mp[1] == "0")
                            {
                                Task.Run(() => ShopeeApi.UpdatePrice(data, hJualInDb.BRG_MP, (float)hargaJualBaru)).Wait();
                            }
                            else if (brg_mp[1] != "")
                            {
                                Task.Run(() => ShopeeApi.UpdateVariationPrice(data, hJualInDb.BRG_MP, (float)hargaJualBaru)).Wait();
                            }
                        }
                    }
                }
            }
            //end add by calvin 18 desember 2018

            var vm = new HargaJualViewModel()
            {
                //change by nurul 18/1/2019 -- ListBarang = ErasoftDbContext.STF02.ToList(),
                ListBarang = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").ToList(),
                ListHargaJualPerMarket = ErasoftDbContext.STF02H.ToList(),
                ListHargaTerakhir = ErasoftDbContext.STF10.ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
            };

            return PartialView("TableHargaJualPartial", vm);
        }

        public class ReturnJson
        {
            public string message { get; set; }
            public bool error { get; set; }
        }
        // =============================================== Bagian Harga Jual Barang (END)

        // =============================================== Bagian Subscription (START)

        [Route("manage/Subscription")]
        public ActionResult Subscription()
        {
            var vm = new SubsViewModel()
            {
                ListSubs = MoDbContext.Subscription.ToList(),
                loggedin = true
            };

            return View(vm);
        }

        // =============================================== Bagian Subscription (END)

        // =============================================== Bagian Support (START)

        [Route("manage/SupportOnline")]
        public ActionResult SupportOnline()
        {

            return View();
        }

        [Route("manage/SupportOffline")]
        public ActionResult SupportOffline()
        {

            return View();
        }
        // =============================================== Bagian Support (END)

        // =============================================== Bagian Upload Barang (START)

        [Route("manage/master/uploadbarang")]
        public ActionResult UploadBarang()
        {
            var barangVm = new UploadBarangViewModel()
            {
                ListTempBrg = new List<TEMP_BRG_MP>(),
                ListMarket = ErasoftDbContext.ARF01.ToList(),
                Stf02 = new STF02(),
                TempBrg = new TEMP_BRG_MP(),
            };
            //add and remark by calvin, untuk excel
            //ProsesTempExcelAutoCompleteBrg("000003");
            //end add and remark by calvin, untuk excel

            //List<string> listBrg = new List<string>();
            //var stt01b = ErasoftDbContext.STT01B.Select(p => p.Kobar).FirstOrDefault();
            //listBrg.Add(stt01b);
            //updateStockMarketPlace(listBrg);

            //var shoAPI = new ShopeeController();
            //ShopeeController.ShopeeAPIData data = new ShopeeController.ShopeeAPIData()
            //{
            //    merchant_code = "6297330",
            //};
            //ShopeeController.ShopeeGetParameterForInitLogisticResult InitParam;
            //InitParam = shoAPI.GetParameterForInitLogistic(data, "");
            //var InitParam = shoAPI.GetParameterForInitLogistic(data, "19012314340WD5C");

            return View(barangVm);
        }

        public ActionResult RefreshTableUploadBarang(string cust)
        {
            var barangVm = new UploadBarangViewModel()
            {
                ListTempBrg = ErasoftDbContext.TEMP_BRG_MP.Where(b => b.CUST.Equals(cust)).ToList(),
                ListMarket = ErasoftDbContext.ARF01.ToList(),
                Stf02 = new STF02(),
                TempBrg = new TEMP_BRG_MP(),
            };

            return PartialView("TableUploadBarangPartial", barangVm);
        }

        public ActionResult RefreshFormUploadBarang()
        {
            var barangVm = new UploadBarangViewModel()
            {
                ListTempBrg = ErasoftDbContext.TEMP_BRG_MP.ToList(),
                ListMarket = ErasoftDbContext.ARF01.ToList(),
                Stf02 = new STF02(),
                TempBrg = new TEMP_BRG_MP(),
            };

            return PartialView("TableUploadBarangPartial", barangVm);
        }

        public ActionResult UploadSatuBarang(UploadBarangViewModel data)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                string listError = "Invalid data :";
                foreach (var e in errors)
                {
                    listError += "\n" + e.Replace("Sort1", "Kategori Barang").Replace("Sort2", "Merek Barang");
                }
                return JsonErrorMessage(listError);
            }
            #region validasi harga

            var kdBL = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BUKALAPAK");
            var kdLazada = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "LAZADA");
            var kdBlibli = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BLIBLI");
            var kdElevenia = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "ELEVENIA");
            var kdShopee = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "SHOPEE");

            var kdMarket = ErasoftDbContext.ARF01.Where(m => m.RecNum == data.TempBrg.IDMARKET).FirstOrDefault().NAMA;
            if (kdMarket == kdLazada.IdMarket.ToString())
            {
                if (data.Stf02.HJUAL < 3000)
                {
                    return JsonErrorMessage("Harga Jual harus lebih dari 3000.");
                }
                else if (data.Stf02.HJUAL % 100 != 0)
                {
                    return JsonErrorMessage("Harga Jual harus kelipatan 100.");
                }
            }
            else if (kdMarket == kdBlibli.IdMarket.ToString())
            {
                if (data.Stf02.HJUAL < 1100)
                {
                    return JsonErrorMessage("Harga Jual minimal 1100.");
                }
            }
            else if (kdMarket == kdBL.IdMarket.ToString() || kdMarket == kdElevenia.IdMarket.ToString())
            {
                if (data.Stf02.HJUAL < 100)
                {
                    return JsonErrorMessage("Harga Jual harus lebih dari 100.");
                }
                else if (data.Stf02.HJUAL % 100 != 0)
                {
                    return JsonErrorMessage("Harga Jual harus kelipatan 100.");
                }
            }
            else if (kdMarket == kdShopee.IdMarket.ToString())
            {
                if (data.Stf02.HJUAL < 100)
                {
                    return JsonErrorMessage("Harga Jual harus lebih dari 100.");
                }
                //else if (data.Stf02.HJUAL > 9999999999999)
                //{
                //    return JsonErrorMessage("Harga Jual tidak boleh lebih dari 9,999,999,999,999.");
                //}
            }
            #endregion
            if (data != null)
            {
                string username = "";
                AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
                if (sessionData?.Account != null)
                {
                    username = sessionData.Account.Username;

                }
                else
                {
                    if (sessionData?.User != null)
                    {
                        username = sessionData.User.Username;
                    }
                }

                var tempBrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP == data.TempBrg.BRG_MP).FirstOrDefault();
                var customer = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper().Equals(data.TempBrg.CUST.ToUpper())).FirstOrDefault();
                if (tempBrginDB != null)
                {
                    if (data.Stf02 != null)
                    {
                        data.Stf02.Deskripsi = HttpUtility.HtmlDecode(data.Stf02.Deskripsi);
                        var tokped = MoDbContext.Marketplaces.Where(a => a.NamaMarket.ToUpper() == "TOKOPEDIA").FirstOrDefault().IdMarket;

                        if (customer.NAMA != Convert.ToString(tokped))
                        {
                            if (!string.IsNullOrEmpty(data.TempBrg.KODE_BRG_INDUK))//handle induk dari barang varian
                            {
                                bool createSTF02Induk = true;
                                var brgInduk = ErasoftDbContext.STF02.Where(b => b.BRG == data.TempBrg.KODE_BRG_INDUK).FirstOrDefault();
                                var tempBrgInduk = ErasoftDbContext.TEMP_BRG_MP.Where(b => b.BRG_MP == tempBrginDB.KODE_BRG_INDUK && b.CUST == data.TempBrg.CUST).FirstOrDefault();
                                if (brgInduk != null)
                                {
                                    var stf02h_induk = ErasoftDbContext.STF02H.Where(b => b.BRG == brgInduk.BRG && b.IDMARKET == customer.RecNum).FirstOrDefault();
                                    if (stf02h_induk == null)
                                    {
                                        createSTF02Induk = false;
                                        if (tempBrgInduk != null)
                                        {
                                            var ret1 = AutoSyncBrgInduk(data.Stf02, tempBrgInduk, data.TempBrg.KODE_BRG_INDUK, customer, username, createSTF02Induk);
                                            if (ret1.status == 0)
                                                return JsonErrorMessage(ret1.message);
                                        }
                                        else
                                        {
                                            //change 25 Feb 2019
                                            //return JsonErrorMessage("Kode Barang Induk tidak ditemukan.");
                                            stf02h_induk = ErasoftDbContext.STF02H.Where(b => b.BRG == tempBrginDB.KODE_BRG_INDUK && b.IDMARKET == customer.RecNum).FirstOrDefault();
                                            if (stf02h_induk != null)
                                            {
                                                //stf02h_induk.BRG = data.TempBrg.KODE_BRG_INDUK;
                                                //stf02h_induk.RecNum = 0;
                                                var dupeStf02h = new STF02H
                                                {
                                                    BRG = data.TempBrg.KODE_BRG_INDUK,
                                                    BRG_MP = stf02h_induk.BRG_MP,
                                                    CATEGORY_CODE = stf02h_induk.CATEGORY_CODE,
                                                    CATEGORY_NAME = stf02h_induk.CATEGORY_NAME,
                                                    HJUAL = stf02h_induk.HJUAL,
                                                    IDMARKET = stf02h_induk.IDMARKET,
                                                    AKUNMARKET = stf02h_induk.AKUNMARKET,
                                                    USERNAME = stf02h_induk.USERNAME,
                                                    DISPLAY = stf02h_induk.DISPLAY,
                                                    DeliveryTempElevenia = stf02h_induk.DeliveryTempElevenia,
                                                    PICKUP_POINT = stf02h_induk.PICKUP_POINT
                                                };
                                                #region attribute mp
                                                dupeStf02h.ACODE_1 = stf02h_induk.ACODE_1;
                                                dupeStf02h.ANAME_1 = stf02h_induk.ANAME_1;
                                                dupeStf02h.AVALUE_1 = stf02h_induk.AVALUE_1;
                                                dupeStf02h.ACODE_2 = stf02h_induk.ACODE_2;
                                                dupeStf02h.ANAME_2 = stf02h_induk.ANAME_2;
                                                dupeStf02h.AVALUE_2 = stf02h_induk.AVALUE_2;
                                                dupeStf02h.ACODE_3 = stf02h_induk.ACODE_3;
                                                dupeStf02h.ANAME_3 = stf02h_induk.ANAME_3;
                                                dupeStf02h.AVALUE_3 = stf02h_induk.AVALUE_3;
                                                dupeStf02h.ACODE_4 = stf02h_induk.ACODE_4;
                                                dupeStf02h.ANAME_4 = stf02h_induk.ANAME_4;
                                                dupeStf02h.AVALUE_4 = stf02h_induk.AVALUE_4;
                                                dupeStf02h.ACODE_5 = stf02h_induk.ACODE_5;
                                                dupeStf02h.ANAME_5 = stf02h_induk.ANAME_5;
                                                dupeStf02h.AVALUE_5 = stf02h_induk.AVALUE_5;
                                                dupeStf02h.ACODE_6 = stf02h_induk.ACODE_6;
                                                dupeStf02h.ANAME_6 = stf02h_induk.ANAME_6;
                                                dupeStf02h.AVALUE_6 = stf02h_induk.AVALUE_6;
                                                dupeStf02h.ACODE_7 = stf02h_induk.ACODE_7;
                                                dupeStf02h.ANAME_7 = stf02h_induk.ANAME_7;
                                                dupeStf02h.AVALUE_7 = stf02h_induk.AVALUE_7;
                                                dupeStf02h.ACODE_8 = stf02h_induk.ACODE_8;
                                                dupeStf02h.ANAME_8 = stf02h_induk.ANAME_8;
                                                dupeStf02h.AVALUE_8 = stf02h_induk.AVALUE_8;
                                                dupeStf02h.ACODE_9 = stf02h_induk.ACODE_9;
                                                dupeStf02h.ANAME_9 = stf02h_induk.ANAME_9;
                                                dupeStf02h.AVALUE_9 = stf02h_induk.AVALUE_9;
                                                dupeStf02h.ACODE_10 = stf02h_induk.ACODE_10;
                                                dupeStf02h.ANAME_10 = stf02h_induk.ANAME_10;
                                                dupeStf02h.AVALUE_10 = stf02h_induk.AVALUE_10;
                                                dupeStf02h.ACODE_11 = stf02h_induk.ACODE_11;
                                                dupeStf02h.ANAME_11 = stf02h_induk.ANAME_11;
                                                dupeStf02h.AVALUE_11 = stf02h_induk.AVALUE_11;
                                                dupeStf02h.ACODE_12 = stf02h_induk.ACODE_12;
                                                dupeStf02h.ANAME_12 = stf02h_induk.ANAME_12;
                                                dupeStf02h.AVALUE_12 = stf02h_induk.AVALUE_12;
                                                dupeStf02h.ACODE_13 = stf02h_induk.ACODE_13;
                                                dupeStf02h.ANAME_13 = stf02h_induk.ANAME_13;
                                                dupeStf02h.AVALUE_13 = stf02h_induk.AVALUE_13;
                                                dupeStf02h.ACODE_14 = stf02h_induk.ACODE_14;
                                                dupeStf02h.ANAME_14 = stf02h_induk.ANAME_14;
                                                dupeStf02h.AVALUE_14 = stf02h_induk.AVALUE_14;
                                                dupeStf02h.ACODE_15 = stf02h_induk.ACODE_15;
                                                dupeStf02h.ANAME_15 = stf02h_induk.ANAME_15;
                                                dupeStf02h.AVALUE_15 = stf02h_induk.AVALUE_15;
                                                dupeStf02h.ACODE_16 = stf02h_induk.ACODE_16;
                                                dupeStf02h.ANAME_16 = stf02h_induk.ANAME_16;
                                                dupeStf02h.AVALUE_16 = stf02h_induk.AVALUE_16;
                                                dupeStf02h.ACODE_17 = stf02h_induk.ACODE_17;
                                                dupeStf02h.ANAME_17 = stf02h_induk.ANAME_17;
                                                dupeStf02h.AVALUE_17 = stf02h_induk.AVALUE_17;
                                                dupeStf02h.ACODE_18 = stf02h_induk.ACODE_18;
                                                dupeStf02h.ANAME_18 = stf02h_induk.ANAME_18;
                                                dupeStf02h.AVALUE_18 = stf02h_induk.AVALUE_18;
                                                dupeStf02h.ACODE_19 = stf02h_induk.ACODE_19;
                                                dupeStf02h.ANAME_19 = stf02h_induk.ANAME_19;
                                                dupeStf02h.AVALUE_19 = stf02h_induk.AVALUE_19;
                                                dupeStf02h.ACODE_20 = stf02h_induk.ACODE_20;
                                                dupeStf02h.ANAME_20 = stf02h_induk.ANAME_20;
                                                dupeStf02h.AVALUE_20 = stf02h_induk.AVALUE_20;
                                                dupeStf02h.ACODE_21 = stf02h_induk.ACODE_21;
                                                dupeStf02h.ANAME_21 = stf02h_induk.ANAME_21;
                                                dupeStf02h.AVALUE_21 = stf02h_induk.AVALUE_21;
                                                dupeStf02h.ACODE_22 = stf02h_induk.ACODE_22;
                                                dupeStf02h.ANAME_22 = stf02h_induk.ANAME_22;
                                                dupeStf02h.AVALUE_22 = stf02h_induk.AVALUE_22;
                                                dupeStf02h.ACODE_23 = stf02h_induk.ACODE_23;
                                                dupeStf02h.ANAME_23 = stf02h_induk.ANAME_23;
                                                dupeStf02h.AVALUE_23 = stf02h_induk.AVALUE_23;
                                                dupeStf02h.ACODE_24 = stf02h_induk.ACODE_24;
                                                dupeStf02h.ANAME_24 = stf02h_induk.ANAME_24;
                                                dupeStf02h.AVALUE_24 = stf02h_induk.AVALUE_24;
                                                dupeStf02h.ACODE_25 = stf02h_induk.ACODE_25;
                                                dupeStf02h.ANAME_25 = stf02h_induk.ANAME_25;
                                                dupeStf02h.AVALUE_25 = stf02h_induk.AVALUE_25;
                                                dupeStf02h.ACODE_26 = stf02h_induk.ACODE_26;
                                                dupeStf02h.ANAME_26 = stf02h_induk.ANAME_26;
                                                dupeStf02h.AVALUE_26 = stf02h_induk.AVALUE_26;
                                                dupeStf02h.ACODE_27 = stf02h_induk.ACODE_27;
                                                dupeStf02h.ANAME_27 = stf02h_induk.ANAME_27;
                                                dupeStf02h.AVALUE_27 = stf02h_induk.AVALUE_27;
                                                dupeStf02h.ACODE_28 = stf02h_induk.ACODE_28;
                                                dupeStf02h.ANAME_28 = stf02h_induk.ANAME_28;
                                                dupeStf02h.AVALUE_28 = stf02h_induk.AVALUE_28;
                                                dupeStf02h.ACODE_29 = stf02h_induk.ACODE_29;
                                                dupeStf02h.ANAME_29 = stf02h_induk.ANAME_29;
                                                dupeStf02h.AVALUE_29 = stf02h_induk.AVALUE_29;
                                                dupeStf02h.ACODE_30 = stf02h_induk.ACODE_30;
                                                dupeStf02h.ANAME_30 = stf02h_induk.ANAME_30;
                                                dupeStf02h.AVALUE_30 = stf02h_induk.AVALUE_30;
                                                dupeStf02h.ACODE_31 = stf02h_induk.ACODE_31;
                                                dupeStf02h.ANAME_31 = stf02h_induk.ANAME_31;
                                                dupeStf02h.AVALUE_31 = stf02h_induk.AVALUE_31;
                                                dupeStf02h.ACODE_32 = stf02h_induk.ACODE_32;
                                                dupeStf02h.ANAME_32 = stf02h_induk.ANAME_32;
                                                dupeStf02h.AVALUE_32 = stf02h_induk.AVALUE_32;
                                                dupeStf02h.ACODE_33 = stf02h_induk.ACODE_33;
                                                dupeStf02h.ANAME_33 = stf02h_induk.ANAME_33;
                                                dupeStf02h.AVALUE_33 = stf02h_induk.AVALUE_33;
                                                dupeStf02h.ACODE_34 = stf02h_induk.ACODE_34;
                                                dupeStf02h.ANAME_34 = stf02h_induk.ANAME_34;
                                                dupeStf02h.AVALUE_34 = stf02h_induk.AVALUE_34;
                                                dupeStf02h.ACODE_35 = stf02h_induk.ACODE_35;
                                                dupeStf02h.ANAME_35 = stf02h_induk.ANAME_35;
                                                dupeStf02h.AVALUE_35 = stf02h_induk.AVALUE_35;
                                                dupeStf02h.ACODE_36 = stf02h_induk.ACODE_36;
                                                dupeStf02h.ANAME_36 = stf02h_induk.ANAME_36;
                                                dupeStf02h.AVALUE_36 = stf02h_induk.AVALUE_36;
                                                dupeStf02h.ACODE_37 = stf02h_induk.ACODE_37;
                                                dupeStf02h.ANAME_37 = stf02h_induk.ANAME_37;
                                                dupeStf02h.AVALUE_37 = stf02h_induk.AVALUE_37;
                                                dupeStf02h.ACODE_38 = stf02h_induk.ACODE_38;
                                                dupeStf02h.ANAME_38 = stf02h_induk.ANAME_38;
                                                dupeStf02h.AVALUE_38 = stf02h_induk.AVALUE_38;
                                                dupeStf02h.ACODE_39 = stf02h_induk.ACODE_39;
                                                dupeStf02h.ANAME_39 = stf02h_induk.ANAME_39;
                                                dupeStf02h.AVALUE_39 = stf02h_induk.AVALUE_39;
                                                dupeStf02h.ACODE_40 = stf02h_induk.ACODE_40;
                                                dupeStf02h.ANAME_40 = stf02h_induk.ANAME_40;
                                                dupeStf02h.AVALUE_40 = stf02h_induk.AVALUE_40;
                                                dupeStf02h.ACODE_41 = stf02h_induk.ACODE_41;
                                                dupeStf02h.ANAME_41 = stf02h_induk.ANAME_41;
                                                dupeStf02h.AVALUE_41 = stf02h_induk.AVALUE_41;
                                                dupeStf02h.ACODE_42 = stf02h_induk.ACODE_42;
                                                dupeStf02h.ANAME_42 = stf02h_induk.ANAME_42;
                                                dupeStf02h.AVALUE_42 = stf02h_induk.AVALUE_42;
                                                dupeStf02h.ACODE_43 = stf02h_induk.ACODE_43;
                                                dupeStf02h.ANAME_43 = stf02h_induk.ANAME_43;
                                                dupeStf02h.AVALUE_43 = stf02h_induk.AVALUE_43;
                                                dupeStf02h.ACODE_44 = stf02h_induk.ACODE_44;
                                                dupeStf02h.ANAME_44 = stf02h_induk.ANAME_44;
                                                dupeStf02h.AVALUE_44 = stf02h_induk.AVALUE_44;
                                                dupeStf02h.ACODE_45 = stf02h_induk.ACODE_45;
                                                dupeStf02h.ANAME_45 = stf02h_induk.ANAME_45;
                                                dupeStf02h.AVALUE_45 = stf02h_induk.AVALUE_45;
                                                dupeStf02h.ACODE_46 = stf02h_induk.ACODE_46;
                                                dupeStf02h.ANAME_46 = stf02h_induk.ANAME_46;
                                                dupeStf02h.AVALUE_46 = stf02h_induk.AVALUE_46;
                                                dupeStf02h.ACODE_47 = stf02h_induk.ACODE_47;
                                                dupeStf02h.ANAME_47 = stf02h_induk.ANAME_47;
                                                dupeStf02h.AVALUE_47 = stf02h_induk.AVALUE_47;
                                                dupeStf02h.ACODE_48 = stf02h_induk.ACODE_48;
                                                dupeStf02h.ANAME_48 = stf02h_induk.ANAME_48;
                                                dupeStf02h.AVALUE_48 = stf02h_induk.AVALUE_48;
                                                dupeStf02h.ACODE_49 = stf02h_induk.ACODE_49;
                                                dupeStf02h.ANAME_49 = stf02h_induk.ANAME_49;
                                                dupeStf02h.AVALUE_49 = stf02h_induk.AVALUE_49;
                                                dupeStf02h.ACODE_50 = stf02h_induk.ACODE_50;
                                                dupeStf02h.ANAME_50 = stf02h_induk.ANAME_50;
                                                dupeStf02h.AVALUE_50 = stf02h_induk.AVALUE_50;
                                                #endregion
                                                ErasoftDbContext.STF02H.Add(dupeStf02h);
                                                if (tempBrginDB.KODE_BRG_INDUK != data.TempBrg.KODE_BRG_INDUK)//user input baru kode brg MO -> update kode brg induk pada brg varian
                                                    EDB.ExecuteSQL("CString", CommandType.Text, "UPDATE TEMP_BRG_MP SET KODE_BRG_INDUK = '" + data.TempBrg.KODE_BRG_INDUK + "' WHERE KODE_BRG_INDUK = '" + tempBrginDB.KODE_BRG_INDUK + "' AND CUST = '" + data.TempBrg.CUST + "'");
                                                ErasoftDbContext.SaveChanges();
                                            }
                                            else
                                            {
                                                return JsonErrorMessage("Kode Barang Induk tidak ditemukan.");
                                            }
                                            //end change 25 Feb 2019

                                        }

                                    }
                                }
                                else
                                {
                                    if (tempBrginDB != null)
                                    {
                                        if (tempBrgInduk != null)
                                        {
                                            //sinkron brg induk terlebih dahulu
                                            var ret2 = AutoSyncBrgInduk(data.Stf02, tempBrgInduk, data.TempBrg.KODE_BRG_INDUK, customer, username, createSTF02Induk);
                                            if (ret2.status == 0)
                                                return JsonErrorMessage(ret2.message);
                                        }
                                        else
                                        {
                                            return JsonErrorMessage("Kode Barang Induk tidak ditemukan.");
                                        }

                                    }
                                    else
                                    {
                                        return JsonErrorMessage("Barang ini sudah diproses.");
                                    }
                                }
                                //if (brgInduk == null)
                                //{
                                //    //user input kode brg induk baru, cari brg induk di temp

                                //}
                                //else 
                                //if(stf02h_induk == null)
                                //{
                                //    // brg induk sudah ada di stf02 tp blm ada di stf02h -> create stf02h saja

                                //}


                            }

                        }
                        var barangInDB = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper().Equals(data.Stf02.BRG.ToUpper())).FirstOrDefault();
                        if (barangInDB != null)
                        {
                            var brgMp = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper().Equals(data.Stf02.BRG.ToUpper()) && b.IDMARKET == data.TempBrg.IDMARKET).FirstOrDefault();
                            if (brgMp != null)
                            {
                                if (!string.IsNullOrEmpty(brgMp.BRG_MP))
                                {
                                    return JsonErrorMessage("Barang ini sudah link dengan barang lain di marketplace ini.");
                                }
                                else
                                {
                                    brgMp.HJUAL = data.Stf02.HJUAL;
                                    brgMp.DISPLAY = data.TempBrg.DISPLAY;
                                    brgMp.BRG_MP = data.TempBrg.BRG_MP;
                                    brgMp.CATEGORY_CODE = data.TempBrg.CATEGORY_CODE;
                                    brgMp.CATEGORY_NAME = data.TempBrg.CATEGORY_NAME;
                                    brgMp.DeliveryTempElevenia = data.TempBrg.DeliveryTempElevenia;
                                    brgMp.PICKUP_POINT = data.TempBrg.PICKUP_POINT;
                                    #region attribute mp
                                    brgMp.ACODE_1 = data.TempBrg.ACODE_1;
                                    brgMp.ANAME_1 = data.TempBrg.ANAME_1;
                                    brgMp.AVALUE_1 = data.TempBrg.AVALUE_1;
                                    brgMp.ACODE_2 = data.TempBrg.ACODE_2;
                                    brgMp.ANAME_2 = data.TempBrg.ANAME_2;
                                    brgMp.AVALUE_2 = data.TempBrg.AVALUE_2;
                                    brgMp.ACODE_3 = data.TempBrg.ACODE_3;
                                    brgMp.ANAME_3 = data.TempBrg.ANAME_3;
                                    brgMp.AVALUE_3 = data.TempBrg.AVALUE_3;
                                    brgMp.ACODE_4 = data.TempBrg.ACODE_4;
                                    brgMp.ANAME_4 = data.TempBrg.ANAME_4;
                                    brgMp.AVALUE_4 = data.TempBrg.AVALUE_4;
                                    brgMp.ACODE_5 = data.TempBrg.ACODE_5;
                                    brgMp.ANAME_5 = data.TempBrg.ANAME_5;
                                    brgMp.AVALUE_5 = data.TempBrg.AVALUE_5;
                                    brgMp.ACODE_6 = data.TempBrg.ACODE_6;
                                    brgMp.ANAME_6 = data.TempBrg.ANAME_6;
                                    brgMp.AVALUE_6 = data.TempBrg.AVALUE_6;
                                    brgMp.ACODE_7 = data.TempBrg.ACODE_7;
                                    brgMp.ANAME_7 = data.TempBrg.ANAME_7;
                                    brgMp.AVALUE_7 = data.TempBrg.AVALUE_7;
                                    brgMp.ACODE_8 = data.TempBrg.ACODE_8;
                                    brgMp.ANAME_8 = data.TempBrg.ANAME_8;
                                    brgMp.AVALUE_8 = data.TempBrg.AVALUE_8;
                                    brgMp.ACODE_9 = data.TempBrg.ACODE_9;
                                    brgMp.ANAME_9 = data.TempBrg.ANAME_9;
                                    brgMp.AVALUE_9 = data.TempBrg.AVALUE_9;
                                    brgMp.ACODE_10 = data.TempBrg.ACODE_10;
                                    brgMp.ANAME_10 = data.TempBrg.ANAME_10;
                                    brgMp.AVALUE_10 = data.TempBrg.AVALUE_10;
                                    brgMp.ACODE_11 = data.TempBrg.ACODE_11;
                                    brgMp.ANAME_11 = data.TempBrg.ANAME_11;
                                    brgMp.AVALUE_11 = data.TempBrg.AVALUE_11;
                                    brgMp.ACODE_12 = data.TempBrg.ACODE_12;
                                    brgMp.ANAME_12 = data.TempBrg.ANAME_12;
                                    brgMp.AVALUE_12 = data.TempBrg.AVALUE_12;
                                    brgMp.ACODE_13 = data.TempBrg.ACODE_13;
                                    brgMp.ANAME_13 = data.TempBrg.ANAME_13;
                                    brgMp.AVALUE_13 = data.TempBrg.AVALUE_13;
                                    brgMp.ACODE_14 = data.TempBrg.ACODE_14;
                                    brgMp.ANAME_14 = data.TempBrg.ANAME_14;
                                    brgMp.AVALUE_14 = data.TempBrg.AVALUE_14;
                                    brgMp.ACODE_15 = data.TempBrg.ACODE_15;
                                    brgMp.ANAME_15 = data.TempBrg.ANAME_15;
                                    brgMp.AVALUE_15 = data.TempBrg.AVALUE_15;
                                    brgMp.ACODE_16 = data.TempBrg.ACODE_16;
                                    brgMp.ANAME_16 = data.TempBrg.ANAME_16;
                                    brgMp.AVALUE_16 = data.TempBrg.AVALUE_16;
                                    brgMp.ACODE_17 = data.TempBrg.ACODE_17;
                                    brgMp.ANAME_17 = data.TempBrg.ANAME_17;
                                    brgMp.AVALUE_17 = data.TempBrg.AVALUE_17;
                                    brgMp.ACODE_18 = data.TempBrg.ACODE_18;
                                    brgMp.ANAME_18 = data.TempBrg.ANAME_18;
                                    brgMp.AVALUE_18 = data.TempBrg.AVALUE_18;
                                    brgMp.ACODE_19 = data.TempBrg.ACODE_19;
                                    brgMp.ANAME_19 = data.TempBrg.ANAME_19;
                                    brgMp.AVALUE_19 = data.TempBrg.AVALUE_19;
                                    brgMp.ACODE_20 = data.TempBrg.ACODE_20;
                                    brgMp.ANAME_20 = data.TempBrg.ANAME_20;
                                    brgMp.AVALUE_20 = data.TempBrg.AVALUE_20;
                                    brgMp.ACODE_21 = data.TempBrg.ACODE_21;
                                    brgMp.ANAME_21 = data.TempBrg.ANAME_21;
                                    brgMp.AVALUE_21 = data.TempBrg.AVALUE_21;
                                    brgMp.ACODE_22 = data.TempBrg.ACODE_22;
                                    brgMp.ANAME_22 = data.TempBrg.ANAME_22;
                                    brgMp.AVALUE_22 = data.TempBrg.AVALUE_22;
                                    brgMp.ACODE_23 = data.TempBrg.ACODE_23;
                                    brgMp.ANAME_23 = data.TempBrg.ANAME_23;
                                    brgMp.AVALUE_23 = data.TempBrg.AVALUE_23;
                                    brgMp.ACODE_24 = data.TempBrg.ACODE_24;
                                    brgMp.ANAME_24 = data.TempBrg.ANAME_24;
                                    brgMp.AVALUE_24 = data.TempBrg.AVALUE_24;
                                    brgMp.ACODE_25 = data.TempBrg.ACODE_25;
                                    brgMp.ANAME_25 = data.TempBrg.ANAME_25;
                                    brgMp.AVALUE_25 = data.TempBrg.AVALUE_25;
                                    brgMp.ACODE_26 = data.TempBrg.ACODE_26;
                                    brgMp.ANAME_26 = data.TempBrg.ANAME_26;
                                    brgMp.AVALUE_26 = data.TempBrg.AVALUE_26;
                                    brgMp.ACODE_27 = data.TempBrg.ACODE_27;
                                    brgMp.ANAME_27 = data.TempBrg.ANAME_27;
                                    brgMp.AVALUE_27 = data.TempBrg.AVALUE_27;
                                    brgMp.ACODE_28 = data.TempBrg.ACODE_28;
                                    brgMp.ANAME_28 = data.TempBrg.ANAME_28;
                                    brgMp.AVALUE_28 = data.TempBrg.AVALUE_28;
                                    brgMp.ACODE_29 = data.TempBrg.ACODE_29;
                                    brgMp.ANAME_29 = data.TempBrg.ANAME_29;
                                    brgMp.AVALUE_29 = data.TempBrg.AVALUE_29;
                                    brgMp.ACODE_30 = data.TempBrg.ACODE_30;
                                    brgMp.ANAME_30 = data.TempBrg.ANAME_30;
                                    brgMp.AVALUE_30 = data.TempBrg.AVALUE_30;
                                    brgMp.ACODE_31 = data.TempBrg.ACODE_31;
                                    brgMp.ANAME_31 = data.TempBrg.ANAME_31;
                                    brgMp.AVALUE_31 = data.TempBrg.AVALUE_31;
                                    brgMp.ACODE_32 = data.TempBrg.ACODE_32;
                                    brgMp.ANAME_32 = data.TempBrg.ANAME_32;
                                    brgMp.AVALUE_32 = data.TempBrg.AVALUE_32;
                                    brgMp.ACODE_33 = data.TempBrg.ACODE_33;
                                    brgMp.ANAME_33 = data.TempBrg.ANAME_33;
                                    brgMp.AVALUE_33 = data.TempBrg.AVALUE_33;
                                    brgMp.ACODE_34 = data.TempBrg.ACODE_34;
                                    brgMp.ANAME_34 = data.TempBrg.ANAME_34;
                                    brgMp.AVALUE_34 = data.TempBrg.AVALUE_34;
                                    brgMp.ACODE_35 = data.TempBrg.ACODE_35;
                                    brgMp.ANAME_35 = data.TempBrg.ANAME_35;
                                    brgMp.AVALUE_35 = data.TempBrg.AVALUE_35;
                                    brgMp.ACODE_36 = data.TempBrg.ACODE_36;
                                    brgMp.ANAME_36 = data.TempBrg.ANAME_36;
                                    brgMp.AVALUE_36 = data.TempBrg.AVALUE_36;
                                    brgMp.ACODE_37 = data.TempBrg.ACODE_37;
                                    brgMp.ANAME_37 = data.TempBrg.ANAME_37;
                                    brgMp.AVALUE_37 = data.TempBrg.AVALUE_37;
                                    brgMp.ACODE_38 = data.TempBrg.ACODE_38;
                                    brgMp.ANAME_38 = data.TempBrg.ANAME_38;
                                    brgMp.AVALUE_38 = data.TempBrg.AVALUE_38;
                                    brgMp.ACODE_39 = data.TempBrg.ACODE_39;
                                    brgMp.ANAME_39 = data.TempBrg.ANAME_39;
                                    brgMp.AVALUE_39 = data.TempBrg.AVALUE_39;
                                    brgMp.ACODE_40 = data.TempBrg.ACODE_40;
                                    brgMp.ANAME_40 = data.TempBrg.ANAME_40;
                                    brgMp.AVALUE_40 = data.TempBrg.AVALUE_40;
                                    brgMp.ACODE_41 = data.TempBrg.ACODE_41;
                                    brgMp.ANAME_41 = data.TempBrg.ANAME_41;
                                    brgMp.AVALUE_41 = data.TempBrg.AVALUE_41;
                                    brgMp.ACODE_42 = data.TempBrg.ACODE_42;
                                    brgMp.ANAME_42 = data.TempBrg.ANAME_42;
                                    brgMp.AVALUE_42 = data.TempBrg.AVALUE_42;
                                    brgMp.ACODE_43 = data.TempBrg.ACODE_43;
                                    brgMp.ANAME_43 = data.TempBrg.ANAME_43;
                                    brgMp.AVALUE_43 = data.TempBrg.AVALUE_43;
                                    brgMp.ACODE_44 = data.TempBrg.ACODE_44;
                                    brgMp.ANAME_44 = data.TempBrg.ANAME_44;
                                    brgMp.AVALUE_44 = data.TempBrg.AVALUE_44;
                                    brgMp.ACODE_45 = data.TempBrg.ACODE_45;
                                    brgMp.ANAME_45 = data.TempBrg.ANAME_45;
                                    brgMp.AVALUE_45 = data.TempBrg.AVALUE_45;
                                    brgMp.ACODE_46 = data.TempBrg.ACODE_46;
                                    brgMp.ANAME_46 = data.TempBrg.ANAME_46;
                                    brgMp.AVALUE_46 = data.TempBrg.AVALUE_46;
                                    brgMp.ACODE_47 = data.TempBrg.ACODE_47;
                                    brgMp.ANAME_47 = data.TempBrg.ANAME_47;
                                    brgMp.AVALUE_47 = data.TempBrg.AVALUE_47;
                                    brgMp.ACODE_48 = data.TempBrg.ACODE_48;
                                    brgMp.ANAME_48 = data.TempBrg.ANAME_48;
                                    brgMp.AVALUE_48 = data.TempBrg.AVALUE_48;
                                    brgMp.ACODE_49 = data.TempBrg.ACODE_49;
                                    brgMp.ANAME_49 = data.TempBrg.ANAME_49;
                                    brgMp.AVALUE_49 = data.TempBrg.AVALUE_49;
                                    brgMp.ACODE_50 = data.TempBrg.ACODE_50;
                                    brgMp.ANAME_50 = data.TempBrg.ANAME_50;
                                    brgMp.AVALUE_50 = data.TempBrg.AVALUE_50;
                                    #endregion
                                    ErasoftDbContext.SaveChanges();
                                }
                            }
                            else
                            {
                                brgMp = new STF02H();
                                brgMp.BRG = data.Stf02.BRG;
                                brgMp.BRG_MP = data.TempBrg.BRG_MP;
                                brgMp.HJUAL = data.Stf02.HJUAL;
                                brgMp.DISPLAY = data.TempBrg.DISPLAY;
                                brgMp.CATEGORY_CODE = data.TempBrg.CATEGORY_CODE;
                                brgMp.CATEGORY_NAME = data.TempBrg.CATEGORY_NAME;
                                brgMp.IDMARKET = data.TempBrg.IDMARKET;
                                brgMp.DeliveryTempElevenia = data.TempBrg.DeliveryTempElevenia;
                                brgMp.PICKUP_POINT = data.TempBrg.PICKUP_POINT;
                                //var customer = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper().Equals(data.TempBrg.CUST.ToUpper())).FirstOrDefault();
                                if (customer != null)
                                    brgMp.AKUNMARKET = customer.PERSO;
                                //brgMp.USERNAME = "SYSTEM_UPLOAD_BRG";
                                brgMp.USERNAME = data.Stf02.USERNAME;
                                #region attribute mp
                                brgMp.ACODE_1 = data.TempBrg.ACODE_1;
                                brgMp.ANAME_1 = data.TempBrg.ANAME_1;
                                brgMp.AVALUE_1 = data.TempBrg.AVALUE_1;
                                brgMp.ACODE_2 = data.TempBrg.ACODE_2;
                                brgMp.ANAME_2 = data.TempBrg.ANAME_2;
                                brgMp.AVALUE_2 = data.TempBrg.AVALUE_2;
                                brgMp.ACODE_3 = data.TempBrg.ACODE_3;
                                brgMp.ANAME_3 = data.TempBrg.ANAME_3;
                                brgMp.AVALUE_3 = data.TempBrg.AVALUE_3;
                                brgMp.ACODE_4 = data.TempBrg.ACODE_4;
                                brgMp.ANAME_4 = data.TempBrg.ANAME_4;
                                brgMp.AVALUE_4 = data.TempBrg.AVALUE_4;
                                brgMp.ACODE_5 = data.TempBrg.ACODE_5;
                                brgMp.ANAME_5 = data.TempBrg.ANAME_5;
                                brgMp.AVALUE_5 = data.TempBrg.AVALUE_5;
                                brgMp.ACODE_6 = data.TempBrg.ACODE_6;
                                brgMp.ANAME_6 = data.TempBrg.ANAME_6;
                                brgMp.AVALUE_6 = data.TempBrg.AVALUE_6;
                                brgMp.ACODE_7 = data.TempBrg.ACODE_7;
                                brgMp.ANAME_7 = data.TempBrg.ANAME_7;
                                brgMp.AVALUE_7 = data.TempBrg.AVALUE_7;
                                brgMp.ACODE_8 = data.TempBrg.ACODE_8;
                                brgMp.ANAME_8 = data.TempBrg.ANAME_8;
                                brgMp.AVALUE_8 = data.TempBrg.AVALUE_8;
                                brgMp.ACODE_9 = data.TempBrg.ACODE_9;
                                brgMp.ANAME_9 = data.TempBrg.ANAME_9;
                                brgMp.AVALUE_9 = data.TempBrg.AVALUE_9;
                                brgMp.ACODE_10 = data.TempBrg.ACODE_10;
                                brgMp.ANAME_10 = data.TempBrg.ANAME_10;
                                brgMp.AVALUE_10 = data.TempBrg.AVALUE_10;
                                brgMp.ACODE_11 = data.TempBrg.ACODE_11;
                                brgMp.ANAME_11 = data.TempBrg.ANAME_11;
                                brgMp.AVALUE_11 = data.TempBrg.AVALUE_11;
                                brgMp.ACODE_12 = data.TempBrg.ACODE_12;
                                brgMp.ANAME_12 = data.TempBrg.ANAME_12;
                                brgMp.AVALUE_12 = data.TempBrg.AVALUE_12;
                                brgMp.ACODE_13 = data.TempBrg.ACODE_13;
                                brgMp.ANAME_13 = data.TempBrg.ANAME_13;
                                brgMp.AVALUE_13 = data.TempBrg.AVALUE_13;
                                brgMp.ACODE_14 = data.TempBrg.ACODE_14;
                                brgMp.ANAME_14 = data.TempBrg.ANAME_14;
                                brgMp.AVALUE_14 = data.TempBrg.AVALUE_14;
                                brgMp.ACODE_15 = data.TempBrg.ACODE_15;
                                brgMp.ANAME_15 = data.TempBrg.ANAME_15;
                                brgMp.AVALUE_15 = data.TempBrg.AVALUE_15;
                                brgMp.ACODE_16 = data.TempBrg.ACODE_16;
                                brgMp.ANAME_16 = data.TempBrg.ANAME_16;
                                brgMp.AVALUE_16 = data.TempBrg.AVALUE_16;
                                brgMp.ACODE_17 = data.TempBrg.ACODE_17;
                                brgMp.ANAME_17 = data.TempBrg.ANAME_17;
                                brgMp.AVALUE_17 = data.TempBrg.AVALUE_17;
                                brgMp.ACODE_18 = data.TempBrg.ACODE_18;
                                brgMp.ANAME_18 = data.TempBrg.ANAME_18;
                                brgMp.AVALUE_18 = data.TempBrg.AVALUE_18;
                                brgMp.ACODE_19 = data.TempBrg.ACODE_19;
                                brgMp.ANAME_19 = data.TempBrg.ANAME_19;
                                brgMp.AVALUE_19 = data.TempBrg.AVALUE_19;
                                brgMp.ACODE_20 = data.TempBrg.ACODE_20;
                                brgMp.ANAME_20 = data.TempBrg.ANAME_20;
                                brgMp.AVALUE_20 = data.TempBrg.AVALUE_20;
                                brgMp.ACODE_21 = data.TempBrg.ACODE_21;
                                brgMp.ANAME_21 = data.TempBrg.ANAME_21;
                                brgMp.AVALUE_21 = data.TempBrg.AVALUE_21;
                                brgMp.ACODE_22 = data.TempBrg.ACODE_22;
                                brgMp.ANAME_22 = data.TempBrg.ANAME_22;
                                brgMp.AVALUE_22 = data.TempBrg.AVALUE_22;
                                brgMp.ACODE_23 = data.TempBrg.ACODE_23;
                                brgMp.ANAME_23 = data.TempBrg.ANAME_23;
                                brgMp.AVALUE_23 = data.TempBrg.AVALUE_23;
                                brgMp.ACODE_24 = data.TempBrg.ACODE_24;
                                brgMp.ANAME_24 = data.TempBrg.ANAME_24;
                                brgMp.AVALUE_24 = data.TempBrg.AVALUE_24;
                                brgMp.ACODE_25 = data.TempBrg.ACODE_25;
                                brgMp.ANAME_25 = data.TempBrg.ANAME_25;
                                brgMp.AVALUE_25 = data.TempBrg.AVALUE_25;
                                brgMp.ACODE_26 = data.TempBrg.ACODE_26;
                                brgMp.ANAME_26 = data.TempBrg.ANAME_26;
                                brgMp.AVALUE_26 = data.TempBrg.AVALUE_26;
                                brgMp.ACODE_27 = data.TempBrg.ACODE_27;
                                brgMp.ANAME_27 = data.TempBrg.ANAME_27;
                                brgMp.AVALUE_27 = data.TempBrg.AVALUE_27;
                                brgMp.ACODE_28 = data.TempBrg.ACODE_28;
                                brgMp.ANAME_28 = data.TempBrg.ANAME_28;
                                brgMp.AVALUE_28 = data.TempBrg.AVALUE_28;
                                brgMp.ACODE_29 = data.TempBrg.ACODE_29;
                                brgMp.ANAME_29 = data.TempBrg.ANAME_29;
                                brgMp.AVALUE_29 = data.TempBrg.AVALUE_29;
                                brgMp.ACODE_30 = data.TempBrg.ACODE_30;
                                brgMp.ANAME_30 = data.TempBrg.ANAME_30;
                                brgMp.AVALUE_30 = data.TempBrg.AVALUE_30;
                                brgMp.ACODE_31 = data.TempBrg.ACODE_31;
                                brgMp.ANAME_31 = data.TempBrg.ANAME_31;
                                brgMp.AVALUE_31 = data.TempBrg.AVALUE_31;
                                brgMp.ACODE_32 = data.TempBrg.ACODE_32;
                                brgMp.ANAME_32 = data.TempBrg.ANAME_32;
                                brgMp.AVALUE_32 = data.TempBrg.AVALUE_32;
                                brgMp.ACODE_33 = data.TempBrg.ACODE_33;
                                brgMp.ANAME_33 = data.TempBrg.ANAME_33;
                                brgMp.AVALUE_33 = data.TempBrg.AVALUE_33;
                                brgMp.ACODE_34 = data.TempBrg.ACODE_34;
                                brgMp.ANAME_34 = data.TempBrg.ANAME_34;
                                brgMp.AVALUE_34 = data.TempBrg.AVALUE_34;
                                brgMp.ACODE_35 = data.TempBrg.ACODE_35;
                                brgMp.ANAME_35 = data.TempBrg.ANAME_35;
                                brgMp.AVALUE_35 = data.TempBrg.AVALUE_35;
                                brgMp.ACODE_36 = data.TempBrg.ACODE_36;
                                brgMp.ANAME_36 = data.TempBrg.ANAME_36;
                                brgMp.AVALUE_36 = data.TempBrg.AVALUE_36;
                                brgMp.ACODE_37 = data.TempBrg.ACODE_37;
                                brgMp.ANAME_37 = data.TempBrg.ANAME_37;
                                brgMp.AVALUE_37 = data.TempBrg.AVALUE_37;
                                brgMp.ACODE_38 = data.TempBrg.ACODE_38;
                                brgMp.ANAME_38 = data.TempBrg.ANAME_38;
                                brgMp.AVALUE_38 = data.TempBrg.AVALUE_38;
                                brgMp.ACODE_39 = data.TempBrg.ACODE_39;
                                brgMp.ANAME_39 = data.TempBrg.ANAME_39;
                                brgMp.AVALUE_39 = data.TempBrg.AVALUE_39;
                                brgMp.ACODE_40 = data.TempBrg.ACODE_40;
                                brgMp.ANAME_40 = data.TempBrg.ANAME_40;
                                brgMp.AVALUE_40 = data.TempBrg.AVALUE_40;
                                brgMp.ACODE_41 = data.TempBrg.ACODE_41;
                                brgMp.ANAME_41 = data.TempBrg.ANAME_41;
                                brgMp.AVALUE_41 = data.TempBrg.AVALUE_41;
                                brgMp.ACODE_42 = data.TempBrg.ACODE_42;
                                brgMp.ANAME_42 = data.TempBrg.ANAME_42;
                                brgMp.AVALUE_42 = data.TempBrg.AVALUE_42;
                                brgMp.ACODE_43 = data.TempBrg.ACODE_43;
                                brgMp.ANAME_43 = data.TempBrg.ANAME_43;
                                brgMp.AVALUE_43 = data.TempBrg.AVALUE_43;
                                brgMp.ACODE_44 = data.TempBrg.ACODE_44;
                                brgMp.ANAME_44 = data.TempBrg.ANAME_44;
                                brgMp.AVALUE_44 = data.TempBrg.AVALUE_44;
                                brgMp.ACODE_45 = data.TempBrg.ACODE_45;
                                brgMp.ANAME_45 = data.TempBrg.ANAME_45;
                                brgMp.AVALUE_45 = data.TempBrg.AVALUE_45;
                                brgMp.ACODE_46 = data.TempBrg.ACODE_46;
                                brgMp.ANAME_46 = data.TempBrg.ANAME_46;
                                brgMp.AVALUE_46 = data.TempBrg.AVALUE_46;
                                brgMp.ACODE_47 = data.TempBrg.ACODE_47;
                                brgMp.ANAME_47 = data.TempBrg.ANAME_47;
                                brgMp.AVALUE_47 = data.TempBrg.AVALUE_47;
                                brgMp.ACODE_48 = data.TempBrg.ACODE_48;
                                brgMp.ANAME_48 = data.TempBrg.ANAME_48;
                                brgMp.AVALUE_48 = data.TempBrg.AVALUE_48;
                                brgMp.ACODE_49 = data.TempBrg.ACODE_49;
                                brgMp.ANAME_49 = data.TempBrg.ANAME_49;
                                brgMp.AVALUE_49 = data.TempBrg.AVALUE_49;
                                brgMp.ACODE_50 = data.TempBrg.ACODE_50;
                                brgMp.ANAME_50 = data.TempBrg.ANAME_50;
                                brgMp.AVALUE_50 = data.TempBrg.AVALUE_50;
                                #endregion
                                ErasoftDbContext.STF02H.Add(brgMp);
                                ErasoftDbContext.SaveChanges();

                            }
                            if (barangInDB.TYPE == "4")
                            {
                                if (tempBrginDB.SELLER_SKU != data.Stf02.BRG)//user input baru kode brg MO -> update kode brg induk pada brg varian
                                    EDB.ExecuteSQL("CString", CommandType.Text, "UPDATE TEMP_BRG_MP SET KODE_BRG_INDUK = '" + data.Stf02.BRG + "' WHERE KODE_BRG_INDUK = '" + tempBrginDB.SELLER_SKU + "' AND CUST = '" + data.TempBrg.CUST + "'");
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(data.TempBrg.IMAGE))
                            {
                                data.Stf02.LINK_GAMBAR_1 = UploadImageService.UploadSingleImageToImgurFromUrl(data.TempBrg.IMAGE, "uploaded-image").data.link_l;
                            }
                            if (!string.IsNullOrEmpty(data.TempBrg.IMAGE2))
                            {
                                data.Stf02.LINK_GAMBAR_2 = UploadImageService.UploadSingleImageToImgurFromUrl(data.TempBrg.IMAGE2, "uploaded-image").data.link_l;
                            }
                            if (!string.IsNullOrEmpty(data.TempBrg.IMAGE3))
                            {
                                data.Stf02.LINK_GAMBAR_3 = UploadImageService.UploadSingleImageToImgurFromUrl(data.TempBrg.IMAGE3, "uploaded-image").data.link_l;
                            }
                            if (!string.IsNullOrEmpty(data.TempBrg.KODE_BRG_INDUK))
                                data.Stf02.PART = data.TempBrg.KODE_BRG_INDUK;
                            //change by Tri 11 Feb 2019, handle brg tokped
                            //data.Stf02.TYPE = data.TempBrg.TYPE;
                            if (data.haveVarian == 0)// barang tanpa varian
                            {
                                data.Stf02.TYPE = "3";
                            }
                            else if (!string.IsNullOrEmpty(data.TempBrg.KODE_BRG_INDUK))// barang varian
                            {
                                data.Stf02.TYPE = "3";
                            }
                            else//barang induk
                            {
                                data.Stf02.TYPE = "4";
                                if (tempBrginDB.SELLER_SKU != data.Stf02.BRG)//user input baru kode brg MO -> update kode brg induk pada brg varian
                                    EDB.ExecuteSQL("CString", CommandType.Text, "UPDATE TEMP_BRG_MP SET KODE_BRG_INDUK = '" + data.Stf02.BRG + "' WHERE KODE_BRG_INDUK = '" + tempBrginDB.SELLER_SKU + "' AND CUST = '" + data.TempBrg.CUST + "'");
                            }
                            //end change by Tri 11 Feb 2019, handle brg tokped

                            ErasoftDbContext.STF02.Add(data.Stf02);

                            var brgMp = new STF02H();
                            brgMp.BRG = data.Stf02.BRG;
                            brgMp.BRG_MP = data.TempBrg.BRG_MP;
                            brgMp.HJUAL = data.Stf02.HJUAL;
                            brgMp.DISPLAY = data.TempBrg.DISPLAY;
                            brgMp.CATEGORY_CODE = data.TempBrg.CATEGORY_CODE;
                            brgMp.CATEGORY_NAME = data.TempBrg.CATEGORY_NAME;
                            brgMp.IDMARKET = data.TempBrg.IDMARKET;
                            brgMp.DeliveryTempElevenia = data.TempBrg.DeliveryTempElevenia;
                            brgMp.PICKUP_POINT = data.TempBrg.PICKUP_POINT;
                            //var customer = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper().Equals(data.TempBrg.CUST.ToUpper())).FirstOrDefault();
                            if (customer != null)
                                brgMp.AKUNMARKET = customer.PERSO;
                            //brgMp.USERNAME = "SYSTEM_UPLOAD_BRG";
                            brgMp.USERNAME = data.Stf02.USERNAME;
                            #region attribute mp
                            brgMp.ACODE_1 = data.TempBrg.ACODE_1;
                            brgMp.ANAME_1 = data.TempBrg.ANAME_1;
                            brgMp.AVALUE_1 = data.TempBrg.AVALUE_1;
                            brgMp.ACODE_2 = data.TempBrg.ACODE_2;
                            brgMp.ANAME_2 = data.TempBrg.ANAME_2;
                            brgMp.AVALUE_2 = data.TempBrg.AVALUE_2;
                            brgMp.ACODE_3 = data.TempBrg.ACODE_3;
                            brgMp.ANAME_3 = data.TempBrg.ANAME_3;
                            brgMp.AVALUE_3 = data.TempBrg.AVALUE_3;
                            brgMp.ACODE_4 = data.TempBrg.ACODE_4;
                            brgMp.ANAME_4 = data.TempBrg.ANAME_4;
                            brgMp.AVALUE_4 = data.TempBrg.AVALUE_4;
                            brgMp.ACODE_5 = data.TempBrg.ACODE_5;
                            brgMp.ANAME_5 = data.TempBrg.ANAME_5;
                            brgMp.AVALUE_5 = data.TempBrg.AVALUE_5;
                            brgMp.ACODE_6 = data.TempBrg.ACODE_6;
                            brgMp.ANAME_6 = data.TempBrg.ANAME_6;
                            brgMp.AVALUE_6 = data.TempBrg.AVALUE_6;
                            brgMp.ACODE_7 = data.TempBrg.ACODE_7;
                            brgMp.ANAME_7 = data.TempBrg.ANAME_7;
                            brgMp.AVALUE_7 = data.TempBrg.AVALUE_7;
                            brgMp.ACODE_8 = data.TempBrg.ACODE_8;
                            brgMp.ANAME_8 = data.TempBrg.ANAME_8;
                            brgMp.AVALUE_8 = data.TempBrg.AVALUE_8;
                            brgMp.ACODE_9 = data.TempBrg.ACODE_9;
                            brgMp.ANAME_9 = data.TempBrg.ANAME_9;
                            brgMp.AVALUE_9 = data.TempBrg.AVALUE_9;
                            brgMp.ACODE_10 = data.TempBrg.ACODE_10;
                            brgMp.ANAME_10 = data.TempBrg.ANAME_10;
                            brgMp.AVALUE_10 = data.TempBrg.AVALUE_10;
                            brgMp.ACODE_11 = data.TempBrg.ACODE_11;
                            brgMp.ANAME_11 = data.TempBrg.ANAME_11;
                            brgMp.AVALUE_11 = data.TempBrg.AVALUE_11;
                            brgMp.ACODE_12 = data.TempBrg.ACODE_12;
                            brgMp.ANAME_12 = data.TempBrg.ANAME_12;
                            brgMp.AVALUE_12 = data.TempBrg.AVALUE_12;
                            brgMp.ACODE_13 = data.TempBrg.ACODE_13;
                            brgMp.ANAME_13 = data.TempBrg.ANAME_13;
                            brgMp.AVALUE_13 = data.TempBrg.AVALUE_13;
                            brgMp.ACODE_14 = data.TempBrg.ACODE_14;
                            brgMp.ANAME_14 = data.TempBrg.ANAME_14;
                            brgMp.AVALUE_14 = data.TempBrg.AVALUE_14;
                            brgMp.ACODE_15 = data.TempBrg.ACODE_15;
                            brgMp.ANAME_15 = data.TempBrg.ANAME_15;
                            brgMp.AVALUE_15 = data.TempBrg.AVALUE_15;
                            brgMp.ACODE_16 = data.TempBrg.ACODE_16;
                            brgMp.ANAME_16 = data.TempBrg.ANAME_16;
                            brgMp.AVALUE_16 = data.TempBrg.AVALUE_16;
                            brgMp.ACODE_17 = data.TempBrg.ACODE_17;
                            brgMp.ANAME_17 = data.TempBrg.ANAME_17;
                            brgMp.AVALUE_17 = data.TempBrg.AVALUE_17;
                            brgMp.ACODE_18 = data.TempBrg.ACODE_18;
                            brgMp.ANAME_18 = data.TempBrg.ANAME_18;
                            brgMp.AVALUE_18 = data.TempBrg.AVALUE_18;
                            brgMp.ACODE_19 = data.TempBrg.ACODE_19;
                            brgMp.ANAME_19 = data.TempBrg.ANAME_19;
                            brgMp.AVALUE_19 = data.TempBrg.AVALUE_19;
                            brgMp.ACODE_20 = data.TempBrg.ACODE_20;
                            brgMp.ANAME_20 = data.TempBrg.ANAME_20;
                            brgMp.AVALUE_20 = data.TempBrg.AVALUE_20;
                            brgMp.ACODE_21 = data.TempBrg.ACODE_21;
                            brgMp.ANAME_21 = data.TempBrg.ANAME_21;
                            brgMp.AVALUE_21 = data.TempBrg.AVALUE_21;
                            brgMp.ACODE_22 = data.TempBrg.ACODE_22;
                            brgMp.ANAME_22 = data.TempBrg.ANAME_22;
                            brgMp.AVALUE_22 = data.TempBrg.AVALUE_22;
                            brgMp.ACODE_23 = data.TempBrg.ACODE_23;
                            brgMp.ANAME_23 = data.TempBrg.ANAME_23;
                            brgMp.AVALUE_23 = data.TempBrg.AVALUE_23;
                            brgMp.ACODE_24 = data.TempBrg.ACODE_24;
                            brgMp.ANAME_24 = data.TempBrg.ANAME_24;
                            brgMp.AVALUE_24 = data.TempBrg.AVALUE_24;
                            brgMp.ACODE_25 = data.TempBrg.ACODE_25;
                            brgMp.ANAME_25 = data.TempBrg.ANAME_25;
                            brgMp.AVALUE_25 = data.TempBrg.AVALUE_25;
                            brgMp.ACODE_26 = data.TempBrg.ACODE_26;
                            brgMp.ANAME_26 = data.TempBrg.ANAME_26;
                            brgMp.AVALUE_26 = data.TempBrg.AVALUE_26;
                            brgMp.ACODE_27 = data.TempBrg.ACODE_27;
                            brgMp.ANAME_27 = data.TempBrg.ANAME_27;
                            brgMp.AVALUE_27 = data.TempBrg.AVALUE_27;
                            brgMp.ACODE_28 = data.TempBrg.ACODE_28;
                            brgMp.ANAME_28 = data.TempBrg.ANAME_28;
                            brgMp.AVALUE_28 = data.TempBrg.AVALUE_28;
                            brgMp.ACODE_29 = data.TempBrg.ACODE_29;
                            brgMp.ANAME_29 = data.TempBrg.ANAME_29;
                            brgMp.AVALUE_29 = data.TempBrg.AVALUE_29;
                            brgMp.ACODE_30 = data.TempBrg.ACODE_30;
                            brgMp.ANAME_30 = data.TempBrg.ANAME_30;
                            brgMp.AVALUE_30 = data.TempBrg.AVALUE_30;
                            brgMp.ACODE_31 = data.TempBrg.ACODE_31;
                            brgMp.ANAME_31 = data.TempBrg.ANAME_31;
                            brgMp.AVALUE_31 = data.TempBrg.AVALUE_31;
                            brgMp.ACODE_32 = data.TempBrg.ACODE_32;
                            brgMp.ANAME_32 = data.TempBrg.ANAME_32;
                            brgMp.AVALUE_32 = data.TempBrg.AVALUE_32;
                            brgMp.ACODE_33 = data.TempBrg.ACODE_33;
                            brgMp.ANAME_33 = data.TempBrg.ANAME_33;
                            brgMp.AVALUE_33 = data.TempBrg.AVALUE_33;
                            brgMp.ACODE_34 = data.TempBrg.ACODE_34;
                            brgMp.ANAME_34 = data.TempBrg.ANAME_34;
                            brgMp.AVALUE_34 = data.TempBrg.AVALUE_34;
                            brgMp.ACODE_35 = data.TempBrg.ACODE_35;
                            brgMp.ANAME_35 = data.TempBrg.ANAME_35;
                            brgMp.AVALUE_35 = data.TempBrg.AVALUE_35;
                            brgMp.ACODE_36 = data.TempBrg.ACODE_36;
                            brgMp.ANAME_36 = data.TempBrg.ANAME_36;
                            brgMp.AVALUE_36 = data.TempBrg.AVALUE_36;
                            brgMp.ACODE_37 = data.TempBrg.ACODE_37;
                            brgMp.ANAME_37 = data.TempBrg.ANAME_37;
                            brgMp.AVALUE_37 = data.TempBrg.AVALUE_37;
                            brgMp.ACODE_38 = data.TempBrg.ACODE_38;
                            brgMp.ANAME_38 = data.TempBrg.ANAME_38;
                            brgMp.AVALUE_38 = data.TempBrg.AVALUE_38;
                            brgMp.ACODE_39 = data.TempBrg.ACODE_39;
                            brgMp.ANAME_39 = data.TempBrg.ANAME_39;
                            brgMp.AVALUE_39 = data.TempBrg.AVALUE_39;
                            brgMp.ACODE_40 = data.TempBrg.ACODE_40;
                            brgMp.ANAME_40 = data.TempBrg.ANAME_40;
                            brgMp.AVALUE_40 = data.TempBrg.AVALUE_40;
                            brgMp.ACODE_41 = data.TempBrg.ACODE_41;
                            brgMp.ANAME_41 = data.TempBrg.ANAME_41;
                            brgMp.AVALUE_41 = data.TempBrg.AVALUE_41;
                            brgMp.ACODE_42 = data.TempBrg.ACODE_42;
                            brgMp.ANAME_42 = data.TempBrg.ANAME_42;
                            brgMp.AVALUE_42 = data.TempBrg.AVALUE_42;
                            brgMp.ACODE_43 = data.TempBrg.ACODE_43;
                            brgMp.ANAME_43 = data.TempBrg.ANAME_43;
                            brgMp.AVALUE_43 = data.TempBrg.AVALUE_43;
                            brgMp.ACODE_44 = data.TempBrg.ACODE_44;
                            brgMp.ANAME_44 = data.TempBrg.ANAME_44;
                            brgMp.AVALUE_44 = data.TempBrg.AVALUE_44;
                            brgMp.ACODE_45 = data.TempBrg.ACODE_45;
                            brgMp.ANAME_45 = data.TempBrg.ANAME_45;
                            brgMp.AVALUE_45 = data.TempBrg.AVALUE_45;
                            brgMp.ACODE_46 = data.TempBrg.ACODE_46;
                            brgMp.ANAME_46 = data.TempBrg.ANAME_46;
                            brgMp.AVALUE_46 = data.TempBrg.AVALUE_46;
                            brgMp.ACODE_47 = data.TempBrg.ACODE_47;
                            brgMp.ANAME_47 = data.TempBrg.ANAME_47;
                            brgMp.AVALUE_47 = data.TempBrg.AVALUE_47;
                            brgMp.ACODE_48 = data.TempBrg.ACODE_48;
                            brgMp.ANAME_48 = data.TempBrg.ANAME_48;
                            brgMp.AVALUE_48 = data.TempBrg.AVALUE_48;
                            brgMp.ACODE_49 = data.TempBrg.ACODE_49;
                            brgMp.ANAME_49 = data.TempBrg.ANAME_49;
                            brgMp.AVALUE_49 = data.TempBrg.AVALUE_49;
                            brgMp.ACODE_50 = data.TempBrg.ACODE_50;
                            brgMp.ANAME_50 = data.TempBrg.ANAME_50;
                            brgMp.AVALUE_50 = data.TempBrg.AVALUE_50;
                            #endregion
                            ErasoftDbContext.STF02H.Add(brgMp);
                            ErasoftDbContext.SaveChanges();
                        }
                    }
                    else
                    {
                        return JsonErrorMessage("Barang tidak ditemukan");
                    }
                }
                else
                {
                    return JsonErrorMessage("Barang tidak ditemukan");
                }

            }
            else
            {
                return JsonErrorMessage("Barang tidak ditemukan");
            }

            ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.Equals(data.TempBrg.BRG_MP)).Delete();
            ErasoftDbContext.SaveChanges();

            return Json("", JsonRequestBehavior.AllowGet);

        }

        public BindingBase AutoSyncBrgInduk(STF02 data, TEMP_BRG_MP tempBrg, string kdBrgMO, ARF01 customer, string username, bool createSTF02Induk)
        {
            var ret = new BindingBase()
            {
                status = 0
            };

            try
            {
                var defaultCategoryCode = ErasoftDbContext.STF02E.Where(c => c.LEVEL.Equals("1")).FirstOrDefault();
                if (defaultCategoryCode == null)
                {
                    ret.message = "Kode Kategori tidak ditemukan";
                    return ret;
                }
                var defaultBrand = ErasoftDbContext.STF02E.Where(c => c.LEVEL.Equals("2")).FirstOrDefault();
                if (defaultBrand == null)
                {
                    ret.message = "Kode Merek tidak ditemukan";
                    return ret;
                }
                if (createSTF02Induk)
                {
                    var stf02 = new STF02
                    {
                        HPP = 0,
                        HBELI = 0,
                        HBESAR = 0,
                        HKECIL = 0,
                        TYPE = "4",//type 4 = brg jasa
                        KLINK = "1",
                        HP_STD = 0,
                        QPROD = 0,
                        ISI3 = 3,
                        ISI4 = 1,
                        TOLERANSI = 0,
                        H_STN_3 = 0,
                        H_STN_4 = 0,
                        SS = 0,
                        METODA_HPP_PER_SN = false,
                        HNA_PPN = 0,
                        LABA = 0,
                        DEFAULT_STN_HRG_JUAL = 0,
                        DEFAULT_STN_JUAL = 0,
                        ISI = 1,
                        Metoda = "1",
                        Tgl_Input = DateTime.Now,
                        TGL_KLR = DateTime.Now,
                        MAXI = 100,
                        MINI = 1,
                        QSALES = 0,
                        DISPLAY_MARKET = false,
                    };
                    stf02.BRG = kdBrgMO;
                    stf02.NAMA = tempBrg.NAMA;
                    stf02.NAMA2 = tempBrg.NAMA2;
                    stf02.NAMA3 = tempBrg.NAMA3;
                    stf02.HJUAL = tempBrg.HJUAL;
                    stf02.STN = "pcs";
                    stf02.STN2 = "pcs";
                    stf02.BERAT = tempBrg.BERAT;
                    stf02.TINGGI = tempBrg.TINGGI;
                    stf02.LEBAR = tempBrg.LEBAR;
                    stf02.PANJANG = tempBrg.PANJANG;
                    stf02.Sort1 = string.IsNullOrEmpty(data.Sort1) ? defaultCategoryCode.KODE : data.Sort1;
                    stf02.Sort2 = string.IsNullOrEmpty(data.Sort2) ? defaultBrand.KODE : data.Sort2;
                    stf02.KET_SORT1 = string.IsNullOrEmpty(data.KET_SORT1) ? defaultCategoryCode.KET : data.KET_SORT1;
                    stf02.KET_SORT2 = string.IsNullOrEmpty(data.KET_SORT2) ? defaultBrand.KET : data.KET_SORT2;
                    stf02.Deskripsi = (string.IsNullOrEmpty(tempBrg.Deskripsi) ? "-" : tempBrg.Deskripsi);

                    if (!string.IsNullOrEmpty(tempBrg.IMAGE))
                    {
                        stf02.LINK_GAMBAR_1 = UploadImageService.UploadSingleImageToImgurFromUrl(tempBrg.IMAGE, "uploaded-image").data.link_l;
                    }
                    if (!string.IsNullOrEmpty(tempBrg.IMAGE2))
                    {
                        stf02.LINK_GAMBAR_2 = UploadImageService.UploadSingleImageToImgurFromUrl(tempBrg.IMAGE2, "uploaded-image").data.link_l;
                    }
                    if (!string.IsNullOrEmpty(tempBrg.IMAGE3))
                    {
                        stf02.LINK_GAMBAR_3 = UploadImageService.UploadSingleImageToImgurFromUrl(tempBrg.IMAGE3, "uploaded-image").data.link_l;
                    }

                    ErasoftDbContext.STF02.Add(stf02);

                }
                bool insertSTF02h = false;
                var brgMp = ErasoftDbContext.STF02H.Where(p => p.BRG == kdBrgMO && p.IDMARKET == tempBrg.IDMARKET).FirstOrDefault();
                if (brgMp == null)
                {
                    brgMp = new STF02H();
                    insertSTF02h = true;
                }

                //brgMp.BRG = tempBrg.BRG_MP;
                brgMp.BRG = kdBrgMO;
                brgMp.BRG_MP = tempBrg.BRG_MP;
                brgMp.HJUAL = tempBrg.HJUAL;
                brgMp.DISPLAY = tempBrg.DISPLAY;
                brgMp.CATEGORY_CODE = tempBrg.CATEGORY_CODE;
                brgMp.CATEGORY_NAME = tempBrg.CATEGORY_NAME;
                brgMp.IDMARKET = tempBrg.IDMARKET;
                brgMp.DeliveryTempElevenia = tempBrg.DeliveryTempElevenia;
                brgMp.PICKUP_POINT = tempBrg.PICKUP_POINT;
                //var customer = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper().Equals(data.TempBrg.CUST.ToUpper())).FirstOrDefault();
                //if (customer != null)
                brgMp.AKUNMARKET = customer.PERSO;
                //brgMp.USERNAME = "SYSTEM_UPLOAD_BRG";
                brgMp.USERNAME = username;
                #region attribute mp
                brgMp.ACODE_1 = tempBrg.ACODE_1;
                brgMp.ANAME_1 = tempBrg.ANAME_1;
                brgMp.AVALUE_1 = tempBrg.AVALUE_1;
                brgMp.ACODE_2 = tempBrg.ACODE_2;
                brgMp.ANAME_2 = tempBrg.ANAME_2;
                brgMp.AVALUE_2 = tempBrg.AVALUE_2;
                brgMp.ACODE_3 = tempBrg.ACODE_3;
                brgMp.ANAME_3 = tempBrg.ANAME_3;
                brgMp.AVALUE_3 = tempBrg.AVALUE_3;
                brgMp.ACODE_4 = tempBrg.ACODE_4;
                brgMp.ANAME_4 = tempBrg.ANAME_4;
                brgMp.AVALUE_4 = tempBrg.AVALUE_4;
                brgMp.ACODE_5 = tempBrg.ACODE_5;
                brgMp.ANAME_5 = tempBrg.ANAME_5;
                brgMp.AVALUE_5 = tempBrg.AVALUE_5;
                brgMp.ACODE_6 = tempBrg.ACODE_6;
                brgMp.ANAME_6 = tempBrg.ANAME_6;
                brgMp.AVALUE_6 = tempBrg.AVALUE_6;
                brgMp.ACODE_7 = tempBrg.ACODE_7;
                brgMp.ANAME_7 = tempBrg.ANAME_7;
                brgMp.AVALUE_7 = tempBrg.AVALUE_7;
                brgMp.ACODE_8 = tempBrg.ACODE_8;
                brgMp.ANAME_8 = tempBrg.ANAME_8;
                brgMp.AVALUE_8 = tempBrg.AVALUE_8;
                brgMp.ACODE_9 = tempBrg.ACODE_9;
                brgMp.ANAME_9 = tempBrg.ANAME_9;
                brgMp.AVALUE_9 = tempBrg.AVALUE_9;
                brgMp.ACODE_10 = tempBrg.ACODE_10;
                brgMp.ANAME_10 = tempBrg.ANAME_10;
                brgMp.AVALUE_10 = tempBrg.AVALUE_10;
                brgMp.ACODE_11 = tempBrg.ACODE_11;
                brgMp.ANAME_11 = tempBrg.ANAME_11;
                brgMp.AVALUE_11 = tempBrg.AVALUE_11;
                brgMp.ACODE_12 = tempBrg.ACODE_12;
                brgMp.ANAME_12 = tempBrg.ANAME_12;
                brgMp.AVALUE_12 = tempBrg.AVALUE_12;
                brgMp.ACODE_13 = tempBrg.ACODE_13;
                brgMp.ANAME_13 = tempBrg.ANAME_13;
                brgMp.AVALUE_13 = tempBrg.AVALUE_13;
                brgMp.ACODE_14 = tempBrg.ACODE_14;
                brgMp.ANAME_14 = tempBrg.ANAME_14;
                brgMp.AVALUE_14 = tempBrg.AVALUE_14;
                brgMp.ACODE_15 = tempBrg.ACODE_15;
                brgMp.ANAME_15 = tempBrg.ANAME_15;
                brgMp.AVALUE_15 = tempBrg.AVALUE_15;
                brgMp.ACODE_16 = tempBrg.ACODE_16;
                brgMp.ANAME_16 = tempBrg.ANAME_16;
                brgMp.AVALUE_16 = tempBrg.AVALUE_16;
                brgMp.ACODE_17 = tempBrg.ACODE_17;
                brgMp.ANAME_17 = tempBrg.ANAME_17;
                brgMp.AVALUE_17 = tempBrg.AVALUE_17;
                brgMp.ACODE_18 = tempBrg.ACODE_18;
                brgMp.ANAME_18 = tempBrg.ANAME_18;
                brgMp.AVALUE_18 = tempBrg.AVALUE_18;
                brgMp.ACODE_19 = tempBrg.ACODE_19;
                brgMp.ANAME_19 = tempBrg.ANAME_19;
                brgMp.AVALUE_19 = tempBrg.AVALUE_19;
                brgMp.ACODE_20 = tempBrg.ACODE_20;
                brgMp.ANAME_20 = tempBrg.ANAME_20;
                brgMp.AVALUE_20 = tempBrg.AVALUE_20;
                brgMp.ACODE_21 = tempBrg.ACODE_21;
                brgMp.ANAME_21 = tempBrg.ANAME_21;
                brgMp.AVALUE_21 = tempBrg.AVALUE_21;
                brgMp.ACODE_22 = tempBrg.ACODE_22;
                brgMp.ANAME_22 = tempBrg.ANAME_22;
                brgMp.AVALUE_22 = tempBrg.AVALUE_22;
                brgMp.ACODE_23 = tempBrg.ACODE_23;
                brgMp.ANAME_23 = tempBrg.ANAME_23;
                brgMp.AVALUE_23 = tempBrg.AVALUE_23;
                brgMp.ACODE_24 = tempBrg.ACODE_24;
                brgMp.ANAME_24 = tempBrg.ANAME_24;
                brgMp.AVALUE_24 = tempBrg.AVALUE_24;
                brgMp.ACODE_25 = tempBrg.ACODE_25;
                brgMp.ANAME_25 = tempBrg.ANAME_25;
                brgMp.AVALUE_25 = tempBrg.AVALUE_25;
                brgMp.ACODE_26 = tempBrg.ACODE_26;
                brgMp.ANAME_26 = tempBrg.ANAME_26;
                brgMp.AVALUE_26 = tempBrg.AVALUE_26;
                brgMp.ACODE_27 = tempBrg.ACODE_27;
                brgMp.ANAME_27 = tempBrg.ANAME_27;
                brgMp.AVALUE_27 = tempBrg.AVALUE_27;
                brgMp.ACODE_28 = tempBrg.ACODE_28;
                brgMp.ANAME_28 = tempBrg.ANAME_28;
                brgMp.AVALUE_28 = tempBrg.AVALUE_28;
                brgMp.ACODE_29 = tempBrg.ACODE_29;
                brgMp.ANAME_29 = tempBrg.ANAME_29;
                brgMp.AVALUE_29 = tempBrg.AVALUE_29;
                brgMp.ACODE_30 = tempBrg.ACODE_30;
                brgMp.ANAME_30 = tempBrg.ANAME_30;
                brgMp.AVALUE_30 = tempBrg.AVALUE_30;
                brgMp.ACODE_31 = tempBrg.ACODE_31;
                brgMp.ANAME_31 = tempBrg.ANAME_31;
                brgMp.AVALUE_31 = tempBrg.AVALUE_31;
                brgMp.ACODE_32 = tempBrg.ACODE_32;
                brgMp.ANAME_32 = tempBrg.ANAME_32;
                brgMp.AVALUE_32 = tempBrg.AVALUE_32;
                brgMp.ACODE_33 = tempBrg.ACODE_33;
                brgMp.ANAME_33 = tempBrg.ANAME_33;
                brgMp.AVALUE_33 = tempBrg.AVALUE_33;
                brgMp.ACODE_34 = tempBrg.ACODE_34;
                brgMp.ANAME_34 = tempBrg.ANAME_34;
                brgMp.AVALUE_34 = tempBrg.AVALUE_34;
                brgMp.ACODE_35 = tempBrg.ACODE_35;
                brgMp.ANAME_35 = tempBrg.ANAME_35;
                brgMp.AVALUE_35 = tempBrg.AVALUE_35;
                brgMp.ACODE_36 = tempBrg.ACODE_36;
                brgMp.ANAME_36 = tempBrg.ANAME_36;
                brgMp.AVALUE_36 = tempBrg.AVALUE_36;
                brgMp.ACODE_37 = tempBrg.ACODE_37;
                brgMp.ANAME_37 = tempBrg.ANAME_37;
                brgMp.AVALUE_37 = tempBrg.AVALUE_37;
                brgMp.ACODE_38 = tempBrg.ACODE_38;
                brgMp.ANAME_38 = tempBrg.ANAME_38;
                brgMp.AVALUE_38 = tempBrg.AVALUE_38;
                brgMp.ACODE_39 = tempBrg.ACODE_39;
                brgMp.ANAME_39 = tempBrg.ANAME_39;
                brgMp.AVALUE_39 = tempBrg.AVALUE_39;
                brgMp.ACODE_40 = tempBrg.ACODE_40;
                brgMp.ANAME_40 = tempBrg.ANAME_40;
                brgMp.AVALUE_40 = tempBrg.AVALUE_40;
                brgMp.ACODE_41 = tempBrg.ACODE_41;
                brgMp.ANAME_41 = tempBrg.ANAME_41;
                brgMp.AVALUE_41 = tempBrg.AVALUE_41;
                brgMp.ACODE_42 = tempBrg.ACODE_42;
                brgMp.ANAME_42 = tempBrg.ANAME_42;
                brgMp.AVALUE_42 = tempBrg.AVALUE_42;
                brgMp.ACODE_43 = tempBrg.ACODE_43;
                brgMp.ANAME_43 = tempBrg.ANAME_43;
                brgMp.AVALUE_43 = tempBrg.AVALUE_43;
                brgMp.ACODE_44 = tempBrg.ACODE_44;
                brgMp.ANAME_44 = tempBrg.ANAME_44;
                brgMp.AVALUE_44 = tempBrg.AVALUE_44;
                brgMp.ACODE_45 = tempBrg.ACODE_45;
                brgMp.ANAME_45 = tempBrg.ANAME_45;
                brgMp.AVALUE_45 = tempBrg.AVALUE_45;
                brgMp.ACODE_46 = tempBrg.ACODE_46;
                brgMp.ANAME_46 = tempBrg.ANAME_46;
                brgMp.AVALUE_46 = tempBrg.AVALUE_46;
                brgMp.ACODE_47 = tempBrg.ACODE_47;
                brgMp.ANAME_47 = tempBrg.ANAME_47;
                brgMp.AVALUE_47 = tempBrg.AVALUE_47;
                brgMp.ACODE_48 = tempBrg.ACODE_48;
                brgMp.ANAME_48 = tempBrg.ANAME_48;
                brgMp.AVALUE_48 = tempBrg.AVALUE_48;
                brgMp.ACODE_49 = tempBrg.ACODE_49;
                brgMp.ANAME_49 = tempBrg.ANAME_49;
                brgMp.AVALUE_49 = tempBrg.AVALUE_49;
                brgMp.ACODE_50 = tempBrg.ACODE_50;
                brgMp.ANAME_50 = tempBrg.ANAME_50;
                brgMp.AVALUE_50 = tempBrg.AVALUE_50;
                #endregion
                if (insertSTF02h)
                    ErasoftDbContext.STF02H.Add(brgMp);
                ErasoftDbContext.SaveChanges();

                //delete brg induk di temp
                ErasoftDbContext.TEMP_BRG_MP.Where(b => b.BRG_MP == tempBrg.BRG_MP).Delete();
                if (tempBrg.BRG_MP != kdBrgMO)//user input baru kode brg MO -> update kode brg induk pada brg varian
                    EDB.ExecuteSQL("CString", CommandType.Text, "UPDATE TEMP_BRG_MP SET KODE_BRG_INDUK = '" + kdBrgMO + "' WHERE KODE_BRG_INDUK = '" + tempBrg.BRG_MP + "' AND CUST = '" + tempBrg.CUST + "'");
                ErasoftDbContext.SaveChanges();

                ret.status = 1;
            }
            catch (Exception ex)
            {
                ret.status = 0;
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }

            return ret;
        }

        public ActionResult UploadItemByCust(string cust, string dataPerPage, int skipDataError)
        {
            var barangVm = new UploadBarangViewModel()
            {
                ListTempBrg = ErasoftDbContext.TEMP_BRG_MP.Where(b => b.CUST.Equals(cust)).ToList(),
                ListMarket = ErasoftDbContext.ARF01.ToList(),
                Stf02 = new STF02(),
                TempBrg = new TEMP_BRG_MP(),
                Errors = new List<string>(),
                contRecursive = "0",
            };
            string username = "";
            List<string> listBrgSuccess = new List<string>();
            AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            if (sessionData?.Account != null)
            {
                username = sessionData.Account.Username;

            }
            else
            {
                if (sessionData?.User != null)
                {
                    username = sessionData.User.Username;
                }
            }
            var customer = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper().Equals(cust.ToUpper())).FirstOrDefault();
            if (customer != null)
            {
                var dataBrg = new List<TEMP_BRG_MP>();
                if (!string.IsNullOrEmpty(dataPerPage))
                {
                    if (skipDataError > 0)
                    {
                        dataBrg = ErasoftDbContext.TEMP_BRG_MP.Where(b => b.CUST.ToUpper().Equals(cust.ToUpper())).OrderBy(b => b.RecNum).Skip(skipDataError).Take(Convert.ToInt32(dataPerPage)).ToList();
                    }
                    else
                    {
                        dataBrg = ErasoftDbContext.TEMP_BRG_MP.Where(b => b.CUST.ToUpper().Equals(cust.ToUpper())).OrderBy(b => b.RecNum).Take(Convert.ToInt32(dataPerPage)).ToList();
                    }
                }
                else
                {
                    dataBrg = ErasoftDbContext.TEMP_BRG_MP.Where(b => b.CUST.ToUpper().Equals(cust.ToUpper())).ToList();
                }
                if (dataBrg.Count > 0)
                {
                    var defaultCategoryCode = ErasoftDbContext.STF02E.Where(c => c.LEVEL.Equals("1")).FirstOrDefault();
                    if (defaultCategoryCode == null)
                    {
                        barangVm.Errors.Add("Kode Kategori tidak ditemukan");
                        return Json(barangVm, JsonRequestBehavior.AllowGet);
                    }
                    var defaultBrand = ErasoftDbContext.STF02E.Where(c => c.LEVEL.Equals("2")).FirstOrDefault();
                    if (defaultBrand == null)
                    {
                        barangVm.Errors.Add("Kode Merek tidak ditemukan");
                        return Json(barangVm, JsonRequestBehavior.AllowGet);
                    }

                    var marketplace = MoDbContext.Marketplaces.Where(m => m.IdMarket.ToString().Equals(customer.NAMA)).FirstOrDefault();

                    foreach (var item in dataBrg)
                    {
                        //string brgBlibli = "";
                        //if (marketplace != null)
                        //{
                        //    if (marketplace.NamaMarket.ToUpper().Equals("BLIBLI"))
                        //    {
                        //        var kdBrgBlibli = item.BRG_MP.Split(';');
                        //        //stf02.BRG = "";
                        //        var kdBrg = kdBrgBlibli[0].Split('-');
                        //        for (int i = 1; i < kdBrg.Length; i++)
                        //        {
                        //            brgBlibli += kdBrg[i] + "-";
                        //        }
                        //        brgBlibli = brgBlibli.Substring(0, brgBlibli.Length - 1);
                        //    }
                        //}

                        //var barangInDB = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper().Equals(string.IsNullOrEmpty(brgBlibli) ? item.BRG_MP.ToUpper() : brgBlibli.ToUpper())).FirstOrDefault();
                        #region handle brg induk untuk brg varian
                        if (!string.IsNullOrEmpty(item.KODE_BRG_INDUK))//handle induk dari barang varian
                        {
                            bool createSTF02Induk = true;
                            var brgInduk = ErasoftDbContext.STF02.Where(b => b.BRG == item.KODE_BRG_INDUK).FirstOrDefault();
                            var tempBrgInduk = ErasoftDbContext.TEMP_BRG_MP.Where(b => b.BRG_MP == item.KODE_BRG_INDUK).FirstOrDefault();
                            if (brgInduk != null)
                            {
                                var stf02h_induk = ErasoftDbContext.STF02H.Where(b => b.BRG == brgInduk.BRG && b.IDMARKET == customer.RecNum).FirstOrDefault();
                                if (stf02h_induk == null)
                                {
                                    createSTF02Induk = false;
                                    if (tempBrgInduk != null)
                                    {
                                        var ret1 = AutoSyncBrgInduk(new STF02(), tempBrgInduk, item.KODE_BRG_INDUK, customer, username, createSTF02Induk);
                                        if (ret1.status == 0)
                                            barangVm.Errors.Add(item.SELLER_SKU + ";" + ret1.message);
                                    }
                                    else
                                    {
                                        barangVm.Errors.Add(item.SELLER_SKU + ";Barang Induk tidak ditemukan.");
                                        //return JsonErrorMessage("Barang Induk tidak ditemukan.");
                                    }

                                }
                            }
                            else
                            {
                                //if (tempBrginDB != null)
                                //{
                                //sinkron brg induk terlebih dahulu
                                var ret2 = AutoSyncBrgInduk(new STF02(), tempBrgInduk, item.KODE_BRG_INDUK, customer, username, createSTF02Induk);
                                if (ret2.status == 0)
                                    barangVm.Errors.Add(item.SELLER_SKU + ";" + ret2.message);
                                //}
                                //else
                                //{
                                //    return JsonErrorMessage("Barang ini sudah diproses");
                                //}
                            }
                        }
                        #endregion
                        var barangInDB = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper().Equals(item.SELLER_SKU.ToUpper())).FirstOrDefault();
                        if (barangInDB != null)
                        {
                            var brgMp = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper().Equals(barangInDB.BRG.ToUpper()) && b.IDMARKET == customer.RecNum).FirstOrDefault();
                            if (brgMp != null)
                            {
                                if (!string.IsNullOrEmpty(brgMp.BRG_MP))
                                {
                                    barangVm.Errors.Add(brgMp.BRG + ";Barang ini sudah link dengan barang lain di marketplace");
                                }
                                else
                                {
                                    brgMp.HJUAL = item.HJUAL;
                                    brgMp.DISPLAY = item.DISPLAY;
                                    brgMp.BRG_MP = item.BRG_MP;
                                    brgMp.CATEGORY_CODE = defaultCategoryCode.KODE;
                                    brgMp.CATEGORY_NAME = defaultBrand.KODE;
                                    brgMp.DeliveryTempElevenia = item.DeliveryTempElevenia;
                                    brgMp.PICKUP_POINT = item.PICKUP_POINT;
                                    #region attribute mp
                                    brgMp.ACODE_1 = item.ACODE_1;
                                    brgMp.ANAME_1 = item.ANAME_1;
                                    brgMp.AVALUE_1 = item.AVALUE_1;
                                    brgMp.ACODE_2 = item.ACODE_2;
                                    brgMp.ANAME_2 = item.ANAME_2;
                                    brgMp.AVALUE_2 = item.AVALUE_2;
                                    brgMp.ACODE_3 = item.ACODE_3;
                                    brgMp.ANAME_3 = item.ANAME_3;
                                    brgMp.AVALUE_3 = item.AVALUE_3;
                                    brgMp.ACODE_4 = item.ACODE_4;
                                    brgMp.ANAME_4 = item.ANAME_4;
                                    brgMp.AVALUE_4 = item.AVALUE_4;
                                    brgMp.ACODE_5 = item.ACODE_5;
                                    brgMp.ANAME_5 = item.ANAME_5;
                                    brgMp.AVALUE_5 = item.AVALUE_5;
                                    brgMp.ACODE_6 = item.ACODE_6;
                                    brgMp.ANAME_6 = item.ANAME_6;
                                    brgMp.AVALUE_6 = item.AVALUE_6;
                                    brgMp.ACODE_7 = item.ACODE_7;
                                    brgMp.ANAME_7 = item.ANAME_7;
                                    brgMp.AVALUE_7 = item.AVALUE_7;
                                    brgMp.ACODE_8 = item.ACODE_8;
                                    brgMp.ANAME_8 = item.ANAME_8;
                                    brgMp.AVALUE_8 = item.AVALUE_8;
                                    brgMp.ACODE_9 = item.ACODE_9;
                                    brgMp.ANAME_9 = item.ANAME_9;
                                    brgMp.AVALUE_9 = item.AVALUE_9;
                                    brgMp.ACODE_10 = item.ACODE_10;
                                    brgMp.ANAME_10 = item.ANAME_10;
                                    brgMp.AVALUE_10 = item.AVALUE_10;
                                    brgMp.ACODE_11 = item.ACODE_11;
                                    brgMp.ANAME_11 = item.ANAME_11;
                                    brgMp.AVALUE_11 = item.AVALUE_11;
                                    brgMp.ACODE_12 = item.ACODE_12;
                                    brgMp.ANAME_12 = item.ANAME_12;
                                    brgMp.AVALUE_12 = item.AVALUE_12;
                                    brgMp.ACODE_13 = item.ACODE_13;
                                    brgMp.ANAME_13 = item.ANAME_13;
                                    brgMp.AVALUE_13 = item.AVALUE_13;
                                    brgMp.ACODE_14 = item.ACODE_14;
                                    brgMp.ANAME_14 = item.ANAME_14;
                                    brgMp.AVALUE_14 = item.AVALUE_14;
                                    brgMp.ACODE_15 = item.ACODE_15;
                                    brgMp.ANAME_15 = item.ANAME_15;
                                    brgMp.AVALUE_15 = item.AVALUE_15;
                                    brgMp.ACODE_16 = item.ACODE_16;
                                    brgMp.ANAME_16 = item.ANAME_16;
                                    brgMp.AVALUE_16 = item.AVALUE_16;
                                    brgMp.ACODE_17 = item.ACODE_17;
                                    brgMp.ANAME_17 = item.ANAME_17;
                                    brgMp.AVALUE_17 = item.AVALUE_17;
                                    brgMp.ACODE_18 = item.ACODE_18;
                                    brgMp.ANAME_18 = item.ANAME_18;
                                    brgMp.AVALUE_18 = item.AVALUE_18;
                                    brgMp.ACODE_19 = item.ACODE_19;
                                    brgMp.ANAME_19 = item.ANAME_19;
                                    brgMp.AVALUE_19 = item.AVALUE_19;
                                    brgMp.ACODE_20 = item.ACODE_20;
                                    brgMp.ANAME_20 = item.ANAME_20;
                                    brgMp.AVALUE_20 = item.AVALUE_20;
                                    brgMp.ACODE_21 = item.ACODE_21;
                                    brgMp.ANAME_21 = item.ANAME_21;
                                    brgMp.AVALUE_21 = item.AVALUE_21;
                                    brgMp.ACODE_22 = item.ACODE_22;
                                    brgMp.ANAME_22 = item.ANAME_22;
                                    brgMp.AVALUE_22 = item.AVALUE_22;
                                    brgMp.ACODE_23 = item.ACODE_23;
                                    brgMp.ANAME_23 = item.ANAME_23;
                                    brgMp.AVALUE_23 = item.AVALUE_23;
                                    brgMp.ACODE_24 = item.ACODE_24;
                                    brgMp.ANAME_24 = item.ANAME_24;
                                    brgMp.AVALUE_24 = item.AVALUE_24;
                                    brgMp.ACODE_25 = item.ACODE_25;
                                    brgMp.ANAME_25 = item.ANAME_25;
                                    brgMp.AVALUE_25 = item.AVALUE_25;
                                    brgMp.ACODE_26 = item.ACODE_26;
                                    brgMp.ANAME_26 = item.ANAME_26;
                                    brgMp.AVALUE_26 = item.AVALUE_26;
                                    brgMp.ACODE_27 = item.ACODE_27;
                                    brgMp.ANAME_27 = item.ANAME_27;
                                    brgMp.AVALUE_27 = item.AVALUE_27;
                                    brgMp.ACODE_28 = item.ACODE_28;
                                    brgMp.ANAME_28 = item.ANAME_28;
                                    brgMp.AVALUE_28 = item.AVALUE_28;
                                    brgMp.ACODE_29 = item.ACODE_29;
                                    brgMp.ANAME_29 = item.ANAME_29;
                                    brgMp.AVALUE_29 = item.AVALUE_29;
                                    brgMp.ACODE_30 = item.ACODE_30;
                                    brgMp.ANAME_30 = item.ANAME_30;
                                    brgMp.AVALUE_30 = item.AVALUE_30;
                                    brgMp.ACODE_31 = item.ACODE_31;
                                    brgMp.ANAME_31 = item.ANAME_31;
                                    brgMp.AVALUE_31 = item.AVALUE_31;
                                    brgMp.ACODE_32 = item.ACODE_32;
                                    brgMp.ANAME_32 = item.ANAME_32;
                                    brgMp.AVALUE_32 = item.AVALUE_32;
                                    brgMp.ACODE_33 = item.ACODE_33;
                                    brgMp.ANAME_33 = item.ANAME_33;
                                    brgMp.AVALUE_33 = item.AVALUE_33;
                                    brgMp.ACODE_34 = item.ACODE_34;
                                    brgMp.ANAME_34 = item.ANAME_34;
                                    brgMp.AVALUE_34 = item.AVALUE_34;
                                    brgMp.ACODE_35 = item.ACODE_35;
                                    brgMp.ANAME_35 = item.ANAME_35;
                                    brgMp.AVALUE_35 = item.AVALUE_35;
                                    brgMp.ACODE_36 = item.ACODE_36;
                                    brgMp.ANAME_36 = item.ANAME_36;
                                    brgMp.AVALUE_36 = item.AVALUE_36;
                                    brgMp.ACODE_37 = item.ACODE_37;
                                    brgMp.ANAME_37 = item.ANAME_37;
                                    brgMp.AVALUE_37 = item.AVALUE_37;
                                    brgMp.ACODE_38 = item.ACODE_38;
                                    brgMp.ANAME_38 = item.ANAME_38;
                                    brgMp.AVALUE_38 = item.AVALUE_38;
                                    brgMp.ACODE_39 = item.ACODE_39;
                                    brgMp.ANAME_39 = item.ANAME_39;
                                    brgMp.AVALUE_39 = item.AVALUE_39;
                                    brgMp.ACODE_40 = item.ACODE_40;
                                    brgMp.ANAME_40 = item.ANAME_40;
                                    brgMp.AVALUE_40 = item.AVALUE_40;
                                    brgMp.ACODE_41 = item.ACODE_41;
                                    brgMp.ANAME_41 = item.ANAME_41;
                                    brgMp.AVALUE_41 = item.AVALUE_41;
                                    brgMp.ACODE_42 = item.ACODE_42;
                                    brgMp.ANAME_42 = item.ANAME_42;
                                    brgMp.AVALUE_42 = item.AVALUE_42;
                                    brgMp.ACODE_43 = item.ACODE_43;
                                    brgMp.ANAME_43 = item.ANAME_43;
                                    brgMp.AVALUE_43 = item.AVALUE_43;
                                    brgMp.ACODE_44 = item.ACODE_44;
                                    brgMp.ANAME_44 = item.ANAME_44;
                                    brgMp.AVALUE_44 = item.AVALUE_44;
                                    brgMp.ACODE_45 = item.ACODE_45;
                                    brgMp.ANAME_45 = item.ANAME_45;
                                    brgMp.AVALUE_45 = item.AVALUE_45;
                                    brgMp.ACODE_46 = item.ACODE_46;
                                    brgMp.ANAME_46 = item.ANAME_46;
                                    brgMp.AVALUE_46 = item.AVALUE_46;
                                    brgMp.ACODE_47 = item.ACODE_47;
                                    brgMp.ANAME_47 = item.ANAME_47;
                                    brgMp.AVALUE_47 = item.AVALUE_47;
                                    brgMp.ACODE_48 = item.ACODE_48;
                                    brgMp.ANAME_48 = item.ANAME_48;
                                    brgMp.AVALUE_48 = item.AVALUE_48;
                                    brgMp.ACODE_49 = item.ACODE_49;
                                    brgMp.ANAME_49 = item.ANAME_49;
                                    brgMp.AVALUE_49 = item.AVALUE_49;
                                    brgMp.ACODE_50 = item.ACODE_50;
                                    brgMp.ANAME_50 = item.ANAME_50;
                                    brgMp.AVALUE_50 = item.AVALUE_50;
                                    #endregion
                                    ErasoftDbContext.SaveChanges();
                                    listBrgSuccess.Add(item.BRG_MP);
                                }
                            }
                            else
                            {
                                brgMp = new STF02H();
                                //change stf02h brg = seller sku
                                //brgMp.BRG = string.IsNullOrEmpty(brgBlibli) ? item.BRG_MP : brgBlibli;
                                brgMp.BRG = item.SELLER_SKU;
                                //end change stf02h brg = seller sku
                                brgMp.BRG_MP = item.BRG_MP;
                                brgMp.HJUAL = item.HJUAL;
                                brgMp.DISPLAY = item.DISPLAY;
                                brgMp.CATEGORY_CODE = item.CATEGORY_CODE;
                                brgMp.CATEGORY_NAME = item.CATEGORY_NAME;
                                brgMp.IDMARKET = item.IDMARKET;
                                brgMp.DeliveryTempElevenia = item.DeliveryTempElevenia;
                                brgMp.PICKUP_POINT = item.PICKUP_POINT;
                                //var customer = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper().Equals(data.TempBrg.CUST.ToUpper())).FirstOrDefault();
                                //if (customer != null)
                                brgMp.AKUNMARKET = customer.PERSO;
                                //brgMp.USERNAME = "SYSTEM_UPLOAD_BRG";
                                brgMp.USERNAME = username;
                                #region attribute mp
                                brgMp.ACODE_1 = item.ACODE_1;
                                brgMp.ANAME_1 = item.ANAME_1;
                                brgMp.AVALUE_1 = item.AVALUE_1;
                                brgMp.ACODE_2 = item.ACODE_2;
                                brgMp.ANAME_2 = item.ANAME_2;
                                brgMp.AVALUE_2 = item.AVALUE_2;
                                brgMp.ACODE_3 = item.ACODE_3;
                                brgMp.ANAME_3 = item.ANAME_3;
                                brgMp.AVALUE_3 = item.AVALUE_3;
                                brgMp.ACODE_4 = item.ACODE_4;
                                brgMp.ANAME_4 = item.ANAME_4;
                                brgMp.AVALUE_4 = item.AVALUE_4;
                                brgMp.ACODE_5 = item.ACODE_5;
                                brgMp.ANAME_5 = item.ANAME_5;
                                brgMp.AVALUE_5 = item.AVALUE_5;
                                brgMp.ACODE_6 = item.ACODE_6;
                                brgMp.ANAME_6 = item.ANAME_6;
                                brgMp.AVALUE_6 = item.AVALUE_6;
                                brgMp.ACODE_7 = item.ACODE_7;
                                brgMp.ANAME_7 = item.ANAME_7;
                                brgMp.AVALUE_7 = item.AVALUE_7;
                                brgMp.ACODE_8 = item.ACODE_8;
                                brgMp.ANAME_8 = item.ANAME_8;
                                brgMp.AVALUE_8 = item.AVALUE_8;
                                brgMp.ACODE_9 = item.ACODE_9;
                                brgMp.ANAME_9 = item.ANAME_9;
                                brgMp.AVALUE_9 = item.AVALUE_9;
                                brgMp.ACODE_10 = item.ACODE_10;
                                brgMp.ANAME_10 = item.ANAME_10;
                                brgMp.AVALUE_10 = item.AVALUE_10;
                                brgMp.ACODE_11 = item.ACODE_11;
                                brgMp.ANAME_11 = item.ANAME_11;
                                brgMp.AVALUE_11 = item.AVALUE_11;
                                brgMp.ACODE_12 = item.ACODE_12;
                                brgMp.ANAME_12 = item.ANAME_12;
                                brgMp.AVALUE_12 = item.AVALUE_12;
                                brgMp.ACODE_13 = item.ACODE_13;
                                brgMp.ANAME_13 = item.ANAME_13;
                                brgMp.AVALUE_13 = item.AVALUE_13;
                                brgMp.ACODE_14 = item.ACODE_14;
                                brgMp.ANAME_14 = item.ANAME_14;
                                brgMp.AVALUE_14 = item.AVALUE_14;
                                brgMp.ACODE_15 = item.ACODE_15;
                                brgMp.ANAME_15 = item.ANAME_15;
                                brgMp.AVALUE_15 = item.AVALUE_15;
                                brgMp.ACODE_16 = item.ACODE_16;
                                brgMp.ANAME_16 = item.ANAME_16;
                                brgMp.AVALUE_16 = item.AVALUE_16;
                                brgMp.ACODE_17 = item.ACODE_17;
                                brgMp.ANAME_17 = item.ANAME_17;
                                brgMp.AVALUE_17 = item.AVALUE_17;
                                brgMp.ACODE_18 = item.ACODE_18;
                                brgMp.ANAME_18 = item.ANAME_18;
                                brgMp.AVALUE_18 = item.AVALUE_18;
                                brgMp.ACODE_19 = item.ACODE_19;
                                brgMp.ANAME_19 = item.ANAME_19;
                                brgMp.AVALUE_19 = item.AVALUE_19;
                                brgMp.ACODE_20 = item.ACODE_20;
                                brgMp.ANAME_20 = item.ANAME_20;
                                brgMp.AVALUE_20 = item.AVALUE_20;
                                brgMp.ACODE_21 = item.ACODE_21;
                                brgMp.ANAME_21 = item.ANAME_21;
                                brgMp.AVALUE_21 = item.AVALUE_21;
                                brgMp.ACODE_22 = item.ACODE_22;
                                brgMp.ANAME_22 = item.ANAME_22;
                                brgMp.AVALUE_22 = item.AVALUE_22;
                                brgMp.ACODE_23 = item.ACODE_23;
                                brgMp.ANAME_23 = item.ANAME_23;
                                brgMp.AVALUE_23 = item.AVALUE_23;
                                brgMp.ACODE_24 = item.ACODE_24;
                                brgMp.ANAME_24 = item.ANAME_24;
                                brgMp.AVALUE_24 = item.AVALUE_24;
                                brgMp.ACODE_25 = item.ACODE_25;
                                brgMp.ANAME_25 = item.ANAME_25;
                                brgMp.AVALUE_25 = item.AVALUE_25;
                                brgMp.ACODE_26 = item.ACODE_26;
                                brgMp.ANAME_26 = item.ANAME_26;
                                brgMp.AVALUE_26 = item.AVALUE_26;
                                brgMp.ACODE_27 = item.ACODE_27;
                                brgMp.ANAME_27 = item.ANAME_27;
                                brgMp.AVALUE_27 = item.AVALUE_27;
                                brgMp.ACODE_28 = item.ACODE_28;
                                brgMp.ANAME_28 = item.ANAME_28;
                                brgMp.AVALUE_28 = item.AVALUE_28;
                                brgMp.ACODE_29 = item.ACODE_29;
                                brgMp.ANAME_29 = item.ANAME_29;
                                brgMp.AVALUE_29 = item.AVALUE_29;
                                brgMp.ACODE_30 = item.ACODE_30;
                                brgMp.ANAME_30 = item.ANAME_30;
                                brgMp.AVALUE_30 = item.AVALUE_30;
                                brgMp.ACODE_31 = item.ACODE_31;
                                brgMp.ANAME_31 = item.ANAME_31;
                                brgMp.AVALUE_31 = item.AVALUE_31;
                                brgMp.ACODE_32 = item.ACODE_32;
                                brgMp.ANAME_32 = item.ANAME_32;
                                brgMp.AVALUE_32 = item.AVALUE_32;
                                brgMp.ACODE_33 = item.ACODE_33;
                                brgMp.ANAME_33 = item.ANAME_33;
                                brgMp.AVALUE_33 = item.AVALUE_33;
                                brgMp.ACODE_34 = item.ACODE_34;
                                brgMp.ANAME_34 = item.ANAME_34;
                                brgMp.AVALUE_34 = item.AVALUE_34;
                                brgMp.ACODE_35 = item.ACODE_35;
                                brgMp.ANAME_35 = item.ANAME_35;
                                brgMp.AVALUE_35 = item.AVALUE_35;
                                brgMp.ACODE_36 = item.ACODE_36;
                                brgMp.ANAME_36 = item.ANAME_36;
                                brgMp.AVALUE_36 = item.AVALUE_36;
                                brgMp.ACODE_37 = item.ACODE_37;
                                brgMp.ANAME_37 = item.ANAME_37;
                                brgMp.AVALUE_37 = item.AVALUE_37;
                                brgMp.ACODE_38 = item.ACODE_38;
                                brgMp.ANAME_38 = item.ANAME_38;
                                brgMp.AVALUE_38 = item.AVALUE_38;
                                brgMp.ACODE_39 = item.ACODE_39;
                                brgMp.ANAME_39 = item.ANAME_39;
                                brgMp.AVALUE_39 = item.AVALUE_39;
                                brgMp.ACODE_40 = item.ACODE_40;
                                brgMp.ANAME_40 = item.ANAME_40;
                                brgMp.AVALUE_40 = item.AVALUE_40;
                                brgMp.ACODE_41 = item.ACODE_41;
                                brgMp.ANAME_41 = item.ANAME_41;
                                brgMp.AVALUE_41 = item.AVALUE_41;
                                brgMp.ACODE_42 = item.ACODE_42;
                                brgMp.ANAME_42 = item.ANAME_42;
                                brgMp.AVALUE_42 = item.AVALUE_42;
                                brgMp.ACODE_43 = item.ACODE_43;
                                brgMp.ANAME_43 = item.ANAME_43;
                                brgMp.AVALUE_43 = item.AVALUE_43;
                                brgMp.ACODE_44 = item.ACODE_44;
                                brgMp.ANAME_44 = item.ANAME_44;
                                brgMp.AVALUE_44 = item.AVALUE_44;
                                brgMp.ACODE_45 = item.ACODE_45;
                                brgMp.ANAME_45 = item.ANAME_45;
                                brgMp.AVALUE_45 = item.AVALUE_45;
                                brgMp.ACODE_46 = item.ACODE_46;
                                brgMp.ANAME_46 = item.ANAME_46;
                                brgMp.AVALUE_46 = item.AVALUE_46;
                                brgMp.ACODE_47 = item.ACODE_47;
                                brgMp.ANAME_47 = item.ANAME_47;
                                brgMp.AVALUE_47 = item.AVALUE_47;
                                brgMp.ACODE_48 = item.ACODE_48;
                                brgMp.ANAME_48 = item.ANAME_48;
                                brgMp.AVALUE_48 = item.AVALUE_48;
                                brgMp.ACODE_49 = item.ACODE_49;
                                brgMp.ANAME_49 = item.ANAME_49;
                                brgMp.AVALUE_49 = item.AVALUE_49;
                                brgMp.ACODE_50 = item.ACODE_50;
                                brgMp.ANAME_50 = item.ANAME_50;
                                brgMp.AVALUE_50 = item.AVALUE_50;
                                #endregion
                                ErasoftDbContext.STF02H.Add(brgMp);
                                ErasoftDbContext.SaveChanges();
                                listBrgSuccess.Add(item.BRG_MP);

                            }
                        }
                        else
                        {
                            var stf02 = new STF02
                            {
                                HPP = 0,
                                HBELI = 0,
                                HBESAR = 0,
                                HKECIL = 0,
                                //TYPE = "3",
                                KLINK = "1",
                                HP_STD = 0,
                                QPROD = 0,
                                ISI3 = 3,
                                ISI4 = 1,
                                TOLERANSI = 0,
                                H_STN_3 = 0,
                                H_STN_4 = 0,
                                SS = 0,
                                METODA_HPP_PER_SN = false,
                                HNA_PPN = 0,
                                LABA = 0,
                                DEFAULT_STN_HRG_JUAL = 0,
                                DEFAULT_STN_JUAL = 0,
                                ISI = 1,
                                Metoda = "1",
                                Tgl_Input = DateTime.Now,
                                TGL_KLR = DateTime.Now,
                                MAXI = 100,
                                MINI = 1,
                                QSALES = 0,
                                DISPLAY_MARKET = false,
                            };
                            //change stf02 brg = seller sku
                            //stf02.BRG = string.IsNullOrEmpty(brgBlibli) ? item.BRG_MP : brgBlibli;
                            stf02.BRG = item.SELLER_SKU;
                            //end change stf02 brg = seller sku
                            //var marketplace = MoDbContext.Marketplaces.Where(m => m.IdMarket.ToString().Equals(customer.NAMA)).FirstOrDefault();
                            //if (marketplace != null)
                            //{
                            //    if (marketplace.NamaMarket.ToUpper().Equals("BLIBLI"))
                            //    {
                            //        var kdBrgBlibli = item.BRG_MP.Split(';');
                            //        stf02.BRG = "";
                            //        var kdBrg = kdBrgBlibli[0].Split('-');
                            //        for(int i =1; i < kdBrg.Length; i++)
                            //        {
                            //            stf02.BRG += kdBrg[i] + "-";
                            //        }
                            //        stf02.BRG = stf02.BRG.Substring(0, stf02.BRG.Length - 1);
                            //    }
                            //}
                            stf02.NAMA = item.NAMA;
                            stf02.NAMA2 = item.NAMA2;
                            stf02.NAMA3 = item.NAMA3;
                            stf02.HJUAL = item.HJUAL;
                            stf02.STN = "pcs";
                            stf02.STN2 = "pcs";
                            stf02.BERAT = item.BERAT;
                            stf02.TINGGI = item.TINGGI;
                            stf02.LEBAR = item.LEBAR;
                            stf02.PANJANG = item.PANJANG;
                            stf02.Sort1 = defaultCategoryCode.KODE;
                            stf02.Sort2 = defaultBrand.KODE;
                            stf02.KET_SORT1 = defaultCategoryCode.KET;
                            stf02.KET_SORT2 = defaultBrand.KET;
                            stf02.Deskripsi = (string.IsNullOrEmpty(item.Deskripsi) ? "-" : item.Deskripsi);

                            //add 25 Jan 2019, handle brg induk & varian
                            stf02.TYPE = item.TYPE;
                            stf02.PART = item.KODE_BRG_INDUK;
                            //end 25 Jan 2019, handle brg induk & varian

                            if (!string.IsNullOrEmpty(item.IMAGE))
                            {
                                stf02.LINK_GAMBAR_1 = UploadImageService.UploadSingleImageToImgurFromUrl(item.IMAGE, "uploaded-image").data.link_l;
                            }
                            if (!string.IsNullOrEmpty(item.IMAGE2))
                            {
                                stf02.LINK_GAMBAR_2 = UploadImageService.UploadSingleImageToImgurFromUrl(item.IMAGE2, "uploaded-image").data.link_l;
                            }
                            if (!string.IsNullOrEmpty(item.IMAGE3))
                            {
                                stf02.LINK_GAMBAR_3 = UploadImageService.UploadSingleImageToImgurFromUrl(item.IMAGE3, "uploaded-image").data.link_l;
                            }

                            ErasoftDbContext.STF02.Add(stf02);
                            var brgMp = new STF02H();

                            //brgMp.BRG = item.BRG_MP;
                            brgMp.BRG = stf02.BRG;
                            brgMp.BRG_MP = item.BRG_MP;
                            brgMp.HJUAL = item.HJUAL;
                            brgMp.DISPLAY = item.DISPLAY;
                            brgMp.CATEGORY_CODE = item.CATEGORY_CODE;
                            brgMp.CATEGORY_NAME = item.CATEGORY_NAME;
                            brgMp.IDMARKET = item.IDMARKET;
                            brgMp.DeliveryTempElevenia = item.DeliveryTempElevenia;
                            brgMp.PICKUP_POINT = item.PICKUP_POINT;
                            //var customer = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper().Equals(data.TempBrg.CUST.ToUpper())).FirstOrDefault();
                            //if (customer != null)
                            brgMp.AKUNMARKET = customer.PERSO;
                            //brgMp.USERNAME = "SYSTEM_UPLOAD_BRG";
                            brgMp.USERNAME = username;
                            #region attribute mp
                            brgMp.ACODE_1 = item.ACODE_1;
                            brgMp.ANAME_1 = item.ANAME_1;
                            brgMp.AVALUE_1 = item.AVALUE_1;
                            brgMp.ACODE_2 = item.ACODE_2;
                            brgMp.ANAME_2 = item.ANAME_2;
                            brgMp.AVALUE_2 = item.AVALUE_2;
                            brgMp.ACODE_3 = item.ACODE_3;
                            brgMp.ANAME_3 = item.ANAME_3;
                            brgMp.AVALUE_3 = item.AVALUE_3;
                            brgMp.ACODE_4 = item.ACODE_4;
                            brgMp.ANAME_4 = item.ANAME_4;
                            brgMp.AVALUE_4 = item.AVALUE_4;
                            brgMp.ACODE_5 = item.ACODE_5;
                            brgMp.ANAME_5 = item.ANAME_5;
                            brgMp.AVALUE_5 = item.AVALUE_5;
                            brgMp.ACODE_6 = item.ACODE_6;
                            brgMp.ANAME_6 = item.ANAME_6;
                            brgMp.AVALUE_6 = item.AVALUE_6;
                            brgMp.ACODE_7 = item.ACODE_7;
                            brgMp.ANAME_7 = item.ANAME_7;
                            brgMp.AVALUE_7 = item.AVALUE_7;
                            brgMp.ACODE_8 = item.ACODE_8;
                            brgMp.ANAME_8 = item.ANAME_8;
                            brgMp.AVALUE_8 = item.AVALUE_8;
                            brgMp.ACODE_9 = item.ACODE_9;
                            brgMp.ANAME_9 = item.ANAME_9;
                            brgMp.AVALUE_9 = item.AVALUE_9;
                            brgMp.ACODE_10 = item.ACODE_10;
                            brgMp.ANAME_10 = item.ANAME_10;
                            brgMp.AVALUE_10 = item.AVALUE_10;
                            brgMp.ACODE_11 = item.ACODE_11;
                            brgMp.ANAME_11 = item.ANAME_11;
                            brgMp.AVALUE_11 = item.AVALUE_11;
                            brgMp.ACODE_12 = item.ACODE_12;
                            brgMp.ANAME_12 = item.ANAME_12;
                            brgMp.AVALUE_12 = item.AVALUE_12;
                            brgMp.ACODE_13 = item.ACODE_13;
                            brgMp.ANAME_13 = item.ANAME_13;
                            brgMp.AVALUE_13 = item.AVALUE_13;
                            brgMp.ACODE_14 = item.ACODE_14;
                            brgMp.ANAME_14 = item.ANAME_14;
                            brgMp.AVALUE_14 = item.AVALUE_14;
                            brgMp.ACODE_15 = item.ACODE_15;
                            brgMp.ANAME_15 = item.ANAME_15;
                            brgMp.AVALUE_15 = item.AVALUE_15;
                            brgMp.ACODE_16 = item.ACODE_16;
                            brgMp.ANAME_16 = item.ANAME_16;
                            brgMp.AVALUE_16 = item.AVALUE_16;
                            brgMp.ACODE_17 = item.ACODE_17;
                            brgMp.ANAME_17 = item.ANAME_17;
                            brgMp.AVALUE_17 = item.AVALUE_17;
                            brgMp.ACODE_18 = item.ACODE_18;
                            brgMp.ANAME_18 = item.ANAME_18;
                            brgMp.AVALUE_18 = item.AVALUE_18;
                            brgMp.ACODE_19 = item.ACODE_19;
                            brgMp.ANAME_19 = item.ANAME_19;
                            brgMp.AVALUE_19 = item.AVALUE_19;
                            brgMp.ACODE_20 = item.ACODE_20;
                            brgMp.ANAME_20 = item.ANAME_20;
                            brgMp.AVALUE_20 = item.AVALUE_20;
                            brgMp.ACODE_21 = item.ACODE_21;
                            brgMp.ANAME_21 = item.ANAME_21;
                            brgMp.AVALUE_21 = item.AVALUE_21;
                            brgMp.ACODE_22 = item.ACODE_22;
                            brgMp.ANAME_22 = item.ANAME_22;
                            brgMp.AVALUE_22 = item.AVALUE_22;
                            brgMp.ACODE_23 = item.ACODE_23;
                            brgMp.ANAME_23 = item.ANAME_23;
                            brgMp.AVALUE_23 = item.AVALUE_23;
                            brgMp.ACODE_24 = item.ACODE_24;
                            brgMp.ANAME_24 = item.ANAME_24;
                            brgMp.AVALUE_24 = item.AVALUE_24;
                            brgMp.ACODE_25 = item.ACODE_25;
                            brgMp.ANAME_25 = item.ANAME_25;
                            brgMp.AVALUE_25 = item.AVALUE_25;
                            brgMp.ACODE_26 = item.ACODE_26;
                            brgMp.ANAME_26 = item.ANAME_26;
                            brgMp.AVALUE_26 = item.AVALUE_26;
                            brgMp.ACODE_27 = item.ACODE_27;
                            brgMp.ANAME_27 = item.ANAME_27;
                            brgMp.AVALUE_27 = item.AVALUE_27;
                            brgMp.ACODE_28 = item.ACODE_28;
                            brgMp.ANAME_28 = item.ANAME_28;
                            brgMp.AVALUE_28 = item.AVALUE_28;
                            brgMp.ACODE_29 = item.ACODE_29;
                            brgMp.ANAME_29 = item.ANAME_29;
                            brgMp.AVALUE_29 = item.AVALUE_29;
                            brgMp.ACODE_30 = item.ACODE_30;
                            brgMp.ANAME_30 = item.ANAME_30;
                            brgMp.AVALUE_30 = item.AVALUE_30;
                            brgMp.ACODE_31 = item.ACODE_31;
                            brgMp.ANAME_31 = item.ANAME_31;
                            brgMp.AVALUE_31 = item.AVALUE_31;
                            brgMp.ACODE_32 = item.ACODE_32;
                            brgMp.ANAME_32 = item.ANAME_32;
                            brgMp.AVALUE_32 = item.AVALUE_32;
                            brgMp.ACODE_33 = item.ACODE_33;
                            brgMp.ANAME_33 = item.ANAME_33;
                            brgMp.AVALUE_33 = item.AVALUE_33;
                            brgMp.ACODE_34 = item.ACODE_34;
                            brgMp.ANAME_34 = item.ANAME_34;
                            brgMp.AVALUE_34 = item.AVALUE_34;
                            brgMp.ACODE_35 = item.ACODE_35;
                            brgMp.ANAME_35 = item.ANAME_35;
                            brgMp.AVALUE_35 = item.AVALUE_35;
                            brgMp.ACODE_36 = item.ACODE_36;
                            brgMp.ANAME_36 = item.ANAME_36;
                            brgMp.AVALUE_36 = item.AVALUE_36;
                            brgMp.ACODE_37 = item.ACODE_37;
                            brgMp.ANAME_37 = item.ANAME_37;
                            brgMp.AVALUE_37 = item.AVALUE_37;
                            brgMp.ACODE_38 = item.ACODE_38;
                            brgMp.ANAME_38 = item.ANAME_38;
                            brgMp.AVALUE_38 = item.AVALUE_38;
                            brgMp.ACODE_39 = item.ACODE_39;
                            brgMp.ANAME_39 = item.ANAME_39;
                            brgMp.AVALUE_39 = item.AVALUE_39;
                            brgMp.ACODE_40 = item.ACODE_40;
                            brgMp.ANAME_40 = item.ANAME_40;
                            brgMp.AVALUE_40 = item.AVALUE_40;
                            brgMp.ACODE_41 = item.ACODE_41;
                            brgMp.ANAME_41 = item.ANAME_41;
                            brgMp.AVALUE_41 = item.AVALUE_41;
                            brgMp.ACODE_42 = item.ACODE_42;
                            brgMp.ANAME_42 = item.ANAME_42;
                            brgMp.AVALUE_42 = item.AVALUE_42;
                            brgMp.ACODE_43 = item.ACODE_43;
                            brgMp.ANAME_43 = item.ANAME_43;
                            brgMp.AVALUE_43 = item.AVALUE_43;
                            brgMp.ACODE_44 = item.ACODE_44;
                            brgMp.ANAME_44 = item.ANAME_44;
                            brgMp.AVALUE_44 = item.AVALUE_44;
                            brgMp.ACODE_45 = item.ACODE_45;
                            brgMp.ANAME_45 = item.ANAME_45;
                            brgMp.AVALUE_45 = item.AVALUE_45;
                            brgMp.ACODE_46 = item.ACODE_46;
                            brgMp.ANAME_46 = item.ANAME_46;
                            brgMp.AVALUE_46 = item.AVALUE_46;
                            brgMp.ACODE_47 = item.ACODE_47;
                            brgMp.ANAME_47 = item.ANAME_47;
                            brgMp.AVALUE_47 = item.AVALUE_47;
                            brgMp.ACODE_48 = item.ACODE_48;
                            brgMp.ANAME_48 = item.ANAME_48;
                            brgMp.AVALUE_48 = item.AVALUE_48;
                            brgMp.ACODE_49 = item.ACODE_49;
                            brgMp.ANAME_49 = item.ANAME_49;
                            brgMp.AVALUE_49 = item.AVALUE_49;
                            brgMp.ACODE_50 = item.ACODE_50;
                            brgMp.ANAME_50 = item.ANAME_50;
                            brgMp.AVALUE_50 = item.AVALUE_50;
                            #endregion
                            ErasoftDbContext.STF02H.Add(brgMp);
                            ErasoftDbContext.SaveChanges();
                            listBrgSuccess.Add(item.BRG_MP);
                        }
                    }
                    if (listBrgSuccess.Count > 0)
                    {
                        //if(Convert.ToInt32(dataPerPage) > listBrgSuccess.Count)
                        //{
                        barangVm.failedRecord = string.IsNullOrEmpty(skipDataError.ToString()) ? 0 : skipDataError + Convert.ToInt32(string.IsNullOrEmpty(dataPerPage) ? "0" : dataPerPage) - listBrgSuccess.Count;
                        //}
                        foreach (var brg_mp in listBrgSuccess)
                        {
                            ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.Equals(brg_mp)).Delete();
                        }
                        ErasoftDbContext.SaveChanges();
                    }
                    barangVm.ListTempBrg = ErasoftDbContext.TEMP_BRG_MP.Where(b => b.CUST.Equals(cust)).ToList();
                    barangVm.contRecursive = "1";
                    //if (barangVm.Errors.Count == 0)
                    //if(dataBrg.Count < Convert.ToInt32(dataPerPage))
                    if (barangVm.Errors.Count == 0 && string.IsNullOrEmpty(dataPerPage))
                    {
                        return PartialView("TableUploadBarangPartial", barangVm);
                    }
                    //else
                    {
                        return Json(barangVm, JsonRequestBehavior.AllowGet);
                    }
                }
                barangVm.Errors.Add("Tidak ada barang untuk di upload pada Toko ini.");
                return Json(barangVm, JsonRequestBehavior.AllowGet);
            }
            barangVm.Errors.Add("Toko ini tidak ditemukan.");
            return Json(barangVm, JsonRequestBehavior.AllowGet);
        }
        public ActionResult EditBarangUpload(string brg_mp, string cust)
        {
            var barangVm = new UploadBarangViewModel()
            {
                ListTempBrg = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.CUST == cust).ToList(),
                ListMarket = ErasoftDbContext.ARF01.ToList(),
                Stf02 = new STF02(),
                TempBrg = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.Equals(brg_mp.ToUpper()) && t.CUST == cust).FirstOrDefault(),
                ListKategoriMerk = ErasoftDbContext.STF02E.Where(m => m.LEVEL.Equals("2")).OrderBy(m => m.KET).ToList(),
                ListKategoriBrg = ErasoftDbContext.STF02E.Where(m => m.LEVEL.Equals("1")).OrderBy(m => m.KET).ToList(),

            };

            return PartialView("FormBarangUploadsPartial", barangVm);
        }

        public ActionResult DeleteBarangTemp(string barangId, string cust)
        {
            try
            {
                var barangInDb = ErasoftDbContext.TEMP_BRG_MP.Single(b => b.BRG_MP.ToUpper() == barangId.ToUpper() && b.CUST == cust);

                ErasoftDbContext.TEMP_BRG_MP.Remove(barangInDb);
                ErasoftDbContext.SaveChanges();

                //change by calvin 14 januari 2019
                //var barangVm = new UploadBarangViewModel()
                //{
                //    ListTempBrg = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.CUST == cust).ToList(),
                //    ListMarket = ErasoftDbContext.ARF01.ToList(),
                //    Stf02 = new STF02(),
                //    TempBrg = new TEMP_BRG_MP(),
                //};

                //return Json(barangVm, JsonRequestBehavior.AllowGet);
                var barangVm = new UploadBarangViewModel()
                {
                    ListTempBrg = ErasoftDbContext.TEMP_BRG_MP.Where(b => b.CUST.Equals(cust)).ToList(),
                    ListMarket = ErasoftDbContext.ARF01.ToList(),
                    Stf02 = new STF02(),
                    TempBrg = new TEMP_BRG_MP(),
                };

                return PartialView("TableUploadBarangPartial", barangVm);
                //end change by calvin 14 januari 2019
            }
            catch (Exception ex)
            {
                return JsonErrorMessage(ex.Message);
            }

        }

        [Route("manage/PromptCustomer")]
        public ActionResult PromptCustomer()
        {
            try
            {
                var PromptModel = new List<PromptCustomerViewModel>();
                var listCust = ErasoftDbContext.ARF01.ToList();
                foreach (var customer in listCust)
                {
                    PromptModel.Add(
                        new PromptCustomerViewModel
                        {
                            KODE = customer.CUST,
                            NAMA = customer.PERSO,
                            MARKETPLACE = MoDbContext.Marketplaces.Where(m => m.IdMarket.ToString() == customer.NAMA).FirstOrDefault().NamaMarket,
                            IDMARKET = customer.NAMA
                        }
                        );
                }
                return View("PromptCustomer", PromptModel);
            }
            catch (Exception ex)
            {
                return JsonErrorMessage("Prompt gagal");
            }
        }

        [Route("manage/PromptBarang")]
        public ActionResult PromptBarang(string cust, string nama, string typeBrg)
        {
            try
            {
                var retObj = new PromptBrg();
                retObj.NAMA_BRG = nama.Length > 10 ? nama.Substring(0, 10) : nama;
                var PromptModel = new List<PromptBarangViewModel>();
                //change 1 Feb 2019, prompt berdasarkan type barang tidak jd dipakai
                //change by Tri 22-01-2019, prompt sesuai type barang
                var listBarang = ErasoftDbContext.STF02.ToList();
                //var listBarang = ErasoftDbContext.STF02.Where(b => b.TYPE == typeBrg).ToList();
                //end change by Tri 22-01-2019, prompt sesuai type barang
                //end change 1 Feb 2019, prompt berdasarkan type barang tidak jd dipakai

                var customer = ErasoftDbContext.ARF01.Where(c => c.CUST.Equals(cust)).FirstOrDefault();
                foreach (var barang in listBarang)
                {
                    if (customer != null)
                    {
                        //remark 1 Feb 2019, tidak terpakai
                        //if (typeBrg == "4")//ambil brg jasa untuk prompt brg induk
                        //{
                        //    PromptModel.Add(
                        //           new PromptBarangViewModel
                        //           {
                        //               KODE = barang.BRG,
                        //               NAMA = barang.NAMA + " " + barang.NAMA2,
                        //               HARGA = barang.HJUAL
                        //           }
                        //           );
                        //}
                        //else
                        //end remark 1 Feb 2019, tidak terpakai
                        {
                            var stf02h = ErasoftDbContext.STF02H.Where(b => b.BRG.Equals(barang.BRG) && b.IDMARKET == customer.RecNum).FirstOrDefault();
                            if (stf02h != null)
                            {
                                if (string.IsNullOrEmpty(stf02h.BRG_MP))//belum ada link dgn cust ini
                                {
                                    PromptModel.Add(
                                        new PromptBarangViewModel
                                        {
                                            KODE = barang.BRG,
                                            NAMA = barang.NAMA + " " + barang.NAMA2,
                                            HARGA = barang.HJUAL
                                        }
                                        );
                                }
                            }
                            else
                            {
                                PromptModel.Add(
                                    new PromptBarangViewModel
                                    {
                                        KODE = barang.BRG,
                                        NAMA = barang.NAMA + " " + barang.NAMA2,
                                        HARGA = barang.HJUAL
                                    }
                                );
                            }
                        }

                    }

                }
                retObj.data = PromptModel;
                retObj.typeBrg = typeBrg;
                return View("PromptBarang", retObj);
            }
            catch (Exception ex)
            {
                return JsonErrorMessage("Prompt gagal");
            }
        }
        [Route("manage/ImportDataMP")]
        public async Task<ActionResult> ImportDataMP(string cust, int page, int recordCount, int statBL)
        {
            if (!string.IsNullOrEmpty(cust))
            {
                try
                {
                    var arf01 = ErasoftDbContext.ARF01.Where(t => t.CUST.Equals(cust)).FirstOrDefault();
                    if (arf01 != null)
                    {
                        var marketplace = MoDbContext.Marketplaces.Where(m => m.IdMarket.ToString().Equals(arf01.NAMA)).FirstOrDefault();
                        if (marketplace != null)
                        {
                            var retBarang = new SyncBarangViewModel
                            {
                                Recursive = false,
                                Page = page + 1,
                                RecordCount = recordCount,
                                Stf02 = new STF02(),
                                TempBrg = new TEMP_BRG_MP(),
                                BLProductActive = statBL,
                            };
                            //int recordCount = 0;
                            switch (marketplace.NamaMarket.ToUpper())
                            {
                                case "LAZADA":
                                    if (string.IsNullOrEmpty(arf01.TOKEN))
                                    {
                                        return JsonErrorMessage("Anda belum link marketplace dengan Akun ini.\nSilahkan ikuti langkah-langkah untuk link Akun pada menu Pengaturan > Link > Link ke marketplace");
                                    }
                                    else
                                    {
                                        var lzdApi = new LazadaController();
                                        var resultLzd = lzdApi.GetBrgLazada(cust, arf01.TOKEN, page, recordCount);
                                        if (resultLzd.status == 1)
                                        {
                                            if (!string.IsNullOrEmpty(resultLzd.message))
                                            {
                                                retBarang.RecordCount = resultLzd.recordCount;
                                                retBarang.Recursive = true;
                                                //return Json(retBarang, JsonRequestBehavior.AllowGet);
                                            }
                                            else
                                            {
                                                retBarang.RecordCount = resultLzd.recordCount;
                                                //retBarang.ListTempBrg = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.CUST.ToUpper().Equals(cust.ToUpper())).ToList();
                                                //retBarang.ListMarket = ErasoftDbContext.ARF01.ToList();
                                                //return PartialView("TableUploadBarangPartial", retBarang);
                                            }
                                        }
                                        else
                                        {
                                            retBarang.RecordCount = resultLzd.recordCount;
                                            //retBarang.ListTempBrg = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.CUST.ToUpper().Equals(cust.ToUpper())).ToList();
                                            //retBarang.ListMarket = ErasoftDbContext.ARF01.ToList();
                                            //return PartialView("TableUploadBarangPartial", retBarang);
                                        }
                                        return Json(retBarang, JsonRequestBehavior.AllowGet);
                                        //var nextPageLzd = true;
                                        //while (nextPageLzd)
                                        //{
                                        //    if (resultLzd.status == 1)
                                        //    {
                                        //        if (!string.IsNullOrEmpty(resultLzd.message))
                                        //        {
                                        //            recordCount += resultLzd.recordCount;
                                        //            resultLzd = lzdApi.GetBrgLazada(cust, arf01.TOKEN, Convert.ToInt32(resultLzd.message), recordCount);
                                        //        }
                                        //        else
                                        //        {
                                        //            nextPageLzd = false;
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        nextPageLzd = false;
                                        //    }
                                        //}
                                    }
                                case "BUKALAPAK":
                                    var blApi = new BukaLapakController();
                                    if (string.IsNullOrEmpty(arf01.TOKEN))
                                    {
                                        return JsonErrorMessage("Anda belum link marketplace dengan Akun ini.\nSilahkan ikuti langkah-langkah untuk link Akun pada menu Pengaturan > Link > Link ke marketplace");
                                    }
                                    else
                                    {
                                        var result = blApi.getListProduct(cust, arf01.API_KEY, arf01.TOKEN, page + 1, (statBL == 1 ? true : false), recordCount);
                                        if (result.status == 1)
                                        {
                                            if (!string.IsNullOrEmpty(result.message))
                                            {
                                                if (result.message == "MOVE_TO_INACTIVE_PRODUCTS")//finish getting active product, move to inactive
                                                {
                                                    retBarang.BLProductActive = 0;
                                                    if (statBL == 1)
                                                        retBarang.Page = 0;
                                                }
                                                //else
                                                //{
                                                retBarang.RecordCount = result.recordCount;
                                                //}
                                                retBarang.Recursive = true;
                                                //return Json(retBarang, JsonRequestBehavior.AllowGet);
                                            }
                                            else
                                            {
                                                retBarang.RecordCount = result.recordCount;
                                                //retBarang.ListTempBrg = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.CUST.ToUpper().Equals(cust.ToUpper())).ToList();
                                                //retBarang.ListMarket = ErasoftDbContext.ARF01.ToList();
                                                //return PartialView("TableUploadBarangPartial", retBarang);
                                            }
                                        }
                                        else
                                        {
                                            retBarang.RecordCount = result.recordCount;
                                            //retBarang.ListTempBrg = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.CUST.ToUpper().Equals(cust.ToUpper())).ToList();
                                            //retBarang.ListMarket = ErasoftDbContext.ARF01.ToList();
                                            //return PartialView("TableUploadBarangPartial", retBarang);
                                        }
                                        return Json(retBarang, JsonRequestBehavior.AllowGet);
                                        //var nextPage = true;
                                        //while (nextPage)
                                        //{
                                        //    if (result.status == 1)
                                        //    {
                                        //        if (!string.IsNullOrEmpty(result.message))
                                        //        {
                                        //            result = blApi.getListProduct(cust, arf01.API_KEY, arf01.TOKEN, Convert.ToInt32(result.message), true);
                                        //        }
                                        //        else
                                        //        {
                                        //            nextPage = false;
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        nextPage = false;
                                        //    }
                                        //}

                                        //result = blApi.getListProduct(cust, arf01.API_KEY, arf01.TOKEN, 1, false);
                                        //nextPage = true;
                                        //while (nextPage)
                                        //{
                                        //    if (result.status == 1)
                                        //    {
                                        //        if (!string.IsNullOrEmpty(result.message))
                                        //        {
                                        //            result = blApi.getListProduct(cust, arf01.API_KEY, arf01.TOKEN, Convert.ToInt32(result.message), false);
                                        //        }
                                        //        else
                                        //        {
                                        //            nextPage = false;
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        nextPage = false;
                                        //    }
                                        //}
                                    }
                                case "BLIBLI":
                                    var BliApi = new BlibliController();
                                    if (string.IsNullOrEmpty(arf01.TOKEN))
                                    {
                                        return JsonErrorMessage("Anda belum link marketplace dengan Akun ini.\nSilahkan ikuti langkah-langkah untuk link Akun pada menu Pengaturan > Link > Link ke marketplace");
                                    }
                                    else
                                    {
                                        BlibliController.BlibliAPIData data = new BlibliController.BlibliAPIData()
                                        {
                                            API_client_username = arf01.API_CLIENT_U,
                                            API_client_password = arf01.API_CLIENT_P,
                                            API_secret_key = arf01.API_KEY,
                                            mta_username_email_merchant = arf01.EMAIL,
                                            mta_password_password_merchant = arf01.PASSWORD,
                                            merchant_code = arf01.Sort1_Cust,
                                            token = arf01.TOKEN,
                                            idmarket = arf01.RecNum.Value
                                        };
                                        var resultBli = BliApi.getProduct(data, "", page, arf01.CUST, recordCount);
                                        if (resultBli.status == 1)
                                        {
                                            if (!string.IsNullOrEmpty(resultBli.message))
                                            {
                                                retBarang.RecordCount = resultBli.recordCount;
                                                retBarang.Recursive = true;
                                                //return Json(retBarang, JsonRequestBehavior.AllowGet);
                                            }
                                            else
                                            {
                                                retBarang.RecordCount = resultBli.recordCount;
                                                //retBarang.ListTempBrg = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.CUST.ToUpper().Equals(cust.ToUpper())).ToList();
                                                //retBarang.ListMarket = ErasoftDbContext.ARF01.ToList();
                                                //return PartialView("TableUploadBarangPartial", retBarang);
                                            }
                                        }
                                        else
                                        {
                                            retBarang.RecordCount = resultBli.recordCount;
                                            //retBarang.ListTempBrg = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.CUST.ToUpper().Equals(cust.ToUpper())).ToList();
                                            //retBarang.ListMarket = ErasoftDbContext.ARF01.ToList();
                                            //return PartialView("TableUploadBarangPartial", retBarang);
                                        }
                                        return Json(retBarang, JsonRequestBehavior.AllowGet);

                                        //var nextPageBli = true;
                                        //while (nextPageBli)
                                        //{
                                        //    if (resultBli.status == 1)
                                        //    {
                                        //        if (!string.IsNullOrEmpty(resultBli.message))
                                        //        {
                                        //            resultBli = BliApi.getProduct(data, "", Convert.ToInt32(resultBli.message), arf01.CUST);
                                        //        }
                                        //        else
                                        //        {
                                        //            nextPageBli = false;
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        nextPageBli = false;
                                        //    }
                                        //}
                                    }

                                case "TOKOPEDIA":
                                    var TokoAPI = new TokopediaController();
                                    if (string.IsNullOrEmpty(arf01.Sort1_Cust))
                                    {
                                        return JsonErrorMessage("Anda belum link marketplace dengan Akun ini.\nSilahkan ikuti langkah-langkah untuk link Akun pada menu Pengaturan > Link > Link ke marketplace");
                                    }
                                    else
                                    {
                                        TokopediaController.TokopediaAPIData data = new TokopediaController.TokopediaAPIData()
                                        {
                                            merchant_code = arf01.Sort1_Cust, //FSID
                                            API_client_password = arf01.API_CLIENT_P, //Client ID
                                            API_client_username = arf01.API_CLIENT_U, //Client Secret
                                            API_secret_key = arf01.API_KEY, //Shop ID 
                                            token = arf01.TOKEN
                                        };

                                        //var resultShopee = await TokoAPI.GetActiveItemList(data, page, recordCount, arf01.CUST, arf01.NAMA, arf01.RecNum.Value);
                                        var resultShopee = await TokoAPI.GetItemListSemua(data, page, recordCount, arf01.CUST, arf01.NAMA, arf01.RecNum.Value);

                                        if (resultShopee.status == 1)
                                        {
                                            if (!string.IsNullOrEmpty(resultShopee.message))
                                            {
                                                retBarang.RecordCount = resultShopee.recordCount;
                                                retBarang.Recursive = true;
                                            }
                                            else
                                            {
                                                retBarang.RecordCount = resultShopee.recordCount;
                                            }
                                        }
                                        else
                                        {
                                            retBarang.RecordCount = resultShopee.recordCount;
                                        }
                                        return Json(retBarang, JsonRequestBehavior.AllowGet);
                                    }

                                case "SHOPEE":
                                    var ShopeeApi = new ShopeeController();
                                    if (string.IsNullOrEmpty(arf01.Sort1_Cust))
                                    {
                                        return JsonErrorMessage("Anda belum link marketplace dengan Akun ini.\nSilahkan ikuti langkah-langkah untuk link Akun pada menu Pengaturan > Link > Link ke marketplace");
                                    }
                                    else
                                    {
                                        ShopeeController.ShopeeAPIData data = new ShopeeController.ShopeeAPIData()
                                        {
                                            merchant_code = arf01.Sort1_Cust,

                                        };
                                        var resultShopee = await ShopeeApi.GetItemsList(data, arf01.RecNum.Value, page, recordCount);
                                        if (resultShopee.status == 1)
                                        {
                                            if (!string.IsNullOrEmpty(resultShopee.message))
                                            {
                                                retBarang.RecordCount = resultShopee.recordCount;
                                                retBarang.Recursive = true;
                                            }
                                            else
                                            {
                                                retBarang.RecordCount = resultShopee.recordCount;
                                            }
                                        }
                                        else
                                        {
                                            retBarang.RecordCount = resultShopee.recordCount;
                                        }
                                        return Json(retBarang, JsonRequestBehavior.AllowGet);
                                    }

                                default:
                                    return JsonErrorMessage("Fasilitas untuk mengambil data dari marketplace ini belum dibuka.");
                            }
                        }
                        else
                        {
                            return JsonErrorMessage("Toko tidak dapat ditemukan.");
                        }

                        //var barangVm = new UploadBarangViewModel()
                        //{
                        //    ListTempBrg = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.CUST.ToUpper().Equals(cust.ToUpper())).ToList(),
                        //    ListMarket = ErasoftDbContext.ARF01.ToList(),
                        //    Stf02 = new STF02(),
                        //    TempBrg = new TEMP_BRG_MP(),
                        //};

                        //return PartialView("TableUploadBarangPartial", barangVm);
                    }
                    else
                    {
                        return JsonErrorMessage("Toko tidak dapat ditemukan.");
                    }
                }
                catch (Exception ex)
                {
                    return JsonErrorMessage(ex.InnerException == null ? ex.Message : ex.InnerException.Message);
                }

            }
            else
            {
                return JsonErrorMessage("Anda belum memilih Toko");
            }
        }

        public ActionResult ProsesTempExcelAutoCompleteBrg(string cust)
        {
            List<string> listBrgSuccess = new List<string>();
            var dataBrg = ErasoftDbContext.Database.SqlQuery<TEMP_BRG_MP_EXCEL>("SELECT B.*, A.SELLER_SKU AS MO_SKU,A.MEREK AS MO_MEREK, A.CATEGORY AS MO_CATEGORY FROM TEMP_BRG_MP_EXCEL A INNER JOIN TEMP_BRG_MP B ON A.CUST = B.CUST AND A.BRG_MP = B.BRG_MP WHERE A.CUST='" + cust + "'").ToList();
            //var dataBrg = ErasoftDbContext.Database.SqlQuery<TEMP_BRG_MP_EXCEL>("SELECT B.*, A.SELLER_SKU AS MO_SKU,A.MEREK AS MO_MEREK, A.CATEGORY AS MO_CATEGORY FROM TEMP_BRG_MP_EXCEL A INNER JOIN TEMP_BRG_MP B ON A.CUST = B.CUST AND A.BRG_MP = B.BRG_MP WHERE A.CUST='000003' and A.SELLER_SKU IN(select b.seller_sku from stf02 a right join TEMP_BRG_MP_EXCEL b on a.brg = b.seller_sku where isnull(a.brg ,'') = '')").ToList();
            foreach (var data in dataBrg)
            {
                var barangInDB = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper().Equals(data.MO_SKU.ToUpper())).FirstOrDefault();
                if (barangInDB != null)
                {
                    var brgMp = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper().Equals(data.MO_SKU.ToUpper()) && b.IDMARKET == data.IDMARKET).FirstOrDefault();
                    if (brgMp != null)
                    {
                        if (!string.IsNullOrEmpty(brgMp.BRG_MP))
                        {
                            //return JsonErrorMessage("Barang ini sudah link dengan barang lain di marketplace");
                        }
                        else
                        {
                            brgMp.HJUAL = data.HJUAL;
                            brgMp.DISPLAY = data.DISPLAY;
                            brgMp.BRG_MP = data.BRG_MP;
                            brgMp.CATEGORY_CODE = data.CATEGORY_CODE;
                            brgMp.CATEGORY_NAME = data.CATEGORY_NAME;
                            brgMp.DeliveryTempElevenia = data.DeliveryTempElevenia;
                            brgMp.PICKUP_POINT = data.PICKUP_POINT;
                            #region attribute mp
                            brgMp.ACODE_1 = data.ACODE_1;
                            brgMp.ANAME_1 = data.ANAME_1;
                            brgMp.AVALUE_1 = data.AVALUE_1;
                            brgMp.ACODE_2 = data.ACODE_2;
                            brgMp.ANAME_2 = data.ANAME_2;
                            brgMp.AVALUE_2 = data.AVALUE_2;
                            brgMp.ACODE_3 = data.ACODE_3;
                            brgMp.ANAME_3 = data.ANAME_3;
                            brgMp.AVALUE_3 = data.AVALUE_3;
                            brgMp.ACODE_4 = data.ACODE_4;
                            brgMp.ANAME_4 = data.ANAME_4;
                            brgMp.AVALUE_4 = data.AVALUE_4;
                            brgMp.ACODE_5 = data.ACODE_5;
                            brgMp.ANAME_5 = data.ANAME_5;
                            brgMp.AVALUE_5 = data.AVALUE_5;
                            brgMp.ACODE_6 = data.ACODE_6;
                            brgMp.ANAME_6 = data.ANAME_6;
                            brgMp.AVALUE_6 = data.AVALUE_6;
                            brgMp.ACODE_7 = data.ACODE_7;
                            brgMp.ANAME_7 = data.ANAME_7;
                            brgMp.AVALUE_7 = data.AVALUE_7;
                            brgMp.ACODE_8 = data.ACODE_8;
                            brgMp.ANAME_8 = data.ANAME_8;
                            brgMp.AVALUE_8 = data.AVALUE_8;
                            brgMp.ACODE_9 = data.ACODE_9;
                            brgMp.ANAME_9 = data.ANAME_9;
                            brgMp.AVALUE_9 = data.AVALUE_9;
                            brgMp.ACODE_10 = data.ACODE_10;
                            brgMp.ANAME_10 = data.ANAME_10;
                            brgMp.AVALUE_10 = data.AVALUE_10;
                            brgMp.ACODE_11 = data.ACODE_11;
                            brgMp.ANAME_11 = data.ANAME_11;
                            brgMp.AVALUE_11 = data.AVALUE_11;
                            brgMp.ACODE_12 = data.ACODE_12;
                            brgMp.ANAME_12 = data.ANAME_12;
                            brgMp.AVALUE_12 = data.AVALUE_12;
                            brgMp.ACODE_13 = data.ACODE_13;
                            brgMp.ANAME_13 = data.ANAME_13;
                            brgMp.AVALUE_13 = data.AVALUE_13;
                            brgMp.ACODE_14 = data.ACODE_14;
                            brgMp.ANAME_14 = data.ANAME_14;
                            brgMp.AVALUE_14 = data.AVALUE_14;
                            brgMp.ACODE_15 = data.ACODE_15;
                            brgMp.ANAME_15 = data.ANAME_15;
                            brgMp.AVALUE_15 = data.AVALUE_15;
                            brgMp.ACODE_16 = data.ACODE_16;
                            brgMp.ANAME_16 = data.ANAME_16;
                            brgMp.AVALUE_16 = data.AVALUE_16;
                            brgMp.ACODE_17 = data.ACODE_17;
                            brgMp.ANAME_17 = data.ANAME_17;
                            brgMp.AVALUE_17 = data.AVALUE_17;
                            brgMp.ACODE_18 = data.ACODE_18;
                            brgMp.ANAME_18 = data.ANAME_18;
                            brgMp.AVALUE_18 = data.AVALUE_18;
                            brgMp.ACODE_19 = data.ACODE_19;
                            brgMp.ANAME_19 = data.ANAME_19;
                            brgMp.AVALUE_19 = data.AVALUE_19;
                            brgMp.ACODE_20 = data.ACODE_20;
                            brgMp.ANAME_20 = data.ANAME_20;
                            brgMp.AVALUE_20 = data.AVALUE_20;
                            brgMp.ACODE_21 = data.ACODE_21;
                            brgMp.ANAME_21 = data.ANAME_21;
                            brgMp.AVALUE_21 = data.AVALUE_21;
                            brgMp.ACODE_22 = data.ACODE_22;
                            brgMp.ANAME_22 = data.ANAME_22;
                            brgMp.AVALUE_22 = data.AVALUE_22;
                            brgMp.ACODE_23 = data.ACODE_23;
                            brgMp.ANAME_23 = data.ANAME_23;
                            brgMp.AVALUE_23 = data.AVALUE_23;
                            brgMp.ACODE_24 = data.ACODE_24;
                            brgMp.ANAME_24 = data.ANAME_24;
                            brgMp.AVALUE_24 = data.AVALUE_24;
                            brgMp.ACODE_25 = data.ACODE_25;
                            brgMp.ANAME_25 = data.ANAME_25;
                            brgMp.AVALUE_25 = data.AVALUE_25;
                            brgMp.ACODE_26 = data.ACODE_26;
                            brgMp.ANAME_26 = data.ANAME_26;
                            brgMp.AVALUE_26 = data.AVALUE_26;
                            brgMp.ACODE_27 = data.ACODE_27;
                            brgMp.ANAME_27 = data.ANAME_27;
                            brgMp.AVALUE_27 = data.AVALUE_27;
                            brgMp.ACODE_28 = data.ACODE_28;
                            brgMp.ANAME_28 = data.ANAME_28;
                            brgMp.AVALUE_28 = data.AVALUE_28;
                            brgMp.ACODE_29 = data.ACODE_29;
                            brgMp.ANAME_29 = data.ANAME_29;
                            brgMp.AVALUE_29 = data.AVALUE_29;
                            brgMp.ACODE_30 = data.ACODE_30;
                            brgMp.ANAME_30 = data.ANAME_30;
                            brgMp.AVALUE_30 = data.AVALUE_30;
                            brgMp.ACODE_31 = data.ACODE_31;
                            brgMp.ANAME_31 = data.ANAME_31;
                            brgMp.AVALUE_31 = data.AVALUE_31;
                            brgMp.ACODE_32 = data.ACODE_32;
                            brgMp.ANAME_32 = data.ANAME_32;
                            brgMp.AVALUE_32 = data.AVALUE_32;
                            brgMp.ACODE_33 = data.ACODE_33;
                            brgMp.ANAME_33 = data.ANAME_33;
                            brgMp.AVALUE_33 = data.AVALUE_33;
                            brgMp.ACODE_34 = data.ACODE_34;
                            brgMp.ANAME_34 = data.ANAME_34;
                            brgMp.AVALUE_34 = data.AVALUE_34;
                            brgMp.ACODE_35 = data.ACODE_35;
                            brgMp.ANAME_35 = data.ANAME_35;
                            brgMp.AVALUE_35 = data.AVALUE_35;
                            brgMp.ACODE_36 = data.ACODE_36;
                            brgMp.ANAME_36 = data.ANAME_36;
                            brgMp.AVALUE_36 = data.AVALUE_36;
                            brgMp.ACODE_37 = data.ACODE_37;
                            brgMp.ANAME_37 = data.ANAME_37;
                            brgMp.AVALUE_37 = data.AVALUE_37;
                            brgMp.ACODE_38 = data.ACODE_38;
                            brgMp.ANAME_38 = data.ANAME_38;
                            brgMp.AVALUE_38 = data.AVALUE_38;
                            brgMp.ACODE_39 = data.ACODE_39;
                            brgMp.ANAME_39 = data.ANAME_39;
                            brgMp.AVALUE_39 = data.AVALUE_39;
                            brgMp.ACODE_40 = data.ACODE_40;
                            brgMp.ANAME_40 = data.ANAME_40;
                            brgMp.AVALUE_40 = data.AVALUE_40;
                            brgMp.ACODE_41 = data.ACODE_41;
                            brgMp.ANAME_41 = data.ANAME_41;
                            brgMp.AVALUE_41 = data.AVALUE_41;
                            brgMp.ACODE_42 = data.ACODE_42;
                            brgMp.ANAME_42 = data.ANAME_42;
                            brgMp.AVALUE_42 = data.AVALUE_42;
                            brgMp.ACODE_43 = data.ACODE_43;
                            brgMp.ANAME_43 = data.ANAME_43;
                            brgMp.AVALUE_43 = data.AVALUE_43;
                            brgMp.ACODE_44 = data.ACODE_44;
                            brgMp.ANAME_44 = data.ANAME_44;
                            brgMp.AVALUE_44 = data.AVALUE_44;
                            brgMp.ACODE_45 = data.ACODE_45;
                            brgMp.ANAME_45 = data.ANAME_45;
                            brgMp.AVALUE_45 = data.AVALUE_45;
                            brgMp.ACODE_46 = data.ACODE_46;
                            brgMp.ANAME_46 = data.ANAME_46;
                            brgMp.AVALUE_46 = data.AVALUE_46;
                            brgMp.ACODE_47 = data.ACODE_47;
                            brgMp.ANAME_47 = data.ANAME_47;
                            brgMp.AVALUE_47 = data.AVALUE_47;
                            brgMp.ACODE_48 = data.ACODE_48;
                            brgMp.ANAME_48 = data.ANAME_48;
                            brgMp.AVALUE_48 = data.AVALUE_48;
                            brgMp.ACODE_49 = data.ACODE_49;
                            brgMp.ANAME_49 = data.ANAME_49;
                            brgMp.AVALUE_49 = data.AVALUE_49;
                            brgMp.ACODE_50 = data.ACODE_50;
                            brgMp.ANAME_50 = data.ANAME_50;
                            brgMp.AVALUE_50 = data.AVALUE_50;
                            #endregion
                            ErasoftDbContext.SaveChanges();

                            listBrgSuccess.Add(data.BRG_MP);
                        }
                    }
                    else
                    {
                        brgMp = new STF02H();
                        brgMp.BRG = data.MO_SKU;
                        brgMp.BRG_MP = data.BRG_MP;
                        brgMp.HJUAL = data.HJUAL;
                        brgMp.DISPLAY = data.DISPLAY;
                        brgMp.CATEGORY_CODE = data.CATEGORY_CODE;
                        brgMp.CATEGORY_NAME = data.CATEGORY_NAME;
                        brgMp.IDMARKET = data.IDMARKET;
                        brgMp.DeliveryTempElevenia = data.DeliveryTempElevenia;
                        brgMp.PICKUP_POINT = data.PICKUP_POINT;
                        var customer = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper().Equals(data.CUST.ToUpper())).FirstOrDefault();
                        if (customer != null)
                            brgMp.AKUNMARKET = customer.PERSO;
                        //brgMp.USERNAME = "SYSTEM_UPLOAD_BRG";
                        brgMp.USERNAME = "EXCEL_SYNC_ITEM";
                        #region attribute mp
                        brgMp.ACODE_1 = data.ACODE_1;
                        brgMp.ANAME_1 = data.ANAME_1;
                        brgMp.AVALUE_1 = data.AVALUE_1;
                        brgMp.ACODE_2 = data.ACODE_2;
                        brgMp.ANAME_2 = data.ANAME_2;
                        brgMp.AVALUE_2 = data.AVALUE_2;
                        brgMp.ACODE_3 = data.ACODE_3;
                        brgMp.ANAME_3 = data.ANAME_3;
                        brgMp.AVALUE_3 = data.AVALUE_3;
                        brgMp.ACODE_4 = data.ACODE_4;
                        brgMp.ANAME_4 = data.ANAME_4;
                        brgMp.AVALUE_4 = data.AVALUE_4;
                        brgMp.ACODE_5 = data.ACODE_5;
                        brgMp.ANAME_5 = data.ANAME_5;
                        brgMp.AVALUE_5 = data.AVALUE_5;
                        brgMp.ACODE_6 = data.ACODE_6;
                        brgMp.ANAME_6 = data.ANAME_6;
                        brgMp.AVALUE_6 = data.AVALUE_6;
                        brgMp.ACODE_7 = data.ACODE_7;
                        brgMp.ANAME_7 = data.ANAME_7;
                        brgMp.AVALUE_7 = data.AVALUE_7;
                        brgMp.ACODE_8 = data.ACODE_8;
                        brgMp.ANAME_8 = data.ANAME_8;
                        brgMp.AVALUE_8 = data.AVALUE_8;
                        brgMp.ACODE_9 = data.ACODE_9;
                        brgMp.ANAME_9 = data.ANAME_9;
                        brgMp.AVALUE_9 = data.AVALUE_9;
                        brgMp.ACODE_10 = data.ACODE_10;
                        brgMp.ANAME_10 = data.ANAME_10;
                        brgMp.AVALUE_10 = data.AVALUE_10;
                        brgMp.ACODE_11 = data.ACODE_11;
                        brgMp.ANAME_11 = data.ANAME_11;
                        brgMp.AVALUE_11 = data.AVALUE_11;
                        brgMp.ACODE_12 = data.ACODE_12;
                        brgMp.ANAME_12 = data.ANAME_12;
                        brgMp.AVALUE_12 = data.AVALUE_12;
                        brgMp.ACODE_13 = data.ACODE_13;
                        brgMp.ANAME_13 = data.ANAME_13;
                        brgMp.AVALUE_13 = data.AVALUE_13;
                        brgMp.ACODE_14 = data.ACODE_14;
                        brgMp.ANAME_14 = data.ANAME_14;
                        brgMp.AVALUE_14 = data.AVALUE_14;
                        brgMp.ACODE_15 = data.ACODE_15;
                        brgMp.ANAME_15 = data.ANAME_15;
                        brgMp.AVALUE_15 = data.AVALUE_15;
                        brgMp.ACODE_16 = data.ACODE_16;
                        brgMp.ANAME_16 = data.ANAME_16;
                        brgMp.AVALUE_16 = data.AVALUE_16;
                        brgMp.ACODE_17 = data.ACODE_17;
                        brgMp.ANAME_17 = data.ANAME_17;
                        brgMp.AVALUE_17 = data.AVALUE_17;
                        brgMp.ACODE_18 = data.ACODE_18;
                        brgMp.ANAME_18 = data.ANAME_18;
                        brgMp.AVALUE_18 = data.AVALUE_18;
                        brgMp.ACODE_19 = data.ACODE_19;
                        brgMp.ANAME_19 = data.ANAME_19;
                        brgMp.AVALUE_19 = data.AVALUE_19;
                        brgMp.ACODE_20 = data.ACODE_20;
                        brgMp.ANAME_20 = data.ANAME_20;
                        brgMp.AVALUE_20 = data.AVALUE_20;
                        brgMp.ACODE_21 = data.ACODE_21;
                        brgMp.ANAME_21 = data.ANAME_21;
                        brgMp.AVALUE_21 = data.AVALUE_21;
                        brgMp.ACODE_22 = data.ACODE_22;
                        brgMp.ANAME_22 = data.ANAME_22;
                        brgMp.AVALUE_22 = data.AVALUE_22;
                        brgMp.ACODE_23 = data.ACODE_23;
                        brgMp.ANAME_23 = data.ANAME_23;
                        brgMp.AVALUE_23 = data.AVALUE_23;
                        brgMp.ACODE_24 = data.ACODE_24;
                        brgMp.ANAME_24 = data.ANAME_24;
                        brgMp.AVALUE_24 = data.AVALUE_24;
                        brgMp.ACODE_25 = data.ACODE_25;
                        brgMp.ANAME_25 = data.ANAME_25;
                        brgMp.AVALUE_25 = data.AVALUE_25;
                        brgMp.ACODE_26 = data.ACODE_26;
                        brgMp.ANAME_26 = data.ANAME_26;
                        brgMp.AVALUE_26 = data.AVALUE_26;
                        brgMp.ACODE_27 = data.ACODE_27;
                        brgMp.ANAME_27 = data.ANAME_27;
                        brgMp.AVALUE_27 = data.AVALUE_27;
                        brgMp.ACODE_28 = data.ACODE_28;
                        brgMp.ANAME_28 = data.ANAME_28;
                        brgMp.AVALUE_28 = data.AVALUE_28;
                        brgMp.ACODE_29 = data.ACODE_29;
                        brgMp.ANAME_29 = data.ANAME_29;
                        brgMp.AVALUE_29 = data.AVALUE_29;
                        brgMp.ACODE_30 = data.ACODE_30;
                        brgMp.ANAME_30 = data.ANAME_30;
                        brgMp.AVALUE_30 = data.AVALUE_30;
                        brgMp.ACODE_31 = data.ACODE_31;
                        brgMp.ANAME_31 = data.ANAME_31;
                        brgMp.AVALUE_31 = data.AVALUE_31;
                        brgMp.ACODE_32 = data.ACODE_32;
                        brgMp.ANAME_32 = data.ANAME_32;
                        brgMp.AVALUE_32 = data.AVALUE_32;
                        brgMp.ACODE_33 = data.ACODE_33;
                        brgMp.ANAME_33 = data.ANAME_33;
                        brgMp.AVALUE_33 = data.AVALUE_33;
                        brgMp.ACODE_34 = data.ACODE_34;
                        brgMp.ANAME_34 = data.ANAME_34;
                        brgMp.AVALUE_34 = data.AVALUE_34;
                        brgMp.ACODE_35 = data.ACODE_35;
                        brgMp.ANAME_35 = data.ANAME_35;
                        brgMp.AVALUE_35 = data.AVALUE_35;
                        brgMp.ACODE_36 = data.ACODE_36;
                        brgMp.ANAME_36 = data.ANAME_36;
                        brgMp.AVALUE_36 = data.AVALUE_36;
                        brgMp.ACODE_37 = data.ACODE_37;
                        brgMp.ANAME_37 = data.ANAME_37;
                        brgMp.AVALUE_37 = data.AVALUE_37;
                        brgMp.ACODE_38 = data.ACODE_38;
                        brgMp.ANAME_38 = data.ANAME_38;
                        brgMp.AVALUE_38 = data.AVALUE_38;
                        brgMp.ACODE_39 = data.ACODE_39;
                        brgMp.ANAME_39 = data.ANAME_39;
                        brgMp.AVALUE_39 = data.AVALUE_39;
                        brgMp.ACODE_40 = data.ACODE_40;
                        brgMp.ANAME_40 = data.ANAME_40;
                        brgMp.AVALUE_40 = data.AVALUE_40;
                        brgMp.ACODE_41 = data.ACODE_41;
                        brgMp.ANAME_41 = data.ANAME_41;
                        brgMp.AVALUE_41 = data.AVALUE_41;
                        brgMp.ACODE_42 = data.ACODE_42;
                        brgMp.ANAME_42 = data.ANAME_42;
                        brgMp.AVALUE_42 = data.AVALUE_42;
                        brgMp.ACODE_43 = data.ACODE_43;
                        brgMp.ANAME_43 = data.ANAME_43;
                        brgMp.AVALUE_43 = data.AVALUE_43;
                        brgMp.ACODE_44 = data.ACODE_44;
                        brgMp.ANAME_44 = data.ANAME_44;
                        brgMp.AVALUE_44 = data.AVALUE_44;
                        brgMp.ACODE_45 = data.ACODE_45;
                        brgMp.ANAME_45 = data.ANAME_45;
                        brgMp.AVALUE_45 = data.AVALUE_45;
                        brgMp.ACODE_46 = data.ACODE_46;
                        brgMp.ANAME_46 = data.ANAME_46;
                        brgMp.AVALUE_46 = data.AVALUE_46;
                        brgMp.ACODE_47 = data.ACODE_47;
                        brgMp.ANAME_47 = data.ANAME_47;
                        brgMp.AVALUE_47 = data.AVALUE_47;
                        brgMp.ACODE_48 = data.ACODE_48;
                        brgMp.ANAME_48 = data.ANAME_48;
                        brgMp.AVALUE_48 = data.AVALUE_48;
                        brgMp.ACODE_49 = data.ACODE_49;
                        brgMp.ANAME_49 = data.ANAME_49;
                        brgMp.AVALUE_49 = data.AVALUE_49;
                        brgMp.ACODE_50 = data.ACODE_50;
                        brgMp.ANAME_50 = data.ANAME_50;
                        brgMp.AVALUE_50 = data.AVALUE_50;
                        #endregion
                        ErasoftDbContext.STF02H.Add(brgMp);
                        ErasoftDbContext.SaveChanges();

                        listBrgSuccess.Add(data.BRG_MP);
                    }
                }
                else
                {
                    try
                    {

                        var data_Stf02 = new STF02
                        {
                            HPP = 0,
                            HBELI = 0,
                            HBESAR = 0,
                            HKECIL = 0,
                            TYPE = "3",
                            KLINK = "1",
                            HP_STD = 0,
                            QPROD = 0,
                            ISI3 = 3,
                            ISI4 = 1,
                            TOLERANSI = 0,
                            H_STN_3 = 0,
                            H_STN_4 = 0,
                            SS = 0,
                            METODA_HPP_PER_SN = false,
                            HNA_PPN = 0,
                            LABA = 0,
                            DEFAULT_STN_HRG_JUAL = 0,
                            DEFAULT_STN_JUAL = 0,
                            ISI = 1,
                            Metoda = "1",
                            Tgl_Input = DateTime.Now,
                            TGL_KLR = DateTime.Now,
                            MAXI = 100,
                            MINI = 1,
                            QSALES = 0,
                            DISPLAY_MARKET = false,
                        };
                        data_Stf02.BRG = data.MO_SKU;

                        data_Stf02.NAMA = data.NAMA;
                        data_Stf02.NAMA2 = data.NAMA2;
                        data_Stf02.NAMA3 = data.NAMA3;
                        data_Stf02.HJUAL = data.HJUAL;
                        data_Stf02.STN = "pcs";
                        data_Stf02.STN2 = "pcs";
                        data_Stf02.BERAT = data.BERAT;
                        data_Stf02.TINGGI = data.TINGGI;
                        data_Stf02.LEBAR = data.LEBAR;
                        data_Stf02.PANJANG = data.PANJANG;
                        data_Stf02.Sort1 = data.MO_CATEGORY;
                        data_Stf02.Sort1 = "1";
                        //data_Stf02.Sort2 = data.MO_MEREK;
                        data_Stf02.Sort2 = "HB";
                        //var customer = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper().Equals(data.CUST.ToUpper())).FirstOrDefault();
                        //if (customer != null)
                        //{
                        //    stf02.KET_SORT1 = defaultCategoryCode.KET;
                        //    stf02.KET_SORT2 = defaultBrand.KET;
                        //}

                        data_Stf02.Deskripsi = HttpUtility.HtmlDecode(data.Deskripsi);
                        if (data_Stf02.Deskripsi == "")
                        {
                            data_Stf02.Deskripsi = "-";
                        }
                        if (!string.IsNullOrEmpty(data.IMAGE))
                        {
                            data_Stf02.LINK_GAMBAR_1 = UploadImageService.UploadSingleImageToImgurFromUrl(data.IMAGE, "uploaded-image").data.link_l;
                        }
                        if (!string.IsNullOrEmpty(data.IMAGE2))
                        {
                            data_Stf02.LINK_GAMBAR_2 = UploadImageService.UploadSingleImageToImgurFromUrl(data.IMAGE2, "uploaded-image").data.link_l;
                        }
                        if (!string.IsNullOrEmpty(data.IMAGE3))
                        {
                            data_Stf02.LINK_GAMBAR_3 = UploadImageService.UploadSingleImageToImgurFromUrl(data.IMAGE3, "uploaded-image").data.link_l;
                        }
                        ErasoftDbContext.STF02.Add(data_Stf02);

                        var brgMp = new STF02H();
                        brgMp.BRG = data.MO_SKU;
                        brgMp.BRG_MP = data.BRG_MP;
                        brgMp.HJUAL = data.HJUAL;
                        brgMp.DISPLAY = data.DISPLAY;
                        brgMp.CATEGORY_CODE = data.CATEGORY_CODE;
                        brgMp.CATEGORY_NAME = data.CATEGORY_NAME;
                        brgMp.IDMARKET = data.IDMARKET;
                        brgMp.DeliveryTempElevenia = data.DeliveryTempElevenia;
                        brgMp.PICKUP_POINT = data.PICKUP_POINT;
                        var customer = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper().Equals(data.CUST.ToUpper())).FirstOrDefault();
                        if (customer != null)
                            brgMp.AKUNMARKET = customer.PERSO;
                        //brgMp.USERNAME = "SYSTEM_UPLOAD_BRG";
                        brgMp.USERNAME = "EXCEL_SYNC_ITEM";
                        #region attribute mp
                        brgMp.ACODE_1 = data.ACODE_1;
                        brgMp.ANAME_1 = data.ANAME_1;
                        brgMp.AVALUE_1 = data.AVALUE_1;
                        brgMp.ACODE_2 = data.ACODE_2;
                        brgMp.ANAME_2 = data.ANAME_2;
                        brgMp.AVALUE_2 = data.AVALUE_2;
                        brgMp.ACODE_3 = data.ACODE_3;
                        brgMp.ANAME_3 = data.ANAME_3;
                        brgMp.AVALUE_3 = data.AVALUE_3;
                        brgMp.ACODE_4 = data.ACODE_4;
                        brgMp.ANAME_4 = data.ANAME_4;
                        brgMp.AVALUE_4 = data.AVALUE_4;
                        brgMp.ACODE_5 = data.ACODE_5;
                        brgMp.ANAME_5 = data.ANAME_5;
                        brgMp.AVALUE_5 = data.AVALUE_5;
                        brgMp.ACODE_6 = data.ACODE_6;
                        brgMp.ANAME_6 = data.ANAME_6;
                        brgMp.AVALUE_6 = data.AVALUE_6;
                        brgMp.ACODE_7 = data.ACODE_7;
                        brgMp.ANAME_7 = data.ANAME_7;
                        brgMp.AVALUE_7 = data.AVALUE_7;
                        brgMp.ACODE_8 = data.ACODE_8;
                        brgMp.ANAME_8 = data.ANAME_8;
                        brgMp.AVALUE_8 = data.AVALUE_8;
                        brgMp.ACODE_9 = data.ACODE_9;
                        brgMp.ANAME_9 = data.ANAME_9;
                        brgMp.AVALUE_9 = data.AVALUE_9;
                        brgMp.ACODE_10 = data.ACODE_10;
                        brgMp.ANAME_10 = data.ANAME_10;
                        brgMp.AVALUE_10 = data.AVALUE_10;
                        brgMp.ACODE_11 = data.ACODE_11;
                        brgMp.ANAME_11 = data.ANAME_11;
                        brgMp.AVALUE_11 = data.AVALUE_11;
                        brgMp.ACODE_12 = data.ACODE_12;
                        brgMp.ANAME_12 = data.ANAME_12;
                        brgMp.AVALUE_12 = data.AVALUE_12;
                        brgMp.ACODE_13 = data.ACODE_13;
                        brgMp.ANAME_13 = data.ANAME_13;
                        brgMp.AVALUE_13 = data.AVALUE_13;
                        brgMp.ACODE_14 = data.ACODE_14;
                        brgMp.ANAME_14 = data.ANAME_14;
                        brgMp.AVALUE_14 = data.AVALUE_14;
                        brgMp.ACODE_15 = data.ACODE_15;
                        brgMp.ANAME_15 = data.ANAME_15;
                        brgMp.AVALUE_15 = data.AVALUE_15;
                        brgMp.ACODE_16 = data.ACODE_16;
                        brgMp.ANAME_16 = data.ANAME_16;
                        brgMp.AVALUE_16 = data.AVALUE_16;
                        brgMp.ACODE_17 = data.ACODE_17;
                        brgMp.ANAME_17 = data.ANAME_17;
                        brgMp.AVALUE_17 = data.AVALUE_17;
                        brgMp.ACODE_18 = data.ACODE_18;
                        brgMp.ANAME_18 = data.ANAME_18;
                        brgMp.AVALUE_18 = data.AVALUE_18;
                        brgMp.ACODE_19 = data.ACODE_19;
                        brgMp.ANAME_19 = data.ANAME_19;
                        brgMp.AVALUE_19 = data.AVALUE_19;
                        brgMp.ACODE_20 = data.ACODE_20;
                        brgMp.ANAME_20 = data.ANAME_20;
                        brgMp.AVALUE_20 = data.AVALUE_20;
                        brgMp.ACODE_21 = data.ACODE_21;
                        brgMp.ANAME_21 = data.ANAME_21;
                        brgMp.AVALUE_21 = data.AVALUE_21;
                        brgMp.ACODE_22 = data.ACODE_22;
                        brgMp.ANAME_22 = data.ANAME_22;
                        brgMp.AVALUE_22 = data.AVALUE_22;
                        brgMp.ACODE_23 = data.ACODE_23;
                        brgMp.ANAME_23 = data.ANAME_23;
                        brgMp.AVALUE_23 = data.AVALUE_23;
                        brgMp.ACODE_24 = data.ACODE_24;
                        brgMp.ANAME_24 = data.ANAME_24;
                        brgMp.AVALUE_24 = data.AVALUE_24;
                        brgMp.ACODE_25 = data.ACODE_25;
                        brgMp.ANAME_25 = data.ANAME_25;
                        brgMp.AVALUE_25 = data.AVALUE_25;
                        brgMp.ACODE_26 = data.ACODE_26;
                        brgMp.ANAME_26 = data.ANAME_26;
                        brgMp.AVALUE_26 = data.AVALUE_26;
                        brgMp.ACODE_27 = data.ACODE_27;
                        brgMp.ANAME_27 = data.ANAME_27;
                        brgMp.AVALUE_27 = data.AVALUE_27;
                        brgMp.ACODE_28 = data.ACODE_28;
                        brgMp.ANAME_28 = data.ANAME_28;
                        brgMp.AVALUE_28 = data.AVALUE_28;
                        brgMp.ACODE_29 = data.ACODE_29;
                        brgMp.ANAME_29 = data.ANAME_29;
                        brgMp.AVALUE_29 = data.AVALUE_29;
                        brgMp.ACODE_30 = data.ACODE_30;
                        brgMp.ANAME_30 = data.ANAME_30;
                        brgMp.AVALUE_30 = data.AVALUE_30;
                        brgMp.ACODE_31 = data.ACODE_31;
                        brgMp.ANAME_31 = data.ANAME_31;
                        brgMp.AVALUE_31 = data.AVALUE_31;
                        brgMp.ACODE_32 = data.ACODE_32;
                        brgMp.ANAME_32 = data.ANAME_32;
                        brgMp.AVALUE_32 = data.AVALUE_32;
                        brgMp.ACODE_33 = data.ACODE_33;
                        brgMp.ANAME_33 = data.ANAME_33;
                        brgMp.AVALUE_33 = data.AVALUE_33;
                        brgMp.ACODE_34 = data.ACODE_34;
                        brgMp.ANAME_34 = data.ANAME_34;
                        brgMp.AVALUE_34 = data.AVALUE_34;
                        brgMp.ACODE_35 = data.ACODE_35;
                        brgMp.ANAME_35 = data.ANAME_35;
                        brgMp.AVALUE_35 = data.AVALUE_35;
                        brgMp.ACODE_36 = data.ACODE_36;
                        brgMp.ANAME_36 = data.ANAME_36;
                        brgMp.AVALUE_36 = data.AVALUE_36;
                        brgMp.ACODE_37 = data.ACODE_37;
                        brgMp.ANAME_37 = data.ANAME_37;
                        brgMp.AVALUE_37 = data.AVALUE_37;
                        brgMp.ACODE_38 = data.ACODE_38;
                        brgMp.ANAME_38 = data.ANAME_38;
                        brgMp.AVALUE_38 = data.AVALUE_38;
                        brgMp.ACODE_39 = data.ACODE_39;
                        brgMp.ANAME_39 = data.ANAME_39;
                        brgMp.AVALUE_39 = data.AVALUE_39;
                        brgMp.ACODE_40 = data.ACODE_40;
                        brgMp.ANAME_40 = data.ANAME_40;
                        brgMp.AVALUE_40 = data.AVALUE_40;
                        brgMp.ACODE_41 = data.ACODE_41;
                        brgMp.ANAME_41 = data.ANAME_41;
                        brgMp.AVALUE_41 = data.AVALUE_41;
                        brgMp.ACODE_42 = data.ACODE_42;
                        brgMp.ANAME_42 = data.ANAME_42;
                        brgMp.AVALUE_42 = data.AVALUE_42;
                        brgMp.ACODE_43 = data.ACODE_43;
                        brgMp.ANAME_43 = data.ANAME_43;
                        brgMp.AVALUE_43 = data.AVALUE_43;
                        brgMp.ACODE_44 = data.ACODE_44;
                        brgMp.ANAME_44 = data.ANAME_44;
                        brgMp.AVALUE_44 = data.AVALUE_44;
                        brgMp.ACODE_45 = data.ACODE_45;
                        brgMp.ANAME_45 = data.ANAME_45;
                        brgMp.AVALUE_45 = data.AVALUE_45;
                        brgMp.ACODE_46 = data.ACODE_46;
                        brgMp.ANAME_46 = data.ANAME_46;
                        brgMp.AVALUE_46 = data.AVALUE_46;
                        brgMp.ACODE_47 = data.ACODE_47;
                        brgMp.ANAME_47 = data.ANAME_47;
                        brgMp.AVALUE_47 = data.AVALUE_47;
                        brgMp.ACODE_48 = data.ACODE_48;
                        brgMp.ANAME_48 = data.ANAME_48;
                        brgMp.AVALUE_48 = data.AVALUE_48;
                        brgMp.ACODE_49 = data.ACODE_49;
                        brgMp.ANAME_49 = data.ANAME_49;
                        brgMp.AVALUE_49 = data.AVALUE_49;
                        brgMp.ACODE_50 = data.ACODE_50;
                        brgMp.ANAME_50 = data.ANAME_50;
                        brgMp.AVALUE_50 = data.AVALUE_50;
                        #endregion
                        ErasoftDbContext.STF02H.Add(brgMp);
                        ErasoftDbContext.SaveChanges();

                        listBrgSuccess.Add(data.BRG_MP);
                    }
                    catch (DbEntityValidationException e)
                    {
                        foreach (var eve in e.EntityValidationErrors)
                        {
                            Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                eve.Entry.Entity.GetType().Name, eve.Entry.State);
                            foreach (var ve in eve.ValidationErrors)
                            {
                                Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                    ve.PropertyName, ve.ErrorMessage);
                            }
                        }
                        throw;
                    }
                }
            }

            if (listBrgSuccess.Count > 0)
            {
                foreach (var brg_mp in listBrgSuccess)
                {
                    ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.Equals(brg_mp)).Delete();
                }
                ErasoftDbContext.SaveChanges();
            }
            return JsonErrorMessage("Akun tidak ada");
        }

        public ActionResult AutoCompleteBrg(string brg, string brg_mp, string cust)
        {
            //var barangVm = new UploadBarangViewModel()
            //{
            //    ListTempBrg = ErasoftDbContext.TEMP_BRG_MP.ToList(),
            //    ListMarket = ErasoftDbContext.ARF01.ToList(),
            //    //Stf02 = new STF02(),
            //    TempBrg = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.Equals(brg_mp)).FirstOrDefault(),
            //    ListKategoriMerk = ErasoftDbContext.STF02E.Where(m => m.LEVEL.Equals("2")).OrderBy(m => m.KET).ToList(),
            //    ListKategoriBrg = ErasoftDbContext.STF02E.Where(m => m.LEVEL.Equals("1")).OrderBy(m => m.KET).ToList(),

            //};

            var retBarang = new STF02();
            if (!string.IsNullOrEmpty(brg))
            {
                retBarang = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper().Equals(brg.ToUpper())).FirstOrDefault();
                if (retBarang != null)
                {
                    if (!string.IsNullOrEmpty(retBarang.PART))
                    {
                        var brg_induk = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper().Equals(retBarang.PART.ToUpper())).FirstOrDefault();
                        if (brg_induk != null)
                        {
                            retBarang.Sort1 = brg_induk.Sort1;
                            retBarang.Sort2 = brg_induk.Sort2;
                            retBarang.KET_SORT1 = brg_induk.KET_SORT1;
                            retBarang.KET_SORT2 = brg_induk.KET_SORT2;
                        }
                    }
                    return Json(retBarang, JsonRequestBehavior.AllowGet);
                    //barangVm.Stf02 = retBarang;
                    //return PartialView("FormBarangUploadsPartial", barangVm);

                }
                else
                {
                    var tempBrg = ErasoftDbContext.TEMP_BRG_MP.Where(b => b.BRG_MP.ToUpper().Equals(brg_mp.ToUpper()) && b.CUST == cust).FirstOrDefault();
                    if (tempBrg != null)
                    {
                        retBarang = new STF02();
                        //retBarang = ErasoftDbContext.STF02.FirstOrDefault();
                        retBarang.NAMA = tempBrg.NAMA;
                        retBarang.NAMA2 = tempBrg.NAMA2;
                        retBarang.BERAT = tempBrg.BERAT;
                        retBarang.PANJANG = tempBrg.PANJANG;
                        retBarang.LEBAR = tempBrg.LEBAR;
                        retBarang.TINGGI = tempBrg.TINGGI;
                        retBarang.HJUAL = tempBrg.HJUAL;
                        retBarang.STN2 = "pcs";
                        retBarang.MINI = 1;
                        retBarang.MAXI = 100;
                        retBarang.Deskripsi = tempBrg.Deskripsi;
                        retBarang.BRG = brg;
                        retBarang.LINK_GAMBAR_1 = tempBrg.IMAGE;
                        retBarang.LINK_GAMBAR_2 = tempBrg.IMAGE2;
                        retBarang.LINK_GAMBAR_3 = tempBrg.IMAGE3;

                        if (!string.IsNullOrEmpty(tempBrg.KODE_BRG_INDUK))
                        {
                            var brg_induk = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper().Equals(tempBrg.KODE_BRG_INDUK.ToUpper())).FirstOrDefault();
                            if (brg_induk != null)
                            {
                                retBarang.Sort1 = brg_induk.Sort1;
                                retBarang.Sort2 = brg_induk.Sort2;
                                retBarang.KET_SORT1 = brg_induk.KET_SORT1;
                                retBarang.KET_SORT2 = brg_induk.KET_SORT2;
                            }
                        }

                        return Json(retBarang, JsonRequestBehavior.AllowGet);
                        //barangVm.Stf02 = retBarang;
                        //return PartialView("FormBarangUploadsPartial", barangVm);

                    }
                    else
                    {
                        return JsonErrorMessage("Barang ini sudah disinkronisasi. Silahkan refresh halaman ini.");
                    }
                }
            }
            else
            {
                return JsonErrorMessage("Kode Barang tidak ditemukan.");

            }
        }

        public ActionResult CreateSTF02HTokped(string cust)
        {
            if (!string.IsNullOrEmpty(cust))
            {
                var marketplace = ErasoftDbContext.ARF01.Where(c => c.CUST == cust).FirstOrDefault();
                var tokped = MoDbContext.Marketplaces.Where(m => m.NamaMarket.ToUpper() == "TOKOPEDIA").FirstOrDefault();
                if (marketplace != null && tokped != null)
                {
                    if (marketplace.NAMA == tokped.IdMarket.ToString())
                    {
                        SqlCommand CommandSQL = new SqlCommand();
                        CommandSQL.Parameters.Add("@idmarket", SqlDbType.Int).Value = marketplace.RecNum;
                        CommandSQL.Parameters.Add("@username", SqlDbType.NVarChar, 30).Value = "AUTO_CREATE_SP";
                        CommandSQL.Parameters.Add("@akunmarket", SqlDbType.NVarChar, 50).Value = marketplace.PERSO;
                        EDB.ExecuteSQL("MOConnectionString", "autocreate_stf02h_tokped", CommandSQL);
                        return Json("sukses", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return JsonErrorMessage("Akun yang anda pilih bukan akun dari marketplace Tokopedia");
                    }

                }
                else
                {
                    return JsonErrorMessage("Akun tidak ditemukan");
                }


            }
            return JsonErrorMessage("Akun tidak ada");

        }
        public ActionResult GetTotalData(string cust)
        {
            var ret = new SimpleJsonObject();
            if (!string.IsNullOrEmpty(cust))
            {
                var customer = ErasoftDbContext.ARF01.Where(m => m.CUST == cust).FirstOrDefault();
                if (customer != null)
                {
                    var tokped = MoDbContext.Marketplaces.Where(m => m.NamaMarket.ToUpper() == "TOKOPEDIA").FirstOrDefault();
                    if (tokped != null)
                    {
                        if (customer.NAMA == tokped.IdMarket.ToString())
                        {
                            ret.Errors = "Silahkan edit per barang untuk sikronisasi barang dari marketplace Tokopedia.";
                            return Json(ret, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                var listTempBrg = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.CUST.ToUpper().Equals(cust.ToUpper())).ToList();
                if (listTempBrg != null)
                {
                    ret.Total = listTempBrg.Count();
                }
                else
                {
                    ret.Errors = "Gagal mengambil data.";
                }
            }
            else
            {
                ret.Errors = "Anda belum memilih Akun.";
            }

            return Json(ret, JsonRequestBehavior.AllowGet);
        }
        // =============================================== Bagian Upload Barang (END)
        protected double GetQOHSTF08A(string Barang, string Gudang)
        {
            double qtyOnHand = 0d;
            {
                object[] spParams = {
                    new SqlParameter("@BRG", Barang),
                    new SqlParameter("@GD", Gudang),
                    new SqlParameter("@Satuan", "2"),
                    new SqlParameter("@THN", Convert.ToInt16(DateTime.Now.ToString("yyyy"))),
                    new SqlParameter("@QOH", SqlDbType.Decimal) {Direction = ParameterDirection.Output}
                };

                ErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);
                qtyOnHand = Convert.ToDouble(((SqlParameter)spParams[4]).Value);
            }

            //ErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);

            double qtySO = ErasoftDbContext.Database.SqlQuery<double>("SELECT ISNULL(SUM(ISNULL(QTY,0)),0) QSO FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI LEFT JOIN SIT01A C ON A.NO_BUKTI = C.NO_SO WHERE A.STATUS_TRANSAKSI IN ('0', '01', '02', '03', '04') AND B.LOKASI = CASE '" + Gudang + "' WHEN 'ALL' THEN B.LOKASI ELSE '" + Gudang + "' END AND ISNULL(C.NO_BUKTI,'') = '' AND B.BRG = '" + Barang + "'").FirstOrDefault();
            qtyOnHand = qtyOnHand - qtySO;
            return qtyOnHand;
        }

        [HttpGet]
        public void UpdateCategoryShopeeAPI()
        {
            var kdShopee = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "SHOPEE");
            var listShopee = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdShopee.IdMarket.ToString()).ToList();
            bool onlyFirst = true;
            foreach (ARF01 tblCustomer in listShopee)
            {
                if (!string.IsNullOrEmpty(tblCustomer.Sort1_Cust))
                {
                    if (onlyFirst)
                    {
                        ShopeeController.ShopeeAPIData iden = new ShopeeController.ShopeeAPIData
                        {
                            merchant_code = tblCustomer.Sort1_Cust,
                        };
                        ShopeeController shoAPI = new ShopeeController();
                        Task.Run(() => shoAPI.GetCategory(iden).Wait());

                        onlyFirst = false;
                    }
                }
            }
        }

        [HttpGet]
        public void UpdateAttributeShopeeAPI()
        {
            var kdShopee = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "SHOPEE");
            var listShopee = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdShopee.IdMarket.ToString()).ToList();
            bool onlyFirst = true;
            foreach (ARF01 tblCustomer in listShopee)
            {
                if (!string.IsNullOrEmpty(tblCustomer.Sort1_Cust))
                {
                    if (onlyFirst)
                    {
                        ShopeeController.ShopeeAPIData iden = new ShopeeController.ShopeeAPIData
                        {
                            merchant_code = tblCustomer.Sort1_Cust,
                        };
                        ShopeeController shoAPI = new ShopeeController();
                        Task.Run(() => shoAPI.GetAttribute(iden).Wait());

                        onlyFirst = false;
                    }
                }
            }
        }
    }
}