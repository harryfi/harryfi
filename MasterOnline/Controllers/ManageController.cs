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

using Erasoft.Function;

using MasterOnline.Models;
using MasterOnline.Services;
using MasterOnline.Utils;

using MasterOnline.ViewModels;
using PagedList;

namespace MasterOnline.Controllers
{
    [SessionCheck]

    public class ManageController : Controller
    {
        public MoDbContext MoDbContext { get; set; }
        public ErasoftContext ErasoftDbContext { get; set; }

        public ManageController()
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
            return View();
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
                ListBarang = ErasoftDbContext.STF02.ToList(),
                ListAkunMarketplace = ErasoftDbContext.ARF01.ToList(),
                ListMarket = MoDbContext.Marketplaces.ToList(),
                ListBarangUntukCekQty = ErasoftDbContext.STF08A.ToList(),
                ListStok = ErasoftDbContext.STT01B.ToList()
            };

            // Pesanan
            vm.JumlahPesananHariIni = vm.ListPesanan?.Where(p => p.TGL == selectedDate).Count();
            vm.NilaiPesananHariIni = vm.ListPesanan?.Where(p => p.TGL == selectedDate).Sum(p => p.BRUTO - p.NILAI_DISC);
            vm.JumlahPesananBulanIni = vm.ListPesanan?.Where(p => p.TGL?.Month == selectedMonth).Count();
            vm.NilaiPesananBulanIni = vm.ListPesanan?.Where(p => p.TGL?.Month == selectedMonth).Sum(p => p.BRUTO - p.NILAI_DISC);

            // Faktur
            vm.JumlahFakturHariIni = vm.ListFaktur?.Where(p => p.TGL == selectedDate && p.JENIS_FORM == "2").Count();
            vm.NilaiFakturHariIni = vm.ListFaktur?.Where(p => p.TGL == selectedDate && p.JENIS_FORM == "2").Sum(p => p.BRUTO - p.NILAI_DISC);
            vm.JumlahFakturBulanIni = vm.ListFaktur?.Where(p => p.TGL.Month == selectedMonth && p.JENIS_FORM == "2").Count();
            vm.NilaiFakturBulanIni = vm.ListFaktur?.Where(p => p.TGL.Month == selectedMonth && p.JENIS_FORM == "2").Sum(p => p.BRUTO - p.NILAI_DISC);

            // Retur
            vm.JumlahReturHariIni = vm.ListFaktur?.Where(p => p.TGL == selectedDate && p.JENIS_FORM == "3").Count();
            vm.NilaiReturHariIni = vm.ListFaktur?.Where(p => p.TGL == selectedDate && p.JENIS_FORM == "3").Sum(p => p.BRUTO - p.NILAI_DISC);
            vm.JumlahReturBulanIni = vm.ListFaktur?.Where(p => p.TGL.Month == selectedMonth && p.JENIS_FORM == "3").Count();
            vm.NilaiReturBulanIni = vm.ListFaktur?.Where(p => p.TGL.Month == selectedMonth && p.JENIS_FORM == "3").Sum(p => p.BRUTO - p.NILAI_DISC);

            if (vm.ListAkunMarketplace.Count > 0)
            {
                foreach (var marketplace in vm.ListAkunMarketplace)
                {
                    var idMarket = Convert.ToInt32(marketplace.NAMA);
                    var namaMarket = vm.ListMarket.Single(m => m.IdMarket == idMarket).NamaMarket;

                    var jumlahPesananToday = vm.ListPesanan?
                        .Where(p => p.CUST == marketplace.CUST && p.TGL == selectedDate).Count();
                    var nilaiPesananToday = $"Rp {String.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:N}", vm.ListPesanan?.Where(p => p.CUST == marketplace.CUST && p.TGL == selectedDate).Sum(p => p.BRUTO - p.NILAI_DISC))}";

                    var jumlahPesananMonth = vm.ListPesanan?

                        .Where(p => p.CUST == marketplace.CUST && p.TGL?.Month == selectedMonth).Count();
                    var nilaiPesananMonth = $"Rp {String.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:N}", vm.ListPesanan?.Where(p => p.CUST == marketplace.CUST && p.TGL?.Month == selectedMonth).Sum(p => p.BRUTO - p.NILAI_DISC))}";

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

        [Route("manage/order")]
        public ActionResult Pesanan()
        {
            //add by Tri call market place api getorder
            var connectionID = Guid.NewGuid().ToString();
            AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            string username = sessionData.Account.Username;
            int bliAcc = 0;
            int lazadaAcc = 0;
            int blAcc = 0;
            int elAcc = 0;

            //GetOrderList
            //var kdBli = MoDbContext.Marketplaces.Single(m => m.NamaMarket.ToUpper() == "BLIBLI");
            //var listBliShop = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdBli.IdMarket.ToString()).ToList();
            //if (listBliShop.Count > 0)
            //{
            //    bliAcc = 1;
            //    foreach (ARF01 tblCustomer in listBliShop)
            //    {
            //        var bliApi = new BlibliController();

            //        BlibliController.BlibliAPIData iden = new BlibliController.BlibliAPIData
            //        {
            //            merchant_code = tblCustomer.Sort1_Cust,
            //            API_client_password = tblCustomer.API_CLIENT_P,
            //            API_client_username = tblCustomer.API_CLIENT_U,
            //            API_secret_key = tblCustomer.API_KEY,
            //            token = tblCustomer.TOKEN,
            //            mta_username_email_merchant = tblCustomer.EMAIL,
            //            mta_password_password_merchant = tblCustomer.PASSWORD
            //        };

            //        bliApi.GetOrderList(iden);
            //    }
            //}
            var kdEL = MoDbContext.Marketplaces.Single(m => m.NamaMarket.ToUpper() == "ELEVENIA");
            var listELShop = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdEL.IdMarket.ToString()).ToList();
            if (listELShop.Count > 0)
            {
                elAcc = 1;
                foreach (ARF01 tblCustomer in listELShop)
                {
                    var elApi = new EleveniaController();
                    elApi.GetOrder(tblCustomer.API_KEY, EleveniaController.StatusOrder.Paid, connectionID, tblCustomer.CUST, tblCustomer.PERSO);
                }
            }
            var kdBL = MoDbContext.Marketplaces.Single(m => m.NamaMarket.ToUpper() == "BUKALAPAK");
            var listBLShop = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdBL.IdMarket.ToString()).ToList();
            if (listBLShop.Count > 0)
            {
                blAcc = 1;
                foreach (ARF01 tblCustomer in listBLShop)
                {
                    var blApi = new BukaLapakController();
                    blApi.cekTransaksi(tblCustomer.CUST, tblCustomer.EMAIL, tblCustomer.API_KEY, tblCustomer.TOKEN, connectionID);
                }

            }
            //remark by calvin, 13 july 2018, dipindah ke dalam masing" API
            //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
            //SqlCommand CommandSQL = new SqlCommand();

            ////add by Tri call sp to insert buyer data
            //CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
            //CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;

            //EDB.ExecuteSQL("", "MoveARF01CFromTempTable", CommandSQL);
            ////end add by Tri call sp to insert buyer data

            //CommandSQL = new SqlCommand();
            //CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;

            //CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;
            //CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
            //CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = lazadaAcc;
            //CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = blAcc;

            //EDB.ExecuteSQL("", "MoveOrderFromTempTable", CommandSQL);

            ////end add by Tri call market place api getorder


            var vm = new PesananViewModel()
            {
                ListBarang = ErasoftDbContext.STF02.ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListSubs = MoDbContext.Subscription.ToList()
            };

            return View(vm);
        }

        [Route("manage/penjualan/faktur")]
        public ActionResult Faktur()
        {
            var vm = new FakturViewModel()
            {
                ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2").ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListPesanan = ErasoftDbContext.SOT01A.ToList(),
                ListNFaktur = ErasoftDbContext.ART03B.ToList(),
                ListSubs = MoDbContext.Subscription.ToList(),
            };

            return View(vm);
        }

        [Route("manage/penjualan/retur")]
        public ActionResult ReturFaktur()
        {
            var vm = new FakturViewModel()
            {
                ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "3").ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "1").ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "2").ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                ListStf02S = ErasoftDbContext.STF02.ToList(),
                ListMarket = ErasoftDbContext.ARF01.OrderBy(p => p.RecNum).ToList(),
                ListHargaJualPermarketView = ErasoftDbContext.STF02H.OrderBy(p => p.IDMARKET).ToList(),
                ListCategoryBlibli = MoDbContext.CategoryBlibli.Where(p => string.IsNullOrEmpty(p.PARENT_CODE)).ToList(),
                DataUsaha = ErasoftDbContext.SIFSYS.Single(p => p.BLN == 1)
            };

            return View(barangVm);
        }

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

        public ActionResult BuyerPopup()
        {
            return View();
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
                return HttpNotFound();
            }
        }

        public ActionResult EditPembeliPopup(string kodePembeli)
        {
            try
            {
                var buyerVm = new BuyerViewModel()
                {
                    Pembeli = ErasoftDbContext.ARF01C.Single(c => c.BUYER_CODE == kodePembeli),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList()
                };

                ViewData["Editing"] = 1;

                return View("BuyerPopup", buyerVm);
            }
            catch (Exception)
            {
                return HttpNotFound();
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
                return HttpNotFound();
            }
        }

        // =============================================== Bagian Pembeli (END)

        // =============================================== Bagian Customer (START)

        [HttpGet]
        public ActionResult CekJumlahMarketplace(string uname)
        {
            var jumlahAkunMarketplace = ErasoftDbContext.ARF01
                .GroupBy(m => m.NAMA)
                .Select(g => new
                {
                    Jumlah = g.Select(o => o.NAMA).Distinct().Count()
                });

            var accInDb = MoDbContext.Account.FirstOrDefault(a => a.Username == uname);

            if (accInDb == null)
            {
                var accIdByUser = MoDbContext.User.FirstOrDefault(u => u.Username == uname)?.AccountId;
                accInDb = MoDbContext.Account.FirstOrDefault(a => a.AccountId == accIdByUser);
            }

            var accSubs = MoDbContext.Subscription.FirstOrDefault(s => s.KODE == accInDb.KODE_SUBSCRIPTION);
            var jumlahSemuaAkun = 0;

            foreach (var market in jumlahAkunMarketplace)
            {
                jumlahSemuaAkun += market.Jumlah;
            }

            var valSubs = new ValidasiSubs()
            {
                JumlahMarketplace = jumlahSemuaAkun,
                JumlahMarketplaceMax = accSubs?.JUMLAH_MP
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
                        token = customer.TOKEN
                    };
                    await BliApi.GetCategoryTree(data);
                    //BliApi.GetCategoryTree(data);
                }
                #endregion
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

                custInDb.TOP = customer.Customers.TOP;
                custInDb.AL = customer.Customers.AL;
                custInDb.KODEPROV = customer.Customers.KODEPROV;
                custInDb.KODEKABKOT = customer.Customers.KODEKABKOT;
                custInDb.KODEPOS = customer.Customers.KODEPOS;
                custInDb.PERSO = customer.Customers.PERSO;
                custInDb.EMAIL = customer.Customers.EMAIL;
                custInDb.TLP = customer.Customers.TLP;
                //add by Tri, add api key
                custInDb.API_KEY = customer.Customers.API_KEY;
                kdCustomer = custInDb.CUST;
                //end add by Tri, add api key
                custInDb.API_CLIENT_U = customer.Customers.API_CLIENT_U;
                custInDb.API_CLIENT_P = customer.Customers.API_CLIENT_P;
            }

            ErasoftDbContext.SaveChanges();
            //add by Tri call bl/lzd api get access key
            if (customer.Customers.NAMA.Equals(MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BUKALAPAK").IdMarket.ToString()))
            {
                var getKey = new BukaLapakController().GetAccessKey(kdCustomer, customer.Customers.EMAIL, customer.Customers.PASSWORD);
            }
            else if (customer.Customers.NAMA.Equals(MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "LAZADA").IdMarket.ToString()))
            {
                var getToken = new LazadaController().GetToken(kdCustomer, customer.Customers.API_KEY);
            }
            #region Elevenia get deliveryTemp
            else if (customer.Customers.NAMA.Equals(MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "ELEVENIA").IdMarket.ToString()))
            {
                var elApi = new EleveniaController();
                elApi.GetDeliveryTemp(Convert.ToString(customer.Customers.RecNum), Convert.ToString(customer.Customers.API_KEY));
            }
            #endregion
            //#region BLIBLI get category dan attribute
            //else if (customer.Customers.NAMA.Equals(MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BLIBLI").IdMarket.ToString()))
            //{
            //    if (!string.IsNullOrEmpty(customer.Customers.API_CLIENT_P) && !string.IsNullOrEmpty(customer.Customers.API_CLIENT_U))
            //    {
            //        var BliApi = new BlibliController();
            //        BlibliController.BlibliAPIData data = new BlibliController.BlibliAPIData()
            //        {
            //            API_client_username = customer.Customers.API_CLIENT_U,
            //            API_client_password = customer.Customers.API_CLIENT_P,
            //            API_secret_key = customer.Customers.API_KEY,
            //            mta_username_email_merchant = customer.Customers.EMAIL,
            //            mta_password_password_merchant = customer.Customers.PASSWORD,
            //            merchant_code = customer.Customers.Sort1_Cust,
            //            token = customer.Customers.TOKEN
            //        };
            //        BliApi.GetPickupPoint(data);
            //    }
            //}
            //#endregion

            //end add by Tri call bl/lzd api get access key
            ModelState.Clear();

            var partialVm = new CustomerViewModel()
            {
                ListCustomer = ErasoftDbContext.ARF01.ToList()
            };

            return PartialView("TableCustomerPartial", partialVm);
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
                return HttpNotFound();
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
                ListStf02S = ErasoftDbContext.STF02.ToList(),
            };

            return PartialView("TableBarang1Partial", barangVm);
        }

        public ActionResult RefreshTableBarangKosong()
        {
            var listBarangMiniStok = new List<PenjualanBarang>();

            foreach (var barang in ErasoftDbContext.STF02.ToList())
            {
                var barangUtkCek = ErasoftDbContext.STF08A.ToList().FirstOrDefault(b => b.BRG == barang.BRG);
                var qtyOnHand = 0d;

                if (barangUtkCek != null)
                {
                    qtyOnHand = barangUtkCek.QAwal + barangUtkCek.QM1 + barangUtkCek.QM2 + barangUtkCek.QM3 + barangUtkCek.QM4
                                + barangUtkCek.QM5 + barangUtkCek.QM6 + barangUtkCek.QM7 + barangUtkCek.QM8 + barangUtkCek.QM9
                                + barangUtkCek.QM10 + barangUtkCek.QM11 + barangUtkCek.QM12 - barangUtkCek.QK1 - barangUtkCek.QK2
                                - barangUtkCek.QK3 - barangUtkCek.QK4 - barangUtkCek.QK5 - barangUtkCek.QK6 - barangUtkCek.QK7
                                - barangUtkCek.QK8 - barangUtkCek.QK9 - barangUtkCek.QK10 - barangUtkCek.QK11 - barangUtkCek.QK12;

                    if (qtyOnHand == 0)
                    {
                        listBarangMiniStok.Add(new PenjualanBarang
                        {
                            KodeBrg = barang.BRG,
                            NamaBrg = $"{barang.NAMA} {barang.NAMA2}",
                            Kategori = barang.KET_SORT1,
                            Merk = barang.KET_SORT2,
                            HJual = barang.HJUAL,
                            Qty = qtyOnHand
                        });
                    }
                }
            }

            return PartialView("TableBarangKosongPartial", listBarangMiniStok.OrderBy(b => b.NamaBrg).ToList());
        }

        public ActionResult RefreshTableBarangTidakLaku()
        {
            var listBarangTidakLaku = new List<PenjualanBarang>();

            foreach (var barang in ErasoftDbContext.STF02.ToList())
            {
                var barangTerpesan = ErasoftDbContext.SOT01B.FirstOrDefault(b => b.BRG == barang.BRG);

                if (barangTerpesan == null)
                {
                    listBarangTidakLaku.Add(new PenjualanBarang
                    {
                        KodeBrg = barang.BRG,
                        NamaBrg = $"{barang.NAMA} {barang.NAMA2}",
                        Kategori = barang.KET_SORT1,
                        Merk = barang.KET_SORT2,
                        HJual = barang.HJUAL,
                        Laku = false
                    });
                }
            }

            if (listBarangTidakLaku.Count == 0)
            {
                listBarangTidakLaku.Add(new PenjualanBarang
                {
                    KodeBrg = "---",
                    NamaBrg = "---",
                    Kategori = "---",
                    Merk = "---",
                    HJual = 0,
                    Laku = false
                });
            }

            return PartialView("TableBarangTidakLakuPartial", listBarangTidakLaku.OrderBy(b => b.NamaBrg).ToList());
        }

        public ActionResult RefreshTableBarangDibawahMinimumStok()
        {
            var listBarangMiniStok = new List<PenjualanBarang>();

            foreach (var barang in ErasoftDbContext.STF02.ToList())
            {
                var barangUtkCek = ErasoftDbContext.STF08A.ToList().FirstOrDefault(b => b.BRG == barang.BRG);
                var qtyOnHand = 0d;

                if (barangUtkCek != null)
                {
                    qtyOnHand = barangUtkCek.QAwal + barangUtkCek.QM1 + barangUtkCek.QM2 + barangUtkCek.QM3 + barangUtkCek.QM4
                                + barangUtkCek.QM5 + barangUtkCek.QM6 + barangUtkCek.QM7 + barangUtkCek.QM8 + barangUtkCek.QM9
                                + barangUtkCek.QM10 + barangUtkCek.QM11 + barangUtkCek.QM12 - barangUtkCek.QK1 - barangUtkCek.QK2
                                - barangUtkCek.QK3 - barangUtkCek.QK4 - barangUtkCek.QK5 - barangUtkCek.QK6 - barangUtkCek.QK7
                                - barangUtkCek.QK8 - barangUtkCek.QK9 - barangUtkCek.QK10 - barangUtkCek.QK11 - barangUtkCek.QK12;

                    if (qtyOnHand < barang.MINI)
                    {
                        listBarangMiniStok.Add(new PenjualanBarang
                        {
                            KodeBrg = barang.BRG,
                            NamaBrg = $"{barang.NAMA} {barang.NAMA2}",
                            Kategori = barang.KET_SORT1,
                            Merk = barang.KET_SORT2,
                            HJual = barang.HJUAL
                        });
                    }
                }
            }

            if (listBarangMiniStok.Count == 0)
            {
                listBarangMiniStok.Add(new PenjualanBarang
                {
                    KodeBrg = "---",
                    NamaBrg = "---",
                    Kategori = "---",
                    Merk = "---",
                    HJual = 0,
                });
            }

            return PartialView("TableBarangDibawahMinimumStokPartial", listBarangMiniStok.OrderBy(b => b.NamaBrg).ToList());
        }

        public ActionResult RefreshTableBarangPalingLaku()
        {
            var listBarangLaku = new List<PenjualanBarang>();

            foreach (var barang in ErasoftDbContext.STF02.ToList())
            {
                var listBarangTerpesan = ErasoftDbContext.SOT01B.Where(b => b.BRG == barang.BRG).ToList();

                if (listBarangTerpesan.Count > 0)
                {
                    listBarangLaku.Add(new PenjualanBarang
                    {
                        KodeBrg = barang.BRG,
                        NamaBrg = $"{barang.NAMA} {barang.NAMA2}",
                        Kategori = barang.KET_SORT1,
                        Merk = barang.KET_SORT2,
                        HJual = barang.HJUAL,
                    });
                }
            }

            if (listBarangLaku.Count == 0)
            {
                listBarangLaku.Add(new PenjualanBarang
                {
                    KodeBrg = "---",
                    NamaBrg = "---",
                    Kategori = "---",
                    Merk = "---",
                    HJual = 0,
                });
            }
            return PartialView("TableBarangPalingLakuPartial", listBarangLaku.OrderBy(b => b.NamaBrg).ToList());
        }

        [HttpGet]
        public ActionResult GetKategoriBarang()
        {
            var listKategori = ErasoftDbContext.STF02E.Where(k => k.LEVEL == "1").OrderBy(m => m.KET).ToList();

            return Json(listKategori, JsonRequestBehavior.AllowGet);
        }
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
        public ActionResult GetAttributeBlibli(string code)
        {
            string[] codelist = code.Split(';');
            var listAttributeBlibli = MoDbContext.AttributeBlibli.Where(k => codelist.Contains(k.CATEGORY_CODE)).ToList();
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

        public ActionResult DeleteFotoProduk(string kodeBarang, int urutan, string uname)
        {
            try
            {
                var namaFile = $"FotoProduk-{uname}-{kodeBarang}-foto-{urutan}.jpg";
                var path = Path.Combine(Server.MapPath("~/Content/Uploaded/"), namaFile);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }

                return new EmptyResult();
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        [HttpPost]
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
            string[] imgPath = new string[Request.Files.Count];
            if (dataBarang.Stf02.ID == null)
            {
                insert = true;
                ErasoftDbContext.STF02.Add(dataBarang.Stf02);

                if (dataBarang.ListHargaJualPermarket?.Count > 0)
                {
                    foreach (var hargaPerMarket in dataBarang.ListHargaJualPermarket)
                    {
                        hargaPerMarket.BRG = dataBarang.Stf02.BRG;
                        ErasoftDbContext.STF02H.Add(hargaPerMarket);
                    }
                }

                var listMarket = dataBarang.ListMarket.ToList();

                if (listMarket.Count > 0)
                {
                    foreach (var market in listMarket)
                    {
                        var dataHarga = new STF02H()
                        {
                            BRG = dataBarang.Stf02.BRG,
                            IDMARKET = Convert.ToInt32(market.RecNum),
                            AKUNMARKET = market.PERSO,
                            HJUAL = 0,
                            DISPLAY = false,
                            USERNAME = dataBarang.Username
                        };

                        ErasoftDbContext.STF02H.Add(dataHarga);
                    }
                }
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
                            var fileExtension = Path.GetExtension(file.FileName);
                            var namaFile = $"FotoProduk-{dataBarang.Stf02.USERNAME}-{dataBarang.Stf02.BRG}-foto-{i + 1}{fileExtension}";
                            var path = Path.Combine(Server.MapPath("~/Content/Uploaded/"), namaFile);
                            try
                            {
                                file.SaveAs(path);
                            }
                            catch (Exception ex)
                            {

                            }
                            //add by tri
                            imgPath[i] = path;
                        }
                    }
                }
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

                    if (dataBarang.ListHargaJualPermarket?.Count > 0)
                    {
                        foreach (var dataBaru in dataBarang.ListHargaJualPermarket)
                        {
                            var dataHarga = ErasoftDbContext.STF02H.SingleOrDefault(h => h.RecNum == dataBaru.RecNum);
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
                            #endregion
                        }
                    }

                    if (Request.Files.Count > 0)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            var file = Request.Files[i];

                            if (file != null && file.ContentLength > 0)
                            {
                                updateGambar = true;
                                var fileExtension = Path.GetExtension(file.FileName);
                                var namaFile = $"FotoProduk-{barangInDb.USERNAME}-{barangInDb.BRG}-foto-{i + 1}{fileExtension}";
                                var path = Path.Combine(Server.MapPath("~/Content/Uploaded/"), namaFile);
                                file.SaveAs(path);
                                //add by tri
                                imgPath[i] = path;
                            }
                        }
                    }
                }
            }

            ErasoftDbContext.SaveChanges();

            #region Sync ke Marketplace
            var kdBL = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BUKALAPAK");
            var listBLShop = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdBL.IdMarket.ToString()).ToList();
            var kdLazada = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "LAZADA");
            var listLazadaShop = ErasoftDbContext.ARF01.Where(m => m.NAMA == kdLazada.IdMarket.ToString()).ToList();
            string[] imageUrl = new string[Request.Files.Count];//variabel penampung url image hasil upload ke markeplace
            var lzdApi = new LazadaController();
            var blApi = new BukaLapakController();

            //add by tri call marketplace api to create product
            if (insert)
            {
                #region lazada
                if (listLazadaShop.Count > 0)
                {
                    foreach (ARF01 tblCustomer in listLazadaShop)
                    {
                        if (!string.IsNullOrEmpty(tblCustomer.TOKEN))
                        {
                            //string[] imageUrl = new string[Request.Files.Count];
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
                                kdBrg = dataBarang.Stf02.BRG,
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
                            var productMarketPlace = dataBarang.ListHargaJualPermarket.SingleOrDefault(m => m.BRG == dataBarang.Stf02.BRG && m.IDMARKET == tblCustomer.RecNum);
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
                            var result = lzdApi.CreateProduct(dataLazada);
                        }

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
                        string[] imgID = new string[Request.Files.Count];
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
                            kdBrg = dataBarang.Stf02.BRG,
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
                        var productMarketPlace = dataBarang.ListHargaJualPermarket.SingleOrDefault(m => m.BRG == dataBarang.Stf02.BRG && m.IDMARKET == tblCustomer.RecNum);
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
                        if (result.status == 1)
                            if (!productMarketPlace.DISPLAY)
                            {
                                //panggil api utk non-aktif barang yg baru di insert
                                result = blApi.prodNonAktif(result.message, tblCustomer.API_KEY, tblCustomer.TOKEN);
                            }
                    }
                }
                #endregion
                #region Elevenia
                saveBarangElevenia(1, dataBarang);
                #endregion
                #region Blibli
                saveBarangBlibli(1, dataBarang);
                #endregion
            }
            //end add by tri call marketplace api to create product
            else
            {
                //update harga, qty, dll
                saveBarangElevenia(2, dataBarang);
                //#region Blibli
                //saveBarangBlibli(1, dataBarang);
                //#endregion
                if (updateHarga)
                {
                    #region lazada
                    if (listLazadaShop.Count > 0)
                    {
                        foreach (ARF01 tblCustomer in listLazadaShop)
                        {
                            if (!string.IsNullOrEmpty(tblCustomer.TOKEN))
                            {
                                var barang = ErasoftDbContext.STF02.SingleOrDefault(b => b.ID == dataBarang.Stf02.ID);
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
                            var barang = ErasoftDbContext.STF02.SingleOrDefault(b => b.ID == dataBarang.Stf02.ID);
                            var tokoBl = ErasoftDbContext.STF02H.SingleOrDefault(h => h.IDMARKET == tblCustomer.RecNum && h.BRG == barang.BRG);
                            var resultBL = blApi.updateProduk(tokoBl.BRG_MP, tokoBl.HJUAL.ToString(), "", tblCustomer.API_KEY, tblCustomer.TOKEN);
                        }
                    }

                    #endregion
                }
                if (updateDisplay)
                {
                    #region lazada
                    if (listLazadaShop.Count > 0)
                    {
                        foreach (ARF01 tblCustomer in listLazadaShop)
                        {
                            if (!string.IsNullOrEmpty(tblCustomer.TOKEN))
                            {
                                var barang = ErasoftDbContext.STF02.SingleOrDefault(b => b.ID == dataBarang.Stf02.ID);
                                var tokoLazada = ErasoftDbContext.STF02H.SingleOrDefault(h => h.IDMARKET == tblCustomer.RecNum && h.BRG == barang.BRG);
                                var resultLazada = lzdApi.setDisplay(tokoLazada.BRG_MP, tokoLazada.DISPLAY, tblCustomer.TOKEN);
                            }
                        }
                    }
                    #endregion
                    #region Elevenia
                    saveBarangElevenia(3, dataBarang);
                    #endregion
                    #region Bukalapak
                    if (listBLShop.Count > 0)
                    {
                        foreach (ARF01 tblCustomer in listBLShop)
                        {
                            var barang = ErasoftDbContext.STF02.SingleOrDefault(b => b.ID == dataBarang.Stf02.ID);
                            var tokoBl = ErasoftDbContext.STF02H.SingleOrDefault(h => h.IDMARKET == tblCustomer.RecNum && h.BRG == barang.BRG);

                            if (tokoBl.DISPLAY)
                            {
                                var result = blApi.prodAktif(tokoBl.BRG_MP, tblCustomer.API_KEY, tblCustomer.TOKEN);
                            }
                            else
                            {
                                var result = blApi.prodNonAktif(tokoBl.BRG_MP, tblCustomer.API_KEY, tblCustomer.TOKEN);

                            }

                        }
                    }
                    #endregion
                }
                //if (updateGambar)
                //{

                //}


                //if (updateGambar)
                //{
                //    #region Bukalapak
                //    if (listBLShop.Count > 0)
                //    {
                //        foreach (ARF01 tblCustomer in listBLShop)
                //        {
                //            var tokoBl = ErasoftDbContext.STF02H.SingleOrDefault(h => h.IDMARKET == tblCustomer.RecNum);
                //            var resultBL = new BukaLapakController().updateProduk(tokoBl.BRG_MP, tokoBl.HJUAL.ToString(), "", tblCustomer.API_KEY, tblCustomer.TOKEN);

                //            string[] imgID = new string[Request.Files.Count];
                //            for (int i = 0; i < imgPath.Length; i++)
                //            {
                //                if (!string.IsNullOrEmpty(imgPath[i]))
                //                {
                //                    var uploadImg = new BukaLapakController().uploadGambar(imgPath[i], tblCustomer.API_KEY, tblCustomer.TOKEN);
                //                    if (uploadImg.status == 1)
                //                        imgID[i] = uploadImg.message;
                //                }
                //            }

                //        }
                //    }
                //}
            }
            #endregion
            ModelState.Clear();

            var partialVm = new BarangViewModel()
            {
                ListStf02S = ErasoftDbContext.STF02.ToList()
            };

            return PartialView("TableBarang1Partial", partialVm);
        }
        protected void saveBarangBlibli(int mode, BarangViewModel dataBarang)
        {
            var barangInDb = ErasoftDbContext.STF02.SingleOrDefault(b => b.ID == dataBarang.Stf02.ID || b.BRG == dataBarang.Stf02.BRG);
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

                                        BlibliController.BlibliAPIData iden = new BlibliController.BlibliAPIData
                                        {
                                            merchant_code = tblCustomer.Sort1_Cust,
                                            API_client_password = tblCustomer.API_CLIENT_P,
                                            API_client_username = tblCustomer.API_CLIENT_U,
                                            API_secret_key = tblCustomer.API_KEY,
                                            token = tblCustomer.TOKEN,
                                            mta_username_email_merchant = tblCustomer.EMAIL,
                                            mta_password_password_merchant = tblCustomer.PASSWORD
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
                                            Height = Convert.ToString(dataBarang.Stf02.TINGGI)
                                        };
                                        data.Brand = ErasoftDbContext.STF02E.SingleOrDefault(m => m.KODE == dataBarang.Stf02.Sort2 && m.LEVEL == "2").KET;
                                        data.Price = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).HJUAL.ToString();
                                        data.MarketPrice = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).HJUAL.ToString();
                                        data.CategoryCode = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).CATEGORY_CODE.ToString();
                                        var display = Convert.ToBoolean(ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).DISPLAY);
                                        data.display = display ? "true" : "false";
                                        new BlibliController().UploadProduk(iden, data);
                                        //new BlibliController().GetQueueFeedDetail(iden, null);
                                        //}
                                    }
                                }
                            }
                            break;
                        #endregion
                        case 2:
                            break;
                        //#region Update Product
                        //case 2:
                        //    {
                        //        #region getUrlImage
                        //        //string[] imgID = new string[Request.Files.Count];
                        //        string[] imgID = new string[3];
                        //        //if (Request.Files.Count > 0)
                        //        //{
                        //        for (int i = 0; i < 3; i++)
                        //        {
                        //            //var file = Request.Files[i];

                        //            //if (file != null && file.ContentLength > 0)
                        //            //{
                        //            //    var fileExtension = Path.GetExtension(file.FileName);
                        //            imgID[i] = "http://masteronline.co.id/ele/image?id=" + $"FotoProduk-{barangInDb.USERNAME}-{barangInDb.BRG}-foto-{i + 1}.jpg";
                        //            imgID[i] = Convert.ToString(imgID[i]).Replace(" ", "%20");
                        //            //}
                        //        }
                        //        //}
                        //        #endregion
                        //        foreach (ARF01 tblCustomer in listElShop)
                        //        {
                        //            var qtyOnHand = 0d;
                        //            {
                        //                object[] spParams = {
                        //                    new SqlParameter("@BRG",string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG),
                        //                    new SqlParameter("@GD","ALL"),
                        //                    new SqlParameter("@Satuan", "2"),
                        //                    new SqlParameter("@THN", Convert.ToInt16(DateTime.Now.ToString("yyyy"))),
                        //                    new SqlParameter("@QOH", SqlDbType.Decimal) {Direction = ParameterDirection.Output}
                        //                };

                        //                ErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);
                        //                qtyOnHand = Convert.ToDouble(((SqlParameter)spParams[4]).Value);
                        //            }
                        //            EleveniaController.EleveniaProductData data = new EleveniaController.EleveniaProductData
                        //            {
                        //                api_key = tblCustomer.API_KEY,
                        //                kode = string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG,
                        //                nama = dataBarang.Stf02.NAMA,
                        //                berat = (dataBarang.Stf02.BERAT / 1000).ToString(),//MO save dalam Gram, Elevenia dalam Kilogram
                        //                imgUrl = imgID,
                        //                Keterangan = dataBarang.Stf02.Deskripsi,
                        //                Qty = Convert.ToString(qtyOnHand),
                        //                DeliveryTempNo = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).DeliveryTempElevenia.ToString(),
                        //                IDMarket = tblCustomer.RecNum.ToString(),
                        //            };
                        //            data.Brand = ErasoftDbContext.STF02E.SingleOrDefault(m => m.KODE == dataBarang.Stf02.Sort2 && m.LEVEL == "2").KET;
                        //            data.Price = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).HJUAL.ToString();
                        //            data.kode_mp = Convert.ToString(ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).BRG_MP);

                        //            if (string.IsNullOrEmpty(data.kode_mp))
                        //            {
                        //                var display = Convert.ToBoolean(ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).DISPLAY);
                        //                var result = new EleveniaController().CreateProduct(data, display);
                        //            }
                        //            else
                        //            if (!string.IsNullOrEmpty(data.kode_mp))
                        //            {
                        //                var result = new EleveniaController().UpdateProduct(data);
                        //            }
                        //            //if (result.resultCode.Equals("200"))
                        //            //{
                        //            //    #region Hide Item
                        //            //    EleveniaController.EleveniaProductData data2 = new EleveniaController.EleveniaProductData
                        //            //    {
                        //            //        api_key = tblCustomer.TOKEN,
                        //            //        kode = Convert.ToString(result.productNo)
                        //            //    };
                        //            //    var resultHide = new EleveniaController().HideItem(data2);
                        //            //    #endregion
                        //            //}
                        //        }
                        //    }
                        //    break;
                        //#endregion
                        //#region Display/Hide Item
                        //case 3:
                        //    foreach (ARF01 tblCustomer in listElShop)
                        //    {
                        //        EleveniaController.EleveniaProductData data = new EleveniaController.EleveniaProductData
                        //        {
                        //            api_key = tblCustomer.API_KEY,
                        //            kode = Convert.ToString(ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).BRG_MP)
                        //        };
                        //        if (Convert.ToBoolean(ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).DISPLAY))
                        //        {
                        //            var result = new EleveniaController().DisplayItem(data);
                        //        }
                        //        else
                        //        {
                        //            var result = new EleveniaController().HideItem(data);
                        //        }
                        //    }
                        //    break;
                        //#endregion
                        default:
                            break;
                    }
                }
            }
        }
        protected void saveBarangElevenia(int mode, BarangViewModel dataBarang)
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
                                #region getUrlImage
                                //string[] imgID = new string[Request.Files.Count];
                                string[] imgID = new string[3];
                                //if (Request.Files.Count > 0)
                                //{
                                for (int i = 0; i < 3; i++)
                                {
                                    //var file = Request.Files[i];

                                    //if (file != null && file.ContentLength > 0)
                                    //{
                                    //    var fileExtension = Path.GetExtension(file.FileName);
                                    imgID[i] = "http://masteronline.co.id/ele/image?id=" + $"FotoProduk-{barangInDb.USERNAME}-{barangInDb.BRG}-foto-{i + 1}.jpg";
                                    imgID[i] = Convert.ToString(imgID[i]).Replace(" ", "%20");
                                    //}
                                }
                                //}
                                #endregion
                                foreach (ARF01 tblCustomer in listElShop)
                                {
                                    EleveniaController.EleveniaProductData data = new EleveniaController.EleveniaProductData
                                    {
                                        api_key = tblCustomer.API_KEY,
                                        kode = string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG,
                                        nama = dataBarang.Stf02.NAMA + ' ' + dataBarang.Stf02.NAMA2 + ' ' + dataBarang.Stf02.NAMA3,
                                        berat = (dataBarang.Stf02.BERAT / 1000).ToString(),//MO save dalam Gram, Elevenia dalam Kilogram
                                        imgUrl = imgID,
                                        Keterangan = dataBarang.Stf02.Deskripsi,
                                        Qty = "1",
                                        DeliveryTempNo = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).DeliveryTempElevenia.ToString(),
                                        IDMarket = tblCustomer.RecNum.ToString(),
                                    };
                                    data.Brand = ErasoftDbContext.STF02E.SingleOrDefault(m => m.KODE == dataBarang.Stf02.Sort2 && m.LEVEL == "2").KET;
                                    data.Price = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).HJUAL.ToString();
                                    var display = Convert.ToBoolean(ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).DISPLAY);
                                    var result = new EleveniaController().CreateProduct(data, display);
                                }
                            }
                            break;
                        #endregion
                        #region Update Product
                        case 2:
                            {
                                #region getUrlImage
                                //string[] imgID = new string[Request.Files.Count];
                                string[] imgID = new string[3];
                                //if (Request.Files.Count > 0)
                                //{
                                for (int i = 0; i < 3; i++)
                                {
                                    //var file = Request.Files[i];

                                    //if (file != null && file.ContentLength > 0)
                                    //{
                                    //    var fileExtension = Path.GetExtension(file.FileName);
                                    imgID[i] = "http://masteronline.co.id/ele/image?id=" + $"FotoProduk-{barangInDb.USERNAME}-{barangInDb.BRG}-foto-{i + 1}.jpg";
                                    imgID[i] = Convert.ToString(imgID[i]).Replace(" ", "%20");
                                    //}
                                }
                                //}
                                #endregion
                                foreach (ARF01 tblCustomer in listElShop)
                                {
                                    var qtyOnHand = 0d;
                                    {
                                        object[] spParams = {
                                            new SqlParameter("@BRG",string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG),
                                            new SqlParameter("@GD","ALL"),
                                            new SqlParameter("@Satuan", "2"),
                                            new SqlParameter("@THN", Convert.ToInt16(DateTime.Now.ToString("yyyy"))),
                                            new SqlParameter("@QOH", SqlDbType.Decimal) {Direction = ParameterDirection.Output}
                                        };

                                        ErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);
                                        qtyOnHand = Convert.ToDouble(((SqlParameter)spParams[4]).Value);
                                    }
                                    EleveniaController.EleveniaProductData data = new EleveniaController.EleveniaProductData
                                    {
                                        api_key = tblCustomer.API_KEY,
                                        kode = string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG,
                                        nama = dataBarang.Stf02.NAMA,
                                        berat = (dataBarang.Stf02.BERAT / 1000).ToString(),//MO save dalam Gram, Elevenia dalam Kilogram
                                        imgUrl = imgID,
                                        Keterangan = dataBarang.Stf02.Deskripsi,
                                        Qty = Convert.ToString(qtyOnHand),
                                        DeliveryTempNo = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).DeliveryTempElevenia.ToString(),
                                        IDMarket = tblCustomer.RecNum.ToString(),
                                    };
                                    data.Brand = ErasoftDbContext.STF02E.SingleOrDefault(m => m.KODE == dataBarang.Stf02.Sort2 && m.LEVEL == "2").KET;
                                    data.Price = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).HJUAL.ToString();
                                    data.kode_mp = Convert.ToString(ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).BRG_MP);

                                    if (string.IsNullOrEmpty(data.kode_mp))
                                    {
                                        var display = Convert.ToBoolean(ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG) && m.IDMARKET == tblCustomer.RecNum).DISPLAY);
                                        var result = new EleveniaController().CreateProduct(data, display);
                                    }
                                    else
                                    if (!string.IsNullOrEmpty(data.kode_mp))
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
                                else
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

        public ActionResult EditBarang(string barangId)
        {
            try
            {
                var vm = new BarangViewModel()
                {
                    Stf02 = ErasoftDbContext.STF02.Single(b => b.BRG == barangId),
                    ListStf02S = ErasoftDbContext.STF02.ToList(),
                    ListCategoryBlibli = MoDbContext.CategoryBlibli.Where(p => string.IsNullOrEmpty(p.PARENT_CODE)).ToList(),
                    ListMarket = ErasoftDbContext.ARF01.OrderBy(p => p.RecNum).ToList(),
                    ListHargaJualPermarketView = ErasoftDbContext.STF02H.Where(h => h.BRG == barangId).OrderBy(p => p.IDMARKET).ToList()
                };

                return PartialView("FormBarangPartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        public ActionResult RefreshFormBarang()
        {
            var vm = new BarangViewModel()
            {
                ListKategoriMerk = ErasoftDbContext.STF02E.ToList(),
                ListMarket = ErasoftDbContext.ARF01.OrderBy(p => p.RecNum).ToList(),
                ListHargaJualPermarketView = ErasoftDbContext.STF02H.OrderBy(p => p.IDMARKET).ToList(),
                ListCategoryBlibli = MoDbContext.CategoryBlibli.Where(p => string.IsNullOrEmpty(p.PARENT_CODE)).ToList(),
                DataUsaha = ErasoftDbContext.SIFSYS.Single(p => p.BLN == 1)
            };

            return PartialView("FormBarangPartial", vm);
        }

        public ActionResult DeleteBarang(string barangId)
        {
            var barangInDb = ErasoftDbContext.STF02.Single(b => b.BRG == barangId);

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
                ListStf02S = ErasoftDbContext.STF02.ToList()
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

        [Route("manage/promptDeliveryProvLazada")]
        public ActionResult PromptDeliveryProvLazada(string cust)
        {
            try
            {
                var PromptModel = ErasoftDbContext.DELIVERY_PROVIDER_LAZADA.Where(a => a.CUST == cust).ToList();
                return View("PromptDeliveryTempElevenia", PromptModel);
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

            if (dataSession?.User != null)
                return View("NoPermission");

            var vm = new AccountUserViewModel()
            {
                ListUser = MoDbContext.User.Where(u => u.AccountId == dataSession.Account.AccountId).ToList(),
                ListSec = MoDbContext.SecUser.ToList()
            };

            return View(vm);
        }

        public ActionResult RefreshTableAkun()
        {
            var dataSession = Session["SessionInfo"] as AccountUserViewModel;

            if (dataSession?.User != null)
                return View("NoPermission");

            var vm = new AccountUserViewModel()
            {
                ListUser = MoDbContext.User.Where(u => u.AccountId == dataSession.Account.AccountId).ToList(),
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
                return HttpNotFound();
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

                if (checkUser == null)
                {
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

                userInDb.Email = viewModel.User.Email;
                userInDb.Username = viewModel.User.Username;
                userInDb.NoHp = viewModel.User.NoHp;

                if (userInDb.Password != viewModel.User.Password)
                    userInDb.Password = viewModel.User.Password;
            }

            MoDbContext.SaveChanges();
            ModelState.Clear();

            var dataSession = Session["SessionInfo"] as AccountUserViewModel;

            if (dataSession?.User != null)
                return View("NoPermission");

            var vm = new AccountUserViewModel()
            {
                ListUser = MoDbContext.User.Where(u => u.AccountId == dataSession.Account.AccountId).ToList(),
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
                return HttpNotFound();
            }
        }

        public ActionResult DeleteUser(int? userId)
        {
            var user = MoDbContext.User.Single(u => u.UserId == userId);

            MoDbContext.User.Remove(user);
            MoDbContext.SaveChanges();

            var dataSession = Session["SessionInfo"] as AccountUserViewModel;

            if (dataSession?.User != null)
                return View("NoPermission");

            var vm = new AccountUserViewModel()
            {
                ListUser = MoDbContext.User.Where(u => u.AccountId == dataSession.Account.AccountId).ToList(),
                ListSec = MoDbContext.SecUser.ToList()
            };

            return PartialView("TableAkunPartial", vm);
        }

        // =============================================== Bagian User (END)

        // =============================================== Bagian Security (START)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveSecUser(SecurityUserViewModel dataSecurity)
        {
            if (Session["formsId"] is string[] formsId)
            {
                var userId = Session["UserId"] as long?;

                foreach (var entity in MoDbContext.SecUser.Where(s => s.UserId == userId).ToList())
                    MoDbContext.SecUser.Remove(entity);

                var dataSession = Session["SessionInfo"] as AccountUserViewModel;
                var parentsId = Session["parentsId"] as string[];
                var counter = 0;

                foreach (var form in formsId)
                {
                    var secUser = new SecUser
                    {
                        AccountId = dataSession?.Account.AccountId,
                        UserId = userId,
                        FormId = Convert.ToInt32(form),
                        ParentId = Convert.ToInt32(parentsId?[counter]),
                        Permission = true
                    };

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

                ViewData["SuccessMessage"] = $"Settingan security untuk user {secuserVm.User.Username} berhasil disimpan.";

                return View("SecUserMenu", secuserVm);
            }

            return RedirectToAction("SecUserMenu");
        }

        [HttpPost]
        public EmptyResult PassSecurityArray(string[] secArray)
        {
            Session["formsId"] = secArray;

            return new EmptyResult();
        }

        [HttpPost]
        public EmptyResult PassParentSecurityArray(string[] secArray)
        {
            Session["parentsId"] = secArray;

            return new EmptyResult();
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
                var qtyOnHand = 0d;
                {
                    object[] spParams = {
                    new SqlParameter("@BRG",dataVm.FakturDetail.BRG),
                    new SqlParameter("@GD",dataVm.FakturDetail.GUDANG),
                    new SqlParameter("@Satuan", "2"),
                    new SqlParameter("@THN", Convert.ToInt16(DateTime.Now.ToString("yyyy"))),
                    new SqlParameter("@QOH", SqlDbType.Decimal) {Direction = ParameterDirection.Output}};

                    ErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);
                    qtyOnHand = Convert.ToDouble(((SqlParameter)spParams[4]).Value);
                }

                if (qtyOnHand < dataVm.FakturDetail.QTY)
                {
                    //var vmError = new FakturViewModel()
                    //{
                    //    Faktur = ErasoftDbContext.SIT01A.Single(p => p.NO_BUKTI == dataVm.Faktur.NO_BUKTI && p.JENIS_FORM == "2"),
                    //    ListFakturDetail = ErasoftDbContext.SIT01B.Where(pd => pd.NO_BUKTI == dataVm.Faktur.NO_BUKTI && pd.JENIS_FORM == "2").ToList(),
                    //    ListBarang = ErasoftDbContext.STF02.ToList(),
                    //    ListPembeli = ErasoftDbContext.ARF01C.ToList(),
                    //    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    //    ListMarketplace = MoDbContext.Marketplaces.ToList()
                    //};
                    dataVm.Errors.Add("Qty penjualan melebihi qty yang ada di gudang ( " + Convert.ToString(qtyOnHand) + " )");
                    return Json(dataVm, JsonRequestBehavior.AllowGet);
                }
                //end add by calvin, validasi QOH

                //var year = Convert.ToInt16(DateTime.Now.ToString("yyyy"));
                //var barangForCheck = ErasoftDbContext.STF08A.SingleOrDefault(b =>
                //    b.BRG == dataVm.FakturDetail.BRG && b.GD == dataVm.FakturDetail.GUDANG && b.Tahun == year);
                //var qtyOnHand = 0d;

                //if (barangForCheck != null)
                //{
                //    qtyOnHand = barangForCheck.QAwal + barangForCheck.QM1 + barangForCheck.QM2 + barangForCheck.QM3 + barangForCheck.QM4
                //                + barangForCheck.QM5 + barangForCheck.QM6 + barangForCheck.QM7 + barangForCheck.QM8 + barangForCheck.QM9
                //                + barangForCheck.QM10 + barangForCheck.QM11 + barangForCheck.QM12 - barangForCheck.QK1 - barangForCheck.QK2
                //                - barangForCheck.QK3 - barangForCheck.QK4 - barangForCheck.QK5 - barangForCheck.QK6 - barangForCheck.QK7
                //                - barangForCheck.QK8 - barangForCheck.QK9 - barangForCheck.QK10 - barangForCheck.QK11 - barangForCheck.QK12;
                //}

                //if (qtyOnHand < dataVm.FakturDetail.QTY)
                //{
                //    dataVm.Errors.Add("Qty penjualan tidak boleh melebihi qty yang ada di gudang!");
                //    return Json(dataVm, JsonRequestBehavior.AllowGet);
                //}

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

                //var year = Convert.ToInt16(DateTime.Now.ToString("yyyy"));
                //var barangForCheck = ErasoftDbContext.STF08A.SingleOrDefault(b =>
                //    b.BRG == dataVm.FakturDetail.BRG && b.GD == dataVm.FakturDetail.GUDANG && b.Tahun == year);
                //var qtyOnHand = 0d;

                //if (barangForCheck != null)
                //{
                //    qtyOnHand = barangForCheck.QAwal + barangForCheck.QM1 + barangForCheck.QM2 + barangForCheck.QM3 + barangForCheck.QM4
                //                + barangForCheck.QM5 + barangForCheck.QM6 + barangForCheck.QM7 + barangForCheck.QM8 + barangForCheck.QM9
                //                + barangForCheck.QM10 + barangForCheck.QM11 + barangForCheck.QM12 - barangForCheck.QK1 - barangForCheck.QK2
                //                - barangForCheck.QK3 - barangForCheck.QK4 - barangForCheck.QK5 - barangForCheck.QK6 - barangForCheck.QK7
                //                - barangForCheck.QK8 - barangForCheck.QK9 - barangForCheck.QK10 - barangForCheck.QK11 - barangForCheck.QK12;
                //}

                //if (qtyOnHand < dataVm.FakturDetail.QTY)
                //{
                //    dataVm.Errors.Add("Qty penjualan tidak boleh melebihi qty yang ada di gudang!");
                //    return Json(dataVm, JsonRequestBehavior.AllowGet);
                //}

                //add by calvin, 22 juni 2018 validasi QOH
                var qtyOnHand = 0d;
                {
                    object[] spParams = {
                    new SqlParameter("@BRG",dataVm.FakturDetail.BRG),
                    new SqlParameter("@GD",dataVm.FakturDetail.GUDANG),
                    new SqlParameter("@Satuan", "2"),
                    new SqlParameter("@THN", Convert.ToInt16(DateTime.Now.ToString("yyyy"))),
                    new SqlParameter("@QOH", SqlDbType.Decimal) {Direction = ParameterDirection.Output}};

                    ErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);
                    qtyOnHand = Convert.ToDouble(((SqlParameter)spParams[4]).Value);
                }

                if (qtyOnHand < dataVm.FakturDetail.QTY)
                {
                    //var vmError = new FakturViewModel()
                    //{
                    //    Faktur = ErasoftDbContext.SIT01A.Single(p => p.NO_BUKTI == dataVm.Faktur.NO_BUKTI && p.JENIS_FORM == "2"),
                    //    ListFakturDetail = ErasoftDbContext.SIT01B.Where(pd => pd.NO_BUKTI == dataVm.Faktur.NO_BUKTI && pd.JENIS_FORM == "2").ToList(),
                    //    ListBarang = ErasoftDbContext.STF02.ToList(),
                    //    ListPembeli = ErasoftDbContext.ARF01C.ToList(),
                    //    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    //    ListMarketplace = MoDbContext.Marketplaces.ToList()
                    //};

                    dataVm.Errors.Add("Qty penjualan melebihi qty yang ada di gudang ( " + Convert.ToString(qtyOnHand) + " )");
                    //return PartialView("BarangFakturPartial", vmError);
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

            var vm = new FakturViewModel()
            {
                Faktur = ErasoftDbContext.SIT01A.Single(p => p.NO_BUKTI == dataVm.Faktur.NO_BUKTI && p.JENIS_FORM == "2"),
                ListFakturDetail = ErasoftDbContext.SIT01B.Where(pd => pd.NO_BUKTI == dataVm.Faktur.NO_BUKTI && pd.JENIS_FORM == "2").ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListNFaktur = ErasoftDbContext.ART03B.ToList()
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
                //UPDATE ANAK
                var FakturDetailDB = ErasoftDbContext.SIT01B.Single(p => p.NO_BUKTI == dataVm.Faktur.NO_BUKTI && p.BRG == dataVm.FakturDetail.BRG);
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
            }

            // autoload detail item, jika buat retur baru
            if (returBaru)
            {
                object[] spParams = {
                new SqlParameter("@NOBUK",dataVm.Faktur.NO_BUKTI),
                new SqlParameter("@NO_REF",dataVm.Faktur.NO_REF)
                };

                ErasoftDbContext.Database.ExecuteSqlCommand("exec [SP_AUTOLOADRETUR_PENJUALAN] @NOBUK, @NO_REF", spParams);
            }
            ModelState.Clear();

            var vm = new FakturViewModel()
            {
                Faktur = ErasoftDbContext.SIT01A.Single(p => p.NO_BUKTI == dataVm.Faktur.NO_BUKTI && p.JENIS_FORM == "3"),
                ListFakturDetail = ErasoftDbContext.SIT01B.Where(pd => pd.NO_BUKTI == dataVm.Faktur.NO_BUKTI && pd.JENIS_FORM == "3").ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList()
            };

            return PartialView("BarangReturPartial", vm);
        }

        public ActionResult RefreshTableFaktur1()
        {
            var vm = new FakturViewModel()
            {
                ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2").ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListNFaktur = ErasoftDbContext.ART03B.ToList(),
                ListPesanan = ErasoftDbContext.SOT01A.ToList()
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
                ListBarang = ErasoftDbContext.STF02.ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListNFaktur = ErasoftDbContext.ART03B.ToList(),
                ListPesanan = ErasoftDbContext.SOT01A.ToList(),
            };

            return PartialView("TableFakturLunasPartial", vm);
        }
        public ActionResult RefreshTableFakturTempo()
        {
            //IEnumerable<ART01D> FakturJatuhTempo = ErasoftDbContext.ART01D.Where(a => a.NETTO.Value - a.KREDIT.Value > 0);
            var vm = new FakturViewModel()
            {
                ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2" && f.TGL_JT_TEMPO <= DateTime.Now).ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList(),
                    ListNFaktur = ErasoftDbContext.ART03B.ToList()
                };

                return PartialView("BarangFakturPartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        public ActionResult RefreshReturFakturForm()
        {
            try
            {
                var vm = new FakturViewModel()
                {
                    ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "3").ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList()
                };

                return PartialView("BarangReturPartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
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
                    ListBarang = ErasoftDbContext.STF02.ToList()
                };

                return PartialView("BarangReturPartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
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
                ListBarang = ErasoftDbContext.STF02.ToList()
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
                ListBarang = ErasoftDbContext.STF02.ToList()
            };

            return PartialView("BarangReturPartial", vm);
        }

        public ActionResult DeleteFaktur(int? orderId)
        {
            var fakturInDb = ErasoftDbContext.SIT01A.Single(p => p.RecNum == orderId && p.JENIS_FORM == "2");

            ErasoftDbContext.SIT01A.Remove(fakturInDb);
            ErasoftDbContext.SaveChanges();

            var vm = new FakturViewModel()
            {
                ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2").ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListPesanan = ErasoftDbContext.SOT01A.ToList(),
                ListNFaktur = ErasoftDbContext.ART03B.ToList()
            };

            return PartialView("TableFakturPartial", vm);
        }

        public ActionResult DeleteReturFaktur(int? orderId)
        {
            var returFakturInDb = ErasoftDbContext.SIT01A.Single(p => p.RecNum == orderId && p.JENIS_FORM == "3");
            var fakturInDbWithRef = ErasoftDbContext.SIT01A.Single(p => p.NO_BUKTI == returFakturInDb.NO_REF && p.JENIS_FORM == "2");
            fakturInDbWithRef.NO_REF = "";

            ErasoftDbContext.SIT01A.Remove(returFakturInDb);
            ErasoftDbContext.SaveChanges();

            var vm = new FakturViewModel()
            {
                ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "3").ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                fakturInDb.NETTO = fakturInDb.BRUTO - fakturInDb.NILAI_DISC + fakturInDb.NILAI_PPN;

                ErasoftDbContext.SIT01B.Remove(barangFakturInDb);
                ErasoftDbContext.SaveChanges();

                var vm = new FakturViewModel()
                {
                    Faktur = ErasoftDbContext.SIT01A.Single(p => p.NO_BUKTI == fakturInDb.NO_BUKTI && p.JENIS_FORM == "2"),
                    ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2").ToList(),
                    ListFakturDetail = ErasoftDbContext.SIT01B.Where(pd => pd.NO_BUKTI == fakturInDb.NO_BUKTI && pd.JENIS_FORM == "2").ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList()
                };

                return PartialView("BarangFakturPartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        [HttpGet]
        public ActionResult DeleteBarangReturFaktur(int noUrut)
        {
            try
            {
                var barangFakturInDb = ErasoftDbContext.SIT01B.Single(b => b.NO_URUT == noUrut && b.JENIS_FORM == "3");
                var fakturInDb = ErasoftDbContext.SIT01A.Single(p => p.NO_BUKTI == barangFakturInDb.NO_BUKTI && p.JENIS_FORM == "3");

                fakturInDb.BRUTO -= barangFakturInDb.HARGA;
                fakturInDb.NILAI_PPN = Math.Ceiling((double)fakturInDb.PPN * (double)fakturInDb.BRUTO / 100);
                fakturInDb.NETTO = fakturInDb.BRUTO - fakturInDb.NILAI_DISC + fakturInDb.NILAI_PPN;

                ErasoftDbContext.SIT01B.Remove(barangFakturInDb);
                ErasoftDbContext.SaveChanges();

                var vm = new FakturViewModel()
                {
                    Faktur = ErasoftDbContext.SIT01A.Single(p => p.NO_BUKTI == fakturInDb.NO_BUKTI && p.JENIS_FORM == "3"),
                    ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2").ToList(),
                    ListFakturDetail = ErasoftDbContext.SIT01B.Where(pd => pd.NO_BUKTI == fakturInDb.NO_BUKTI && pd.JENIS_FORM == "3").ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList()
                };

                return PartialView("BarangReturPartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
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

        [HttpGet]
        public ActionResult GetInvoiceBySupp(string kodeSupplier)
        {
            var listInvoice = ErasoftDbContext.PBT01A
                                .Where(f => f.JENISFORM == "1" && f.SUPP == kodeSupplier)
                                .OrderBy(f => f.INV).ThenByDescending(f => f.TGLINPUT).ToList();
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

            var vm = new InvoiceViewModel()
            {
                Invoice = ErasoftDbContext.PBT01A.Single(p => p.INV == dataVm.Invoice.INV && p.JENISFORM == "1"),
                ListInvoiceDetail = ErasoftDbContext.PBT01B.Where(pd => pd.INV == dataVm.Invoice.INV && pd.JENISFORM == "1").ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
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

                    //var year = Convert.ToInt16(DateTime.Now.ToString("yyyy"));
                    //var barangForCheck = ErasoftDbContext.STF08A.SingleOrDefault(b =>
                    //    b.BRG == dataVm.InvoiceDetail.BRG && b.GD == dataVm.InvoiceDetail.GD && b.Tahun == year);
                    //var qtyOnHand = 0d;
                    //if (barangForCheck != null)
                    //{
                    //    qtyOnHand = barangForCheck.QAwal + barangForCheck.QM1 + barangForCheck.QM2 + barangForCheck.QM3 + barangForCheck.QM4
                    //                + barangForCheck.QM5 + barangForCheck.QM6 + barangForCheck.QM7 + barangForCheck.QM8 + barangForCheck.QM9
                    //                + barangForCheck.QM10 + barangForCheck.QM11 + barangForCheck.QM12 - barangForCheck.QK1 - barangForCheck.QK2
                    //                - barangForCheck.QK3 - barangForCheck.QK4 - barangForCheck.QK5 - barangForCheck.QK6 - barangForCheck.QK7
                    //                - barangForCheck.QK8 - barangForCheck.QK9 - barangForCheck.QK10 - barangForCheck.QK11 - barangForCheck.QK12;
                    //}
                    //if (qtyOnHand < dataVm.InvoiceDetail.QTY)
                    //{
                    //    dataVm.Errors.Add("Qty retur pembelian tidak boleh melebihi qty yang ada di gudang!");
                    //    return Json(dataVm, JsonRequestBehavior.AllowGet);
                    //}

                    var returInDb = ErasoftDbContext.PBT01A.SingleOrDefault(f => f.INV == dataVm.Invoice.REF);
                    if (returInDb != null)
                    {
                        dataVm.Invoice.TERM = returInDb.TERM;
                        dataVm.Invoice.TGJT = returInDb.TGJT;
                        dataVm.Invoice.BRUTO = returInDb.BRUTO;
                        dataVm.Invoice.NETTO = returInDb.NETTO;
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
                var invoiceInDb = ErasoftDbContext.PBT01A.Single(p => p.INV == dataVm.Invoice.INV && p.JENISFORM == "2");

                //var year = Convert.ToInt16(DateTime.Now.ToString("yyyy"));
                //var barangForCheck = ErasoftDbContext.STF08A.SingleOrDefault(b =>
                //    b.BRG == dataVm.InvoiceDetail.BRG && b.GD == dataVm.InvoiceDetail.GD && b.Tahun == year);
                //var qtyOnHand = 0d;

                //if (barangForCheck != null)
                //{
                //    qtyOnHand = barangForCheck.QAwal + barangForCheck.QM1 + barangForCheck.QM2 + barangForCheck.QM3 + barangForCheck.QM4
                //                + barangForCheck.QM5 + barangForCheck.QM6 + barangForCheck.QM7 + barangForCheck.QM8 + barangForCheck.QM9
                //                + barangForCheck.QM10 + barangForCheck.QM11 + barangForCheck.QM12 - barangForCheck.QK1 - barangForCheck.QK2
                //                - barangForCheck.QK3 - barangForCheck.QK4 - barangForCheck.QK5 - barangForCheck.QK6 - barangForCheck.QK7
                //                - barangForCheck.QK8 - barangForCheck.QK9 - barangForCheck.QK10 - barangForCheck.QK11 - barangForCheck.QK12;
                //}

                //if (qtyOnHand < dataVm.InvoiceDetail.QTY)
                //{
                //    dataVm.Errors.Add("Qty retur pembelian tidak boleh melebihi qty yang ada di gudang!");
                //    return Json(dataVm, JsonRequestBehavior.AllowGet);
                //}


                //UPDATE ANAK
                var invDetailDb = ErasoftDbContext.PBT01B.Single(p => p.INV == dataVm.Invoice.INV && p.BRG == dataVm.InvoiceDetail.BRG);
                invDetailDb.QTY = dataVm.InvoiceDetail.QTY;
                invDetailDb.NILAI_DISC_1 = dataVm.InvoiceDetail.NILAI_DISC_1;
                invDetailDb.NILAI_DISC_2 = dataVm.InvoiceDetail.NILAI_DISC_2;
                invDetailDb.THARGA = (dataVm.InvoiceDetail.QTY) * (invDetailDb.HBELI) - (invDetailDb.NILAI_DISC_1 + invDetailDb.NILAI_DISC_2);

                //UPDATE BAPAK
                invoiceInDb.NETTO = dataVm.Invoice.NETTO;
                invoiceInDb.BRUTO = dataVm.Invoice.BRUTO;
                invoiceInDb.NDISC1 = dataVm.Invoice.NDISC1;
                invoiceInDb.PPN = dataVm.Invoice.PPN;
                invoiceInDb.NILAI_PPN = dataVm.Invoice.NILAI_PPN;

                //dataVm.InvoiceDetail.INV = dataVm.Invoice.INV;
                //if (dataVm.InvoiceDetail.NO == null)
                //{
                //    ErasoftDbContext.PBT01B.Add(dataVm.InvoiceDetail);
                //}
                returBaru = false;
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
            }
            ModelState.Clear();

            var vm = new InvoiceViewModel()
            {
                Invoice = ErasoftDbContext.PBT01A.Single(p => p.INV == dataVm.Invoice.INV && p.JENISFORM == "2"),
                ListInvoiceDetail = ErasoftDbContext.PBT01B.Where(pd => pd.INV == dataVm.Invoice.INV && pd.JENISFORM == "2").ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                ListBarang = ErasoftDbContext.STF02.ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList(),
                ListNInvoice = ErasoftDbContext.APT03B.ToList()
            };

            return PartialView("TableInvoiceLunasPartial", vm);
        }

        public ActionResult RefreshTableInvoiceTempo()
        {
            var vm = new InvoiceViewModel()
            {
                ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "1" && f.TGJT <= DateTime.Now).ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList()
                };

                return PartialView("BarangInvoicePartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        public ActionResult RefreshReturInvoiceForm()
        {
            try
            {
                var vm = new InvoiceViewModel()
                {
                    ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "2").ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList()
                };

                return PartialView("BarangReturInvoicePartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
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
                    ListBarang = ErasoftDbContext.STF02.ToList()
                };

                return PartialView("BarangReturInvoicePartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
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
                    ListBarang = ErasoftDbContext.STF02.ToList()
                };

                return PartialView("BarangInvoicePartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        public ActionResult DeleteInvoice(int? orderId)
        {
            var invoiceInDb = ErasoftDbContext.PBT01A.Single(p => p.RecNum == orderId && p.JENISFORM == "1");

            //add by calvin, validasi QOH
            var invoiceDetailInDb = ErasoftDbContext.PBT01B.Where(b => b.INV == invoiceInDb.INV && b.JENISFORM == "1").ToList();
            foreach (var item in invoiceDetailInDb)
            {
                var qtyOnHand = 0d;
                {
                    object[] spParams = {
                    new SqlParameter("@BRG",item.BRG),
                    new SqlParameter("@GD",item.GD),
                    new SqlParameter("@Satuan", "2"),
                    new SqlParameter("@THN", Convert.ToInt16(DateTime.Now.ToString("yyyy"))),
                    new SqlParameter("@QOH", SqlDbType.Decimal) {Direction = ParameterDirection.Output}};

                    ErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);
                    qtyOnHand = Convert.ToDouble(((SqlParameter)spParams[4]).Value);
                }

                if (qtyOnHand - item.QTY < 0)
                {
                    var vmError = new InvoiceViewModel()
                    {

                    };
                    vmError.Errors.Add("Tidak bisa delete, Qty di gudang sisa ( " + Convert.ToString(qtyOnHand) + " )");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
            }
            //end add by calvin, validasi QOH

            ErasoftDbContext.PBT01A.Remove(invoiceInDb);
            ErasoftDbContext.SaveChanges();

            var vm = new InvoiceViewModel()
            {
                ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "1").ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
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

            ErasoftDbContext.PBT01A.Remove(invoiceInDb);
            ErasoftDbContext.SaveChanges();

            var vm = new InvoiceViewModel()
            {
                ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "2").ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                var qtyOnHand = 0d;
                {
                    object[] spParams = {
                    new SqlParameter("@BRG",barangInvoiceInDb.BRG),
                    new SqlParameter("@GD",barangInvoiceInDb.GD),
                    new SqlParameter("@Satuan", "2"),
                    new SqlParameter("@THN", Convert.ToInt16(DateTime.Now.ToString("yyyy"))),
                    new SqlParameter("@QOH", SqlDbType.Decimal) {Direction = ParameterDirection.Output}};

                    ErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);
                    qtyOnHand = Convert.ToDouble(((SqlParameter)spParams[4]).Value);
                }

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
                invoiceInDb.NILAI_PPN = Math.Ceiling((double)invoiceInDb.PPN * (double)invoiceInDb.BRUTO / 100);
                invoiceInDb.NETTO = invoiceInDb.BRUTO - invoiceInDb.NDISC1 + invoiceInDb.NILAI_PPN;

                ErasoftDbContext.PBT01B.Remove(barangInvoiceInDb);
                ErasoftDbContext.SaveChanges();

                var vm = new InvoiceViewModel()
                {
                    Invoice = ErasoftDbContext.PBT01A.Single(p => p.INV == invoiceInDb.INV && p.JENISFORM == "1"),
                    ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "2").ToList(),
                    ListInvoiceDetail = ErasoftDbContext.PBT01B.Where(pd => pd.INV == invoiceInDb.INV && pd.JENISFORM == "1").ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList()
                };

                return PartialView("BarangInvoicePartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
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
                invoiceInDb.NILAI_PPN = Math.Ceiling((double)invoiceInDb.PPN * (double)invoiceInDb.BRUTO / 100);
                invoiceInDb.NETTO = invoiceInDb.BRUTO - invoiceInDb.NDISC1 + invoiceInDb.NILAI_PPN;

                ErasoftDbContext.PBT01B.Remove(barangInvoiceInDb);
                ErasoftDbContext.SaveChanges();

                var vm = new InvoiceViewModel()
                {
                    Invoice = ErasoftDbContext.PBT01A.Single(p => p.INV == invoiceInDb.INV && p.JENISFORM == "2"),
                    ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "2").ToList(),
                    ListInvoiceDetail = ErasoftDbContext.PBT01B.Where(pd => pd.INV == invoiceInDb.INV && pd.JENISFORM == "2").ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList()

                };

                return PartialView("BarangReturInvoicePartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        [HttpPost]
        public ActionResult UpdateInvoice(UpdateData dataUpdate)
        {
            var invoiceInDb = ErasoftDbContext.PBT01A.Single(p => p.INV == dataUpdate.OrderId);
            invoiceInDb.BRUTO = dataUpdate.Bruto;
            invoiceInDb.NDISC1 = dataUpdate.NilaiDisc;
            invoiceInDb.PPN = dataUpdate.Ppn;
            invoiceInDb.NPPN = dataUpdate.Bruto * (invoiceInDb.PPN / 100);
            invoiceInDb.KODE_REF_PESANAN = dataUpdate.KodeRefPesanan;
            invoiceInDb.TGL = DateTime.ParseExact(dataUpdate.Tgl.Substring(0, 10), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            invoiceInDb.SUPP = dataUpdate.Supp;
            invoiceInDb.TERM = dataUpdate.TermInvoice;
            invoiceInDb.NAMA = ErasoftDbContext.APF01.Single(s => s.SUPP == dataUpdate.Supp).NAMA;
            invoiceInDb.TGJT = DateTime.ParseExact(dataUpdate.Tempo, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            invoiceInDb.NETTO = invoiceInDb.BRUTO - invoiceInDb.NDISC1 + invoiceInDb.NPPN;

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
            invoiceInDb.NPPN = dataUpdate.Bruto * (invoiceInDb.PPN / 100);
            //invoiceInDb.KODE_REF_PESANAN = dataUpdate.KodeRefPesanan;
            invoiceInDb.NETTO = invoiceInDb.BRUTO - invoiceInDb.NDISC1 + invoiceInDb.NPPN;

            ErasoftDbContext.SaveChanges();

            return new EmptyResult();
        }
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
        public ActionResult GetPembeli()
        {
            var listPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList();

            return Json(listPembeli, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetDataBarang()
        {
            var listBarang = ErasoftDbContext.STF02.ToList();

            return Json(listBarang, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetEkspedisi()
        {
            var listEkspedisi = MoDbContext.Ekspedisi.ToList();

            return Json(listEkspedisi, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CekJumlahPesananBulanIni(string uname)
        {
            var listPesanan = ErasoftDbContext.SOT01A.ToList();
            var jumlahPesananBulanIni = listPesanan.Count(p => p.TGL?.Month == DateTime.Today.Month);
            var accInDb = MoDbContext.Account.FirstOrDefault(a => a.Username == uname);

            if (accInDb == null)
            {
                var accIdByUser = MoDbContext.User.FirstOrDefault(u => u.Username == uname)?.AccountId;
                accInDb = MoDbContext.Account.FirstOrDefault(a => a.AccountId == accIdByUser);
            }

            var accSubs = MoDbContext.Subscription.FirstOrDefault(s => s.KODE == accInDb.KODE_SUBSCRIPTION);

            var valSubs = new ValidasiSubs()
            {
                JumlahPesananBulanIni = jumlahPesananBulanIni,
                JumlahPesananMax = accSubs?.JUMLAH_PESANAN,
                SudahSampaiBatasTanggal = (accInDb?.TGL_SUBSCRIPTION <= DateTime.Today.Date && accInDb.KODE_SUBSCRIPTION != "01")
            };

            return Json(valSubs, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPesananInfo(string nobuk)
        {
            var pesananInDb = ErasoftDbContext.SOT01A.Single(p => p.NO_BUKTI == nobuk);
            var marketInDb = ErasoftDbContext.ARF01.Single(m => m.CUST == pesananInDb.CUST);
            var idMarket = Convert.ToInt32(marketInDb.NAMA);
            var namaMarketplace = MoDbContext.Marketplaces.Single(m => m.IdMarket == idMarket).NamaMarket;
            var namaAkunMarket = $"{namaMarketplace} ({marketInDb.PERSO})";
            var namaBuyer = ErasoftDbContext.ARF01C.Single(b => b.BUYER_CODE == pesananInDb.PEMESAN).NAMA;

            var infoPesanan = new InfoPesanan()
            {
                NoPesanan = pesananInDb.NO_BUKTI,
                TglPesanan = pesananInDb.TGL?.ToString("dd/MM/yyyy"),
                Marketplace = namaAkunMarket,
                Pembeli = namaBuyer,
                Total = String.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:N}", pesananInDb.NETTO)
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

                dataVm.PesananDetail.NO_BUKTI = dataVm.Pesanan.NO_BUKTI;
                dataVm.PesananDetail.NILAI_DISC = dataVm.PesananDetail.NILAI_DISC_1 + dataVm.PesananDetail.NILAI_DISC_2;

                if (dataVm.PesananDetail.NO_URUT == null)
                {
                    ErasoftDbContext.SOT01B.Add(dataVm.PesananDetail);
                }
            }

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            var vm = new PesananViewModel()
            {
                Pesanan = ErasoftDbContext.SOT01A.Single(p => p.NO_BUKTI == dataVm.Pesanan.NO_BUKTI),
                ListPesananDetail = ErasoftDbContext.SOT01B.Where(pd => pd.NO_BUKTI == dataVm.Pesanan.NO_BUKTI).ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                if (pesananInDb.TRACKING_SHIPMENT.Trim() == "")
                {

                    var vmError = new StokViewModel();
                    vmError.Errors.Add("Resi belum diisi");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }

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

            pesananInDb.STATUS_TRANSAKSI = tipeStatus;
            ErasoftDbContext.SaveChanges();
            //add by Tri, call marketplace api to update order status
            ChangeStatusPesanan(pesananInDb.NO_BUKTI, pesananInDb.STATUS_TRANSAKSI);
            //end add by Tri, call marketplace api to update order status
            return new EmptyResult();
        }

        public ActionResult RefreshTablePesanan()
        {
            var vm = new PesananViewModel()
            {
                ListPesanan = ErasoftDbContext.SOT01A.Where(p => p.STATUS_TRANSAKSI == "0").ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList()
            };

            return PartialView("TablePesananPartial", vm);
        }

        public ActionResult RefreshGudangQtyPesanan(string noBuk)
        {
            var vm = new PesananViewModel()
            {
                ListPesananDetail = ErasoftDbContext.SOT01B.Where(b => b.NO_BUKTI == noBuk).ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList()
            };

            return PartialView("GudangQtyPartial", vm);
        }

        public ActionResult RefreshTablePesananSudahDibayar()
        {
            var vm = new PesananViewModel()
            {
                ListPesanan = ErasoftDbContext.SOT01A.Where(p => p.STATUS_TRANSAKSI == "01").ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
                ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                ListMarketplace = MoDbContext.Marketplaces.ToList()
            };

            return PartialView("TablePesananSudahDibayarPartial", vm);
        }

        public ActionResult RefreshTablePesananSiapKirim()
        {
            var vm = new PesananViewModel()
            {
                ListPesanan = ErasoftDbContext.SOT01A.Where(p => p.STATUS_TRANSAKSI == "02").ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList()
                };

                return PartialView("BarangPesananPartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
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
                    ListBarang = ErasoftDbContext.STF02.ToList()
                };

                return PartialView("BarangPesananPartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        //add by Tri, call marketplace api to change status
        [HttpGet]
        public void ChangeStatusPesanan(string nobuk, string status)
        {
            var pesanan = ErasoftDbContext.SOT01A.Single(p => p.NO_BUKTI == nobuk);
            var marketPlace = ErasoftDbContext.ARF01.Single(p => p.CUST == pesanan.CUST);
            var mp = MoDbContext.Marketplaces.Single(p => p.IdMarket.ToString() == marketPlace.NAMA);
            var blAPI = new BukaLapakController();
            switch (status)
            {
                case "02":
                    if (mp.NamaMarket.ToUpper().Contains("BUKALAPAK"))
                    {

                    }
                    break;
                case "03":
                    if (mp.NamaMarket.ToUpper().Contains("BUKALAPAK"))
                    {
                        if (!string.IsNullOrEmpty(pesanan.TRACKING_SHIPMENT))
                            blAPI.KonfirmasiPengiriman(/*nobuk,*/ pesanan.TRACKING_SHIPMENT, pesanan.NO_REFERENSI, pesanan.SHIPMENT, marketPlace.API_KEY, marketPlace.TOKEN);
                    }
                    break;
            }

        }
        //end add by Tri, call marketplace api to change status

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
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListEkspedisi = MoDbContext.Ekspedisi.ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList()
                };

                return PartialView("BarangPesananSelesaiPartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
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

            ErasoftDbContext.SOT01A.Remove(pesananInDb);
            ErasoftDbContext.SaveChanges();

            var vm = new PesananViewModel()
            {
                ListPesanan = ErasoftDbContext.SOT01A.Where(p => p.STATUS_TRANSAKSI == "00").ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
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

                var vm = new PesananViewModel()
                {
                    Pesanan = ErasoftDbContext.SOT01A.Single(p => p.NO_BUKTI == pesananInDb.NO_BUKTI),
                    ListPesanan = ErasoftDbContext.SOT01A.ToList(),
                    ListPesananDetail = ErasoftDbContext.SOT01B.Where(pd => pd.NO_BUKTI == pesananInDb.NO_BUKTI).ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList()
                };

                return PartialView("BarangPesananPartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
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

            return Json(pesananInDb.TRACKING_SHIPMENT, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SaveResi(int? recNum, string noResi)
        {
            var pesananInDb = ErasoftDbContext.SOT01A.Single(p => p.RecNum == recNum);
            //add by Tri, check if user input new resi
            bool changeStat = false;
            if (string.IsNullOrEmpty(pesananInDb.TRACKING_SHIPMENT))
                changeStat = true;
            //end add by Tri, check if user input new resi

            pesananInDb.TRACKING_SHIPMENT = noResi;
            ErasoftDbContext.SaveChanges();

            //add by Tri, call mp api if user input new resi
            if (changeStat)
                ChangeStatusPesanan(pesananInDb.NO_BUKTI, "03");
            //end add by Tri, call mp api if user input new resi

            return new EmptyResult();
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

                // Bagian Save Faktur Generated

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

                dataVm.Faktur.NO_BUKTI = noOrder;
                dataVm.Faktur.NO_F_PAJAK = "-";
                dataVm.Faktur.NO_SO = pesananInDb.NO_BUKTI;
                dataVm.Faktur.CUST = pesananInDb.CUST;
                dataVm.Faktur.NAMAPEMESAN = (pesananInDb.NAMAPEMESAN.Length > 20 ? pesananInDb.NAMAPEMESAN.Substring(0, 17) + "..." : pesananInDb.NAMAPEMESAN);
                dataVm.Faktur.PEMESAN = pesananInDb.PEMESAN;
                dataVm.Faktur.NAMA_CUST = ErasoftDbContext.ARF01.Single(p => p.CUST == dataVm.Faktur.CUST).PERSO;
                dataVm.Faktur.AL = ErasoftDbContext.ARF01.Single(p => p.CUST == dataVm.Faktur.CUST).AL;
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
                dataVm.Faktur.BRUTO = pesananInDb.BRUTO;
                dataVm.Faktur.PPN = pesananInDb.PPN;
                dataVm.Faktur.NILAI_PPN = pesananInDb.NILAI_PPN;
                dataVm.Faktur.DISCOUNT = pesananInDb.DISCOUNT;
                dataVm.Faktur.NILAI_DISC = pesananInDb.NILAI_DISC;
                dataVm.Faktur.MATERAI = pesananInDb.ONGKOS_KIRIM;
                dataVm.Faktur.NETTO = pesananInDb.NETTO;
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

                foreach (var pesananDetail in listBarangPesananInDb)
                {
                    dataVm.FakturDetail.NILAI_DISC = pesananDetail.NILAI_DISC_1 + pesananDetail.NILAI_DISC_2;
                    dataVm.FakturDetail.BRG = pesananDetail.BRG;
                    dataVm.FakturDetail.SATUAN = pesananDetail.SATUAN;
                    dataVm.FakturDetail.H_SATUAN = pesananDetail.H_SATUAN;
                    dataVm.FakturDetail.GUDANG = pesananDetail.LOKASI;
                    dataVm.FakturDetail.QTY = pesananDetail.QTY;
                    dataVm.FakturDetail.DISCOUNT = pesananDetail.DISCOUNT;
                    dataVm.FakturDetail.NILAI_DISC_1 = pesananDetail.NILAI_DISC_1;
                    dataVm.FakturDetail.DISCOUNT_2 = pesananDetail.DISCOUNT_2;
                    dataVm.FakturDetail.NILAI_DISC_2 = pesananDetail.NILAI_DISC_2;
                    dataVm.FakturDetail.HARGA = pesananDetail.HARGA;

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
                }

                // End Bagian Save Faktur Generated

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
            barangPesananInDb.LOKASI = gd;
            barangPesananInDb.QTY = qty;

            if (Math.Abs(barangPesananInDb.DISCOUNT) > 0)
            {
                barangPesananInDb.NILAI_DISC_1 = (barangPesananInDb.DISCOUNT * barangPesananInDb.H_SATUAN * qty) / 100;
            }

            if (Math.Abs(barangPesananInDb.DISCOUNT_2) > 0)
            {
                barangPesananInDb.NILAI_DISC_2 = (barangPesananInDb.DISCOUNT * (barangPesananInDb.H_SATUAN - barangPesananInDb.NILAI_DISC_1) * qty) / 100;
            }

            barangPesananInDb.HARGA = barangPesananInDb.H_SATUAN * qty - barangPesananInDb.NILAI_DISC_1 -
                                      barangPesananInDb.NILAI_DISC_2;

            var pesananInDb = ErasoftDbContext.SOT01A.Single(p => p.NO_BUKTI == barangPesananInDb.NO_BUKTI);
            var listBarangPesanan = ErasoftDbContext.SOT01B.Where(b => b.NO_BUKTI == pesananInDb.NO_BUKTI).ToList();
            var brutoPesanan = 0d;

            foreach (var barang in listBarangPesanan)
            {
                brutoPesanan += barang.HARGA;
            }

            pesananInDb.BRUTO = brutoPesanan;

            //add by nurul 6/8/2018
            //var ppnBaru = 0d;
            pesananInDb.NILAI_PPN = (pesananInDb.PPN * pesananInDb.BRUTO) / 100;
            //end add

            pesananInDb.NETTO = pesananInDb.BRUTO - pesananInDb.NILAI_DISC + pesananInDb.NILAI_PPN + pesananInDb.ONGKOS_KIRIM;

            ErasoftDbContext.SaveChanges();

            return new EmptyResult();
        }

        [HttpGet]
        public ActionResult LihatFaktur(string noBukPesanan)
        {
            try
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

                var vm = new FakturViewModel()
                {
                    NamaToko = namaToko,
                    NamaPerusahaan = namaPT,
                    LogoMarket = urlLogoMarket,
                    Faktur = fakturInDb,
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListFakturDetail = ErasoftDbContext.SIT01B.Where(fd => fd.NO_BUKTI == fakturInDb.NO_BUKTI).ToList()
                };

                return View(vm);
            }
            catch (Exception)
            {
                return View("NotFoundPage");
            }
        }

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
                return HttpNotFound();
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
                ListHutang = ErasoftDbContext.APT01A.ToList()
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

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            return RedirectToAction("HutangMenu");
        }

        public ActionResult RefreshTableHutang()
        {
            var vm = new SaHutangViewModel()
            {
                ListHutang = ErasoftDbContext.APT01A.ToList()
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
                return HttpNotFound();
            }
        }

        public ActionResult EditHutang(int? recNum)
        {
            try
            {
                var hutVm = new SaHutangViewModel()
                {
                    Hutang = ErasoftDbContext.APT01A.Single(h => h.RECNUM == recNum)
                };

                return PartialView("FormHutangPartial", hutVm);
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        public ActionResult DeleteHutang(int? recNum)
        {
            var hutangInDb = ErasoftDbContext.APT01A.Single(h => h.RECNUM == recNum);

            ErasoftDbContext.APT01A.Remove(hutangInDb);
            ErasoftDbContext.SaveChanges();

            var vm = new SaHutangViewModel()
            {
                ListHutang = ErasoftDbContext.APT01A.ToList()
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
                ListPiutang = ErasoftDbContext.ART01A.ToList()
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

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            return RedirectToAction("PiutangMenu");
        }

        public ActionResult RefreshTablePiutang()
        {
            var vm = new SaPiutangViewModel()
            {
                ListPiutang = ErasoftDbContext.ART01A.ToList()
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
                return HttpNotFound();
            }
        }

        public ActionResult EditPiutang(int? recNum)
        {
            try
            {
                var piuVm = new SaPiutangViewModel()
                {
                    Piutang = ErasoftDbContext.ART01A.Single(h => h.RecNum == recNum)
                };

                return PartialView("FormPiutangPartial", piuVm);
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        public ActionResult DeletePiutang(int? recNum)
        {
            var piutangInDb = ErasoftDbContext.ART01A.Single(h => h.RecNum == recNum);

            ErasoftDbContext.ART01A.Remove(piutangInDb);
            ErasoftDbContext.SaveChanges();

            var vm = new SaPiutangViewModel()
            {
                ListPiutang = ErasoftDbContext.ART01A.ToList()
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
                return HttpNotFound();
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

        [Route("manage/sa/stok")]
        public ActionResult StokMenu()
        {
            var vm = new StokViewModel()
            {
                ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("ST")).ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                    ErasoftDbContext.STT01B.Add(dataVm.BarangStok);
                }
            }

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            var vm = new StokViewModel()
            {
                Stok = ErasoftDbContext.STT01A.Single(p => p.Nobuk == dataVm.Stok.Nobuk),
                ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("ST")).ToList(),
                ListBarangStok = ErasoftDbContext.STT01B.Where(bs => bs.Nobuk == dataVm.Stok.Nobuk).ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
                ListGudang = ErasoftDbContext.STF18.ToList()
            };

            return PartialView("BarangStokPartial", vm);
        }

        public ActionResult RefreshTableStok()
        {
            var vm = new StokViewModel()
            {
                ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("ST")).ToList()
            };

            return PartialView("TableStokPartial", vm);
        }

        public ActionResult RefreshStokForm()
        {
            try
            {
                var vm = new StokViewModel()
                {
                    ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("ST")).ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                return PartialView("BarangStokPartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        public ActionResult EditStok(int? stokId)
        {
            try
            {
                var stokInDb = ErasoftDbContext.STT01A.Single(p => p.ID == stokId);

                var vm = new StokViewModel()
                {
                    Stok = stokInDb,
                    ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("ST")).ToList(),
                    ListBarangStok = ErasoftDbContext.STT01B.Where(bs => bs.Nobuk == stokInDb.Nobuk).ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                return PartialView("BarangStokPartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        public ActionResult DeleteStok(int? stokId)
        {
            var stokInDb = ErasoftDbContext.STT01A.Single(p => p.ID == stokId);

            //add by calvin, 22 juni 2018 validasi QOH
            var stokDetailInDb = ErasoftDbContext.STT01B.Where(b => b.Jenis_Form == stokInDb.Jenis_Form && b.Nobuk == stokInDb.Nobuk).ToList();
            foreach (var item in stokDetailInDb)
            {
                var qtyOnHand = 0d;
                {
                    object[] spParams = {
                    new SqlParameter("@BRG",item.Kobar),
                    new SqlParameter("@GD",item.Ke_Gd),
                    new SqlParameter("@Satuan", "2"),
                    new SqlParameter("@THN", Convert.ToInt16(DateTime.Now.ToString("yyyy"))),
                    new SqlParameter("@QOH", SqlDbType.Decimal) {Direction = ParameterDirection.Output}};

                    ErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);
                    qtyOnHand = Convert.ToDouble(((SqlParameter)spParams[4]).Value);
                }
                if (qtyOnHand - item.Qty < 0)
                {
                    var vmError = new StokViewModel()
                    {

                    };

                    var namaItem = ErasoftDbContext.STF02.Where(b => b.BRG == item.Kobar).FirstOrDefault();
                    vmError.Errors.Add("Tidak bisa delete, Qty di gudang sisa ( " + Convert.ToString(qtyOnHand) + " ) untuk item " + namaItem.NAMA + "");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
            }
            //end add by calvin, validasi QOH

            ErasoftDbContext.STT01A.Remove(stokInDb);
            ErasoftDbContext.SaveChanges();

            var vm = new StokViewModel()
            {
                ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("ST")).ToList()
            };

            return PartialView("TableStokPartial", vm);
        }

        [HttpGet]
        public ActionResult DeleteBarangStok(int noUrut)
        {
            try
            {
                var barangStokInDb = ErasoftDbContext.STT01B.Single(b => b.No == noUrut);
                var stokInDb = ErasoftDbContext.STT01A.Single(p => p.Nobuk == barangStokInDb.Nobuk);

                //add by calvin, 22 juni 2018 validasi QOH
                var qtyOnHand = 0d;
                {
                    object[] spParams = {
                    new SqlParameter("@BRG",barangStokInDb.Kobar),
                    new SqlParameter("@GD",barangStokInDb.Ke_Gd),
                    new SqlParameter("@Satuan", "2"),
                    new SqlParameter("@THN", Convert.ToInt16(DateTime.Now.ToString("yyyy"))),
                    new SqlParameter("@QOH", SqlDbType.Decimal) {Direction = ParameterDirection.Output}};

                    ErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);
                    qtyOnHand = Convert.ToDouble(((SqlParameter)spParams[4]).Value);
                }
                if (qtyOnHand - barangStokInDb.Qty < 0)
                {
                    var vmError = new StokViewModel()
                    {

                    };

                    var namaItem = ErasoftDbContext.STF02.Where(b => b.BRG == barangStokInDb.Kobar).FirstOrDefault();
                    vmError.Errors.Add("Tidak bisa delete, Qty di gudang sisa ( " + Convert.ToString(qtyOnHand) + " ) untuk item " + namaItem.NAMA + "");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
                //end add by calvin, validasi QOH

                ErasoftDbContext.STT01B.Remove(barangStokInDb);
                ErasoftDbContext.SaveChanges();

                var vm = new StokViewModel()
                {
                    Stok = ErasoftDbContext.STT01A.Single(p => p.Nobuk == stokInDb.Nobuk),
                    ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("ST")).ToList(),
                    ListBarangStok = ErasoftDbContext.STT01B.Where(bs => bs.Nobuk == stokInDb.Nobuk).ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                return PartialView("BarangStokPartial", vm);
            }
            catch (Exception ex)
            {
                return HttpNotFound();
            }
        }

        [HttpPost]
        public ActionResult UpdateStok(UpdateData dataUpdate)
        {
            var stokInDb = ErasoftDbContext.STT01A.Single(p => p.Nobuk == dataUpdate.NoBuktiStok);
            stokInDb.TglInput = DateTime.ParseExact(dataUpdate.TglInput, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            ErasoftDbContext.SaveChanges();

            return new EmptyResult();
        }

        // =============================================== Bagian SA. Stock (END)

        // =============================================== Bagian Report (START)

        [Route("manage/reports")]
        public ActionResult Reports()
        {
            return View();
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
                return HttpNotFound();
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
                return HttpNotFound();
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
                return HttpNotFound();
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

                if (dataVm.PiutangDetail.NO == null)
                {
                    ErasoftDbContext.ART03B.Add(dataVm.PiutangDetail);
                }
            }
            else
            {
                dataVm.PiutangDetail.BUKTI = dataVm.Piutang.BUKTI;

                if (dataVm.PiutangDetail.NO == null)
                {
                    ErasoftDbContext.ART03B.Add(dataVm.PiutangDetail);
                }
            }

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            var vm = new BayarPiutangViewModel()
            {
                Piutang = ErasoftDbContext.ART03A.Single(p => p.BUKTI == dataVm.PiutangDetail.BUKTI),
                ListPiutangDetail = ErasoftDbContext.ART03B.Where(pd => pd.BUKTI == dataVm.Piutang.BUKTI).ToList(),
                ListFaktur = ErasoftDbContext.SIT01A.ToList(),
                ListSisa = ErasoftDbContext.ART01D.Where(s => s.CUST == dataVm.Piutang.CUST).ToList()
            };

            return PartialView("DetailBayarPiutangPartial", vm);
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
                return HttpNotFound();
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
                return HttpNotFound();
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
                return HttpNotFound();
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
                return HttpNotFound();
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
                return HttpNotFound();
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
                return HttpNotFound();
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
                return HttpNotFound();
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
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                    ErasoftDbContext.STT01B.Add(dataVm.BarangStok);
                }
            }

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            var vm = new StokViewModel()
            {
                Stok = ErasoftDbContext.STT01A.Single(p => p.Nobuk == dataVm.Stok.Nobuk),
                ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("IN")).ToList(),
                ListBarangStok = ErasoftDbContext.STT01B.Where(bs => bs.Nobuk == dataVm.Stok.Nobuk).ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
                ListGudang = ErasoftDbContext.STF18.ToList()
            };

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
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                return PartialView("BarangTransaksiMasukPartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
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
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                return PartialView("BarangTransaksiMasukPartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        public ActionResult DeleteTransaksiMasuk(int? stokId)
        {
            var stokInDb = ErasoftDbContext.STT01A.Single(p => p.ID == stokId);

            //add by calvin, 22 juni 2018 validasi QOH
            var stokDetailInDb = ErasoftDbContext.STT01B.Where(b => b.Nobuk == stokInDb.Nobuk).ToList();
            foreach (var item in stokDetailInDb)
            {
                var qtyOnHand = 0d;
                {
                    object[] spParams = {
                    new SqlParameter("@BRG",item.Kobar),
                    new SqlParameter("@GD",item.Ke_Gd),
                    new SqlParameter("@Satuan", "2"),
                    new SqlParameter("@THN", Convert.ToInt16(DateTime.Now.ToString("yyyy"))),
                    new SqlParameter("@QOH", SqlDbType.Decimal) {Direction = ParameterDirection.Output}};

                    ErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);
                    qtyOnHand = Convert.ToDouble(((SqlParameter)spParams[4]).Value);
                }
                if (qtyOnHand - item.Qty < 0)
                {
                    var vmError = new StokViewModel()
                    {

                    };
                    vmError.Errors.Add("Tidak bisa delete, Qty di gudang sisa ( " + Convert.ToString(qtyOnHand) + " )");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
            }
            //end add by calvin, validasi QOH

            ErasoftDbContext.STT01A.Remove(stokInDb);
            ErasoftDbContext.SaveChanges();

            var vm = new StokViewModel()
            {
                ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("IN")).ToList()
            };

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
                var qtyOnHand = 0d;
                {
                    object[] spParams = {
                    new SqlParameter("@BRG",barangStokInDb.Kobar),
                    new SqlParameter("@GD",barangStokInDb.Ke_Gd),
                    new SqlParameter("@Satuan", "2"),
                    new SqlParameter("@THN", Convert.ToInt16(DateTime.Now.ToString("yyyy"))),
                    new SqlParameter("@QOH", SqlDbType.Decimal) {Direction = ParameterDirection.Output}};

                    ErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);
                    qtyOnHand = Convert.ToDouble(((SqlParameter)spParams[4]).Value);
                }
                if (qtyOnHand - barangStokInDb.Qty < 0)
                {
                    var vmError = new StokViewModel()
                    {

                    };
                    vmError.Errors.Add("Tidak bisa delete, Qty di gudang sisa ( " + Convert.ToString(qtyOnHand) + " )");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
                //end add by calvin, validasi QOH

                ErasoftDbContext.STT01B.Remove(barangStokInDb);
                ErasoftDbContext.SaveChanges();

                var vm = new StokViewModel()
                {
                    Stok = ErasoftDbContext.STT01A.Single(p => p.Nobuk == stokInDb.Nobuk),
                    ListStok = ErasoftDbContext.STT01A.Where(a => a.Nobuk.Substring(0, 2).Equals("IN")).ToList(),
                    ListBarangStok = ErasoftDbContext.STT01B.Where(bs => bs.Nobuk == stokInDb.Nobuk).ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                return PartialView("BarangTransaksiMasukPartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        [HttpPost]
        public ActionResult UpdateTransaksiMasuk(UpdateData dataUpdate)
        {
            var stokInDb = ErasoftDbContext.STT01A.Single(p => p.Nobuk == dataUpdate.NoBuktiStok);
            stokInDb.TglInput = DateTime.ParseExact(dataUpdate.TglInput, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            ErasoftDbContext.SaveChanges();

            return new EmptyResult();
        }

        // =============================================== Bagian Transaksi Masuk Barang (END)

        // =============================================== Bagian Transaksi Keluar Barang (START)

        [Route("manage/persediaan/keluar")]
        public ActionResult TransaksiKeluarMenu()
        {
            var vm = new StokViewModel()
            {
                ListStok = ErasoftDbContext.STT01A.ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                var qtyOnHand = 0d;
                {
                    object[] spParams = {
                    new SqlParameter("@BRG",dataVm.BarangStok.Kobar),
                    new SqlParameter("@GD",dataVm.BarangStok.Dr_Gd),
                    new SqlParameter("@Satuan", "2"),
                    new SqlParameter("@THN", Convert.ToInt16(DateTime.Now.ToString("yyyy"))),
                    new SqlParameter("@QOH", SqlDbType.Decimal) {Direction = ParameterDirection.Output}};

                    ErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);
                    qtyOnHand = Convert.ToDouble(((SqlParameter)spParams[4]).Value);
                }
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
                    ErasoftDbContext.STT01B.Add(dataVm.BarangStok);
                }
            }
            else
            {
                var stokInDb = ErasoftDbContext.STT01A.Single(p => p.Nobuk == dataVm.Stok.Nobuk);

                var year = Convert.ToInt16(DateTime.Now.ToString("yyyy"));
                var barangForCheck = ErasoftDbContext.STF08A.SingleOrDefault(b =>
                    b.BRG == dataVm.BarangStok.Kobar && b.GD == dataVm.BarangStok.Dr_Gd && b.Tahun == year);
                //var qtyOnHand = 0d;

                //if (barangForCheck != null)
                //{
                //    qtyOnHand = barangForCheck.QAwal + barangForCheck.QM1 + barangForCheck.QM2 + barangForCheck.QM3 + barangForCheck.QM4
                //                + barangForCheck.QM5 + barangForCheck.QM6 + barangForCheck.QM7 + barangForCheck.QM8 + barangForCheck.QM9
                //                + barangForCheck.QM10 + barangForCheck.QM11 + barangForCheck.QM12 - barangForCheck.QK1 - barangForCheck.QK2
                //                - barangForCheck.QK3 - barangForCheck.QK4 - barangForCheck.QK5 - barangForCheck.QK6 - barangForCheck.QK7
                //                - barangForCheck.QK8 - barangForCheck.QK9 - barangForCheck.QK10 - barangForCheck.QK11 - barangForCheck.QK12;
                //}

                //if (qtyOnHand < dataVm.BarangStok.Qty)
                //{
                //    dataVm.Errors.Add("Qty transaksi keluar tidak boleh melebihi qty yang ada di gudang!");
                //    return Json(dataVm, JsonRequestBehavior.AllowGet);
                //}
                //add by calvin, 22 juni 2018 validasi QOH
                var qtyOnHand = 0d;
                {
                    object[] spParams = {
                    new SqlParameter("@BRG",dataVm.BarangStok.Kobar),
                    new SqlParameter("@GD",dataVm.BarangStok.Dr_Gd),
                    new SqlParameter("@Satuan", "2"),
                    new SqlParameter("@THN", Convert.ToInt16(DateTime.Now.ToString("yyyy"))),
                    new SqlParameter("@QOH", SqlDbType.Decimal) {Direction = ParameterDirection.Output}};

                    ErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);
                    qtyOnHand = Convert.ToDouble(((SqlParameter)spParams[4]).Value);
                }
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
                    ErasoftDbContext.STT01B.Add(dataVm.BarangStok);
                }
            }

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            var vm = new StokViewModel()
            {
                Stok = ErasoftDbContext.STT01A.Single(p => p.Nobuk == dataVm.Stok.Nobuk),
                ListStok = ErasoftDbContext.STT01A.ToList(),
                ListBarangStok = ErasoftDbContext.STT01B.Where(bs => bs.Nobuk == dataVm.Stok.Nobuk).ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
                ListGudang = ErasoftDbContext.STF18.ToList()
            };

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
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                return PartialView("BarangTransaksiKeluarPartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
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
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                return PartialView("BarangTransaksiKeluarPartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        public ActionResult DeleteTransaksiKeluar(int? stokId)
        {
            var stokInDb = ErasoftDbContext.STT01A.Single(p => p.ID == stokId);

            ErasoftDbContext.STT01A.Remove(stokInDb);
            ErasoftDbContext.SaveChanges();

            var vm = new StokViewModel()
            {
                ListStok = ErasoftDbContext.STT01A.ToList()
            };

            return PartialView("TableTransaksiKeluarPartial", vm);
        }

        [HttpGet]
        public ActionResult DeleteBarangTransaksiKeluar(int noUrut)
        {
            try
            {
                var barangStokInDb = ErasoftDbContext.STT01B.Single(b => b.No == noUrut);
                var stokInDb = ErasoftDbContext.STT01A.Single(p => p.Nobuk == barangStokInDb.Nobuk);

                ErasoftDbContext.STT01B.Remove(barangStokInDb);
                ErasoftDbContext.SaveChanges();

                var vm = new StokViewModel()
                {
                    Stok = ErasoftDbContext.STT01A.Single(p => p.Nobuk == stokInDb.Nobuk),
                    ListStok = ErasoftDbContext.STT01A.ToList(),
                    ListBarangStok = ErasoftDbContext.STT01B.Where(bs => bs.Nobuk == stokInDb.Nobuk).ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                return PartialView("BarangTransaksiKeluarPartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        [HttpPost]
        public ActionResult UpdateTransaksiKeluar(UpdateData dataUpdate)
        {
            var stokInDb = ErasoftDbContext.STT01A.Single(p => p.Nobuk == dataUpdate.NoBuktiStok);
            stokInDb.TglInput = DateTime.ParseExact(dataUpdate.TglInput, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
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
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                var qtyOnHand = 0d;
                {
                    object[] spParams = {
                    new SqlParameter("@BRG",dataVm.BarangStok.Kobar),
                    new SqlParameter("@GD",dataVm.BarangStok.Dr_Gd),
                    new SqlParameter("@Satuan", "2"),
                    new SqlParameter("@THN", Convert.ToInt16(DateTime.Now.ToString("yyyy"))),
                    new SqlParameter("@QOH", SqlDbType.Decimal) {Direction = ParameterDirection.Output}};

                    ErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);
                    qtyOnHand = Convert.ToDouble(((SqlParameter)spParams[4]).Value);
                }
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
                    ErasoftDbContext.STT01B.Add(dataVm.BarangStok);
                }
            }
            else
            {
                //add by calvin, 22 juni 2018 validasi QOH
                var qtyOnHand = 0d;
                {
                    object[] spParams = {
                    new SqlParameter("@BRG",dataVm.BarangStok.Kobar),
                    new SqlParameter("@GD",dataVm.BarangStok.Dr_Gd),
                    new SqlParameter("@Satuan", "2"),
                    new SqlParameter("@THN", Convert.ToInt16(DateTime.Now.ToString("yyyy"))),
                    new SqlParameter("@QOH", SqlDbType.Decimal) {Direction = ParameterDirection.Output}};

                    ErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);
                    qtyOnHand = Convert.ToDouble(((SqlParameter)spParams[4]).Value);
                }
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
                    ErasoftDbContext.STT01B.Add(dataVm.BarangStok);
                }
            }

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            var vm = new StokViewModel()
            {
                Stok = ErasoftDbContext.STT01A.Single(p => p.Nobuk == dataVm.Stok.Nobuk),
                ListStok = ErasoftDbContext.STT01A.ToList(),
                ListBarangStok = ErasoftDbContext.STT01B.Where(bs => bs.Nobuk == dataVm.Stok.Nobuk).ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                return PartialView("BarangTransaksiPindahPartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
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
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                return PartialView("BarangTransaksiPindahPartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        public ActionResult DeleteTransaksiPindah(int? stokId)
        {
            var stokInDb = ErasoftDbContext.STT01A.Single(p => p.ID == stokId);

            //add by calvin, 25 juni 2018 validasi QOH
            var stokDetailInDb = ErasoftDbContext.STT01B.Where(b => b.Nobuk == stokInDb.Nobuk).ToList();
            foreach (var item in stokDetailInDb)
            {
                var qtyOnHand = 0d;
                {
                    object[] spParams = {
                    new SqlParameter("@BRG",item.Kobar),
                    new SqlParameter("@GD",item.Ke_Gd),
                    new SqlParameter("@Satuan", "2"),
                    new SqlParameter("@THN", Convert.ToInt16(DateTime.Now.ToString("yyyy"))),
                    new SqlParameter("@QOH", SqlDbType.Decimal) {Direction = ParameterDirection.Output}};

                    ErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);
                    qtyOnHand = Convert.ToDouble(((SqlParameter)spParams[4]).Value);
                }
                if (qtyOnHand - item.Qty < 0)
                {
                    var vmError = new StokViewModel()
                    {

                    };
                    vmError.Errors.Add("Tidak bisa dihapus, Qty di gudang " + Convert.ToString(item.Ke_Gd) + " sisa ( " + Convert.ToString(qtyOnHand) + " )");
                    return Json(vmError, JsonRequestBehavior.AllowGet);
                }
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
                var qtyOnHand = 0d;
                {
                    object[] spParams = {
                    new SqlParameter("@BRG",barangStokInDb.Kobar),
                    new SqlParameter("@GD",barangStokInDb.Ke_Gd),
                    new SqlParameter("@Satuan", "2"),
                    new SqlParameter("@THN", Convert.ToInt16(DateTime.Now.ToString("yyyy"))),
                    new SqlParameter("@QOH", SqlDbType.Decimal) {Direction = ParameterDirection.Output}};

                    ErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);
                    qtyOnHand = Convert.ToDouble(((SqlParameter)spParams[4]).Value);
                }
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
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListGudang = ErasoftDbContext.STF18.ToList()
                };

                return PartialView("BarangTransaksiPindahPartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        [HttpPost]
        public ActionResult UpdateTransaksiPindah(UpdateData dataUpdate)
        {
            var stokInDb = ErasoftDbContext.STT01A.Single(p => p.Nobuk == dataUpdate.NoBuktiStok);
            stokInDb.TglInput = DateTime.ParseExact(dataUpdate.TglInput, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
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

        [HttpGet]
        public ActionResult CekKetMerk(string kode)
        {
            var res = new CekKode()
            {
                Kode = kode
            };

            var gudangInDb = ErasoftDbContext.STF02E.FirstOrDefault(k => k.LEVEL == "2" && k.KET == kode);
            if (gudangInDb != null) res.Available = false;

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
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
                DataUsaha = ErasoftDbContext.SIFSYS.Single(p => p.BLN == 1),
                DataUsahaTambahan = ErasoftDbContext.SIFSYS_TAMBAHAN.Single(p => p.RecNum == 1)
            };

            return View(dataPerusahaanVm);
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
                return HttpNotFound();
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
            dataPerusahaanInDb.BCA_API_KEY = dataVm.DataUsaha.BCA_API_KEY;
            dataPerusahaanInDb.BCA_API_SECRET = dataVm.DataUsaha.BCA_API_SECRET;
            dataPerusahaanInDb.BCA_CLIENT_ID = dataVm.DataUsaha.BCA_CLIENT_ID;
            dataPerusahaanInDb.BCA_CLIENT_SECRET = dataVm.DataUsaha.BCA_CLIENT_SECRET;

            var dataPerusahaanTambahanInDb = ErasoftDbContext.SIFSYS_TAMBAHAN.Single(p => p.RecNum == 1);
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

        // =============================================== Bagian Promosi (START)

        [Route("manage/master/promosi-barang")]
        public ActionResult Promosi()
        {
            var vm = new PromosiViewModel()
            {
                ListPromosi = ErasoftDbContext.PROMOSI.ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList()
                };

                return PartialView("BarangPromosiPartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        public ActionResult RefreshTablePromosi()
        {
            var vm = new PromosiViewModel()
            {
                ListPromosi = ErasoftDbContext.PROMOSI.ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                ListBarang = ErasoftDbContext.STF02.ToList()
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

            var vm = new PromosiViewModel()
            {
                ListPromosi = ErasoftDbContext.PROMOSI.ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
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

                var vm = new PromosiViewModel()
                {
                    Promosi = ErasoftDbContext.PROMOSI.Single(p => p.RecNum == promosiInDb.RecNum),
                    ListPromosi = ErasoftDbContext.PROMOSI.ToList(),
                    ListPromosiDetail = ErasoftDbContext.DETAILPROMOSI.Where(pd => pd.RecNumPromosi == promosiInDb.RecNum).ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList()
                };

                return PartialView("BarangPromosiPartial", vm);
            }
            catch (Exception)
            {
                return HttpNotFound();
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
                    dataVm.PromosiDetail.RecNumPromosi = lastRecNum;
                    ErasoftDbContext.DETAILPROMOSI.Add(dataVm.PromosiDetail);
                }
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
                }
            }

            ErasoftDbContext.SaveChanges();
            ModelState.Clear();

            var vm = new PromosiViewModel()
            {
                Promosi = ErasoftDbContext.PROMOSI.Single(p => p.RecNum == dataVm.Promosi.RecNum),
                ListPromosiDetail = ErasoftDbContext.DETAILPROMOSI.Where(pd => pd.RecNumPromosi == dataVm.Promosi.RecNum).ToList(),
                ListBarang = ErasoftDbContext.STF02.ToList(),
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
                ListBarang = ErasoftDbContext.STF02.ToList(),
                ListHargaJualPerMarket = ErasoftDbContext.STF02H.ToList(),
                ListHargaTerakhir = ErasoftDbContext.STF10.ToList()
            };

            return View("HargaJualMenu", vm);
        }

        // =============================================== Bagian Harga Jual Barang (END)
    }
}