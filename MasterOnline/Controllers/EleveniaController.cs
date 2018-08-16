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

namespace MasterOnline.Controllers
{
    public class EleveniaController : Controller
    {
        AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        public MoDbContext MoDbContext { get; set; }
        public ErasoftContext ErasoftDbContext { get; set; }
        DatabaseSQL EDB;
        public EleveniaController() {
            MoDbContext = new MoDbContext();
            var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
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
                    ErasoftDbContext = new ErasoftContext(accFromUser.UserId);
                    EDB = new DatabaseSQL(accFromUser.UserId);
                }
            }
        }
        [Route("ele/image")]
        public ActionResult Image(string id)
        {
            var dir = Server.MapPath("~/Content/Uploaded");
            var dirNotFound = Server.MapPath("~/Content/Images");
            var path = Path.Combine(dir, id); //validate the path for security or use other means to generate the path.
            if (!System.IO.File.Exists(path))
            {
                path = Path.Combine(dirNotFound, "photo_not_available.jpg");
            }
            return base.File(path, "image/jpeg");
        }

        public enum StatusOrder
        {
            Cancel = 1,
            Paid = 2,
            PackagingINP = 3,
            ShippingINP = 4,
            Completed = 5
        }

        public ClientMessage CreateProduct(EleveniaProductData data, bool display)
        {
            //string val = form.data;
            //EleveniaCreateProductData data = Newtonsoft.Json.JsonConvert.DeserializeObject(val, typeof(EleveniaCreateProductData)) as EleveniaCreateProductData;

            var ret = new ClientMessage();
            string auth = data.api_key;//"f6875334a817a9ee4c20387a5b8b9d0b";

            Utils.HttpRequest req = new Utils.HttpRequest();

            string xmlString = "<Product>";
            xmlString += "<selMnbdNckNm><![CDATA[" + data.nama + "]]></selMnbdNckNm>";//nickname
            xmlString += "<selMthdCd>01</selMthdCd>";//sales type : 01 = ready stok ; 04 = preorder ; 05 = used item
            xmlString += "<dispCtgrNo>5475</dispCtgrNo>";//category id //5475 = Hobi lain lain

            //var attr = await GetAttribute("");

            //foreach (productCtgrAttributesProductCtgrAttribute prodAttr in attr.productCtgrAttribute)
            //{
            //    xmlString += "<ProductCtgrAttribute><prdAttrCd>" + prodAttr.prdAttrCd + "</prdAttrCd>";//category attribute code
            //    xmlString += "<prdAttrNm>" + prodAttr.prdAttrNm + "</prdAttrNm>";//category attribute name i.e: brand, model, type, ISBN
            //    xmlString += "<prdAttrNo>" + prodAttr.prdAttrNo + "</prdAttrNo>";//category attribute id
            //    xmlString += "<prdAttrVal>(Check Product Details)</prdAttrVal></ProductCtgrAttribute>";//category attribute value
            //}
            xmlString += "<ProductCtgrAttribute><prdAttrCd>2000005</prdAttrCd>";//category attribute code
            xmlString += "<prdAttrNm>Brand</prdAttrNm>";//category attribute name i.e: brand, model, type, ISBN
            xmlString += "<prdAttrNo>178026</prdAttrNo>";//category attribute id
            xmlString += "<prdAttrVal>" + data.Brand + "</prdAttrVal></ProductCtgrAttribute>";//category attribute value

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
                    //xmlString += "<prdImage0" + Convert.ToString(prodImageCount) + "><![CDATA[http://soffice.11st.co.kr/img/layout/logo.gif]]></prdImage0" + Convert.ToString(prodImageCount) + ">";//image url (can use up to 5 image)
                    prodImageCount++;
                }
                else if (i == 0)
                {
                    xmlString += "<prdImage01><![CDATA[http://soffice.11st.co.kr/img/layout/logo.gif]]></prdImage01>";//image url (can use up to 5 image)
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
            //xmlString += "<prdNo>25908965</prdNo>";//return/exchange information
            xmlString += "</Product>";
            //var content = new System.Net.Http.StringContent(xmlString, Encoding.UTF8, "text/xml");

            //ClientMessage result = await req.RequestJSONObjectEl(Utils.HttpRequest.PROTOCOL.Http, Utils.HttpRequest.RESTServices.rest, Utils.HttpRequest.METHOD.POST, "prodservices/product", content, typeof(ClientMessage), auth) as ClientMessage;
            ClientMessage result = req.CallElevAPI(Utils.HttpRequest.PROTOCOL.Http, Utils.HttpRequest.RESTServices.rest, Utils.HttpRequest.METHOD.POST, "prodservices/product", xmlString,typeof(ClientMessage), auth) as ClientMessage;


            if (result != null)
            {
                if (Convert.ToString(result.resultCode).Equals("200"))
                {
                    EDB.ExecuteSQL("", CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + Convert.ToString(result.productNo) + "' WHERE BRG = '" + data.kode + "' AND IDMARKET = '" + data.IDMarket + "'");
                    #region Hide Item
                    if (!display)
                    {
                        EleveniaController.EleveniaProductData data2 = new EleveniaController.EleveniaProductData
                        {
                            api_key = data.api_key,
                            kode = Convert.ToString(result.productNo)
                        };
                        var resultHide = new EleveniaController().HideItem(data2);
                    }
                    #endregion
                }
                ret = result;
            }

            return ret;
        }

        public ClientMessage UpdateProduct(EleveniaProductData data)
        {
            //string val = form.data;
            //EleveniaCreateProductData data = Newtonsoft.Json.JsonConvert.DeserializeObject(val, typeof(EleveniaCreateProductData)) as EleveniaCreateProductData;

            var ret = new ClientMessage();
            string auth = data.api_key;//"f6875334a817a9ee4c20387a5b8b9d0b";

            Utils.HttpRequest req = new Utils.HttpRequest();

            string xmlString = "<Product>";
            xmlString += "<selMnbdNckNm><![CDATA[" + data.nama + "]]></selMnbdNckNm>";//nickname
            xmlString += "<selMthdCd>01</selMthdCd>";//sales type : 01 = ready stok ; 04 = preorder ; 05 = used item
            xmlString += "<dispCtgrNo>5475</dispCtgrNo>";//category id //5475 = Hobi lain lain

            //var attr = await GetAttribute("");

            //foreach (productCtgrAttributesProductCtgrAttribute prodAttr in attr.productCtgrAttribute)
            //{
            //    xmlString += "<ProductCtgrAttribute><prdAttrCd>" + prodAttr.prdAttrCd + "</prdAttrCd>";//category attribute code
            //    xmlString += "<prdAttrNm>" + prodAttr.prdAttrNm + "</prdAttrNm>";//category attribute name i.e: brand, model, type, ISBN
            //    xmlString += "<prdAttrNo>" + prodAttr.prdAttrNo + "</prdAttrNo>";//category attribute id
            //    xmlString += "<prdAttrVal>(Check Product Details)</prdAttrVal></ProductCtgrAttribute>";//category attribute value
            //}
            xmlString += "<ProductCtgrAttribute><prdAttrCd>2000005</prdAttrCd>";//category attribute code
            xmlString += "<prdAttrNm>Brand</prdAttrNm>";//category attribute name i.e: brand, model, type, ISBN
            xmlString += "<prdAttrNo>178026</prdAttrNo>";//category attribute id
            xmlString += "<prdAttrVal>" + data.Brand + "</prdAttrVal></ProductCtgrAttribute>";//category attribute value

            xmlString += "<prdNm><![CDATA[" + data.nama + "]]></prdNm>";//product name
            xmlString += "<prdStatCd>01</prdStatCd>";//item condition : 01 = new ; 02 = used
            xmlString += "<prdWght>" + data.berat + "</prdWght>";//weight in kg
            xmlString += "<dlvGrntYn>N</dlvGrntYn>";//guarantee of delivery Y/N value
            xmlString += "<minorSelCnYn>Y</minorSelCnYn>";//minor(under 17 years old) can buy
            xmlString += "<suplDtyfrPrdClfCd>02</suplDtyfrPrdClfCd>";//VAT : 01 = item with tax ; 02 = item without tax

            int prodImageCount = 1;
            for (int i = 0; i < data.imgUrl.Length; i++)
            {
                if (data.imgUrl[i].Length > 0)
                {
                    xmlString += "<prdImage0" + Convert.ToString(prodImageCount) + "><![CDATA[" + data.imgUrl[i] + "]]></prdImage0" + Convert.ToString(prodImageCount) + ">";//image url (can use up to 5 image)
                    //xmlString += "<prdImage0" + Convert.ToString(prodImageCount) + "><![CDATA[http://soffice.11st.co.kr/img/layout/logo.gif]]></prdImage0" + Convert.ToString(prodImageCount) + ">";//image url (can use up to 5 image)
                    prodImageCount++;
                }
                else if (i == 0)
                {
                    xmlString += "<prdImage01><![CDATA[http://soffice.11st.co.kr/img/layout/logo.gif]]></prdImage01>";//image url (can use up to 5 image)
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
            //var content = new System.Net.Http.StringContent(xmlString, Encoding.UTF8, "text/xml");

            //ClientMessage result = await req.RequestJSONObjectEl(Utils.HttpRequest.PROTOCOL.Http, Utils.HttpRequest.RESTServices.rest, Utils.HttpRequest.METHOD.POST, "prodservices/product", content, typeof(ClientMessage), auth) as ClientMessage;
            ClientMessage result = req.CallElevAPI(Utils.HttpRequest.PROTOCOL.Http, Utils.HttpRequest.RESTServices.rest, Utils.HttpRequest.METHOD.POST, "prodservices/product", xmlString, typeof(ClientMessage), auth) as ClientMessage;
            if (result != null)
            {
                if (Convert.ToString(result.resultCode).Equals("200"))
                {

                }
                ret = result;
            }

            return ret;
        }

        public ClientMessage UpdateProductQOH_Price(EleveniaProductData data)
        {
            //string val = form.data;
            //EleveniaCreateProductData data = Newtonsoft.Json.JsonConvert.DeserializeObject(val, typeof(EleveniaCreateProductData)) as EleveniaCreateProductData;

            var ret = new ClientMessage();
            string auth = data.api_key;//"f6875334a817a9ee4c20387a5b8b9d0b";

            Utils.HttpRequest req = new Utils.HttpRequest();

            string xmlString = "<Product>";
            xmlString += "<selMnbdNckNm><![CDATA[" + data.nama + "]]></selMnbdNckNm>";//nickname
            xmlString += "<selMthdCd>01</selMthdCd>";//sales type : 01 = ready stok ; 04 = preorder ; 05 = used item
            xmlString += "<dispCtgrNo>5475</dispCtgrNo>";//category id //5475 = Hobi lain lain

            //var attr = await GetAttribute("");

            //foreach (productCtgrAttributesProductCtgrAttribute prodAttr in attr.productCtgrAttribute)
            //{
            //    xmlString += "<ProductCtgrAttribute><prdAttrCd>" + prodAttr.prdAttrCd + "</prdAttrCd>";//category attribute code
            //    xmlString += "<prdAttrNm>" + prodAttr.prdAttrNm + "</prdAttrNm>";//category attribute name i.e: brand, model, type, ISBN
            //    xmlString += "<prdAttrNo>" + prodAttr.prdAttrNo + "</prdAttrNo>";//category attribute id
            //    xmlString += "<prdAttrVal>(Check Product Details)</prdAttrVal></ProductCtgrAttribute>";//category attribute value
            //}
            xmlString += "<ProductCtgrAttribute><prdAttrCd>2000005</prdAttrCd>";//category attribute code
            xmlString += "<prdAttrNm>Brand</prdAttrNm>";//category attribute name i.e: brand, model, type, ISBN
            xmlString += "<prdAttrNo>178026</prdAttrNo>";//category attribute id
            xmlString += "<prdAttrVal>" + data.Brand + "</prdAttrVal></ProductCtgrAttribute>";//category attribute value

            xmlString += "<prdNm><![CDATA[" + data.nama + "]]></prdNm>";//product name
            xmlString += "<prdStatCd>01</prdStatCd>";//item condition : 01 = new ; 02 = used
            xmlString += "<prdWght>" + data.berat + "</prdWght>";//weight in kg
            xmlString += "<dlvGrntYn>N</dlvGrntYn>";//guarantee of delivery Y/N value
            xmlString += "<minorSelCnYn>Y</minorSelCnYn>";//minor(under 17 years old) can buy
            xmlString += "<suplDtyfrPrdClfCd>02</suplDtyfrPrdClfCd>";//VAT : 01 = item with tax ; 02 = item without tax

            int prodImageCount = 1;
            for (int i = 0; i < data.imgUrl.Length; i++)
            {
                if (data.imgUrl[i].Length > 0)
                {
                    xmlString += "<prdImage0" + Convert.ToString(prodImageCount) + "><![CDATA[" + data.imgUrl[i] + "]]></prdImage0" + Convert.ToString(prodImageCount) + ">";//image url (can use up to 5 image)
                    //xmlString += "<prdImage0" + Convert.ToString(prodImageCount) + "><![CDATA[http://soffice.11st.co.kr/img/layout/logo.gif]]></prdImage0" + Convert.ToString(prodImageCount) + ">";//image url (can use up to 5 image)
                    prodImageCount++;
                }
                else if (i == 0)
                {
                    xmlString += "<prdImage01><![CDATA[http://soffice.11st.co.kr/img/layout/logo.gif]]></prdImage01>";//image url (can use up to 5 image)
                    prodImageCount++;
                }
            }

            xmlString += "<htmlDetail><![CDATA[" + data.Keterangan + "]]></htmlDetail>";//item detail(html supported)
            xmlString += "<sellerPrdCd><![CDATA[" + data.kode + "]]></sellerPrdCd>";//seller sku(optional)
            xmlString += "<selTermUseYn>N</selTermUseYn>";//whether to use sales period Y/N value
            //xmlString += "<wrhsPlnDy></wrhsPlnDy>";//due date of stock(optional)
            xmlString += "<selPrc>" + data.Price + "</selPrc>";//product price
            xmlString += "<prdSelQty>" + data.Qty + "</prdSelQty>";//product stock
            xmlString += "<dscAmtPercnt></dscAmtPercnt>";//discount value(optional)
            xmlString += "<cupnDscMthdCd></cupnDscMthdCd>";//discount unit(optional) : 01 = in Rp ; 02 = in %
            xmlString += "<cupnIssEndDy></cupnIssEndDy>";//discount end date(optional)
            xmlString += "<tmpltSeq>" + data.DeliveryTempNo + "</tmpltSeq>";//number of delivery
            xmlString += "<asDetail></asDetail>";//after service information(garansi)
            xmlString += "<rtngExchDetail>Hubungi toko untuk retur</rtngExchDetail>";//return/exchange information
            xmlString += "<prdNo>" + data.kode_mp + "</prdNo>";//Marketplace Product ID
            xmlString += "</Product>";
            //var content = new System.Net.Http.StringContent(xmlString, Encoding.UTF8, "text/xml");

            //ClientMessage result = await req.RequestJSONObjectEl(Utils.HttpRequest.RESTServices.rest, "prodservices/product", content, typeof(ClientMessage), auth) as ClientMessage;
            //ClientMessage result = await req.RequestJSONObjectEl(Utils.HttpRequest.PROTOCOL.Http, Utils.HttpRequest.RESTServices.rest, Utils.HttpRequest.METHOD.POST, "prodservices/product", content, typeof(ClientMessage), auth) as ClientMessage;
            ClientMessage result = req.CallElevAPI(Utils.HttpRequest.PROTOCOL.Http, Utils.HttpRequest.RESTServices.rest, Utils.HttpRequest.METHOD.POST, "prodservices/product", xmlString, typeof(ClientMessage), auth) as ClientMessage;
            if (result != null)
            {
                //if (Convert.ToString(result.resultCode).Equals("200"))
                //{

                //}
                //ret = result;
            }

            return ret;
        }

        public ClientMessage DisplayItem(EleveniaProductData data)
        {
            var ret = new ClientMessage();
            string auth = data.api_key;

            var content = new System.Net.Http.StringContent("", Encoding.UTF8, "text/xml");
            Utils.HttpRequest req = new Utils.HttpRequest();
            //var result = await req.RequestJSONObjectEl(Utils.HttpRequest.PROTOCOL.Http, Utils.HttpRequest.RESTServices.rest, Utils.HttpRequest.METHOD.PUT, "prodstatservice/stat/restartdisplay/" + data.kode, content, typeof(ClientMessage), auth) as ClientMessage;
            var result = req.CallElevAPI(Utils.HttpRequest.PROTOCOL.Http, Utils.HttpRequest.RESTServices.rest, Utils.HttpRequest.METHOD.PUT, "prodstatservice/stat/restartdisplay/" + data.kode, "", typeof(ClientMessage), auth) as ClientMessage;
            if (result != null)
            {
                ret = result;
            }

            return ret;
        }

        public ClientMessage HideItem(EleveniaProductData data)
        {
            var ret = new ClientMessage();
            string auth = data.api_key;

            var content = new System.Net.Http.StringContent("", Encoding.UTF8, "text/xml");
            Utils.HttpRequest req = new Utils.HttpRequest();
            //var result = req.RequestJSONObjectEl(Utils.HttpRequest.PROTOCOL.Http, Utils.HttpRequest.RESTServices.rest, Utils.HttpRequest.METHOD.PUT, "prodstatservice/stat/stopdisplay/" + data.kode, content, typeof(ClientMessage), auth) as ClientMessage;
            var result = req.CallElevAPI(Utils.HttpRequest.PROTOCOL.Http, Utils.HttpRequest.RESTServices.rest, Utils.HttpRequest.METHOD.PUT, "prodstatservice/stat/stopdisplay/" + data.kode, "", typeof(ClientMessage), auth) as ClientMessage;
            if (result != null)
            {
                ret = result;
            }

            return ret;
        }

        //public async Task<DeliveryTemplates> GetDeliveryTemp(EleveniaProductData data)
        public ActionResult GetDeliveryTemp(string recNum, string auth)
        {
            //string auth = "";
            //ManageController MC = new ManageController();
            //var kdEL = MC.MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "ELEVENIA");
            //var listELShop = MC.ErasoftDbContext.ARF01.Where(m => m.NAMA == kdEL.IdMarket.ToString() && m.RecNum.ToString().Equals(recNum)).ToList();
            //if (listELShop.Count > 0)
            //{
                //foreach (ARF01 tblCustomer in listELShop)
            //    {
            //        auth = tblCustomer.API_KEY;
            //    }
            //}
            //var ret = new DeliveryTemplates();
            var ret = string.Empty;
            //string auth = data.api_key;//"f6875334a817a9ee4c20387a5b8b9d0b";

            var content = new System.Net.Http.StringContent("", Encoding.UTF8, "text/xml");
            Utils.HttpRequest req = new Utils.HttpRequest();
            //var result = await req.RequestJSONObjectEl(Utils.HttpRequest.PROTOCOL.Http, Utils.HttpRequest.RESTServices.rest, Utils.HttpRequest.METHOD.GET, "delivery/template", content, typeof(string), auth) as string;
            var result = req.CallElevAPI(Utils.HttpRequest.PROTOCOL.Http, Utils.HttpRequest.RESTServices.rest, Utils.HttpRequest.METHOD.GET, "delivery/template", "", typeof(string), auth) as string;
            if (result != null)
            {
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.LoadXml(result.Substring(55));
                string json = JsonConvert.SerializeXmlNode(doc);

                try
                {
                    var res = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<DeliveryTemplatesRootM>(json);
                    //var res = JsonConvert.DeserializeObject<DeliveryTemplatesRoot>(json);
                    if (res.DeliveryTemplates != null)
                    {

                        EDB.ExecuteSQL("ConnectionString", CommandType.Text, "DELETE FROM DeliveryTemplateElevenia WHERE RECNUM_ARF01 = " + Convert.ToString(recNum));
                        string sSQL = "INSERT INTO DeliveryTemplateElevenia (KODE,KETERANGAN,RECNUM_ARF01) VALUES (";
                        foreach (var item in res.DeliveryTemplates.template)
                        {
                            sSQL += "'" + item.dlvTmpltSeq + "','" + item.dlvTmpltNm + "'," + Convert.ToString(recNum) + "),(";
                        }
                        sSQL += ")";
                        sSQL = sSQL.Replace(",()", "");
                        EDB.ExecuteSQL("ConnectionString", CommandType.Text, sSQL);

                        DataSet ds = new DataSet();
                        ds = EDB.GetDataSet("Con", "DeliveryTemplateElevenia", "SELECT KODE,KETERANGAN,RECNUM_ARF01 FROM DeliveryTemplateElevenia WHERE RECNUM_ARF01='" + recNum + "'");
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            ret = recNum;
                        }
                    }
                }
                catch (Exception ex)
                {
                    var res = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<DeliveryTemplatesRootS>(json);
                    //var res = JsonConvert.DeserializeObject<DeliveryTemplatesRoot>(json);
                    if (res.DeliveryTemplates != null)
                    {

                        EDB.ExecuteSQL("ConnectionString", CommandType.Text, "DELETE FROM DeliveryTemplateElevenia WHERE RECNUM_ARF01 = " + Convert.ToString(recNum));
                        string sSQL = "INSERT INTO DeliveryTemplateElevenia (KODE,KETERANGAN,RECNUM_ARF01) VALUES (";
                        sSQL += "'" + res.DeliveryTemplates.template.dlvTmpltSeq + "','" + res.DeliveryTemplates.template.dlvTmpltNm + "'," + Convert.ToString(recNum) + ")";
                        EDB.ExecuteSQL("ConnectionString", CommandType.Text, sSQL);

                        DataSet ds = new DataSet();
                        ds = EDB.GetDataSet("Con", "DeliveryTemplateElevenia", "SELECT KODE,KETERANGAN,RECNUM_ARF01 FROM DeliveryTemplateElevenia WHERE RECNUM_ARF01='" + recNum + "'");
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            ret = recNum;
                        }
                    }
                }

            }

            return Json(ret, JsonRequestBehavior.AllowGet);
        }

        public BindingBase GetOrder(string auth, StatusOrder stat, string connId, string CUST, string NAMA_CUST)
        {
            var connIdARF01C = Guid.NewGuid().ToString();
            var ret = new BindingBase();
            string status = "";
            switch (stat)
            {
                case StatusOrder.Cancel:
                    //Cancel Order
                    status = "B01";
                    break;
                case StatusOrder.Paid:
                    //paid
                    status = "202";
                    break;
                case StatusOrder.PackagingINP:
                    //Packaging in Progress
                    status = "301";
                    break;
                case StatusOrder.ShippingINP:
                    //Shipping in Progress
                    status = "401";
                    break;
                case StatusOrder.Completed:
                    //Completed (Shipping)
                    status = "501";
                    break;
                default:
                    break;
            }
            ret.status = 0;
            string fromDt = DateTime.Now.AddDays(-14).ToString("yyyy/MM/dd");
            string toDt = DateTime.Now.ToString("yyyy/MM/dd");

            var content = new System.Net.Http.StringContent("", Encoding.UTF8, "text/xml");
            Utils.HttpRequest req = new Utils.HttpRequest();
            string param = "ordStat=" + status + "&dateFrom=" + fromDt + "&dateTo=" + toDt;
            //var test = await req.RequestJSONObjectEl(Utils.HttpRequest.PROTOCOL.Https, Utils.HttpRequest.RESTServices.rest, Utils.HttpRequest.METHOD.GET, "orderservices/orders?" + param, content, typeof(string), auth) as string;
            var test = req.CallElevAPI(Utils.HttpRequest.PROTOCOL.Https, Utils.HttpRequest.RESTServices.rest, Utils.HttpRequest.METHOD.GET, "orderservices/orders?" + param, "", typeof(string), auth) as string;
            if (!string.IsNullOrEmpty(test))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(test.Substring(55));

                string json = JsonConvert.SerializeXmlNode(doc);
                var res = new JavaScriptSerializer().Deserialize<ElOrdersM>(json);
                if (res.Orders != null)
                {
                    string username = "Auto Elevenia";
                    string sellerShop = "seller elv";
                    string sSQL = "insert into TEMP_ELV_ORDERS ([DELIVERY_NO],[DELIVERY_MTD_CD],[DELIVERY_ETR_CD],[DELIVERY_ETR_NAME],[ORDER_NO]";
                    sSQL += ",[ORDER_NAME],[ORDER_DATE],[ORDER_AMOUNT],[ORDER_PROD_NO],[ORDER_PROD_QTY],[PROD_NO],[RECEIVER_ADDRESS],[RECEIVER_POSTCODE]";
                    sSQL += ",[DELIVERY_PHONE],[DELIVERY_COST],[ORDER_STAT],[SHOP_NAME],[CUST],[NAMA_CUST],[USERNAME],[CONN_ID]) VALUES";

                    string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                    insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                    insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV,CONNECTION_ID) VALUES ";

                    string PESANAN_DI_ELEVENIA = "";
                    if (res.Orders.order.Count > 0)
                    {
                        ret.message = "multi\n" + json;
                        ret.status = 1;
                        foreach (ElOrder dataOrder in res.Orders.order)
                        {
                            sSQL += "('" + dataOrder.dlvNo + "','" + dataOrder.dlvMthdCd + "','" + dataOrder.dlvEtprsCd + "','" + dataOrder.dlvEtprsNm + "','" + dataOrder.ordNo + "',";
                            sSQL += "'" + dataOrder.ordNm + "','" + dataOrder.ordDt + "'," + dataOrder.orderAmt + ",'" + dataOrder.ordPrdSeq + "'," + dataOrder.ordQty + ",'" + dataOrder.prdNo + "','" + dataOrder.rcvrBaseAddr + "','" + dataOrder.rcvrPostalCode + "',";
                            sSQL += "'" + dataOrder.rcvrTlphn + "','" + Convert.ToDecimal(dataOrder.lstDlvCst) + "','" + dataOrder.ordPrdStat + "','" + sellerShop + "','" + CUST + "','" + NAMA_CUST.Replace(',', '.') + "','" + username + "','" + connId + "')";
                            PESANAN_DI_ELEVENIA += "'" + dataOrder.dlvNo + "',";
                            //var tblKabKot = EDB.GetDataSet("dotnet", "SCREEN_MO", "KabupatenKota", "SELECT TOP 1 * FROM KabupatenKota WHERE NamaKabKot LIKE '%" + order.consignee.city + "%'");
                            //var tblProv = EDB.GetDataSet("dotnet", "SCREEN_MO", "Provinsi", "SELECT TOP 1 * FROM Provinsi WHERE NamaProv LIKE '%" + order.consignee.province + "%'");

                            var kabKot = "3174";
                            var prov = "31";
                            //if (tblProv.Tables[0].Rows.Count > 0)
                            //    prov = tblProv.Tables[0].Rows[0]["KodeProv"].ToString();
                            //if (tblKabKot.Tables[0].Rows.Count > 0)
                            //    kabKot = tblKabKot.Tables[0].Rows[0]["KodeKabKot"].ToString();

                            insertPembeli += "('" + dataOrder.rcvrNm + "','" + dataOrder.rcvrBaseAddr + "','" + dataOrder.rcvrTlphn + "','" + NAMA_CUST.Replace(',', '.') + "',0,0,'0','01',";
                            insertPembeli += "1, 'IDR', '01', '" + dataOrder.rcvrBaseAddr + "', 0, 0, 0, 0, '1', 0, 0, ";
                            insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + dataOrder.rcvrPostalCode + "', '" + dataOrder.memId + "', '" + kabKot + "', '" + prov + "', '', '','" + connIdARF01C + "')";


                            sSQL += ",";
                            insertPembeli += ",";
                        }
                        PESANAN_DI_ELEVENIA = PESANAN_DI_ELEVENIA.Substring(0, PESANAN_DI_ELEVENIA.Length - 1);
                        insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 1);
                        sSQL = sSQL.Substring(0, sSQL.Length - 1);
                    }
                    else
                    {
                        var res2 = new JavaScriptSerializer().Deserialize<ElOrderS>(json);
                        if (res2 != null)
                        {
                            ret.status = 1;
                            ret.message = "single\n" + json;

                            sSQL += "('" + res2.Orders.order.dlvNo + "','" + res2.Orders.order.dlvMthdCd + "','" + res2.Orders.order.dlvEtprsCd + "','" + res2.Orders.order.dlvEtprsNm + "','" + res2.Orders.order.ordNo + "',";
                            sSQL += "'" + res2.Orders.order.ordNm + "','" + res2.Orders.order.ordDt + "'," + res2.Orders.order.orderAmt + ",'" + res2.Orders.order.ordPrdSeq + "'," + res2.Orders.order.ordQty + ",'" + res2.Orders.order.prdNo + "','" + res2.Orders.order.rcvrBaseAddr + "','" + res2.Orders.order.rcvrPostalCode + "',";
                            sSQL += "'" + res2.Orders.order.rcvrTlphn + "'," + Convert.ToDecimal(res2.Orders.order.lstDlvCst) + ",'" + res2.Orders.order.ordPrdStat + "','" + sellerShop + "','" + CUST + "','" + NAMA_CUST.Replace(',', '.') + "','" + username + "','" + connId + "')";

                            PESANAN_DI_ELEVENIA += "'" + res2.Orders.order.dlvNo + "'";

                            var kabKot = "3174";
                            var prov = "31";

                            insertPembeli += "('" + res2.Orders.order.rcvrNm + "','" + res2.Orders.order.rcvrBaseAddr + "','" + res2.Orders.order.rcvrTlphn + "','" + NAMA_CUST.Replace(',', '.') + "',0,0,'0','01',";
                            insertPembeli += "1, 'IDR', '01', '" + res2.Orders.order.rcvrBaseAddr + "', 0, 0, 0, 0, '1', 0, 0, ";
                            insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + res2.Orders.order.rcvrPostalCode + "', '" + res2.Orders.order.memId + "', '" + kabKot + "', '" + prov + "', '', '','" + connIdARF01C + "')";

                        }
                    }
                    EDB.ExecuteSQL("Constring", CommandType.Text, insertPembeli);
                    EDB.ExecuteSQL("Constring", CommandType.Text, sSQL);
                    //SqlCommand CommandSQL = new SqlCommand();
                    //CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                    //CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connId;
                    //CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
                    //CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    //CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                    //CommandSQL.Parameters.Add("@Bukalapak", SqlDbType.Int).Value = 0;
                    //CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 1;

                    SqlCommand CommandSQL = new SqlCommand();
                    //call sp to insert buyer data
                    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIdARF01C;

                    EDB.ExecuteSQL("Con", "MoveARF01CFromTempTable", CommandSQL);

                    //call sp to insert pesanan
                    CommandSQL = new SqlCommand();
                    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;

                    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connId;
                    CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
                    CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 1;
                    CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;

                    EDB.ExecuteSQL("Con", "MoveOrderFromTempTable", CommandSQL);
                    ////remark by calvin, dipindah ke saat ChangeStatusPesanan di managecontroller
                    ////setelah ambil pesanan yang berstatus paid, sync ( cek SO berstatus paid di MO berdasarkan delivery_no, jika sudha jadi 02 ( siap dikirim ), jalankan API 
                    //if (stat == StatusOrder.Paid)
                    //{
                    //    //CEK APAKAH DI SOT01A, PESANAN TERSEBUT SUDAH BERSTATUS SIAP DIKIRIM, JIKA YA, JALANKAN API ACCEPT_ORDER
                    //    DataSet dsMO = new DataSet();
                    //    dsMO = EDB.GetDataSet("Con", "SOT01A", "SELECT NO_REFERENSI FROM SOT01A WHERE NO_REFERENSI IN (" + PESANAN_DI_ELEVENIA + ") AND STATUS_TRANSAKSI='02'");
                    //    if (dsMO.Tables[0].Rows.Count > 0)
                    //    {
                    //        for (int i = 0; i < dsMO.Tables[0].Rows.Count; i++)
                    //        {
                    //            string ordNo = Convert.ToString(EDB.GetFieldValue("Con", "TEMP_ELV_ORDERS", "DELIVERY_NO='" + Convert.ToString(dsMO.Tables[0].Rows[i]["NO_REFERENSI"]) + "' AND CONN_ID='" + connId + "'", "ORDER_NO"));
                    //            string ordPrdSeq = Convert.ToString(EDB.GetFieldValue("Con", "TEMP_ELV_ORDERS", "DELIVERY_NO='" + Convert.ToString(dsMO.Tables[0].Rows[i]["NO_REFERENSI"]) + "' AND CONN_ID='" + connId + "'", "ORDER_PROD_NO"));
                    //            AcceptOrder(auth, ordNo, ordPrdSeq);
                    //        }
                    //    }
                    //    //setelah cek status order paid, cek status order packagingINP
                    //    GetOrder(auth, EleveniaController.StatusOrder.PackagingINP, connId, CUST, NAMA_CUST);
                    //}
                    //if (stat == StatusOrder.PackagingINP)
                    //{
                    //    //CEK APAKAH DI SOT01A, PESANAN TERSEBUT SUDAH BERSTATUS SUDAH DIKIRIM, JIKA YA, JALANKAN API Update Airway Bill Number
                    //    DataSet dsMO = new DataSet();
                    //    dsMO = EDB.GetDataSet("Con", "SOT01A", "SELECT NO_REFERENSI,TRACKING_SHIPMENT FROM SOT01A WHERE NO_REFERENSI IN (" + PESANAN_DI_ELEVENIA + ") AND STATUS_TRANSAKSI='03' AND ISNULL(TRACKING_SHIPMENT,'') <> ''");
                    //    if (dsMO.Tables[0].Rows.Count > 0)
                    //    {
                    //        for (int i = 0; i < dsMO.Tables[0].Rows.Count; i++)
                    //        {
                    //            DataSet dsTEMP_ELV_ORDER = new DataSet();
                    //            dsTEMP_ELV_ORDER = EDB.GetDataSet("Con", "TEMP_ELV_ORDERS", "SELECT DELIVERY_MTD_CD,DELIVERY_ETR_CD,ORDER_NO,DELIVERY_ETR_NAME,ORDER_PROD_NO FROM TEMP_ELV_ORDERS WHERE DELIVERY_NO='" + Convert.ToString(dsMO.Tables[0].Rows[i]["NO_REFERENSI"]) + "' AND CONN_ID='" + connId + "' ");
                    //            if (dsTEMP_ELV_ORDER.Tables[0].Rows.Count > 0)
                    //            {
                    //                string awb = Convert.ToString(dsMO.Tables[0].Rows[i]["TRACKING_SHIPMENT"]);
                    //                string dlvNo = Convert.ToString(dsMO.Tables[0].Rows[i]["NO_REFERENSI"]);
                    //                string dlvMthdCd = Convert.ToString(dsTEMP_ELV_ORDER.Tables[0].Rows[0]["DELIVERY_MTD_CD"]);
                    //                string dlvEtprsCd = Convert.ToString(dsTEMP_ELV_ORDER.Tables[0].Rows[0]["DELIVERY_ETR_CD"]);
                    //                string ordNo = Convert.ToString(dsTEMP_ELV_ORDER.Tables[0].Rows[0]["ORDER_NO"]);
                    //                string dlvEtprsNm = Convert.ToString(dsTEMP_ELV_ORDER.Tables[0].Rows[0]["DELIVERY_ETR_NAME"]);
                    //                string ordPrdSeq = Convert.ToString(dsTEMP_ELV_ORDER.Tables[0].Rows[0]["ORDER_PROD_NO"]);
                    //                UpdateAWBNumber(auth, awb, dlvNo, dlvMthdCd, dlvEtprsCd, ordNo, dlvEtprsNm, ordPrdSeq);
                    //            }
                    //        }
                    //    }
                    //}
                    ////end remark by calvin, dipindah ke saat ChangeStatusPesanan

                }
                else
                {
                    var res3 = new JavaScriptSerializer().Deserialize<ClientMessage>(json);
                    if (res3 != null)
                    {
                        ret.message = "error\n" + json;
                        if (stat == StatusOrder.Paid)
                        {
                            //jika tidak ada data PAID, lanjut ke PackagingINP
                            GetOrder(auth, EleveniaController.StatusOrder.PackagingINP, connId, CUST, NAMA_CUST);
                        }
                    }
                    else
                    {
                        ret.message = "unknown error";

                    }
                }
            }

            return ret;
        }
        public ClientMessage AcceptOrder(string auth, string ordNo, string ordPrdSeq)
        {
            var ret = new ClientMessage();

            var content = new System.Net.Http.StringContent("", Encoding.UTF8, "text/xml");
            Utils.HttpRequest req = new Utils.HttpRequest();
            //var result = await req.RequestJSONObjectEl(Utils.HttpRequest.PROTOCOL.Http, Utils.HttpRequest.RESTServices.rest, Utils.HttpRequest.METHOD.POST, "orderservices/orders/accept?ordNo=" + ordNo + "&ordPrdSeq=" + ordPrdSeq, content, typeof(ClientMessage), auth) as ClientMessage;
            var result = req.CallElevAPI(Utils.HttpRequest.PROTOCOL.Http, Utils.HttpRequest.RESTServices.rest, Utils.HttpRequest.METHOD.POST, "orderservices/orders/accept?ordNo=" + ordNo + "&ordPrdSeq=" + ordPrdSeq, "", typeof(ClientMessage), auth) as ClientMessage;
            if (result != null)
            {
                ret = result;
            }

            return ret;
        }
        public ClientMessage UpdateAWBNumber(string auth, string awb, string dlvNo, string dlvMthdCd, string dlvEtprsCd, string ordNo, string dlvEtprsNm, string ordPrdSeq)
        {
            var ret = new ClientMessage();

            var content = new System.Net.Http.StringContent("", Encoding.UTF8, "text/xml");
            Utils.HttpRequest req = new Utils.HttpRequest();
            //var result = await req.RequestJSONObjectEl(Utils.HttpRequest.PROTOCOL.Http, Utils.HttpRequest.RESTServices.rest, Utils.HttpRequest.METHOD.POST, "orderservices/orders/inputAwb?awb=" + awb + "&dlvNo=" + dlvNo + "&dlvMthdCd=" + dlvMthdCd + "&dlvEtprsCd=" + dlvEtprsCd + "&ordNo=" + ordNo + "&dlvEtprsNm=" + dlvEtprsNm + "&ordPrdSeq=" + ordPrdSeq, content, typeof(ClientMessage), auth) as ClientMessage;
            var result = req.CallElevAPI(Utils.HttpRequest.PROTOCOL.Http, Utils.HttpRequest.RESTServices.rest, Utils.HttpRequest.METHOD.POST, "orderservices/orders/inputAwb?awb=" + Uri.EscapeDataString(awb) + "&dlvNo=" + Uri.EscapeDataString(dlvNo) + "&dlvMthdCd=" + Uri.EscapeDataString(dlvMthdCd) + "&dlvEtprsCd=" + Uri.EscapeDataString(dlvEtprsCd) + "&ordNo=" + Uri.EscapeDataString(ordNo) + "&dlvEtprsNm=" + Uri.EscapeDataString(dlvEtprsNm) + "&ordPrdSeq=" + Uri.EscapeDataString(ordPrdSeq), "", typeof(ClientMessage), auth) as ClientMessage;
            if (result != null)
            {
                ret = result;
            }

            return ret;
        }
        public object CallBukaLapakAPI(string APIMethod, string url, string myData, string BukaLapakID, string BukaLapakToken, Type typeObj)
        {
            try
            {
                string urll = "https://api.bukalapak.com/v2/" + url;
                WebRequest myReq = WebRequest.Create(urll);
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(BukaLapakID + ":" + BukaLapakToken))));

                Stream dataStream;
                if (!string.IsNullOrEmpty(APIMethod))
                {
                    myReq.Method = APIMethod;
                    myReq.ContentType = "application/json";
                    dataStream = myReq.GetRequestStream();

                    if (!string.IsNullOrEmpty(myData))
                    {
                        dataStream.Write(Encoding.UTF8.GetBytes(myData), 0, Encoding.UTF8.GetBytes(myData).Length);
                        dataStream.Close();
                    }
                }

                //Stream dataStream = response.GetResponseStream();


                WebResponse response = myReq.GetResponse();
                //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                dataStream = response.GetResponseStream();

                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

                reader.Close();
                dataStream.Close();
                response.Close();

                dynamic retObj = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeObj);

                return retObj;
            }
            catch (Exception ex)
            {
                return null;
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
        #region Multiple Orders
        public class ElOrdersM
        {
            public OrdersM Orders { get; set; }
        }

        public class OrdersM
        {
            public List<ElOrder> order { get; set; }
        }
        #endregion
        #region Single Order
        public class ElOrderS
        {
            public OrderS Orders { get; set; }
        }

        public class OrderS
        {
            public ElOrder order { get; set; }
        }
        #endregion
        public class ElOrder
        {
            public string addPrdNo { get; set; }
            public string addPrdYn { get; set; }
            public string appmtDdDlvDy { get; set; }
            public string buyMemNo { get; set; }
            public string delvlaceSeq { get; set; }
            public string dlvCstStlTypNm { get; set; }
            public string dlvEtprsCd { get; set; }
            public string dlvEtprsNm { get; set; }
            public string dlvKdCdName { get; set; }
            public string dlvMthdCd { get; set; }
            public string dlvMthdCdNm { get; set; }
            public string dlvNo { get; set; }
            public string invcUpdateDt { get; set; }
            public string lstDlvCst { get; set; }
            public string memId { get; set; }
            public string ordDlvReqCont { get; set; }
            public string ordDt { get; set; }
            public string ordNm { get; set; }
            public string ordNo { get; set; }
            public string ordPrdSeq { get; set; }
            public string ordPrdStat { get; set; }
            public string ordQty { get; set; }
            public string ordStlEndDt { get; set; }
            public string orderAmt { get; set; }
            public Orderproduct orderProduct { get; set; }
            public string plcodrCnfDt { get; set; }
            public string prdClfCdNm { get; set; }
            public string prdNm { get; set; }
            public string prdNo { get; set; }
            public string rcvrBaseAddr { get; set; }
            public string rcvrMailNo { get; set; }
            public string rcvrNm { get; set; }
            public string rcvrPostalCode { get; set; }
            public string rcvrPrtblNo { get; set; }
            public string rcvrTlphn { get; set; }
            public string selFeeAmt { get; set; }
            public string selFeeRt { get; set; }
            public string selFixedFee { get; set; }
            public string selPrc { get; set; }
            public string sellerDscPrc { get; set; }
            public string sndPlnDd { get; set; }
            public string tmallApplyDscAmt { get; set; }
        }
        public class Orderproduct
        {
            public string advrt_stmt { get; set; }
            public string appmtDdDlvDy { get; set; }
            public string atmt_buy_cnfrm_yn { get; set; }
            public string barCode { get; set; }
            public string batchYn { get; set; }
            public string bonusDscAmt { get; set; }
            public string bonusDscGb { get; set; }
            public string bonusDscRt { get; set; }
            public string chinaSaleYn { get; set; }
            public string ctgrCupnExYn { get; set; }
            public string ctgrPntPreRt { get; set; }
            public string ctgrPntPreRtAmt { get; set; }
            public string cupnDlv { get; set; }
            public string deliveryTimeOut { get; set; }
            public string delvplaceSeq { get; set; }
            public string dlvInsOrgCst { get; set; }
            public string dlvNo { get; set; }
            public string dlvRewardAmt { get; set; }
            public string errMsg { get; set; }
            public string finalDscPrc { get; set; }
            public string firstDscAmt { get; set; }
            public string fixedDlvPrd { get; set; }
            public string giftPrdOptNo { get; set; }
            public string imgurl { get; set; }
            public string isChangePayMethod { get; set; }
            public string isHistory { get; set; }
            public string limitDt { get; set; }
            public string lowPrcCompYn { get; set; }
            public string mileDscAmt { get; set; }
            public string mileDscRt { get; set; }
            public string mileSaveAmt { get; set; }
            public string mileSaveRt { get; set; }
            public string ordPrdCpnAmtWithoutJang { get; set; }
            public string ordPrdRewardAmt { get; set; }
            public string ordPrdRewardItmCd { get; set; }
            public string ordPrdRewardItmCdNm { get; set; }
            public string ordPrdSeq { get; set; }
            public string ordPrdStat { get; set; }
            public string ordQty { get; set; }
            public string orgBonusDscRt { get; set; }
            public string pluDscAmt { get; set; }
            public string pluDscBasis { get; set; }
            public string pluDscRt { get; set; }
            public string plusDscOcbRwd { get; set; }
            public string prdNm { get; set; }
            public string prdNo { get; set; }
            public string prdOptSpcNo { get; set; }
            public string prdTtoDisconAmt { get; set; }
            public string prdTypCd { get; set; }
            public string procReturnDlvCstByBndl { get; set; }
            public string returnDlvCstByBndl { get; set; }
            public string rfndMtdCd { get; set; }
            public string selPrc { get; set; }
            public string stPntDscAmt { get; set; }
            public string stPntDscRt { get; set; }
            public string stlStat { get; set; }
            public string tiketSelPrc { get; set; }
            public string tiketTransFee { get; set; }
            public string visitDlvYn { get; set; }
        }

        //public partial class ClientMessage
        //{

        //    private string messageField;

        //    private uint productNoField;

        //    private ushort resultCodeField;

        //    /// <remarks/>
        //    public string message
        //    {
        //        get
        //        {
        //            return this.messageField;
        //        }
        //        set
        //        {
        //            this.messageField = value;
        //        }
        //    }

        //    /// <remarks/>
        //    public uint productNo
        //    {
        //        get
        //        {
        //            return this.productNoField;
        //        }
        //        set
        //        {
        //            this.productNoField = value;
        //        }
        //    }

        //    /// <remarks/>
        //    public ushort resultCode
        //    {
        //        get
        //        {
        //            return this.resultCodeField;
        //        }
        //        set
        //        {
        //            this.resultCodeField = value;
        //        }
        //    }
        //}


        #region XML Classes
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class DeliveryTemplatesTemplate
        {

            private string dlvTmpltNmField;

            private uint dlvTmpltSeqField;

            /// <remarks/>
            public string dlvTmpltNm
            {
                get
                {
                    return this.dlvTmpltNmField;
                }
                set
                {
                    this.dlvTmpltNmField = value;
                }
            }

            /// <remarks/>
            public uint dlvTmpltSeq
            {
                get
                {
                    return this.dlvTmpltSeqField;
                }
                set
                {
                    this.dlvTmpltSeqField = value;
                }
            }
        }

        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public partial class DeliveryTemplates
        {

            private DeliveryTemplatesTemplate templateField;

            /// <remarks/>
            public DeliveryTemplatesTemplate template
            {
                get
                {
                    return this.templateField;
                }
                set
                {
                    this.templateField = value;
                }
            }
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
        #endregion

        #region json Classes
        public class DeliveryTemplatesRootM
        {
            public DeliverytemplatesM DeliveryTemplates { get; set; }
        }

        public class DeliverytemplatesM
        {
            public Template[] template { get; set; }
        }
        public class DeliveryTemplatesRootS
        {
            public DeliverytemplatesS DeliveryTemplates { get; set; }
        }

        public class DeliverytemplatesS
        {
            public Template template { get; set; }
        }

        public class Template
        {
            public string dlvTmpltNm { get; set; }
            public string dlvTmpltSeq { get; set; }
        }
        #endregion


    }
}