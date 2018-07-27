using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MasterOnline.Models;
using System.Text;
using System.Data;
using MasterOnline.ViewModels;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using Erasoft.Function;
using System.Data.SqlClient;

namespace MasterOnline.Controllers
{
    public class BukaLapakController : Controller
    {
        // GET: BukaLapak
        AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        DatabaseSQL EDB;
        MoDbContext MoDbContext;

        public BukaLapakController()
        {
            MoDbContext = new MoDbContext();
            if (sessionData?.Account != null)
            {
                //if (sessionData.Account.UserId == "admin_manage")
                //    ErasoftDbContext = new ErasoftContext();
                //else
                //    ErasoftDbContext = new ErasoftContext(sessionData.Account.UserId);
                EDB = new DatabaseSQL(sessionData.Account.UserId);

            }
            else
            {
                if (sessionData?.User != null)
                {
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                    EDB = new DatabaseSQL(accFromUser.UserId);
                }
            }
        }
        [HttpPost]
        public BindingBase GetAccessKey(string cust, string email, string password)
        {
            var ret = new BindingBase();
            ret.status = 0;

            var urll = ("https://api.bukalapak.com/v2/authenticate.json");

            var myReq = HttpWebRequest.Create(urll);

            myReq.Method = "POST";
            myReq.ContentType = "application/json";
            myReq.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(email + ":" + password)));
            var myData = "{\"version\":\"1.1\",\"method\":\"\",\"params\":{\"account\":{\"type\":\"\",\"value\":\"0\"}}}";
            myReq.GetRequestStream().Write(Encoding.UTF8.GetBytes(myData), 0, Encoding.UTF8.GetBytes(myData).Count());
            var myResp = myReq.GetResponse();
            var myreader = new System.IO.StreamReader(myResp.GetResponseStream());
            //Dim myText As String;
            var stringRet = myreader.ReadToEnd();
            if (!string.IsNullOrEmpty(stringRet))
            {
                AccessKeyBL retObj = JsonConvert.DeserializeObject(stringRet, typeof(AccessKeyBL)) as AccessKeyBL;
                if (retObj != null)
                {
                    if (retObj.status.Equals("OK"))
                    {
                        //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
                        ret.status = 1;
                        string username = sessionData.Account.Username;

                        EDB.ExecuteSQL("ARConnectionString", CommandType.Text, "UPDATE ARF01 SET API_KEY='" + retObj.user_id + "', TOKEN='" + retObj.token + "' WHERE CUST ='" + cust + "'");
                        var a = EDB.GetDataSet("ARConnectionString", "ARF01", "SELECT * FROM ARF01");
                    }
                    else
                    {
                        ret.message = retObj.message;
                    }
                }
                else
                {
                    ret.message = "Failed to call Buka Lapak API";
                }
            }

            return ret;
        }
        [HttpPost]
        public BindingBase CreateProduct(BrgViewModel data)
        {
            var ret = new BindingBase();
            ret.status = 0;
            //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);

            var dataProduct = new BindingBukaLapakProduct
            {
                images = data.imageId,
                product = new ProductBukaLpk
                {
                    category_id = "564",
                    name = data.nama,
                    @new = "true",
                    price = data.harga,
                    negotiable = "false",
                    weight = data.weight,//weight in gram
                    stock = data.qty,
                    description_bb = data.deskripsi,
                    product_detail_attributes = new Product_Detail_Attributes
                    {
                        bahan = "baku",
                        merek = data.merk,
                        tipe = "formal",
                        //type = "",
                        ukuran = "S",
                    }
                }
            };
            if (!string.IsNullOrEmpty(data.nama2))
            {
                dataProduct.product.name += " " + data.nama2;
            }
            if (!string.IsNullOrEmpty(data.imageId2))
            {
                if (!string.IsNullOrEmpty(dataProduct.images))
                    dataProduct.images += ",";
                dataProduct.images += data.imageId2;
            }
            if (!string.IsNullOrEmpty(data.imageId3))
            {
                if (!string.IsNullOrEmpty(dataProduct.images))
                    dataProduct.images += ",";
                dataProduct.images += "," + data.imageId3;
            }
            //string hargaMarket = EDB.GetFieldValue("", "STF02H", "BRG = '" + data.kdBrg + "' AND AKUNMARKET = '" + data.akunMarket + "'", "HJUAL").ToString();
            //if (!string.IsNullOrEmpty(hargaMarket))
            //    dataProduct.product.price = hargaMarket;
            string dataPost = JsonConvert.SerializeObject(dataProduct);
            Utils.HttpRequest req = new Utils.HttpRequest();
            var bindResponse = req.CallBukaLapakAPI("POST", "products.json", dataPost, data.key, data.token, typeof(CreateProductBukaLapak)) as CreateProductBukaLapak;
            if (bindResponse != null)
            {
                if (bindResponse.status.Equals("OK"))
                {
                    ret.status = 1;
                    ret.message = bindResponse.product_detail.id;
                    EDB.ExecuteSQL("", CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + bindResponse.product_detail.id + "' WHERE BRG = '" + data.kdBrg + "' AND IDMARKET = '" + data.idMarket + "'");
                }
                else
                {
                    ret.message = bindResponse.message;
                }
            }
            else
            {
                ret.message = "Failed to call Buka Lapak API";
            }
            return ret;
        }

        [HttpGet]
        public BindingBase uploadGambar(string imagePath, string userId, string token)
        {
            //ukuran minimum gambar adalah 300x300
            BindingBase ret = new BindingBase();
            ret.status = 0;

            string URL = "https://api.bukalapak.com/v2/images.json";
            string post_data = imagePath;//"D:\\kaos.jpg"; //alamat file

            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");

            WebRequest myReq = WebRequest.Create(URL);
            myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(userId + ":" + token))));

            myReq.Method = "POST";
            myReq.ContentType = "multipart/form-data; boundary=" + boundary;

            Stream postDataStream = GetPostStream(post_data, boundary);

            myReq.ContentLength = postDataStream.Length;
            Stream reqStream = myReq.GetRequestStream();

            postDataStream.Position = 0;

            byte[] buffer = new byte[1024];
            int bytesRead = 0;

            while ((bytesRead = postDataStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                reqStream.Write(buffer, 0, bytesRead);
            }

            postDataStream.Close();
            reqStream.Close();
            try
            {
                StreamReader sr = new StreamReader(myReq.GetResponse().GetResponseStream());
                var stringRes = JsonConvert.DeserializeObject(sr.ReadToEnd(), typeof(BukaLapakRes)) as BukaLapakRes;
                if (stringRes != null)
                {
                    if (stringRes.status.Equals("OK"))
                    {
                        ret.status = 1;
                        ret.message = stringRes.id;
                    }
                    else
                    {
                        ret.message = stringRes.message;
                    }
                }
                else
                {
                    ret.message = "Failed to call Buka Lapak API";
                }
            }
            catch (Exception ex)
            {
                ret.message = ex.ToString();
                return ret;
            }

            return ret;
        }

        private static Stream GetPostStream(string filePath, string boundary)
        {
            Stream postDataStream = new System.IO.MemoryStream();

            FileInfo fileInfo = new FileInfo(filePath);

            string fileHeaderTemplate = Environment.NewLine + "--" + boundary + Environment.NewLine +
            "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"" +
            Environment.NewLine + "Content-Type: multipart/form-data" + Environment.NewLine + Environment.NewLine;

            byte[] fileHeaderBytes = System.Text.Encoding.UTF8.GetBytes(string.Format(fileHeaderTemplate,
            "file", fileInfo.FullName));

            postDataStream.Write(fileHeaderBytes, 0, fileHeaderBytes.Length);

            FileStream fileStream = fileInfo.OpenRead();

            byte[] buffer = new byte[1024];

            int bytesRead = 0;

            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                postDataStream.Write(buffer, 0, bytesRead);
            }

            fileStream.Close();

            byte[] endBoundaryBytes = System.Text.Encoding.UTF8.GetBytes("--" + boundary + "--");
            postDataStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);

            return postDataStream;
        }

        [HttpGet]
        public CreateProductBukaLapak updateProduk(string brgMp, string price, string stock, string userId, string token)
        {

            var ret = new CreateProductBukaLapak();
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
            ret = req.CallBukaLapakAPI("PUT", "products/" + brgMp + ".json", Myprod, userId, token, typeof(CreateProductBukaLapak)) as CreateProductBukaLapak;
            return ret;
        }

        [HttpGet]
        public BindingBase updateProdukStat(string kdBrg, string id, string akunMarket, bool stat, string userId, string token)
        {
            var ret = new BindingBase { status = 0 };
            Utils.HttpRequest req = new Utils.HttpRequest();
            BindingProductBL response = req.CallBukaLapakAPI("", "products/" + id + ".json", "", userId, token, typeof(BindingProductBL)) as BindingProductBL;
            if (response != null)
            {
                if (response.status.ToString().Equals("OK"))
                {
                    if (stat != Convert.ToBoolean(response.product.active))
                    {
                        if (stat)
                        {
                            var res = prodAktif(id, userId, token);
                            ret.status = res.status;
                            ret.message = res.message;

                        }
                        else
                        {
                            var res = prodNonAktif(id, userId, token);
                            ret.status = res.status;
                            ret.message = res.message;
                        }
                    }
                }
                else//no product on bukalapak > create new
                {
                    //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
                    var dsSTF02 = EDB.GetDataSet("MOConnectionString", "STF02", "SELECT * FROM STF02 WHERE BRG = '" + kdBrg + "'");
                    if (dsSTF02.Tables[0].Rows.Count > 0)
                    {
                        dsSTF02 = EDB.GetDataSet("MOConnectionString", "STF02H", "SELECT * FROM STF02H WHERE BRG = '" + kdBrg + "' AND AKUN_MARKET = '" + akunMarket + "'");

                        BrgViewModel dataBrg = new BrgViewModel
                        {
                            nama = dsSTF02.Tables["STF02"].Rows[0]["NAMA"].ToString(),
                            weight = dsSTF02.Tables["STF02"].Rows[0]["NAMA"].ToString(),
                            qty = "1",
                            deskripsi = dsSTF02.Tables["STF02"].Rows[0]["DESKRIPSI"].ToString(),
                            merk = dsSTF02.Tables["STF02"].Rows[0]["KET_SORT2"].ToString(),

                        };
                        if (dsSTF02.Tables["STF02H"].Rows.Count > 0)
                        {
                            dataBrg.harga = dsSTF02.Tables["STF02H"].Rows[0]["HJUAL"].ToString();
                        }
                        else
                        {
                            //tidak ada data barang dengan brg dan marketplace tsb, gunakan harga brg dr marketplace lain
                            dsSTF02 = EDB.GetDataSet("", "STF02H", "SELECT * FROM STF02H WHERE BRG = '" + kdBrg + "'");
                            if (dsSTF02.Tables["STF02H"].Rows.Count > 0)
                            {
                                dataBrg.harga = dsSTF02.Tables["STF02H"].Rows[0]["HJUAL"].ToString();
                            }
                        }
                        string path = "C:\\MasterOnline\\Content\\Uploaded\\";
                        string fileName = "FotoProduk-" + dsSTF02.Tables["STF02"].Rows[0]["USERNAME"].ToString() + "-" + kdBrg + "-foto-";
                        string[] files = System.IO.Directory.GetFiles(path, "*-" + kdBrg + "-foto-1.*", System.IO.SearchOption.TopDirectoryOnly);
                        if (files.Length > 0)
                        {
                            var a = uploadGambar(path + fileName + "1", userId, token);
                            dataBrg.imageId = a.message;
                        }
                        files = System.IO.Directory.GetFiles(path, "*-" + kdBrg + "-foto-2.*", System.IO.SearchOption.TopDirectoryOnly);
                        if (files.Length > 0)
                        {
                            var a = uploadGambar(path + fileName + "2", userId, token);
                            dataBrg.imageId2 = a.message;
                        }
                        files = System.IO.Directory.GetFiles(path, "*-" + kdBrg + "-foto-3.*", System.IO.SearchOption.TopDirectoryOnly);
                        if (files.Length > 0)
                        {
                            var a = uploadGambar(path + fileName + "3", userId, token);
                            dataBrg.imageId3 = a.message;
                        }
                        var res = CreateProduct(dataBrg);
                    }

                }
            }
            else
            {
                ret.message = "Failed to call Buka Lapak API";
            }

            return ret;
        }

        public BindingBase prodNonAktif(string id, string userId, string token)
        {
            var ret = new BindingBase { status = 0 };
            Utils.HttpRequest req = new Utils.HttpRequest();
            ResProduct response = req.CallBukaLapakAPI("PUT", "products/" + id + "/sold.json", "", userId, token, typeof(ResProduct)) as ResProduct;
            if (response != null)
            {
                if (response.status.ToString().Equals("OK"))
                {
                    ret.status = 1;
                }
                else
                {
                    ret.message = response.message;
                }
            }
            else
            {
                ret.message = "Failed to call Buka Lapak API";
            }
            return ret;
        }


        public BindingBase prodAktif(string id, string userId, string token)
        {
            var ret = new BindingBase { status = 0 };
            Utils.HttpRequest req = new Utils.HttpRequest();
            ResProduct response = req.CallBukaLapakAPI("PUT", "products/" + id + "/relist.json", "", userId, token, typeof(ResProduct)) as ResProduct;
            if (response != null)
            {
                if (response.status.ToString().Equals("OK"))
                {
                    ret.status = 1;
                }
                else
                {
                    ret.message = response.message;
                }
            }
            else
            {
                ret.message = "Failed to call Buka Lapak API";
            }
            return ret;
        }

        [HttpGet]
        public BindingBase cekTransaksi(/*string transId,*/ string Cust, string email, string userId, string token, string connectionID)
        {
            //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
            var ret = new BindingBase();
            ret.status = 0;

            Utils.HttpRequest req = new Utils.HttpRequest();
            string url = "transactions.json";
            //if (!string.IsNullOrEmpty(transId))
            //    url = "transactions/" + transId + ".json";
            var bindOrder = req.CallBukaLapakAPI("", url, "", userId, token, typeof(BukaLapakOrder)) as BukaLapakOrder;
            if (bindOrder != null)
            {
                //ret = bindOrder;
                if (bindOrder.status.Equals("OK"))
                {
                    string insertQ = "INSERT INTO TEMP_BL_ORDER ([ID],[INVOICE_ID],[STATE],[TRANSACTION_ID],[AMOUNT],[QUANTITY],[COURIER],[BUYERS_NOTE],[SHIPPING_FEE],";
                    insertQ += "[SHIPPING_ID],[SHIPPING_CODE],[SHIPPING_SERVICE],[SUBTOTAL_AMOUNT],[TOTAL_AMOUNT],[PAYMENT_AMOUNT],[CREATED_AT],[UPDATED_AT],";
                    insertQ += "[BUYER_EMAIL],[BUYER_ID],[BUYER_NAME],[BUYER_USERNAME],[BUYER_LOGISTIC_CHOICE],[CONSIGNEE_ADDRESS],[CONSIGNEE_AREA],[CONSIGNEE_CITY],";
                    insertQ += "[CONSIGNEE_NAME],[CONSIGNEE_PHONE],[CONSIGNEE_POSTCODE],[CONSIGNEE_PROVICE],[CUST],[USERNAME],[CONNECTION_ID]) VALUES ";

                    string insertOrderItems = "INSERT INTO TEMP_BL_ORDERITEMS ([ID],[TRANSACTION_ID],[PRODUCT_ID],[CATEGORY],[CATEGORY_ID],[NAME]";
                    insertOrderItems += ",[PRICE],[WEIGHT],[DESC],[CONDITON],[STOCK],[QTY], [CREATED_AT], [UPDATED_AT], [USERNAME], [CONNECTION_ID]) VALUES ";

                    string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                    insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                    insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV, CONNECTION_ID) VALUES ";
                    //int i = 1;
                    var connIDARF01C = Guid.NewGuid().ToString();
                    string username = sessionData.Account.Username;

                    foreach (Transaction order in bindOrder.transactions)
                    {
                        if (!order.buyer.email.Equals(email))//cek email pembeli != email user untuk mendapatkan order penjualan
                        {
                            var statusEra = "";
                            switch (order.state.ToString().ToLower())
                            {
                                case "pending":
                                case "addressed":
                                case "payment_chosen":
                                case "confirm_payment":
                                case "paid":
                                    statusEra = "01";
                                    break;
                                case "accepted":
                                    statusEra = "02";
                                    break;
                                case "delivered":
                                    statusEra = "03";
                                    break;
                                case "received":
                                //statusEra = "03";
                                //break;
                                case "remitted":
                                    statusEra = "04";
                                    break;
                                case "rejected":
                                case "expired":
                                case "cancelled":
                                //statusEra = "11";
                                //break;
                                case "refunded":
                                    statusEra = "11";
                                    break;
                                default:
                                    statusEra = "99";
                                    break;
                            }
                            //jika status pesanan sudah diubah di mo, dari 01 -> 02, status tidak dikembalikan ke 01
                            if (statusEra == "01")
                            {
                                var currentStatus = EDB.GetFieldValue("", "SOT01A", "NO_REFERENSI = '" + order.id + "'", "STATUS_TRANSAKSI").ToString();
                                if (!string.IsNullOrEmpty(currentStatus))
                                    if (currentStatus == "02")
                                        statusEra = "02";
                            }
                            //end jika status pesanan sudah diubah di mo, dari 01 -> 02, status tidak dikembalikan ke 01

                            insertQ += "(" + order.id + "," + order.invoice_id + ",'" + statusEra + "','" + order.transaction_id + "'," + order.amount + "," + order.quantity + ",'" + order.courier + "','" + order.buyer_notes + "'," + order.shipping_fee + ",";
                            insertQ += order.shipping_id + ",'" + order.shipping_code + "','" + order.shipping_service + "'," + order.subtotal_amount + "," + order.total_amount + "," + order.payment_amount + ",'" + Convert.ToDateTime(order.created_at).ToString("yyyy-MM-dd HH:mm:ss") + "','" + Convert.ToDateTime(order.updated_at).ToString("yyyy-MM-dd HH:mm:ss") + "','";
                            insertQ += order.buyer.email + "','" + order.buyer.id + "','" + order.buyer.name + "','" + order.buyer.username + "','" + order.buyer_logistic_choice + "','" + order.consignee.address + "','" + order.consignee.area + "','" + order.consignee.city + "','";
                            insertQ += order.consignee.name + "','" + order.consignee.phone + "','" + order.consignee.post_code + "','" + order.consignee.province + "','" + Cust + "','" + username + "','" + connectionID + "')";

                            if (order.products != null)
                            {
                                foreach (ProductBukaLapak items in order.products)
                                {
                                    var ds = EDB.GetDataSet("MOConnectionString", "0", "SELECT STF02.NAMA AS NAMA_BRG FROM STF02H S INNER JOIN ARF01 A ON S.AKUNMARKET = A.PERSO INNER JOIN STF02 ON S.BRG = STF02.BRG WHERE BRG_MP = '" + items.id + "' AND CUST = '" + Cust + "'");
                                    string namaBrg = "";
                                    if (ds.Tables[0].Rows.Count > 0)
                                    {
                                        namaBrg = ds.Tables[0].Rows[0]["NAMA_BRG"].ToString();
                                    }
                                    insertOrderItems += "(" + order.id + ", '" + order.transaction_id + "','" + items.id + "','" + items.category + "'," + items.category_id + ",'" + namaBrg + "',";
                                    insertOrderItems += items.price + "," + items.weight + ",'" + items.desc + "','" + items.condition + "'," + items.stock + "," + items.order_quantity + ",'" + Convert.ToDateTime(order.created_at).ToString("yyyy-MM-dd HH:mm:ss") + "','" + Convert.ToDateTime(order.updated_at).ToString("yyyy-MM-dd HH:mm:ss") + "','" + username + "','" + connectionID + "')";
                                    insertOrderItems += " ,";
                                }
                            }

                            var tblKabKot = EDB.GetDataSet("MOConnectionString", "KabupatenKota", "SELECT TOP 1 * FROM KabupatenKota WHERE NamaKabKot LIKE '%" + order.consignee.city + "%'");
                            var tblProv = EDB.GetDataSet("MOConnectionString", "Provinsi", "SELECT TOP 1 * FROM Provinsi WHERE NamaProv LIKE '%" + order.consignee.province + "%'");

                            var kabKot = "3174";//set default value jika tidak ada di db
                            var prov = "31";//set default value jika tidak ada di db

                            if (tblProv.Tables[0].Rows.Count > 0)
                                prov = tblProv.Tables[0].Rows[0]["KodeProv"].ToString();
                            if (tblKabKot.Tables[0].Rows.Count > 0)
                                kabKot = tblKabKot.Tables[0].Rows[0]["KodeKabKot"].ToString();

                            insertPembeli += "('" + order.buyer.name + "','" + order.consignee.address + "','" + order.consignee.phone + "','" + order.buyer.email + "',0,0,'0','01',";
                            insertPembeli += "1, 'IDR', '01', '" + order.consignee.address + "', 0, 0, 0, 0, '1', 0, 0, ";
                            insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + order.consignee.post_code + "', '" + order.buyer.email + "', '" + kabKot + "', '" + prov + "', '" + order.consignee.city + "', '" + order.consignee.province + "', '" + connIDARF01C + "')";

                            //if (i < bindOrder.transactions.Length)
                            insertQ += " ,";
                            insertPembeli += " ,";
                        }

                        //i = i + 1;
                    }
                    insertQ = insertQ.Substring(0, insertQ.Length - 2);
                    var a = EDB.ExecuteSQL(username, CommandType.Text, insertQ);

                    insertOrderItems = insertOrderItems.Substring(0, insertOrderItems.Length - 2);
                    a = EDB.ExecuteSQL(username, CommandType.Text, insertOrderItems);

                    insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 2);
                    a = EDB.ExecuteSQL(username, CommandType.Text, insertPembeli);

                    ret.status = 1;
                    ret.message = a.ToString();

                    #region call sp
                    SqlCommand CommandSQL = new SqlCommand();

                    //add by Tri call sp to insert buyer data
                    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIDARF01C;

                    EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);
                    //end add by Tri call sp to insert buyer data

                    CommandSQL = new SqlCommand();
                    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;

                    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;
                    CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
                    CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 1;
                    CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;

                    EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);
                    #endregion
                }
                else
                {
                    ret.message = bindOrder.message;
                }
            }
            else
            {
                ret.message = "failed to call buka lapak api";
            }

            return ret;
        }

        [HttpGet]
        public BindingBase KonfirmasiPengiriman(/*string noBukti,*/ string shipCode, string transId, string courier, string userId, string token)
        {
            var ret = new BindingBase();
            ret.status = 0;

            var data = new BindingShipBL
            {
                payment_shipping = new ShippingBukaLapak
                {
                    shipping_code = shipCode,
                    transaction_id = transId,
                }
            };
            if (courier.ToUpper().Contains("POS") || courier.ToUpper().Contains("TIKI") || courier.ToUpper().Contains("JNE"))
            {
                //tidak perlu ditambahkan nama courier
            }
            else
            {
                data.payment_shipping.new_courier = courier;
            }
            string dataPost = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            Utils.HttpRequest req = new Utils.HttpRequest();
            var bindStatus = req.CallBukaLapakAPI("POST", "transactions/confirm_shipping.json", dataPost, userId, token, typeof(BukaLapakRes)) as BukaLapakRes;
            if (bindStatus != null)
            {
                if (bindStatus.status.Equals("OK"))
                {
                    string username = sessionData.Account.Username;
                    //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
                    ret.status = 1;
                    //change status menjadi  04 => shipped
                    //EDB.ExecuteSQL("", CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '04' WHERE NO_REFERENSI = '" + noBukti + "'");
                }
                else
                {
                    ret.message = bindStatus.message;
                }
            }
            else
            {
                ret.message = "failed to call Buka Lapak api";
            }

            return ret;
        }

        [HttpGet]
        public BindingBase CancelOrder(string noBukti, string transId, string userId, string token)
        {
            var ret = new BindingBase();
            string Myprod = "{ \"data\": { \"id\":\"" + transId + "\", \"payment_rejection[reason]\":\"Stok Habis\" } }";
            /* REASON (case sensitive) :
             * Stok Habis
             * Harga barang/biaya kirim tidak sesuai
             * Ada kesibukan lain yang sifatnya mendadak
             * Permintaan pembeli tidak dapat dilayani
            */
            Utils.HttpRequest req = new Utils.HttpRequest();
            var bindCancel = req.CallBukaLapakAPI("PUT", "transactions/reject.json", Myprod, userId, token, typeof(BukaLapakRes)) as BukaLapakRes;
            if (bindCancel != null)
            {
                if (bindCancel.status.Equals("OK"))
                {
                    string username = sessionData.Account.Username;
                    //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
                    ret.status = 1;
                    //change status menjadi  11 => cancelled
                    EDB.ExecuteSQL("", CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '11' WHERE NO_REFERENSI = '" + noBukti + "'");
                }
                else
                {
                    ret.message = bindCancel.message;
                }
            }
            else
            {
                ret.message = "failed to call Buka Lapak api";
            }

            return ret;
        }
    }
}