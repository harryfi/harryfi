using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Erasoft.Function;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Storage;

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

#if (DEBUG || Debug_AWS)
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
                    var EDB = new DatabaseSQL(item.DatabasePathErasoft);

                    string EDBConnID = EDB.GetConnectionString("ConnID");
                    var sqlStorage = new SqlServerStorage(EDBConnID);

                    var monitoringApi = sqlStorage.GetMonitoringApi();
                    var serverList = monitoringApi.Servers();

                    if (serverList.Count() == 0)
                    {
                        startHangfireServer(sqlStorage);
                    }
                    else
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
#else
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
                    var EDB = new DatabaseSQL(item.DatabasePathErasoft);

                    string EDBConnID = EDB.GetConnectionString("ConnID");
                    var sqlStorage = new SqlServerStorage(EDBConnID);

                    var monitoringApi = sqlStorage.GetMonitoringApi();
                    var serverList = monitoringApi.Servers();

                    if (serverList.Count() == 0)
                    {
                        startHangfireServer(sqlStorage);
                    }
                    else
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
                    recurJobM.AddOrUpdate(recurringJob.Id, recurringJob.Job, Cron.MinuteInterval(30), recurJobOpt);
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
}