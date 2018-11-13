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
    }
}