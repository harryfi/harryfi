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

namespace MasterOnline.Controllers
{
    public class LazadaController : Controller
    {
        AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        string urlLazada = "https://api.lazada.co.id/rest";
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

        public LazadaController()
        {
            MoDbContext = new MoDbContext();
            if (sessionData?.Account != null)
            {
                if (sessionData.Account.UserId == "admin_manage")
                    ErasoftDbContext = new ErasoftContext();
                else
                    ErasoftDbContext = new ErasoftContext(sessionData.Account.DatabasePathErasoft);
                EDB = new DatabaseSQL(sessionData.Account.DatabasePathErasoft);

            }
            else
            {
                if (sessionData?.User != null)
                {
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                    EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
                    ErasoftDbContext = new ErasoftContext(accFromUser.DatabasePathErasoft);
                }
            }
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

        [HttpGet]
        public string LazadaUrl(string cust)
        {
            string userId = "";
            if (sessionData?.Account != null)
            {
                userId = sessionData.Account.DatabasePathErasoft;

            }
            else
            {
                if (sessionData?.User != null)
                {
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                    userId = accFromUser.DatabasePathErasoft;
                }
            }

            string lzdId = cust;
            string compUrl = eraCallbackUrl + userId + "_param_" + lzdId;
            string uri = "https://auth.lazada.com/oauth/authorize?response_type=code&force_auth=true&redirect_uri=" + compUrl + "&client_id=" + eraAppKey + "&country=id";
            return uri;
        }

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

            ErasoftDbContext = new ErasoftContext(user);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Get Token",
                REQUEST_DATETIME = DateTime.Now,
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
                    var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET TOKEN = '" + bindAuth.access_token + "', REFRESH_TOKEN = '" + bindAuth.refresh_token + "', STATUS_API = '1' WHERE CUST = '" + cust + "'");
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

        public LazadaAuth GetRefToken(string cust, string refreshToken)
        {
            LazadaAuth ret = new LazadaAuth();
            string url;
            url = "https://auth.lazada.com/rest";
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
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Refresh Token",
                REQUEST_DATETIME = DateTime.Now,
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
                    //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
                    var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET TOKEN = '" + ret.access_token + "', REFRESH_TOKEN = '" + ret.refresh_token + "', STATUS_API = '1'  WHERE CUST = '" + cust + "'");
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
            return ret;
        }

        public BindingBase CreateProduct(BrgViewModel data)
        {
            var ret = new BindingBase();
            ret.status = 0;


            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Create Product",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = data.kdBrg,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data.token, currentLog);

            string sSQL = "SELECT * FROM (";
            for (int i = 1; i <= 50; i++)
            {
                sSQL += "SELECT A.ACODE_" + i.ToString() + " AS CATEGORY_CODE,A.ANAME_" + i.ToString() + " AS CATEGORY_NAME,B.ATYPE" + i.ToString();
                sSQL += " AS CATEGORY_TYPE,A.AVALUE_" + i.ToString() + " AS VALUE FROM STF02H A INNER JOIN MO.DBO.ATTRIBUTE_LAZADA B ON A.CATEGORY_CODE = B.CATEGORY_CODE WHERE A.BRG='" + data.kdBrg + "' " + System.Environment.NewLine;
                if (i < 50)
                {
                    sSQL += "UNION ALL " + System.Environment.NewLine;
                }
            }

            DataSet dsSku = EDB.GetDataSet("sCon", "STF02H", sSQL + ") ASD WHERE ISNULL(CATEGORY_CODE,'') <> '' AND CATEGORY_TYPE <> 'normal' AND ISNULL(VALUE, 'NULL') <> 'NULL' ");
            DataSet dsNormal = EDB.GetDataSet("sCon", "STF02H", sSQL + ") ASD WHERE ISNULL(CATEGORY_CODE,'') <> '' AND CATEGORY_TYPE = 'normal' AND ISNULL(VALUE, 'NULL') <> 'NULL' ");

            string primCategory = EDB.GetFieldValue("MOConnectionString", "STF02H", "BRG = '" + data.kdBrg + "' AND IDMARKET = '" + data.idMarket + "'", "category_code").ToString();
            string xmlString = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>";
            xmlString = "<Request><Product><PrimaryCategory>" + primCategory + "</PrimaryCategory>";
            xmlString += "<Attributes><name>" + data.nama + (string.IsNullOrEmpty(data.nama2) ? "" : " " + data.nama2) + "</name>";
            //xmlString += "<short_description><![CDATA[" + data.deskripsi + "]]></short_description>";
            xmlString += "<description><![CDATA[" + data.deskripsi.Replace(System.Environment.NewLine, "<br>") + "]]></description>";
            xmlString += "<brand>No Brand</brand>";
            //xmlString += "<model>" + data.kdBrg + "</model>";
            //xmlString += "<warranty_type>No Warranty</warranty_type>";

            for (int i = 0; i < dsNormal.Tables[0].Rows.Count; i++)
            {
                xmlString += "<" + dsNormal.Tables[0].Rows[i]["CATEGORY_CODE"].ToString() + ">";
                xmlString += dsNormal.Tables[0].Rows[i]["VALUE"].ToString();
                xmlString += "</" + dsNormal.Tables[0].Rows[i]["CATEGORY_CODE"].ToString() + ">";
            }

            xmlString += "</Attributes>";
            xmlString += "<Skus><Sku><SellerSku>" + data.kdBrg + "</SellerSku>";
            xmlString += "<active>" + (data.activeProd ? "true" : "false") + "</active>";
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

            for (int i = 0; i < dsSku.Tables[0].Rows.Count; i++)
            {
                xmlString += "<" + dsSku.Tables[0].Rows[i]["CATEGORY_CODE"].ToString() + ">";
                xmlString += dsSku.Tables[0].Rows[i]["VALUE"].ToString();
                xmlString += "</" + dsSku.Tables[0].Rows[i]["CATEGORY_CODE"].ToString() + ">";
            }

            xmlString += "</Sku></Skus></Product></Request>";

            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/product/create");
            request.AddApiParameter("payload", xmlString);

            //LazopResponse response = client.Execute(request, data.token);
            try
            {
                LazopResponse response = client.Execute(request, data.token);

                var res = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaResponseObj)) as LazadaResponseObj;
                if (res.code.Equals("0"))
                {
                    ret.status = 1;
                    //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
                    var result = EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + data.kdBrg + "' WHERE BRG = '" + data.kdBrg + "' AND IDMARKET = '" + data.idMarket + "'");
                    if (result == 1)
                    {
                        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data.key, currentLog);
                    }
                    else
                    {
                        currentLog.REQUEST_EXCEPTION = "failed to update brg_mp;execute result=" + result;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data.token, currentLog);
                    }
                }
                else
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

                    currentLog.REQUEST_EXCEPTION = ret.message;
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

        public BindingBase setDisplay(string kdBrg, bool display, string token)
        {
            var ret = new BindingBase();
            ret.status = 0;

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Show Product",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = kdBrg,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, token, currentLog);

            string xmlString = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>";
            xmlString += "<Request><Product><Skus><Sku>";
            xmlString += "<SellerSku>" + kdBrg + "</SellerSku>";
            xmlString += "<active>" + (display ? "true" : "false") + "</active>";
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
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Set Promo",
                REQUEST_DATETIME = DateTime.Now,
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
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Update Price/Stok Product",
                REQUEST_DATETIME = DateTime.Now,
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
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Update Promo Product",
                REQUEST_DATETIME = DateTime.Now,
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
            xmlString += "<SaleStartDate>" + SaleStartDate.ToString("yyyy-MM-dd") + "</SaleStartDate>";
            xmlString += "<SaleEndDate>" + SaleEndDate.ToString("yyyy-MM-dd") + "</SaleEndDate>";
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

        public BindingBase GetShipment(string cust, string accessToken)
        {
            var ret = new BindingBase();
            ret.status = 0;

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Get Shipment Provider",
                REQUEST_DATETIME = DateTime.Now,
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

        public LazadaToDeliver GetToPacked(List<string> orderItemId, string shippingProvider, string accessToken)
        {
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

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Set Status Order to Packed",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = ordItems,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, accessToken, currentLog);

            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/order/pack");
            request.AddApiParameter("shipping_provider", shippingProvider);
            request.AddApiParameter("delivery_type", "dropship");
            request.AddApiParameter("order_item_ids", "[" + ordItems + "]");
            try
            {
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
                            ErasoftDbContext.SaveChanges();
                        }
                    }
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, accessToken, currentLog);
                }
                else
                {
                    currentLog.REQUEST_EXCEPTION = ret.message;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, accessToken, currentLog);
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

        public LazadaToDeliver GetToDeliver(List<string> orderItemId, string shippingProvider, string trackingNumber, string accessToken)
        {
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

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Set Status Order to Deliver",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = ordItems,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, accessToken, currentLog);

            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/order/rts");
            request.AddApiParameter("shipping_provider", shippingProvider);
            request.AddApiParameter("delivery_type", "dropship");
            request.AddApiParameter("order_item_ids", "[" + ordItems + "]");
            request.AddApiParameter("tracking_number", trackingNumber);
            try
            {
                LazopResponse response = client.Execute(request, accessToken);
                ret = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaToDeliver)) as LazadaToDeliver;
                if (ret.code.Equals("0"))
                {
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, accessToken, currentLog);
                }
                else
                {
                    currentLog.REQUEST_EXCEPTION = ret.message;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, accessToken, currentLog);
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

        public BindingBase SetStatusToCanceled(string orderItemId, string accessToken)
        {
            var ret = new BindingBase();
            ret.status = 0;

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Set Status Order to Cancel",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = orderItemId,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, accessToken, currentLog);

            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/order/cancel");
            request.AddApiParameter("reason_detail", "Out of stock");
            request.AddApiParameter("reason_id", "15");
            request.AddApiParameter("order_item_id", orderItemId);
            try
            {
                LazopResponse response = client.Execute(request, accessToken);
                //ret = tgl + ":" + param;
                var resCancel = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaCancelOrder)) as LazadaCancelOrder;
                if (resCancel != null)
                {
                    if (resCancel.code.Equals("0"))
                    {
                        ret.status = 1;
                        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, accessToken, currentLog);
                    }
                    else
                    {
                        currentLog.REQUEST_EXCEPTION = ret.message;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, accessToken, currentLog);
                    }
                }
            }
            catch (Exception ex)
            {
                ret.message = ex.Message;
                currentLog.REQUEST_EXCEPTION = ex.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, accessToken, currentLog);
            }

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
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Upload Image Product",
                REQUEST_DATETIME = DateTime.Now,
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
            System.IO.Stream stream = req.GetResponse().GetResponseStream();
            request.AddFileParameter("image", new FileItem("image", stream));
            //end change by calvin 19 nov 2018
            try
            {
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

        public BindingBase GetOrders(string cust, string accessToken, string connectionID)
        {
            var ret = new BindingBase();
            ret.status = 0;

            var fromDt = DateTime.Now.AddDays(-14);
            var toDt = DateTime.Now.AddDays(1);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Get Order",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = connectionID,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, accessToken, currentLog);

            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/orders/get");
            request.SetHttpMethod("GET");
            request.AddApiParameter("created_before", toDt.ToString("yyyy-MM-ddTHH:mm:ss") + "+07:00");
            request.AddApiParameter("created_after", fromDt.ToString("yyyy-MM-ddTHH:mm:ss") + "+07:00");
            request.AddApiParameter("sort_direction", "DESC");
            request.AddApiParameter("offset", "0");
            request.AddApiParameter("limit", "100");
            request.AddApiParameter("sort_by", "updated_at");
            try
            {
                LazopResponse response = client.Execute(request, accessToken);
                var bindOrder = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(NewLzdOrders)) as NewLzdOrders;
                if (bindOrder != null)
                {
                    //ret = bindOrder;
                    if (bindOrder.code.Equals("0"))
                    {
                        string listOrderId = "[";
                        if (bindOrder.data.orders.Count > 0)
                        {
                            var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == cust).Select(p => p.NO_REFERENSI).ToList();
                            bool adaInsert = false;

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
                            string username = sessionData?.Account != null ? sessionData.Account.Username : sessionData.User.Username;

                            foreach (Order order in bindOrder.data.orders)
                            {
                                bool doInsert = true;
                                if (OrderNoInDb.Contains(Convert.ToString(order.order_id)) && (order.statuses[0].ToString() == "pending" || order.statuses[0].ToString() == "processing"))
                                {
                                    doInsert = false;
                                }
                                //add 19 Feb 2019
                                else if (order.statuses[0].ToString() == "delivered" || order.statuses[0].ToString() == "shipped")
                                {
                                    if (OrderNoInDb.Contains(Convert.ToString(order.order_id)))
                                    {
                                        //tidak ubah status menjadi selesai jika belum diisi faktur
                                        var dsSIT01A = EDB.GetDataSet("CString", "SIT01A", "SELECT NO_REFERENSI, O.NO_BUKTI, O.STATUS_TRANSAKSI FROM SIT01A I INNER JOIN SOT01A O ON I.NO_SO = O.NO_BUKTI WHERE NO_REFERENSI = '" + order.order_id + "'");
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
                                //end add 19 Feb 2019

                                if (doInsert)
                                {
                                    adaInsert = true;
                                    var giftOptionBit = (order.gift_option.Equals("")) ? 1 : 0;
                                    var price = order.price.Split('.');
                                    var statusEra = "";
                                    #region convert status
                                    switch (order.statuses[0].ToString())
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
                                        var currentStatus = EDB.GetFieldValue("", "SOT01A", "NO_REFERENSI = '" + order.order_id + "'", "STATUS_TRANSAKSI").ToString();
                                        if (!string.IsNullOrEmpty(currentStatus))
                                            if (currentStatus == "02" || currentStatus == "03")
                                                statusEra = currentStatus;
                                    }
                                    //end jika status pesanan sudah diubah di mo, dari 01 -> 02/03, status tidak dikembalikan ke 01
                                    #endregion convert status
                                    insertQ += "('" + order.order_id + "','" + order.customer_first_name.Replace('\'', '`') + "','" + order.customer_last_name.Replace('\'', '`') + "','" + order.order_number + "','" + order.payment_method + "','" + order.remarks;
                                    insertQ += "','" + order.delivery_info + "','" + price[0].Replace(",", "") + "'," + giftOptionBit + ",'" + order.gift_message + "','" + order.voucher_code + "','" + order.created_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + order.updated_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + order.address_billing.first_name.Replace('\'', '`') + "','" + order.address_billing.last_name.Replace('\'', '`');
                                    insertQ += "','" + order.address_billing.phone + "','" + order.address_billing.phone2 + "','" + order.address_billing.address1.Replace('\'', '`') + "','" + order.address_billing.address2.Replace('\'', '`') + "','" + order.address_billing.address3.Replace('\'', '`') + "','" + order.address_billing.address4.Replace('\'', '`') + "','" + order.address_billing.address5.Replace('\'', '`');
                                    insertQ += "','" + order.address_billing.customer_email + "','" + order.address_billing.city.Replace('\'', '`') + "','" + order.address_billing.post_code.Replace('\'', '`') + "','" + order.address_billing.country.Replace('\'', '`') + "','" + order.address_shipping.first_name.Replace('\'', '`') + "','" + order.address_shipping.last_name.Replace('\'', '`') + "','" + order.address_shipping.phone + "','" + order.address_shipping.phone2;
                                    insertQ += "','" + order.address_shipping.address1.Replace('\'', '`') + "','" + order.address_shipping.address2.Replace('\'', '`') + "','" + order.address_shipping.address3.Replace('\'', '`') + "','" + order.address_shipping.address4.Replace('\'', '`') + "','" + order.address_shipping.address5.Replace('\'', '`') + "','" + order.address_shipping.customer_email + "','" + order.address_shipping.city.Replace('\'', '`');
                                    insertQ += "','" + order.address_shipping.post_code + "','" + order.address_shipping.country.Replace('\'', '`') + "','" + order.national_registration_number + "'," + order.items_count + ",'" + order.promised_shipping_times + "','" + order.extra_attributes + "','" + statusEra;
                                    insertQ += "'," + order.voucher + "," + order.shipping_fee + ",'" + order.tax_code + "','" + order.branch_number + "','" + cust + "','" + username + "','" + connectionID + "')";

                                    var tblKabKot = EDB.GetDataSet("MOConnectionString", "KabupatenKota", "SELECT TOP 1 * FROM KabupatenKota WHERE NamaKabKot LIKE '%" + order.address_billing.address4 + "%'");
                                    var tblProv = EDB.GetDataSet("MOConnectionString", "Provinsi", "SELECT TOP 1 * FROM Provinsi WHERE NamaProv LIKE '%" + order.address_billing.address5 + "%'");

                                    var kabKot = "3174";//set default value jika tidak ada di db
                                    var prov = "31";//set default value jika tidak ada di db

                                    if (tblProv.Tables[0].Rows.Count > 0)
                                        prov = tblProv.Tables[0].Rows[0]["KodeProv"].ToString();
                                    if (tblKabKot.Tables[0].Rows.Count > 0)
                                        kabKot = tblKabKot.Tables[0].Rows[0]["KodeKabKot"].ToString();

                                    insertPembeli += "('" + order.address_billing.first_name.Replace('\'', '`') + "','" + order.address_billing.address1.Replace('\'', '`') + "','" + order.address_billing.phone + "','" + order.address_billing.customer_email + "',0,0,'0','01',";
                                    insertPembeli += "1, 'IDR', '01', '" + order.address_billing.address1.Replace('\'', '`') + "', 0, 0, 0, 0, '1', 0, 0, ";
                                    //change by calvin 12 desember 2018, ada data dari lazada yang order.address_billing.post_code nya diisi "Bekasi Timur"
                                    //insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + order.address_billing.post_code + "', '" + order.address_billing.customer_email + "', '" + kabKot + "', '" + prov + "', '" + order.address_billing.address4 + "', '" + order.address_billing.address5 + "', '" + connIDARF01C + "')";
                                    insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + order.address_billing.post_code.Substring(0, order.address_billing.post_code.Length > 5 ? 5 : order.address_billing.post_code.Length).Replace('\'', '`') + "', '" + order.address_billing.customer_email + "', '" + kabKot + "', '" + prov + "', '" + order.address_billing.address4.Replace('\'', '`') + "', '" + order.address_billing.address5.Replace('\'', '`') + "', '" + connIDARF01C + "')";
                                    //end change by calvin 12 desember 2018

                                    listOrderId += order.order_id;

                                    insertQ += " , ";
                                    insertPembeli += " , ";

                                    //if (i < bindOrder.data.orders.Count)
                                    //{
                                    listOrderId += ",";
                                    //}
                                    //else
                                    //{
                                    //    listOrderId += "]";
                                    //}
                                    //i = i + 1;
                                }
                            }
                            if (adaInsert)
                            {

                                insertQ = insertQ.Substring(0, insertQ.Length - 2);
                                var a = EDB.ExecuteSQL(username, CommandType.Text, insertQ);

                                insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 2);
                                a = EDB.ExecuteSQL(username, CommandType.Text, insertPembeli);

                                ret.status = 1;
                                //ret.message = a.ToString();

                                SqlCommand CommandSQL = new SqlCommand();

                                CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIDARF01C;
                                EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);

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
                                CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = cust;

                                EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);

                                listOrderId = listOrderId.Substring(0, listOrderId.Length - 1) + "]";
                                getMultiOrderItems(listOrderId, accessToken, connectionID);
                            }
                            manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, accessToken, currentLog);

                        }
                        else
                        {
                            ret.message = "no order";
                        }
                    }
                    else
                    {
                        currentLog.REQUEST_EXCEPTION = bindOrder.message;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, accessToken, currentLog);
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
                currentLog.REQUEST_EXCEPTION = ex.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, accessToken, currentLog);
            }
            return ret;
        }

        public BindingBase getOrderItems(string orderId, string accessToken)
        {
            var ret = new BindingBase();
            ret.status = 0;

            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/order/items/get");
            request.SetHttpMethod("GET");
            request.AddApiParameter("order_id", orderId);
            LazopResponse response = client.Execute(request, accessToken);

            var bindOrderItems = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LzdNewOrderItems)) as LzdNewOrderItems;
            if (bindOrderItems != null)
            {
                if (bindOrderItems.code.Equals("0"))
                {
                    if (bindOrderItems.data.Count > 0)
                    {
                        string insertQ = "INSERT INTO TEMP_LAZADA_GETORDERITEMS ([ORDER_ITEM_ID],[SHOP_ID],[ORDER_ID],[NAME],[SKU],[SHOP_SKU],[SHIPPING_TYPE]";
                        insertQ += ",[ITEM_PRICE],[PAID_PRICE],[CURRENCY],[TAX_AMOUNT],[SHIPPING_AMOUNT],[SHIPPING_SERVICE_COST],[VOUCHER_AMOUNT]";
                        insertQ += ",[STATUS],[SHIPMENT_PROVIDER],[IS_DIGITAL],[TRACKING_CODE],[REASON],[REASON_DETAIL],[PURCHASE_ORDERID]";
                        insertQ += ",[PURCHASE_ORDER_NUM],[PACKAGE_ID],[EXTRA_ATTRIBUTES],[SHIPPING_PROVIDER_TYPE],[CREATED_AT],[UPDATED_AT]";
                        insertQ += ",[RETURN_STATUS],[PRODUCT_MAIN_IMAGE],[VARIATION],[PRODUCT_DETAIL_URL],[INVOICE_NUM],[USERNAME],[CONNECTION_ID]) VALUES ";

                        int i = 1;
                        var connectionID = Guid.NewGuid().ToString();
                        string username = sessionData?.Account != null ? sessionData.Account.Username : sessionData.User.Username;

                        foreach (Orderitem items in bindOrderItems.data)
                        {
                            //var isDigital = (items.IsDigital == 1) ? 1 : 0;
                            var statusEra = "";
                            switch (items.Status.ToString())
                            {
                                case "processing":
                                    statusEra = "01";
                                    break;
                                case "ready_to_ship":
                                    statusEra = "02";
                                    break;
                                case "delivered":
                                    statusEra = "03";
                                    break;
                                case "shipped":
                                    statusEra = "04";
                                    break;
                                case "pending":
                                    statusEra = "05";
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
                            insertQ += "('" + items.OrderItemId + "','" + items.ShopId + "','" + items.OrderId + "','" + items.Name + "','" + items.Sku + "','" + items.ShopSku + "','" + items.ShippingType;
                            insertQ += "'," + items.ItemPrice + "," + items.PaidPrice + ",'" + items.Currency + "'," + items.TaxAmount + "," + items.ShippingAmount + "," + items.ShippingServiceCost + "," + items.VoucherAmount;
                            insertQ += ",'" + statusEra + "','" + items.ShipmentProvider + "'," + items.IsDigital + ",'" + items.TrackingCode + "','" + items.Reason + "','" + items.ReasonDetail + "','" + items.PurchaseOrderId;
                            insertQ += "','" + items.PurchaseOrderNumber + "','" + items.PackageId + "','" + items.ExtraAttributes + "','" + items.ShippingProviderType + "','" + items.CreatedAt + "','" + items.UpdatedAt;
                            insertQ += "','" + items.ReturnStatus + "','" + items.productMainImage + "','" + items.Variation + "','" + items.ProductDetailUrl + "','" + items.invoiceNumber + "','" + username + "','" + connectionID + "')";

                            if (i < bindOrderItems.data.Count)
                                insertQ += " , ";
                            i = i + 1;
                        }
                        var a = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, insertQ);
                        ret.status = 1;
                        ret.message = a.ToString();

                        SqlCommand CommandSQL = new SqlCommand();
                        CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                        CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;
                        CommandSQL.Parameters.Add("@NoBukti", SqlDbType.VarChar).Value = orderId;

                        EDB.ExecuteSQL("MOConnectionString", "MoveOrderItemsFromTempTable", CommandSQL);
                    }
                    else
                    {
                        ret.message = "no item";
                    }
                }
                else
                {
                    ret.message = "lazada api return error";
                    if (!string.IsNullOrEmpty(bindOrderItems.message))
                        ret.message += "\n" + bindOrderItems.message;
                }
            }
            else
            {
                ret.message = "failed to call lazada api";
            }
            return ret;
        }

        public BindingBase getMultiOrderItems(string orderIds, string accessToken, string connectionID)
        {
            var ret = new BindingBase();
            ret.status = 0;

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Get Order Items",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = orderIds,
                REQUEST_ATTRIBUTE_2 = connectionID,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, accessToken, currentLog);

            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/orders/items/get");
            request.SetHttpMethod("GET");
            request.AddApiParameter("order_ids", orderIds);
            try
            {
                LazopResponse response = client.Execute(request, accessToken);

                var bindOrderItems = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaOrderItems)) as LazadaOrderItems;
                if (bindOrderItems != null)
                {
                    if (bindOrderItems.code.Equals("0"))
                    {
                        if (bindOrderItems.data.Count > 0)
                        {
                            string insertQ = "INSERT INTO TEMP_LAZADA_GETORDERITEMS ([ORDER_ITEM_ID],[SHOP_ID],[ORDER_ID],[NAME],[SKU],[SHOP_SKU],[SHIPPING_TYPE]";
                            insertQ += ",[ITEM_PRICE],[PAID_PRICE],[CURRENCY],[TAX_AMOUNT],[SHIPPING_AMOUNT],[SHIPPING_SERVICE_COST],[VOUCHER_AMOUNT]";
                            insertQ += ",[STATUS],[SHIPMENT_PROVIDER],[IS_DIGITAL],[TRACKING_CODE],[REASON],[REASON_DETAIL],[PURCHASE_ORDERID]";
                            insertQ += ",[PURCHASE_ORDER_NUM],[PACKAGE_ID],[EXTRA_ATTRIBUTES],[SHIPPING_PROVIDER_TYPE],[CREATED_AT],[UPDATED_AT]";
                            insertQ += ",[RETURN_STATUS],[PRODUCT_MAIN_IMAGE],[VARIATION],[PRODUCT_DETAIL_URL],[INVOICE_NUM],[USERNAME],[CONNECTION_ID]) VALUES ";
                            string username = sessionData?.Account != null ? sessionData.Account.Username : sessionData.User.Username;

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

                                        insertQ += "('" + items.order_item_id + "','" + items.shop_id + "','" + items.order_id + "','" + items.name.Replace('\'', '`') + "','" + items.sku.Replace('\'', '`') + "','" + items.shop_sku.Replace('\'', '`') + "','" + items.shipping_type;
                                        insertQ += "'," + items.item_price + "," + items.paid_price + ",'" + items.currency + "'," + items.tax_amount + "," + items.shipping_amount + "," + items.shipping_service_cost + "," + items.voucher_amount;
                                        insertQ += ",'" + statusEra + "','" + items.shipment_provider.Replace('\'', '`') + "'," + items.is_digital + ",'" + items.tracking_code + "','" + items.reason.Replace('\'', '`') + "','" + items.reason_detail.Replace('\'', '`') + "','" + items.purchase_order_id;
                                        insertQ += "','" + items.purchase_order_number + "','" + items.package_id + "','" + items.extra_attributes.Replace('\'', '`') + "','" + items.shipping_provider_type + "','" + items.created_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + items.updated_at.ToString("yyyy-MM-dd HH:mm:ss");
                                        insertQ += "','" + items.return_status + "','" + items.product_main_image + "','" + items.variation.Replace('\'', '`') + "','" + items.product_detail_url + "','" + items.invoice_number + "','" + username + "','" + connectionID + "')";

                                        //if (i < bindOrderItems.data.Count)
                                        insertQ += ",";
                                        //i = i + 1;
                                    }

                                }
                            }
                            insertQ = insertQ.Substring(0, insertQ.Length - 1);
                            var a = EDB.ExecuteSQL(username, CommandType.Text, insertQ);

                            SqlCommand CommandSQL = new SqlCommand();
                            CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                            CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;
                            //CommandSQL.Parameters.Add("@NoBukti", SqlDbType.VarChar).Value = orderId;

                            EDB.ExecuteSQL("MOConnectionString", "MoveOrderItemsFromTempTable", CommandSQL);
                            manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, accessToken, currentLog);
                        }
                        else
                        {
                            ret.message = "no item";
                        }
                    }
                    else
                    {
                        currentLog.REQUEST_EXCEPTION = bindOrderItems.message;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, accessToken, currentLog); ret.message = "lazada api return error";
                        if (!string.IsNullOrEmpty(bindOrderItems.message))
                            ret.message += "\n" + bindOrderItems.message;
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
                currentLog.REQUEST_EXCEPTION = ex.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, accessToken, currentLog);
            }
            return ret;
        }
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
            //request.AddApiParameter("sku_seller_list", " [\"OX-292\"]");
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
                            sSQL += "DISPLAY, CATEGORY_CODE, CATEGORY_NAME, MEREK, IMAGE, IMAGE2, IMAGE3, KODE_BRG_INDUK, TYPE,";
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
                                    if (varian && i == 0)
                                    {
                                        kodeBrg = brg.item_id;
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
                                        else if (brgInDB != null)
                                        {
                                            kdBrgInduk = kodeBrg;
                                        }
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
                        sSQL_Value += ", '', '', ''";
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
        public List<ATTRIBUTE_OPT_LAZADA> getAttrLzd(string code, string aCode)
        {
            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/category/attributes/get");
            request.SetHttpMethod("GET");
            request.AddApiParameter("primary_category_id", code);
            LazopResponse response = client.Execute(request);
            if (response != null)
            {
                var bindAttr = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(AttributeBody)) as AttributeBody;
                if (bindAttr.code == "0")
                {
                    var attrBrg = bindAttr.data.Where(m => m.name.ToUpper() == aCode.ToUpper()).SingleOrDefault();
                    var ret = new List<ATTRIBUTE_OPT_LAZADA>();
                    if (attrBrg != null)
                    {
                        foreach (var opt in attrBrg.options)
                        {
                            var optAttrBrg = new ATTRIBUTE_OPT_LAZADA
                            {
                                A_NAME = attrBrg.name,
                                CATEGORY_CODE = code,
                                O_NAME = opt.name,
                            };
                            ret.Add(optAttrBrg);
                        }
                    }

                    return ret;
                }
            }
            return new List<ATTRIBUTE_OPT_LAZADA>();
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