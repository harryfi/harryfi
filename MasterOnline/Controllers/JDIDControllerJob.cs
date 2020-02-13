using Erasoft.Function;
using Hangfire;
using MasterOnline.Models;
using MasterOnline.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;

namespace MasterOnline.Controllers
{
    public class JDIDControllerJob : ApiController
    {
        AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        public MoDbContext MoDbContext { get; set; }
        public string imageUrl = "https://img20.jd.id/Indonesia/s300x300_/";
        //public string ServerUrl = "https://open.jd.id/api";
        //public string AccessToken = "";
        //public string AppKey = "";
        //public string AppSecret = "";
        //public string Version = "1.0";
        //public string Format = "json";
        //public string SignMethod = "md5";
        //private string Charset_utf8 = "UTF-8";
        //public string Method;
        //public string ParamJson;
        //public string ParamFile;
        protected List<long> listCategory = new List<long>();
        public List<Model_Brand> listBrand = new List<Model_Brand>();

        public ErasoftContext ErasoftDbContext { get; set; }
        DatabaseSQL EDB;
        string username;

        public JDIDControllerJob()
        {
            //MoDbContext = new MoDbContext();
            //var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            //if (sessionData?.Account != null)
            //{
            //    if (sessionData.Account.UserId == "admin_manage")
            //        ErasoftDbContext = new ErasoftContext();
            //    else
            //        ErasoftDbContext = new ErasoftContext(sessionData.Account.DatabasePathErasoft);

            //    EDB = new DatabaseSQL(sessionData.Account.DatabasePathErasoft);
            //    username = sessionData.Account.Username;
            //}
            //else
            //{
            //    if (sessionData?.User != null)
            //    {
            //        var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
            //        ErasoftDbContext = new ErasoftContext(accFromUser.DatabasePathErasoft);
            //        EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
            //        username = accFromUser.Username;
            //    }
            //}
        }

        protected void SetupContext(string DatabasePathErasoft, string uname)
        {
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, DatabasePathErasoft);
            username = uname;
        }

        [System.Web.Mvc.HttpGet]
        public void getCategory(JDIDAPIData data)
        {
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Get Category",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = data.accessToken,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data, currentLog);

            var mgrApiManager = new JDIDController();
            mgrApiManager.AppKey = data.appKey;
            mgrApiManager.AppSecret = data.appSecret;
            mgrApiManager.AccessToken = data.accessToken;
            mgrApiManager.Method = "epi.ware.openapi.CategoryApi.getAllCategoryTree";
            mgrApiManager.ParamJson = "";

            try
            {
                var response = mgrApiManager.Call();
                var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
                if (ret != null)
                {
                    if (ret.openapi_code == 0)
                    {
                        var listKategori = JsonConvert.DeserializeObject(ret.openapi_data, typeof(DATA_CAT)) as DATA_CAT;
                        if (listKategori != null)
                        {
                            if (listKategori.success)
                            {
                                EDB.ExecuteSQL("CString", CommandType.Text, "Update ARF01 SET STATUS_API = '1' WHERE TOKEN = '" + data.accessToken + "' AND API_KEY = '" + data.appKey + "'");
                                string dbPath = "";
                                var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
                                if (sessionData?.Account != null)
                                {
                                    dbPath = sessionData.Account.DatabasePathErasoft;
                                }
                                else
                                {
                                    if (sessionData?.User != null)
                                    {
                                        var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                                        dbPath = accFromUser.DatabasePathErasoft;
                                    }
                                }

                                #region connstring
#if AWS
                    string con = "Data Source=localhost;Initial Catalog=" + dbPath + ";Persist Security Info=True;User ID=sa;Password=admin123^";
#elif Debug_AWS
                                string con = "Data Source=13.250.232.74;Initial Catalog=" + dbPath + ";Persist Security Info=True;User ID=sa;Password=admin123^";
#else
                                string con = "Data Source=13.251.222.53;Initial Catalog=" + dbPath + ";Persist Security Info=True;User ID=sa;Password=admin123^";
#endif
                                #endregion
                                using (SqlConnection oConnection = new SqlConnection(con))
                                {
                                    oConnection.Open();
                                    //using (SqlTransaction oTransaction = oConnection.BeginTransaction())
                                    //{
                                    using (SqlCommand oCommand = oConnection.CreateCommand())
                                    {
                                        //oCommand.CommandText = "DELETE FROM [CATEGORY_BLIBLI] WHERE ARF01_SORT1_CUST='" + data.merchant_code + "'";
                                        //oCommand.ExecuteNonQuery();
                                        //oCommand.Transaction = oTransaction;
                                        oCommand.CommandType = CommandType.Text;
                                        oCommand.CommandText = "INSERT INTO [CATEGORY_JDID] ([CATEGORY_CODE], [CATEGORY_NAME], [CATE_STATE], [TYPE], [LEAF], [PARENT_CODE]) VALUES (@CATEGORY_CODE, @CATEGORY_NAME, @CATE_STATE, @TYPE, @LEAF, @PARENT_CODE)";
                                        //oCommand.Parameters.Add(new SqlParameter("@ARF01_SORT1_CUST", SqlDbType.NVarChar, 50));
                                        oCommand.Parameters.Add(new SqlParameter("@CATEGORY_CODE", SqlDbType.NVarChar, 50));
                                        oCommand.Parameters.Add(new SqlParameter("@CATEGORY_NAME", SqlDbType.NVarChar, 250));
                                        oCommand.Parameters.Add(new SqlParameter("@CATE_STATE", SqlDbType.NVarChar, 3));
                                        oCommand.Parameters.Add(new SqlParameter("@TYPE", SqlDbType.NVarChar, 3));
                                        oCommand.Parameters.Add(new SqlParameter("@LEAF", SqlDbType.NVarChar, 1));
                                        oCommand.Parameters.Add(new SqlParameter("@PARENT_CODE", SqlDbType.NVarChar, 50));

                                        //try
                                        //{
                                        foreach (var item in listKategori.model) //foreach parent level top
                                        {
                                            oCommand.Parameters[0].Value = item.cateId;
                                            oCommand.Parameters[1].Value = item.cateName;
                                            oCommand.Parameters[2].Value = item.cateState;
                                            oCommand.Parameters[3].Value = item.type;
                                            oCommand.Parameters[4].Value = "1";
                                            if (item.parentCateVo != null)
                                            {
                                                oCommand.Parameters[5].Value = item.parentCateVo.cateId;
                                            }
                                            else
                                            {
                                                oCommand.Parameters[5].Value = "";
                                            }
                                            if (oCommand.ExecuteNonQuery() > 0)
                                            {
                                                listCategory.Add(item.cateId);
                                                if (item.parentCateVo != null)
                                                {
                                                    if (!listCategory.Contains(item.parentCateVo.cateId))
                                                    {
                                                        RecursiveInsertCategory(oCommand, item.parentCateVo);
                                                    }
                                                }
                                            }
                                            else
                                            {

                                            }
                                        }
                                        //oTransaction.Commit();
                                        //}
                                        //catch (Exception ex)
                                        //{
                                        //    //oTransaction.Rollback();
                                        //}
                                    }
                                }
                            }
                            else
                            {
                                currentLog.REQUEST_EXCEPTION = ret.openapi_data;
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            }
                        }
                        else
                        {
                            currentLog.REQUEST_EXCEPTION = ret.openapi_data;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        }
                    }
                }
                else
                {
                    currentLog.REQUEST_EXCEPTION = response;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                }
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data, currentLog);
            }


        }
        protected void RecursiveInsertCategory(SqlCommand oCommand, Model_Cat item)
        {
            //foreach (var child in categories.Where(p => p.parent_id == parent))
            //{

            oCommand.Parameters[0].Value = item.cateId;
            oCommand.Parameters[1].Value = item.cateName;
            oCommand.Parameters[2].Value = item.cateState;
            oCommand.Parameters[3].Value = item.type;
            oCommand.Parameters[4].Value = "0";
            if (item.parentCateVo != null)
            {
                oCommand.Parameters[5].Value = item.parentCateVo.cateId;

            }
            else
            {
                oCommand.Parameters[5].Value = "";
            }

            if (oCommand.ExecuteNonQuery() > 0)
            {
                listCategory.Add(item.cateId);
                if (item.parentCateVo != null)
                {
                    if (!listCategory.Contains(item.parentCateVo.cateId))
                    {
                        RecursiveInsertCategory(oCommand, item.parentCateVo);
                    }
                }
            }
            else
            {

            }
            //}
        }

        public ATTRIBUTE_JDID getAttribute(JDIDAPIData data, string catId)
        {
            var retAttr = new ATTRIBUTE_JDID();
            var mgrApiManager = new JDIDController();

            mgrApiManager.AppKey = data.appKey;
            mgrApiManager.AppSecret = data.appSecret;
            mgrApiManager.AccessToken = data.accessToken;

            mgrApiManager.Method = "epi.ware.openapi.WareAttributeApi.getAttributesByCatId";
            mgrApiManager.ParamJson = "{ \"catId\":\"" + catId + "\"}";

            var response = mgrApiManager.Call();
            var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
            if (ret != null)
            {
                if (ret.openapi_code == 0)
                {
                    var listAttr = JsonConvert.DeserializeObject(ret.openapi_data, typeof(AttrData)) as AttrData;
                    if (listAttr != null)
                    {
                        if (listAttr.model.Count > 0)
                        {
                            string a = "";
                            int i = 0;
                            retAttr.CATEGORY_CODE = catId;
                            foreach (var attr in listAttr.model)
                            {

                                a = Convert.ToString(i + 1);
                                retAttr["ACODE_" + a] = Convert.ToString(attr.propertyId);
                                retAttr["ATYPE_" + a] = attr.type.ToString();
                                retAttr["ANAME_" + a] = attr.nameEn;
                                i = i + 1;
                            }
                            for (int j = i; j < 20; j++)
                            {
                                a = Convert.ToString(j + 1);
                                retAttr["ACODE_" + a] = "";
                                retAttr["ATYPE_" + a] = "";
                                retAttr["ANAME_" + a] = "";
                            }
                        }

                    }
                }
            }
            //return response;
            //return ret.openapi_data;
            return retAttr;
        }

        public List<ATTRIBUTE_OPT_JDID> getAttributeOpt(JDIDAPIData data, string catId, string attrId, int page)
        {
            var mgrApiManager = new JDIDController();
            var listOpt = new List<ATTRIBUTE_OPT_JDID>();
            mgrApiManager.AppKey = data.appKey;
            mgrApiManager.AppSecret = data.appSecret;
            mgrApiManager.AccessToken = data.accessToken;

            mgrApiManager.Method = "epi.ware.openapi.WareAttributeApi.getAttrValuesByCatIdAndAttrId";
            mgrApiManager.ParamJson = "{ \"catId\":\"" + catId + "\", \"attrId\":\"" + attrId + "\", \"currentPage\":" + page + ", \"pageSize\":20}";

            var response = mgrApiManager.Call();
            var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
            if (ret != null)
            {
                if (ret.openapi_code == 0)
                {
                    var retOpt = JsonConvert.DeserializeObject(ret.openapi_data, typeof(JDID_ATTRIBUTE_OPT)) as JDID_ATTRIBUTE_OPT;
                    if (retOpt != null)
                    {
                        if (retOpt.model != null)
                        {
                            if (retOpt.model.data.Count > 0)
                            {
                                foreach (var opt in retOpt.model.data)
                                {
                                    var newOpt = new ATTRIBUTE_OPT_JDID()
                                    {
                                        ACODE = opt.attributeValueId.ToString(),
                                        OPTION_VALUE = opt.nameEn
                                    };
                                    listOpt.Add(newOpt);
                                }
                                //if(retOpt.model.data.Count == 20)
                                //{
                                var cursiveOpt = getAttributeOpt(data, catId, attrId, page + 1);
                                if (cursiveOpt.Count > 0)
                                {
                                    foreach (var opt2 in cursiveOpt)
                                    {
                                        listOpt.Add(opt2);
                                    }
                                }
                                //}
                            }
                        }

                    }
                }
            }


            return listOpt;
        }

        public BindingBase getListProduct(JDIDAPIData data, int page, string cust, int recordCount)
        {
            var ret = new BindingBase
            {
                status = 0,
                recordCount = recordCount,
            };

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Get Item List",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = (page + 1).ToString(),
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data, currentLog);

            try
            {
                if (listBrand.Count == 0)
                {
                    getShopBrand(data);
                }
                var mgrApiManager = new JDIDController();
                mgrApiManager.AppKey = data.appKey;
                mgrApiManager.AppSecret = data.appSecret;
                mgrApiManager.AccessToken = data.accessToken;
                mgrApiManager.Method = "com.jd.eptid.warecenter.api.ware.WarePlusClient.getWareTinyInfoListByVenderId";
                mgrApiManager.ParamJson = (page + 1) + ",10";

                var response = mgrApiManager.Call();
                var retData = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
                if (retData != null)
                {
                    if (retData.openapi_msg.ToLower() == "success")
                    {
                        var listProd = JsonConvert.DeserializeObject(retData.openapi_data, typeof(Data_ListProd)) as Data_ListProd;
                        if (listProd != null)
                        {
                            if (listProd.success)
                            {
                                string msg = "";
                                bool adaError = false;
                                if (listProd.model.spuInfoVoList.Count > 0)
                                {
                                    ret.status = 1;
                                    int IdMarket = ErasoftDbContext.ARF01.Where(c => c.CUST == cust).FirstOrDefault().RecNum.Value;
                                    if (listProd.model.spuInfoVoList.Count == 10)
                                        ret.message = (page + 1).ToString();

                                    foreach (var item in listProd.model.spuInfoVoList)
                                    {
                                        //product status: 1.online,2.offline,3.punish,4.deleted
                                        if (item.wareStatus == 1 || item.wareStatus == 2)
                                        {
                                            var retProd = GetProduct(data, item, IdMarket, cust);
                                            if (retProd.status == 1)
                                            {
                                                ret.recordCount += retProd.recordCount;
                                            }
                                            else
                                            {
                                                adaError = true;
                                                msg += item.spuId + ":" + retProd.message + "___||___";
                                            }
                                        }

                                    }
                                }
                                if (adaError)
                                {
                                    currentLog.REQUEST_EXCEPTION = msg;
                                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                                }
                                else
                                {
                                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data, currentLog);
                                }
                            }
                            else
                            {
                                ret.message = retData.openapi_data;
                                currentLog.REQUEST_EXCEPTION = ret.message;
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            }
                        }
                        else
                        {
                            ret.message = retData.openapi_data;
                            currentLog.REQUEST_EXCEPTION = ret.message;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        }
                    }
                    else
                    {
                        ret.message = retData.openapi_msg;
                        currentLog.REQUEST_EXCEPTION = ret.message;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                    }
                }
            }
            catch (Exception ex)
            {
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                currentLog.REQUEST_EXCEPTION = ret.message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data, currentLog);
            }
            return ret;
        }

        public BindingBase GetProduct(JDIDAPIData data, Spuinfovolist itemFromList, int IdMarket, string cust)
        {
            var ret = new BindingBase
            {
                status = 0,
            };

            try
            {
                var mgrApiManager = new JDIDController();
                mgrApiManager.AppKey = data.appKey;
                mgrApiManager.AppSecret = data.appSecret;
                mgrApiManager.AccessToken = data.accessToken;
                mgrApiManager.Method = "com.jd.eptid.warecenter.api.ware.WarePlusClient.getSkuInfoBySpuId";
                mgrApiManager.ParamJson = "[" + itemFromList.spuId + "]";

                var response = mgrApiManager.Call();
                var retProd = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
                if (retProd != null)
                {
                    if (retProd.openapi_msg.ToLower() == "success")
                    {
                        var dataProduct = JsonConvert.DeserializeObject(retProd.openapi_data, typeof(ProductData)) as ProductData;
                        if (dataProduct != null)
                        {
                            if (dataProduct.success)
                            {
                                var stf02h_local = ErasoftDbContext.STF02H.Where(m => m.IDMARKET == IdMarket).ToList();
                                var tempBrg_local = ErasoftDbContext.TEMP_BRG_MP.Where(m => m.IDMARKET == IdMarket).ToList();

                                var haveVarian = false;
                                if (dataProduct.model.Count > 1)
                                {
                                    haveVarian = true;
                                }

                                foreach (var item in dataProduct.model)
                                {
                                    var tempbrginDB = new TEMP_BRG_MP();
                                    var brgInDB = new STF02H();

                                    if (haveVarian)
                                    {
                                        //handle parent
                                        string kdBrgInduk = item.spuId.ToString();
                                        bool createParent = false;
                                        tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP) == kdBrgInduk).FirstOrDefault();
                                        brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP) == kdBrgInduk).FirstOrDefault();
                                        if (tempbrginDB == null && brgInDB == null)
                                        {
                                            if (item.skuId == dataProduct.model[0].skuId)
                                                createParent = true;
                                        }
                                        else if (brgInDB != null)
                                        {
                                            kdBrgInduk = brgInDB.BRG;
                                        }
                                        //end handle parent

                                        //foreach (var varian in item.skuIds)
                                        //{
                                        //    tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == varian.ToString().ToUpper()).FirstOrDefault();
                                        //    brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == varian.ToString().ToUpper()).FirstOrDefault();
                                        //    if (tempbrginDB == null && brgInDB == null)
                                        //    {

                                        //    }
                                        //}
                                        //for (int i = 0; i < item.skuIds.Count; i++)
                                        //{
                                        tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == item.skuId.ToString().ToUpper()).FirstOrDefault();
                                        brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == item.skuId.ToString().ToUpper()).FirstOrDefault();
                                        if (tempbrginDB == null && brgInDB == null)
                                        {
                                            var retData = getProductDetail(data, item, kdBrgInduk, createParent, item.skuId.ToString(), cust, IdMarket, itemFromList);
                                            if (retData.status == 1)
                                            {
                                                ret.recordCount += retData.recordCount;
                                                //createParent = false;
                                            }
                                        }
                                        //}

                                    }
                                    else
                                    {
                                        tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == item.skuId.ToString().ToUpper()).FirstOrDefault();
                                        brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == item.skuId.ToString().ToUpper()).FirstOrDefault();
                                        if (tempbrginDB == null && brgInDB == null)
                                        {
                                            var retData = getProductDetail(data, item, "", false, item.skuId.ToString(), cust, IdMarket, itemFromList);
                                            if (retData.status == 1)
                                            {
                                                ret.recordCount += retData.recordCount;
                                            }
                                        }
                                    }

                                }

                                ret.status = 1;
                            }
                            else
                            {
                                ret.message = retProd.openapi_data;
                            }
                        }
                        else
                        {
                            ret.message = retProd.openapi_data;
                        }
                    }
                    else
                    {
                        ret.message = retProd.openapi_msg;
                    }
                }
            }
            catch (Exception ex)
            {
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }

            return ret;
        }

        public BindingBase getProductDetail(JDIDAPIData data, Model_Product item, string kdBrgInduk, bool createParent, string skuId, string cust, int IdMarket, Spuinfovolist itemFromList)
        {
            var ret = new BindingBase
            {
                status = 0,
            };

            try
            {
                var mgrApiManager = new JDIDController();
                mgrApiManager.AppKey = data.appKey;
                mgrApiManager.AppSecret = data.appSecret;
                mgrApiManager.AccessToken = data.accessToken;
                mgrApiManager.Method = "epi.ware.openapi.SkuApi.getSkuBySkuIds";
                //mgrApiManager.ParamJson = "{ \"page\":" + page + ", \"pageSize\":10}";
                mgrApiManager.ParamJson = "{\"skuIds\" : \"" + skuId + "\"}";

                string sSQL = "INSERT INTO TEMP_BRG_MP (BRG_MP, SELLER_SKU, NAMA, NAMA2, BERAT, PANJANG, LEBAR, TINGGI, CUST, Deskripsi, IDMARKET, HJUAL, HJUAL_MP, ";
                sSQL += "DISPLAY, CATEGORY_CODE, CATEGORY_NAME, MEREK, IMAGE, IMAGE2, IMAGE3, KODE_BRG_INDUK, TYPE, ";
                sSQL += "ACODE_1, ANAME_1, AVALUE_1, ACODE_2, ANAME_2, AVALUE_2, ACODE_3, ANAME_3, AVALUE_3, ACODE_4, ANAME_4, AVALUE_4, ACODE_5, ANAME_5, AVALUE_5, ACODE_6, ANAME_6, AVALUE_6, ACODE_7, ANAME_7, AVALUE_7, ACODE_8, ANAME_8, AVALUE_8, ACODE_9, ANAME_9, AVALUE_9, ACODE_10, ANAME_10, AVALUE_10, ";
                sSQL += "ACODE_11, ANAME_11, AVALUE_11, ACODE_12, ANAME_12, AVALUE_12, ACODE_13, ANAME_13, AVALUE_13, ACODE_14, ANAME_14, AVALUE_14, ACODE_15, ANAME_15, AVALUE_15, ACODE_16, ANAME_16, AVALUE_16, ACODE_17, ANAME_17, AVALUE_17, ACODE_18, ANAME_18, AVALUE_18, ACODE_19, ANAME_19, AVALUE_19, ACODE_20, ANAME_20, AVALUE_20) VALUES ";

                string sSQLVal = "";

                //if (!string.IsNullOrEmpty(kdBrgInduk))
                //{
                var response = mgrApiManager.Call();
                var retProd = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
                if (retProd != null)
                {
                    if (retProd.openapi_msg.ToLower() == "success")
                    {
                        var detailData = JsonConvert.DeserializeObject(retProd.openapi_data, typeof(Data_Detail_Product)) as Data_Detail_Product;
                        if (detailData != null)
                        {
                            if (detailData.success)
                            {
                                if (!string.IsNullOrEmpty(kdBrgInduk))
                                {
                                    if (createParent)
                                    {
                                        var retSQL = CreateSQLValue(item, detailData.model[0], kdBrgInduk, "", cust, IdMarket, 1, itemFromList);
                                        if (retSQL.status == 1)
                                            sSQLVal += retSQL.message;
                                    }

                                    var retSQL2 = CreateSQLValue(item, detailData.model[0], kdBrgInduk, skuId, cust, IdMarket, 2, itemFromList);
                                    if (retSQL2.status == 1)
                                        sSQLVal += retSQL2.message;
                                }
                                else
                                {
                                    var retSQL = CreateSQLValue(item, detailData.model[0], "", skuId, cust, IdMarket, 0, itemFromList);
                                    if (retSQL.status == 1)
                                        sSQLVal += retSQL.message;
                                }
                            }
                            else
                            {
                                ret.message = string.IsNullOrEmpty(detailData.message) ? retProd.openapi_data : detailData.message;
                            }
                        }
                        else
                        {
                            ret.message = retProd.openapi_data;
                        }
                    }
                    else
                    {
                        ret.message = retProd.openapi_msg;
                    }
                }
                else
                {
                    ret.message = response;
                }

                if (!string.IsNullOrEmpty(sSQLVal))
                {
                    sSQL = sSQL + sSQLVal;
                    sSQL = sSQL.Substring(0, sSQL.Length - 1);
                    var a = EDB.ExecuteSQL("CString", CommandType.Text, sSQL);
                    ret.recordCount += a;
                    ret.status = 1;
                }
            }
            catch (Exception ex)
            {
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }
            return ret;
        }

        protected BindingBase CreateSQLValue(Model_Product item, Model_Detail_Product detItem, string kdBrgInduk, string skuId, string cust, int IdMarket, int typeBrg, Spuinfovolist itemFromList)
        {
            // typeBrg : 0 = barang tanpa varian; 1 = barang induk; 2 = barang varian
            var ret = new BindingBase
            {
                status = 0,
            };

            string sSQL_Value = "";
            try
            {

                string[] attrVal;
                string value = "";
                var brgAttribute = new Dictionary<string, string>();
                string namaBrg = item.skuName;
                string nama, nama2, urlImage, urlImage2, urlImage3;
                urlImage = "";
                urlImage2 = "";
                urlImage3 = "";
                var categoryCode = itemFromList.fullCategoryId.Split('/');
                var categoryName = itemFromList.fullCategoryName.Split('/');
                double price = Convert.ToDouble(item.jdPrice);
                //var statusBrg = detItem != null ? detItem.status : 1;
                var statusBrg = detItem.status;
                string brand = itemFromList.brandId.ToString();
                //var display = statusBrg.Equals("active") ? 1 : 0;
                string deskripsi = itemFromList.description;

                if (typeBrg != 1)
                {
                    sSQL_Value += " ( '" + skuId + "' , '" + skuId + "' , '";
                }
                else
                {
                    namaBrg = itemFromList.spuName;
                    sSQL_Value += " ( '" + kdBrgInduk + "' , '" + kdBrgInduk + "' , '";
                }

                if (typeBrg == 2)
                {
                    //namaBrg += " " + detItem.skuName;
                    urlImage = imageUrl + detItem.mainImgUri;
                }
                else
                {
                    urlImage = imageUrl + item.mainImgUri;
                }

                if (namaBrg.Length > 30)
                {
                    nama = namaBrg.Substring(0, 30);
                    if (namaBrg.Length > 285)
                    {
                        nama2 = namaBrg.Substring(30, 255);
                    }
                    else
                    {
                        nama2 = namaBrg.Substring(30);
                    }
                }
                else
                {
                    nama = namaBrg;
                    nama2 = "";
                }


                sSQL_Value += nama.Replace('\'', '`') + "' , '" + nama2.Replace('\'', '`') + "' ,";

                //var attrVal = detItem.saleAttributeIds.Split(';');
                //if (detItem == null)
                //{
                //    attrVal = item.saleAttributeIds.Split(';');
                //    foreach (Newtonsoft.Json.Linq.JProperty property in item.saleAttributeNameMap)
                //    {
                //        brgAttribute.Add(property.Name, property.Value.ToString());
                //    }
                //}
                //else
                //{
                attrVal = detItem.saleAttributeIds.Split(';');
                foreach (Newtonsoft.Json.Linq.JProperty property in detItem.saleAttributeNameMap)
                {
                    brgAttribute.Add(property.Name, property.Value.ToString());
                }
                //price = brgAttribute.TryGetValue("jdPrice", out value) ? Convert.ToDouble(value) : Convert.ToDouble(item.jdPrice);
                if (Convert.ToDouble(detItem.jdPrice) > 0)
                    price = Convert.ToDouble(detItem.jdPrice);

                //}
                if (listBrand.Count > 0)
                {
                    var a = listBrand.Where(m => m.brandId == itemFromList.brandId).FirstOrDefault();
                    if (a != null)
                        brand = a.brandName;
                }
                //sSQL_Value += Convert.ToDouble(detItem != null ? detItem.netWeight : item.netWeight) * 1000 + " , " + Convert.ToDouble(detItem != null ? detItem.packLong : item.packLong) + " , ";
                //sSQL_Value += Convert.ToDouble(detItem != null ? detItem.packWide : item.packWide) + " , " + Convert.ToDouble(detItem != null ? detItem.packHeight : item.packHeight);
                sSQL_Value += Convert.ToDouble(detItem.netWeight) * 1000 + " , " + Convert.ToDouble(detItem.packLong) + " , ";
                sSQL_Value += Convert.ToDouble(detItem.packWide) + " , " + Convert.ToDouble(detItem.packHeight);
                sSQL_Value += " , '" + cust + "' , '" + deskripsi.Replace('\'', '`') + "' , " + IdMarket + " , " + price + " , " + price + " , ";
                sSQL_Value += statusBrg + " , '" + categoryCode[categoryCode.Length - 1] + "' , '" + categoryName[categoryName.Length - 1] + "' , '";
                sSQL_Value += brand + "' , '" + urlImage + "' , '" + urlImage2 + "' , '" + urlImage3 + "' , '" + (typeBrg == 2 ? kdBrgInduk : "") + "' , '" + (typeBrg == 1 ? "4" : "3") + "'";
                int i;
                for (i = 0; i < attrVal.Length; i++)
                {
                    var attr = attrVal[i].Split(':');
                    if (attr.Length == 2)
                    {
                        var attrName = (brgAttribute.TryGetValue(attrVal[i], out value) ? value : "").Split(':');

                        sSQL_Value += ",'" + attr[0] + "','" + attrName[0] + "','" + attr[1] + "'";
                    }
                    else
                    {
                        sSQL_Value += ",'','',''";
                    }
                }

                for (int j = i; j < 20; j++)
                {
                    sSQL_Value += ",'','',''";
                }

                sSQL_Value += "),";
                //if (typeBrg == 1)
                //    sSQL_Value += ",";
                ret.status = 1;
                ret.message = sSQL_Value;
            }
            catch (Exception ex)
            {
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }
            return ret;
        }

        protected void getShopBrand(JDIDAPIData data)
        {
            try
            {

                var mgrApiManager = new JDIDController();
                mgrApiManager.AppKey = data.appKey;
                mgrApiManager.AppSecret = data.appSecret;
                mgrApiManager.AccessToken = data.accessToken;
                mgrApiManager.Method = "epi.popShop.getShopBrandList";
                var response = mgrApiManager.Call();
                var retBrand = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
                if (retBrand != null)
                {
                    if (retBrand.openapi_msg.ToLower() == "success")
                    {
                        var dataBrand = JsonConvert.DeserializeObject(retBrand.openapi_data, typeof(Data_Brand)) as Data_Brand;
                        if (dataBrand.success)
                        {
                            listBrand = dataBrand.model;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void UpdateStock(JDIDAPIData data, string id, int stok)
        {
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Update Stock",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = id,
                REQUEST_ATTRIBUTE_2 = stok.ToString(),
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data, currentLog);
            var mgrApiManager = new JDIDController();

            mgrApiManager.AppKey = data.appKey;
            mgrApiManager.AppSecret = data.appSecret;
            mgrApiManager.AccessToken = data.accessToken;
            mgrApiManager.Method = "epi.ware.openapi.warestock.updateWareStock";
            mgrApiManager.ParamJson = "{\"jsonStr\":[{\"skuId\":" + id + ", \"realNum\": " + stok + "}]}";
            try
            {
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
                                currentLog.REQUEST_EXCEPTION = retStok.message;
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            }
                        }
                        else
                        {
                            currentLog.REQUEST_EXCEPTION = ret.openapi_data;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        }
                    }
                    else
                    {
                        currentLog.REQUEST_EXCEPTION = ret.openapi_data;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                    }
                }
                else
                {
                    currentLog.REQUEST_EXCEPTION = response;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                }
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data, currentLog);
            }

        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public void Order_JD(JDIDAPIData data, string cust, string dbPathEra, string uname)
        {
            //1: waiting for delivery, 2: shipped, 3: Waiting_Cancel, 4: Waiting_Refuse, 5: canceled, 6: Completed
            var listOrderId = new List<long>();
            SetupContext(dbPathEra, uname);
            listOrderId.AddRange(GetOrderList(data, "1", 1));
            listOrderId.AddRange(GetOrderList(data, "2", 1));
            listOrderId.AddRange(GetOrderList(data, "3", 1));
            listOrderId.AddRange(GetOrderList(data, "4", 1));
            listOrderId.AddRange(GetOrderList(data, "5", 1));
            listOrderId.AddRange(GetOrderList(data, "6", 1));
            string connectionID = Guid.NewGuid().ToString();

            if (listOrderId.Count > 0)
            {
                var ord = new List_Order_JD { orderIds = new List<string>() };
                string order_10 = "";
                int i = 1;
                foreach (var id in listOrderId)
                {
                    order_10 += id.ToString() + ",";
                    i++;
                    if (i >= 10)
                    {
                        i = 0;
                        order_10 = order_10.Substring(0, order_10.Length - 1);
                        ord.orderIds.Add(order_10);
                        order_10 = "";
                    }
                }
                if (!string.IsNullOrEmpty(order_10))
                {
                    order_10 = order_10.Substring(0, order_10.Length - 1);
                    ord.orderIds.Add(order_10);
                }

                bool callSP = false;
                int newRecord = 0;
                foreach (var listOrder in ord.orderIds)
                {
                    var insertTemp = GetOrderDetail(data, listOrder, cust, connectionID);
                    if (insertTemp.status == 1)
                    {
                        callSP = true;
                        if (insertTemp.recordCount > 0)
                            newRecord += insertTemp.recordCount;
                    }
                }

                if (callSP)
                {
                    SqlCommand CommandSQL = new SqlCommand();

                    //add by Tri call sp to insert buyer data
                    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;

                    EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);
                    //end add by Tri call sp to insert buyer data

                    CommandSQL = new SqlCommand();
                    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;

                    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;
                    CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
                    CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 1;
                    CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = cust;

                    EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);

                    if (newRecord > 0)
                    {
                        var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                        contextNotif.Clients.Group(dbPathEra).moNewOrder("Terdapat " + Convert.ToString(newRecord) + " Pesanan baru dari Lazada.");

                        new StokControllerJob().updateStockMarketPlace(connectionID, dbPathEra, uname);
                    }
                }
            }
        }

        public List<long> GetOrderList(JDIDAPIData data, string status, int page)
        {
            var ret = new List<long>();
            var mgrApiManager = new JDIDController();
            mgrApiManager.AppKey = data.appKey;
            mgrApiManager.AppSecret = data.appSecret;
            mgrApiManager.AccessToken = data.accessToken;

            mgrApiManager.Method = "epi.popOrder.getOrderIdListByCondition";
            mgrApiManager.ParamJson = "{\"orderStatus\":" + status + ", \"startRow\": " + page * 20 + ", \"bookTimeBegin\": "
                + DateTimeOffset.Now.AddDays(-14).ToUnixTimeSeconds() + "}";

            try
            {
                var response = mgrApiManager.Call();
                var retData = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
                if (retData.openapi_code == 0)
                {
                    var listOrderId = JsonConvert.DeserializeObject(retData.openapi_data, typeof(Data_OrderIds)) as Data_OrderIds;
                    if (listOrderId.success)
                    {
                        ret = listOrderId.model;
                        if (listOrderId.model.Count == 20)
                        {
                            var nextOrders = GetOrderList(data, status, page + 1);
                            ret.AddRange(nextOrders);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return ret;
        }

        public BindingBase GetOrderDetail(JDIDAPIData data, string listOrderIds, string cust, string conn_id)
        {
            //var ret = new List<long>();
            var ret = new BindingBase();
            ret.status = 0;
            bool adaInsert = false;
            var mgrApiManager = new JDIDController();
            mgrApiManager.AppKey = data.appKey;
            mgrApiManager.AppSecret = data.appSecret;
            mgrApiManager.AccessToken = data.accessToken;

            mgrApiManager.Method = "epi.popOrder.getOrderInfoListForBatch";
            mgrApiManager.ParamJson = "[" + listOrderIds + "]";
            var jmlhNewOrder = 0;
            try
            {
                var response = mgrApiManager.Call();
                var retData = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
                if (retData.openapi_code == 0)
                {
                    var listOrderId = JsonConvert.DeserializeObject(retData.openapi_data, typeof(Data_OrderDetail)) as Data_OrderDetail;
                    if (listOrderId.success)
                    {
                        var str = "{\"data\":" + listOrderId.model + "}";
                        var listDetails = JsonConvert.DeserializeObject(str, typeof(ModelOrder)) as ModelOrder;
                        if (listDetails != null)
                        {

                            string insertQ = "INSERT INTO TEMP_ORDER_JD ([ADDRESS],[AREA],[BOOKTIME],[CITY],[COUPON_AMOUNT],[CUSTOMER_NAME],";
                            insertQ += "[DELIVERY_ADDR],[DELIVERY_TYPE],[EMAIL],[FREIGHT_AMOUNT],[FULL_CUT_AMMOUNT],[INSTALLMENT_FEE],[ORDER_COMPLETE_TIME],";
                            insertQ += "[ORDER_ID],[ORDER_SKU_NUM],[ORDER_STATE],[ORDER_TYPE],[PAY_SUBTOTAL],[PAYMENT_TYPE],[PHONE],[POSTCODE],[PROMOTION_AMOUNT],";
                            insertQ += "[SENDPAY],[STATE],[TOTAL_PRICE],[USER_PIN],[CUST],[USERNAME],[CONN_ID]) VALUES ";

                            string insertOrderItems = "INSERT INTO TEMP_ORDERITEMS_JD ([ORDER_ID],[COMMISSION],[COST_PRICE],[COUPON_AMOUNT],[FULL_CUT_AMMOUNT]";
                            insertOrderItems += ",[HAS_PROMO],[JDPRICE],[PROMOTION_AMOUNT],[SKUID],[SKU_NAME],[SKU_NUMBER],[SPUID],[WEIGHT],[USERNAME],[CONN_ID]) VALUES ";

                            string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                            insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                            insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV, CONNECTION_ID) VALUES ";

                            var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == cust).Select(p => p.NO_REFERENSI).ToList();
                            foreach (var order in listDetails.data)
                            {
                                bool doInsert = true;
                                if (OrderNoInDb.Contains(Convert.ToString(order.orderId)) && order.orderState.ToString() == "1")
                                {
                                    doInsert = false;
                                }
                                else if (order.orderState.ToString() == "5" || order.orderState.ToString() == "6")
                                {
                                    if (OrderNoInDb.Contains(Convert.ToString(order.orderId)))
                                    {
                                        //tidak ubah status menjadi selesai jika belum diisi faktur
                                        var dsSIT01A = EDB.GetDataSet("CString", "SIT01A", "SELECT NO_REFERENSI, O.NO_BUKTI, O.STATUS_TRANSAKSI FROM SIT01A I INNER JOIN SOT01A O ON I.NO_SO = O.NO_BUKTI WHERE NO_REFERENSI = '" + order.orderId + "'");
                                        if (dsSIT01A.Tables[0].Rows.Count == 0)
                                        {
                                            doInsert = false;
                                        }
                                    }
                                    else
                                    {
                                        //tidak diinput jika order sudah selesai sebelum masuk MO
                                        doInsert = false;
                                    }
                                }
                                else if (order.orderState.ToString() == "3" || order.orderState.ToString() == "4")
                                {
                                    if (!OrderNoInDb.Contains(Convert.ToString(order.orderId)))
                                    {
                                        doInsert = false;
                                    }
                                }

                                if (doInsert)
                                {
                                    var dtNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    adaInsert = true;
                                    var statusEra = "";
                                    switch (order.orderState.ToString())
                                    {
                                        //1: waiting for delivery, 2: shipped, 3: Waiting_Cancel, 4: Waiting_Refuse, 5: canceled, 6: Completed
                                        case "1":
                                            statusEra = "01";
                                            break;
                                        case "2":
                                            statusEra = "03";
                                            break;
                                        case "3":
                                        case "4":
                                        case "5":
                                            statusEra = "11";
                                            break;
                                        case "6":
                                            statusEra = "04";
                                            break;
                                        default:
                                            statusEra = "99";
                                            break;
                                    }

                                    var nama = order.customerName.Replace('\'', '`');
                                    if (nama.Length > 30)
                                        nama = nama.Substring(0, 30);

                                    //insertQ += "('" + order.address.Replace('\'', '`') + "','" + order.area.Replace('\'', '`') + "','" + DateTimeOffset.FromUnixTimeSeconds(order.bookTime / 1000).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd hh:mm:ss") + "','" + order.city.Replace('\'', '`') + "'," + order.couponAmount + ",'" + order.customerName + "','";
                                    insertQ += "('" + order.address.Replace('\'', '`') + "','" + order.area.Replace('\'', '`') + "','" + DateTimeOffset.FromUnixTimeSeconds(order.bookTime / 1000).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd hh:mm:ss") + "','" + order.city.Replace('\'', '`') + "'," + order.couponAmount + ",'" + nama + "','";
                                    insertQ += order.deliveryAddr.Replace('\'', '`') + "'," + order.deliveryType + ",'" + order.email + "'," + order.freightAmount + "," + order.fullCutAmount + "," + order.installmentFee + ",'" + DateTimeOffset.FromUnixTimeSeconds(order.orderCompleteTime / 1000).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd hh:mm:ss") + "','";
                                    insertQ += order.orderId + "'," + order.orderSkuNum + "," + statusEra + "," + order.orderType + "," + order.paySubtotal + "," + order.paymentType + ",'" + order.phone + "','" + order.postCode + "'," + order.promotionAmount + ",'";
                                    insertQ += order.sendPay + "','" + order.state.Replace('\'', '`') + "'," + order.totalPrice + ",'" + order.userPin + "','" + cust + "','" + username + "','" + conn_id + "') ,";

                                    if (order.orderSkuinfos != null)
                                    {
                                        foreach (var ordItem in order.orderSkuinfos)
                                        {
                                            insertOrderItems += "('" + order.orderId + "'," + ordItem.commission + "," + ordItem.costPrice + "," + ordItem.couponAmount + "," + ordItem.fullCutAmount + ",";
                                            insertOrderItems += ordItem.hasPromo + "," + ordItem.jdPrice + "," + ordItem.promotionAmount + ",'" + ordItem.skuId + "','" + ordItem.skuName + "',";
                                            insertOrderItems += ordItem.skuNumber + ",'" + ordItem.spuId + "'," + ordItem.weight + ",'" + username + "','" + conn_id + "') ,";
                                        }
                                    }

                                    var tblKabKot = EDB.GetDataSet("MOConnectionString", "KabupatenKota", "SELECT TOP 1 * FROM KabupatenKota WHERE NamaKabKot LIKE '%" + order.city + "%'");
                                    var tblProv = EDB.GetDataSet("MOConnectionString", "Provinsi", "SELECT TOP 1 * FROM Provinsi WHERE NamaProv LIKE '%" + order.state + "%'");

                                    var kabKot = "3174";//set default value jika tidak ada di db
                                    var prov = "31";//set default value jika tidak ada di db

                                    if (tblProv.Tables[0].Rows.Count > 0)
                                        prov = tblProv.Tables[0].Rows[0]["KodeProv"].ToString();
                                    if (tblKabKot.Tables[0].Rows.Count > 0)
                                        kabKot = tblKabKot.Tables[0].Rows[0]["KodeKabKot"].ToString();

                                    //insertPembeli += "('" + order.customerName.Replace('\'', '`') + "','" + order.address.Replace('\'', '`') + "','" + order.phone + "','" + order.email.Replace('\'', '`') + "',0,0,'0','01',";
                                    insertPembeli += "('" + nama + "','" + order.address.Replace('\'', '`') + "','" + order.phone + "','" + order.email.Replace('\'', '`') + "',0,0,'0','01',";
                                    insertPembeli += "1, 'IDR', '01', '" + order.address.Replace('\'', '`') + "', 0, 0, 0, 0, '1', 0, 0, ";
                                    insertPembeli += "'FP', '" + dtNow + "', '" + username + "', '" + order.postCode.Replace('\'', '`') + "', '" + order.email.Replace('\'', '`') + "', '" + kabKot + "', '" + prov + "', '" + order.city.Replace('\'', '`') + "', '" + order.state.Replace('\'', '`') + "', '" + conn_id + "') ,";

                                    if (!OrderNoInDb.Contains(Convert.ToString(order.orderId)))
                                        jmlhNewOrder++;
                                }
                            }

                            if (adaInsert)
                            {
                                ret.status = 1;
                                insertQ = insertQ.Substring(0, insertQ.Length - 2);
                                EDB.ExecuteSQL(username, CommandType.Text, insertQ);


                                insertOrderItems = insertOrderItems.Substring(0, insertOrderItems.Length - 2);
                                EDB.ExecuteSQL(username, CommandType.Text, insertOrderItems);


                                insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 2);
                                EDB.ExecuteSQL(username, CommandType.Text, insertPembeli);

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            //return adaInsert;
            ret.recordCount = jmlhNewOrder;
            return ret;
        }

        protected enum api_status
        {
            Pending = 1,
            Success = 2,
            Failed = 3,
            Exception = 4
        }
        protected void manageAPI_LOG_MARKETPLACE(api_status action, ErasoftContext db, JDIDAPIData iden, API_LOG_MARKETPLACE data)
        {
            switch (action)
            {
                case api_status.Pending:
                    {
                        var arf01 = ErasoftDbContext.ARF01.Where(p => p.TOKEN == iden.accessToken).FirstOrDefault();
                        var apiLog = new MasterOnline.API_LOG_MARKETPLACE
                        {
                            CUST = arf01 != null ? arf01.CUST : iden.accessToken,
                            CUST_ATTRIBUTE_1 = iden.accessToken,
                            CUST_ATTRIBUTE_2 = data.CUST_ATTRIBUTE_2 != null ? data.CUST_ATTRIBUTE_2 : "",
                            CUST_ATTRIBUTE_3 = data.CUST_ATTRIBUTE_3 != null ? data.CUST_ATTRIBUTE_3 : "",
                            CUST_ATTRIBUTE_4 = data.CUST_ATTRIBUTE_4 != null ? data.CUST_ATTRIBUTE_4 : "",
                            CUST_ATTRIBUTE_5 = data.CUST_ATTRIBUTE_5 != null ? data.CUST_ATTRIBUTE_5 : "",
                            MARKETPLACE = "JD",
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

    }
}
