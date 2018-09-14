using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using MasterOnline.Models;
using MasterOnline.Models.Api;
using MasterOnline.Utils;
using MasterOnline.ViewModels;
using Newtonsoft.Json;

namespace MasterOnline.Controllers.Api
{
    public class MobileApiController : ApiController
    {
        public MoDbContext MoDbContext { get; set; }
        public ErasoftContext ErasoftDbContext { get; set; }
        private AccountUserViewModel _viewModel;

        public MobileApiController()
        {
            MoDbContext = new MoDbContext();
            _viewModel = new AccountUserViewModel();
        }

        protected override void Dispose(bool disposing)
        {
            MoDbContext.Dispose();
        }

        [System.Web.Http.Route("api/mobile/logging")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult LoggingIn([FromBody] Account account)
        {
            JsonApi result;
            string apiKey = "";

            var re = Request;
            var headers = re.Headers;

            if (headers.Contains("X-API-KEY"))
            {
                apiKey = headers.GetValues("X-API-KEY").First();
            }

            if (apiKey != "M@STERONLINE4P1K3Y")
            {
                result = new JsonApi()
                {
                    code = 401,
                    message = "Wrong API KEY!",
                    data = null
                };

                return Json(result);
            }

            var accFromDb = MoDbContext.Account.SingleOrDefault(a => a.Email == account.Email);

            if (accFromDb == null)
            {
                var userFromDb = MoDbContext.User.SingleOrDefault(a => a.Email == account.Email);

                if (userFromDb == null)
                {
                    result = new JsonApi()
                    {
                        code = 200,
                        message = "Email tidak ditemukan!",
                        data = null
                    };

                    return Json(result);
                }

                var pass = userFromDb?.Password;

                if (!account.Password.Equals(pass))
                {
                    result = new JsonApi()
                    {
                        code = 200,
                        message = "Password salah!",
                        data = null
                    };

                    return Json(result);
                }

                if (!userFromDb.Status)
                {
                    result = new JsonApi()
                    {
                        code = 200,
                        message = "Akun belum diaktifkan oleh admin!",
                        data = null
                    };

                    return Json(result);
                }

                _viewModel.User = userFromDb;
            }
            else
            {
                var pass = accFromDb.Password;
                var hashCode = accFromDb.VCode;
                var encodingPassString = Helper.EncodePassword(account.Password, hashCode);

                if (!encodingPassString.Equals(pass))
                {
                    result = new JsonApi()
                    {
                        code = 200,
                        message = "Password salah!",
                        data = null
                    };

                    return Json(result);
                }

                if (!accFromDb.Status)
                {
                    result = new JsonApi()
                    {
                        code = 200,
                        message = "Akun belum diaktifkan oleh admin!",
                        data = null
                    };

                    return Json(result);
                }

                _viewModel.Account = accFromDb;
            }

            if (_viewModel?.Account != null)
            {
                ErasoftDbContext = _viewModel.Account.UserId == "admin_manage" ? new ErasoftContext() : new ErasoftContext(_viewModel.Account.UserId);
            }
            else
            {
                var accFromUser = MoDbContext.Account.Single(a => a.AccountId == _viewModel.User.AccountId);
                ErasoftDbContext = new ErasoftContext(accFromUser.UserId);
            }

            result = new JsonApi()
            {
                code = 200,
                message = "Login Berhasil",
                data = new
                {
                    logged = true,
                    userId = _viewModel.Account != null ? _viewModel.Account.UserId : _viewModel.User.UserId.ToString()
                }
            };

            return Json(result);
        }

        [System.Web.Http.Route("api/mobile/register")]
        [System.Web.Http.HttpPost]
        public async Task<IHttpActionResult> SaveAccount([FromBody]Account account)
        {
            JsonApi result;
            string apiKey = "";

            var re = Request;
            var headers = re.Headers;

            if (headers.Contains("X-API-KEY"))
            {
                apiKey = headers.GetValues("X-API-KEY").First();
            }

            if (apiKey != "M@STERONLINE4P1K3Y")
            {
                result = new JsonApi()
                {
                    code = 401,
                    message = "Wrong API KEY!",
                    data = null
                };

                return Json(result);
            }

            if (account.Password != account.ConfirmPassword)
            {
                result = new JsonApi()
                {
                    code = 204,
                    message = "Password dan konfirmasi password berbeda!",
                    data = null
                };

                return Json(result);
            }

            var accInDb = MoDbContext.Account.SingleOrDefault(a => a.Email == account.Email);

            if (accInDb != null)
            {
                result = new JsonApi()
                {
                    code = 204,
                    message = "Email sudah terdaftar di database kami!",
                    data = null
                };

                return Json(result);
            }

            if (String.IsNullOrWhiteSpace(account.PhotoKtpBase64))
            {
                result = new JsonApi()
                {
                    code = 204,
                    message = "Harap sertakan foto / scan KTP Anda!",
                    data = null
                };

                return Json(result);
            }

            var keyNew = Helper.GeneratePassword(10);
            var originPassword = account.Password;
            var password = Helper.EncodePassword(account.Password, keyNew);

            var email = new MailAddress(account.Email);
            account.UserId = email.User;
            account.Status = false; //User tidak aktif untuk pertama kali

            account.KODE_SUBSCRIPTION = "01"; //Free account
            account.TGL_SUBSCRIPTION = DateTime.Today.Date; //First time subs
            account.Password = password;
            account.ConfirmPassword = password;
            account.VCode = keyNew;

            byte[] bytes = Convert.FromBase64String(account.PhotoKtpBase64);
            using (Image image = Image.FromStream(new MemoryStream(bytes)))
            {
                var path = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/Content/Uploaded/"), "KTP-" + account.UserId + ".jpg");
                account.PhotoKtpUrl = "~/Content/Uploaded/KTP-" + account.UserId + ".jpg";
                image.Save(path, ImageFormat.Jpeg);
            }

            MoDbContext.Account.Add(account);
            MoDbContext.SaveChanges();

            var body = "<p>Selamat, akun Anda berhasil didaftarkan pada sistem kami&nbsp;<img src=\"https://html-online.com/editor/tinymce4_6_5/plugins/emoticons/img/smiley-laughing.gif\" alt=\"laughing\" /></p>" +
                       "<p>&nbsp;</p>" +
                       "<p>Detail akun Anda ialah sebagai berikut,</p>" +
                       "<p>Email: {0}</p>" +
                       "<p>Password: {1}</p>" +
                       "<p>Semoga sukses selalu dalam bisnis Anda di MasterOnline.</p><p>&nbsp;</p>" +
                       "<p>Best regards,</p>" +
                       "<p>CS MasterOnline.</p>";

            var message = new MailMessage();
            message.To.Add(email);
            message.From = new MailAddress("csmasteronline@gmail.com");
            message.Subject = "Pendaftaran MasterOnline berhasil!";
            message.Body = string.Format(body, account.Email, originPassword);
            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = "csmasteronline@gmail.com",
                    Password = "erasoft123"
                };
                smtp.Credentials = credential;
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(message);
            }

            result = new JsonApi()
            {
                code = 200,
                message = "Kami telah menerima pendaftaran Anda. Silakan menunggu approval dari admin kami, terima kasih.",
                data = null
            };

            return Json(result);
        }

        [System.Web.Http.Route("api/mobile/dashboard")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult DashboardResult([FromBody]JsonData data)
        {
            try
            {
                JsonApi result;
                string apiKey = "";

                var re = Request;
                var headers = re.Headers;

                if (headers.Contains("X-API-KEY"))
                {
                    apiKey = headers.GetValues("X-API-KEY").First();
                }

                if (apiKey != "M@STERONLINE4P1K3Y")
                {
                    result = new JsonApi()
                    {
                        code = 401,
                        message = "Wrong API KEY!",
                        data = null
                    };

                    return Json(result);
                }

                ErasoftDbContext = data.UserId == "admin_manage" ? new ErasoftContext() : new ErasoftContext(data.UserId);

                var selectedDate = (data.SelDate != "" ? DateTime.ParseExact(data.SelDate, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture) : DateTime.Today.Date);

                var selectedMonth = (data.SelDate != "" ? DateTime.ParseExact(data.SelDate, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture).Month : DateTime.Today.Month);

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

                foreach (var barang in vm.ListBarang)
                {
                    var barangUtkCek = vm.ListBarangUntukCekQty.FirstOrDefault(b => b.BRG == barang.BRG);

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
                            vm.ListBarangMiniStok.Add(new PenjualanBarang
                            {
                                KodeBrg = barang.BRG,
                                NamaBrg = $"{barang.NAMA} {barang.NAMA2}",
                                Qty = qtyOnHand
                            });
                        }
                    }
                }

                vm.ListBarangLaku = vm.ListBarangLaku.OrderByDescending(b => b.Qty).Take(10).ToList();
                vm.ListBarangTidakLaku = vm.ListBarangTidakLaku.OrderByDescending(b => b.Qty).Take(10).ToList();
                vm.ListBarangMiniStok = vm.ListBarangMiniStok.OrderByDescending(b => b.Qty).Take(10).ToList();

                result = new JsonApi()
                {
                    code = 200,
                    message = "Success",
                    data = vm
                };

                return Json(result);
            }
            catch (Exception e)
            {
                var result = new JsonApi()
                {
                    code = 500,
                    message = e.Message,
                    data = null
                };

                return Json(result);
            }
        }

        [System.Web.Http.Route("api/mobile/pesanan")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult DataPesanan([FromBody]JsonData data)
        {
            try
            {
                JsonApi result;
                string apiKey = "";

                var re = Request;
                var headers = re.Headers;

                if (headers.Contains("X-API-KEY"))
                {
                    apiKey = headers.GetValues("X-API-KEY").First();
                }

                if (apiKey != "M@STERONLINE4P1K3Y")
                {
                    result = new JsonApi()
                    {
                        code = 401,
                        message = "Wrong API KEY!",
                        data = null
                    };

                    return Json(result);
                }

                ErasoftDbContext = data.UserId == "admin_manage" ? new ErasoftContext() : new ErasoftContext(data.UserId);

                var vm = new PesananViewModel()
                {
                    ListPesanan = ErasoftDbContext.SOT01A.Where(p => p.STATUS_TRANSAKSI == data.StatusTransaksi).OrderByDescending(p => p.TGL).ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList()
                };

                var listData = new List<object>();

                foreach (var pesanan in vm.ListPesanan)
                {
                    var buyer = vm.ListPembeli.SingleOrDefault(m => m.BUYER_CODE == pesanan.PEMESAN);
                    var pelanggan = vm.ListPelanggan.FirstOrDefault(m => m.CUST == pesanan.CUST);
                    var idMarket = 0;

                    if (pelanggan != null)
                    {
                        idMarket = Convert.ToInt32(pelanggan.NAMA);
                    }

                    var market = vm.ListMarketplace.FirstOrDefault(m => m.IdMarket == idMarket);
                    var namaMarket = "";

                    if (market != null)
                    {
                        namaMarket = market.NamaMarket;
                    }

                    listData.Add(new
                    {
                        Pesanan = pesanan,
                        MarketName = namaMarket,
                        BuyerName = buyer?.NAMA + " (" + buyer?.PERSO + ")"
                    });
                }

                result = new JsonApi()
                {
                    code = 200,
                    message = "Success",
                    data = listData
                };

                return Json(result);
            }
            catch (Exception e)
            {
                var result = new JsonApi()
                {
                    code = 500,
                    message = e.Message,
                    data = null
                };

                return Json(result);
            }
        }

        [System.Web.Http.Route("api/mobile/pesanan/ubahstatus")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult UbahStatusPesanan([FromBody]JsonData data)
        {
            JsonApi result;
            string apiKey = "";

            var re = Request;
            var headers = re.Headers;

            if (headers.Contains("X-API-KEY"))
            {
                apiKey = headers.GetValues("X-API-KEY").First();
            }

            if (apiKey != "M@STERONLINE4P1K3Y")
            {
                result = new JsonApi()
                {
                    code = 401,
                    message = "Wrong API KEY!",
                    data = null
                };

                return Json(result);
            }

            ErasoftDbContext = data.UserId == "admin_manage" ? new ErasoftContext() : new ErasoftContext(data.UserId);
            var pesananInDb = ErasoftDbContext.SOT01A.Single(p => p.RecNum == data.RecNumPesanan);

            if (data.StatusTransaksi == "04") // validasi di tab Siap dikirim
            {
                if (pesananInDb.TRACKING_SHIPMENT.Trim() == "")
                {
                    var resultError = new JsonApi()
                    {
                        code = 204,
                        message = "Resi belum diisi",
                        data = null
                    };

                    return Json(resultError);
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
                    result = new JsonApi()
                    {
                        code = 204,
                        message = "Gd & Qty belum lengkap",
                        data = null
                    };

                    return Json(result);
                }
            }

            pesananInDb.STATUS_TRANSAKSI = data.StatusTransaksi;
            ErasoftDbContext.SaveChanges();
            ChangeStatusPesanan(pesananInDb.NO_BUKTI, pesananInDb.STATUS_TRANSAKSI);

            result = new JsonApi()
            {
                code = 200,
                message = "Status pesanan berhasil diubah",
                data = null
            };

            return Json(result);
        }

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

        // --- PEMBELIAN (BEGIN) --- //

        [System.Web.Http.Route("api/mobile/invoice")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult DataInvoice([FromBody]JsonData data)
        {
            try
            {
                JsonApi result;
                string apiKey = "";

                var re = Request;
                var headers = re.Headers;

                if (headers.Contains("X-API-KEY"))
                {
                    apiKey = headers.GetValues("X-API-KEY").First();
                }

                if (apiKey != "M@STERONLINE4P1K3Y")
                {
                    result = new JsonApi()
                    {
                        code = 401,
                        message = "Wrong API KEY!",
                        data = null
                    };

                    return Json(result);
                }

                ErasoftDbContext = data.UserId == "admin_manage" ? new ErasoftContext() : new ErasoftContext(data.UserId);

                var vm = new InvoiceViewModel()
                {
                    ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "1").OrderByDescending(f => f.TGL).ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListSubs = MoDbContext.Subscription.ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList(),
                    ListNInvoice = ErasoftDbContext.APT03B.ToList()
                };

                var listData = new List<object>();

                foreach (var invoice in vm.ListInvoice)
                {
                    var suppInDb = ErasoftDbContext.APF01.SingleOrDefault(s => s.SUPP == invoice.SUPP);

                    listData.Add(new
                    {
                        Invoice = invoice,
                        Supplier = suppInDb?.NAMA,
                    });
                }

                result = new JsonApi()
                {
                    code = 200,
                    message = "Success",
                    data = listData
                };

                return Json(result);
            }
            catch (Exception e)
            {
                var result = new JsonApi()
                {
                    code = 500,
                    message = e.Message,
                    data = null
                };

                return Json(result);
            }
        }

        [System.Web.Http.Route("api/mobile/returinvoice")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult DataReturInvoice([FromBody]JsonData data)
        {
            try
            {
                JsonApi result;
                string apiKey = "";

                var re = Request;
                var headers = re.Headers;

                if (headers.Contains("X-API-KEY"))
                {
                    apiKey = headers.GetValues("X-API-KEY").First();
                }

                if (apiKey != "M@STERONLINE4P1K3Y")
                {
                    result = new JsonApi()
                    {
                        code = 401,
                        message = "Wrong API KEY!",
                        data = null
                    };

                    return Json(result);
                }

                ErasoftDbContext = data.UserId == "admin_manage" ? new ErasoftContext() : new ErasoftContext(data.UserId);

                var vm = new InvoiceViewModel()
                {
                    ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "2").OrderByDescending(f => f.TGL).ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList()
                };

                var listData = new List<object>();

                foreach (var returInvoice in vm.ListInvoice)
                {
                    var suppInDb = ErasoftDbContext.APF01.SingleOrDefault(s => s.SUPP == returInvoice.SUPP);

                    listData.Add(new
                    {
                        ReturInvoice = returInvoice,
                        Supplier = suppInDb?.NAMA,
                    });
                }

                result = new JsonApi()
                {
                    code = 200,
                    message = "Success",
                    data = listData
                };

                return Json(result);
            }
            catch (Exception e)
            {
                var result = new JsonApi()
                {
                    code = 500,
                    message = e.Message,
                    data = null
                };

                return Json(result);
            }
        }

        // --- PEMBELIAN (END) --- //

        // --- PENJUALAN (BEGIN) --- //

        [System.Web.Http.Route("api/mobile/faktur")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult DataFaktur([FromBody]JsonData data)
        {
            try
            {
                JsonApi result;
                string apiKey = "";

                var re = Request;
                var headers = re.Headers;

                if (headers.Contains("X-API-KEY"))
                {
                    apiKey = headers.GetValues("X-API-KEY").First();
                }

                if (apiKey != "M@STERONLINE4P1K3Y")
                {
                    result = new JsonApi()
                    {
                        code = 401,
                        message = "Wrong API KEY!",
                        data = null
                    };

                    return Json(result);
                }

                ErasoftDbContext = data.UserId == "admin_manage" ? new ErasoftContext() : new ErasoftContext(data.UserId);

                var vm = new FakturViewModel()
                {
                    ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2").OrderByDescending(f => f.TGL).ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList(),
                    ListPesanan = ErasoftDbContext.SOT01A.ToList(),
                    ListNFaktur = ErasoftDbContext.ART03B.ToList(),
                    ListSubs = MoDbContext.Subscription.ToList(),
                };

                var listData = new List<object>();

                foreach (var faktur in vm.ListPesanan)
                {
                    var buyer = vm.ListPembeli.SingleOrDefault(m => m.BUYER_CODE == faktur.PEMESAN);
                    var pelanggan = vm.ListPelanggan.FirstOrDefault(m => m.CUST == faktur.CUST);
                    var idMarket = 0;

                    if (pelanggan != null)
                    {
                        idMarket = Convert.ToInt32(pelanggan.NAMA);
                    }

                    var market = vm.ListMarketplace.FirstOrDefault(m => m.IdMarket == idMarket);
                    var namaMarket = "";

                    if (market != null)
                    {
                        namaMarket = market.NamaMarket;
                    }

                    listData.Add(new
                    {
                        Faktur = faktur,
                        MarketName = namaMarket,
                        BuyerName = buyer?.NAMA + " (" + buyer?.PERSO + ")"
                    });
                }

                result = new JsonApi()
                {
                    code = 200,
                    message = "Success",
                    data = listData
                };

                return Json(result);
            }
            catch (Exception e)
            {
                var result = new JsonApi()
                {
                    code = 500,
                    message = e.Message,
                    data = null
                };

                return Json(result);
            }
        }

        [System.Web.Http.Route("api/mobile/returfaktur")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult DataReturFaktur([FromBody]JsonData data)
        {
            try
            {
                JsonApi result;
                string apiKey = "";

                var re = Request;
                var headers = re.Headers;

                if (headers.Contains("X-API-KEY"))
                {
                    apiKey = headers.GetValues("X-API-KEY").First();
                }

                if (apiKey != "M@STERONLINE4P1K3Y")
                {
                    result = new JsonApi()
                    {
                        code = 401,
                        message = "Wrong API KEY!",
                        data = null
                    };

                    return Json(result);
                }

                ErasoftDbContext = data.UserId == "admin_manage" ? new ErasoftContext() : new ErasoftContext(data.UserId);

                var vm = new FakturViewModel()
                {
                    ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "3").OrderByDescending(f => f.TGL).ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList()
                };

                var listData = new List<object>();

                foreach (var returFaktur in vm.ListPesanan)
                {
                    var buyer = vm.ListPembeli.SingleOrDefault(m => m.BUYER_CODE == returFaktur.PEMESAN);
                    var pelanggan = vm.ListPelanggan.FirstOrDefault(m => m.CUST == returFaktur.CUST);
                    var idMarket = 0;

                    if (pelanggan != null)
                    {
                        idMarket = Convert.ToInt32(pelanggan.NAMA);
                    }

                    var market = vm.ListMarketplace.FirstOrDefault(m => m.IdMarket == idMarket);
                    var namaMarket = "";

                    if (market != null)
                    {
                        namaMarket = market.NamaMarket;
                    }

                    listData.Add(new
                    {
                        ReturFaktur = returFaktur,
                        MarketName = namaMarket,
                        BuyerName = buyer?.NAMA + " (" + buyer?.PERSO + ")"
                    });
                }

                result = new JsonApi()
                {
                    code = 200,
                    message = "Success",
                    data = listData
                };

                return Json(result);
            }
            catch (Exception e)
            {
                var result = new JsonApi()
                {
                    code = 500,
                    message = e.Message,
                    data = null
                };

                return Json(result);
            }
        }

        // --- PENJUALAN (END) --- //

    }
}
