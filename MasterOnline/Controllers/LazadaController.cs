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
                            foreach (Shipment_Providers shipProv in bindDelivery.data.shipment_providers)
                            {
                                if (ErasoftDbContext.DELIVERY_PROVIDER_LAZADA.Where(m => m.CUST.Equals(cust) && m.NAME.Equals(shipProv.name)).ToList().Count == 0)
                                {
                                    var newProvider = new DELIVERY_PROVIDER_LAZADA();
                                    newProvider.CUST = cust;
                                    newProvider.NAME = shipProv.name;
                                    newProvider.COD = shipProv.cod;

                                    ErasoftDbContext.DELIVERY_PROVIDER_LAZADA.Add(newProvider);
                                }
                                ErasoftDbContext.SaveChanges();

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
            request.AddFileParameter("image", new FileItem(imagePath));
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
            var toDt = DateTime.Now;

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

                            int i = 1;
                            var connIDARF01C = Guid.NewGuid().ToString();
                            string username = sessionData?.Account != null ? sessionData.Account.Username : sessionData.User.Username;

                            foreach (Order order in bindOrder.data.orders)
                            {
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

                                insertQ += "('" + order.order_id + "','" + order.customer_first_name + "','" + order.customer_last_name + "','" + order.order_number + "','" + order.payment_method + "','" + order.remarks;
                                insertQ += "','" + order.delivery_info + "','" + price[0].Replace(",", "") + "'," + giftOptionBit + ",'" + order.gift_message + "','" + order.voucher_code + "','" + order.created_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + order.updated_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + order.address_billing.first_name + "','" + order.address_billing.last_name;
                                insertQ += "','" + order.address_billing.phone + "','" + order.address_billing.phone2 + "','" + order.address_billing.address1 + "','" + order.address_billing.address2 + "','" + order.address_billing.address3 + "','" + order.address_billing.address4 + "','" + order.address_billing.address5;
                                insertQ += "','" + order.address_billing.customer_email + "','" + order.address_billing.city + "','" + order.address_billing.post_code + "','" + order.address_billing.country + "','" + order.address_shipping.first_name + "','" + order.address_shipping.last_name + "','" + order.address_shipping.phone + "','" + order.address_shipping.phone2;
                                insertQ += "','" + order.address_shipping.address1 + "','" + order.address_shipping.address2 + "','" + order.address_shipping.address3 + "','" + order.address_shipping.address4 + "','" + order.address_shipping.address5 + "','" + order.address_shipping.customer_email + "','" + order.address_shipping.city;
                                insertQ += "','" + order.address_shipping.post_code + "','" + order.address_shipping.country + "','" + order.national_registration_number + "'," + order.items_count + ",'" + order.promised_shipping_times + "','" + order.extra_attributes + "','" + statusEra;
                                insertQ += "'," + order.voucher + "," + order.shipping_fee + ",'" + order.tax_code + "','" + order.branch_number + "','" + cust + "','" + username + "','" + connectionID + "')";

                                var tblKabKot = EDB.GetDataSet("MOConnectionString", "KabupatenKota", "SELECT TOP 1 * FROM KabupatenKota WHERE NamaKabKot LIKE '%" + order.address_billing.address4 + "%'");
                                var tblProv = EDB.GetDataSet("MOConnectionString", "Provinsi", "SELECT TOP 1 * FROM Provinsi WHERE NamaProv LIKE '%" + order.address_billing.address5 + "%'");

                                var kabKot = "3174";//set default value jika tidak ada di db
                                var prov = "31";//set default value jika tidak ada di db

                                if (tblProv.Tables[0].Rows.Count > 0)
                                    prov = tblProv.Tables[0].Rows[0]["KodeProv"].ToString();
                                if (tblKabKot.Tables[0].Rows.Count > 0)
                                    kabKot = tblKabKot.Tables[0].Rows[0]["KodeKabKot"].ToString();

                                insertPembeli += "('" + order.address_billing.first_name + "','" + order.address_billing.address1 + "','" + order.address_billing.phone + "','" + order.address_billing.customer_email + "',0,0,'0','01',";
                                insertPembeli += "1, 'IDR', '01', '" + order.address_billing.address1 + "', 0, 0, 0, 0, '1', 0, 0, ";
                                insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + order.address_billing.post_code + "', '" + order.address_billing.customer_email + "', '" + kabKot + "', '" + prov + "', '" + order.address_billing.address4 + "', '" + order.address_billing.address5 + "', '" + connIDARF01C + "')";

                                listOrderId += order.order_id;

                                insertQ += " , ";
                                insertPembeli += " , ";

                                if (i < bindOrder.data.orders.Count)
                                {
                                    listOrderId += ",";
                                }
                                else
                                {
                                    listOrderId += "]";
                                }
                                i = i + 1;
                            }

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


                            EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);
                            manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, accessToken, currentLog);

                            getMultiOrderItems(listOrderId, accessToken, connectionID);
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

                                        insertQ += "('" + items.order_item_id + "','" + items.shop_id + "','" + items.order_id + "','" + items.name + "','" + items.sku + "','" + items.shop_sku + "','" + items.shipping_type;
                                        insertQ += "'," + items.item_price + "," + items.paid_price + ",'" + items.currency + "'," + items.tax_amount + "," + items.shipping_amount + "," + items.shipping_service_cost + "," + items.voucher_amount;
                                        insertQ += ",'" + statusEra + "','" + items.shipment_provider + "'," + items.is_digital + ",'" + items.tracking_code + "','" + items.reason + "','" + items.reason_detail + "','" + items.purchase_order_id;
                                        insertQ += "','" + items.purchase_order_number + "','" + items.package_id + "','" + items.extra_attributes + "','" + items.shipping_provider_type + "','" + items.created_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + items.updated_at.ToString("yyyy-MM-dd HH:mm:ss");
                                        insertQ += "','" + items.return_status + "','" + items.product_main_image + "','" + items.variation + "','" + items.product_detail_url + "','" + items.invoice_number + "','" + username + "','" + connectionID + "')";

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
        public BindingBase GetBrgLazada(string cust, string accessToken)
        {
            var ret = new BindingBase();
            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/products/get");
            request.SetHttpMethod("GET");
            request.AddApiParameter("filter", "live");//Possible values are all, live, inactive, deleted, image-missing, pending, rejected, sold-out. 
            //request.AddApiParameter("update_before", "2018-01-01T00:00:00+0800");
            //request.AddApiParameter("search", "product_name");
            //request.AddApiParameter("create_before", "2018-01-01T00:00:00+0800");
            request.AddApiParameter("offset", "0");
            //request.AddApiParameter("create_after", "2010-01-01T00:00:00+0800");
            //request.AddApiParameter("update_after", "2010-01-01T00:00:00+0800");
            request.AddApiParameter("limit", "10");
            //request.AddApiParameter("options", "1");
            //request.AddApiParameter("sku_seller_list", " [\"39817:01:01\", \"Apple 6S Black\"]");
            try
            {
                LazopResponse response = client.Execute(request, accessToken);
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body);
                if (result.code.equal("0"))
                {
                    if (result.data.products.length() > 0)
                    {
                        string sSQL = "INSERT INTO TEMP_BRG_MP (BRG_MP, NAMA, NAMA2, BERAT, PANJANG, LEBAR, TINGGI, Deskripsi, IDMARKET, HJUAL_MP, DISPLAY, CATEGORY_CODE, CATEGORY_NAME, ";
                        sSQL += "ACODE_1, ANAME_1, AVALUE_1, ACODE_2, ANAME_2, AVALUE_2, ACODE_3, ANAME_3, AVALUE_3, ACODE_4, ANAME_4, AVALUE_4, ACODE_5, ANAME_5, AVALUE_5, ACODE_6, ANAME_6, AVALUE_6, ACODE_7, ANAME_7, AVALUE_7, ACODE_8, ANAME_8, AVALUE_8, ACODE_9, ANAME_9, AVALUE_9, ACODE_10, ANAME_10, AVALUE_10, ";
                        sSQL += "ACODE_11, ANAME_11, AVALUE_11, ACODE_12, ANAME_12, AVALUE_12, ACODE_13, ANAME_13, AVALUE_13, ACODE_14, ANAME_14, AVALUE_14, ACODE_15, ANAME_15, AVALUE_15, ACODE_16, ANAME_16, AVALUE_16, ACODE_17, ANAME_17, AVALUE_17, ACODE_18, ANAME_18, AVALUE_18, ACODE_19, ANAME_19, AVALUE_19, ACODE_20, ANAME_20, AVALUE_20, ";
                        sSQL += "ACODE_21, ANAME_21, AVALUE_21, ACODE_22, ANAME_22, AVALUE_22, ACODE_23, ANAME_23, AVALUE_23, ACODE_24, ANAME_24, AVALUE_24, ACODE_25, ANAME_25, AVALUE_25, ACODE_26, ANAME_26, AVALUE_26, ACODE_27, ANAME_27, AVALUE_27, ACODE_28, ANAME_28, AVALUE_28, ACODE_29, ANAME_29, AVALUE_29, ACODE_30, ANAME_30, AVALUE_30, ";
                        sSQL += "ACODE_31, ANAME_31, AVALUE_31, ACODE_32, ANAME_32, AVALUE_32, ACODE_33, ANAME_33, AVALUE_33, ACODE_34, ANAME_34, AVALUE_34, ACODE_35, ANAME_35, AVALUE_35, ACODE_36, ANAME_36, AVALUE_36, ACODE_37, ANAME_37, AVALUE_37, ACODE_38, ANAME_38, AVALUE_38, ACODE_39, ANAME_39, AVALUE_39, ACODE_40, ANAME_40, AVALUE_40, ";
                        sSQL += "ACODE_41, ANAME_41, AVALUE_41, ACODE_42, ANAME_42, AVALUE_42, ACODE_43, ANAME_43, AVALUE_43, ACODE_44, ANAME_44, AVALUE_44, ACODE_45, ANAME_45, AVALUE_45, ACODE_46, ANAME_46, AVALUE_46, ACODE_47, ANAME_47, AVALUE_47, ACODE_48, ANAME_48, AVALUE_48, ACODE_49, ANAME_49, AVALUE_49, ACODE_50, ANAME_50, AVALUE_50) VALUES ";
                        foreach (var brg in result.data.products)
                        {
                            string kodeBrg = brg.SellerSku;
                            var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.Equals(kodeBrg)).FirstOrDefault();
                            if (tempbrginDB == null)
                            {
                                sSQL += " ( '" + brg.SellerSku + "' , '";
                                string namaBrg = brg.name;
                                string categoryCode = brg.primary_category;
                                if (namaBrg.Length > 30)
                                {
                                    sSQL += namaBrg.Substring(0, 30) + "' , '" + namaBrg.Substring(30) + "' , ";
                                }
                                else
                                {
                                    sSQL += namaBrg + "' , '' , ";

                                }
                                sSQL += brg.skus[0].package_weight + " , " + brg.skus[0].package_length + " , " + brg.skus[0].package_width + " , " + brg.skus[0].package_height + " , '";
                                sSQL += brg.description + "' , " + ErasoftDbContext.ARF01.Where(c => c.CUST.Equals(cust)).FirstOrDefault().NAMA + " , " + brg.skus[0].price + " , ";
                                sSQL += "1 , '" + categoryCode + "' , '" + MoDbContext.CATEGORY_LAZADA.Where(c => c.CATEGORY_ID.Equals(categoryCode)).FirstOrDefault().NAME + "' ";

                                var attributeLzd = MoDbContext.ATTRIBUTE_LAZADA.Where(a => a.CATEGORY_CODE.Equals(categoryCode)).FirstOrDefault();
                                #region set attribute
                                if(attributeLzd != null)
                                {
                                    if (!string.IsNullOrEmpty(attributeLzd.ANAME1))
                                    {
                                        if (attributeLzd.AINPUT_TYPE1.Equals("sku"))
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME1 + "' , '" + attributeLzd.ALABEL1 + "' , '" + brg.skus[0].GetType().GetProperty(attributeLzd.ANAME1).GetValue(brg.skus[0], null) + "'";
                                        }
                                        else
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME1 + "' , '" + attributeLzd.ALABEL1 + "' , '" + brg.GetType().GetProperty(attributeLzd.ANAME1).GetValue(brg, null) + "'";
                                        }
                                    }
                                    else
                                    {
                                        sSQL += ", '', '', ''";
                                    }
                                    if (!string.IsNullOrEmpty(attributeLzd.ANAME2))
                                    {
                                        if (attributeLzd.AINPUT_TYPE2.Equals("sku"))
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME2 + "' , '" + attributeLzd.ALABEL2 + "' , '" + brg.skus[0].GetType().GetProperty(attributeLzd.ANAME2).GetValue(brg.skus[0], null) + "'";
                                        }
                                        else
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME2 + "' , '" + attributeLzd.ALABEL2 + "' , '" + brg.GetType().GetProperty(attributeLzd.ANAME2).GetValue(brg, null) + "'";
                                        }
                                    }
                                    else
                                    {
                                        sSQL += ", '', '', ''";
                                    }
                                    if (!string.IsNullOrEmpty(attributeLzd.ANAME3))
                                    {
                                        if (attributeLzd.AINPUT_TYPE3.Equals("sku"))
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME3 + "' , '" + attributeLzd.ALABEL3 + "' , '" + brg.skus[0].GetType().GetProperty(attributeLzd.ANAME3).GetValue(brg.skus[0], null) + "'";
                                        }
                                        else
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME3 + "' , '" + attributeLzd.ALABEL3 + "' , '" + brg.GetType().GetProperty(attributeLzd.ANAME3).GetValue(brg, null) + "'";
                                        }
                                    }
                                    else
                                    {
                                        sSQL += ", '', '', ''";
                                    }
                                    if (!string.IsNullOrEmpty(attributeLzd.ANAME4))
                                    {
                                        if (attributeLzd.AINPUT_TYPE4.Equals("sku"))
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME4 + "' , '" + attributeLzd.ALABEL4 + "' , '" + brg.skus[0].GetType().GetProperty(attributeLzd.ANAME4).GetValue(brg.skus[0], null) + "'";
                                        }
                                        else
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME4 + "' , '" + attributeLzd.ALABEL4 + "' , '" + brg.GetType().GetProperty(attributeLzd.ANAME4).GetValue(brg, null) + "'";
                                        }
                                    }
                                    else
                                    {
                                        sSQL += ", '', '', ''";
                                    }
                                    if (!string.IsNullOrEmpty(attributeLzd.ANAME5))
                                    {
                                        if (attributeLzd.AINPUT_TYPE5.Equals("sku"))
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME5 + "' , '" + attributeLzd.ALABEL5 + "' , '" + brg.skus[0].GetType().GetProperty(attributeLzd.ANAME5).GetValue(brg.skus[0], null) + "'";
                                        }
                                        else
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME5 + "' , '" + attributeLzd.ALABEL5 + "' , '" + brg.GetType().GetProperty(attributeLzd.ANAME5).GetValue(brg, null) + "'";
                                        }
                                    }
                                    else
                                    {
                                        sSQL += ", '', '', ''";
                                    }
                                    if (!string.IsNullOrEmpty(attributeLzd.ANAME6))
                                    {
                                        if (attributeLzd.AINPUT_TYPE6.Equals("sku"))
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME6 + "' , '" + attributeLzd.ALABEL6 + "' , '" + brg.skus[0].GetType().GetProperty(attributeLzd.ANAME6).GetValue(brg.skus[0], null) + "'";
                                        }
                                        else
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME6 + "' , '" + attributeLzd.ALABEL6 + "' , '" + brg.GetType().GetProperty(attributeLzd.ANAME6).GetValue(brg, null) + "'";
                                        }
                                    }
                                    else
                                    {
                                        sSQL += ", '', '', ''";
                                    }
                                    if (!string.IsNullOrEmpty(attributeLzd.ANAME7))
                                    {
                                        if (attributeLzd.AINPUT_TYPE7.Equals("sku"))
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME7 + "' , '" + attributeLzd.ALABEL7 + "' , '" + brg.skus[0].GetType().GetProperty(attributeLzd.ANAME7).GetValue(brg.skus[0], null) + "'";
                                        }
                                        else
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME7 + "' , '" + attributeLzd.ALABEL7 + "' , '" + brg.GetType().GetProperty(attributeLzd.ANAME7).GetValue(brg, null) + "'";
                                        }
                                    }
                                    else
                                    {
                                        sSQL += ", '', '', ''";
                                    }
                                    if (!string.IsNullOrEmpty(attributeLzd.ANAME8))
                                    {
                                        if (attributeLzd.AINPUT_TYPE8.Equals("sku"))
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME8 + "' , '" + attributeLzd.ALABEL8 + "' , '" + brg.skus[0].GetType().GetProperty(attributeLzd.ANAME8).GetValue(brg.skus[0], null) + "'";
                                        }
                                        else
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME8 + "' , '" + attributeLzd.ALABEL8 + "' , '" + brg.GetType().GetProperty(attributeLzd.ANAME8).GetValue(brg, null) + "'";
                                        }
                                    }
                                    else
                                    {
                                        sSQL += ", '', '', ''";
                                    }
                                    if (!string.IsNullOrEmpty(attributeLzd.ANAME9))
                                    {
                                        if (attributeLzd.AINPUT_TYPE9.Equals("sku"))
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME9 + "' , '" + attributeLzd.ALABEL9 + "' , '" + brg.skus[0].GetType().GetProperty(attributeLzd.ANAME9).GetValue(brg.skus[0], null) + "'";
                                        }
                                        else
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME9 + "' , '" + attributeLzd.ALABEL9 + "' , '" + brg.GetType().GetProperty(attributeLzd.ANAME9).GetValue(brg, null) + "'";
                                        }
                                    }
                                    else
                                    {
                                        sSQL += ", '', '', ''";
                                    }
                                    if (!string.IsNullOrEmpty(attributeLzd.ANAME10))
                                    {
                                        if (attributeLzd.AINPUT_TYPE10.Equals("sku"))
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME10 + "' , '" + attributeLzd.ALABEL10 + "' , '" + brg.skus[0].GetType().GetProperty(attributeLzd.ANAME10).GetValue(brg.skus[0], null) + "'";
                                        }
                                        else
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME10 + "' , '" + attributeLzd.ALABEL10 + "' , '" + brg.GetType().GetProperty(attributeLzd.ANAME10).GetValue(brg, null) + "'";
                                        }
                                    }
                                    else
                                    {
                                        sSQL += ", '', '', ''";
                                    }
                                    if (!string.IsNullOrEmpty(attributeLzd.ANAME11))
                                    {
                                        if (attributeLzd.AINPUT_TYPE11.Equals("sku"))
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME11 + "' , '" + attributeLzd.ALABEL11 + "' , '" + brg.skus[0].GetType().GetProperty(attributeLzd.ANAME11).GetValue(brg.skus[0], null) + "'";
                                        }
                                        else
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME11 + "' , '" + attributeLzd.ALABEL11 + "' , '" + brg.GetType().GetProperty(attributeLzd.ANAME11).GetValue(brg, null) + "'";
                                        }
                                    }
                                    else
                                    {
                                        sSQL += ", '', '', ''";
                                    }
                                    if (!string.IsNullOrEmpty(attributeLzd.ANAME12))
                                    {
                                        if (attributeLzd.AINPUT_TYPE12.Equals("sku"))
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME12 + "' , '" + attributeLzd.ALABEL12 + "' , '" + brg.skus[0].GetType().GetProperty(attributeLzd.ANAME12).GetValue(brg.skus[0], null) + "'";
                                        }
                                        else
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME12 + "' , '" + attributeLzd.ALABEL12 + "' , '" + brg.GetType().GetProperty(attributeLzd.ANAME12).GetValue(brg, null) + "'";
                                        }
                                    }
                                    else
                                    {
                                        sSQL += ", '', '', ''";
                                    }
                                    if (!string.IsNullOrEmpty(attributeLzd.ANAME13))
                                    {
                                        if (attributeLzd.AINPUT_TYPE13.Equals("sku"))
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME13 + "' , '" + attributeLzd.ALABEL13 + "' , '" + brg.skus[0].GetType().GetProperty(attributeLzd.ANAME13).GetValue(brg.skus[0], null) + "'";
                                        }
                                        else
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME13 + "' , '" + attributeLzd.ALABEL13 + "' , '" + brg.GetType().GetProperty(attributeLzd.ANAME13).GetValue(brg, null) + "'";
                                        }
                                    }
                                    else
                                    {
                                        sSQL += ", '', '', ''";
                                    }
                                    if (!string.IsNullOrEmpty(attributeLzd.ANAME14))
                                    {
                                        if (attributeLzd.AINPUT_TYPE14.Equals("sku"))
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME14 + "' , '" + attributeLzd.ALABEL14 + "' , '" + brg.skus[0].GetType().GetProperty(attributeLzd.ANAME14).GetValue(brg.skus[0], null) + "'";
                                        }
                                        else
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME14 + "' , '" + attributeLzd.ALABEL14 + "' , '" + brg.GetType().GetProperty(attributeLzd.ANAME14).GetValue(brg, null) + "'";
                                        }
                                    }
                                    else
                                    {
                                        sSQL += ", '', '', ''";
                                    }
                                    if (!string.IsNullOrEmpty(attributeLzd.ANAME15))
                                    {
                                        if (attributeLzd.AINPUT_TYPE15.Equals("sku"))
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME15 + "' , '" + attributeLzd.ALABEL15 + "' , '" + brg.skus[0].GetType().GetProperty(attributeLzd.ANAME15).GetValue(brg.skus[0], null) + "'";
                                        }
                                        else
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME15 + "' , '" + attributeLzd.ALABEL15 + "' , '" + brg.GetType().GetProperty(attributeLzd.ANAME15).GetValue(brg, null) + "'";
                                        }
                                    }
                                    else
                                    {
                                        sSQL += ", '', '', ''";
                                    }
                                    if (!string.IsNullOrEmpty(attributeLzd.ANAME16))
                                    {
                                        if (attributeLzd.AINPUT_TYPE16.Equals("sku"))
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME16 + "' , '" + attributeLzd.ALABEL16 + "' , '" + brg.skus[0].GetType().GetProperty(attributeLzd.ANAME16).GetValue(brg.skus[0], null) + "'";
                                        }
                                        else
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME16 + "' , '" + attributeLzd.ALABEL16 + "' , '" + brg.GetType().GetProperty(attributeLzd.ANAME16).GetValue(brg, null) + "'";
                                        }
                                    }
                                    else
                                    {
                                        sSQL += ", '', '', ''";
                                    }
                                    if (!string.IsNullOrEmpty(attributeLzd.ANAME17))
                                    {
                                        if (attributeLzd.AINPUT_TYPE17.Equals("sku"))
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME17 + "' , '" + attributeLzd.ALABEL17 + "' , '" + brg.skus[0].GetType().GetProperty(attributeLzd.ANAME17).GetValue(brg.skus[0], null) + "'";
                                        }
                                        else
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME17 + "' , '" + attributeLzd.ALABEL17 + "' , '" + brg.GetType().GetProperty(attributeLzd.ANAME17).GetValue(brg, null) + "'";
                                        }
                                    }
                                    else
                                    {
                                        sSQL += ", '', '', ''";
                                    }
                                    if (!string.IsNullOrEmpty(attributeLzd.ANAME18))
                                    {
                                        if (attributeLzd.AINPUT_TYPE18.Equals("sku"))
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME18 + "' , '" + attributeLzd.ALABEL18 + "' , '" + brg.skus[0].GetType().GetProperty(attributeLzd.ANAME18).GetValue(brg.skus[0], null) + "'";
                                        }
                                        else
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME18 + "' , '" + attributeLzd.ALABEL18 + "' , '" + brg.GetType().GetProperty(attributeLzd.ANAME18).GetValue(brg, null) + "'";
                                        }
                                    }
                                    else
                                    {
                                        sSQL += ", '', '', ''";
                                    }
                                    if (!string.IsNullOrEmpty(attributeLzd.ANAME19))
                                    {
                                        if (attributeLzd.AINPUT_TYPE19.Equals("sku"))
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME19 + "' , '" + attributeLzd.ALABEL19 + "' , '" + brg.skus[0].GetType().GetProperty(attributeLzd.ANAME19).GetValue(brg.skus[0], null) + "'";
                                        }
                                        else
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME19 + "' , '" + attributeLzd.ALABEL19 + "' , '" + brg.GetType().GetProperty(attributeLzd.ANAME19).GetValue(brg, null) + "'";
                                        }
                                    }
                                    else
                                    {
                                        sSQL += ", '', '', ''";
                                    }
                                    if (!string.IsNullOrEmpty(attributeLzd.ANAME20))
                                    {
                                        if (attributeLzd.AINPUT_TYPE20.Equals("sku"))
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME20 + "' , '" + attributeLzd.ALABEL20 + "' , '" + brg.skus[0].GetType().GetProperty(attributeLzd.ANAME20).GetValue(brg.skus[0], null) + "'";
                                        }
                                        else
                                        {
                                            sSQL += ", '" + attributeLzd.ANAME20 + "' , '" + attributeLzd.ALABEL20 + "' , '" + brg.GetType().GetProperty(attributeLzd.ANAME20).GetValue(brg, null) + "'";
                                        }
                                    }
                                    else
                                    {
                                        sSQL += ", '', '', ''";
                                    }
                                }
                                
                                #endregion

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

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