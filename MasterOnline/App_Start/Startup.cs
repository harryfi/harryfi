using System;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Security.Claims;
using Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.AspNet.Identity;
using MasterOnline.Utils;
using Hangfire;

[assembly: OwinStartup(typeof(MasterOnline.App_Start.Startup))]

namespace MasterOnline.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
            //app.UseHangfireServer();
            //app.UseHangfireDashboard("/job_dashboard", new DashboardOptions
            //{
            //    Authorization = new[] { new DashboardAuth() }
            //});
        }
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/login")
            });

            //app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            //// App.Secrets is application specific and holds values in CodePasteKeys.json
            //// Values are NOT included in repro – auto-created on first load
            //if (!string.IsNullOrEmpty(App.Secrets.GoogleClientId))
            //{
            //    app.UseGoogleAuthentication(
            //        clientId: App.Secrets.GoogleClientId,
            //        clientSecret: App.Secrets.GoogleClientSecret);
            //}

            //if (!string.IsNullOrEmpty(App.Secrets.TwitterConsumerKey))
            //{
            //    app.UseTwitterAuthentication(
            //        consumerKey: App.Secrets.TwitterConsumerKey,
            //        consumerSecret: App.Secrets.TwitterConsumerSecret);
            //}

            //if (!string.IsNullOrEmpty(App.Secrets.GitHubClientId))
            //{
            //    app.UseGitHubAuthentication(
            //        clientId: App.Secrets.GitHubClientId,
            //        clientSecret: App.Secrets.GitHubClientSecret);
            //}

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
        }

        //public void ConfigureService(IServiceCollection services)
        //{
        //    services.AddDistributedRedisCache(options =>
        //    {
        //        //options.Configuration = "mo-prod-redis.df2l2v.0001.apse1.cache.amazonaws.com:6379";
        //        options.Configuration = "127.0.0.1:6379";
        //        options.InstanceName = "MasterOnline";
        //    });

        //    services.AddSession(options =>
        //    {
        //        options.CookieName = "Session.Testing";
        //        options.IdleTimeout = TimeSpan.FromMinutes(60);
        //        options.Cookie.HttpOnly = true;
        //    });

        //    services.AddMvc();
        //}

        //public void ConfigureSession()
        //{
        //    //apps.UseSession();
        //    //apps.UseMvc();
        //}
    }
}
