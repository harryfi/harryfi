namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class API_LOG_MARKETPLACE_HANGFIRE
    {
        public string Marketplace { get; set; }
        public string AkunMarketplace { get; set; }
        public string REQUEST_CATEGORY { get; set; }
        public string REQUEST_ACTION { get; set; }
        public DateTime REQUEST_DATETIME { get; set; }
        public string REQUEST_RESULT { get; set; }
        public string REQUEST_EXCEPTION { get; set; }
        public string REQUEST_ID { get; set; }
        public string CUST_ATTRIBUTE_1 { get; set; }
    }
    public partial class HANGFIRE_SERVER_STATUS
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string NamaTokoOnline { get; set; }
        public string DatabaseSourceErasoft { get; set; }
        public string DatabasePathErasoft { get; set; }
        public int HangfireServerCount { get; set; }
        public DateTime? LAST_LOGIN_DATE { get; set; }
        public DateTime? TGL_SUBSCRIPTION { get; set; }
        public DateTime? LAST_HEARTBEAT { get; set; }
        public int StokJobEnqueued { get; set; }
        public int PesananJobEnqueued { get; set; }
        public int CreateProductJobEnqueued { get; set; }
    }

    public class PesananHangfireJob
    {
        public long ID { get; set; }
        public long STATEID { get; set; }
        public string STATENAME { get; set; }
        public string METHOD { get; set; }
        public string MARKETPLACE { get; set; }
        public DateTime? CREATEDAT { get; set; }
        public DateTime? EXPIREAT { get; set; }
        public string NAME { get; set; }
        public DateTime? LASTCREATEJOBPROCESS { get; set; }
        public DateTime? LASTCREATEJOBSUCCESS { get; set; }
        public string SELISIH { get; set; }
    }


    public class FieldInvocationData
    {
        public string Type { get; set; }
        public string Method { get; set; }
        public string ParameterTypes { get; set; }
        public object Arguments { get; set; }
    }

    public class FieldArguments
    {
        public string[] Arguments { get; set; }
    }

}
