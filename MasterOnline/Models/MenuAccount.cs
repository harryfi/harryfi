using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    public class MenuAccount
    {
        //add by nurul 20/2/2019
        public Account Account { get; set; }
        //end add by nurul 20/2/2019
        public List<Account> ListAccount { get; set; } = new List<Account>();
        public List<Partner> ListPartner { get; set; } = new List<Partner>();
        public List<Subscription> ListSubs { get; set; } = new List<Subscription>();
        public List<String> Errors { get; set; } = new List<String>();
    }

    public class mdlAkun
    {
        public Int64 AccountId { get; set; }
        public String UserId { get; set; }
        public string VCode { get; set; }
        public String Email { get; set; }
        public String Username { get; set; }
        public String Password { get; set; }
        public String ConfirmPassword { get; set; }
        public String NoHp { get; set; }
        public String DatabasePathMo { get; set; }
        public String PhotoKtpUrl { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
        public String NamaTokoOnline { get; set; }
        public String DatabasePathErasoft { get; set; }
        public Boolean Status { get; set; }
        public string KODE_SUBSCRIPTION { get; set; }
        public DateTime? TGL_SUBSCRIPTION { get; set; }
        public string PhotoKtpBase64 { get; set; }
        public string TOKEN_CC { get; set; }
        public string KODE_REFERRAL { get; set; }
        public DateTime? TGL_DAFTAR { get; set; }
        public int? jumlahUser { get; set; }
        public DateTime? tgl_approve { get; set; }
        public DateTime? tgl_email1 { get; set; }
        public DateTime? tgl_email2 { get; set; }
        public DateTime? LAST_LOGIN_DATE { get; set; }
        public string DataSourcePath { get; set; }
        public string DataSourcePathDebug { get; set; }
        public string confirm_broadcast { get; set; }
        
        public string nmPartner { get; set; }
        public string Subscription { get; set; }
    }

    public class REMINDER_EXPIRED_VIEW
    {
        public long AccountId { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string NoHp { get; set; }
        public string NamaTokoOnline { get; set; }
        public string NamaPartner { get; set; }
        public DateTime? TGL_SUBSCRIPTION { get; set; }
        public string KETERANGAN { get; set; }
        public int? JumlahUser { get; set; }
        public bool Status { get; set; }
        public DateTime? tgl_email1 { get; set; }
        public DateTime? tgl_email2 { get; set; }

    }
}