using System;
using System.IO;
using System.Web;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using MasterOnline.Utils;
using Amazon.S3.Transfer;
using System.Threading.Tasks;



namespace MasterOnline.Services
{
    public class UploadFileServices
    {
        private static readonly string _awsAccessKey = AwsConfig._awsAccessKey;
        private static readonly string _awsSecretKey = AwsConfig._awsSecretKey;
        private static readonly string _bucketName = AwsConfig._bucketFileName;
        private static readonly string _amazonS3PublicUrl = AwsConfig._amazonS3PublicUrl;
        private static readonly string _amazonAwsUrl = AwsConfig._amazonAwsUrl;
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.APSoutheast1;
        private const string keyName = "upload_1";
        private static IAmazonS3 s3Client;
        

        public static string UploadFile(HttpPostedFileBase file)
        {
            var fileName = Guid.NewGuid().ToString();
            var ret = "";

            try
            {
                Stream inputSteram = file.InputStream;

                IAmazonS3 client;
                using (client = new AmazonS3Client(_awsAccessKey, _awsSecretKey, Amazon.RegionEndpoint.APSoutheast1))
                {
                    PutObjectRequest putRequest = new PutObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = string.Format(file.FileName),
                        //CannedACL = S3CannedACL.PublicRead,//PERMISSION TO FILE PUBLIC ACCESIBLE
                        ContentType = file.ContentType,
                        InputStream = inputSteram
                    };

                    ret = _amazonS3PublicUrl + "uploaded-file/" + string.Format(file.FileName);
                    client.PutObject(putRequest);
                }
                            
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("Check the provided AWS Credentials.");
                }
                else
                {
                    //throw new Exception("Error occurred: " + amazonS3Exception.Message);
                }
            }
            return ret;
        }
    }
}