using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Xml.Serialization;
using System.Text;

namespace MasterOnline.Utils
{
    public class HttpRequest
    {
        public const string REST_URL_API = "https://api.sellercenter.lazada.co.id";

        public enum RESTServices
        {
            v2 = 1,
            GetCode = 2,
            rest = 3,
            v1 = 4,
        }

        public enum METHOD
        {
            POST = 1,
            GET = 2,
            PUT = 3
        }
        public enum PROTOCOL
        {
            Http = 1,
            Https = 2
        }

        public HttpRequest()
        {
        }
        public async Task<string> UploadFile(RESTServices service, string RouteMap, KeyValuePair<string, string>[] param, string FileName, byte[] data)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    using (var content = new MultipartFormDataContent())
                    {
                        var values = param;

                        foreach (var keyValuePair in values)
                        {
                            content.Add(new StringContent(keyValuePair.Value), keyValuePair.Key);
                        }

                        var fileContent = new ByteArrayContent(data);
                        fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = FileName
                        };
                        content.Add(fileContent);

                        //var requestUri = "/api/action";
                        //var result = client.PostAsync(requestUri, content).Result;

                        string url = "";
                        url = string.Format(REST_URL_API, string.Empty) + service.ToString() + "/" + RouteMap;

                        client.BaseAddress = new Uri(url);

                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        HttpResponseMessage response = null;
                        response = await client.PostAsync(url, content);
                        if (response != null)
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                var retcontent = await response.Content.ReadAsStringAsync();
                                return retcontent;
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return "";
        }
        public async Task<object> UploadJSONObject(RESTServices service, string RouteMap, KeyValuePair<string, string>[] param, string FileName, byte[] data, Type retObject)
        {
            try
            {
                string ret = await UploadFile(service, RouteMap, param, FileName, data);
                if (ret != "")
                {
                    dynamic retObj = Newtonsoft.Json.JsonConvert.DeserializeObject(ret, retObject);
                    var retLogin = await ValidateResponse(ret, RouteMap);
                    if (retLogin)
                    {
                        return retObj;
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
            //string ret = await UploadFile(service, RouteMap, param, FileName, data);
            //if (ret != "")
            //{
            //    return Newtonsoft.Json.JsonConvert.DeserializeObject(ret, retObject);
            //}
            //return null;
        }

        public async Task<bool> ValidateResponse(string Response, string RouteMap)
        {
            return true;
            //try
            //{
            //    dynamic retObjLogin = Newtonsoft.Json.Linq.JObject.Parse(Response);
            //    var LogIn = Convert.ToBoolean(retObjLogin["StatusLogIn"]);
            //    if (App.PageHome != null)
            //    {
            //        if (LogIn == false && RouteMap != "Login")
            //        {
            //            await App.PageHome.DisplayAlert("Logout", "Sesi login telah berakhir. Silahkan login kembali.", "Ok");
            //            App.PageHome.HideDialog();
            //            App.CARDatabase.Connection.DeleteAll(typeof(Users));
            //            App.CARDatabase.Connection.DeleteAll(typeof(TableDaftar));
            //            App.CARDatabase.Connection.DeleteAll(typeof(TableJaringan));
            //            App.CARDatabase.Connection.DeleteAll(typeof(TableStaterKit));
            //            LogIn login = new LogIn();

            //            App.PageHome.Navigation.InsertPageBefore(login, App.PageHome.Navigation.NavigationStack[0]);
            //            if (App.Page != null)
            //            {
            //                try
            //                {
            //                    await App.Page.Navigation.PopModalAsync();
            //                    await App.Page.Navigation.PopAsync();
            //                }
            //                catch { }
            //            }
            //            await App.PageHome.Navigation.PopToRootAsync();
            //            App.PageHome = null;
            //            return false;
            //        }
            //    }
            //}
            //catch { }
            //return true;
        }

        public async Task<object> RequestJSONObject(RESTServices service, METHOD method, string RouteMap, string Parameter, Type retObject)
        {
            try
            {
                string ret = await RequestString(service, method, RouteMap, Parameter);
                if (ret != "")
                {
                    dynamic retObj = Newtonsoft.Json.JsonConvert.DeserializeObject(ret, retObject);
                    var retLogin = await ValidateResponse(ret, RouteMap);
                    if (retLogin)
                    {
                        return retObj;
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        public async Task<object> RequestJSONObject(RESTServices service, string RouteMap, HttpContent Content, Type retObject)
        {
            try
            {
                string ret = await RequestString(service, RouteMap, Content);
                if (ret != "")
                {
                    dynamic retObj = Newtonsoft.Json.JsonConvert.DeserializeObject(ret, retObject);
                    var retLogin = await ValidateResponse(ret, RouteMap);
                    if (retLogin)
                    {
                        return retObj;
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
            //string ret = await RequestString(service, RouteMap, Content);
            //if (ret != "")
            //{
            //    return Newtonsoft.Json.JsonConvert.DeserializeObject(ret, retObject);
            //}
            //return null;
        }
        public async Task<string> RequestString(RESTServices service, METHOD method, string RouteMap, string Parameter)
        {
            try
            {
                var client = new System.Net.Http.HttpClient();

                string url = string.Format(REST_URL_API, string.Empty) + service.ToString() + "/" + RouteMap;
                //string url = string.Format(REST_URL_API, string.Empty) + service.ToString();
                client.BaseAddress = new Uri(url);

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = null;
                switch (method)
                {
                    case METHOD.GET:
                        url += "?" + Parameter;
                        response = await client.GetAsync(url);
                        break;
                    case METHOD.POST:
                        response = await client.PostAsync(url, new StringContent(Parameter));
                        break;
                }
                if (response != null)
                {

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        client.Dispose();
                        response.Dispose();
                        return content;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return "";
        }
        public async Task<string> RequestString(RESTServices service, string RouteMap, HttpContent Content)
        {
            try
            {
                var client = new System.Net.Http.HttpClient();

                //string url = string.Format(REST_URL_API, string.Empty) + service.ToString() + "/" + RouteMap;
                string url = "https://api.bukalapak.com/v1/authenticate.json";
                client.BaseAddress = new Uri(url);

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = null;

                //response.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
                //response.Content.Headers.Add("Authorization", "AUTH_STRING");

                response = await client.PostAsync(url, Content);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        client.Dispose();
                        response.Dispose();
                        return content;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return "";
        }

        #region lazada
        public async Task<object> RequestJSONObject(METHOD method, string Parameter, Type retObject)
        {
            try
            {
                string ret = await RequestString(method, Parameter);
                if (ret != "")
                {
                    dynamic retObj = Newtonsoft.Json.JsonConvert.DeserializeObject(ret, retObject);
                    //var retLogin = await ValidateResponse(ret, RouteMap);
                    //if (retLogin)
                    //{
                    return retObj;
                    //}
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<string> RequestString(METHOD method, string Parameter)
        {
            try
            {
                var client = new System.Net.Http.HttpClient();

                string url = string.Format(REST_URL_API, string.Empty);
                //string url = string.Format(REST_URL_API, string.Empty) + service.ToString();
                client.BaseAddress = new Uri(url);

                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = null;
                switch (method)
                {
                    case METHOD.GET:
                        url += "?" + Parameter;
                        response = await client.GetAsync(url);
                        break;
                    case METHOD.POST:
                        response = await client.PostAsync(url, new StringContent(Parameter));
                        break;
                }
                if (response != null)
                {

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        client.Dispose();
                        response.Dispose();
                        return content;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return "";
        }

        public async Task<object> RequestJSONObject(string urlParam, StringContent Parameter, Type retObject)
        {
            try
            {
                string ret = await RequestString(urlParam, Parameter);
                if (ret != "")
                {
                    dynamic retObj = Newtonsoft.Json.JsonConvert.DeserializeObject(ret, retObject);
                    //var retLogin = await ValidateResponse(ret, RouteMap);
                    //if (retLogin)
                    //{
                    return retObj;
                    //}
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<string> RequestString(string urlParam, StringContent Parameter)
        {
            try
            {
                var client = new System.Net.Http.HttpClient();

                string url = string.Format(REST_URL_API, string.Empty) + "?" + urlParam;
                client.BaseAddress = new Uri(url);

                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = null;

                //response.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
                //response.Content.Headers.Add("Authorization", "AUTH_STRING");

                response = await client.PostAsync(url, Parameter);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        client.Dispose();
                        response.Dispose();
                        return content;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return "";
        }
        public async Task<string> Upload(string actionUrl, byte[] paramFileBytes /*Stream paramFileStream*/)
        {
            //HttpContent stringContent = new StringContent(paramString);
            //HttpContent fileStreamContent = new StreamContent(paramFileStream);
            HttpContent bytesContent = new ByteArrayContent(paramFileBytes);
            using (var client = new HttpClient())
            using (var formData = new MultipartFormDataContent())
            {
                //formData.Add(stringContent, "param1", "param1");
                //formData.Add(fileStreamContent, "file1", "file1");
                formData.Add(bytesContent, "image", "file2");
                var response = client.PostAsync(REST_URL_API + "?" + actionUrl, bytesContent).Result;
                //if (!response.IsSuccessStatusCode)
                //{
                //    return null;
                //}
                //return response.Content.ReadAsStreamAsync().Result;
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    client.Dispose();
                    response.Dispose();
                    return content;
                }
            }
            return "";
        }
        #endregion

        #region elevenia
        public async Task<object> RequestJSONObjectEl(RESTServices service, string RouteMap, HttpContent Content, Type retObject, string Auth)
        {
            try
            {
                string ret = await RequestStringEl(service, RouteMap, Content, Auth);
                if (ret != "")
                {
                    XmlSerializer serializer = new XmlSerializer(retObject);
                    StringReader rdr = new StringReader(ret);
                    dynamic retObj = serializer.Deserialize(rdr);
                    return retObj;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        public async Task<string> RequestStringEl(RESTServices service, string RouteMap, HttpContent Content, string Auth)
        {
            try
            {
                var client = new System.Net.Http.HttpClient();
                //string url = string.Format(REST_URL_API, string.Empty) + service.ToString() + "/" + RouteMap;
                string url = string.Format("http://api.elevenia.co.id/", string.Empty) + service.ToString() + "/" + RouteMap;

                client.BaseAddress = new Uri(url);

                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

                //add request header
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Auth);
                client.DefaultRequestHeaders.TryAddWithoutValidation("openapikey", Auth);
                //client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                //end add

                HttpResponseMessage response = null;
                //Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");//content-type change sucsess!
                Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/xml");//content-type change sucsess!
                response = await client.PostAsync(url, Content);
                //response.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
                if (response != null)
                {
                    if (response.IsSuccessStatusCode || HttpStatusCode.BadRequest == response.StatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        client.Dispose();
                        response.Dispose();
                        return content;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return "";
        }
        //method get
        public async Task<object> RequestJSONObjectEl(PROTOCOL protocol, RESTServices service, METHOD method, string RouteMap, HttpContent Parameter, Type retObject, string Auth)
        {
            try
            {
                string ret = await RequestStringEl(protocol, service, method, RouteMap, Parameter, Auth);
                if (ret != "")
                {
                    if (retObject == typeof(string))
                    {
                        return ret;
                    }
                    else
                    {
                        XmlSerializer serializer = new XmlSerializer(retObject);
                        StringReader rdr = new StringReader(ret);
                        dynamic retObj = serializer.Deserialize(rdr);
                        return retObj;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<string> RequestStringEl(PROTOCOL protocol, RESTServices service, METHOD method, string RouteMap, HttpContent Parameter, string Auth)
        {
            try
            {
                var client = new System.Net.Http.HttpClient();

                //string url = string.Format(REST_URL_API, string.Empty) + service.ToString() + "/" + RouteMap;
                string url = string.Format(protocol + "://api.elevenia.co.id/", string.Empty) + service.ToString() + "/" + RouteMap;
                client.BaseAddress = new Uri(url);

                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                //add request header
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Auth);
                client.DefaultRequestHeaders.TryAddWithoutValidation("openapikey", Auth);
                //client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
                //end add

                HttpResponseMessage response = null;
                //Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/xml");//content-type change sucsess!
                switch (method)
                {
                    case METHOD.GET:
                        //if (!string.IsNullOrEmpty(Parameter))
                        //    url += "?" + Parameter;
                        response = await client.GetAsync(url);
                        break;
                    case METHOD.POST:
                        //response = await client.PostAsync(url, new StringContent(Parameter));
                        Parameter.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/xml");//content-type change sucsess!
                        response = await client.PostAsync(url, Parameter);
                        break;
                    case METHOD.PUT:
                        //var content = new StringContent(Parameter, Encoding.UTF8, "text/xml");
                        //content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/xml");
                        //response = await client.PutAsync(url, content);
                        Parameter.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/xml");//content-type change sucsess!
                        response = await client.PutAsync(url, Parameter);
                        break;
                }
                if (response != null)
                {
                    if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        client.Dispose();
                        response.Dispose();
                        return content;
                    }
                }
            }
            catch (Exception ex)
            {
                var a = ex;
            }
            return "";
        }

        public object CallElevAPI(PROTOCOL protocol, RESTServices service, METHOD method, string RouteMap, string myData, Type retObject, string Auth)
        {
            try
            {
                string url = string.Format(protocol + "://api.elevenia.co.id/", string.Empty) + service.ToString() + "/" + RouteMap;

                WebRequest myReq = WebRequest.Create(url);
                myReq.Headers.Add("openapikey", Auth);

                Stream dataStream;
                switch (method)
                {
                    case METHOD.GET:
                        myReq.Method = "GET";
                        break;
                    case METHOD.POST:
                        myReq.Method = "POST";
                        myReq.ContentType = "application/xml";
                        dataStream = myReq.GetRequestStream();
                        if (!string.IsNullOrEmpty(myData))
                        {
                            dataStream.Write(Encoding.UTF8.GetBytes(myData), 0, Encoding.UTF8.GetBytes(myData).Length);
                            dataStream.Close();
                        }
                        break;
                    case METHOD.PUT:
                        myReq.Method = "PUT";
                        myReq.ContentType = "application/xml";
                        dataStream = myReq.GetRequestStream();
                        if (!string.IsNullOrEmpty(myData))
                        {
                            dataStream.Write(Encoding.UTF8.GetBytes(myData), 0, Encoding.UTF8.GetBytes(myData).Length);
                            dataStream.Close();
                        }
                        break;
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

                //dynamic retObj = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, retObject);

                if (retObject == typeof(string))
                {
                    return responseFromServer;
                }
                else
                {
                    XmlSerializer serializer = new XmlSerializer(retObject);
                    StringReader rdr = new StringReader(responseFromServer);
                    dynamic retObj = serializer.Deserialize(rdr);
                    return retObj;
                }
                
            }
            catch (Exception ex)
            {
                return new Controllers.EleveniaController.ClientMessage() {
                    resultCode = "Ex;" + (ex.InnerException == null ? ex.Message : ex.InnerException.HResult.ToString()),
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
            }
        }
        #endregion
        #region buka lapak
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
        #endregion
        #region bca
        public async Task<string> RequestJSONObjectBCA(METHOD method, string urll, string access_token, string api_key, string timeStamp, string signature, string bodyContent)
        {
            try
            {
                var client = new System.Net.Http.HttpClient();
                string url = string.Format(urll, string.Empty);
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", ("Bearer " + access_token));
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-BCA-Key", api_key);
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-BCA-Timestamp", timeStamp);
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-BCA-Signature", signature);
                HttpResponseMessage response = null;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                //var content = new StringContent(bodyContent, Encoding.UTF8, "application/json");
                switch (method)
                {
                    case METHOD.GET:
                        //if (!string.IsNullOrEmpty(Parameter))
                        //    url += "?" + Parameter;
                        response = await client.GetAsync(url);
                        break;
                    case METHOD.POST:
                        var content = new StringContent(bodyContent, Encoding.UTF8, "application/json");
                        content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
                        response = await client.PostAsync(url, content);
                        break;
                        //case METHOD.PUT:
                        //    //var content = new StringContent(bodyContent, Encoding.UTF8, "application/json");
                        //    content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
                        //    response = await client.PutAsync(url, content);
                        //    break;
                }
                //response = await client.GetAsync(url);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        var contentR = await response.Content.ReadAsStringAsync();
                        client.Dispose();
                        response.Dispose();
                        return contentR;
                    }
                    //else
                    //{
                    //    return response.ToString();
                    //}
                }
                return "";

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        #endregion
        #region midtrans
        public async Task<object> RequestJSONObject(RESTServices service, string RouteMap, HttpContent Content, Type retObject, string Auth)
        {
            try
            {
                string ret = await RequestString(service, RouteMap, Content, Auth);
                if (ret != "")
                {
                    dynamic retObj = Newtonsoft.Json.JsonConvert.DeserializeObject(ret, retObject);
                    var retLogin = await ValidateResponse(ret, RouteMap);
                    if (retLogin)
                    {
                        return retObj;
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
            //string ret = await RequestString(service, RouteMap, Content);
            //if (ret != "")
            //{
            //    return Newtonsoft.Json.JsonConvert.DeserializeObject(ret, retObject);
            //}
            //return null;
        }
        public async Task<string> RequestString(RESTServices service, string RouteMap, HttpContent Content, string Auth)
        {
            try
            {
                var client = new System.Net.Http.HttpClient();
                //var REST_URL_API = "https://app.midtrans.com/snap/";
                var REST_URL_API = "https://app.sandbox.midtrans.com/snap/";

                string url = string.Format(REST_URL_API, string.Empty) + service.ToString() + "/" + RouteMap;
                client.BaseAddress = new Uri(url);

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //add request header
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Auth);
                //client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Auth);
                //client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                //end add

                HttpResponseMessage response = null;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");//content-type change sucsess!
                response = await client.PostAsync(url, Content);
                //response.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        client.Dispose();
                        response.Dispose();
                        return content;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return "";
        }
        #endregion
    }
}