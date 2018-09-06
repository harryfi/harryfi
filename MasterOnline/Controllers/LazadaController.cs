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
        string eraAppKey = "101775";
        string eraAppSecret = "QwUJjjtZ3eCy2qaz6Rv1PEXPyPaPkDSu";
        //string eraCallbackUrl = "https://masteronline.co.id/lzd/code?user=&lzdID=";
        string eraCallbackUrl = "https://example.com/lzd/code?user=";
        //string eraAppKey = "";
        // GET: Lazada
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
                    ErasoftDbContext = new ErasoftContext(sessionData.Account.UserId);
                EDB = new DatabaseSQL(sessionData.Account.UserId);

            }
            else
            {
                if (sessionData?.User != null)
                {
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                    EDB = new DatabaseSQL(accFromUser.UserId);
                    ErasoftDbContext = new ErasoftContext(accFromUser.UserId);
                }
            }
        }
        [Route("lzd/code")]
        [HttpGet]
        public void LazadaCode(string user, string lzdID, string code)
        {
            DatabaseSQL EDB = new DatabaseSQL(user);
            var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET API_KEY = '" + code + "' WHERE CUST = '" + lzdID + "'");
        }

        [HttpGet]
        public string LazadaUrl(string cust)
        {
            string userId = sessionData.Account.UserId;
            string lzdId = cust;
            string compUrl = eraCallbackUrl + userId + "&lzdID=" + lzdId;
            string uri = "https://auth.lazada.com/oauth/authorize?response_type=code&force_auth=true&redirect_uri=" + compUrl + "&client_id=" + eraAppKey;
            return uri;
        }

        public string GetToken(string cust, string accessToken)
        {
            string ret;
            string url;
            url = "https://auth.lazada.com/rest";
            DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
            ILazopClient client = new LazopClient(url, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest("/auth/token/create");
            request.SetHttpMethod("GET");
            request.AddApiParameter("code", accessToken);
            try
            {
                LazopResponse response = client.Execute(request);
                ret = "error:" + response.IsError() + "\nbody:" + response.Body;

                if (!response.IsError())
                {
                    var bindAuth = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaAuth)) as LazadaAuth;
                    var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET TOKEN = '" + bindAuth.access_token + "', REFRESH_TOKEN = '" + bindAuth.refresh_token + "' WHERE CUST = '" + cust + "'");
                }
                return ret;
            }
            catch (Exception ex)
            {
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
            LazopResponse response = client.Execute(request);
            //Console.WriteLine(response.IsError());
            //Console.WriteLine(response.Body);
            ret = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaAuth)) as LazadaAuth;
            if (!response.IsError())
            {
                //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
                var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET TOKEN = '" + ret.access_token + "', REFRESH_TOKEN = '" + ret.refresh_token + "' WHERE CUST = '" + cust + "'");

            }
            return ret;
        }

        public BindingBase CreateProduct(BrgViewModel data)
        {
            var ret = new BindingBase();
            ret.status = 0;

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


            string xmlString = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>";
            xmlString = "<Request><Product><PrimaryCategory>"+EDB.GetFieldValue("MOConnectionString", "STF02H", "BRG = '" + data.kdBrg + "' AND IDMARKET = '" + data.idMarket + "'", "category_code") + "</PrimaryCategory>";
            xmlString += "<Attributes><name>" + data.nama + (string.IsNullOrEmpty(data.nama2) ? "" : " " + data.nama2) + "</name>";
            //xmlString += "<short_description><![CDATA[" + data.deskripsi + "]]></short_description>";
            xmlString += "<description><![CDATA[" + data.deskripsi + "]]></description>";
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

            LazopResponse response = client.Execute(request, data.token);
            var res = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaResponseObj)) as LazadaResponseObj;
            if (res.code.Equals("0"))
            {
                ret.status = 1;
                //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
                EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + data.kdBrg + "' WHERE BRG = '" + data.kdBrg + "' AND IDMARKET = '" + data.idMarket + "'");

            }
            else
            {
                ret.message = res.message;
            }

            return ret;
        }

        public BindingBase setDisplay(string kdBrg, bool display, string token)
        {
            var ret = new BindingBase();
            ret.status = 0;

            string xmlString = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>";
            xmlString += "<Request><Product><Skus><Sku>";
            xmlString += "<SellerSku>" + kdBrg + "</SellerSku>";
            xmlString += "<active>" + (display ? "true" : "false") + "</active>";
            xmlString += "</Sku></Skus></Product></Request>";

            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/product/update");
            request.AddApiParameter("payload", xmlString);

            LazopResponse response = client.Execute(request, token);
            var res = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaResponseObj)) as LazadaResponseObj;
            if (res.code.Equals("0"))
            {
                ret.status = 1;
                ret.message = response.Body;
            }
            else
            {
                ret.message = res.message;
            }

            return ret;
        }

        public BindingBase setPromo(PromoLazadaObj data)
        {
            var ret = new BindingBase();
            ret.status = 0;

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

            LazopResponse response = client.Execute(request, data.token);
            var res = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaResponseObj)) as LazadaResponseObj;
            if (res.code.Equals("0"))
            {
                ret.status = 1;
                ret.message = response.Body;
            }
            else
            {
                ret.message = res.message;
            }

            return ret;
        }

        public BindingBase UpdatePriceQuantity(string kdBrg, string harga, string qty, string token)
        {
            var ret = new BindingBase();
            ret.status = 0;

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
                }
                else
                {
                    ret.message = res.message;
                }
            }
            catch (Exception ex)
            {
                ret.message = ex.ToString();
            }
            

            return ret;
        }

        public BindingBase GetShipment(string cust, string accessToken)
        {
            var ret = new BindingBase();
            ret.status = 0;
            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/shipment/providers/get");
            request.SetHttpMethod("GET");
            LazopResponse response = client.Execute(request, accessToken);
            var bindDelivery = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(ShipmentLazada)) as ShipmentLazada;
            if (bindDelivery != null)
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
                }
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
            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/order/pack");
            request.AddApiParameter("shipping_provider", shippingProvider);
            request.AddApiParameter("delivery_type", "dropship");
            request.AddApiParameter("order_item_ids", "[" + ordItems + "]");
            LazopResponse response = client.Execute(request, accessToken);

            return Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaToDeliver)) as LazadaToDeliver;

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
            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/order/rts");
            request.AddApiParameter("shipping_provider", shippingProvider);
            request.AddApiParameter("delivery_type", "dropship");
            request.AddApiParameter("order_item_ids", "[" + ordItems + "]");
            request.AddApiParameter("tracking_number", trackingNumber);
            LazopResponse response = client.Execute(request, accessToken);

            return Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaToDeliver)) as LazadaToDeliver;

        }

        public BindingBase UploadImage(string imagePath, string accessToken)
        {
            var ret = new BindingBase();
            ret.status = 0;
            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/image/upload");
            request.AddFileParameter("image", new FileItem(imagePath));
            LazopResponse response = client.Execute(request, accessToken);
            var bindImg = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(ImageLzd)) as ImageLzd;
            if (bindImg.code.Equals("0"))
            {
                ret.status = 1;
                ret.message = bindImg.data.image.url;
            }
            else
            {
                ret.message = bindImg.message;
            }
            return ret;
        }

        public BindingBase GetOrders(string cust, string accessToken, string connectionID)
        {
            var ret = new BindingBase();
            ret.status = 0;

            var fromDt = DateTime.Now.AddDays(-14);
            var toDt = DateTime.Now;

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
                            switch (order.statuses[0].ToString())
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

                            if (i < bindOrder.data.orders.Count)
                            {
                                insertQ += " , ";
                                insertPembeli += " , ";
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
                        ret.message = a.ToString();

                        SqlCommand CommandSQL = new SqlCommand();

                        CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                        CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIDARF01C;
                        EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);

                        CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                        CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;
                        CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = fromDt.ToString("yyyy-MM-dd HH:mm:ss");
                        CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = toDt.ToString("yyyy-MM-dd HH:mm:ss");
                        CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 1;
                        CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                        CommandSQL.Parameters.Add("@elevenia", SqlDbType.Int).Value = 0;
                        CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;


                        EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);

                        getMultiOrderItems(listOrderId, accessToken, connectionID);
                    }
                    else
                    {
                        ret.message = "no order";
                    }
                }
                else
                {
                    ret.message = "lazada api return error";
                    if (string.IsNullOrEmpty(bindOrder.message))
                        ret.message += "\n" + bindOrder.message.ToString();

                }
            }
            else
            {
                ret.message = "failed to call lazada api";
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

            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/orders/items/get");
            request.SetHttpMethod("GET");
            request.AddApiParameter("order_ids", orderIds);
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

        public async Task<string> GetAttributeList(/*BlibliAPIData data*/)
        {
            var category = MoDbContext.CategoryBlibli.Where(p => p.IS_LAST_NODE.Equals("1")).ToList();
            string ret = "";
            foreach (var item in category)
            {
                string categoryCode = item.CATEGORY_CODE;
                string categoryName = item.CATEGORY_NAME;
                //    string categoryCode = "3 -1000001";
                //string categoryName = "3 Kamar +";

                ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
                LazopRequest request = new LazopRequest();
                request.SetApiName("/category/tree/get");
                request.SetHttpMethod("GET");
                LazopResponse response = client.Execute(request);

                
                if (response != null)
                {
                    dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body);
                    if (response.Code.Equals("0"))
                    {
                        if (result.children.Count > 0)
                        {
                            bool insertAttribute = false;
                            using (SqlConnection oConnection = new SqlConnection("Data Source=202.67.14.92;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^"))
                            {
                                oConnection.Open();
                                using (SqlCommand oCommand = oConnection.CreateCommand())
                                {
                                    var AttributeInDb = MoDbContext.AttributeBlibli.ToList();

                                    //cek jika sudah ada di database
                                    var cari = AttributeInDb.Where(p => p.CATEGORY_CODE.ToUpper().Equals(categoryCode.ToUpper())
                                    && p.CATEGORY_NAME.ToUpper().Equals(categoryName.ToUpper())
                                    ).ToList();
                                    //cek jika sudah ada di database

                                    if (cari.Count == 0)
                                    {
                                        insertAttribute = true;
                                        oCommand.CommandType = CommandType.Text;
                                        oCommand.Parameters.Add(new SqlParameter("@CATEGORY_CODE", SqlDbType.NVarChar, 50));
                                        oCommand.Parameters.Add(new SqlParameter("@CATEGORY_NAME", SqlDbType.NVarChar, 250));
                                        string sSQL = "INSERT INTO [ATTRIBUTE_BLIBLI] ([CATEGORY_CODE], [CATEGORY_NAME],";
                                        string sSQLValue = ") VALUES (@CATEGORY_CODE, @CATEGORY_NAME,";
                                        string a = "";
                                        #region Generate Parameters dan CommandText
                                        for (int i = 1; i <= 30; i++)
                                        {
                                            a = Convert.ToString(i);
                                            sSQL += "[ACODE_" + a + "],[ATYPE_" + a + "],[ANAME_" + a + "],[AOPTIONS_" + a + "],";
                                            sSQLValue += "@ACODE_" + a + ",@ATYPE_" + a + ",@ANAME_" + a + ",@AOPTIONS_" + a + ",";
                                            oCommand.Parameters.Add(new SqlParameter("@ACODE_" + a, SqlDbType.NVarChar, 50));
                                            oCommand.Parameters.Add(new SqlParameter("@ATYPE_" + a, SqlDbType.NVarChar, 50));
                                            oCommand.Parameters.Add(new SqlParameter("@ANAME_" + a, SqlDbType.NVarChar, 250));
                                            oCommand.Parameters.Add(new SqlParameter("@AOPTIONS_" + a, SqlDbType.NVarChar, 1));
                                        }
                                        sSQL = sSQL.Substring(0, sSQL.Length - 1) + sSQLValue.Substring(0, sSQLValue.Length - 1) + ")";
                                        #endregion
                                        oCommand.CommandText = sSQL;
                                        oCommand.Parameters[0].Value = categoryCode;
                                        oCommand.Parameters[1].Value = categoryName;
                                        for (int i = 0; i < 30; i++)
                                        {
                                            a = Convert.ToString(i * 4 + 2);
                                            oCommand.Parameters[(i * 4) + 2].Value = "";
                                            oCommand.Parameters[(i * 4) + 3].Value = "";
                                            oCommand.Parameters[(i * 4) + 4].Value = "";
                                            oCommand.Parameters[(i * 4) + 5].Value = "";
                                            try
                                            {
                                                oCommand.Parameters[(i * 4) + 2].Value = result.value.attributes[i].attributeCode.Value;
                                                oCommand.Parameters[(i * 4) + 3].Value = result.value.attributes[i].attributeType.Value;
                                                oCommand.Parameters[(i * 4) + 4].Value = result.value.attributes[i].name.Value;
                                                oCommand.Parameters[(i * 4) + 5].Value = result.value.attributes[i].options.Count > 0 ? "1" : "0";
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                        }
                                        oCommand.ExecuteNonQuery();
                                    }
                                }
                                if (insertAttribute)
                                {
                                    using (SqlCommand oCommand2 = oConnection.CreateCommand())
                                    {
                                        oCommand2.CommandType = CommandType.Text;
                                        oCommand2.Parameters.Add(new SqlParameter("@ACODE", SqlDbType.NVarChar, 50));
                                        oCommand2.Parameters.Add(new SqlParameter("@ATYPE", SqlDbType.NVarChar, 50));
                                        oCommand2.Parameters.Add(new SqlParameter("@ANAME", SqlDbType.NVarChar, 250));
                                        oCommand2.Parameters.Add(new SqlParameter("@OPTION_VALUE", SqlDbType.NVarChar, 250));
                                        oCommand2.CommandText = "INSERT INTO ATTRIBUTE_OPT_BLIBLI (ACODE,ATYPE,ANAME,OPTION_VALUE) VALUES (@ACODE,@ATYPE,@ANAME,@OPTION_VALUE)";
                                        string a = "";
                                        var AttributeOptInDb = MoDbContext.AttributeOptBlibli.ToList();
                                        for (int i = 0; i < 30; i++)
                                        {
                                            a = Convert.ToString(i + 1);
                                            try
                                            {
                                                if (result.value.attributes[i].options.Count > 0)
                                                {
                                                    string ACODE = "";
                                                    string ATYPE = "";
                                                    string ANAME = "";
                                                    string OPTION_VALUE = "";
                                                    if (Convert.ToString(result.value.attributes[i].attributeCode.Value) != "WA-0000002") // warna
                                                    {
                                                        for (int j = 0; j < result.value.attributes[i].options.Count; j++)
                                                        {
                                                            ACODE = result.value.attributes[i].attributeCode.Value;
                                                            ATYPE = result.value.attributes[i].attributeType.Value;
                                                            ANAME = result.value.attributes[i].name.Value;
                                                            OPTION_VALUE = result.value.attributes[i].options[j].Value;

                                                            //cek jika sudah ada di database
                                                            var cari = AttributeOptInDb.Where(p => p.ACODE.ToUpper().Equals(ACODE.ToUpper())
                                                            && p.ATYPE.ToUpper().Equals(ATYPE.ToUpper())
                                                            && p.ANAME.ToUpper().Equals(ANAME.ToUpper())
                                                            && p.OPTION_VALUE.ToUpper().Equals(OPTION_VALUE.ToUpper())
                                                            ).ToList();
                                                            //cek jika sudah ada di database

                                                            if (cari.Count == 0)
                                                            {
                                                                oCommand2.Parameters[0].Value = ACODE;
                                                                oCommand2.Parameters[1].Value = ATYPE;
                                                                oCommand2.Parameters[2].Value = ANAME;
                                                                oCommand2.Parameters[3].Value = OPTION_VALUE;
                                                                oCommand2.ExecuteNonQuery();

                                                                AttributeOptInDb = MoDbContext.AttributeOptBlibli.ToList();
                                                            }

                                                        }
                                                    }
                                                }

                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                        }
                                    }
                                }
                                //}
                            }
                        }
                    }
                }
            }

            return ret;
        }
    }
}