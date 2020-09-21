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
        public async Task<string> FTP_listFakturJob(string DatabasePathErasoft, string uname)
        {
            //DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname, null
            string ret = "";
            SetupContext(DatabasePathErasoft, uname);
            var MoDbContext = new MoDbContext("");
            var EDB = new DatabaseSQL(DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            var ErasoftDbContext = new ErasoftContext(EraServerName, DatabasePathErasoft);

            try
            {
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("FAKTUR");

                    //string dt1 = DateTime.ParseExact(DateTime.Now.ToString("dd/MM/yyyy"), "dd'/'MM'/'yyyy", CultureInfo.InvariantCulture).ToString("yyyy'-'MM'-'dd 23:59:59.999");
                    string dt1 = DateTime.ParseExact(DateTime.Now.ToString("dd/MM/yyyy"), "dd'/'MM'/'yyyy", CultureInfo.InvariantCulture).ToString("yyyy'-'MM'-'dd");

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
                        "WHERE A.TGL = '" + dt1 + "'" +
                        "AND A.JENIS_FORM = '2' " +
                        "ORDER BY A.TGL DESC, A.NO_BUKTI DESC";

                    var lsFaktur = EDB.GetDataSet("CString", "SIT01A", sSQL);

                    var dataParamFTP = ErasoftDbContext.LINKFTP.ToList();

                    if (lsFaktur.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < lsFaktur.Tables[0].Rows.Count; i++)
                        {
                            string record = "TSOM";
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
                            worksheet.Cells[1 + i, 1].Value = record;
                        }

                        worksheet.Cells.AutoFitColumns(0);

                        byte[] byteExcel = package.GetAsByteArray();

                        #region initial folder
                        string filename = username.Replace(" ", "") + "_faktur_" + DateTime.Now.AddHours(7).ToString("yyyyMMddhhmmss") + ".csv";
                        var path = System.IO.Path.Combine(Server.MapPath("~/Content/Uploaded/UploadFTP/"), filename);
                        #endregion

                        if (!System.IO.File.Exists(path))
                        {
                            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Server.MapPath("~/Content/Uploaded/UploadFTP/"), ""));
                            //System.IO.File.Create(path);
                            using (FileStream stream = System.IO.File.Create(path))
                            {
                                stream.Write(byteExcel, 0, byteExcel.Length);
                                stream.Close();
                            }

                            //Task.Run(() => FTP_uploadFile(dbPathEra, namaFile, "QC", "Transfer FTP", "Upload File Faktur to FTP", "139.255.17.38", path, "masteronline", "Doremi135", null).Wait());
                            FTP_uploadFile(filename, dataParamFTP[0].IP, path, dataParamFTP[0].LOGIN, dataParamFTP[0].PASSWORD);
                        }

                        //if (System.IO.File.Exists(path))
                        //{
                        //    System.IO.File.Delete(path);
                        //}


                    }
                    else
                    {
                        //ret.Errors.Add("Tidak ada data faktur");
                    }

                }
            }
            catch (Exception ex)
            {
                ret = ex.Message.ToString();
            }

            return ret;
        }

        //add by fauzi 16 September 2020 untuk Upload File ke FTP Server
        public async Task<string> FTP_uploadFile(string nama_file, string ip_serverFTP, string locationFile, string usernameFTP, string passwordFTP)
        {
            //public async Task<string> FTP_uploadFile(string DatabasePathErasoft, string nama_file, string log_CUST, string log_ActionCategory, string log_ActionName, string ip_serverFTP, string locationFile, string usernameFTP, string passwordFTP, PerformContext context)
            //{
            string ret = "";

            FtpWebRequest request;
            //string absoluteFileName = Path.GetFileName("D:\\LEE SUSANTI_faktur exclude PPN.csv");

            request = WebRequest.Create(new Uri(string.Format(@"ftp://{0}/{1}", ip_serverFTP, nama_file))) as FtpWebRequest;
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.UseBinary = false;
            request.UsePassive = false;
            request.KeepAlive = false;
            request.Timeout = 600000;
            request.ReadWriteTimeout = 600000;
            request.Credentials = new NetworkCredential(usernameFTP, passwordFTP);
            request.ConnectionGroupName = "group";

            using (FileStream fs = System.IO.File.OpenRead(locationFile))
            {
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                fs.Close();
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(buffer, 0, buffer.Length);
                requestStream.Flush();
                requestStream.Close();
            }

            return ret;
        }
        //end by fauzi 16 September 2020        

        #endregion
        // END REGION UPLOAD FTP WITH HANGFIRE JOB by fauzi 20/09/2020


    }
}