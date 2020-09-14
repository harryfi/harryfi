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

namespace MasterOnline.Controllers
{
    public class MasterOnlineController : Controller
    {
        private MoDbContext MoDbContext { get; set; }
        private ErasoftContext ErasoftDbContext { get; set; }
        private DatabaseSQL EDB;
        private string username;
        // GET: MasterOnline
        public ActionResult Index()
        {
            return View();
        }
        public MasterOnlineController()
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

        [AutomaticRetry(Attempts = 0)]
        [Queue("3_general")]
        public async Task<string> UpdateHJulaMassal(string dbPathEra, string nobuk, string log_CUST, string log_ActionCategory, string log_ActionName, int indexFile, string user_name)
        {
            //if merchant code diisi. barulah GetOrderList
            string ret = "";
            SetupContext(dbPathEra, user_name);

            string sSQL = "UPDATE S SET HJUAL = T.HJUAL ";
            var sSQL2 = "FROM TEMP_UPDATE_HJUAL T INNER JOIN STF02H S ON T.BRG = S.BRG AND T.IDMARKET = S.IDMARKET ";
            sSQL2 += "WHERE INDEX_FILE = " + indexFile;

            var result = EDB.ExecuteSQL("CString", CommandType.Text, sSQL + sSQL2);
            if(result > 0)
            {
                var customer = ErasoftDbContext.ARF01.Where(m => m.CUST == log_CUST).FirstOrDefault();
                if(customer != null)
                {
                    var dsUpdate = EDB.GetDataSet("", "STF02H", "SELECT T.BRG, T.BRG_MP, T.HJUAL, DISPLAY " + sSQL2 + " AND ISNULL(BRG_MP, '') <> ''");
                    if(dsUpdate.Tables[0].Rows.Count > 0)
                    {
                        string EDBConnID = EDB.GetConnectionString("ConnId");
                        var sqlStorage = new SqlServerStorage(EDBConnID);

                        var clientJobServer = new BackgroundJobClient(sqlStorage);

                        switch (customer.NAMA)
                        {
                            case "7"://LAZADA
                                for(int i = 0; i < dsUpdate.Tables[0].Rows.Count; i++)
                                {
                                    clientJobServer.Enqueue<LazadaControllerJob>(x => x.UpdatePrice_Job(dbPathEra, dsUpdate.Tables[0].Rows[i]["BRG"].ToString(), log_CUST, "Price", "CHILD_" + log_ActionName,
                                       dsUpdate.Tables[0].Rows[i]["BRG_MP"].ToString(), dsUpdate.Tables[0].Rows[i]["HJUAL"].ToString(), customer.TOKEN, username));
                                }
                                break;
                            case "9"://ELEVENIA
                                //for (int i = 0; i < dsUpdate.Tables[0].Rows.Count; i++)
                                //{
                                //    var brg = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").SingleOrDefault(b => b.BRG == dsUpdate.Tables[0].Rows[i]["BRG"].ToString());

                                //    string[] imgID = new string[3]; 
                                //    for (int j = 0; j < 3; j++)
                                //    {
                                //        switch (j)
                                //        {
                                //            case 0:
                                //                imgID[0] = brg.LINK_GAMBAR_1;
                                //                break;
                                //            case 1:
                                //                imgID[1] = brg.LINK_GAMBAR_2;
                                //                break;
                                //            case 2:
                                //                imgID[2] = brg.LINK_GAMBAR_3;
                                //                break;
                                //        }
                                //    }
                                //    //end change by calvin 4 desember 2018
                                //    EleveniaController.EleveniaProductData data = new EleveniaController.EleveniaProductData
                                //    {
                                //        api_key = customer.API_KEY,
                                //        kode = dsUpdate.Tables[0].Rows[i]["BRG"].ToString(),
                                //        nama = brg.NAMA + ' ' + brg.NAMA2 + ' ' + brg.NAMA3,
                                //        berat = (brg.BERAT / 1000).ToString(),//MO save dalam Gram, Elevenia dalam Kilogram
                                //        imgUrl = imgID,
                                //        Keterangan = brg.Deskripsi,
                                //        Qty = Convert.ToString(qtyOnHand),
                                //        DeliveryTempNo = hJualInDb.DeliveryTempElevenia.ToString(),
                                //        IDMarket = customer.RecNum.ToString(),
                                //    };
                                //    data.Brand = ErasoftDbContext.STF02E.SingleOrDefault(m => m.KODE == brg.Sort2 && m.LEVEL == "2").KET;
                                //    data.Price = hargaJualBaru.ToString();
                                //    data.kode_mp = hJualInDb.BRG_MP;

                                //    var display = Convert.ToBoolean(hJualInDb.DISPLAY);
                                //    if (!string.IsNullOrEmpty(data.kode_mp))
                                //    {
                                //        var result = new EleveniaController().UpdateProduct(data);
                                //    }
                                //}
                                break;
                            case "15"://tokopedia
                                TokopediaControllerJob.TokopediaAPIData iden = new TokopediaControllerJob.TokopediaAPIData()
                                {
                                    merchant_code = customer.Sort1_Cust, //FSID
                                    API_client_password = customer.API_CLIENT_P, //Client ID
                                    API_client_username = customer.API_CLIENT_U, //Client Secret
                                    API_secret_key = customer.API_KEY, //Shop ID 
                                    token = customer.TOKEN,
                                    idmarket = customer.RecNum.Value,
                                    DatabasePathErasoft = dbPathEra,
                                    username = username
                                };
                                for (int i = 0; i < dsUpdate.Tables[0].Rows.Count; i++)
                                {
                                    if (dsUpdate.Tables[0].Rows[i]["BRG_MP"].ToString().Contains("PENDING") || dsUpdate.Tables[0].Rows[i]["BRG_MP"].ToString().Contains("PEDITENDING"))
                                    {

                                    }
                                    else
                                    {
                                        clientJobServer.Enqueue<TokopediaControllerJob>(x => x.UpdatePrice_Job(dbPathEra, dsUpdate.Tables[0].Rows[i]["BRG"].ToString(), customer.CUST, "Price", "CHILD_" + log_ActionName, 
                                            Convert.ToInt32(dsUpdate.Tables[0].Rows[i]["BRG_MP"].ToString()), iden, Convert.ToInt32(dsUpdate.Tables[0].Rows[i]["HJUAL"].ToString())));
                                    }
                                }
                                    
                                    break;
                            case "16"://BLIBLI

                                BlibliControllerJob.BlibliAPIData idenJob = new BlibliControllerJob.BlibliAPIData
                                {
                                    merchant_code = customer.Sort1_Cust,
                                    API_client_password = customer.API_CLIENT_P,
                                    API_client_username = customer.API_CLIENT_U,
                                    API_secret_key = customer.API_KEY,
                                    //API_client_password = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk",
                                    //API_secret_key = "2232587F9E9C2A58E8C75BBF8DF302D43B209E0E9F66C60756FFB0E7F16DFD8F",
                                    token = customer.TOKEN,
                                    mta_username_email_merchant = customer.EMAIL,
                                    mta_password_password_merchant = customer.PASSWORD,
                                    idmarket = customer.RecNum.Value,
                                    DatabasePathErasoft = dbPathEra,
                                    versiToken = customer.KD_ANALISA
                                };
                                for (int i = 0; i < dsUpdate.Tables[0].Rows.Count; i++)
                                {
                                    BlibliControllerJob.BlibliProductData dataJob = new BlibliControllerJob.BlibliProductData
                                    {
                                        kode = dsUpdate.Tables[0].Rows[i]["BRG"].ToString(),
                                        kode_mp = dsUpdate.Tables[0].Rows[i]["BRG_MP"].ToString()
                                    };
                                    var brg = ErasoftDbContext.STF02.Where(a => a.TYPE == "3").SingleOrDefault(b => b.BRG == dsUpdate.Tables[0].Rows[i]["BRG"].ToString());
                                    dataJob.Price = brg.HJUAL.ToString();
                                    if (!string.IsNullOrEmpty(brg.PART))
                                    {
                                        brg = ErasoftDbContext.STF02.Where(m => m.BRG == brg.PART).FirstOrDefault();
                                        if(brg != null)
                                        {
                                            dataJob.Price = brg.HJUAL.ToString();

                                        }
                                    }
                                    dataJob.MarketPrice = dsUpdate.Tables[0].Rows[i]["HJUAL"].ToString();
                                    var displayJob = Convert.ToBoolean(dsUpdate.Tables[0].Rows[i]["DISPLAY"].ToString());
                                    dataJob.display = displayJob ? "true" : "false";
                                    clientJobServer.Enqueue<BlibliControllerJob>(x => x.UpdateProdukQOH_Display_Job(dbPathEra, dataJob.kode, customer.CUST, "Price", "CHILD_" + log_ActionName, dataJob.kode_mp, idenJob, dataJob));
                                }
                                break;
                            case "17":
                                if (!string.IsNullOrWhiteSpace(customer.Sort1_Cust))
                                {
                                    ShopeeControllerJob.ShopeeAPIData dataJob = new ShopeeControllerJob.ShopeeAPIData()
                                    {
                                        merchant_code = customer.Sort1_Cust,
                                        DatabasePathErasoft = dbPathEra,
                                        username = username
                                    };
                                    for (int i = 0; i < dsUpdate.Tables[0].Rows.Count; i++)
                                    {
                                        string[] brg_mp = dsUpdate.Tables[0].Rows[i]["BRG_MP"].ToString().Split(';');
                                        if (brg_mp.Count() == 2)
                                        {

                                            var ShopeeApiJob = new ShopeeControllerJob();
                                            var hargaJualBaru = Convert.ToDouble(dsUpdate.Tables[0].Rows[i]["HJUAL"].ToString());



                                            if (brg_mp[1] == "0")
                                            {
                                                clientJobServer.Enqueue<ShopeeControllerJob>(x => x.UpdatePrice_Job(dbPathEra, dsUpdate.Tables[0].Rows[i]["BRG"].ToString(), customer.CUST, "Price", "CHILD_" + log_ActionName, dsUpdate.Tables[0].Rows[i]["BRG_MP"].ToString(), dataJob, (float)hargaJualBaru));

                                            }
                                            else if (brg_mp[1] != "")
                                            {
                                                clientJobServer.Enqueue<ShopeeControllerJob>(x => x.UpdateVariationPrice_Job(dbPathEra, dsUpdate.Tables[0].Rows[i]["BRG"].ToString(), customer.CUST, "Price", "CHILD_" + log_ActionName, dsUpdate.Tables[0].Rows[i]["BRG_MP"].ToString(), dataJob, (float)hargaJualBaru));
                                
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            }


            return ret;

        }
    }
}