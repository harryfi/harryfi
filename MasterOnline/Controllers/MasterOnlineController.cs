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
            




            return ret;

        }
    }
}