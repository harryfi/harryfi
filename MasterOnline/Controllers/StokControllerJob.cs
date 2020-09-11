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
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.IO;
using Erasoft.Function;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Server;
using Hangfire.Common;
using Hangfire.Client;
using Hangfire.States;
using Lazop.Api;
using Lazop.Api.Util;
using System.Security.Cryptography;
using System.Xml;

namespace MasterOnline.Controllers
{
    public class NotifyOnFailed : JobFilterAttribute,
    IElectStateFilter, IApplyStateFilter
    //IClientFilter, IServerFilter, IElectStateFilter, IApplyStateFilter
    {
        private string _deskripsi;

        public NotifyOnFailed()
        {

        }

        public NotifyOnFailed(string deskripsi)
        {
            _deskripsi = deskripsi;
        }

        //private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        //public void OnCreating(CreatingContext context)
        //{
        //    Logger.InfoFormat("Creating a job based on method `{0}`...", context.Job.Method.Name);
        //}

        //public void OnCreated(CreatedContext context)
        //{
        //    Logger.InfoFormat(
        //        "Job that is based on method `{0}` has been created with id `{1}`",
        //        context.Job.Method.Name,
        //        context.BackgroundJob?.Id);
        //}

        //public void OnPerforming(PerformingContext context)
        //{
        //    Logger.InfoFormat("Starting to perform job `{0}`", context.BackgroundJob.Id);
        //}

        //public void OnPerformed(PerformedContext context)
        //{
        //    Logger.InfoFormat("Job `{0}` has been performed", context.BackgroundJob.Id);
        //}

        public void OnStateElection(ElectStateContext context)
        {
            //move by calvin 15 mei 2019 from OnStateElection to OnStateApplied
            //var failedState = context.CandidateState as FailedState;
            //if (failedState != null)
            //{
            //    string dbPathEra = Convert.ToString(context.BackgroundJob.Job.Args[0]);// mengambil dbPathEra 
            //    string subjectDescription = Convert.ToString(context.BackgroundJob.Job.Args[1]); //mengambil Subject

            //    var jobId = context.BackgroundJob.Id;
            //    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
            //    contextNotif.Clients.Group(dbPathEra).moFailedJobs(this._deskripsi, subjectDescription, jobId);
            //    try
            //    {
            //        //add by calvin 14 mei 2019
            //        string CUST = Convert.ToString(context.BackgroundJob.Job.Args[2]); //mengambil Cust
            //        string ActionCategory = Convert.ToString(context.BackgroundJob.Job.Args[3]); //mengambil Kategori
            //        string ActionName = Convert.ToString(context.BackgroundJob.Job.Args[4]); //mengambil Action
            //        string exceptionMessage = failedState.Exception.InnerException == null ? failedState.Exception.Message : failedState.Exception.InnerException.Message;
            //        var EDB = new DatabaseSQL(dbPathEra);
            //        string sSQL = "INSERT INTO API_LOG_MARKETPLACE (CUST,MARKETPLACE,REQUEST_ID,";
            //        sSQL += "REQUEST_ACTION,REQUEST_DATETIME,";
            //        sSQL += "REQUEST_ATTRIBUTE_3, REQUEST_ATTRIBUTE_4,REQUEST_ATTRIBUTE_5,";
            //        sSQL += "REQUEST_RESULT,REQUEST_EXCEPTION) ";
            //        sSQL += "VALUES ('" + CUST + "',(SELECT TOP 1 B.NAMAMARKET FROM ARF01 A INNER JOIN MO.DBO.MARKETPLACE B ON A.NAMA = B.IDMARKET AND A.CUST='" + CUST + "'), '" + jobId + "', ";
            //        sSQL += "'" + ActionName + "', '" + context.BackgroundJob.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss") + "', ";
            //        sSQL += "'" + ActionCategory + "','" + subjectDescription + "', 'HANGFIRE', ";
            //        sSQL += "'"+ this._deskripsi.Replace("{obj}", subjectDescription) +"', '"+ exceptionMessage.Replace("'","`") + "')";
            //        EDB.ExecuteSQL("sConn", CommandType.Text, sSQL);
            //        //end add by calvin 14 mei 2019
            //    }
            //    catch (Exception ex)
            //    {

            //    }
            //}
            //end move by calvin 15 mei 2019 from OnStateElection to OnStateApplied
        }

        public void OnStateApplied(ApplyStateContext context, Hangfire.Storage.IWriteOnlyTransaction transaction)
        {
            try
            {
                var failedState = context.NewState as FailedState;
                if (failedState != null)
                {
                    string dbPathEra = Convert.ToString(context.BackgroundJob.Job.Args[0]);// mengambil dbPathEra 
                    string subjectDescription = Convert.ToString(context.BackgroundJob.Job.Args[1]); //mengambil Subject
                    subjectDescription = subjectDescription.Replace("'", "`");
                    var jobId = context.BackgroundJob.Id;
                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                    contextNotif.Clients.Group(dbPathEra).moFailedJobs(this._deskripsi, subjectDescription, jobId);
                    try
                    {
                        //add by calvin 14 mei 2019
                        string CUST = Convert.ToString(context.BackgroundJob.Job.Args[2]); //mengambil Cust
                        string ActionCategory = Convert.ToString(context.BackgroundJob.Job.Args[3]); //mengambil Kategori
                        string ActionName = Convert.ToString(context.BackgroundJob.Job.Args[4]); //mengambil Action
                        string exceptionMessage = failedState.Exception.InnerException == null ? failedState.Exception.Message : failedState.Exception.InnerException.Message;
                        var EDB = new DatabaseSQL(dbPathEra);
                        string sSQL = "INSERT INTO API_LOG_MARKETPLACE (REQUEST_STATUS,CUST_ATTRIBUTE_1,CUST_ATTRIBUTE_2,CUST,MARKETPLACE,REQUEST_ID,";
                        sSQL += "REQUEST_ACTION,REQUEST_DATETIME,";
                        sSQL += "REQUEST_ATTRIBUTE_3, REQUEST_ATTRIBUTE_4,REQUEST_ATTRIBUTE_5,";
                        sSQL += "REQUEST_RESULT,REQUEST_EXCEPTION) ";
                        sSQL += "SELECT 'FAILED',A.CUST_ATTRIBUTE_1, '1', A.CUST,A.MARKETPLACE,A.REQUEST_ID,A.REQUEST_ACTION,A.REQUEST_DATETIME,A.REQUEST_ATTRIBUTE_3,A.REQUEST_ATTRIBUTE_4,A.REQUEST_ATTRIBUTE_5,A.REQUEST_RESULT,A.REQUEST_EXCEPTION ";
                        sSQL += "FROM ( SELECT '" + subjectDescription + "' CUST_ATTRIBUTE_1,'" + CUST + "' CUST,(SELECT TOP 1 B.NAMAMARKET FROM ARF01 A INNER JOIN MO.DBO.MARKETPLACE B ON A.NAMA = B.IDMARKET AND A.CUST='" + CUST + "') MARKETPLACE, '" + jobId + "' REQUEST_ID, ";
                        sSQL += "'" + ActionName + "' REQUEST_ACTION, '" + context.BackgroundJob.CreatedAt.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "' REQUEST_DATETIME, ";
                        sSQL += "'" + ActionCategory + "' REQUEST_ATTRIBUTE_3,'" + subjectDescription + "' REQUEST_ATTRIBUTE_4, 'HANGFIRE' REQUEST_ATTRIBUTE_5, ";
                        sSQL += "'" + this._deskripsi.Replace("{obj}", subjectDescription) + "' REQUEST_RESULT, '" + exceptionMessage.Replace("'", "`") + "' REQUEST_EXCEPTION ) A ";
                        sSQL += "LEFT JOIN API_LOG_MARKETPLACE B ON B.REQUEST_ATTRIBUTE_5 = 'HANGFIRE' AND A.REQUEST_ACTION = B.REQUEST_ACTION AND A.CUST = B.CUST AND A.CUST_ATTRIBUTE_1 = B.CUST_ATTRIBUTE_1 WHERE ISNULL(B.RECNUM,0) = 0 ";
                        int adaInsert = EDB.ExecuteSQL("sConn", CommandType.Text, sSQL);
                        if (adaInsert == 0) //JIKA 
                        {
                            //update REQUEST_STATUS = 'FAILED', DATE, FAIL COUNT
                            sSQL = "UPDATE B SET REQUEST_STATUS = 'FAILED', REQUEST_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', CUST_ATTRIBUTE_2 = CONVERT(INT,CUST_ATTRIBUTE_2) + 1 ";
                            //add by nurul 12/2/2020, update exception terbaru
                            sSQL += ", REQUEST_RESULT = '" + this._deskripsi.Replace("{obj}", subjectDescription) + "', REQUEST_EXCEPTION = '" + exceptionMessage.Replace("'", "`") + "' ";
                            //end add by nurul 12/2/2020, update exception terbaru
                            sSQL += "FROM API_LOG_MARKETPLACE B WHERE B.REQUEST_ATTRIBUTE_5 = 'HANGFIRE' AND B.REQUEST_STATUS = 'RETRYING' AND B.REQUEST_ID = '" + jobId + "'";
                            EDB.ExecuteSQL("sConn", CommandType.Text, sSQL);

                            //update JOBID MENJADI JOBID BARU JIKA TIDAK SEDANG RETRY,STATUS,DATE,FAIL COUNT
                            sSQL = "UPDATE B SET REQUEST_STATUS = 'FAILED', REQUEST_ID = '" + jobId + "', REQUEST_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', CUST_ATTRIBUTE_2 = CONVERT(INT,CUST_ATTRIBUTE_2) + 1 ";
                            //add by nurul 12/2/2020, update exception terbaru
                            sSQL += ", REQUEST_RESULT = '" + this._deskripsi.Replace("{obj}", subjectDescription) + "', REQUEST_EXCEPTION = '" + exceptionMessage.Replace("'", "`") + "' ";
                            //end add by nurul 12/2/2020, update exception terbaru
                            sSQL += "FROM API_LOG_MARKETPLACE B INNER JOIN ";
                            sSQL += "( SELECT '" + subjectDescription + "' CUST_ATTRIBUTE_1,'" + CUST + "' CUST,(SELECT TOP 1 B.NAMAMARKET FROM ARF01 A INNER JOIN MO.DBO.MARKETPLACE B ON A.NAMA = B.IDMARKET AND A.CUST='" + CUST + "') MARKETPLACE, '" + jobId + "' REQUEST_ID, ";
                            sSQL += "'" + ActionName + "' REQUEST_ACTION, '" + context.BackgroundJob.CreatedAt.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "' REQUEST_DATETIME, ";
                            sSQL += "'" + ActionCategory + "' REQUEST_ATTRIBUTE_3,'" + subjectDescription + "' REQUEST_ATTRIBUTE_4, 'HANGFIRE' REQUEST_ATTRIBUTE_5, ";
                            sSQL += "'" + this._deskripsi.Replace("{obj}", subjectDescription) + "' REQUEST_RESULT, '" + exceptionMessage.Replace("'", "`") + "' REQUEST_EXCEPTION ) A ";
                            sSQL += "ON B.REQUEST_ATTRIBUTE_5 = 'HANGFIRE' AND A.REQUEST_ACTION = B.REQUEST_ACTION AND A.CUST = B.CUST AND A.CUST_ATTRIBUTE_1 = B.CUST_ATTRIBUTE_1 AND B.REQUEST_STATUS IN ('FAILED','RETRYING')";
                            EDB.ExecuteSQL("sConn", CommandType.Text, sSQL);
                        }
                        //end add by calvin 14 mei 2019
                        //add by calvin 28 agustus 2019
                        if (ActionName == "Buat Produk")
                        {
                            sSQL = "UPDATE S SET LINK_STATUS='Buat Produk Gagal', LINK_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', ";
                            //jobid;request_action;request_result;request_exception
                            string Link_Error = jobId + ";" + ActionName + ";" + this._deskripsi.Replace("{obj}", subjectDescription) + ";" + exceptionMessage.Replace("'", "`");
                            sSQL += "LINK_ERROR = '" + Link_Error + "' FROM STF02H S INNER JOIN ARF01 A ON S.IDMARKET = A.RECNUM AND A.CUST = '" + CUST + "' WHERE S.BRG = '" + subjectDescription + "' ";
                            EDB.ExecuteSQL("sConn", CommandType.Text, sSQL);
                        }
                        //end add by calvin 28 agustus 2019
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void OnStateUnapplied(ApplyStateContext context, Hangfire.Storage.IWriteOnlyTransaction transaction)
        {
            //Logger.InfoFormat(
            //    "Job `{0}` state `{1}` was unapplied.",
            //    context.BackgroundJob.Id,
            //    context.OldStateName);
        }
    }

    public class StokControllerJob : Controller
    {
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);//string auth = Base64Encode();
        //private MoDbContext MoDbContext { get; set; }
        //private ErasoftContext ErasoftDbContext { get; set; }
        //private DatabaseSQL EDB;
        private string username;
        private string dbPathEra;

        //add by fauzi for JD.ID 21 Juli 2020
        public string ServerUrl_JDID = "https://open.jd.id/api";
        public string AccessToken_JDID = "";
        public string AppKey_JDID = "";
        public string AppSecret_JDID = "";
        public string Version_JDID = "1.0";
        public string Format_JDID = "json";
        public string SignMethod_JDID = "md5";
        private string Charset_utf8_JDID = "UTF-8";
        public string Method_JDID;
        public string ParamJson_JDID;
        public string ParamFile_JDID;
        //end add by fauzi for JD.ID


        public StokControllerJob()
        {
            //Catatan by calvin :
            //untuk menghandle update stok semua marketplace
        }
        public StokControllerJob(string DatabasePathErasoft, string uname)
        {
            //Catatan by calvin :
            //untuk menghandle update stok semua marketplace
            SetupContext(DatabasePathErasoft, uname);
            dbPathEra = DatabasePathErasoft;
        }
        public string[] SplitItemName(string name)
        {
            var result = new string[2];
            var length_nama = name.Length;
            if (length_nama > 285)
            {
                length_nama = 285;
            }
            if (length_nama > 30)
            {
                int lastIndexOfSpaceIn30 = name.Substring(0, 30).LastIndexOf(' ');
                if (lastIndexOfSpaceIn30 < 0 || lastIndexOfSpaceIn30 == 29)
                {
                    result[0] = name.Substring(0, 30).Trim();
                    result[1] = name.Substring(30, length_nama - 30).Trim();
                }
                else
                {
                    result[0] = name.Substring(0, lastIndexOfSpaceIn30).Trim();
                    result[1] = name.Substring(lastIndexOfSpaceIn30, length_nama - lastIndexOfSpaceIn30).Trim();
                }
            }
            else
            {
                result[0] = name;
                result[1] = "";
            }

            return result;
        }
        public string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9')
                    || (c >= 'A' && c <= 'Z')
                    || (c >= 'a' && c <= 'z')
                    || c == '`' || c == '!' || c == '@' || c == '#' || c == '$' || c == '%' || c == '^' || c == '&'
                    || c == '(' || c == ')' || c == '-' || c == '=' || c == '_' || c == ',' || c == '.'
                    || c == '?' || c == ';' || c == ':' || c == '\'' || c == '"' || c == '_' || c == '\\' || c == '|'
                    || c == '[' || c == ']' || c == '{' || c == '}' || c == '<' || c == '>'
                    || c == '/' || c == '*' || c == '-' || c == '+' || c == (char)13 || c == ' '
                    )
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        protected void SetupContext(string DatabasePathErasoft, string uname)
        {
            dbPathEra = DatabasePathErasoft;
            username = uname;
        }

        #region jdid tools
        private string getCurrentTimeFormatted()
        {
            var dt = System.DateTime.Now.ToLocalTime();
            return dt.ToString("yyyy-MM-dd HH:mm:ss.fff") + dt.ToString("zzzz").Replace(":", "");
        }

        public string Call(string sappKey, string saccessToken, string sappSecret, string sMethod, string sParamJson)
        {
            //construct system parameters
            var sysParams = new Dictionary<string, string>();
            //sysParams.Add("app_key", this.AppKey);
            sysParams.Add("app_key", sappKey);
            sysParams.Add("v", this.Version_JDID);
            sysParams.Add("format", this.Format_JDID);
            sysParams.Add("sign_method", this.SignMethod_JDID);
            //sysParams.Add("method", this.Method);
            sysParams.Add("method", sMethod);
            sysParams.Add("timestamp", this.getCurrentTimeFormatted());
            //sysParams.Add("access_token", this.AccessToken);
            sysParams.Add("access_token", saccessToken);

            //get business parameters
            if (sParamJson != null && sParamJson.Length > 0)
            {
                sysParams.Add("param_json", sParamJson);
            }
            else
            {
                sysParams.Add("param_json", "{}");
            }
            //sign
            sysParams.Add("sign", this.generateSign(sysParams, sappSecret));

            //send http post request
            var content = this.curl(this.ServerUrl_JDID, null, sysParams);
            return content;
        }

        public string curl(string url, string[] files, Dictionary<string, string> formFields = null)
        {
            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "multipart/form-data; boundary=" +
                                    boundary;
            request.Method = "POST";
            request.KeepAlive = true;

            Stream memStream = new System.IO.MemoryStream();

            var boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" +
                                                                    boundary + "\r\n");
            var endBoundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" +
                                                                        boundary + "--");


            string formdataTemplate = "\r\n--" + boundary +
                                        "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";
            try
            {
                if (formFields != null)
                {
                    foreach (string key in formFields.Keys)
                    {
                        string formitem = string.Format(formdataTemplate, key, formFields[key]);
                        byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                        memStream.Write(formitembytes, 0, formitembytes.Length);
                    }
                }


                //file
                if (files != null)
                {
                    string headerTemplate =
                        "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n" +
                        "Content-Type: application/octet-stream\r\n\r\n";
                    for (int i = 0; i < files.Length; i++)
                    {
                        memStream.Write(boundarybytes, 0, boundarybytes.Length);
                        var header = string.Format(headerTemplate, "param_file", files[i]);
                        var headerbytes = System.Text.Encoding.UTF8.GetBytes(header);

                        memStream.Write(headerbytes, 0, headerbytes.Length);

                        using (var fileStream = new FileStream(files[i], FileMode.Open, FileAccess.Read))
                        {
                            var buffer = new byte[1024];
                            var bytesRead = 0;
                            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                memStream.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }
                //~:end file


                memStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
                request.ContentLength = memStream.Length;

                using (Stream requestStream = request.GetRequestStream())
                {
                    memStream.Position = 0;
                    byte[] tempBuffer = new byte[memStream.Length];
                    memStream.Read(tempBuffer, 0, tempBuffer.Length);
                    memStream.Close();
                    requestStream.Write(tempBuffer, 0, tempBuffer.Length);
                }

                using (var response = request.GetResponse())
                {
                    Stream stream2 = response.GetResponseStream();
                    StreamReader reader2 = new StreamReader(stream2);
                    return reader2.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                return ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }

        }

        private string generateSign(Dictionary<string, string> sysDataDictionary, string sappSecret)
        {
            var dic = sysDataDictionary.OrderBy(key => key.Key).ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
            var sb = new System.Text.StringBuilder();
            foreach (var item in dic)
            {
                if (!"".Equals(item.Key) && !"".Equals(item.Value))
                {
                    sb.Append(item.Key).Append(item.Value);
                }

            }
            //prepend and append appsecret   
            //sb.Insert(0, this.AppSecret);
            //sb.Append(this.AppSecret);
            sb.Insert(0, sappSecret);
            sb.Append(sappSecret);
            var signValue = this.calculateMD5Hash(sb.ToString());
            //Console.WriteLine("raw string=" + sb.ToString());
            //Console.WriteLine("signValue=" + signValue);
            return signValue;
        }


        private string calculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();

        }
        #endregion


        public int PesananBatal(string ordersn)
        {

            var EDB = new DatabaseSQL(dbPathEra);
            string EraServerName = EDB.GetServerName("sConn");
            var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);

            var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '11' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI <> '11'");
            EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS = '2' WHERE NO_REF IN (" + ordersn + ") AND STATUS <> '2' AND ST_POSTING = 'T' AND JENIS_FORM='2'");

            var dsFakturRetur = new DataSet();
            dsFakturRetur = EDB.GetDataSet("MOConnectionString", "CREATE_RETUR", "SELECT SI.* FROM SIT01A SI LEFT JOIN SIT01A RT ON SI.NO_BUKTI = RT.NO_REF AND SI.JENIS_FORM = '2' AND RT.JENIS_FORM = '3' WHERE SI.NO_REF IN (" + ordersn + ") AND SI.STATUS <> '2' AND SI.ST_POSTING = 'Y' AND SI.JENIS_FORM='2' AND ISNULL(RT.NO_BUKTI,'') = ''");
            if (dsFakturRetur.Tables[0].Rows.Count > 0)
            {
                var digitAkhir = "";
                var noOrder = "";
                var lastRecNum = 0;
                var lastDigitSIT01A = ErasoftDbContext.SIT01A.OrderByDescending(p => p.RecNum).FirstOrDefault();

                if (lastDigitSIT01A == null)
                {
                    digitAkhir = "000001";
                    noOrder = $"RJ{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                    ErasoftDbContext.Database.ExecuteSqlCommand("DBCC CHECKIDENT (SIT01A, RESEED, 0)");
                }
                else
                {
                    lastRecNum = lastDigitSIT01A.RecNum.Value;
                    lastRecNum++;

                    digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                    noOrder = $"RJ{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                }

                for (int i = 0; i < dsFakturRetur.Tables[0].Rows.Count; i++)
                {
                    var created = 0;
                    var newRetur = new SIT01A()
                    {
                        NO_BUKTI = noOrder,
                        NO_REF = Convert.ToString(dsFakturRetur.Tables[0].Rows[i]["NO_BUKTI"]),
                        JENIS_FORM = "3",
                        TGL = DateTime.Now,
                        NO_F_PAJAK = "",
                        APPROVAL = Convert.ToBoolean(dsFakturRetur.Tables[0].Rows[i]["APPROVAL"]),
                        BATAL = Convert.ToBoolean(dsFakturRetur.Tables[0].Rows[i]["BATAL"]),
                        CUST_QQ = Convert.ToString(dsFakturRetur.Tables[0].Rows[i]["CUST_QQ"]),
                        JAMKIRIM = Convert.ToDateTime(dsFakturRetur.Tables[0].Rows[i]["JAMKIRIM"]),
                        BRUTO = 0,
                        DISCOUNT = Convert.ToDouble(dsFakturRetur.Tables[0].Rows[i]["DISCOUNT"]),
                        NILAI_DISC = Convert.ToDouble(dsFakturRetur.Tables[0].Rows[i]["NILAI_DISC"]),
                        NETTO = 0,
                        PPN = Convert.ToDouble(dsFakturRetur.Tables[0].Rows[i]["PPN"]),
                        NILAI_PPN = Convert.ToDouble(dsFakturRetur.Tables[0].Rows[i]["NILAI_PPN"]),
                        KIRIM_PENUH = false,
                        MATERAI = Convert.ToDouble(dsFakturRetur.Tables[0].Rows[i]["MATERAI"]),
                        RETUR_PENUH = false,
                        TERM = Convert.ToDouble(dsFakturRetur.Tables[0].Rows[i]["TERM"]),
                        PPN_ditangguhkan = Convert.ToBoolean(dsFakturRetur.Tables[0].Rows[i]["PPN_ditangguhkan"]),
                        TGL_JT_TEMPO = Convert.ToDateTime(dsFakturRetur.Tables[0].Rows[i]["TGL_JT_TEMPO"]),
                        SJ_ADA_FAKTUR = false,
                        NILAI_ANGKUTAN = Convert.ToDouble(dsFakturRetur.Tables[0].Rows[i]["NILAI_ANGKUTAN"]),
                        JENIS_RETUR = "2",
                        STATUS = "1",
                        ST_POSTING = "T",
                        VLT = "IDR",
                        NO_FA_OUTLET = "-",
                        NO_LPB = "-",
                        GROUP_LIMIT = "-",
                        KODE_ANGKUTAN = "-",
                        JENIS_MOBIL = "-",
                        NAMA_CUST = "-",
                        TUKAR = 1,
                        TUKAR_PPN = 1,
                        SOPIR = "-",
                        KET = "-",
                        PPNBM = 0,
                        KODE_SALES = "-",
                        KODE_WIL = "-",
                        U_MUKA = 0,
                        U_MUKA_FA = 0,
                        JTRAN = "SI",
                        JENIS = "1",
                        TGLINPUT = DateTime.Now,
                        NILAI_PPNBM = 0,
                        PEMESAN = Convert.ToString(dsFakturRetur.Tables[0].Rows[i]["PEMESAN"]),
                        NAMAPEMESAN = Convert.ToString(dsFakturRetur.Tables[0].Rows[i]["NAMAPEMESAN"]),
                        CUST = Convert.ToString(dsFakturRetur.Tables[0].Rows[i]["CUST"])
                    };

                    var CustInDb = ErasoftDbContext.ARF01.SingleOrDefault(p => p.CUST == Convert.ToString(dsFakturRetur.Tables[0].Rows[i]["CUST"]));
                    if (CustInDb != null)
                    {
                        newRetur.NAMA_CUST = CustInDb.NAMA;
                        newRetur.AL = CustInDb.AL;
                        newRetur.AL2 = CustInDb.AL2;
                        newRetur.AL3 = CustInDb.AL3;
                    }

                    newRetur.PPN_Bln_Lapor = Convert.ToByte(Convert.ToDateTime(dsFakturRetur.Tables[0].Rows[i]["TGL"]).ToString("MM"));
                    newRetur.PPN_Thn_Lapor = Convert.ToByte(Convert.ToDateTime(dsFakturRetur.Tables[0].Rows[i]["TGL"]).ToString("yyyy").Substring(2, 2));
                    ErasoftDbContext.SIT01A.Add(newRetur);
                    created = ErasoftDbContext.SaveChanges();

                    if (created > 0)
                    {
                        object[] spParams = {
                            new SqlParameter("@NOBUK",newRetur.NO_BUKTI),
                            new SqlParameter("@NO_REF",newRetur.NO_REF)
                        };
                        ErasoftDbContext.Database.ExecuteSqlCommand("exec [SP_AUTOLOADRETUR_PENJUALAN] @NOBUK, @NO_REF", spParams);
                    }

                    lastRecNum++;
                    digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                    noOrder = $"RJ{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                }
            }

            return rowAffected;
        }

        public class BliBliToken
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string refresh_token { get; set; }
            public int expires_in { get; set; }
            public string scope { get; set; }
            public string error { get; set; }
            public string error_description { get; set; }
        }
        protected string SetupContextBlibli(string DatabasePathErasoft, string uname, BlibliAPIData data)
        {
            string ret = "";
            dbPathEra = DatabasePathErasoft;
            var EDB = new DatabaseSQL(dbPathEra);
            string EraServerName = EDB.GetServerName("sConn");
            var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);
            username = uname;

            var arf01inDB = ErasoftDbContext.ARF01.Where(p => p.RecNum == data.idmarket).SingleOrDefault();
            if (arf01inDB != null)
            {
                ret = arf01inDB.TOKEN;


                bool TokenExpired = true;
                var currentTimeRequest = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (!string.IsNullOrWhiteSpace(arf01inDB.REFRESH_TOKEN))
                {
                    var splitRefreshToken = arf01inDB.REFRESH_TOKEN.Split(';');
                    if (splitRefreshToken.Count() == 3)
                    {
                        if ((Convert.ToInt64(splitRefreshToken[2]) + Convert.ToInt64(splitRefreshToken[1]) - 10000) >= currentTimeRequest)
                        {
                            TokenExpired = false;
                        }
                    }
                }
                if (TokenExpired && data.versiToken != "2")
                {
                    string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
                    string userMTA = data.mta_username_email_merchant;//<-- email user merchant
                    string passMTA = data.mta_password_password_merchant;//<-- pass merchant
                                                                         //apiId = "mta-api-sandbox:sandbox-secret-key";
                                                                         //string urll = "https://apisandbox.blibli.com/v2/oauth/token?grant_type=client_credentials";
                    string urll = "https://api.blibli.com/v2/oauth/token?channelId=MasterOnline";
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                    myReq.Method = "POST";
                    myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(apiId))));
                    myReq.ContentType = "application/x-www-form-urlencoded";
                    myReq.Accept = "application/json";
                    string myData = "grant_type=password&password=" + passMTA + "&username=" + userMTA + "";
                    //Stream dataStream = myReq.GetRequestStream();
                    //WebResponse response = myReq.GetResponse();
                    //dataStream = response.GetResponseStream();
                    //StreamReader reader = new StreamReader(dataStream);
                    string responseFromServer = "";
                    try
                    {
                        myReq.ContentLength = myData.Length;
                        using (var dataStream = myReq.GetRequestStream())
                        {
                            dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
                        }
                        using (WebResponse response = myReq.GetResponse())
                        {
                            using (Stream stream = response.GetResponseStream())
                            {
                                StreamReader reader = new StreamReader(stream);
                                responseFromServer = reader.ReadToEnd();
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    //dataStream.Close();
                    //response.Close();
                    // nilai token yg diambil adalah access-token. setelah 24jam biasanya harus masuk ke refresh token. dan harus diambil lagi acces token yg baru
                    //cek refreshToken
                    if (responseFromServer != "")
                    {
                        var retAPI = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(BliBliToken)) as BliBliToken;
                        if (retAPI.error == null)
                        {
                            //var arf01inDB = ErasoftDbContext.ARF01.Where(p => p.API_CLIENT_P.Equals(data.API_client_password) && p.API_CLIENT_U.Equals(data.API_client_username)).SingleOrDefault();
                            //if (arf01inDB != null)
                            //{
                            arf01inDB.TOKEN = retAPI.access_token;
                            arf01inDB.REFRESH_TOKEN = retAPI.refresh_token + ";" + Convert.ToString(retAPI.expires_in) + ";" + Convert.ToString(currentTimeRequest);

                            //ADD BY TRI, SET STATUS_API
                            arf01inDB.STATUS_API = "1";
                            //END ADD BY TRI, SET STATUS_API

                            ErasoftDbContext.SaveChanges();

                            ret = retAPI.access_token;
                        }
                        else
                        {
                            //ADD BY TRI, SET STATUS_API
                            arf01inDB.STATUS_API = "0";
                            //END ADD BY TRI, SET STATUS_API

                            ErasoftDbContext.SaveChanges();
                        }
                    }
                }
            }
            return ret;
        }

        protected string SetupContextTokopedia(string DatabasePathErasoft, string uname, TokopediaAPIData data)
        {

            string ret = "";
            dbPathEra = DatabasePathErasoft;
            var EDB = new DatabaseSQL(dbPathEra);
            string EraServerName = EDB.GetServerName("sConn");
            var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);
            username = uname;

            var arf01inDB = ErasoftDbContext.ARF01.Where(p => p.RecNum == data.idmarket).SingleOrDefault();
            if (arf01inDB != null)
            {
                ret = arf01inDB.TOKEN;

                TokopediaControllerJob.TokopediaAPIData dataJob = new TokopediaControllerJob.TokopediaAPIData
                {
                    merchant_code = data.merchant_code, //FSID
                    API_client_password = data.API_client_password, //Client Secret
                    API_client_username = data.API_client_username, //Client ID
                    API_secret_key = data.API_secret_key, //Shop ID 
                    idmarket = data.idmarket,
                    DatabasePathErasoft = DatabasePathErasoft,
                    username = data.username
                };
                var tokenRet = new TokopediaControllerJob().GetToken(dataJob);

                ret = tokenRet.access_token;
            }
            return ret;
        }
        protected enum api_status
        {
            Pending = 1,
            Success = 2,
            Failed = 3,
            Exception = 4
        }

        //contoh
        //queue sesuai dengan queue yang tersedia oleh BackgroundJobClient
        //NotifyOnFailed untuk message failed pada notifikasi, {obj} adalah nama object yang gagal ( contoh, kode barang, nomor so )
        //jika dipasangkan NotifyOnFailed, method harus memiliki parameter DBPathEra sebagai parameter pertama, dan nama object sebagai parameter kedua
        [AutomaticRetry(Attempts = 1)]
        [Queue("1_critical")]
        [NotifyOnFailed("Test notifikasi {obj} Gagal.")]
        public void testFailedNotif(string dbPathEra, string namaObj, string CUST, string category, string action_name)
        {
            var a = namaObj.Substring(0, 30);
        }

        protected void manageAPI_LOG_MARKETPLACE(api_status action, ErasoftContext db, string CUST, API_LOG_MARKETPLACE data, string Marketplace)
        {
            switch (action)
            {
                case api_status.Pending:
                    {
                        var apiLog = new MasterOnline.API_LOG_MARKETPLACE
                        {
                            CUST = CUST,
                            CUST_ATTRIBUTE_1 = CUST,
                            CUST_ATTRIBUTE_2 = data.CUST_ATTRIBUTE_2 != null ? data.CUST_ATTRIBUTE_2 : "",
                            CUST_ATTRIBUTE_3 = data.CUST_ATTRIBUTE_3 != null ? data.CUST_ATTRIBUTE_3 : "",
                            CUST_ATTRIBUTE_4 = data.CUST_ATTRIBUTE_4 != null ? data.CUST_ATTRIBUTE_4 : "",
                            CUST_ATTRIBUTE_5 = data.CUST_ATTRIBUTE_5 != null ? data.CUST_ATTRIBUTE_5 : "",
                            MARKETPLACE = Marketplace,
                            REQUEST_ACTION = data.REQUEST_ACTION,
                            REQUEST_ATTRIBUTE_1 = data.REQUEST_ATTRIBUTE_1 != null ? data.REQUEST_ATTRIBUTE_1 : "",
                            REQUEST_ATTRIBUTE_2 = data.REQUEST_ATTRIBUTE_2 != null ? data.REQUEST_ATTRIBUTE_2 : "",
                            REQUEST_ATTRIBUTE_3 = data.REQUEST_ATTRIBUTE_3 != null ? data.REQUEST_ATTRIBUTE_3 : "",
                            REQUEST_ATTRIBUTE_4 = data.REQUEST_ATTRIBUTE_4 != null ? data.REQUEST_ATTRIBUTE_4 : "",
                            REQUEST_ATTRIBUTE_5 = data.REQUEST_ATTRIBUTE_5 != null ? data.REQUEST_ATTRIBUTE_5 : "",
                            REQUEST_DATETIME = data.REQUEST_DATETIME,
                            REQUEST_ID = data.REQUEST_ID,
                            REQUEST_STATUS = data.REQUEST_STATUS,
                            REQUEST_EXCEPTION = data.REQUEST_EXCEPTION != null ? data.REQUEST_EXCEPTION : "",
                            REQUEST_RESULT = data.REQUEST_RESULT != null ? data.REQUEST_RESULT : "",
                        };
                        db.API_LOG_MARKETPLACE.Add(apiLog);
                        db.SaveChanges();
                    }
                    break;
                case api_status.Success:
                    {
                        var apiLogInDb = db.API_LOG_MARKETPLACE.Where(p => p.REQUEST_ID == data.REQUEST_ID).SingleOrDefault();
                        if (apiLogInDb != null)
                        {
                            apiLogInDb.REQUEST_STATUS = "Success";
                            apiLogInDb.REQUEST_RESULT = data.REQUEST_RESULT;
                            apiLogInDb.REQUEST_EXCEPTION = data.REQUEST_EXCEPTION;
                            db.SaveChanges();
                        }
                    }
                    break;
                case api_status.Failed:
                    {
                        var apiLogInDb = db.API_LOG_MARKETPLACE.Where(p => p.REQUEST_ID == data.REQUEST_ID).SingleOrDefault();
                        if (apiLogInDb != null)
                        {
                            apiLogInDb.REQUEST_STATUS = "Failed";
                            apiLogInDb.REQUEST_RESULT = data.REQUEST_RESULT;
                            apiLogInDb.REQUEST_EXCEPTION = data.REQUEST_EXCEPTION;
                            db.SaveChanges();
                        }
                    }
                    break;
                case api_status.Exception:
                    {
                        var apiLogInDb = db.API_LOG_MARKETPLACE.Where(p => p.REQUEST_ID == data.REQUEST_ID).SingleOrDefault();
                        if (apiLogInDb != null)
                        {
                            apiLogInDb.REQUEST_STATUS = "Failed";
                            apiLogInDb.REQUEST_RESULT = "Exception";
                            apiLogInDb.REQUEST_EXCEPTION = data.REQUEST_EXCEPTION;
                            db.SaveChanges();
                        }
                    }
                    break;
            }
        }

        public class mp_and_item_data
        {
            public string SORT1_CUST { get; set; }
            public string API_CLIENT_P { get; set; }
            public string API_CLIENT_U { get; set; }
            public string API_KEY { get; set; }
            public string TOKEN { get; set; }
            public string EMAIL { get; set; }
            public string PASSWORD { get; set; }
            public int RECNUM { get; set; }
            public string KODE_BRG_MP { get; set; }
        }

        public double GetQOHSTF08A(string Barang, string Gudang)
        {
            var EDB = new DatabaseSQL(dbPathEra);
            string EraServerName = EDB.GetServerName("sConn");
            var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);

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

            #region Hitung Qty Reserved Blibli
            //remark by calvin 7 agustus 2019, req dan confirm by pak dani
            //karena reserved stock blibli sudah terisi saat pembeli belum memilih metode pembayaran, sehingga besar kemungkinan dapat membatalkan pesanan.
            //{
            //    var list_brg_mp = ErasoftDbContext.Database.SqlQuery<mp_and_item_data>("SELECT SORT1_CUST,API_CLIENT_P,API_CLIENT_U,API_KEY,TOKEN,EMAIL,PASSWORD,A.RECNUM,ISNULL(B.BRG_MP,'') KODE_BRG_MP FROM ARF01 (NOLOCK) A INNER JOIN STF02H (NOLOCK) B ON A.RECNUM = B.IDMARKET WHERE B.BRG = '" + Barang +"' AND B.DISPLAY = '1' AND A.NAMA='16' AND A.STATUS_API='1'").ToList();
            //    foreach (var item in list_brg_mp)
            //    {
            //        BlibliAPIData iden = new BlibliAPIData
            //        {
            //            merchant_code = item.SORT1_CUST,
            //            API_client_password = item.API_CLIENT_P,
            //            API_client_username = item.API_CLIENT_U,
            //            API_secret_key = item.API_KEY,
            //            token = item.TOKEN,
            //            mta_username_email_merchant = item.EMAIL,
            //            mta_password_password_merchant = item.PASSWORD,
            //            idmarket = item.RECNUM,
            //            DatabasePathErasoft = dbPathEra
            //        };
            //        double qtyBlibliReserved = 0;
            //        try
            //        {
            //            qtyBlibliReserved = Blibli_getReservedStockLv2(iden, item.KODE_BRG_MP);
            //        }
            //        catch (Exception ex)
            //        {

            //        }
            //        qtyOnHand -= qtyBlibliReserved;
            //    }
            //}
            //end remark by calvin 7 agustus 2019
            #endregion
            return qtyOnHand;
        }

        public void updateStockMarketPlace_ForItemInSTF08A(string connId, string DatabasePathErasoft, string uname)
        {
            SetupContext(DatabasePathErasoft, uname);
            var MoDbContext = new MoDbContext("");
            var EDB = new DatabaseSQL(DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            var ErasoftDbContext = new ErasoftContext(EraServerName, DatabasePathErasoft);
            // remark by fauzi tgl 07 Januari 2020
            //var DataUsaha = ErasoftDbContext.SIFSYS.FirstOrDefault();
            //bool doAPI = false;
            //if (DataUsaha != null)
            //{
            //    if (DataUsaha.JTRAN_RETUR == "1")
            //    {
            //        doAPI = true;
            //    }
            //}
            //if (doAPI)
            //{
            //var Marketplaces = MoDbContext.Marketplaces;
            //var kdBL = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BUKALAPAK").IdMarket;
            //var kdLazada = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "LAZADA").IdMarket;
            //var kdBli = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BLIBLI").IdMarket;
            //var kdElevenia = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "ELEVENIA").IdMarket;
            //var kdShopee = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "SHOPEE").IdMarket;
            //var kdTokped = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "TOKOPEDIA").IdMarket;
            //var kdJD = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "JD.ID").IdMarket;
            // remark by fauzi tgl 07 Januari 2020
            // change by fauzi 07 Januari 2020
            var kdBL = 8;
            var kdLazada = 7;
            var kdBli = 16;
            var kdElevenia = 9;
            var kdShopee = 17;
            var kdTokped = 15;
            var kdJD = 19;
            var kdShopify = 21;
            var kd82Cart = 20;
            // change by fauzi 07 Januari 2020
            int delayTokped = 0;

            string EDBConnID = EDB.GetConnectionString("ConnId");
            var sqlStorage = new SqlServerStorage(EDBConnID);

            var client = new BackgroundJobClient(sqlStorage);

            var TEMP_ALL_MP_ORDER_ITEMs = ErasoftDbContext.Database.SqlQuery<TEMP_ALL_MP_ORDER_ITEM>("SELECT DISTINCT BRG, 'ALL_ITEM_WITH_MUTATION' AS CONN_ID FROM STF08A").ToList();

            List<string> listBrg = new List<string>();
            foreach (var item in TEMP_ALL_MP_ORDER_ITEMs)
            {
                listBrg.Add(item.BRG);
            }

            var ListARF01 = ErasoftDbContext.ARF01.ToList();
            foreach (string kdBrg in listBrg)
            {
                //var qtyOnHand = GetQOHSTF08A(kdBrg, "ALL");
                var barangInDb = ErasoftDbContext.STF02.SingleOrDefault(b => b.BRG.Equals(kdBrg));
                var brgMarketplace = ErasoftDbContext.STF02H.Where(p => p.BRG.Equals(kdBrg) && !string.IsNullOrEmpty(p.BRG_MP)).ToList();

                foreach (var stf02h in brgMarketplace)
                {
                    var marketPlace = ListARF01.SingleOrDefault(p => p.RecNum == stf02h.IDMARKET);
                    if (marketPlace.NAMA.Equals(kdBL.ToString()))
                    {
                        if (marketPlace.TIDAK_HIT_UANG_R == true)
                        {
#if (DEBUG || Debug_AWS)
                            Bukalapak_updateStock(DatabasePathErasoft, kdBrg, marketPlace.CUST, "Stock", "Update Stok", stf02h.BRG_MP, "", "", marketPlace.API_KEY, marketPlace.TOKEN, uname, null);
#else
                            client.Enqueue<StokControllerJob>(x => x.Bukalapak_updateStock(DatabasePathErasoft, kdBrg, marketPlace.CUST, "Stock", "Update Stok", stf02h.BRG_MP, "", "", marketPlace.API_KEY, marketPlace.TOKEN, uname, null));
#endif
                        }
                    }
                    else if (marketPlace.NAMA.Equals(kdLazada.ToString()))
                    {
                        if (marketPlace.TIDAK_HIT_UANG_R == true)
                        {
#if (DEBUG || Debug_AWS)
                            Lazada_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", stf02h.BRG_MP, "", "", marketPlace.TOKEN, uname, null);
#else
                            client.Enqueue<StokControllerJob>(x => x.Lazada_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", stf02h.BRG_MP, "", "", marketPlace.TOKEN, uname, null));
#endif
                        }
                    }
                    else if (marketPlace.NAMA.Equals(kdElevenia.ToString()))
                    {
                        if (marketPlace.TIDAK_HIT_UANG_R == true)
                        {
                            string[] imgID = new string[3];
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

                            EleveniaProductData data = new EleveniaProductData
                            {
                                api_key = marketPlace.API_KEY,
                                kode = barangInDb.BRG,
                                nama = barangInDb.NAMA + ' ' + barangInDb.NAMA2 + ' ' + barangInDb.NAMA3,
                                berat = (barangInDb.BERAT / 1000).ToString(),//MO save dalam Gram, Elevenia dalam Kilogram
                                imgUrl = imgID,
                                Keterangan = barangInDb.Deskripsi,
                                Qty = "",
                                DeliveryTempNo = stf02h.DeliveryTempElevenia,
                                IDMarket = marketPlace.RecNum.ToString(),
                            };
                            data.Brand = ErasoftDbContext.STF02E.SingleOrDefault(m => m.KODE == barangInDb.Sort2 && m.LEVEL == "2").KET;
                            data.Price = stf02h.HJUAL.ToString();
                            data.kode_mp = stf02h.BRG_MP;
                            //eleApi.UpdateProductQOH_Price(data);
                            client.Enqueue<StokControllerJob>(x => x.Elevenia_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, uname, null));
                        }
                    }
                    else if (marketPlace.NAMA.Equals(kdBli.ToString()))
                    {
                        if (marketPlace.TIDAK_HIT_UANG_R == true)
                        {
                            if (!string.IsNullOrEmpty(marketPlace.Kode))
                            {
                                BlibliAPIData iden = new BlibliAPIData
                                {
                                    merchant_code = marketPlace.Sort1_Cust,
                                    API_client_password = marketPlace.API_CLIENT_P,
                                    API_client_username = marketPlace.API_CLIENT_U,
                                    API_secret_key = marketPlace.API_KEY,
                                    token = marketPlace.TOKEN,
                                    mta_username_email_merchant = marketPlace.EMAIL,
                                    mta_password_password_merchant = marketPlace.PASSWORD,
                                    idmarket = marketPlace.RecNum.Value,
                                    //add by nurul 22/7/2020
                                    versiToken = marketPlace.KD_ANALISA
                                    //end add by nurul 22/7/2020
                                };
                                BlibliProductData data = new BlibliProductData
                                {
                                    kode = kdBrg,
                                    kode_mp = stf02h.BRG_MP,
                                    Qty = "",
                                    MinQty = "0"
                                };
                                data.Price = barangInDb.HJUAL.ToString();
                                data.MarketPrice = stf02h.HJUAL.ToString();
                                var display = Convert.ToBoolean(stf02h.DISPLAY);
                                data.display = display ? "true" : "false";
                                //var BliApi = new BlibliController();
#if (DEBUG || Debug_AWS)
                                Task.Run(() => Blibli_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", iden, data, uname, null).Wait());
#else
                                client.Enqueue<StokControllerJob>(x => x.Blibli_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", iden, data, uname, null));
#endif
                            }
                        }
                    }
                    //add by calvin 18 desember 2018
                    else if (marketPlace.NAMA.Equals(kdTokped.ToString()))
                    {
                        if (marketPlace.TIDAK_HIT_UANG_R == true)
                        {
                            if (!string.IsNullOrEmpty(marketPlace.Sort1_Cust))
                            {
                                if (!string.IsNullOrEmpty(stf02h.BRG_MP))
                                {
                                    TokopediaAPIData iden = new TokopediaAPIData()
                                    {
                                        merchant_code = marketPlace.Sort1_Cust, //FSID
                                        API_client_password = marketPlace.API_CLIENT_P, //Client ID
                                        API_client_username = marketPlace.API_CLIENT_U, //Client Secret
                                        API_secret_key = marketPlace.API_KEY, //Shop ID 
                                        token = marketPlace.TOKEN,
                                        idmarket = marketPlace.RecNum.Value
                                    };
                                    //if (stf02h.BRG_MP.Contains("PENDING"))
                                    if (stf02h.BRG_MP.Contains("PENDING") || stf02h.BRG_MP.Contains("PEDITENDING"))
                                    {
                                        //dibuat recurrent nanti
                                        //var cekPendingCreate = ErasoftDbContext.STF02H.Where(p => p.IDMARKET == marketPlace.RecNum && p.BRG_MP == stf02h.BRG_MP).ToList();
                                        //if (cekPendingCreate.Count > 0)
                                        //{
                                        //    foreach (var item in cekPendingCreate)
                                        //    {
                                        //        Task.Run(() => TokoAPI.CreateProductGetStatus(iden, item.BRG, Convert.ToInt32(item.BRG_MP.Split(';')[1]), item.BRG_MP.Split(';')[2]).Wait());
                                        //    }
                                        //}
                                    }
                                    else
                                    {
#if (DEBUG || Debug_AWS)
                                        Task.Run(() => Tokped_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", iden, Convert.ToInt32(stf02h.BRG_MP), 0, uname, null)).Wait();
#else
                                        //delayTokped++;
                                        client.Enqueue<StokControllerJob>(x => x.Tokped_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", iden, Convert.ToInt32(stf02h.BRG_MP), 0, uname, null));
                                        //client.Schedule<StokControllerJob>(x => x.Tokped_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", iden, Convert.ToInt32(stf02h.BRG_MP), 0, uname, null), TimeSpan.FromSeconds(delayTokped));
#endif
                                    }
                                }
                            }
                        }
                    }
                    else if (marketPlace.NAMA.Equals(kdShopee.ToString()))
                    {
                        if (marketPlace.TIDAK_HIT_UANG_R == true)
                        {
                            ShopeeAPIData data = new ShopeeAPIData()
                            {
                                merchant_code = marketPlace.Sort1_Cust,
                            };
                            if (stf02h.BRG_MP != "")
                            {
                                string[] brg_mp = stf02h.BRG_MP.Split(';');
                                if (brg_mp.Count() == 2)
                                {
                                    if (brg_mp[1] == "0" || brg_mp[1] == "")
                                    {
#if (DEBUG || Debug_AWS)
                                        Task.Run(() => Shopee_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname, null)).Wait();
#else
                                        client.Enqueue<StokControllerJob>(x => x.Shopee_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname, null));
#endif
                                    }
                                    else if (brg_mp[1] != "")
                                    {
#if (DEBUG || Debug_AWS)
                                        Task.Run(() => Shopee_updateVariationStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname, null)).Wait();
#else
                                        client.Enqueue<StokControllerJob>(x => x.Shopee_updateVariationStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname, null));
#endif
                                    }
                                }
                            }
                        }
                    }
                    else if (marketPlace.NAMA.Equals(kdShopify.ToString()))
                    {
                        if (marketPlace.TIDAK_HIT_UANG_R == true)
                        {
                            ShopifyAPIData data = new ShopifyAPIData()
                            {
                                no_cust = marketPlace.Sort1_Cust,
                                account_store = marketPlace.PERSO,
                                API_key = marketPlace.API_KEY,
                                API_password = marketPlace.API_CLIENT_P,
                                email = marketPlace.EMAIL
                            };
                            if (stf02h.BRG_MP != "")
                            {
                                string[] brg_mp = stf02h.BRG_MP.Split(';');
                                if (brg_mp.Count() == 2)
                                {
                                    //if (brg_mp[1] == "0" || brg_mp[1] == "")
                                    //{
#if (DEBUG || Debug_AWS)
                                    Shopify_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname, null);
#else
                                        client.Enqueue<StokControllerJob>(x => x.Shopify_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname, null));
#endif
                                    //}
                                    //else if (brg_mp[1] != "")
                                    //{
                                    //#if (DEBUG || Debug_AWS)
                                    //                                        Task.Run(() => Shopify_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname, null)).Wait();
                                    //#else
                                    //                                        client.Enqueue<StokControllerJob>(x => x.Shopify_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname, null));
                                    //#endif
                                    //}
                                }
                            }
                        }
                    }
                    //end add by calvin 18 desember 2018
                    //add by Tri 11 April 2019
                    else if (marketPlace.NAMA.Equals(kdJD.ToString()))
                    {
                        if (marketPlace.TIDAK_HIT_UANG_R == true)
                        {
                            JDIDAPIData data = new JDIDAPIData()
                            {
                                accessToken = marketPlace.TOKEN,
                                appKey = marketPlace.API_KEY,
                                appSecret = marketPlace.API_CLIENT_U,
                            };
                            if (stf02h.BRG_MP != "")
                            {
#if (DEBUG || Debug_AWS)
                                Task.Run(() => JD_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname, null)).Wait();
#else
                                client.Enqueue<StokControllerJob>(x => x.JD_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname, null));
#endif
                            }
                        }
                    }
                    //end add by Tri 11 April 2019
                    //add by fauzi for 82Cart
                    else if (marketPlace.NAMA.Equals(kd82Cart.ToString()))
                    {
                        if (marketPlace.TIDAK_HIT_UANG_R == true)
                        {
                            if (!string.IsNullOrEmpty(marketPlace.Sort1_Cust))
                            {
                                if (!string.IsNullOrEmpty(stf02h.BRG_MP))
                                {
                                    E2CartAPIData data = new E2CartAPIData()
                                    {
                                        no_cust = marketPlace.CUST,
                                        account_store = marketPlace.PERSO,
                                        API_key = marketPlace.API_KEY,
                                        API_credential = marketPlace.Sort1_Cust,
                                        API_url = marketPlace.PERSO,
                                        DatabasePathErasoft = dbPathEra
                                    };
                                    if (stf02h.BRG_MP.Contains("PENDING") || stf02h.BRG_MP.Contains("PEDITENDING"))
                                    {

                                    }
                                    else
                                    {
#if (DEBUG || Debug_AWS)
                                        Task.Run(() => E2Cart_UpdateStock_82Cart(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname)).Wait();
                                        //E2Cart_UpdateStock_82Cart(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname);
#else
                                        client.Enqueue<StokControllerJob>(x => x.E2Cart_UpdateStock_82Cart(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname));
#endif
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //}
        }

        public void updateStockMarketPlace(string connId, string DatabasePathErasoft, string uname)
        {
            SetupContext(DatabasePathErasoft, uname);
            var MoDbContext = new MoDbContext("");
            var EDB = new DatabaseSQL(DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            var ErasoftDbContext = new ErasoftContext(EraServerName, DatabasePathErasoft);

            var ListARF01 = ErasoftDbContext.ARF01.ToList();
            // remark by fauzi 18 desember 2019
            // var DataUsaha = ErasoftDbContext.SIFSYS.FirstOrDefault();
            // bool doAPI = false;
            // if (DataUsaha != null)
            //{
            //    if (DataUsaha.JTRAN_RETUR == "1")
            //    {
            //        doAPI = true;
            //    }
            //}
            //if (doAPI)
            //{
            // change by fauzi 18 Desember 2019
            //var Marketplaces = MoDbContext.Marketplaces;

            // change by fauzi 18 Desember 2019
            var kdBL = 8;
            var kdLazada = 7;
            var kdBli = 16;
            var kdElevenia = 9;
            var kdShopee = 17;
            var kdTokped = 15;
            var kdJD = 19;
            var kd82Cart = 20;
            var kdshopify = 21;
            int delayTokped = 0;
            string EDBConnID = EDB.GetConnectionString("ConnId");
            var sqlStorage = new SqlServerStorage(EDBConnID);

            var client = new BackgroundJobClient(sqlStorage);

            var TEMP_ALL_MP_ORDER_ITEMs = ErasoftDbContext.Database.SqlQuery<TEMP_ALL_MP_ORDER_ITEM>("SELECT * FROM TEMP_ALL_MP_ORDER_ITEM WHERE CONN_ID = '" + connId + "'").ToList();

            List<string> listBrg = new List<string>();
            foreach (var item in TEMP_ALL_MP_ORDER_ITEMs)
            {
                listBrg.Add(item.BRG);
            }
            if (connId == "MANUAL")
            {
                listBrg.Add("03.MIC00.00");
                listBrg.Add("17.TTOT00.00.6m");
                //listBrg.Add("1578");
                //listBrg.Add("2004");
                //listBrg.Add("2495");
                //listBrg.Add("2497");
                //listBrg.Add("SP1930.01.38");
                //listBrg.Add("SP1930.01.39");
                //listBrg.Add("SP1930.01.40");
                //listBrg.Add("SP1930.02.36");
                //listBrg.Add("SP1930.02.37");
                //listBrg.Add("SP1930.02.38");
                //listBrg.Add("SP1930.02.39");
                //listBrg.Add("SP1930.02.40");
                //listBrg.Add("SP1939.03.02");
                //listBrg.Add("SP1939.03.03");
                //listBrg.Add("SP1939.03.04");
                //listBrg.Add("SP1939.03.05");
                //listBrg.Add("SP1939.03.06");
                //listBrg.Add("SP1939.06.02");
                //listBrg.Add("SP1939.06.03");
                //listBrg.Add("SP1939.06.04");
                //listBrg.Add("SP1939.06.05");
                //listBrg.Add("SP1939.06.06");
                //listBrg.Add("SP1939.08.02");
                //listBrg.Add("SP1939.08.03");
                //listBrg.Add("SP1939.08.04");
                //listBrg.Add("SP1939.08.05");
                //listBrg.Add("SP1939.08.06");
            }

            foreach (string kdBrg in listBrg)
            {
                //var qtyOnHand = GetQOHSTF08A(kdBrg, "ALL");
                var barangInDb = ErasoftDbContext.STF02.SingleOrDefault(b => b.BRG.Equals(kdBrg));
                var brgMarketplace = ErasoftDbContext.STF02H.Where(p => p.BRG.Equals(kdBrg) && !string.IsNullOrEmpty(p.BRG_MP)).ToList();

                foreach (var stf02h in brgMarketplace)
                {
                    var marketPlace = ListARF01.SingleOrDefault(p => p.RecNum == stf02h.IDMARKET);
                    if (marketPlace.NAMA.Equals(kdBL.ToString()))
                    {
                        if (marketPlace.TIDAK_HIT_UANG_R == true)
                        {
#if (DEBUG || Debug_AWS)
                            Bukalapak_updateStock(DatabasePathErasoft, kdBrg, marketPlace.CUST, "Stock", "Update Stok", stf02h.BRG_MP, "", "", marketPlace.API_KEY, marketPlace.TOKEN, uname, null);
#else
                            client.Enqueue<StokControllerJob>(x => x.Bukalapak_updateStock(DatabasePathErasoft, kdBrg, marketPlace.CUST, "Stock", "Update Stok", stf02h.BRG_MP, "", "", marketPlace.API_KEY, marketPlace.TOKEN, uname, null));
#endif
                        }

                    }
                    else if (marketPlace.NAMA.Equals(kdLazada.ToString()))
                    {
                        if (marketPlace.TIDAK_HIT_UANG_R == true)
                        {
#if (DEBUG || Debug_AWS)
                            Lazada_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", stf02h.BRG_MP, "", "", marketPlace.TOKEN, uname, null);
#else
                            client.Enqueue<StokControllerJob>(x => x.Lazada_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", stf02h.BRG_MP, "", "", marketPlace.TOKEN, uname, null));
#endif
                        }
                    }
                    else if (marketPlace.NAMA.Equals(kdElevenia.ToString()))
                    {
                        if (marketPlace.TIDAK_HIT_UANG_R == true)
                        {
                            string[] imgID = new string[3];
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


                            EleveniaProductData data = new EleveniaProductData
                            {
                                api_key = marketPlace.API_KEY,
                                kode = barangInDb.BRG,
                                nama = barangInDb.NAMA + ' ' + barangInDb.NAMA2 + ' ' + barangInDb.NAMA3,
                                berat = (barangInDb.BERAT / 1000).ToString(),//MO save dalam Gram, Elevenia dalam Kilogram
                                imgUrl = imgID,
                                Keterangan = barangInDb.Deskripsi,
                                Qty = "",
                                DeliveryTempNo = stf02h.DeliveryTempElevenia,
                                IDMarket = marketPlace.RecNum.ToString(),
                            };
                            data.Brand = ErasoftDbContext.STF02E.SingleOrDefault(m => m.KODE == barangInDb.Sort2 && m.LEVEL == "2").KET;
                            data.Price = stf02h.HJUAL.ToString();
                            data.kode_mp = stf02h.BRG_MP;
                            //eleApi.UpdateProductQOH_Price(data);
#if (DEBUG || Debug_AWS)
                            Elevenia_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, uname, null);
#else
                            client.Enqueue<StokControllerJob>(x => x.Elevenia_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, uname, null));
#endif
                        }
                    }
                    else if (marketPlace.NAMA.Equals(kdBli.ToString()))
                    {
                        if (marketPlace.TIDAK_HIT_UANG_R == true)
                        {
                            if (!string.IsNullOrEmpty(marketPlace.Kode))
                            {
                                BlibliAPIData iden = new BlibliAPIData
                                {
                                    merchant_code = marketPlace.Sort1_Cust,
                                    API_client_password = marketPlace.API_CLIENT_P,
                                    API_client_username = marketPlace.API_CLIENT_U,
                                    API_secret_key = marketPlace.API_KEY,
                                    token = marketPlace.TOKEN,
                                    mta_username_email_merchant = marketPlace.EMAIL,
                                    mta_password_password_merchant = marketPlace.PASSWORD,
                                    idmarket = marketPlace.RecNum.Value,
                                    //add by nurul 22/7/2020
                                    versiToken = marketPlace.KD_ANALISA
                                    //end add by nurul 22/7/2020
                                };
                                BlibliProductData data = new BlibliProductData
                                {
                                    kode = kdBrg,
                                    kode_mp = stf02h.BRG_MP,
                                    Qty = "",
                                    MinQty = "0"
                                };
                                data.Price = barangInDb.HJUAL.ToString();
                                data.MarketPrice = stf02h.HJUAL.ToString();
                                var display = Convert.ToBoolean(stf02h.DISPLAY);
                                data.display = display ? "true" : "false";
                                //var BliApi = new BlibliController();
#if (DEBUG || Debug_AWS)
                                Task.Run(() => Blibli_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", iden, data, uname, null).Wait());
#else
                                client.Enqueue<StokControllerJob>(x => x.Blibli_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", iden, data, uname, null));
#endif
                            }
                        }
                    }
                    //add by calvin 18 desember 2018
                    else if (marketPlace.NAMA.Equals(kdTokped.ToString()))
                    {
                        if (marketPlace.TIDAK_HIT_UANG_R == true)
                        {
                            if (!string.IsNullOrEmpty(marketPlace.Sort1_Cust))
                            {
                                if (!string.IsNullOrEmpty(stf02h.BRG_MP))
                                {
                                    TokopediaAPIData iden = new TokopediaAPIData()
                                    {
                                        merchant_code = marketPlace.Sort1_Cust, //FSID
                                        API_client_password = marketPlace.API_CLIENT_P, //Client ID
                                        API_client_username = marketPlace.API_CLIENT_U, //Client Secret
                                        API_secret_key = marketPlace.API_KEY, //Shop ID 
                                        token = marketPlace.TOKEN,
                                        idmarket = marketPlace.RecNum.Value
                                    };
                                    //if (stf02h.BRG_MP.Contains("PENDING"))
                                    if (stf02h.BRG_MP.Contains("PENDING") || stf02h.BRG_MP.Contains("PEDITENDING"))
                                    {
                                        //dibuat recurrent nanti
                                        //var cekPendingCreate = ErasoftDbContext.STF02H.Where(p => p.IDMARKET == marketPlace.RecNum && p.BRG_MP == stf02h.BRG_MP).ToList();
                                        //if (cekPendingCreate.Count > 0)
                                        //{
                                        //    foreach (var item in cekPendingCreate)
                                        //    {
                                        //        Task.Run(() => TokoAPI.CreateProductGetStatus(iden, item.BRG, Convert.ToInt32(item.BRG_MP.Split(';')[1]), item.BRG_MP.Split(';')[2]).Wait());
                                        //    }
                                        //}
                                    }
                                    else
                                    {
#if (DEBUG || Debug_AWS)
                                        Task.Run(() => Tokped_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", iden, Convert.ToInt32(stf02h.BRG_MP), 0, uname, null)).Wait();
#else
                                        //delayTokped++;
                                        client.Enqueue<StokControllerJob>(x => x.Tokped_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", iden, Convert.ToInt32(stf02h.BRG_MP), 0, uname, null));
                                        //client.Schedule<StokControllerJob>(x => x.Tokped_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", iden, Convert.ToInt32(stf02h.BRG_MP), 0, uname, null), TimeSpan.FromSeconds(delayTokped));

#endif
                                    }
                                }
                            }
                        }
                    }
                    else if (marketPlace.NAMA.Equals(kdShopee.ToString()))
                    {
                        if (marketPlace.TIDAK_HIT_UANG_R == true)
                        {
                            ShopeeAPIData data = new ShopeeAPIData()
                            {
                                merchant_code = marketPlace.Sort1_Cust,
                            };
                            if (stf02h.BRG_MP != "")
                            {
                                string[] brg_mp = stf02h.BRG_MP.Split(';');
                                if (brg_mp.Count() == 2)
                                {
                                    if (brg_mp[1] == "0" || brg_mp[1] == "")
                                    {
#if (DEBUG || Debug_AWS)
                                        Task.Run(() => Shopee_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname, null)).Wait();
#else
                                        client.Enqueue<StokControllerJob>(x => x.Shopee_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname, null));
#endif
                                    }
                                    else if (brg_mp[1] != "")
                                    {
#if (DEBUG || Debug_AWS)
                                        Task.Run(() => Shopee_updateVariationStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname, null)).Wait();
#else
                                        client.Enqueue<StokControllerJob>(x => x.Shopee_updateVariationStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname, null));
#endif
                                    }
                                }
                            }
                        }
                    }
                    //end add by calvin 18 desember 2018
                    else if (marketPlace.NAMA.Equals(kdshopify.ToString()))
                    {
                        if (marketPlace.TIDAK_HIT_UANG_R == true)
                        {
                            ShopifyAPIData data = new ShopifyAPIData()
                            {
                                no_cust = marketPlace.Sort1_Cust,
                                account_store = marketPlace.PERSO,
                                API_key = marketPlace.API_KEY,
                                API_password = marketPlace.API_CLIENT_P
                            };
                            if (stf02h.BRG_MP != "")
                            {
                                string[] brg_mp = stf02h.BRG_MP.Split(';');
                                if (brg_mp.Count() == 2)
                                {
                                    //if (brg_mp[1] == "0" || brg_mp[1] == "")
                                    //{
#if (DEBUG || Debug_AWS)
                                    Shopify_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname, null);
#else
                                    client.Enqueue<StokControllerJob>(x => x.Shopify_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname, null));
#endif
                                    //}
                                    //else if (brg_mp[1] != "")
                                    //{
                                    //#if (DEBUG || Debug_AWS)
                                    //                                        Task.Run(() => Shopify_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname, null)).Wait();
                                    //#else
                                    //                                        client.Enqueue<StokControllerJob>(x => x.Shopify_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname, null));
                                    //#endif
                                    //}
                                }
                            }
                        }
                    }
                    //add by Fauzi 21 Juli 2020
                    else if (marketPlace.NAMA.Equals(kdJD.ToString()))
                    {
                        if (marketPlace.TIDAK_HIT_UANG_R == true)
                        {
                            JDIDAPIData data = new JDIDAPIData()
                            {
                                no_cust = marketPlace.CUST,
                                accessToken = marketPlace.TOKEN,
                                appKey = marketPlace.API_KEY,
                                appSecret = marketPlace.API_CLIENT_U,
                                username = marketPlace.USERNAME,
                                email = marketPlace.EMAIL,
                                DatabasePathErasoft = dbPathEra
                            };
                            if (stf02h.BRG_MP != "")
                            {
#if (DEBUG || Debug_AWS)
                                Task.Run(() => JD_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname, null)).Wait();
#else
                                client.Enqueue<StokControllerJob>(x => x.JD_updateStock(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname, null));
#endif
                            }
                        }
                    }
                    //end add by Fauzi 21 Juli 2020
                    //add by fauzi for 82 Cart
                    else if (marketPlace.NAMA.Equals(kd82Cart.ToString()))
                    {
                        if (marketPlace.TIDAK_HIT_UANG_R == true)
                        {
                            if (!string.IsNullOrEmpty(marketPlace.Sort1_Cust))
                            {
                                if (!string.IsNullOrEmpty(stf02h.BRG_MP))
                                {
                                    E2CartAPIData data = new E2CartAPIData()
                                    {
                                        no_cust = marketPlace.CUST,
                                        account_store = marketPlace.PERSO,
                                        API_key = marketPlace.API_KEY,
                                        API_credential = marketPlace.Sort1_Cust,
                                        API_url = marketPlace.PERSO,
                                        DatabasePathErasoft = dbPathEra
                                    };
                                    if (stf02h.BRG_MP.Contains("PENDING") || stf02h.BRG_MP.Contains("PEDITENDING"))
                                    {

                                    }
                                    else
                                    {
#if (DEBUG || Debug_AWS)
                                        Task.Run(() => E2Cart_UpdateStock_82Cart(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname)).Wait();
                                        //E2Cart_UpdateStock_82Cart(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname);
#else
                                        client.Enqueue<StokControllerJob>(x => x.E2Cart_UpdateStock_82Cart(DatabasePathErasoft, stf02h.BRG, marketPlace.CUST, "Stock", "Update Stok", data, stf02h.BRG_MP, 0, uname));
#endif
                                    }
                                }
                            }
                        }
                    }

                }
            }
            EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "DELETE FROM TEMP_ALL_MP_ORDER_ITEM WHERE CONN_ID = '" + connId + "'");
            //}
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_update_stok")]
        [NotifyOnFailed("Update Stok {obj} ke Bukalapak gagal.")]
        public void Bukalapak_updateStock(string DatabasePathErasoft, string brg, string log_CUST, string log_ActionCategory, string log_ActionName, string brgMp, string price, string stock, string userId, string token, string uname, PerformContext context)
        {
            SetupContext(DatabasePathErasoft, uname);
            var MoDbContext = new MoDbContext("");
            var EDB = new DatabaseSQL(DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            var ErasoftDbContext = new ErasoftContext(EraServerName, DatabasePathErasoft);

            var qtyOnHand = GetQOHSTF08A(brg, "ALL");
            //add by calvin 17 juni 2019
            if (qtyOnHand < 0)
            {
                qtyOnHand = 0;
            }
            //end add by calvin 17 juni 2019

            stock = (qtyOnHand > 0) ? qtyOnHand.ToString() : "0";
            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
            //    REQUEST_ACTION = "Update Price/Stock Product",
            //    REQUEST_DATETIME = DateTime.Now,
            //    REQUEST_ATTRIBUTE_1 = brg,
            //    REQUEST_ATTRIBUTE_2 = price,
            //    REQUEST_ATTRIBUTE_3 = stock,
            //    REQUEST_ATTRIBUTE_4 = token,
            //    REQUEST_STATUS = "Pending",
            //};
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, userId, currentLog);

            string Myprod = "{\"product\": {";
            if (!string.IsNullOrEmpty(price))
            {
                Myprod += "\"price\":\"" + price + "\"";
            }
            if (!string.IsNullOrEmpty(price) && !string.IsNullOrEmpty(stock))
                Myprod += ",";
            if (!string.IsNullOrEmpty(stock))
            {
                Myprod += "\"stock\":\"" + stock + "\"";
            }
            Myprod += "}}";
            Utils.HttpRequest req = new Utils.HttpRequest();
            var ret = req.CallBukaLapakAPI("PUT", "products/" + brgMp + ".json", Myprod, userId, token, typeof(CreateProductBukaLapak)) as CreateProductBukaLapak;
            if (ret != null)
            {
                if (ret.status.ToString().Equals("OK"))
                {
                    //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, userId, currentLog);
                    if (!string.IsNullOrEmpty(stock))
                    {
                        //jika stok di bukalapak 0, di bukalapak akan menjadi non display, MO disamakan
                        if (Convert.ToDouble(stock) == 0)
                        {
                            var arf01Bukalapak = ErasoftDbContext.ARF01.Where(p => p.NAMA == "8").ToList();
                            foreach (var akun in arf01Bukalapak)
                            {
                                string sSQL = "UPDATE STF02H SET DISPLAY = '0' WHERE IDMARKET = '" + Convert.ToString(akun.RecNum) + "' AND BRG = '" + brg + "'";
                                var a = EDB.ExecuteSQL(sSQL, CommandType.Text, sSQL);
                                if (a <= 0)
                                {

                                }
                            }
                        }
                    }
                    //end add by calvin 8 nov 2018
                }
                else
                {
                    ret.message = ret.message;
                    //currentLog.REQUEST_EXCEPTION = ret.message;
                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
                }
            }
            else
            {
                ret = new CreateProductBukaLapak();
                ret.message = "Failed to call Buka Lapak API";
                //currentLog.REQUEST_EXCEPTION = "Failed to call Buka Lapak API";
                //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
            }
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_update_stok")]
        [NotifyOnFailed("Update Stok {obj} ke Lazada gagal.")]
        public BindingBase Lazada_updateStock(string DatabasePathErasoft, string stf02_brg, string log_CUST, string log_ActionCategory, string log_ActionName, string kdBrg, string harga, string qty, string token, string uname, PerformContext context)
        {
            SetupContext(DatabasePathErasoft, uname);
            var MoDbContext = new MoDbContext("");
            var EDB = new DatabaseSQL(DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            var ErasoftDbContext = new ErasoftContext(EraServerName, DatabasePathErasoft);

            var dsArf01 = EDB.GetDataSet("sConn", "ARF01", "SELECT STATUS_API FROM ARF01 WHERE CUST='" + log_CUST + "'");
            if (dsArf01.Tables[0].Rows.Count > 0)
            {
                if (Convert.ToString(dsArf01.Tables[0].Rows[0]["STATUS_API"]) == "2")
                {
                    throw new Exception("Link ke marketplace Lazada Expired. lakukan Link Ulang di menu Link ke Marketplace.");
                }
            }

            string urlLazada = "https://api.lazada.co.id/rest";
            string eraAppKey = "101775";
            string eraAppSecret = "QwUJjjtZ3eCy2qaz6Rv1PEXPyPaPkDSu";

            var qtyOnHand = GetQOHSTF08A(stf02_brg, "ALL");

            //add by calvin 17 juni 2019
            if (qtyOnHand < 0)
            {
                qtyOnHand = 0;
            }
            //end add by calvin 17 juni 2019
            qty = (qtyOnHand > 0) ? qtyOnHand.ToString() : "0";

            var ret = new BindingBase();
            ret.status = 0;

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
            //    REQUEST_ACTION = "Update Price/Stok Product",
            //    REQUEST_DATETIME = DateTime.Now,
            //    REQUEST_ATTRIBUTE_1 = kdBrg,
            //    REQUEST_ATTRIBUTE_2 = harga,
            //    REQUEST_ATTRIBUTE_3 = qty,
            //    REQUEST_STATUS = "Pending",
            //};
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, token, currentLog);

            if (string.IsNullOrEmpty(kdBrg))
            {
                ret.message = "Item not linked to MP";
                //currentLog.REQUEST_EXCEPTION = ret.message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, token, currentLog);
                return ret;
            }

            //add 12-04-2019, cek qty on lazada
            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            //remark 22 juli 2020, ubah cara cek stok tertahan lazada
            //request.SetApiName("/product/item/get");
            //request.SetHttpMethod("GET");
            //request.AddApiParameter("seller_sku", kdBrg);
            //LazopResponse response = client.Execute(request, token);
            //dynamic resStok = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body);
            //if (resStok.code == "0")
            //{
            //    int stok = Convert.ToInt32(resStok.data.skus[0].quantity);
            //    int stokAvaliable = Convert.ToInt32(resStok.data.skus[0].Available);
            //    qty = (Convert.ToInt32(qty) + (stok - stokAvaliable)).ToString();
            //}
            //end remark 22 juli 2020, ubah cara cek stok tertahan lazada
            //end add 12-04-2019, cek qty on lazada
            #region cek stok tertahan lazada
            //remark 10 Aug 2020, perhitungan stok tertahan dan terpakai di seller center lazada tidak stabil
            //string sSQL = "SELECT DISTINCT NO_REFERENSI FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI ";
            //sSQL += "WHERE A.CUST = '"+log_CUST+"' AND A.TGL >= '"+DateTime.UtcNow.AddHours(7).AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss") + "' ";
            //sSQL += "AND A.STATUS_TRANSAKSI IN ('0','01','02') AND ISNULL(convert(nvarchar(max),KET_DETAIL), '') <> 'NO_COUNT_LZD' AND B.BRG = '" + stf02_brg+ "' AND ISNULL(NO_REFERENSI, '') <> ''";
            //var dsPesanan = EDB.GetDataSet("CString", "SO", sSQL);
            //if(dsPesanan.Tables[0].Rows.Count > 0)
            //{
            //    List<string> listID = new List<string>();
            //    string listNoRef = "[";
            //    for(int i = 0; i < dsPesanan.Tables[0].Rows.Count; i++)
            //    {
            //        listNoRef += dsPesanan.Tables[0].Rows[i]["NO_REFERENSI"].ToString();
            //        if ((i + 1) % 100 == 0)
            //        {
            //            listNoRef += "]";
            //            listID.Add(listNoRef);
            //            listNoRef = "[";
            //        }
            //        else
            //        {
            //            listNoRef += ",";
            //        }
            //    }
            //    if (!string.IsNullOrEmpty(listNoRef))
            //    {
            //        listNoRef = listNoRef.Substring(0, listNoRef.Length - 1) + "]";
            //        listID.Add(listNoRef);
            //    }
            //    var resStok = getOrderStatusLazada(listID, token, kdBrg, stf02_brg, log_CUST, dbPathEra);
            //    if(resStok.status == 1)
            //    {
            //        qty = Convert.ToString(Convert.ToInt32(qty) + resStok.recordCount);
            //    }
            //}
            //end remark 10 Aug 2020, perhitungan stok tertahan dan terpakai di seller center lazada tidak stabil
            #endregion
            string xmlString = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?><Request><Product>";
            //change 16 apr 2020
            //xmlString += "<Skus><Sku><SellerSku>" + kdBrg + "</SellerSku>";
            xmlString += "<Skus><Sku><SellerSku>" + XmlEscape(kdBrg) + "</SellerSku>";
            //end change 16 apr 2020
            if (!string.IsNullOrEmpty(qty))
                xmlString += "<Quantity>" + qty + "</Quantity>";
            if (!string.IsNullOrEmpty(harga))
                xmlString += "<Price>" + harga + "</Price>";
            xmlString += "</Sku></Skus></Product></Request>";

            //ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            //LazopRequest request = new LazopRequest();
            request.SetApiName("/product/price_quantity/update");
            request.AddApiParameter("payload", xmlString);
            request.SetHttpMethod("POST");
            try
            {
                LazopResponse response = client.Execute(request, token);
                var res = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaResponseObj)) as LazadaResponseObj;
                if (res.code.Equals("0"))
                {
                    ret.status = 1;
                    //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, token, currentLog);


                }
                else
                {
                    if (res.detail != null)
                    {
                        ret.message = res.detail[0].message;
                    }
                    else
                    {
                        ret.message = res.message;
                    }
                    throw new Exception(ret.message);
                    //currentLog.REQUEST_EXCEPTION = res.detail[0].message;
                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, token, currentLog);
                }
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                //ret.message = ex.ToString();
                throw new Exception(msg);

                //currentLog.REQUEST_EXCEPTION = ex.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, token, currentLog);
            }

            return ret;
        }
        public static string XmlEscape(string unescaped)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode node = doc.CreateElement("root");
            node.InnerText = unescaped;
            return node.InnerXml;
        }
        #region cek stok tertahan lazada
        public BindingBase getOrderStatusLazada(List<string> listID, string accessToken, string sellerSku, string brg, string cust, string dbPathEra)
        {
            var ret = new BindingBase();
            ret.status = 0;

            string urlLazada = "https://api.lazada.co.id/rest";
            string eraAppKey = "101775";
            string eraAppSecret = "QwUJjjtZ3eCy2qaz6Rv1PEXPyPaPkDSu";

            //var MoDbContext = new MoDbContext("");
            var EDB = new DatabaseSQL(dbPathEra);
            //string EraServerName = EDB.GetServerName("sConn");
            //var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);
            try
            {
                foreach (var listOrderIds in listID)
                {
                    string listOrderItemID = "";
                    ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
                    LazopRequest request = new LazopRequest();
                    request.SetApiName("/orders/items/get");
                    request.SetHttpMethod("GET");
                    request.AddApiParameter("order_ids", listOrderIds);

                    LazopResponse response = client.Execute(request, accessToken);

                    var bindOrderItems = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaOrderItems)) as LazadaOrderItems;
                    if (bindOrderItems != null)
                    {
                        if (bindOrderItems.code.Equals("0"))
                        {
                            if (bindOrderItems.data.Count > 0)
                            {
                                ret.status = 1;

                                foreach (Datum order in bindOrderItems.data)
                                {
                                    if (order.order_items.Count() > 0)
                                    {
                                        //var connectionID = Guid.NewGuid().ToString();

                                        foreach (Order_Items items in order.order_items)
                                        {
                                            if (items.sku == sellerSku)
                                            {
                                                if (items.status == "pending" || items.status == "unpaid")//stok tertahan
                                                {
                                                    ret.recordCount++;
                                                }
                                                else//sudah di proses
                                                {
                                                    ret.totalData++;
                                                    listOrderItemID += "'" + items.order_item_id + "',";
                                                }
                                            }
                                        }

                                    }
                                }
                            }
                            else
                            {
                                ret.message = "no item";
                            }
                        }
                        else
                        {
                            //currentLog.REQUEST_EXCEPTION = bindOrderItems.message;
                            //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, accessToken, currentLog); ret.message = "lazada api return error";
                            if (!string.IsNullOrEmpty(bindOrderItems.message))
                                ret.message += "\n" + bindOrderItems.message;
                        }
                    }
                    else
                    {
                        ret.message = "failed to call lazada api";
                    }
                    if (!string.IsNullOrEmpty(listOrderItemID))//ada yg di update karena tidak perlu di cek lagi
                    {
                        listOrderItemID = listOrderItemID.Substring(0, listOrderItemID.Length - 1);
                        var sSQL = "UPDATE B SET KET_DETAIL = 'NO_COUNT_LZD' ";
                        sSQL += "FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI ";
                        sSQL += "WHERE A.CUST = '" + cust + "' AND B.ORDER_ITEM_ID IN (" + listOrderItemID + ") AND B.BRG = '" + brg + "'";
                        var result = EDB.ExecuteSQL("CString", CommandType.Text, sSQL);
                    }
                }


            }
            catch (Exception ex)
            {
                ret.status = 0;
            }

            return ret;
        }

        #endregion
        [AutomaticRetry(Attempts = 3)]
        [Queue("1_update_stok")]
        [NotifyOnFailed("Update Stok {obj} ke Elevenia gagal.")]
        public ClientMessage Elevenia_updateStock(string DatabasePathErasoft, string stf02_brg, string log_CUST, string log_ActionCategory, string log_ActionName, EleveniaProductData data, string uname, PerformContext context)
        {
            SetupContext(DatabasePathErasoft, uname);
            var MoDbContext = new MoDbContext("");
            var EDB = new DatabaseSQL(DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            var ErasoftDbContext = new ErasoftContext(EraServerName, DatabasePathErasoft);

            var qtyOnHand = GetQOHSTF08A(stf02_brg, "ALL");
            //add by calvin 17 juni 2019
            if (qtyOnHand < 0)
            {
                qtyOnHand = 0;
            }
            //end add by calvin 17 juni 2019

            string stock = (qtyOnHand > 0) ? qtyOnHand.ToString() : "0";
            data.Qty = stock;

            var ret = new ClientMessage();
            string auth = data.api_key;

            Utils.HttpRequest req = new Utils.HttpRequest();
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            // Jan1st1970.AddMilliseconds(Convert.ToDouble(milis)).AddHours(7);
            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = milis.ToString(),
            //    REQUEST_ACTION = "Update QOH",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = data.kode,
            //    REQUEST_ATTRIBUTE_2 = data.nama,
            //    REQUEST_ATTRIBUTE_3 = data.kode_mp,
            //    REQUEST_STATUS = "Pending",
            //};
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data.api_key, currentLog);

            string xmlString = "<Product>";
            xmlString += "<selMnbdNckNm><![CDATA[" + data.nama + "]]></selMnbdNckNm>";//nickname
            xmlString += "<selMthdCd>01</selMthdCd>";//sales type : 01 = ready stok ; 04 = preorder ; 05 = used item

            //string sSQL = "SELECT * FROM (";
            //for (int i = 1; i <= 30; i++)
            //{
            //    sSQL += "SELECT A.ACODE_" + i.ToString() + " AS ATTRIBUTE_CODE,A.ANAME_" + i.ToString() + " AS ATTRIBUTE_NAME,B.ATYPE_" + i.ToString() + " AS ATTRIBUTE_ID,A.AVALUE_" + i.ToString() + " AS VALUE FROM STF02H A INNER JOIN MO.DBO.ATTRIBUTE_ELEVENIA B ON A.CATEGORY_CODE = B.CATEGORY_CODE WHERE A.BRG='" + data.kode + "' AND A.IDMARKET = '" + data.IDMarket + "' " + System.Environment.NewLine;
            //    if (i < 30)
            //    {
            //        sSQL += "UNION ALL " + System.Environment.NewLine;
            //    }
            //}
            //DataSet dsAttribute = EDB.GetDataSet("sCon", "STF02H", sSQL + ") ASD WHERE ISNULL(ATTRIBUTE_CODE,'') <> ''");
            //int data_idmarket = Convert.ToInt32(data.IDMarket);
            //var nilaiStf02h = (from p in ErasoftDbContext.STF02H where p.BRG == data.kode && p.IDMARKET == data_idmarket select p).FirstOrDefault();
            //xmlString += "<dispCtgrNo>" + nilaiStf02h.CATEGORY_CODE + "</dispCtgrNo>";//category id //5475 = Hobi lain lain

            //for (int i = 0; i < dsAttribute.Tables[0].Rows.Count; i++)
            //{
            //    xmlString += "<ProductCtgrAttribute><prdAttrCd><![CDATA[" + Convert.ToString(dsAttribute.Tables[0].Rows[i]["ATTRIBUTE_CODE"]) + "]]></prdAttrCd>";//category attribute code
            //    xmlString += "<prdAttrNm><![CDATA[" + Convert.ToString(dsAttribute.Tables[0].Rows[i]["ATTRIBUTE_NAME"]) + "]]></prdAttrNm>";//category attribute name i.e: brand, model, type, ISBN
            //    xmlString += "<prdAttrNo><![CDATA[" + Convert.ToString(dsAttribute.Tables[0].Rows[i]["ATTRIBUTE_ID"]) + "]]></prdAttrNo>";//category attribute id
            //    xmlString += "<prdAttrVal><![CDATA[" + Convert.ToString(dsAttribute.Tables[0].Rows[i]["VALUE"]) + "]]></prdAttrVal></ProductCtgrAttribute>";//category attribute value
            //}
            var stf02h = ErasoftDbContext.STF02H.Where(p => p.BRG == data.kode && p.IDMARKET.ToString() == data.IDMarket).FirstOrDefault();

            //List<string> dsNormal = new List<string>();
            Dictionary<string, string> listAttr = new Dictionary<string, string>();

            var attributeEl = new EleveniaControllerJob().GetAttributeByCategory(auth, stf02h.CATEGORY_CODE);
            for (int i = 1; i <= 30; i++)
            {
                string attribute_code = Convert.ToString(attributeEl["ACODE_" + i.ToString()]);
                string attribute_id = Convert.ToString(attributeEl["ATYPE_" + i.ToString()]);
                string attribute_name = Convert.ToString(attributeEl["ANAME_" + i.ToString()]);
                if (!string.IsNullOrWhiteSpace(attribute_code))
                {
                    listAttr.Add(attribute_code, attribute_id + "[;]" + attribute_name);
                }
            }

            Dictionary<string, string> elAttrWithVal = new Dictionary<string, string>();
            for (int i = 1; i <= 30; i++)
            {
                string attribute_id = Convert.ToString(stf02h["ACODE_" + i.ToString()]);
                string value = Convert.ToString(stf02h["AVALUE_" + i.ToString()]);
                if (!string.IsNullOrWhiteSpace(value) && value != "null")
                {
                    if (listAttr.ContainsKey(attribute_id))
                    {
                        if (!elAttrWithVal.ContainsKey(attribute_id))
                        {
                            //var sVar = listAttr[attribute_id].Split(new string[] { "[;]" }, StringSplitOptions.None);
                            elAttrWithVal.Add(attribute_id + "[;]" + listAttr[attribute_id], value.Trim());
                        }
                    }
                }
            }
            xmlString += "<dispCtgrNo>" + stf02h.CATEGORY_CODE + "</dispCtgrNo>";

            foreach (var elSkuAttr in elAttrWithVal)
            {
                var sKey = elSkuAttr.Key.Split(new string[] { "[;]" }, StringSplitOptions.None);
                xmlString += "<ProductCtgrAttribute><prdAttrCd><![CDATA[" + sKey[0] + "]]></prdAttrCd>";//category attribute code
                xmlString += "<prdAttrNm><![CDATA[" + sKey[2] + "]]></prdAttrNm>";//category attribute name i.e: brand, model, type, ISBN
                xmlString += "<prdAttrNo><![CDATA[" + sKey[1] + "]]></prdAttrNo>";//category attribute id
                xmlString += "<prdAttrVal><![CDATA[" + elSkuAttr.Value + "]]></prdAttrVal></ProductCtgrAttribute>";//category attribute value
            }

            xmlString += "<prdNm><![CDATA[" + data.nama + "]]></prdNm>";//product name
            xmlString += "<prdStatCd>01</prdStatCd>";//item condition : 01 = new ; 02 = used
            xmlString += "<prdWght>" + data.berat + "</prdWght>";//weight in kg
            xmlString += "<dlvGrntYn>N</dlvGrntYn>";//guarantee of delivery Y/N value
            xmlString += "<minorSelCnYn>Y</minorSelCnYn>";//minor(under 17 years old) can buy
            xmlString += "<suplDtyfrPrdClfCd>02</suplDtyfrPrdClfCd>";//VAT : 01 = item with tax ; 02 = item without tax

            int prodImageCount = 1;
            for (int i = 0; i < data.imgUrl.Length; i++)
            {
                if (data.imgUrl[i] != null)
                {
                    xmlString += "<prdImage0" + Convert.ToString(prodImageCount) + "><![CDATA[" + data.imgUrl[i] + "]]></prdImage0" + Convert.ToString(prodImageCount) + ">";//image url (can use up to 5 image)

                    prodImageCount++;
                }
                else if (i == 0)
                {
                    xmlString += "<prdImage01><![CDATA[https://masteronline.co.id/ele/image/photo_not_available]]></prdImage01>";//image url (can use up to 5 image)
                    prodImageCount++;
                }
            }

            xmlString += "<htmlDetail><![CDATA[" + data.Keterangan + "]]></htmlDetail>";//item detail(html supported)
            xmlString += "<sellerPrdCd><![CDATA[" + data.kode + "]]></sellerPrdCd>";//seller sku(optional)
            xmlString += "<selTermUseYn>N</selTermUseYn>";//whether to use sales period Y/N value
            //xmlString += "<wrhsPlnDy></wrhsPlnDy>";//due date of stock(optional)
            xmlString += "<selPrc>" + data.Price + "</selPrc>";//product price
            xmlString += "<prdSelQty>" + data.Qty + "</prdSelQty>";//product stock
            //xmlString += "<dscAmtPercnt></dscAmtPercnt>";//discount value(optional)
            //xmlString += "<cupnDscMthdCd></cupnDscMthdCd>";//discount unit(optional) : 01 = in Rp ; 02 = in %
            //xmlString += "<cupnIssEndDy></cupnIssEndDy>";//discount end date(optional)
            xmlString += "<tmpltSeq>" + data.DeliveryTempNo + "</tmpltSeq>";//number of delivery
            xmlString += "<asDetail></asDetail>";//after service information(garansi)
            xmlString += "<rtngExchDetail>Hubungi toko untuk retur</rtngExchDetail>";//return/exchange information
            xmlString += "<prdNo>" + data.kode_mp + "</prdNo>";//Marketplace Product ID
            xmlString += "</Product>";

            ClientMessage result = req.CallElevAPI(Utils.HttpRequest.PROTOCOL.Http, Utils.HttpRequest.RESTServices.rest, Utils.HttpRequest.METHOD.POST, "prodservices/product", xmlString, typeof(ClientMessage), auth) as ClientMessage;
            if (result != null)
            {
                if (Convert.ToString(result.resultCode).Equals("200"))
                {
                    //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data.api_key, currentLog);
                }
                else
                {
                    if (Convert.ToString(result.resultCode).Contains("Ex;"))
                    {
                        //if (result.resultCode.Split(';').Count() > 1)
                        //{
                        //    currentLog.REQUEST_RESULT = result.resultCode.Split(';')[1];
                        //}
                        //currentLog.REQUEST_EXCEPTION = result.Message;
                        //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data.api_key, currentLog);
                    }
                    else
                    {
                        //currentLog.REQUEST_RESULT = result.Message;
                        //currentLog.REQUEST_RESULT = string.IsNullOrEmpty(result.Message) ? result.message : result.Message;
                        //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data.api_key, currentLog);
                    }
                }
            }

            return ret;
        }

        public async Task<int> BlibliCheckUpdateStock(BlibliAPIData iden, BlibliProductData data, string skuUpdate)
        {
            int newQty = -1;

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string signature_1 = CreateTokenBlibli("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/detailProduct", iden.API_secret_key);

            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

            //add by nurul 13/7/2020
            string urll_1 = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/detailProduct?";
            HttpWebRequest myReq_1 = (HttpWebRequest)WebRequest.Create(urll_1);
            //end add by nurul 13/7/2020

            //change by nurul 13/7/2020
            if (iden.versiToken != "2")
            {
                urll_1 = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/detailProduct?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&gdnSku=" + Uri.EscapeDataString(skuUpdate) + "&channelId=MasterOnline";

                myReq_1 = (HttpWebRequest)WebRequest.Create(urll_1);
                myReq_1.Method = "GET";
                myReq_1.Headers.Add("Authorization", ("bearer " + iden.token));
                myReq_1.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature_1));
                myReq_1.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq_1.Accept = "application/json";
                myReq_1.ContentType = "application/json";
                myReq_1.Headers.Add("requestId", "MasterOnline-" + milis.ToString());
                myReq_1.Headers.Add("sessionId", milis.ToString());
                myReq_1.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = iden.API_client_username;
                string passMO = iden.API_client_password;
                urll_1 = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/detailProduct?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&gdnSku=" + Uri.EscapeDataString(skuUpdate) + "&channelId=MasterOnline";

                myReq_1 = (HttpWebRequest)WebRequest.Create(urll_1);
                myReq_1.Method = "GET";
                myReq_1.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq_1.Accept = "application/json";
                myReq_1.ContentType = "application/json";
                myReq_1.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                myReq_1.Headers.Add("Signature-Time", milis.ToString());
            }
            //end change by nurul 13/7/2020
            string responseFromServer_1 = "";

            using (WebResponse response = await myReq_1.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseFromServer_1 = reader.ReadToEnd();
                }
            }

            if (responseFromServer_1 != null)
            {
                BlibliDetailProductResult result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer_1, typeof(BlibliDetailProductResult)) as BlibliDetailProductResult;
                if (string.IsNullOrEmpty(Convert.ToString(result.errorCode)))
                {
                    if (result.value.items.Count() > 0)
                    {
                        newQty = result.value.items[0].availableStockLevel2;
                    }
                }
            }

            return newQty;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_update_stok")]
        [NotifyOnFailed("Update Stok {obj} ke Blibli gagal.")]
        public async Task<string> Blibli_updateStock(string DatabasePathErasoft, string stf02_brg, string log_CUST, string log_ActionCategory, string log_ActionName, BlibliAPIData iden, BlibliProductData data, string uname, PerformContext context)
        {
            string ret = "";

            string newToken = SetupContextBlibli(DatabasePathErasoft, uname, iden);
            iden.token = newToken;

            var EDB = new DatabaseSQL(DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");

            var qtyOnHand = GetQOHSTF08A(stf02_brg, "ALL");

            //add by calvin 17 juni 2019
            if (qtyOnHand < 0)
            {
                qtyOnHand = 0;
            }
            //end add by calvin 17 juni 2019

            string stock = (qtyOnHand > 0) ? qtyOnHand.ToString() : "0";
            data.Qty = Convert.ToString(stock);

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant


            #region Get Product List ( untuk dapatkan QOH di Blibi )
            double QOHBlibli = 0;
            string signature_1 = CreateTokenBlibli("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/detailProduct", iden.API_secret_key);
            string skuUpdate = data.kode_mp;

            string[] brg_mp = data.kode_mp.Split(';');
            if (brg_mp.Length == 2)
            {
                skuUpdate = brg_mp[0];
            }
            bool allowUpdate = true;
            if (skuUpdate.Contains("PENDING") || skuUpdate.Contains("PEDITENDING") || skuUpdate.Contains("NEED_CORRECTION"))
            {
                allowUpdate = false;
            }

            if (allowUpdate)
            {
                //add by nurul 13/7/2020
                string urll_1 = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/detailProduct?";
                HttpWebRequest myReq_1 = (HttpWebRequest)WebRequest.Create(urll_1);
                //end add by nurul 13/7/2020

                //change by nurul 13/7/2020
                if (iden.versiToken != "2")
                {
                    urll_1 = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/detailProduct?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&gdnSku=" + Uri.EscapeDataString(skuUpdate) + "&channelId=MasterOnline";

                    myReq_1 = (HttpWebRequest)WebRequest.Create(urll_1);
                    myReq_1.Method = "GET";
                    myReq_1.Headers.Add("Authorization", ("bearer " + iden.token));
                    myReq_1.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature_1));
                    myReq_1.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                    myReq_1.Accept = "application/json";
                    myReq_1.ContentType = "application/json";
                    myReq_1.Headers.Add("requestId", "MasterOnline-" + milis.ToString());
                    myReq_1.Headers.Add("sessionId", milis.ToString());
                    myReq_1.Headers.Add("username", userMTA);
                }
                else
                {
                    string usernameMO = iden.API_client_username;
                    string passMO = iden.API_client_password;
                    urll_1 = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/detailProduct?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&gdnSku=" + Uri.EscapeDataString(skuUpdate) + "&channelId=MasterOnline";

                    myReq_1 = (HttpWebRequest)WebRequest.Create(urll_1);
                    myReq_1.Method = "GET";
                    myReq_1.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                    myReq_1.Accept = "application/json";
                    myReq_1.ContentType = "application/json";
                    myReq_1.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                    myReq_1.Headers.Add("Signature-Time", milis.ToString());
                }
                //end change by nurul 13/7/2020
                string responseFromServer_1 = "";

                using (WebResponse response = await myReq_1.GetResponseAsync())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseFromServer_1 = reader.ReadToEnd();
                    }
                }

                if (responseFromServer_1 != null)
                {
                    BlibliDetailProductResult result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer_1, typeof(BlibliDetailProductResult)) as BlibliDetailProductResult;
                    if (string.IsNullOrEmpty(Convert.ToString(result.errorCode)))
                    {
                        if (result.value.items.Count() > 0)
                        {
                            string myData = "{";
                            myData += "\"merchantCode\": \"" + iden.merchant_code + "\", ";
                            myData += "\"productRequests\": ";
                            myData += "[ ";  //MERCHANT ID ADA DI https://merchant.blibli.com/MTA/store-info/store-info
                            {
                                if (result.value.items.Count() > 0)
                                {
                                    QOHBlibli = result.value.items[0].availableStockLevel2;
                                    if (Convert.ToInt32(data.Qty) - QOHBlibli != 0) // tidak sama
                                    {
                                        QOHBlibli = Convert.ToInt32(data.Qty) - QOHBlibli;
                                    }
                                    else
                                    {
                                        QOHBlibli = 0;
                                    }
                                    {
                                        myData += "{";
                                        myData += "\"gdnSku\": \"" + skuUpdate + "\",  ";
                                        myData += "\"stock\": " + Convert.ToString(QOHBlibli) + ", ";
                                        myData += "\"minimumStock\": " + data.MinQty + ", ";
                                        //change by Tri 30 Jan 2020, harga dan harga promo ikut harga di blibli saja karena function ini untuk update stok
                                        //myData += "\"price\": " + data.Price + ", ";
                                        //myData += "\"salePrice\": " + data.MarketPrice + ", ";// harga yg tercantum di display blibli
                                        //myData += "\"salePrice\": " + item.sellingPrice + ", ";// harga yg promo di blibli
                                        myData += "\"price\": " + result.value.items[0].prices[0].price + ", ";
                                        myData += "\"salePrice\": " + result.value.items[0].prices[0].salePrice + ", ";
                                        //end change by Tri 30 Jan 2020, harga dan harga promo ikut harga di blibli saja karena function ini untuk update stok
                                        myData += "\"buyable\": " + data.display + ", ";
                                        myData += "\"displayable\": " + data.display + " "; // true=tampil    
                                        myData += "},";
                                    }
                                }
                            }
                            myData = myData.Remove(myData.Length - 1);
                            myData += "]";
                            myData += "}";

                            //add by nurul 13/7/2020
                            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/updateProduct?";
                            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                            //end add by nurul 13/7/2020

                            //change by nurul 13/7/2020
                            if (iden.versiToken != "2")
                            {
                                string signature = CreateTokenBlibli("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/updateProduct", iden.API_secret_key);
                                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/updateProduct?channelId=MasterOnline";

                                myReq = (HttpWebRequest)WebRequest.Create(urll);
                                myReq.Method = "POST";
                                myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                                myReq.Accept = "application/json";
                                myReq.ContentType = "application/json";
                                myReq.Headers.Add("requestId", "MasterOnline-" + milis.ToString());
                                myReq.Headers.Add("sessionId", milis.ToString());
                                myReq.Headers.Add("username", userMTA);
                            }
                            else
                            {
                                string usernameMO = iden.API_client_username;
                                string passMO = iden.API_client_password;
                                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/updateProduct?channelId=MasterOnline";

                                myReq = (HttpWebRequest)WebRequest.Create(urll);
                                myReq.Method = "POST";
                                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                                myReq.Accept = "application/json";
                                myReq.ContentType = "application/json";
                                myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                                myReq.Headers.Add("Signature-Time", milis.ToString());
                            }
                            //end change by nurul 13/7/2020
                            string responseFromServer = "";

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
                            if (responseFromServer != null)
                            {
                                dynamic result2 = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                                //add by Tri 31 jan 2019
                                MasterOnline.API_LOG_MARKETPLACE saveQueID = new API_LOG_MARKETPLACE
                                {
                                    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                    REQUEST_ACTION = "Selisih Stok",
                                    REQUEST_DATETIME = DateTime.Now,
                                    REQUEST_ATTRIBUTE_1 = stf02_brg,
                                    REQUEST_ATTRIBUTE_2 = "MO Stock : " + Convert.ToString(data.Qty), //updating to stock
                                    REQUEST_ATTRIBUTE_3 = Convert.ToString(result2.requestId), //requestid
                                    REQUEST_STATUS = "Pending",
                                };
                                var ErasoftDbContext2 = new ErasoftContext(EraServerName, dbPathEra);
                                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext2, log_CUST, saveQueID, "Blibli");
                                //end add by Tri 31 jan 2019
                                //add by calvin 28 oktober 2019
                                if (dbPathEra.ToLower() == "erasoft_100144" || dbPathEra.ToLower() == "erasoft_120149" || dbPathEra.ToLower() == "erasoft_80069" || dbPathEra.ToUpper() == "ERASOFT_1000390")
                                {
                                    try
                                    {
                                        var a = await BlibliCheckUpdateStock(iden, data, skuUpdate);

                                        if (a < Convert.ToInt32(data.Qty) || a > Convert.ToInt32(data.Qty))
                                        {
                                            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                                            {
                                                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                                REQUEST_ACTION = "Selisih Stok",
                                                REQUEST_DATETIME = DateTime.Now,
                                                REQUEST_ATTRIBUTE_1 = stf02_brg,
                                                REQUEST_ATTRIBUTE_2 = "MO Stock : " + Convert.ToString(data.Qty), //updating to stock
                                                REQUEST_ATTRIBUTE_3 = "Blibli Stock : " + Convert.ToString(a), //marketplace stock
                                                REQUEST_STATUS = "Pending",
                                            };
                                            var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);
                                            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, log_CUST, currentLog, "Blibli");

                                            //#if (DEBUG || Debug_AWS)
                                            //                                Task.Run(() => Shopee_updateStock(DatabasePathErasoft, stf02_brg, log_CUST, "Stock", "Update Stok", iden, brg_mp, 0, uname, null)).Wait();
                                            //#else
                                            //                            var EDB = new DatabaseSQL(dbPathEra);
                                            //                            string EDBConnID = EDB.GetConnectionString("ConnId");
                                            //                            var sqlStorage = new SqlServerStorage(EDBConnID);
                                            //                            var client = new BackgroundJobClient(sqlStorage);
                                            //                            client.Enqueue<StokControllerJob>(x => x.Shopee_updateStock(DatabasePathErasoft, stf02_brg, log_CUST, "Stock", "Update Stok", iden, brg_mp, 0, uname, null));
                                            //#endif
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                                        MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                                        {
                                            REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                            REQUEST_ACTION = "Selisih Stok",
                                            REQUEST_DATETIME = DateTime.Now,
                                            REQUEST_ATTRIBUTE_1 = stf02_brg,
                                            REQUEST_ATTRIBUTE_2 = "MO Stock : " + Convert.ToString(data.Qty), //updating to stock
                                            REQUEST_ATTRIBUTE_3 = "Exception", //marketplace stock
                                            REQUEST_STATUS = "Pending",
                                            REQUEST_EXCEPTION = msg
                                        };
                                        var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);
                                        manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, log_CUST, currentLog, "Blibli");
                                    }
                                }
                                //end add by calvin 28 oktober 2019
                            }
                        }
                    }
                }
                #endregion
            }

            return ret;
        }

        public double Blibli_getReservedStockLv2(BlibliAPIData iden, string kode_mp)
        {
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

            double QOHBlibli = 0;
            string signature_1 = CreateTokenBlibli("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/detailProduct", iden.API_secret_key);
            string skuUpdate = kode_mp;

            string[] brg_mp = kode_mp.Split(';');
            if (brg_mp.Length == 2)
            {
                skuUpdate = brg_mp[0];
            }
            bool allowUpdate = true;
            if (skuUpdate.Contains("PENDING") || skuUpdate.Contains("PEDITENDING"))
            {
                allowUpdate = false;
            }

            if (allowUpdate)
            {
                //add by nurul 13/7/2020
                string urll_1 = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/detailProduct?";
                HttpWebRequest myReq_1 = (HttpWebRequest)WebRequest.Create(urll_1);
                //end add by nurul 13/7/2020

                //change by nurul 13/7/2020
                if (iden.versiToken != "2")
                {
                    urll_1 = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/detailProduct?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&gdnSku=" + Uri.EscapeDataString(skuUpdate) + "&channelId=MasterOnline";

                    myReq_1 = (HttpWebRequest)WebRequest.Create(urll_1);
                    myReq_1.Method = "GET";
                    myReq_1.Headers.Add("Authorization", ("bearer " + iden.token));
                    myReq_1.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature_1));
                    myReq_1.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                    myReq_1.Accept = "application/json";
                    myReq_1.ContentType = "application/json";
                    myReq_1.Headers.Add("requestId", "MasterOnline-" + milis.ToString());
                    myReq_1.Headers.Add("sessionId", milis.ToString());
                    myReq_1.Headers.Add("username", userMTA);
                }
                else
                {
                    string usernameMO = iden.API_client_username;
                    string passMO = iden.API_client_password;
                    urll_1 = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/detailProduct?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&gdnSku=" + Uri.EscapeDataString(skuUpdate) + "&channelId=MasterOnline";

                    myReq_1 = (HttpWebRequest)WebRequest.Create(urll_1);
                    myReq_1.Method = "GET";
                    myReq_1.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                    myReq_1.Accept = "application/json";
                    myReq_1.ContentType = "application/json";
                    myReq_1.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                    myReq_1.Headers.Add("Signature-Time", milis.ToString());
                }
                //end change by nurul 13/7/2020
                string responseFromServer_1 = "";

                using (WebResponse response = myReq_1.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseFromServer_1 = reader.ReadToEnd();
                    }
                }

                if (responseFromServer_1 != null)
                {
                    BlibliDetailProductResult result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer_1, typeof(BlibliDetailProductResult)) as BlibliDetailProductResult;
                    if (string.IsNullOrEmpty(Convert.ToString(result.errorCode)))
                    {
                        if (result.value.items.Count() > 0)
                        {
                            QOHBlibli = result.value.items[0].reservedStockLevel2;
                        }
                    }
                }
            }

            return QOHBlibli;
        }

        public async Task<int> TokpedCheckUpdateStock(TokopediaAPIData data, int product_id)
        {
            int newQty = -1;
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(data.merchant_code) + "/product/info?product_id=" + Uri.EscapeDataString(Convert.ToString(product_id));
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + data.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
            string responseFromServer = "";

            using (WebResponse response = await myReq.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseFromServer = reader.ReadToEnd();
                }
            }

            if (responseFromServer != "")
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokpedGetProductInfoRootobject)) as TokpedGetProductInfoRootobject;
                //change by Tri 15 apr 2020, message ada isi nya saat sukses
                //if (!string.IsNullOrWhiteSpace(result.header.messages))
                if (result != null)
                //if(result.header.error_code != 0)
                ////end change by Tri 15 apr 2020, message ada isi nya saat sukses
                //    {

                //    }
                //else
                {
                    var a = result.data.FirstOrDefault();
                    if (a != null)
                    {
                        newQty = a.stock.value;
                    }
                }
            }

            return newQty;
        }

        public class Tokped_updateStockResult
        {
            public Tokped_updateStockResultHeader header { get; set; }
            public Tokped_updateStockResultData data { get; set; }
        }

        public class Tokped_updateStockResultHeader
        {
            public float process_time { get; set; }
            public string messages { get; set; }
            public string reason { get; set; }
            //public int error_code { get; set; }
        }

        public class Tokped_updateStockResultData
        {
            public int failed_rows { get; set; }
            public failed_rows_data[] failed_rows_data { get; set; }
            public int succeed_rows { get; set; }
        }


        public class failed_rows_data
        {
            public int product_id { get; set; }
            public string sku { get; set; }
            public string product_url { get; set; }
            public int new_stock { get; set; }
            public string message { get; set; }
        }



        public class TokpedGetProductInfoRootobject
        {
            public TokpedGetProductInfoHeader header { get; set; }
            public TokpedGetProductInfoDatum[] data { get; set; }
        }

        public class TokpedGetProductInfoHeader
        {
            public float process_time { get; set; }
            public string messages { get; set; }
            public string reason { get; set; }
            //public int error_code { get; set; }
        }

        public class TokpedGetProductInfoDatum
        {
            public TokpedGetProductInfoBasic basic { get; set; }
            public TokpedGetProductInfoPrice price { get; set; }
            public TokpedGetProductInfoWeight weight { get; set; }
            public TokpedGetProductInfoStock stock { get; set; }
            public TokpedGetProductInfoVariant variant { get; set; }
            public TokpedGetProductInfoMenu menu { get; set; }
            public TokpedGetProductInfoPreorder preorder { get; set; }
            public TokpedGetProductInfoExtraattribute extraAttribute { get; set; }
            public TokpedGetProductInfoCategorytree[] categoryTree { get; set; }
            public TokpedGetProductInfoPicture[] pictures { get; set; }
            public TokpedGetProductInfoGmstats GMStats { get; set; }
            public TokpedGetProductInfoStats stats { get; set; }
            public TokpedGetProductInfoOther other { get; set; }
            public TokpedGetProductInfoCampaign campaign { get; set; }
            public TokpedGetProductInfoWarehouse[] warehouses { get; set; }
        }

        public class TokpedGetProductInfoBasic
        {
            public long productID { get; set; }
            public long shopID { get; set; }
            public int status { get; set; }
            public string name { get; set; }
            public int condition { get; set; }
            public long childCategoryID { get; set; }
        }

        public class TokpedGetProductInfoPrice
        {
            public long value { get; set; }
            public int currency { get; set; }
            public long LastUpdateUnix { get; set; }
            public long idr { get; set; }
        }

        public class TokpedGetProductInfoWeight
        {
            public double value { get; set; }
            public int unit { get; set; }
        }

        public class TokpedGetProductInfoStock
        {
            public bool useStock { get; set; }
            public int value { get; set; }
            public string stockWording { get; set; }
        }

        public class TokpedGetProductInfoVariant
        {
            public long parentID { get; set; }
            public bool isVariant { get; set; }
            public long[] childrenID { get; set; }
        }

        public class TokpedGetProductInfoMenu
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class TokpedGetProductInfoPreorder
        {
        }

        public class TokpedGetProductInfoExtraattribute
        {
            public int minOrder { get; set; }
            public long lastUpdateCategory { get; set; }
            public bool isEligibleCOD { get; set; }
        }

        public class TokpedGetProductInfoGmstats
        {
            public int transactionSuccess { get; set; }
            public int countSold { get; set; }
        }

        public class TokpedGetProductInfoStats
        {
            public int countView { get; set; }
            public int countReview { get; set; }
            public int rating { get; set; }
        }

        public class TokpedGetProductInfoOther
        {
            public string url { get; set; }
            public string mobileURL { get; set; }
        }

        public class TokpedGetProductInfoCampaign
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }

        public class TokpedGetProductInfoCategorytree
        {
            public long id { get; set; }
            public string name { get; set; }
            public string title { get; set; }
            public string breadcrumbURL { get; set; }
        }

        public class TokpedGetProductInfoPicture
        {
            public long picID { get; set; }
            public string fileName { get; set; }
            public string filePath { get; set; }
            public int status { get; set; }
            public string OriginalURL { get; set; }
            public string ThumbnailURL { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string URL300 { get; set; }
        }

        public class TokpedGetProductInfoWarehouse
        {
            public long productID { get; set; }
            public long warehouseID { get; set; }
            public TokpedGetProductInfoPrice1 price { get; set; }
            public TokpedGetProductInfoStock1 stock { get; set; }
        }

        public class TokpedGetProductInfoPrice1
        {
            public long value { get; set; }
            public int currency { get; set; }
            public long LastUpdateUnix { get; set; }
            public long idr { get; set; }
        }

        public class TokpedGetProductInfoStock1
        {
            public bool useStock { get; set; }
            public int value { get; set; }
        }


        [AutomaticRetry(Attempts = 3)]
        [Queue("1_update_stok")]
        [NotifyOnFailed("Update Stok {obj} ke Tokopedia gagal.")]
        public async Task<string> Tokped_updateStock(string DatabasePathErasoft, string stf02_brg, string log_CUST, string log_ActionCategory, string log_ActionName, TokopediaAPIData iden, int product_id, int stok, string uname, PerformContext context)
        {
            await Task.Delay(1000);
            var token = SetupContextTokopedia(DatabasePathErasoft, uname, iden);

            var EDB = new DatabaseSQL(DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");

            iden.token = token;
            if (!string.IsNullOrWhiteSpace(token))
            {
                var qtyOnHand = GetQOHSTF08A(stf02_brg, "ALL");
                //add by calvin 17 juni 2019
                if (qtyOnHand < 0)
                {
                    qtyOnHand = 0;
                }
                //end add by calvin 17 juni 2019
                stok = Convert.ToInt32(qtyOnHand);

                long milis = CurrentTimeMillis();
                DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
                string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/stock/update?shop_id=" + Uri.EscapeDataString(iden.API_secret_key);

                string responseFromServer = "";
                List<TokopediaUpdateStockData> HttpBodies = new List<TokopediaUpdateStockData>();
                TokopediaUpdateStockData HttpBody = new TokopediaUpdateStockData()
                {
                    sku = "",
                    product_id = product_id,
                    new_stock = stok
                };
                HttpBodies.Add(HttpBody);

                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "POST";
                myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                string myData = JsonConvert.SerializeObject(HttpBodies);
                //try
                //{
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

                if (responseFromServer != "")
                {

                    var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(Tokped_updateStockResult)) as Tokped_updateStockResult;

                    if (result.data != null)
                        if (result.data.failed_rows > 0 && result.data.succeed_rows == 0)
                        {
                            if (result.data.failed_rows_data.Length > 0)
                            {
                                var rowFailedMessage = "";
                                foreach (var itemRow in result.data.failed_rows_data)
                                {
                                    if (!string.IsNullOrEmpty(itemRow.message) && itemRow.product_id != 0)
                                    {
                                        rowFailedMessage = rowFailedMessage + Convert.ToString(itemRow.message) + " product id:" + Convert.ToString(itemRow.product_id) + ";";
                                    }
                                }
                                throw new Exception("failed_rows_data:" + rowFailedMessage);
                            }
                            else
                            {
                                throw new Exception("failed_rows:" + Convert.ToString(result.data.failed_rows));
                            }

                        }
                        else
                        {
                            try
                            {
                                //if (dbPathEra.ToLower() == "erasoft_100144" || dbPathEra.ToLower() == "erasoft_120149" || dbPathEra.ToLower() == "erasoft_80069" || dbPathEra.ToUpper() == "ERASOFT_1000390")
                                if (dbPathEra.ToLower() == "erasoft_100144" || dbPathEra.ToUpper() == "ERASOFT_1310644" || dbPathEra.ToUpper() == "ERASOFT_1000390")
                                {
                                    var a = await TokpedCheckUpdateStock(iden, product_id);
                                    if (a < stok || a > stok)
                                    {
                                        MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                                        {
                                            REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                            REQUEST_ACTION = "Selisih Stok",
                                            REQUEST_DATETIME = DateTime.Now,
                                            REQUEST_ATTRIBUTE_1 = stf02_brg,
                                            REQUEST_ATTRIBUTE_2 = "MO Stock : " + Convert.ToString(stok), //updating to stock
                                            REQUEST_ATTRIBUTE_3 = "Tokped Stock : " + Convert.ToString(a), //marketplace stock
                                            REQUEST_STATUS = "Pending",
                                        };
                                        var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);
                                        manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, log_CUST, currentLog, "Tokped");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                                MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                                {
                                    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                    REQUEST_ACTION = "Selisih Stok",
                                    REQUEST_DATETIME = DateTime.Now,
                                    REQUEST_ATTRIBUTE_1 = stf02_brg,
                                    REQUEST_ATTRIBUTE_2 = "MO Stock : " + Convert.ToString(stok), //updating to stock
                                    REQUEST_ATTRIBUTE_3 = "Exception", //marketplace stock
                                    REQUEST_STATUS = "Pending",
                                    REQUEST_EXCEPTION = msg
                                };
                                var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);
                                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, log_CUST, currentLog, "Tokped");
                            }
                        }
                }
            }

            //}
            //catch (Exception ex)
            //{

            //}
            return "";
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_update_stok")]
        [NotifyOnFailed("Update Stok {obj} ke 82Cart gagal.")]
        public async Task<string> E2Cart_UpdateStock_82Cart(string DatabasePathErasoft, string brg, string no_cust, string log_ActionCategory, string log_ActionName, E2CartAPIData iden, string brg_mp, int qty, string uname)
        {
            string ret = "";
            SetupContext(iden.DatabasePathErasoft, uname);
            //var EDB = new DatabaseSQL(DatabasePathErasoft);
            //string EraServerName = EDB.GetServerName("sConn");

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            //handle log activity
            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = milis.ToString(),
            //    REQUEST_ACTION = "Update Stock",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = "Kode Barang : " + brg,
            //    REQUEST_ATTRIBUTE_2 = "MO Stock : " + Convert.ToString(qty), //updating to stock
            //    REQUEST_STATUS = "Pending",
            //};
            //var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, no_cust, currentLog, "82Cart");
            //handle log activity

            var qtyOnHand = GetQOHSTF08A(brg, "ALL");
            if (qtyOnHand < 0)
            {
                qtyOnHand = 0;
            }

            qty = Convert.ToInt32(qtyOnHand);

            string urll = string.Format("{0}/api/v1/editProductdetail", iden.API_url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            string[] brg_mp_split = brg_mp.Split(';');

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(iden.API_key);
            postData += "&apiCredential=" + Uri.EscapeDataString(iden.API_credential);
            if (brg_mp_split[1] == "0")
            {
                postData += "&id_product=" + Uri.EscapeDataString(brg_mp_split[0]);
                postData += "&quantity=" + Uri.EscapeDataString(qty.ToString());
            }
            else
            {
                postData += "&id_product=" + Uri.EscapeDataString(brg_mp_split[0]);
                postData += "&id_product_attribute=" + Uri.EscapeDataString(brg_mp_split[1]);
                postData += "&quantity_attribute=" + Uri.EscapeDataString(qty.ToString());
            }
            postData += "&available_for_order=" + Uri.EscapeDataString("1");
            //postData += "&id_product=" + Uri.EscapeDataString(brg_mp_split[0]);
            ////postData += "&id_product_attribute=" + Uri.EscapeDataString(brg_mp_split[1]);
            //postData += "&stock=" + Uri.EscapeDataString(qty.ToString());

            var data = Encoding.ASCII.GetBytes(postData);

            myReq.Method = "POST";
            myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.ContentLength = data.Length;

            try
            {
                string responseFromServer = "";

                using (var stream = myReq.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                using (WebResponse response = await myReq.GetResponseAsync())
                {
                    using (Stream stream2 = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream2);
                        responseFromServer = reader.ReadToEnd();
                    }
                }

                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    var resultAPI = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ResultUpdateStock82Cart)) as ResultUpdateStock82Cart;
                    if (resultAPI.error != "none" && resultAPI.error != null)
                    {
                        //currentLog.REQUEST_ATTRIBUTE_3 = "Exception"; //marketplace stock
                        //currentLog.REQUEST_EXCEPTION = resultAPI.error.ToString();
                        //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, no_cust, currentLog, "82Cart");
                        throw new Exception(resultAPI.error.ToString());
                    }
                    //else
                    //{
                    //    //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, no_cust, currentLog, "82Cart");
                    //}
                }
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                //currentLog.REQUEST_ATTRIBUTE_3 = "Exception"; //marketplace stock
                //currentLog.REQUEST_EXCEPTION = msg;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, no_cust, currentLog, "82Cart");
                throw new Exception(msg);
            }

            return ret;
        }

        //add by fauzi 9 Maret 2020
        [AutomaticRetry(Attempts = 3)]
        [Queue("1_update_stok")]
        [NotifyOnFailed("Update Stok {obj} ke Shopify gagal.")]
        public async Task<string> Shopify_updateStock(string DatabasePathErasoft, string stf02_brg, string log_CUST, string log_ActionCategory, string log_ActionName, ShopifyAPIData iden, string brg_mp, int qty, string uname, PerformContext context)
        {
            string ret = "";

            SetupContext(DatabasePathErasoft, uname);

            var EDB = new DatabaseSQL(DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");

            var qtyOnHand = GetQOHSTF08A(stf02_brg, "ALL");
            if (qtyOnHand < 0)
            {
                qtyOnHand = 0;
            }

            qty = Convert.ToInt32(qtyOnHand);

            string[] brg_mp_split = brg_mp.Split(';');

            //string urll = "https://{0}:{1}@{2}.myshopify.com/admin/products/{3}.json";
            //string urll = "https://{0}:{1}@{2}.myshopify.com/admin/variants/{3}.json";
            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/api/2020-07/inventory_levels/set.json"; //admin/api/2020-07/inventory_levels/adjust.json for increase and decrease quantity
            var kodeBrg = "";
            if (brg_mp_split[1] != "0")
            {
                kodeBrg = brg_mp_split[1];
            }
            else
            {
                kodeBrg = brg_mp_split[0];
            }

            //ShopifyController.ShopifyAPIData dataAPI = new ShopifyController.ShopifyAPIData();
            //dataAPI.no_cust = iden.no_cust;
            //dataAPI.username = uname;
            //dataAPI.DatabasePathErasoft = DatabasePathErasoft;
            //dataAPI.account_store = iden.account_store;
            //dataAPI.API_key = iden.API_key;
            //dataAPI.API_password = iden.API_password;
            //dataAPI.email = iden.email;

            //var shopify = new ShopifyController();
            //string resultGetLocationID = shopify.Shopify_GetLocationID(dataAPI);
            //string resultGetInventoryID = shopify.Shopify_getSingleProductforUpdateStock(dataAPI, brg_mp);
            string resultGetLocationID = Shopify_GetLocationID(iden);
            string resultGetInventoryID = Shopify_getSingleProductforUpdateStock(iden, brg_mp);

            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store, Convert.ToInt64(kodeBrg));

            //ShopifyUpdateStockProduct putProdData = new ShopifyUpdateStockProduct
            //{
            //    //id = Convert.ToInt64(brg_mp_split[0]),
            //    //published = true,
            //    //available = true,
            //    variant = new ShopifyUpdateStockProductVariant()
            //};
            //ShopifyUpdateStockProductVariant variants = new ShopifyUpdateStockProductVariant
            //{
            //    id = Convert.ToInt64(kodeBrg),
            //    inventory_quantity = qty.ToString()
            //};

            //putProdData.variant.id = Convert.ToInt64(kodeBrg);
            //putProdData.variant.inventory_quantity = qty.ToString();


            //ShopifyUpdateStock putData = new ShopifyUpdateStock
            //{
            //    product = putProdData
            //};

            ShopifyUpdateStockNewAPI dataJsonAPI = new ShopifyUpdateStockNewAPI
            {
                inventory_item_id = Convert.ToInt64(resultGetInventoryID),
                location_id = Convert.ToInt64(resultGetLocationID),
                available = Convert.ToInt32(qty)
            };

            //string myData = JsonConvert.SerializeObject(putProdData);
            string myData = JsonConvert.SerializeObject(dataJsonAPI);

            string responseFromServer = "";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Shopify-Access-Token", (iden.API_password));
            var content = new StringContent(myData, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json");
            HttpResponseMessage clientResponse = await client.PostAsync(vformatUrl, content);

            using (HttpContent responseContent = clientResponse.Content)
            {
                using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                {
                    responseFromServer = await reader.ReadToEndAsync();
                }
            };

            //HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            //myReq.Method = "PUT";
            //myReq.Headers.Add("X-Shopify-Access-Token", (iden.API_password));
            //myReq.Accept = "application/json";
            //myReq.ContentType = "application/json";
            //myReq.ContentLength = myData.Length;
            //using (var dataStream = myReq.GetRequestStream())
            //{
            //    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
            //}
            //using (WebResponse response = myReq.GetResponse())
            //{
            //    using (Stream stream = response.GetResponseStream())
            //    {
            //        StreamReader reader = new StreamReader(stream);
            //        responseFromServer = reader.ReadToEnd();
            //    }
            //}


            if (responseFromServer != "")
            {
                try
                {
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyUpdateStockNewAPIResult)) as ShopifyUpdateStockNewAPIResult;
                    if (!string.IsNullOrWhiteSpace(result.ToString()))
                    {
                        if (result != null)
                        {
                            if (result.inventory_level != null)
                            {
                                //foreach (var item in result.variant)
                                //{
                                //    if (item.inventory_quantity == qty)
                                //    {
                                //        //throw new Exception("Success update stock " + stf02_brg + ": " + Convert.ToString(qty) + " stock");
                                //    }
                                //}
                            }
                            else
                            {
                                //var msgError = "";
                                //if (result != null)
                                //{
                                //    msgError = result.errors;
                                //}
                                //if (result.error != null)
                                //{
                                //    msgError = result.error;
                                //}
                                throw new Exception("Failed update stock " + stf02_brg + ":" + Convert.ToString(qty) + " stock. ");
                            }
                        }
                        else
                        {
                            throw new Exception("Failed update stock " + stf02_brg + ":" + Convert.ToString(qty) + ". API no response");
                        }
                    }
                    else
                    {
                        throw new Exception("Failed update stock " + stf02_brg + ":" + Convert.ToString(qty) + " stock" + ". API no response");
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    throw new Exception(msg);
                }
            }

            return ret;
        }

        public string Shopify_getSingleProductforUpdateStock(ShopifyAPIData iden, string kode_barang)
        {
            string result = "";
            var kodeBrg = "";
            string[] brg_mp = kode_barang.Split(';');

            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/products/{3}.json";
            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store, brg_mp[0].ToString());

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "GET";
            myReq.Headers.Add("X-Shopify-Access-Token", iden.API_password);
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";

            string responseFromServer = "";
            try
            {
                using (WebResponse response = myReq.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseFromServer = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {

            }

            if (responseFromServer != null)
            {
                try
                {
                    var detailBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyGetItemDetailResult)) as ShopifyGetItemDetailResult;
                    if (detailBrg != null)
                    {
                        if (detailBrg.product != null)
                        {
                            if (Convert.ToString(detailBrg.product.id) != null)
                            {
                                if (detailBrg.product.variants.Count() > 0)
                                {
                                    foreach (var itemVar in detailBrg.product.variants)
                                    {
                                        if (itemVar.product_id.ToString() == brg_mp[0] && itemVar.id.ToString() == brg_mp[1])
                                        {
                                            result = itemVar.inventory_item_id.ToString();
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
                catch (Exception ex2)
                {

                }
            }

            return result;
        }

        public string Shopify_GetLocationID(ShopifyAPIData dataAPI)
        {
            var result = "";
            var vurl = "https://{0}:{1}@{2}.myshopify.com/admin/shop.json";
            var vformatUrl = String.Format(vurl, dataAPI.API_key, dataAPI.API_password, dataAPI.account_store);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "GET";
            myReq.Headers.Add("X-Shopify-Access-Token", dataAPI.API_password);
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
            string responseFromServer = "";
            try
            {
                using (WebResponse response = myReq.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseFromServer = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {

            }

            if (responseFromServer != "")
            {
                try
                {
                    var resultAPI = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyGetLocationID)) as ShopifyGetLocationID;

                    if (!String.IsNullOrWhiteSpace(resultAPI.ToString()))
                    {
                        //if (result.shop != null && result.errors == null)
                        if (resultAPI.shop != null)
                        {
                            //if (resultAPI.shop.email == dataAPI.email || resultAPI.shop.customer_email == dataAPI.email)
                            //{
                            if (resultAPI.shop.primary_location_id != 0)
                            {
                                result = Convert.ToString(resultAPI.shop.primary_location_id);
                            }
                            //}
                        }
                    }
                }
                catch (Exception ex2)
                {

                }
            }

            return result;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_update_stok")]
        [NotifyOnFailed("Update Stok {obj} ke Shopee gagal.")]
        public async Task<string> Shopee_updateStock(string DatabasePathErasoft, string stf02_brg, string log_CUST, string log_ActionCategory, string log_ActionName, ShopeeAPIData iden, string brg_mp, int qty, string uname, PerformContext context)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            SetupContext(DatabasePathErasoft, uname);

            var EDB = new DatabaseSQL(DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");

            var qtyOnHand = GetQOHSTF08A(stf02_brg, "ALL");
            //add by calvin 17 juni 2019
            if (qtyOnHand < 0)
            {
                qtyOnHand = 0;
            }
            //end add by calvin 17 juni 2019

            qty = Convert.ToInt32(qtyOnHand);

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = seconds.ToString(),
            //    REQUEST_ACTION = "Update QOH",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = iden.merchant_code,
            //    REQUEST_STATUS = "Pending",
            //};

            string urll = "https://partner.shopeemobile.com/api/v1/items/update_stock";
            string[] brg_mp_split = brg_mp.Split(';');
            ShopeeUpdateStockData HttpBody = new ShopeeUpdateStockData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                item_id = Convert.ToInt64(brg_mp_split[0]),
                stock = qty
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            string responseFromServer = "";
            //try
            //{
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

            if (responseFromServer != "")
            {
                try
                {
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeUpdateStockResult)) as ShopeeUpdateStockResult;
                    if (!string.IsNullOrWhiteSpace(result.error))
                    {
                        throw new Exception(result.msg + ";request_id:" + result.request_id);
                    }
                    else
                    {
                        //add by calvin 28 oktober 2019
                        try
                        {
                            //if (dbPathEra.ToLower() == "erasoft_100144" || dbPathEra.ToLower() == "erasoft_120149" || dbPathEra.ToLower() == "erasoft_80069" || dbPathEra.ToUpper() == "ERASOFT_1000390")
                            if (dbPathEra.ToLower() == "erasoft_100144" || dbPathEra.ToUpper() == "ERASOFT_1310644" || dbPathEra.ToUpper() == "ERASOFT_1000390")
                            {
                                string EDBConnID = EDB.GetConnectionString("ConnId");
                                var sqlStorage = new SqlServerStorage(EDBConnID);
                                var client = new BackgroundJobClient(sqlStorage);
                                client.Schedule<StokControllerJob>(x => x.ShopeeCheckUpdateStock(DatabasePathErasoft, stf02_brg, log_CUST, uname, iden, Convert.ToInt64(brg_mp_split[0]), Convert.ToInt64(0), qty), TimeSpan.FromMinutes(2));
                            }
                        }
                        catch (Exception ex)
                        {
                            string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                            {
                                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                REQUEST_ACTION = "Selisih Stok",
                                REQUEST_DATETIME = DateTime.Now,
                                REQUEST_ATTRIBUTE_1 = stf02_brg,
                                REQUEST_ATTRIBUTE_2 = "MO Stock : " + Convert.ToString(qty), //updating to stock
                                REQUEST_ATTRIBUTE_3 = "Exception", //marketplace stock
                                REQUEST_STATUS = "Pending",
                                REQUEST_EXCEPTION = msg
                            };
                            var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);
                            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, log_CUST, currentLog, "Shopee");
                        }
                        //end add by calvin 28 oktober 2019
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    if (msg.Contains("not allowed to edit"))
                    {

#if (DEBUG || Debug_AWS)
                        await ShopeeUnlinkProduct(DatabasePathErasoft, stf02_brg, log_CUST, uname, iden, Convert.ToInt64(brg_mp_split[0]), Convert.ToInt64(0), qty);
#else
                        string EDBConnID = EDB.GetConnectionString("ConnId");
                        var sqlStorage = new SqlServerStorage(EDBConnID);
                        var client = new BackgroundJobClient(sqlStorage);
                        client.Schedule<StokControllerJob>(x => x.ShopeeUnlinkProduct(DatabasePathErasoft, stf02_brg, log_CUST, uname, iden, Convert.ToInt64(brg_mp_split[0]), Convert.ToInt64(0), qty), TimeSpan.FromMinutes(1));
#endif
                    }
                    else
                    {
                        throw new Exception(msg);
                    }
                }
            }

            return ret;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_update_stok")]
        [NotifyOnFailed("Update Stok {obj} ke Shopee gagal.")]
        public async Task<string> Shopee_updateVariationStock(string DatabasePathErasoft, string stf02_brg, string log_CUST, string log_ActionCategory, string log_ActionName, ShopeeAPIData iden, string brg_mp, int qty, string uname, PerformContext context)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            SetupContext(DatabasePathErasoft, uname);

            var EDB = new DatabaseSQL(DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");

            var qtyOnHand = GetQOHSTF08A(stf02_brg, "ALL");
            //add by calvin 17 juni 2019
            if (qtyOnHand < 0)
            {
                qtyOnHand = 0;
            }
            //end add by calvin 17 juni 2019
            qty = Convert.ToInt32(qtyOnHand);

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = seconds.ToString(),
            //    REQUEST_ACTION = "Update QOH",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = iden.merchant_code,
            //    REQUEST_STATUS = "Pending",
            //};

            string urll = "https://partner.shopeemobile.com/api/v1/items/update_variation_stock";
            string[] brg_mp_split = brg_mp.Split(';');
            ShopeeUpdateVariationStockData HttpBody = new ShopeeUpdateVariationStockData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                item_id = Convert.ToInt64(brg_mp_split[0]),
                variation_id = Convert.ToInt64(brg_mp_split[1]),
                stock = qty
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            string responseFromServer = "";
            //try
            //{
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
            //    manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            //}
            //catch (Exception ex)
            //{
            //    currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}

            if (responseFromServer != "")
            {
                try
                {
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeUpdateStockResult)) as ShopeeUpdateStockResult;
                    if (!string.IsNullOrWhiteSpace(result.error))
                    {
                        throw new Exception(result.msg + ";request_id:" + result.request_id);
                    }
                    else
                    {
                        //add by calvin 28 oktober 2019
                        try
                        {
                            //if (dbPathEra.ToLower() == "erasoft_100144" || dbPathEra.ToLower() == "erasoft_120149" || dbPathEra.ToLower() == "erasoft_80069" || dbPathEra.ToUpper() == "ERASOFT_1000390")
                            if (dbPathEra.ToLower() == "erasoft_100144" || dbPathEra.ToUpper() == "ERASOFT_1310644" || dbPathEra.ToUpper() == "ERASOFT_1000390")
                            {
                                string EDBConnID = EDB.GetConnectionString("ConnId");
                                var sqlStorage = new SqlServerStorage(EDBConnID);
                                var client = new BackgroundJobClient(sqlStorage);
                                client.Schedule<StokControllerJob>(x => x.ShopeeCheckUpdateStock(DatabasePathErasoft, stf02_brg, log_CUST, uname, iden, Convert.ToInt64(brg_mp_split[0]), Convert.ToInt64(brg_mp_split[1]), qty), TimeSpan.FromMinutes(2));
                            }
                        }
                        catch (Exception ex)
                        {
                            string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                            {
                                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                REQUEST_ACTION = "Selisih Stok",
                                REQUEST_DATETIME = DateTime.Now,
                                REQUEST_ATTRIBUTE_1 = stf02_brg,
                                REQUEST_ATTRIBUTE_2 = "MO Stock : " + Convert.ToString(qty), //updating to stock
                                REQUEST_ATTRIBUTE_3 = "Exception", //marketplace stock
                                REQUEST_STATUS = "Pending",
                                REQUEST_EXCEPTION = msg
                            };
                            var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);
                            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, log_CUST, currentLog, "Shopee");
                        }
                        //end add by calvin 28 oktober 2019
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    if (msg.Contains("not allowed to edit"))
                    {

#if (DEBUG || Debug_AWS)
                        await ShopeeUnlinkProduct(DatabasePathErasoft, stf02_brg, log_CUST, uname, iden, Convert.ToInt64(brg_mp_split[0]), Convert.ToInt64(brg_mp_split[1]), qty);
#else
                        string EDBConnID = EDB.GetConnectionString("ConnId");
                        var sqlStorage = new SqlServerStorage(EDBConnID);
                        var client = new BackgroundJobClient(sqlStorage);
                        client.Schedule<StokControllerJob>(x => x.ShopeeUnlinkProduct(DatabasePathErasoft, stf02_brg, log_CUST, uname, iden, Convert.ToInt64(brg_mp_split[0]), Convert.ToInt64(brg_mp_split[1]), qty), TimeSpan.FromMinutes(1));
#endif
                    }
                    else
                    {
                        throw new Exception(msg);
                    }
                }
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 1)]
        [Queue("1_update_stok")]
        public async Task<BindingBase> ShopeeUnlinkProduct(string DatabasePathErasoft, string stf02_brg, string log_CUST, string uname, ShopeeAPIData iden, Int64 item_id, Int64 variation_id, int MO_qty)
        {
            //    int MOPartnerID = 841371;
            //    string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            //string ret = "";
            SetupContext(DatabasePathErasoft, uname);
            var EDB = new DatabaseSQL(DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            var statusProduct = "";
            var requestAction = "Selisih Stok";

            var ret = new BindingBase
            {
                status = 0,
                recordCount = -1,
            };
            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);
            var dateNowLog = DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");

            string urll = "https://partner.shopeemobile.com/api/v1/item/get";

            ShopeeControllerJob.ShopeeGetItemDetailData HttpBody = new ShopeeControllerJob.ShopeeGetItemDetailData
            {
                partner_id = 841371,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                item_id = item_id,
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c");

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            string responseFromServer = "";

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

            if (responseFromServer != null)
            {
                var detailBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeControllerJob.ShopeeGetItemDetailResult)) as ShopeeControllerJob.ShopeeGetItemDetailResult;

                ret.status = 1;

                var sellerSku = "";

                if (detailBrg.item.has_variation)
                {
                    //insert brg induk
                    string brgMpInduk = Convert.ToString(detailBrg.item.item_id) + ";";

                    foreach (var item in detailBrg.item.variations)
                    {
                        if (detailBrg.item.item_id == item_id && item.variation_id == variation_id)
                        {
                            if (item.status.ToLower() == "deleted")
                            {
                                var rowsAffected = EDB.ExecuteSQL("ConnId", CommandType.Text, "UPDATE STF02H SET BRG_MP = '', DISPLAY = 'false', LINK_STATUS = 'Barang dihapus oleh Shopee', LINK_ERROR = '0;Status;;', LINK_DATETIME = '" + dateNowLog + "' WHERE BRG_MP = '" + Convert.ToString(item_id) + ";" + Convert.ToString(variation_id) + "' AND BRG = '" + stf02_brg + "'");
                                var personame = Convert.ToString(EDB.GetFieldValue("ConnId", "ARF01", "CUST = '" + log_CUST + "'", "PERSO"));
                                if (rowsAffected > 0)
                                {
                                    requestAction = "Unlink Product";
                                    statusProduct = "Barang " + stf02_brg + " telah dihapus oleh Shopee. Unlink Otomatis barang di akun Shopee " + personame + " sudah selesai.";
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(dbPathEra).monotification(statusProduct.ToString());
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (detailBrg.item.item_id == item_id)
                    {
                        if (detailBrg.item.status.ToLower() == "deleted")
                        {
                            var rowsAffected = EDB.ExecuteSQL("ConnId", CommandType.Text, "UPDATE STF02H SET BRG_MP = '', DISPLAY = 'false', LINK_STATUS = 'Barang dihapus oleh Shopee', LINK_ERROR = '0;Status;;', LINK_DATETIME = '" + dateNowLog + "' WHERE BRG_MP = '" + Convert.ToString(item_id) + ";" + Convert.ToString(variation_id) + "' AND BRG = '" + stf02_brg + "'");
                            var personame = Convert.ToString(EDB.GetFieldValue("ConnId", "ARF01", "CUST = '" + log_CUST + "'", "PERSO"));
                            if (rowsAffected > 0)
                            {
                                requestAction = "Unlink Product";
                                statusProduct = "Barang " + stf02_brg + " telah dihapus oleh Shopee. Unlink Otomatis barang di akun Shopee " + personame + " sudah selesai.";
                                var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                contextNotif.Clients.Group(dbPathEra).monotification(statusProduct.ToString());
                            }
                        }
                    }
                }
            }

            if (ret.recordCount < MO_qty || ret.recordCount > MO_qty)
            {
                MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                {
                    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    REQUEST_ACTION = requestAction,
                    //REQUEST_DATETIME = DateTime.Now,
                    REQUEST_DATETIME = DateTime.UtcNow.AddHours(7), // update to +7 hour
                    REQUEST_ATTRIBUTE_1 = stf02_brg,
                    REQUEST_ATTRIBUTE_2 = "MO Stock : " + Convert.ToString(MO_qty), //updating to stock
                    REQUEST_ATTRIBUTE_3 = "Shopee Stock : " + Convert.ToString(ret.recordCount), //marketplace stock
                    REQUEST_STATUS = "Pending",
                    REQUEST_EXCEPTION = statusProduct
                };
                var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, log_CUST, currentLog, "Shopee");

                //#if (DEBUG || Debug_AWS)
                //                            Task.Run(() => Shopee_updateVariationStock(DatabasePathErasoft, stf02_brg, log_CUST, "Stock", "Update Stok", iden, brg_mp, 0, uname, null)).Wait();
                //#else
                //                            var EDB = new DatabaseSQL(dbPathEra);
                //                            string EDBConnID = EDB.GetConnectionString("ConnId");
                //                            var sqlStorage = new SqlServerStorage(EDBConnID);
                //                            var client = new BackgroundJobClient(sqlStorage);
                //                            client.Enqueue<StokControllerJob>(x => x.Shopee_updateVariationStock(DatabasePathErasoft, stf02_brg, log_CUST, "Stock", "Update Stok", iden, brg_mp, 0, uname, null));
                //#endif
            }

            return ret;
        }

        [AutomaticRetry(Attempts = 1)]
        [Queue("1_update_stok")]
        public async Task<BindingBase> ShopeeCheckUpdateStock(string DatabasePathErasoft, string stf02_brg, string log_CUST, string uname, ShopeeAPIData iden, Int64 item_id, Int64 variation_id, int MO_qty)
        {
            //    int MOPartnerID = 841371;
            //    string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            //string ret = "";
            SetupContext(DatabasePathErasoft, uname);
            var EDB = new DatabaseSQL(DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");

            var ret = new BindingBase
            {
                status = 0,
                recordCount = -1,
            };
            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/item/get";

            ShopeeControllerJob.ShopeeGetItemDetailData HttpBody = new ShopeeControllerJob.ShopeeGetItemDetailData
            {
                partner_id = 841371,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                item_id = item_id
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c");

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            string responseFromServer = "";

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

            if (responseFromServer != null)
            {
                var detailBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeControllerJob.ShopeeGetItemDetailResult)) as ShopeeControllerJob.ShopeeGetItemDetailResult;

                ret.status = 1;

                var sellerSku = "";

                if (detailBrg.item.has_variation)
                {
                    //insert brg induk
                    string brgMpInduk = Convert.ToString(detailBrg.item.item_id) + ";";

                    foreach (var item in detailBrg.item.variations)
                    {
                        if (detailBrg.item.item_id == item_id && item.variation_id == variation_id)
                        {
                            ret.recordCount = item.stock;
                        }
                    }
                }
                else
                {
                    if (detailBrg.item.item_id == item_id)
                    {
                        ret.recordCount = detailBrg.item.stock;
                    }
                }
            }

            if (ret.recordCount < MO_qty || ret.recordCount > MO_qty)
            {
                MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                {
                    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    REQUEST_ACTION = "Selisih Stok",
                    REQUEST_DATETIME = DateTime.Now,
                    REQUEST_ATTRIBUTE_1 = stf02_brg,
                    REQUEST_ATTRIBUTE_2 = "MO Stock : " + Convert.ToString(MO_qty), //updating to stock
                    REQUEST_ATTRIBUTE_3 = "Shopee Stock : " + Convert.ToString(ret.recordCount), //marketplace stock
                    REQUEST_STATUS = "Pending",
                };
                var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, log_CUST, currentLog, "Shopee");

                //#if (DEBUG || Debug_AWS)
                //                            Task.Run(() => Shopee_updateVariationStock(DatabasePathErasoft, stf02_brg, log_CUST, "Stock", "Update Stok", iden, brg_mp, 0, uname, null)).Wait();
                //#else
                //                            var EDB = new DatabaseSQL(dbPathEra);
                //                            string EDBConnID = EDB.GetConnectionString("ConnId");
                //                            var sqlStorage = new SqlServerStorage(EDBConnID);
                //                            var client = new BackgroundJobClient(sqlStorage);
                //                            client.Enqueue<StokControllerJob>(x => x.Shopee_updateVariationStock(DatabasePathErasoft, stf02_brg, log_CUST, "Stock", "Update Stok", iden, brg_mp, 0, uname, null));
                //#endif
            }

            return ret;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_update_stok")]
        [NotifyOnFailed("Update Stok {obj} ke JD.ID gagal.")]
        public async Task<string> JD_updateStock(string DatabasePathErasoft, string stf02_brg, string log_CUST, string log_ActionCategory, string log_ActionName, JDIDAPIData data, string id, int stok, string uname, PerformContext context)
        {
            SetupContext(DatabasePathErasoft, uname);

            var qtyOnHand = GetQOHSTF08A(stf02_brg, "ALL");

            if (qtyOnHand < 0)
            {
                qtyOnHand = 0;
            }

            stok = Convert.ToInt32(qtyOnHand);

            try
            {
                string sMethod = "epi.ware.openapi.warestock.updateWareStock";
                string sParamJson = "{\"jsonStr\":[{\"skuId\":" + id + ", \"realNum\": " + stok + "}]}";

                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
                if (ret != null)
                {
                    if (ret.openapi_msg.ToLower() == "success")
                    {
                        var retStok = JsonConvert.DeserializeObject(ret.openapi_data, typeof(Data_UpStok)) as Data_UpStok;
                        if (retStok != null)
                        {
                            if (retStok.success)
                            {

                            }
                            else
                            {
                                throw new Exception(retStok.message.ToString());
                            }
                        }
                        else
                        {
                            throw new Exception(ret.openapi_msg.ToString());
                        }
                    }
                    else
                    {
                        throw new Exception(ret.openapi_msg.ToString());
                    }
                }
                else
                {
                    throw new Exception("Tidak ada respon dari API.");
                }
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                throw new Exception(msg);
            }

            return "";
        }



        //add by nurul 29/7/2020
        [AutomaticRetry(Attempts = 2)]
        [Queue("1_manage_pesanan")]
        public async Task<string> updateBrutoSit01a(string connId, string DatabasePathErasoft, string uname, string nobukSI, double? bruto)
        {
            string ret = "";
            try
            {
                SetupContext(DatabasePathErasoft, uname);
                var MoDbContext = new MoDbContext("");
                var EDB = new DatabaseSQL(DatabasePathErasoft);
                string EraServerName = EDB.GetServerName("sConn");
                var ErasoftDbContext = new ErasoftContext(EraServerName, DatabasePathErasoft);

                //ErasoftDbContext.SIT01A.Where(p => p.NO_BUKTI == nobukSI && p.JENIS_FORM == "2").Update(p => new SIT01A() { BRUTO = bruto });
                if (nobukSI != null)
                {
                    var cekSI = ErasoftDbContext.SIT01A.Where(p => p.NO_BUKTI == nobukSI && p.JENIS_FORM == "2").FirstOrDefault();
                    if (cekSI != null)
                    {
                        //cekSI.BRUTO = bruto;
                        string sSQL = "update sit01a set BRUTO = '" + bruto + "' where NO_BUKTI = '" + nobukSI + "' and JENIS_FORM ='2'";
                        ErasoftDbContext.Database.ExecuteSqlCommand(sSQL);
                        ErasoftDbContext.SaveChanges();
                    }
                    else
                    {
                        throw new Exception("Faktur Tidak Ditemukan.");
                    }
                }
                else
                {
                    throw new Exception("Faktur Tidak Ditemukan.");
                }
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
                throw new Exception(err);
            }

            return ret;
        }
        //end add by nurul 29/7/2020

        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public partial class ClientMessage
        {

            private string messageField;

            private string productNoField;

            private string resultCodeField;

            /// <remarks/>
            public string Message
            {
                get
                {
                    return this.messageField;
                }
                set
                {
                    this.messageField = value;
                }
            }
            public string message { get; set; }
            /// <remarks/>
            public string productNo
            {
                get
                {
                    return this.productNoField;
                }
                set
                {
                    this.productNoField = value;
                }
            }

            /// <remarks/>
            public string resultCode
            {
                get
                {
                    return this.resultCodeField;
                }
                set
                {
                    this.resultCodeField = value;
                }
            }
        }
        public class EleveniaProductData
        {
            public string api_key { get; set; }
            public string kode { get; set; }
            public string nama { get; set; }
            public string berat { get; set; }
            public string[] imgUrl { get; set; }
            public string Keterangan { get; set; }
            public string Price { get; set; }
            public string Qty { get; set; }
            public string DeliveryTempNo { get; set; }
            public string Brand { get; set; }
            public string IDMarket { get; set; }
            public string kode_mp { get; set; }

        }

        public class BlibliAPIData
        {
            public string merchant_code { get; set; }
            public string API_client_username { get; set; }
            public string API_client_password { get; set; }
            public string API_secret_key { get; set; }
            public string mta_username_email_merchant { get; set; }
            public string mta_password_password_merchant { get; set; }
            public string token { get; set; }
            public int idmarket { get; set; }
            public string DatabasePathErasoft { get; set; }
            public string username { get; set; }
            //add by nurul 16/7/2020
            public string versiToken { get; set; }
            //end add by nurul 16/7/2020
        }

        public class BlibliProductData
        {
            public string type { get; set; }
            public string kode { get; set; }
            public string nama { get; set; }
            public string display { get; set; }
            public string Length { get; set; }
            public string Width { get; set; }
            public string Height { get; set; }
            public string berat { get; set; }
            public string[] imgUrl { get; set; }
            public string Keterangan { get; set; }
            public string Price { get; set; }
            public string MarketPrice { get; set; }
            public string Qty { get; set; }
            public string MinQty { get; set; }
            public string DeliveryTempNo { get; set; }
            public string Brand { get; set; }
            public string IDMarket { get; set; }
            public string kode_mp { get; set; }
            public string CategoryCode { get; set; }
            public string[] attribute { get; set; }
            public string feature { get; set; }
            public string PickupPoint { get; set; }
            public STF02 dataBarangInDb { get; set; }


        }

        public class BlibliDetailProductResult
        {
            public string requestId { get; set; }
            public object headers { get; set; }
            public object errorMessage { get; set; }
            public object errorCode { get; set; }
            public bool success { get; set; }
            public BlibliDetailProductResultValue value { get; set; }
        }

        public class BlibliDetailProductResultValue
        {
            public string id { get; set; }
            public string storeId { get; set; }
            public long createdDate { get; set; }
            public string createdBy { get; set; }
            public long updatedDate { get; set; }
            public string updatedBy { get; set; }
            public object version { get; set; }
            public string productSku { get; set; }
            public string productCode { get; set; }
            public string businessPartnerCode { get; set; }
            public bool synchronize { get; set; }
            public string productName { get; set; }
            public int productType { get; set; }
            public string categoryCode { get; set; }
            public string categoryName { get; set; }
            public string categoryHierarchy { get; set; }
            public string brand { get; set; }
            public string description { get; set; }
            public string specificationDetail { get; set; }
            public string uniqueSellingPoint { get; set; }
            public string productStory { get; set; }
            public BlibliDetailProductResultItem[] items { get; set; }
            public BlibliDetailProductResultAttribute[] attributes { get; set; }
            public BlibliDetailProductResultImage1[] images { get; set; }
            public string url { get; set; }
            public bool installationRequired { get; set; }
            public string categoryId { get; set; }
        }

        public class BlibliDetailProductResultItem
        {
            public string id { get; set; }
            public string storeId { get; set; }
            public long createdDate { get; set; }
            public string createdBy { get; set; }
            public long updatedDate { get; set; }
            public string updatedBy { get; set; }
            public object version { get; set; }
            public string itemSku { get; set; }
            public string skuCode { get; set; }
            public string merchantSku { get; set; }
            public string upcCode { get; set; }
            public string itemName { get; set; }
            public float length { get; set; }
            public float width { get; set; }
            public float height { get; set; }
            public float weight { get; set; }
            public float shippingWeight { get; set; }
            public int dangerousGoodsLevel { get; set; }
            public bool lateFulfillment { get; set; }
            public string pickupPointCode { get; set; }
            public string pickupPointName { get; set; }
            public int availableStockLevel1 { get; set; }
            public int reservedStockLevel1 { get; set; }
            public int availableStockLevel2 { get; set; }
            public int reservedStockLevel2 { get; set; }
            public int minimumStock { get; set; }
            public bool synchronizeStock { get; set; }
            public bool off2OnActiveFlag { get; set; }
            public object pristineId { get; set; }
            public BlibliDetailProductResultPrice[] prices { get; set; }
            public BlibliDetailProductResultViewconfig[] viewConfigs { get; set; }
            public BlibliDetailProductResultImage[] images { get; set; }
            public object cogs { get; set; }
            public string cogsErrorCode { get; set; }
            public bool promoBundling { get; set; }
        }

        public class BlibliDetailProductResultPrice
        {
            public object id { get; set; }
            public object storeId { get; set; }
            public object createdDate { get; set; }
            public object createdBy { get; set; }
            public object updatedDate { get; set; }
            public object updatedBy { get; set; }
            public object version { get; set; }
            public string channelId { get; set; }
            public float price { get; set; }
            public float salePrice { get; set; }
            public object discountAmount { get; set; }
            public object discountStartDate { get; set; }
            public object discountEndDate { get; set; }
            public object promotionName { get; set; }
        }

        public class BlibliDetailProductResultViewconfig
        {
            public object id { get; set; }
            public object storeId { get; set; }
            public object createdDate { get; set; }
            public object createdBy { get; set; }
            public object updatedDate { get; set; }
            public object updatedBy { get; set; }
            public object version { get; set; }
            public string channelId { get; set; }
            public bool display { get; set; }
            public bool buyable { get; set; }
        }

        public class BlibliDetailProductResultImage
        {
            public object id { get; set; }
            public object storeId { get; set; }
            public object createdDate { get; set; }
            public object createdBy { get; set; }
            public object updatedDate { get; set; }
            public object updatedBy { get; set; }
            public object version { get; set; }
            public bool mainImage { get; set; }
            public int sequence { get; set; }
            public string locationPath { get; set; }
        }

        public class BlibliDetailProductResultAttribute
        {
            public object id { get; set; }
            public object storeId { get; set; }
            public object createdDate { get; set; }
            public object createdBy { get; set; }
            public object updatedDate { get; set; }
            public object updatedBy { get; set; }
            public object version { get; set; }
            public string attributeCode { get; set; }
            public string attributeType { get; set; }
            public string[] values { get; set; }
            public bool skuValue { get; set; }
            public string attributeName { get; set; }
            public string itemSku { get; set; }
        }

        public class BlibliDetailProductResultImage1
        {
            public object id { get; set; }
            public object storeId { get; set; }
            public object createdDate { get; set; }
            public object createdBy { get; set; }
            public object updatedDate { get; set; }
            public object updatedBy { get; set; }
            public object version { get; set; }
            public bool mainImage { get; set; }
            public int sequence { get; set; }
            public string locationPath { get; set; }
        }

        public class TokopediaAPIData
        {
            public string merchant_code { get; set; }
            public string API_client_username { get; set; }
            public string API_client_password { get; set; }
            public string API_secret_key { get; set; }
            public string mta_username_email_merchant { get; set; }
            public string mta_password_password_merchant { get; set; }
            public string token { get; set; }
            public int idmarket { get; set; }
            public string DatabasePathErasoft { get; set; }
            public string username { get; set; }
        }

        public class TokopediaUpdateStockData
        {
            public string sku { get; set; }
            public int product_id { get; set; }
            public int new_stock { get; set; }

        }

        public class ShopifyAPIData
        {
            public string no_cust { get; set; }
            public string account_store { get; set; }
            public string API_key { get; set; }
            public string API_password { get; set; }
            public string DatabasePathErasoft { get; set; }
            public string email { get; set; }
            public int rec_num { get; set; }
        }

        public class ShopifyUpdateData
        {
            public List<ShopifyUpdateDataProduct> product { get; set; }
            public object errors { get; set; }
        }

        public class ShopifyUpdateDataProduct
        {
            public long id { get; set; }
            public string title { get; set; }
            public string body_html { get; set; }
            public string vendor { get; set; }
            public string product_type { get; set; }
            public bool published { get; set; }
            public string[] tags { get; set; }
            public List<ShopifyUpdateDataProductVariant> variants { get; set; }
            public List<ShopifyUpdateProductOptions> options { get; set; }
            public List<ShopifyProductImage> images { get; set; }
        }

        public class ShopifyUpdateProductOptions
        {
            public long id { get; set; }
            public string name { get; set; }
            public object values { get; set; }
        }

        public class ShopifyProductImage
        {
            public long id { get; set; }
            public string position { get; set; }
            public string src { get; set; }
        }

        public class ShopifyUpdateDataProductVariant
        {
            public long id { get; set; }
            public string title { get; set; }
            public string price { get; set; }
            public string sku { get; set; }
            public string option1 { get; set; }
            public string option2 { get; set; }
            public string option3 { get; set; }
            public string weight { get; set; }
            public string weight_unit { get; set; }
            public string inventory_item_id { get; set; }
            public string inventory_quantity { get; set; }
        }


        public class ShopifyUpdateStock
        {
            public ShopifyUpdateStockProduct product { get; set; }
        }

        public class ShopifyUpdateStockProduct
        {
            //public long id { get; set; }
            //public bool published { get; set; }
            //public bool available { get; set; }
            public ShopifyUpdateStockProductVariant variant { get; set; }
        }

        public class ShopifyUpdateStockProductVariant
        {
            public long id { get; set; }
            //public string price { get; set; }
            //public string sku { get; set; }
            public string inventory_quantity { get; set; }
            //public string weight { get; set; }
            //public string weight_unit { get; set; }
        }

        public class ShopifyGetLocationID
        {
            public ShopifyGetShopAccountResultLocationID shop { get; set; }
        }

        public class ShopifyGetShopAccountResultLocationID
        {
            public long id { get; set; }
            public string name { get; set; }
            public string email { get; set; }
            //public string domain { get; set; }
            //public string province { get; set; }
            //public string country { get; set; }
            //public string address1 { get; set; }
            //public string zip { get; set; }
            //public string city { get; set; }
            //public object source { get; set; }
            //public string phone { get; set; }
            //public float latitude { get; set; }
            //public float longitude { get; set; }
            public string primary_locale { get; set; }
            //public string address2 { get; set; }
            //public DateTime created_at { get; set; }
            //public DateTime updated_at { get; set; }
            //public string country_code { get; set; }
            //public string country_name { get; set; }
            //public string currency { get; set; }
            public string customer_email { get; set; }
            //public string timezone { get; set; }
            //public string iana_timezone { get; set; }
            //public string shop_owner { get; set; }
            //public string money_format { get; set; }
            //public string money_with_currency_format { get; set; }
            //public string weight_unit { get; set; }
            //public string province_code { get; set; }
            //public bool taxes_included { get; set; }
            //public object tax_shipping { get; set; }
            //public bool county_taxes { get; set; }
            //public string plan_display_name { get; set; }
            //public string plan_name { get; set; }
            //public bool has_discounts { get; set; }
            //public bool has_gift_cards { get; set; }
            //public string myshopify_domain { get; set; }
            //public object google_apps_domain { get; set; }
            //public object google_apps_login_enabled { get; set; }
            //public string money_in_emails_format { get; set; }
            //public string money_with_currency_in_emails_format { get; set; }
            //public bool eligible_for_payments { get; set; }
            //public bool requires_extra_payments_agreement { get; set; }
            //public bool password_enabled { get; set; }
            //public bool has_storefront { get; set; }
            //public bool eligible_for_card_reader_giveaway { get; set; }
            //public bool finances { get; set; }
            public long primary_location_id { get; set; }
            //public string cookie_consent_level { get; set; }
            //public string visitor_tracking_consent_preference { get; set; }
            //public bool force_ssl { get; set; }
            //public bool checkout_api_supported { get; set; }
            //public bool multi_location_enabled { get; set; }
            //public bool setup_required { get; set; }
            //public bool pre_launch_enabled { get; set; }
            //public string[] enabled_presentment_currencies { get; set; }
        }


        public class ShopifyGetItemDetailResult
        {
            public ShopifyGetItemDetailResultProduct product { get; set; }
        }

        public class ShopifyGetItemDetailResultProduct
        {
            public long id { get; set; }
            public string title { get; set; }
            public string body_html { get; set; }
            public string vendor { get; set; }
            public string product_type { get; set; }
            public DateTime created_at { get; set; }
            public string handle { get; set; }
            public DateTime updated_at { get; set; }
            public DateTime published_at { get; set; }
            public string template_suffix { get; set; }
            public string published_scope { get; set; }
            public string tags { get; set; }
            public string admin_graphql_api_id { get; set; }
            public ShopifyGetItemDetailResultProductVariant[] variants { get; set; }
            public ShopifyGetItemDetailResultProductOption[] options { get; set; }
            public ShopifyGetItemDetailResultProductImageMore[] images { get; set; }
            public ShopifyGetItemDetailResultProductImage image { get; set; }
        }

        public class ShopifyGetItemDetailResultProductImage
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public int position { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public object alt { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string src { get; set; }
            public object[] variant_ids { get; set; }
            public string admin_graphql_api_id { get; set; }
        }

        public class ShopifyGetItemDetailResultProductVariant
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public string title { get; set; }
            public string price { get; set; }
            public string sku { get; set; }
            public int position { get; set; }
            public string inventory_policy { get; set; }
            public object compare_at_price { get; set; }
            public string fulfillment_service { get; set; }
            public string inventory_management { get; set; }
            public string option1 { get; set; }
            public object option2 { get; set; }
            public object option3 { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public bool taxable { get; set; }
            public string barcode { get; set; }
            public int grams { get; set; }
            public object image_id { get; set; }
            public float weight { get; set; }
            public string weight_unit { get; set; }
            public long inventory_item_id { get; set; }
            public int inventory_quantity { get; set; }
            public int old_inventory_quantity { get; set; }
            public bool requires_shipping { get; set; }
            public string admin_graphql_api_id { get; set; }
        }

        public class ShopifyGetItemDetailResultProductOption
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public string name { get; set; }
            public int position { get; set; }
            public string[] values { get; set; }
        }

        public class ShopifyGetItemDetailResultProductImageMore
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public int position { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public object alt { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string src { get; set; }
            public object[] variant_ids { get; set; }
            public string admin_graphql_api_id { get; set; }
        }




        public class ShopifyUpdateStockNewAPI
        {
            public long inventory_item_id { get; set; }
            public long location_id { get; set; }
            public int available { get; set; }
        }

        public class ShopifyUpdateStockNewAPIResult
        {
            public ShopifyUpdateStockNewAPIResult_Inventory_Level inventory_level { get; set; }
        }

        public class ShopifyUpdateStockNewAPIResult_Inventory_Level
        {
            public long inventory_item_id { get; set; }
            public long location_id { get; set; }
            public int available { get; set; }
            public DateTime updated_at { get; set; }
            public string admin_graphql_api_id { get; set; }
        }

        public class ResultUpdateStockVariant
        {
            public object variant { get; set; }
            public string errors { get; set; }
            public string error { get; set; }
        }

        public class ShopifyUpdateStockResult
        {
            public ShopifyUpdateStockResultProduct product { get; set; }
            public ShopifyUpdateStockResultError errors { get; set; }
        }

        public class ShopifyUpdateStockResultError
        {
            public string[] inventory_quantity { get; set; }
        }

        public class ShopifyUpdateStockResultProduct
        {
            public long id { get; set; }
            public string title { get; set; }
            public string body_html { get; set; }
            public string vendor { get; set; }
            public string product_type { get; set; }
            public DateTime created_at { get; set; }
            public string handle { get; set; }
            public DateTime updated_at { get; set; }
            public DateTime published_at { get; set; }
            public string template_suffix { get; set; }
            public string published_scope { get; set; }
            public string tags { get; set; }
            public string admin_graphql_api_id { get; set; }
            public ShopifyUpdateStockResultProductVariant[] variants { get; set; }
            public ShopifyUpdateStockResultProductOption[] options { get; set; }
            public ShopifyUpdateStockResultProductImage1[] images { get; set; }
            public ShopifyUpdateStockResultProductImage image { get; set; }
        }

        public class ShopifyUpdateStockResultProductImage
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public int position { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public object alt { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string src { get; set; }
            public object[] variant_ids { get; set; }
            public string admin_graphql_api_id { get; set; }
        }

        public class ShopifyUpdateStockResultProductVariant
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public string title { get; set; }
            public string price { get; set; }
            public string sku { get; set; }
            public int position { get; set; }
            public string inventory_policy { get; set; }
            public object compare_at_price { get; set; }
            public string fulfillment_service { get; set; }
            public string inventory_management { get; set; }
            public string option1 { get; set; }
            public object option2 { get; set; }
            public object option3 { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public bool taxable { get; set; }
            public string barcode { get; set; }
            public int grams { get; set; }
            public object image_id { get; set; }
            public float weight { get; set; }
            public string weight_unit { get; set; }
            public long inventory_item_id { get; set; }
            public int inventory_quantity { get; set; }
            public int old_inventory_quantity { get; set; }
            public bool requires_shipping { get; set; }
            public string admin_graphql_api_id { get; set; }
        }

        public class ShopifyUpdateStockResultProductOption
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public string name { get; set; }
            public int position { get; set; }
            public string[] values { get; set; }
        }

        public class ShopifyUpdateStockResultProductImage1
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public int position { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public object alt { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string src { get; set; }
            public object[] variant_ids { get; set; }
            public string admin_graphql_api_id { get; set; }
        }


        public class ShopeeAPIData
        {
            public string merchant_code { get; set; }
            public string API_client_username { get; set; }
            public string API_client_password { get; set; }
            public string API_secret_key { get; set; }
            public string mta_username_email_merchant { get; set; }
            public string mta_password_password_merchant { get; set; }
            public string token { get; set; }
            public string DatabasePathErasoft { get; set; }
            public string username { get; set; }
        }

        public class ShopeeUpdateStockData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public long item_id { get; set; }
            public int stock { get; set; }
        }

        public class ShopeeUpdateVariationStockData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public long item_id { get; set; }
            public long variation_id { get; set; }
            public int stock { get; set; }
        }

        private string CreateSign(string signBase, string secretKey)
        {
            secretKey = secretKey ?? "";
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secretKey);
            byte[] messageBytes = encoding.GetBytes(signBase);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
                //return BitConverter.ToString(hashmessage).ToLower();
            }
        }

        public static long CurrentTimeSecond()
        {
            //        return (long)DateTime.Now.ToUniversalTime().Subtract(
            //new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            //).TotalMilliseconds;
            return (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        public static long CurrentTimeMillis()
        {
            //        return (long)DateTime.Now.ToUniversalTime().Subtract(
            //new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            //).TotalMilliseconds;
            return (long)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        private string CreateTokenBlibli(string urlBlili, string secretMTA)
        {
            secretMTA = secretMTA ?? "";
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secretMTA);
            byte[] messageBytes = encoding.GetBytes(urlBlili);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
                //return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();

            }
        }
        protected string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            //byte[] encodedBytes = Encoding.UTF8.GetBytes(input);
            //Encoding.Convert(Encoding.UTF8, Encoding.Unicode, encodedBytes);
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            //byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public class ShopeeUpdateStockResult
        {
            public ShopeeUpdateStockResultItem item { get; set; }
            public string request_id { get; set; }
            public string error { get; set; }
            public string msg { get; set; }
        }

        public class ShopeeUpdateStockResultItem
        {
            public long item_id { get; set; }
            public long variation_id { get; set; }
            public int modified_time { get; set; }
            public int stock { get; set; }
            public string request_id { get; set; }
        }

        public class E2CartAPIData
        {
            public string no_cust { get; set; }
            public string username { get; set; }
            public string account_store { get; set; }
            public string API_key { get; set; }
            public string API_password { get; set; }
            public string DatabasePathErasoft { get; set; }
            public string email { get; set; }
            public int rec_num { get; set; }
            public string API_url { get; set; }
            public string API_credential { get; set; }
        }

        public class ResultUpdateStock82Cart
        {
            public string requestid { get; set; }
            public string error { get; set; }
            public string results { get; set; }
            public object data { get; set; }
        }

        public class ResultUpdateStockData
        {
            public string id_stock_available { get; set; }
            public string id_product { get; set; }
            public string id_product_attribute { get; set; }
            public string id_shop { get; set; }
            public string id_shop_group { get; set; }
            public string quantity { get; set; }
            public string depends_on_stock { get; set; }
            public string out_of_stock { get; set; }
        }

    }
}