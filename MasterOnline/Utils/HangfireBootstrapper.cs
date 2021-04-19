using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Erasoft.Function;
using Hangfire;
using Hangfire.Pro.Redis;
using Hangfire.SqlServer;
using Hangfire.Storage;
using MasterOnline.Services;

namespace MasterOnline.Utils
{
    public class HangfireBootstrapper : IRegisteredObject
    {
        public static readonly HangfireBootstrapper Instance = new HangfireBootstrapper();
        private readonly object _lockObject = new object();
        private bool _started;

        //private BackgroundJobServer _backgroundJobServer;

        private MoDbContext MoDbContext;

        private HangfireBootstrapper()
        {
        }

        public void Start()
        {
            lock (_lockObject)
            {
                if (_started) return;
                _started = true;

                HostingEnvironment.RegisterObject(this);

                ////initialize log txt
                //#region Logging
                //string messageErrorLog = "";
                //string filename = "Log_Intitial_AppPreload_HangfireBootstrapper_" + DateTime.Now.AddHours(7).ToString("yyyyMMddhhmmss") + ".txt";
                //var path = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Content/log/"), filename);

                //if (!System.IO.File.Exists(path))
                //{
                //    System.IO.Directory.CreateDirectory(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Content/log/"), ""));
                //    var asd = System.IO.File.Create(path);
                //    asd.Close();
                //}
                //StreamWriter tw = new StreamWriter(path);
                //var msglog = "Log HangfireBootstrapper Running...... Pada waktu " + DateTime.Now.AddHours(7).ToString("yyyy-MM-dd hh:mm:ss");
                //tw.WriteLine(msglog);
                //tw.Close();
                //tw.Dispose();
                //#endregion

#if (Debug_AWS || DEBUG)
                var testing = "";
#elif (DEV)
                // START SETTING HANGFIRE PRO REDIS
                //Hangfire.GlobalConfiguration.Configuration.UseRedisStorage("mo-prod-redis.df2l2v.0001.apse1.cache.amazonaws.com,abortConnect=false,ssl=true,password=...");
                //Hangfire.GlobalConfiguration.Configuration.UseRedisStorage("127.0.0.1,abortConnect=false,ssl=true,password=...");

                //var optionsPrefix = new Hangfire.Pro.Redis.RedisStorageOptions
                //{
                //    Prefix = "hangfire:app1:",
                //};

                //Hangfire.GlobalConfiguration.Configuration.UseRedisStorage("127.0.0.1:6379",
                //    new RedisStorageOptions { Prefix = "{hangfire-1}:" });

                //Hangfire.GlobalConfiguration.Configuration.UseRedisStorage("127.0.0.1:6379", optionsPrefix);
                //Hangfire.GlobalConfiguration.Configuration.UseRedisStorage("mo-prod-redis.df2l2v.0001.apse1.cache.amazonaws.com:6379", optionsPrefix);

                // END SETTING HANGFIRE PRO REDIS

                var lastYear = DateTime.UtcNow.AddYears(-1);
                var last2Week = DateTime.UtcNow.AddHours(7).AddDays(-14);
                var datenow = DateTime.UtcNow.AddHours(7);

                MoDbContext = new MoDbContext();

                var accountInDb = (from a in MoDbContext.Account
                                   where
                                   (a.LAST_LOGIN_DATE ?? lastYear) >= last2Week
                                   &&
                                   (a.TGL_SUBSCRIPTION ?? lastYear) >= datenow
                                   orderby a.LAST_LOGIN_DATE descending
                                   select a).ToList();
                foreach (var item in accountInDb)
                {
                    if (!string.IsNullOrEmpty(item.DataSourcePath) && !string.IsNullOrEmpty(item.DatabasePathErasoft))
                    {
                        //var EDB = new DatabaseSQL(item.DatabasePathErasoft);

                        //string EDBConnID = EDB.GetConnectionString("ConnID");
                        //var sqlStorage = new SqlServerStorage(EDBConnID);

                        //var monitoringApi = sqlStorage.GetMonitoringApi();
                        //var serverList = monitoringApi.Servers();

                        //if (serverList.Count() == 0)
                        //{
                        //    startHangfireServer(sqlStorage);
                        //}
                        //else
                        //{
                        //    foreach (var server in serverList)
                        //    {
                        //        var serverConnection = sqlStorage.GetConnection();
                        //        serverConnection.RemoveServer(server.Name);
                        //        serverConnection.Dispose();
                        //    }
                        //    startHangfireServer(sqlStorage);
                        //}

                        var EDB = new DatabaseSQL(item.DatabasePathErasoft);
                        var erasoft = new ErasoftContext(item.DataSourcePath, item.DatabasePathErasoft);
                        string sSQL = "select * from hangfire.server";
                        var check = erasoft.Database.SqlQuery<HANGFIRE_SERVER>(sSQL).ToList();
                        string EDBConnID = EDB.GetConnectionString("ConnID");
                        var sqlStorage = new SqlServerStorage(EDBConnID);

                        var monitoringApi = sqlStorage.GetMonitoringApi();
                        var serverList = monitoringApi.Servers();

                        if (check.Count() == 0)
                        {
                            //if (serverList.Count() == 0)
                            //{
                            //    startHangfireServer(sqlStorage);
                            //}
                            //else
                            //{
                                foreach (var server in serverList)
                                {
                                    var serverConnection = sqlStorage.GetConnection();
                                    serverConnection.RemoveServer(server.Name);
                                    serverConnection.Dispose();
                                }
                                startHangfireServer(sqlStorage);
                            //}
                        }
                        else
                        {
                            string sSQLCheckDuplicate = "select * from hangfire.server where lastheartbeat > dateadd(MINUTE, -2, GETDATE())";
                            var checkDuplicate = erasoft.Database.SqlQuery<HANGFIRE_SERVER>(sSQLCheckDuplicate).ToList();

                            if(checkDuplicate.Count() == 0)
                            {
                                foreach (var server in serverList)
                                {
                                    var serverConnection = sqlStorage.GetConnection();
                                    serverConnection.RemoveServer(server.Name);
                                    serverConnection.Dispose();
                                }
                                startHangfireServer(sqlStorage);
                            }
                        }
                    }
                }
#else
                // START SETTING HANGFIRE PRO REDIS
                //Hangfire.GlobalConfiguration.Configuration.UseRedisStorage("contoso5.redis.cache.windows.net,abortConnect=false,ssl=true,password=...");

                //var optionsPrefix = new Hangfire.Pro.Redis.RedisStorageOptions
                //{
                //    Prefix = "hangfire:app1:"
                //};

                ////Hangfire.GlobalConfiguration.Configuration.UseRedisStorage("localhost:6379", optionsPrefix);
                //Hangfire.GlobalConfiguration.Configuration.UseRedisStorage("mo-prod-redis.df2l2v.0001.apse1.cache.amazonaws.com:6379", optionsPrefix);

                // END SETTING HANGFIRE PRO REDIS

                var lastYear = DateTime.UtcNow.AddYears(-1);
                var last2Week = DateTime.UtcNow.AddHours(7).AddDays(-14);
                var datenow = DateTime.UtcNow.AddHours(7);

                MoDbContext = new MoDbContext();

                var accountInDb = (from a in MoDbContext.Account
                                   where
                                   (a.LAST_LOGIN_DATE ?? lastYear) >= last2Week
                                   &&
                                   (a.TGL_SUBSCRIPTION ?? lastYear) >= datenow
                                   orderby a.LAST_LOGIN_DATE descending
                                   select a).ToList();
                foreach (var item in accountInDb)
                {
                    if (!string.IsNullOrEmpty(item.DataSourcePath) && !string.IsNullOrEmpty(item.DatabasePathErasoft))
                    {
                        //var EDB = new DatabaseSQL(item.DatabasePathErasoft);

                        //string EDBConnID = EDB.GetConnectionString("ConnID");
                        //var sqlStorage = new SqlServerStorage(EDBConnID);

                        //var monitoringApi = sqlStorage.GetMonitoringApi();
                        //var serverList = monitoringApi.Servers();

                        //if (serverList.Count() == 0)
                        //{
                        //    startHangfireServer(sqlStorage);
                        //}
                        //else
                        //{
                        //    foreach (var server in serverList)
                        //    {
                        //        var serverConnection = sqlStorage.GetConnection();
                        //        serverConnection.RemoveServer(server.Name);
                        //        serverConnection.Dispose();
                        //    }
                        //    startHangfireServer(sqlStorage);
                        //}

                        var EDB = new DatabaseSQL(item.DatabasePathErasoft);
                        var erasoft = new ErasoftContext(item.DataSourcePath, item.DatabasePathErasoft);
                        string sSQL = "select * from hangfire.server";
                        var check = erasoft.Database.SqlQuery<HANGFIRE_SERVER>(sSQL).ToList();
                        string EDBConnID = EDB.GetConnectionString("ConnID");
                        var sqlStorage = new SqlServerStorage(EDBConnID);
                        //var sqlStorage = new SqlServerStorage("Data Source=54.151.175.62, 12350;Initial Catalog=ERASOFT_rahmamk;Persist Security Info=True;User ID=sa;Password=admin123^");
                        
                        var monitoringApi = sqlStorage.GetMonitoringApi();
                        var serverList = monitoringApi.Servers();

                        if (Convert.ToInt32(check.Count()) == 0)
                        {
                            //if (serverList.Count() == 0)
                            //{
                            //    startHangfireServer(sqlStorage);
                            //}
                            //else
                            //{
                            //foreach (var server in serverList)
                            //{
                            //    var serverConnection = sqlStorage.GetConnection();
                            //    serverConnection.RemoveServer(server.Name);
                            //    serverConnection.Dispose();
                            //}
                            startHangfireServer(sqlStorage);
                            if (item.DatabasePathErasoft == "ERASOFT_80069")
                            {
                                //initialize log txt
                                #region Logging
                                messageErrorLog = "";
                                filename = "Log_AppPreload_HangfireBootstrapper_CONDITION_SERVER_EMPTY_" + item.DatabasePathErasoft + "_" + DateTime.Now.AddHours(7).ToString("yyyyMMddhhmmss") + ".txt";
                                path = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Content/log/"), filename);

                                if (!System.IO.File.Exists(path))
                                {
                                    System.IO.Directory.CreateDirectory(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Content/log/"), ""));
                                    var asd = System.IO.File.Create(path);
                                    asd.Close();
                                }
                                StreamWriter tw = new StreamWriter(path);
                                var msglog = "Log Masuk kondisi Hangfire Server Kosong lalu lanjutkan dengan startHangfireServer() untuk startUp Hangfire. Pada waktu " + DateTime.Now.AddHours(7).ToString("yyyy-MM-dd hh:mm:ss");
                                tw.WriteLine(msglog);
                                tw.Close();
                                tw.Dispose();

                                //byte[] byteLog = System.IO.File.ReadAllBytes(path);
                                //var pathLoc = UploadFileServices.UploadFile_Log(byteLog, filename);
                                #endregion

                            }
                            //}
                        }
                        else
                        {
                            string sSQLCheckDuplicate = "select * from hangfire.server where lastheartbeat > dateadd(MINUTE, -2, GETDATE())";
                            var checkDuplicate = erasoft.Database.SqlQuery<HANGFIRE_SERVER>(sSQLCheckDuplicate).ToList();

                            if(checkDuplicate.Count() == 0)
                            {
                                //foreach (var server in serverList)
                                //{
                                //    var serverConnection = sqlStorage.GetConnection();
                                //    serverConnection.RemoveServer(server.Name);
                                //    serverConnection.Dispose();
                                //}
                                startHangfireServer(sqlStorage);

                            if (item.DatabasePathErasoft == "ERASOFT_80069")
                            {
                                #region Logging
                                messageErrorLog = "";
                                filename = "Log_AppPreload_HangfireBootstrapper_CONDITION_SERVER_ALREADYEXIST_" + item.DatabasePathErasoft + "_" + DateTime.Now.AddHours(7).ToString("yyyyMMddhhmmss") + ".txt";
                                path = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Content/log/"), filename);

                                if (!System.IO.File.Exists(path))
                                {
                                    System.IO.Directory.CreateDirectory(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Content/log/"), ""));
                                    var asd = System.IO.File.Create(path);
                                    asd.Close();
                                }
                                StreamWriter tw = new StreamWriter(path);
                                var msglog = "Log Masuk kondisi Hangfire Server Ada lalu lanjut dengan melakukan hapus server. Pada waktu " + DateTime.Now.AddHours(7).ToString("yyyy-MM-dd hh:mm:ss");
                                tw.WriteLine(msglog);
                                tw.Close();
                                tw.Dispose();

                                //byte[] byteLog = System.IO.File.ReadAllBytes(path);
                                //var pathLoc = UploadFileServices.UploadFile_Log(byteLog, filename);
                                #endregion
                            }

                            startHangfireServer(sqlStorage);

                            if (item.DatabasePathErasoft == "ERASOFT_80069")
                            {
                                #region Logging
                                messageErrorLog = "";
                                filename = "Log_AppPreload_HangfireBootstrapper_CONDITION_SERVER_ALREADYEXIST_" + item.DatabasePathErasoft + "_" + DateTime.Now.AddHours(7).ToString("yyyyMMddhhmmss") + ".txt";
                                path = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Content/log/"), filename);

                                if (!System.IO.File.Exists(path))
                                {
                                    System.IO.Directory.CreateDirectory(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Content/log/"), ""));
                                    var asd = System.IO.File.Create(path);
                                    asd.Close();
                                }
                                StreamWriter tw = new StreamWriter(path);
                                var msglog = "Log Masuk kondisi Hangfire Server Sudah terhapus lalu lanjut dengan melakukan startHangfireServer() untuk startUp Hangfire. Pada waktu " + DateTime.Now.AddHours(7).ToString("yyyy-MM-dd hh:mm:ss");
                                tw.WriteLine(msglog);
                                tw.Close();
                                tw.Dispose();

                                //byte[] byteLog = System.IO.File.ReadAllBytes(path);
                                //var pathLoc = UploadFileServices.UploadFile_Log(byteLog, filename);
                                #endregion
                            }
                        }
                    }
                } 
#endif
            }
        }

        public void startHangfireServer(SqlServerStorage sqlStorage) {
            var optionsStatusResiServer = new BackgroundJobServerOptions
            {
                ServerName = "StatusResiPesanan",
                Queues = new[] { "1_manage_pesanan" },
                WorkerCount = 1,

            };
            var newStatusResiServer = new BackgroundJobServer(optionsStatusResiServer, sqlStorage);

            var options = new BackgroundJobServerOptions
            {
                ServerName = "Account",
                Queues = new[] { "1_critical", "2_get_token", "3_general", "4_tokped_cek_pending" },
                WorkerCount = 1,
            };
            var newserver = new BackgroundJobServer(options, sqlStorage);

            var optionsStokServer = new BackgroundJobServerOptions
            {
                ServerName = "Stok",
                Queues = new[] { "1_update_stok" },
                WorkerCount = 2,
            };
            var newStokServer = new BackgroundJobServer(optionsStokServer, sqlStorage);

            var optionsBarangServer = new BackgroundJobServerOptions
            {
                ServerName = "Product",
                Queues = new[] { "1_create_product" },
                WorkerCount = 1,
            };
            var newProductServer = new BackgroundJobServer(optionsBarangServer, sqlStorage);

            RecurringJobManager recurJobM = new RecurringJobManager(sqlStorage);
            RecurringJobOptions recurJobOpt = new RecurringJobOptions()
            {
                QueueName = "3_general"
            };
            using (var connection = sqlStorage.GetConnection())
            {
                //update semua recurring job dengan interval sesuai setting timer
                foreach (var recurringJob in connection.GetRecurringJobs())
                {
                    if (recurringJob.Job != null)
                    {
                        recurJobM.AddOrUpdate(recurringJob.Id, recurringJob.Job, recurringJob.Cron, recurJobOpt);
                    }
                    else
                    {
                        recurJobM.RemoveIfExists(recurringJob.Id);
                    }
                }
            }
        }
        public void Stop()
        {
            lock (_lockObject)
            {
                //if (_backgroundJobServer != null)
                //{
                //    _backgroundJobServer.Dispose();
                //}

                HostingEnvironment.UnregisterObject(this);
            }
        }

        void IRegisteredObject.Stop(bool immediate)
        {
            Stop();
        }
    }

    public partial class HANGFIRE_SERVER
    {
        public string Id { get; set; }
        public string Data { get; set; }
        public DateTime LastHeartbeat { get; set; }
    }
}