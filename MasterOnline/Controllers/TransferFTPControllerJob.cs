using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using MasterOnline.Models;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using MasterOnline.ViewModels;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using Erasoft.Function;
using System.Xml;
using System.Web.Script.Serialization;
using System.Security.Cryptography;
using System.Net.Http;
using Hangfire;
using Hangfire.SqlServer;
using OfficeOpenXml;
using System.Globalization;

namespace MasterOnline.Controllers
{
    public class TransferFTPControllerJob : Controller
    {
        private MoDbContext MoDbContext { get; set; }
        private ErasoftContext ErasoftDbContext { get; set; }
        private DatabaseSQL EDB;
        private string username;
        public ActionResult Index()
        {
            return View();
        }

        public TransferFTPControllerJob()
        {

        }
        protected void SetupContext(string data, string user_name)
        {
            //string ret = "";
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(data);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, data);
            username = user_name;
            //return ret;
        }

        // REGION UPLOAD FAKTUR FTP WITH HANGFIRE JOB by fauzi 20/09/2020
        #region uploadfakturftp

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_manage_pesanan")]
        public async Task<string> FTP_listFakturJob(string DatabasePathErasoft, string typeFile, string cust, string logActionCategory, string logActionName, string uname)
        {
            string ret = "";
            SetupContext(DatabasePathErasoft, uname);
            byte[] byteExcel;

            try
            {
                var dataParamFTP = ErasoftDbContext.LINKFTP.ToList();
                //string filename = username.Replace(" ", "") + "_faktur_" + DateTime.Now.AddHours(7).ToString("yyyyMMddhhmmss") + ".csv";
                string filename = "aros" + DateTime.Now.AddHours(7).ToString("yyyyMMddhhmmss") + ".csv";
                //string dt1 = DateTime.ParseExact(DateTime.Now.ToString("dd/MM/yyyy"), "dd'/'MM'/'yyyy", CultureInfo.InvariantCulture).ToString("yyyy'-'MM'-'dd 23:59:59.999");
                string dt1 = DateTime.ParseExact(DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy"), "dd'/'MM'/'yyyy", CultureInfo.InvariantCulture).ToString("yyyy'-'MM'-'dd");

                string sSQL = "SELECT ISNULL(A.NO_BUKTI,'') AS NO_FAKTUR, A.TGL AS TGL_FAKTUR, A.STATUS AS STATUS_FAKTUR, " +
                        "ISNULL(D.NO_BUKTI,'') AS NO_PESANAN, ISNULL(A.NO_REF, '') AS NO_REFERENSI, ISNULL(D.TGL, '') AS TGL_PESANAN, " +
                        "C.NAMAMARKET + '(' + B.PERSO + ')' MARKETPLACE, ISNULL(B.ATTR5_AREA, '') AS KODE_SAP, ISNULL(H.BRG_SAP, '') AS BRG_SAP, " +
                        "ISNULL(A.PEMESAN, '') AS KODE_PEMBELI, ISNULL(A.NAMAPEMESAN, '') AS PEMBELI, " +
                        "ISNULL(I.KODEPOS, '') AS KODEPOS, ISNULL(I.KODEKABKOT, '') AS KODEKOTA, " +
                        "ISNULL(A.AL, '') AS ALAMAT_KIRIM, ISNULL(A.TERM, '') AS [TOP], ISNULL(A.NAMAPENGIRIM, '') AS KURIR, ISNULL(A.TGL_JT_TEMPO, '') AS TGL_JATUH_TEMPO, " +
                        "ISNULL(D.KET, '') AS KETERANGAN, ISNULL(A.BRUTO, '') AS BRUTO, ISNULL(A.DISCOUNT,'') AS DISC, ISNULL(A.PPN, '') AS PPN, ISNULL(A.NILAI_PPN, '') AS NILAI_PPN, " +
                        "ISNULL(D.ONGKOS_KIRIM, '') AS ONGKOS_KIRIM, ISNULL(A.NETTO, '') AS NETTO, ISNULL(D.STATUS_TRANSAKSI, '') AS STATUS_PESANAN, " +
                        "ISNULL(G.BRG, '') AS KODE_BRG, ISNULL(H.NAMA,'') + ' ' + ISNULL(H.NAMA2, '') AS NAMA_BARANG, ISNULL(QTY, '') AS QTY, " +
                        "ISNULL(H_SATUAN, '') AS HARGA_SATUAN, ISNULL(G.DISCOUNT, '') AS DISC1, ISNULL(G.NILAI_DISC_1, '') AS NDISC1, " +
                        "ISNULL(G.DISCOUNT_2, '') AS DISC2, ISNULL(G.NILAI_DISC_2, '') AS NDISC2, ISNULL(HARGA, '') AS TOTAL " +
                        "FROM SIT01A A LEFT JOIN ARF01 B ON A.CUST = B.CUST " +
                        "LEFT JOIN MO.dbo.MARKETPLACE C ON B.NAMA = C.IdMarket " +
                        "LEFT JOIN SOT01A D ON A.NO_SO = D.NO_BUKTI " +
                        "LEFT JOIN SIT01B G ON A.NO_BUKTI = G.NO_BUKTI " +
                        "LEFT JOIN STF02 H ON G.BRG = H.BRG " +
                        "LEFT JOIN (SELECT DISTINCT NO_BUKTI FROM SIT01A A INNER JOIN ART03B B ON A.NO_BUKTI = B.NFAKTUR)E ON A.NO_BUKTI = E.NO_BUKTI " +
                        "LEFT JOIN (select ret.jenis_form,ret.no_bukti as bukti_ret,ret.no_ref as no_si,fkt.no_bukti as bukti_faktur from sit01a ret inner join sit01a fkt on fkt.no_bukti=ret.no_ref where ret.jenis_form='3') F ON A.NO_BUKTI=F.BUKTI_FAKTUR " +
                        "LEFT JOIN ARF01C I ON A.PEMESAN = I.BUYER_CODE " +
                        //"WHERE A.TGL = '" + dt1 + "'" +
                        "WHERE A.TGL_KIRIM = '" + dt1 + "'" +
                        "AND D.STATUS_TRANSAKSI = '04' " +
                        "AND A.JENIS_FORM = '2' " +
                        "ORDER BY A.TGL DESC, A.NO_BUKTI DESC";

                var lsFaktur = EDB.GetDataSet("CString", "SIT01A", sSQL);

                string record = "";
                string allRecord = "";
                if (lsFaktur.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < lsFaktur.Tables[0].Rows.Count; i++)
                    {
                        record = dataParamFTP[0].KODE_TRANSAKSI ?? "";
                        record += ";";
                        record += ";";
                        record += ";";
                        record += ";";
                        record += lsFaktur.Tables[0].Rows[i]["KODE_SAP"].ToString();
                        record += ";";
                        record += lsFaktur.Tables[0].Rows[i]["KODE_SAP"].ToString();
                        record += ";";
                        record += Convert.ToDateTime(lsFaktur.Tables[0].Rows[i]["TGL_FAKTUR"]).ToString("dd/MM/yyyy");
                        record += ";";
                        record += ";";
                        record += lsFaktur.Tables[0].Rows[i]["BRG_SAP"].ToString();
                        //record += lsFaktur.Tables[0].Rows[i]["KODE_BRG"].ToString() + "SAP";
                        record += ";";
                        record += lsFaktur.Tables[0].Rows[i]["QTY"].ToString();
                        record += ";";
                        record += ";";
                        record += ";";
                        record += ";";
                        record += ";";
                        record += ";";
                        record += ";";
                        record += ";";
                        record += lsFaktur.Tables[0].Rows[i]["NO_PESANAN"].ToString();
                        record += ";";
                        record += lsFaktur.Tables[0].Rows[i]["NO_PESANAN"].ToString();
                        record += ";";
                        record += Convert.ToDateTime(lsFaktur.Tables[0].Rows[i]["TGL_FAKTUR"]).ToString("dd/MM/yyyy");
                        record += ";";
                        record += ";";
                        record += "1";
                        record += ";";
                        record += ";";
                        if (dataParamFTP[0].PPN == "0")
                        {
                            record += Decimal.Round(Convert.ToDecimal(lsFaktur.Tables[0].Rows[i]["HARGA_SATUAN"])).ToString();
                        }
                        else
                        {
                            decimal hrgPpn = Convert.ToDecimal(lsFaktur.Tables[0].Rows[i]["HARGA_SATUAN"]) / Convert.ToDecimal(1.1);
                            record += Decimal.Round(hrgPpn);
                        }
                        record += ";";
                        record += "0";
                        record += ";";
                        record += lsFaktur.Tables[0].Rows[i]["NO_REFERENSI"].ToString();
                        record += ";";
                        record += lsFaktur.Tables[0].Rows[i]["PEMBELI"].ToString();
                        record += ";";
                        record += lsFaktur.Tables[0].Rows[i]["ALAMAT_KIRIM"].ToString();
                        record += ";";
                        record += ";";
                        record += lsFaktur.Tables[0].Rows[i]["KODEPOS"].ToString();
                        record += ";";
                        string KodeKota = lsFaktur.Tables[0].Rows[i]["KODEKOTA"].ToString();
                        string Kota = MoDbContext.KabupatenKota.Where(x => x.KodeKabKot == KodeKota).Select(x => x.NamaKabKot).FirstOrDefault();
                        record += Kota;
                        allRecord += record.Replace(",", "").Replace("\n", "").Replace("\r", "") + "\n";
                    }

                    #region initial folder
                    var path = System.IO.Path.Combine(Server.MapPath("~/Content/Uploaded/UploadFTP/"), filename);
                    //var path = "D:\\document_kerja_fauzi\\project\\@MO_source\\MasterOnlineDevelopment\\MasterOnline\\Content\\Uploaded\\UploadFTP\\" + filename;

                    if (!System.IO.File.Exists(path))
                    {
                        //System.IO.Directory.CreateDirectory("D:\\document_kerja_fauzi\\project\\@MO_source\\MasterOnlineDevelopment\\MasterOnline\\Content\\Uploaded\\UploadFTP\\");
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/Content/Uploaded/UploadFTP/"));

                        //string output = "D:\\" + "testdoang.csv";
                        StreamWriter csv = new StreamWriter(path, false);
                        csv.Write(allRecord);
                        csv.Close();

                        byteExcel = System.IO.File.ReadAllBytes(path);

                        if (byteExcel != null)
                        {
                            FtpWebRequest request;
                            request = WebRequest.Create(new Uri(string.Format(@"ftp://{0}/{1}", dataParamFTP[0].IP.ToString(), filename.ToString()))) as FtpWebRequest;
                            request.Method = WebRequestMethods.Ftp.UploadFile;
                            request.UseBinary = false;
                            request.UsePassive = false;
                            request.KeepAlive = true;
                            request.Timeout = 600000;
                            request.ReadWriteTimeout = 600000;
                            request.Credentials = new NetworkCredential(dataParamFTP[0].LOGIN.ToString(), dataParamFTP[0].PASSWORD.ToString());
                            request.ConnectionGroupName = "group";

                            using (var resp = (FtpWebResponse)request.GetResponse())
                            {
                                var respon = resp.StatusCode;
                                if (resp.WelcomeMessage.Contains("230 Login successful"))
                                {
                                    using (Stream requestStream = await request.GetRequestStreamAsync())
                                    {
                                        requestStream.Write(byteExcel, 0, byteExcel.Length);
                                        requestStream.Flush();
                                        requestStream.Close();
                                    }
                                }
                            }
                        }
                    }
                    #endregion              
                }
                else
                {
                    throw new Exception("Tidak ada faktur hari ini.");
                }

            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                throw new Exception(msg);
            }

            return ret;
        }

        ////add by fauzi 16 September 2020 untuk Upload File ke FTP Server
        //public async Task<string> FTP_uploadFile(string nama_file, string ip_serverFTP, string usernameFTP, string passwordFTP, byte[] byteData)
        //{
        //    string ret = "";

        //    FtpWebRequest request;
        //    request = WebRequest.Create(new Uri(string.Format(@"ftp://{0}/{1}", ip_serverFTP, nama_file))) as FtpWebRequest;
        //    request.Method = WebRequestMethods.Ftp.UploadFile;
        //    request.UseBinary = false;
        //    request.UsePassive = false;
        //    request.KeepAlive = false;
        //    request.Timeout = 600000;
        //    request.ReadWriteTimeout = 600000;
        //    request.Credentials = new NetworkCredential(usernameFTP, passwordFTP);
        //    request.ConnectionGroupName = "group";

        //    //using (FileStream fs = System.IO.File.OpenRead(locationFile))
        //    //{
        //    //    byte[] buffer = new byte[fs.Length];
        //    //    fs.Read(buffer, 0, buffer.Length);
        //    //    fs.Close();
        //    Stream requestStream = request.GetRequestStream();
        //    requestStream.Write(byteData, 0, byteData.Length);
        //    requestStream.Flush();
        //    requestStream.Close();
        //    //}

        //    return ret;
        //}
        ////end by fauzi 16 September 2020        

        #endregion
        // END REGION UPLOAD FTP WITH HANGFIRE JOB by fauzi 20/09/2020
    }
}