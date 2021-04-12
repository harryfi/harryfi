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
            #endregion


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
            #endregion

            HangfireBootstrapper.Instance.Start();
#endif
        }
    }
}