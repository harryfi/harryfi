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
using System.Web.Security;
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
        [System.Web.Http.AcceptVerbs("GET", "POST")]
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
                ErasoftDbContext = _viewModel.Account.UserId == "admin_manage" ? new ErasoftContext() : new ErasoftContext(_viewModel.Account.DataSourcePath,_viewModel.Account.UserId);
            }
            else
            {
                var accFromUser = MoDbContext.Account.Single(a => a.AccountId == _viewModel.User.AccountId);
                ErasoftDbContext = new ErasoftContext(accFromUser.DataSourcePath, accFromUser.UserId);
            }

            result = new JsonApi()
            {
                code = 200,
                message = "Login Berhasil",
                data = new
                {
                    logged = true,
                    dbPath = _viewModel.Account != null ? _viewModel.Account.DatabasePathErasoft : MoDbContext.Account.Single(ac => ac.AccountId == _viewModel.User.AccountId).DatabasePathErasoft,
                    userId = _viewModel.Account != null ? _viewModel.Account.UserId : _viewModel.User.UserId.ToString(),
                    name = _viewModel.Account != null ? _viewModel.Account.Username : _viewModel.User.Username,
                    email = _viewModel.Account != null ? _viewModel.Account.Email : _viewModel.User.Email,
                }
            };

            return Json(result);
        }

        [System.Web.Http.Route("api/mobile/register")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
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

#if AWS
            //using (var smtp = new SmtpClient())
            //{
            //    var credential = new NetworkCredential
            //    {
            //        UserName = "AKIAIXN2D33JPSDL7WEQ",
            //        Password = "ApBddkFZF8hwJtbo+s4Oq31MqDtWOpzYKDhyVGSHGCEl"
            //    };
            //    smtp.Credentials = credential;
            //    smtp.Host = "email-smtp.us-east-1.amazonaws.com";
            //    smtp.Port = 587;
            //    smtp.EnableSsl = true;
            //    await smtp.SendMailAsync(message);
            //}
            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = "csmasteronline@gmail.com",
                    Password = "kmblwexkeretrwxv"
                };
                smtp.Credentials = credential;
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(message);
            }
#else
            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = "csmasteronline@gmail.com",
                    Password = "kmblwexkeretrwxv"
                };
                smtp.Credentials = credential;
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(message);
            }
#endif

            result = new JsonApi()
            {
                code = 200,
                message = "Kami telah menerima pendaftaran Anda. Silakan menunggu approval dari admin kami, terima kasih.",
                data = null
            };

            return Json(result);
        }

        //remark by calvin 17 september 2019
        //[System.Web.Http.Route("api/mobile/dashboard")]
        //[System.Web.Http.AcceptVerbs("GET", "POST")]
        //public IHttpActionResult DashboardResult([FromBody]JsonData data)
        //{
        //    try
        //    {
        //        JsonApi result;
        //        string apiKey = "";

        //        var re = Request;
        //        var headers = re.Headers;

        //        if (headers.Contains("X-API-KEY"))
        //        {
        //            apiKey = headers.GetValues("X-API-KEY").First();
        //        }

        //        if (apiKey != "M@STERONLINE4P1K3Y")
        //        {
        //            result = new JsonApi()
        //            {
        //                code = 401,
        //                message = "Wrong API KEY!",
        //                data = null
        //            };

        //            return Json(result);
        //        }

        //        ErasoftDbContext = data.DbPath == "ERASOFT" ? new ErasoftContext() : new ErasoftContext(data.DbPath);

        //        var selectedDate = (data.SelDate != "" ? DateTime.ParseExact(data.SelDate, "dd/MM/yyyy",
        //            CultureInfo.InvariantCulture) : DateTime.Today.Date);

        //        var selectedMonth = (data.SelDate != "" ? DateTime.ParseExact(data.SelDate, "dd/MM/yyyy",
        //            CultureInfo.InvariantCulture).Month : DateTime.Today.Month);

        //        var vm = new DashboardViewModel()
        //        {
        //            ListPesanan = ErasoftDbContext.SOT01A.ToList(),
        //            ListPesananDetail = ErasoftDbContext.SOT01B.ToList(),
        //            ListFaktur = ErasoftDbContext.SIT01A.ToList(),
        //            ListFakturDetail = ErasoftDbContext.SIT01B.ToList(),
        //            ListBarang = ErasoftDbContext.STF02.ToList(),
        //            ListAkunMarketplace = ErasoftDbContext.ARF01.ToList(),
        //            ListMarket = MoDbContext.Marketplaces.ToList(),
        //            ListBarangUntukCekQty = ErasoftDbContext.STF08A.ToList(),
        //            ListStok = ErasoftDbContext.STT01B.ToList()
        //        };

        //        // Pesanan
        //        vm.JumlahPesananHariIni = vm.ListPesanan?.Where(p => p.TGL?.Date == selectedDate).Count();
        //        vm.NilaiPesananHariIni = vm.ListPesanan?.Where(p => p.TGL?.Date == selectedDate).Sum(p => p.NETTO);
        //        vm.JumlahPesananBulanIni = vm.ListPesanan?.Where(p => p.TGL?.Month == selectedMonth).Count();
        //        vm.NilaiPesananBulanIni = vm.ListPesanan?.Where(p => p.TGL?.Month == selectedMonth).Sum(p => p.NETTO);

        //        // Faktur
        //        vm.JumlahFakturHariIni = vm.ListFaktur?.Where(p => p.TGL == selectedDate && p.JENIS_FORM == "2").Count();
        //        vm.NilaiFakturHariIni = vm.ListFaktur?.Where(p => p.TGL == selectedDate && p.JENIS_FORM == "2").Sum(p => p.NETTO);
        //        vm.JumlahFakturBulanIni = vm.ListFaktur?.Where(p => p.TGL.Month == selectedMonth && p.JENIS_FORM == "2").Count();
        //        vm.NilaiFakturBulanIni = vm.ListFaktur?.Where(p => p.TGL.Month == selectedMonth && p.JENIS_FORM == "2").Sum(p => p.NETTO);


        //        // Retur
        //        vm.JumlahReturHariIni = vm.ListFaktur?.Where(p => p.TGL == selectedDate && p.JENIS_FORM == "3").Count();
        //        vm.NilaiReturHariIni = vm.ListFaktur?.Where(p => p.TGL == selectedDate && p.JENIS_FORM == "3").Sum(p => p.NETTO);
        //        vm.JumlahReturBulanIni = vm.ListFaktur?.Where(p => p.TGL.Month == selectedMonth && p.JENIS_FORM == "3").Count();
        //        vm.NilaiReturBulanIni = vm.ListFaktur?.Where(p => p.TGL.Month == selectedMonth && p.JENIS_FORM == "3").Sum(p => p.NETTO);

        //        if (vm.ListAkunMarketplace.Count > 0)
        //        {
        //            foreach (var marketplace in vm.ListAkunMarketplace)
        //            {
        //                var idMarket = Convert.ToInt32(marketplace.NAMA);
        //                var namaMarket = vm.ListMarket.Single(m => m.IdMarket == idMarket).NamaMarket;

        //                var jumlahPesananToday = vm.ListPesanan?
        //                    .Where(p => p.CUST == marketplace.CUST && p.TGL == selectedDate).Count();
        //                var nilaiPesananToday = $"Rp {String.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:N}", vm.ListPesanan?.Where(p => p.CUST == marketplace.CUST && p.TGL == selectedDate).Sum(p => p.NETTO))}";

        //                var jumlahPesananMonth = vm.ListPesanan?

        //                    .Where(p => p.CUST == marketplace.CUST && p.TGL?.Month == selectedMonth).Count();
        //                var nilaiPesananMonth = $"Rp {String.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:N}", vm.ListPesanan?.Where(p => p.CUST == marketplace.CUST && p.TGL?.Month == selectedMonth).Sum(p => p.NETTO))}";

        //                vm.ListPesananPerMarketplace.Add(new PesananPerMarketplaceModel()
        //                {
        //                    NamaMarket = $"{namaMarket} ({marketplace.PERSO})",
        //                    JumlahPesananHariIni = jumlahPesananToday.ToString(),
        //                    NilaiPesananHariIni = nilaiPesananToday,
        //                    JumlahPesananBulanIni = jumlahPesananMonth.ToString(),
        //                    NilaiPesananBulanIni = nilaiPesananMonth
        //                });
        //            }
        //        }

        //        foreach (var barang in vm.ListBarang)
        //        {
        //            var listBarangTerpesan = vm.ListPesananDetail.Where(b => b.BRG == barang.BRG).ToList();

        //            if (listBarangTerpesan.Count > 0)
        //            {
        //                var qtyBarang = listBarangTerpesan.Where(b => b.TGL_INPUT?.Month >= (selectedMonth - 3) &&
        //                                                              b.TGL_INPUT?.Month <= selectedMonth).Sum(b => b.QTY);
        //                vm.ListBarangLaku.Add(new PenjualanBarang
        //                {
        //                    KodeBrg = barang.BRG,
        //                    NamaBrg = $"{barang.NAMA} {barang.NAMA2}",
        //                    Qty = qtyBarang,
        //                    Laku = true
        //                });
        //            }
        //        }

        //        foreach (var barang in vm.ListBarang.Where(b => b.Tgl_Input?.Month >= (selectedMonth - 3) && b.Tgl_Input?.Month <= selectedMonth))
        //        {
        //            var barangTerpesan = vm.ListPesananDetail.FirstOrDefault(b => b.BRG == barang.BRG);
        //            var stokBarang = vm.ListStok.FirstOrDefault(b => b.Kobar == barang.BRG);

        //            if (barangTerpesan == null)
        //            {
        //                vm.ListBarangTidakLaku.Add(new PenjualanBarang
        //                {
        //                    KodeBrg = barang.BRG,
        //                    NamaBrg = $"{barang.NAMA} {barang.NAMA2}",
        //                    Qty = Convert.ToDouble(stokBarang?.Qty),
        //                    Laku = false
        //                });
        //            }
        //        }

        //        foreach (var barang in vm.ListBarang)
        //        {
        //            var barangUtkCek = vm.ListBarangUntukCekQty.FirstOrDefault(b => b.BRG == barang.BRG);

        //            var qtyOnHand = 0d;

        //            if (barangUtkCek != null)
        //            {
        //                qtyOnHand = barangUtkCek.QAwal + barangUtkCek.QM1 + barangUtkCek.QM2 + barangUtkCek.QM3 + barangUtkCek.QM4
        //                            + barangUtkCek.QM5 + barangUtkCek.QM6 + barangUtkCek.QM7 + barangUtkCek.QM8 + barangUtkCek.QM9
        //                            + barangUtkCek.QM10 + barangUtkCek.QM11 + barangUtkCek.QM12 - barangUtkCek.QK1 - barangUtkCek.QK2
        //                            - barangUtkCek.QK3 - barangUtkCek.QK4 - barangUtkCek.QK5 - barangUtkCek.QK6 - barangUtkCek.QK7
        //                            - barangUtkCek.QK8 - barangUtkCek.QK9 - barangUtkCek.QK10 - barangUtkCek.QK11 - barangUtkCek.QK12;

        //                if (qtyOnHand < barang.MINI)
        //                {
        //                    vm.ListBarangMiniStok.Add(new PenjualanBarang
        //                    {
        //                        KodeBrg = barang.BRG,
        //                        NamaBrg = $"{barang.NAMA} {barang.NAMA2}",
        //                        Qty = qtyOnHand
        //                    });
        //                }
        //            }
        //        }

        //        vm.ListBarangLaku = vm.ListBarangLaku.OrderByDescending(b => b.Qty).Take(10).ToList();
        //        vm.ListBarangTidakLaku = vm.ListBarangTidakLaku.OrderByDescending(b => b.Qty).Take(10).ToList();
        //        vm.ListBarangMiniStok = vm.ListBarangMiniStok.OrderByDescending(b => b.Qty).Take(10).ToList();

        //        result = new JsonApi()
        //        {
        //            code = 200,
        //            message = "Success",
        //            data = vm
        //        };

        //        return Json(result);
        //    }
        //    catch (Exception e)
        //    {
        //        var result = new JsonApi()
        //        {
        //            code = 500,
        //            message = e.Message,
        //            data = null
        //        };

        //        return Json(result);
        //    }
        //}
        //end remark by calvin 17 september 2019

        [System.Web.Http.Route("api/mobile/pesanan")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
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

                ErasoftDbContext = data.DbPath == "ERASOFT" ? new ErasoftContext() : new ErasoftContext("", data.DbPath);

                var vm = new PesananViewModel()
                {
                    ListPesanan = ErasoftDbContext.SOT01A.Where(p => p.STATUS_TRANSAKSI == data.StatusTransaksi).OrderByDescending(p => p.TGL).ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList()
                };

                var listData = new List<ResultDataPesanan>();
                var listFinalData = vm.ListPesanan.Where(p => p.NO_BUKTI.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                              p.NAMAPEMESAN.ToLower().Contains(data.SearchParam.ToLower())).ToList();

                foreach (var pesanan in listFinalData)
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

                    listData.Add(new ResultDataPesanan
                    {
                        Pesanan = pesanan,
                        MarketName = namaMarket,
                        BuyerName = buyer?.NAMA + " (" + buyer?.PERSO + ")"
                    });
                }

                switch (data.SortBy)
                {
                    case 1:
                        listData = listData.OrderBy(d => d.Pesanan.NO_BUKTI).ToList();
                        break;
                    case 2:
                        listData = listData.OrderBy(d => d.Pesanan.TGL).ToList();
                        break;
                    case 3:
                        listData = listData.OrderBy(d => d.MarketName).ToList();
                        break;
                    case 4:
                        listData = listData.OrderBy(d => d.BuyerName).ToList();
                        break;
                    case 5:
                        listData = listData.OrderBy(d => d.Pesanan.NETTO).ToList();
                        break;
                }

                result = new JsonApi()
                {
                    code = 200,
                    message = $"{listData.Count} data has been found!",
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
        [System.Web.Http.AcceptVerbs("GET", "POST")]
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

            ErasoftDbContext = data.DbPath == "ERASOFT" ? new ErasoftContext() : new ErasoftContext("", data.DbPath);
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
        [System.Web.Http.AcceptVerbs("GET", "POST")]
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

                ErasoftDbContext = data.DbPath == "ERASOFT" ? new ErasoftContext() : new ErasoftContext("", data.DbPath);

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

                var listData = new List<ResultDataInvoice>();
                var listFinalData = vm.ListInvoice.Where(i => i.INV.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                              i.NAMA.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                              i.TGL.ToString().ToLower().Contains(data.SearchParam.ToLower())).ToList();

                foreach (var invoice in listFinalData)
                {
                    var suppInDb = ErasoftDbContext.APF01.SingleOrDefault(s => s.SUPP == invoice.SUPP);

                    listData.Add(new ResultDataInvoice
                    {
                        Invoice = invoice,
                        Supplier = suppInDb?.NAMA,
                    });
                }

                switch (data.SortBy)
                {
                    case 1:
                        listData = listData.OrderBy(d => d.Invoice.INV).ToList();
                        break;
                    case 2:
                        listData = listData.OrderBy(d => d.Invoice.TGL).ToList();
                        break;
                    case 3:
                        listData = listData.OrderBy(d => d.Supplier).ToList();
                        break;
                    case 4:
                        listData = listData.OrderBy(d => d.Invoice.NETTO).ToList();
                        break;
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

        [System.Web.Http.Route("api/mobile/invoicebelumlunas")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public IHttpActionResult DataInvoiceBelumLunas([FromBody] JsonData data)
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

                ErasoftDbContext = data.DbPath == "ERASOFT" ? new ErasoftContext() : new ErasoftContext("", data.DbPath);

                var invBelumLunas = ErasoftDbContext.APT01D.Where(a => a.NETTO - a.DEBET > 0);
                var vm = new InvoiceViewModel()
                {
                    ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "1" && invBelumLunas.Any(a => a.INV == f.INV)).ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList(),
                    ListNInvoice = ErasoftDbContext.APT03B.ToList()
                };

                var listData = new List<ResultDataInvoice>();
                var listFinalData = vm.ListInvoice.Where(i => i.INV.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                              i.NAMA.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                              i.TGL.ToString().ToLower().Contains(data.SearchParam.ToLower())).ToList();

                foreach (var invoice in listFinalData)
                {
                    var suppInDb = ErasoftDbContext.APF01.SingleOrDefault(s => s.SUPP == invoice.SUPP);

                    listData.Add(new ResultDataInvoice
                    {
                        Invoice = invoice,
                        Supplier = suppInDb?.NAMA,
                    });
                }

                switch (data.SortBy)
                {
                    case 1:
                        listData = listData.OrderBy(d => d.Invoice.INV).ToList();
                        break;
                    case 2:
                        listData = listData.OrderBy(d => d.Invoice.TGL).ToList();
                        break;
                    case 3:
                        listData = listData.OrderBy(d => d.Supplier).ToList();
                        break;
                    case 4:
                        listData = listData.OrderBy(d => d.Invoice.NETTO).ToList();
                        break;
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

        [System.Web.Http.Route("api/mobile/invoicejatuhtempo")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public IHttpActionResult DataInvoiceJatuhTempo([FromBody] JsonData data)
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

                var vm = new InvoiceViewModel()
                {
                    ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "1" && f.TGJT <= DateTime.Now).ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList(),
                    ListNInvoice = ErasoftDbContext.APT03B.ToList()
                };

                var listData = new List<ResultDataInvoice>();
                var listFinalData = vm.ListInvoice.Where(i => i.INV.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                              i.NAMA.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                              i.TGL.ToString().ToLower().Contains(data.SearchParam.ToLower())).ToList();

                foreach (var invoice in listFinalData)
                {
                    var suppInDb = ErasoftDbContext.APF01.SingleOrDefault(s => s.SUPP == invoice.SUPP);

                    listData.Add(new ResultDataInvoice
                    {
                        Invoice = invoice,
                        Supplier = suppInDb?.NAMA,
                    });
                }

                switch (data.SortBy)
                {
                    case 1:
                        listData = listData.OrderBy(d => d.Invoice.INV).ToList();
                        break;
                    case 2:
                        listData = listData.OrderBy(d => d.Invoice.TGL).ToList();
                        break;
                    case 3:
                        listData = listData.OrderBy(d => d.Supplier).ToList();
                        break;
                    case 4:
                        listData = listData.OrderBy(d => d.Invoice.NETTO).ToList();
                        break;
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
        [System.Web.Http.AcceptVerbs("GET", "POST")]
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

                ErasoftDbContext = data.DbPath == "ERASOFT" ? new ErasoftContext() : new ErasoftContext("", data.DbPath);

                var vm = new InvoiceViewModel()
                {
                    ListInvoice = ErasoftDbContext.PBT01A.Where(f => f.JENISFORM == "2").OrderByDescending(f => f.TGL).ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList()
                };

                var listData = new List<ResultDataInvoice>();
                var listFinalData = vm.ListInvoice.Where(i => i.INV.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                              i.NAMA.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                              i.TGL.ToString().ToLower().Contains(data.SearchParam.ToLower())).ToList();

                foreach (var returInvoice in listFinalData)
                {
                    var suppInDb = ErasoftDbContext.APF01.SingleOrDefault(s => s.SUPP == returInvoice.SUPP);

                    listData.Add(new ResultDataInvoice
                    {
                        ReturInvoice = returInvoice,
                        Supplier = suppInDb?.NAMA,
                    });
                }

                switch (data.SortBy)
                {
                    case 1:
                        listData = listData.OrderBy(d => d.ReturInvoice.INV).ToList();
                        break;
                    case 2:
                        listData = listData.OrderBy(d => d.ReturInvoice.TGL).ToList();
                        break;
                    case 3:
                        listData = listData.OrderBy(d => d.Supplier).ToList();
                        break;
                    case 4:
                        listData = listData.OrderBy(d => d.ReturInvoice.NETTO).ToList();
                        break;
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

        [System.Web.Http.Route("api/mobile/pembayaranbeli")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public IHttpActionResult DataPembayaranPembelian([FromBody]JsonData data)
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

                ErasoftDbContext = data.DbPath == "ERASOFT" ? new ErasoftContext() : new ErasoftContext("", data.DbPath);

                var vm = new BayarHutangViewModel()
                {
                    ListHutang = ErasoftDbContext.APT03A.ToList(),
                    ListHutangDetail = ErasoftDbContext.APT03B.ToList(),
                    ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2").ToList()
                };

                var listData = new List<object>();
                var listFinalData = vm.ListHutang.Where(h => h.NSUPP.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                             h.BUKTI.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                             h.TGL.ToString("dd/MM/yyyy").Contains(data.SearchParam.ToLower())).ToList();

                switch (data.SortBy)
                {
                    case 1:
                        listFinalData = listFinalData.OrderBy(d => d.BUKTI).ToList();
                        break;
                    case 2:
                        listFinalData = listFinalData.OrderBy(d => d.TGL).ToList();
                        break;
                    case 3:
                        listFinalData = listFinalData.OrderBy(d => d.TPOT).ToList();
                        break;
                    case 4:
                        listFinalData = listFinalData.OrderBy(d => d.TBAYAR).ToList();
                        break;
                }

                foreach (var hutang in listFinalData)
                {
                    listData.Add(new
                    {
                        Hutang = hutang
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
        [System.Web.Http.AcceptVerbs("GET", "POST")]
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

                ErasoftDbContext = data.DbPath == "ERASOFT" ? new ErasoftContext() : new ErasoftContext("", data.DbPath);

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

                var listData = new List<ResultDataFaktur>();
                var listFinalData = vm.ListFaktur.Where(i => i.NO_BUKTI.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                             i.NO_REF.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                             i.AL.ToLower().Contains(data.SearchParam.ToLower()) || 
                                                             i.TGL.ToString("dd/MM/yyyy").ToLower().Contains(data.SearchParam.ToLower())).ToList();

                foreach (var faktur in listFinalData)
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

                    listData.Add(new ResultDataFaktur
                    {
                        Faktur = faktur,
                        MarketName = namaMarket,
                        BuyerName = buyer?.NAMA + " (" + buyer?.PERSO + ")"
                    });
                }

                switch (data.SortBy)
                {
                    case 1:
                        listData = listData.OrderBy(d => d.Faktur.NO_BUKTI).ToList();
                        break;
                    case 2:
                        listData = listData.OrderBy(d => d.Faktur.TGL).ToList();
                        break;
                    case 3:
                        listData = listData.OrderBy(d => d.MarketName).ToList();
                        break;
                    case 4:
                        listData = listData.OrderBy(d => d.BuyerName).ToList();
                        break;
                    case 5:
                        listData = listData.OrderBy(d => d.Faktur.NETTO).ToList();
                        break;
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

        [System.Web.Http.Route("api/mobile/fakturbelumlunas")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public IHttpActionResult DataFakturBelumLunas([FromBody] JsonData data)
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

                ErasoftDbContext = data.DbPath == "ERASOFT" ? new ErasoftContext() : new ErasoftContext("", data.DbPath);

                var fakturSudahLunas = ErasoftDbContext.ART01D.Where(a => a.NETTO.Value - a.KREDIT.Value > 0);

                var vm = new FakturViewModel()
                {
                    ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2" && fakturSudahLunas.Any(a => a.FAKTUR == f.NO_BUKTI))
                        .ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList(),
                    ListNFaktur = ErasoftDbContext.ART03B.ToList(),
                    ListPesanan = ErasoftDbContext.SOT01A.ToList(),
                };

                var listData = new List<ResultDataFaktur>();
                var listFinalData = vm.ListFaktur.Where(i => i.NO_BUKTI.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                             i.NO_REF.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                             i.AL.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                             i.TGL.ToString("dd/MM/yyyy").ToLower().Contains(data.SearchParam.ToLower())).ToList();

                foreach (var faktur in listFinalData)
                {
                    var buyer = vm.ListPembeli.SingleOrDefault(m => m.BUYER_CODE == faktur.PEMESAN);
                    var pelanggan = vm.ListPelanggan.FirstOrDefault(m => m.CUST == faktur.CUST);
                    var idMarket = 0;
                    var tglJtTempo = faktur.TGL_JT_TEMPO ?? DateTime.MinValue;

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

                    listData.Add(new ResultDataFaktur
                    {
                        Faktur = faktur,
                        TglJatuhTempo = tglJtTempo,
                        MarketName = namaMarket,
                        BuyerName = buyer?.NAMA + " (" + buyer?.PERSO + ")"
                    });
                }

                switch (data.SortBy)
                {
                    case 1:
                        listData = listData.OrderBy(d => d.Faktur.NO_BUKTI).ToList();
                        break;
                    case 2:
                        listData = listData.OrderBy(d => d.Faktur.TGL).ToList();
                        break;
                    case 3:
                        listData = listData.OrderBy(d => d.MarketName).ToList();
                        break;
                    case 4:
                        listData = listData.OrderBy(d => d.BuyerName).ToList();
                        break;
                    case 5:
                        listData = listData.OrderBy(d => d.Faktur.NETTO).ToList();
                        break;
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

        [System.Web.Http.Route("api/mobile/fakturjatuhtempo")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public IHttpActionResult DataFakturJatuhTempo([FromBody] JsonData data)
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

                ErasoftDbContext = data.DbPath == "ERASOFT" ? new ErasoftContext() : new ErasoftContext("", data.DbPath);

                var vm = new FakturViewModel()
                {
                    ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2" && f.TGL_JT_TEMPO <= DateTime.Now).ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList(),
                    ListNFaktur = ErasoftDbContext.ART03B.ToList(),
                    ListPesanan = ErasoftDbContext.SOT01A.ToList(),
                };

                var listData = new List<ResultDataFaktur>();
                var listFinalData = vm.ListFaktur.Where(i => i.NO_BUKTI.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                             i.NO_REF.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                             i.AL.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                             i.TGL.ToString("dd/MM/yyyy").ToLower().Contains(data.SearchParam.ToLower())).ToList();

                foreach (var faktur in listFinalData)
                {
                    var buyer = vm.ListPembeli.SingleOrDefault(m => m.BUYER_CODE == faktur.PEMESAN);
                    var pelanggan = vm.ListPelanggan.FirstOrDefault(m => m.CUST == faktur.CUST);
                    var idMarket = 0;
                    var tglJtTempo = faktur.TGL_JT_TEMPO ?? DateTime.MinValue;

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

                    listData.Add(new ResultDataFaktur
                    {
                        Faktur = faktur,
                        TglJatuhTempo = tglJtTempo,
                        MarketName = namaMarket,
                        BuyerName = buyer?.NAMA + " (" + buyer?.PERSO + ")"
                    });
                }

                switch (data.SortBy)
                {
                    case 1:
                        listData = listData.OrderBy(d => d.Faktur.NO_BUKTI).ToList();
                        break;
                    case 2:
                        listData = listData.OrderBy(d => d.Faktur.TGL).ToList();
                        break;
                    case 3:
                        listData = listData.OrderBy(d => d.MarketName).ToList();
                        break;
                    case 4:
                        listData = listData.OrderBy(d => d.BuyerName).ToList();
                        break;
                    case 5:
                        listData = listData.OrderBy(d => d.Faktur.NETTO).ToList();
                        break;
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
        [System.Web.Http.AcceptVerbs("GET", "POST")]
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

                ErasoftDbContext = data.DbPath == "ERASOFT" ? new ErasoftContext() : new ErasoftContext("", data.DbPath);

                var vm = new FakturViewModel()
                {
                    ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "3").OrderByDescending(f => f.TGL).ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListPembeli = ErasoftDbContext.ARF01C.OrderBy(x => x.NAMA).ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList()
                };

                var listData = new List<ResultDataFaktur>();
                var listFinalData = vm.ListFaktur.Where(i => i.NO_BUKTI.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                             i.NO_REF.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                             i.AL.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                             i.TGL.ToString("dd/MM/yyyy").ToLower().Contains(data.SearchParam.ToLower())).ToList();

                foreach (var returFaktur in listFinalData)
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

                    listData.Add(new ResultDataFaktur
                    {
                        ReturFaktur = returFaktur,
                        MarketName = namaMarket,
                        BuyerName = buyer?.NAMA + " (" + buyer?.PERSO + ")"
                    });
                }

                switch (data.SortBy)
                {
                    case 1:
                        listData = listData.OrderBy(d => d.ReturFaktur.NO_BUKTI).ToList();
                        break;
                    case 2:
                        listData = listData.OrderBy(d => d.ReturFaktur.TGL).ToList();
                        break;
                    case 3:
                        listData = listData.OrderBy(d => d.ReturFaktur.NO_REF).ToList();
                        break;
                    case 4:
                        listData = listData.OrderBy(d => d.MarketName).ToList();
                        break;
                    case 5:
                        listData = listData.OrderBy(d => d.BuyerName).ToList();
                        break;
                    case 6:
                        listData = listData.OrderBy(d => d.ReturFaktur.NETTO).ToList();
                        break;
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

        [System.Web.Http.Route("api/mobile/pembayaranjual")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public IHttpActionResult DataPembayaranPenjualan([FromBody]JsonData data)
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

                ErasoftDbContext = data.DbPath == "ERASOFT" ? new ErasoftContext() : new ErasoftContext("", data.DbPath);

                var vm = new BayarPiutangViewModel()
                {
                    ListPiutang = ErasoftDbContext.ART03A.ToList(),
                    ListPiutangDetail = ErasoftDbContext.ART03B.ToList(),
                    ListFaktur = ErasoftDbContext.SIT01A.Where(f => f.JENIS_FORM == "2").ToList()
                };

                var listData = new List<object>();
                var listFinalData = vm.ListPiutang.Where(h => h.NCUST.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                             h.BUKTI.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                             h.TGL.ToString("dd/MM/yyyy").Contains(data.SearchParam.ToLower())).ToList();

                switch (data.SortBy)
                {
                    case 1:
                        listFinalData = listFinalData.OrderBy(d => d.BUKTI).ToList();
                        break;
                    case 2:
                        listFinalData = listFinalData.OrderBy(d => d.TGL).ToList();
                        break;
                    case 3:
                        listFinalData = listFinalData.OrderBy(d => d.TBAYAR).ToList();
                        break;
                    case 4:
                        listFinalData = listFinalData.OrderBy(d => d.TPOT).ToList();
                        break;
                    case 5:
                        listFinalData = listFinalData.OrderBy(d => (d.TPOT + d.TBAYAR)).ToList();
                        break;
                }

                foreach (var piutang in listFinalData)
                {
                    listData.Add(new
                    {
                        Piutang = piutang
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

        // --- BARANG (BEGIN) --- //

        [System.Web.Http.Route("api/mobile/barang")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public IHttpActionResult DataBarang([FromBody]JsonData data)
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

                ErasoftDbContext = data.DbPath == "ERASOFT" ? new ErasoftContext() : new ErasoftContext("",data.DbPath);

                var vm = new BarangViewModel()
                {
                    ListStf02S = ErasoftDbContext.STF02.ToList(),
                    ListMarket = ErasoftDbContext.ARF01.OrderBy(p => p.RecNum).ToList(),
                    ListHargaJualPermarketView = ErasoftDbContext.STF02H.OrderBy(p => p.IDMARKET).ToList(),
                    ListCategoryBlibli = MoDbContext.CategoryBlibli.Where(p => string.IsNullOrEmpty(p.PARENT_CODE)).ToList(),
                    DataUsaha = ErasoftDbContext.SIFSYS.Single(p => p.BLN == 1)
                };

                var listData = new List<object>();
                var listFinalData = vm.ListStf02S.Where(b => b.NAMA.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                             b.NAMA2.ToLower().Contains(data.SearchParam.ToLower())).ToList();

                switch (data.SortBy)
                {
                    case 1:
                        listFinalData = listFinalData.OrderBy(d => d.BRG).ToList();
                        break;
                    case 2:
                        listFinalData = listFinalData.OrderBy(d => d.NAMA).ToList();
                        break;
                    case 3:
                        listFinalData = listFinalData.OrderBy(d => d.KET_SORT1).ToList();
                        break;
                    case 4:
                        listFinalData = listFinalData.OrderBy(d => d.KET_SORT2).ToList();
                        break;
                    case 5:
                        listFinalData = listFinalData.OrderBy(d => d.HJUAL).ToList();
                        break;
                }

                foreach (var barang in listFinalData)
                {
                    listData.Add(new
                    {
                        Barang = barang,
                    });
                }

                result = new JsonApi()
                {
                    code = 200,
                    message = $"{listData.Count} data has been found!",
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

        [System.Web.Http.Route("api/mobile/barangkosong")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public IHttpActionResult DataBarangKosong([FromBody]JsonData data)
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

                ErasoftDbContext = data.DbPath == "ERASOFT" ? new ErasoftContext() : new ErasoftContext("", data.DbPath);

                var vm = new BarangViewModel()
                {
                    ListStf02S = ErasoftDbContext.STF02.ToList(),
                    ListMarket = ErasoftDbContext.ARF01.OrderBy(p => p.RecNum).ToList(),
                    ListHargaJualPermarketView = ErasoftDbContext.STF02H.OrderBy(p => p.IDMARKET).ToList(),
                    ListCategoryBlibli = MoDbContext.CategoryBlibli.Where(p => string.IsNullOrEmpty(p.PARENT_CODE)).ToList(),
                    DataUsaha = ErasoftDbContext.SIFSYS.Single(p => p.BLN == 1)
                };

                var listData = new List<object>();
                var listFinalData = vm.ListStf02S.Where(b => b.NAMA.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                             b.NAMA2.ToLower().Contains(data.SearchParam.ToLower())).ToList();

                switch (data.SortBy)
                {
                    case 1:
                        listFinalData = listFinalData.OrderBy(d => d.BRG).ToList();
                        break;
                    case 2:
                        listFinalData = listFinalData.OrderBy(d => d.NAMA).ToList();
                        break;
                    case 3:
                        listFinalData = listFinalData.OrderBy(d => d.KET_SORT1).ToList();
                        break;
                    case 4:
                        listFinalData = listFinalData.OrderBy(d => d.KET_SORT2).ToList();
                        break;
                    case 5:
                        listFinalData = listFinalData.OrderBy(d => d.HJUAL).ToList();
                        break;
                }

                foreach (var barang in listFinalData)
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
                            listData.Add(new
                            {
                                KodeBrg = barang.BRG,
                                NamaBrg = $"{barang.NAMA} {barang.NAMA2}",
                                Kategori = barang.KET_SORT1,
                                Merk = barang.KET_SORT2,
                                HJual = barang.HJUAL,
                                Qty = qtyOnHand,
                            });
                        }
                    }
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

        [System.Web.Http.Route("api/mobile/barangtidaklaku")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public IHttpActionResult DataBarangTidakLaku([FromBody]JsonData data)
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

                ErasoftDbContext = data.DbPath == "ERASOFT" ? new ErasoftContext() : new ErasoftContext("", data.DbPath);

                var vm = new BarangViewModel()
                {
                    ListStf02S = ErasoftDbContext.STF02.ToList(),
                    ListMarket = ErasoftDbContext.ARF01.OrderBy(p => p.RecNum).ToList(),
                    ListHargaJualPermarketView = ErasoftDbContext.STF02H.OrderBy(p => p.IDMARKET).ToList(),
                    ListCategoryBlibli = MoDbContext.CategoryBlibli.Where(p => string.IsNullOrEmpty(p.PARENT_CODE)).ToList(),
                    DataUsaha = ErasoftDbContext.SIFSYS.Single(p => p.BLN == 1)
                };

                var listData = new List<object>();
                var listFinalData = vm.ListStf02S.Where(b => b.NAMA.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                             b.NAMA2.ToLower().Contains(data.SearchParam.ToLower())).ToList();

                switch (data.SortBy)
                {
                    case 1:
                        listFinalData = listFinalData.OrderBy(d => d.BRG).ToList();
                        break;
                    case 2:
                        listFinalData = listFinalData.OrderBy(d => d.NAMA).ToList();
                        break;
                    case 3:
                        listFinalData = listFinalData.OrderBy(d => d.KET_SORT1).ToList();
                        break;
                    case 4:
                        listFinalData = listFinalData.OrderBy(d => d.KET_SORT2).ToList();
                        break;
                    case 5:
                        listFinalData = listFinalData.OrderBy(d => d.HJUAL).ToList();
                        break;
                }

                foreach (var barang in listFinalData)
                {
                    var barangTerpesan = ErasoftDbContext.SOT01B.FirstOrDefault(b => b.BRG == barang.BRG);

                    // Kalo barangTerpesan == null tandanya ga laku
                    if (barangTerpesan == null)
                    {
                        listData.Add(new
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

        [System.Web.Http.Route("api/mobile/barangminimumstok")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public IHttpActionResult DataBarangMinimumStok([FromBody]JsonData data)
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

                ErasoftDbContext = data.DbPath == "ERASOFT" ? new ErasoftContext() : new ErasoftContext("", data.DbPath);

                var vm = new BarangViewModel()
                {
                    ListStf02S = ErasoftDbContext.STF02.ToList(),
                    ListMarket = ErasoftDbContext.ARF01.OrderBy(p => p.RecNum).ToList(),
                    ListHargaJualPermarketView = ErasoftDbContext.STF02H.OrderBy(p => p.IDMARKET).ToList(),
                    ListCategoryBlibli = MoDbContext.CategoryBlibli.Where(p => string.IsNullOrEmpty(p.PARENT_CODE)).ToList(),
                    DataUsaha = ErasoftDbContext.SIFSYS.Single(p => p.BLN == 1)
                };

                var listData = new List<object>();
                var listFinalData = vm.ListStf02S.Where(b => b.NAMA.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                             b.NAMA2.ToLower().Contains(data.SearchParam.ToLower())).ToList();

                switch (data.SortBy)
                {
                    case 1:
                        listFinalData = listFinalData.OrderBy(d => d.BRG).ToList();
                        break;
                    case 2:
                        listFinalData = listFinalData.OrderBy(d => d.NAMA).ToList();
                        break;
                    case 3:
                        listFinalData = listFinalData.OrderBy(d => d.KET_SORT1).ToList();
                        break;
                    case 4:
                        listFinalData = listFinalData.OrderBy(d => d.KET_SORT2).ToList();
                        break;
                    case 5:
                        listFinalData = listFinalData.OrderBy(d => d.HJUAL).ToList();
                        break;
                }

                foreach (var barang in listFinalData)
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
                            listData.Add(new 
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

        [System.Web.Http.Route("api/mobile/barangpalinglaku")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public IHttpActionResult DataBarangPalingLaku([FromBody]JsonData data)
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

                ErasoftDbContext = data.DbPath == "ERASOFT" ? new ErasoftContext() : new ErasoftContext("", data.DbPath);

                var vm = new BarangViewModel()
                {
                    ListStf02S = ErasoftDbContext.STF02.ToList(),
                    ListMarket = ErasoftDbContext.ARF01.OrderBy(p => p.RecNum).ToList(),
                    ListHargaJualPermarketView = ErasoftDbContext.STF02H.OrderBy(p => p.IDMARKET).ToList(),
                    ListCategoryBlibli = MoDbContext.CategoryBlibli.Where(p => string.IsNullOrEmpty(p.PARENT_CODE)).ToList(),
                    DataUsaha = ErasoftDbContext.SIFSYS.Single(p => p.BLN == 1)
                };

                var listData = new List<object>();
                var listFinalData = vm.ListStf02S.Where(b => b.NAMA.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                             b.NAMA2.ToLower().Contains(data.SearchParam.ToLower())).ToList();

                switch (data.SortBy)
                {
                    case 1:
                        listFinalData = listFinalData.OrderBy(d => d.BRG).ToList();
                        break;
                    case 2:
                        listFinalData = listFinalData.OrderBy(d => d.NAMA).ToList();
                        break;
                    case 3:
                        listFinalData = listFinalData.OrderBy(d => d.KET_SORT1).ToList();
                        break;
                    case 4:
                        listFinalData = listFinalData.OrderBy(d => d.KET_SORT2).ToList();
                        break;
                    case 5:
                        listFinalData = listFinalData.OrderBy(d => d.HJUAL).ToList();
                        break;
                }

                foreach (var barang in listFinalData)
                {
                    var listBarangTerpesan = ErasoftDbContext.SOT01B.Where(b => b.BRG == barang.BRG).ToList();

                    if (listBarangTerpesan.Count > 0)
                    {
                        listData.Add(new
                        {
                            KodeBrg = barang.BRG,
                            NamaBrg = $"{barang.NAMA} {barang.NAMA2}",
                            Kategori = barang.KET_SORT1,
                            Merk = barang.KET_SORT2,
                            HJual = barang.HJUAL,
                        });
                    }
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

        [System.Web.Http.Route("api/mobile/promosibarang")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public IHttpActionResult DataPromosiBarang([FromBody]JsonData data)
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

                ErasoftDbContext = data.DbPath == "ERASOFT" ? new ErasoftContext() : new ErasoftContext("", data.DbPath);

                var vm = new PromosiViewModel()
                {
                    ListPromosi = ErasoftDbContext.PROMOSI.ToList(),
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListPelanggan = ErasoftDbContext.ARF01.ToList(),
                    ListMarketplace = MoDbContext.Marketplaces.ToList()
                };

                var listData = new List<object>();
                var listFinalData = vm.ListPromosi.Where(p => p.NAMA_MARKET.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                              p.NAMA_PROMOSI.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                              p.TGL_MULAI.ToString().Contains(data.SearchParam.ToLower()) ||
                                                              p.TGL_AKHIR.ToString().Contains(data.SearchParam.ToLower())).ToList();

                switch (data.SortBy)
                {
                    case 1:
                        listFinalData = listFinalData.OrderBy(d => d.NAMA_PROMOSI).ToList();
                        break;
                    case 2:
                        listFinalData = listFinalData.OrderBy(d => d.NAMA_MARKET).ToList();
                        break;
                    case 3:
                        listFinalData = listFinalData.OrderBy(d => d.TGL_MULAI).ToList();
                        break;
                    case 4:
                        listFinalData = listFinalData.OrderBy(d => d.TGL_AKHIR).ToList();
                        break;
                }

                foreach (var barang in listFinalData)
                {
                    listData.Add(new
                    {
                        BarangPromosi = barang
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

        [System.Web.Http.Route("api/mobile/hargajualbarang")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public IHttpActionResult DataHargaJualBarang([FromBody]JsonData data)
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

                ErasoftDbContext = data.DbPath == "ERASOFT" ? new ErasoftContext() : new ErasoftContext("", data.DbPath);

                var vm = new HargaJualViewModel()
                {
                    ListBarang = ErasoftDbContext.STF02.ToList(),
                    ListHargaJualPerMarket = ErasoftDbContext.STF02H.ToList(),
                    ListHargaTerakhir = ErasoftDbContext.STF10.ToList()
                };

                var listData = new List<ResultDataHargaJualBarang>();
                var listFinalData = vm.ListHargaJualPerMarket.Where(p => p.AKUNMARKET.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                              p.BRG.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                              p.CATEGORY_NAME.ToString().Contains(data.SearchParam.ToLower()) ||
                                                              p.HJUAL.ToString("N").Contains(data.SearchParam.ToLower())).ToList();

                foreach (var barangDijualPerMarket in listFinalData)
                {
                    var namaMarket = MoDbContext.Marketplaces.FirstOrDefault(m => m.IdMarket == barangDijualPerMarket.IDMARKET)?.NamaMarket;
                    var namaDepanBarang = vm?.ListBarang?.FirstOrDefault(b => b.BRG == barangDijualPerMarket.BRG)?.NAMA;
                    var namaBelakangBarang = vm?.ListBarang?.FirstOrDefault(b => b.BRG == barangDijualPerMarket.BRG)?.NAMA2;
                    var hargaTerakhir = vm?.ListHargaTerakhir?.FirstOrDefault(b => b.BRG == barangDijualPerMarket.BRG)?.HPOKOK;

                    listData.Add(new ResultDataHargaJualBarang()
                    {
                        Barang = barangDijualPerMarket,
                        NamaBarang = $"{namaDepanBarang} {namaBelakangBarang}",
                        AkunMarket = $"{namaMarket} ({barangDijualPerMarket.AKUNMARKET})",
                        HargaTerakhir = hargaTerakhir
                    });
                }

                switch (data.SortBy)
                {
                    case 1:
                        listData = listData.OrderBy(d => d.Barang.BRG).ToList();
                        break;
                    case 2:
                        listData = listData.OrderBy(d => d.NamaBarang).ToList();
                        break;
                    case 3:
                        listData = listData.OrderBy(d => d.AkunMarket).ToList();
                        break;
                    case 4:
                        listData = listData.OrderBy(d => d.HargaTerakhir).ToList();
                        break;
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

        // --- BARANG (END) --- //

        // --- AKUNTING (BEGIN) --- //

        [System.Web.Http.Route("api/mobile/jurnal")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public IHttpActionResult DataJurnalAkunting([FromBody]JsonData data)
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

                ErasoftDbContext = data.DbPath == "ERASOFT" ? new ErasoftContext() : new ErasoftContext("", data.DbPath);

                var vm = new JurnalViewModel()
                {
                    ListJurnal = ErasoftDbContext.GLFTRAN1.ToList(),
                    ListRekening = ErasoftDbContext.GLFREKs.ToList(),
                    ListJurnalDetail = ErasoftDbContext.GLFTRAN2.ToList()
                };

                var listData = new List<ResultDataJurnalAkunting>();
                var listFinalData = vm.ListJurnal.Where(j => j.bukti.ToLower().Contains(data.SearchParam.ToLower()) ||
                                                             j.tgl.ToString("dd/MM/yyyy").Contains(data.SearchParam.ToLower()) ||
                                                             j.tdebet.ToString("N").Contains(data.SearchParam) ||
                                                             j.tkredit.ToString("N").Contains(data.SearchParam)).ToList();

                foreach (var jurnal in listFinalData)
                {
                    var bukti = jurnal.bukti;
                    var lks = jurnal.lks;
                    var totalDebet = 0d;
                    var totalKredit = 0d;

                    totalDebet = 0d;
                    totalKredit = 0d;

                    foreach (var rekening in vm.ListJurnalDetail.Where(a => a.bukti.Equals(bukti) && a.lks.Equals(lks)))
                    {
                        if (rekening.dk == "D")
                        {
                            totalDebet += rekening.nilai;
                        }
                        else
                        {
                            totalKredit += rekening.nilai;
                        }
                    }

                    listData.Add(new ResultDataJurnalAkunting
                    {
                        Jurnal = jurnal,
                        Debet = totalDebet,
                        Kredit = totalKredit
                    });
                }

                switch (data.SortBy)
                {
                    case 1:
                        listData = listData.OrderBy(d => d.Jurnal.bukti).ToList();
                        break;
                    case 2:
                        listData = listData.OrderBy(d => d.Jurnal.tgl).ToList();
                        break;
                    case 3:
                        listData = listData.OrderBy(d => d.Debet).ToList();
                        break;
                    case 4:
                        listData = listData.OrderBy(d => d.Kredit).ToList();
                        break;
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

        // --- AKUNTING (END) --- //

        // --- CHART (BEGIN) --- //
        public class WeekOnMonth
        {
            public string Week_Start_Date { get; set; }
        }
        public class tempMonth
        {
            public int no { get; set; }
            public string nmMonth { get; set; }
        }
        [System.Web.Http.Route("api/chart")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public IHttpActionResult DashboardChart([FromBody]JsonData data)
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

                var dbPath = MoDbContext.Account.Single(ac => ac.AccountId == data.AccId).DatabasePathErasoft;
                ErasoftDbContext = dbPath == "ERASOFT" ? new ErasoftContext() : new ErasoftContext("", dbPath);

                var selectedDate = (data.SelDate != "" ? DateTime.ParseExact(data.SelDate, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture) : DateTime.Today.Date);

                var selectedMonth = (data.SelDate != "" ? DateTime.ParseExact(data.SelDate, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture).Month : DateTime.Today.Month);

                var vm = new DashboardViewModel()
                {
                    //remark by calvin 17 september 2019
                    //ListPesanan = ErasoftDbContext.SOT01A.Where(p => p.TGL.Value.Month == selectedMonth && p.TGL.Value.Year == selectedDate.Year).ToList(),
                    //ListFaktur = ErasoftDbContext.SIT01A.Where(p => p.TGL.Month == selectedMonth && p.TGL.Year == selectedDate.Year).ToList(),
                    //end remark by calvin 17 september 2019

                    ListAkunMarketplace = ErasoftDbContext.ARF01.ToList(),
                    ListMarket = MoDbContext.Marketplaces.ToList(),
                };

                //change by calvin 17 september 2019
                //// Pesanan
                //vm.JumlahPesananHariIni = vm.ListPesanan?.Where(p => p.TGL?.Date == selectedDate).Count();
                //vm.NilaiPesananHariIni = vm.ListPesanan?.Where(p => p.TGL?.Date == selectedDate).Sum(p => p.NETTO);
                //vm.JumlahPesananBulanIni = vm.ListPesanan?.Where(p => p.TGL?.Month == selectedMonth).Count();
                //vm.NilaiPesananBulanIni = vm.ListPesanan?.Where(p => p.TGL?.Month == selectedMonth).Sum(p => p.NETTO);

                //// Faktur
                //vm.JumlahFakturHariIni = vm.ListFaktur?.Where(p => p.TGL == selectedDate && p.JENIS_FORM == "2").Count();
                //vm.NilaiFakturHariIni = vm.ListFaktur?.Where(p => p.TGL == selectedDate && p.JENIS_FORM == "2").Sum(p => p.NETTO);
                //vm.JumlahFakturBulanIni = vm.ListFaktur?.Where(p => p.TGL.Month == selectedMonth && p.JENIS_FORM == "2").Count();
                //vm.NilaiFakturBulanIni = vm.ListFaktur?.Where(p => p.TGL.Month == selectedMonth && p.JENIS_FORM == "2").Sum(p => p.NETTO);


                //// Retur
                //vm.JumlahReturHariIni = vm.ListFaktur?.Where(p => p.TGL == selectedDate && p.JENIS_FORM == "3").Count();
                //vm.NilaiReturHariIni = vm.ListFaktur?.Where(p => p.TGL == selectedDate && p.JENIS_FORM == "3").Sum(p => p.NETTO);
                //vm.JumlahReturBulanIni = vm.ListFaktur?.Where(p => p.TGL.Month == selectedMonth && p.JENIS_FORM == "3").Count();
                //vm.NilaiReturBulanIni = vm.ListFaktur?.Where(p => p.TGL.Month == selectedMonth && p.JENIS_FORM == "3").Sum(p => p.NETTO);

                var ListPesanan = ErasoftDbContext.SOT01A.Where(p => p.TGL.Value.Month == selectedMonth && p.TGL.Value.Year == selectedDate.Year);
                var ListFaktur = ErasoftDbContext.SIT01A.Where(p => p.TGL.Month == selectedMonth && p.TGL.Year == selectedDate.Year);

                // Pesanan
                vm.JumlahPesananHariIni = ListPesanan.Where(p => System.Data.Entity.DbFunctions.TruncateTime(p.TGL.Value) == selectedDate).Count();
                vm.NilaiPesananHariIni = ListPesanan.Where(p => System.Data.Entity.DbFunctions.TruncateTime(p.TGL.Value) == selectedDate).Sum(p => (double?)(p.NETTO)) ?? 0;
                vm.JumlahPesananBulanIni = ListPesanan.Where(p => p.TGL.Value.Month == selectedMonth).Count();
                vm.NilaiPesananBulanIni = ListPesanan.Where(p => p.TGL.Value.Month == selectedMonth).Sum(p => (double?)(p.NETTO)) ?? 0;

                // Faktur
                vm.JumlahFakturHariIni = ListFaktur.Where(p => p.TGL == selectedDate && p.JENIS_FORM == "2").Count();
                vm.NilaiFakturHariIni = ListFaktur.Where(p => p.TGL == selectedDate && p.JENIS_FORM == "2").Sum(p => (double?)(p.NETTO)) ?? 0;
                vm.JumlahFakturBulanIni = ListFaktur.Where(p => p.TGL.Month == selectedMonth && p.JENIS_FORM == "2").Count();
                vm.NilaiFakturBulanIni = ListFaktur.Where(p => p.TGL.Month == selectedMonth && p.JENIS_FORM == "2").Sum(p => (double?)(p.NETTO)) ?? 0;


                // Retur
                vm.JumlahReturHariIni = ListFaktur.Where(p => p.TGL == selectedDate && p.JENIS_FORM == "3").Count();
                vm.NilaiReturHariIni = ListFaktur.Where(p => p.TGL == selectedDate && p.JENIS_FORM == "3").Sum(p => (double?)(p.NETTO)) ?? 0;
                vm.JumlahReturBulanIni = ListFaktur.Where(p => p.TGL.Month == selectedMonth && p.JENIS_FORM == "3").Count();
                vm.NilaiReturBulanIni = ListFaktur.Where(p => p.TGL.Month == selectedMonth && p.JENIS_FORM == "3").Sum(p => (double?)(p.NETTO)) ?? 0;
                //end change by calvin 17 september 2019

                //add by nurul 11/7/2019
                #region pesanan
                var pesananMingguIni = ListPesanan.Where(a => a.TGL.Value.DayOfWeek == selectedDate.DayOfWeek).ToList();
                if (pesananMingguIni.Count() > 0)
                {
                    List<String> nmDay = new List<String>();
                    for (int i = 0; i < 7; i++)
                    {
                        nmDay.Add(CultureInfo.CurrentUICulture.DateTimeFormat.DayNames[i]);
                    
                    //string[] day = { "Sunday","Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
                    //for (int i = 0; i < nmDay.Length; i++)
                    //{
                        var cekjumlahPesanan = pesananMingguIni.Where(a => Convert.ToString(a.TGL.Value.DayOfWeek) == nmDay[i]).Count();
                        //var cekNilaiPesanan = pesananMingguIni.Where(a => Convert.ToString(a.TGL.Value.DayOfWeek) == day[i]).Sum(p => p.NETTO);
                        var NilaiPesanan = $"Rp {String.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:N}", pesananMingguIni.Where(a => Convert.ToString(a.TGL.Value.DayOfWeek) == nmDay[i]).Sum(p => (double?)(p.NETTO)) ?? 0)}";
                        //var cekPesanan1 = (from a in pesananMingguIni
                        //                   where Convert.ToString(a.TGL.Value.DayOfWeek) == day[i]
                        //                   select a);
                        vm.ListdashboardPesananMingguan.Add(new DashboardMingguanModel()
                        {
                            No = nmDay[i],
                            Jumlah = cekjumlahPesanan.ToString(),
                            Nilai = NilaiPesanan
                        });
                    }
                }
                //var pesananBulanIni = vm.ListPesanan.Where(a => a.TGL.Value.Month == selectedDate.Month).ToList();
                //if (pesananBulanIni.Count() > 0)
                //{
                //    //List<String> getWeek = new List<String>();
                //    ////var minggu = ErasoftDbContext.Database.SqlQuery<WeekOnMonth>("SELECT  DATEADD(DAY, 2 - DATEPART(WEEKDAY, GETDATE()), CAST(GETDATE() AS DATE))");
                //    //var minggu = ErasoftDbContext.Database.SqlQuery<WeekOnMonth>("SELECT CONVERT(VARCHAR,DATEADD(DAY, 2 - 5, '" + selectedDate.ToString("yyyy-MM-dd") + "'),103) [Week_Start_Date]").Single();
                //    //getWeek.Add(Convert.ToString(minggu.Week_Start_Date));
                //    //List<String> nmMonth = new List<String>();
                //    for (int i = 0; i < 12; i++)
                //    {
                //        //nmMonth.Add(CultureInfo.CurrentUICulture.DateTimeFormat.MonthNames[i]);
                //        //var ch = new tempMonth()
                //        //{
                //        //    no = i,
                //        //    nmMonth = CultureInfo.CurrentUICulture.DateTimeFormat.MonthNames[i]
                //        //};
                    
                    
                    
                //    //for (int i = 0; i < ch ; i++)
                //    //{
                //        var cekjumlahPesanan = pesananMingguIni.Where(a => a.TGL.Value.Month == i).Count();
                //        //var cekNilaiPesanan = pesananMingguIni.Where(a => Convert.ToString(a.TGL.Value.DayOfWeek) == day[i]).Sum(p => p.NETTO);
                //        var NilaiPesanan = $"Rp {String.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:N}", pesananMingguIni.Where(a => Convert.ToString(a.TGL.Value.DayOfWeek) == getWeek[i]).Sum(p => p.NETTO))}";
                //        //var cekPesanan1 = (from a in pesananMingguIni
                //        //                   where Convert.ToString(a.TGL.Value.DayOfWeek) == day[i]
                //        //                   select a);
                //        vm.ListdashboardPesananMingguan.Add(new DashboardMingguanModel()
                //        {
                //            No = CultureInfo.CurrentUICulture.DateTimeFormat.MonthNames[i],
                //            Jumlah = cekjumlahPesanan.ToString(),
                //            Nilai = NilaiPesanan
                //        });
                //    //}
                //    }
                //}
                var pesananTahunIni = ListPesanan.Where(a => a.TGL.Value.Year == selectedDate.Year).ToList();
                if (pesananTahunIni.Count() > 0)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        //var ch = new tempMonth()
                        //{
                        //    no = i,
                        //    nmMonth = CultureInfo.CurrentUICulture.DateTimeFormat.MonthNames[i]
                        //}
                        var cekjumlahPesanan = pesananTahunIni.Where(a => a.TGL.Value.Month == i).Count();
                        var NilaiPesanan = $"Rp {String.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:N}", pesananTahunIni.Where(a => a.TGL.Value.Month == i).Sum(p => (double?)(p.NETTO)) ?? 0)}";
                        vm.ListdashboardPesananTahunan.Add(new DashboardTahunanModel()
                        {
                            No = CultureInfo.CurrentUICulture.DateTimeFormat.MonthNames[i],
                            Jumlah = cekjumlahPesanan.ToString(),
                            Nilai = NilaiPesanan
                        });
                    }
                }
                #endregion
                #region faktur
                var fakturMingguIni = ListFaktur.Where(a => a.TGL.DayOfWeek == selectedDate.DayOfWeek && a.JENIS_FORM == "2").ToList();
                if (fakturMingguIni.Count() > 0)
                {
                    List<String> nmDay = new List<String>();
                    for (int i = 0; i < 7; i++)
                    {
                        nmDay.Add(CultureInfo.CurrentUICulture.DateTimeFormat.DayNames[i]);

                        //string[] day = { "Sunday","Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
                        //for (int i = 0; i < nmDay.Length; i++)
                        //{
                        var cekjumlahFaktur = fakturMingguIni.Where(a => Convert.ToString(a.TGL.DayOfWeek) == nmDay[i]).Count();
                        //var cekNilaiPesanan = pesananMingguIni.Where(a => Convert.ToString(a.TGL.Value.DayOfWeek) == day[i]).Sum(p => p.NETTO);
                        var NilaiFaktur = $"Rp {String.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:N}", fakturMingguIni.Where(a => Convert.ToString(a.TGL.DayOfWeek) == nmDay[i]).Sum(p => (double?)(p.NETTO)) ?? 0)}";
                        //var cekPesanan1 = (from a in pesananMingguIni
                        //                   where Convert.ToString(a.TGL.Value.DayOfWeek) == day[i]
                        //                   select a);
                        vm.ListdashboardFakturMingguan.Add(new DashboardMingguanModel()
                        {
                            No = nmDay[i],
                            Jumlah = cekjumlahFaktur.ToString(),
                            Nilai = NilaiFaktur
                        });
                    }
                }
                var fakturTahunIni = ListFaktur.Where(a => a.TGL.Year == selectedDate.Year && a.JENIS_FORM == "2").ToList();
                if (fakturTahunIni.Count() > 0)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        //var ch = new tempMonth()
                        //{
                        //    no = i,
                        //    nmMonth = CultureInfo.CurrentUICulture.DateTimeFormat.MonthNames[i]
                        //}
                        var cekjumlahFaktur = fakturTahunIni.Where(a => a.TGL.Month == i).Count();
                        var NilaiFaktur = $"Rp {String.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:N}", fakturTahunIni.Where(a => a.TGL.Month == i).Sum(p => (double?)(p.NETTO)) ?? 0)}";
                        vm.ListdashboardFakturTahunan.Add(new DashboardTahunanModel()
                        {
                            No = CultureInfo.CurrentUICulture.DateTimeFormat.MonthNames[i],
                            Jumlah = cekjumlahFaktur.ToString(),
                            Nilai = NilaiFaktur
                        });
                    }
                }
                #endregion
                #region retur
                var returMingguIni = ListFaktur.Where(a => a.TGL.DayOfWeek == selectedDate.DayOfWeek && a.JENIS_FORM == "3").ToList();
                if (returMingguIni.Count() > 0)
                {
                    List<String> nmDay = new List<String>();
                    for (int i = 0; i < 7; i++)
                    {
                        nmDay.Add(CultureInfo.CurrentUICulture.DateTimeFormat.DayNames[i]);

                        //string[] day = { "Sunday","Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
                        //for (int i = 0; i < nmDay.Length; i++)
                        //{
                        var cekjumlahRetur = returMingguIni.Where(a => Convert.ToString(a.TGL.DayOfWeek) == nmDay[i]).Count();
                        //var cekNilaiPesanan = pesananMingguIni.Where(a => Convert.ToString(a.TGL.Value.DayOfWeek) == day[i]).Sum(p => p.NETTO);
                        var NilaiRetur = $"Rp {String.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:N}", returMingguIni.Where(a => Convert.ToString(a.TGL.DayOfWeek) == nmDay[i]).Sum(p => (double?)(p.NETTO)) ?? 0)}";
                        //var cekPesanan1 = (from a in pesananMingguIni
                        //                   where Convert.ToString(a.TGL.Value.DayOfWeek) == day[i]
                        //                   select a);
                        vm.ListdashboardReturMingguan.Add(new DashboardMingguanModel()
                        {
                            No = nmDay[i],
                            Jumlah = cekjumlahRetur.ToString(),
                            Nilai = NilaiRetur
                        });
                    }
                }
                var returTahunIni = ListFaktur.Where(a => a.TGL.Year == selectedDate.Year && a.JENIS_FORM == "3").ToList();
                if (returTahunIni.Count() > 0)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        //var ch = new tempMonth()
                        //{
                        //    no = i,
                        //    nmMonth = CultureInfo.CurrentUICulture.DateTimeFormat.MonthNames[i]
                        //}
                        var cekjumlahRetur = returTahunIni.Where(a => a.TGL.Month == i).Count();
                        var NilaiRetur = $"Rp {String.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:N}", returTahunIni.Where(a => a.TGL.Month == i).Sum(p => (double?)(p.NETTO)) ?? 0)}";
                        vm.ListdashboardReturTahunan.Add(new DashboardTahunanModel()
                        {
                            No = CultureInfo.CurrentUICulture.DateTimeFormat.MonthNames[i],
                            Jumlah = cekjumlahRetur.ToString(),
                            Nilai = NilaiRetur
                        });
                    }
                }
                #endregion
                //end add by nurul 11/7/2019

                if (vm.ListAkunMarketplace.Count > 0)
                {
                    foreach (var marketplace in vm.ListAkunMarketplace)
                    {
                        var idMarket = Convert.ToInt32(marketplace.NAMA);
                        var namaMarket = vm.ListMarket.Single(m => m.IdMarket == idMarket).NamaMarket;

                        var jumlahPesananToday = ListPesanan
                            .Where(p => p.CUST == marketplace.CUST && System.Data.Entity.DbFunctions.TruncateTime(p.TGL.Value) == selectedDate).Count();
                        var nilaiPesananToday = $"Rp {String.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:N}", ListPesanan.Where(p => p.CUST == marketplace.CUST && System.Data.Entity.DbFunctions.TruncateTime(p.TGL.Value) == selectedDate).Sum(p => (double?)(p.NETTO)) ?? 0)}";

                        var jumlahPesananMonth = ListPesanan

                            .Where(p => p.CUST == marketplace.CUST && p.TGL.Value.Month == selectedMonth).Count();
                        var nilaiPesananMonth = $"Rp {String.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:N}", ListPesanan.Where(p => p.CUST == marketplace.CUST && p.TGL.Value.Month == selectedMonth).Sum(p => (double?)(p.NETTO)) ?? 0)}";

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

        // --- CHART (END) --- //
    }
}
