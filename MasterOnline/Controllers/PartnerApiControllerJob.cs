using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Threading.Tasks;
using MasterOnline.Models;
using System.Data;
using System.Data.SqlClient;
using MasterOnline.ViewModels;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using Erasoft.Function;
using Hangfire;
using Hangfire.SqlServer;

namespace MasterOnline.Controllers
{
    public class PartnerApiControllerJob : Controller
    {
        private MoDbContext MoDbContext { get; set; }
        private ErasoftContext ErasoftDbContext { get; set; }
        private DatabaseSQL EDB;
        private string username;
        string dbPathEra = "";
        string DataSourcePath = "";
        string dbSourceEra = "";

        // GET: PartnerApiControllerJob
        public ActionResult Index()
        {
            return View();
        }

        protected void SetupContext(string dbPathEra, string dbSourceEra)
        {
            //string ret = "";
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(dbPathEra);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);
            //username = user_name;
            //return ret;
        }

        public async Task<string> TadaAuthorization(PartnerApiData data)
        {
            string token = "";
            string url = "https://api.gift.id/v1/pos/token";

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);

            var encodedData = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(data.ClientId + ":" + data.ClientSecret));

            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", "Basic " + encodedData);
            myReq.Accept = "*/*";
            myReq.ContentType = "application/json";
            myReq.ContentLength = 0;
            string myData = "{\"username\":\"" + data.Username + "\",\"password\":\"" + data.Password + "\",\"grant_type\":\"password\",\"scope\":\"offline_access\"}";

            string responseFromServer = "";

            try
            {
                myReq.ContentLength = myData.Length;
                using (var dataStream = myReq.GetRequestStream())
                {
                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
                }
                using (WebResponse response = await myReq.GetResponseAsync())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseFromServer = reader.ReadToEnd();
                    }
                }

                var response_token = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokenClass)) as TokenClass;
                //var date_expires = DateTime.UtcNow.AddHours(7).AddDays((response_token.expires_in) / 86400).ToString("yyyy-MM-dd HH:mm:ss");// +1
                var date_expires = response_token.expiredAt.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
                ErasoftDbContext.Database.ExecuteSqlCommand("UPDATE PARTNER_API SET Access_Token = '" + response_token.access_token + "', Session_ExpiredDate = '" + date_expires + "' WHERE PartnerId = 30007 ");
                token = response_token.access_token;
            }
            catch (WebException e)
            {
                string err = "";
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        err = sr.ReadToEnd();
                    }
                }
            }
            return token;
        }

        [AutomaticRetry(Attempts = 0)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("{obj}")]
        public async Task<string> TADATopupByPhoneJob(string DatabasePathErasoft, string msgError, string cust, string logActionCategory, string logActionName, string dbSourceEra)
        {
            string ret = "";
            SetupContext(DatabasePathErasoft, dbSourceEra);

            var partnerDb = ErasoftDbContext.PARTNER_API.SingleOrDefault(p => p.PartnerId == 30007);

            string idProgram = partnerDb.ProgramId;
            string idWallet = partnerDb.WalletId;
            string token = partnerDb.Access_Token;
            string fs_id = partnerDb.fs_id.ToString();

            PartnerApiData data = new PartnerApiData()
            {
                ClientId = partnerDb.ClientId,
                ClientSecret = partnerDb.ClientSecret,
                Username = partnerDb.Username,
                Password = partnerDb.Password
            };

            if (DateTime.UtcNow.AddHours(7) >= partnerDb.Session_ExpiredDate || partnerDb.Session_ExpiredDate == null)
            {
                token = await TadaAuthorization(data);
            }

            try
            {
                string dateFrom = DateTime.UtcNow.AddDays(-1).AddHours(7).ToString("yyyy-MM-dd");
                string dateTo = DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");

                string sSQLHeader = "SELECT ISNULL(SOA.NO_BUKTI,'') [NOBUK], ISNULL(ARC.TLP,'') [PHONE], " +
                        "ISNULL(SOA.NO_REFERENSI,'') [BILLNUMBER], ISNULL(SOA.BRUTO,'') [AMOUNT] " +
                        "FROM SOT01A SOA " +
                        "JOIN SIT01A SIA ON SOA.NO_BUKTI = SIA.NO_SO " +
                        "JOIN ARF01C ARC ON ARC.BUYER_CODE = SOA.PEMESAN " +
                        "WHERE SOA.STATUS_TRANSAKSI = '04'" +
                        //"AND SIA.TGL_KIRIM BETWEEN '" + dateFrom + "' and '" + dateTo + "'"; //04 //01 //TGL //TGL_KIRIM
                        "AND CONVERT(VARCHAR(25), SIA.TGL_KIRIM, 126) LIKE '" + dateFrom + "%'"; // SIA.TGL_KIRIM = '" + dateFrom + "'

                var sot01a = ErasoftDbContext.Database.SqlQuery<Order>(sSQLHeader).ToList();
                if (sot01a.Count == 0)
                {
                    ErasoftDbContext.Database.ExecuteSqlCommand(@"INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (2, 'Sync TADA', 'SOT01A_NULL', 'Data Pesanan dari tanggal " + dateFrom + " sampai tanggal " + dateTo + ", kosong.', dateadd(hour, 7, getdate()), null, 1) ");
                    //throw new Exception("Data Pesanan dari tanggal " + dateFrom + " sampai tanggal " + dateTo + ", kosong.");
                    return ret;
                }
                string listNobuk = "";
                foreach (var pesanan in sot01a)
                {
                    listNobuk += "'" + pesanan.NOBUK + "' , ";
                }
                listNobuk = listNobuk.Substring(0, listNobuk.Length - 2);

                string sSQLDetail = "SELECT ISNULL(SOB.NO_BUKTI,'') [NOBUK], ISNULL(SOB.BRG,'') [SKU], " +
                        "REPLACE(REPLACE(REPLACE(CONVERT(NVARCHAR(MAX),STF.NAMA + ' ' + ISNULL(STF.NAMA2, '')), CHAR(9), ' '), CHAR(10), ' ') , CHAR(13), ' ') [ITEMNAME], " +
                        "SOB.QTY [QUANTITY], SOB.H_SATUAN [PRICE] " +
                        "FROM SOT01B SOB JOIN STF02 STF ON STF.BRG = SOB.BRG " +
                        "WHERE SOB.NO_BUKTI IN (" + listNobuk + ")";

                var sot01b = ErasoftDbContext.Database.SqlQuery<OrderItem>(sSQLDetail).ToList();
                if (sot01b.Count == 0)
                {
                    ErasoftDbContext.Database.ExecuteSqlCommand(@"INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (2, 'Sync TADA', 'SOT01B_NULL', 'Data Barang kosong.', dateadd(hour, 7, getdate()), null, 1) ");
                    //throw new Exception("Data Barang kosong.");
                    return ret;
                }

                foreach (var a in sot01a)
                {
                    var dataheader = new TopupByPhone();

                    string phonenumber = "";
                    string initialnumber = a.PHONE.Substring(0, 2);
                    if (a.PHONE.Substring(0, 4) == "6208")
                    {
                        phonenumber = "+" + a.PHONE.Remove(2, 1);
                    }
                    else if (a.PHONE.Substring(0, 3) == "628")
                    {
                        phonenumber = "+" + a.PHONE;
                    }
                    else if (initialnumber == "08")
                    {
                        phonenumber = initialnumber.Replace("08", "+628") + a.PHONE.Remove(0, 2);
                    }
                    dataheader.phone = phonenumber;

                    dataheader.amount = a.AMOUNT;
                    //if (!string.IsNullOrEmpty(a.BILLNUMBER))
                    //    dataheader.billNumber = a.BILLNUMBER;
                    //else
                    dataheader.billNumber = a.NOBUK;
                    dataheader.programId = idProgram;
                    dataheader.walletId = idWallet;
                    dataheader.paymentMethod = "cash";
                    dataheader.items = new List<Item>();

                    var newsot01b = sot01b.Where(x => x.NOBUK == a.NOBUK).ToList();
                    foreach (var b in newsot01b)
                    {
                        var datadetail = new Item();
                        datadetail.sku = b.SKU;
                        datadetail.itemName = b.ITEMNAME;
                        datadetail.quantity = b.QUANTITY;
                        datadetail.price = b.PRICE;
                        dataheader.items.Add(datadetail);
                    }

                    string url = "https://api.gift.id/v1/pos/phone/topup";

                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);

                    myReq.Method = "POST";
                    myReq.Headers.Add("Authorization", "Bearer " + token);
                    myReq.Accept = "*/*";
                    myReq.ContentType = "application/json";
                    myReq.ContentLength = 0;
                    string myData = Newtonsoft.Json.JsonConvert.SerializeObject(dataheader);

                    string responseFromServer = "";
                    string idTrx = "";

                    try
                    {
                        myReq.ContentLength = myData.Length;
                        using (var dataStream = myReq.GetRequestStream())
                        {
                            dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
                        }
                        using (WebResponse response = await myReq.GetResponseAsync())
                        {
                            using (Stream stream = response.GetResponseStream())
                            {
                                StreamReader reader = new StreamReader(stream);
                                responseFromServer = reader.ReadToEnd();
                            }
                        }

                        if (responseFromServer.Contains("error"))
                        {
                            var response_error = JsonConvert.DeserializeObject(responseFromServer, typeof(ErrorTada)) as ErrorTada;
                            string errorMessage = response_error.error[0].ToString();
                            string messageError = response_error.message[0].ToString();
                            string msg_response = errorMessage.Replace("\n", "").Replace("\r", "").Replace("\"", "").Replace("[", "").Replace("]", "").Replace("\\", "").TrimStart();

                            ErasoftDbContext.Database.ExecuteSqlCommand(@"INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (2, 'Sync TADA', '" + a.NOBUK + "', '" + errorMessage + ".', dateadd(hour, 7, getdate()), null, 0) ");
                            continue;
                        }

                        var response_tada = JsonConvert.DeserializeObject(responseFromServer, typeof(Response_TADA)) as Response_TADA;
                        foreach (var trx in response_tada.transactions)
                        {
                            if (trx.trxType == "walletTopup")
                            {
                                idTrx = trx.id.ToString();
                            }
                        }

                        ErasoftDbContext.Database.ExecuteSqlCommand("INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (2, 'Sync TADA', '" + a.NOBUK + "', '', dateadd(hour, 7, getdate()), '" + responseFromServer + "', 1) ");
                        ErasoftDbContext.Database.ExecuteSqlCommand("INSERT INTO FAKTUR_API (No_Faktur, No_FakturRef, Tgl, LUNAS, PARTNER) VALUES ('" + a.NOBUK + "', '" + idTrx + "', dateadd(hour, 7, getdate()), '1', '2')"); //'" + id.ToString() + "'

                    }
                    catch (WebException e)
                    {
                        string err = "";
                        string errorMessage = "";
                        string messageError = "";
                        string messageResponse = "";

                        if (e.Status == WebExceptionStatus.ProtocolError)
                        {
                            WebResponse resp = e.Response;
                            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                            {
                                err = sr.ReadToEnd();
                            }

                            var response_error = JsonConvert.DeserializeObject(err, typeof(ErrorTada)) as ErrorTada;
                            if (response_error.message != null)
                            {
                                errorMessage = response_error.message;
                                errorMessage.Replace("\n", "").Replace("\r", "").Replace("\"", "").Replace("[", "").Replace("]", "").Replace("\\", "").Replace("'", "").TrimStart();
                                ErasoftDbContext.Database.ExecuteSqlCommand(@"INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (2, 'Sync TADA', '" + a.NOBUK + "', '" + errorMessage + ".', dateadd(hour, 7, getdate()), '" + myData + "', 0) ");
                                if (errorMessage.Contains("Access token expired"))
                                {
                                    token = await TadaAuthorization(data);
                                }
                                continue;
                            }
                            if (response_error.error != null)
                            {
                                messageError = response_error.error.system.message;
                                messageError.Replace("\n", "").Replace("\r", "").Replace("\"", "").Replace("[", "").Replace("]", "").Replace("\\", "").Replace("'", "").TrimStart();
                                ErasoftDbContext.Database.ExecuteSqlCommand(@"INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (2, 'Sync TADA', '" + a.NOBUK + "', '" + messageError + ".', dateadd(hour, 7, getdate()), '" + myData + "', 0) ");
                                if (messageError.Contains("Access token expired"))
                                {
                                    token = await TadaAuthorization(data);
                                }
                                continue;
                            }

                            //string errorMessage = response_error.error.ToString();
                            //string messageError = response_error.message[0].ToString();
                            //string msg_response = errorMessage.Replace("\n", "").Replace("\r", "").Replace("\"", "").Replace("[", "").Replace("]", "").Replace("\\", "").Replace("'", "").TrimStart();
                            //ErasoftDbContext.Database.ExecuteSqlCommand(@"INSERT INTO PARTNER_API_LOG_ERROR VALUES (2, 'Sync TADA', '" + a.NOBUK + "', '" + msg_response + ".', dateadd(hour, 7, getdate()), '" + myData + "', 0) ");
                            continue;
                        }
                        else
                        {
                            err = "Call API " + e.Message;
                        }

                        ErasoftDbContext.Database.ExecuteSqlCommand(@"INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (2, 'Sync TADA', '" + a.NOBUK + "', '" + err + ".', dateadd(hour, 7, getdate()), '" + myData + "', 0) ");
                        //ErasoftDbContext.Database.ExecuteSqlCommand(@"DELETE FROM PARTNER_API_LOG_ERROR WHERE Created_Date < DATEADD(DAY, -14, GETDATE()) ");
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ErasoftDbContext.Database.ExecuteSqlCommand(@"INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (2, 'Sync TADA', 'ERR_EXC', '" + msg + ".', dateadd(hour, 7, getdate()), null, 1) ");
                //ErasoftDbContext.Database.ExecuteSqlCommand(@"DELETE FROM PARTNER_API_LOG_ERROR WHERE Created_Date < DATEADD(DAY, -14, GETDATE()) ");
                //throw new Exception(msg);
            }

            ErasoftDbContext.Database.ExecuteSqlCommand(@"DELETE FROM PARTNER_API_LOG_ERROR WHERE Created_Date < DATEADD(DAY, -14, GETDATE()) ");

            return ret;
        }

        //add by nurul 9/6/2022
        public void logErrorFunction(string email, string modul, string nobukti, string keterangan, string json)
        {
             ErasoftDbContext.Database.ExecuteSqlCommand(@"BEGIN INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (1, '" + modul + "', '" + nobukti + "', '" + keterangan + "', dateadd(hour, 7, getdate()), '" + json + "', 1) END ");
        }
        public class getStokFisik
        {
            public string BRG { get; set; }
            public string GUD { get; set; }
            public double STOK_FISIK { get; set; }
        }
        public class HargaPbSt
        {
            public double harga_pb { get; set; }
            public double harga_st { get; set; }
        }
        //start from "STATUS_LOADING" is fixValueProcess
        string queryProcess = @"INSERT INTO STT01A (Tgl, Ref, UserName, TglInput, Nobuk, JTran, MK, Jenis_Form, STATUS_LOADING, Satuan, Ket, ST_Cetak, ST_Posting, JRef, Retur_Penuh, Terima_Penuh, VALUTA, TUKAR, TERIMA_PENUH_PO_QC, JLH_KARYAWAN, NILAI_ANGKUTAN, KOLI, BERAT, VOLUME) VALUES ( ";
        string fixValueProcess = "'0', '', '', '', '-', '6', 0, 0, 'IDR', 1, 0, 0, 0, 0, 0, 0)";

        //start from "Satuan" is fixValueProcessDetail
        string queryProcessDetail = @"INSERT INTO STT01B (Kobar, UserName, TglInput, Nobuk, Ke_Gd, Dr_Gd, Qty, Jenis_Form, Harsat, Harga, Satuan, Qty_Retur, Qty_Berat, TOTAL_LOT, TOTAL_QTY, QTY_TERIMA, QTY_CLAIM, NO_URUT_PO, NO_URUT_SJ, QTY_TERIMA_PO_QC) VALUES ";
        string fixValueProcessDetail = "'2', 0, 0, 0, 0, 0, 0, 0, 0, 0)";

        [Queue("1_create_product")]
        public string prosesStokOpname(string batch, string noStok, string email, string token, bool isAccurate, string DatabasePathErasoft, string dbSourceEra)
        {
            try
            {
                SetupContext(DatabasePathErasoft, dbSourceEra);
                string json = "";
                if (token.Contains("|"))
                {
                    string[] token_fs_id = token.Split('|');
                    token = token_fs_id[0];
                    json = token_fs_id[1];
                }
                //add by nurul 19/11/2021
                var listBrgUpdate = new List<string>();
                //end add by nurul 19/11/2021
                var stokOpDb = ErasoftDbContext.Database.SqlQuery<STT04A>("SELECT * FROM STT04A (NOLOCK) WHERE NOBUK = '" + noStok + "'").FirstOrDefault();
                //change by nurul 28/4/2022
                var stokDetailOpDb = ErasoftDbContext.STT04B.AsNoTracking().Where(b => b.NOBUK == noStok).ToList();
                //var stokDetailOpDb = ErasoftDbContext.Database.SqlQuery<STT04B>("SELECT *FROM STT04B (NOLOCK) WHERE NOBUK='" + noStok + "' ORDER BY NO").ToList();
                //end change by nurul 28/4/2022

                string todayPrcs = DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd 23:59:59.999");

                logErrorFunction(email, "03. Validasi STT04A", noStok, "-", json);
                //ErasoftDbContext.Database.ExecuteSqlCommand(@"BEGIN INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (1, '03. Validasi STT04A', '" + noStok + "', '-', dateadd(hour, 7, getdate()), NULL, 1) END ");

                if (stokOpDb != null)
                {
                    //int cekSTF09A = ErasoftDbContext.Database.SqlQuery<int>("SELECT COUNT(Bukti) FROM STF09A WHERE Tgl > '" + todayPrcs + "'").Single();
                    int cekSTF09A = ErasoftDbContext.Database.SqlQuery<int>("select count(bukti) from stf09a (nolock) where tgl >= '" + stokOpDb.TGL?.AddDays(1).ToString("yyyy-MM-dd 00:00:00.000") + "'").Single();

                    logErrorFunction(email, "04. Validasi STF09A", noStok, "-", json);
                    //ErasoftDbContext.Database.ExecuteSqlCommand(@"BEGIN INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (1, '04. Validasi STF09A', '" + noStok + "', '-', dateadd(hour, 7, getdate()), NULL, 1) END ");

                    if (cekSTF09A > 0)
                    {
                        return "Transaksi stok opname tidak dapat diproses karena sudah ada transaksi di atas tanggal " + stokOpDb.TGL?.ToString("dd/MM/yyyy") + " .";
                    }
                    else
                    {
                        //add by nurul 28/4/2022
                        string sSQLStok = "SELECT B.BRG,B.GUD, " +
                        "ISNULL((SELECT ISNULL(SUM(QAwal + QM1 + QM2 + QM3 + QM4 + QM5 + QM6 + QM7 + QM8 + QM9 + QM10 + QM11 + QM12) - SUM(QK1 + QK2 + QK3 + QK4 + QK5 + QK6 + QK7 + QK8 + QK9 + QK10 + QK11 + QK12), 0) FROM STF08A A(NOLOCK) " +
                        "WHERE Tahun = YEAR(DATEADD(HOUR, +7, GETUTCDATE())) AND NOBUK = '" + noStok + "' AND BRG = B.BRG AND A.GD = B.GUD),0) AS STOK_FISIK " +
                        "FROM STT04B B(NOLOCK) WHERE NOBUK = '" + noStok + "'";
                        var stokFisik = ErasoftDbContext.Database.SqlQuery<getStokFisik>(sSQLStok).ToList();
                        //end add by nurul 28/4/2022

                        logErrorFunction(email, "05. Cek stok fisik STF08A", noStok, "-", json);
                        //ErasoftDbContext.Database.ExecuteSqlCommand(@"BEGIN INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (1, '05. Cek stok fisik STF08A', '" + noStok + "', '-', dateadd(hour, 7, getdate()), NULL, 1) END ");

                        var noStokOM = new ManageController().GenerateAutoNumber(ErasoftDbContext, "OM", "STT01A", "Nobuk");
                        var noStokOK = new ManageController().GenerateAutoNumber(ErasoftDbContext, "OK", "STT01A", "Nobuk");

                        //change by nurul 28/4/2022
                        //string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        string now = DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        //end change by nurul 28/4/2022
                        string TGL = stokOpDb.TGL?.ToString("yyyy-MM-dd HH:mm:ss.fff");

                        string comma = ",";
                        string exec = "";

                        int jmRowOM = 0; int jmRowOK = 0;
                        foreach (var item in stokDetailOpDb)
                        {
                            string query = queryProcess;
                            //change by nurul 28/4/2022
                            //string sSQL = @"SELECT ISNULL(SUM(QAwal+QM1+QM2+QM3+QM4+QM5+QM6+QM7+QM8+QM9+QM10+QM11+QM12) - SUM(QK1+QK2+QK3+QK4+QK5+QK6+QK7+QK8+QK9+QK10+QK11+QK12), 0) AS STOK_FISIK 
                            //            FROM STF08A (NOLOCK) WHERE Tahun = YEAR(GETDATE()) AND BRG = '" + item.Brg + "' AND GD = '" + item.Gud + "'";
                            //double stok = ErasoftDbContext.Database.SqlQuery<double>(sSQL).Single();
                            var cekstok = stokFisik.Where(a => a.BRG == item.Brg && a.GUD == item.Gud).FirstOrDefault();
                            double stok = 0;
                            if (cekstok != null)
                            {
                                stok = cekstok.STOK_FISIK;
                            }
                            //end change by nurul 28/4/2022

                            //if (stokDetailOpDb.IndexOf(item) == stokDetailOpDb.Count() - 1 && stok == item.Qty)
                            //{
                            //    exec.Remove(exec.Length - 3, 3);
                            //    continue;
                            //}

                            //if (item.Qty == 0)
                            //    continue;

                            logErrorFunction(email, "06. Looping STT04B", noStok, item.Brg + ";F" + stok + ";S" + item.Qty, json);
                            try
                            {
                                if (stok < item.Qty)
                                {
                                    double selisihOM = item.Qty - stok;

                                    double Harsat = 0, Harga = 0;
                                    var sSQL1 = "select isnull((select top 1 isnull(hbeli,0) harga_pb from pbt01a a (nolock) inner join pbt01b b (nolock) on a.inv=b.inv where brg='" + item.Brg + "' and hbeli > 0 order by a.tgl desc, a.inv desc),0) harga_pb " +
                                            ", isnull((select top 1 isnull(harsat, 0) harga_st from stt01a a (nolock) inner join stt01b b (nolock) on a.nobuk = b.nobuk where kobar = '" + item.Brg + "' and harsat > 0 order by a.tgl desc, b.no desc),0) harga_st ";
                                    var cekPb_St = ErasoftDbContext.Database.SqlQuery<HargaPbSt>(sSQL1).SingleOrDefault();
                                    if (cekPb_St.harga_pb > 0)
                                    {
                                        Harsat = cekPb_St.harga_pb;
                                        Harga = cekPb_St.harga_pb * selisihOM;
                                    }
                                    else if (cekPb_St.harga_st > 0)
                                    {
                                        Harsat = cekPb_St.harga_st;
                                        Harga = cekPb_St.harga_st * selisihOM;
                                    }

                                    //comma = stokDetailOpDb.IndexOf(item) == stokDetailOpDb.Count() - 1 ? "" : ", \n";
                                    exec += "('" + item.Brg + "', '" + batch.ToString() + "', '" + now + "', '" + noStokOM + "', '" + item.Gud + "', '', " + selisihOM + ", 1, " + Harsat + ", " + Harga + ", " + fixValueProcessDetail + comma;

                                    jmRowOM++;
                                    if (jmRowOM == 1)
                                    {
                                        query += "'" + TGL + "', '" + noStok + "', '" + batch.ToString() + "', '" + now + "', '" + noStokOM + "', 'M', 'M', 1, " + fixValueProcess;
                                        //System.Threading.Thread.Sleep(5000);
                                        ErasoftDbContext.Database.ExecuteSqlCommand(query);
                                        query = "";

                                        logErrorFunction(email, "07. Insert STT01A OM ", noStok, noStokOM + ";" + item.Brg + ";F" + stok + ";S" + item.Qty, json);
                                        //ErasoftDbContext.Database.ExecuteSqlCommand(@"BEGIN INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (1, '05. Insert STT01A', '" + noStok + "', '"+ noStokOM + ";"+ item.Brg + ";F"+ stok + ";S"+ item.Qty + "', dateadd(hour, 7, getdate()), NULL, 1) END ");
                                        //ErasoftDbContext.Database.ExecuteSqlCommand(@"BEGIN INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (1, '06. Insert STT01A', '" + noStok + "', '" + noStokOM + "', dateadd(hour, 7, getdate()), NULL, 1) END ");
                                    }

                                    //add by nurul 19/11/2021
                                    listBrgUpdate.Add(item.Brg);
                                    //end add by nurul 19/11/2021
                                }
                            }
                            catch (Exception e)
                            {
                                ErasoftDbContext.Database.ExecuteSqlCommand(@"BEGIN INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (1, '1. Webhook Update Stok MO', '" + noStokOM + "', '" + e.Message + " | " + e.Source + " | " + e.StackTrace + ".', dateadd(hour, 7, getdate()), '', 0) END ");
                            }

                            try
                            {
                                if (stok > item.Qty)
                                {
                                    double selisihOK = stok - item.Qty;
                                    //comma = stokDetailOpDb.IndexOf(item) == stokDetailOpDb.Count() - 1 ? "" : ", \n";
                                    exec += "('" + item.Brg + "', '" + batch.ToString() + "', '" + now + "', '" + noStokOK + "', '', '" + item.Gud + "', " + selisihOK + ", 0, 0, 0, " + fixValueProcessDetail + comma;

                                    jmRowOK++;
                                    if (jmRowOK == 1)
                                    {
                                        query += "'" + TGL + "', '" + noStok + "', '" + batch.ToString() + "', '" + now + "', '" + noStokOK + "', 'K', 'K', 0, " + fixValueProcess;
                                        //System.Threading.Thread.Sleep(5000);
                                        ErasoftDbContext.Database.ExecuteSqlCommand(query);
                                        query = "";

                                        logErrorFunction(email, "07. Insert STT01A OK ", noStok, noStokOK + ";" + item.Brg + ";F" + stok + ";S" + item.Qty, json);
                                        //ErasoftDbContext.Database.ExecuteSqlCommand(@"BEGIN INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (1, '06. Insert STT01A', '" + noStok + "', '" + noStokOK + "', dateadd(hour, 7, getdate()), NULL, 1) END ");
                                    }

                                    //add by nurul 19/11/2021
                                    listBrgUpdate.Add(item.Brg);
                                    //end add by nurul 19/11/2021
                                }
                            }
                            catch (Exception ex)
                            {
                                ErasoftDbContext.Database.ExecuteSqlCommand(@"BEGIN INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (1, '2. Webhook Update Stok MO', '" + noStokOK + "', '" + ex.Message + " | " + ex.Source + " | " + ex.StackTrace + ".', dateadd(hour, 7, getdate()), '', 0) END ");
                            }
                        }

                        try
                        {
                            if (!string.IsNullOrEmpty(exec))
                            {
                                exec = exec.Substring(0, exec.Length - 1);
                                string exec_insert = exec.Replace("'", "\"");

                                //ErasoftDbContext.Database.ExecuteSqlCommand(@"BEGIN INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (1, '07,5. Pre-insert STT01B', '" + noStok + "', '" + exec_insert + "', dateadd(hour, 7, getdate()), NULL, 1) END ");
                                logErrorFunction(email, "07,5. Pre-insert STT01B_0", noStok, exec_insert, json);

                                //throw new Exception("TEST ERROR");

                                //exec.Remove(exec.Length - 3, 3);
                                ErasoftDbContext.Database.ExecuteSqlCommand(queryProcessDetail + exec);

                                if (exec.Length > 65535)
                                {
                                    exec = exec.Substring(0, 65535);
                                }
                                logErrorFunction(email, "08. Insert STT01B_0", noStok, exec_insert, json);
                                //ErasoftDbContext.Database.ExecuteSqlCommand(@"BEGIN INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (1, '07. Insert STT01B', '" + noStok + "', '" + exec_insert + "', dateadd(hour, 7, getdate()), NULL, 1) END ");
                            }
                        }
                        catch (Exception ey)
                        {
                            string error = ey.Message + " | " + ey.Source + " | " + ey.StackTrace;
                            ErasoftDbContext.Database.ExecuteSqlCommand(@"BEGIN INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (1, 'prosesStokOpname_exec_0 MO', '" + noStok + "', '" + error + ".', dateadd(hour, 7, getdate()), '', 1) END ");
                            //return error;
                            //if (error.Contains("Execution Timeout Expired"))
                            //{
                            string ret = retryInsertStt01b(email, "08. Insert STT01B_1", noStok, error, exec, json, 1);
                            if (ret != "RETRY")
                            {
                                return ey.Message;
                            }
                            //}
                        }


                        try
                        {
                            //add by nurul 19/11/2021
                            if (listBrgUpdate.Count() > 0)
                            {
                                var listBrgJson = Newtonsoft.Json.JsonConvert.SerializeObject(listBrgUpdate);
                                //UpdateStokMP(email, token, listBrgJson, isAccurate, dbID, "stokOpname");
                                string ConnId = "[WBH_STOK_OP][" + DateTime.UtcNow.AddHours(7).ToString("yyyyMMddhhmmss") + "]";
                                //new StokControllerJob().updateStockMarketPlace(ConnId, DatabasePathErasoft, "WebhookStokOp");
                                //new ManageController().updateStockMarketPlace(listBrgUpdate, ConnId);

                                var EDB = new DatabaseSQL(dbPathEra);
                                string sSQLValues = "";
                                foreach (var item in listBrgUpdate)
                                {
                                    sSQLValues = sSQLValues + "('" + item + "', '" + ConnId + "'),";
                                }

                                if (sSQLValues != "")
                                {
                                    sSQLValues = sSQLValues.Substring(0, sSQLValues.Length - 1);
                                    EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + sSQLValues);
                                    //stok bundling
                                    var sSQLInsertTempBundling = "INSERT INTO TEMP_ALL_MP_ORDER_ITEM_BUNDLING ([BRG],[CONN_ID],[TGL]) " +
                                                             "SELECT DISTINCT C.UNIT AS BRG, '" + ConnId + "' AS CONN_ID, DATEADD(HOUR, +7, GETUTCDATE()) AS TGL " +
                                                             "FROM TEMP_ALL_MP_ORDER_ITEM A (NOLOCK) " +
                                                             "LEFT JOIN TEMP_ALL_MP_ORDER_ITEM_BUNDLING B(NOLOCK) ON B.CONN_ID = '" + ConnId + "' AND A.BRG = B.BRG " +
                                                             "INNER JOIN STF03 C(NOLOCK) ON A.BRG = C.BRG " +
                                                             "WHERE ISNULL(A.CONN_ID,'') = '" + ConnId + "' " +
                                                             "AND ISNULL(B.BRG,'') = '' AND A.BRG <> 'NOT_FOUND'";
                                    var execInsertTempBundling = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, sSQLInsertTempBundling);
                                    
                                    if (execInsertTempBundling > 0)
                                    {
                                        new StokControllerJob().getQtyBundling(dbPathEra, "WebhookStokOp", "'" + ConnId + "'");
                                    }
                                    //end stok bundling

                                    new StokControllerJob().updateStockMarketPlace(ConnId, DatabasePathErasoft, "WebhookStokOp");
                                }
                            }

                            logErrorFunction(email, "09. API UpdateStokMP MO", noStok, "-", json);
                            //ErasoftDbContext.Database.ExecuteSqlCommand(@"BEGIN INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (1, '08. API UpdateStokMP MO', '" + noStok + "', '-', dateadd(hour, 7, getdate()), NULL, 1) END ");
                            //end add by nurul 19/11/2021
                        }
                        catch (Exception ez)
                        {
                            ErasoftDbContext.Database.ExecuteSqlCommand(@"BEGIN INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (1, 'prosesStokOpname_exec_listBrgUpdate MO', '" + noStok + "', '" + ez.Message + " | " + ez.Source + " | " + ez.StackTrace + ".', dateadd(hour, 7, getdate()), '', 0) END ");
                            return ez.Message;
                        }

                    }
                }

                //using (DbContextTransaction transaction = ErasoftDbContext.Database.BeginTransaction())
                //{
                    try
                    {
                        ErasoftDbContext.Database.ExecuteSqlCommand("UPDATE STT04A SET POSTING = '1' WHERE NOBUK = {0}", noStok);
                        //transaction.Commit();

                        logErrorFunction(email, "10. Update STT04A", noStok, "-", json);
                        //ErasoftDbContext.Database.ExecuteSqlCommand(@"BEGIN INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (1, '09. Update STT04A', '" + noStok + "', '-', dateadd(hour, 7, getdate()), NULL, 1) END ");
                    }
                    catch (Exception ex)
                    {
                        //transaction.Rollback();
                        ErasoftDbContext.Database.ExecuteSqlCommand(@"BEGIN INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (1, '3. Webhook Update Stok MO', '" + noStok + "', '" + ex.InnerException == null ? ex.Message : "Data tidak berhasil diproses, " + ex.InnerException.Message + ".', dateadd(hour, 7, getdate()), '', 0) END ");
                        return ex.InnerException == null ? ex.Message : "Data tidak berhasil diproses, " + ex.InnerException.Message;
                    }
                //}

                logErrorFunction(email, "11. Function berakhir", noStok, "-", json);
                //ErasoftDbContext.Database.ExecuteSqlCommand(@"BEGIN INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (1, '10. Function berakhir', '" + noStok + "', '-', dateadd(hour, 7, getdate()), NULL, 1) END ");
                return "OK";
            }
            catch (Exception e)
            {
                ErasoftDbContext.Database.ExecuteSqlCommand(@"BEGIN INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (1, '4. Webhook Update Stok MO', '" + noStok + "', '" + e.Message + " | " + e.Source + " | " + e.StackTrace + ".', dateadd(hour, 7, getdate()), '', 0) END ");
                return e.Message;
            }
        }
        public string retryInsertStt01b(string email, string modul, string nostok, string error, string exec, string json, int retry)
        {
            try
            {
                System.Threading.Thread.Sleep(5000);
                string exec_insert = exec.Replace("'", "\"");

                logErrorFunction(email, "07,5. Pre-insert STT01B_" + retry, nostok, exec_insert, json);
                ErasoftDbContext.Database.ExecuteSqlCommand(queryProcessDetail + exec);

                if (exec.Length > 65535)
                {
                    exec = exec.Substring(0, 65535);
                }

                logErrorFunction(email, "08. Insert STT01B_" + retry, nostok, exec_insert, json);
                return "RETRY";
            }
            catch (Exception e)
            {
                string error_exception = e.Message + " | " + e.Source + " | " + e.StackTrace;
                if (retry == 2)
                {
                    ErasoftDbContext.Database.ExecuteSqlCommand(@"BEGIN INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (1, 'prosesStokOpname_exec_" + retry + "', '" + nostok + "', '" + error_exception + ".', dateadd(hour, 7, getdate()), '', 0) END ");
                }
                else
                {
                    ErasoftDbContext.Database.ExecuteSqlCommand(@"BEGIN INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (1, 'prosesStokOpname_exec_" + retry + "', '" + nostok + "', '" + error_exception + ".', dateadd(hour, 7, getdate()), '', 1) END ");
                }
                if (retry < 2) //error_exception.Contains("Execution Timeout Expired") &&
                {
                    retryInsertStt01b(email, modul, nostok, error_exception, exec, json, retry + 1);
                }
                return error_exception;
            }

        }
        //end add by nurul 9/6/2022

        public class Order
        {
            public string NOBUK { get; set; }
            public string PHONE { get; set; }
            public string CUST_NAME { get; set; }
            public string BILLNUMBER { get; set; }
            public double AMOUNT { get; set; }
            public List<OrderItem> detailItem { get; set; }
        }

        public class OrderItem
        {
            public string NOBUK { get; set; }
            public string SKU { get; set; }
            public string ITEMNAME { get; set; }
            public double QUANTITY { get; set; }
            public double PRICE { get; set; }
        }

        public class TopupByPhone
        {
            public string phone { get; set; }
            public double amount { get; set; }
            public string programId { get; set; }
            public string billNumber { get; set; }
            public string walletId { get; set; }
            public string paymentMethod { get; set; }
            public List<Item> items { get; set; }
        }

        public class Item
        {
            public string sku { get; set; }
            public string itemName { get; set; }
            public double quantity { get; set; }
            public double price { get; set; }
        }

        public class TokenClass
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string refresh_token { get; set; }
            public DateTime expiredAt { get; set; }
            public int expires_in { get; set; }
            public string scope { get; set; }
        }

        public class PartnerApiData
        {
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
            public string Token { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }


        public class Response_TADA
        {
            public string phone { get; set; }
            public int amount { get; set; }
            public string billNumber { get; set; }
            public string programId { get; set; }
            public string paymentMethod { get; set; }
            public List<Item> items { get; set; }
            public string walletId { get; set; }
            public string walletName { get; set; }
            public int walletBalance { get; set; }
            public object balance { get; set; }
            public int reward { get; set; }
            public Card card { get; set; }
            public User user { get; set; }
            public List<Transaction> transactions { get; set; } //Transaction[]
        }

        public class Card
        {
            public int id { get; set; }
            public string no { get; set; }
            public int batchNum { get; set; }
            public string distributionId { get; set; }
            public string status { get; set; }
            public DateTime createdAt { get; set; }
        }

        public class User
        {
            public int id { get; set; }
            public string phone { get; set; }
            public object name { get; set; }
            public object email { get; set; }
        }

        //public class Item
        //{
        //    public string sku { get; set; }
        //    public string itemName { get; set; }
        //    public int quantity { get; set; }
        //    public int price { get; set; }
        //}

        public class Transaction
        {
            public int id { get; set; }
            public string trxNo { get; set; }
            public string trxType { get; set; }
            public string trxStatus { get; set; }
            public int trxAmount { get; set; }
            public string trxAmountType { get; set; }
            public string trxTime { get; set; }
            public int approvalCode { get; set; }
            public string cardNo { get; set; }
            public int balance { get; set; }
            public string balanceType { get; set; }
            public List<Item1> items { get; set; }
            public object amount { get; set; }
            public int point { get; set; }
            public string name { get; set; }
        }

        public class Item1
        {
            public string sku { get; set; }
            public string itemName { get; set; }
            public int quantity { get; set; }
            public int price { get; set; }
        }

        public class ErrorTada
        {
            public dynamic message { get; set; }
            public DataBN data { get; set; }
            public dynamic error { get; set; }
            public string title { get; set; }
        }


        public class ErrorCredential
        {
            public dynamic error { get; set; }
        }

        //public class Error
        //{
        //    public System system { get; set; }
        //    public User user { get; set; }
        //}

        //public class System
        //{
        //    public string message { get; set; }
        //}

        //public class User
        //{
        //    public string message { get; set; }
        //}s



        public class ErrorBillNumber
        {
            public DataBN data { get; set; }
            public string message { get; set; }
            public dynamic error { get; set; }
            public string title { get; set; }
        }

        public class DataBN
        {
            public string trxNo { get; set; }
            public string trxTime { get; set; }
            public int amount { get; set; }
            public int reward { get; set; }
        }


        public class ErrorToken
        {
            public string message { get; set; }
            public dynamic error { get; set; }
            public string title { get; set; }
        }
    }
}