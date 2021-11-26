using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Erasoft.Function;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Hangfire.SqlServer;

using System.Net;
using System.IO;


namespace MasterOnline.Controllers
{
    public class PartnerApiController : Controller
    {
        public MoDbContext MoDbContext { get; set; }
        public ErasoftContext ErasoftDbContext { get; set; }
        DatabaseSQL EDB;
        //string username;

        //string dbPathEra = "";
        //string DataSourcePath = "";
        //string dbSourceEra = "";

        // GET: PartnerApi
        public PartnerApiController()
        {

        }

        protected void SetupContext(string dbPathEra, string dbSourceEra)
        {
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(dbPathEra);
            ErasoftDbContext = new ErasoftContext(dbSourceEra, dbPathEra);
        }

        public class TokenClass
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string refresh_token { get; set; }
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

        public async Task<string> TadaAuthorization(PartnerApiData data)
        {
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
                var date_expires = DateTime.UtcNow.AddHours(7).AddDays((response_token.expires_in) / 86400).ToString("yyyy-MM-dd HH:mm:ss");// +1
                ErasoftDbContext.Database.ExecuteSqlCommand("UPDATE PARTNER_API SET Access_Token = '" + response_token.access_token + "', Session_ExpiredDate = '" + date_expires + "' WHERE PartnerId = 30007 ");

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
            return "";
        }

        #region deactivate
        //public async Task<string> DeactivateHangfireTADA(string dbSourceEra, string dbPathEra)
        //{
        //    string ret = "";
        //    SetupContext(dbPathEra, dbSourceEra);

        //    string EDBConnID = EDB.GetConnectionString("ConnId");
        //    var sqlStorage = new SqlServerStorage(EDBConnID);
        //    var client = new BackgroundJobClient(sqlStorage);

        //    RecurringJobManager recurJobM = new RecurringJobManager(sqlStorage);
        //    RecurringJobOptions recurJobOpt = new RecurringJobOptions()
        //    {
        //        QueueName = "1_manage_pesanan",
        //    };

        //    var connection_id_topup_by_phone = dbPathEra + "_job_tada_topup_by_phone";

        //    recurJobM.RemoveIfExists(connection_id_topup_by_phone);
        //    return "";
        //}
        #endregion

        public async Task<string> TadaTopupByPhone(string dbSourceEra, string dbPathEra)
        {
            //var ret = new BindDownloadExcel
            //{
            //    Errors = new List<string>()
            //};
            string ret = "";
            SetupContext(dbPathEra, dbSourceEra);

            try
            {
                string EDBConnID = EDB.GetConnectionString("ConnId");
                var sqlStorage = new SqlServerStorage(EDBConnID);
                var client = new BackgroundJobClient(sqlStorage);

                RecurringJobManager recurJobM = new RecurringJobManager(sqlStorage);
                RecurringJobOptions recurJobOpt = new RecurringJobOptions()
                {
                    QueueName = "1_manage_pesanan",
                };

                var connection_id_topup_by_phone = dbPathEra + "_job_tada_topup_by_phone";

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
                    await TadaAuthorization(data);
                }


                if (partnerDb != null && partnerDb.Status == true)
                {
                    string dateFrom = DateTime.UtcNow.AddDays(-1).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
                    string dateTo = DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");

                    string msgError = "Data Pesanan dari tanggal " + dateFrom + " sampai tanggal " + dateTo + ", kosong.";
#if (DEBUG || Debug_AWS)
                    var job = new PartnerApiControllerJob();
                    Task.Run(() => job.TADATopupByPhoneJob(dbPathEra, msgError, "000000", "API", "TADA_TOPUP_BY_PHONE", dbSourceEra).Wait());
#else
                    //hangfire jalan jam 02:00 wib
                    recurJobM.RemoveIfExists(connection_id_topup_by_phone);
                    recurJobM.AddOrUpdate(connection_id_topup_by_phone, Hangfire.Common.Job.FromExpression<PartnerApiControllerJob>(x => x.TADATopupByPhoneJob(dbPathEra, msgError, "000000", "API", "TOPUP_BY_PHONE", dbSourceEra)), "0 19 * * *", recurJobOpt); //Cron.Daily(jam1)
                    
                    //utk testing hangfire di dev
                    //recurJobM.AddOrUpdate(connection_id_topup_by_phone, Hangfire.Common.Job.FromExpression<PartnerApiControllerJob>(x => x.TADATopupByPhoneJob(dbPathEra, msgError, "000000", "API", "TOPUP_BY_PHONE", dbSourceEra)), "0 8 * * *", recurJobOpt);
#endif
                }
                else
                {
                    recurJobM.RemoveIfExists(connection_id_topup_by_phone);
                }
            }
            catch (Exception ex)
            {
                //ret.Errors.Add(ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }

            return ret;

        }
    }
}