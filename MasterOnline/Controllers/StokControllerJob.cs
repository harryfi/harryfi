﻿using System;
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

namespace MasterOnline.Controllers
{
    public class NotifyOnFailed : JobFilterAttribute,
    IElectStateFilter
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
            var failedState = context.CandidateState as FailedState;
            if (failedState != null)
            {
                string dbPathEra = Convert.ToString(context.BackgroundJob.Job.Args[0]);// mengambil dbPathEra 
                string subjectDescription = Convert.ToString(context.BackgroundJob.Job.Args[1]); //mengambil Subject
                var jobId = context.BackgroundJob.Id;
                var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                contextNotif.Clients.Group(dbPathEra).moFailedJobs(this._deskripsi, subjectDescription, jobId);
            }
        }

        //public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        //{
        //    Logger.InfoFormat(
        //        "Job `{0}` state was changed from `{1}` to `{2}`",
        //        context.BackgroundJob.Id,
        //        context.OldStateName,
        //        context.NewState.Name);
        //}

        //public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        //{
        //    Logger.InfoFormat(
        //        "Job `{0}` state `{1}` was unapplied.",
        //        context.BackgroundJob.Id,
        //        context.OldStateName);
        //}
    }

    public class StokControllerJob : Controller
    {
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);//string auth = Base64Encode();
        private MoDbContext MoDbContext { get; set; }
        private ErasoftContext ErasoftDbContext { get; set; }
        private DatabaseSQL EDB;
        private string username;
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
        }
        protected void SetupContext(string DatabasePathErasoft, string uname)
        {
            MoDbContext = new MoDbContext();
            ErasoftDbContext = new ErasoftContext(DatabasePathErasoft);
            EDB = new DatabaseSQL(DatabasePathErasoft);
            username = uname;
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
        public void testFailedNotif(string dbPathEra, string namaObj)
        {
            //var a = namaObj.Substring(0, 30);
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
                        ErasoftDbContext.API_LOG_MARKETPLACE.Add(apiLog);
                        ErasoftDbContext.SaveChanges();
                    }
                    break;
                case api_status.Success:
                    {
                        var apiLogInDb = ErasoftDbContext.API_LOG_MARKETPLACE.Where(p => p.REQUEST_ID == data.REQUEST_ID).SingleOrDefault();
                        if (apiLogInDb != null)
                        {
                            apiLogInDb.REQUEST_STATUS = "Success";
                            apiLogInDb.REQUEST_RESULT = data.REQUEST_RESULT;
                            apiLogInDb.REQUEST_EXCEPTION = data.REQUEST_EXCEPTION;
                            ErasoftDbContext.SaveChanges();
                        }
                    }
                    break;
                case api_status.Failed:
                    {
                        var apiLogInDb = ErasoftDbContext.API_LOG_MARKETPLACE.Where(p => p.REQUEST_ID == data.REQUEST_ID).SingleOrDefault();
                        if (apiLogInDb != null)
                        {
                            apiLogInDb.REQUEST_STATUS = "Failed";
                            apiLogInDb.REQUEST_RESULT = data.REQUEST_RESULT;
                            apiLogInDb.REQUEST_EXCEPTION = data.REQUEST_EXCEPTION;
                            ErasoftDbContext.SaveChanges();
                        }
                    }
                    break;
                case api_status.Exception:
                    {
                        var apiLogInDb = ErasoftDbContext.API_LOG_MARKETPLACE.Where(p => p.REQUEST_ID == data.REQUEST_ID).SingleOrDefault();
                        if (apiLogInDb != null)
                        {
                            apiLogInDb.REQUEST_STATUS = "Failed";
                            apiLogInDb.REQUEST_RESULT = "Exception";
                            apiLogInDb.REQUEST_EXCEPTION = data.REQUEST_EXCEPTION;
                            ErasoftDbContext.SaveChanges();
                        }
                    }
                    break;
            }
        }

        public double GetQOHSTF08A(string Barang, string Gudang)
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

        public void updateStockMarketPlace(string connId, string DatabasePathErasoft, string uname)
        {
            SetupContext(DatabasePathErasoft, uname);

            var DataUsaha = ErasoftDbContext.SIFSYS.FirstOrDefault();
            bool doAPI = false;
            if (DataUsaha != null)
            {
                if (DataUsaha.JTRAN_RETUR == "1")
                {
                    doAPI = true;
                }
            }
            if (doAPI)
            {
                var Marketplaces = MoDbContext.Marketplaces;
                var kdBL = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BUKALAPAK").IdMarket;
                var kdLazada = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "LAZADA").IdMarket;
                var kdBli = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BLIBLI").IdMarket;
                var kdElevenia = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "ELEVENIA").IdMarket;
                var kdShopee = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "SHOPEE").IdMarket;
                var kdTokped = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "TOKOPEDIA").IdMarket;
                var kdJD = Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "JD.ID").IdMarket;

                string EDBConnID = EDB.GetConnectionString("ConnId");
                var sqlStorage = new SqlServerStorage(EDBConnID);

                var client = new BackgroundJobClient(sqlStorage);

                var TEMP_ALL_MP_ORDER_ITEMs = ErasoftDbContext.Database.SqlQuery<TEMP_ALL_MP_ORDER_ITEM>("SELECT * FROM TEMP_ALL_MP_ORDER_ITEM WHERE CONN_ID = '" + connId + "'").ToList();

                List<string> listBrg = new List<string>();
                foreach (var item in TEMP_ALL_MP_ORDER_ITEMs)
                {
                    listBrg.Add(item.BRG);
                }

                foreach (string kdBrg in listBrg)
                {
                    //var qtyOnHand = GetQOHSTF08A(kdBrg, "ALL");
                    var barangInDb = ErasoftDbContext.STF02.SingleOrDefault(b => b.BRG == kdBrg);
                    var brgMarketplace = ErasoftDbContext.STF02H.Where(p => p.BRG == kdBrg && !string.IsNullOrEmpty(p.BRG_MP)).ToList();
                    var ListARF01 = ErasoftDbContext.ARF01.ToList();

                    foreach (var stf02h in brgMarketplace)
                    {
                        var marketPlace = ListARF01.SingleOrDefault(p => p.RecNum == stf02h.IDMARKET);
                        if (marketPlace.NAMA.Equals(kdBL.ToString()))
                        {
                            //blApi.updateProduk(kdBrg, stf02h.BRG_MP, "", (qtyOnHand > 0) ? qtyOnHand.ToString() : "0", marketPlace.API_KEY, marketPlace.TOKEN);
                            client.Enqueue<StokControllerJob>(x => x.Bukalapak_updateStock(DatabasePathErasoft, kdBrg, stf02h.BRG_MP, "", "", marketPlace.API_KEY, marketPlace.TOKEN, uname, null));
                        }
                        else if (marketPlace.NAMA.Equals(kdLazada.ToString()))
                        {
                            //lzdApi.UpdatePriceQuantity(stf02h.BRG_MP, "", (qtyOnHand > 0) ? qtyOnHand.ToString() : "0", marketPlace.TOKEN);
                            client.Enqueue<StokControllerJob>(x => x.Lazada_updateStock(DatabasePathErasoft, stf02h.BRG, stf02h.BRG_MP, "", "", marketPlace.TOKEN, uname, null));
                        }
                        else if (marketPlace.NAMA.Equals(kdElevenia.ToString()))
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
                            client.Enqueue<StokControllerJob>(x => x.Elevenia_updateStock(DatabasePathErasoft, stf02h.BRG, data, uname, null));
                        }
                        else if (marketPlace.NAMA.Equals(kdBli.ToString()))
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
                                    idmarket = marketPlace.RecNum.Value
                                };
                                BlibliProductData data = new BlibliProductData
                                {
                                    kode = kdBrg,
                                    kode_mp = stf02h.BRG_MP,
                                    Qty = "",
                                    MinQty = "0"
                                };
                                data.Price = stf02h.HJUAL.ToString();
                                data.MarketPrice = stf02h.HJUAL.ToString();
                                var display = Convert.ToBoolean(stf02h.DISPLAY);
                                data.display = display ? "true" : "false";
                                var BliApi = new BlibliController();
                                //Task.Run(() => BliApi.UpdateProdukQOH_Display(iden, data).Wait());
                                client.Enqueue<StokControllerJob>(x => x.Blibli_updateStock(DatabasePathErasoft, stf02h.BRG, iden, data, uname, null));
                            }
                        }
                        //add by calvin 18 desember 2018
                        else if (marketPlace.NAMA.Equals(kdTokped.ToString()))
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
                                    if (stf02h.BRG_MP.Contains("PENDING"))
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
                                        //Task.Run(() => TokoAPI.UpdateStock(iden, Convert.ToInt32(stf02h.BRG_MP), Convert.ToInt32(qtyOnHand))).Wait();
                                        client.Enqueue<StokControllerJob>(x => x.Tokped_updateStock(DatabasePathErasoft, stf02h.BRG, iden, Convert.ToInt32(stf02h.BRG_MP), 0, uname, null));
                                    }
                                }
                            }
                        }
                        else if (marketPlace.NAMA.Equals(kdShopee.ToString()))
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
                                    if (brg_mp[1] == "0")
                                    {
                                        //Task.Run(() => ShopeeApi.UpdateStock(data, stf02h.BRG_MP, Convert.ToInt32(qtyOnHand))).Wait();
                                        client.Enqueue<StokControllerJob>(x => x.Shopee_updateStock(DatabasePathErasoft, stf02h.BRG, data, stf02h.BRG_MP, 0, uname, null));
                                    }
                                    else if (brg_mp[1] != "")
                                    {
                                        //Task.Run(() => ShopeeApi.UpdateVariationStock(data, stf02h.BRG_MP, Convert.ToInt32(qtyOnHand))).Wait();
                                        client.Enqueue<StokControllerJob>(x => x.Shopee_updateVariationStock(DatabasePathErasoft, stf02h.BRG, data, stf02h.BRG_MP, 0, uname, null));
                                    }
                                }
                            }
                        }
                        //end add by calvin 18 desember 2018
                        //add by Tri 11 April 2019
                        else if (marketPlace.NAMA.Equals(kdJD.ToString()))
                        {
                            JDIDAPIData data = new JDIDAPIData()
                            {
                                accessToken = marketPlace.TOKEN,
                                appKey = marketPlace.API_KEY,
                                appSecret = marketPlace.API_CLIENT_U,
                            };
                            if (stf02h.BRG_MP != "")
                            {
                                //Task.Run(() => ShopeeApi.UpdateStock(data, stf02h.BRG_MP, Convert.ToInt32(qtyOnHand))).Wait();
                                client.Enqueue<StokControllerJob>(x => x.JD_updateStock(DatabasePathErasoft, stf02h.BRG, data, stf02h.BRG_MP, 0, uname, null));
                            }
                        }
                        //end add by Tri 11 April 2019

                    }
                }
                EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "DELETE FROM TEMP_ALL_MP_ORDER_ITEM WHERE CONN_ID = '" + connId + "'");
            }
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_update_stok")]
        [NotifyOnFailed("Update Stock {obj} ke Bukalapak gagal.")]
        public void Bukalapak_updateStock(string DatabasePathErasoft, string brg, string brgMp, string price, string stock, string userId, string token, string uname, PerformContext context)
        {
            SetupContext(DatabasePathErasoft, uname);

            var qtyOnHand = GetQOHSTF08A(brg, "ALL");
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
        [NotifyOnFailed("Update Stock {obj} ke Lazada gagal.")]
        public BindingBase Lazada_updateStock(string DatabasePathErasoft, string stf02_brg, string kdBrg, string harga, string qty, string token, string uname, PerformContext context)
        {
            SetupContext(DatabasePathErasoft, uname);
            string urlLazada = "https://api.lazada.co.id/rest";
            string eraAppKey = "101775";
            string eraAppSecret = "QwUJjjtZ3eCy2qaz6Rv1PEXPyPaPkDSu";

            var qtyOnHand = GetQOHSTF08A(stf02_brg, "ALL");
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
            request.SetApiName("/product/item/get");
            request.SetHttpMethod("GET");
            request.AddApiParameter("seller_sku", kdBrg);
            LazopResponse response = client.Execute(request, token);
            dynamic resStok = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body);
            if (resStok.code == "0")
            {
                int stok = Convert.ToInt32(resStok.data.skus[0].quantity);
                int stokAvaliable = Convert.ToInt32(resStok.data.skus[0].Available);
                qty = (Convert.ToInt32(qty) + (stok - stokAvaliable)).ToString();
            }

            //end add 12-04-2019, cek qty on lazada

            string xmlString = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?><Request><Product>";
            xmlString += "<Skus><Sku><SellerSku>" + kdBrg + "</SellerSku>";
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
                response = client.Execute(request, token);
                var res = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaResponseObj)) as LazadaResponseObj;
                if (res.code.Equals("0"))
                {
                    ret.status = 1;
                    //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, token, currentLog);
                }
                else
                {
                    ret.message = res.detail[0].message;
                    //currentLog.REQUEST_EXCEPTION = res.detail[0].message;
                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, token, currentLog);
                }
            }
            catch (Exception ex)
            {
                ret.message = ex.ToString();
                //currentLog.REQUEST_EXCEPTION = ex.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, token, currentLog);
            }

            return ret;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_update_stok")]
        [NotifyOnFailed("Update Stock {obj} ke Elevenia gagal.")]
        public ClientMessage Elevenia_updateStock(string DatabasePathErasoft, string stf02_brg, EleveniaProductData data, string uname, PerformContext context)
        {
            SetupContext(DatabasePathErasoft, uname);

            var qtyOnHand = GetQOHSTF08A(stf02_brg, "ALL");
            string stock = (qtyOnHand > 0) ? qtyOnHand.ToString() : "0";
            data.Qty = stock;

            var ret = new ClientMessage();
            string auth = data.api_key;

            Utils.HttpRequest req = new Utils.HttpRequest();
            long milis = BlibliController.CurrentTimeMillis();
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

            string sSQL = "SELECT * FROM (";
            for (int i = 1; i <= 30; i++)
            {
                sSQL += "SELECT A.ACODE_" + i.ToString() + " AS ATTRIBUTE_CODE,A.ANAME_" + i.ToString() + " AS ATTRIBUTE_NAME,B.ATYPE_" + i.ToString() + " AS ATTRIBUTE_ID,A.AVALUE_" + i.ToString() + " AS VALUE FROM STF02H A INNER JOIN MO.DBO.ATTRIBUTE_ELEVENIA B ON A.CATEGORY_CODE = B.CATEGORY_CODE WHERE A.BRG='" + data.kode + "' AND A.IDMARKET = '" + data.IDMarket + "' " + System.Environment.NewLine;
                if (i < 30)
                {
                    sSQL += "UNION ALL " + System.Environment.NewLine;
                }
            }
            DataSet dsAttribute = EDB.GetDataSet("sCon", "STF02H", sSQL + ") ASD WHERE ISNULL(ATTRIBUTE_CODE,'') <> ''");
            int data_idmarket = Convert.ToInt32(data.IDMarket);
            var nilaiStf02h = (from p in ErasoftDbContext.STF02H where p.BRG == data.kode && p.IDMARKET == data_idmarket select p).FirstOrDefault();
            xmlString += "<dispCtgrNo>" + nilaiStf02h.CATEGORY_CODE + "</dispCtgrNo>";//category id //5475 = Hobi lain lain

            for (int i = 0; i < dsAttribute.Tables[0].Rows.Count; i++)
            {
                xmlString += "<ProductCtgrAttribute><prdAttrCd><![CDATA[" + Convert.ToString(dsAttribute.Tables[0].Rows[i]["ATTRIBUTE_CODE"]) + "]]></prdAttrCd>";//category attribute code
                xmlString += "<prdAttrNm><![CDATA[" + Convert.ToString(dsAttribute.Tables[0].Rows[i]["ATTRIBUTE_NAME"]) + "]]></prdAttrNm>";//category attribute name i.e: brand, model, type, ISBN
                xmlString += "<prdAttrNo><![CDATA[" + Convert.ToString(dsAttribute.Tables[0].Rows[i]["ATTRIBUTE_ID"]) + "]]></prdAttrNo>";//category attribute id
                xmlString += "<prdAttrVal><![CDATA[" + Convert.ToString(dsAttribute.Tables[0].Rows[i]["VALUE"]) + "]]></prdAttrVal></ProductCtgrAttribute>";//category attribute value
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

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_update_stok")]
        [NotifyOnFailed("Update Stock {obj} ke Blibli gagal.")]
        public async Task<string> Blibli_updateStock(string DatabasePathErasoft, string stf02_brg, BlibliAPIData iden, BlibliProductData data, string uname, PerformContext context)
        {
            string ret = "";
            SetupContext(DatabasePathErasoft, uname);

            var qtyOnHand = GetQOHSTF08A(stf02_brg, "ALL");
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
            string[] brg_mp = data.kode_mp.Split(';');
            if (brg_mp.Length == 2)
            {
                string urll_1 = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/detailProduct?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&gdnSku=" + Uri.EscapeDataString(brg_mp[0]) + "&channelId=MasterOnline";

                HttpWebRequest myReq_1 = (HttpWebRequest)WebRequest.Create(urll_1);
                myReq_1.Method = "GET";
                myReq_1.Headers.Add("Authorization", ("bearer " + iden.token));
                myReq_1.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature_1));
                myReq_1.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq_1.Accept = "application/json";
                myReq_1.ContentType = "application/json";
                myReq_1.Headers.Add("requestId", "MasterOnline-" + milis.ToString());
                myReq_1.Headers.Add("sessionId", milis.ToString());
                myReq_1.Headers.Add("username", userMTA);
                string responseFromServer_1 = "";
                //try
                //{
                using (WebResponse response = await myReq_1.GetResponseAsync())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseFromServer_1 = reader.ReadToEnd();
                    }
                }
                //}
                //catch (Exception ex)
                //{
                //}
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
                                    //if (QOHBlibli != 0)
                                    {
                                        myData += "{";
                                        myData += "\"gdnSku\": \"" + brg_mp[0] + "\",  ";
                                        myData += "\"stock\": " + Convert.ToString(QOHBlibli) + ", ";
                                        myData += "\"minimumStock\": " + data.MinQty + ", ";
                                        myData += "\"price\": " + data.MarketPrice + ", ";
                                        myData += "\"salePrice\": " + data.MarketPrice + ", ";// harga yg tercantum di display blibli
                                                                                              //myData += "\"salePrice\": " + item.sellingPrice + ", ";// harga yg promo di blibli
                                        myData += "\"buyable\": " + data.display + ", ";
                                        myData += "\"displayable\": " + data.display + " "; // true=tampil    
                                        myData += "},";
                                    }
                                }
                            }
                            myData = myData.Remove(myData.Length - 1);
                            myData += "]";
                            myData += "}";

                            string signature = CreateTokenBlibli("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/updateProduct", iden.API_secret_key);
                            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/updateProduct?channelId=MasterOnline";

                            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                            //{
                            //    REQUEST_ID = milis.ToString(),
                            //    REQUEST_ACTION = "Update QOH dan Display",
                            //    REQUEST_DATETIME = milisBack,
                            //    REQUEST_ATTRIBUTE_1 = data.kode,
                            //    REQUEST_ATTRIBUTE_2 = brg_mp[1], //product_code
                            //    REQUEST_ATTRIBUTE_3 = brg_mp[0], //gdnsku
                            //    REQUEST_STATUS = "Pending",
                            //};
                            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

                            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                            myReq.Method = "POST";
                            myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                            myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                            myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                            myReq.Accept = "application/json";
                            myReq.ContentType = "application/json";
                            myReq.Headers.Add("requestId", "MasterOnline-" + milis.ToString());
                            myReq.Headers.Add("sessionId", milis.ToString());
                            myReq.Headers.Add("username", userMTA);
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
                            //}
                            //catch (Exception ex)
                            //{
                            //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                            //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);

                            //}
                            if (responseFromServer != null)
                            {
                                dynamic result2 = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                                if (string.IsNullOrEmpty(result2.errorCode.Value))
                                {
                                    //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);

                                    //remark by calvin 2 april 2019
                                    //BlibliQueueFeedData queueData = new BlibliQueueFeedData
                                    //{
                                    //    request_id = result2.requestId.Value,
                                    //    log_request_id = currentLog.REQUEST_ID
                                    //};
                                    //await GetQueueFeedDetail(iden, queueData);
                                    //end remark by calvin 2 april 2019
                                }
                                else
                                {
                                    //currentLog.REQUEST_RESULT = Convert.ToString(result.errorCode);
                                    //currentLog.REQUEST_EXCEPTION = Convert.ToString(result.errorMessage);
                                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            return ret;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_update_stok")]
        [NotifyOnFailed("Update Stock {obj} ke Tokopedia gagal.")]
        public async Task<string> Tokped_updateStock(string DatabasePathErasoft, string stf02_brg, TokopediaAPIData iden, int product_id, int stok, string uname, PerformContext context)
        {
            SetupContext(DatabasePathErasoft, uname);

            var qtyOnHand = GetQOHSTF08A(stf02_brg, "ALL");
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
            //}
            //catch (Exception ex)
            //{

            //}
            return "";
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_update_stok")]
        [NotifyOnFailed("Update Stock {obj} ke Shopee gagal.")]
        public async Task<string> Shopee_updateStock(string DatabasePathErasoft, string stf02_brg, ShopeeAPIData iden, string brg_mp, int qty, string uname, PerformContext context)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            SetupContext(DatabasePathErasoft, uname);

            var qtyOnHand = GetQOHSTF08A(stf02_brg, "ALL");
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
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            //}
            //catch (Exception ex)
            //{
            //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}

            //if (responseFromServer != null)
            //{
            //    try
            //    {
            //        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
            //    }
            //    catch (Exception ex2)
            //    {
            //        currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
            //        manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //    }
            //}
            return ret;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_update_stok")]
        [NotifyOnFailed("Update Stock {obj} ke Shopee gagal.")]
        public async Task<string> Shopee_updateVariationStock(string DatabasePathErasoft, string stf02_brg, ShopeeAPIData iden, string brg_mp, int qty, string uname, PerformContext context)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            SetupContext(DatabasePathErasoft, uname);

            var qtyOnHand = GetQOHSTF08A(stf02_brg, "ALL");
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

            //if (responseFromServer != null)
            //{
            //    try
            //    {
            //        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
            //    }
            //    catch (Exception ex2)
            //    {
            //        currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
            //        manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //    }
            //}
            return ret;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_update_stok")]
        [NotifyOnFailed("Update Stock {obj} ke JD.ID gagal.")]
        public async Task<string> JD_updateStock(string DatabasePathErasoft, string stf02_brg, JDIDAPIData data, string id, int stok, string uname, PerformContext context)
        {
            SetupContext(DatabasePathErasoft, uname);

            var qtyOnHand = GetQOHSTF08A(stf02_brg, "ALL");
            stok = Convert.ToInt32(qtyOnHand);

            var mgrApiManager = new JDIDController();

            mgrApiManager.AppKey = data.appKey;
            mgrApiManager.AppSecret = data.appSecret;
            mgrApiManager.AccessToken = data.accessToken;
            mgrApiManager.Method = "epi.ware.openapi.warestock.updateWareStock";
            mgrApiManager.ParamJson = "{\"jsonStr\":[{\"skuId\":" + id + ", \"realNum\": " + stok + "}]}";

            var response = mgrApiManager.Call();
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
                            //currentLog.REQUEST_EXCEPTION = retStok.message;
                            //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        }
                    }
                    else
                    {
                        //currentLog.REQUEST_EXCEPTION = ret.openapi_data;
                        //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                    }
                }
                else
                {
                    //currentLog.REQUEST_EXCEPTION = ret.openapi_data;
                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                }
            }
            else
            {
                //currentLog.REQUEST_EXCEPTION = response;
                //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
            }

            return "";
        }

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
    }
}