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
                    code = 400,
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
                    code = 400,
                    message = "Wrong API KEY!",
                    data = null
                };

                return Json(result);
            }

            if (account.Password != account.ConfirmPassword)
            {
                result = new JsonApi()
                {
                    code = 200,
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
                    code = 200,
                    message = "Email sudah terdaftar di database kami!",
                    data = null
                };

                return Json(result);
            }

            if (String.IsNullOrWhiteSpace(account.PhotoKtpBase64))
            {
                result = new JsonApi()
                {
                    code = 200,
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
                ErasoftDbContext = new ErasoftContext();

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

                var result = new JsonApi()
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
                    message = e.Message + data.SelDate,
                    data = null
                };

                return Json(result);
            }
        }
    }
}
