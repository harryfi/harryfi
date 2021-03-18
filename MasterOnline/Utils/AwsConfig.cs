using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Utils
{
    public class AwsConfig
    {
        public static string _awsAccessKey { get; set; } = "AKIAIUKY7MJGRJULBXZQ";
        public static string _awsSecretKey { get; set; } = "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";
        public static string _bucketName { get; set; } = "masteronlinebucket";
        public static string _amazonS3PublicUrl { get; set; } = "https://masteronlinebucket.s3-ap-southeast-1.amazonaws.com/";
        public static string _amazonAwsUrl { get; set; } = "https://s3-ap-southeast-1.amazonaws.com/";

        //for uploadfileservice.cs
        public static string _bucketFileName { get; set; } = "uploaded-file/";

        //for upload excel pesanan
        public static string _bucketFileName_Pesanan { get; set; } = "uploaded-file/upload_pesanan_batch/";
        
        //for upload print label MP
        public static string _bucketFileName_PrintLabel { get; set; } = "uploaded-file-printlabel/";

        //for upload log history
        public static string _bucketFileName_Log { get; set; } = "uploaded-file-log/";

        //for upload foto-ktp
        public static string _bucketFileName_FotoKTP { get; set; } = "foto-ktp/";
    }
}