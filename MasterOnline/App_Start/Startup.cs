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
            //var storage1 = new Hangfire.SqlServer.SqlServerStorage("Data Source=54.151.175.62, 12350;Initial Catalog=ERASOFT_930355_QC;Persist Security Info=True;User ID=sa;Password=admin123^");

            //app.UseHangfireDashboard("/job_dashboard", new DashboardOptions
            //{
            //    //Authorization = new[] { new DashboardAuth() }
            //    AppPath = "https://www.masteronline.my.id"
            //}, storage1);
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
    }
}
