using Hangfire;
using MasterOnline.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace MasterOnline.Utils
{
    public class ApplicationPreload : System.Web.Hosting.IProcessHostPreloadClient
    {
        public void Preload(string[] parameters)
        {

#if (DEBUG || Debug_AWS)
            HangfireBootstrapper.Instance.Start();
#else
            //initialize log txt
            #region Logging
            string messageErrorLog = "";
            string filename = "Log_Initial_AppPreload_" + DateTime.Now.AddHours(7).ToString("yyyyMMddhhmmss") + ".txt";
            var path = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Content/log/"), filename);

            if (!System.IO.File.Exists(path))
            {
                System.IO.Directory.CreateDirectory(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Content/log/"), ""));
                var asd = System.IO.File.Create(path);
                asd.Close();
            }
            StreamWriter tw = new StreamWriter(path);
            var msglog = "Log ApplicationPreload Running...... Pada waktu " + DateTime.Now.AddHours(7).ToString("yyyy-MM-dd hh:mm:ss");
            tw.WriteLine(msglog);
            tw.Close();
            tw.Dispose();

            //byte[] byteLog = System.IO.File.ReadAllBytes(path);
            //var pathLoc = UploadFileServices.UploadFile_Log(byteLog, filename);
            #endregion

            //add by Tri 21 apr 2021, ikut function di application start
            var optionsPrefix = new Hangfire.Pro.Redis.RedisStorageOptions
            {
                //InvisibilityTimeout = TimeSpan.FromMinutes(1440),
                Prefix = "hangfire:app1:",
            };
            Hangfire.GlobalConfiguration.Configuration.UseRedisStorage("mo-prod-redis.df2l2v.0001.apse1.cache.amazonaws.com:6379", optionsPrefix);
            //end add by Tri 21 apr 2021, ikut function di application start

            HangfireBootstrapper.Instance.Start();
#endif
        }
    }
}