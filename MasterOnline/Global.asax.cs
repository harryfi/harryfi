using Hangfire;
using Hangfire.Pro.Redis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MasterOnline
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            System.Web.Http.GlobalConfiguration.Configure(WebApiConfig.Register);
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // START SETTING HANGFIRE PRO REDIS
            //Hangfire.GlobalConfiguration.Configuration.UseRedisStorage("mo-prod-redis.df2l2v.0001.apse1.cache.amazonaws.com,abortConnect=false,ssl=false,password=...");
            //Hangfire.GlobalConfiguration.Configuration.UseRedisStorage("127.0.0.1,abortConnect=false,ssl=true,password=...");

            var optionsPrefix = new Hangfire.Pro.Redis.RedisStorageOptions
            {
                Prefix = "hangfire:app1:",
            };

            //Hangfire.GlobalConfiguration.Configuration.UseRedisStorage("mo-prod-redis.df2l2v.0001.apse1.cache.amazonaws.com:6379",
                //new RedisStorageOptions { Prefix = "{hangfire-1}:" });
            //Hangfire.GlobalConfiguration.Configuration.UseRedisStorage("127.0.0.1:6379",
            //    new RedisStorageOptions { Prefix = "{hangfire-1}:" });

            //Hangfire.GlobalConfiguration.Configuration.UseRedisStorage("127.0.0.1:6379", optionsPrefix);
            //Hangfire.GlobalConfiguration.Configuration.UseRedisStorage("mo-prod-redis.df2l2v.0001.apse1.cache.amazonaws.com:6379", optionsPrefix);

            // END SETTING HANGFIRE PRO REDIS

#if (DEBUG || Debug_AWS)
            Utils.HangfireBootstrapper.Instance.Start();
#else
            Utils.HangfireBootstrapper.Instance.Start();
#endif
        }

        protected void Application_BeginRequest()
        {
            CultureInfo info = new CultureInfo(System.Threading.Thread.CurrentThread.CurrentCulture.ToString());
            info.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
            System.Threading.Thread.CurrentThread.CurrentCulture = info;

#if (DEBUG || Debug_AWS)
            Utils.HangfireBootstrapper.Instance.Stop();
#else
            Utils.HangfireBootstrapper.Instance.Stop();
#endif
        }
    }
}
