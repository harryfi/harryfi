using Erasoft.Function;
using Lazop.Api;
using Lazop.Api.Util;
using MasterOnline.Models;
using MasterOnline.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Server;
using Hangfire.Common;
using Hangfire.Client;
using Hangfire.States;
using System.Xml;

namespace MasterOnline.Controllers
{
    public class LazadaControllerJob : Controller
    {
        //AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        string urlLazada = "https://api.lazada.co.id/rest";
        List<string> listSku = new List<string>();
        //string eraCallbackUrl = "https://dev.masteronline.co.id/lzd/code?user=";
        //string eraAppKey = "";101775;106147
#if AWS
                        
        string eraAppKey = "101775";
        string eraAppSecret = "QwUJjjtZ3eCy2qaz6Rv1PEXPyPaPkDSu";
        string eraCallbackUrl = "https://masteronline.co.id/lzd/code?user=";
#elif Debug_AWS

        string eraAppKey = "101775";
        string eraAppSecret = "QwUJjjtZ3eCy2qaz6Rv1PEXPyPaPkDSu";
        string eraCallbackUrl = "https://masteronline.co.id/lzd/code?user=";
#else

        string eraAppKey = "101775";
        string eraAppSecret = "QwUJjjtZ3eCy2qaz6Rv1PEXPyPaPkDSu";
        string eraCallbackUrl = "https://dev.masteronline.co.id/lzd/code?user=";
#endif
        // GET: Lazada; QwUJjjtZ3eCy2qaz6Rv1PEXPyPaPkDSu;So2KEplWTt4XFO9OGmXjuFFVIT1Wc6FU
        DatabaseSQL EDB;
        MoDbContext MoDbContext;
        ErasoftContext ErasoftDbContext;
        string username;
        public LazadaControllerJob()
        {
            //MoDbContext = new MoDbContext();
            //if (sessionData?.Account != null)
            //{
            //    if (sessionData.Account.UserId == "admin_manage")
            //        ErasoftDbContext = new ErasoftContext();
            //    else
            //        ErasoftDbContext = new ErasoftContext(sessionData.Account.DatabasePathErasoft);
            //    EDB = new DatabaseSQL(sessionData.Account.DatabasePathErasoft);

            //}
            //else
            //{
            //    if (sessionData?.User != null)
            //    {
            //        var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
            //        EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
            //        ErasoftDbContext = new ErasoftContext(accFromUser.DatabasePathErasoft);
            //    }
            //}
        }
        protected void SetupContext(string DatabasePathErasoft, string uname)
        {
            //string ret = "";
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, DatabasePathErasoft);
            username = uname;
            //var arf01inDB = ErasoftDbContext.ARF01.Where(p => p.RecNum == idmarket).SingleOrDefault();
            //if (arf01inDB != null)
            //{
            //    ret = arf01inDB.TOKEN;
            //}
            //return ret;
        }
        [Route("lzd/code")]
        [HttpGet]
        public ActionResult LazadaCode(string user, string code)
        {
            var param = user.Split(new string[] { "_param_" }, StringSplitOptions.None);
            if (param.Count() == 2)
            {
                DatabaseSQL EDB = new DatabaseSQL(param[0]);
                var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET API_KEY = '" + code + "' WHERE CUST = '" + param[1] + "'");

                GetToken(param[0], param[1], code);
            }
            return View("LazadaAuth");
        }

        //[HttpGet]
        //public string LazadaUrl(string cust)
        //{
        //    AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        //    string userId = "";
        //    if (sessionData?.Account != null)
        //    {
        //        userId = sessionData.Account.DatabasePathErasoft;

        //    }
        //    else
        //    {
        //        if (sessionData?.User != null)
        //        {
        //            var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
        //            userId = accFromUser.DatabasePathErasoft;
        //        }
        //    }

        //    string lzdId = cust;
        //    string compUrl = eraCallbackUrl + userId + "_param_" + lzdId;
        //    string uri = "https://auth.lazada.com/oauth/authorize?response_type=code&force_auth=true&redirect_uri=" + compUrl + "&client_id=" + eraAppKey + "&country=id";
        //    return uri;
        //}

        public string GetToken(string user, string cust, string accessToken)
        {
            string ret;
            string url;
            url = "https://auth.lazada.com/rest";
            DatabaseSQL EDB = new DatabaseSQL(user);
            ILazopClient client = new LazopClient(url, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest("/auth/token/create");
            request.SetHttpMethod("GET");
            request.AddApiParameter("code", accessToken);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, user);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Get Token",
                REQUEST_DATETIME = DateTime.UtcNow.AddHours(7),
                REQUEST_ATTRIBUTE_1 = cust,
                REQUEST_ATTRIBUTE_2 = accessToken,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, "", currentLog);

            try
            {
                LazopResponse response = client.Execute(request);
                ret = "error:" + response.IsError() + "\nbody:" + response.Body;

                if (!response.IsError())
                {
                    var bindAuth = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaAuth)) as LazadaAuth;
                    // add by fauzi 20 februari 2020
                    var dateExpired = DateTime.UtcNow.AddSeconds(bindAuth.expires_in).ToString("yyyy-MM-dd HH:mm:ss");
                    var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET TOKEN = '" + bindAuth.access_token + "', REFRESH_TOKEN = '" + bindAuth.refresh_token + "', STATUS_API = '1', TGL_EXPIRED = '" + dateExpired + "'  WHERE CUST = '" + cust + "'");
                    if (result == 1)
                    {
                        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, "", currentLog);
                        GetShipment(cust, bindAuth.access_token);
                    }
                    else
                    {
                        currentLog.REQUEST_EXCEPTION = "failed to update token;execute result=" + result;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, "", currentLog);
                    }
                }
                else
                {
                    var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '0' WHERE CUST = '" + cust + "'");
                    currentLog.REQUEST_EXCEPTION = response.Body;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, "", currentLog);
                }
                return ret;
            }
            catch (Exception ex)
            {
                var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '0' WHERE CUST = '" + cust + "'");
                currentLog.REQUEST_EXCEPTION = ex.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, "", currentLog);
                return ex.ToString();
            }

        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("2_get_token")]
        public LazadaAuth GetRefToken(string cust, string refreshToken, string dbPathEra, string uname, DateTime? tgl_expired, bool bForceRefresh)
        {
            LazadaAuth ret = new LazadaAuth();
            DateTime dateNow = DateTime.UtcNow.AddHours(7);
            bool TokenExpired = false;

            if (!string.IsNullOrWhiteSpace(tgl_expired.ToString()))
            {
                if (dateNow >= tgl_expired)
                {
                    TokenExpired = true;
                }
            }
            else
            {
                TokenExpired = true;
            }

            if (TokenExpired || bForceRefresh)
                {
                    string url;
                    url = "https://auth.lazada.com/rest";
                    SetupContext(dbPathEra, uname);

                    ILazopClient client = new LazopClient(url, eraAppKey, eraAppSecret);
                    LazopRequest request = new LazopRequest("/auth/token/refresh");
                    request.SetHttpMethod("GET");
                    request.AddApiParameter("refresh_token", refreshToken);
                    //moved to inside try catch
                    //LazopResponse response = client.Execute(request);
                    //end moved to inside try catch

                    ////Console.WriteLine(response.IsError());
                    ////Console.WriteLine(response.Body);

                    MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                    {
                        REQUEST_ID = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmssfff"),
                        REQUEST_ACTION = "Refresh Token",
                        REQUEST_DATETIME = DateTime.UtcNow.AddHours(7),
                        REQUEST_ATTRIBUTE_1 = cust,
                        REQUEST_ATTRIBUTE_2 = refreshToken,
                        REQUEST_STATUS = "Pending",
                    };
                    manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, "", currentLog);
                    try
                    {
                        LazopResponse response = client.Execute(request);
                        //Console.WriteLine(response.IsError());
                        //Console.WriteLine(response.Body);


                        ret = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaAuth)) as LazadaAuth;
                        if (!response.IsError())
                        {
                            // add by fauzi 20 februari 2020
                            var dateExpired = DateTime.UtcNow.AddSeconds(ret.expires_in).ToString("yyyy-MM-dd HH:mm:ss");

                            //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
                            var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET TOKEN = '" + ret.access_token + "', REFRESH_TOKEN = '" + ret.refresh_token + "', STATUS_API = '1', TGL_EXPIRED = '" + dateExpired + "'  WHERE CUST = '" + cust + "'");
                            if (result == 1)
                            {
                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, "", currentLog);
                            }
                            else
                            {
                                currentLog.REQUEST_EXCEPTION = "failed to update token;execute result=" + result;
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, "", currentLog);
                            }
                        }
                        else
                        {
                            var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '2' WHERE CUST = '" + cust + "'");
                            currentLog.REQUEST_EXCEPTION = response.Body;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, "", currentLog);
                        }
                    }
                    catch (Exception ex)
                    {
                        currentLog.REQUEST_EXCEPTION = ex.Message;
                        manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, "", currentLog);
                        return null;
                    }
                }
            
            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Product {obj} ke Lazada Gagal.")]
        public BindingBase CreateProduct(string dbPathEra, string namaPemesan, string log_CUST, string log_ActionCategory, string log_ActionName, string uname, BrgViewModel data)
        {
            var ret = new BindingBase();
            ret.status = 0;
            SetupContext(dbPathEra, uname);


            //change 8 Apriil 2019, get attr from api
            //string sSQL = "SELECT * FROM (";
            //for (int i = 1; i <= 50; i++)
            //{
            //    sSQL += "SELECT A.ACODE_" + i.ToString() + " AS CATEGORY_CODE,A.ANAME_" + i.ToString() + " AS CATEGORY_NAME,B.ATYPE" + i.ToString();
            //    sSQL += " AS CATEGORY_TYPE,A.AVALUE_" + i.ToString() + " AS VALUE FROM STF02H A INNER JOIN MO.DBO.ATTRIBUTE_LAZADA B ON A.CATEGORY_CODE = B.CATEGORY_CODE WHERE A.BRG='" + data.kdBrg + "' " + System.Environment.NewLine;
            //    if (i < 50)
            //    {
            //        sSQL += "UNION ALL " + System.Environment.NewLine;
            //    }
            //}

            //DataSet dsSku = EDB.GetDataSet("sCon", "STF02H", sSQL + ") ASD WHERE ISNULL(CATEGORY_CODE,'') <> '' AND CATEGORY_TYPE <> 'normal' AND ISNULL(VALUE, 'NULL') <> 'NULL' ");
            //DataSet dsNormal = EDB.GetDataSet("sCon", "STF02H", sSQL + ") ASD WHERE ISNULL(CATEGORY_CODE,'') <> '' AND CATEGORY_TYPE = 'normal' AND ISNULL(VALUE, 'NULL') <> 'NULL' ");
            var stf02h = ErasoftDbContext.STF02H.Where(p => p.BRG == data.kdBrg && p.IDMARKET.ToString() == data.idMarket).FirstOrDefault();
            List<string> dsSku = new List<string>();
            List<string> dsNormal = new List<string>();

            var attributeLzd = getAttrLzd(stf02h.CATEGORY_CODE);
            for (int i = 1; i <= 50; i++)
            {
                string attribute_id = Convert.ToString(attributeLzd["ANAME" + i.ToString()]);
                string attribute_type = Convert.ToString(attributeLzd["ATYPE" + i.ToString()]);
                if (!string.IsNullOrWhiteSpace(attribute_id))
                {
                    if (attribute_type != "normal")
                    {
                        dsSku.Add(attribute_id);
                    }
                    else
                    {
                        dsNormal.Add(attribute_id);
                    }
                }
            }
            //end change 8 Apriil 2019, get attr from api

            string primCategory = EDB.GetFieldValue("MOConnectionString", "STF02H", "BRG = '" + data.kdBrg + "' AND IDMARKET = '" + data.idMarket + "'", "category_code").ToString();
            string xmlString = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>";
            xmlString = "<Request><Product><PrimaryCategory>" + primCategory + "</PrimaryCategory>";
            xmlString += "<Attributes><name>" + XmlEscape(data.nama + (string.IsNullOrEmpty(data.nama2) ? "" : " " + data.nama2)) + "</name>";
            //xmlString += "<short_description><![CDATA[" + data.deskripsi + "]]></short_description>";
            xmlString += "<description><![CDATA[" + data.deskripsi.Replace(System.Environment.NewLine, "<br>") + "]]></description>";

            //xmlString += "<brand>No Brand</brand>";
            xmlString += "<brand><![CDATA[" + stf02h.ANAME_38 + "]]></brand>";

            //xmlString += "<model>" + data.kdBrg + "</model>";
            //xmlString += "<warranty_type>No Warranty</warranty_type>";

            //change 8 Apriil 2019, get attr from api
            //for (int i = 0; i < dsNormal.Tables[0].Rows.Count; i++)
            //{
            //    xmlString += "<" + dsNormal.Tables[0].Rows[i]["CATEGORY_CODE"].ToString() + ">";
            //    xmlString += dsNormal.Tables[0].Rows[i]["VALUE"].ToString();
            //    xmlString += "</" + dsNormal.Tables[0].Rows[i]["CATEGORY_CODE"].ToString() + ">";
            //}
            Dictionary<string, string> lzdAttrWithVal = new Dictionary<string, string>();
            Dictionary<string, string> lzdAttrSkuWithVal = new Dictionary<string, string>();
            for (int i = 1; i <= 50; i++)
            {
                string attribute_id = Convert.ToString(stf02h["ACODE_" + i.ToString()]);
                string value = Convert.ToString(stf02h["AVALUE_" + i.ToString()]);
                if (!string.IsNullOrWhiteSpace(value) && value != "null")
                {
                    if (dsNormal.Contains(attribute_id))
                    {
                        if (!lzdAttrWithVal.ContainsKey(attribute_id))
                        {
                            lzdAttrWithVal.Add(attribute_id, value.Trim());
                        }
                    }
                    else if (dsSku.Contains(attribute_id))
                    {
                        if (!lzdAttrSkuWithVal.ContainsKey(attribute_id))
                        {
                            lzdAttrSkuWithVal.Add(attribute_id, value.Trim());
                        }
                    }
                }
            }
            //for (int i = 0; i < lzdAttrWithVal.Count; i++)
            //{
            //    xmlString += "<" + dsNormal[i].ToString() + ">";
            //    xmlString += lzdAttrWithVal[dsNormal[i].ToString()].ToString();
            //    xmlString += "</" + dsNormal[i].ToString() + ">";                
            //}
            foreach (var lzdAttr in lzdAttrWithVal)
            {
                xmlString += "<" + lzdAttr.Key + ">";
                //xmlString += XmlEscape(lzdAttr.Value.ToString());
                xmlString += "<![CDATA[" + XmlEscape(Convert.ToString(lzdAttr.Value).Replace(System.Environment.NewLine, "<br>")) + "]]>";
                xmlString += "</" + lzdAttr.Key + ">";
            }
            //end change 8 Apriil 2019, get attr from api

            xmlString += "</Attributes>";

            var stf02 = ErasoftDbContext.STF02.Where(p => p.BRG == data.kdBrg).FirstOrDefault();
            if (Convert.ToString(stf02.TYPE) == "3")
            {

                xmlString += "<Skus><Sku><SellerSku>" + XmlEscape(data.kdBrg) + "</SellerSku>";
                //xmlString += "<active>" + (data.activeProd ? "true" : "false") + "</active>";
                xmlString += "<Status>" + (data.activeProd ? "active" : "inactive") + "</Status>";
                //xmlString += "<color_family>Not Specified</color_family>";

                //add by calvin 1 mei 2019
                var qty_stock = new StokControllerJob(dbPathEra, uname).GetQOHSTF08A(data.kdBrg, "ALL");
                if (qty_stock > 0)
                {
                    xmlString += "<quantity>" + Convert.ToString(qty_stock) + "</quantity>";
                }
                //end add by calvin 1 mei 2019

                //xmlString += "<quantity>1</quantity>";
                xmlString += "<price>" + data.harga + "</price>";
                //xmlString += "<size>Int: One size</size>";
                xmlString += "<package_length>" + data.length + "</package_length><package_height>" + data.height + "</package_height>";
                xmlString += "<package_width>" + data.width + "</package_width><package_weight>" + Convert.ToDouble(data.weight) / 1000 + "</package_weight>";//weight in kg
                xmlString += "<Images>";
                if (!string.IsNullOrEmpty(data.imageUrl))
                    xmlString += "<Image><![CDATA[" + data.imageUrl + "]]></Image>";
                if (!string.IsNullOrEmpty(data.imageUrl2))
                    xmlString += "<Image><![CDATA[" + data.imageUrl2 + "]]></Image>";
                if (!string.IsNullOrEmpty(data.imageUrl3))
                    xmlString += "<Image><![CDATA[" + data.imageUrl3 + "]]></Image>";
                //add 6/9/2019, 5 gambar
                if (!string.IsNullOrEmpty(data.imageUrl4))
                    xmlString += "<Image><![CDATA[" + data.imageUrl4 + "]]></Image>";
                if (!string.IsNullOrEmpty(data.imageUrl5))
                    xmlString += "<Image><![CDATA[" + data.imageUrl5 + "]]></Image>";
                //end add 6/9/2019, 5 gambar
                xmlString += "</Images>";

                //change 8 Apriil 2019, get attr from api
                //for (int i = 0; i < dsSku.Tables[0].Rows.Count; i++)
                //{
                //    xmlString += "<" + dsSku.Tables[0].Rows[i]["CATEGORY_CODE"].ToString() + ">";
                //    xmlString += dsSku.Tables[0].Rows[i]["VALUE"].ToString();
                //    xmlString += "</" + dsSku.Tables[0].Rows[i]["CATEGORY_CODE"].ToString() + ">";
                //}
                //for (int i = 0; i < lzdAttrSkuWithVal.Count; i++)
                //{
                //    xmlString += "<" + dsSku[i].ToString() + ">";
                //    xmlString += lzdAttrSkuWithVal[dsSku[i].ToString()].ToString();
                //    xmlString += "</" + dsSku[i].ToString() + ">";
                //}
                foreach (var lzdSkuAttr in lzdAttrSkuWithVal)
                {
                    xmlString += "<" + lzdSkuAttr.Key + ">";
                    //xmlString += XmlEscape(lzdSkuAttr.Value.ToString());
                    xmlString += "<![CDATA[" + XmlEscape(Convert.ToString(lzdSkuAttr.Value).Replace(System.Environment.NewLine, "<br>")) + "]]>";
                    xmlString += "</" + lzdSkuAttr.Key + ">";
                }
                //end change 8 Apriil 2019, get attr from api

                xmlString += "</Sku></Skus>";
            }
            else if (Convert.ToString(stf02.TYPE) == "4")
            {
                var ListSettingVariasi = ErasoftDbContext.STF02I.Where(p => p.BRG == data.kdBrg && p.MARKET == "LAZADA").ToList();
                var ListStf02Var = ErasoftDbContext.STF02.Where(p => p.PART == data.kdBrg).ToList();
                var ListStf02Var_BRG = ListStf02Var.Select(p => p.BRG).ToList();
                int idmarket_int = Convert.ToInt32(data.idMarket);
                var List_STF02H_Var = ErasoftDbContext.STF02H.Where(p => ListStf02Var_BRG.Contains(p.BRG) && p.IDMARKET == idmarket_int).ToList();
                //add 14-08-2019,validasi tambah varian
                var list_STF02H_created = List_STF02H_Var.Where(p => !string.IsNullOrEmpty(p.BRG_MP)).ToList();
                var list_BRGMP_created = "";
                if (list_STF02H_created.Count > 0)
                {
                    list_BRGMP_created = "<AssociatedSku>";
                    list_BRGMP_created += list_STF02H_created[0].BRG_MP;
                    list_BRGMP_created += "</AssociatedSku>";

                }
                //end 14-08-2019,add validasi tambah varian

                //untuk pastikan tidak ada duplikat kombinasi attribute variasi
                Dictionary<string, string> KombinasiAttribute = new Dictionary<string, string>();
                foreach (var item in ListStf02Var)
                {
                    if (!string.IsNullOrWhiteSpace(item.Sort8))
                    {
                        var getMPJudul_and_ValueVar = ListSettingVariasi.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == item.Sort8).FirstOrDefault();
                        string attributeUnique = getMPJudul_and_ValueVar.MP_JUDUL_VAR + "[;]" + getMPJudul_and_ValueVar.MP_VALUE_VAR + "[;]" + item.BRG;
                        if (!KombinasiAttribute.ContainsKey(attributeUnique))
                        {
                            KombinasiAttribute.Add(attributeUnique, item.BRG);

                        }
                        if (!string.IsNullOrWhiteSpace(item.Sort9))
                        {
                            var getMPJudul_and_ValueVarLv2 = ListSettingVariasi.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == item.Sort9).FirstOrDefault();
                            string attributeUniqueLv2 = getMPJudul_and_ValueVarLv2.MP_JUDUL_VAR + "[;]" + getMPJudul_and_ValueVarLv2.MP_VALUE_VAR + "[;]" + item.BRG;
                            if (!KombinasiAttribute.ContainsKey(attributeUniqueLv2))
                            {
                                KombinasiAttribute.Add(attributeUniqueLv2, item.BRG);
                            }
                        }
                    }
                }
                //end untuk pastikan tidak ada duplikat kombinasi attribute variasi
                List<string> attributesAdded;
                if (!string.IsNullOrEmpty(list_BRGMP_created))
                {
                    xmlString += list_BRGMP_created;
                }
                xmlString += "<Skus>";
                foreach (var item in ListStf02Var)
                {
                    attributesAdded = new List<string>();
                    bool input = false;
                    foreach (var attribute in KombinasiAttribute)
                    {
                        if (attribute.Value == item.BRG)
                        {
                            input = true;
                        }
                    }

                    var GetStf02h = List_STF02H_Var.Where(p => p.BRG == item.BRG).FirstOrDefault();
                    if (input && (GetStf02h != null))
                    {
                        if (string.IsNullOrEmpty(GetStf02h.BRG_MP))
                        {
                            xmlString += "<Sku><SellerSku>" + XmlEscape(item.BRG) + "</SellerSku>";
                            //if (!string.IsNullOrEmpty(list_BRGMP_created))
                            //{
                            //    xmlString += list_BRGMP_created;
                            //}
                            //xmlString += "<active>" + (data.activeProd ? "true" : "false") + "</active>";
                            xmlString += "<Status>" + (data.activeProd ? "active" : "inactive") + "</Status>";

                            foreach (var attribute in KombinasiAttribute)
                            {
                                if (attribute.Value == item.BRG)
                                {
                                    string[] getId = attribute.Key.Split(new string[] { "[;]" }, StringSplitOptions.None);
                                    xmlString += "<" + getId[0] + ">" + XmlEscape(getId[1]) + "</" + getId[0] + ">";
                                    attributesAdded.Add(getId[0]);
                                }
                            }

                            //CEK JIKA ADA ATTRIBUTE YANG KURANG DI STF02I ( MAPPING ATTRIBUTE ), MAKA AMBIL KE STF02H
                            //change 8 Apriil 2019, get attr from api
                            //for (int i = 0; i < dsSku.Tables[0].Rows.Count; i++)
                            //{
                            //    if (!attributesAdded.Contains(dsSku.Tables[0].Rows[i]["CATEGORY_CODE"].ToString()))
                            //    {
                            //        xmlString += "<" + dsSku.Tables[0].Rows[i]["CATEGORY_CODE"].ToString() + ">";
                            //        xmlString += dsSku.Tables[0].Rows[i]["VALUE"].ToString();
                            //        xmlString += "</" + dsSku.Tables[0].Rows[i]["CATEGORY_CODE"].ToString() + ">";
                            //    }
                            //}
                            for (int i = 0; i < lzdAttrSkuWithVal.Count; i++)
                            {
                                if (!attributesAdded.Contains(dsSku[i].ToString()))
                                {
                                    try
                                    {
                                        var getAttrValue = lzdAttrSkuWithVal[dsSku[i].ToString()].ToString();
                                        xmlString += "<" + dsSku[i].ToString() + ">";
                                        xmlString += XmlEscape(getAttrValue);
                                        xmlString += "</" + dsSku[i].ToString() + ">";
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                            }
                            //end change 8 Apriil 2019, get attr from api
                            var qty_stock = new StokControllerJob(dbPathEra, uname).GetQOHSTF08A(item.BRG, "ALL");
                            if (qty_stock > 0)
                            {
                                xmlString += "<quantity>" + Convert.ToString(qty_stock) + "</quantity>";
                            }
                            //change 1/8/2019, gunakan hjual stf02h
                            //xmlString += "<price>" + data.harga + "</price>";
                            xmlString += "<price>" + GetStf02h.HJUAL + "</price>";
                            //change 1/8/2019, gunakan hjual stf02h
                            xmlString += "<package_length>" + data.length + "</package_length><package_height>" + data.height + "</package_height>";
                            xmlString += "<package_width>" + data.width + "</package_width><package_weight>" + Convert.ToDouble(data.weight) / 1000 + "</package_weight>";//weight in kg
                            xmlString += "<Images>";
                            //CHANGE BY CALVIN 10 JUNI 2019
                            //if (!string.IsNullOrEmpty(data.imageUrl))
                            //    xmlString += "<Image><![CDATA[" + data.imageUrl + "]]></Image>";
                            //if (!string.IsNullOrEmpty(data.imageUrl2))
                            //    xmlString += "<Image><![CDATA[" + data.imageUrl2 + "]]></Image>";
                            //if (!string.IsNullOrEmpty(data.imageUrl3))
                            //    xmlString += "<Image><![CDATA[" + data.imageUrl3 + "]]></Image>";
                            if (!string.IsNullOrEmpty(item.LINK_GAMBAR_1))
                            {
                                var uploadImg = UploadImage(item.LINK_GAMBAR_1, data.token);
                                if (uploadImg.status == 1)
                                {
                                    xmlString += "<Image><![CDATA[" + uploadImg.message + "]]></Image>";
                                }
                            }
                            // 6/9/2019, 2 gambar untuk varian
                            //remark by calvin 19 agustus 2019
                            if (!string.IsNullOrEmpty(item.LINK_GAMBAR_2))
                            {
                                var uploadImg = UploadImage(item.LINK_GAMBAR_2, data.token);
                                if (uploadImg.status == 1)
                                {
                                    xmlString += "<Image><![CDATA[" + uploadImg.message + "]]></Image>";
                                }
                            }
                            //end  6/9/2019, 2 gambar untuk varian
                            //if (!string.IsNullOrEmpty(item.LINK_GAMBAR_3))
                            //{
                            //    var uploadImg = UploadImage(item.LINK_GAMBAR_3, data.token);
                            //    if (uploadImg.status == 1)
                            //    {
                            //        xmlString += "<Image><![CDATA[" + uploadImg.message + "]]></Image>";
                            //    }
                            //}
                            //end remark by calvin 19 agustus 2019

                            //END CHANGE BY CALVIN 10 JUNI 2019
                            xmlString += "</Images>";
                            xmlString += "</Sku>";
                        }
                    }
                }
                xmlString += "</Skus>";
            }
            xmlString += "</Product></Request>";


            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/product/create");
            request.AddApiParameter("payload", xmlString);

            //LazopResponse response = client.Execute(request, data.token);
            //try
            //{

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Create Product",
                REQUEST_DATETIME = DateTime.UtcNow.AddHours(7),
                REQUEST_ATTRIBUTE_1 = data.kdBrg,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data.token, currentLog);

            LazopResponse response = client.Execute(request, data.token);

            var res = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaCreateBarangResponse)) as LazadaCreateBarangResponse;
            if (res.code.Equals("0"))
            {
                ret.status = 1;
                //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
                var result = EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + res.data.item_id + "',LINK_STATUS='Buat Produk Berhasil', LINK_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "',LINK_ERROR = '0;Buat Produk;;' WHERE BRG = '" + data.kdBrg + "' AND IDMARKET = '" + data.idMarket + "'");
                foreach (var item in res.data.sku_list)
                {
                    EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + item.seller_sku + "',LINK_STATUS='Buat Produk Berhasil', LINK_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "',LINK_ERROR = '0;;;' WHERE BRG = '" + item.seller_sku + "' AND IDMARKET = '" + data.idMarket + "'");
                }

                if (result == 1)
                {
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data.key, currentLog);
                }
                else
                {
                    currentLog.REQUEST_EXCEPTION = "failed to update brg_mp;execute result=" + result;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data.token, currentLog);
                    throw new Exception("failed to update brg_mp;execute result=" + result);
                }
            }
            else
            {
                if (res.detail != null)
                {
                    if (res.detail.Length == 1)
                    {
                        if (!string.IsNullOrEmpty(res.detail[0].field))
                        {
                            ret.message = res.detail[0].field + " : " + res.detail[0].message;
                        }
                        else
                        {
                            ret.message = res.detail[0].message;

                        }
                    }
                    else if (res.detail.Length > 1)
                    {
                        ret.message = "";
                        for (int i = 0; i < res.detail.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(res.detail[i].field))
                            {
                                ret.message += res.detail[i].field + " : " + res.detail[i].message + "\n";
                            }
                            else
                            {
                                ret.message += res.detail[i].message + "\n";

                            }
                        }
                    }
                }
                else
                {
                    ret.message = res.message;
                }

                currentLog.REQUEST_EXCEPTION = ret.message;
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data.token, currentLog);

                throw new Exception(ret.message);
            }

            //}
            //catch (Exception ex)
            //{
            //    currentLog.REQUEST_EXCEPTION = ex.Message;
            //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data.token, currentLog);
            //}
            return ret;
        }

        public static string XmlEscape(string unescaped)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode node = doc.CreateElement("root");
            node.InnerText = unescaped;
            return node.InnerXml;
        }

        public static string XmlUnescape(string escaped)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode node = doc.CreateElement("root");
            node.InnerXml = escaped;
            return node.InnerText;
        }

        public BindingBase UpdateProduct(BrgViewModel data)
        {
            var ret = new BindingBase();
            ret.status = 0;


            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Update Product",
                REQUEST_DATETIME = DateTime.UtcNow.AddHours(7),
                REQUEST_ATTRIBUTE_1 = data.kdBrg,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data.token, currentLog);

            //change by calvin 16 mei 2019
            //string sSQL = "SELECT * FROM (";
            //for (int i = 1; i <= 50; i++)
            //{
            //    sSQL += "SELECT A.ACODE_" + i.ToString() + " AS CATEGORY_CODE,A.ANAME_" + i.ToString() + " AS CATEGORY_NAME,B.ATYPE" + i.ToString();
            //    sSQL += " AS CATEGORY_TYPE,A.AVALUE_" + i.ToString() + " AS VALUE FROM STF02H A INNER JOIN MO.DBO.ATTRIBUTE_LAZADA B ON A.CATEGORY_CODE = B.CATEGORY_CODE WHERE A.BRG='" + data.kdBrg + "' " + System.Environment.NewLine;
            //    if (i < 50)
            //    {
            //        sSQL += "UNION ALL " + System.Environment.NewLine;
            //    }
            //}

            //DataSet dsSku = EDB.GetDataSet("sCon", "STF02H", sSQL + ") ASD WHERE ISNULL(CATEGORY_CODE,'') <> '' AND CATEGORY_TYPE <> 'normal' AND ISNULL(VALUE, 'NULL') <> 'NULL' ");
            //DataSet dsNormal = EDB.GetDataSet("sCon", "STF02H", sSQL + ") ASD WHERE ISNULL(CATEGORY_CODE,'') <> '' AND CATEGORY_TYPE = 'normal' AND ISNULL(VALUE, 'NULL') <> 'NULL' ");
            var stf02h = ErasoftDbContext.STF02H.Where(p => p.BRG == data.kdBrg && p.IDMARKET.ToString() == data.idMarket).FirstOrDefault();
            List<string> dsSku = new List<string>();
            List<string> dsNormal = new List<string>();

            var attributeLzd = getAttrLzd(stf02h.CATEGORY_CODE);
            for (int i = 1; i <= 50; i++)
            {
                string attribute_id = Convert.ToString(attributeLzd["ANAME" + i.ToString()]);
                string attribute_type = Convert.ToString(attributeLzd["ATYPE" + i.ToString()]);
                if (!string.IsNullOrWhiteSpace(attribute_id))
                {
                    if (attribute_type != "normal")
                    {
                        dsSku.Add(attribute_id);
                    }
                    else
                    {
                        dsNormal.Add(attribute_id);
                    }
                }
            }
            //end change by calvin 16 mei 2019

            string primCategory = EDB.GetFieldValue("MOConnectionString", "STF02H", "BRG = '" + data.kdBrg + "' AND IDMARKET = '" + data.idMarket + "'", "category_code").ToString();
            string xmlString = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>";
            xmlString = "<Request><Product><PrimaryCategory>" + primCategory + "</PrimaryCategory>";
            xmlString += "<Attributes><name>" + data.nama + (string.IsNullOrEmpty(data.nama2) ? "" : " " + data.nama2) + "</name>";
            //xmlString += "<short_description><![CDATA[" + data.deskripsi + "]]></short_description>";
            xmlString += "<description><![CDATA[" + data.deskripsi.Replace(System.Environment.NewLine, "<br>") + "]]></description>";

            //xmlString += "<brand>No Brand</brand>";
            xmlString += "<brand><![CDATA[" + stf02h.ANAME_38 + "]]></brand>";

            //xmlString += "<model>" + data.kdBrg + "</model>";
            //xmlString += "<warranty_type>No Warranty</warranty_type>";

            //change 16 Mei 2019, get attr from api
            //for (int i = 0; i < dsNormal.Tables[0].Rows.Count; i++)
            //{
            //    xmlString += "<" + dsNormal.Tables[0].Rows[i]["CATEGORY_CODE"].ToString() + ">";
            //    xmlString += dsNormal.Tables[0].Rows[i]["VALUE"].ToString();
            //    xmlString += "</" + dsNormal.Tables[0].Rows[i]["CATEGORY_CODE"].ToString() + ">";
            //}
            Dictionary<string, string> lzdAttrWithVal = new Dictionary<string, string>();
            Dictionary<string, string> lzdAttrSkuWithVal = new Dictionary<string, string>();
            for (int i = 1; i <= 50; i++)
            {
                string attribute_id = Convert.ToString(stf02h["ACODE_" + i.ToString()]);
                string value = Convert.ToString(stf02h["AVALUE_" + i.ToString()]);
                if (!string.IsNullOrWhiteSpace(value) && value != "null")
                {
                    if (dsNormal.Contains(attribute_id))
                    {
                        if (!lzdAttrWithVal.ContainsKey(attribute_id))
                        {
                            lzdAttrWithVal.Add(attribute_id, value.Trim());
                        }
                    }
                    else if (dsSku.Contains(attribute_id))
                    {
                        if (!lzdAttrSkuWithVal.ContainsKey(attribute_id))
                        {
                            lzdAttrSkuWithVal.Add(attribute_id, value.Trim());
                        }
                    }
                }
            }
            //for (int i = 0; i < lzdAttrWithVal.Count; i++)
            //{
            //    xmlString += "<" + dsNormal[i].ToString() + ">";
            //    xmlString += lzdAttrWithVal[dsNormal[i].ToString()].ToString();
            //    xmlString += "</" + dsNormal[i].ToString() + ">";                
            //}
            foreach (var lzdAttr in lzdAttrWithVal)
            {
                xmlString += "<" + lzdAttr.Key + ">";
                xmlString += lzdAttr.Value.ToString();
                xmlString += "</" + lzdAttr.Key + ">";
            }
            //end change 16 Mei 2019, get attr from api

            xmlString += "</Attributes>";

            var stf02 = ErasoftDbContext.STF02.Where(p => p.BRG == data.kdBrg).FirstOrDefault();
            if (Convert.ToString(stf02.TYPE) == "3")
            {
                //xmlString += "<Skus><Sku><SellerSku>" + data.kdBrg + "</SellerSku>";
                xmlString += "<Skus><Sku><SellerSku>" + stf02h.BRG_MP + "</SellerSku>";
                //xmlString += "<active>" + (data.activeProd ? "true" : "false") + "</active>";
                xmlString += "<Status>" + (data.activeProd ? "active" : "inactive") + "</Status>";
                //xmlString += "<color_family>Not Specified</color_family>";
                //xmlString += "<quantity>1</quantity>";
                xmlString += "<price>" + data.harga + "</price>";
                //xmlString += "<size>Int: One size</size>";
                xmlString += "<package_length>" + data.length + "</package_length><package_height>" + data.height + "</package_height>";
                xmlString += "<package_width>" + data.width + "</package_width><package_weight>" + Convert.ToDouble(data.weight) / 1000 + "</package_weight>";//weight in kg
                xmlString += "<Images>";
                if (!string.IsNullOrEmpty(data.imageUrl))
                    xmlString += "<Image><![CDATA[" + data.imageUrl + "]]></Image>";
                if (!string.IsNullOrEmpty(data.imageUrl2))
                    xmlString += "<Image><![CDATA[" + data.imageUrl2 + "]]></Image>";
                if (!string.IsNullOrEmpty(data.imageUrl3))
                    xmlString += "<Image><![CDATA[" + data.imageUrl3 + "]]></Image>";
                xmlString += "</Images>";


                //change 16 Mei 2019, get attr from api
                //for (int i = 0; i < dsSku.Tables[0].Rows.Count; i++)
                //{
                //    xmlString += "<" + dsSku.Tables[0].Rows[i]["CATEGORY_CODE"].ToString() + ">";
                //    xmlString += dsSku.Tables[0].Rows[i]["VALUE"].ToString();
                //    xmlString += "</" + dsSku.Tables[0].Rows[i]["CATEGORY_CODE"].ToString() + ">";
                //}
                foreach (var lzdSkuAttr in lzdAttrSkuWithVal)
                {
                    xmlString += "<" + lzdSkuAttr.Key + ">";
                    xmlString += lzdSkuAttr.Value.ToString();
                    xmlString += "</" + lzdSkuAttr.Key + ">";
                }
                //end change 16 Mei 2019, get attr from api
                xmlString += "</Sku></Skus>";
            }
            else if (Convert.ToString(stf02.TYPE) == "4")
            {
                var ListSettingVariasi = ErasoftDbContext.STF02I.Where(p => p.BRG == data.kdBrg && p.MARKET == "LAZADA").ToList();
                var ListStf02Var = ErasoftDbContext.STF02.Where(p => p.PART == data.kdBrg).ToList();
                var ListStf02Var_BRG = ListStf02Var.Select(p => p.BRG).ToList();
                int idmarket_int = Convert.ToInt32(data.idMarket);
                var List_STF02H_Var = ErasoftDbContext.STF02H.Where(p => ListStf02Var_BRG.Contains(p.BRG) && p.IDMARKET == idmarket_int).ToList();

                //untuk pastikan tidak ada duplikat kombinasi attribute variasi
                Dictionary<string, string> KombinasiAttribute = new Dictionary<string, string>();
                foreach (var item in ListStf02Var)
                {
                    if (!string.IsNullOrWhiteSpace(item.Sort8))
                    {
                        var getMPJudul_and_ValueVar = ListSettingVariasi.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == item.Sort8).FirstOrDefault();
                        string attributeUnique = getMPJudul_and_ValueVar.MP_JUDUL_VAR + "[;]" + getMPJudul_and_ValueVar.MP_VALUE_VAR + "[;]" + item.BRG;
                        if (!KombinasiAttribute.ContainsKey(attributeUnique))
                        {
                            KombinasiAttribute.Add(attributeUnique, item.BRG);


                        }
                        if (!string.IsNullOrWhiteSpace(item.Sort9))
                        {
                            var getMPJudul_and_ValueVarLv2 = ListSettingVariasi.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == item.Sort9).FirstOrDefault();
                            string attributeUniqueLv2 = getMPJudul_and_ValueVarLv2.MP_JUDUL_VAR + "[;]" + getMPJudul_and_ValueVarLv2.MP_VALUE_VAR + "[;]" + item.BRG;
                            if (!KombinasiAttribute.ContainsKey(attributeUniqueLv2))
                            {
                                KombinasiAttribute.Add(attributeUniqueLv2, item.BRG);
                            }
                        }
                    }
                }
                //end untuk pastikan tidak ada duplikat kombinasi attribute variasi
                List<string> attributesAdded;
                xmlString += "<Skus>";
                foreach (var item in ListStf02Var)
                {
                    attributesAdded = new List<string>();
                    bool input = false;
                    foreach (var attribute in KombinasiAttribute)
                    {
                        if (attribute.Value == item.BRG)
                        {
                            input = true;
                        }
                    }

                    var GetStf02h = List_STF02H_Var.Where(p => p.BRG == item.BRG).FirstOrDefault();
                    if (input && (GetStf02h != null))
                    {
                        if (!string.IsNullOrEmpty(GetStf02h.BRG_MP))
                        {
                            //xmlString += "<Sku><SellerSku>" + item.BRG + "</SellerSku>";
                            xmlString += "<Sku><SellerSku>" + GetStf02h.BRG_MP + "</SellerSku>";
                            //xmlString += "<active>" + (data.activeProd ? "true" : "false") + "</active>";
                            xmlString += "<Status>" + (data.activeProd ? "active" : "inactive") + "</Status>";

                            foreach (var attribute in KombinasiAttribute)
                            {
                                if (attribute.Value == item.BRG)
                                {
                                    string[] getId = attribute.Key.Split(new string[] { "[;]" }, StringSplitOptions.None);
                                    xmlString += "<" + getId[0] + ">" + getId[1] + "</" + getId[0] + ">";
                                    attributesAdded.Add(getId[0]);
                                }
                            }

                            //CEK JIKA ADA ATTRIBUTE YANG KURANG DI STF02I ( MAPPING ATTRIBUTE ), MAKA AMBIL KE STF02H
                            //change 16 Mei 2019, get attr from api
                            //for (int i = 0; i < dsSku.Tables[0].Rows.Count; i++)
                            //{
                            //    if (!attributesAdded.Contains(dsSku.Tables[0].Rows[i]["CATEGORY_CODE"].ToString()))
                            //    {
                            //        xmlString += "<" + dsSku.Tables[0].Rows[i]["CATEGORY_CODE"].ToString() + ">";
                            //        xmlString += dsSku.Tables[0].Rows[i]["VALUE"].ToString();
                            //        xmlString += "</" + dsSku.Tables[0].Rows[i]["CATEGORY_CODE"].ToString() + ">";
                            //    }
                            //}
                            for (int i = 0; i < lzdAttrSkuWithVal.Count; i++)
                            {
                                if (!attributesAdded.Contains(dsSku[i].ToString()))
                                {
                                    xmlString += "<" + dsSku[i].ToString() + ">";
                                    xmlString += lzdAttrSkuWithVal[dsSku[i].ToString()].ToString();
                                    xmlString += "</" + dsSku[i].ToString() + ">";
                                }
                            }
                            //end change 16 Mei 2019, get attr from api

                            //change 1/8/2019, gunakan hjual stf02h
                            //xmlString += "<price>" + data.harga + "</price>";
                            xmlString += "<price>" + GetStf02h.HJUAL + "</price>";
                            //change 1/8/2019, gunakan hjual stf02h
                            xmlString += "<package_length>" + data.length + "</package_length><package_height>" + data.height + "</package_height>";
                            xmlString += "<package_width>" + data.width + "</package_width><package_weight>" + Convert.ToDouble(data.weight) / 1000 + "</package_weight>";//weight in kg
                            xmlString += "<Images>";
                            //CHANGE BY CALVIN 10 JUNI 2019
                            //if (!string.IsNullOrEmpty(data.imageUrl))
                            //    xmlString += "<Image><![CDATA[" + data.imageUrl + "]]></Image>";
                            //if (!string.IsNullOrEmpty(data.imageUrl2))
                            //    xmlString += "<Image><![CDATA[" + data.imageUrl2 + "]]></Image>";
                            //if (!string.IsNullOrEmpty(data.imageUrl3))
                            //    xmlString += "<Image><![CDATA[" + data.imageUrl3 + "]]></Image>";
                            if (!string.IsNullOrEmpty(item.LINK_GAMBAR_1))
                            {
                                var uploadImg = UploadImage(item.LINK_GAMBAR_1, data.token);
                                if (uploadImg.status == 1)
                                {
                                    xmlString += "<Image><![CDATA[" + uploadImg.message + "]]></Image>";
                                }
                            }
                            if (!string.IsNullOrEmpty(item.LINK_GAMBAR_2))
                            {
                                var uploadImg = UploadImage(item.LINK_GAMBAR_2, data.token);
                                if (uploadImg.status == 1)
                                {
                                    xmlString += "<Image><![CDATA[" + uploadImg.message + "]]></Image>";
                                }
                            }
                            if (!string.IsNullOrEmpty(item.LINK_GAMBAR_3))
                            {
                                var uploadImg = UploadImage(item.LINK_GAMBAR_3, data.token);
                                if (uploadImg.status == 1)
                                {
                                    xmlString += "<Image><![CDATA[" + uploadImg.message + "]]></Image>";
                                }
                            }
                            //END CHANGE BY CALVIN 10 JUNI 2019
                            xmlString += "</Images>";
                            xmlString += "</Sku>";
                        }

                    }
                }
                xmlString += "</Skus>";
            }
            xmlString += "</Product></Request>";

            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/product/update");
            request.AddApiParameter("payload", xmlString);

            //LazopResponse response = client.Execute(request, data.token);
            try
            {
                LazopResponse response = client.Execute(request, data.token);

                //change by calvin 10 juni 2019
                //var res = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaCreateBarangResponse)) as LazadaCreateBarangResponse;
                var res = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaCommonRes)) as LazadaCommonRes;
                //end change by calvin 10 juni 2019
                if (res.code.Equals("0"))
                {
                    ret.status = 1;
                    //change by calvin 10 juni 2019
                    ////DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
                    //var result = EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + res.data.item_id + "' WHERE BRG = '" + data.kdBrg + "' AND IDMARKET = '" + data.idMarket + "'");
                    //foreach (var item in res.data.sku_list)
                    //{
                    //    EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + item.seller_sku + "' WHERE BRG = '" + item.seller_sku + "' AND IDMARKET = '" + data.idMarket + "'");
                    //}

                    //if (result == 1)
                    //{
                    //    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data.key, currentLog);
                    //}
                    //else
                    //{
                    //    currentLog.REQUEST_EXCEPTION = "failed to update brg_mp;execute result=" + result;
                    //    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data.token, currentLog);
                    //}
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data.key, currentLog);
                    //end change by calvin 10 juni 2019
                }
                else
                {
                    //change by calvin 10 juni 2019
                    //if (res.detail != null)
                    //{
                    //    if (res.detail.Length == 1)
                    //    {
                    //        if (!string.IsNullOrEmpty(res.detail[0].field))
                    //        {
                    //            ret.message = res.detail[0].field + " : " + res.detail[0].message;
                    //        }
                    //        else
                    //        {
                    //            ret.message = res.detail[0].message;

                    //        }
                    //    }
                    //    else if (res.detail.Length > 1)
                    //    {
                    //        ret.message = "";
                    //        for (int i = 0; i < res.detail.Length; i++)
                    //        {
                    //            if (!string.IsNullOrEmpty(res.detail[i].field))
                    //            {
                    //                ret.message += res.detail[i].field + " : " + res.detail[i].message + "\n";
                    //            }
                    //            else
                    //            {
                    //                ret.message += res.detail[i].message + "\n";

                    //            }
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    ret.message = res.message;
                    //}
                    currentLog.REQUEST_EXCEPTION = ret.message;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data.token, currentLog);
                    //end change by calvin 10 juni 2019
                }

            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data.token, currentLog);
            }
            return ret;
        }

        public BindingBase setDisplay(string kdBrg, bool display, string token)
        {
            var ret = new BindingBase();
            ret.status = 0;

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Show Product",
                REQUEST_DATETIME = DateTime.UtcNow.AddHours(7),
                REQUEST_ATTRIBUTE_1 = kdBrg,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, token, currentLog);

            string xmlString = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>";
            xmlString += "<Request><Product><Skus><Sku>";
            xmlString += "<SellerSku>" + kdBrg + "</SellerSku>";
            //xmlString += "<active>" + (display ? "true" : "false") + "</active>";
            xmlString += "<Status>" + (display ? "active" : "inactive") + "</Status>";
            xmlString += "</Sku></Skus></Product></Request>";

            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/product/update");
            request.AddApiParameter("payload", xmlString);

            //LazopResponse response = client.Execute(request, token);
            try
            {
                LazopResponse response = client.Execute(request, token);

                var res = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaResponseObj)) as LazadaResponseObj;
                if (res.code.Equals("0"))
                {
                    ret.status = 1;
                    ret.message = response.Body;
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, token, currentLog);
                }
                else
                {
                    ret.message = res.detail[0].message;
                    currentLog.REQUEST_EXCEPTION = res.detail[0].message;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, token, currentLog);
                }

            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, token, currentLog);
            }
            return ret;
        }

        public BindingBase setPromo(PromoLazadaObj data)
        {
            var ret = new BindingBase();
            ret.status = 0;

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Set Promo",
                REQUEST_DATETIME = DateTime.UtcNow.AddHours(7),
                REQUEST_ATTRIBUTE_1 = data.kdBrg,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data.token, currentLog);

            string xmlString = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>";
            xmlString += "<Request><Product><Skus><Sku>";
            xmlString += "<SellerSku>" + data.kdBrg + "</SellerSku>";
            xmlString += "<special_price>" + data.promoPrice + "</special_price>";
            xmlString += "<special_from_date>" + data.fromDt + "</special_from_date>";
            xmlString += "<special_to_date>" + data.toDt + "</special_to_date>";
            xmlString += "</Sku></Skus></Product></Request>";

            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/product/update");
            request.AddApiParameter("payload", xmlString);

            //LazopResponse response = client.Execute(request, data.token);
            try
            {
                LazopResponse response = client.Execute(request, data.token);
                var res = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaResponseObj)) as LazadaResponseObj;
                if (res.code.Equals("0"))
                {
                    ret.status = 1;
                    ret.message = response.Body;
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data.token, currentLog);
                }
                else
                {
                    ret.message = res.detail[0].message;
                    currentLog.REQUEST_EXCEPTION = res.detail[0].message;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data.token, currentLog);
                }
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data.token, currentLog);
            }

            return ret;
        }

        public BindingBase UpdatePriceQuantity(string kdBrg, string harga, string qty, string token)
        {
            var ret = new BindingBase();
            ret.status = 0;

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Update Price/Stok Product",
                REQUEST_DATETIME = DateTime.UtcNow.AddHours(7),
                REQUEST_ATTRIBUTE_1 = kdBrg,
                REQUEST_ATTRIBUTE_2 = harga,
                REQUEST_ATTRIBUTE_3 = qty,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, token, currentLog);

            if (string.IsNullOrEmpty(kdBrg))
            {
                ret.message = "Item not linked to MP";
                currentLog.REQUEST_EXCEPTION = ret.message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, token, currentLog);
                return ret;
            }
            string xmlString = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?><Request><Product>";
            xmlString += "<Skus><Sku><SellerSku>" + kdBrg + "</SellerSku>";
            if (!string.IsNullOrEmpty(qty))
                xmlString += "<Quantity>" + qty + "</Quantity>";
            if (!string.IsNullOrEmpty(harga))
                xmlString += "<Price>" + harga + "</Price>";
            xmlString += "</Sku></Skus></Product></Request>";


            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/product/price_quantity/update");
            request.AddApiParameter("payload", xmlString);
            try
            {
                LazopResponse response = client.Execute(request, token);
                var res = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaResponseObj)) as LazadaResponseObj;
                if (res.code.Equals("0"))
                {
                    ret.status = 1;
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, token, currentLog);
                }
                else
                {
                    ret.message = res.detail[0].message;
                    currentLog.REQUEST_EXCEPTION = res.detail[0].message;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, token, currentLog);
                }
            }
            catch (Exception ex)
            {
                ret.message = ex.ToString();
                currentLog.REQUEST_EXCEPTION = ex.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, token, currentLog);
            }


            return ret;
        }

        public BindingBase UpdatePromoPrice(string kdBrg, double SalePrice, DateTime SaleStartDate, DateTime SaleEndDate, string token)
        {
            var ret = new BindingBase();
            ret.status = 0;

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Update Promo Product",
                REQUEST_DATETIME = DateTime.UtcNow.AddHours(7),
                REQUEST_ATTRIBUTE_1 = kdBrg,
                REQUEST_ATTRIBUTE_2 = SalePrice.ToString(),
                REQUEST_ATTRIBUTE_3 = SaleStartDate + ":" + SaleEndDate,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, token, currentLog);

            if (string.IsNullOrEmpty(kdBrg))
            {
                ret.message = "Item not linked to MP";
                currentLog.REQUEST_EXCEPTION = ret.message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, token, currentLog);
                return ret;
            }
            string xmlString = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?><Request><Product>";
            xmlString += "<Skus><Sku><SellerSku>" + kdBrg + "</SellerSku>";
            xmlString += "<SalePrice>" + SalePrice + "</SalePrice>";
            if (SaleEndDate != DateTime.Today && SaleStartDate != DateTime.Today)
            {
                xmlString += "<SaleStartDate>" + SaleStartDate.ToString("yyyy-MM-dd") + "</SaleStartDate>";
                xmlString += "<SaleEndDate>" + SaleEndDate.ToString("yyyy-MM-dd") + "</SaleEndDate>";
            }
            xmlString += "</Sku></Skus></Product></Request>";


            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/product/price_quantity/update");
            request.AddApiParameter("payload", xmlString);
            try
            {
                LazopResponse response = client.Execute(request, token);
                var res = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaResponseObj)) as LazadaResponseObj;
                if (res.code.Equals("0"))
                {
                    ret.status = 1;
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, token, currentLog);
                }
                else
                {
                    ret.message = res.detail[0].message;
                    currentLog.REQUEST_EXCEPTION = res.detail[0].message;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, token, currentLog);
                }
            }
            catch (Exception ex)
            {
                ret.message = ex.ToString();
                currentLog.REQUEST_EXCEPTION = ex.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, token, currentLog);
            }


            return ret;
        }

        public BindingBase UpdatePromoPrice2(string kdBrg, double SalePrice, DateTime SaleStartDate, DateTime SaleEndDate, string token)
        {
            var ret = new BindingBase();
            ret.status = 0;

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Update Promo Product",
                REQUEST_DATETIME = DateTime.UtcNow.AddHours(7),
                REQUEST_ATTRIBUTE_1 = kdBrg,
                REQUEST_ATTRIBUTE_2 = SalePrice.ToString(),
                REQUEST_ATTRIBUTE_3 = SaleStartDate + ":" + SaleEndDate,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, token, currentLog);

            if (string.IsNullOrEmpty(kdBrg))
            {
                ret.message = "Item not linked to MP";
                currentLog.REQUEST_EXCEPTION = ret.message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, token, currentLog);
                return ret;
            }
            string xmlString = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?><Request><Product>";
            xmlString += "<Skus><Sku><SellerSku>" + kdBrg + "</SellerSku>";
            xmlString += "<SalePrice>" + SalePrice + "</SalePrice>";
            if (SaleEndDate != DateTime.Today && SaleStartDate != DateTime.Today)
            {
                xmlString += "<SaleStartDate>" + SaleStartDate.ToString("yyyy-MM-dd") + "</SaleStartDate>";
                xmlString += "<SaleEndDate>" + SaleEndDate.ToString("yyyy-MM-dd") + "</SaleEndDate>";
            }
            xmlString += "</Sku></Skus></Product></Request>";


            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/product/update");
            request.AddApiParameter("payload", xmlString);
            try
            {
                LazopResponse response = client.Execute(request, token);
                var res = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaResponseObj)) as LazadaResponseObj;
                if (res.code.Equals("0"))
                {
                    ret.status = 1;
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, token, currentLog);
                }
                else
                {
                    ret.message = res.detail[0].message;
                    currentLog.REQUEST_EXCEPTION = res.detail[0].message;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, token, currentLog);
                }
            }
            catch (Exception ex)
            {
                ret.message = ex.ToString();
                currentLog.REQUEST_EXCEPTION = ex.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, token, currentLog);
            }


            return ret;
        }

        public BindingBase GetShipment(string cust, string accessToken)
        {
            var ret = new BindingBase();
            ret.status = 0;

            accessToken = ErasoftDbContext.ARF01.Where(p => p.CUST == cust).SingleOrDefault().TOKEN;

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Get Shipment Provider",
                REQUEST_DATETIME = DateTime.UtcNow.AddHours(7),
                REQUEST_ATTRIBUTE_1 = cust,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, accessToken, currentLog);

            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/shipment/providers/get");
            request.SetHttpMethod("GET");
            //LazopResponse response = client.Execute(request, accessToken);
            try
            {
                LazopResponse response = client.Execute(request, accessToken);
                var bindDelivery = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(ShipmentLazada)) as ShipmentLazada;
                if (bindDelivery != null)
                {
                    if (bindDelivery.code.Equals("0"))
                    {
                        if (bindDelivery.data.shipment_providers.Count() > 0)
                        {
                            var tempProvLzd = ErasoftDbContext.DELIVERY_PROVIDER_LAZADA.Where(m => m.CUST == cust).ToList();
                            foreach (Shipment_Providers shipProv in bindDelivery.data.shipment_providers)
                            {
                                if (tempProvLzd.Where(m => m.NAME == shipProv.name).ToList().Count == 0)
                                {
                                    var newProvider = new DELIVERY_PROVIDER_LAZADA();
                                    newProvider.CUST = cust;
                                    newProvider.NAME = shipProv.name;
                                    newProvider.COD = shipProv.cod;

                                    ErasoftDbContext.DELIVERY_PROVIDER_LAZADA.Add(newProvider);
                                    ErasoftDbContext.SaveChanges();
                                }

                            }
                            manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, accessToken, currentLog);

                        }
                    }
                    else
                    {
                        ret.message = bindDelivery.message;
                        currentLog.REQUEST_EXCEPTION = bindDelivery.message;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, accessToken, currentLog);
                    }
                }
            }
            catch (Exception ex)
            {
                ret.message = ex.ToString();
                currentLog.REQUEST_EXCEPTION = ex.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, accessToken, currentLog);
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Konfirmasi Pengiriman Pesanan {obj} ke Lazada Gagal.")]
        public LazadaToDeliver GetToPackedToDeliver(string dbPathEra, string namaPemesan, string log_CUST, string log_ActionCategory, string log_ActionName, string uname, List<string> orderItemId, string shippingProvider, string accessToken)
        {
            SetupContext(dbPathEra, uname);
            var ret = new LazadaToDeliver();
            string ordItems = "";
            if (orderItemId.Count > 1)
            {
                foreach (var id in orderItemId)
                {
                    ordItems += id;
                    ordItems += ",";
                }
                ordItems = ordItems.Substring(0, ordItems.Length - 1);
            }
            else
            {
                ordItems = orderItemId[0];
            }

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
            //    REQUEST_ACTION = "Set Status Order to Packed",
            //    REQUEST_DATETIME = DateTime.Now,
            //    REQUEST_ATTRIBUTE_1 = ordItems,
            //    REQUEST_STATUS = "Pending",
            //};
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, accessToken, currentLog);

            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/order/pack");
            request.AddApiParameter("shipping_provider", shippingProvider);
            request.AddApiParameter("delivery_type", "dropship");
            request.AddApiParameter("order_item_ids", "[" + ordItems + "]");
            //try
            //{
            LazopResponse response = client.Execute(request, accessToken);
            ret = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaToDeliver)) as LazadaToDeliver;
            if (ret.code.Equals("0"))
            {
                var orderid = orderItemId[0];
                var orderDetail = ErasoftDbContext.SOT01B.Where(p => p.ORDER_ITEM_ID == orderid).FirstOrDefault();
                if (orderDetail != null)
                {
                    var order = ErasoftDbContext.SOT01A.Where(p => p.NO_BUKTI == orderDetail.NO_BUKTI).FirstOrDefault();
                    if (order != null)
                    {
                        order.TRACKING_SHIPMENT = ret.data.order_items[0].tracking_number;
                        order.status_kirim = "2";
                        if (string.IsNullOrWhiteSpace(order.TRACKING_SHIPMENT))
                        {
                            order.status_kirim = "1";
                        }
                        ErasoftDbContext.SaveChanges();
                        //
                        //
                        //                        string EDBConnID = EDB.GetConnectionString("ConnId");
                        //                        var sqlStorage = new SqlServerStorage(EDBConnID);
                        //                        var jobClient = new BackgroundJobClient(sqlStorage);
                        //                        jobClient.Enqueue<LazadaControllerJob>(x => x.GetToDeliver(dbPathEra, order.NAMAPEMESAN, order.CUST, "Pesanan", "Ganti Status", uname, orderItemId, order.SHIPMENT, order.TRACKING_SHIPMENT, accessToken));
                    }
                }
            }
            else
            {
                var orderid = orderItemId[0];
                var orderDetail = ErasoftDbContext.SOT01B.Where(p => p.ORDER_ITEM_ID == orderid).FirstOrDefault();

                EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE SOT01A SET STATUS_KIRIM='2' WHERE NO_BUKTI = '" + orderDetail.NO_BUKTI + "'");
                throw new Exception(ret.message);
            }
            //}
            //catch (Exception ex)
            //{
            //    ret.message = ex.ToString();
            //    currentLog.REQUEST_EXCEPTION = ex.Message;
            //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, accessToken, currentLog);
            //}

            return ret;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Konfirmasi Pengiriman Pesanan {obj} ke Lazada Gagal.")]
        public LazadaToDeliver GetToDeliver(string dbPathEra, string namaPemesan, string log_CUST, string log_ActionCategory, string log_ActionName, string uname, List<string> orderItemId, string shippingProvider, string trackingNumber, string accessToken)
        {
            var ret = new LazadaToDeliver();
            SetupContext(dbPathEra, uname);
            string ordItems = "";
            if (orderItemId.Count > 1)
            {
                foreach (var id in orderItemId)
                {
                    ordItems += id;
                    ordItems += ",";
                }
                ordItems = ordItems.Substring(0, ordItems.Length - 1);
            }
            else
            {
                ordItems = orderItemId[0];
            }

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
            //    REQUEST_ACTION = "Set Status Order to Deliver",
            //    REQUEST_DATETIME = DateTime.Now,
            //    REQUEST_ATTRIBUTE_1 = ordItems,
            //    REQUEST_STATUS = "Pending",
            //};
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, accessToken, currentLog);

            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/order/rts");
            request.AddApiParameter("shipping_provider", shippingProvider);
            request.AddApiParameter("delivery_type", "dropship");
            request.AddApiParameter("order_item_ids", "[" + ordItems + "]");
            request.AddApiParameter("tracking_number", trackingNumber);
            //try
            //{
            LazopResponse response = client.Execute(request, accessToken);
            ret = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaToDeliver)) as LazadaToDeliver;
            if (ret.code.Equals("0"))
            {
                var orderid = orderItemId[0];
                var orderDetail = ErasoftDbContext.SOT01B.Where(p => p.ORDER_ITEM_ID == orderid).FirstOrDefault();

                EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE SOT01A SET STATUS_KIRIM='2' WHERE NO_BUKTI = '" + orderDetail.NO_BUKTI + "'");
                //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, accessToken, currentLog);
            }
            else
            {
                var orderid = orderItemId[0];
                var orderDetail = ErasoftDbContext.SOT01B.Where(p => p.ORDER_ITEM_ID == orderid).FirstOrDefault();

                EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE SOT01A SET STATUS_KIRIM='1' WHERE NO_BUKTI = '" + orderDetail.NO_BUKTI + "'");
                throw new Exception(ret.message);
                //currentLog.REQUEST_EXCEPTION = ret.message;
                //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, accessToken, currentLog);
            }
            //}
            //catch (Exception ex)
            //{
            //    ret.message = ex.ToString();
            //    currentLog.REQUEST_EXCEPTION = ex.Message;
            //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, accessToken, currentLog);
            //}
            return ret;

        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Update Status Cancel Pesanan {obj} ke Lazada Gagal.")]
        public BindingBase SetStatusToCanceled(string dbPathEra, string namaPemesan, string log_CUST, string log_ActionCategory, string log_ActionName, string orderItemId, string accessToken, string uname)
        {
            var ret = new BindingBase();
            ret.status = 0;
            SetupContext(dbPathEra, uname);

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
            //    REQUEST_ACTION = "Set Status Order to Cancel",
            //    REQUEST_DATETIME = DateTime.Now,
            //    REQUEST_ATTRIBUTE_1 = orderItemId,
            //    REQUEST_STATUS = "Pending",
            //};
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, accessToken, currentLog);

            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/order/cancel");
            request.AddApiParameter("reason_detail", "Out of stock");
            request.AddApiParameter("reason_id", "15");
            request.AddApiParameter("order_item_id", orderItemId);
            //try
            //{
            LazopResponse response = client.Execute(request, accessToken);
            //ret = tgl + ":" + param;
            var resCancel = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaCancelOrder)) as LazadaCancelOrder;
            if (resCancel != null)
            {
                if (resCancel.code.Equals("0"))
                {
                    ret.status = 1;
                    //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, accessToken, currentLog);
                }
                else
                {
                    throw new Exception(resCancel.message);
                    //currentLog.REQUEST_EXCEPTION = ret.message;
                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, accessToken, currentLog);
                }
            }
            //}
            //catch (Exception ex)
            //{
            //    ret.message = ex.Message;
            //    currentLog.REQUEST_EXCEPTION = ex.Message;
            //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, accessToken, currentLog);
            //}

            return ret;

        }
        public LazadaGetLabel GetLabel(List<string> orderItemId, string accessToken)
        {
            string ordItems = "";
            if (orderItemId.Count > 1)
            {
                foreach (var id in orderItemId)
                {
                    ordItems += id;
                    ordItems += ",";
                }
                ordItems = ordItems.Substring(0, ordItems.Length - 1);
            }
            else
            {
                if (orderItemId.Count == 1)
                    ordItems = orderItemId[0];
            }

            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/order/document/get");
            request.SetHttpMethod("GET");
            request.AddApiParameter("doc_type", "shippingLabel");
            request.AddApiParameter("order_item_ids", "[" + ordItems + "]");
            LazopResponse response = client.Execute(request, accessToken);

            return Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaGetLabel)) as LazadaGetLabel; ;
        }
        public BindingBase UploadImage(string imagePath, string accessToken)
        {
            var ret = new BindingBase();
            ret.status = 0;

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Upload Image Product",
                REQUEST_DATETIME = DateTime.UtcNow.AddHours(7),
                REQUEST_ATTRIBUTE_1 = imagePath,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, accessToken, currentLog);

            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/image/upload");

            //change by calvin 19 nov 2018
            //request.AddFileParameter("image", new FileItem(imagePath));
            var req = System.Net.WebRequest.Create(imagePath);
            //remark 15/10/2019, pindah ke dalam try catch
            //System.IO.Stream stream = req.GetResponse().GetResponseStream();
            //request.AddFileParameter("image", new FileItem("image", stream));
            //end remark 15/10/2019, pindah ke dalam try catch
            //end change by calvin 19 nov 2018
            try
            {
                System.IO.Stream stream = req.GetResponse().GetResponseStream();
                request.AddFileParameter("image", new FileItem("image", stream));

                LazopResponse response = client.Execute(request, accessToken);
                var bindImg = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(ImageLzd)) as ImageLzd;
                if (bindImg.code.Equals("0"))
                {
                    ret.status = 1;
                    ret.message = bindImg.data.image.url;
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, accessToken, currentLog);
                }
                else
                {
                    ret.message = bindImg.message;
                    currentLog.REQUEST_EXCEPTION = ret.message;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, accessToken, currentLog);
                    if (!string.IsNullOrWhiteSpace(ret.message))
                    {
                        if (ret.message.Contains("service timeout"))
                        {
                            ret = UploadImage(imagePath, accessToken);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ret.message = ex.ToString();
                currentLog.REQUEST_EXCEPTION = ex.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, accessToken, currentLog);
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public BindingBase GetOrders(string cust, string accessToken, string dbPathEra, string uname)
        {
            var ret = new BindingBase();
            //SetupContext(dbPathEra, uname);
            int page = 0;
            var more = true;

            while (more)
            {
                var count = GetOrdersWithPage(cust, accessToken, dbPathEra, uname, page, "pending");
                page++;
                if (count.recordCount < 100)
                {
                    more = false;
                }
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public BindingBase GetOrdersRTS(string cust, string accessToken, string dbPathEra, string uname)
        {
            var ret = new BindingBase();
            //SetupContext(dbPathEra, uname);
            int page = 0;
            var more = true;

            while (more)
            {
                var count = GetOrdersWithPage(cust, accessToken, dbPathEra, uname, page, "ready_to_ship");
                page++;
                if (count.recordCount < 100)
                {
                    more = false;
                }
            }
            return ret;
        }

        public BindingBase GetOrdersWithPage(string cust, string accessToken, string dbPathEra, string uname, int page, string statusLzd)
        {
            var ret = new BindingBase();
            ret.status = 0;
            ret.recordCount = 0;

            var MoDbContext = new MoDbContext("");
            var EDB = new DatabaseSQL(dbPathEra);
            string EraServerName = EDB.GetServerName("sConn");
            var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);
            var username = uname;

            var jmlhNewOrder = 0;//add by calvin 1 april 2019
            string connectionID = Guid.NewGuid().ToString();
            //change by Tri 4 Nov 2019, ambil pesanan baru 3 hari terakhir saja
            //var fromDt = DateTime.Now.AddDays(-14);
            var fromDt = DateTime.Now.AddDays(-3);
            //end change by Tri 4 Nov 2019, ambil pesanan baru 3 hari terakhir saja
            var toDt = DateTime.Now.AddDays(1);
            //SetupContext(dbPathEra, uname);
            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
            //    REQUEST_ACTION = "Get Order",
            //    REQUEST_DATETIME = DateTime.Now,
            //    REQUEST_ATTRIBUTE_1 = connectionID,
            //    REQUEST_STATUS = "Pending",
            //};
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, accessToken, currentLog);

            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/orders/get");
            request.SetHttpMethod("GET");
            request.AddApiParameter("created_before", toDt.ToString("yyyy-MM-ddTHH:mm:ss") + "+07:00");
            request.AddApiParameter("created_after", fromDt.ToString("yyyy-MM-ddTHH:mm:ss") + "+07:00");
            request.AddApiParameter("sort_direction", "DESC");
            //request.AddApiParameter("offset", "0");
            request.AddApiParameter("offset", (page * 100).ToString());
            request.AddApiParameter("limit", "100");
            //change by Tri 12 Maret, ambil pesanan sesuai status dari param
            //add by Tri 4 Nov 2019, ambil pesanan baru saja
            //request.AddApiParameter("status", "pending");
            request.AddApiParameter("status", statusLzd);
            //end add by Tri 4 Nov 2019, ambil pesanan baru saja
            //end change by Tri 12 Maret, ambil pesanan sesuai status dari param
            request.AddApiParameter("sort_by", "created_at");
            try
            {
                LazopResponse response = client.Execute(request, accessToken);
                var bindOrder = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(NewLzdOrders)) as NewLzdOrders;
                if (bindOrder != null)
                {
                    //ret = bindOrder;
                    if (bindOrder.code.Equals("0"))
                    {
                        //change 12 Maret 2019, handle record > 100
                        //string listOrderId = "[";
                        List<string> listOrderId = new List<string>();
                        //end change 12 Maret 2019, handle record > 100

                        if (bindOrder.data.orders.Count > 0)
                        {
                            ret.recordCount = bindOrder.data.orders.Count;
                            var fromDtSO = fromDt.AddDays(-1);
                            var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == cust && p.TGL.Value >= fromDtSO).Select(p => p.NO_REFERENSI).ToList();
                            bool adaInsert = false;
                            bool adaInsertPembeli = false;

                            string insertQ = "INSERT INTO TEMP_LAZADA_GETORDERS ([ORDERID],[CUST_FIRSTNAME],[CUST_LASTNAME],[ORDER_NUMBER],[PAYMENT_METHOD],[REMARKS]";
                            insertQ += ",[DELIVERY_INFO],[PRICE],[GIFT_OPTION],[GIFT_MESSAGE],[VOUCHER_CODE],[CREATED_AT],[UPDATED_AT],[BILLING_FIRSTNAME],[BILLING_LASTNAME]";
                            insertQ += ",[BILLING_PHONE],[BILLING_PHONE2],[BILLING_ADDRESS],[BILLING_ADDRESS2],[BILLING_ADDRESS3],[BILLING_ADDRESS4],[BILLING_ADDRESS5]";
                            insertQ += ",[BILLING_EMAIL],[BILLING_CITY],[BILLING_POSTCODE],[BILLING_COUNTRY],[SHIPPING_FIRSTNAME],[SHIPPING_LASTNAME],[SHIPPING_PHONE],[SHIPPING_PHONE2]";
                            insertQ += ",[SHIPPING_ADDRESS],[SHIPPING_ADDRESS2],[SHIPPING_ADDRESS3],[SHIPPING_ADDRESS4],[SHIPPING_ADDRESS5],[SHIPPING_EMAIL],[SHIPPING_CITY]";
                            insertQ += ",[SHIPPING_POSTCODE],[SHIPPING_COUNTRY],[NATIONAL_REGISTRASION_NUM],[ITEM_COUNT],[PROMISED_SHIPPING_TIME],[EXTRA_ATTRIBUTES],[STATUSES]";
                            insertQ += ",[VOUCHER],[SHIPPING_FEE],[TAXCODE],[BRANCH_NUMBER],[CUST],[USERNAME],[CONNECTION_ID]) VALUES ";

                            string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                            insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                            insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV, CONNECTION_ID) VALUES ";

                            //int i = 1;
                            var connIDARF01C = Guid.NewGuid().ToString();
                            //string username = sessionData?.Account != null ? sessionData.Account.Username : sessionData.User.Username;
                            string no_referensi_update_status = "";

                            var list_pesanan_update_pembeli = new List<string>();

                            foreach (Order order in bindOrder.data.orders.Where(p => OrderNoInDb.Contains(Convert.ToString(p.order_id))).ToList())
                            {
                                no_referensi_update_status += "'" + Convert.ToString(order.order_id) + "',";
                            }
                            if (no_referensi_update_status != "")
                            {
                                no_referensi_update_status = no_referensi_update_status.Substring(0, no_referensi_update_status.Length - 1);

                                var dsPesananUnpaid = EDB.GetDataSet("CString", "SIT01A", "SELECT NO_REFERENSI FROM SOT01A WHERE NO_REFERENSI IN (" + no_referensi_update_status + ") AND STATUS_TRANSAKSI = '0'");
                                if (dsPesananUnpaid.Tables[0].Rows.Count > 0)
                                {
                                    for (int i = 0; i < dsPesananUnpaid.Tables[0].Rows.Count; i++)
                                    {
                                        list_pesanan_update_pembeli.Add(Convert.ToString(dsPesananUnpaid.Tables[0].Rows[i]["NO_REFERENSI"]));
                                    }
                                    var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '01' WHERE NO_REFERENSI IN (" + no_referensi_update_status + ") AND STATUS_TRANSAKSI = '0'");
                                }
                            }
                            if (list_pesanan_update_pembeli.Count() > 0)
                            {
                                var noref_update_pembeli = bindOrder.data.orders.Where(p => list_pesanan_update_pembeli.Contains(Convert.ToString(p.order_id))).ToList();
                                foreach (var order in noref_update_pembeli)
                                {
                                    if (order.address_billing != null)
                                    {
                                        if (!string.IsNullOrEmpty(order.address_billing.phone))
                                        {
                                            var ordInDB = ErasoftDbContext.SOT01A.Where(p => p.CUST == cust && p.NO_REFERENSI == order.order_id).FirstOrDefault();
                                            if (ordInDB != null)//add by calvin 16 oktober 2019
                                            {
                                                if (string.IsNullOrEmpty(ordInDB.PEMESAN))
                                                {
                                                    var pembeliInDB = new ARF01C();
                                                    pembeliInDB = ErasoftDbContext.ARF01C.Where(m => m.TLP == order.address_billing.phone).FirstOrDefault();
                                                    if (pembeliInDB != null)
                                                    {
                                                        var rowAffected2 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET PEMESAN = '" + pembeliInDB.BUYER_CODE + "' WHERE NO_BUKTI = '" + ordInDB.NO_BUKTI + "'");
                                                    }
                                                    else
                                                    {
                                                        InsertPembeli(order, connIDARF01C, dbPathEra, username);
                                                        pembeliInDB = ErasoftDbContext.ARF01C.Where(m => m.TLP == order.address_billing.phone).FirstOrDefault();
                                                        if (pembeliInDB != null)
                                                        {
                                                            var rowAffected2 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET PEMESAN = '" + pembeliInDB.BUYER_CODE + "' WHERE NO_BUKTI = '" + ordInDB.NO_BUKTI + "'");
                                                        }
                                                        else
                                                        {
                                                            var adaPembeliGagalInsert = true;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            var notinso = bindOrder.data.orders.Where(p => !OrderNoInDb.Contains(Convert.ToString(p.order_id))).ToList();
                            foreach (Order order in notinso)
                            {
                                bool doInsert = true;
                                bool doInsertPembeli = true;
                                var pembeliInDB = new ARF01C();
                                //if (OrderNoInDb.Contains(Convert.ToString(order.order_id)) && (order.statuses[0].ToString() == "unpaid" || order.statuses[0].ToString() == "pending" || order.statuses[0].ToString() == "processing" || order.statuses[0].ToString() == "canceled"))
                                //{
                                //    doInsert = false;
                                //    doInsertPembeli = false;
                                //    if (order.statuses[0].ToString() == "pending")
                                //    {
                                //        //tidak perlu insert karena pesanan sudah ada di MO pada saat statusnya masih unpaid, update status transaksi jadi 01 dan bila perlu update juga ongkir dll
                                //        var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '01' WHERE NO_REFERENSI IN ('" + order.order_id + "') AND STATUS_TRANSAKSI = '0'");
                                //    }
                                //    if (order.statuses[0].ToString() == "canceled")
                                //    {
                                //        //remark by nurul 10/10/2019, dijalanin di GetOrdersCancelled
                                //        //var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '11' WHERE NO_REFERENSI IN ('" + order.order_id + "')");
                                //    }

                                //    if (!string.IsNullOrEmpty(order.address_billing.phone))
                                //    {
                                //        var ordInDB = ErasoftDbContext.SOT01A.Where(p => p.CUST == cust && p.NO_REFERENSI == order.order_id).FirstOrDefault();
                                //        if (ordInDB != null)//add by calvin 16 oktober 2019
                                //        {
                                //            if (string.IsNullOrEmpty(ordInDB.PEMESAN))
                                //            {
                                //                pembeliInDB = ErasoftDbContext.ARF01C.Where(m => m.TLP == order.address_billing.phone).FirstOrDefault();
                                //                if (pembeliInDB != null)
                                //                {
                                //                    var rowAffected2 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET PEMESAN = '" + pembeliInDB.BUYER_CODE + "' WHERE NO_BUKTI = '" + ordInDB.NO_BUKTI + "'");
                                //                }
                                //                else
                                //                {
                                //                    InsertPembeli(order, connIDARF01C);
                                //                    pembeliInDB = ErasoftDbContext.ARF01C.Where(m => m.TLP == order.address_billing.phone).FirstOrDefault();
                                //                    if (pembeliInDB != null)
                                //                    {
                                //                        var rowAffected2 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET PEMESAN = '" + pembeliInDB.BUYER_CODE + "' WHERE NO_BUKTI = '" + ordInDB.NO_BUKTI + "'");
                                //                    }
                                //                    else
                                //                    {
                                //                        var adaPembeliGagalInsert = true;
                                //                    }
                                //                }
                                //            }
                                //        }
                                //    }
                                //}
                                ////add 19 Feb 2019
                                //else if (order.statuses[0].ToString() == "delivered" || order.statuses[0].ToString() == "shipped")
                                //{
                                //    doInsertPembeli = false;
                                //    if (OrderNoInDb.Contains(Convert.ToString(order.order_id)))
                                //    {
                                //        //tidak ubah status menjadi selesai jika belum diisi faktur
                                //        var dsSIT01A = EDB.GetDataSet("CString", "SIT01A", "SELECT NO_REFERENSI, O.NO_BUKTI, O.STATUS_TRANSAKSI FROM SIT01A I INNER JOIN SOT01A O ON I.NO_SO = O.NO_BUKTI WHERE NO_REFERENSI = '" + order.order_id + "'");
                                //        if (dsSIT01A.Tables[0].Rows.Count == 0)
                                //        {
                                //            doInsert = false;
                                //        }
                                //    }
                                //    else
                                //    {
                                //        //tidak diinput jika order sudah selesai sebelum masuk MO
                                //        doInsert = false;
                                //    }
                                //    if (!string.IsNullOrEmpty(order.address_billing.phone))
                                //    {
                                //        var ordInDB = ErasoftDbContext.SOT01A.Where(p => p.CUST == cust && p.NO_REFERENSI == order.order_id).FirstOrDefault();
                                //        if (ordInDB != null)//add by calvin 16 oktober 2019
                                //        {
                                //            if (string.IsNullOrEmpty(ordInDB.PEMESAN))
                                //            {
                                //                pembeliInDB = ErasoftDbContext.ARF01C.Where(m => m.TLP == order.address_billing.phone).FirstOrDefault();
                                //                if (pembeliInDB != null)
                                //                {
                                //                    var rowAffected2 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET PEMESAN = '" + pembeliInDB.BUYER_CODE + "' WHERE NO_BUKTI = '" + ordInDB.NO_BUKTI + "'");
                                //                }
                                //                else
                                //                {
                                //                    InsertPembeli(order, connIDARF01C);
                                //                    pembeliInDB = ErasoftDbContext.ARF01C.Where(m => m.TLP == order.address_billing.phone).FirstOrDefault();
                                //                    if (pembeliInDB != null)
                                //                    {
                                //                        var rowAffected2 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET PEMESAN = '" + pembeliInDB.BUYER_CODE + "' WHERE NO_BUKTI = '" + ordInDB.NO_BUKTI + "'");
                                //                    }
                                //                    else
                                //                    {
                                //                        var adaPembeliGagalInsert = true;
                                //                    }
                                //                }
                                //            }
                                //        }
                                //    }
                                //}
                                ////end add 19 Feb 2019

                                if (doInsert)
                                {
                                    adaInsert = true;
                                    var giftOptionBit = (order.gift_option.Equals("")) ? 1 : 0;
                                    var price = order.price.Split('.');
                                    //add by nurul 4/12/2019, local time
                                    var tempCreated_at = order.created_at.AddHours(7);
                                    var tempUpdated_at = order.updated_at.AddHours(7);
                                    //end add by nurul 4/12/2019, local time
                                    var statusEra = "";
                                    #region convert status
                                    switch (order.statuses[0].ToString())
                                    {
                                        case "processing":
                                        case "pending":
                                            statusEra = "01";
                                            break;
                                        case "ready_to_ship":
                                            //change by Tri 12 mar 2020, status rts juga masuk ke MO untuk handle customer yang ubah status pesanan di lazada langsung
                                            //statusEra = "03";
                                            statusEra = "01";
                                            //end change by Tri 12 mar 2020, status rts juga masuk ke MO untuk handle customer yang ubah status pesanan di lazada langsung
                                            break;
                                        case "delivered":
                                        //statusEra = "03";
                                        //break;
                                        case "shipped":
                                            statusEra = "04";
                                            break;
                                        case "returned":
                                            statusEra = "06";
                                            break;
                                        case "return_waiting_for_approval":
                                            statusEra = "07";
                                            break;
                                        case "return_shipped_by_customer":
                                            statusEra = "08";
                                            break;
                                        case "return_rejected":
                                            statusEra = "09";
                                            break;
                                        case "failed":
                                            statusEra = "10";
                                            break;
                                        case "canceled":
                                            statusEra = "11";
                                            break;
                                        default:
                                            statusEra = "99";
                                            break;
                                    }
                                    //jika status pesanan sudah diubah di mo, dari 01 -> 02/03, status tidak dikembalikan ke 01
                                    if (statusEra == "01")
                                    {
                                        var currentStatus = EDB.GetFieldValue("", "SOT01A", "NO_REFERENSI = '" + order.order_id + "'", "STATUS_TRANSAKSI").ToString();
                                        if (!string.IsNullOrEmpty(currentStatus))
                                            if (currentStatus == "02" || currentStatus == "03")
                                                statusEra = currentStatus;
                                    }
                                    //end jika status pesanan sudah diubah di mo, dari 01 -> 02/03, status tidak dikembalikan ke 01
                                    #endregion convert status
                                    var nama1 = order.customer_first_name.Replace("'", "`");
                                    if (nama1.Length > 30)
                                        nama1 = nama1.Substring(0, 30);
                                    var nama2 = order.customer_last_name.Replace("'", "`");
                                    if (nama2.Length > 30)
                                        nama2 = nama2.Substring(0, 30);
                                    //insertQ += "('" + order.order_id + "','" + order.customer_first_name.Replace('\'', '`') + "','" + order.customer_last_name.Replace('\'', '`') + "','" + order.order_number + "','" + order.payment_method + "','" + order.remarks;
                                    insertQ += "('" + order.order_id + "','" + nama1 + "','" + nama2 + "','" + order.order_number + "','" + order.payment_method + "','" + order.remarks;
                                    //change by nurul 4/12/2019, local time
                                    //insertQ += "','" + order.delivery_info + "','" + price[0].Replace(",", "") + "'," + giftOptionBit + ",'" + order.gift_message + "','" + order.voucher_code + "','" + order.created_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + order.updated_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + order.address_billing.first_name.Replace('\'', '`') + "','" + order.address_billing.last_name.Replace('\'', '`');
                                    insertQ += "','" + order.delivery_info + "','" + price[0].Replace(",", "") + "'," + giftOptionBit + ",'" + order.gift_message + "','" + order.voucher_code + "','" + tempCreated_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + tempUpdated_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + order.address_billing.first_name.Replace('\'', '`') + "','" + order.address_billing.last_name.Replace('\'', '`');
                                    //end change by nurul 4/12/2019, local time 
                                    //insertQ += "','" + order.address_billing.phone + "','" + order.address_billing.phone2 + "','" + order.address_billing.address1.Replace('\'', '`') + "','" + order.address_billing.address2.Replace('\'', '`') + "','" + order.address_billing.address3.Replace('\'', '`') + "','" + order.address_billing.address4.Replace('\'', '`') + "','" + order.address_billing.address5.Replace('\'', '`');
                                    insertQ += "','" + (string.IsNullOrEmpty(order.address_billing.phone) ? "NO_PHONE" : order.address_billing.phone) + "','" + order.address_billing.phone2 + "','" + order.address_billing.address1.Replace('\'', '`') + "','" + order.address_billing.address2.Replace('\'', '`') + "','" + order.address_billing.address3.Replace('\'', '`') + "','" + order.address_billing.address4.Replace('\'', '`') + "','" + order.address_billing.address5.Replace('\'', '`');
                                    insertQ += "','" + order.address_billing.customer_email + "','" + order.address_billing.city.Replace('\'', '`') + "','" + order.address_billing.post_code.Replace('\'', '`') + "','" + order.address_billing.country.Replace('\'', '`') + "','" + order.address_shipping.first_name.Replace('\'', '`') + "','" + order.address_shipping.last_name.Replace('\'', '`') + "','" + order.address_shipping.phone + "','" + order.address_shipping.phone2;
                                    insertQ += "','" + order.address_shipping.address1.Replace('\'', '`') + "','" + order.address_shipping.address2.Replace('\'', '`') + "','" + order.address_shipping.address3.Replace('\'', '`') + "','" + order.address_shipping.address4.Replace('\'', '`') + "','" + order.address_shipping.address5.Replace('\'', '`') + "','" + order.address_shipping.customer_email + "','" + order.address_shipping.city.Replace('\'', '`');
                                    insertQ += "','" + order.address_shipping.post_code + "','" + order.address_shipping.country.Replace('\'', '`') + "','" + order.national_registration_number + "'," + order.items_count + ",'" + order.promised_shipping_times + "','" + order.extra_attributes + "','" + statusEra;
                                    insertQ += "'," + order.voucher + "," + order.shipping_fee + ",'" + order.tax_code + "','" + order.branch_number + "','" + cust + "','" + username + "','" + connectionID + "')";

                                    //remark by Tri 24/9/2019, pindah insert pembeli
                                    //var tblKabKot = EDB.GetDataSet("MOConnectionString", "KabupatenKota", "SELECT TOP 1 * FROM KabupatenKota WHERE NamaKabKot LIKE '%" + order.address_billing.address4 + "%'");
                                    //var tblProv = EDB.GetDataSet("MOConnectionString", "Provinsi", "SELECT TOP 1 * FROM Provinsi WHERE NamaProv LIKE '%" + order.address_billing.address5 + "%'");

                                    //var kabKot = "3174";//set default value jika tidak ada di db
                                    //var prov = "31";//set default value jika tidak ada di db

                                    //if (tblProv.Tables[0].Rows.Count > 0)
                                    //    prov = tblProv.Tables[0].Rows[0]["KodeProv"].ToString();
                                    //if (tblKabKot.Tables[0].Rows.Count > 0)
                                    //    kabKot = tblKabKot.Tables[0].Rows[0]["KodeKabKot"].ToString();

                                    //insertPembeli += "('" + order.address_billing.first_name.Replace('\'', '`') + "','" + order.address_billing.address1.Replace('\'', '`') + "','" + order.address_billing.phone + "','" + order.address_billing.customer_email + "',0,0,'0','01',";
                                    //insertPembeli += "1, 'IDR', '01', '" + order.address_billing.address1.Replace('\'', '`') + "', 0, 0, 0, 0, '1', 0, 0, ";
                                    ////change by calvin 12 desember 2018, ada data dari lazada yang order.address_billing.post_code nya diisi "Bekasi Timur"
                                    ////insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + order.address_billing.post_code + "', '" + order.address_billing.customer_email + "', '" + kabKot + "', '" + prov + "', '" + order.address_billing.address4 + "', '" + order.address_billing.address5 + "', '" + connIDARF01C + "')";
                                    //insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + order.address_billing.post_code.Substring(0, order.address_billing.post_code.Length > 5 ? 5 : order.address_billing.post_code.Length).Replace('\'', '`') + "', '" + order.address_billing.customer_email + "', '" + kabKot + "', '" + prov + "', '" + order.address_billing.address4.Replace('\'', '`') + "', '" + order.address_billing.address5.Replace('\'', '`') + "', '" + connIDARF01C + "')";
                                    ////end change by calvin 12 desember 2018
                                    //remark by Tri 24/9/2019, pindah insert pembeli

                                    //change 12 Maret 2019, handle record > 100
                                    //listOrderId += order.order_id;
                                    listOrderId.Add(order.order_id);
                                    //end change 12 Maret 2019, handle record > 100

                                    insertQ += " , ";
                                    //insertPembeli += " , ";//remark by Tri 24/9/2019, pindah insert pembeli

                                    //if (i < bindOrder.data.orders.Count)
                                    //{
                                    //remark 12 Maret 2019, handle record > 100
                                    //listOrderId += ",";
                                    //remark 12 Maret 2019, handle record > 100
                                    //}
                                    //else
                                    //{
                                    //    listOrderId += "]";
                                    //}
                                    //i = i + 1;
                                    if (!OrderNoInDb.Contains(Convert.ToString(order.order_id)))
                                        jmlhNewOrder++;
                                }
                                //if (doInsertPembeli && !string.IsNullOrEmpty(order.address_billing.phone))
                                if (doInsertPembeli && order.address_billing != null)
                                {
                                    if (!string.IsNullOrEmpty(order.address_billing.phone))
                                    {
                                        adaInsertPembeli = true;
                                        var tblKabKot = EDB.GetDataSet("MOConnectionString", "KabupatenKota", "SELECT TOP 1 * FROM KabupatenKota WHERE NamaKabKot LIKE '%" + order.address_billing.address4 + "%'");
                                        var tblProv = EDB.GetDataSet("MOConnectionString", "Provinsi", "SELECT TOP 1 * FROM Provinsi WHERE NamaProv LIKE '%" + order.address_billing.address5 + "%'");

                                        var kabKot = "3174";//set default value jika tidak ada di db
                                        var prov = "31";//set default value jika tidak ada di db

                                        if (tblProv.Tables[0].Rows.Count > 0)
                                            prov = tblProv.Tables[0].Rows[0]["KodeProv"].ToString();
                                        if (tblKabKot.Tables[0].Rows.Count > 0)
                                            kabKot = tblKabKot.Tables[0].Rows[0]["KodeKabKot"].ToString();

                                        var nama = order.address_billing.first_name.Replace('\'', '`').Replace("'", "`");
                                        if (nama.Length > 30)
                                            nama = nama.Substring(0, 30);

                                        //insertPembeli += "('" + order.address_billing.first_name.Replace('\'', '`') + "','" + order.address_billing.address1.Replace('\'', '`') + "','" + order.address_billing.phone + "','" + order.address_billing.customer_email + "',0,0,'0','01',";
                                        insertPembeli += "('" + nama + "','" + order.address_billing.address1.Replace('\'', '`') + "','" + order.address_billing.phone + "','" + order.address_billing.customer_email + "',0,0,'0','01',";
                                        insertPembeli += "1, 'IDR', '01', '" + order.address_billing.address1.Replace('\'', '`') + "', 0, 0, 0, 0, '1', 0, 0, ";
                                        //change by calvin 12 desember 2018, ada data dari lazada yang order.address_billing.post_code nya diisi "Bekasi Timur"
                                        //insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + order.address_billing.post_code + "', '" + order.address_billing.customer_email + "', '" + kabKot + "', '" + prov + "', '" + order.address_billing.address4 + "', '" + order.address_billing.address5 + "', '" + connIDARF01C + "')";
                                        insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + order.address_billing.post_code.Substring(0, order.address_billing.post_code.Length > 5 ? 5 : order.address_billing.post_code.Length).Replace('\'', '`') + "', '" + order.address_billing.customer_email + "', '" + kabKot + "', '" + prov + "', '" + order.address_billing.address4.Replace('\'', '`') + "', '" + order.address_billing.address5.Replace('\'', '`') + "', '" + connIDARF01C + "')";
                                        //end change by calvin 12 desember 2018
                                        insertPembeli += " , ";
                                    }
                                }
                            }
                            //change by Tri 24/9/2019
                            //if (adaInsert)
                            if (adaInsert || adaInsertPembeli)
                            //end change by Tri 24/9/2019
                            {
                                SqlCommand CommandSQL = new SqlCommand();

                                insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 2);
                                if (adaInsertPembeli)
                                {
                                    var aa = EDB.ExecuteSQL(username, CommandType.Text, insertPembeli);
                                    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIDARF01C;
                                    EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);
                                }

                                insertQ = insertQ.Substring(0, insertQ.Length - 2);
                                if (adaInsert)
                                {
                                    var a = EDB.ExecuteSQL(username, CommandType.Text, insertQ);

                                    //add by Tri 21 Feb 2020, gabung sp header dan detail move order
                                    var getDetail = getMultiOrderItems2(listOrderId, accessToken, connectionID, dbPathEra, uname, cust);
                                    if(getDetail.status == 1)
                                    {
                                        EDB.ExecuteSQL("MOConnectionString", CommandType.Text, getDetail.message);
                                    }
                                    else
                                    {
                                        throw new Exception(getDetail.message);
                                    }
                                    //end add by Tri 21 Feb 2020, gabung sp header dan detail move order

                                    CommandSQL = new SqlCommand();
                                    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;
                                    CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = fromDt.ToString("yyyy-MM-dd HH:mm:ss");
                                    CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = toDt.ToString("yyyy-MM-dd HH:mm:ss");
                                    CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 1;
                                    CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                                    CommandSQL.Parameters.Add("@elevenia", SqlDbType.Int).Value = 0;
                                    CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                                    CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                                    CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                                    CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 0;
                                    CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = cust;

                                    EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);
                                }


                                ret.status = 1;
                                //ret.message = a.ToString();

                                //SqlCommand CommandSQL = new SqlCommand();

                                //CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                //CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIDARF01C;
                                //EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);

                                //CommandSQL = new SqlCommand();
                                //CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                //CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;
                                //CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = fromDt.ToString("yyyy-MM-dd HH:mm:ss");
                                //CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = toDt.ToString("yyyy-MM-dd HH:mm:ss");
                                //CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 1;
                                //CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                                //CommandSQL.Parameters.Add("@elevenia", SqlDbType.Int).Value = 0;
                                //CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                                //CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                                //CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                                //CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 0;
                                //CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = cust;

                                //EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);

                                //change 12 Maret 2019, handle record > 100
                                //listOrderId = listOrderId.Substring(0, listOrderId.Length - 1) + "]";
                                //getMultiOrderItems(listOrderId, accessToken, connectionID);

                                //remark by Tri 21 Feb 2020, gabung sp header dan detail move order
                                //getMultiOrderItems2(listOrderId, accessToken, connectionID, dbPathEra, uname, cust);
                                //end remark by Tri 21 Feb 2020, gabung sp header dan detail move order

                                //change 12 Maret 2019, handle record > 100
                                //jmlhNewOrder++;
                            }
                            //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, accessToken, currentLog);

                            if (jmlhNewOrder > 0)
                            {
                                var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                contextNotif.Clients.Group(dbPathEra).moNewOrder("Terdapat " + Convert.ToString(jmlhNewOrder) + " Pesanan baru dari Lazada.");

                                new StokControllerJob().updateStockMarketPlace(connectionID, dbPathEra, uname);
                            }
                        }
                        else
                        {
                            ret.message = "no order";
                        }
                    }
                    else
                    {
                        //currentLog.REQUEST_EXCEPTION = bindOrder.message;
                        //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, accessToken, currentLog);
                        ret.message = "lazada api return error";
                        if (string.IsNullOrEmpty(bindOrder.message))
                            ret.message += "\n" + bindOrder.message.ToString();

                    }
                }
                else
                {
                    ret.message = "failed to call lazada api";
                }
            }
            catch (Exception ex)
            {
                ret.message = ex.ToString();
                //currentLog.REQUEST_EXCEPTION = ex.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, accessToken, currentLog);
            }
            return ret;
        }
        private void InsertPembeli(Order order, string connIDARF01C, string dbPathEra, string username)
        {
            var MoDbContext = new MoDbContext("");
            var EDB = new DatabaseSQL(dbPathEra);
            string EraServerName = EDB.GetServerName("sConn");
            var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);
            var tblKabKot = EDB.GetDataSet("MOConnectionString", "KabupatenKota", "SELECT TOP 1 * FROM KabupatenKota WHERE NamaKabKot LIKE '%" + order.address_billing.address4 + "%'");
            var tblProv = EDB.GetDataSet("MOConnectionString", "Provinsi", "SELECT TOP 1 * FROM Provinsi WHERE NamaProv LIKE '%" + order.address_billing.address5 + "%'");

            var kabKot = "3174";//set default value jika tidak ada di db
            var prov = "31";//set default value jika tidak ada di db

            if (tblProv.Tables[0].Rows.Count > 0)
                prov = tblProv.Tables[0].Rows[0]["KodeProv"].ToString();
            if (tblKabKot.Tables[0].Rows.Count > 0)
                kabKot = tblKabKot.Tables[0].Rows[0]["KodeKabKot"].ToString();

            string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
            insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
            insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV, CONNECTION_ID) VALUES ";

            var nama = order.address_billing.first_name.Replace('\'', '`');
            if (nama.Length > 30) { nama = nama.Substring(1, 30); }

            insertPembeli += "('" + nama + "','" + order.address_billing.address1.Replace('\'', '`') + "','" + order.address_billing.phone + "','" + order.address_billing.customer_email + "',0,0,'0','01',";
            insertPembeli += "1, 'IDR', '01', '" + order.address_billing.address1.Replace('\'', '`') + "', 0, 0, 0, 0, '1', 0, 0, ";
            //change by calvin 12 desember 2018, ada data dari lazada yang order.address_billing.post_code nya diisi "Bekasi Timur"
            //insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + order.address_billing.post_code + "', '" + order.address_billing.customer_email + "', '" + kabKot + "', '" + prov + "', '" + order.address_billing.address4 + "', '" + order.address_billing.address5 + "', '" + connIDARF01C + "')";
            insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + order.address_billing.post_code.Substring(0, order.address_billing.post_code.Length > 5 ? 5 : order.address_billing.post_code.Length).Replace('\'', '`') + "', '" + order.address_billing.customer_email + "', '" + kabKot + "', '" + prov + "', '" + order.address_billing.address4.Replace('\'', '`') + "', '" + order.address_billing.address5.Replace('\'', '`') + "', '" + connIDARF01C + "')";
            //end change by calvin 12 desember 2018
            var aa = EDB.ExecuteSQL(username, CommandType.Text, insertPembeli);
            SqlCommand CommandSQL = new SqlCommand();
            CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
            CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIDARF01C;
            EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public BindingBase GetOrdersUnpaid(string cust, string accessToken, string dbPathEra, string uname)
        {
            var ret = new BindingBase();
            //SetupContext(dbPathEra, uname);
            int page = 0;
            var more = true;

            while (more)
            {
                var count = GetOrdersUnpaidWithPage(cust, accessToken, dbPathEra, uname, page);
                page++;
                if (count.recordCount < 100)
                {
                    more = false;
                }
            }
            return ret;
        }
        public BindingBase GetOrdersUnpaidWithPage(string cust, string accessToken, string dbPathEra, string uname, int page)
        {
            var ret = new BindingBase();
            ret.status = 0;
            ret.recordCount = 0;

            var MoDbContext = new MoDbContext("");
            var EDB = new DatabaseSQL(dbPathEra);
            string EraServerName = EDB.GetServerName("sConn");
            var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);
            var username = uname;

            var jmlhNewOrder = 0;//add by calvin 1 april 2019
            string connectionID = Guid.NewGuid().ToString();
            //change by Tri 4 Nov 2019, ambil pesanan baru 3 hari terakhir saja
            //var fromDt = DateTime.Now.AddDays(-14);
            var fromDt = DateTime.Now.AddDays(-1);
            //end change by Tri 4 Nov 2019, ambil pesanan baru 3 hari terakhir saja
            var toDt = DateTime.Now.AddDays(1);
            //SetupContext(dbPathEra, uname);
            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
            //    REQUEST_ACTION = "Get Order",
            //    REQUEST_DATETIME = DateTime.Now,
            //    REQUEST_ATTRIBUTE_1 = connectionID,
            //    REQUEST_STATUS = "Pending",
            //};
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, accessToken, currentLog);

            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/orders/get");
            request.SetHttpMethod("GET");
            request.AddApiParameter("created_before", toDt.ToString("yyyy-MM-ddTHH:mm:ss") + "+07:00");
            request.AddApiParameter("created_after", fromDt.ToString("yyyy-MM-ddTHH:mm:ss") + "+07:00");
            request.AddApiParameter("sort_direction", "DESC");
            //request.AddApiParameter("offset", "0");
            request.AddApiParameter("offset", (page * 100).ToString());
            request.AddApiParameter("limit", "100");
            request.AddApiParameter("sort_by", "created_at");
            request.AddApiParameter("status", "unpaid");
            try
            {
                LazopResponse response = client.Execute(request, accessToken);
                var bindOrder = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(NewLzdOrders)) as NewLzdOrders;
                if (bindOrder != null)
                {
                    //ret = bindOrder;
                    if (bindOrder.code.Equals("0"))
                    {
                        //change 12 Maret 2019, handle record > 100
                        //string listOrderId = "[";
                        List<string> listOrderId = new List<string>();
                        //end change 12 Maret 2019, handle record > 100

                        if (bindOrder.data.orders.Count > 0)
                        {
                            ret.recordCount = bindOrder.data.orders.Count;
                            var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == cust && p.TGL.Value >= fromDt).Select(p => p.NO_REFERENSI).ToList();
                            bool adaInsert = false;

                            string insertQ = "INSERT INTO TEMP_LAZADA_GETORDERS ([ORDERID],[CUST_FIRSTNAME],[CUST_LASTNAME],[ORDER_NUMBER],[PAYMENT_METHOD],[REMARKS]";
                            insertQ += ",[DELIVERY_INFO],[PRICE],[GIFT_OPTION],[GIFT_MESSAGE],[VOUCHER_CODE],[CREATED_AT],[UPDATED_AT],[BILLING_FIRSTNAME],[BILLING_LASTNAME]";
                            insertQ += ",[BILLING_PHONE],[BILLING_PHONE2],[BILLING_ADDRESS],[BILLING_ADDRESS2],[BILLING_ADDRESS3],[BILLING_ADDRESS4],[BILLING_ADDRESS5]";
                            insertQ += ",[BILLING_EMAIL],[BILLING_CITY],[BILLING_POSTCODE],[BILLING_COUNTRY],[SHIPPING_FIRSTNAME],[SHIPPING_LASTNAME],[SHIPPING_PHONE],[SHIPPING_PHONE2]";
                            insertQ += ",[SHIPPING_ADDRESS],[SHIPPING_ADDRESS2],[SHIPPING_ADDRESS3],[SHIPPING_ADDRESS4],[SHIPPING_ADDRESS5],[SHIPPING_EMAIL],[SHIPPING_CITY]";
                            insertQ += ",[SHIPPING_POSTCODE],[SHIPPING_COUNTRY],[NATIONAL_REGISTRASION_NUM],[ITEM_COUNT],[PROMISED_SHIPPING_TIME],[EXTRA_ATTRIBUTES],[STATUSES]";
                            insertQ += ",[VOUCHER],[SHIPPING_FEE],[TAXCODE],[BRANCH_NUMBER],[CUST],[USERNAME],[CONNECTION_ID]) VALUES ";

                            //string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                            //insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                            //insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV, CONNECTION_ID) VALUES ";

                            //int i = 1;
                            var connIDARF01C = Guid.NewGuid().ToString();
                            //string username = sessionData?.Account != null ? sessionData.Account.Username : sessionData.User.Username;

                            foreach (Order order in bindOrder.data.orders)
                            {
                                bool doInsert = true;
                                if (OrderNoInDb.Contains(Convert.ToString(order.order_id)))
                                {
                                    doInsert = false;
                                }
                                ////add 19 Feb 2019
                                //else if (order.statuses[0].ToString() == "delivered" || order.statuses[0].ToString() == "shipped")
                                //{
                                //    if (OrderNoInDb.Contains(Convert.ToString(order.order_id)))
                                //    {
                                //        //tidak ubah status menjadi selesai jika belum diisi faktur
                                //        var dsSIT01A = EDB.GetDataSet("CString", "SIT01A", "SELECT NO_REFERENSI, O.NO_BUKTI, O.STATUS_TRANSAKSI FROM SIT01A I INNER JOIN SOT01A O ON I.NO_SO = O.NO_BUKTI WHERE NO_REFERENSI = '" + order.order_id + "'");
                                //        if (dsSIT01A.Tables[0].Rows.Count == 0)
                                //        {
                                //            doInsert = false;
                                //        }
                                //    }
                                //    else
                                //    {
                                //        //tidak diinput jika order sudah selesai sebelum masuk MO
                                //        doInsert = false;
                                //    }
                                //}
                                ////end add 19 Feb 2019

                                if (doInsert)
                                {
                                    adaInsert = true;
                                    var giftOptionBit = (order.gift_option.Equals("")) ? 1 : 0;
                                    var price = order.price.Split('.');
                                    //add by nurul 4/12/2019, local time
                                    var tempCreated_at = order.created_at.AddHours(7);
                                    var tempUpdated_at = order.updated_at.AddHours(7);
                                    //end add by nurul 4/12/2019, local time
                                    var statusEra = "";
                                    #region convert status
                                    switch (order.statuses[0].ToString())
                                    {
                                        case "unpaid":
                                            statusEra = "0";
                                            break;
                                        case "processing":
                                        case "pending":
                                            statusEra = "01";
                                            break;
                                        case "ready_to_ship":
                                            statusEra = "03";
                                            break;
                                        case "delivered":
                                        //statusEra = "03";
                                        //break;
                                        case "shipped":
                                            statusEra = "04";
                                            break;
                                        case "returned":
                                            statusEra = "06";
                                            break;
                                        case "return_waiting_for_approval":
                                            statusEra = "07";
                                            break;
                                        case "return_shipped_by_customer":
                                            statusEra = "08";
                                            break;
                                        case "return_rejected":
                                            statusEra = "09";
                                            break;
                                        case "failed":
                                            statusEra = "10";
                                            break;
                                        case "canceled":
                                            statusEra = "11";
                                            break;
                                        default:
                                            statusEra = "99";
                                            break;
                                    }
                                    #endregion convert status
                                    var nama1 = order.customer_first_name.Replace("'", "`");
                                    if (nama1.Length > 30)
                                        nama1 = nama1.Substring(0, 30);
                                    var nama2 = order.customer_last_name.Replace("'", "`");
                                    if (nama2.Length > 30)
                                        nama2 = nama2.Substring(0, 30);
                                    //insertQ += "('" + order.order_id + "','" + order.customer_first_name.Replace('\'', '`') + "','" + order.customer_last_name.Replace('\'', '`') + "','" + order.order_number + "','" + order.payment_method + "','" + order.remarks;
                                    insertQ += "('" + order.order_id + "','" + nama1 + "','" + nama2 + "','" + order.order_number + "','" + order.payment_method + "','" + order.remarks;
                                    //change by nurul 4/12/2019, local time
                                    //insertQ += "','" + order.delivery_info + "','" + price[0].Replace(",", "") + "'," + giftOptionBit + ",'" + order.gift_message + "','" + order.voucher_code + "','" + order.created_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + order.updated_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + order.address_billing.first_name.Replace('\'', '`') + "','" + order.address_billing.last_name.Replace('\'', '`');
                                    insertQ += "','" + order.delivery_info + "','" + price[0].Replace(",", "") + "'," + giftOptionBit + ",'" + order.gift_message + "','" + order.voucher_code + "','" + tempCreated_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + tempUpdated_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + order.address_billing.first_name.Replace('\'', '`') + "','" + order.address_billing.last_name.Replace('\'', '`');
                                    //end change by nurul 4/12/2019, local time 
                                    //insertQ += "','" + order.address_billing.phone + "','" + order.address_billing.phone2 + "','" + order.address_billing.address1.Replace('\'', '`') + "','" + order.address_billing.address2.Replace('\'', '`') + "','" + order.address_billing.address3.Replace('\'', '`') + "','" + order.address_billing.address4.Replace('\'', '`') + "','" + order.address_billing.address5.Replace('\'', '`');
                                    insertQ += "','" + (string.IsNullOrEmpty(order.address_billing.phone) ? "NO_PHONE" : order.address_billing.phone) + "','" + order.address_billing.phone2 + "','" + order.address_billing.address1.Replace('\'', '`') + "','" + order.address_billing.address2.Replace('\'', '`') + "','" + order.address_billing.address3.Replace('\'', '`') + "','" + order.address_billing.address4.Replace('\'', '`') + "','" + order.address_billing.address5.Replace('\'', '`');
                                    insertQ += "','" + order.address_billing.customer_email + "','" + order.address_billing.city.Replace('\'', '`') + "','" + order.address_billing.post_code.Replace('\'', '`') + "','" + order.address_billing.country.Replace('\'', '`') + "','" + order.address_shipping.first_name.Replace('\'', '`') + "','" + order.address_shipping.last_name.Replace('\'', '`') + "','" + order.address_shipping.phone + "','" + order.address_shipping.phone2;
                                    insertQ += "','" + order.address_shipping.address1.Replace('\'', '`') + "','" + order.address_shipping.address2.Replace('\'', '`') + "','" + order.address_shipping.address3.Replace('\'', '`') + "','" + order.address_shipping.address4.Replace('\'', '`') + "','" + order.address_shipping.address5.Replace('\'', '`') + "','" + order.address_shipping.customer_email + "','" + order.address_shipping.city.Replace('\'', '`');
                                    insertQ += "','" + order.address_shipping.post_code + "','" + order.address_shipping.country.Replace('\'', '`') + "','" + order.national_registration_number + "'," + order.items_count + ",'" + order.promised_shipping_times + "','" + order.extra_attributes + "','" + statusEra;
                                    insertQ += "'," + order.voucher + "," + order.shipping_fee + ",'" + order.tax_code + "','" + order.branch_number + "','" + cust + "','" + username + "','" + connectionID + "')";

                                    var tblKabKot = EDB.GetDataSet("MOConnectionString", "KabupatenKota", "SELECT TOP 1 * FROM KabupatenKota WHERE NamaKabKot LIKE '%" + order.address_billing.address4 + "%'");
                                    var tblProv = EDB.GetDataSet("MOConnectionString", "Provinsi", "SELECT TOP 1 * FROM Provinsi WHERE NamaProv LIKE '%" + order.address_billing.address5 + "%'");

                                    //var kabKot = "3174";//set default value jika tidak ada di db
                                    //var prov = "31";//set default value jika tidak ada di db

                                    //if (tblProv.Tables[0].Rows.Count > 0)
                                    //    prov = tblProv.Tables[0].Rows[0]["KodeProv"].ToString();
                                    //if (tblKabKot.Tables[0].Rows.Count > 0)
                                    //    kabKot = tblKabKot.Tables[0].Rows[0]["KodeKabKot"].ToString();

                                    //insertPembeli += "('" + order.address_billing.first_name.Replace('\'', '`') + "','" + order.address_billing.address1.Replace('\'', '`') + "','" + order.address_billing.phone + "','" + order.address_billing.customer_email + "',0,0,'0','01',";
                                    //insertPembeli += "1, 'IDR', '01', '" + order.address_billing.address1.Replace('\'', '`') + "', 0, 0, 0, 0, '1', 0, 0, ";
                                    ////change by calvin 12 desember 2018, ada data dari lazada yang order.address_billing.post_code nya diisi "Bekasi Timur"
                                    ////insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + order.address_billing.post_code + "', '" + order.address_billing.customer_email + "', '" + kabKot + "', '" + prov + "', '" + order.address_billing.address4 + "', '" + order.address_billing.address5 + "', '" + connIDARF01C + "')";
                                    //insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + order.address_billing.post_code.Substring(0, order.address_billing.post_code.Length > 5 ? 5 : order.address_billing.post_code.Length).Replace('\'', '`') + "', '" + order.address_billing.customer_email + "', '" + kabKot + "', '" + prov + "', '" + order.address_billing.address4.Replace('\'', '`') + "', '" + order.address_billing.address5.Replace('\'', '`') + "', '" + connIDARF01C + "')";
                                    ////end change by calvin 12 desember 2018

                                    //change 12 Maret 2019, handle record > 100
                                    //listOrderId += order.order_id;
                                    listOrderId.Add(order.order_id);
                                    //end change 12 Maret 2019, handle record > 100

                                    insertQ += " , ";
                                    //insertPembeli += " , ";

                                    //if (i < bindOrder.data.orders.Count)
                                    //{
                                    //remark 12 Maret 2019, handle record > 100
                                    //listOrderId += ",";
                                    //remark 12 Maret 2019, handle record > 100
                                    //}
                                    //else
                                    //{
                                    //    listOrderId += "]";
                                    //}
                                    //i = i + 1;
                                    if (!OrderNoInDb.Contains(Convert.ToString(order.order_id)))
                                        jmlhNewOrder++;
                                }
                            }
                            if (adaInsert)
                            {
                                insertQ = insertQ.Substring(0, insertQ.Length - 2);
                                var a = EDB.ExecuteSQL(username, CommandType.Text, insertQ);

                                //insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 2);
                                //a = EDB.ExecuteSQL(username, CommandType.Text, insertPembeli);

                                ret.status = 1;
                                //ret.message = a.ToString();

                                //add by Tri 21 Feb 2020, gabung sp header dan detail move order
                                var getDetail = getMultiOrderItems2(listOrderId, accessToken, connectionID, dbPathEra, uname, cust);
                                if (getDetail.status == 1)
                                {
                                    EDB.ExecuteSQL("MOConnectionString", CommandType.Text, getDetail.message);
                                }
                                else
                                {
                                    throw new Exception(getDetail.message);
                                }
                                //end add by Tri 21 Feb 2020, gabung sp header dan detail move order

                                SqlCommand CommandSQL = new SqlCommand();

                                //CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                //CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIDARF01C;
                                //EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);

                                CommandSQL = new SqlCommand();
                                CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;
                                CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = fromDt.ToString("yyyy-MM-dd HH:mm:ss");
                                CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = toDt.ToString("yyyy-MM-dd HH:mm:ss");
                                CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 1;
                                CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                                CommandSQL.Parameters.Add("@elevenia", SqlDbType.Int).Value = 0;
                                CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                                CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                                CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                                CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 0;
                                CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = cust;
                                //remark to test
                                EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);
                                //end remark to test

                                //change 12 Maret 2019, handle record > 100
                                //listOrderId = listOrderId.Substring(0, listOrderId.Length - 1) + "]";
                                //getMultiOrderItems(listOrderId, accessToken, connectionID);

                                //remark 24 feb 2020, gabung sp header dan detail move order
                                //getMultiOrderItems2(listOrderId, accessToken, connectionID, dbPathEra, uname, cust);
                                //end remark 24 feb 2020, gabung sp header dan detail move order

                                //change 12 Maret 2019, handle record > 100
                                //jmlhNewOrder++;
                            }
                            //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, accessToken, currentLog);

                            if (jmlhNewOrder > 0)
                            {
                                var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                contextNotif.Clients.Group(dbPathEra).moNewOrder("Terdapat " + Convert.ToString(jmlhNewOrder) + " Pesanan baru dari Lazada.");

                                new StokControllerJob().updateStockMarketPlace(connectionID, dbPathEra, uname);
                            }
                        }
                        else
                        {
                            ret.message = "no order";
                        }
                    }
                    else
                    {
                        //currentLog.REQUEST_EXCEPTION = bindOrder.message;
                        //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, accessToken, currentLog);
                        ret.message = "lazada api return error";
                        if (string.IsNullOrEmpty(bindOrder.message))
                            ret.message += "\n" + bindOrder.message.ToString();

                    }
                }
                else
                {
                    ret.message = "failed to call lazada api";
                }
            }
            catch (Exception ex)
            {
                ret.message = ex.ToString();
                //currentLog.REQUEST_EXCEPTION = ex.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, accessToken, currentLog);
            }
            return ret;
        }


        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public BindingBase GetOrdersCancelled(string cust, string accessToken, string dbPathEra, string uname)
        {
            var ret = new BindingBase();
            ret.status = 0;
            //var jmlhNewOrder = 0;//add by calvin 1 april 2019
            //string connectionID = Guid.NewGuid().ToString();
            var fromDt = DateTime.Now.AddDays(-14);
            var toDt = DateTime.Now.AddDays(1);

            var MoDbContext = new MoDbContext("");
            var EDB = new DatabaseSQL(dbPathEra);
            string EraServerName = EDB.GetServerName("sConn");
            var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);
            //var username = uname;

            var orderUnpaidList = (from a in ErasoftDbContext.SOT01A
                                       //change by nurul 10/10/2019, cari semua status kecuali cancel (11)
                                       //where a.USER_NAME == "Auto Lazada" && a.STATUS_TRANSAKSI == "0"
                                       //where a.USER_NAME == "Auto Lazada" && a.STATUS_TRANSAKSI != "11"
                                   where a.USER_NAME == "Auto Lazada" && a.STATUS_TRANSAKSI != "11" && a.CUST == cust && a.TGL < toDt && a.TGL > fromDt
                                   //end change by nurul 10/10/2019, cari semua status kecuali cancel (11)
                                   //select new { a.NO_REFERENSI }).ToList();
                                   select a.NO_REFERENSI).ToList();

            GetOrdersCancelledWithPage(cust, accessToken, dbPathEra, uname, 0, orderUnpaidList);

            //add by Tri 10 Des 2019, cek pesanan belum dibayar
            UpdateOrderUnpaidToCancel(cust, accessToken, dbPathEra, uname);
            //end add by Tri 10 Des 2019, cek pesanan belum dibayar

            return ret;
        }
        public BindingBase GetOrdersCancelledWithPage(string cust, string accessToken, string dbPathEra, string uname, int page, List<string> orderUnpaidList)
        {
            //order unpaid yang cancelled
            var ret = new BindingBase();
            ret.status = 0;
            var jmlhNewOrder = 0;//add by calvin 1 april 2019
            string connectionID = Guid.NewGuid().ToString();
            var fromDt = DateTime.Now.AddDays(-14);
            var toDt = DateTime.Now.AddDays(1);
            //SetupContext(dbPathEra, uname);
            var MoDbContext = new MoDbContext("");
            var EDB = new DatabaseSQL(dbPathEra);
            string EraServerName = EDB.GetServerName("sConn");
            var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);
            var username = uname;
            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
            //    REQUEST_ACTION = "Get Order",
            //    REQUEST_DATETIME = DateTime.Now,
            //    REQUEST_ATTRIBUTE_1 = connectionID,
            //    REQUEST_STATUS = "Pending",
            //};
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, accessToken, currentLog);

            //pesanan baru yang unpaid
            var brgCancelled = new List<TEMP_ALL_MP_ORDER_ITEM>();
            //var orderUnpaidList = (from a in ErasoftDbContext.SOT01A
            //                           //change by nurul 10/10/2019, cari semua status kecuali cancel (11)
            //                           //where a.USER_NAME == "Auto Lazada" && a.STATUS_TRANSAKSI == "0"
            //                       where a.USER_NAME == "Auto Lazada" && a.STATUS_TRANSAKSI != "11" && a.TGL < toDt && a.TGL > fromDt
            //                       //end change by nurul 10/10/2019, cari semua status kecuali cancel (11)
            //                       //select new { a.NO_REFERENSI }).ToList();
            //                       select  a.NO_REFERENSI ).ToList();
            var connIDStok = Guid.NewGuid().ToString();
            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/orders/get");
            request.SetHttpMethod("GET");
            request.AddApiParameter("created_before", toDt.ToString("yyyy-MM-ddTHH:mm:ss") + "+07:00");
            request.AddApiParameter("created_after", fromDt.ToString("yyyy-MM-ddTHH:mm:ss") + "+07:00");
            request.AddApiParameter("sort_direction", "DESC");
            request.AddApiParameter("offset", (page * 100).ToString());
            request.AddApiParameter("limit", "100");
            request.AddApiParameter("status", "canceled");
            LazopResponse response = client.Execute(request, accessToken);
            var bindOrder = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(NewLzdOrders)) as NewLzdOrders;
            if (bindOrder != null)
            {
                if (bindOrder.code.Equals("0") && bindOrder.data.orders != null)
                {
                    foreach (var order in bindOrder.data.orders)
                    {
                        if (orderUnpaidList.Contains(order.order_id))
                        {
                            var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2',STATUS_TRANSAKSI = '11' WHERE NO_REFERENSI IN ('" + order.order_id + "') AND STATUS_TRANSAKSI <> '11'");

                            if (rowAffected > 0)
                            {
                                var rowAffectedSI = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS='2' WHERE NO_REF IN ('" + order.order_id + "') AND STATUS <> '2'");

                                var orderDetail = (from a in ErasoftDbContext.SOT01A
                                                   join b in ErasoftDbContext.SOT01B on a.NO_BUKTI equals b.NO_BUKTI
                                                   where a.NO_REFERENSI == order.order_id
                                                   select new { b.BRG }).ToList();
                                foreach (var item in orderDetail)
                                {
                                    if (brgCancelled.Where(p => p.BRG == item.BRG).Count() <= 0)
                                    {
                                        brgCancelled.Add(new TEMP_ALL_MP_ORDER_ITEM() { BRG = item.BRG, CONN_ID = connIDStok });
                                    }
                                }
                            }
                        }
                    }
                    if (bindOrder.data.orders.Count >= 100)
                    {
                        GetOrdersCancelledWithPage(cust, accessToken, dbPathEra, uname, page + 1, orderUnpaidList);
                    }
                }
            }
            //foreach (var order in orderUnpaidList)
            //{
            //    ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            //    LazopRequest request = new LazopRequest();
            //    request.SetApiName("/order/get");
            //    request.SetHttpMethod("GET");
            //    request.AddApiParameter("order_id", order.NO_REFERENSI);
            //    try
            //    {
            //        LazopResponse response = client.Execute(request, accessToken);
            //        var bindOrder = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(SingleOrderReturn)) as SingleOrderReturn;
            //        if (bindOrder != null)
            //        {
            //            //ret = bindOrder;
            //            if (bindOrder.code.Equals("0"))
            //            {
            //                //change 12 Maret 2019, handle record > 100
            //                //string listOrderId = "[";
            //                List<string> listOrderId = new List<string>();
            //                //end change 12 Maret 2019, handle record > 100
            //                //TEMP_ALL_MP_ORDER_ITEM
            //                if (!string.IsNullOrWhiteSpace(bindOrder.data.order_id))
            //                {
            //                    //change by Tri 5 Juli 2019, cek status pesanan menjadi cancelled di lazada baru update status di mo
            //                    //var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == cust && p.TGL.Value >= fromDt).Select(p => p.NO_REFERENSI).ToList();

            //                    //if (OrderNoInDb.Contains(Convert.ToString(bindOrder.data.order_id)))
            //                    if (bindOrder.data.statuses[0].ToLower() == "canceled")
            //                    //end change by Tri 5 Juli 2019, cek status pesanan menjadi cancelled di lazada baru update status di mo
            //                    {
            //                        var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2',STATUS_TRANSAKSI = '11' WHERE NO_REFERENSI IN ('" + bindOrder.data.order_id + "') AND STATUS_TRANSAKSI <> '11'");

            //                        if (rowAffected > 0)
            //                        {
            //                            var rowAffectedSI = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS='2' WHERE NO_REF IN ('" + bindOrder.data.order_id + "') AND STATUS <> '2'");

            //                            var orderDetail = (from a in ErasoftDbContext.SOT01A
            //                                               join b in ErasoftDbContext.SOT01B on a.NO_BUKTI equals b.NO_BUKTI
            //                                               where a.NO_REFERENSI == bindOrder.data.order_id
            //                                               select new { b.BRG }).ToList();
            //                            foreach (var item in orderDetail)
            //                            {
            //                                if (brgCancelled.Where(p => p.BRG == item.BRG).Count() <= 0)
            //                                {
            //                                    brgCancelled.Add(new TEMP_ALL_MP_ORDER_ITEM() { BRG = item.BRG, CONN_ID = connIDStok });
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //                else
            //                {
            //                    ret.message = "no order";
            //                }
            //            }
            //            else
            //            {
            //                //currentLog.REQUEST_EXCEPTION = bindOrder.message;
            //                //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, accessToken, currentLog);
            //                ret.message = "lazada api return error";
            //            }
            //        }
            //        else
            //        {
            //            ret.message = "failed to call lazada api";
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        ret.message = ex.ToString();
            //        //currentLog.REQUEST_EXCEPTION = ex.Message;
            //        //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, accessToken, currentLog);
            //    }
            //}

            var itemCount = brgCancelled.Count();
            if (itemCount > 0)
            {
                string sSQL = "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG,CONN_ID)" + System.Environment.NewLine;
                int indexCount = 0;
                foreach (var item in brgCancelled)
                {
                    indexCount = indexCount + 1;
                    sSQL += "SELECT '' AS BRG, '' AS CONN_ID " + System.Environment.NewLine;
                    if (indexCount < itemCount)
                    {
                        sSQL += "UNION ALL " + System.Environment.NewLine;
                    }
                }
                var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, sSQL);
                new StokControllerJob().updateStockMarketPlace(connIDStok, dbPathEra, uname);
            }

            return ret;
        }

        public void UpdateOrderUnpaidToCancel(string cust, string accessToken, string dbPathEra, string uname)
        {
            var MoDbContext = new MoDbContext("");
            var EDB = new DatabaseSQL(dbPathEra);
            string EraServerName = EDB.GetServerName("sConn");
            var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);
            var username = uname;
            //var dmin2 = DateTime.Today.AddDays(-2);
            var dmin2 = DateTime.Now.AddDays(-1);
            var brgCancelled = new List<TEMP_ALL_MP_ORDER_ITEM>();
            var connIDStok = Guid.NewGuid().ToString();
            var listOrderUnpaid = ErasoftDbContext.SOT01A.Where(m => m.CUST == cust && m.TGL < dmin2 && m.STATUS_TRANSAKSI == "0").Select(m => new { nobuk = m.NO_BUKTI, noref = m.NO_REFERENSI }).ToList();
            if (listOrderUnpaid.Count > 0)
            {
                //var listOrder = new List<string>();
                foreach (var order in listOrderUnpaid)
                {
                    var ret = GetOrderStatus(order.noref, accessToken);
                    if (!string.IsNullOrEmpty(ret))
                    {
                        //listOrder.Add(order.nobuk);
                        var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2',STATUS_TRANSAKSI = '11' WHERE NO_REFERENSI IN ('" + order.noref + "') AND STATUS_TRANSAKSI <> '11' AND CUST = '" + cust + "'");

                        if (rowAffected > 0)
                        {
                            var rowAffectedSI = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS='2' WHERE NO_REF IN ('" + order.noref + "') AND STATUS <> '2' AND CUST = '" + cust + "'");

                            var orderDetail = (from a in ErasoftDbContext.SOT01A
                                               join b in ErasoftDbContext.SOT01B on a.NO_BUKTI equals b.NO_BUKTI
                                               where a.NO_REFERENSI == order.noref && a.CUST == cust
                                               select new { b.BRG }).ToList();
                            foreach (var item in orderDetail)
                            {
                                if (brgCancelled.Where(p => p.BRG == item.BRG).Count() <= 0)
                                {
                                    brgCancelled.Add(new TEMP_ALL_MP_ORDER_ITEM() { BRG = item.BRG, CONN_ID = connIDStok });
                                }
                            }
                        }
                    }
                }

                var itemCount = brgCancelled.Count();
                if (itemCount > 0)
                {
                    string sSQL = "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG,CONN_ID)" + System.Environment.NewLine;
                    int indexCount = 0;
                    foreach (var item in brgCancelled)
                    {
                        indexCount = indexCount + 1;
                        sSQL += "SELECT '' AS BRG, '' AS CONN_ID " + System.Environment.NewLine;
                        if (indexCount < itemCount)
                        {
                            sSQL += "UNION ALL " + System.Environment.NewLine;
                        }
                    }
                    var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, sSQL);
                    new StokControllerJob().updateStockMarketPlace(connIDStok, dbPathEra, uname);
                }
            }
        }

        public string GetOrderStatus(string orderid, string accessToken)
        {
            string ret = "";
            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/order/get");
            request.SetHttpMethod("GET");
            request.AddApiParameter("order_id", orderid);
            LazopResponse response = client.Execute(request, accessToken);
            var bindOrder = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(SingleOrderReturn)) as SingleOrderReturn;
            if (bindOrder != null)
            {
                //ret = bindOrder;
                if (bindOrder.code.Equals("0"))
                {
                    if (bindOrder.data.statuses[0].ToString() == "canceled")
                    {
                        ret = bindOrder.data.statuses[0].ToString();
                    }
                }
            }
            return ret;
        }

        public BindingBase getMultiOrderItems2(List<string> orderIds, string accessToken, string connectionID, string dbPathEra, string uname, string cust)
        {
            var ret = new BindingBase();
            ret.status = 0;

            var MoDbContext = new MoDbContext("");
            var EDB = new DatabaseSQL(dbPathEra);
            string EraServerName = EDB.GetServerName("sConn");
            var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);
            var username = uname;

            List<string> listID = new List<string>();
            string addOrderID = "[";
            if (orderIds.Count > 100)
            {
                for (int i = 0; i < orderIds.Count; i++)
                {
                    addOrderID += orderIds[i];
                    if ((i + 1) % 100 == 0)
                    {
                        addOrderID += "]";
                        listID.Add(addOrderID);
                        addOrderID = "[";
                    }
                    else
                    {
                        addOrderID += ",";
                    }
                }
            }
            else
            {
                foreach (var ids in orderIds)
                {
                    addOrderID += ids + ",";
                }
                //remark 10 Feb 2020
                //addOrderID = addOrderID.Substring(0, addOrderID.Length - 1) + "]";
                //listID.Add(addOrderID);
                //end remark 10 Feb 2020
            }
            //add by Tri 10 Feb 2020, untuk data lebih dari 100 belum di add
            addOrderID = addOrderID.Substring(0, addOrderID.Length - 1) + "]";
            listID.Add(addOrderID);
            //end add by Tri 10 Feb 2020, untuk data lebih dari 100 belum di add

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
            //    REQUEST_ACTION = "Get Order Items",
            //    REQUEST_DATETIME = DateTime.Now,
            //    REQUEST_ATTRIBUTE_1 = "",
            //    REQUEST_ATTRIBUTE_2 = connectionID,
            //    REQUEST_STATUS = "Pending",
            //};
            //foreach (var a in listID)
            //{
            //    currentLog.REQUEST_ATTRIBUTE_1 += a;
            //}
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, accessToken, currentLog);

            string insertQ = "INSERT INTO TEMP_LAZADA_GETORDERITEMS ([ORDER_ITEM_ID],[SHOP_ID],[ORDER_ID],[NAME],[SKU],[SHOP_SKU],[SHIPPING_TYPE]";
            insertQ += ",[ITEM_PRICE],[PAID_PRICE],[CURRENCY],[TAX_AMOUNT],[SHIPPING_AMOUNT],[SHIPPING_SERVICE_COST],[VOUCHER_AMOUNT]";
            insertQ += ",[STATUS],[SHIPMENT_PROVIDER],[IS_DIGITAL],[TRACKING_CODE],[REASON],[REASON_DETAIL],[PURCHASE_ORDERID]";
            insertQ += ",[PURCHASE_ORDER_NUM],[PACKAGE_ID],[EXTRA_ATTRIBUTES],[SHIPPING_PROVIDER_TYPE],[CREATED_AT],[UPDATED_AT]";
            insertQ += ",[RETURN_STATUS],[PRODUCT_MAIN_IMAGE],[VARIATION],[PRODUCT_DETAIL_URL],[INVOICE_NUM],[USERNAME],[CONNECTION_ID]) VALUES ";

            string sSQL_Value = "";
            foreach (var listOrderIds in listID)
            {
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
                            foreach (Datum order in bindOrderItems.data)
                            {
                                if (order.order_items.Count() > 0)
                                {
                                    //var connectionID = Guid.NewGuid().ToString();

                                    foreach (Order_Items items in order.order_items)
                                    {
                                        //var isDigital = (items.IsDigital == 1) ? 1 : 0;
                                        var statusEra = "";
                                        switch (items.status.ToString())
                                        {
                                            case "processing":
                                            case "pending":
                                                statusEra = "01";
                                                break;
                                            case "ready_to_ship":
                                                statusEra = "03";
                                                break;
                                            case "delivered":
                                            //statusEra = "03";
                                            //break;
                                            case "shipped":
                                                statusEra = "04";
                                                break;
                                            case "returned":
                                                statusEra = "06";
                                                break;
                                            case "return_waiting_for_approval":
                                                statusEra = "07";
                                                break;
                                            case "return_shipped_by_customer":
                                                statusEra = "08";
                                                break;
                                            case "return_rejected":
                                                statusEra = "09";
                                                break;
                                            case "failed":
                                                statusEra = "10";
                                                break;
                                            case "canceled":
                                                statusEra = "11";
                                                break;
                                            default:
                                                statusEra = "99";
                                                break;
                                        }
                                        //jika status pesanan sudah diubah di mo, dari 01 -> 02/03, status tidak dikembalikan ke 01
                                        if (statusEra == "01")
                                        {
                                            var currentStatus = EDB.GetFieldValue("", "SOT01B", "ORDER_IEM_ID = '" + items.order_item_id + "'", "STATUS_BRG").ToString();
                                            if (!string.IsNullOrEmpty(currentStatus))
                                                if (currentStatus == "02" || currentStatus == "03")
                                                    statusEra = currentStatus;
                                        }
                                        //end jika status pesanan sudah diubah di mo, dari 01 -> 02/03, status tidak dikembalikan ke 01

                                        sSQL_Value += "('" + items.order_item_id + "','" + items.shop_id + "','" + items.order_id + "','" + items.name.Replace('\'', '`') + "','" + items.sku.Replace('\'', '`') + "','" + items.shop_sku.Replace('\'', '`') + "','" + items.shipping_type;
                                        sSQL_Value += "'," + items.item_price + "," + items.paid_price + ",'" + items.currency + "'," + items.tax_amount + "," + items.shipping_amount + "," + items.shipping_service_cost + "," + items.voucher_amount;
                                        sSQL_Value += ",'" + statusEra + "','" + items.shipment_provider.Replace('\'', '`') + "'," + items.is_digital + ",'" + items.tracking_code + "','" + items.reason.Replace('\'', '`') + "','" + items.reason_detail.Replace('\'', '`') + "','" + items.purchase_order_id;
                                        sSQL_Value += "','" + items.purchase_order_number + "','" + items.package_id + "','" + items.extra_attributes.Replace('\'', '`') + "','" + items.shipping_provider_type + "','" + items.created_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + items.updated_at.ToString("yyyy-MM-dd HH:mm:ss");
                                        sSQL_Value += "','" + items.return_status + "','" + items.product_main_image + "','" + items.variation.Replace('\'', '`') + "','" + items.product_detail_url + "','" + items.invoice_number + "','" + username + "','" + connectionID + "')";

                                        //if (i < bindOrderItems.data.Count)
                                        sSQL_Value += ",";
                                        //i = i + 1;
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
            }

            if (!string.IsNullOrEmpty(sSQL_Value))
            {
                insertQ = insertQ + sSQL_Value.Substring(0, sSQL_Value.Length - 1);
                //change by Tri 21 Feb 2020, gabung sp header dan detail move order
                ret.status = 1;
                ret.message = insertQ;
                //var a = EDB.ExecuteSQL(username, CommandType.Text, insertQ);

                //SqlCommand CommandSQL = new SqlCommand();
                //CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                //CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;
                //CommandSQL.Parameters.Add("@customer", SqlDbType.VarChar).Value = cust;

                //EDB.ExecuteSQL("MOConnectionString", "MoveOrderItemsFromTempTable", CommandSQL);
                //end change by Tri 21 Feb 2020, gabung sp header dan detail move order
                //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, accessToken, currentLog);
            }


            return ret;
        }


        public BindingBase getMultiOrderItems2WithQueue(string dbPathEra, string uname, List<string> orderIds, string accessToken, string connectionID, string cust)
        {
            var ret = new BindingBase();
            ret.status = 0;
            SetupContext(dbPathEra, uname);

            List<string> listID = new List<string>();
            string addOrderID = "[";
            if (orderIds.Count > 100)
            {
                for (int i = 0; i < orderIds.Count; i++)
                {
                    addOrderID += orderIds[i];
                    if ((i + 1) % 100 == 0)
                    {
                        addOrderID += "]";
                        listID.Add(addOrderID);
                        addOrderID = "[";
                    }
                    else
                    {
                        addOrderID += ",";
                    }
                }
            }
            else
            {
                foreach (var ids in orderIds)
                {
                    addOrderID += ids + ",";
                }
                addOrderID = addOrderID.Substring(0, addOrderID.Length - 1) + "]";
                listID.Add(addOrderID);
            }
            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
            //    REQUEST_ACTION = "Get Order Items",
            //    REQUEST_DATETIME = DateTime.Now,
            //    REQUEST_ATTRIBUTE_1 = "",
            //    REQUEST_ATTRIBUTE_2 = connectionID,
            //    REQUEST_STATUS = "Pending",
            //};
            //foreach (var a in listID)
            //{
            //    currentLog.REQUEST_ATTRIBUTE_1 += a;
            //}
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, accessToken, currentLog);

            string insertQ = "INSERT INTO TEMP_LAZADA_GETORDERITEMS ([ORDER_ITEM_ID],[SHOP_ID],[ORDER_ID],[NAME],[SKU],[SHOP_SKU],[SHIPPING_TYPE]";
            insertQ += ",[ITEM_PRICE],[PAID_PRICE],[CURRENCY],[TAX_AMOUNT],[SHIPPING_AMOUNT],[SHIPPING_SERVICE_COST],[VOUCHER_AMOUNT]";
            insertQ += ",[STATUS],[SHIPMENT_PROVIDER],[IS_DIGITAL],[TRACKING_CODE],[REASON],[REASON_DETAIL],[PURCHASE_ORDERID]";
            insertQ += ",[PURCHASE_ORDER_NUM],[PACKAGE_ID],[EXTRA_ATTRIBUTES],[SHIPPING_PROVIDER_TYPE],[CREATED_AT],[UPDATED_AT]";
            insertQ += ",[RETURN_STATUS],[PRODUCT_MAIN_IMAGE],[VARIATION],[PRODUCT_DETAIL_URL],[INVOICE_NUM],[USERNAME],[CONNECTION_ID]) VALUES ";

            string sSQL_Value = "";
            foreach (var listOrderIds in listID)
            {
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
                            foreach (Datum order in bindOrderItems.data)
                            {
                                if (order.order_items.Count() > 0)
                                {
                                    //var connectionID = Guid.NewGuid().ToString();

                                    foreach (Order_Items items in order.order_items)
                                    {
                                        //var isDigital = (items.IsDigital == 1) ? 1 : 0;
                                        var statusEra = "";
                                        switch (items.status.ToString())
                                        {
                                            case "processing":
                                            case "pending":
                                                statusEra = "01";
                                                break;
                                            case "ready_to_ship":
                                                statusEra = "03";
                                                break;
                                            case "delivered":
                                            //statusEra = "03";
                                            //break;
                                            case "shipped":
                                                statusEra = "04";
                                                break;
                                            case "returned":
                                                statusEra = "06";
                                                break;
                                            case "return_waiting_for_approval":
                                                statusEra = "07";
                                                break;
                                            case "return_shipped_by_customer":
                                                statusEra = "08";
                                                break;
                                            case "return_rejected":
                                                statusEra = "09";
                                                break;
                                            case "failed":
                                                statusEra = "10";
                                                break;
                                            case "canceled":
                                                statusEra = "11";
                                                break;
                                            default:
                                                statusEra = "99";
                                                break;
                                        }
                                        //jika status pesanan sudah diubah di mo, dari 01 -> 02/03, status tidak dikembalikan ke 01
                                        if (statusEra == "01")
                                        {
                                            var currentStatus = EDB.GetFieldValue("", "SOT01B", "ORDER_IEM_ID = '" + items.order_item_id + "'", "STATUS_BRG").ToString();
                                            if (!string.IsNullOrEmpty(currentStatus))
                                                if (currentStatus == "02" || currentStatus == "03")
                                                    statusEra = currentStatus;
                                        }
                                        //end jika status pesanan sudah diubah di mo, dari 01 -> 02/03, status tidak dikembalikan ke 01

                                        sSQL_Value += "('" + items.order_item_id + "','" + items.shop_id + "','" + items.order_id + "','" + items.name.Replace('\'', '`') + "','" + items.sku.Replace('\'', '`') + "','" + items.shop_sku.Replace('\'', '`') + "','" + items.shipping_type;
                                        sSQL_Value += "'," + items.item_price + "," + items.paid_price + ",'" + items.currency + "'," + items.tax_amount + "," + items.shipping_amount + "," + items.shipping_service_cost + "," + items.voucher_amount;
                                        sSQL_Value += ",'" + statusEra + "','" + items.shipment_provider.Replace('\'', '`') + "'," + items.is_digital + ",'" + items.tracking_code + "','" + items.reason.Replace('\'', '`') + "','" + items.reason_detail.Replace('\'', '`') + "','" + items.purchase_order_id;
                                        sSQL_Value += "','" + items.purchase_order_number + "','" + items.package_id + "','" + items.extra_attributes.Replace('\'', '`') + "','" + items.shipping_provider_type + "','" + items.created_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + items.updated_at.ToString("yyyy-MM-dd HH:mm:ss");
                                        sSQL_Value += "','" + items.return_status + "','" + items.product_main_image + "','" + items.variation.Replace('\'', '`') + "','" + items.product_detail_url + "','" + items.invoice_number + "','" + username + "','" + connectionID + "')";

                                        //if (i < bindOrderItems.data.Count)
                                        sSQL_Value += ",";
                                        //i = i + 1;
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
            }

            if (!string.IsNullOrEmpty(sSQL_Value))
            {
                insertQ = insertQ + sSQL_Value.Substring(0, sSQL_Value.Length - 1);
                var a = EDB.ExecuteSQL(username, CommandType.Text, insertQ);

                SqlCommand CommandSQL = new SqlCommand();
                CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;
                CommandSQL.Parameters.Add("@customer", SqlDbType.VarChar).Value = cust;

                EDB.ExecuteSQL("MOConnectionString", "MoveOrderItemsFromTempTable", CommandSQL);
                //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, accessToken, currentLog);
            }


            return ret;
        }

        //add by Tri 4 Nov 2019, update pesanan yg sudah ada di MO
        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public BindingBase GetOrdersToUpdateMO(string cust, string accessToken, string dbPathEra, string uname)
        {
            var ret = new BindingBase();
            //SetupContext(dbPathEra, uname);
            int page = 0;
            var more = true;

            while (more)
            {
                var count = GetOrdersToUpdateMOWithPage(cust, accessToken, dbPathEra, uname, page);
                page++;
                if (count.recordCount < 100)
                {
                    more = false;
                }
            }
            return ret;
        }

        //public BindingBase GetOrdersToUpdateMOWithPage(string cust, string accessToken, string dbPathEra, string uname, int page)
        //{
        //    var ret = new BindingBase();
        //    ret.status = 0;
        //    ret.recordCount = 0;
        //    var jmlhNewOrder = 0;//add by calvin 1 april 2019
        //    string connectionID = Guid.NewGuid().ToString();
        //    var fromDt = DateTime.Now.AddDays(-14);
        //    var toDt = DateTime.Now.AddDays(1);
        //    //SetupContext(dbPathEra, uname);
        //    //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
        //    //{
        //    //    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
        //    //    REQUEST_ACTION = "Get Order",
        //    //    REQUEST_DATETIME = DateTime.Now,
        //    //    REQUEST_ATTRIBUTE_1 = connectionID,
        //    //    REQUEST_STATUS = "Pending",
        //    //};
        //    //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, accessToken, currentLog);

        //    ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
        //    LazopRequest request = new LazopRequest();
        //    request.SetApiName("/orders/get");
        //    request.SetHttpMethod("GET");
        //    request.AddApiParameter("created_before", toDt.ToString("yyyy-MM-ddTHH:mm:ss") + "+07:00");
        //    request.AddApiParameter("created_after", fromDt.ToString("yyyy-MM-ddTHH:mm:ss") + "+07:00");
        //    request.AddApiParameter("sort_direction", "DESC");
        //    //request.AddApiParameter("offset", "0");
        //    request.AddApiParameter("offset", (page * 100).ToString());
        //    request.AddApiParameter("limit", "100");
        //    request.AddApiParameter("sort_by", "updated_at");
        //    try
        //    {
        //        LazopResponse response = client.Execute(request, accessToken);
        //        var bindOrder = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(NewLzdOrders)) as NewLzdOrders;
        //        if (bindOrder != null)
        //        {
        //            //ret = bindOrder;
        //            if (bindOrder.code.Equals("0"))
        //            {
        //                //change 12 Maret 2019, handle record > 100
        //                //string listOrderId = "[";
        //                List<string> listOrderId = new List<string>();
        //                //end change 12 Maret 2019, handle record > 100

        //                if (bindOrder.data.orders.Count > 0)
        //                {
        //                    ret.recordCount = bindOrder.data.orders.Count;
        //                    var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == cust && p.TGL.Value >= fromDt).Select(p => p.NO_REFERENSI).ToList();
        //                    bool adaInsert = false;
        //                    bool adaInsertPembeli = false;

        //                    string insertQ = "INSERT INTO TEMP_LAZADA_GETORDERS ([ORDERID],[CUST_FIRSTNAME],[CUST_LASTNAME],[ORDER_NUMBER],[PAYMENT_METHOD],[REMARKS]";
        //                    insertQ += ",[DELIVERY_INFO],[PRICE],[GIFT_OPTION],[GIFT_MESSAGE],[VOUCHER_CODE],[CREATED_AT],[UPDATED_AT],[BILLING_FIRSTNAME],[BILLING_LASTNAME]";
        //                    insertQ += ",[BILLING_PHONE],[BILLING_PHONE2],[BILLING_ADDRESS],[BILLING_ADDRESS2],[BILLING_ADDRESS3],[BILLING_ADDRESS4],[BILLING_ADDRESS5]";
        //                    insertQ += ",[BILLING_EMAIL],[BILLING_CITY],[BILLING_POSTCODE],[BILLING_COUNTRY],[SHIPPING_FIRSTNAME],[SHIPPING_LASTNAME],[SHIPPING_PHONE],[SHIPPING_PHONE2]";
        //                    insertQ += ",[SHIPPING_ADDRESS],[SHIPPING_ADDRESS2],[SHIPPING_ADDRESS3],[SHIPPING_ADDRESS4],[SHIPPING_ADDRESS5],[SHIPPING_EMAIL],[SHIPPING_CITY]";
        //                    insertQ += ",[SHIPPING_POSTCODE],[SHIPPING_COUNTRY],[NATIONAL_REGISTRASION_NUM],[ITEM_COUNT],[PROMISED_SHIPPING_TIME],[EXTRA_ATTRIBUTES],[STATUSES]";
        //                    insertQ += ",[VOUCHER],[SHIPPING_FEE],[TAXCODE],[BRANCH_NUMBER],[CUST],[USERNAME],[CONNECTION_ID]) VALUES ";

        //                    string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
        //                    insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
        //                    insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV, CONNECTION_ID) VALUES ";

        //                    //int i = 1;
        //                    var connIDARF01C = Guid.NewGuid().ToString();
        //                    //string username = sessionData?.Account != null ? sessionData.Account.Username : sessionData.User.Username;

        //                    foreach (Order order in bindOrder.data.orders)
        //                    {
        //                        bool doInsert = false;
        //                        bool doInsertPembeli = false;
        //                        if (order.statuses[0].ToString() == "ready_to_ship")
        //                        {
        //                            doInsertPembeli = true;
        //                            doInsert = true;
        //                        }
        //                        var pembeliInDB = new ARF01C();
        //                        if (OrderNoInDb.Contains(Convert.ToString(order.order_id)) && (order.statuses[0].ToString() == "unpaid" || order.statuses[0].ToString() == "pending" || order.statuses[0].ToString() == "processing" || order.statuses[0].ToString() == "canceled"))
        //                        {
        //                            doInsert = false;
        //                            doInsertPembeli = false;
        //                            if (order.statuses[0].ToString() == "pending")
        //                            {
        //                                //tidak perlu insert karena pesanan sudah ada di MO pada saat statusnya masih unpaid, update status transaksi jadi 01 dan bila perlu update juga ongkir dll
        //                                var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '01' WHERE NO_REFERENSI IN ('" + order.order_id + "') AND STATUS_TRANSAKSI = '0'");
        //                            }
        //                            if (order.statuses[0].ToString() == "canceled")
        //                            {
        //                                //remark by nurul 10/10/2019, dijalanin di GetOrdersCancelled
        //                                //var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '11' WHERE NO_REFERENSI IN ('" + order.order_id + "')");
        //                            }

        //                            if (!string.IsNullOrEmpty(order.address_billing.phone))
        //                            {
        //                                var ordInDB = ErasoftDbContext.SOT01A.Where(p => p.CUST == cust && p.NO_REFERENSI == order.order_id).FirstOrDefault();
        //                                if (ordInDB != null)//add by calvin 16 oktober 2019
        //                                {
        //                                    if (string.IsNullOrEmpty(ordInDB.PEMESAN))
        //                                    {
        //                                        pembeliInDB = ErasoftDbContext.ARF01C.Where(m => m.TLP == order.address_billing.phone).FirstOrDefault();
        //                                        if (pembeliInDB != null)
        //                                        {
        //                                            var rowAffected2 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET PEMESAN = '" + pembeliInDB.BUYER_CODE + "' WHERE NO_BUKTI = '" + ordInDB.NO_BUKTI + "'");
        //                                        }
        //                                        else
        //                                        {
        //                                            InsertPembeli(order, connIDARF01C);
        //                                            pembeliInDB = ErasoftDbContext.ARF01C.Where(m => m.TLP == order.address_billing.phone).FirstOrDefault();
        //                                            if (pembeliInDB != null)
        //                                            {
        //                                                var rowAffected2 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET PEMESAN = '" + pembeliInDB.BUYER_CODE + "' WHERE NO_BUKTI = '" + ordInDB.NO_BUKTI + "'");
        //                                            }
        //                                            else
        //                                            {
        //                                                var adaPembeliGagalInsert = true;
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        //add 19 Feb 2019
        //                        else if (order.statuses[0].ToString() == "delivered" || order.statuses[0].ToString() == "shipped")
        //                        {
        //                            doInsertPembeli = false;
        //                            if (OrderNoInDb.Contains(Convert.ToString(order.order_id)))
        //                            {
        //                                //tidak ubah status menjadi selesai jika belum diisi faktur
        //                                var dsSIT01A = EDB.GetDataSet("CString", "SIT01A", "SELECT NO_REFERENSI, O.NO_BUKTI, O.STATUS_TRANSAKSI FROM SIT01A I INNER JOIN SOT01A O ON I.NO_SO = O.NO_BUKTI WHERE NO_REFERENSI = '" + order.order_id + "'");
        //                                if (dsSIT01A.Tables[0].Rows.Count == 0)
        //                                {
        //                                    doInsert = false;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                //tidak diinput jika order sudah selesai sebelum masuk MO
        //                                doInsert = false;
        //                            }
        //                            if (!string.IsNullOrEmpty(order.address_billing.phone))
        //                            {
        //                                var ordInDB = ErasoftDbContext.SOT01A.Where(p => p.CUST == cust && p.NO_REFERENSI == order.order_id).FirstOrDefault();
        //                                if (ordInDB != null)//add by calvin 16 oktober 2019
        //                                {
        //                                    if (string.IsNullOrEmpty(ordInDB.PEMESAN))
        //                                    {
        //                                        pembeliInDB = ErasoftDbContext.ARF01C.Where(m => m.TLP == order.address_billing.phone).FirstOrDefault();
        //                                        if (pembeliInDB != null)
        //                                        {
        //                                            var rowAffected2 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET PEMESAN = '" + pembeliInDB.BUYER_CODE + "' WHERE NO_BUKTI = '" + ordInDB.NO_BUKTI + "'");
        //                                        }
        //                                        else
        //                                        {
        //                                            InsertPembeli(order, connIDARF01C);
        //                                            pembeliInDB = ErasoftDbContext.ARF01C.Where(m => m.TLP == order.address_billing.phone).FirstOrDefault();
        //                                            var rowAffected2 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET PEMESAN = '" + pembeliInDB.BUYER_CODE + "' WHERE NO_BUKTI = '" + ordInDB.NO_BUKTI + "'");
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        //end add 19 Feb 2019

        //                        if (doInsert)
        //                        {
        //                            adaInsert = true;
        //                            var giftOptionBit = (order.gift_option.Equals("")) ? 1 : 0;
        //                            var price = order.price.Split('.');
        //                            var statusEra = "";
        //                            #region convert status
        //                            switch (order.statuses[0].ToString())
        //                            {
        //                                case "processing":
        //                                case "pending":
        //                                    statusEra = "01";
        //                                    break;
        //                                case "ready_to_ship":
        //                                    statusEra = "03";
        //                                    break;
        //                                case "delivered":
        //                                //statusEra = "03";
        //                                //break;
        //                                case "shipped":
        //                                    statusEra = "04";
        //                                    break;
        //                                case "returned":
        //                                    statusEra = "06";
        //                                    break;
        //                                case "return_waiting_for_approval":
        //                                    statusEra = "07";
        //                                    break;
        //                                case "return_shipped_by_customer":
        //                                    statusEra = "08";
        //                                    break;
        //                                case "return_rejected":
        //                                    statusEra = "09";
        //                                    break;
        //                                case "failed":
        //                                    statusEra = "10";
        //                                    break;
        //                                case "canceled":
        //                                    statusEra = "11";
        //                                    break;
        //                                default:
        //                                    statusEra = "99";
        //                                    break;
        //                            }
        //                            //jika status pesanan sudah diubah di mo, dari 01 -> 02/03, status tidak dikembalikan ke 01
        //                            if (statusEra == "01")
        //                            {
        //                                var currentStatus = EDB.GetFieldValue("", "SOT01A", "NO_REFERENSI = '" + order.order_id + "'", "STATUS_TRANSAKSI").ToString();
        //                                if (!string.IsNullOrEmpty(currentStatus))
        //                                    if (currentStatus == "02" || currentStatus == "03")
        //                                        statusEra = currentStatus;
        //                            }
        //                            //end jika status pesanan sudah diubah di mo, dari 01 -> 02/03, status tidak dikembalikan ke 01
        //                            #endregion convert status
        //                            insertQ += "('" + order.order_id + "','" + order.customer_first_name.Replace('\'', '`') + "','" + order.customer_last_name.Replace('\'', '`') + "','" + order.order_number + "','" + order.payment_method + "','" + order.remarks;
        //                            insertQ += "','" + order.delivery_info + "','" + price[0].Replace(",", "") + "'," + giftOptionBit + ",'" + order.gift_message + "','" + order.voucher_code + "','" + order.created_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + order.updated_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + order.address_billing.first_name.Replace('\'', '`') + "','" + order.address_billing.last_name.Replace('\'', '`');
        //                            //insertQ += "','" + order.address_billing.phone + "','" + order.address_billing.phone2 + "','" + order.address_billing.address1.Replace('\'', '`') + "','" + order.address_billing.address2.Replace('\'', '`') + "','" + order.address_billing.address3.Replace('\'', '`') + "','" + order.address_billing.address4.Replace('\'', '`') + "','" + order.address_billing.address5.Replace('\'', '`');
        //                            insertQ += "','" + (string.IsNullOrEmpty(order.address_billing.phone) ? "NO_PHONE" : order.address_billing.phone) + "','" + order.address_billing.phone2 + "','" + order.address_billing.address1.Replace('\'', '`') + "','" + order.address_billing.address2.Replace('\'', '`') + "','" + order.address_billing.address3.Replace('\'', '`') + "','" + order.address_billing.address4.Replace('\'', '`') + "','" + order.address_billing.address5.Replace('\'', '`');
        //                            insertQ += "','" + order.address_billing.customer_email + "','" + order.address_billing.city.Replace('\'', '`') + "','" + order.address_billing.post_code.Replace('\'', '`') + "','" + order.address_billing.country.Replace('\'', '`') + "','" + order.address_shipping.first_name.Replace('\'', '`') + "','" + order.address_shipping.last_name.Replace('\'', '`') + "','" + order.address_shipping.phone + "','" + order.address_shipping.phone2;
        //                            insertQ += "','" + order.address_shipping.address1.Replace('\'', '`') + "','" + order.address_shipping.address2.Replace('\'', '`') + "','" + order.address_shipping.address3.Replace('\'', '`') + "','" + order.address_shipping.address4.Replace('\'', '`') + "','" + order.address_shipping.address5.Replace('\'', '`') + "','" + order.address_shipping.customer_email + "','" + order.address_shipping.city.Replace('\'', '`');
        //                            insertQ += "','" + order.address_shipping.post_code + "','" + order.address_shipping.country.Replace('\'', '`') + "','" + order.national_registration_number + "'," + order.items_count + ",'" + order.promised_shipping_times + "','" + order.extra_attributes + "','" + statusEra;
        //                            insertQ += "'," + order.voucher + "," + order.shipping_fee + ",'" + order.tax_code + "','" + order.branch_number + "','" + cust + "','" + username + "','" + connectionID + "')";

        //                            //remark by Tri 24/9/2019, pindah insert pembeli
        //                            //var tblKabKot = EDB.GetDataSet("MOConnectionString", "KabupatenKota", "SELECT TOP 1 * FROM KabupatenKota WHERE NamaKabKot LIKE '%" + order.address_billing.address4 + "%'");
        //                            //var tblProv = EDB.GetDataSet("MOConnectionString", "Provinsi", "SELECT TOP 1 * FROM Provinsi WHERE NamaProv LIKE '%" + order.address_billing.address5 + "%'");

        //                            //var kabKot = "3174";//set default value jika tidak ada di db
        //                            //var prov = "31";//set default value jika tidak ada di db

        //                            //if (tblProv.Tables[0].Rows.Count > 0)
        //                            //    prov = tblProv.Tables[0].Rows[0]["KodeProv"].ToString();
        //                            //if (tblKabKot.Tables[0].Rows.Count > 0)
        //                            //    kabKot = tblKabKot.Tables[0].Rows[0]["KodeKabKot"].ToString();

        //                            //insertPembeli += "('" + order.address_billing.first_name.Replace('\'', '`') + "','" + order.address_billing.address1.Replace('\'', '`') + "','" + order.address_billing.phone + "','" + order.address_billing.customer_email + "',0,0,'0','01',";
        //                            //insertPembeli += "1, 'IDR', '01', '" + order.address_billing.address1.Replace('\'', '`') + "', 0, 0, 0, 0, '1', 0, 0, ";
        //                            ////change by calvin 12 desember 2018, ada data dari lazada yang order.address_billing.post_code nya diisi "Bekasi Timur"
        //                            ////insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + order.address_billing.post_code + "', '" + order.address_billing.customer_email + "', '" + kabKot + "', '" + prov + "', '" + order.address_billing.address4 + "', '" + order.address_billing.address5 + "', '" + connIDARF01C + "')";
        //                            //insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + order.address_billing.post_code.Substring(0, order.address_billing.post_code.Length > 5 ? 5 : order.address_billing.post_code.Length).Replace('\'', '`') + "', '" + order.address_billing.customer_email + "', '" + kabKot + "', '" + prov + "', '" + order.address_billing.address4.Replace('\'', '`') + "', '" + order.address_billing.address5.Replace('\'', '`') + "', '" + connIDARF01C + "')";
        //                            ////end change by calvin 12 desember 2018
        //                            //remark by Tri 24/9/2019, pindah insert pembeli

        //                            //change 12 Maret 2019, handle record > 100
        //                            //listOrderId += order.order_id;
        //                            listOrderId.Add(order.order_id);
        //                            //end change 12 Maret 2019, handle record > 100

        //                            insertQ += " , ";
        //                            //insertPembeli += " , ";//remark by Tri 24/9/2019, pindah insert pembeli

        //                            //if (i < bindOrder.data.orders.Count)
        //                            //{
        //                            //remark 12 Maret 2019, handle record > 100
        //                            //listOrderId += ",";
        //                            //remark 12 Maret 2019, handle record > 100
        //                            //}
        //                            //else
        //                            //{
        //                            //    listOrderId += "]";
        //                            //}
        //                            //i = i + 1;
        //                            if (!OrderNoInDb.Contains(Convert.ToString(order.order_id)))
        //                                jmlhNewOrder++;
        //                        }
        //                        if (doInsertPembeli && !string.IsNullOrEmpty(order.address_billing.phone))
        //                        {
        //                            adaInsertPembeli = true;
        //                            var tblKabKot = EDB.GetDataSet("MOConnectionString", "KabupatenKota", "SELECT TOP 1 * FROM KabupatenKota WHERE NamaKabKot LIKE '%" + order.address_billing.address4 + "%'");
        //                            var tblProv = EDB.GetDataSet("MOConnectionString", "Provinsi", "SELECT TOP 1 * FROM Provinsi WHERE NamaProv LIKE '%" + order.address_billing.address5 + "%'");

        //                            var kabKot = "3174";//set default value jika tidak ada di db
        //                            var prov = "31";//set default value jika tidak ada di db

        //                            if (tblProv.Tables[0].Rows.Count > 0)
        //                                prov = tblProv.Tables[0].Rows[0]["KodeProv"].ToString();
        //                            if (tblKabKot.Tables[0].Rows.Count > 0)
        //                                kabKot = tblKabKot.Tables[0].Rows[0]["KodeKabKot"].ToString();

        //                            insertPembeli += "('" + order.address_billing.first_name.Replace('\'', '`') + "','" + order.address_billing.address1.Replace('\'', '`') + "','" + order.address_billing.phone + "','" + order.address_billing.customer_email + "',0,0,'0','01',";
        //                            insertPembeli += "1, 'IDR', '01', '" + order.address_billing.address1.Replace('\'', '`') + "', 0, 0, 0, 0, '1', 0, 0, ";
        //                            //change by calvin 12 desember 2018, ada data dari lazada yang order.address_billing.post_code nya diisi "Bekasi Timur"
        //                            //insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + order.address_billing.post_code + "', '" + order.address_billing.customer_email + "', '" + kabKot + "', '" + prov + "', '" + order.address_billing.address4 + "', '" + order.address_billing.address5 + "', '" + connIDARF01C + "')";
        //                            insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + order.address_billing.post_code.Substring(0, order.address_billing.post_code.Length > 5 ? 5 : order.address_billing.post_code.Length).Replace('\'', '`') + "', '" + order.address_billing.customer_email + "', '" + kabKot + "', '" + prov + "', '" + order.address_billing.address4.Replace('\'', '`') + "', '" + order.address_billing.address5.Replace('\'', '`') + "', '" + connIDARF01C + "')";
        //                            //end change by calvin 12 desember 2018
        //                            insertPembeli += " , ";

        //                        }
        //                    }
        //                    //change by Tri 24/9/2019
        //                    //if (adaInsert)
        //                    if (adaInsert || adaInsertPembeli)
        //                    //end change by Tri 24/9/2019
        //                    {
        //                        SqlCommand CommandSQL = new SqlCommand();

        //                        insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 2);
        //                        if (adaInsertPembeli)
        //                        {
        //                            var aa = EDB.ExecuteSQL(username, CommandType.Text, insertPembeli);
        //                            CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
        //                            CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIDARF01C;
        //                            EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);
        //                        }

        //                        insertQ = insertQ.Substring(0, insertQ.Length - 2);
        //                        if (adaInsert)
        //                        {
        //                            var a = EDB.ExecuteSQL(username, CommandType.Text, insertQ);
        //                            CommandSQL = new SqlCommand();
        //                            CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
        //                            CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;
        //                            CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = fromDt.ToString("yyyy-MM-dd HH:mm:ss");
        //                            CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = toDt.ToString("yyyy-MM-dd HH:mm:ss");
        //                            CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 1;
        //                            CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
        //                            CommandSQL.Parameters.Add("@elevenia", SqlDbType.Int).Value = 0;
        //                            CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
        //                            CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
        //                            CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
        //                            CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 0;
        //                            CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = cust;

        //                            EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);
        //                        }


        //                        ret.status = 1;
        //                        //ret.message = a.ToString();

        //                        //SqlCommand CommandSQL = new SqlCommand();

        //                        //CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
        //                        //CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIDARF01C;
        //                        //EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);

        //                        //CommandSQL = new SqlCommand();
        //                        //CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
        //                        //CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;
        //                        //CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = fromDt.ToString("yyyy-MM-dd HH:mm:ss");
        //                        //CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = toDt.ToString("yyyy-MM-dd HH:mm:ss");
        //                        //CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 1;
        //                        //CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
        //                        //CommandSQL.Parameters.Add("@elevenia", SqlDbType.Int).Value = 0;
        //                        //CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
        //                        //CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
        //                        //CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
        //                        //CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 0;
        //                        //CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = cust;

        //                        //EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);

        //                        //change 12 Maret 2019, handle record > 100
        //                        //listOrderId = listOrderId.Substring(0, listOrderId.Length - 1) + "]";
        //                        //getMultiOrderItems(listOrderId, accessToken, connectionID);
        //                        getMultiOrderItems2(listOrderId, accessToken, connectionID);
        //                        //change 12 Maret 2019, handle record > 100
        //                        //jmlhNewOrder++;
        //                    }
        //                    //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, accessToken, currentLog);

        //                    //if (jmlhNewOrder > 0)
        //                    //{
        //                    //    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
        //                    //    contextNotif.Clients.Group(dbPathEra).moNewOrder("Terdapat " + Convert.ToString(jmlhNewOrder) + " Pesanan baru dari Lazada.");

        //                    //    new StokControllerJob().updateStockMarketPlace(connectionID, dbPathEra, uname);
        //                    //}
        //                }
        //                else
        //                {
        //                    ret.message = "no order";
        //                }
        //            }
        //            else
        //            {
        //                //currentLog.REQUEST_EXCEPTION = bindOrder.message;
        //                //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, accessToken, currentLog);
        //                ret.message = "lazada api return error";
        //                if (string.IsNullOrEmpty(bindOrder.message))
        //                    ret.message += "\n" + bindOrder.message.ToString();

        //            }
        //        }
        //        else
        //        {
        //            ret.message = "failed to call lazada api";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ret.message = ex.ToString();
        //        //currentLog.REQUEST_EXCEPTION = ex.Message;
        //        //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, accessToken, currentLog);
        //    }
        //    return ret;
        //}
        public class cekSO
        {
            public string NO_REFERENSI { get; set; }
            public string PEMESAN { get; set; }
            public string STATUS_TRANSAKSI { get; set; }
            public string NO_BUKTI { get; set; }
        }
        public BindingBase GetOrdersToUpdateMOWithPage(string cust, string accessToken, string dbPathEra, string uname, int page)
        {
            var ret = new BindingBase();
            ret.status = 0;
            ret.recordCount = 0;

            var MoDbContext = new MoDbContext("");
            var EDB = new DatabaseSQL(dbPathEra);
            string EraServerName = EDB.GetServerName("sConn");
            var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);
            var username = uname;

            string connectionID = Guid.NewGuid().ToString();
            var fromDt = DateTime.Now.AddDays(-14);
            var toDt = DateTime.Now.AddDays(1);
            var list_02 = new List<string>();
            var list_03 = new List<string>();
            var list_04 = new List<string>();
            var list_11 = new List<string>();

            //SetupContext(dbPathEra, uname);
            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
            //    REQUEST_ACTION = "Get Order",
            //    REQUEST_DATETIME = DateTime.Now,
            //    REQUEST_ATTRIBUTE_1 = connectionID,
            //    REQUEST_STATUS = "Pending",
            //};
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, accessToken, currentLog);

            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/orders/get");
            request.SetHttpMethod("GET");
            request.AddApiParameter("created_before", toDt.ToString("yyyy-MM-ddTHH:mm:ss") + "+07:00");
            request.AddApiParameter("created_after", fromDt.ToString("yyyy-MM-ddTHH:mm:ss") + "+07:00");
            request.AddApiParameter("sort_direction", "DESC");
            //request.AddApiParameter("offset", "0");
            request.AddApiParameter("offset", (page * 100).ToString());
            request.AddApiParameter("limit", "100");
            request.AddApiParameter("sort_by", "updated_at");
            LazopResponse response = client.Execute(request, accessToken);
            var bindOrder = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(NewLzdOrders)) as NewLzdOrders;
            if (bindOrder != null)
            {
                if (bindOrder.code.Equals("0"))
                {
                    ret.recordCount = bindOrder.data.orders.Count;
                    //karena toh logic disini hanya jalan jika sudah ada di DB
                    var untukCekdiSO = bindOrder.data.orders.Select(p => p.order_id).ToList();

                    var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == cust && p.TGL.Value >= fromDt && untukCekdiSO.Contains(p.NO_REFERENSI)).
                        Select(p => new cekSO { NO_REFERENSI = p.NO_REFERENSI, NO_BUKTI = p.NO_BUKTI, PEMESAN = p.PEMESAN, STATUS_TRANSAKSI = p.STATUS_TRANSAKSI }).
                        ToList();
                    var listNoRefInSO = OrderNoInDb.Select(p => p.NO_REFERENSI).ToList();
                    var orderWithOrderIdInSO = bindOrder.data.orders.Where(p => listNoRefInSO.Contains(p.order_id)).ToList();
                    var listNoRefUntukCekSIT01a = orderWithOrderIdInSO.Select(p => p.order_id).ToList();
                    var getSIT01A = ErasoftDbContext.SIT01A.Where(p => listNoRefUntukCekSIT01a.Contains(p.NO_REF)).Select(p => p.NO_REF).ToList();
                    foreach (var order in orderWithOrderIdInSO)
                    {
                        //if (OrderNoInDb.Contains(Convert.ToString(order.order_id)))
                        //{
                        var orderMO = OrderNoInDb.Where(p => p.NO_REFERENSI == order.order_id).FirstOrDefault();
                        if (orderMO != null)
                        {
                            if (string.IsNullOrEmpty(orderMO.PEMESAN) && order.address_billing != null)
                            {
                                if (!string.IsNullOrEmpty(order.address_billing.phone))
                                {
                                    InsertPembeli(order, connectionID, dbPathEra, username);
                                    var pembeliInDB = ErasoftDbContext.ARF01C.Where(m => m.TLP == order.address_billing.phone).FirstOrDefault();
                                    if (pembeliInDB != null)
                                    {
                                        var rowAffected2 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET PEMESAN = '" + pembeliInDB.BUYER_CODE + "' WHERE NO_BUKTI = '" + orderMO.NO_BUKTI + "'");
                                    }
                                    else
                                    {
                                        var adaPembeliGagalInsert = true;
                                    }
                                }
                            }
                            //change by Tri 12 mar 2020, shipped akan menggunakan status lain, bukan selesai/04
                            //if (order.statuses[0].ToString() == "delivered" || order.statuses[0].ToString() == "shipped")
                            if (order.statuses[0].ToString() == "delivered")
                            //end change by Tri 12 mar 2020, shipped akan menggunakan status lain, bukan selesai/04
                            {
                                if (orderMO.STATUS_TRANSAKSI != "04")
                                {
                                    //ubah status menjadi selesai jika sudah ada faktur
                                    if (getSIT01A.Contains(order.order_id))
                                    {
                                        list_04.Add(orderMO.NO_BUKTI);
                                    }
                                    //var dsSIT01A = EDB.GetDataSet("CString", "SIT01A", "SELECT NO_REFERENSI, O.NO_BUKTI, O.STATUS_TRANSAKSI FROM SIT01A I INNER JOIN SOT01A O ON I.NO_SO = O.NO_BUKTI WHERE NO_REFERENSI = '" + order.order_id + "'");
                                    //if (dsSIT01A.Tables[0].Rows.Count > 0)
                                    //{
                                    //}
                                }
                            }
                            else if (order.statuses[0].ToString() == "ready_to_ship")
                            {
                                //remark by calvin, approve by pak dani, MO hanya ubah jika statusnya sudah delivered / shipped
                                //if (orderMO.STATUS_TRANSAKSI != "03")
                                //{
                                //    if (getSIT01A.Contains(order.order_id))
                                //    {
                                //        list_03.Add(orderMO.NO_BUKTI);
                                //    }
                                //}
                            }
                            //else if (order.statuses[0].ToString() == "canceled" || order.statuses[0].ToString() == "failed")
                            //{

                            //}
                        }
                        //}
                        //else
                        //{

                        //}
                    }
                    if (list_04.Count > 0)
                    {
                        string sSQL = "UPDATE SOT01A SET STATUS_TRANSAKSI = '04' WHERE NO_BUKTI IN (";
                        foreach (var nobuk in list_04)
                        {
                            sSQL += "'" + nobuk + "' ,";
                        }
                        sSQL = sSQL.Substring(0, sSQL.Length - 2) + ")";
                        var result = EDB.ExecuteSQL("CString", CommandType.Text, sSQL);
                    }
                    //remark by calvin, approve by pak dani, MO hanya ubah jika statusnya sudah delivered / shipped
                    //if (list_03.Count > 0)
                    //{
                    //    string sSQL2 = "UPDATE SOT01A SET STATUS_TRANSAKSI = '03' WHERE NO_BUKTI IN (";
                    //    foreach (var nobuk in list_03)
                    //    {
                    //        sSQL2 += "'" + nobuk + "' ,";
                    //    }
                    //    sSQL2 = sSQL2.Substring(0, sSQL2.Length - 2) + ")";
                    //    var result2 = EDB.ExecuteSQL("CString", CommandType.Text, sSQL2);
                    //}
                    //end remark by calvin, approve by pak dani, MO hanya ubah jika statusnya sudah delivered / shipped

                }

            }
            return ret;
        }
        //end add by Tri 4 Nov 2019, update pesanan yg sudah ada di MO

        public BindingBase GetBrgLazada(string cust, string accessToken, int page, int recordCount)
        {
            var ret = new BindingBase();
            ret.status = 0;
            ret.recordCount = recordCount;
            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/products/get");
            request.SetHttpMethod("GET");
            request.AddApiParameter("filter", "all");//Possible values are all, live, inactive, deleted, image-missing, pending, rejected, sold-out. 
                                                     //request.AddApiParameter("update_before", "2018-01-01T00:00:00+0800");
                                                     //request.AddApiParameter("search", "vincenza tea set");
                                                     //request.AddApiParameter("create_before", "2018-01-01T00:00:00+0800");
            request.AddApiParameter("offset", (10 * page).ToString());
            //request.AddApiParameter("create_after", "2010-01-01T00:00:00+0800");
            //request.AddApiParameter("update_after", "2010-01-01T00:00:00+0800");
            request.AddApiParameter("limit", "10");
            //request.AddApiParameter("options", "1");
            //request.AddApiParameter("sku_seller_list", " [\"N105\"]");
            try
            {
                LazopResponse response = client.Execute(request, accessToken);
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body);
                if (response.Code.Equals("0"))
                {
                    if (result.data.products != null)
                    {
                        if (result.data.products.Count > 0)
                        {
                            ret.status = 1;
                            int IdMarket = ErasoftDbContext.ARF01.Where(c => c.CUST.Equals(cust)).FirstOrDefault().RecNum.Value;
                            if (result.data.products.Count == 10)
                            {
                                ret.message = (page + 1).ToString();
                            }
                            string sSQL = "INSERT INTO TEMP_BRG_MP (BRG_MP, SELLER_SKU, NAMA, NAMA2, NAMA3, BERAT, PANJANG, LEBAR, TINGGI, CUST, Deskripsi, IDMARKET, HJUAL, HJUAL_MP, ";
                            sSQL += "DISPLAY, CATEGORY_CODE, CATEGORY_NAME, MEREK, IMAGE, IMAGE2, IMAGE3, KODE_BRG_INDUK, TYPE, DeliveryTempElevenia, PICKUP_POINT,";
                            sSQL += "ACODE_1, ANAME_1, AVALUE_1, ACODE_2, ANAME_2, AVALUE_2, ACODE_3, ANAME_3, AVALUE_3, ACODE_4, ANAME_4, AVALUE_4, ACODE_5, ANAME_5, AVALUE_5, ACODE_6, ANAME_6, AVALUE_6, ACODE_7, ANAME_7, AVALUE_7, ACODE_8, ANAME_8, AVALUE_8, ACODE_9, ANAME_9, AVALUE_9, ACODE_10, ANAME_10, AVALUE_10, ";
                            sSQL += "ACODE_11, ANAME_11, AVALUE_11, ACODE_12, ANAME_12, AVALUE_12, ACODE_13, ANAME_13, AVALUE_13, ACODE_14, ANAME_14, AVALUE_14, ACODE_15, ANAME_15, AVALUE_15, ACODE_16, ANAME_16, AVALUE_16, ACODE_17, ANAME_17, AVALUE_17, ACODE_18, ANAME_18, AVALUE_18, ACODE_19, ANAME_19, AVALUE_19, ACODE_20, ANAME_20, AVALUE_20, ";
                            sSQL += "ACODE_21, ANAME_21, AVALUE_21, ACODE_22, ANAME_22, AVALUE_22, ACODE_23, ANAME_23, AVALUE_23, ACODE_24, ANAME_24, AVALUE_24, ACODE_25, ANAME_25, AVALUE_25, ACODE_26, ANAME_26, AVALUE_26, ACODE_27, ANAME_27, AVALUE_27, ACODE_28, ANAME_28, AVALUE_28, ACODE_29, ANAME_29, AVALUE_29, ACODE_30, ANAME_30, AVALUE_30, ";
                            sSQL += "ACODE_31, ANAME_31, AVALUE_31, ACODE_32, ANAME_32, AVALUE_32, ACODE_33, ANAME_33, AVALUE_33, ACODE_34, ANAME_34, AVALUE_34, ACODE_35, ANAME_35, AVALUE_35, ACODE_36, ANAME_36, AVALUE_36, ACODE_37, ANAME_37, AVALUE_37, ACODE_38, ANAME_38, AVALUE_38, ACODE_39, ANAME_39, AVALUE_39, ACODE_40, ANAME_40, AVALUE_40, ";
                            sSQL += "ACODE_41, ANAME_41, AVALUE_41, ACODE_42, ANAME_42, AVALUE_42, ACODE_43, ANAME_43, AVALUE_43, ACODE_44, ANAME_44, AVALUE_44, ACODE_45, ANAME_45, AVALUE_45, ACODE_46, ANAME_46, AVALUE_46, ACODE_47, ANAME_47, AVALUE_47, ACODE_48, ANAME_48, AVALUE_48, ACODE_49, ANAME_49, AVALUE_49, ACODE_50, ANAME_50, AVALUE_50) VALUES ";

                            string sSQL_Value = "";
                            bool varian = false;
                            //add 13 Feb 2019, tuning
                            var stf02h_local = ErasoftDbContext.STF02H.Where(m => m.IDMARKET == IdMarket).ToList();
                            var tempBrg_local = ErasoftDbContext.TEMP_BRG_MP.Where(m => m.IDMARKET == IdMarket).ToList();
                            //end add 13 Feb 2019, tuning

                            foreach (var brg in result.data.products)
                            {
                                if (brg.skus.Count > 1)
                                {
                                    varian = true;
                                }
                                else
                                {
                                    varian = false;
                                }
                                string kdBrgInduk = "";
                                for (int i = 0; i < brg.skus.Count; i++)
                                {
                                    var tempbrginDB = new TEMP_BRG_MP();
                                    var brgInDB = new STF02H();
                                    string kodeBrg = "";
                                    //add 18-03-2019, cek item id
                                    bool createInduk = true;
                                    kodeBrg = brg.item_id;
                                    string SkuId = brg.skus[i].SkuId;
                                    if (i == 0)
                                    {
                                        brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG_MP == kodeBrg && t.IDMARKET == IdMarket).FirstOrDefault();
                                        if (brgInDB != null)
                                        {
                                            kdBrgInduk = brgInDB.BRG;
                                            //item id sudah ada di master
                                            if (!varian)
                                            {
                                                //jika brg ini terbaca sebagai barang non varian -> ubah menjadi varian
                                                varian = true;
                                            }
                                        }
                                        else
                                        {
                                            var listTemp = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.PICKUP_POINT == kodeBrg && t.DeliveryTempElevenia != SkuId && t.IDMARKET == IdMarket).ToList();
                                            if (listTemp.Count == 1)
                                            {
                                                //if(listTemp[0].TYPE != "4")
                                                //{
                                                //item id sudah ada di temp, tp masuk sebagai barang non varian
                                                if (!varian)
                                                {
                                                    //jika brg ini terbaca sebagai barang non varian -> ubah menjadi varian
                                                    varian = true;
                                                }
                                                //update brg sebelumnya menjadi brg varian
                                                tempbrginDB = listTemp[0];
                                                tempbrginDB.TYPE = "3";
                                                tempbrginDB.KODE_BRG_INDUK = tempbrginDB.PICKUP_POINT;
                                                ErasoftDbContext.SaveChanges();
                                                //}
                                            }
                                            else if (listTemp.Count > 1 && !varian)
                                            {
                                                varian = true;
                                                createInduk = false;
                                            }
                                        }

                                    }
                                    //end add 18-03-2019, cek item id

                                    if (varian && i == 0 && createInduk)
                                    {
                                        //kodeBrg = brg.item_id;
                                        if (string.IsNullOrEmpty(kdBrgInduk))
                                            kdBrgInduk = kodeBrg;
                                        //tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.Equals(kodeBrg)).FirstOrDefault();
                                        //brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG_MP.Equals(kodeBrg) && t.IDMARKET == IdMarket).FirstOrDefault();
                                        tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == kodeBrg.ToUpper()).FirstOrDefault();
                                        brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == kodeBrg.ToUpper()).FirstOrDefault();

                                        if (tempbrginDB == null && brgInDB == null)
                                        {
                                            //create brg induk
                                            BindingBase retSQLInduk = insertTempBrgQry(brg, i, IdMarket, cust, 1, "");
                                            if (retSQLInduk.status == 1)
                                                sSQL_Value += retSQLInduk.message;
                                        }
                                        //else if (brgInDB != null)
                                        //{
                                        //    kdBrgInduk = kodeBrg;
                                        //}
                                    }
                                    kodeBrg = brg.skus[i].SellerSku;
                                    //tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.Equals(kodeBrg)).FirstOrDefault();
                                    //brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG_MP.Equals(kodeBrg) && t.IDMARKET == IdMarket).FirstOrDefault();
                                    tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == kodeBrg.ToUpper()).FirstOrDefault();
                                    brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == kodeBrg.ToUpper()).FirstOrDefault();

                                    if (tempbrginDB == null && brgInDB == null)
                                    {
                                        #region remark 21-01-2019, handle brg induk dan varian
                                        //ret.recordCount++;
                                        //sSQL_Value += " ( '" + brg.skus[i].SellerSku + "' , '" + brg.skus[i].SellerSku + "' , '";
                                        //string namaBrg = brg.attributes.name;
                                        //string nama, nama2, nama3, urlImage, urlImage2, urlImage3;
                                        //urlImage = "";
                                        //urlImage2 = "";
                                        //urlImage3 = "";
                                        //if (namaBrg.Length > 30)
                                        //{
                                        //    nama = namaBrg.Substring(0, 30);
                                        //    //change by calvin 15 januari 2019
                                        //    //if (namaBrg.Length > 60)
                                        //    //{
                                        //    //    nama2 = namaBrg.Substring(30, 30);
                                        //    //    nama3 = (namaBrg.Length > 90) ? namaBrg.Substring(60, 30) : namaBrg.Substring(60);
                                        //    //}
                                        //    if (namaBrg.Length > 285)
                                        //    {
                                        //        nama2 = namaBrg.Substring(30, 255);
                                        //        nama3 = "";
                                        //    }
                                        //    //end change by calvin 15 januari 2019
                                        //    else
                                        //    {
                                        //        nama2 = namaBrg.Substring(30);
                                        //        nama3 = "";
                                        //    }
                                        //}
                                        //else
                                        //{
                                        //    nama = namaBrg;
                                        //    nama2 = "";
                                        //    nama3 = "";
                                        //}
                                        //string categoryCode = brg.primary_category;
                                        ////if (namaBrg.Length > 30)
                                        ////{
                                        ////    sSQL += namaBrg.Substring(0, 30) + "' , '" + namaBrg.Substring(30) + "' , ";
                                        ////}
                                        ////else
                                        ////{
                                        ////    sSQL += namaBrg + "' , '' , ";

                                        ////}
                                        //sSQL_Value += nama.Replace('\'', '`') + "' , '" + nama2.Replace('\'', '`') + "' , '" + nama3.Replace('\'', '`') + "' ,";

                                        //if (brg.skus[i].Images != null)
                                        //{
                                        //    if (brg.skus[i].Images[0] != null)
                                        //        urlImage = brg.skus[i].Images[0];
                                        //    if (brg.skus[i].Images[1] != null)
                                        //        urlImage2 = brg.skus[i].Images[1];
                                        //    if (brg.skus[i].Images[2] != null)
                                        //        urlImage3 = brg.skus[i].Images[2];
                                        //}

                                        //var brgAttribute = new Dictionary<string, string>();
                                        //var brgSku = new Dictionary<string, string>();
                                        //foreach (Newtonsoft.Json.Linq.JProperty property in brg.attributes)
                                        //{
                                        //    brgAttribute.Add(property.Name, property.Value.ToString());
                                        //}
                                        //foreach (Newtonsoft.Json.Linq.JProperty property in brg.skus[i])
                                        //{
                                        //    brgSku.Add(property.Name, property.Value.ToString());
                                        //}
                                        //string value;
                                        //var statusBrg = (brgSku.TryGetValue("Status", out value) ? value : "");
                                        //var display = statusBrg.Equals("active") ? 1 : 0;
                                        //string deskripsi = brg.attributes.description;
                                        //string s_deskripsi = (brgAttribute.TryGetValue("short_description", out value) ? value : "").ToString().Replace("<br/>", "\r\n").Replace("<br />", "\r\n").Replace('\'', '`');
                                        ////remark by Tri, length avalue_1 sudah diubah 250 -> max
                                        ////if (s_deskripsi.Length > 250)
                                        ////    s_deskripsi = s_deskripsi.Substring(0, 250);
                                        ////end remark by Tri, length avalue_1 sudah diubah 250 -> max
                                        //sSQL_Value += Convert.ToDouble(brg.skus[i].package_weight) * 1000 + " , " + brg.skus[i].package_length + " , " + brg.skus[i].package_width + " , " + brg.skus[i].package_height + " , '" + cust + "' , '";
                                        //sSQL_Value += string.IsNullOrEmpty(deskripsi) ? "" : brg.attributes.description.ToString().Replace("<br/>", "\r\n").Replace("<br />", "\r\n").Replace('\'', '`');
                                        //sSQL_Value += "' , " + IdMarket + " , " + brg.skus[i].price + " , " + brg.skus[i].price + " , ";
                                        //sSQL_Value += display + " , '" + categoryCode + "' , '" + MoDbContext.CATEGORY_LAZADA.Where(c => c.CATEGORY_ID.Equals(categoryCode)).FirstOrDefault().NAME + "' , '";
                                        //sSQL_Value += brg.attributes.brand + "' , '" + urlImage + "' , '" + urlImage2 + "' , '" + urlImage3 + "'";
                                        //var attributeLzd = MoDbContext.ATTRIBUTE_LAZADA.Where(a => a.CATEGORY_CODE.Equals(categoryCode)).FirstOrDefault();
                                        ////bool getAttr = true;
                                        //if (attributeLzd == null)
                                        //{
                                        //    var retAPI = getMissingAttr(categoryCode);
                                        //    if(retAPI.status == 1)
                                        //    {
                                        //        attributeLzd = MoDbContext.ATTRIBUTE_LAZADA.AsNoTracking().Where(a => a.CATEGORY_CODE.Equals(categoryCode)).FirstOrDefault();
                                        //    }
                                        //}
                                        //#region set attribute

                                        //if (attributeLzd != null)
                                        ////if (getAttr)
                                        //{

                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME1))
                                        //    {
                                        //        if (attributeLzd.ANAME1.Equals("short_description"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME1 + "' , '" + attributeLzd.ALABEL1.Replace("\'", "\'\'") + "' , '" + s_deskripsi + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            if (attributeLzd.ATYPE1.Equals("sku"))
                                        //            {
                                        //                sSQL_Value += ", '" + attributeLzd.ANAME1 + "' , '" + attributeLzd.ALABEL1.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME1, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //            }
                                        //            else
                                        //            {
                                        //                sSQL_Value += ", '" + attributeLzd.ANAME1 + "' , '" + attributeLzd.ALABEL1.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME1, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //            }
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME2))
                                        //    {
                                        //        if (attributeLzd.ATYPE2.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME2 + "' , '" + attributeLzd.ALABEL2.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME2, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME2 + "' , '" + attributeLzd.ALABEL2.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME2, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME3))
                                        //    {
                                        //        if (attributeLzd.ATYPE3.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME3 + "' , '" + attributeLzd.ALABEL3.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME3, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME3 + "' , '" + attributeLzd.ALABEL3.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME3, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME4))
                                        //    {
                                        //        if (attributeLzd.ATYPE4.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME4 + "' , '" + attributeLzd.ALABEL4.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME4, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME4 + "' , '" + attributeLzd.ALABEL4.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME4, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME5))
                                        //    {
                                        //        if (attributeLzd.ATYPE5.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME5 + "' , '" + attributeLzd.ALABEL5.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME5, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME5 + "' , '" + attributeLzd.ALABEL5.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME5, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME6))
                                        //    {
                                        //        if (attributeLzd.ATYPE6.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME6 + "' , '" + attributeLzd.ALABEL6.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME6, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME6 + "' , '" + attributeLzd.ALABEL6.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME6, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME7))
                                        //    {
                                        //        if (attributeLzd.ATYPE7.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME7 + "' , '" + attributeLzd.ALABEL7.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME7, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME7 + "' , '" + attributeLzd.ALABEL7.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME7, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME8))
                                        //    {
                                        //        if (attributeLzd.ATYPE8.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME8 + "' , '" + attributeLzd.ALABEL8.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME8, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME8 + "' , '" + attributeLzd.ALABEL8.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME8, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME9))
                                        //    {
                                        //        if (attributeLzd.ATYPE9.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME9 + "' , '" + attributeLzd.ALABEL9.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME9, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME9 + "' , '" + attributeLzd.ALABEL9.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME9, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME10))
                                        //    {
                                        //        if (attributeLzd.ATYPE10.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME10 + "' , '" + attributeLzd.ALABEL10.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME10, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME10 + "' , '" + attributeLzd.ALABEL10.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME10, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME11))
                                        //    {
                                        //        if (attributeLzd.ATYPE11.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME11 + "' , '" + attributeLzd.ALABEL11.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME11, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME11 + "' , '" + attributeLzd.ALABEL11.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME11, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME12))
                                        //    {
                                        //        if (attributeLzd.ATYPE12.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME12 + "' , '" + attributeLzd.ALABEL12.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME12, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME12 + "' , '" + attributeLzd.ALABEL12.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME12, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME13))
                                        //    {
                                        //        if (attributeLzd.ATYPE13.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME13 + "' , '" + attributeLzd.ALABEL13.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME13, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME13 + "' , '" + attributeLzd.ALABEL13.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME13, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME14))
                                        //    {
                                        //        if (attributeLzd.ATYPE14.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME14 + "' , '" + attributeLzd.ALABEL14.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME14, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME14 + "' , '" + attributeLzd.ALABEL14.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME14, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME15))
                                        //    {
                                        //        if (attributeLzd.ATYPE15.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME15 + "' , '" + attributeLzd.ALABEL15.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME15, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME15 + "' , '" + attributeLzd.ALABEL15.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME15, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME16))
                                        //    {
                                        //        if (attributeLzd.ATYPE16.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME16 + "' , '" + attributeLzd.ALABEL16.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME16, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME16 + "' , '" + attributeLzd.ALABEL16.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME16, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME17))
                                        //    {
                                        //        if (attributeLzd.ATYPE17.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME17 + "' , '" + attributeLzd.ALABEL17.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME17, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME17 + "' , '" + attributeLzd.ALABEL17.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME17, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME18))
                                        //    {
                                        //        if (attributeLzd.ATYPE18.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME18 + "' , '" + attributeLzd.ALABEL18.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME18, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME18 + "' , '" + attributeLzd.ALABEL18.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME18, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME19))
                                        //    {
                                        //        if (attributeLzd.ATYPE19.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME19 + "' , '" + attributeLzd.ALABEL19.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME19, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME19 + "' , '" + attributeLzd.ALABEL19.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME19, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME20))
                                        //    {
                                        //        if (attributeLzd.ATYPE20.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME20 + "' , '" + attributeLzd.ALABEL20.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME20, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME20 + "' , '" + attributeLzd.ALABEL20.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME20, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME21))
                                        //    {
                                        //        if (attributeLzd.ATYPE21.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME21 + "' , '" + attributeLzd.ALABEL21.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME21, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME21 + "' , '" + attributeLzd.ALABEL21.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME21, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME22))
                                        //    {
                                        //        if (attributeLzd.ATYPE22.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME22 + "' , '" + attributeLzd.ALABEL22.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME22, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME22 + "' , '" + attributeLzd.ALABEL22.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME22, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME23))
                                        //    {
                                        //        if (attributeLzd.ATYPE23.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME23 + "' , '" + attributeLzd.ALABEL23.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME23, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME23 + "' , '" + attributeLzd.ALABEL23.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME23, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME24))
                                        //    {
                                        //        if (attributeLzd.ATYPE24.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME24 + "' , '" + attributeLzd.ALABEL24.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME24, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME24 + "' , '" + attributeLzd.ALABEL24.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME24, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME25))
                                        //    {
                                        //        if (attributeLzd.ATYPE25.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME25 + "' , '" + attributeLzd.ALABEL25.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME25, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME25 + "' , '" + attributeLzd.ALABEL25.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME25, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME26))
                                        //    {
                                        //        if (attributeLzd.ATYPE26.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME26 + "' , '" + attributeLzd.ALABEL26.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME26, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME26 + "' , '" + attributeLzd.ALABEL26.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME26, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME27))
                                        //    {
                                        //        if (attributeLzd.ATYPE27.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME27 + "' , '" + attributeLzd.ALABEL27.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME27, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME27 + "' , '" + attributeLzd.ALABEL27.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME27, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME28))
                                        //    {
                                        //        if (attributeLzd.ATYPE28.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME28 + "' , '" + attributeLzd.ALABEL28.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME28, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME28 + "' , '" + attributeLzd.ALABEL28.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME28, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME29))
                                        //    {
                                        //        if (attributeLzd.ATYPE29.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME29 + "' , '" + attributeLzd.ALABEL29.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME29, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME29 + "' , '" + attributeLzd.ALABEL29.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME29, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME30))
                                        //    {
                                        //        if (attributeLzd.ATYPE30.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME30 + "' , '" + attributeLzd.ALABEL30.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME30, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME30 + "' , '" + attributeLzd.ALABEL30.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME30, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME31))
                                        //    {
                                        //        if (attributeLzd.ATYPE31.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME31 + "' , '" + attributeLzd.ALABEL31.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME31, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME31 + "' , '" + attributeLzd.ALABEL31.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME31, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME32))
                                        //    {
                                        //        if (attributeLzd.ATYPE32.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME32 + "' , '" + attributeLzd.ALABEL32.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME32, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME32 + "' , '" + attributeLzd.ALABEL32.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME32, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME33))
                                        //    {
                                        //        if (attributeLzd.ATYPE33.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME33 + "' , '" + attributeLzd.ALABEL33.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME33, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME33 + "' , '" + attributeLzd.ALABEL33.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME33, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME34))
                                        //    {
                                        //        if (attributeLzd.ATYPE34.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME34 + "' , '" + attributeLzd.ALABEL34.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME34, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME34 + "' , '" + attributeLzd.ALABEL34.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME34, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME35))
                                        //    {
                                        //        if (attributeLzd.ATYPE35.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME35 + "' , '" + attributeLzd.ALABEL35.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME35, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME35 + "' , '" + attributeLzd.ALABEL35.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME35, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME36))
                                        //    {
                                        //        if (attributeLzd.ATYPE36.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME36 + "' , '" + attributeLzd.ALABEL36.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME36, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME36 + "' , '" + attributeLzd.ALABEL36.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME36, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME37))
                                        //    {
                                        //        if (attributeLzd.ATYPE37.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME37 + "' , '" + attributeLzd.ALABEL37.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME37, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME37 + "' , '" + attributeLzd.ALABEL37.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME37, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME38))
                                        //    {
                                        //        if (attributeLzd.ATYPE38.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME38 + "' , '" + attributeLzd.ALABEL38.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME38, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME38 + "' , '" + attributeLzd.ALABEL38.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME38, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME39))
                                        //    {
                                        //        if (attributeLzd.ATYPE39.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME39 + "' , '" + attributeLzd.ALABEL39.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME39, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME39 + "' , '" + attributeLzd.ALABEL39.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME39, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME40))
                                        //    {
                                        //        if (attributeLzd.ATYPE40.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME40 + "' , '" + attributeLzd.ALABEL40.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME40, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME40 + "' , '" + attributeLzd.ALABEL40.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME40, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME41))
                                        //    {
                                        //        if (attributeLzd.ATYPE41.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME41 + "' , '" + attributeLzd.ALABEL41.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME41, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME41 + "' , '" + attributeLzd.ALABEL41.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME41, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME42))
                                        //    {
                                        //        if (attributeLzd.ATYPE42.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME42 + "' , '" + attributeLzd.ALABEL42.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME42, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME42 + "' , '" + attributeLzd.ALABEL42.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME42, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME43))
                                        //    {
                                        //        if (attributeLzd.ATYPE43.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME43 + "' , '" + attributeLzd.ALABEL43.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME43, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME43 + "' , '" + attributeLzd.ALABEL43.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME43, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME44))
                                        //    {
                                        //        if (attributeLzd.ATYPE44.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME44 + "' , '" + attributeLzd.ALABEL44.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME44, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME44 + "' , '" + attributeLzd.ALABEL44.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME44, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME45))
                                        //    {
                                        //        if (attributeLzd.ATYPE45.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME45 + "' , '" + attributeLzd.ALABEL45.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME45, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME45 + "' , '" + attributeLzd.ALABEL45.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME45, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME46))
                                        //    {
                                        //        if (attributeLzd.ATYPE46.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME46 + "' , '" + attributeLzd.ALABEL46.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME46, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME46 + "' , '" + attributeLzd.ALABEL46.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME46, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME47))
                                        //    {
                                        //        if (attributeLzd.ATYPE47.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME47 + "' , '" + attributeLzd.ALABEL47.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME47, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME47 + "' , '" + attributeLzd.ALABEL47.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME47, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME48))
                                        //    {
                                        //        if (attributeLzd.ATYPE48.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME48 + "' , '" + attributeLzd.ALABEL48.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME48, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME48 + "' , '" + attributeLzd.ALABEL48.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME48, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME49))
                                        //    {
                                        //        if (attributeLzd.ATYPE49.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME49 + "' , '" + attributeLzd.ALABEL49.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME49, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME49 + "' , '" + attributeLzd.ALABEL49.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME49, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', ''";
                                        //    }
                                        //    if (!string.IsNullOrEmpty(attributeLzd.ANAME50))
                                        //    {
                                        //        if (attributeLzd.ATYPE50.Equals("sku"))
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME50 + "' , '" + attributeLzd.ALABEL50.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME50, out value) ? value.Replace("\'", "\'\'") : "") + "') ,";
                                        //        }
                                        //        else
                                        //        {
                                        //            sSQL_Value += ", '" + attributeLzd.ANAME50 + "' , '" + attributeLzd.ALABEL50.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME50, out value) ? value.Replace("\'", "\'\'") : "") + "') ,";
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        sSQL_Value += ", '', '', '') ,";
                                        //    }
                                        //}
                                        //else//fail to get lazada attribute from db and API
                                        //{
                                        //    sSQL_Value += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', ''";
                                        //    sSQL_Value += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', ''";
                                        //    sSQL_Value += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', ''";
                                        //    sSQL_Value += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', ''";
                                        //    sSQL_Value += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '')";
                                        //}

                                        //#endregion
                                        #endregion
                                        if (!varian)
                                        {
                                            BindingBase retSQL = insertTempBrgQry(brg, i, IdMarket, cust, 0, "");
                                            if (retSQL.status == 1)
                                                sSQL_Value += retSQL.message;
                                        }
                                        else
                                        {
                                            BindingBase retSQL = insertTempBrgQry(brg, i, IdMarket, cust, 2, kdBrgInduk);
                                            if (retSQL.status == 1)
                                                sSQL_Value += retSQL.message;
                                        }
                                    }
                                }

                            }
                            if (!string.IsNullOrEmpty(sSQL_Value))
                            {
                                sSQL = sSQL + sSQL_Value;
                                sSQL = sSQL.Substring(0, sSQL.Length - 1);
                                var a = EDB.ExecuteSQL("CString", CommandType.Text, sSQL);
                                ret.recordCount += a;
                            }
                        }
                    }

                }
                else
                {
                    ret.message = response.Message;
                }
            }
            catch (Exception ex)
            {
                ret.status = 0;
                ret.message = ex.Message;
            }


            return ret;
        }

        public BindingBase insertTempBrgQry(dynamic brg, int i, int IdMarket, string cust, int typeBrg, string kodeBrgInduk)
        {
            // typeBrg : 0 = barang tanpa varian; 1 = barang induk; 2 = barang varian
            var ret = new BindingBase();
            ret.status = 0;
            string sSQL_Value = "";
            try
            {
                if (typeBrg != 1)
                {
                    sSQL_Value += " ( '" + brg.skus[i].SellerSku + "' , '" + brg.skus[i].SellerSku + "' , '";
                }
                else
                {
                    sSQL_Value += " ( '" + brg.item_id + "' , '" + brg.item_id + "' , '";
                }
                string namaBrg = brg.attributes.name;
                if (typeBrg == 2)
                {
                    //tambah jenis varian
                    string namaVar = brg.skus[i]._compatible_variation_;
                    namaBrg += " " + namaVar;
                }
                string nama, nama2, nama3, urlImage, urlImage2, urlImage3;
                urlImage = "";
                urlImage2 = "";
                urlImage3 = "";
                if (namaBrg.Length > 30)
                {
                    nama = namaBrg.Substring(0, 30);
                    //change by calvin 15 januari 2019
                    //if (namaBrg.Length > 60)
                    //{
                    //    nama2 = namaBrg.Substring(30, 30);
                    //    nama3 = (namaBrg.Length > 90) ? namaBrg.Substring(60, 30) : namaBrg.Substring(60);
                    //}
                    if (namaBrg.Length > 285)
                    {
                        nama2 = namaBrg.Substring(30, 255);
                        nama3 = "";
                    }
                    //end change by calvin 15 januari 2019
                    else
                    {
                        nama2 = namaBrg.Substring(30);
                        nama3 = "";
                    }
                }
                else
                {
                    nama = namaBrg;
                    nama2 = "";
                    nama3 = "";
                }
                string categoryCode = brg.primary_category;
                //if (namaBrg.Length > 30)
                //{
                //    sSQL += namaBrg.Substring(0, 30) + "' , '" + namaBrg.Substring(30) + "' , ";
                //}
                //else
                //{
                //    sSQL += namaBrg + "' , '' , ";

                //}
                sSQL_Value += nama.Replace('\'', '`') + "' , '" + nama2.Replace('\'', '`') + "' , '" + nama3.Replace('\'', '`') + "' ,";

                if (brg.skus[i].Images != null)
                {
                    if (brg.skus[i].Images[0] != null)
                        urlImage = brg.skus[i].Images[0];
                    if (brg.skus[i].Images[1] != null)
                        urlImage2 = brg.skus[i].Images[1];
                    if (brg.skus[i].Images[2] != null)
                        urlImage3 = brg.skus[i].Images[2];
                }

                var brgAttribute = new Dictionary<string, string>();
                var brgSku = new Dictionary<string, string>();
                foreach (Newtonsoft.Json.Linq.JProperty property in brg.attributes)
                {
                    brgAttribute.Add(property.Name, property.Value.ToString());
                }
                foreach (Newtonsoft.Json.Linq.JProperty property in brg.skus[i])
                {
                    brgSku.Add(property.Name, property.Value.ToString());
                }
                string value;
                var statusBrg = (brgSku.TryGetValue("Status", out value) ? value : "");
                var display = statusBrg.Equals("active") ? 1 : 0;
                string deskripsi = brg.attributes.description;
                string s_deskripsi = (brgAttribute.TryGetValue("short_description", out value) ? value : "").ToString().Replace("<br/>", "\r\n").Replace("<br />", "\r\n").Replace('\'', '`');
                //remark by Tri, length avalue_1 sudah diubah 250 -> max
                //if (s_deskripsi.Length > 250)
                //    s_deskripsi = s_deskripsi.Substring(0, 250);
                //end remark by Tri, length avalue_1 sudah diubah 250 -> max
                sSQL_Value += Convert.ToDouble(brg.skus[i].package_weight) * 1000 + " , " + brg.skus[i].package_length + " , " + brg.skus[i].package_width + " , " + brg.skus[i].package_height + " , '" + cust + "' , '";
                sSQL_Value += string.IsNullOrEmpty(deskripsi) ? "" : brg.attributes.description.ToString().Replace("<br/>", "\r\n").Replace("<br />", "\r\n").Replace('\'', '`');
                sSQL_Value += "' , " + IdMarket + " , " + brg.skus[i].price + " , " + brg.skus[i].price + " , ";
                sSQL_Value += display + " , '" + categoryCode + "' , '" + MoDbContext.CATEGORY_LAZADA.Where(c => c.CATEGORY_ID.Equals(categoryCode)).FirstOrDefault().NAME + "' , '";
                sSQL_Value += brg.attributes.brand + "' , '" + urlImage + "' , '" + urlImage2 + "' , '" + urlImage3 + "' , '" + (typeBrg == 2 ? kodeBrgInduk : "") + "' , '" + (typeBrg == 1 ? "4" : "3") + "'";
                sSQL_Value += ",'" + brg.skus[i].SkuId + "','" + brg.item_id + "'";
                var attributeLzd = MoDbContext.ATTRIBUTE_LAZADA.Where(a => a.CATEGORY_CODE.Equals(categoryCode)).FirstOrDefault();
                //bool getAttr = true;
                if (attributeLzd == null)
                {
                    var retAPI = getMissingAttr(categoryCode);
                    if (retAPI.status == 1)
                    {
                        attributeLzd = MoDbContext.ATTRIBUTE_LAZADA.AsNoTracking().Where(a => a.CATEGORY_CODE.Equals(categoryCode)).FirstOrDefault();
                    }
                }
                #region set attribute

                if (attributeLzd != null)
                //if (getAttr)
                {

                    if (!string.IsNullOrEmpty(attributeLzd.ANAME1))
                    {
                        if (attributeLzd.ANAME1.Equals("short_description"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME1 + "' , '" + attributeLzd.ALABEL1.Replace("\'", "\'\'") + "' , '" + s_deskripsi + "'";
                        }
                        else
                        {
                            if (attributeLzd.ATYPE1.Equals("sku"))
                            {
                                sSQL_Value += ", '" + attributeLzd.ANAME1 + "' , '" + attributeLzd.ALABEL1.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME1, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                            }
                            else
                            {
                                sSQL_Value += ", '" + attributeLzd.ANAME1 + "' , '" + attributeLzd.ALABEL1.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME1, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                            }
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME2))
                    {
                        if (attributeLzd.ATYPE2.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME2 + "' , '" + attributeLzd.ALABEL2.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME2, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME2 + "' , '" + attributeLzd.ALABEL2.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME2, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME3))
                    {
                        if (attributeLzd.ATYPE3.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME3 + "' , '" + attributeLzd.ALABEL3.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME3, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME3 + "' , '" + attributeLzd.ALABEL3.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME3, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME4))
                    {
                        if (attributeLzd.ATYPE4.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME4 + "' , '" + attributeLzd.ALABEL4.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME4, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME4 + "' , '" + attributeLzd.ALABEL4.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME4, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME5))
                    {
                        if (attributeLzd.ATYPE5.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME5 + "' , '" + attributeLzd.ALABEL5.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME5, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME5 + "' , '" + attributeLzd.ALABEL5.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME5, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME6))
                    {
                        if (attributeLzd.ATYPE6.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME6 + "' , '" + attributeLzd.ALABEL6.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME6, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME6 + "' , '" + attributeLzd.ALABEL6.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME6, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME7))
                    {
                        if (attributeLzd.ATYPE7.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME7 + "' , '" + attributeLzd.ALABEL7.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME7, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME7 + "' , '" + attributeLzd.ALABEL7.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME7, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME8))
                    {
                        if (attributeLzd.ATYPE8.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME8 + "' , '" + attributeLzd.ALABEL8.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME8, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME8 + "' , '" + attributeLzd.ALABEL8.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME8, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME9))
                    {
                        if (attributeLzd.ATYPE9.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME9 + "' , '" + attributeLzd.ALABEL9.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME9, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME9 + "' , '" + attributeLzd.ALABEL9.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME9, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME10))
                    {
                        if (attributeLzd.ATYPE10.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME10 + "' , '" + attributeLzd.ALABEL10.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME10, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME10 + "' , '" + attributeLzd.ALABEL10.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME10, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME11))
                    {
                        if (attributeLzd.ATYPE11.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME11 + "' , '" + attributeLzd.ALABEL11.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME11, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME11 + "' , '" + attributeLzd.ALABEL11.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME11, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME12))
                    {
                        if (attributeLzd.ATYPE12.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME12 + "' , '" + attributeLzd.ALABEL12.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME12, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME12 + "' , '" + attributeLzd.ALABEL12.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME12, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME13))
                    {
                        if (attributeLzd.ATYPE13.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME13 + "' , '" + attributeLzd.ALABEL13.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME13, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME13 + "' , '" + attributeLzd.ALABEL13.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME13, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME14))
                    {
                        if (attributeLzd.ATYPE14.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME14 + "' , '" + attributeLzd.ALABEL14.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME14, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME14 + "' , '" + attributeLzd.ALABEL14.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME14, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME15))
                    {
                        if (attributeLzd.ATYPE15.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME15 + "' , '" + attributeLzd.ALABEL15.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME15, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME15 + "' , '" + attributeLzd.ALABEL15.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME15, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME16))
                    {
                        if (attributeLzd.ATYPE16.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME16 + "' , '" + attributeLzd.ALABEL16.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME16, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME16 + "' , '" + attributeLzd.ALABEL16.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME16, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME17))
                    {
                        if (attributeLzd.ATYPE17.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME17 + "' , '" + attributeLzd.ALABEL17.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME17, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME17 + "' , '" + attributeLzd.ALABEL17.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME17, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME18))
                    {
                        if (attributeLzd.ATYPE18.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME18 + "' , '" + attributeLzd.ALABEL18.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME18, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME18 + "' , '" + attributeLzd.ALABEL18.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME18, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME19))
                    {
                        if (attributeLzd.ATYPE19.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME19 + "' , '" + attributeLzd.ALABEL19.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME19, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME19 + "' , '" + attributeLzd.ALABEL19.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME19, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME20))
                    {
                        if (attributeLzd.ATYPE20.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME20 + "' , '" + attributeLzd.ALABEL20.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME20, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME20 + "' , '" + attributeLzd.ALABEL20.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME20, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME21))
                    {
                        if (attributeLzd.ATYPE21.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME21 + "' , '" + attributeLzd.ALABEL21.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME21, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME21 + "' , '" + attributeLzd.ALABEL21.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME21, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME22))
                    {
                        if (attributeLzd.ATYPE22.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME22 + "' , '" + attributeLzd.ALABEL22.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME22, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME22 + "' , '" + attributeLzd.ALABEL22.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME22, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME23))
                    {
                        if (attributeLzd.ATYPE23.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME23 + "' , '" + attributeLzd.ALABEL23.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME23, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME23 + "' , '" + attributeLzd.ALABEL23.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME23, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME24))
                    {
                        if (attributeLzd.ATYPE24.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME24 + "' , '" + attributeLzd.ALABEL24.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME24, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME24 + "' , '" + attributeLzd.ALABEL24.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME24, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME25))
                    {
                        if (attributeLzd.ATYPE25.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME25 + "' , '" + attributeLzd.ALABEL25.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME25, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME25 + "' , '" + attributeLzd.ALABEL25.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME25, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME26))
                    {
                        if (attributeLzd.ATYPE26.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME26 + "' , '" + attributeLzd.ALABEL26.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME26, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME26 + "' , '" + attributeLzd.ALABEL26.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME26, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME27))
                    {
                        if (attributeLzd.ATYPE27.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME27 + "' , '" + attributeLzd.ALABEL27.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME27, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME27 + "' , '" + attributeLzd.ALABEL27.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME27, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME28))
                    {
                        if (attributeLzd.ATYPE28.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME28 + "' , '" + attributeLzd.ALABEL28.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME28, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME28 + "' , '" + attributeLzd.ALABEL28.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME28, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME29))
                    {
                        if (attributeLzd.ATYPE29.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME29 + "' , '" + attributeLzd.ALABEL29.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME29, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME29 + "' , '" + attributeLzd.ALABEL29.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME29, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME30))
                    {
                        if (attributeLzd.ATYPE30.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME30 + "' , '" + attributeLzd.ALABEL30.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME30, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME30 + "' , '" + attributeLzd.ALABEL30.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME30, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME31))
                    {
                        if (attributeLzd.ATYPE31.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME31 + "' , '" + attributeLzd.ALABEL31.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME31, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME31 + "' , '" + attributeLzd.ALABEL31.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME31, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME32))
                    {
                        if (attributeLzd.ATYPE32.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME32 + "' , '" + attributeLzd.ALABEL32.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME32, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME32 + "' , '" + attributeLzd.ALABEL32.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME32, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME33))
                    {
                        if (attributeLzd.ATYPE33.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME33 + "' , '" + attributeLzd.ALABEL33.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME33, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME33 + "' , '" + attributeLzd.ALABEL33.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME33, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME34))
                    {
                        if (attributeLzd.ATYPE34.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME34 + "' , '" + attributeLzd.ALABEL34.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME34, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME34 + "' , '" + attributeLzd.ALABEL34.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME34, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME35))
                    {
                        if (attributeLzd.ATYPE35.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME35 + "' , '" + attributeLzd.ALABEL35.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME35, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME35 + "' , '" + attributeLzd.ALABEL35.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME35, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME36))
                    {
                        if (attributeLzd.ATYPE36.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME36 + "' , '" + attributeLzd.ALABEL36.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME36, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME36 + "' , '" + attributeLzd.ALABEL36.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME36, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME37))
                    {
                        if (attributeLzd.ATYPE37.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME37 + "' , '" + attributeLzd.ALABEL37.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME37, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME37 + "' , '" + attributeLzd.ALABEL37.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME37, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME38))
                    {
                        if (attributeLzd.ATYPE38.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME38 + "' , '" + attributeLzd.ALABEL38.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME38, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME38 + "' , '" + attributeLzd.ALABEL38.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME38, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME39))
                    {
                        if (attributeLzd.ATYPE39.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME39 + "' , '" + attributeLzd.ALABEL39.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME39, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME39 + "' , '" + attributeLzd.ALABEL39.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME39, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME40))
                    {
                        if (attributeLzd.ATYPE40.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME40 + "' , '" + attributeLzd.ALABEL40.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME40, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME40 + "' , '" + attributeLzd.ALABEL40.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME40, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME41))
                    {
                        if (attributeLzd.ATYPE41.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME41 + "' , '" + attributeLzd.ALABEL41.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME41, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME41 + "' , '" + attributeLzd.ALABEL41.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME41, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME42))
                    {
                        if (attributeLzd.ATYPE42.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME42 + "' , '" + attributeLzd.ALABEL42.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME42, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME42 + "' , '" + attributeLzd.ALABEL42.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME42, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME43))
                    {
                        if (attributeLzd.ATYPE43.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME43 + "' , '" + attributeLzd.ALABEL43.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME43, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME43 + "' , '" + attributeLzd.ALABEL43.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME43, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME44))
                    {
                        if (attributeLzd.ATYPE44.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME44 + "' , '" + attributeLzd.ALABEL44.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME44, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME44 + "' , '" + attributeLzd.ALABEL44.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME44, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME45))
                    {
                        if (attributeLzd.ATYPE45.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME45 + "' , '" + attributeLzd.ALABEL45.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME45, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME45 + "' , '" + attributeLzd.ALABEL45.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME45, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        //change 19 maret 2019 request by Calvin, isi nama barang jika attribute 45 kosong
                        //sSQL_Value += ", '', '', ''";
                        sSQL_Value += ", '', '', '" + namaBrg + "'";
                        //end change 19 maret 2019 request by Calvin, isi nama barang jika attribute 45 kosong
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME46))
                    {
                        if (attributeLzd.ATYPE46.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME46 + "' , '" + attributeLzd.ALABEL46.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME46, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME46 + "' , '" + attributeLzd.ALABEL46.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME46, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME47))
                    {
                        if (attributeLzd.ATYPE47.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME47 + "' , '" + attributeLzd.ALABEL47.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME47, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME47 + "' , '" + attributeLzd.ALABEL47.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME47, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME48))
                    {
                        if (attributeLzd.ATYPE48.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME48 + "' , '" + attributeLzd.ALABEL48.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME48, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME48 + "' , '" + attributeLzd.ALABEL48.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME48, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME49))
                    {
                        if (attributeLzd.ATYPE49.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME49 + "' , '" + attributeLzd.ALABEL49.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME49, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME49 + "' , '" + attributeLzd.ALABEL49.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME49, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(attributeLzd.ANAME50))
                    {
                        if (attributeLzd.ATYPE50.Equals("sku"))
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME50 + "' , '" + attributeLzd.ALABEL50.Replace("\'", "\'\'") + "' , '" + (brgSku.TryGetValue(attributeLzd.ANAME50, out value) ? value.Replace("\'", "\'\'") : "") + "') ,";
                        }
                        else
                        {
                            sSQL_Value += ", '" + attributeLzd.ANAME50 + "' , '" + attributeLzd.ALABEL50.Replace("\'", "\'\'") + "' , '" + (brgAttribute.TryGetValue(attributeLzd.ANAME50, out value) ? value.Replace("\'", "\'\'") : "") + "') ,";
                        }
                    }
                    else
                    {
                        sSQL_Value += ", '', '', '') ,";
                    }
                }
                else//fail to get lazada attribute from db and API
                {
                    sSQL_Value += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', ''";
                    sSQL_Value += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', ''";
                    sSQL_Value += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', ''";
                    sSQL_Value += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', ''";
                    sSQL_Value += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '')";
                }

                #endregion
                ret.status = 1;
                ret.message = sSQL_Value;
            }
            catch (Exception ex)
            {
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }
            return ret;
        }
        public ATTRIBUTE_LAZADA getAttrLzd(string code)
        {
            var retAttr = new ATTRIBUTE_LAZADA();

            var tbl = MoDbContext.CATEGORY_LAZADA.Where(c => c.CATEGORY_ID == code).FirstOrDefault();
            if (tbl != null)
            {
                ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
                LazopRequest request = new LazopRequest();
                request.SetApiName("/category/attributes/get");
                request.SetHttpMethod("GET");
                request.AddApiParameter("primary_category_id", code);
                LazopResponse response = client.Execute(request);
                //Console.WriteLine(response.IsError());
                //Console.WriteLine(response.Body);
                var bindAttr = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(AttributeBody)) as AttributeBody;
                if (bindAttr != null)
                {
                    if (bindAttr.code == "0")
                    {
                        try
                        {
                            retAttr.CATEGORY_CODE = code;
                            int i = 1;
                            foreach (var attr in bindAttr.data)
                            {
                                if (attr.name != "name" && attr.name != "description" && attr.name != "brand" && attr.name != "SellerSku" && attr.name != "price"
                                    && attr.name != "package_weight" && attr.name != "package_length" && attr.name != "package_width" && attr.name != "package_height"
                                    && attr.name != "__images__" && attr.name != "color_thumbnail" && attr.name != "special_price" && attr.name != "special_from_date"
                                    && attr.name != "special_to_date" && attr.name != "seller_promotion" && attr.name != "tax_class")
                                {
                                    retAttr["ALABEL" + i] = attr.label;
                                    retAttr["ANAME" + i] = attr.name;
                                    retAttr["ATYPE" + i] = attr.attribute_type;
                                    retAttr["AINPUT_TYPE" + i] = attr.input_type;
                                    retAttr["ASALE_PROP" + i] = attr.is_sale_prop.ToString();
                                    retAttr["AMANDATORY" + i] = attr.is_mandatory.ToString();
                                    i++;
                                }

                            }
                            for (int j = i; j <= 50; j++)
                            {
                                retAttr["ALABEL" + j] = "";
                                retAttr["ANAME" + j] = "";
                                retAttr["ATYPE" + j] = "";
                                retAttr["AINPUT_TYPE" + j] = "";
                                retAttr["ASALE_PROP" + j] = "0";
                                retAttr["AMANDATORY" + j] = "0";
                            }
                        }
                        catch (Exception ex)
                        {

                        }

                    }
                }
            }

            return retAttr;
        }

        public BindingBase getMissingAttr(string code)
        {
            var ret = new BindingBase();
            ret.status = 0;

            var tbl = MoDbContext.CATEGORY_LAZADA.Where(c => c.CATEGORY_ID == code).FirstOrDefault();
            if (tbl != null)
            {
                ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
                LazopRequest request = new LazopRequest();
                request.SetApiName("/category/attributes/get");
                request.SetHttpMethod("GET");
                request.AddApiParameter("primary_category_id", code);
                LazopResponse response = client.Execute(request);
                //Console.WriteLine(response.IsError());
                //Console.WriteLine(response.Body);
                var bindAttr = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(AttributeBody)) as AttributeBody;
                if (bindAttr != null)
                {
                    if (bindAttr.code == "0")
                    {
                        try
                        {
#if AWS
                    string con = "Data Source=localhost;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#elif Debug_AWS
                            string con = "Data Source=13.250.232.74;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#else
                            string con = "Data Source=13.251.222.53;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#endif
                            using (SqlConnection oConnection = new SqlConnection(con))
                            {

                                oConnection.Open();
                                using (SqlCommand oCommand = oConnection.CreateCommand())
                                {
                                    oCommand.CommandType = CommandType.Text;
                                    oCommand.Parameters.Add(new SqlParameter("@CATEGORY_CODE", SqlDbType.VarChar, 50));
                                    oCommand.Parameters.Add(new SqlParameter("@CATEGORY_NAME", SqlDbType.NVarChar, 150));
                                    string sSQL = "INSERT INTO ATTRIBUTE_LAZADA ([CATEGORY_CODE], [CATEGORY_NAME],";
                                    string sSQLValue = ") VALUES (@CATEGORY_CODE, @CATEGORY_NAME,";
                                    string a = "";
                                    #region Generate Parameters dan CommandText
                                    for (int j = 1; j <= 50; j++)
                                    {
                                        a = Convert.ToString(j);
                                        sSQL += "[ALABEL" + a + "],[ANAME" + a + "],[ATYPE" + a + "],[AINPUT_TYPE" + a + "],[ASALE_PROP" + a + "],[AMANDATORY" + a + "],";
                                        sSQLValue += "@ALABEL" + a + ",@ANAME" + a + ",@ATYPE" + a + ",@AINPUT_TYPE" + a + ",@ASALE_PROP" + a + ",@AMANDATORY" + a + ",";
                                        oCommand.Parameters.Add(new SqlParameter("@ALABEL" + a, SqlDbType.NVarChar, 50));
                                        oCommand.Parameters.Add(new SqlParameter("@ANAME" + a, SqlDbType.NVarChar, 50));
                                        oCommand.Parameters.Add(new SqlParameter("@ATYPE" + a, SqlDbType.NVarChar, 50));
                                        oCommand.Parameters.Add(new SqlParameter("@AINPUT_TYPE" + a, SqlDbType.NVarChar, 50));
                                        oCommand.Parameters.Add(new SqlParameter("@ASALE_PROP" + a, SqlDbType.VarChar, 1));
                                        oCommand.Parameters.Add(new SqlParameter("@AMANDATORY" + a, SqlDbType.VarChar, 1));
                                    }
                                    sSQL = sSQL.Substring(0, sSQL.Length - 1) + sSQLValue.Substring(0, sSQLValue.Length - 1) + ")";
                                    #endregion
                                    oCommand.CommandText = sSQL;
                                    oCommand.Parameters[0].Value = tbl.CATEGORY_ID;
                                    oCommand.Parameters[1].Value = tbl.NAME;
                                    int i = 0;

                                    //for (int i = 0; i < 30; i++)
                                    foreach (var attr in bindAttr.data)
                                    {
                                        //a = Convert.ToString(i * 4 + 2);
                                        //if (i < bindData.data.Count())
                                        //{
                                        //if (ANAME != "__images__" && ANAME != "color_thumbnail" && ANAME != "special_price" && ANAME != "special_from_date" && ANAME != "special_to_date" && ANAME != "seller_promotion" && ANAME != "tax_class") {
                                        if (attr.name.Equals("name") || attr.name.Equals("description") || attr.name.Equals("brand") || attr.name.Equals("SellerSku") || attr.name.Equals("price") || attr.name.Equals("package_weight") || attr.name.Equals("package_length") || attr.name.Equals("package_width") || attr.name.Equals("package_height") || attr.name.Equals("__images__") || attr.name.Equals("color_thumbnail") || attr.name.Equals("special_price") || attr.name.Equals("special_from_date") || attr.name.Equals("special_to_date") || attr.name.Equals("seller_promotion") || attr.name.Equals("tax_class"))
                                        {

                                        }
                                        else
                                        {
                                            oCommand.Parameters[(i * 6) + 2].Value = attr.label;
                                            oCommand.Parameters[(i * 6) + 3].Value = attr.name;
                                            oCommand.Parameters[(i * 6) + 4].Value = attr.attribute_type;
                                            oCommand.Parameters[(i * 6) + 5].Value = attr.input_type;
                                            oCommand.Parameters[(i * 6) + 6].Value = attr.is_sale_prop.ToString();
                                            oCommand.Parameters[(i * 6) + 7].Value = attr.is_mandatory.ToString();
                                            if (attr.options != null)
                                            {
                                                if (attr.options.Count > 0)
                                                {
                                                    //var cekOpt = (from t in MoDbContext.ATTRIBUTE_OPT_LAZADA where t.A_NAME.Equals(attr.name) && t.CATEGORY_CODE.Equals(tbl.CATEGORY_ID) select t).ToList();
                                                    var cekOpt = MoDbContext.ATTRIBUTE_OPT_LAZADA.Where(t => t.A_NAME.ToUpper() == attr.name.ToUpper() && t.CATEGORY_CODE == tbl.CATEGORY_ID).ToList();
                                                    if (cekOpt.Count == 0)
                                                    {
                                                        //string sSQL2 = "INSERT INTO ATTRIBUTE_OPT_LAZADA ([A_NAME] , [O_NAME]) VALUES ";
                                                        //string sSQL2Value = "";
                                                        foreach (var opt in attr.options)
                                                        {
                                                            //if (cekOpt.Where(t => t.O_NAME.Equals(opt.name)).FirstOrDefault() == null)
                                                            //{
                                                            var newOpt = new ATTRIBUTE_OPT_LAZADA();
                                                            newOpt.CATEGORY_CODE = tbl.CATEGORY_ID;
                                                            newOpt.A_NAME = attr.name;
                                                            newOpt.O_NAME = opt.name;

                                                            MoDbContext.ATTRIBUTE_OPT_LAZADA.Add(newOpt);
                                                            MoDbContext.SaveChanges();

                                                        }

                                                    }
                                                }
                                            }
                                            i++;

                                        }

                                    }
                                    for (int k = i; k < 50; k++)
                                    {
                                        oCommand.Parameters[(k * 6) + 2].Value = "";
                                        oCommand.Parameters[(k * 6) + 3].Value = "";
                                        oCommand.Parameters[(k * 6) + 4].Value = "";
                                        oCommand.Parameters[(k * 6) + 5].Value = "";
                                        oCommand.Parameters[(k * 6) + 6].Value = "0";
                                        oCommand.Parameters[(k * 6) + 7].Value = "0";
                                    }
                                    oCommand.ExecuteNonQuery();

                                }

                            }
                            ret.status = 1;
                        }
                        catch (Exception ex)
                        {

                        }

                    }
                }
            }

            return ret;
        }

        public enum api_status
        {
            Pending = 1,
            Success = 2,
            Failed = 3,
            Exception = 4
        }
        public void manageAPI_LOG_MARKETPLACE(api_status action, ErasoftContext db, string iden, API_LOG_MARKETPLACE data)
        {
            switch (action)
            {
                case api_status.Pending:
                    {
                        var arf01 = ErasoftDbContext.ARF01.Where(p => p.TOKEN == iden).FirstOrDefault();
                        var apiLog = new MasterOnline.API_LOG_MARKETPLACE
                        {
                            CUST = arf01 != null ? arf01.CUST : "",
                            CUST_ATTRIBUTE_1 = arf01 != null ? arf01.PERSO : "",
                            CUST_ATTRIBUTE_2 = data.CUST_ATTRIBUTE_2 != null ? data.CUST_ATTRIBUTE_2 : "",
                            CUST_ATTRIBUTE_3 = data.CUST_ATTRIBUTE_3 != null ? data.CUST_ATTRIBUTE_3 : "",
                            CUST_ATTRIBUTE_4 = data.CUST_ATTRIBUTE_4 != null ? data.CUST_ATTRIBUTE_4 : "",
                            CUST_ATTRIBUTE_5 = data.CUST_ATTRIBUTE_5 != null ? data.CUST_ATTRIBUTE_5 : "",
                            MARKETPLACE = "Lazada",
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
                        var apiLogInDb = ErasoftDbContext.API_LOG_MARKETPLACE.FirstOrDefault(p => p.REQUEST_ID == data.REQUEST_ID);
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
