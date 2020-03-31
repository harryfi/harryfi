using System;
using System.IO;
using System.Web;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using MasterOnline.Utils;

namespace MasterOnline.Services
{
    public class UploadFileServices
    {
        private static readonly string _awsAccessKey = AwsConfig._awsAccessKey;
        private static readonly string _awsSecretKey = AwsConfig._awsSecretKey;
        private static readonly string _bucketName = AwsConfig._bucketName;
        private static readonly string _bucketFileName = AwsConfig._bucketFileName;
        private static readonly string _amazonS3PublicUrl = AwsConfig._amazonS3PublicUrl;
        private static readonly string _amazonAwsUrl = AwsConfig._amazonAwsUrl;
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.APSoutheast1;

        public static byte[] UploadFile(HttpPostedFileBase file)
        {
            byte[] dataByte = null;
            var fileName = Guid.NewGuid().ToString();

            try
            {
                Stream inputSteram = file.InputStream;
                IAmazonS3 client;
                using (client = new AmazonS3Client(_awsAccessKey, _awsSecretKey, Amazon.RegionEndpoint.APSoutheast1))
                {
                    PutObjectRequest putRequest = new PutObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = _bucketFileName + string.Format(file.FileName),
                        CannedACL = S3CannedACL.PublicRead,//PERMISSION TO FILE PUBLIC ACCESIBLE
                        ContentType = file.ContentType,
                        InputStream = inputSteram,
                    };
                    client.PutObject(putRequest);                    

                    using (GetObjectResponse response = client.GetObject(_bucketName, _bucketFileName + string.Format(file.FileName)))
                    {
                        using (Stream inputStream = response.ResponseStream)
                        {
                            MemoryStream memoryStream = inputStream as MemoryStream;
                            
                                if(memoryStream == null)
                                {
                                    memoryStream = new MemoryStream();
                                    inputStream.CopyTo(memoryStream);
                                }
                            dataByte = memoryStream.ToArray();
                        }
                    }
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
            return dataByte;
        }
    }
}